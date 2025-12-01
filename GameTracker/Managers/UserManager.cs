using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace GameTracker
{
    public class UserManager
    {
        // Kullanıcı Kaydı (Signup)
        public static bool RegisterUser(string username, string email, string password)
        {
            try
            {
                string passwordHash = HashPassword(password);

                string query = @"INSERT INTO users (username, email, password, created_at) 
                               VALUES (@username, @email, @password, GETDATE())";

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
                // Unique constraint hatası (aynı username veya email varsa)
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("Bu kullanıcı adı veya email zaten kullanılıyor.");
                }
                throw;
            }
        }

        // Kullanıcı Girişi (Login)
        public static DataRow LoginUser(string usernameOrEmail, string password)
        {
            string passwordHash = HashPassword(password);

            string query = @"SELECT user_id, username, email, created_at 
                            FROM users 
                            WHERE (username = @usernameOrEmail OR email = @usernameOrEmail) 
                            AND password = @password";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@usernameOrEmail", usernameOrEmail),
                new SqlParameter("@password", passwordHash)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }

            return null;
        }

        // Şifre hash'leme (güvenlik için)
        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
