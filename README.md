# 🎮 GameTracker

> **Oyun dünyasını avucunun içine al!**
> "Ne oynasam?" derdine son. Oyun kütüphaneni yönet, yapay zekadan tavsiye al ve arşivini **şekil** bir arayüzle takip et.

![GameTracker Dashboard](https://github.com/user-attachments/assets/19471807-5380-4fd6-b81a-91de8cb21183)

## 🚀 Proje Hakkında

**GameTracker**, oyun tutkunları için geliştirilmiş modern bir **Windows Forms (WinForms)** uygulamasıdır. Klasik, sıkıcı form tasarımlarını unutun. Bu proje, **DevExpress** gücüyle oluşturulmuş **Cyberpunk/Dark** temalı arayüzü ve **akıcı (responsive)** kullanıcı deneyimi ile öne çıkar.

**GameTracker**, sıradan ve sıkıcı Windows formlarından sıkılanlar için geliştirilmiş, **DevExpress** destekli modern bir oyun kütüphanesi yöneticisidir. Klasik WinForms projelerinin aksine Cyberpunk/Dark teması, akıcı animasyonları ve responsive (duyarlı) yapısıyla premium bir his verir.

Arka planda **AWG.io API** ile devasa bir veri havuzuna erişirken, **Google Gemini AI** entegrasyonu sayesinde oyun zevkinize göre nokta atışı öneriler sunar.

## 🔥 Öne Çıkan Özellikler

### 🤖 Yapay Zeka Destekli Öneriler
* **Gemini AI Entegrasyonu:** Kütüphanene eklediğin oyunları analiz eder ve sana özel, oynamaktan keyif alacağın 20 yeni oyun önerir.
* **Akıllı Analiz:** Sadece rastgele oyunlar değil, senin tarzına uygun "hidden gem"leri bulur.
  
### 🔒 Üst Düzey Güvenlik & Doğrulama
* **E-Posta Doğrulaması:** Kayıt olurken SMTP üzerinden doğrulama kodu (OTP) gönderilir. Fake hesaplara yer yok!
* **SHA-256 Şifreleme:** Parolalarınız veritabanında asla açık metin olarak saklanmaz.
* **Beni Hatırla (Secure):** Windows DPAPI ile şifrelenmiş güvenli oturum açma özelliği.

### 📚 Gelişmiş Kütüphane Yönetimi
* **Detaylı Statü Takibi:** Oyunlarını **Plan to Play**, **Playing**, **Played** ve **Dropped** olarak kategorize et.
* **Sağ Tık Menüsü:** Kütüphanedeki oyunlara sağ tıklayarak hızlıca durum güncelle veya kaldır.
* **NSFW Filtresi:** Ayarlar menüsünden +18 içerikleri tek tıkla gizle veya göster. (Varsayılan: Kapalı)

### 🎨 Modern UI & UX Performansı
* **Responsive Kartlar:** Pencere boyutuna göre otomatik yeniden hesaplanan (LayoutCalculator) dinamik ızgara yapısı.
* **Smart Caching:** İndirilen görseller RAM'de önbelleklenir, internetin yavaşlasa bile arayüz **yağ gibi akar**.
* **Live Search:** Aradığın oyunu yazarken anlık olarak API'den sonuçları getirir.

## 🛠️ Teknolojiler

Bu proje aşağıdaki teknolojilerle **inşa edildi**:

* **Dil:** C# (.NET Framework 4.8)
* **Arayüz:** Windows Forms & **DevExpress v24.1**
* **Veritabanı:** Microsoft SQL Server (ADO.NET & Parameterized Queries)
* **Oyun Verisi API:** [RAWG Video Games Database](https://rawg.io/apidocs)
* **Yapay Zeka API:** [Google Gemini 2.5 Flash](https://ai.google.dev/)
* **Diğer:** Newtonsoft.Json, System.Net.Mail (SMTP)

## 📸 Ekran Görüntüleri

| Giriş Ekranı | Kayıt Ekranı |
| :---: | :---: |
| <img width="400" height="540" alt="image" src="https://github.com/user-attachments/assets/076deeba-b4d7-4c77-abaf-628c102014d3" /> | <img width="387" height="540" alt="image" src="https://github.com/user-attachments/assets/d07e1925-a9ae-4fe7-976c-6f0032648c73" /> |

| Kütüphane | AI Öneri Sistemi |
| :---: | :---: |
| <img width="1000" height="525" alt="image" src="https://github.com/user-attachments/assets/84f92c88-92f8-449f-9201-c3bc6b2be749" /> | <img width="1000" height="525" alt="image" src="https://github.com/user-attachments/assets/0b30c609-e406-4a14-8c24-010946b8a462" /> |

| Oyun Sayfası | Oyun Arama |
| :---: | :---: |
| <img width="1000" height="525" alt="image" src="https://github.com/user-attachments/assets/8cd544a7-4d93-4911-aa64-37ed8d57dfdb" /> | <img width="1000" height="525" alt="image" src="https://github.com/user-attachments/assets/957c5f1c-0e1d-40d6-8903-4c2e4df78f30" /> |

| Settings |
| :---: |
| <img width="1919" height="1011" alt="image" src="https://github.com/user-attachments/assets/71d29560-656a-4f05-a8ac-adcc32905d49" /> |

## 📥 İndir

Kurulumla, ayarlarla uğraşmana gerek yok. Setup dosyasını indir, kur ve hemen kullanmaya başla!

[🔗 GameTracker İndir](https://github.com/GalipEfeOncu/GameTracker/releases/tag/v1.0)

## 🤝 Katkıda Bulunma

Hata mı buldun? Veya "Şunu eklesek proje uçar" mı diyorsun?
Çekinme, hemen bir **Issue** aç veya **Pull Request** gönder. Kodumuz her zaman geliştirmeye açık!

## 👨‍💻 Geliştirici

**Galip Efe Öncü**

---
*Bu proje eğitim ve hobi amaçlı geliştirilmiştir.*
