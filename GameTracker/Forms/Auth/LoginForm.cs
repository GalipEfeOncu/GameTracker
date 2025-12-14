using System;
using System.Windows.Forms;

namespace GameTracker
{
    public partial class LoginForm : DevExpress.XtraEditors.XtraForm
    {
        public LoginForm()
        {
            InitializeComponent();
            lblError.Visible = false;

            // Eğer RememberMe varsa kutuları doldurur hata alırsak hazır olsun
            if (Properties.Settings.Default.RememberMe)
            {
                txtEmail.Text = Properties.Settings.Default.StoredEmail;
                string encryptedPassword = Properties.Settings.Default.StoredPassword;
                txtPassword.Text = SecurityHelper.Decrypt(encryptedPassword);

                chckBoxRememberMe.Checked = true;

                this.ActiveControl = BtnLogin;
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        public void SetEmail(string email)
        {
            txtEmail.Text = email;
            txtPassword.Focus();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (!email.Contains("@") || !email.Contains("."))
            {
                ShowError("Please enter a valid email");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Password cannot be blank");
                return;
            }

            // Database

            try
            {
                System.Data.DataRow user = UserManager.LoginUser(email, password); //Bu email ve şifreye sahip kullanıcı var mı?

                if (user != null)
                {
                    if (chckBoxRememberMe.Checked)
                    {
                        // Kutucuk işaretliyse ayarları kaydeder
                        Properties.Settings.Default.StoredEmail = email;

                        // Parolayı encrypt edip kaydeder
                        string safePassword = SecurityHelper.Encrypt(txtPassword.Text);
                        Properties.Settings.Default.StoredPassword = safePassword;

                        Properties.Settings.Default.RememberMe = true;
                    }
                    else
                    {
                        // İşaretli değilse ayarları temizler
                        Properties.Settings.Default.StoredEmail = "";
                        Properties.Settings.Default.StoredPassword = "";
                        Properties.Settings.Default.RememberMe = false;
                    }

                    Properties.Settings.Default.Save();

                    // Kullanıcı bilgilerini Session'a kaydet (bellekte tut)
                    Session.UserId = Convert.ToInt32(user["user_id"]);
                    Session.Username = Convert.ToString(user["username"]);
                    Session.Email = Convert.ToString(user["email"]);

                    MainForm mainForm = new MainForm();
                    mainForm.Show();
                    this.Hide();
                }
                else
                {
                    ShowError("Invalid email or password");
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show(
                    $"An error occurred while trying to log in. Please try again later. Error: {ex.Message}",
                    "Error");
            }
        }

        private void lblSignup_Click(object sender, EventArgs e)
        {
            SignupForm signupForm = new SignupForm();
            signupForm.Show();
            this.Hide();
        }

        /// <summary>
        /// Form tamamen kapandığında uygulamayı sonlandırır.
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}