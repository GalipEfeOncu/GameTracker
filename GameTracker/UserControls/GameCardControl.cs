using DevExpress.XtraEditors;
using GameTracker.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameTracker
{
    /// <summary>
    /// Normal Panel’in flicker (titreme) yapmasını engelleyen özel panel.
    /// Animasyonlu uygulamalarda gereklidir.
    /// </summary>
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true
            );

            UpdateStyles();
        }
    }

    /// <summary>
    /// Oyun kartı bileşeni.
    /// Hover edildiğinde border rengi ve resim zoom animasyonu yapar.
    /// </summary>
    public partial class GameCardControl : XtraUserControl
    {
        #region Fields
        public Game GameData { get; private set; }

        // Animasyon elemanları
        private Panel maskPanel;
        private Timer timer;

        // Zoom değişkenleri
        private float currentZoom = 1.0f;
        private float targetZoom = 1.0f;

        private const float DESIRED_ZOOM = 1.15f; // Hover yakınlaştırma
        private const float BASE_ZOOM = 1.0f;     // Normal boyut
        private const float LERP_SPEED = 0.2f;    // Yaklaşma hızı (0.0 - 1.0)

        #endregion

        #region Constructor
        public GameCardControl()
        {
            InitializeComponent();

            // DesignMode = true iken runtime mantığı çalışmaz
            if (!DesignMode)
            {
                SetupLayoutStructure();
                SetupAnimations();
                SetupEvents();
            }

            peGameImage.Dock = DockStyle.Fill;

            // Kontrolün kendi flicker koruması
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true
            );

            UpdateStyles();
        }

        #endregion

        #region Setup Methods
        /// <summary>
        /// maskPanel oluşturulur ve peGameImage içine alınır.
        /// Bu panel, taşan zoom bölgelerini kırparak güzel bir hover animasyonu sağlar.
        /// </summary>
        private void SetupLayoutStructure()
        {
            maskPanel = new DoubleBufferedPanel
            {
                Name = "maskPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // Resmi maskPanel içine al
            borderPanel.Controls.Remove(peGameImage);
            maskPanel.Controls.Add(peGameImage);

            // DevExpress resmi daha stabil davransın diye ayarlar
            peGameImage.Properties.AllowFocused = false;
            peGameImage.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            peGameImage.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Stretch;
            peGameImage.Margin = new Padding(0);
            peGameImage.Padding = new Padding(0);

            // maskPanel'i borderPanel'e ekle
            borderPanel.Controls.Add(maskPanel);
        }

        /// <summary>
        /// 60 FPS hızında çalışan animasyon motoru.
        /// Hover geldiğinde resim boyutunu yumuşakça büyütür/küçültür.
        /// </summary>
        private void SetupAnimations()
        {
            timer = new Timer
            {
                Interval = 16 // yaklaşık 60 FPS
            };

            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Hover eventlerini tüm alt kontrollerden UserControl'e yönlendiren sistem.
        /// Bu sayede resme veya label'a girince hover bozulmaz.
        /// </summary>
        private void SetupEvents()
        {
            Control[] controls = { peGameImage, maskPanel, borderPanel, lblGameTitle };

            foreach (var ctrl in controls)
            {
                ctrl.MouseEnter += (s, e) => SetHoverState(true);
                ctrl.MouseLeave += (s, e) => SetHoverState(false);
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // Başka sayfaya gidildiyse hover durumunu zorla kapat  
            if (!this.Visible)
                SetHoverState(false);
            else
            {
                // Geri dönüldüğünde fare pozisyonunu kontrol et
                if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
                    SetHoverState(false);
            }
        }

        #endregion

        #region Context Menu Override

        /// <summary>
        /// Context menu atandığında event'leri dinlemeye başla.
        /// Menu kapanınca fare pozisyonunu kontrol et.
        /// </summary>
        protected override void OnContextMenuStripChanged(EventArgs e)
        {
            base.OnContextMenuStripChanged(e);

            if (ContextMenuStrip != null)
            {
                ContextMenuStrip.Closed += ContextMenuStrip_Closed;
            }
        }

        /// <summary>
        /// Context menu kapandıktan sonra fare pozisyonunu kontrol eder.
        /// Fare kart dışındaysa hover'ı kapat, içindeyse devam ettir.
        /// </summary>
        private void ContextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            // Kısa bir gecikmeyle fare pozisyonunu kontrol et
            var checkTimer = new Timer { Interval = 50 };
            checkTimer.Tick += (s, args) =>
            {
                checkTimer.Stop();
                checkTimer.Dispose();

                // Fare kontrolün dışındaysa hover'ı kapat
                if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
                {
                    SetHoverState(false);
                }
                // Fare içerideyse hover zaten aktif, hiçbir şey yapma
            };
            checkTimer.Start();
        }

        #endregion

        #region Hover Logic
        /// <summary>
        /// Hover başladığında veya bittiğinde hedef zoom değerini ayarlar.
        /// Border panel görünümünü de burada kontrol ederiz.
        /// </summary>
        private void SetHoverState(bool isHovering)
        {
            // Fare gerçekten kontrolün dışına çıkmadan "leave" tetiklendiyse, ignorela
            if (!isHovering)
            {
                if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
                    return;
            }

            if (isHovering)
            {
                borderPanel.BackColor = Color.White;
                borderPanel.Padding = new Padding(3);

                targetZoom = DESIRED_ZOOM;

                // Zoom sırasında dock kullanamayız.
                peGameImage.Dock = DockStyle.None;
                ApplyZoom();
            }
            else
            {
                borderPanel.BackColor = Color.FromArgb(26, 29, 41);
                borderPanel.Padding = new Padding(0);

                targetZoom = BASE_ZOOM;
                ApplyZoom();
            }

            if (!timer.Enabled)
                timer.Start();
        }
        #endregion

        #region Animation
        /// <summary>
        /// Her tick'te zoom değerini hedefe doğru yumuşakça yaklaştırır.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            currentZoom += (targetZoom - currentZoom) * LERP_SPEED;

            // Lerp mantığında hedefe asla tam ulaşamayız bu yüzden hedefe çok yaklaştıysa direkt bitir
            if (Math.Abs(targetZoom - currentZoom) < 0.01f)
            {
                currentZoom = targetZoom;
                timer.Stop();

                // Orijinal boyuta döndüyse dockı geri aç
                if (currentZoom == BASE_ZOOM)
                {
                    peGameImage.Dock = DockStyle.Fill;
                    return;
                }
            }

            ApplyZoom();
        }

        /// <summary>
        /// Zoom'lu boyutu hesaplar ve resmi maskPanel'in ortasına yerleştirir.
        /// </summary>
        private void ApplyZoom()
        {
            // Fill modunda zoom yapılamaz
            if (peGameImage.Dock == DockStyle.Fill)
                return;

            int baseW = maskPanel.Width;
            int baseH = maskPanel.Height;

            if (baseW == 0 || baseH == 0)
                return;

            int newW = (int)(baseW * currentZoom);
            int newH = (int)(baseH * currentZoom);

            peGameImage.Size = new Size(newW, newH);
            peGameImage.Location = new Point(
                (baseW - newW) / 2,
                (baseH - newH) / 2
            );
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Dışarıdan oyun verisi atamak için kullanılır.
        /// </summary>
        public void SetData(Game game)
        {
            GameData = game;
            lblGameTitle.Text = game.Name ?? "Unknown";
        }
        #endregion
    }
}
