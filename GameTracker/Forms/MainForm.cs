using DevExpress.XtraEditors;
using GameTracker.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameTracker
{

    /// <summary>
    /// Ana uygulama formu. Popüler oyunları gösterir, kütüphaneyi yönetir ve
    /// RAWG API'sinden veri çeker.
    /// </summary>
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        #region Fields
        private RawgApiService rawgapi;
        private readonly ImageManager imageManager;
        private readonly LayoutCalculator layoutCalculator;

        // Sayfalama değişkenleri
        private List<Game> allGames = new List<Game>();
        private int currentPage = 1;
        private int cardsPerPage = 24; // Başlangıç değeri, dinamik hesaplanacak
        private int totalPages = 1;
        private int RAWGPageNumber = 1;
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

            // Timer'ı oluştur
            resizeTimer = new System.Windows.Forms.Timer();
            resizeTimer.Interval = 300; // 300ms bekler
            resizeTimer.Tick += ResizeTimer_Tick;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Yeni calculator'ı çağır ve sonucu field'a atar
            currentLayoutMetrics = layoutCalculator.Calculate(flowLayoutPanelPopulerGames.ClientSize);
            cardsPerPage = currentLayoutMetrics.CardsPerPage; // cardsPerPage'i günceller
            await LoadAllGamesAsync(RAWGPageNumber, gameToLoadPerRequest);
            ShowCurrentPage();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            imageManager?.Dispose(); // ImageManager'daki cache'i temizle
        }
        #endregion

        #region Resize Timer
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop(); // Timer'ı durdurur

            // Yeni calculator'ı çağır ve sonucu field'a atar
            currentLayoutMetrics = layoutCalculator.Calculate(flowLayoutPanelPopulerGames.ClientSize);
            cardsPerPage = currentLayoutMetrics.CardsPerPage; // cardsPerPage'i günceller
            if (allGames.Count > 0)
            {
                ShowCurrentPage();
            }

            totalPages = (int)Math.Ceiling((double)allGames.Count / cardsPerPage);

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
                ShowCurrentPage();
            }
        }
        #endregion

        #region Layout Calculation & Initialization
        /// <summary>
        /// Uygulamadaki 'Popüler Oyunlar' ve 'Kütüphane' FlowLayoutPanel'lerinin temel görsel ayarlarını yapar.
        /// </summary>
        private void InitializeFlowLayoutPanel()
        {
            // FlowLayoutPanel ayarları
            FlowLayoutPanel fPopuler = flowLayoutPanelPopulerGames;
            fPopuler.AutoSize = false;
            fPopuler.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fPopuler.FlowDirection = FlowDirection.LeftToRight;
            fPopuler.WrapContents = true;
            fPopuler.Padding = new Padding(0);
            fPopuler.AutoScroll = false;

            FlowLayoutPanel fLib = flowLayoutPanelLibrary;
            fLib.AutoSize = false;
            fLib.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fLib.FlowDirection = FlowDirection.LeftToRight;
            fLib.WrapContents = true;
            fLib.Padding = new Padding(0);
            fLib.AutoScroll = false;

            pageHome.AutoScroll = false;
            pageLibrary.AutoScroll = false;
            pageSearch.AutoScroll = false;
        }
        #endregion

        #region Data Loading & Pagination

        /// <summary>
        /// RAWG API'sinden belirtilen sayfa ve adette popüler oyun verisini asenkron olarak çeker ve 'allGames' listesini doldurur.
        /// </summary>
        private async Task LoadAllGamesAsync(int pageNumber, int totalGames)
        {
            allGames = await rawgapi.GetPopularGamesAsync(pageNumber, totalGames);
        }

        /// <summary>
        /// Kütüphanedeki oyunları veritabanından çeker ve ekrana basar.
        /// </summary>
        private void LoadLibraryGames()
        {
            flowLayoutPanelLibrary.SuspendLayout();
            flowLayoutPanelLibrary.Controls.Clear();

            // Session.UserId 0 ise giriş yapılmamıştır
            if (Session.UserId <= 0) return;

            // LibraryManager'dan oyunları çek
            var libraryGames = LibraryManager.GetUserLibrary(Session.UserId);

            foreach (var game in libraryGames)
            {
                var card = CreateGameCard(game);
                flowLayoutPanelLibrary.Controls.Add(card);
            }

            flowLayoutPanelLibrary.ResumeLayout();
        }

        /// <summary>
        /// Mevcut sayfadaki oyunları 'allGames' listesinden alarak ekrana oyun kartlarını çizer ve sayfa numarasını günceller.
        /// </summary>
        private void ShowCurrentPage()
        {
            flowLayoutPanelPopulerGames.SuspendLayout();
            flowLayoutPanelPopulerGames.Controls.Clear();

            // Mevcut sayfanın oyunlarını alır
            var currentGames = allGames
                .Skip((currentPage - 1) * cardsPerPage)
                .Take(cardsPerPage)
                .ToList();

            foreach (var games in currentGames)
            {
                var card = CreateGameCard(games);
                flowLayoutPanelPopulerGames.Controls.Add(card);
            }

            flowLayoutPanelPopulerGames.ResumeLayout();

            lblPage.Text = $"Sayfa {currentPage} / {totalPages}";
        }
        #endregion

        #region Card Creation

        /// <summary>
        /// Verilen 'Game' objesi için resim, başlık ve hover efekti içeren bir UI kartı (Panel) oluşturur.
        /// </summary>
        private Panel CreateGameCard(Game game)
        {

            // Panel
            Panel card = new Panel();
            card.Width = currentLayoutMetrics.CardWidth;
            card.Height = currentLayoutMetrics.ImageHeight + labelHeight;
            card.Margin = new Padding(10, 10, 10, 10 + currentLayoutMetrics.ExtraSpacingPerRow);
            card.Padding = new Padding(0);
            card.BorderStyle = BorderStyle.None;
            card.BackColor = Color.FromArgb(26, 29, 41);

            // Resim
            PictureEdit pe = new PictureEdit();
            pe.Location = new Point(0, 0);
            pe.Width = currentLayoutMetrics.CardWidth;
            pe.Height = currentLayoutMetrics.ImageHeight;
            pe.Margin = new Padding(0);
            pe.Properties.Appearance.BackColor = Color.FromArgb(26, 29, 41);
            pe.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            pe.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pe.Properties.ReadOnly = true; // kullanıcı resmi değiştiremez
            pe.Properties.ShowMenu = false; // sağ tıklama menüsünü gizler
            pe.Image = Resource1.loading;
            card.Controls.Add(pe);

            int addedWidth = (int)(pe.Width * 0.1);
            int addedHeight = (int)(pe.Height * 0.1);

            pe.MouseEnter += (s, e) =>
            {
                pe.Location = new Point(-addedWidth / 2, -addedHeight / 2);
                pe.Width += addedWidth;
                pe.Height += addedHeight;
                pe.SendToBack();
            };

            pe.MouseLeave += (s, e) =>
            {
                pe.Location = new Point(0, 0);
                pe.Width -= addedWidth;
                pe.Height -= addedHeight;
            };

            // Label
            LabelControl lbl = new LabelControl();
            lbl.Text = game.Name ?? "No Name";
            lbl.Dock = DockStyle.Bottom;
            lbl.Height = labelHeight;
            lbl.Margin = new Padding(0);
            lbl.AutoSizeMode = LabelAutoSizeMode.None;
            lbl.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lbl.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lbl.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lbl.Appearance.BackColor = Color.FromArgb(26, 29, 41);
            lbl.Appearance.ForeColor = Color.White;
            lbl.BringToFront();
            card.Controls.Add(lbl);

            // Sağ Tık Menüsü (Context Menu)
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            // LOGIC: Eğer Kütüphane sayfasında DEĞİLSEK "Add" göster.
            // Böylece kütüphanedeyken gereksiz yere "Ekle" çıkmaz.
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

            // Eğer Kütüphane sayfasındaysak hem "Remove" hem de "Durum Değiştir" olsun
            if (navigationFrame1.SelectedPage == pageLibrary)
            {
                // 1. Move to... (Statü Değiştirme - Bonus Özellik 😉)
                ToolStripMenuItem changeStatusItem = new ToolStripMenuItem("Move to...");
                ToolStripMenuItem movePlan = new ToolStripMenuItem("Plan to Play");
                ToolStripMenuItem movePlaying = new ToolStripMenuItem("Playing");
                ToolStripMenuItem movePlayed = new ToolStripMenuItem("Played");

                // Yeni helper metodu kullanıyoruz
                movePlan.Click += (s, e) => UpdateGameStatusDb(game, "PlanToPlay");
                movePlaying.Click += (s, e) => UpdateGameStatusDb(game, "Playing");
                movePlayed.Click += (s, e) => UpdateGameStatusDb(game, "Played");

                changeStatusItem.DropDownItems.Add(movePlan);
                changeStatusItem.DropDownItems.Add(movePlaying);
                changeStatusItem.DropDownItems.Add(movePlayed);
                contextMenu.Items.Add(changeStatusItem);

                // 2. Remove
                ToolStripMenuItem removeItem = new ToolStripMenuItem("Remove from Library");
                removeItem.Click += (s, e) => RemoveGameFromDb(game);
                contextMenu.Items.Add(removeItem);
            }

            // Menüyü panele ve resme bağla (Nereye sağ tıklarsa açılsın)
            card.ContextMenuStrip = contextMenu;
            pe.ContextMenuStrip = contextMenu; // PictureEdit'e de ekle
            lbl.ContextMenuStrip = contextMenu; // Label'a da ekle

            // Async resim yükle
            imageManager.LoadImageAsync(game.BackgroundImage, pe, 420);

            return card;
        }
        #endregion

        #region Helper Methods
        private void AddGameToDb(Game game, string status)
        {
            if (Session.UserId <= 0)
            {
                XtraMessageBox.Show("Please login first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = LibraryManager.AddGameToLibrary(Session.UserId, game, status);
            if (success)
            {
                XtraMessageBox.Show($"{game.Name} added to {status} list!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                XtraMessageBox.Show($"{game.Name} is already in your library!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Helper metod: Oyunun statüsünü günceller (Playing -> Played vs.)
        private void UpdateGameStatusDb(Game game, string newStatus)
        {
            if (Session.UserId <= 0) return;

            bool success = LibraryManager.UpdateGameStatus(Session.UserId, game.Id, newStatus);
            if (success)
            {
                // Kullanıcıyı çok darlamadan ufak bir bilgi verelim ya da direkt listeyi yenileyelim
                LoadLibraryGames(); // Listeyi yenile ki oyun yeni sekmesine ışınlansın
            }
        }

        // Helper metod: Oyunu DB'den siler
        private void RemoveGameFromDb(Game game)
        {
            if (Session.UserId <= 0) return;

            bool success = LibraryManager.RemoveGame(Session.UserId, game.Id);
            if (success)
            {
                LoadLibraryGames(); // Listeyi yenile ki silinen gitsin
            }
        }
        #endregion

        #region Navigation & Paging Buttons
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                ShowCurrentPage();
            }
        }

        private async void btnNext_Click(object sender, EventArgs e)
        {
            if (currentPage >= totalPages - 1)
            {
                btnNext.Enabled = false;

                RAWGPageNumber++;
                var moreGames = await rawgapi.GetPopularGamesAsync(RAWGPageNumber, gameToLoadPerRequest);

                if (moreGames != null && moreGames.Count > 0)
                {
                    allGames.AddRange(moreGames);
                    totalPages = (int)Math.Ceiling((double)allGames.Count / cardsPerPage);
                }

                btnNext.Enabled = true;
            }

            if (currentPage < totalPages)
            {
                currentPage++;
                ShowCurrentPage();
            }
        }

        private void btnHomeMenu_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageHome;
        }

        private void btnLibrary_Click(object sender, EventArgs e)
        {
            navigationFrame1.SelectedPage = pageLibrary;
            LoadLibraryGames();
        }
        #endregion
    }
}