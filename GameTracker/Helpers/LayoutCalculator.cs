using System;
using System.Drawing;

namespace GameTracker
{
    /// <summary>
    /// LayoutCalculator tarafından hesaplanan ve UI'ın çiziminde kullanılan
    /// değerleri tutan yapı (struct).
    /// </summary>
    public struct LayoutMetrics
    {
        public int CardsPerPage { get; set; }
        public int CardWidth { get; set; }
        public int ImageHeight { get; set; }
        public int ExtraSpacingPerRow { get; set; }
    }

    /// <summary>
    /// Kartların ve layout'un boyutlarını ekran genişliğine göre hesaplayan
    /// yardımcı (helper) sınıf.
    /// </summary>
    public class LayoutCalculator
    {
        // Kart boyut limitleri
        private const int minCardWidth = 280;
        private const int maxCardWidth = 350;
        private const int cardMargin = 30;
        private const int labelHeight = 30;

        /// <summary>
        /// Verilen alan boyutuna göre layout metriklerini hesaplar.
        /// </summary>
        /// <param name="availableSize">Kullanılabilir alanın boyutu (ClientSize).</param>
        /// <returns>Hesaplanmış layout metrikleri.</returns>
        public LayoutMetrics Calculate(Size availableSize)
        {
            int availableWidth = availableSize.Width - 10; // -10 right padding için
            int availableHeight = availableSize.Height;

            // Max kart sayısını hesaplar
            int maxCardsPerRow = Math.Max(1, availableWidth / (minCardWidth + cardMargin));

            // Dinamik kart genişliğini hesaplar
            int dynamicCardWidth = (availableWidth - maxCardsPerRow * cardMargin) / maxCardsPerRow;
            dynamicCardWidth = Math.Max(minCardWidth, Math.Min(maxCardWidth, dynamicCardWidth));

            int cardTotalWidth = dynamicCardWidth + cardMargin;

            int imageHeight = (int)(dynamicCardWidth * 2.0 / 3.0);
            int cardHeight = imageHeight + labelHeight;
            int cardTotalHeight = cardHeight + cardMargin;

            int cardsPerRow = Math.Max(1, availableWidth / cardTotalWidth);
            int rowsPerPage = Math.Max(1, availableHeight / cardTotalHeight);

            // Satır arası boşluğu dinamik hesaplar
            int usedSpace = rowsPerPage * cardHeight + rowsPerPage * cardMargin;
            int remainingSpace = Math.Max(0, availableHeight - usedSpace);
            int extraSpacingPerRow = remainingSpace / (rowsPerPage);

            return new LayoutMetrics
            {
                CardsPerPage = cardsPerRow * rowsPerPage,
                CardWidth = dynamicCardWidth,
                ImageHeight = imageHeight,
                ExtraSpacingPerRow = extraSpacingPerRow
            };
        }
    }
}