namespace GameTracker
{
    partial class GameCardControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.borderPanel = new System.Windows.Forms.Panel();
            this.peGameImage = new DevExpress.XtraEditors.PictureEdit();
            this.lblGameTitle = new DevExpress.XtraEditors.LabelControl();
            this.borderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.peGameImage.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // borderPanel
            // 
            this.borderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.borderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.borderPanel.Controls.Add(this.peGameImage);
            this.borderPanel.Location = new System.Drawing.Point(0, 0);
            this.borderPanel.Margin = new System.Windows.Forms.Padding(0);
            this.borderPanel.Name = "borderPanel";
            this.borderPanel.Size = new System.Drawing.Size(280, 186);
            this.borderPanel.TabIndex = 0;
            // 
            // peGameImage
            // 
            this.peGameImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.peGameImage.Location = new System.Drawing.Point(0, 0);
            this.peGameImage.Name = "peGameImage";
            this.peGameImage.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.peGameImage.Properties.Appearance.Options.UseBackColor = true;
            this.peGameImage.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.peGameImage.Properties.ReadOnly = true;
            this.peGameImage.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.peGameImage.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.peGameImage.Size = new System.Drawing.Size(280, 186);
            this.peGameImage.TabIndex = 0;
            this.peGameImage.MouseEnter += new System.EventHandler(this.peGameImage_MouseEnter);
            this.peGameImage.MouseLeave += new System.EventHandler(this.peGameImage_MouseLeave);
            // 
            // lblGameTitle
            // 
            this.lblGameTitle.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.lblGameTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGameTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblGameTitle.Appearance.Options.UseBackColor = true;
            this.lblGameTitle.Appearance.Options.UseFont = true;
            this.lblGameTitle.Appearance.Options.UseForeColor = true;
            this.lblGameTitle.Appearance.Options.UseTextOptions = true;
            this.lblGameTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblGameTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblGameTitle.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblGameTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblGameTitle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblGameTitle.Location = new System.Drawing.Point(0, 186);
            this.lblGameTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblGameTitle.Name = "lblGameTitle";
            this.lblGameTitle.Size = new System.Drawing.Size(280, 30);
            this.lblGameTitle.TabIndex = 1;
            this.lblGameTitle.Text = "Game Title";
            // 
            // GameCardControl
            // 
            this.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.Appearance.Options.UseBackColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblGameTitle);
            this.Controls.Add(this.borderPanel);
            this.Name = "GameCardControl";
            this.Size = new System.Drawing.Size(280, 216);
            this.borderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.peGameImage.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Panel borderPanel;
        public DevExpress.XtraEditors.PictureEdit peGameImage;
        public DevExpress.XtraEditors.LabelControl lblGameTitle;
    }
}