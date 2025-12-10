using DevExpress.XtraEditors;
using GameTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameTracker
{
    /// <summary>
    /// Uygulamanın ana arayüz formudur.
    /// Home, Library ve Search sayfalarını yönetir.
    /// API isteklerini, sayfalamayı ve dinamik kart yerleşimini kontrol eder.
    /// </summary>
    public partial class MainForm : XtraForm
    {
        #region Private Fields

        // Servisler
        private RawgApiService rawgapi;
        private readonly ImageManager imageManager;
        private readonly LayoutCalculator layoutCalculator;

        // Home sayfa verileri
        private List<Game> homeGames = new List<Game>();
        private int homePage = 1;
        private int homeApiPage = 1;

        // Library sayfa verileri
        private List<Game> libraryGames = new List<Game>();
        private int libPage = 1;

        // Search sayfa verileri
        private List<Game> searchGames = new List<Game>();
        private int searchPage = 1;

        // Global ayarlar
        private int cardsPerPage = 24;
        private int gameToLoadPerRequest = 100;
        private const int LABEL_HEIGHT = 30;
        private LayoutMetrics currentLayoutMetrics;

        // UI optimizasyon
        private System.Windows.Forms.Timer resizeTimer;

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// Form oluşturulduğunda başlatıcı ayarları yapar.
        /// Servisleri, layout hesaplayıcıyı ve arayüz yapılandırmasını kurar.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Servisleri başlat
            rawgapi = new RawgApiService();
            imageManager = new ImageManager();
            layoutCalculator = new LayoutCalculator();

            InitializeFlowLayoutPanels();
            InitializeResizeTimer();
            EnableDoubleBuffering();
        }

        /// <summary>
        /// Tüm FlowLayoutPanel'lerin temel özelliklerini yapılandırır.
        /// </summary>
        private void InitializeFlowLayoutPanels()
        {
            ConfigureFlowLayoutPanel(flowLayoutPanelPopulerGames);
            ConfigureFlowLayoutPanel(flowLayoutPanelLibrary);
            ConfigureFlowLayoutPanel(flowLayoutPanelSearch);
        }

        /// <summary>
        /// Tek bir FlowLayoutPanel'i yapılandırır.
        /// </summary>
        /// <param name="panel">Yapılandırılacak panel</param>
        private void ConfigureFlowLayoutPanel(FlowLayoutPanel panel)
        {
            panel.AutoSize = false;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = true;
            panel.Padding = new Padding(0);
            panel.AutoScroll = false;
        }

        /// <summary>
        /// Pencere boyutlandırma optimizasyonu için timer'ı başlatır.
        /// Jitter (titreme) problemini önler.
        /// </summary>
        private void InitializeResizeTimer()
        {
            resizeTimer = new Timer();
            resizeTimer.Interval = 300;
            resizeTimer.Tick += ResizeTimer_Tick;
        }

        /// <summary>
        /// Panel titremesini azaltmak için double-buffer etkinleştirir.
        /// </summary>
        private void EnableDoubleBuffering()
        {
            SetDoubleBuffered(flowLayoutPanelPopulerGames);
            SetDoubleBuffered(flowLayoutPanelLibrary);
            SetDoubleBuffered(flowLayoutPanelSearch);
        }

        /// <summary>
        /// Bir kontrole double buffering özelliği ekler.
        /// Reflection kullanarak NonPublic property'e erişir.
        /// </summary>
        /// <param name="control">Double buffering eklenecek kontrol</param>
        private static void SetDoubleBuffered(Control control)
        {
            if (SystemInformation.TerminalServerSession)
                return;

            var doubleBufferedProperty = typeof(Control).GetProperty(
                "DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            doubleBufferedProperty?.SetValue(control, true, null);
        }

        #endregion

        #region Form Lifecycle Events

        /// <summary>
        /// Form yüklendiğinde başlangıç verilerini çeker ve Home sayfasını doldurur.
        /// </summary>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Layout metriklerini hesapla
            currentLayoutMetrics = layoutCalculator.Calculate(flowLayoutPanelPopulerGames.ClientSize);
            cardsPerPage = currentLayoutMetrics.CardsPerPage;

            // İlk Home verilerini yükle
            await LoadHomeGamesAsync(homeApiPage, gameToLoadPerRequest);

            // Home sayfasını render et
            RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);
        }

        /// <summary>
        /// Form kapanırken kaynakları temizler.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            imageManager?.Dispose();
        }

        /// <summary>
        /// Form tamamen kapandığında uygulamayı sonlandırır.
        /// </summary>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        #endregion

        #region Window Resize Handling

        /// <summary>
        /// Form yeniden boyutlanırken timer'ı tetikler.
        /// Her pixel hareketi için render yapmayı önler.
        /// </summary>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        /// <summary>
        /// Pencere boyutlandırma işlemi durduğunda tetiklenir.
        /// Layout yeniden hesaplanır ve aktif sayfa yeniden çizilir.
        /// </summary>
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop();

            // Aktif panel'i bul
            var activePanel = navigationFrame1.SelectedPage?.Controls[0] as FlowLayoutPanel;
            if (activePanel == null)
                return;

            // Layout metriklerini yeniden hesapla
            RecalculateLayoutMetrics(activePanel);

            // Aktif sayfayı yeniden render et
            RefreshActivePage();
        }

        /// <summary>
        /// Verilen panel boyutuna göre layout metriklerini yeniden hesaplar.
        /// </summary>
        private void RecalculateLayoutMetrics(FlowLayoutPanel panel)
        {
            currentLayoutMetrics = layoutCalculator.Calculate(panel.ClientSize);
            cardsPerPage = currentLayoutMetrics.CardsPerPage;
        }

        /// <summary>
        /// Şu anda aktif olan sayfayı yeniden render eder.
        /// </summary>
        private void RefreshActivePage()
        {
            if (navigationFrame1.SelectedPage == pageHome)
                RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);
            else if (navigationFrame1.SelectedPage == pageLibrary)
                RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);
            else if (navigationFrame1.SelectedPage == pageSearch)
                RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
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

        #region Generic Page Rendering

        /// <summary>
        /// Herhangi bir sayfa için genel render mantığı.
        /// Pagination (sayfalama) ve kart yerleşimi bu metot üzerinden yapılır.
        /// </summary>
        /// <param name="sourceList">Gösterilecek oyun listesi</param>
        /// <param name="targetPanel">Oyunların yerleştirileceği panel</param>
        /// <param name="pageIndex">Mevcut sayfa numarası</param>
        /// <param name="pageLabel">Sayfa bilgisinin gösterileceği label</param>
        /// <param name="noResultLabel">Sonuç bulunamadığında gösterilecek label (opsiyonel)</param>
        private void RenderPage(List<Game> sourceList, FlowLayoutPanel targetPanel, int pageIndex, LabelControl pageLabel, LabelControl noResultLabel = null)
        {
            if (sourceList == null)
                return;

            targetPanel.SuspendLayout();
            targetPanel.Controls.Clear();

            // Sonuç yoksa "No Result" label'ını göster
            if (HandleNoResults(sourceList, targetPanel, pageLabel, noResultLabel))
            {
                targetPanel.ResumeLayout();
                return;
            }

            // Sayfa bilgilerini hesapla
            int totalPages = CalculateTotalPages(sourceList.Count);
            pageIndex = ClampPageIndex(pageIndex, totalPages);

            // Mevcut sayfa için oyunları getir
            var pagedGames = GetGamesForPage(sourceList, pageIndex);

            // Oyun kartlarını oluştur ve panele ekle
            AddGameCardsToPanel(pagedGames, targetPanel);

            targetPanel.ResumeLayout();

            // Sayfa label'ını güncelle
            UpdatePageLabel(pageLabel, pageIndex, totalPages);
        }

        /// <summary>
        /// Sonuç yoksa ilgili label'ı gösterir ve true döner.
        /// </summary>
        private bool HandleNoResults(List<Game> sourceList, FlowLayoutPanel targetPanel, LabelControl pageLabel, LabelControl noResultLabel)
        {
            if (noResultLabel == null)
                return false;

            if (sourceList.Count == 0)
            {
                noResultLabel.Width = targetPanel.Width - 50;
                targetPanel.Controls.Add(noResultLabel);
                noResultLabel.Visible = true;
                pageLabel.Text = "Page 0 / 0";
                return true;
            }

            noResultLabel.Visible = false;
            return false;
        }

        /// <summary>
        /// Toplam sayfa sayısını hesaplar.
        /// </summary>
        private int CalculateTotalPages(int totalGames)
        {
            int totalPages = (int)Math.Ceiling((double)totalGames / cardsPerPage);
            return Math.Max(totalPages, 1);
        }

        /// <summary>
        /// Sayfa indeksini geçerli aralıkta tutar.
        /// </summary>
        private int ClampPageIndex(int pageIndex, int totalPages)
        {
            return Math.Max(1, Math.Min(pageIndex, totalPages));
        }

        /// <summary>
        /// Belirtilen sayfa için oyun listesini döndürür.
        /// </summary>
        private List<Game> GetGamesForPage(List<Game> sourceList, int pageIndex)
        {
            return sourceList
                .Skip((pageIndex - 1) * cardsPerPage)
                .Take(cardsPerPage)
                .ToList();
        }

        /// <summary>
        /// Oyun kartlarını oluşturur ve panele ekler.
        /// </summary>
        private void AddGameCardsToPanel(List<Game> games, FlowLayoutPanel panel)
        {
            foreach (var game in games)
            {
                var card = CreateGameCard(game);
                panel.Controls.Add(card);
            }
        }

        /// <summary>
        /// Sayfa bilgisi label'ını günceller.
        /// </summary>
        private void UpdatePageLabel(LabelControl label, int currentPage, int totalPages)
        {
            label.Text = $"Page {currentPage} / {totalPages}";
        }

        #endregion

        #region Home Page Logic

        /// <summary>
        /// API'den popüler oyunları yükler ve homeGames listesine ekler.
        /// </summary>
        /// <param name="apiPage">API sayfa numarası</param>
        /// <param name="count">Getirilecek oyun sayısı</param>
        private async Task LoadHomeGamesAsync(int apiPage, int count)
        {
            var newGames = await rawgapi.GetPopularGamesAsync(apiPage, count);
            if (newGames != null)
                homeGames.AddRange(newGames);
        }

        /// <summary>
        /// Home sayfasında önceki sayfaya geçer.
        /// </summary>
        private void btnHomePrev_Click(object sender, EventArgs e)
        {
            if (homePage > 1)
            {
                homePage--;
                RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);
            }
        }

        /// <summary>
        /// Home sayfasında sonraki sayfaya geçer.
        /// Gerekirse API'den yeni veri çeker.
        /// </summary>
        private async void btnHomeNext_Click(object sender, EventArgs e)
        {
            int totalPages = CalculateTotalPages(homeGames.Count);

            // Son sayfaya yaklaştıysak daha fazla veri yükle
            if (homePage >= totalPages - 1)
            {
                await LoadMoreHomeGames();
                totalPages = CalculateTotalPages(homeGames.Count);
            }

            // Sonraki sayfaya geç
            if (homePage < totalPages)
            {
                homePage++;
                RenderPage(homeGames, flowLayoutPanelPopulerGames, homePage, lblHomePage);
            }
        }

        /// <summary>
        /// API'den daha fazla Home oyunu yükler.
        /// </summary>
        private async Task LoadMoreHomeGames()
        {
            btnHomeNext.Enabled = false;
            homeApiPage++;
            await LoadHomeGamesAsync(homeApiPage, gameToLoadPerRequest);
            btnHomeNext.Enabled = true;
        }
        #endregion

        #region Library Page Logic
        /// <summary>
        /// Kullanıcının kütüphanesini veritabanından yükler ve render eder.
        /// </summary>
        private void LoadLibraryGames()
        {
            if (Session.UserId <= 0)
                return;

            libraryGames = LibraryManager.GetUserLibrary(Session.UserId);
            libPage = 1; // Her yüklemede ilk sayfaya dön
            RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);
        }

        /// <summary>
        /// Library sayfasında önceki sayfaya geçer.
        /// </summary>
        private void btnLibPrev_Click(object sender, EventArgs e)
        {
            if (libPage > 1)
            {
                libPage--;
                RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);
            }
        }

        /// <summary>
        /// Library sayfasında sonraki sayfaya geçer.
        /// </summary>
        private void btnLibNext_Click(object sender, EventArgs e)
        {
            int totalPages = CalculateTotalPages(libraryGames.Count);
            if (libPage < totalPages)
            {
                libPage++;
                RenderPage(libraryGames, flowLayoutPanelLibrary, libPage, lblLibPage);
            }
        }
        #endregion

        #region Search Page Logic
        /// <summary>
        /// Arama kutusunda Enter tuşuna basıldığında aramayı başlatır.
        /// </summary>
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

            // Arama öncesi UI hazırlığı
            this.Cursor = Cursors.WaitCursor;
            lblNoResult.Visible = false;
            lblNoResult.Text = $"No results found for '{searchTerm}'";

            try
            {
                await ExecuteSearch(searchTerm);
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

        /// <summary>
        /// API'den arama sonuçlarını çeker ve render eder.
        /// </summary>
        private async Task ExecuteSearch(string searchTerm)
        {
            searchGames = await rawgapi.GetGamesBySearchAsync(searchTerm, 50);
            searchPage = 1; // Yeni aramada ilk sayfaya dön
            RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
        }

        /// <summary>
        /// Search sayfasında önceki sayfaya geçer.
        /// </summary>
        private void btnSearchPrev_Click(object sender, EventArgs e)
        {
            if (searchPage > 1)
            {
                searchPage--;
                RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
            }
        }

        /// <summary>
        /// Search sayfasında sonraki sayfaya geçer.
        /// </summary>
        private void btnSearchNext_Click(object sender, EventArgs e)
        {
            int totalPages = CalculateTotalPages(searchGames.Count);
            if (searchPage < totalPages)
            {
                searchPage++;
                RenderPage(searchGames, flowLayoutPanelSearch, searchPage, lblSearchPage, lblNoResult);
            }
        }
        #endregion

        #region Game Card Creation
        /// <summary>
        /// Bir oyun için görsel kart kontrolü oluşturur.
        /// Layout, context menu ve resim yükleme işlemlerini yapar.
        /// </summary>
        /// <param name="game">Kartı oluşturulacak oyun</param>
        /// <returns>Yapılandırılmış GameCardControl</returns>
        private GameCardControl CreateGameCard(Game game)
        {
            // Yeni UserControl'ü oluştur
            GameCardControl card = new GameCardControl();

            ConfigureCardDimensions(card);

            // Veriyi Bas
            card.SetData(game);

            // Context Menu Oluştur
            ContextMenuStrip contextMenu = CreateContextMenuForGame(game);

            // Menüyü UserControl'ün içindeki bileşenlere bağlar
            card.ContextMenuStrip = contextMenu;
            card.peGameImage.ContextMenuStrip = contextMenu;
            card.lblGameTitle.ContextMenuStrip = contextMenu;

            // Oyun resmini asenkron yükle
            imageManager.LoadImageAsync(game.BackgroundImage, card.peGameImage, 420);

            return card;
        }

        /// <summary>
        /// Kart boyutlarını layout metriklerine göre ayarlar.
        /// </summary>
        private void ConfigureCardDimensions(GameCardControl card)
        {
            // Boyutları LayoutCalculator'dan gelen verilerle hesaplar
            card.Width = currentLayoutMetrics.CardWidth;
            card.Height = currentLayoutMetrics.ImageHeight + LABEL_HEIGHT;
            card.Margin = new Padding(10, 10, 10, 10 + currentLayoutMetrics.ExtraSpacingPerRow);

            // İçindeki panel ve resmin boyutlarını da ayarla
            card.borderPanel.Height = currentLayoutMetrics.ImageHeight;
            card.borderPanel.Width = currentLayoutMetrics.CardWidth;
        }

        /// <summary>
        /// Oyun için uygun context menü oluşturur.
        /// Bulunulan sayfaya göre menü öğeleri değişir.
        /// </summary>
        private ContextMenuStrip CreateContextMenuForGame(Game game)
        {
            var contextMenu = new ContextMenuStrip();

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
            else
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

            return contextMenu;
        }
        #endregion

        #region Data Base

        /// <summary>
        /// Oyunu veritabanına ekler.
        /// </summary>
        /// <param name="game">Eklenecek oyun</param>
        /// <param name="status">Oyunun durumu (PlanToPlay, Playing, Played)</param>
        private void AddGameToDb(Game game, string status)
        {
            if (!ValidateUserSession())
                return;

            bool success = LibraryManager.AddGameToLibrary(Session.UserId, game, status);

            if (success)
                MyMessageBox.Show($"{game.Name} added to {status} list!", "Success");
            else
                MyMessageBox.Show($"{game.Name} is already in your library!", "Info");
        }

        /// <summary>
        /// Oyunun durumunu veritabanında günceller.
        /// </summary>
        /// <param name="game">Güncellenecek oyun</param>
        /// <param name="newStatus">Yeni durum</param>
        private void UpdateGameStatusDb(Game game, string newStatus)
        {
            if (!ValidateUserSession())
                return;

            bool success = LibraryManager.UpdateGameStatus(Session.UserId, game.Id, newStatus);
            if (success)
                LoadLibraryGames();
        }

        /// <summary>
        /// Oyunu veritabanından kaldırır.
        /// </summary>
        /// <param name="game">Kaldırılacak oyun</param>
        private void RemoveGameFromDb(Game game)
        {
            if (!ValidateUserSession())
                return;

            bool success = LibraryManager.RemoveGame(Session.UserId, game.Id);
            if (success)
                LoadLibraryGames();
        }

        /// <summary>
        /// Kullanıcının giriş yapmış olup olmadığını kontrol eder.
        /// </summary>
        /// <returns>Giriş yapılmışsa true, değilse false</returns>
        private bool ValidateUserSession()
        {
            if (Session.UserId > 0)
                return true;

            MyMessageBox.Show("Please login first!", "Warning");
            return false;
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
    }
}