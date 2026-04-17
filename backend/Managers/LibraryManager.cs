using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using GameTracker.Models;

namespace GameTracker
{
    public class LibraryManager
    {
        // Oyunu kütüphaneye ekler
        public static bool AddGameToLibrary(int userId, Game game, string status = "PlanToPlay")
        {
            if (!LibraryStatuses.IsAllowed(status))
                return false;

            var canonicalStatus = LibraryStatuses.Normalize(status);

            try
            {
                string query = @"INSERT INTO UserLibrary (user_id, game_id, game_name, image_url, status) 
                               VALUES (@userId, @gameId, @gameName, @imageUrl, @status)";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@gameId", game.Id),
                    new SqlParameter("@gameName", game.Name ?? "Unknown"), // Null check
                    new SqlParameter("@imageUrl", game.BackgroundImage ?? ""), // Null check
                    new SqlParameter("@status", canonicalStatus)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (SqlException ex)
            {
                // Eğer kullanıcı oyunu zaten eklediyse (Unique constraint hatası)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    return false;
                }
                throw;
            }
        }

        // Kullanıcının kütüphanesindeki oyunları getirir
        public static List<Game> GetUserLibrary(int userId, string statusFilter = null)
        {
            List<Game> libraryGames = new List<Game>();

            if (!string.IsNullOrEmpty(statusFilter) && !LibraryStatuses.IsAllowed(statusFilter))
                return libraryGames;

            string query = "SELECT game_id, game_name, image_url, status, playtime_minutes FROM UserLibrary WHERE user_id = @userId";

            var canonicalFilter = LibraryStatuses.Normalize(statusFilter);
            bool filterCompleted = string.Equals(canonicalFilter, LibraryStatuses.Completed, StringComparison.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                // 'Completed' filtresi migration öncesi legacy 'Played' satırlarını da kapsasın.
                query += filterCompleted
                    ? " AND status IN ('Completed', 'Played')"
                    : " AND status = @status";
            }

            query += " ORDER BY added_at DESC"; // En son eklenen en üstte

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId)
            };

            if (!string.IsNullOrEmpty(statusFilter) && !filterCompleted)
            {
                var paramList = new List<SqlParameter>(parameters);
                paramList.Add(new SqlParameter("@status", canonicalFilter));
                parameters = paramList.ToArray();
            }

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                // Legacy 'Played' satırları kanonik 'Completed' olarak dönülür (frontend yalnızca 'Completed' bilir).
                // DB tek seferlik migration ile de normalize edilir: scripts/NormalizePlayedToCompleted.sql.
                var rawStatus = row["status"]?.ToString();
                libraryGames.Add(new Game
                {
                    Id = Convert.ToInt32(row["game_id"]),
                    Name = row["game_name"].ToString(),
                    BackgroundImage = row["image_url"].ToString(),
                    Status = LibraryStatuses.Normalize(rawStatus),
                    PlaytimeMinutes = row["playtime_minutes"] != DBNull.Value
                        ? Convert.ToInt32(row["playtime_minutes"])
                        : 0,
                });
            }

            return libraryGames;
        }

        // Oyunun durumunu günceller (örn: Playing -> Completed)
        public static bool UpdateGameStatus(int userId, int gameId, string newStatus)
        {
            if (!LibraryStatuses.IsAllowed(newStatus))
                return false;

            string query = "UPDATE UserLibrary SET status = @status WHERE user_id = @userId AND game_id = @gameId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@gameId", gameId),
                new SqlParameter("@status", LibraryStatuses.Normalize(newStatus))
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        // Oyunu kütüphaneden siler
        public static bool RemoveGame(int userId, int gameId)
        {
            string query = "DELETE FROM UserLibrary WHERE user_id = @userId AND game_id = @gameId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@gameId", gameId)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        // Oyunun oynama süresine dakika delta'sı ekler. Desktop istemcisi her 30 sn'de heartbeat gönderir.
        // Negatif veya 0 delta reddedilir; oyun kütüphanede değilse 0 döner (kontrollü no-op).
        public static int IncrementPlaytime(int userId, int gameId, int minutesDelta)
        {
            if (minutesDelta <= 0) return 0;

            const string query = @"
UPDATE UserLibrary
   SET playtime_minutes = playtime_minutes + @delta
 OUTPUT INSERTED.playtime_minutes
 WHERE user_id = @userId
   AND game_id = @gameId;";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@gameId", gameId),
                new SqlParameter("@delta",  minutesDelta),
            };

            var result = DatabaseHelper.ExecuteScalar(query, parameters);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        // Bir oyunun kütüphanede olup olmadığını kontrol eder
        public static bool IsGameInLibrary(int userId, int gameId)
        {
            string query = "SELECT COUNT(*) FROM UserLibrary WHERE user_id = @userId AND game_id = @gameId";
            SqlParameter[] parameters = new SqlParameter[]
           {
                new SqlParameter("@userId", userId),
                new SqlParameter("@gameId", gameId)
           };

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }
    }
}