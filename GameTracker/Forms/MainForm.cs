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

        // Sayfa Yöneticileri
        private PageManager homeManager;
        private PageManager libraryManager;
        private PageManager searchManager;

        // Global ayarlar
        private int cardsPerPage = 24;
        private int gameToLoadPerRequest = 100;
        private LayoutMetrics currentLayoutMetrics;

        // --- Durum Değişkenleri ---
        private int homeApiPage = 1; // API'den çekilen sayfa numarası
        private string currentLibraryFilter = null; // Kütüphane filtre durumu (Played, Dropped vs.)
        private DevExpress.XtraBars.Navigation.NavigationPage previousPage; // Geri butonu için hafıza

        // --- UI Optimizasyon ---
        private Timer resizeTimer; // Pencere boyutlandırma performans yönetimi

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

            // Sayfa yöneticilerini başlat
            homeManager = new PageManager(24);
            libraryManager = new PageManager(24);
            searchManager = new PageManager(24);

            // Arayüz optimizasyonları
            UiHelper.InitializeFlowPanels(flowLayoutPanelPopulerGames, flowLayoutPanelLibrary, flowLayoutPanelSearch);
            UiHelper.EnableDoubleBuffering(flowLayoutPanelPopulerGames, flowLayoutPanelLibrary, flowLayoutPanelSearch, flowLayoutScreenshots, scrollableDetailContainer);

            // Kullanıcı tercihlerini yükle
            InitializeUserPreferences();
            LoadUserPreferences();

            // Görsel efektler
            AddHoverEffectToButton(btnLibPlanToPlay);
            AddHoverEffectToButton(btnLibPlaying);
            AddHoverEffectToButton(btnLibPlayed);
            AddHoverEffectToButton(btnLibDropped);

            // Resize timer'ı başlat
            InitializeResizeTimer();

        }

        /// <summary>
        /// Pencere boyutlandırma optimizasyonu için timer'ı başlatır.
        /// Flicker problemini önler.
        /// </summary>
        private void InitializeResizeTimer()
        {
            resizeTimer = new Timer();
            resizeTimer.Interval = 300;
            resizeTimer.Tick += ResizeTimer_Tick;
        }

        /// <summary>
        /// Kullanıcının başlangıç sayfası tercihine göre uygulamayı açar.
        /// </summary>
        private void InitializeUserPreferences()
        {
            if (Properties.Settings.Default.StartPage == "Library")
            {
                navigationFrame1.SelectedPage = pageLibrary;
                LoadLibraryGames(); // Kütüphane verilerini çek
            }
            else
                navigationFrame1.SelectedPage = pageHome;

            var activePanel = navigationFrame1.SelectedPage?.Controls[0] as FlowLayoutPanel;
            RecalculateLayoutMetrics(activePanel);
            RefreshActivePage();
        }

        #endregion

        #region Form Lifecycle Events

        /// <summary>
        /// Form yüklendiğinde çalışır. API'den ilk verileri çeker.
        /// </summary>
        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Layout metriklerini hesapla
            currentLayoutMetrics = layoutCalculator.Calculate(flowLayoutPanelPopulerGames.ClientSize);
            cardsPerPage = currentLayoutMetrics.CardsPerPage;

            // İlk Home verilerini yükle
            await LoadHomeGamesAsync(homeApiPage, gameToLoadPerRequest);

            // Eğer ana sayfa açıksa render et
            if (navigationFrame1.SelectedPage == pageHome)
                RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
        }

        /// <summary>
        /// Form kapanırken kaynakları temizler.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            resizeTimer?.Stop();
            resizeTimer?.Dispose();
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
        /// Boyutlandırma bittiğinde kartları yeni düzene göre yeniden hesaplar ve çizer.
        /// </summary>
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop();

            // Aktif panel'i bul
            var activePanel = navigationFrame1.SelectedPage?.Controls[0] as FlowLayoutPanel;
            if (activePanel == null)
                return;

            RecalculateLayoutMetrics(activePanel);
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

        #endregion

        #region Page Rendering Logic

        /// <summary>
        /// Sayfaların ekrana çizilmesinden sorumlu genel metot.
        /// </summary>
        /// <param name="manager">İlgili sayfanın veri yöneticisi.</param>
        /// <param name="targetPanel">Kartların ekleneceği panel.</param>
        /// <param name="pageLabel">Sayfa numarasını gösteren etiket.</param>
        /// <param name="noResultLabel">Veri yoksa gösterilecek uyarı etiketi.</param>
        private void RenderPage(PageManager manager, FlowLayoutPanel targetPanel, LabelControl pageLabel, LabelControl noResultLabel = null)
        {
            if (HandleNoResults(manager, targetPanel, pageLabel, noResultLabel))
                return;

            // Verileri çek
            var gamesToShow = manager.GetCurrentPageItems();

            targetPanel.SuspendLayout();
            targetPanel.Controls.Clear();

            AddGameCardsToPanel(gamesToShow, targetPanel);

            targetPanel.ResumeLayout();
            pageLabel.Text = manager.GetPageInfoString(); // Etiketi güncelle
        }

        /// <summary>
        /// Eğer listede hiç oyun yoksa uygun mesajı gösterir veya paneli temizler.
        /// </summary>
        private bool HandleNoResults(PageManager manager, FlowLayoutPanel targetPanel, LabelControl pageLabel, LabelControl noResultLabel)
        {
            if (manager.AllItems.Count == 0)
            {
                targetPanel.Controls.Clear();
                pageLabel.Text = "Page 0 / 0";

                if (noResultLabel != null)
                {
                    noResultLabel.Width = targetPanel.Width - 50;
                    targetPanel.Controls.Add(noResultLabel);
                    noResultLabel.Visible = true;
                }
                return true;
            }

            if (noResultLabel != null) noResultLabel.Visible = false;
            return false; // Devam et
        }

        /// <summary>
        /// Oyun listesini kartlara dönüştürür ve panele ekler.
        /// </summary>
        private void AddGameCardsToPanel(List<Game> games, FlowLayoutPanel panel)
        {
            foreach (var game in games)
            {
                var card = GameCardFactory.CreateCard(game, currentLayoutMetrics, imageManager, onCardClick: (g) => ShowGameDetails(g));
                panel.Controls.Add(card);
            }
        }

        #endregion

        #region Home Page Actions

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
        /// API'den daha fazla Home oyunu yükler.
        /// </summary>
        private async Task LoadMoreHomeGames()
        {
            btnHomeNext.Enabled = false;
            homeApiPage++;
            await LoadHomeGamesAsync(homeApiPage, gameToLoadPerRequest);
            btnHomeNext.Enabled = true;
        }

        /// <summary>
        /// Home sayfasında sonraki sayfaya geçer.
        /// Gerekirse API'den yeni veri çeker.
        /// </summary>
        private async void btnHomeNext_Click(object sender, EventArgs e)
        {
            if (homeManager.NextPage())
                RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
            else // Eğer son sayfadaysak API'den yeni veri çek
            {
                await LoadMoreHomeGames();

                // Yeni veri gelince NextPage yap
                if (homeManager.NextPage())
                    RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
            }
        }

        /// <summary>
        /// Home sayfasında önceki sayfaya geçer.
        /// </summary>
        private void btnHomePrev_Click(object sender, EventArgs e)
        {
            if (homeManager.PrevPage())
                RenderPage(homeManager, flowLayoutPanelPopulerGames, lblHomePage);
        }
        #endregion

        #region Library Page Actions

        /// <summary>
        /// Kullanıcının kütüphanesini veritabanından yükler ve render eder.
        /// </summary>
        private void LoadLibraryGames()
        {
            if (Session.UserId <= 0)
                return;

            var myGames = LibraryManager.GetUserLibrary(Session.UserId, currentLibraryFilter); // Veriyi çek
            libraryManager.SetDataSource(myGames); // Veriyi manager'a yükle
            RenderPage(libraryManager, flowLayoutPanelLibrary, lblLibPage);
        }

        // --- Filtreleme Butonları ---
        private void FilterLibrary_Click(object sender, EventArgs e)
        {
            var clickedButton = sender as SimpleButton;
            if (clickedButton == null) return;

            string selectedTag = clickedButton.Tag as string;

            // Toggle mantığı: Aynı butona basılırsa filtreyi kaldır
            if (currentLibraryFilter == selectedTag)
            {
                currentLibraryFilter = null;
                HighlightActiveFilterButton(null); // Hiçbir butonu yakma
            }
            else
            {
                currentLibraryFilter = selectedTag;
                HighlightActiveFilterButton(clickedButton); // Sadece bunu yak
            }

            LoadLibraryGames();
        }

        // Buton renklerini ayarlayan yardımcı metot
        private void HighlightActiveFilterButton(SimpleButton activeButton)
        {
            // Paneldeki tüm butonları bul
            var buttons = new[] { btnLibPlanToPlay, btnLibPlaying, btnLibPlayed, btnLibDropped };

            foreach (var btn in buttons)
            {
                if (btn == activeButton)
                {
                    btn.Appearance.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                    btn.Appearance.ForeColor = System.Drawing.Color.White;
                }
                else
                {
                    btn.Appearance.BackColor = System.Drawing.Color.FromArgb(32, 34, 50);
                    btn.Appearance.ForeColor = System.Drawing.Color.WhiteSmoke;
                }
            }
        }

        private void btnLibNext_Click(object sender, EventArgs e)
        {
            if (libraryManager.NextPage())
                RenderPage(libraryManager, flowLayoutPanelLibrary, lblLibPage);
        }

        private void btnLibPrev_Click(object sender, EventArgs e)
        {
            if (libraryManager.PrevPage())
                RenderPage(libraryManager, flowLayoutPanelLibrary, lblLibPage);
        }
        #endregion

        #region Search Page Logic

        private async void PerformSearch()
        {
            string searchTerm = searchControlSearchPage.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm)) return;

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
            searchManager.SetDataSource(results); // Veriyi manager'a ver
            RenderPage(searchManager, flowLayoutPanelSearch, lblSearchPage, lblNoResult);
        }

        private void searchControlSearchPage_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformSearch();
            }
        }

        private void btnSearchNext_Click(object sender, EventArgs e)
        {
            if (searchManager.NextPage())
                RenderPage(searchManager, flowLayoutPanelSearch, lblSearchPage, lblNoResult);
        }

        private void btnSearchPrev_Click(object sender, EventArgs e)
        {
            if (searchManager.PrevPage())
                RenderPage(searchManager, flowLayoutPanelSearch, lblSearchPage, lblNoResult);
        }

        #endregion

        #region Game Detail & ScreenShots

        /// <summary>
        /// Oyunun detay sayfasını açar. Önce eldeki veriyi basar, sonra API'den detayları çeker.
        /// </summary>
        private async void ShowGameDetails(Game simpleGame)
        {
            // 1. Navigation
            previousPage = navigationFrame1.SelectedPage as DevExpress.XtraBars.Navigation.NavigationPage;
            navigationFrame1.SelectedPage = pageGameDetail;
            scrollableDetailContainer.VerticalScroll.Value = 0;

            // 2. Basit Verileri Göster
            lblDetailTitle.Text = simpleGame.Name;
            peDetailImage.Image = null;
            await imageManager.LoadImageAsync(simpleGame.BackgroundImage, peDetailImage, 600);

            // Loading Mesajları
            lblDetailDeveloper.Text = "Loading info...";
            lblDetailGenres.Text = "Loading genres...";
            lblDetailRating.Text = $"★ {simpleGame.Rating}";
            lblDetailMetacritic.Text = "-";
            lblDetailPlaytime.Text = "-";
            lblDetailDescription.Text = "Fetching details from RAWG...";
            lblDetailRequirements.Text = "Checking system requirements...";

            // 3. API Detay Sorgusu
            try
            {
                Game fullGame = await rawgapi.GetGameDetailsAsync(simpleGame.Id);

                if (fullGame != null)
                {
                    // Developer
                    if (fullGame.Developers != null && fullGame.Developers.Any())
                        lblDetailDeveloper.Text = $"Developer: {string.Join(", ", fullGame.Developers.Select(d => d.Name))}";
                    else
                        lblDetailDeveloper.Text = $"Released: {fullGame.Released ?? "Unknown"}";

                    // Genres
                    if (fullGame.Genres != null && fullGame.Genres.Any())
                        lblDetailGenres.Text = string.Join(" • ", fullGame.Genres.Select(g => g.Name));
                    else
                        lblDetailGenres.Text = "Genre info not available";

                    // Stats
                    lblDetailRating.Text = $"★ {fullGame.Rating:0.0} / 5";
                    lblDetailMetacritic.Text = fullGame.Metacritic != null ? fullGame.Metacritic.ToString() : "N/A";
                    lblDetailAge.Text = fullGame.EsrbRating != null ? fullGame.EsrbRating.Name : "Not Rated";
                    lblDetailPlaytime.Text = fullGame.Playtime > 0 ? $"⏱ {fullGame.Playtime} Hours" : "⏱ Not Set";

                    // Description
                    lblDetailDescription.Text = !string.IsNullOrEmpty(fullGame.DescriptionRaw) ? fullGame.DescriptionRaw : "No description provided.";

                    // Requirements (PC)
                    var pcPlatform = fullGame.Platforms?.FirstOrDefault(p => p.Platform.Slug == "pc" || p.Platform.Name.ToLower() == "pc");
                    if (pcPlatform != null && pcPlatform.Requirements != null)
                    {
                        string reqText = "";
                        if (!string.IsNullOrEmpty(pcPlatform.Requirements.Minimum))
                            reqText += "MINIMUM:\n" + CleanRequirementText(pcPlatform.Requirements.Minimum) + "\n\n";
                        if (!string.IsNullOrEmpty(pcPlatform.Requirements.Recommended))
                            reqText += "RECOMMENDED:\n" + CleanRequirementText(pcPlatform.Requirements.Recommended);

                        lblDetailRequirements.Text = string.IsNullOrWhiteSpace(reqText) ? "System requirements are not specified." : reqText;
                    }
                    else
                    {
                        lblDetailRequirements.Text = "PC requirements not found or available.";
                    }

                    // Platforms & Stores
                    if (fullGame.Platforms != null)
                        lblDetailPlatforms.Text = string.Join(", ", fullGame.Platforms.Select(p => p.Platform.Name).Take(5));

                    if (fullGame.Stores != null)
                        lblDetailStores.Text = "Stores: " + string.Join(", ", fullGame.Stores.Select(s => s.Store.Name));

                    // Modes
                    var modes = new List<string>();
                    if (fullGame.Tags != null)
                    {
                        if (fullGame.Tags.Any(t => t.Slug == "singleplayer")) modes.Add("Singleplayer");
                        if (fullGame.Tags.Any(t => t.Slug == "multiplayer")) modes.Add("Multiplayer");
                        if (fullGame.Tags.Any(t => t.Slug == "co-op")) modes.Add("Co-op");
                    }
                    lblDetailModes.Text = modes.Any() ? string.Join(" • ", modes) : "Mode info N/A";

                    // Actions
                    UpdateLibraryButtonState(fullGame);
                    LoadScreenshots(fullGame.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Detay Hatası: {ex.Message}");
                // Hata olsa bile eldeki basit veriyle devam, kullanıcıya hata basmıyoruz.
            }
        }

        private async void LoadScreenshots(int gameId)
        {
            flowLayoutScreenshots.Controls.Clear();
            var screenshots = await rawgapi.GetGameScreenshotsAsync(gameId); // API'den çek

            if (screenshots != null && screenshots.Any())
            {
                flowLayoutScreenshots.Visible = true;
                foreach (var ss in screenshots)
                {
                    PictureEdit pe = new PictureEdit();
                    pe.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
                    pe.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
                    pe.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                    pe.BackColor = System.Drawing.Color.Transparent;
                    pe.Size = new System.Drawing.Size(330, 200);
                    pe.Properties.AllowFocused = false;
                    pe.Properties.ShowMenu = false;

                    // Resmi Yükle
                    await imageManager.LoadImageAsync(ss.ImageUrl, pe, 600); // 600px kalite 
                    flowLayoutScreenshots.Controls.Add(pe);
                }
            }
            else
                flowLayoutScreenshots.Visible = false;
        }

        // Gereksinim yazılarında bazen "Minimum:" kelimesi tekrar ediyor, temizlemek için ufak helper
        private string CleanRequirementText(string rawReq)
        {
            if (string.IsNullOrEmpty(rawReq)) return "";
            return rawReq.Replace("Minimum:", "").Replace("Recommended:", "").Trim();
        }

        private void btnDetailBack_Click(object sender, EventArgs e)
        {
            // Nereden geldiysek oraya dönelim, yoksa Home
            if (previousPage != null)
                navigationFrame1.SelectedPage = previousPage;
            else
                navigationFrame1.SelectedPage = pageHome;
        }

        #endregion

        #region Database Interactions (Library Management)

        private void btnLibraryAction_Click(object sender, EventArgs e)
        {
            if (btnLibraryAction.Tag is Game game) // Tag'den oyunu al
            {
                // Context Menu oluştur
                ContextMenuStrip menu = new ContextMenuStrip();

                // Durum seçenekleri
                menu.Items.Add("Plan to Play", null, (s, args) => AddOrUpdateGame(game, "PlanToPlay"));
                menu.Items.Add("Playing", null, (s, args) => AddOrUpdateGame(game, "Playing"));
                menu.Items.Add("Played", null, (s, args) => AddOrUpdateGame(game, "Played"));
                menu.Items.Add("Dropped", null, (s, args) => AddOrUpdateGame(game, "Dropped"));
                menu.Items.Add(new ToolStripSeparator());
                menu.Items.Add("Remove from Library", null, (s, args) =>
                {
                    RemoveGameFromDb(game);
                    UpdateLibraryButtonState(game); // Butonu güncelle
                });

                // Menüyü butonun hemen altında göster
                menu.Show(btnLibraryAction, 0, btnLibraryAction.Height);
            }
        }

        // Hem Ekleme Hem Güncelleme yapar
        private void AddOrUpdateGame(Game game, string status)
        {
            if (!ValidateUserSession()) return;

            bool exists = LibraryManager.IsGameInLibrary(Session.UserId, game.Id);

            if (exists)
            {
                // Varsa güncelle
                LibraryManager.UpdateGameStatus(Session.UserId, game.Id, status);
                MyMessageBox.Show($"Game status updated to: {status}", "Updated");
            }
            else
            {
                // Yoksa ekle
                LibraryManager.AddGameToLibrary(Session.UserId, game, status);
                MyMessageBox.Show($"Game added to library as: {status}", "Success");
            }

            UpdateLibraryButtonState(game);
        }

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
        /// Kütüphane butonunun görünümünü oyunun durumuna göre günceller.
        /// </summary>
        private void UpdateLibraryButtonState(Game game)
        {
            if (Session.UserId <= 0) return;

            bool isInLib = LibraryManager.IsGameInLibrary(Session.UserId, game.Id);

            if (isInLib)
            {
                btnLibraryAction.Text = "✔ In Library";
                btnLibraryAction.Appearance.BackColor = System.Drawing.Color.FromArgb(40, 44, 60); // Koyu Gri (Pasif gibi)
                btnLibraryAction.Appearance.ForeColor = System.Drawing.Color.LightGreen;
            }
            else
            {
                btnLibraryAction.Text = "+ Add to Library";
                btnLibraryAction.Appearance.BackColor = System.Drawing.Color.FromArgb(40, 167, 69); // Yeşil (Aktif)
                btnLibraryAction.Appearance.ForeColor = System.Drawing.Color.White;
            }

            // Butonun Tag özelliğine oyunu atıyoruz ki tıklayınca hangi oyun olduğunu bilelim
            btnLibraryAction.Tag = game;
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

        #region Settings Page Logic

        // --- HESAP AYARLARI ---

        /// <summary>
        /// Kullanıcı adını günceller.
        /// </summary>
        private void btnUpdateUsername_Click(object sender, EventArgs e)
        {
            string newName = txtNewUsername.Text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                lblUsernameWarning.Text = "Username cannot be empty.";
                lblUsernameWarning.Visible = true;
                return;
            }

            if (newName == Session.Username)
            {
                lblUsernameWarning.Text = "This is already your current username.";
                lblUsernameWarning.Visible = true;
                return;
            }

            try
            {
                // Veritabanında güncelle
                bool success = UserManager.UpdateUsername(Session.UserId, newName);

                if (success)
                {
                    // Session'ı güncelle
                    Session.Username = newName;
                    MyMessageBox.Show($"Username updated to '{newName}' successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtNewUsername.Text = ""; // Inputu temizle
                    lblUsernameWarning.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Update failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Kullanıcı şifresini günceller.
        /// </summary>
        private void btnUpdatePass_Click(object sender, EventArgs e)
        {
            string currentPass = txtCurrentPass.Text;
            string newPass = txtNewPass.Text;
            string newPassAgain = txtNewPassAgain.Text;

            if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass))
            {
                lblPassWarning.Text = "Both fields are required.";
                lblPassWarning.Visible = true;
                return;
            }

            if (newPass != newPassAgain)
            {
                lblPassWarning.Text = "New passwords do not match.";
                lblPassWarning.Visible = true;
                return;
            }

            if (newPass.Length < 8)
            {
                lblPassWarning.Text = "New password must be at least 8 characters long.";
                lblPassWarning.Visible = true;
                return;
            }

            try
            {
                // Mevcut şifreyi doğrula
                bool isOldPassCorrect = UserManager.VerifyPassword(Session.UserId, currentPass);
                if (!isOldPassCorrect)
                {
                    lblPassWarning.Text = "Current password is incorrect.";
                    lblPassWarning.Visible = true;
                    return;
                }

                // Yeni şifreyi kaydet
                bool success = UserManager.UpdatePassword(Session.UserId, newPass);
                if (success)
                {
                    MyMessageBox.Show("Password changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblPassWarning.Visible = false;

                    // Inputları temizle
                    txtCurrentPass.Text = "";
                    txtNewPass.Text = "";
                    txtNewPassAgain.Text = "";

                    Properties.Settings.Default.StoredEmail = string.Empty;
                    Properties.Settings.Default.StoredPassword = string.Empty;
                    Properties.Settings.Default.RememberMe = false;
                    Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                MyMessageBox.Show($"Error changing password: {ex.Message}", "Error");
            }
        }

        // --- TERCİHLER ---

        private void cmbStartPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartPage = cmbStartPage.SelectedIndex == 0 ? "Home" : "Library";
            Properties.Settings.Default.Save();
        }

        private void toggleNSFW_Toggled(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowNSFW = toggleNSFW.IsOn;
            Properties.Settings.Default.Save();
        }

        private void LoadUserPreferences()
        {
            string startPage = Properties.Settings.Default.StartPage; // Start Page Ayarını Yükle
            cmbStartPage.SelectedIndex = (startPage == "Library") ? 1 : 0;
            toggleNSFW.IsOn = Properties.Settings.Default.ShowNSFW; // NSFW Ayarını Yükle
        }

        // --- SİSTEM ---

        /// <summary>
        /// RAM'deki resim önbelleğini temizler.
        /// </summary>
        private void btnClearCache_Click(object sender, EventArgs e)
        {
            // Doğrulama mesajı
            if (MyMessageBox.Show("This will clear all downloaded images from RAM. Continue?", "Clear Cache", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                imageManager.ClearMemoryCache();
                MyMessageBox.Show("Image cache cleared successfully! RAM is free now.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Oturumu kapatır, kayıtlı bilgileri siler ve Login ekranına döner.
        /// </summary>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            // Doğrulama mesajı
            if (MyMessageBox.Show("Are you sure you want to log out?", "Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                // Oturum verilerini sil
                Session.Clear();

                // Kaydedilmiş bilgileri temizle
                Properties.Settings.Default.StoredEmail = string.Empty;
                Properties.Settings.Default.StoredPassword = string.Empty;
                Properties.Settings.Default.RememberMe = false;
                Properties.Settings.Default.Save();

                this.Hide();

                // Login ekranını aç
                LoginForm login = new LoginForm();
                login.Show();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Butonlara hover efekti ekler.
        /// </summary>
        private void AddHoverEffectToButton(SimpleButton btn)
        {
            // Hover 
            btn.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(40, 42, 60);
            btn.AppearanceHovered.ForeColor = System.Drawing.Color.White;

            btn.AppearanceHovered.Options.UseBackColor = true;
            btn.AppearanceHovered.Options.UseForeColor = true;

            btn.Cursor = Cursors.Hand;
        }
        #endregion

        #region Navigation Menu
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