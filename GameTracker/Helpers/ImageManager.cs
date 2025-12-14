using DevExpress.XtraEditors;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Net.Http;

namespace GameTracker
{
    /// <summary>
    /// Resim indirme, cache'leme ve boyutlandırma işlemlerini yöneten
    /// helper class.
    /// </summary>
    public class ImageManager : IDisposable
    {
        private static readonly HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 10
        })
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly ConcurrentDictionary<string, Image> imageCache = new ConcurrentDictionary<string, Image>();

        /// <summary>
        /// Bir URL'den asenkron olarak resim yükler, cache'ler ve PictureEdit'e atar.
        /// </summary>
        /// <param name="imageUrl">Orijinal resim URL'si.</param>
        /// <param name="pictureEdit">Resmin yükleneceği PictureEdit kontrolü.</param>
        /// <param name="targetWidth">API'den istenecek resmin yaklaşık genişliği.</param>
        public async void LoadImageAsync(string imageUrl, PictureEdit pictureEdit, int targetWidth)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            // Boyutu düşürmek için URL Resize yapar
            var resizedUrl = GetResizedImageUrl(imageUrl, targetWidth);

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
                    if (pictureEdit.InvokeRequired)
                    {
                        pictureEdit.Invoke(new Action(() => pictureEdit.Image = FixTo3x2(img)));
                    }
                    else
                    {
                        pictureEdit.Image = FixTo3x2(img);
                    }
                }
            }
            catch (Exception ex)
            { Console.WriteLine($"Resim yüklenemedi: {imageUrl} -> {ex.Message}"); }
        }

        /// <summary>
        /// RAWG API'sinin resize özelliğini kullanmak için URL'i modifiye eder.
        /// </summary>
        private string GetResizedImageUrl(string originalUrl, int width)
        {
            if (string.IsNullOrEmpty(originalUrl)) return originalUrl;

            string resizedUrl = originalUrl
                .Replace("media/games/", $"media/resize/{width}/-/games/")
                .Replace("media/screenshots/", $"media/resize/{width}/-/screenshots/");

            return resizedUrl;
        }

        /// <summary>
        /// Gelen resmi 3:2 (genişlik:yükseklik) oranına ortalayarak kırpar.
        /// </summary>
        private Image FixTo3x2(Image original)
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

        /// <summary>
        /// Cache'te tutulan tüm resimleri Dispose ederek hafızayı boşaltır.
        /// </summary>
        public void Dispose()
        {
            foreach (var img in imageCache.Values)
            {
                img?.Dispose();
            }
            imageCache.Clear();
            GC.Collect();
        }

        /// <summary>
        /// Cache'te biriken resimleri siler ve RAM'i boşaltır.
        /// </summary>
        public void ClearMemoryCache()
        {
            // 1. Mevcut resimleri bellekten uçur
            foreach (var img in imageCache.Values)
            {
                img?.Dispose(); // Resim kaynağını serbest bırak
            }

            // 2. Listeyi sıfırla
            imageCache.Clear();

            // 3. Çöp toplayıcıyı (Garbage Collector) göreve çağır (Opsiyonel ama etkili)
            // Bu komut RAM'de boşta kalan alanları anında temizler.
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}