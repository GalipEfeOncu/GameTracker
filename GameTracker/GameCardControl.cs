using DevExpress.XtraEditors;
using GameTracker.Models;
using System.Drawing;
using System.Windows.Forms;

namespace GameTracker
{
    public partial class GameCardControl : XtraUserControl
    {
        public Game GameData { get; private set; }

        public GameCardControl()
        {
            InitializeComponent();

            // Label'a tıklayınca da context menu açılsın diye event zinciri
            lblGameTitle.MouseEnter += (s, e) => this.OnMouseEnter(e);
        }

        // Dışarıdan veriyi bu metotla alacağız
        public void SetData(Game game)
        {
            this.GameData = game;
            lblGameTitle.Text = game.Name ?? "Unknown";
        }

        // Hover Efektleri
        private void peGameImage_MouseEnter(object sender, System.EventArgs e)
        {
            borderPanel.Padding = new Padding(3);
            borderPanel.BackColor = Color.White;

            int addedWidth = (int)(borderPanel.Width * 0.1);
            int addedHeight = (int)(borderPanel.Height * 0.1);

            peGameImage.Location = new Point(-addedWidth / 2, -addedHeight / 2);
            peGameImage.Width += addedWidth;
            peGameImage.Height += addedHeight;
            peGameImage.BringToFront();
        }

        private void peGameImage_MouseLeave(object sender, System.EventArgs e)
        {
            borderPanel.Padding = new Padding(0);
            borderPanel.BackColor = Color.FromArgb(26, 29, 41);

            // Orijinal boyutuna döndür (Dock fill olduğu için parent boyutu neyse o olur, location 0,0 olur)
            peGameImage.Location = new Point(0, 0);
            peGameImage.Dock = DockStyle.Fill;
        }
    }
}