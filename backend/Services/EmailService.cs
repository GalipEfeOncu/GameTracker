using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace GameTracker.Helpers
{
    public static class EmailService
    {
        private static bool TryCreateClient(out SmtpClient client)
        {
            client = null;
            var addr = GameTracker.Api.AppConfig.MailAddress?.Trim();
            var pwd = GameTracker.Api.AppConfig.MailPassword;
            if (string.IsNullOrWhiteSpace(addr) || string.IsNullOrWhiteSpace(pwd))
            {
                Console.WriteLine("[EmailService] EmailSettings MailAddress/MailPassword tanımlı değil (ör. Render env: EmailSettings__MailAddress, EmailSettings__MailPassword).");
                return false;
            }

            client = new SmtpClient(GameTracker.Api.AppConfig.SmtpHost)
            {
                Port = GameTracker.Api.AppConfig.SmtpPort,
                Credentials = new NetworkCredential(addr, pwd),
                EnableSsl = true,
            };
            return true;
        }

        private static void LogMailError(Exception ex)
        {
            Console.WriteLine($"[EmailService] {ex.GetType().Name}: {ex.Message}");
        }

        public static bool SendVerificationCode(string receiverEmail, string code)
        {
            try
            {
                if (!TryCreateClient(out var smtpClient))
                    return false;

                using (smtpClient)
                {
                var senderEmail = GameTracker.Api.AppConfig.MailAddress.Trim();

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "GameTracker Security"),
                    Subject = "GameTracker Verification Code",
                    Body = $@"
                    <!DOCTYPE html>
                    <html>
                        <head>
                            <meta charset='UTF-8'>
                        </head>
                        <body style='margin:0; padding:0; background-color:#f4f8ff; font-family:Segoe UI, Arial, sans-serif;'>
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td align='center' style='padding:40px 0;'>
                                        <table width='420' cellpadding='0' cellspacing='0' style='background:#ffffff; border-radius:12px; box-shadow:0 10px 30px rgba(0,0,0,0.08);'>
                    
                                            <!-- Header -->
                                            <tr>
                                                <td style='background:#2563eb; color:#ffffff; padding:24px; border-radius:12px 12px 0 0; text-align:center;'>
                                                    <h1 style='margin:0; font-size:22px;'>GameTracker</h1>
                                                    <p style='margin:6px 0 0; font-size:14px; opacity:0.9;'>Security Verification</p>
                                                </td>
                                            </tr>

                                            <!-- Content -->
                                            <tr>
                                                <td style='padding:28px; color:#1f2937;'>
                                                    <h2 style='margin-top:0; font-size:18px;'>Welcome 👋</h2>
                                                    <p style='font-size:14px; line-height:1.6;'>
                                                        To complete your registration, please use the verification code below:
                                                    </p>

                                                    <div style='margin:24px 0; text-align:center;'>
                                                        <span style='display:inline-block; padding:14px 28px; font-size:24px; letter-spacing:4px;
                                                            background:#eff6ff; color:#2563eb; border-radius:10px; font-weight:700;'>
                                                            {code}
                                                        </span>
                                                    </div>

                                                    <p style='font-size:13px; color:#6b7280; line-height:1.6;'>
                                                        This code will expire in a few minutes.  
                                                        If you didn’t request this, you can safely ignore this email.
                                                    </p>
                                                </td>
                                            </tr>

                                            <!-- Footer -->
                                            <tr>
                                                <td style='padding:16px; text-align:center; font-size:12px; color:#9ca3af;'>
                                                    © {DateTime.Now.Year} GameTracker · All rights reserved
                                                </td>
                                            </tr>

                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </body>
                    </html>
                    ",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(receiverEmail);

                smtpClient.Send(mailMessage);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMailError(ex);
                return false;
            }
        }

        public static bool SendPasswordResetCode(string receiverEmail, string code)
        {
            try
            {
                if (!TryCreateClient(out var smtpClient))
                    return false;

                using (smtpClient)
                {
                var senderEmail = GameTracker.Api.AppConfig.MailAddress.Trim();

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "GameTracker"),
                    Subject = "GameTracker - Şifre Sıfırlama Kodu",
                    Body = $@"
                    <!DOCTYPE html>
                    <html>
                        <head><meta charset='UTF-8'></head>
                        <body style='margin:0; padding:0; background:#f4f8ff; font-family:Segoe UI, Arial, sans-serif;'>
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td align='center' style='padding:40px 0;'>
                                        <table width='420' cellpadding='0' cellspacing='0' style='background:#fff; border-radius:12px; box-shadow:0 10px 30px rgba(0,0,0,0.08);'>
                                            <tr>
                                                <td style='background:#2563eb; color:#fff; padding:24px; border-radius:12px 12px 0 0; text-align:center;'>
                                                    <h1 style='margin:0; font-size:22px;'>GameTracker</h1>
                                                    <p style='margin:6px 0 0; font-size:14px; opacity:0.9;'>Şifre Sıfırlama</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding:28px; color:#1f2937;'>
                                                    <p style='font-size:14px; line-height:1.6;'>Şifre sıfırlama talebiniz için doğrulama kodunuz:</p>
                                                    <div style='margin:24px 0; text-align:center;'>
                                                        <span style='display:inline-block; padding:14px 28px; font-size:24px; letter-spacing:4px; background:#eff6ff; color:#2563eb; border-radius:10px; font-weight:700;'>{code}</span>
                                                    </div>
                                                    <p style='font-size:13px; color:#6b7280;'>Bu kod 15 dakika içinde geçerlidir. Talebi siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding:16px; text-align:center; font-size:12px; color:#9ca3af;'>© {DateTime.Now.Year} GameTracker</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </body>
                    </html>
                    ",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(receiverEmail);
                smtpClient.Send(mailMessage);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMailError(ex);
                return false;
            }
        }

        public static bool SendAccountDeletionCode(string receiverEmail, string code)
        {
            try
            {
                if (!TryCreateClient(out var smtpClient))
                    return false;

                using (smtpClient)
                {
                var senderEmail = GameTracker.Api.AppConfig.MailAddress.Trim();

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "GameTracker"),
                    Subject = "GameTracker - Hesap Silme Onay Kodu",
                    Body = $@"
                    <!DOCTYPE html>
                    <html>
                        <head><meta charset='UTF-8'></head>
                        <body style='margin:0; padding:0; background:#f4f8ff; font-family:Segoe UI, Arial, sans-serif;'>
                            <table width='100%' cellpadding='0' cellspacing='0'>
                                <tr>
                                    <td align='center' style='padding:40px 0;'>
                                        <table width='420' cellpadding='0' cellspacing='0' style='background:#fff; border-radius:12px; box-shadow:0 10px 30px rgba(0,0,0,0.08);'>
                                            <tr>
                                                <td style='background:#dc2626; color:#fff; padding:24px; border-radius:12px 12px 0 0; text-align:center;'>
                                                    <h1 style='margin:0; font-size:22px;'>GameTracker</h1>
                                                    <p style='margin:6px 0 0; font-size:14px; opacity:0.9;'>Hesap Silme Onayı</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding:28px; color:#1f2937;'>
                                                    <p style='font-size:14px; line-height:1.6;'>Hesabınızı silmek için doğrulama kodunuz:</p>
                                                    <div style='margin:24px 0; text-align:center;'>
                                                        <span style='display:inline-block; padding:14px 28px; font-size:24px; letter-spacing:4px; background:#fef2f2; color:#dc2626; border-radius:10px; font-weight:700;'>{code}</span>
                                                    </div>
                                                    <p style='font-size:13px; color:#6b7280;'>Bu kod 15 dakika geçerlidir. Bu talebi siz yapmadıysanız bu e-postayı yok sayın ve şifrenizi değiştirin.</p>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style='padding:16px; text-align:center; font-size:12px; color:#9ca3af;'>© {DateTime.Now.Year} GameTracker</td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </body>
                    </html>
                    ",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(receiverEmail);
                smtpClient.Send(mailMessage);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogMailError(ex);
                return false;
            }
        }
    }
}