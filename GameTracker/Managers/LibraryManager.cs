using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using GameTracker.Models;

namespace GameTracker
{
    public class LibraryManager
    {
        // Oyunu kütüphaneye ekler
        public static bool AddGameToLibrary(int userId, Game game, string status = "PlanToPlay")
        {
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
                    new SqlParameter("@status", status)
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

            string query = "SELECT game_id, game_name, image_url, status FROM UserLibrary WHERE user_id = @userId";

            // Eğer status filtresi varsa (örn: sadece 'Played' olanları getir)
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query += " AND status = @status";
            }

            query += " ORDER BY added_at DESC"; // En son eklenen en üstte

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId)
            };

            if (!string.IsNullOrEmpty(statusFilter))
            {
                // Array resize yerine yeni liste oluşturup ekleme yapıyoruz ve sonra array'e çeviriyoruz
                var paramList = new List<SqlParameter>(parameters);
                paramList.Add(new SqlParameter("@status", statusFilter));
                parameters = paramList.ToArray();
            }

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                // DB'den gelen veriyi Game objesine çevirir
                libraryGames.Add(new Game
                {
                    Id = Convert.ToInt32(row["game_id"]),
                    Name = row["game_name"].ToString(),
                    BackgroundImage = row["image_url"].ToString(),
                });
            }

            return libraryGames;
        }

        // Oyunun durumunu günceller (örn: Playing -> Played)
        public static bool UpdateGameStatus(int userId, int gameId, string newStatus)
        {
            string query = "UPDATE UserLibrary SET status = @status WHERE user_id = @userId AND game_id = @gameId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@gameId", gameId),
                new SqlParameter("@status", newStatus)
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