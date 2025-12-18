namespace GameTracker
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.txtEmail = new DevExpress.XtraEditors.TextEdit();
            this.txtPassword = new DevExpress.XtraEditors.TextEdit();
            this.chckBoxRememberMe = new DevExpress.XtraEditors.CheckEdit();
            this.BtnLogin = new DevExpress.XtraEditors.SimpleButton();
            this.lblGoSignup = new DevExpress.XtraEditors.HyperlinkLabelControl();
            this.panelCntrlLogin = new DevExpress.XtraEditors.PanelControl();
            this.lblError = new DevExpress.XtraEditors.LabelControl();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.txtEmail.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chckBoxRememberMe.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelCntrlLogin)).BeginInit();
            this.panelCntrlLogin.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(25, 120);
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
            this.txtEmail.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(25, 180);
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
            this.txtPassword.TabIndex = 2;
            // 
            // chckBoxRememberMe
            // 
            this.chckBoxRememberMe.Location = new System.Drawing.Point(25, 240);
            this.chckBoxRememberMe.Name = "chckBoxRememberMe";
            this.chckBoxRememberMe.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.chckBoxRememberMe.Properties.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.chckBoxRememberMe.Properties.Appearance.Options.UseFont = true;
            this.chckBoxRememberMe.Properties.Appearance.Options.UseForeColor = true;
            this.chckBoxRememberMe.Properties.Caption = "Remember me";
            this.chckBoxRememberMe.Size = new System.Drawing.Size(154, 21);
            this.chckBoxRememberMe.TabIndex = 3;
            // 
            // BtnLogin
            // 
            this.BtnLogin.AllowFocus = false;
            this.BtnLogin.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.BtnLogin.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.BtnLogin.Appearance.ForeColor = System.Drawing.Color.White;
            this.BtnLogin.Appearance.Options.UseBackColor = true;
            this.BtnLogin.Appearance.Options.UseFont = true;
            this.BtnLogin.Appearance.Options.UseForeColor = true;
            this.BtnLogin.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.BtnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnLogin.Location = new System.Drawing.Point(25, 290);
            this.BtnLogin.Name = "BtnLogin";
            this.BtnLogin.Size = new System.Drawing.Size(300, 45);
            this.BtnLogin.TabIndex = 4;
            this.BtnLogin.Text = "LOG IN";
            this.BtnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            // 
            // lblGoSignup
            // 
            this.lblGoSignup.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.lblGoSignup.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.lblGoSignup.Appearance.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.lblGoSignup.Appearance.Options.UseFont = true;
            this.lblGoSignup.Appearance.Options.UseForeColor = true;
            this.lblGoSignup.Appearance.Options.UseLinkColor = true;
            this.lblGoSignup.Appearance.Options.UseTextOptions = true;
            this.lblGoSignup.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblGoSignup.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblGoSignup.Location = new System.Drawing.Point(25, 360);
            this.lblGoSignup.Name = "lblGoSignup";
            this.lblGoSignup.Size = new System.Drawing.Size(300, 20);
            this.lblGoSignup.TabIndex = 5;
            this.lblGoSignup.Text = "Don\'t have an account? <href=signup>Sign Up</href>";
            this.lblGoSignup.Click += new System.EventHandler(this.lblSignup_Click);
            // 
            // panelCntrlLogin
            // 
            this.panelCntrlLogin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panelCntrlLogin.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.panelCntrlLogin.Appearance.Options.UseBackColor = true;
            this.panelCntrlLogin.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelCntrlLogin.Controls.Add(this.lblError);
            this.panelCntrlLogin.Controls.Add(this.lblTitle);
            this.panelCntrlLogin.Controls.Add(this.txtEmail);
            this.panelCntrlLogin.Controls.Add(this.txtPassword);
            this.panelCntrlLogin.Controls.Add(this.chckBoxRememberMe);
            this.panelCntrlLogin.Controls.Add(this.BtnLogin);
            this.panelCntrlLogin.Controls.Add(this.lblGoSignup);
            this.panelCntrlLogin.Location = new System.Drawing.Point(235, 55);
            this.panelCntrlLogin.Name = "panelCntrlLogin";
            this.panelCntrlLogin.Size = new System.Drawing.Size(350, 450);
            this.panelCntrlLogin.TabIndex = 0;
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
            this.lblError.Location = new System.Drawing.Point(25, 258);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(300, 26);
            this.lblError.TabIndex = 6;
            this.lblError.Text = "Warning";
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
            this.lblTitle.Location = new System.Drawing.Point(25, 30);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(300, 50);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Welcome Back";
            // 
            // LoginForm
            // 
            this.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.Appearance.Options.UseBackColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 561);
            this.Controls.Add(this.panelCntrlLogin);
            this.IconOptions.Image = global::GameTracker.Properties.Resources.icon___1_;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login - GameTracker";
            ((System.ComponentModel.ISupportInitialize)(this.txtEmail.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chckBoxRememberMe.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelCntrlLogin)).EndInit();
            this.panelCntrlLogin.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.TextEdit txtEmail;
        private DevExpress.XtraEditors.TextEdit txtPassword;
        private DevExpress.XtraEditors.CheckEdit chckBoxRememberMe;
        private DevExpress.XtraEditors.SimpleButton BtnLogin;
        private DevExpress.XtraEditors.HyperlinkLabelControl lblGoSignup;
        private DevExpress.XtraEditors.PanelControl panelCntrlLogin;
        private DevExpress.XtraEditors.LabelControl lblTitle; // Yeni ekledim
        private DevExpress.XtraEditors.LabelControl lblError;
    }
}