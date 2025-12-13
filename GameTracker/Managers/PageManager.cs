using GameTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameTracker.Managers
{
    /// <summary>
    /// Liste tabanlı verilerin sayfalama işlemlerini yöneten sınıf.
    /// Sayfa numarası takibi, toplam sayfa hesaplama ve veri dilimleme (pagination) işlemlerini yapar.
    /// </summary>
    public class PageManager
    {
        #region Properties

        public List<Game> AllItems { get; private set; } = new List<Game>();    // Sayfalanacak tüm verilerin listesi.
        public int CurrentPage { get; private set; } = 1;                       // Şu anki aktif sayfa numarası (1'den başlar).
        public int ItemsPerPage { get; set; } = 24;                             // Sayfa başına gösterilecek öğe sayısı.
        public int TotalPages => (int)Math.Ceiling((double)AllItems.Count / ItemsPerPage); // Toplam sayfa sayısı.

        #endregion

        #region Initialization

        /// <summary>
        /// Sayfalama yöneticisini başlatır.
        /// </summary>
        /// <param name="itemsPerPage">Sayfa başına düşecek kart sayısı.</param>
        public PageManager(int itemsPerPage)
        {
            ItemsPerPage = itemsPerPage;
        }

        /// <summary>
        /// Yönetilecek veri listesini günceller.
        /// Yeni veri seti atandığında sayfa başa (1) döner.
        /// </summary>
        /// <param name="items">Yeni veri listesi.</param>
        public void SetDataSource(List<Game> items)
        {
            AllItems = items ?? new List<Game>();
            CurrentPage = 1;
        }

        /// <summary>
        /// Mevcut listeye yeni veriler ekler (Append).
        /// Genellikle "Daha Fazla Yükle" senaryolarında kullanılır.
        /// </summary>
        /// <param name="newItems">Eklenecek yeni veriler.</param>
        public void AddItems(List<Game> newItems)
        {
            if (newItems != null && newItems.Any())
            {
                AllItems.AddRange(newItems);
            }
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Bir sonraki sayfaya geçmeye çalışır.
        /// </summary>
        /// <returns>Geçiş başarılıysa true, son sayfadaysa false döner.</returns>
        public bool NextPage()
        {
            if (CurrentPage < TotalPages - 1)
            {
                CurrentPage++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Bir önceki sayfaya geçmeye çalışır.
        /// </summary>
        /// <returns>Geçiş başarılıysa true, ilk sayfadaysa false döner.</returns>
        public bool PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Mevcut sayfa numarasına denk gelen verileri dilimleyip döndürür.
        /// </summary>
        /// <returns>O anki sayfada gösterilecek oyunların listesi.</returns>
        public List<Game> GetCurrentPageItems()
        {
            if (AllItems.Count == 0) return new List<Game>();

            // Sayfa sınırlarını kontrol et (Safe check)
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;
            if (CurrentPage < 1) CurrentPage = 1;

            return AllItems
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();
        }

        /// <summary>
        /// Sayfa bilgisini string formatında döndürür (Örn: "Page 1 / 5").
        /// </summary>
        public string GetPageInfoString()
        {
            int total = Math.Max(TotalPages, 1); // Hiç veri yoksa bile 1 gösterelim.
            return $"Page {CurrentPage} / {total}";
        }

        /// <summary>
        /// Kullanıcının son sayfada olup olmadığını kontrol eder.
        /// </summary>
        public bool IsLastPage => CurrentPage >= TotalPages;

        #endregion
    }
}