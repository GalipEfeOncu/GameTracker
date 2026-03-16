using System;
using Microsoft.AspNetCore.Mvc;
using GameTracker;
using GameTracker.Helpers;

namespace GameTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (req == null)
                return BadRequest(new { message = "Invalid request body." });

            try
            {
                if (string.IsNullOrWhiteSpace(req.Email))
                    return BadRequest(new { message = "Email is required." });
                if (UserManager.IsEmailExists(req.Email.Trim()))
                    return BadRequest(new { message = "This email is already registered." });

                if (string.IsNullOrWhiteSpace(req.Username))
                    return BadRequest(new { message = "Username is required." });
                if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
                    return BadRequest(new { message = "Password must be at least 8 characters." });

                bool success = UserManager.RegisterUser(req.Username.Trim(), req.Email.Trim(), req.Password);
                if (!success)
                    return BadRequest(new { message = "Could not register user." });

                var code = new Random().Next(100000, 999999).ToString();
                EmailVerificationStore.Set(req.Email.Trim(), code);
                bool sent = EmailService.SendVerificationCode(req.Email.Trim(), code);
                if (!sent)
                    return BadRequest(new { message = "Registration saved but we could not send the verification email. Try again later or contact support." });

                return Ok(new { message = "Registration successful. Check your email for the verification code.", requireVerification = true });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Kayıt sonrası e-posta doğrulama kodu ile hesabı aktifleştirir.
        /// </summary>
        [HttpPost("verify-email")]
        public IActionResult VerifyEmail([FromBody] VerifyEmailRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new { message = "Email is required." });
            if (string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new { message = "Verification code is required." });

            if (!EmailVerificationStore.TryValidate(req.Email.Trim(), req.Code.Trim(), out var normalizedEmail))
                return BadRequest(new { message = "Invalid or expired verification code." });

            if (!UserManager.SetEmailVerified(normalizedEmail))
                return BadRequest(new { message = "Could not verify email." });

            return Ok(new { message = "Email verified. You can now log in." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.EmailOrUsername) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Email/username and password are required." });

            var user = UserManager.LoginUser(req.EmailOrUsername.Trim(), req.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid email/username or password." });

            bool verified = user["email_verified"] != DBNull.Value && Convert.ToBoolean(user["email_verified"]);
            if (!verified)
                return BadRequest(new { message = "Please verify your email first. Check your inbox for the verification code.", code = "EmailNotVerified" });

            return Ok(new
            {
                UserId = Convert.ToInt32(user["user_id"]),
                Username = user["username"].ToString(),
                Email = user["email"].ToString()
            });
        }

        /// <summary>
        /// Kullanıcı adını günceller. İstek gövdesinde yeni kullanıcı adı gönderilir.
        /// </summary>
        [HttpPut("{userId}/profile/username")]
        public IActionResult UpdateUsername(int userId, [FromBody] UpdateUsernameRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.NewUsername))
                return BadRequest("New username is required.");

            var newUsername = req.NewUsername.Trim();
            if (newUsername.Length < 2)
                return BadRequest("Username must be at least 2 characters.");

            try
            {
                bool success = UserManager.UpdateUsername(userId, newUsername);
                if (success)
                    return Ok(new { message = "Username updated successfully." });
                return BadRequest("Could not update username.");
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Şifreyi günceller. Mevcut şifre doğrulanır; yeni şifre en az 8 karakter olmalıdır.
        /// </summary>
        [HttpPut("{userId}/profile/password")]
        public IActionResult UpdatePassword(int userId, [FromBody] UpdatePasswordRequest req)
        {
            if (req == null)
                return BadRequest("Invalid request body.");

            if (string.IsNullOrWhiteSpace(req.CurrentPassword))
                return BadRequest("Current password is required.");
            if (string.IsNullOrWhiteSpace(req.NewPassword))
                return BadRequest("New password is required.");
            if (req.NewPassword.Length < 8)
                return BadRequest("New password must be at least 8 characters.");
            if (req.NewPassword != req.NewPasswordAgain)
                return BadRequest("New password and confirmation do not match.");

            string email = UserManager.GetUserEmail(userId);
            if (string.IsNullOrEmpty(email))
                return NotFound("User not found.");

            if (!UserManager.VerifyPassword(userId, email, req.CurrentPassword))
                return BadRequest("Current password is incorrect.");

            bool success = UserManager.UpdatePassword(userId, email, req.NewPassword);
            if (success)
                return Ok(new { message = "Password updated successfully." });

            return BadRequest("Could not update password.");
        }

        /// <summary>
        /// Şifre sıfırlama kodu ister. E-posta kayıtlıysa kodu e-posta ile gönderir.
        /// </summary>
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("Email is required.");

            var email = req.Email.Trim();
            if (!UserManager.IsEmailExists(email))
                return Ok(new { message = "If this email is registered, you will receive a reset code." });

            var rnd = new Random();
            var code = rnd.Next(100000, 999999).ToString();
            PasswordResetStore.Set(email, code);

            bool sent = EmailService.SendPasswordResetCode(email, code);
            if (!sent)
                return BadRequest(new { message = "Could not send reset email. Try again later." });

            return Ok(new { message = "If this email is registered, you will receive a reset code." });
        }

        /// <summary>
        /// E-posta ve kod ile şifreyi sıfırlar.
        /// </summary>
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (req == null)
                return BadRequest("Invalid request body.");
            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("Email is required.");
            if (string.IsNullOrWhiteSpace(req.Code))
                return BadRequest("Code is required.");
            if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 8)
                return BadRequest("New password must be at least 8 characters.");
            if (req.NewPassword != req.NewPasswordAgain)
                return BadRequest("New password and confirmation do not match.");

            if (!PasswordResetStore.TryValidate(req.Email, req.Code, out var normalizedEmail))
                return BadRequest(new { message = "Invalid or expired code." });

            var userId = UserManager.GetUserIdByEmail(normalizedEmail);
            if (userId == null)
                return BadRequest("User not found.");

            var dbEmail = UserManager.GetUserEmail(userId.Value);
            if (string.IsNullOrEmpty(dbEmail))
                return BadRequest("User not found.");

            bool success = UserManager.UpdatePassword(userId.Value, dbEmail, req.NewPassword);
            if (success)
                return Ok(new { message = "Password has been reset. You can log in with your new password." });
            return BadRequest(new { message = "Could not reset password." });
        }

        /// <summary>
        /// Hesap silmek için e-posta ile onay kodu gönderir.
        /// </summary>
        [HttpPost("{userId}/request-delete-account")]
        public IActionResult RequestDeleteAccount(int userId)
        {
            string email = UserManager.GetUserEmail(userId);
            if (string.IsNullOrEmpty(email))
                return NotFound(new { message = "User not found." });

            var code = new Random().Next(100000, 999999).ToString();
            DeleteAccountStore.Set(userId, code);
            bool sent = EmailService.SendAccountDeletionCode(email, code);
            if (!sent)
                return BadRequest(new { message = "Could not send verification email. Try again later." });

            return Ok(new { message = "A verification code has been sent to your email." });
        }

        /// <summary>
        /// Gönderilen kod ile hesabı kalıcı olarak siler.
        /// </summary>
        [HttpPost("{userId}/confirm-delete-account")]
        public IActionResult ConfirmDeleteAccount(int userId, [FromBody] ConfirmDeleteAccountRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Code))
                return BadRequest(new { message = "Verification code is required." });

            if (!DeleteAccountStore.TryValidate(userId, req.Code.Trim()))
                return BadRequest(new { message = "Invalid or expired code." });

            if (!UserManager.DeleteUser(userId))
                return BadRequest(new { message = "Could not delete account." });

            return Ok(new { message = "Account has been deleted." });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginRequest
    {
        public string EmailOrUsername { get; set; }
        public string Password { get; set; }
    }

    public class UpdateUsernameRequest
    {
        public string NewUsername { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordAgain { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordAgain { get; set; }
    }

    public class VerifyEmailRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class ConfirmDeleteAccountRequest
    {
        public string Code { get; set; }
    }
}
