namespace GameTracker
{
    partial class MainForm
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
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.btnSearch = new DevExpress.XtraEditors.SimpleButton();
            this.btnSettings = new DevExpress.XtraEditors.SimpleButton();
            this.btnLibrary = new DevExpress.XtraEditors.SimpleButton();
            this.btnHomeMenu = new DevExpress.XtraEditors.SimpleButton();
            this.lblTitle = new DevExpress.XtraEditors.LabelControl();
            this.navigationFrame1 = new DevExpress.XtraBars.Navigation.NavigationFrame();
            this.pageHome = new DevExpress.XtraBars.Navigation.NavigationPage();
            this.flowLayoutPanelPopulerGames = new System.Windows.Forms.FlowLayoutPanel();
            this.panelHomePagination = new DevExpress.XtraEditors.PanelControl();
            this.lblHomePage = new DevExpress.XtraEditors.LabelControl();
            this.btnHomeNext = new DevExpress.XtraEditors.SimpleButton();
            this.btnHomePrev = new DevExpress.XtraEditors.SimpleButton();
            this.lblPopulerGames = new DevExpress.XtraEditors.LabelControl();
            this.pageLibrary = new DevExpress.XtraBars.Navigation.NavigationPage();
            this.flowLayoutPanelLibrary = new System.Windows.Forms.FlowLayoutPanel();
            this.panelLibPagination = new DevExpress.XtraEditors.PanelControl();
            this.lblLibPage = new DevExpress.XtraEditors.LabelControl();
            this.btnLibNext = new DevExpress.XtraEditors.SimpleButton();
            this.btnLibPrev = new DevExpress.XtraEditors.SimpleButton();
            this.panelControl6 = new DevExpress.XtraEditors.PanelControl();
            this.btnLibPlayed = new DevExpress.XtraEditors.SimpleButton();
            this.lblMyLibrary = new DevExpress.XtraEditors.LabelControl();
            this.btnLibPlaying = new DevExpress.XtraEditors.SimpleButton();
            this.btnLibDropped = new DevExpress.XtraEditors.SimpleButton();
            this.btnLibPlanToPlay = new DevExpress.XtraEditors.SimpleButton();
            this.pageSearch = new DevExpress.XtraBars.Navigation.NavigationPage();
            this.flowLayoutPanelSearch = new System.Windows.Forms.FlowLayoutPanel();
            this.lblNoResult = new DevExpress.XtraEditors.LabelControl();
            this.panelSearchPagination = new DevExpress.XtraEditors.PanelControl();
            this.lblSearchPage = new DevExpress.XtraEditors.LabelControl();
            this.btnSearchNext = new DevExpress.XtraEditors.SimpleButton();
            this.btnSearchPrev = new DevExpress.XtraEditors.SimpleButton();
            this.panelControl7 = new DevExpress.XtraEditors.PanelControl();
            this.searchControlSearchPage = new DevExpress.XtraEditors.SearchControl();
            this.pageSettings = new DevExpress.XtraBars.Navigation.NavigationPage();
            this.flowSettingsMain = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlAccountSettings = new DevExpress.XtraEditors.PanelControl();
            this.lblPassWarning = new DevExpress.XtraEditors.LabelControl();
            this.lblUsernameWarning = new DevExpress.XtraEditors.LabelControl();
            this.txtNewPassAgain = new DevExpress.XtraEditors.TextEdit();
            this.lblAccountHeader = new DevExpress.XtraEditors.LabelControl();
            this.separatorAccount = new DevExpress.XtraEditors.LabelControl();
            this.lblChangeUser = new DevExpress.XtraEditors.LabelControl();
            this.txtNewUsername = new DevExpress.XtraEditors.TextEdit();
            this.btnUpdateUsername = new DevExpress.XtraEditors.SimpleButton();
            this.lblChangePass = new DevExpress.XtraEditors.LabelControl();
            this.txtCurrentPass = new DevExpress.XtraEditors.TextEdit();
            this.txtNewPass = new DevExpress.XtraEditors.TextEdit();
            this.btnUpdatePass = new DevExpress.XtraEditors.SimpleButton();
            this.pnlAppPreferences = new DevExpress.XtraEditors.PanelControl();
            this.lblPrefHeader = new DevExpress.XtraEditors.LabelControl();
            this.separatorPref = new DevExpress.XtraEditors.LabelControl();
            this.lblStartPage = new DevExpress.XtraEditors.LabelControl();
            this.cmbStartPage = new DevExpress.XtraEditors.ComboBoxEdit();
            this.lblNSFW = new DevExpress.XtraEditors.LabelControl();
            this.toggleNSFW = new DevExpress.XtraEditors.ToggleSwitch();
            this.pnlSystemData = new DevExpress.XtraEditors.PanelControl();
            this.lblSystemHeader = new DevExpress.XtraEditors.LabelControl();
            this.separatorSystem = new DevExpress.XtraEditors.LabelControl();
            this.btnClearCache = new DevExpress.XtraEditors.SimpleButton();
            this.btnLogout = new DevExpress.XtraEditors.SimpleButton();
            this.pageGameDetail = new DevExpress.XtraBars.Navigation.NavigationPage();
            this.scrollableDetailContainer = new DevExpress.XtraEditors.XtraScrollableControl();
            this.lblDetailRequirements = new DevExpress.XtraEditors.LabelControl();
            this.lblRequirementsHeader = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailDescription = new DevExpress.XtraEditors.LabelControl();
            this.lblDescriptionHeader = new DevExpress.XtraEditors.LabelControl();
            this.panelControl2 = new DevExpress.XtraEditors.PanelControl();
            this.flowLayoutScreenshots = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSS = new DevExpress.XtraEditors.LabelControl();
            this.panelTopSection = new DevExpress.XtraEditors.PanelControl();
            this.btnLibraryAction = new DevExpress.XtraEditors.SimpleButton();
            this.lblDetailAge = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailModes = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailStores = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailPlatforms = new DevExpress.XtraEditors.LabelControl();
            this.btnDetailBack = new DevExpress.XtraEditors.SimpleButton();
            this.peDetailImage = new DevExpress.XtraEditors.PictureEdit();
            this.lblDetailTitle = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailDeveloper = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailGenres = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailRating = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailMetacritic = new DevExpress.XtraEditors.LabelControl();
            this.lblDetailPlaytime = new DevExpress.XtraEditors.LabelControl();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.navigationFrame1)).BeginInit();
            this.navigationFrame1.SuspendLayout();
            this.pageHome.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelHomePagination)).BeginInit();
            this.panelHomePagination.SuspendLayout();
            this.pageLibrary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelLibPagination)).BeginInit();
            this.panelLibPagination.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl6)).BeginInit();
            this.panelControl6.SuspendLayout();
            this.pageSearch.SuspendLayout();
            this.flowLayoutPanelSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelSearchPagination)).BeginInit();
            this.panelSearchPagination.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl7)).BeginInit();
            this.panelControl7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.searchControlSearchPage.Properties)).BeginInit();
            this.pageSettings.SuspendLayout();
            this.flowSettingsMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pnlAccountSettings)).BeginInit();
            this.pnlAccountSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewPassAgain.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewUsername.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCurrentPass.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewPass.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlAppPreferences)).BeginInit();
            this.pnlAppPreferences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbStartPage.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.toggleNSFW.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSystemData)).BeginInit();
            this.pnlSystemData.SuspendLayout();
            this.pageGameDetail.SuspendLayout();
            this.scrollableDetailContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).BeginInit();
            this.panelControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelTopSection)).BeginInit();
            this.panelTopSection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.peDetailImage.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // panelControl1
            // 
            this.panelControl1.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.panelControl1.Appearance.Options.UseBackColor = true;
            this.panelControl1.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl1.Controls.Add(this.btnSearch);
            this.panelControl1.Controls.Add(this.btnSettings);
            this.panelControl1.Controls.Add(this.btnLibrary);
            this.panelControl1.Controls.Add(this.btnHomeMenu);
            this.panelControl1.Controls.Add(this.lblTitle);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(220, 718);
            this.panelControl1.TabIndex = 0;
            // 
            // btnSearch
            // 
            this.btnSearch.AllowFocus = false;
            this.btnSearch.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnSearch.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnSearch.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(180)))));
            this.btnSearch.Appearance.Options.UseBackColor = true;
            this.btnSearch.Appearance.Options.UseFont = true;
            this.btnSearch.Appearance.Options.UseForeColor = true;
            this.btnSearch.Appearance.Options.UseTextOptions = true;
            this.btnSearch.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnSearch.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(60)))));
            this.btnSearch.AppearanceHovered.ForeColor = System.Drawing.Color.White;
            this.btnSearch.AppearanceHovered.Options.UseBackColor = true;
            this.btnSearch.AppearanceHovered.Options.UseForeColor = true;
            this.btnSearch.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnSearch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearch.Location = new System.Drawing.Point(10, 200);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(200, 45);
            this.btnSearch.TabIndex = 4;
            this.btnSearch.Text = "  Search";
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnSettings
            // 
            this.btnSettings.AllowFocus = false;
            this.btnSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSettings.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnSettings.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnSettings.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(180)))));
            this.btnSettings.Appearance.Options.UseBackColor = true;
            this.btnSettings.Appearance.Options.UseFont = true;
            this.btnSettings.Appearance.Options.UseForeColor = true;
            this.btnSettings.Appearance.Options.UseTextOptions = true;
            this.btnSettings.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnSettings.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(60)))));
            this.btnSettings.AppearanceHovered.ForeColor = System.Drawing.Color.White;
            this.btnSettings.AppearanceHovered.Options.UseBackColor = true;
            this.btnSettings.AppearanceHovered.Options.UseForeColor = true;
            this.btnSettings.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.Location = new System.Drawing.Point(10, 658);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(200, 45);
            this.btnSettings.TabIndex = 3;
            this.btnSettings.Text = "  Settings";
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnLibrary
            // 
            this.btnLibrary.AllowFocus = false;
            this.btnLibrary.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnLibrary.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnLibrary.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(180)))));
            this.btnLibrary.Appearance.Options.UseBackColor = true;
            this.btnLibrary.Appearance.Options.UseFont = true;
            this.btnLibrary.Appearance.Options.UseForeColor = true;
            this.btnLibrary.Appearance.Options.UseTextOptions = true;
            this.btnLibrary.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnLibrary.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(60)))));
            this.btnLibrary.AppearanceHovered.ForeColor = System.Drawing.Color.White;
            this.btnLibrary.AppearanceHovered.Options.UseBackColor = true;
            this.btnLibrary.AppearanceHovered.Options.UseForeColor = true;
            this.btnLibrary.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibrary.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibrary.Location = new System.Drawing.Point(10, 150);
            this.btnLibrary.Name = "btnLibrary";
            this.btnLibrary.Size = new System.Drawing.Size(200, 45);
            this.btnLibrary.TabIndex = 2;
            this.btnLibrary.Text = "  My Library";
            this.btnLibrary.Click += new System.EventHandler(this.btnLibrary_Click);
            // 
            // btnHomeMenu
            // 
            this.btnHomeMenu.AllowFocus = false;
            this.btnHomeMenu.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnHomeMenu.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.btnHomeMenu.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(180)))));
            this.btnHomeMenu.Appearance.Options.UseBackColor = true;
            this.btnHomeMenu.Appearance.Options.UseFont = true;
            this.btnHomeMenu.Appearance.Options.UseForeColor = true;
            this.btnHomeMenu.Appearance.Options.UseTextOptions = true;
            this.btnHomeMenu.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.btnHomeMenu.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(60)))));
            this.btnHomeMenu.AppearanceHovered.ForeColor = System.Drawing.Color.White;
            this.btnHomeMenu.AppearanceHovered.Options.UseBackColor = true;
            this.btnHomeMenu.AppearanceHovered.Options.UseForeColor = true;
            this.btnHomeMenu.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnHomeMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHomeMenu.Location = new System.Drawing.Point(10, 100);
            this.btnHomeMenu.Name = "btnHomeMenu";
            this.btnHomeMenu.Size = new System.Drawing.Size(200, 45);
            this.btnHomeMenu.TabIndex = 1;
            this.btnHomeMenu.Text = "  Popular Games";
            this.btnHomeMenu.Click += new System.EventHandler(this.btnHomeMenu_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Appearance.Options.UseFont = true;
            this.lblTitle.Appearance.Options.UseForeColor = true;
            this.lblTitle.Appearance.Options.UseTextOptions = true;
            this.lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblTitle.Location = new System.Drawing.Point(0, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(220, 40);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "GameTracker";
            // 
            // navigationFrame1
            // 
            this.navigationFrame1.AllowTransitionAnimation = DevExpress.Utils.DefaultBoolean.False;
            this.navigationFrame1.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.navigationFrame1.Appearance.Options.UseBackColor = true;
            this.navigationFrame1.Controls.Add(this.pageHome);
            this.navigationFrame1.Controls.Add(this.pageLibrary);
            this.navigationFrame1.Controls.Add(this.pageSearch);
            this.navigationFrame1.Controls.Add(this.pageSettings);
            this.navigationFrame1.Controls.Add(this.pageGameDetail);
            this.navigationFrame1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigationFrame1.Location = new System.Drawing.Point(220, 0);
            this.navigationFrame1.Name = "navigationFrame1";
            this.navigationFrame1.Pages.AddRange(new DevExpress.XtraBars.Navigation.NavigationPageBase[] {
            this.pageHome,
            this.pageLibrary,
            this.pageSearch,
            this.pageSettings,
            this.pageGameDetail});
            this.navigationFrame1.SelectedPage = this.pageHome;
            this.navigationFrame1.Size = new System.Drawing.Size(1278, 718);
            this.navigationFrame1.TabIndex = 1;
            this.navigationFrame1.TransitionType = DevExpress.Utils.Animation.Transitions.Push;
            // 
            // pageHome
            // 
            this.pageHome.Controls.Add(this.flowLayoutPanelPopulerGames);
            this.pageHome.Controls.Add(this.panelHomePagination);
            this.pageHome.Controls.Add(this.lblPopulerGames);
            this.pageHome.Name = "pageHome";
            this.pageHome.Size = new System.Drawing.Size(1278, 718);
            // 
            // flowLayoutPanelPopulerGames
            // 
            this.flowLayoutPanelPopulerGames.AutoScroll = true;
            this.flowLayoutPanelPopulerGames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelPopulerGames.Location = new System.Drawing.Point(0, 70);
            this.flowLayoutPanelPopulerGames.Name = "flowLayoutPanelPopulerGames";
            this.flowLayoutPanelPopulerGames.Padding = new System.Windows.Forms.Padding(20);
            this.flowLayoutPanelPopulerGames.Size = new System.Drawing.Size(1278, 598);
            this.flowLayoutPanelPopulerGames.TabIndex = 20;
            // 
            // panelHomePagination
            // 
            this.panelHomePagination.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.panelHomePagination.Appearance.Options.UseBackColor = true;
            this.panelHomePagination.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelHomePagination.Controls.Add(this.lblHomePage);
            this.panelHomePagination.Controls.Add(this.btnHomeNext);
            this.panelHomePagination.Controls.Add(this.btnHomePrev);
            this.panelHomePagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelHomePagination.Location = new System.Drawing.Point(0, 668);
            this.panelHomePagination.Name = "panelHomePagination";
            this.panelHomePagination.Size = new System.Drawing.Size(1278, 50);
            this.panelHomePagination.TabIndex = 4;
            // 
            // lblHomePage
            // 
            this.lblHomePage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHomePage.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblHomePage.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.lblHomePage.Appearance.Options.UseFont = true;
            this.lblHomePage.Appearance.Options.UseForeColor = true;
            this.lblHomePage.Appearance.Options.UseTextOptions = true;
            this.lblHomePage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblHomePage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblHomePage.Location = new System.Drawing.Point(120, 15);
            this.lblHomePage.Name = "lblHomePage";
            this.lblHomePage.Size = new System.Drawing.Size(1038, 20);
            this.lblHomePage.TabIndex = 2;
            this.lblHomePage.Text = "Page 1 / 1";
            // 
            // btnHomeNext
            // 
            this.btnHomeNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHomeNext.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnHomeNext.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHomeNext.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnHomeNext.Appearance.Options.UseBackColor = true;
            this.btnHomeNext.Appearance.Options.UseFont = true;
            this.btnHomeNext.Appearance.Options.UseForeColor = true;
            this.btnHomeNext.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnHomeNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHomeNext.Location = new System.Drawing.Point(1166, 10);
            this.btnHomeNext.Name = "btnHomeNext";
            this.btnHomeNext.Size = new System.Drawing.Size(90, 30);
            this.btnHomeNext.TabIndex = 1;
            this.btnHomeNext.Text = "Next >";
            this.btnHomeNext.Click += new System.EventHandler(this.btnHomeNext_Click);
            // 
            // btnHomePrev
            // 
            this.btnHomePrev.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(44)))), ((int)(((byte)(60)))));
            this.btnHomePrev.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnHomePrev.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnHomePrev.Appearance.Options.UseBackColor = true;
            this.btnHomePrev.Appearance.Options.UseFont = true;
            this.btnHomePrev.Appearance.Options.UseForeColor = true;
            this.btnHomePrev.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnHomePrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHomePrev.Location = new System.Drawing.Point(20, 10);
            this.btnHomePrev.Name = "btnHomePrev";
            this.btnHomePrev.Size = new System.Drawing.Size(90, 30);
            this.btnHomePrev.TabIndex = 0;
            this.btnHomePrev.Text = "< Prev";
            this.btnHomePrev.Click += new System.EventHandler(this.btnHomePrev_Click);
            // 
            // lblPopulerGames
            // 
            this.lblPopulerGames.Appearance.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblPopulerGames.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblPopulerGames.Appearance.Options.UseFont = true;
            this.lblPopulerGames.Appearance.Options.UseForeColor = true;
            this.lblPopulerGames.Appearance.Options.UseTextOptions = true;
            this.lblPopulerGames.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            this.lblPopulerGames.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPopulerGames.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPopulerGames.Location = new System.Drawing.Point(0, 0);
            this.lblPopulerGames.Name = "lblPopulerGames";
            this.lblPopulerGames.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblPopulerGames.Size = new System.Drawing.Size(1278, 70);
            this.lblPopulerGames.TabIndex = 22;
            this.lblPopulerGames.Text = "Popular Games";
            // 
            // pageLibrary
            // 
            this.pageLibrary.Caption = "pageLibrary";
            this.pageLibrary.Controls.Add(this.flowLayoutPanelLibrary);
            this.pageLibrary.Controls.Add(this.panelLibPagination);
            this.pageLibrary.Controls.Add(this.panelControl6);
            this.pageLibrary.Name = "pageLibrary";
            this.pageLibrary.Size = new System.Drawing.Size(1278, 718);
            // 
            // flowLayoutPanelLibrary
            // 
            this.flowLayoutPanelLibrary.AutoScroll = true;
            this.flowLayoutPanelLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelLibrary.Location = new System.Drawing.Point(0, 70);
            this.flowLayoutPanelLibrary.Name = "flowLayoutPanelLibrary";
            this.flowLayoutPanelLibrary.Padding = new System.Windows.Forms.Padding(20);
            this.flowLayoutPanelLibrary.Size = new System.Drawing.Size(1278, 598);
            this.flowLayoutPanelLibrary.TabIndex = 18;
            // 
            // panelLibPagination
            // 
            this.panelLibPagination.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.panelLibPagination.Appearance.Options.UseBackColor = true;
            this.panelLibPagination.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelLibPagination.Controls.Add(this.lblLibPage);
            this.panelLibPagination.Controls.Add(this.btnLibNext);
            this.panelLibPagination.Controls.Add(this.btnLibPrev);
            this.panelLibPagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelLibPagination.Location = new System.Drawing.Point(0, 668);
            this.panelLibPagination.Name = "panelLibPagination";
            this.panelLibPagination.Size = new System.Drawing.Size(1278, 50);
            this.panelLibPagination.TabIndex = 19;
            // 
            // lblLibPage
            // 
            this.lblLibPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLibPage.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblLibPage.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.lblLibPage.Appearance.Options.UseFont = true;
            this.lblLibPage.Appearance.Options.UseForeColor = true;
            this.lblLibPage.Appearance.Options.UseTextOptions = true;
            this.lblLibPage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblLibPage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblLibPage.Location = new System.Drawing.Point(120, 15);
            this.lblLibPage.Name = "lblLibPage";
            this.lblLibPage.Size = new System.Drawing.Size(1038, 20);
            this.lblLibPage.TabIndex = 2;
            this.lblLibPage.Text = "Page 1 / 1";
            // 
            // btnLibNext
            // 
            this.btnLibNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLibNext.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnLibNext.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLibNext.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnLibNext.Appearance.Options.UseBackColor = true;
            this.btnLibNext.Appearance.Options.UseFont = true;
            this.btnLibNext.Appearance.Options.UseForeColor = true;
            this.btnLibNext.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibNext.Location = new System.Drawing.Point(1166, 10);
            this.btnLibNext.Name = "btnLibNext";
            this.btnLibNext.Size = new System.Drawing.Size(90, 30);
            this.btnLibNext.TabIndex = 1;
            this.btnLibNext.Text = "Next >";
            this.btnLibNext.Click += new System.EventHandler(this.btnLibNext_Click);
            // 
            // btnLibPrev
            // 
            this.btnLibPrev.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(44)))), ((int)(((byte)(60)))));
            this.btnLibPrev.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLibPrev.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnLibPrev.Appearance.Options.UseBackColor = true;
            this.btnLibPrev.Appearance.Options.UseFont = true;
            this.btnLibPrev.Appearance.Options.UseForeColor = true;
            this.btnLibPrev.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibPrev.Location = new System.Drawing.Point(20, 10);
            this.btnLibPrev.Name = "btnLibPrev";
            this.btnLibPrev.Size = new System.Drawing.Size(90, 30);
            this.btnLibPrev.TabIndex = 0;
            this.btnLibPrev.Text = "< Prev";
            this.btnLibPrev.Click += new System.EventHandler(this.btnLibPrev_Click);
            // 
            // panelControl6
            // 
            this.panelControl6.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.panelControl6.Appearance.Options.UseBackColor = true;
            this.panelControl6.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl6.Controls.Add(this.btnLibPlayed);
            this.panelControl6.Controls.Add(this.lblMyLibrary);
            this.panelControl6.Controls.Add(this.btnLibPlaying);
            this.panelControl6.Controls.Add(this.btnLibDropped);
            this.panelControl6.Controls.Add(this.btnLibPlanToPlay);
            this.panelControl6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl6.Location = new System.Drawing.Point(0, 0);
            this.panelControl6.Name = "panelControl6";
            this.panelControl6.Size = new System.Drawing.Size(1278, 70);
            this.panelControl6.TabIndex = 0;
            // 
            // btnLibPlayed
            // 
            this.btnLibPlayed.AllowFocus = false;
            this.btnLibPlayed.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.btnLibPlayed.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLibPlayed.Appearance.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.btnLibPlayed.Appearance.Options.UseBackColor = true;
            this.btnLibPlayed.Appearance.Options.UseFont = true;
            this.btnLibPlayed.Appearance.Options.UseForeColor = true;
            this.btnLibPlayed.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibPlayed.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibPlayed.Location = new System.Drawing.Point(310, 20);
            this.btnLibPlayed.Name = "btnLibPlayed";
            this.btnLibPlayed.Size = new System.Drawing.Size(100, 35);
            this.btnLibPlayed.TabIndex = 15;
            this.btnLibPlayed.Tag = "Played";
            this.btnLibPlayed.Text = "Played";
            this.btnLibPlayed.Click += new System.EventHandler(this.FilterLibrary_Click);
            // 
            // lblMyLibrary
            // 
            this.lblMyLibrary.Appearance.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblMyLibrary.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblMyLibrary.Appearance.Options.UseFont = true;
            this.lblMyLibrary.Appearance.Options.UseForeColor = true;
            this.lblMyLibrary.Location = new System.Drawing.Point(20, 12);
            this.lblMyLibrary.Name = "lblMyLibrary";
            this.lblMyLibrary.Size = new System.Drawing.Size(163, 45);
            this.lblMyLibrary.TabIndex = 12;
            this.lblMyLibrary.Text = "My Library";
            // 
            // btnLibPlaying
            // 
            this.btnLibPlaying.AllowFocus = false;
            this.btnLibPlaying.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.btnLibPlaying.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLibPlaying.Appearance.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.btnLibPlaying.Appearance.Options.UseBackColor = true;
            this.btnLibPlaying.Appearance.Options.UseFont = true;
            this.btnLibPlaying.Appearance.Options.UseForeColor = true;
            this.btnLibPlaying.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibPlaying.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibPlaying.Location = new System.Drawing.Point(420, 20);
            this.btnLibPlaying.Name = "btnLibPlaying";
            this.btnLibPlaying.Size = new System.Drawing.Size(100, 35);
            this.btnLibPlaying.TabIndex = 16;
            this.btnLibPlaying.Tag = "Playing";
            this.btnLibPlaying.Text = "Playing";
            this.btnLibPlaying.Click += new System.EventHandler(this.FilterLibrary_Click);
            // 
            // btnLibDropped
            // 
            this.btnLibDropped.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.btnLibDropped.AllowFocus = false;
            this.btnLibDropped.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.btnLibDropped.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLibDropped.Appearance.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.btnLibDropped.Appearance.Options.UseBackColor = true;
            this.btnLibDropped.Appearance.Options.UseFont = true;
            this.btnLibDropped.Appearance.Options.UseForeColor = true;
            this.btnLibDropped.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibDropped.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibDropped.Location = new System.Drawing.Point(200, 20);
            this.btnLibDropped.Name = "btnLibDropped";
            this.btnLibDropped.Size = new System.Drawing.Size(100, 35);
            this.btnLibDropped.TabIndex = 13;
            this.btnLibDropped.Tag = "Dropped";
            this.btnLibDropped.Text = "Dropped";
            this.btnLibDropped.Click += new System.EventHandler(this.FilterLibrary_Click);
            // 
            // btnLibPlanToPlay
            // 
            this.btnLibPlanToPlay.AllowFocus = false;
            this.btnLibPlanToPlay.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.btnLibPlanToPlay.Appearance.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLibPlanToPlay.Appearance.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.btnLibPlanToPlay.Appearance.Options.UseBackColor = true;
            this.btnLibPlanToPlay.Appearance.Options.UseFont = true;
            this.btnLibPlanToPlay.Appearance.Options.UseForeColor = true;
            this.btnLibPlanToPlay.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibPlanToPlay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibPlanToPlay.Location = new System.Drawing.Point(530, 20);
            this.btnLibPlanToPlay.Name = "btnLibPlanToPlay";
            this.btnLibPlanToPlay.Size = new System.Drawing.Size(100, 35);
            this.btnLibPlanToPlay.TabIndex = 17;
            this.btnLibPlanToPlay.Tag = "PlanToPlay";
            this.btnLibPlanToPlay.Text = "Plan to Play";
            this.btnLibPlanToPlay.Click += new System.EventHandler(this.FilterLibrary_Click);
            // 
            // pageSearch
            // 
            this.pageSearch.Caption = "pageSearch";
            this.pageSearch.Controls.Add(this.flowLayoutPanelSearch);
            this.pageSearch.Controls.Add(this.panelSearchPagination);
            this.pageSearch.Controls.Add(this.panelControl7);
            this.pageSearch.Name = "pageSearch";
            this.pageSearch.Size = new System.Drawing.Size(1278, 718);
            // 
            // flowLayoutPanelSearch
            // 
            this.flowLayoutPanelSearch.AutoScroll = true;
            this.flowLayoutPanelSearch.Controls.Add(this.lblNoResult);
            this.flowLayoutPanelSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelSearch.Location = new System.Drawing.Point(0, 70);
            this.flowLayoutPanelSearch.Name = "flowLayoutPanelSearch";
            this.flowLayoutPanelSearch.Padding = new System.Windows.Forms.Padding(20);
            this.flowLayoutPanelSearch.Size = new System.Drawing.Size(1278, 598);
            this.flowLayoutPanelSearch.TabIndex = 16;
            // 
            // lblNoResult
            // 
            this.lblNoResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoResult.Appearance.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNoResult.Appearance.Options.UseFont = true;
            this.lblNoResult.Appearance.Options.UseTextOptions = true;
            this.lblNoResult.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblNoResult.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.lblNoResult.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblNoResult.Location = new System.Drawing.Point(23, 23);
            this.lblNoResult.Name = "lblNoResult";
            this.lblNoResult.Size = new System.Drawing.Size(1197, 52);
            this.lblNoResult.TabIndex = 0;
            // 
            // panelSearchPagination
            // 
            this.panelSearchPagination.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.panelSearchPagination.Appearance.Options.UseBackColor = true;
            this.panelSearchPagination.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelSearchPagination.Controls.Add(this.lblSearchPage);
            this.panelSearchPagination.Controls.Add(this.btnSearchNext);
            this.panelSearchPagination.Controls.Add(this.btnSearchPrev);
            this.panelSearchPagination.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSearchPagination.Location = new System.Drawing.Point(0, 668);
            this.panelSearchPagination.Name = "panelSearchPagination";
            this.panelSearchPagination.Size = new System.Drawing.Size(1278, 50);
            this.panelSearchPagination.TabIndex = 20;
            // 
            // lblSearchPage
            // 
            this.lblSearchPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchPage.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSearchPage.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.lblSearchPage.Appearance.Options.UseFont = true;
            this.lblSearchPage.Appearance.Options.UseForeColor = true;
            this.lblSearchPage.Appearance.Options.UseTextOptions = true;
            this.lblSearchPage.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblSearchPage.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblSearchPage.Location = new System.Drawing.Point(120, 15);
            this.lblSearchPage.Name = "lblSearchPage";
            this.lblSearchPage.Size = new System.Drawing.Size(1038, 20);
            this.lblSearchPage.TabIndex = 2;
            this.lblSearchPage.Text = "Page 1 / 1";
            // 
            // btnSearchNext
            // 
            this.btnSearchNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSearchNext.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnSearchNext.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearchNext.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnSearchNext.Appearance.Options.UseBackColor = true;
            this.btnSearchNext.Appearance.Options.UseFont = true;
            this.btnSearchNext.Appearance.Options.UseForeColor = true;
            this.btnSearchNext.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnSearchNext.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchNext.Location = new System.Drawing.Point(1166, 10);
            this.btnSearchNext.Name = "btnSearchNext";
            this.btnSearchNext.Size = new System.Drawing.Size(90, 30);
            this.btnSearchNext.TabIndex = 1;
            this.btnSearchNext.Text = "Next >";
            this.btnSearchNext.Click += new System.EventHandler(this.btnSearchNext_Click);
            // 
            // btnSearchPrev
            // 
            this.btnSearchPrev.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(44)))), ((int)(((byte)(60)))));
            this.btnSearchPrev.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearchPrev.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnSearchPrev.Appearance.Options.UseBackColor = true;
            this.btnSearchPrev.Appearance.Options.UseFont = true;
            this.btnSearchPrev.Appearance.Options.UseForeColor = true;
            this.btnSearchPrev.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnSearchPrev.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSearchPrev.Location = new System.Drawing.Point(20, 10);
            this.btnSearchPrev.Name = "btnSearchPrev";
            this.btnSearchPrev.Size = new System.Drawing.Size(90, 30);
            this.btnSearchPrev.TabIndex = 0;
            this.btnSearchPrev.Text = "< Prev";
            this.btnSearchPrev.Click += new System.EventHandler(this.btnSearchPrev_Click);
            // 
            // panelControl7
            // 
            this.panelControl7.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.panelControl7.Appearance.Options.UseBackColor = true;
            this.panelControl7.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl7.Controls.Add(this.searchControlSearchPage);
            this.panelControl7.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl7.Location = new System.Drawing.Point(0, 0);
            this.panelControl7.Name = "panelControl7";
            this.panelControl7.Size = new System.Drawing.Size(1278, 70);
            this.panelControl7.TabIndex = 15;
            // 
            // searchControlSearchPage
            // 
            this.searchControlSearchPage.Location = new System.Drawing.Point(20, 20);
            this.searchControlSearchPage.Name = "searchControlSearchPage";
            this.searchControlSearchPage.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(40)))), ((int)(((byte)(55)))));
            this.searchControlSearchPage.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.searchControlSearchPage.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.searchControlSearchPage.Properties.Appearance.Options.UseBackColor = true;
            this.searchControlSearchPage.Properties.Appearance.Options.UseFont = true;
            this.searchControlSearchPage.Properties.Appearance.Options.UseForeColor = true;
            this.searchControlSearchPage.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.searchControlSearchPage.Properties.NullValuePrompt = "Search games...";
            this.searchControlSearchPage.Properties.ShowClearButton = false;
            this.searchControlSearchPage.Properties.ShowSearchButton = false;
            this.searchControlSearchPage.Size = new System.Drawing.Size(400, 26);
            this.searchControlSearchPage.TabIndex = 14;
            this.searchControlSearchPage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchControlSearchPage_KeyDown_1);
            // 
            // pageSettings
            // 
            this.pageSettings.Caption = "pageSettings";
            this.pageSettings.Controls.Add(this.flowSettingsMain);
            this.pageSettings.Name = "pageSettings";
            this.pageSettings.Size = new System.Drawing.Size(1278, 718);
            // 
            // flowSettingsMain
            // 
            this.flowSettingsMain.AutoScroll = true;
            this.flowSettingsMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.flowSettingsMain.Controls.Add(this.pnlAccountSettings);
            this.flowSettingsMain.Controls.Add(this.pnlAppPreferences);
            this.flowSettingsMain.Controls.Add(this.pnlSystemData);
            this.flowSettingsMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSettingsMain.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowSettingsMain.Location = new System.Drawing.Point(0, 0);
            this.flowSettingsMain.Name = "flowSettingsMain";
            this.flowSettingsMain.Padding = new System.Windows.Forms.Padding(40);
            this.flowSettingsMain.Size = new System.Drawing.Size(1278, 718);
            this.flowSettingsMain.TabIndex = 0;
            this.flowSettingsMain.WrapContents = false;
            // 
            // pnlAccountSettings
            // 
            this.pnlAccountSettings.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.pnlAccountSettings.Appearance.Options.UseBackColor = true;
            this.pnlAccountSettings.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlAccountSettings.Controls.Add(this.lblPassWarning);
            this.pnlAccountSettings.Controls.Add(this.lblUsernameWarning);
            this.pnlAccountSettings.Controls.Add(this.txtNewPassAgain);
            this.pnlAccountSettings.Controls.Add(this.lblAccountHeader);
            this.pnlAccountSettings.Controls.Add(this.separatorAccount);
            this.pnlAccountSettings.Controls.Add(this.lblChangeUser);
            this.pnlAccountSettings.Controls.Add(this.txtNewUsername);
            this.pnlAccountSettings.Controls.Add(this.btnUpdateUsername);
            this.pnlAccountSettings.Controls.Add(this.lblChangePass);
            this.pnlAccountSettings.Controls.Add(this.txtCurrentPass);
            this.pnlAccountSettings.Controls.Add(this.txtNewPass);
            this.pnlAccountSettings.Controls.Add(this.btnUpdatePass);
            this.pnlAccountSettings.Location = new System.Drawing.Point(40, 40);
            this.pnlAccountSettings.Margin = new System.Windows.Forms.Padding(0, 0, 0, 30);
            this.pnlAccountSettings.Name = "pnlAccountSettings";
            this.pnlAccountSettings.Size = new System.Drawing.Size(800, 260);
            this.pnlAccountSettings.TabIndex = 0;
            // 
            // lblPassWarning
            // 
            this.lblPassWarning.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblPassWarning.Appearance.Options.UseForeColor = true;
            this.lblPassWarning.Appearance.Options.UseTextOptions = true;
            this.lblPassWarning.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblPassWarning.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblPassWarning.Location = new System.Drawing.Point(166, 202);
            this.lblPassWarning.Name = "lblPassWarning";
            this.lblPassWarning.Size = new System.Drawing.Size(154, 28);
            this.lblPassWarning.TabIndex = 11;
            this.lblPassWarning.Text = "Warning";
            this.lblPassWarning.Visible = false;
            // 
            // lblUsernameWarning
            // 
            this.lblUsernameWarning.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.lblUsernameWarning.Appearance.Options.UseForeColor = true;
            this.lblUsernameWarning.Appearance.Options.UseTextOptions = true;
            this.lblUsernameWarning.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblUsernameWarning.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblUsernameWarning.Location = new System.Drawing.Point(526, 127);
            this.lblUsernameWarning.Name = "lblUsernameWarning";
            this.lblUsernameWarning.Size = new System.Drawing.Size(154, 28);
            this.lblUsernameWarning.TabIndex = 10;
            this.lblUsernameWarning.Text = "Warning";
            this.lblUsernameWarning.Visible = false;
            // 
            // txtNewPassAgain
            // 
            this.txtNewPassAgain.Location = new System.Drawing.Point(20, 160);
            this.txtNewPassAgain.Name = "txtNewPassAgain";
            this.txtNewPassAgain.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(48)))), ((int)(((byte)(65)))));
            this.txtNewPassAgain.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNewPassAgain.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtNewPassAgain.Properties.Appearance.Options.UseBackColor = true;
            this.txtNewPassAgain.Properties.Appearance.Options.UseFont = true;
            this.txtNewPassAgain.Properties.Appearance.Options.UseForeColor = true;
            this.txtNewPassAgain.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtNewPassAgain.Properties.NullValuePrompt = "New Password Again";
            this.txtNewPassAgain.Properties.UseSystemPasswordChar = true;
            this.txtNewPassAgain.Size = new System.Drawing.Size(300, 22);
            this.txtNewPassAgain.TabIndex = 9;
            // 
            // lblAccountHeader
            // 
            this.lblAccountHeader.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblAccountHeader.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblAccountHeader.Appearance.Options.UseFont = true;
            this.lblAccountHeader.Appearance.Options.UseForeColor = true;
            this.lblAccountHeader.Location = new System.Drawing.Point(20, 15);
            this.lblAccountHeader.Name = "lblAccountHeader";
            this.lblAccountHeader.Size = new System.Drawing.Size(151, 25);
            this.lblAccountHeader.TabIndex = 0;
            this.lblAccountHeader.Text = "Account Settings";
            // 
            // separatorAccount
            // 
            this.separatorAccount.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(80)))));
            this.separatorAccount.Appearance.Options.UseBackColor = true;
            this.separatorAccount.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.separatorAccount.Location = new System.Drawing.Point(20, 50);
            this.separatorAccount.Name = "separatorAccount";
            this.separatorAccount.Size = new System.Drawing.Size(760, 1);
            this.separatorAccount.TabIndex = 1;
            // 
            // lblChangeUser
            // 
            this.lblChangeUser.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblChangeUser.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.lblChangeUser.Appearance.Options.UseFont = true;
            this.lblChangeUser.Appearance.Options.UseForeColor = true;
            this.lblChangeUser.Location = new System.Drawing.Point(380, 60);
            this.lblChangeUser.Name = "lblChangeUser";
            this.lblChangeUser.Size = new System.Drawing.Size(97, 15);
            this.lblChangeUser.TabIndex = 2;
            this.lblChangeUser.Text = "Change Username";
            // 
            // txtNewUsername
            // 
            this.txtNewUsername.Location = new System.Drawing.Point(380, 85);
            this.txtNewUsername.Name = "txtNewUsername";
            this.txtNewUsername.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(48)))), ((int)(((byte)(65)))));
            this.txtNewUsername.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNewUsername.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtNewUsername.Properties.Appearance.Options.UseBackColor = true;
            this.txtNewUsername.Properties.Appearance.Options.UseFont = true;
            this.txtNewUsername.Properties.Appearance.Options.UseForeColor = true;
            this.txtNewUsername.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtNewUsername.Properties.NullValuePrompt = "New Username";
            this.txtNewUsername.Size = new System.Drawing.Size(300, 22);
            this.txtNewUsername.TabIndex = 3;
            // 
            // btnUpdateUsername
            // 
            this.btnUpdateUsername.AllowFocus = false;
            this.btnUpdateUsername.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnUpdateUsername.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUpdateUsername.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnUpdateUsername.Appearance.Options.UseBackColor = true;
            this.btnUpdateUsername.Appearance.Options.UseFont = true;
            this.btnUpdateUsername.Appearance.Options.UseForeColor = true;
            this.btnUpdateUsername.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnUpdateUsername.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdateUsername.Location = new System.Drawing.Point(380, 127);
            this.btnUpdateUsername.Name = "btnUpdateUsername";
            this.btnUpdateUsername.Size = new System.Drawing.Size(140, 28);
            this.btnUpdateUsername.TabIndex = 4;
            this.btnUpdateUsername.Text = "Update Username";
            this.btnUpdateUsername.Click += new System.EventHandler(this.btnUpdateUsername_Click);
            // 
            // lblChangePass
            // 
            this.lblChangePass.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblChangePass.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.lblChangePass.Appearance.Options.UseFont = true;
            this.lblChangePass.Appearance.Options.UseForeColor = true;
            this.lblChangePass.Location = new System.Drawing.Point(20, 60);
            this.lblChangePass.Name = "lblChangePass";
            this.lblChangePass.Size = new System.Drawing.Size(94, 15);
            this.lblChangePass.TabIndex = 5;
            this.lblChangePass.Text = "Change Password";
            // 
            // txtCurrentPass
            // 
            this.txtCurrentPass.Location = new System.Drawing.Point(20, 85);
            this.txtCurrentPass.Name = "txtCurrentPass";
            this.txtCurrentPass.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(48)))), ((int)(((byte)(65)))));
            this.txtCurrentPass.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtCurrentPass.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtCurrentPass.Properties.Appearance.Options.UseBackColor = true;
            this.txtCurrentPass.Properties.Appearance.Options.UseFont = true;
            this.txtCurrentPass.Properties.Appearance.Options.UseForeColor = true;
            this.txtCurrentPass.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtCurrentPass.Properties.NullValuePrompt = "Current Password";
            this.txtCurrentPass.Properties.UseSystemPasswordChar = true;
            this.txtCurrentPass.Size = new System.Drawing.Size(300, 22);
            this.txtCurrentPass.TabIndex = 6;
            // 
            // txtNewPass
            // 
            this.txtNewPass.Location = new System.Drawing.Point(20, 125);
            this.txtNewPass.Name = "txtNewPass";
            this.txtNewPass.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(48)))), ((int)(((byte)(65)))));
            this.txtNewPass.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNewPass.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.txtNewPass.Properties.Appearance.Options.UseBackColor = true;
            this.txtNewPass.Properties.Appearance.Options.UseFont = true;
            this.txtNewPass.Properties.Appearance.Options.UseForeColor = true;
            this.txtNewPass.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.txtNewPass.Properties.NullValuePrompt = "New Password";
            this.txtNewPass.Properties.UseSystemPasswordChar = true;
            this.txtNewPass.Size = new System.Drawing.Size(300, 22);
            this.txtNewPass.TabIndex = 7;
            // 
            // btnUpdatePass
            // 
            this.btnUpdatePass.AllowFocus = false;
            this.btnUpdatePass.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.btnUpdatePass.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUpdatePass.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnUpdatePass.Appearance.Options.UseBackColor = true;
            this.btnUpdatePass.Appearance.Options.UseFont = true;
            this.btnUpdatePass.Appearance.Options.UseForeColor = true;
            this.btnUpdatePass.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnUpdatePass.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdatePass.Location = new System.Drawing.Point(20, 200);
            this.btnUpdatePass.Name = "btnUpdatePass";
            this.btnUpdatePass.Size = new System.Drawing.Size(140, 30);
            this.btnUpdatePass.TabIndex = 8;
            this.btnUpdatePass.Text = "Update Password";
            this.btnUpdatePass.Click += new System.EventHandler(this.btnUpdatePass_Click);
            // 
            // pnlAppPreferences
            // 
            this.pnlAppPreferences.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.pnlAppPreferences.Appearance.Options.UseBackColor = true;
            this.pnlAppPreferences.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlAppPreferences.Controls.Add(this.lblPrefHeader);
            this.pnlAppPreferences.Controls.Add(this.separatorPref);
            this.pnlAppPreferences.Controls.Add(this.lblStartPage);
            this.pnlAppPreferences.Controls.Add(this.cmbStartPage);
            this.pnlAppPreferences.Controls.Add(this.lblNSFW);
            this.pnlAppPreferences.Controls.Add(this.toggleNSFW);
            this.pnlAppPreferences.Location = new System.Drawing.Point(40, 330);
            this.pnlAppPreferences.Margin = new System.Windows.Forms.Padding(0, 0, 0, 30);
            this.pnlAppPreferences.Name = "pnlAppPreferences";
            this.pnlAppPreferences.Size = new System.Drawing.Size(800, 140);
            this.pnlAppPreferences.TabIndex = 1;
            // 
            // lblPrefHeader
            // 
            this.lblPrefHeader.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblPrefHeader.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblPrefHeader.Appearance.Options.UseFont = true;
            this.lblPrefHeader.Appearance.Options.UseForeColor = true;
            this.lblPrefHeader.Location = new System.Drawing.Point(20, 15);
            this.lblPrefHeader.Name = "lblPrefHeader";
            this.lblPrefHeader.Size = new System.Drawing.Size(146, 25);
            this.lblPrefHeader.TabIndex = 0;
            this.lblPrefHeader.Text = "App Preferences";
            // 
            // separatorPref
            // 
            this.separatorPref.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(80)))));
            this.separatorPref.Appearance.Options.UseBackColor = true;
            this.separatorPref.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.separatorPref.Location = new System.Drawing.Point(20, 50);
            this.separatorPref.Name = "separatorPref";
            this.separatorPref.Size = new System.Drawing.Size(760, 1);
            this.separatorPref.TabIndex = 1;
            // 
            // lblStartPage
            // 
            this.lblStartPage.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStartPage.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.lblStartPage.Appearance.Options.UseFont = true;
            this.lblStartPage.Appearance.Options.UseForeColor = true;
            this.lblStartPage.Location = new System.Drawing.Point(20, 60);
            this.lblStartPage.Name = "lblStartPage";
            this.lblStartPage.Size = new System.Drawing.Size(53, 15);
            this.lblStartPage.TabIndex = 2;
            this.lblStartPage.Text = "Start Page";
            // 
            // cmbStartPage
            // 
            this.cmbStartPage.Location = new System.Drawing.Point(20, 85);
            this.cmbStartPage.Name = "cmbStartPage";
            this.cmbStartPage.Properties.AllowFocused = false;
            this.cmbStartPage.Properties.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(48)))), ((int)(((byte)(65)))));
            this.cmbStartPage.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbStartPage.Properties.Appearance.ForeColor = System.Drawing.Color.White;
            this.cmbStartPage.Properties.Appearance.Options.UseBackColor = true;
            this.cmbStartPage.Properties.Appearance.Options.UseFont = true;
            this.cmbStartPage.Properties.Appearance.Options.UseForeColor = true;
            this.cmbStartPage.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cmbStartPage.Properties.Items.AddRange(new object[] {
            "Popular Games (Home)",
            "My Library"});
            this.cmbStartPage.Size = new System.Drawing.Size(200, 22);
            this.cmbStartPage.TabIndex = 3;
            this.cmbStartPage.SelectedIndexChanged += new System.EventHandler(this.cmbStartPage_SelectedIndexChanged);
            // 
            // lblNSFW
            // 
            this.lblNSFW.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblNSFW.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.lblNSFW.Appearance.Options.UseFont = true;
            this.lblNSFW.Appearance.Options.UseForeColor = true;
            this.lblNSFW.Location = new System.Drawing.Point(380, 60);
            this.lblNSFW.Name = "lblNSFW";
            this.lblNSFW.Size = new System.Drawing.Size(159, 15);
            this.lblNSFW.TabIndex = 4;
            this.lblNSFW.Text = "Show Mature Content (NSFW)";
            // 
            // toggleNSFW
            // 
            this.toggleNSFW.Location = new System.Drawing.Point(380, 85);
            this.toggleNSFW.Name = "toggleNSFW";
            this.toggleNSFW.Properties.AllowFocused = false;
            this.toggleNSFW.Properties.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.toggleNSFW.Properties.Appearance.ForeColor = System.Drawing.Color.Silver;
            this.toggleNSFW.Properties.Appearance.Options.UseFont = true;
            this.toggleNSFW.Properties.Appearance.Options.UseForeColor = true;
            this.toggleNSFW.Properties.OffText = "Off";
            this.toggleNSFW.Properties.OnText = "On";
            this.toggleNSFW.Size = new System.Drawing.Size(150, 22);
            this.toggleNSFW.TabIndex = 5;
            this.toggleNSFW.Toggled += new System.EventHandler(this.toggleNSFW_Toggled);
            // 
            // pnlSystemData
            // 
            this.pnlSystemData.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(34)))), ((int)(((byte)(50)))));
            this.pnlSystemData.Appearance.Options.UseBackColor = true;
            this.pnlSystemData.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlSystemData.Controls.Add(this.lblSystemHeader);
            this.pnlSystemData.Controls.Add(this.separatorSystem);
            this.pnlSystemData.Controls.Add(this.btnClearCache);
            this.pnlSystemData.Controls.Add(this.btnLogout);
            this.pnlSystemData.Location = new System.Drawing.Point(40, 500);
            this.pnlSystemData.Margin = new System.Windows.Forms.Padding(0, 0, 0, 30);
            this.pnlSystemData.Name = "pnlSystemData";
            this.pnlSystemData.Size = new System.Drawing.Size(800, 140);
            this.pnlSystemData.TabIndex = 2;
            // 
            // lblSystemHeader
            // 
            this.lblSystemHeader.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblSystemHeader.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblSystemHeader.Appearance.Options.UseFont = true;
            this.lblSystemHeader.Appearance.Options.UseForeColor = true;
            this.lblSystemHeader.Location = new System.Drawing.Point(20, 15);
            this.lblSystemHeader.Name = "lblSystemHeader";
            this.lblSystemHeader.Size = new System.Drawing.Size(148, 25);
            this.lblSystemHeader.TabIndex = 0;
            this.lblSystemHeader.Text = "System and Data";
            // 
            // separatorSystem
            // 
            this.separatorSystem.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(80)))));
            this.separatorSystem.Appearance.Options.UseBackColor = true;
            this.separatorSystem.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.separatorSystem.Location = new System.Drawing.Point(20, 50);
            this.separatorSystem.Name = "separatorSystem";
            this.separatorSystem.Size = new System.Drawing.Size(760, 1);
            this.separatorSystem.TabIndex = 1;
            // 
            // btnClearCache
            // 
            this.btnClearCache.AllowFocus = false;
            this.btnClearCache.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(140)))), ((int)(((byte)(0)))));
            this.btnClearCache.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClearCache.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnClearCache.Appearance.Options.UseBackColor = true;
            this.btnClearCache.Appearance.Options.UseFont = true;
            this.btnClearCache.Appearance.Options.UseForeColor = true;
            this.btnClearCache.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnClearCache.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearCache.Location = new System.Drawing.Point(20, 70);
            this.btnClearCache.Name = "btnClearCache";
            this.btnClearCache.Size = new System.Drawing.Size(200, 30);
            this.btnClearCache.TabIndex = 2;
            this.btnClearCache.Text = "Clear Image Cache";
            this.btnClearCache.Click += new System.EventHandler(this.btnClearCache_Click);
            // 
            // btnLogout
            // 
            this.btnLogout.AllowFocus = false;
            this.btnLogout.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(53)))), ((int)(((byte)(69)))));
            this.btnLogout.Appearance.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLogout.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnLogout.Appearance.Options.UseBackColor = true;
            this.btnLogout.Appearance.Options.UseFont = true;
            this.btnLogout.Appearance.Options.UseForeColor = true;
            this.btnLogout.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.Location = new System.Drawing.Point(380, 70);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(150, 30);
            this.btnLogout.TabIndex = 3;
            this.btnLogout.Text = "Log Out";
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // pageGameDetail
            // 
            this.pageGameDetail.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.pageGameDetail.Appearance.Options.UseBackColor = true;
            this.pageGameDetail.Caption = "pageGameDetail";
            this.pageGameDetail.Controls.Add(this.scrollableDetailContainer);
            this.pageGameDetail.Name = "pageGameDetail";
            this.pageGameDetail.Size = new System.Drawing.Size(1278, 718);
            // 
            // scrollableDetailContainer
            // 
            this.scrollableDetailContainer.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.scrollableDetailContainer.Appearance.Options.UseBackColor = true;
            this.scrollableDetailContainer.Controls.Add(this.lblDetailRequirements);
            this.scrollableDetailContainer.Controls.Add(this.lblRequirementsHeader);
            this.scrollableDetailContainer.Controls.Add(this.lblDetailDescription);
            this.scrollableDetailContainer.Controls.Add(this.lblDescriptionHeader);
            this.scrollableDetailContainer.Controls.Add(this.panelControl2);
            this.scrollableDetailContainer.Controls.Add(this.panelTopSection);
            this.scrollableDetailContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollableDetailContainer.Location = new System.Drawing.Point(0, 0);
            this.scrollableDetailContainer.Name = "scrollableDetailContainer";
            this.scrollableDetailContainer.Padding = new System.Windows.Forms.Padding(0, 0, 0, 50);
            this.scrollableDetailContainer.Size = new System.Drawing.Size(1278, 718);
            this.scrollableDetailContainer.TabIndex = 5;
            // 
            // lblDetailRequirements
            // 
            this.lblDetailRequirements.Appearance.Font = new System.Drawing.Font("Consolas", 10F);
            this.lblDetailRequirements.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.lblDetailRequirements.Appearance.Options.UseFont = true;
            this.lblDetailRequirements.Appearance.Options.UseForeColor = true;
            this.lblDetailRequirements.Appearance.Options.UseTextOptions = true;
            this.lblDetailRequirements.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblDetailRequirements.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.lblDetailRequirements.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailRequirements.Location = new System.Drawing.Point(0, 880);
            this.lblDetailRequirements.Name = "lblDetailRequirements";
            this.lblDetailRequirements.Padding = new System.Windows.Forms.Padding(20, 0, 20, 50);
            this.lblDetailRequirements.Size = new System.Drawing.Size(1261, 65);
            this.lblDetailRequirements.TabIndex = 12;
            this.lblDetailRequirements.Text = "Requirements...";
            // 
            // lblRequirementsHeader
            // 
            this.lblRequirementsHeader.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblRequirementsHeader.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblRequirementsHeader.Appearance.Options.UseFont = true;
            this.lblRequirementsHeader.Appearance.Options.UseForeColor = true;
            this.lblRequirementsHeader.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.lblRequirementsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRequirementsHeader.Location = new System.Drawing.Point(0, 810);
            this.lblRequirementsHeader.Name = "lblRequirementsHeader";
            this.lblRequirementsHeader.Padding = new System.Windows.Forms.Padding(20, 30, 0, 10);
            this.lblRequirementsHeader.Size = new System.Drawing.Size(1261, 70);
            this.lblRequirementsHeader.TabIndex = 11;
            this.lblRequirementsHeader.Text = "System Requirements";
            // 
            // lblDetailDescription
            // 
            this.lblDetailDescription.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.lblDetailDescription.Appearance.ForeColor = System.Drawing.Color.Silver;
            this.lblDetailDescription.Appearance.Options.UseFont = true;
            this.lblDetailDescription.Appearance.Options.UseForeColor = true;
            this.lblDetailDescription.Appearance.Options.UseTextOptions = true;
            this.lblDetailDescription.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.lblDetailDescription.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblDetailDescription.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.lblDetailDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetailDescription.Location = new System.Drawing.Point(0, 790);
            this.lblDetailDescription.Name = "lblDetailDescription";
            this.lblDetailDescription.Padding = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.lblDetailDescription.Size = new System.Drawing.Size(1261, 20);
            this.lblDetailDescription.TabIndex = 10;
            this.lblDetailDescription.Text = "Game description...";
            // 
            // lblDescriptionHeader
            // 
            this.lblDescriptionHeader.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblDescriptionHeader.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblDescriptionHeader.Appearance.Options.UseFont = true;
            this.lblDescriptionHeader.Appearance.Options.UseForeColor = true;
            this.lblDescriptionHeader.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.lblDescriptionHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDescriptionHeader.Location = new System.Drawing.Point(0, 730);
            this.lblDescriptionHeader.Name = "lblDescriptionHeader";
            this.lblDescriptionHeader.Padding = new System.Windows.Forms.Padding(20, 20, 0, 10);
            this.lblDescriptionHeader.Size = new System.Drawing.Size(1261, 60);
            this.lblDescriptionHeader.TabIndex = 9;
            this.lblDescriptionHeader.Text = "About";
            // 
            // panelControl2
            // 
            this.panelControl2.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.panelControl2.Appearance.Options.UseBackColor = true;
            this.panelControl2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelControl2.Controls.Add(this.flowLayoutScreenshots);
            this.panelControl2.Controls.Add(this.lblSS);
            this.panelControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl2.Location = new System.Drawing.Point(0, 450);
            this.panelControl2.Name = "panelControl2";
            this.panelControl2.Size = new System.Drawing.Size(1261, 280);
            this.panelControl2.TabIndex = 26;
            // 
            // flowLayoutScreenshots
            // 
            this.flowLayoutScreenshots.AutoScroll = true;
            this.flowLayoutScreenshots.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.flowLayoutScreenshots.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutScreenshots.Location = new System.Drawing.Point(0, 60);
            this.flowLayoutScreenshots.Name = "flowLayoutScreenshots";
            this.flowLayoutScreenshots.Padding = new System.Windows.Forms.Padding(0, 10, 0, 20);
            this.flowLayoutScreenshots.Size = new System.Drawing.Size(1261, 240);
            this.flowLayoutScreenshots.TabIndex = 8;
            this.flowLayoutScreenshots.WrapContents = false;
            // 
            // lblSS
            // 
            this.lblSS.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblSS.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblSS.Appearance.Options.UseFont = true;
            this.lblSS.Appearance.Options.UseForeColor = true;
            this.lblSS.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.lblSS.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSS.Location = new System.Drawing.Point(0, 0);
            this.lblSS.Name = "lblSS";
            this.lblSS.Padding = new System.Windows.Forms.Padding(20, 20, 0, 10);
            this.lblSS.Size = new System.Drawing.Size(1261, 60);
            this.lblSS.TabIndex = 10;
            this.lblSS.Text = "Screen Shots";
            // 
            // panelTopSection
            // 
            this.panelTopSection.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.panelTopSection.Appearance.Options.UseBackColor = true;
            this.panelTopSection.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.panelTopSection.Controls.Add(this.btnLibraryAction);
            this.panelTopSection.Controls.Add(this.lblDetailAge);
            this.panelTopSection.Controls.Add(this.lblDetailModes);
            this.panelTopSection.Controls.Add(this.lblDetailStores);
            this.panelTopSection.Controls.Add(this.lblDetailPlatforms);
            this.panelTopSection.Controls.Add(this.btnDetailBack);
            this.panelTopSection.Controls.Add(this.peDetailImage);
            this.panelTopSection.Controls.Add(this.lblDetailTitle);
            this.panelTopSection.Controls.Add(this.lblDetailDeveloper);
            this.panelTopSection.Controls.Add(this.lblDetailGenres);
            this.panelTopSection.Controls.Add(this.lblDetailRating);
            this.panelTopSection.Controls.Add(this.lblDetailMetacritic);
            this.panelTopSection.Controls.Add(this.lblDetailPlaytime);
            this.panelTopSection.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTopSection.Location = new System.Drawing.Point(0, 0);
            this.panelTopSection.Name = "panelTopSection";
            this.panelTopSection.Size = new System.Drawing.Size(1261, 450);
            this.panelTopSection.TabIndex = 0;
            // 
            // btnLibraryAction
            // 
            this.btnLibraryAction.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLibraryAction.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(167)))), ((int)(((byte)(69)))));
            this.btnLibraryAction.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLibraryAction.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnLibraryAction.Appearance.Options.UseBackColor = true;
            this.btnLibraryAction.Appearance.Options.UseFont = true;
            this.btnLibraryAction.Appearance.Options.UseForeColor = true;
            this.btnLibraryAction.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnLibraryAction.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLibraryAction.Location = new System.Drawing.Point(992, 70);
            this.btnLibraryAction.Name = "btnLibraryAction";
            this.btnLibraryAction.Size = new System.Drawing.Size(220, 50);
            this.btnLibraryAction.TabIndex = 20;
            this.btnLibraryAction.Text = "+ Add to Library";
            this.btnLibraryAction.Click += new System.EventHandler(this.btnLibraryAction_Click);
            // 
            // lblDetailAge
            // 
            this.lblDetailAge.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblDetailAge.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDetailAge.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblDetailAge.Appearance.Options.UseBackColor = true;
            this.lblDetailAge.Appearance.Options.UseFont = true;
            this.lblDetailAge.Appearance.Options.UseForeColor = true;
            this.lblDetailAge.Appearance.Options.UseTextOptions = true;
            this.lblDetailAge.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblDetailAge.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblDetailAge.Location = new System.Drawing.Point(530, 290);
            this.lblDetailAge.Name = "lblDetailAge";
            this.lblDetailAge.Size = new System.Drawing.Size(140, 30);
            this.lblDetailAge.TabIndex = 21;
            this.lblDetailAge.Text = "ESRB";
            // 
            // lblDetailModes
            // 
            this.lblDetailModes.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDetailModes.Appearance.ForeColor = System.Drawing.Color.Silver;
            this.lblDetailModes.Appearance.Options.UseFont = true;
            this.lblDetailModes.Appearance.Options.UseForeColor = true;
            this.lblDetailModes.Location = new System.Drawing.Point(530, 360);
            this.lblDetailModes.Name = "lblDetailModes";
            this.lblDetailModes.Size = new System.Drawing.Size(71, 17);
            this.lblDetailModes.TabIndex = 24;
            this.lblDetailModes.Text = "Singleplayer";
            // 
            // lblDetailStores
            // 
            this.lblDetailStores.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblDetailStores.Appearance.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(149)))), ((int)(((byte)(237)))));
            this.lblDetailStores.Appearance.Options.UseFont = true;
            this.lblDetailStores.Appearance.Options.UseForeColor = true;
            this.lblDetailStores.Location = new System.Drawing.Point(530, 205);
            this.lblDetailStores.Name = "lblDetailStores";
            this.lblDetailStores.Size = new System.Drawing.Size(111, 17);
            this.lblDetailStores.TabIndex = 23;
            this.lblDetailStores.Text = "Steam, Epic Games";
            // 
            // lblDetailPlatforms
            // 
            this.lblDetailPlatforms.Appearance.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblDetailPlatforms.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblDetailPlatforms.Appearance.Options.UseFont = true;
            this.lblDetailPlatforms.Appearance.Options.UseForeColor = true;
            this.lblDetailPlatforms.Location = new System.Drawing.Point(530, 180);
            this.lblDetailPlatforms.Name = "lblDetailPlatforms";
            this.lblDetailPlatforms.Size = new System.Drawing.Size(221, 20);
            this.lblDetailPlatforms.TabIndex = 22;
            this.lblDetailPlatforms.Text = "PC, PlayStation 5, Xbox Series X";
            // 
            // btnDetailBack
            // 
            this.btnDetailBack.AllowFocus = false;
            this.btnDetailBack.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.btnDetailBack.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnDetailBack.Appearance.ForeColor = System.Drawing.Color.White;
            this.btnDetailBack.Appearance.Options.UseBackColor = true;
            this.btnDetailBack.Appearance.Options.UseFont = true;
            this.btnDetailBack.Appearance.Options.UseForeColor = true;
            this.btnDetailBack.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.btnDetailBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDetailBack.Location = new System.Drawing.Point(20, 10);
            this.btnDetailBack.Name = "btnDetailBack";
            this.btnDetailBack.Size = new System.Drawing.Size(100, 40);
            this.btnDetailBack.TabIndex = 0;
            this.btnDetailBack.Text = "< Back";
            this.btnDetailBack.Click += new System.EventHandler(this.btnDetailBack_Click);
            // 
            // peDetailImage
            // 
            this.peDetailImage.Location = new System.Drawing.Point(30, 70);
            this.peDetailImage.Name = "peDetailImage";
            this.peDetailImage.Properties.AllowFocused = false;
            this.peDetailImage.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.peDetailImage.Properties.Appearance.Options.UseBackColor = true;
            this.peDetailImage.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.peDetailImage.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this.peDetailImage.Properties.ShowMenu = false;
            this.peDetailImage.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            this.peDetailImage.Size = new System.Drawing.Size(480, 320);
            this.peDetailImage.TabIndex = 1;
            // 
            // lblDetailTitle
            // 
            this.lblDetailTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDetailTitle.Appearance.Font = new System.Drawing.Font("Segoe UI", 28F, System.Drawing.FontStyle.Bold);
            this.lblDetailTitle.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblDetailTitle.Appearance.Options.UseFont = true;
            this.lblDetailTitle.Appearance.Options.UseForeColor = true;
            this.lblDetailTitle.Appearance.Options.UseTextOptions = true;
            this.lblDetailTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.lblDetailTitle.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.lblDetailTitle.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.lblDetailTitle.Location = new System.Drawing.Point(530, 65);
            this.lblDetailTitle.Name = "lblDetailTitle";
            this.lblDetailTitle.Size = new System.Drawing.Size(1529, 50);
            this.lblDetailTitle.TabIndex = 2;
            this.lblDetailTitle.Text = "Game Title";
            // 
            // lblDetailDeveloper
            // 
            this.lblDetailDeveloper.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblDetailDeveloper.Appearance.ForeColor = System.Drawing.Color.LightGray;
            this.lblDetailDeveloper.Appearance.Options.UseFont = true;
            this.lblDetailDeveloper.Appearance.Options.UseForeColor = true;
            this.lblDetailDeveloper.Location = new System.Drawing.Point(530, 120);
            this.lblDetailDeveloper.Name = "lblDetailDeveloper";
            this.lblDetailDeveloper.Size = new System.Drawing.Size(153, 21);
            this.lblDetailDeveloper.TabIndex = 3;
            this.lblDetailDeveloper.Text = "Released: 2024-01-01";
            // 
            // lblDetailGenres
            // 
            this.lblDetailGenres.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            this.lblDetailGenres.Appearance.ForeColor = System.Drawing.Color.Gray;
            this.lblDetailGenres.Appearance.Options.UseFont = true;
            this.lblDetailGenres.Appearance.Options.UseForeColor = true;
            this.lblDetailGenres.Location = new System.Drawing.Point(530, 150);
            this.lblDetailGenres.Name = "lblDetailGenres";
            this.lblDetailGenres.Size = new System.Drawing.Size(66, 17);
            this.lblDetailGenres.TabIndex = 6;
            this.lblDetailGenres.Text = "Action, RPG";
            // 
            // lblDetailRating
            // 
            this.lblDetailRating.Appearance.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblDetailRating.Appearance.ForeColor = System.Drawing.Color.Gold;
            this.lblDetailRating.Appearance.Options.UseFont = true;
            this.lblDetailRating.Appearance.Options.UseForeColor = true;
            this.lblDetailRating.Location = new System.Drawing.Point(600, 246);
            this.lblDetailRating.Name = "lblDetailRating";
            this.lblDetailRating.Size = new System.Drawing.Size(67, 32);
            this.lblDetailRating.TabIndex = 4;
            this.lblDetailRating.Text = "★ 4.5";
            // 
            // lblDetailMetacritic
            // 
            this.lblDetailMetacritic.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(106)))), ((int)(((byte)(186)))), ((int)(((byte)(46)))));
            this.lblDetailMetacritic.Appearance.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblDetailMetacritic.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblDetailMetacritic.Appearance.Options.UseBackColor = true;
            this.lblDetailMetacritic.Appearance.Options.UseFont = true;
            this.lblDetailMetacritic.Appearance.Options.UseForeColor = true;
            this.lblDetailMetacritic.Appearance.Options.UseTextOptions = true;
            this.lblDetailMetacritic.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.lblDetailMetacritic.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.lblDetailMetacritic.Location = new System.Drawing.Point(530, 230);
            this.lblDetailMetacritic.Name = "lblDetailMetacritic";
            this.lblDetailMetacritic.Size = new System.Drawing.Size(60, 50);
            this.lblDetailMetacritic.TabIndex = 25;
            this.lblDetailMetacritic.Text = "95";
            // 
            // lblDetailPlaytime
            // 
            this.lblDetailPlaytime.Appearance.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.lblDetailPlaytime.Appearance.ForeColor = System.Drawing.Color.White;
            this.lblDetailPlaytime.Appearance.Options.UseFont = true;
            this.lblDetailPlaytime.Appearance.Options.UseForeColor = true;
            this.lblDetailPlaytime.Location = new System.Drawing.Point(530, 330);
            this.lblDetailPlaytime.Name = "lblDetailPlaytime";
            this.lblDetailPlaytime.Size = new System.Drawing.Size(93, 25);
            this.lblDetailPlaytime.TabIndex = 8;
            this.lblDetailPlaytime.Text = "⏱ 25 Hours";
            // 
            // MainForm
            // 
            this.Appearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(29)))), ((int)(((byte)(41)))));
            this.Appearance.ForeColor = System.Drawing.Color.White;
            this.Appearance.Options.UseBackColor = true;
            this.Appearance.Options.UseForeColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1498, 718);
            this.Controls.Add(this.navigationFrame1);
            this.Controls.Add(this.panelControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GameTracker";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.navigationFrame1)).EndInit();
            this.navigationFrame1.ResumeLayout(false);
            this.pageHome.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelHomePagination)).EndInit();
            this.panelHomePagination.ResumeLayout(false);
            this.pageLibrary.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelLibPagination)).EndInit();
            this.panelLibPagination.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl6)).EndInit();
            this.panelControl6.ResumeLayout(false);
            this.panelControl6.PerformLayout();
            this.pageSearch.ResumeLayout(false);
            this.flowLayoutPanelSearch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelSearchPagination)).EndInit();
            this.panelSearchPagination.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl7)).EndInit();
            this.panelControl7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.searchControlSearchPage.Properties)).EndInit();
            this.pageSettings.ResumeLayout(false);
            this.flowSettingsMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pnlAccountSettings)).EndInit();
            this.pnlAccountSettings.ResumeLayout(false);
            this.pnlAccountSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewPassAgain.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewUsername.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCurrentPass.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewPass.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlAppPreferences)).EndInit();
            this.pnlAppPreferences.ResumeLayout(false);
            this.pnlAppPreferences.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbStartPage.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.toggleNSFW.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlSystemData)).EndInit();
            this.pnlSystemData.ResumeLayout(false);
            this.pnlSystemData.PerformLayout();
            this.pageGameDetail.ResumeLayout(false);
            this.scrollableDetailContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelControl2)).EndInit();
            this.panelControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.panelTopSection)).EndInit();
            this.panelTopSection.ResumeLayout(false);
            this.panelTopSection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.peDetailImage.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.SimpleButton btnHomeMenu;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.SimpleButton btnSettings;
        private DevExpress.XtraEditors.SimpleButton btnLibrary;
        private DevExpress.XtraEditors.SimpleButton btnSearch;
        private DevExpress.XtraBars.Navigation.NavigationFrame navigationFrame1;
        private DevExpress.XtraBars.Navigation.NavigationPage pageHome;
        private DevExpress.XtraBars.Navigation.NavigationPage pageLibrary;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLibrary;
        private DevExpress.XtraEditors.LabelControl lblMyLibrary;
        private DevExpress.XtraEditors.SearchControl searchControlSearchPage;
        private DevExpress.XtraEditors.SimpleButton btnLibPlanToPlay;
        private DevExpress.XtraEditors.SimpleButton btnLibDropped;
        private DevExpress.XtraEditors.SimpleButton btnLibPlaying;
        private DevExpress.XtraEditors.SimpleButton btnLibPlayed;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelPopulerGames;
        private DevExpress.XtraEditors.LabelControl lblPopulerGames;
        private DevExpress.XtraEditors.PanelControl panelHomePagination;
        private DevExpress.XtraEditors.SimpleButton btnHomeNext;
        private DevExpress.XtraEditors.SimpleButton btnHomePrev;
        private DevExpress.XtraEditors.LabelControl lblHomePage;
        private DevExpress.XtraBars.Navigation.NavigationPage pageSearch;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelSearch;
        private DevExpress.XtraEditors.PanelControl panelControl7;
        private DevExpress.XtraBars.Navigation.NavigationPage pageSettings;
        private DevExpress.XtraEditors.LabelControl lblNoResult;
        private DevExpress.XtraEditors.PanelControl panelLibPagination;
        private DevExpress.XtraEditors.LabelControl lblLibPage;
        private DevExpress.XtraEditors.SimpleButton btnLibNext;
        private DevExpress.XtraEditors.SimpleButton btnLibPrev;
        private DevExpress.XtraEditors.PanelControl panelControl6;
        private DevExpress.XtraEditors.PanelControl panelSearchPagination;
        private DevExpress.XtraEditors.LabelControl lblSearchPage;
        private DevExpress.XtraEditors.SimpleButton btnSearchNext;
        private DevExpress.XtraEditors.SimpleButton btnSearchPrev;
        private DevExpress.XtraBars.Navigation.NavigationPage pageGameDetail;
        private DevExpress.XtraEditors.XtraScrollableControl scrollableDetailContainer;
        private DevExpress.XtraEditors.SimpleButton btnDetailBack;
        private DevExpress.XtraEditors.PictureEdit peDetailImage;
        private DevExpress.XtraEditors.LabelControl lblDetailTitle;
        private DevExpress.XtraEditors.LabelControl lblDetailDeveloper;
        private DevExpress.XtraEditors.LabelControl lblDetailGenres;
        private DevExpress.XtraEditors.LabelControl lblDetailRating;
        private DevExpress.XtraEditors.LabelControl lblDetailMetacritic;
        private DevExpress.XtraEditors.LabelControl lblDetailPlaytime;
        private DevExpress.XtraEditors.LabelControl lblDescriptionHeader;
        private DevExpress.XtraEditors.LabelControl lblDetailDescription;
        private DevExpress.XtraEditors.LabelControl lblRequirementsHeader;
        private DevExpress.XtraEditors.LabelControl lblDetailRequirements;
        private DevExpress.XtraEditors.PanelControl panelTopSection;
        private DevExpress.XtraEditors.SimpleButton btnLibraryAction;
        private DevExpress.XtraEditors.LabelControl lblDetailAge;
        private DevExpress.XtraEditors.LabelControl lblDetailPlatforms;
        private DevExpress.XtraEditors.LabelControl lblDetailStores;
        private DevExpress.XtraEditors.LabelControl lblDetailModes;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutScreenshots;
        private DevExpress.XtraEditors.LabelControl lblSS;
        private DevExpress.XtraEditors.PanelControl panelControl2;

        // --- SETTINGS VARIABLES ---
        private System.Windows.Forms.FlowLayoutPanel flowSettingsMain;

        // Card 1: Account
        private DevExpress.XtraEditors.PanelControl pnlAccountSettings;
        private DevExpress.XtraEditors.LabelControl lblAccountHeader;
        private DevExpress.XtraEditors.LabelControl separatorAccount;
        private DevExpress.XtraEditors.LabelControl lblChangeUser;
        private DevExpress.XtraEditors.TextEdit txtNewUsername;
        private DevExpress.XtraEditors.SimpleButton btnUpdateUsername;
        private DevExpress.XtraEditors.LabelControl lblChangePass;
        private DevExpress.XtraEditors.TextEdit txtCurrentPass;
        private DevExpress.XtraEditors.TextEdit txtNewPass;
        private DevExpress.XtraEditors.SimpleButton btnUpdatePass;

        // Card 2: Preferences
        private DevExpress.XtraEditors.PanelControl pnlAppPreferences;
        private DevExpress.XtraEditors.LabelControl lblPrefHeader;
        private DevExpress.XtraEditors.LabelControl separatorPref;
        private DevExpress.XtraEditors.LabelControl lblStartPage;
        private DevExpress.XtraEditors.ComboBoxEdit cmbStartPage;
        private DevExpress.XtraEditors.LabelControl lblNSFW;
        private DevExpress.XtraEditors.ToggleSwitch toggleNSFW;

        // Card 3: System
        private DevExpress.XtraEditors.PanelControl pnlSystemData;
        private DevExpress.XtraEditors.LabelControl lblSystemHeader;
        private DevExpress.XtraEditors.LabelControl separatorSystem;
        private DevExpress.XtraEditors.SimpleButton btnClearCache;
        private DevExpress.XtraEditors.SimpleButton btnLogout;
        private DevExpress.XtraEditors.TextEdit txtNewPassAgain;
        private DevExpress.XtraEditors.LabelControl lblUsernameWarning;
        private DevExpress.XtraEditors.LabelControl lblPassWarning;
    }
}