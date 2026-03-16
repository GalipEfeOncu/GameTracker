using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace GameTracker
{
    public class UserManager
    {
        /// <summary>
        /// Bu email'in sistemde kayıtlı olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="email"> Kullanıcının maili </param>
        /// <returns></returns>
        public static bool IsEmailExists(string email)
        {
            // Varsa 1 döner, yoksa boş döner.
            string query = "SELECT TOP 1 1 FROM users WHERE email = @email";
            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@email", email) };
            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            // Eğer satır sayısı 0'dan büyükse, demek ki böyle bir mail var.
            return dt.Rows.Count > 0;
        }

        /// <summary>
        /// Kullanıcıyı kayıt eder.
        /// </summary>
        /// <param name="username">Kullanıcının kullanıcı adı</param>
        /// <param name="email">Kullanıcının maili</param>
        /// <param name="password">Kullanıcının parolası</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool RegisterUser(string username, string email, string password)
        {
            try
            {
                string passwordHash = HashPassword(password, email);

                string query = @"INSERT INTO users (username, email, password, email_verified, created_at) 
                               VALUES (@username, @email, @password, 0, GETDATE())";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@username", username),
                    new SqlParameter("@email", email),
                    new SqlParameter("@password", passwordHash)
                };

                int result = DatabaseHelper.ExecuteNonQuery(query, parameters);
                return result > 0;
            }
            catch (SqlException ex)
            {
                // aynı username veya email varsa
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("Bu kullanıcı adı veya email zaten kullanılıyor.");
                throw;
            }
        }

        /// <summary>
        /// Kullanıcıyı giriş yapar.
        /// </summary>
        /// <param name="usernameOrEmail">Kullanıcının kullanıcı adı veya maili</param>
        /// <param name="password">Kullanıcının parolası</param>
        /// <returns></returns>
        public static DataRow LoginUser(string usernameOrEmail, string plainPassword)
        {
            string query = @"SELECT user_id, username, email, password, email_verified, created_at 
                            FROM users 
                            WHERE (username = @usernameOrEmail OR email = @usernameOrEmail)";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@usernameOrEmail", usernameOrEmail)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            // kullanıcı bulunduysa
            if (dt.Rows.Count > 0)
            {
                DataRow userRow = dt.Rows[0];

                // DB'deki gerçek maili ve hashli şifreyi al
                string dbEmail = userRow["email"].ToString();
                string dbPasswordHash = userRow["password"].ToString();

                // Girilen şifreyi, DB'den gelen mail ile tekrar hashle
                string computedHash = HashPassword(plainPassword, dbEmail);

                // Şifre eşleşiyorsa satırı döndür (doğrulanmamış olsa da; controller e-posta doğrulama mesajı döner)
                if (computedHash == dbPasswordHash)
                    return userRow;
            }

            return null;
        }

        /// <summary>
        /// E-posta doğrulama kodunu onaylayıp kullanıcıyı doğrulanmış yapar.
        /// </summary>
        public static bool SetEmailVerified(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            string query = "UPDATE users SET email_verified = 1 WHERE LOWER(LTRIM(RTRIM(email))) = LOWER(@email)";
            var parameters = new[] { new SqlParameter("@email", email.Trim()) };
            int rows = DatabaseHelper.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        /// <summary>
        /// Kullanıcıyı ve kütüphanesini siler (CASCADE ile UserLibrary zaten silinir).
        /// </summary>
        public static bool DeleteUser(int userId)
        {
            string query = "DELETE FROM users WHERE user_id = @userId";
            var parameters = new[] { new SqlParameter("@userId", userId) };
            int rows = DatabaseHelper.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        /// <summary>
        /// Kullanıcının parolasını hashler.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private static string HashPassword(string password, string salt)
        {
            string saltedPass = password + salt;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPass));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));

                return builder.ToString();
            }
        }

        /// <summary>
        /// Kullanıcının e-posta adresini döndürür (şifre hash doğrulama için gerekli).
        /// </summary>
        public static string GetUserEmail(int userId)
        {
            string query = "SELECT email FROM users WHERE user_id = @userId";
            var parameters = new[] { new SqlParameter("@userId", userId) };
            var dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 0) return null;
            return dt.Rows[0]["email"]?.ToString();
        }

        /// <summary>
        /// E-posta ile kullanıcı ID'si döndürür (şifre sıfırlama için).
        /// </summary>
        public static int? GetUserIdByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalized = email.Trim();
            string query = "SELECT user_id FROM users WHERE LOWER(LTRIM(RTRIM(email))) = LOWER(@email)";
            var parameters = new[] { new SqlParameter("@email", normalized) };
            var dt = DatabaseHelper.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 0) return null;
            return Convert.ToInt32(dt.Rows[0]["user_id"]);
        }

        /// <summary>
        /// Kullanıcının ismini günceller.
        /// </summary>
        public static bool UpdateUsername(int userId, string newUsername)
        {
            try
            {
                string query = "UPDATE users SET username = @username WHERE user_id = @userId";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@userId", userId),
                    new SqlParameter("@username", newUsername)
                };

                return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
            }
            catch (SqlException ex)
            {
                // Eğer isim başkası tarafından alınmışsa (Unique Constraint)
                if (ex.Number == 2627 || ex.Number == 2601)
                    throw new Exception("This username is already taken.");

                throw;
            }
        }

        /// <summary>
        /// Şifre değiştirmeden önce mevcut şifreyi doğrular.
        /// </summary>
        public static bool VerifyPassword(int userId, string email, string plainPassword)
        {
            string hash = HashPassword(plainPassword, email);
            string query = "SELECT COUNT(*) FROM users WHERE user_id = @userId AND password = @password";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@password", hash)
            };

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }

        /// <summary>
        /// Kullanıcının şifresini günceller.
        /// </summary>
        public static bool UpdatePassword(int userId, string email, string newPlainPassword)
        {
            if (string.IsNullOrEmpty(email)) return false;

            string newHash = HashPassword(newPlainPassword, email);
            string query = "UPDATE users SET password = @password WHERE user_id = @userId";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@password", newHash)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}
