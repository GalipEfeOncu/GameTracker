using System;

namespace GameTracker
{
    public partial class SignupForm : DevExpress.XtraEditors.XtraForm
    {
        public SignupForm()
        {
            InitializeComponent();
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string passwordAgain = txtPasswordAgain.Text;

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

            if (password.Length < 8)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    "Password must be longer than 8 characters.",
                    "Warning",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);

                txtPassword.Focus();
                return;
            }

            if (password != passwordAgain)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    "Your passwords doesn't match.",
                    "Warning",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);

                return;
            }

            // Database

            try
            {
                System.Data.DataRow user = UserManager.LoginUser(email, password);

                if (user != null)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(
                    "There is an account registered to this email address.\r\n",
                    "Warning",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);

                    return;
                }

                bool success = UserManager.RegisterUser(username, email, password);

                if (success)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(
                        "Kayıt başarılı! Giriş yapabilirsiniz.",
                        "Başarılı",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Information);

                    LoginForm loginForm = new LoginForm();
                    loginForm.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    $"Kayıt hatası: {ex.Message}",  // ex.Message = Hatanın açıklaması
                    "Hata",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}