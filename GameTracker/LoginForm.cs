using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameTracker
{
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim(); // Trim baş ve sondaki boşlukları siler.
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(email))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    "Email cannot be blank!",
                    "Warning",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);

                txtEmail.Focus();
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    "Please enter a valid email adress",
                    "Warning",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    "Password cannot be blank!",
                    "Warning",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);

                txtPassword.Focus();
                return;
            }

            // Database

            try
            {
                System.Data.DataRow user = UserManager.LoginUser(email, password); //Bu email ve şifreye sahip kullanıcı var mı?

                if (user != null)
                {
                    // Kullanıcı bilgilerini Session'a kaydet (bellekte tut)
                    Session.UserId = Convert.ToInt32(user["user_id"]);
                    Session.Username = Convert.ToString(user["username"]);
                    Session.Email = Convert.ToString(user["email"]);

                    // Başarı mesajı göster
                    DevExpress.XtraEditors.XtraMessageBox.Show(
                        $"Hoş geldin {Session.Username}!",
                        "Başarılı",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);

                    // TODO: Ana forma geç (henüz MainForm yok, sonra ekleriz)
                    // MainForm mainForm = new MainForm();
                    // mainForm.Show();
                    // this.Hide();
                }
                else
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(
                        "Email veya şifre hatalı!",
                        "Hata",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    $"Giriş hatası: {ex.Message}",  // ex.Message = Hatanın açıklaması
                    "Hata",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void lblSignup_Click(object sender, EventArgs e)
        {
            SignupForm signupForm = new SignupForm();
            signupForm.Show();
            this.Hide();
        }
    }
}
