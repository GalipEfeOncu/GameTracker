using System;
using System.Windows.Forms;

namespace GameTracker
{
    public partial class SignupForm : DevExpress.XtraEditors.XtraForm
    {
        public SignupForm()
        {
            InitializeComponent();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private void btnSignup_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string passwordAgain = txtPasswordAgain.Text;

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Username cannot be blank");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(email))
            {
                ShowError("Email cannot be blank");
                txtEmail.Focus();
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                ShowError("Please enter a valid email adress");
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Password cannot be blank");
                txtPassword.Focus();
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Password must be longer than 8 characters.");
                txtPassword.Focus();
                return;
            }

            if (password != passwordAgain)
            {
                ShowError("Passwords do not match.");
                return;
            }

            // Database
            try
            {
                if (UserManager.IsEmailExists(email))
                {
                    ShowError("User with this email already exists");
                    return;
                }

                // --- E-POSTA DOĞRULAMA ADIMLARI ---
                // Doğrulama kodu üret
                Random rnd = new Random();
                string verificationCode = rnd.Next(100000, 999999).ToString();

                // Mail gönder
                this.Cursor = Cursors.WaitCursor;
                bool mailSent = Helpers.EmailService.SendVerificationCode(email, verificationCode);
                this.Cursor = Cursors.Default;

                if (!mailSent)
                {
                    MyMessageBox.Show("Could not send verification email. Please check your email address.", "Error");
                    return;
                }

                // Kullanıcıdan Kodu İste
                string userEnteredCode = DevExpress.XtraEditors.XtraInputBox.Show(
                    "A verification code has been sent to your email.\nPlease enter it below:",
                    "Verify Email",
                    "");

                // Kullanıcı "İptal"e basarsa veya boş bırakırsa işlemi kes
                if (string.IsNullOrEmpty(userEnteredCode))
                {
                    MyMessageBox.Show("Registration cancelled.", "Info");
                    return;
                }

                // Kod yanlışsa işlemi kes
                if (userEnteredCode != verificationCode)
                {
                    MyMessageBox.Show("Invalid verification code! Registration cancelled.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool success = UserManager.RegisterUser(username, email, password);

                if (success)
                {
                    MyMessageBox.Show("Account created successfully! You can now login.", "Welcome Aboard");

                    LoginForm loginForm = new LoginForm();

                    loginForm.SetEmail(email);

                    loginForm.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show(
                    $"An error occurred during registration. Please try again later. Error: {ex.Message}",
                    "Error");
            }
        }

        private void lblLogin_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Hide();
        }
    }
}