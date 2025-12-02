using System;
using System.Data.SqlClient;
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
                //System.Data.DataRow user = UserManager.LoginUser(email, password);

                if (UserManager.IsEmailExists(email))
                {
                    ShowError("User with this email already exists");
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