using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameTracker
{
    public partial class MainForm : DevExpress.XtraEditors.XtraForm
    {
        private RawgApiService rawgapi;

        public MainForm()
        {
            InitializeComponent();
            rawgapi = new RawgApiService();
            SetupSearchControl();
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

            this.Load += async (s, e) =>
            {
                await LoadPopulerGames();
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
                var games = await rawgapi.GetGamesBySearchAsync(searchText, 3); // API'dan oyunları çeker 20 adet.
                DisplayGames(games, flowLayoutPanel3);
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
                var games = await rawgapi.GetPopularGamesAsync(3); // API'dan populer 20 oyun çek
                DisplayGames(games, flowLayoutPanel2);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayGames(List<Game> games, FlowLayoutPanel panel)
        {
            panel.Controls.Clear();

            foreach (var game in games)
            {
                var gameCard = CreateGameCard(game);
                panel.Controls.Add(gameCard);
            }
        }

        private PanelControl CreateGameCard(Game game)
        {
            var panel = new PanelControl();
            panel.Size = new System.Drawing.Size(200, 330);
            panel.BorderStyle = BorderStyles.NoBorder;
            panel.Appearance.BackColor = System.Drawing.Color.FromArgb(26, 29, 41);
            panel.Appearance.Options.UseBackColor = true;

            // Oyun Resmi
            var pictureEdit = new PictureEdit();
            pictureEdit.Dock = DockStyle.Top;
            pictureEdit.Size = new System.Drawing.Size(200, 300);

            if (!string.IsNullOrEmpty(game.background_image))
            {
                try
                {
                    // HttpClient internetten veri indirmek için kullanılan bir araç
                    // using iş bitince otomatik olarak kaynakları temizler
                    // memory leak önlemi için
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        // URL deki resmi indirir // .Result async işin bitmesini bekler
                        var imageData = client.GetByteArrayAsync(game.background_image).Result;
                        // İndirilen resmi belleğe koyar
                        var ms = new System.IO.MemoryStream(imageData);
                        // Bellekteki byte verisini resme çevirip pictureEdit controlüne atar
                        pictureEdit.Image = System.Drawing.Image.FromStream(ms);
                    }
                }
                catch { }
            }

            panel.Controls.Add(pictureEdit);

            // Oyun Adı
            var label = new DevExpress.XtraEditors.LabelControl();
            label.Dock = DockStyle.Bottom;
            label.Size = new System.Drawing.Size(200, 30);
            label.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            label.Text = game.name;
            label.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F);
            label.Appearance.ForeColor = System.Drawing.Color.White;
            label.Appearance.Options.UseFont = true;
            label.Appearance.Options.UseForeColor = true;

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