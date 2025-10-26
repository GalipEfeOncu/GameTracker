using DevExpress.Data;
using DevExpress.XtraEditors;
using GameTracker.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GameTracker
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private RawgApiService rawgapi;
        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 10 // Paralel bağlantı artır
        })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        // İmage cache
        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>();

        // Sayfalama değişkenleri
        private List<Game> allGames = new List<Game>();
        private int currentPage = 1;
        private int cardsPerPage = 24; // Başlangıç değeri, dinamik hesaplanacak
        private int totalPages = 1;
        private int RAWGPageNumber = 1;
        private int gameToLoadPerRequest = 100;

        int cardWidth = 250;
        int imageHeight;
        int labelHeight = 30;

        private System.Windows.Forms.Timer resizeTimer;

        public MainForm()
        {
            InitializeComponent();
            InitializeFlowLayoutPanel();
            rawgapi = new RawgApiService();

            // Timer'ı oluştur
            resizeTimer = new System.Windows.Forms.Timer();
            resizeTimer.Interval = 300; // 300ms bekle
            resizeTimer.Tick += ResizeTimer_Tick;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            CalculateGamesPerPage();
            await LoadAllGamesAsync(RAWGPageNumber, gameToLoadPerRequest);
            ShowCurrentPage();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            resizeTimer.Stop();
            resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            resizeTimer.Stop(); // Timer'ı durdur

            // Asıl işlemleri yap
            CalculateGamesPerPage();
            if (allGames.Count > 0)
            {
                ShowCurrentPage();
            }
        }

        private void CalculateGamesPerPage()
        {
            int availableWidth = flowLayoutPanelPopulerGames.ClientSize.Width;
            int availableHeight = flowLayoutPanelPopulerGames.ClientSize.Height;

            int cardTotalWidth = cardWidth + 20;
            int cardTotalHeight = (int)(cardWidth * 2 / 3) + labelHeight + 20;

            int cardsPerRow = Math.Max(1, availableWidth / cardTotalWidth);
            int rowsPerPage = Math.Max(1, availableHeight / cardTotalHeight);

            cardsPerPage = cardsPerRow * rowsPerPage;
            totalPages = (int)Math.Ceiling((double)allGames.Count / cardsPerPage);
        }

        private void InitializeFlowLayoutPanel()
        {
            // FlowLayoutPanel ayarları
            flowLayoutPanelPopulerGames.AutoSize = false;
            flowLayoutPanelPopulerGames.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowLayoutPanelPopulerGames.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanelPopulerGames.WrapContents = true;
            flowLayoutPanelPopulerGames.Padding = new Padding(0);
            flowLayoutPanelPopulerGames.AutoScroll = false;

            pageHome.AutoScroll = false;
        }

        private async Task LoadAllGamesAsync(int pageNumber, int totalGames)
        {
            allGames = await rawgapi.GetPopularGamesAsync(pageNumber, totalGames);
        }

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
        }

        private Panel CreateGameCard(Game game)
        {
            imageHeight = (int)(cardWidth * 2 / 3);

            // Panel
            Panel card = new Panel();
            card.Width = cardWidth;
            card.Height = imageHeight + 30; // +30 label için
            card.Margin = new Padding(10);
            card.BorderStyle = BorderStyle.None;
            card.BackColor = Color.FromArgb(26, 29, 41);

            // Resim
            PictureEdit pe = new PictureEdit();
            pe.Dock = DockStyle.Top;
            pe.Width = cardWidth;
            pe.Height = imageHeight;

            pe.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Zoom;
            pe.Properties.ReadOnly = true; // kullanıcı resmi değiştiremez
            pe.Image = Resource1.loading;
            card.Controls.Add(pe);

            // Label
            LabelControl lbl = new LabelControl();
            lbl.Text = game.Name ?? "No Name";
            lbl.Dock = DockStyle.Bottom;
            lbl.Height = labelHeight;
            lbl.AutoSizeMode = LabelAutoSizeMode.None;
            lbl.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lbl.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lbl.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            lbl.Appearance.BackColor = Color.FromArgb(26, 29, 41);
            lbl.Appearance.ForeColor = Color.White;
            card.Controls.Add(lbl);

            // Async resim yükle
            LoadImageAsync(game.BackgroundImage, pe);

            return card;
        }

        private string GetResizedImageUrl(string originalUrl, int width)
        {
            if (string.IsNullOrEmpty(originalUrl)) return originalUrl;

            string resizedUrl = originalUrl
                .Replace("media/games/", $"media/resize/{width}/-/games/")
                .Replace("media/screenshots/", $"media/resize/{width}/-/screenshots/");

            return resizedUrl;
        }

        Image FixTo3x2(Image original)
        {
            int targetWidth = original.Width;
            int targetHeight = (int)(targetWidth * 2.0 / 3.0);

            if (targetHeight > original.Height)
            {
                targetHeight = original.Height;
                targetWidth = (int)(targetHeight * 3.0 / 2.0);
            }

            int x = (original.Width - targetWidth) / 2;
            int y = (original.Height - targetHeight) / 2;

            Bitmap bmp = new Bitmap(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(original, 0, 0, new Rectangle(x, y, targetWidth, targetHeight), GraphicsUnit.Pixel);
            }

            return bmp;
        }


        private async void LoadImageAsync(string imageUrl, PictureEdit pictureEdit)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            // Boyutu düşürmek için URL Resize yapar
            var resizedUrl = GetResizedImageUrl(imageUrl, 420);

            // Eğer cache'te varsa oradan alır
            if (imageCache.TryGetValue(resizedUrl, out Image cachedImage))
            {
                pictureEdit.Image = FixTo3x2(cachedImage);
                return;
            }
            try
            {
                // Resmi indirir
                var bytes = await httpClient.GetByteArrayAsync(resizedUrl);

                using (var ms = new System.IO.MemoryStream(bytes)) // Byte dizisini stream'e çevirir
                {
                    var img = Image.FromStream(ms);
                    imageCache[resizedUrl] = img; // Cache'e ekler
                    pictureEdit.Image = FixTo3x2(img);
                }
            }
            catch (Exception ex)
            { Console.WriteLine($"Resim yüklenemedi: {imageUrl} -> {ex.Message}"); }
        }

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
        }

        // Memory leak önleme
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Cache'teki resimleri temizle
            foreach (var img in imageCache.Values)
            {
                img?.Dispose();
            }
            imageCache.Clear();
        }
    }
}