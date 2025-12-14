using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace GameTracker.Helpers
{
    public static class EmailService
    {
        private static readonly string senderEmail = ConfigurationManager.AppSettings["MailAddress"];
        private static readonly string senderPassword = ConfigurationManager.AppSettings["MailPassword"];

        public static bool SendVerificationCode(string receiverEmail, string code)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "GameTracker Security"),
                    Subject = "GameTracker Verification Code",
                    Body = $@"
                        <h2>Welcome to GameTracker!</h2>
                        <p>Your verification code is: <b style='font-size:20px; color:blue;'>{code}</b></p>
                        <p>Please enter this code in the application to complete your registration.</p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(receiverEmail);

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Mail Hatası: " + ex.Message);
                return false;
            }
        }
    }
}