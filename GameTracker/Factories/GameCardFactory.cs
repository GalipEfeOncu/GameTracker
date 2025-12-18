using DevExpress.XtraEditors;
using GameTracker.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameTracker.Factories
{
    /// <summary>
    /// Oyun kartlarının oluşturulması ve yapılandırılmasından sorumlu fabrika sınıfı.
    /// UI bileşenlerinin boyutlandırılması, veri bağlama ve bağlam menüsü (Context Menu) işlemlerini yönetir.
    /// </summary>
    public static class GameCardFactory
    {
        /// <summary>
        /// Verilen oyun verisi ve layout ayarlarına göre yeni bir oyun kartı oluşturur.
        /// </summary>
        /// <param name="game">Kart üzerinde gösterilecek oyun nesnesi.</param>
        /// <param name="metrics">Kartın boyut ve yerleşim hesaplamaları.</param>
        /// <param name="imageManager">Resim yükleme işlemlerini yönetecek servis.</param>
        /// <param name="onCardClick">Kart tıklandığında (Detay açmak için) tetiklenecek eylem.</param>
        /// <returns>Yapılandırılmış GameCardControl nesnesi döndürür.</returns>
        public static GameCardControl CreateCard(
            Game game,
            LayoutMetrics metrics,
            ImageManager imageManager,
            Action<Game> onCardClick)
        {

            var card = new GameCardControl();   // Kartı oluştur
            ConfigureCardDimensions(card, metrics); // Boyutları ayarla
            card.SetData(game);  // Veriyi karta bas

            // Kullanıcı resme, yazıya veya kartın kendisine tıklasa da detay açılsın.
            card.Click += (s, e) => onCardClick?.Invoke(game);
            card.peGameImage.Click += (s, e) => onCardClick?.Invoke(game);
            card.lblGameTitle.Click += (s, e) => onCardClick?.Invoke(game);

            // Resmi asenkron olarak yükle
            _ = imageManager.LoadImageAsync(game.BackgroundImage, card.peGameImage, 420);

            return card;
        }

        /// <summary>
        /// Kartın boyutlarını ve kenar boşluklarını hesaplanan metriklere göre ayarlar.
        /// </summary>
        private static void ConfigureCardDimensions(GameCardControl card, LayoutMetrics metrics)
        {
            // Sabit etiket yüksekliği (MainForm'daki const değer)
            const int LABEL_HEIGHT = 30;

            card.Width = metrics.CardWidth;
            card.Height = metrics.ImageHeight + LABEL_HEIGHT;
            card.Margin = new Padding(20, 20, 10, 10 + metrics.ExtraSpacingPerRow);

            // UserControl içindeki panel boyutlandırması
            card.borderPanel.Height = metrics.ImageHeight;
            card.borderPanel.Width = metrics.CardWidth;
        }
    }
}