using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ERPIndia.Utilities
{

    /// <summary>
    /// Complete Gmail SMTP utility with anti-spam features
    /// Prevents emails from going to spam folder
    /// </summary>
    public static class GmailEmailUtility
    {
        #region Configuration

        // Gmail SMTP Configuration
        private static readonly string SmtpServer = "smtp.gmail.com";
        private static readonly int SmtpPort = 587;

        // Email credentials from web.config
        private static string SenderEmail => ConfigurationManager.AppSettings["GmailSenderEmail"];
        private static string SenderPassword => ConfigurationManager.AppSettings["GmailSenderPassword"];
        private static string SenderDisplayName => ConfigurationManager.AppSettings["GmailSenderDisplayName"] ?? "Your Application";
        private static string CompanyAddress => ConfigurationManager.AppSettings["CompanyAddress"] ?? "123 Main St, City, State 12345";

        // Rate limiting variables
        private static DateTime lastSentTime = DateTime.MinValue;
        private static int emailsSentInCurrentMinute = 0;
        private static readonly object rateLimitLock = new object();

        #endregion

        #region Public Methods

        /// <summary>
        /// Sends professional email with anti-spam features
        /// </summary>
        /// <param name="toEmail">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body content</param>
        /// <param name="recipientName">Optional recipient name for personalization</param>
        /// <param name="isHtml">Whether body is HTML formatted</param>
        /// <param name="includeUnsubscribe">Whether to include unsubscribe link</param>
        /// <returns>True if email sent successfully</returns>
        public static bool SendProfessionalEmail(string toEmail, string subject, string body,
            string recipientName = "", bool isHtml = true, bool includeUnsubscribe = true)
        {
            try
            {
                // Validate credentials and inputs
                if (!ValidateCredentials() || !IsValidEmail(toEmail))
                {
                    LogError("Invalid credentials or email address");
                    return false;
                }

                // Apply rate limiting
                if (!CheckRateLimit())
                {
                    LogError("Rate limit exceeded");
                    return false;
                }

                // Clean and optimize content
                subject = CleanSubjectLine(subject);

                // Create professional email content
                string emailContent = isHtml ?
                    CreateProfessionalEmailTemplate(recipientName, body, includeUnsubscribe) :
                    CreateTextEmailTemplate(recipientName, body, includeUnsubscribe);

                using (var smtpClient = CreateSmtpClient())
                {
                    var mailMessage = CreateMailMessage(toEmail, subject, emailContent, isHtml);
                    smtpClient.Send(mailMessage);
                    LogSuccess($"Email sent successfully to {toEmail}");
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                HandleSmtpException(smtpEx);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends professional email asynchronously
        /// </summary>
        public static async Task<bool> SendProfessionalEmailAsync(string toEmail, string subject, string body,
            string recipientName = "", bool isHtml = true, bool includeUnsubscribe = true)
        {
            try
            {
                if (!ValidateCredentials() || !IsValidEmail(toEmail))
                {
                    LogError("Invalid credentials or email address");
                    return false;
                }

                if (!CheckRateLimit())
                {
                    LogError("Rate limit exceeded");
                    return false;
                }

                subject = CleanSubjectLine(subject);
                string emailContent = isHtml ?
                    CreateProfessionalEmailTemplate(recipientName, body, includeUnsubscribe) :
                    CreateTextEmailTemplate(recipientName, body, includeUnsubscribe);

                using (var smtpClient = CreateSmtpClient())
                {
                    var mailMessage = CreateMailMessage(toEmail, subject, emailContent, isHtml);
                    await smtpClient.SendMailAsync(mailMessage);
                    LogSuccess($"Email sent successfully to {toEmail}");
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                HandleSmtpException(smtpEx);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends email to multiple recipients with rate limiting
        /// </summary>
        public static Dictionary<string, bool> SendToMultipleRecipients(
            IEnumerable<EmailRecipient> recipients, string subject, string body,
            bool isHtml = true, bool includeUnsubscribe = true, int delayBetweenEmails = 6000)
        {
            var results = new Dictionary<string, bool>();
            var recipientList = recipients.ToList();

            LogInfo($"Starting bulk email send to {recipientList.Count} recipients");

            foreach (var recipient in recipientList)
            {
                if (string.IsNullOrWhiteSpace(recipient.Email) || !IsValidEmail(recipient.Email))
                {
                    results[recipient.Email] = false;
                    continue;
                }

                bool sent = SendProfessionalEmail(
                    recipient.Email,
                    subject,
                    body,
                    recipient.Name,
                    isHtml,
                    includeUnsubscribe
                );

                results[recipient.Email] = sent;

                // Add delay between emails to prevent spam detection
                if (delayBetweenEmails > 0)
                {
                    Thread.Sleep(delayBetweenEmails);
                }
            }

            var successCount = results.Values.Count(x => x);
            LogInfo($"Bulk email completed: {successCount}/{recipientList.Count} sent successfully");

            return results;
        }

        /// <summary>
        /// Sends email to multiple recipients asynchronously
        /// </summary>
        public static async Task<Dictionary<string, bool>> SendToMultipleRecipientsAsync(
            IEnumerable<EmailRecipient> recipients, string subject, string body,
            bool isHtml = true, bool includeUnsubscribe = true, int delayBetweenEmails = 6000)
        {
            var results = new Dictionary<string, bool>();
            var recipientList = recipients.ToList();

            LogInfo($"Starting async bulk email send to {recipientList.Count} recipients");

            foreach (var recipient in recipientList)
            {
                if (string.IsNullOrWhiteSpace(recipient.Email) || !IsValidEmail(recipient.Email))
                {
                    results[recipient.Email] = false;
                    continue;
                }

                bool sent = await SendProfessionalEmailAsync(
                    recipient.Email,
                    subject,
                    body,
                    recipient.Name,
                    isHtml,
                    includeUnsubscribe
                );

                results[recipient.Email] = sent;

                // Add delay between emails
                if (delayBetweenEmails > 0)
                {
                    await Task.Delay(delayBetweenEmails);
                }
            }

            var successCount = results.Values.Count(x => x);
            LogInfo($"Async bulk email completed: {successCount}/{recipientList.Count} sent successfully");

            return results;
        }

        /// <summary>
        /// Sends email to all users using a function to get user emails
        /// </summary>
        public static Dictionary<string, bool> SendToAllUsers(
            Func<IEnumerable<EmailRecipient>> getUsersFunction, string subject, string body,
            bool isHtml = true, bool includeUnsubscribe = true)
        {
            try
            {
                var users = getUsersFunction();
                return SendToMultipleRecipients(users, subject, body, isHtml, includeUnsubscribe);
            }
            catch (Exception ex)
            {
                LogError($"Failed to get user emails: {ex.Message}");
                return new Dictionary<string, bool>();
            }
        }

        /// <summary>
        /// Tests SMTP connection and email delivery
        /// </summary>
        public static bool TestEmailConfiguration()
        {
            try
            {
                if (!ValidateCredentials())
                {
                    LogError("Credential validation failed");
                    return false;
                }

                using (var smtpClient = CreateSmtpClient())
                {
                    var testMessage = new MailMessage(SenderEmail, SenderEmail)
                    {
                        Subject = "SMTP Configuration Test - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        Body = "This is a test email to verify SMTP configuration is working correctly.",
                        IsBodyHtml = false
                    };

                    smtpClient.Send(testMessage);
                    LogSuccess("SMTP test successful!");
                    return true;
                }
            }
            catch (SmtpException smtpEx)
            {
                HandleSmtpException(smtpEx);
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Connection test error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets configuration status for troubleshooting
        /// </summary>
        public static string GetConfigurationStatus()
        {
            var status = new StringBuilder();

            status.AppendLine("📧 Gmail SMTP Configuration Status:");
            status.AppendLine($"Sender Email: {(string.IsNullOrWhiteSpace(SenderEmail) ? "❌ NOT CONFIGURED" : SenderEmail)}");

            if (!string.IsNullOrWhiteSpace(SenderPassword))
            {
                string cleanPassword = SenderPassword.Replace(" ", "");
                status.AppendLine(cleanPassword.Length == 16 ?
                    "App Password: ✅ CONFIGURED (16 characters)" :
                    $"App Password: ⚠️ CONFIGURED but wrong length ({cleanPassword.Length} chars, should be 16)");
            }
            else
            {
                status.AppendLine("App Password: ❌ NOT CONFIGURED");
            }

            status.AppendLine($"Display Name: {SenderDisplayName}");
            status.AppendLine($"SMTP Server: {SmtpServer}:{SmtpPort}");
            status.AppendLine();

            if (!ValidateCredentials())
            {
                status.AppendLine("🚨 CONFIGURATION ERRORS DETECTED:");
                AppendConfigurationErrors(status);
            }
            else
            {
                status.AppendLine("✅ Configuration looks correct! Try the connection test.");
            }

            return status.ToString();
        }

        /// <summary>
        /// Validates Gmail credentials and configuration
        /// </summary>
        public static bool ValidateCredentials()
        {
            if (string.IsNullOrWhiteSpace(SenderEmail) || string.IsNullOrWhiteSpace(SenderPassword))
                return false;

            if (!SenderEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                return false;

            string cleanPassword = SenderPassword.Replace(" ", "");
            return cleanPassword.Length == 16;
        }

        #endregion

        #region Email Templates

        /// <summary>
        /// Creates professional HTML email template
        /// </summary>
        public static string CreateProfessionalEmailTemplate(string recipientName, string content, bool includeUnsubscribe = true)
        {
            var template = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email from {SenderDisplayName}</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden;"">
        <!-- Header -->
        <tr>
            <td style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;"">
                <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;"">{SenderDisplayName}</h1>
            </td>
        </tr>
        
        <!-- Content -->
        <tr>
            <td style=""padding: 40px 30px;"">
                <div style=""margin-bottom: 20px;"">
                    <p style=""margin: 0 0 15px 0; font-size: 16px; color: #555;"">Dear {(string.IsNullOrWhiteSpace(recipientName) ? "Valued Customer" : recipientName)},</p>
                </div>
                
                <div style=""margin-bottom: 30px; font-size: 15px; line-height: 1.8; color: #444;"">
                    {content}
                </div>
                
                <div style=""margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;"">
                    <p style=""margin: 0; font-size: 14px; color: #666;"">Best regards,<br>
                    <strong style=""color: #333;"">{SenderDisplayName} Team</strong></p>
                </div>
            </td>
        </tr>
        
        <!-- Footer -->
        <tr>
            <td style=""background-color: #f8f9fa; padding: 20px 30px; border-top: 1px solid #eee;"">
                <div style=""font-size: 12px; color: #888; text-align: center;"">
                    <p style=""margin: 0 0 10px 0;"">This email was sent by {SenderDisplayName}</p>
                    <p style=""margin: 0 0 10px 0;"">{CompanyAddress}</p>
                    <p style=""margin: 0 0 10px 0;"">📧 {SenderEmail}</p>
                    {(includeUnsubscribe ? CreateUnsubscribeFooter() : "")}
                </div>
            </td>
        </tr>
    </table>
    
    <!-- Tracking Pixel (Optional) -->
    <img src=""data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7"" width=""1"" height=""1"" alt="""" style=""display: block;"">
</body>
</html>";

            return template.Trim();
        }

        /// <summary>
        /// Creates professional text email template
        /// </summary>
        public static string CreateTextEmailTemplate(string recipientName, string content, bool includeUnsubscribe = true)
        {
            var template = new StringBuilder();

            template.AppendLine($"Dear {(string.IsNullOrWhiteSpace(recipientName) ? "Valued Customer" : recipientName)},");
            template.AppendLine();
            template.AppendLine(content);
            template.AppendLine();
            template.AppendLine("Best regards,");
            template.AppendLine($"{SenderDisplayName} Team");
            template.AppendLine();
            template.AppendLine("---");
            template.AppendLine($"This email was sent by {SenderDisplayName}");
            template.AppendLine($"{CompanyAddress}");
            template.AppendLine($"Email: {SenderEmail}");

            if (includeUnsubscribe)
            {
                template.AppendLine();
                template.AppendLine("To unsubscribe from future emails, please reply with 'UNSUBSCRIBE' in the subject line.");
            }

            return template.ToString();
        }

        /// <summary>
        /// Creates unsubscribe footer for HTML emails
        /// </summary>
        private static string CreateUnsubscribeFooter()
        {
            return @"
                <div style=""margin-top: 15px; padding-top: 15px; border-top: 1px solid #ddd;"">
                    <p style=""margin: 0; font-size: 11px;"">
                        <a href=""#unsubscribe"" style=""color: #888; text-decoration: underline;"">Unsubscribe from future emails</a> | 
                        <a href=""#preferences"" style=""color: #888; text-decoration: underline;"">Update email preferences</a>
                    </p>
                </div>";
        }

        #endregion

        #region Email Types

        /// <summary>
        /// Sends welcome email to new users
        /// </summary>
        public static bool SendWelcomeEmail(string userEmail, string userName = "")
        {
            string content = @"
                <h2 style=""color: #2c3e50; margin-bottom: 20px;"">Welcome to Our Platform! 🎉</h2>
                
                <p>We're thrilled to have you join our community of users who are already experiencing the benefits of our platform.</p>
                
                <div style=""background-color: #e8f4f8; border-left: 4px solid #3498db; padding: 15px; margin: 20px 0;"">
                    <h3 style=""color: #2c3e50; margin: 0 0 10px 0;"">Getting Started:</h3>
                    <ul style=""margin: 0; padding-left: 20px;"">
                        <li>Complete your profile setup</li>
                        <li>Explore our key features</li>
                        <li>Join our community forum</li>
                        <li>Check out our help documentation</li>
                    </ul>
                </div>
                
                <p>If you have any questions or need assistance, our support team is here to help. Simply reply to this email or contact us through our help center.</p>
                
                <p>Thank you for choosing us!</p>";

            return SendProfessionalEmail(
                userEmail,
                "Welcome to Our Platform - Let's Get Started!",
                content,
                userName,
                true,
                true
            );
        }

        /// <summary>
        /// Sends password reset email
        /// </summary>
        public static bool SendPasswordResetEmail(string userEmail, string resetToken, string userName = "")
        {
            string resetUrl = $"https://yourwebsite.com/reset-password?token={resetToken}";

            string content = $@"
                <div style=""background-color: #fff3cd; border: 1px solid #ffeaa7; color: #856404; padding: 15px; border-radius: 4px; margin-bottom: 20px;"">
                    <h3 style=""margin: 0 0 10px 0;"">🔒 Password Reset Request</h3>
                    <p style=""margin: 0;"">We received a request to reset your password.</p>
                </div>
                
                <p>If you requested this password reset, click the button below to create a new password:</p>
                
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{resetUrl}"" 
                       style=""background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold; font-size: 16px;"">
                        Reset Your Password
                    </a>
                </div>
                
                <div style=""background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin: 20px 0;"">
                    <h4 style=""color: #2c3e50; margin: 0 0 10px 0;"">Security Notice:</h4>
                    <ul style=""margin: 0; padding-left: 20px; color: #555;"">
                        <li>This link will expire in 24 hours</li>
                        <li>If you didn't request this reset, please ignore this email</li>
                        <li>Your password will remain unchanged unless you click the link above</li>
                    </ul>
                </div>
                
                <div style=""background-color: #e9ecef; padding: 15px; border-radius: 4px; margin-top: 20px;"">
                    <p style=""margin: 0; font-size: 14px; color: #666;"">
                        <strong>Can't click the button?</strong> Copy and paste this link into your browser:<br>
                        <span style=""word-break: break-all; color: #007bff;"">{resetUrl}</span>
                    </p>
                </div>";

            return SendProfessionalEmail(
                userEmail,
                "Password Reset Request - Action Required",
                content,
                userName,
                true,
                false // No unsubscribe for security emails
            );
        }
        public static bool SendPasswordEmail(string Email, string Subject, string Body)
        {
            return SendProfessionalEmail(
                Email,
                Subject,
                Body,
                "",
                true,
                true
            );
        }

        /// <summary>
        /// Sends order confirmation email
        /// </summary>
        public static bool SendOrderConfirmationEmail(string userEmail, string orderId, decimal orderTotal, string userName = "")
        {
            string content = $@"
                <div style=""background-color: #d4edda; border: 1px solid #c3e6cb; color: #155724; padding: 15px; border-radius: 4px; margin-bottom: 20px;"">
                    <h3 style=""margin: 0 0 10px 0;"">✅ Order Confirmed!</h3>
                    <p style=""margin: 0;"">Your order has been successfully placed and is being processed.</p>
                </div>
                
                <h3 style=""color: #2c3e50; border-bottom: 2px solid #eee; padding-bottom: 10px;"">Order Details</h3>
                <table style=""width: 100%; border-collapse: collapse; margin-bottom: 20px;"">
                    <tr style=""background-color: #f8f9fa;"">
                        <td style=""padding: 12px; border: 1px solid #dee2e6; font-weight: bold; width: 30%;"">Order ID</td>
                        <td style=""padding: 12px; border: 1px solid #dee2e6;"">{orderId}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 12px; border: 1px solid #dee2e6; font-weight: bold;"">Order Date</td>
                        <td style=""padding: 12px; border: 1px solid #dee2e6;"">{DateTime.Now:yyyy-MM-dd HH:mm}</td>
                    </tr>
                    <tr style=""background-color: #f8f9fa;"">
                        <td style=""padding: 12px; border: 1px solid #dee2e6; font-weight: bold;"">Total Amount</td>
                        <td style=""padding: 12px; border: 1px solid #dee2e6; font-weight: bold; color: #28a745;"">${orderTotal:F2}</td>
                    </tr>
                    <tr>
                        <td style=""padding: 12px; border: 1px solid #dee2e6; font-weight: bold;"">Status</td>
                        <td style=""padding: 12px; border: 1px solid #dee2e6;""><span style=""background-color: #ffc107; color: #212529; padding: 3px 8px; border-radius: 3px; font-size: 12px;"">Processing</span></td>
                    </tr>
                </table>
                
                <h3 style=""color: #2c3e50;"">What's Next?</h3>
                <ol style=""color: #555; line-height: 1.8;"">
                    <li>We'll process your order within 24 hours</li>
                    <li>You'll receive a shipping confirmation email with tracking information</li>
                    <li>Track your order status using the Order ID above</li>
                </ol>
                
                <div style=""background-color: #e8f4f8; border-left: 4px solid #3498db; padding: 15px; margin: 20px 0;"">
                    <p style=""margin: 0;""><strong>Questions about your order?</strong><br>
                    Contact our support team at support@yourcompany.com or call 1-800-123-4567</p>
                </div>";

            return SendProfessionalEmail(
                userEmail,
                $"Order Confirmation #{orderId} - Thank You for Your Purchase",
                content,
                userName,
                true,
                false // No unsubscribe for transactional emails
            );
        }

        #endregion

        #region Private Helper Methods

        private static SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(SmtpServer, SmtpPort)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                Timeout = 30000 // 30 seconds
            };
        }

        private static MailMessage CreateMailMessage(string toEmail, string subject, string body, bool isHtml)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(SenderEmail, SenderDisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            mailMessage.To.Add(toEmail);

            // Add anti-spam headers
            AddAntiSpamHeaders(mailMessage);

            return mailMessage;
        }

        private static void AddAntiSpamHeaders(MailMessage mailMessage)
        {
            mailMessage.Headers.Add("X-Mailer", $"{SenderDisplayName} Email System v2.0");
            mailMessage.Headers.Add("X-Priority", "3"); // Normal priority
            mailMessage.Headers.Add("X-MSMail-Priority", "Normal");
            mailMessage.Headers.Add("Importance", "Normal");
            mailMessage.Headers.Add("Message-ID", $"<{Guid.NewGuid()}@{SenderEmail.Split('@')[1]}>");
            mailMessage.Headers.Add("Date", DateTime.UtcNow.ToString("r"));
            mailMessage.Headers.Add("MIME-Version", "1.0");

            // Add List-Unsubscribe header for better compliance
            mailMessage.Headers.Add("List-Unsubscribe", $"<mailto:unsubscribe@{SenderEmail.Split('@')[1]}>");
            mailMessage.Headers.Add("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");

            // Reply-to should be same as sender for better reputation
            mailMessage.ReplyToList.Add(new MailAddress(SenderEmail, SenderDisplayName));
        }

        private static string CleanSubjectLine(string subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
                return $"Message from {SenderDisplayName}";

            // Remove excessive exclamation marks and caps
            subject = Regex.Replace(subject, @"!{2,}", "!");

            // Clean up spam trigger words
            var spamWords = new Dictionary<string, string>
            {
                { "FREE", "Free" },
                { "URGENT", "Important" },
                { "WINNER", "Selected" },
                { "CONGRATULATIONS", "Congratulations" },
                { "CLICK HERE", "Click here" },
                { "BUY NOW", "Purchase" }
            };

            foreach (var kvp in spamWords)
            {
                subject = subject.Replace(kvp.Key, kvp.Value);
            }

            return subject.Trim();
        }

        private static bool CheckRateLimit()
        {
            lock (rateLimitLock)
            {
                var now = DateTime.Now;

                // Reset counter if more than a minute has passed
                if ((now - lastSentTime).TotalMinutes >= 1)
                {
                    emailsSentInCurrentMinute = 0;
                    lastSentTime = now;
                }

                // Gmail limit: 10-15 emails per minute for new senders
                if (emailsSentInCurrentMinute >= 10)
                {
                    LogWarning("Rate limit reached, waiting 1 minute...");
                    Thread.Sleep(60000); // Wait 1 minute
                    emailsSentInCurrentMinute = 0;
                    lastSentTime = DateTime.Now;
                }

                emailsSentInCurrentMinute++;
                return true;
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static void HandleSmtpException(SmtpException smtpEx)
        {
            if (smtpEx.Message.Contains("5.7.0") || smtpEx.Message.Contains("Authentication Required"))
            {
                LogError("🚨 AUTHENTICATION ERROR: Use App Password, not regular Gmail password!");
                LogError("Steps to fix:");
                LogError("1. Enable 2-Factor Authentication on Gmail");
                LogError("2. Generate App Password: https://myaccount.google.com/apppasswords");
                LogError("3. Use 16-character App Password in web.config");
            }
            else if (smtpEx.Message.Contains("5.5.1") || smtpEx.Message.Contains("Username and Password not accepted"))
            {
                LogError("CREDENTIAL ERROR: Check email address and App Password");
            }
            else
            {
                LogError($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
            }
        }

        private static void AppendConfigurationErrors(StringBuilder status)
        {
            if (string.IsNullOrWhiteSpace(SenderEmail))
            {
                status.AppendLine("• Missing GmailSenderEmail in web.config");
            }
            else if (!SenderEmail.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                status.AppendLine("• Email must be a @gmail.com address");
            }

            if (string.IsNullOrWhiteSpace(SenderPassword))
            {
                status.AppendLine("• Missing GmailSenderPassword in web.config");
            }
            else
            {
                string cleanPassword = SenderPassword.Replace(" ", "");
                if (cleanPassword.Length != 16)
                {
                    status.AppendLine($"• App Password must be 16 characters (currently {cleanPassword.Length})");
                }
            }

            status.AppendLine();
            status.AppendLine("📋 REQUIRED STEPS:");
            status.AppendLine("1. Enable 2-Factor Authentication: https://myaccount.google.com/security");
            status.AppendLine("2. Generate App Password: https://myaccount.google.com/apppasswords");
            status.AppendLine("3. Update web.config with App Password");
        }

        #endregion

        #region Logging Methods

        private static void LogSuccess(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[EMAIL SUCCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        private static void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[EMAIL ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        private static void LogWarning(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[EMAIL WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        private static void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[EMAIL INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        #endregion
    }

    /// <summary>
    /// Email recipient class for structured email sending
    /// </summary>
    public class EmailRecipient
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public EmailRecipient(string email, string name = "")
        {
            Email = email;
            Name = name;
        }
    }
}
    