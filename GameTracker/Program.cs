using System;
using System.Windows.Forms;

namespace GameTracker
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isAutoLogin = false;

            // Remember Me açık ise
            if (Properties.Settings.Default.RememberMe)
            {
                try
                {
                    // Bilgileri çeker
                    string email = Properties.Settings.Default.StoredEmail;
                    string encPass = Properties.Settings.Default.StoredPassword;
                    string password = SecurityHelper.Decrypt(encPass);

                    // Veritabanından doğrular
                    var user = UserManager.LoginUser(email, password);

                    // Giriş başarılı ise
                    if (user != null)
                    {
                        // Session'ı doldurur
                        Session.UserId = Convert.ToInt32(user["user_id"]);
                        Session.Username = Convert.ToString(user["username"]);
                        Session.Email = Convert.ToString(user["email"]);

                        isAutoLogin = true;
                    }
                }
                catch
                {
                    // Bir hata olursa çaktırmadan login formunu aç
                    isAutoLogin = false;
                }
            }

            if (isAutoLogin)
                Application.Run(new MainForm());
            else
                Application.Run(new LoginForm());
        }
    }
}
