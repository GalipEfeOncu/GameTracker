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
        /// <param name="isLibraryContext">Kartın kütüphane sayfasında olup olmadığını belirtir. Menü içeriğini değiştirir.</param>
        /// <param name="onStatusChange">Oyun durumu değiştirildiğinde tetiklenecek eylem (Delegate).</param>
        /// <param name="onRemove">Oyun kaldırılmak istendiğinde tetiklenecek eylem (Delegate).</param>
        /// <returns>Yapılandırılmış GameCardControl nesnesi döndürür.</returns>
        public static GameCardControl CreateCard(
            Game game,
            LayoutMetrics metrics,
            ImageManager imageManager,
            bool isLibraryContext,
            Action<Game, string> onStatusChange,
            Action<Game> onRemove)
        {
            // 1. Kartı oluştur
            var card = new GameCardControl();

            // 2. Boyutları ayarla
            ConfigureCardDimensions(card, metrics);

            // 3. Veriyi karta bas
            card.SetData(game);

            // 4. Sağ tık menüsünü (Context Menu) oluştur ve bağla
            var contextMenu = CreateContextMenu(game, isLibraryContext, onStatusChange, onRemove);

            card.ContextMenuStrip = contextMenu;
            card.peGameImage.ContextMenuStrip = contextMenu;
            card.lblGameTitle.ContextMenuStrip = contextMenu;

            // 5. Resmi asenkron olarak yükle
            imageManager.LoadImageAsync(game.BackgroundImage, card.peGameImage, 420);

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
            card.Margin = new Padding(10, 10, 10, 10 + metrics.ExtraSpacingPerRow);

            // UserControl içindeki panel boyutlandırması
            card.borderPanel.Height = metrics.ImageHeight;
            card.borderPanel.Width = metrics.CardWidth;
        }

        /// <summary>
        /// Oyunun bulunduğu bağlama (Kütüphane veya Arama) göre uygun sağ tık menüsünü oluşturur.
        /// </summary>
        private static ContextMenuStrip CreateContextMenu(
            Game game,
            bool isLibraryContext,
            Action<Game, string> onStatusChange,
            Action<Game> onRemove)
        {
            var contextMenu = new ContextMenuStrip();

            if (isLibraryContext)
            {
                // --- KÜTÜPHANE MODU MENÜSÜ ---

                // Durum Değiştirme Alt Menüsü
                ToolStripMenuItem changeStatusItem = new ToolStripMenuItem("Move to...");
                AddStatusMenuItem(changeStatusItem, "Plan to Play", "PlanToPlay", game, onStatusChange);
                AddStatusMenuItem(changeStatusItem, "Playing", "Playing", game, onStatusChange);
                AddStatusMenuItem(changeStatusItem, "Played", "Played", game, onStatusChange);

                contextMenu.Items.Add(changeStatusItem);

                // Kaldırma Seçeneği
                ToolStripMenuItem removeItem = new ToolStripMenuItem("Remove from Library");
                removeItem.Click += (s, e) => onRemove?.Invoke(game);
                contextMenu.Items.Add(removeItem);
            }
            else
            {
                // --- KEŞİF/ARAMA MODU MENÜSÜ ---

                // Kütüphaneye Ekleme Alt Menüsü
                ToolStripMenuItem addToLibItem = new ToolStripMenuItem("Add to Library");
                AddStatusMenuItem(addToLibItem, "Plan to Play", "PlanToPlay", game, onStatusChange);
                AddStatusMenuItem(addToLibItem, "Playing", "Playing", game, onStatusChange);
                AddStatusMenuItem(addToLibItem, "Played", "Played", game, onStatusChange);

                contextMenu.Items.Add(addToLibItem);
            }

            return contextMenu;
        }

        /// <summary>
        /// Menüye durum değiştirme seçeneklerini ekleyen yardımcı metot.
        /// </summary>
        private static void AddStatusMenuItem(
            ToolStripMenuItem parentItem,
            string title,
            string statusCode,
            Game game,
            Action<Game, string> action)
        {
            var item = new ToolStripMenuItem(title);
            item.Click += (s, e) => action?.Invoke(game, statusCode);
            parentItem.DropDownItems.Add(item);
        }
    }
}