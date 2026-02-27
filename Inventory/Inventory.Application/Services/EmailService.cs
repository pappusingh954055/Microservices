using System.Net;
using System.Net.Mail;
using Inventory.Application.Clients.DTOs;

namespace Inventory.Application.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendPoEmailAsync(CompanyProfileDto company, string supplierEmail, string poNumber, decimal amount)
        {
            if (string.IsNullOrEmpty(company.SmtpHost) || string.IsNullOrEmpty(company.SmtpEmail) || string.IsNullOrEmpty(company.SmtpPassword))
            {
                Console.WriteLine("[EmailService] SMTP settings missing. Skipping email.");
                return;
            }

            if (string.IsNullOrEmpty(supplierEmail))
            {
                Console.WriteLine("[EmailService] Supplier email missing. Skipping email.");
                return;
            }

            try
            {
                var fromAddress = new MailAddress(company.SmtpEmail, company.Name);
                var toAddress = new MailAddress(supplierEmail);
                string subject = $"Purchase Order: {poNumber} from {company.Name}";
                string body = $@"
                    <html>
                    <body>
                        <h2>Dear Supplier,</h2>
                        <p>We are pleased to place a New Purchase Order with you.</p>
                        <p><strong>PO Number:</strong> {poNumber}</p>
                        <p><strong>Total Amount:</strong> {amount}</p>
                        <p>Please find the details in the attached document (coming soon) or log in to our portal.</p>
                        <br/>
                        <p>Regards,</p>
                        <p><strong>{company.Name}</strong></p>
                    </body>
                    </html>";

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = company.SmtpHost;
                    smtp.Port = company.SmtpPort ?? 587;
                    smtp.EnableSsl = company.SmtpUseSsl;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromAddress.Address, company.SmtpPassword);

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtp.SendMailAsync(message);
                    }
                }
                Console.WriteLine($"[EmailService] Email sent successfully to {supplierEmail} for PO {poNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Failed to send email: {ex.Message}");
            }
        }

        public async Task SendSoEmailAsync(CompanyProfileDto company, string customerEmail, string soNumber, decimal amount)
        {
            if (string.IsNullOrEmpty(company.SmtpHost) || string.IsNullOrEmpty(company.SmtpEmail) || string.IsNullOrEmpty(company.SmtpPassword))
            {
                Console.WriteLine("[EmailService] SMTP settings missing. Skipping SO email.");
                return;
            }

            if (string.IsNullOrEmpty(customerEmail))
            {
                Console.WriteLine("[EmailService] Customer email missing. Skipping SO email.");
                return;
            }

            try
            {
                var fromAddress = new MailAddress(company.SmtpEmail, company.Name);
                var toAddress = new MailAddress(customerEmail);
                string subject = $"Sale Order Confirmation: {soNumber} - {company.Name}";
                string body = $@"
                    <html>
                    <body>
                        <h2>Dear Customer,</h2>
                        <p>Thank you for your order!</p>
                        <p><strong>Order Number:</strong> {soNumber}</p>
                        <p><strong>Total Amount:</strong> {amount}</p>
                        <p>We are processing your order and will notify you once it's shipped.</p>
                        <br/>
                        <p>Regards,</p>
                        <p><strong>{company.Name}</strong></p>
                    </body>
                    </html>";

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = company.SmtpHost;
                    smtp.Port = company.SmtpPort ?? 587;
                    smtp.EnableSsl = company.SmtpUseSsl;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromAddress.Address, company.SmtpPassword);

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtp.SendMailAsync(message);
                    }
                }
                Console.WriteLine($"[EmailService] SO Email sent successfully to {customerEmail} for SO {soNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Failed to send SO email: {ex.Message}");
            }
        }
        
        public async Task SendGrnEmailAsync(CompanyProfileDto company, string supplierEmail, string grnNumber, string poNumber, decimal amount)
        {
            if (string.IsNullOrEmpty(company.SmtpHost) || string.IsNullOrEmpty(company.SmtpEmail) || string.IsNullOrEmpty(company.SmtpPassword))
            {
                Console.WriteLine("[EmailService] SMTP settings missing. Skipping GRN email.");
                return;
            }

            if (string.IsNullOrEmpty(supplierEmail))
            {
                Console.WriteLine("[EmailService] Supplier email missing. Skipping GRN email.");
                return;
            }

            try
            {
                var fromAddress = new MailAddress(company.SmtpEmail, company.Name);
                var toAddress = new MailAddress(supplierEmail);
                string subject = $"Goods Received Advice: {grnNumber} (Ref PO: {poNumber}) - {company.Name}";
                string body = $@"
                    <html>
                    <body>
                        <h2>Dear Supplier,</h2>
                        <p>This is to inform you that we have received the goods against your supply.</p>
                        <p><strong>GRN Number:</strong> {grnNumber}</p>
                        <p><strong>PO Reference:</strong> {poNumber}</p>
                        <p><strong>Accepted Amount:</strong> {amount}</p>
                        <p>The inventory has been updated in our system. Thank you for the timely delivery.</p>
                        <br/>
                        <p>Regards,</p>
                        <p><strong>{company.Name}</strong></p>
                    </body>
                    </html>";

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = company.SmtpHost;
                    smtp.Port = company.SmtpPort ?? 587;
                    smtp.EnableSsl = company.SmtpUseSsl;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromAddress.Address, company.SmtpPassword);

                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        await smtp.SendMailAsync(message);
                    }
                }
                Console.WriteLine($"[EmailService] GRN Email sent successfully to {supplierEmail} for GRN {grnNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] Failed to send GRN email: {ex.Message}");
            }
        }
    }
}
