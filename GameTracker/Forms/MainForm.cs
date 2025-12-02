using DevExpress.XtraEditors;
using GameTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameTracker
{
    public partial class MainForm : XtraForm
    {
        #region Fields
        private RawgApiService rawgapi;
        private readonly ImageManager imageManager;
        private readonly LayoutCalculator layoutCalculator;

        // Home
        private List<Game> homeGames = new List<Game>();
        private int homePage = 1;
        private int homeApiPage = 1; // RAWG API'den çekilen sayfa sayısı

        // Library
        private List<Game> libraryGames = new List<Game>();
        private int libPage = 1;

        // Search
        private List<Game> searchGames = new List<Game>();
        private int searchPage = 1;

        // Ortak Ayarlar
        private int cardsPerPage = 24;
        private int gameToLoadPerRequest = 100;
        int labelHeight = 30;
        private LayoutMetrics currentLayoutMetrics;
        private System.Windows.Forms.Timer resizeTimer;
        #endregion

        #region Constructor & Load
        public MainForm()
        {
            InitializeComponent();
            InitializeFlowLayoutPanel();

            rawgapi = new RawgApiService();
            this.imageManager = new ImageManager();
            this.layoutCalculator = new LayoutCalculator();

            // Timer
            resizeTimer = new Timer();
            resizeTimer.Interval = 300;
            resizeTimer.Tick += ResizeTimer_Tick;

            SetDoubleBuffered(flowLayoutPanelPopulerGames);
            SetDoubleBuffered(flowLayoutPanelLibrary);
            SetDoubleBuffered(flowLayoutPanelSearch);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            currentLayoutMetrics = layoutCalculator.Calculate(flowLayoutPanelPopulerGames.ClientSize);
            cardsPerPage = currentLayoutMetrics.CardsPerPage;

            // Home datasını yükle
            await LoadHomeGamesAsync(homeApiPage, gameToLoadPerRequest);
            RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop();

            // 1. Ekran boyutunu hesapla
            Control activePanel = navigationFrame1.SelectedPage.Controls[0] as FlowLayoutPanel;
            if (activePanel == null) return;

            currentLayoutMetrics = layoutCalculator.Calculate(activePanel.ClientSize);
            cardsPerPage = currentLayoutMetrics.CardsPerPage;

            // 2. Aktif sayfayı yenile
            if (navigationFrame1.SelectedPage == pageHome)
                RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);

            else if (navigationFrame1.SelectedPage == pageLibrary)
                RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);

            else if (navigationFrame1.SelectedPage == pageSearch)
                RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            imageManager?.Dispose();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }
        #endregion

        #region Initialization
        private void InitializeFlowLayoutPanel()
        {
            void SetupPanel(FlowLayoutPanel p)
            {
                p.AutoSize = false;
                p.FlowDirection = FlowDirection.LeftToRight;
                p.WrapContents = true;
                p.Padding = new Padding(0);
                p.AutoScroll = false;
            }

            SetupPanel(flowLayoutPanelPopulerGames);
            SetupPanel(flowLayoutPanelLibrary);
            SetupPanel(flowLayoutPanelSearch);
        }
        #endregion

        #region Generic Render Logic

        // Bu metot HERHANGİ bir sayfayı render edebilir. Kod tekrarını önler.
        private void RenderPage(List<Game> sourceList, FlowLayoutPanel targetPanel, int pageIndex, LabelControl pageLabel, LabelControl noResultLabel = null)
        {
            if (sourceList == null) return;

            targetPanel.SuspendLayout();
            targetPanel.Controls.Clear();

            // No Result Label varsa ekle (Search için)
            if (noResultLabel != null)
            {
                if (sourceList.Count == 0)
                {
                    noResultLabel.Width = targetPanel.Width - 50;
                    targetPanel.Controls.Add(noResultLabel);
                    noResultLabel.Visible = true;
                    targetPanel.ResumeLayout();
                    pageLabel.Text = "Page 0 / 0";
                    return;
                }
                else
                {
                    noResultLabel.Visible = false;
                }
            }

            // Toplam sayfa sayısı
            int totalPages = (int)Math.Ceiling((double)sourceList.Count / cardsPerPage);
            if (totalPages < 1) totalPages = 1;

            // Güvenlik kontrolü (Page index sınırları aşmasın)
            if (pageIndex > totalPages) pageIndex = totalPages;
            if (pageIndex < 1) pageIndex = 1;

            // Listeyi dilimle (Pagination)
            var pagedGames = sourceList
                .Skip((pageIndex - 1) * cardsPerPage)
                .Take(cardsPerPage)
                .ToList();

            // Kartları oluştur ve ekle
            foreach (var game in pagedGames)
            {
                var card = CreateGameCard(game);
                targetPanel.Controls.Add(card);
            }

            targetPanel.ResumeLayout();

            // Label güncelle
            pageLabel.Text = $"Page {pageIndex} / {totalPages}";
        }

        #endregion

        #region Home Logic
        private async Task LoadHomeGamesAsync(int apiPage, int count)
        {
            var newGames = await rawgapi.GetPopularGamesAsync(apiPage, count);
            if (newGames != null) homeGames.AddRange(newGames);
        }

        private void btnHomePrev_Click(object sender, EventArgs e)
        {
            if (homePage > 1)
            {
                homePage--;
                RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);
            }
        }

        private async void btnHomeNext_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)homeGames.Count / cardsPerPage);

            // Son sayfadaysak API'den daha fazla veri çek
            if (homePage >= totalPages - 1)
            {
                btnHomeNext.Enabled = false;
                homeApiPage++;
                await LoadHomeGamesAsync(homeApiPage, gameToLoadPerRequest);
                btnHomeNext.Enabled = true;

                // Sayfa sayısını güncelle
                totalPages = (int)Math.Ceiling((double)homeGames.Count / cardsPerPage);
            }

            if (homePage < totalPages)
            {
                homePage++;
                RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);
            }
        }
        #endregion

        #region Library Logic
        private void LoadLibraryGames()
        {
            // DB'den tüm kütüphaneyi çek
            if (Session.UserId > 0)
            {
                libraryGames = LibraryManager.GetUserLibrary(Session.UserId);
                libPage = 1; // Her yüklemede başa dön
                RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);
            }
        }

        private void btnLibPrev_Click(object sender, EventArgs e)
        {
            if (libPage > 1)
            {
                libPage--;
                RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);
            }
        }

        private void btnLibNext_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)libraryGames.Count / cardsPerPage);
            if (libPage < totalPages)
            {
                libPage++;
                RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);
            }
        }
        #endregion

        #region Search Logic
        private void searchControlSearchPage_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }

        private async void PerformSearch()
        {
            string searchTerm = searchControlSearchPage.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm)) return;

            this.Cursor = Cursors.WaitCursor;
            lblNoResult.Visible = false;
            lblNoResult.Text = $"No results found for '{searchTerm}'";

            try
            {
                // API'den 50 sonuç çekelim
                searchGames = await rawgapi.GetGamesBySearchAsync(searchTerm, 50);
                searchPage = 1; // Aramada başa dön
                RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Search error: {ex.Message}", "Error");
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnSearchPrev_Click(object sender, EventArgs e)
        {
            if (searchPage > 1)
            {
                searchPage--;
                RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
            }
        }

        private void btnSearchNext_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)searchGames.Count / cardsPerPage);
            if (searchPage < totalPages)
            {
                searchPage++;
                RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
            }
        }
        #endregion

        #region Navigation Menu
        private void btnHomeMenu_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageHome;
        }

        private void btnLibrary_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageLibrary;
            LoadLibraryGames();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageSearch;
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageSettings;
        }
        #endregion

        #region Card Creation & Helpers
        private GameCardControl CreateGameCard(Game game)
        {
            // Yeni UserControl'ü oluştur
            GameCardControl card = new GameCardControl();

            // Boyutları LayoutCalculator'dan gelen verilerle hesaplar
            card.Width = currentLayoutMetrics.CardWidth;
            card.Height = currentLayoutMetrics.ImageHeight + labelHeight;
            card.Margin = new Padding(10, 10, 10, 10 + currentLayoutMetrics.ExtraSpacingPerRow);

            // İçindeki panel ve resmin boyutlarını da ayarla
            card.borderPanel.Height = currentLayoutMetrics.ImageHeight;
            card.borderPanel.Width = currentLayoutMetrics.CardWidth;

            // Veriyi Bas
            card.SetData(game);

            // Context Menu Oluştur
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            if (navigationFrame1.SelectedPage != pageLibrary)
            {
                ToolStripMenuItem addToLibItem = new ToolStripMenuItem("Add to Library");
                ToolStripMenuItem itemPlan = new ToolStripMenuItem("Plan to Play");
                ToolStripMenuItem itemPlaying = new ToolStripMenuItem("Playing");
                ToolStripMenuItem itemPlayed = new ToolStripMenuItem("Played");

                itemPlan.Click += (s, e) => AddGameToDb(game, "PlanToPlay");
                itemPlaying.Click += (s, e) => AddGameToDb(game, "Playing");
                itemPlayed.Click += (s, e) => AddGameToDb(game, "Played");

                addToLibItem.DropDownItems.Add(itemPlan);
                addToLibItem.DropDownItems.Add(itemPlaying);
                addToLibItem.DropDownItems.Add(itemPlayed);
                contextMenu.Items.Add(addToLibItem);
            }

            if (navigationFrame1.SelectedPage == pageLibrary)
            {
                ToolStripMenuItem changeStatusItem = new ToolStripMenuItem("Move to...");
                ToolStripMenuItem movePlan = new ToolStripMenuItem("Plan to Play");
                ToolStripMenuItem movePlaying = new ToolStripMenuItem("Playing");
                ToolStripMenuItem movePlayed = new ToolStripMenuItem("Played");

                movePlan.Click += (s, e) => UpdateGameStatusDb(game, "PlanToPlay");
                movePlaying.Click += (s, e) => UpdateGameStatusDb(game, "Playing");
                movePlayed.Click += (s, e) => UpdateGameStatusDb(game, "Played");

                changeStatusItem.DropDownItems.Add(movePlan);
                changeStatusItem.DropDownItems.Add(movePlaying);
                changeStatusItem.DropDownItems.Add(movePlayed);
                contextMenu.Items.Add(changeStatusItem);

                ToolStripMenuItem removeItem = new ToolStripMenuItem("Remove from Library");
                removeItem.Click += (s, e) => RemoveGameFromDb(game);
                contextMenu.Items.Add(removeItem);
            }

            // Menüyü UserControl'ün içindeki bileşenlere bağlar
            card.ContextMenuStrip = contextMenu;
            card.peGameImage.ContextMenuStrip = contextMenu;
            card.lblGameTitle.ContextMenuStrip = contextMenu;

            // Resmi yükler
            imageManager.LoadImageAsync(game.BackgroundImage, card.peGameImage, 420);

            return card;
        }

        private void AddGameToDb(Game game, string status)
        {
            if (Session.UserId <= 0)
            {
                XtraMessageBox.Show("Please login first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = LibraryManager.AddGameToLibrary(Session.UserId, game, status);
            if (success)
                XtraMessageBox.Show($"{game.Name} added to {status} list!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                XtraMessageBox.Show($"{game.Name} is already in your library!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateGameStatusDb(Game game, string newStatus)
        {
            if (Session.UserId <= 0) return;
            bool success = LibraryManager.UpdateGameStatus(Session.UserId, game.Id, newStatus);
            if (success) LoadLibraryGames();
        }

        private void RemoveGameFromDb(Game game)
        {
            if (Session.UserId <= 0) return;
            bool success = LibraryManager.RemoveGame(Session.UserId, game.Id);
            if (success) LoadLibraryGames();
        }

        public static void SetDoubleBuffered(Control c)
        {
            if (SystemInformation.TerminalServerSession) return;
            System.Reflection.PropertyInfo aProp = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }
        #endregion
    }
}