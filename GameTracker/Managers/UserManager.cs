using System;
using System.Data;
using System.Data.SqlClient;
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
            string query = @"SELECT user_id, username, email, password, created_at 
                            FROM users 
                            WHERE username = @usernameOrEmail OR email = @usernameOrEmail";

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

                // Eşleşiyorsa giriş başarılı
                if (computedHash == dbPasswordHash)
                    return userRow;
            }

            return null;
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
            string hash = HashPassword(plainPassword, Session.Email);
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
            if (string.IsNullOrEmpty(Session.Email)) return false;

            string newHash = HashPassword(newPlainPassword, Session.Email);
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
