using MailKit.Net.Smtp;
using MimeKit;

namespace PartSphere.Services
{
    /// <summary>
    /// Email service using SMTP (MailKit) for sending invoices and reminders.
    /// </summary>
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody);
        Task SendInvoiceEmailAsync(string to, string customerName, int invoiceId, decimal total);
        Task SendCreditReminderAsync(string to, string customerName, decimal amount, DateTime dueDate);
        Task SendLowStockAlertAsync(string to, string partName, string brand, int currentStock);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _config["Email:SenderName"] ?? "PartSphere",
                    _config["Email:SenderEmail"] ?? "noreply@partsphere.com"));
                message.To.Add(MailboxAddress.Parse(to));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = htmlBody };

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _config["Email:SmtpHost"] ?? "smtp.gmail.com",
                    int.Parse(_config["Email:SmtpPort"] ?? "587"),
                    MailKit.Security.SecureSocketOptions.StartTls);

                var username = _config["Email:Username"];
                var password = _config["Email:Password"];

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    await client.AuthenticateAsync(username, password);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {To} with subject: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                // Don't throw - email failure shouldn't crash the application
            }
        }

        public async Task SendInvoiceEmailAsync(string to, string customerName, int invoiceId, decimal total)
        {
            var subject = $"PartSphere - Invoice #{invoiceId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>PartSphere</h1>
                        <p style='color: rgba(255,255,255,0.8);'>Vehicle Parts & Services</p>
                    </div>
                    <div style='padding: 30px; background: #f8f9fa;'>
                        <h2 style='color: #333;'>Invoice #{invoiceId}</h2>
                        <p>Dear <strong>{customerName}</strong>,</p>
                        <p>Thank you for your purchase. Here is your invoice summary:</p>
                        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>Invoice Number</strong></td>
                                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>#{invoiceId}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>Total Amount</strong></td>
                                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right; font-size: 1.2em; color: #667eea;'>Rs. {total:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 10px;'><strong>Date</strong></td>
                                    <td style='padding: 10px; text-align: right;'>{DateTime.Now:yyyy-MM-dd}</td>
                                </tr>
                            </table>
                        </div>
                        <p style='color: #666;'>If you have questions about this invoice, please contact us.</p>
                    </div>
                    <div style='padding: 20px; text-align: center; color: #999; font-size: 12px;'>
                        <p>&copy; {DateTime.Now.Year} PartSphere. All rights reserved.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendCreditReminderAsync(string to, string customerName, decimal amount, DateTime dueDate)
        {
            var subject = "PartSphere - Credit Payment Reminder";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>Payment Reminder</h1>
                    </div>
                    <div style='padding: 30px; background: #f8f9fa;'>
                        <p>Dear <strong>{customerName}</strong>,</p>
                        <p>This is a friendly reminder that you have an outstanding credit payment:</p>
                        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <p><strong>Amount Due:</strong> Rs. {amount:N2}</p>
                            <p><strong>Due Date:</strong> {dueDate:yyyy-MM-dd}</p>
                            <p style='color: #f5576c;'><strong>Status: Overdue</strong></p>
                        </div>
                        <p>Please settle your payment at your earliest convenience.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendLowStockAlertAsync(string to, string partName, string brand, int currentStock)
        {
            var subject = "PartSphere - Low stock alert (Admin)";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #f5576c 0%, #f093fb 100%); padding: 24px; text-align: center;'>
                        <h1 style='color: white; margin: 0; font-size: 1.25rem;'>Low stock alert</h1>
                    </div>
                    <div style='padding: 24px; background: #f8f9fa;'>
                        <p>A vehicle part has dropped <strong>below 10 units</strong> in inventory.</p>
                        <div style='background: white; padding: 16px; border-radius: 8px; margin: 16px 0;'>
                            <p><strong>Part:</strong> {System.Net.WebUtility.HtmlEncode(partName)}</p>
                            <p><strong>Brand:</strong> {System.Net.WebUtility.HtmlEncode(brand)}</p>
                            <p style='color: #c0392b;'><strong>Current stock:</strong> {currentStock}</p>
                        </div>
                        <p style='color: #666;'>Please review purchasing or transfers in the admin dashboard.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }
    }
}
