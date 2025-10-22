using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameTracker.Models;

namespace GameTracker
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private RawgApiService rawgapi;
        private static readonly HttpClient httpClient = new HttpClient();

        // Populer games değişkenleri
        private int currentPopularPage = 1;
        private bool isLoadingMorePopular = false;

        public MainForm()
        {
            InitializeComponent();
            rawgapi = new RawgApiService();
            SetupSearchControl();
            SetupScrollListener();

            this.MinimumSize = new System.Drawing.Size(1200, 675);

            this.Load += async (s, e) =>
            {
                await LoadPopulerGames();
                ApplyDynamicLayout();
            };

            // FlowLayoutPanel resize olduğunda tekrar düzenle
            this.Resize += (s, e) =>
            {
                ApplyDynamicLayout();
            };
        }

        private void SetupScrollListener()
        {
            xtraScrollableControlHome.MouseWheel += async (s, e) =>
            {
                var scrollControl = s as XtraScrollableControl;
                if (scrollControl == null) return;

                int scrollPos = scrollControl.VerticalScroll.Value; // Scroll çubuğunun şuanki pozisyonu
                int scrollMax = scrollControl.VerticalScroll.Maximum - scrollControl.VerticalScroll.LargeChange; // Scroll çubuğunun maksimum pozisyonu

                // Scroll çubuğu en alta %80 yaklaştığında daha fazla oyun yükler
                if (scrollPos >= scrollMax * 0.7 && !isLoadingMorePopular)
                {
                    await LoadMorePopularGames();
                }
            };
        }

        private async Task LoadMorePopularGames()
        {
            // Anlık olarak yükleme yapılıyorsa çıkar
            if (isLoadingMorePopular)
                return;

            isLoadingMorePopular = true; // Yükleme işlemi başladığını belirtir

            try
            {
                // Sonraki sayfadan oyunları çek (API'den sürekli yeni oyunlar gelir)
                currentPopularPage++;
                var newGames = await rawgapi.GetPopularGamesAsync(20, currentPopularPage);

                if (newGames.Count > 0)
                {
                    // Her oyun için paralel olarak CreateGameCard methodunu çağırması için Select ile görevler oluşturur
                    var tasks = newGames.Select(g => CreateGameCard(g));
                    // Task.WhenAll ile tüm görevlerin paralel çalıştırır ve tamamlanmasını bekler (await)
                    var cards = await Task.WhenAll(tasks);
                    flowLayoutPanelPopulerGames.Controls.AddRange(cards);

                    DynamicCards(flowLayoutPanelPopulerGames, multiRow: true);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Daha fazla oyun yüklenirken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isLoadingMorePopular = false;   // Hata olsa da olmasa da yükleme işlemi bittiğinde bayrağı indirir
            }
        }

        private void ApplyDynamicLayout()
        {
            DynamicCards(flowLayoutPanelPopulerGames, true);
            DynamicCards(flowLayoutPanelSuggests, false);
            DynamicCards(flowLayoutPanelLibrary, true);
        }

        private void DynamicCards(FlowLayoutPanel flowPanel, bool multiRow = false)
        {
            int cardWidth;
            int cardHeight;
            int spaceBetweenCards = 10;
            int margin = 20;

            flowPanel.SuspendLayout();

            // flowPanelin genişliğini XtraScrollableControl'ün genişliğine eşitler
            if (flowPanel.Parent is XtraScrollableControl scrollControl) // flowPanelin parenti XtraScrollableControl mü?
            {
                flowPanel.Width = scrollControl.ClientSize.Width - margin;
            }

            // Kart sayısını hesapla (flowPanel içinde kaç kart var)
            int cardCount = flowPanel.Controls.Count;
            if (cardCount == 0)
            {
                flowPanel.ResumeLayout();
                return;
            }

            // Kullanılabilir genişlik
            int availableWidth = flowPanel.Width - (margin * 2);

            #region --- KART BOYUTU HESAPLAMA ---
            int minCardWidth = 250;
            int maxCardWidth = 350;
            cardWidth = maxCardWidth;

            int cardsPerRow = 1;

            // Boyut hesaplama döngüsü
            for (int i = 1; i <= cardCount; i++)
            {
                int testWidth = (availableWidth - (spaceBetweenCards * (i - 1))) / i;

                if (testWidth >= minCardWidth)
                {
                    cardsPerRow = i;
                    cardWidth = Math.Min(testWidth, maxCardWidth);
                }
                else
                {
                    break;
                }
            }

            // 3:2 kart boyutu oranı
            cardHeight = (cardWidth * 2) / 3;

            if (multiRow)
            {
                int rows = (int)Math.Ceiling((double)cardCount / cardsPerRow); // Her zaman üste yuvarlanması için cast kullanıldı
                flowPanel.Height = (cardHeight * rows) + (spaceBetweenCards * (rows - 1)) + (margin * 2);
            }
            else
                flowPanel.Height = cardHeight + (margin * 2);
            #endregion

            // Tüm kartları yeniden boyutlandırır ve görünürlüğü ayarlar
            int visibleCardIndex = 0;
            foreach (Control ctrl in flowPanel.Controls) // flowPaneldeki tüm controlleri dolaşıp ctrl değişkenine atar
            {
                if (ctrl is PanelControl panel) // ctrl değişkeni bir panel türünde mi? eğer öyleyse panel değişkenine atar
                {
                    panel.Size = new System.Drawing.Size(cardWidth, cardHeight);
                    panel.Margin = new Padding(spaceBetweenCards / 2);

                    // Çok satırlı ise tüm paneller görünür
                    if (multiRow) panel.Visible = true;
                    // Tek satırlı ise sadece sığan paneller görünür
                    else panel.Visible = visibleCardIndex < cardsPerRow;

                    visibleCardIndex++;

                    // Panel içindeki resimleri yeniden boyutlandır
                    foreach (Control picture in panel.Controls) // Panel içindeki her controlü alır
                    {
                        if (picture is PictureEdit pic && pic.Dock == DockStyle.Top) // Sadece PictureEditleri alır
                        {
                            pic.Height = cardHeight - 30; // Label için 30px yer ayırır
                        }
                    }
                }
            }

            flowPanel.ResumeLayout();
        }

        private void SetupSearchControl()
        {
            searchControlMainMenu.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    // Bu satır basılan tuşun işlendiğini söyler ve o tuşla başka bir işlem yapılmamasını sağlar
                    e.Handled = true;

                    // await bu kodu işlem bitene kadar durdurur fakat async method kullandığımız için form donmaz
                    // ve kullanıcı farklı işlemler yapabilir (async veya Task olmadan await kullanılmaz)
                    await SearchGames(searchControlMainMenu.Text);
                }
            };

            searchControlLibrary.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.Handled = true;
                    await SearchGames(searchControlLibrary.Text);
                }
            };
        }

        private async Task SearchGames(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            try
            {
                var games = await rawgapi.GetGamesBySearchAsync(searchText, 5); // API'dan oyunları çeker 5 adet.
                await DisplayGames(games, flowLayoutPanelSuggests);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadPopulerGames()
        {

            try
            {
                currentPopularPage = 1; // İlk sayfaya ayarlar
                var games = await rawgapi.GetPopularGamesAsync(20, currentPopularPage); // API'dan populer 20 oyun çek
                await DisplayGames(games, flowLayoutPanelPopulerGames, true);

            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DisplayGames(List<Game> games, FlowLayoutPanel panel, bool multiRow = false)
        {
            panel.Controls.Clear();

            // tüm kartları paralel olarak oluşturur
            var tasks = games.Select(g => CreateGameCard(g));
            var cards = await Task.WhenAll(tasks);
            panel.Controls.AddRange(cards);

            DynamicCards(panel, multiRow);
        }

        private static readonly Dictionary<string, System.Drawing.Image> imageCache = new Dictionary<string, System.Drawing.Image>();

        private async Task<PanelControl> CreateGameCard(Game game)
        {
            var panel = new PanelControl();
            panel.Size = new System.Drawing.Size(350, 230);
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = System.Drawing.Color.FromArgb(26, 29, 41);
            panel.Appearance.Options.UseBackColor = true;

            // Oyun Resmi
            var pictureEdit = new PictureEdit();
            pictureEdit.Dock = DockStyle.Top;
            pictureEdit.Size = new System.Drawing.Size(panel.Width, 200);
            pictureEdit.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;

            if (!string.IsNullOrEmpty(game.background_image))
            {
                try
                {
                    if (imageCache.ContainsKey(game.background_image))
                    {
                        pictureEdit.Image = imageCache[game.background_image];
                    }
                    else
                    {
                        // HttpClient internetten veri indirmek için kullanılan bir araç
                        // using iş bitince otomatik olarak kaynakları temizler
                        // memory leak önlemi için
                        var imageData = await httpClient.GetByteArrayAsync(game.background_image);
                        using (var ms = new System.IO.MemoryStream(imageData))
                        {
                            pictureEdit.Image = System.Drawing.Image.FromStream(ms);
                        }
                    }
                }

                catch { }
            }

            panel.Controls.Add(pictureEdit);

            // Oyun Adı
            var label = new DevExpress.XtraEditors.LabelControl();

            // Dock ve boyut
            label.Dock = DockStyle.Bottom;
            label.Size = new System.Drawing.Size(panel.Width, 30);
            label.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;

            // Yazı içeriği
            label.Text = game.name;
            label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            label.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;

            // Görünüm
            label.Appearance.Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F);
            label.Appearance.ForeColor = System.Drawing.Color.White;
            label.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            label.Padding = new Padding(0);
            label.Margin = new Padding(0);

            // Stil seçeneklerinin uygulanması
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;
            label.Appearance.Options.UseTextOptions = true;

            panel.Controls.Add(label);

            return panel;
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