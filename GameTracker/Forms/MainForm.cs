using DevExpress.XtraEditors;
using GameTracker.Factories;
using GameTracker.Helpers;
using GameTracker.Managers;
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

        // Global ayarlar
        private int cardsPerPage = 24;
        private int gameToLoadPerRequest = 100;
        private LayoutMetrics currentLayoutMetrics;

        // Sayfa Yöneticileri
        private PageManager homeManager;
        private PageManager libraryManager;
        private PageManager searchManager;

        // API Takibi
        private int homeApiPage = 1;

        // UI optimizasyon
        private Timer resizeTimer;

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

            homeManager = new PageManager(24);
            libraryManager = new PageManager(24);
            searchManager = new PageManager(24);

            UiHelper.InitializeFlowPanels(flowLayoutPanelPopulerGames, flowLayoutPanelLibrary, flowLayoutPanelSearch);
            UiHelper.EnableDoubleBuffering(flowLayoutPanelPopulerGames, flowLayoutPanelLibrary, flowLayoutPanelSearch);

            // Resize timer'ı başlat
            InitializeResizeTimer();
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
            RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
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

            // Yöneticilere yeni kart kapasitesini bildir
            homeManager.ItemsPerPage = cardsPerPage;
            libraryManager.ItemsPerPage = cardsPerPage;
            searchManager.ItemsPerPage = cardsPerPage;
        }

        /// <summary>
        /// Şu anda aktif olan sayfayı yeniden render eder.
        /// </summary>
        private void RefreshActivePage()
        {
            if (navigationFrame1.SelectedPage == pageHome)
                RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
            else if (navigationFrame1.SelectedPage == pageLibrary)
                RenderPage(libraryManager, flowLayoutPanelLibrary, lblLibPage);
            else if (navigationFrame1.SelectedPage == pageSearch)
                RenderPage(searchManager, flowLayoutPanelSearch, lblSearchPage, lblNoResult);
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
        private void RenderPage(PageManager manager, FlowLayoutPanel targetPanel, LabelControl pageLabel, LabelControl noResultLabel = null)
        {
            // Verileri çek
            var gamesToShow = manager.GetCurrentPageItems();

            targetPanel.SuspendLayout();
            targetPanel.Controls.Clear();

            // Sonuç yok kontrolü (Eski HandleNoResults metodunu buna göre revize edebilirsin 
            // veya basitçe burada kontrol edebilirsin)
            if (gamesToShow.Count == 0 && manager.AllItems.Count == 0)
            {
                if (noResultLabel != null)
                {
                    noResultLabel.Visible = true;
                    pageLabel.Text = "Page 0 / 0";
                }
                targetPanel.ResumeLayout();
                return;
            }

            if (noResultLabel != null) noResultLabel.Visible = false;

            // Kartları Ekle (Factory ile güncellediğin metod)
            AddGameCardsToPanel(gamesToShow, targetPanel);

            targetPanel.ResumeLayout();

            // Etiketi güncelle
            pageLabel.Text = manager.GetPageInfoString();
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
        /// Factory Design Pattern kullanılarak kart üretimi delegate edilir.
        /// </summary>
        private void AddGameCardsToPanel(List<Game> games, FlowLayoutPanel panel)
        {
            // Hangi sayfada olduğumuza göre bağlamı belirle
            bool isLibrary = (navigationFrame1.SelectedPage == pageLibrary);

            foreach (var game in games)
            {
                // Factory'i çağırıyoruz.
                // Action'lar için lambda expression kullanıyoruz.
                // (g, status) => AddGameToDb(g, status) kısmı bizim 'Action'ımız.
                var card = GameCardFactory.CreateCard(game, currentLayoutMetrics, imageManager, isLibraryContext: isLibrary,
                    onStatusChange: (g, status) =>
                    {
                        // Eğer zaten kütüphanedeysek update, değilse add çalışır
                        if (isLibrary) UpdateGameStatusDb(g, status);
                        else AddGameToDb(g, status);
                    },
                    onRemove: (g) => RemoveGameFromDb(g)
                );

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
                homeManager.AddItems(newGames);
        }

        /// <summary>
        /// Home sayfasında sonraki sayfaya geçer.
        /// Gerekirse API'den yeni veri çeker.
        /// </summary>
        private async void btnHomeNext_Click(object sender, EventArgs e)
        {
            // Önce elimizdeki listede sonraki sayfaya geçmeye çalış
            if (homeManager.NextPage())
            {
                RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
            }
            // Eğer son sayfadaysak API'den yeni veri çek
            else
            {
                // API yüklemesi
                await LoadMoreHomeGames();

                // Yeni veri gelince NextPage yap
                if (homeManager.NextPage())
                {
                    RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
                }
            }
        }

        /// <summary>
        /// Home sayfasında önceki sayfaya geçer.
        /// </summary>
        private void btnHomePrev_Click(object sender, EventArgs e)
        {
            if (homeManager.PrevPage())
            {
                RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
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

            // Veriyi çek
            var myGames = LibraryManager.GetUserLibrary(Session.UserId);

            // Veriyi manager'a yükle (SetDataSource kullanıyoruz çünkü liste sıfırdan yükleniyor)
            libraryManager.SetDataSource(myGames);

            // Render ederken libraryManager kullan
            RenderPage(libraryManager, flowLayoutPanelLibrary, lblLibPage);
        }

        /// <summary>
        /// Library sayfasında önceki sayfaya geçer.
        /// </summary>
        private void btnLibPrev_Click(object sender, EventArgs e)
        {
            if (libraryManager.PrevPage())
                RenderPage(libraryManager, flowLayoutPanelLibrary, lblLibPage);
        }

        /// <summary>
        /// Library sayfasında sonraki sayfaya geçer.
        /// </summary>
        private void btnLibNext_Click(object sender, EventArgs e)
        {
            if (libraryManager.NextPage())
                RenderPage(libraryManager, flowLayoutPanelLibrary, lblLibPage);
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
            var results = await rawgapi.GetGamesBySearchAsync(searchTerm, 50);

            // Veriyi manager'a ver
            searchManager.SetDataSource(results);

            // Render ederken searchManager kullan
            RenderPage(searchManager, flowLayoutPanelSearch, lblSearchPage, lblNoResult);
        }

        /// <summary>
        /// Search sayfasında önceki sayfaya geçer.
        /// </summary>
        private void btnSearchPrev_Click(object sender, EventArgs e)
        {
            if (searchManager.PrevPage())
                RenderPage(searchManager, flowLayoutPanelSearch, lblSearchPage, lblNoResult);
        }

        /// <summary>
        /// Search sayfasında sonraki sayfaya geçer.
        /// </summary>
        private void btnSearchNext_Click(object sender, EventArgs e)
        {
            if (searchManager.NextPage())
                RenderPage(searchManager, flowLayoutPanelSearch, lblSearchPage, lblNoResult);
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
        private void ResizePage()
        {
            var activePanel = navigationFrame1.SelectedPage?.Controls[0] as FlowLayoutPanel;
            if (activePanel == null)
                return;

            // Layout metriklerini yeniden hesapla
            RecalculateLayoutMetrics(activePanel);

            // Aktif sayfayı yeniden render et
            RefreshActivePage();
        }

        private void btnHomeMenu_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageHome;
            ResizePage();
        }

        private void btnLibrary_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageLibrary;
            LoadLibraryGames();
            ResizePage();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageSearch;
            ResizePage();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageSettings;
        }
        #endregion
    }
}