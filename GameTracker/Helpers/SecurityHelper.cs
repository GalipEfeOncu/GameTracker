using System;
using System.Security.Cryptography;
using System.Text;

namespace GameTracker
{
    public static class SecurityHelper
    {
        // Şifrelemek için (Kaydederken kullanacağız)
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return null;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText); // Texti byte dizisine çevir

                // DataProtectionScope.CurrentUser: Sadece bu Windows oturumu açan kişi çözebilir.
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes); // Byte dizisini tekrar yazıya çevir
            }
            catch
            {
                return null; // Bir hata olursa boş dön
            }
        }

        // Şifreyi Çözmek için (Form açılırken kullanacağız)
        // Buradaki anahtar şifrede değil Windows kullanıcısında saklanır. Bu yüzden başka bir PC'de çözülemez.
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return null;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText); // Yazıyı şifreli byte dizisine çevir
                // Windows, kullanıcı anahtarı ile şifreyi çözer
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes); // Byte dizisini tekrar yazıya çevir
            }
            catch
            {
                return null; // Şifre çözülemezse (başka PC vs.) boş dön
            }
        }
    }
}