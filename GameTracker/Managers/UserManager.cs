using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace GameTracker
{
    public class UserManager
    {
        // Bu metot veritabanına sadece "Var mı?" diye sorar.
        // Kayıt yapmaz, login yapmaz, sadece kontrol eder.
        public static bool IsEmailExists(string email)
        {
            // Varsa 1 döner, yoksa boş döner.
            string query = "SELECT TOP 1 1 FROM users WHERE email = @email";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@email", email)
            };

            DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

            // Eğer satır sayısı 0'dan büyükse, demek ki böyle bir mail var.
            return dt.Rows.Count > 0;
        }

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
        public static bool VerifyPassword(int userId, string plainPassword)
        {
            string hash = HashPassword(plainPassword);
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
        public static bool UpdatePassword(int userId, string newPlainPassword)
        {
            string newHash = HashPassword(newPlainPassword);
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
