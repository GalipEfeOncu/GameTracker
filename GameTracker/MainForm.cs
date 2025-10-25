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
        private static readonly HttpClient httpClient = new HttpClient();

        // İmage cache
        private readonly ConcurrentDictionary<string, Image> _imageCache = new ConcurrentDictionary<string, Image>();

        // Sayfalama değişkenleri
        private List<Game> allGames = new List<Game>();
        private int currentPage = 1;
        private int cardsPerPage = 24; // Başlangıç değeri, dinamik hesaplanacak
        private int totalPages = 1;

        int cardWidth = 250;
        int imageHeight;
        int labelHeight = 30;

        public MainForm()
        {
            InitializeComponent();
            InitializeFlowLayoutPanel();
            rawgapi = new RawgApiService();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            CalculateGamesPerPage();
            await LoadAllGamesAsync(100);
            ShowCurrentPage();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
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

        private async Task LoadAllGamesAsync(int totalGames)
        {
            allGames = await rawgapi.GetPopularGamesAsync(totalGames);
            //totalPages = (int)Math.Ceiling((double)allGames.Count / cardsPerPage);
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
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.None;
            card.BackColor = Color.FromArgb(26, 29, 41);

            // Resim
            PictureEdit pe = new PictureEdit();
            pe.Dock = DockStyle.Top;
            pe.Width = cardWidth;
            pe.Height = imageHeight;
            pe.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            pe.Properties.ReadOnly = true; // kullanıcı resmi değiştiremez
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

        private async void LoadImageAsync(string imageUrl, PictureEdit pictureEdit)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            if (_imageCache.TryGetValue(imageUrl, out Image cachedImage))
            {
                pictureEdit.Image = cachedImage;
                return;
            }
            try
            {
                var bytes = await httpClient.GetByteArrayAsync(imageUrl);
                using (var ms = new System.IO.MemoryStream(bytes))
                {
                    var img = Image.FromStream(ms);
                    _imageCache[imageUrl] = img;
                    pictureEdit.Image = img;
                }
            }
            catch { }
        }

        private async Task LoadPopulerGamesAsync(int gameNumber)
        {
            var games = await rawgapi.GetPopularGamesAsync(gameNumber);

            flowLayoutPanelPopulerGames.Controls.Clear();

            foreach (var game in games)
            {
                var card = CreateGameCard(game);
                flowLayoutPanelPopulerGames.Controls.Add(card);
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                ShowCurrentPage();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
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
    }
}