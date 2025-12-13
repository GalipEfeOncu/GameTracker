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

        // Geri dönünce hangi sayfadaydık hatırlamak için
        private DevExpress.XtraBars.Navigation.NavigationPage previousPage;

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
            UiHelper.EnableDoubleBuffering(flowLayoutPanelPopulerGames, flowLayoutPanelLibrary, flowLayoutPanelSearch, flowLayoutScreenshots, scrollableDetailContainer);

            var activePanel = navigationFrame1.SelectedPage?.Controls[0] as FlowLayoutPanel;
            RecalculateLayoutMetrics(activePanel);
            RefreshActivePage();

            // Resize timer'ı başlat
            InitializeResizeTimer();

            AddHoverEffectToButton(btnLibPlanToPlay);
            AddHoverEffectToButton(btnLibPlaying);
            AddHoverEffectToButton(btnLibPlayed);
            AddHoverEffectToButton(btnLibDropped);
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
            if (HandleNoResults(manager, targetPanel, pageLabel, noResultLabel))
                return;

            // Verileri çek
            var gamesToShow = manager.GetCurrentPageItems();

            targetPanel.SuspendLayout();
            targetPanel.Controls.Clear();

            // Kartları Ekle
            AddGameCardsToPanel(gamesToShow, targetPanel);
            targetPanel.ResumeLayout();

            pageLabel.Text = manager.GetPageInfoString(); // Etiketi güncelle
        }

        /// <summary>
        /// Eğer hiç veri yoksa uyarı gösterir.
        /// </summary>
        private bool HandleNoResults(PageManager manager, FlowLayoutPanel targetPanel, LabelControl pageLabel, LabelControl noResultLabel)
        {
            // Eğer sonuç yoksa (AllItems.Count 0 ise)
            if (manager.AllItems.Count == 0)
            {
                targetPanel.Controls.Clear(); // Paneli temizle
                pageLabel.Text = "Page 0 / 0";
                if (noResultLabel != null)
                {
                    noResultLabel.Width = targetPanel.Width - 50;
                    targetPanel.Controls.Add(noResultLabel);
                    noResultLabel.Visible = true;
                }
                return true; // İşlemi durdur
            }

            // Sonuç varsa label'ı gizle
            if (noResultLabel != null) noResultLabel.Visible = false;
            return false; // Devam et
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
                // onStatusChange parametresine 'null' vererek kart üzerindeki sağ tıkı iptal ediyoruz.
                // Artık aksiyonu sadece detay sayfasından alacağız.
                var card = GameCardFactory.CreateCard(game, currentLayoutMetrics, imageManager, onCardClick: (g) => ShowGameDetails(g));

                panel.Controls.Add(card);
            }
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
            var myGames = LibraryManager.GetUserLibrary(Session.UserId, currentLibraryFilter);

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

        #region Game Detail Logic
        /// <summary>
        /// Oyun detaylarını gösterir. Önce eldeki veriyi basar, sonra API'den full detayı çeker.
        /// </summary>
        private async void ShowGameDetails(Game simpleGame)
        {
            // 1. NAVIGATION & UI RESET
            // Önceki sayfayı hatırla (Geri butonu için)
            previousPage = navigationFrame1.SelectedPage as DevExpress.XtraBars.Navigation.NavigationPage;

            // Detay sayfasına geç
            navigationFrame1.SelectedPage = pageGameDetail;

            // Scroll'u en tepeye al (önceki oyundan aşağıda kalmasın)
            scrollableDetailContainer.VerticalScroll.Value = 0;

            // 2. ELDEKİ VERİYİ HEMEN GÖSTER (Kullanıcı Beklemesin)
            lblDetailTitle.Text = simpleGame.Name;

            // Resmi yükle (Cache mekanizması sayesinde hızlı gelir)
            peDetailImage.Image = null;
            imageManager.LoadImageAsync(simpleGame.BackgroundImage, peDetailImage, 600); // 600px genişlik istedik

            // Geçici "Yükleniyor" mesajları
            lblDetailDeveloper.Text = "Loading info...";
            lblDetailGenres.Text = "Loading genres...";
            lblDetailRating.Text = $"★ {simpleGame.Rating}";
            lblDetailMetacritic.Text = "-";
            lblDetailPlaytime.Text = "-";
            lblDetailDescription.Text = "Fetching details from RAWG...";
            lblDetailRequirements.Text = "Checking system requirements...";

            // 3. API'DEN FULL DETAYLARI ÇEK (Async)
            try
            {
                // API isteği atılıyor...
                Game fullGame = await rawgapi.GetGameDetailsAsync(simpleGame.Id);

                if (fullGame != null)
                {
                    // --- BAŞLIK & GELİŞTİRİCİ ---
                    // Developer listesini birleştir (Örn: "Ubisoft, Massive Entertainment")
                    if (fullGame.Developers != null && fullGame.Developers.Any())
                    {
                        string devs = string.Join(", ", fullGame.Developers.Select(d => d.Name));
                        lblDetailDeveloper.Text = $"Developer: {devs}";
                    }
                    else
                    {
                        lblDetailDeveloper.Text = $"Released: {fullGame.Released ?? "Unknown"}";
                    }

                    // --- TÜRLER (GENRES) ---
                    if (fullGame.Genres != null && fullGame.Genres.Any())
                    {
                        string genres = string.Join(" • ", fullGame.Genres.Select(g => g.Name));
                        lblDetailGenres.Text = genres;
                    }
                    else
                    {
                        lblDetailGenres.Text = "Genre info not available";
                    }

                    // --- İSTATİSTİKLER ---
                    lblDetailRating.Text = $"★ {fullGame.Rating:0.0} / 5"; // 0.0 formatı (örn: 4.5)

                    // Metacritic (Varsa yaz, yoksa N/A)
                    if (fullGame.Metacritic != null)
                        lblDetailMetacritic.Text = fullGame.Metacritic.ToString();
                    else
                        lblDetailMetacritic.Text = "N/A";

                    // --- YAŞ SINIRI (ESRB) ---
                    if (fullGame.EsrbRating != null)
                        lblDetailAge.Text = fullGame.EsrbRating.Name; // "Mature", "Everyone" vs.
                    else
                        lblDetailAge.Text = "Not Rated";

                    // Playtime
                    if (fullGame.Playtime > 0)
                        lblDetailPlaytime.Text = $"⏱ {fullGame.Playtime} Hours";
                    else
                        lblDetailPlaytime.Text = "⏱ Not Set";

                    // --- AÇIKLAMA (DESCRIPTION) ---
                    // HTML olmayan temiz text (DescriptionRaw) kullanıyoruz
                    if (!string.IsNullOrEmpty(fullGame.DescriptionRaw))
                        lblDetailDescription.Text = fullGame.DescriptionRaw;
                    else
                        lblDetailDescription.Text = "No description provided for this game.";

                    // --- SİSTEM GEREKSİNİMLERİ (PC) ---
                    // Platforms listesinden "PC" olanı bulup gereksinimlerini çekeceğiz
                    var pcPlatform = fullGame.Platforms?.FirstOrDefault(p =>
                        p.Platform.Slug == "pc" || p.Platform.Name.ToLower() == "pc");

                    if (pcPlatform != null && pcPlatform.Requirements != null)
                    {
                        string reqText = "";

                        // Minimum var mı?
                        if (!string.IsNullOrEmpty(pcPlatform.Requirements.Minimum))
                        {
                            reqText += "MINIMUM:\n" + CleanRequirementText(pcPlatform.Requirements.Minimum) + "\n\n";
                        }

                        // Recommended var mı?
                        if (!string.IsNullOrEmpty(pcPlatform.Requirements.Recommended))
                        {
                            reqText += "RECOMMENDED:\n" + CleanRequirementText(pcPlatform.Requirements.Recommended);
                        }

                        if (string.IsNullOrWhiteSpace(reqText))
                            lblDetailRequirements.Text = "System requirements are not specified.";
                        else
                            lblDetailRequirements.Text = reqText;
                    }
                    else
                    {
                        lblDetailRequirements.Text = "PC requirements not found or available.";
                    }

                    // --- PLATFORMLAR ---
                    if (fullGame.Platforms != null && fullGame.Platforms.Any())
                    {
                        var platNames = fullGame.Platforms.Select(p => p.Platform.Name).Take(5); // İlk 5'i al çok uzamasın
                        lblDetailPlatforms.Text = string.Join(", ", platNames);
                    }
                    else lblDetailPlatforms.Text = "Platforms: N/A";

                    // --- MAĞAZALAR ---
                    if (fullGame.Stores != null && fullGame.Stores.Any())
                    {
                        var storeNames = fullGame.Stores.Select(s => s.Store.Name);
                        lblDetailStores.Text = "Stores: " + string.Join(", ", storeNames);
                    }
                    else lblDetailStores.Text = "Store info unavailable";

                    // --- OYNANIŞ ---
                    // API'de "Tags" içinde "Singleplayer", "Multiplayer" geçer.
                    var modes = new List<string>();
                    if (fullGame.Tags != null)
                    {
                        if (fullGame.Tags.Any(t => t.Slug == "singleplayer")) modes.Add("Singleplayer");
                        if (fullGame.Tags.Any(t => t.Slug == "multiplayer")) modes.Add("Multiplayer");
                        if (fullGame.Tags.Any(t => t.Slug == "co-op")) modes.Add("Co-op");
                    }
                    lblDetailModes.Text = modes.Any() ? string.Join(" • ", modes) : "Mode info N/A";

                    // --- KÜTÜPHANE BUTONUNU GÜNCELLE ---
                    // Burası çok önemli: Oyunun ID'sini bildiğimiz için durumunu kontrol edelim
                    UpdateLibraryButtonState(fullGame);

                    // --- EKRAN GÖRÜNTÜLERİ ---
                    LoadScreenshots(fullGame.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Detay Hatası: {ex.Message}");
                // Hata olsa bile eldeki basit veriyle devam, kullanıcıya hata basmıyoruz.
            }
        }

        // Gereksinim yazılarında bazen "Minimum:" kelimesi tekrar ediyor, temizlemek için ufak helper
        private string CleanRequirementText(string rawReq)
        {
            if (string.IsNullOrEmpty(rawReq)) return "";
            // Bazı API yanıtlarında "Minimum:" kelimesi metnin içinde geliyor, temizleyelim
            return rawReq.Replace("Minimum:", "").Replace("Recommended:", "").Trim();
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

                // Silme seçeneği
                menu.Items.Add("Remove from Library", null, (s, args) =>
                {
                    RemoveGameFromDb(game);
                    UpdateLibraryButtonState(game); // Butonu güncelle
                });

                // Menüyü butonun hemen altında göster
                menu.Show(btnLibraryAction, 0, btnLibraryAction.Height);
            }
        }

        // Yardımcı Metot: Hem Ekleme Hem Güncelleme yapar
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

            // İşlem bitince butonu güncelle
            UpdateLibraryButtonState(game);
        }

        private void btnDetailBack_Click(object sender, EventArgs e)
        {
            // Nereden geldiysek oraya dönelim, yoksa Home'a dönelim
            if (previousPage != null)
                navigationFrame1.SelectedPage = previousPage;
            else
                navigationFrame1.SelectedPage = pageHome;
        }

        private async void LoadScreenshots(int gameId)
        {
            // Paneli temizle
            flowLayoutScreenshots.Controls.Clear();

            // API'den çek
            var screenshots = await rawgapi.GetGameScreenshotsAsync(gameId);

            if (screenshots != null && screenshots.Any())
            {
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
                    imageManager.LoadImageAsync(ss.ImageUrl, pe, 600); // 600px kalite 
                    flowLayoutScreenshots.Controls.Add(pe);
                }
            }
            else
            {
                // Screenshot yoksa paneli gizle ki boşluk kalmasın
                flowLayoutScreenshots.Visible = false;
            }
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

        #region Library Filter Logic

        // Global bir değişken tutalım ki şu an hangi filtredeyiz bilelim
        private string currentLibraryFilter = null;

        // Tüm filtre butonları bu evente bağlı
        private void FilterLibrary_Click(object sender, EventArgs e)
        {
            var clickedButton = sender as SimpleButton;
            if (clickedButton == null) return;

            string selectedTag = clickedButton.Tag as string;

            // --- TOGGLE (AÇ/KAPA) MANTIĞI ---
            if (currentLibraryFilter == selectedTag)
            {
                // Eğer zaten bu filtredeysek, filtreyi kaldır (Hepsini göster)
                currentLibraryFilter = null;
                HighlightActiveFilterButton(null); // Hiçbir butonu yakma
            }
            else
            {
                // Yeni bir filtre seçildi
                currentLibraryFilter = selectedTag;
                HighlightActiveFilterButton(clickedButton); // Sadece bunu yak
            }

            // Listeyi yeni duruma göre güncelle
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
                    // Aktif Buton: Yeşil ve Kalın Font
                    btn.Appearance.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
                    btn.Appearance.ForeColor = System.Drawing.Color.White;
                    btn.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
                }
                else
                {
                    // Pasif Buton: Koyu Gri ve Normal Font
                    btn.Appearance.BackColor = System.Drawing.Color.FromArgb(32, 34, 50);
                    btn.Appearance.ForeColor = System.Drawing.Color.WhiteSmoke;
                    btn.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
                }
            }
        }
        #endregion

        /// <summary>
        /// Butonlara hover (üzerine gelme) efekti ekler.
        /// </summary>
        private void AddHoverEffectToButton(SimpleButton btn)
        {
            // Butonun kenarlıklarını kaldır (Zaten kaldırmıştık ama garanti olsun)
            btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            // Hover (Üzerine gelince) Rengi: O soldaki menüdeki koyu gri tonu
            btn.AppearanceHovered.BackColor = System.Drawing.Color.FromArgb(40, 42, 60);
            btn.AppearanceHovered.ForeColor = System.Drawing.Color.White;

            // DevExpress'e "Bu ayarları kullan" diyoruz
            btn.AppearanceHovered.Options.UseBackColor = true;
            btn.AppearanceHovered.Options.UseForeColor = true;

            // El işareti çıksın (Hand Cursor)
            btn.Cursor = Cursors.Hand;
        }
    }
}