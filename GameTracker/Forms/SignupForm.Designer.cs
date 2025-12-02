namespace GameTracker
{
    partial class SignupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelCntrlSignup = new DevExpress.XtraEditors.PanelControl();
            this.lblError = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.txtUsername = new DevExpress.XtraEditors.TextEdit();
            this.txtEmail = new DevExpress.XtraEditors.TextEdit();
            this.txtPassword = new DevExpress.XtraEditors.TextEdit();
            this.txtPasswordAgain = new DevExpress.XtraEditors.TextEdit();
            this.btnSignup = new DevExpress.XtraEditors.SimpleButton();
            this.lblLogin = new DevExpress.XtraEditors.HyperlinkLabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.panelCntrlSignup)).BeginInit();
            this.panelCntrlSignup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEmail.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPasswordAgain.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelCntrlSignup
            // 
            this.panelCntrlSignup.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelCntrlSignup.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.panelCntrlSignup.Appearance.Options.UseBackColor = true;
            this.panelCntrlSignup.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelCntrlSignup.Controls.Add(this.lblError);
            this.panelCntrlSignup.Controls.Add(this.lblTitle);
            this.panelCntrlSignup.Controls.Add(this.txtUsername);
            this.panelCntrlSignup.Controls.Add(this.txtEmail);
            this.panelCntrlSignup.Controls.Add(this.txtPassword);
            this.panelCntrlSignup.Controls.Add(this.txtPasswordAgain);
            this.panelCntrlSignup.Controls.Add(this.btnSignup);
            this.panelCntrlSignup.Controls.Add(this.lblLogin);
            this.panelCntrlSignup.Location = new System.Drawing.Point(235, 30);
            this.panelCntrlSignup.Name = "panelCntrlSignup";
            this.panelCntrlSignup.Size = new System.Drawing.Size(350, 500);
            this.panelCntrlSignup.TabIndex = 0;
            // 
            // lblError
            // 
            this.lblError.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblError.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblError.Appearance.Options.UseFont = true;
            this.lblError.Appearance.Options.UseForeColor = true;
            this.lblError.Appearance.Options.UseTextOptions = true;
            this.lblError.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblError.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblError.Location = new System.Drawing.Point(25, 335);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(300, 20);
            this.lblError.TabIndex = 0;
            this.lblError.Text = "Warning Message";
            this.lblError.Visible = false;
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Appearance.Options.UseTextOptions = true;
            this.lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle.Location = new System.Drawing.Point(25, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(300, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Create Account";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(25, 90);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.txtUsername.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtUsername.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtUsername.Properties.Appearance.Options.UseBackColor = true;
            this.txtUsername.Properties.Appearance.Options.UseFont = true;
            this.txtUsername.Properties.Appearance.Options.UseForeColor = true;
            this.txtUsername.Properties.AutoHeight = false;
            this.txtUsername.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtUsername.Properties.NullValuePrompt = "👤 Username";
            this.txtUsername.Size = new System.Drawing.Size(300, 40);
            this.txtUsername.TabIndex = 1;
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(25, 150);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.txtEmail.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtEmail.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtEmail.Properties.Appearance.Options.UseBackColor = true;
            this.txtEmail.Properties.Appearance.Options.UseFont = true;
            this.txtEmail.Properties.Appearance.Options.UseForeColor = true;
            this.txtEmail.Properties.AutoHeight = false;
            this.txtEmail.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtEmail.Properties.NullValuePrompt = "📧 Email Address";
            this.txtEmail.Size = new System.Drawing.Size(300, 40);
            this.txtEmail.TabIndex = 2;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(25, 210);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.txtPassword.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtPassword.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtPassword.Properties.Appearance.Options.UseBackColor = true;
            this.txtPassword.Properties.Appearance.Options.UseFont = true;
            this.txtPassword.Properties.Appearance.Options.UseForeColor = true;
            this.txtPassword.Properties.AutoHeight = false;
            this.txtPassword.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtPassword.Properties.NullValuePrompt = "🔒 Password";
            this.txtPassword.Properties.UseSystemPasswordChar = true;
            this.txtPassword.Size = new System.Drawing.Size(300, 40);
            this.txtPassword.TabIndex = 3;
            // 
            // txtPasswordAgain
            // 
            this.txtPasswordAgain.Location = new System.Drawing.Point(25, 270);
            this.txtPasswordAgain.Name = "txtPasswordAgain";
            this.txtPasswordAgain.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(45)))), ((int)(((byte)(64)))));
            this.txtPasswordAgain.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtPasswordAgain.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtPasswordAgain.Properties.Appearance.Options.UseBackColor = true;
            this.txtPasswordAgain.Properties.Appearance.Options.UseFont = true;
            this.txtPasswordAgain.Properties.Appearance.Options.UseForeColor = true;
            this.txtPasswordAgain.Properties.AutoHeight = false;
            this.txtPasswordAgain.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtPasswordAgain.Properties.NullValuePrompt = "🔒 Confirm Password";
            this.txtPasswordAgain.Properties.UseSystemPasswordChar = true;
            this.txtPasswordAgain.Size = new System.Drawing.Size(300, 40);
            this.txtPasswordAgain.TabIndex = 4;
            // 
            // btnSignup
            // 
            this.btnSignup.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnSignup.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnSignup.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnSignup.Appearance.Options.UseBackColor = true;
            this.btnSignup.Appearance.Options.UseFont = true;
            this.btnSignup.Appearance.Options.UseForeColor = true;
            this.btnSignup.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnSignup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSignup.Location = new System.Drawing.Point(25, 365);
            this.btnSignup.Name = "btnSignup";
            this.btnSignup.Size = new System.Drawing.Size(300, 45);
            this.btnSignup.TabIndex = 5;
            this.btnSignup.Text = "SIGN UP";
            this.btnSignup.Click += new System.EventHandler(this.btnSignup_Click);
            // 
            // lblLogin
            // 
            this.lblLogin.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblLogin.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.lblLogin.Appearance.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblLogin.Appearance.Options.UseFont = true;
            this.lblLogin.Appearance.Options.UseForeColor = true;
            this.lblLogin.Appearance.Options.UseLinkColor = true;
            this.lblLogin.Appearance.Options.UseTextOptions = true;
            this.lblLogin.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblLogin.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblLogin.Location = new System.Drawing.Point(25, 430);
            this.lblLogin.Name = "lblLogin";
            this.lblLogin.Size = new System.Drawing.Size(300, 20);
            this.lblLogin.TabIndex = 6;
            this.lblLogin.Text = "Already have an account? <href=login>Log In</href>";
            this.lblLogin.Click += new System.EventHandler(this.lblLogin_Click);
            // 
            // SignupForm
            // 
            this.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.Appearance.Options.UseBackColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 561);
            this.Controls.Add(this.panelCntrlSignup);
            this.Name = "SignupForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sign Up - GameTracker";
            ((System.ComponentModel.ISupportInitialize)(this.panelCntrlSignup)).EndInit();
            this.panelCntrlSignup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEmail.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPasswordAgain.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelCntrlSignup;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblError;
        private DevExpress.XtraEditors.TextEdit txtUsername;
        private DevExpress.XtraEditors.TextEdit txtEmail;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.TextEdit txtPasswordAgain;
        private DevExpress.XtraEditors.SimpleButton btnSignup;
        private DevExpress.XtraEditors.HyperlinkLabelControl lblLogin;
    }
}