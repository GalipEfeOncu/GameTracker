using DevExpress.XtraEditors;
using GameTracker.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameTracker
{
    public partial class GameCardControl : XtraUserControl
    {
        public Game GameData { get; private set; }

        // --- Animasyon Değişkenleri ---
        private Panel maskPanel;
        private Timer timer;

        // Zoom Hedefleri
        private float currentZoom = 1.0f; // Anlık büyüklük
        private float targetZoom = 1.0f;  // Hedeflenen büyüklük
        private const float desiredZoom = 1.15f; // İstenen büyüklük
        private const float baseZoom = 1.0f;  // Normal büyüklük
        private const float LerpSpeed = 0.2f; // Geçiş hızı

        public GameCardControl()
        {
            InitializeComponent();
            SetupLayoutStructure();
            SetupAnimationTimer();
            SetupEvents();

            peGameImage.Dock = DockStyle.Fill;
        }

        private void SetupLayoutStructure()
        {
            // Yeni bir "Maske Panel" oluşturuyoruz.
            maskPanel = new Panel();
            maskPanel.Name = "maskPanel";
            maskPanel.Dock = DockStyle.Fill; // BorderPanel'in içini dolduracak
            maskPanel.BackColor = Color.Transparent; // Arkası gözüksün

            // Taşan kısımları gizlemesi için margin/padding ayarları
            maskPanel.Margin = new Padding(0);
            maskPanel.Padding = new Padding(0);

            // Resim kontrolünü MaskPanel'e veriyoruz
            // Bu işlem runtime'da çalışır
            this.borderPanel.Controls.Remove(peGameImage);
            maskPanel.Controls.Add(peGameImage);

            // MaskPanel'i BorderPanel'e ekliyoruz.
            this.borderPanel.Controls.Add(maskPanel);

            // Hiyerarşi: borderPanel -> maskPanel -> peGameImage
        }

        private void SetupAnimationTimer()
        {
            timer = new Timer();
            timer.Interval = 16; // ~60 FPS
            timer.Tick += Timer_Tick;
        }

        private void SetupEvents()
        {
            Control[] controls = { peGameImage, maskPanel, borderPanel, lblGameTitle };
            foreach (var ctrl in controls)
            {
                ctrl.MouseEnter += (s, e) => SetHoverState(true);
                ctrl.MouseLeave += (s, e) => SetHoverState(false);
            }
        }

        private void SetHoverState(bool isHovering)
        {
            // Titreme önleyici
            if (!isHovering)
            {
                Point cursorPoint = this.PointToClient(Cursor.Position);
                if (this.ClientRectangle.Contains(cursorPoint)) return;
            }

            if (isHovering)
            {
                // Border anında gelsin, animasyona gerek yok.
                borderPanel.BackColor = Color.White;
                borderPanel.Padding = new Padding(3);
                targetZoom = desiredZoom;
                peGameImage.Dock = DockStyle.None;
                ApplyZoom();
            }
            else
            {
                borderPanel.BackColor = Color.FromArgb(26, 29, 41);
                borderPanel.Padding = new Padding(0);
                targetZoom = baseZoom;
                ApplyZoom();
            }

            // Animasyon motorunu çalıştır
            if (!timer.Enabled) timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Mevcut değeri hedefe yaklaştır
            currentZoom += (targetZoom - currentZoom) * LerpSpeed;

            // Lerp işlemi asla tam sayıya ulaşamaz bu nedenle çok yaklaştıysa hedefe kendimiz ulaştırıyoruz
            if (Math.Abs(targetZoom - currentZoom) < 0.01f)
            {
                currentZoom = targetZoom;
                timer.Stop(); // İşlem bitti, motoru durdur

                if (currentZoom <= baseZoom)
                {
                    peGameImage.Dock = DockStyle.Fill;
                    return;
                }
            }

            ApplyZoom();
        }

        private void ApplyZoom()
        {
            if (peGameImage.Dock == DockStyle.Fill) return;

            // Yeni boyutları hesapla
            int baseW = maskPanel.Width;
            int baseH = maskPanel.Height;

            if (baseW == 0 || baseH == 0) return; // 0'a bölme hatası olmasını önlemek için

            int newW = (int)(baseW * currentZoom);
            int newH = (int)(baseH * currentZoom);

            peGameImage.Size = new Size(newW, newH);

            // Merkeze hizala
            peGameImage.Location = new Point((baseW - newW) / 2, (baseH - newH) / 2);
        }

        // Dışarıdan veriyi bu metotla alacağız
        public void SetData(Game game)
        {
            this.GameData = game;
            lblGameTitle.Text = game.Name ?? "Unknown";
        }
    }
}