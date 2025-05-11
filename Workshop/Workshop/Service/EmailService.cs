using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Workshop.Service
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            // Diagnostic output for debugging
            Console.WriteLine("Preparing to send email...");
            Console.WriteLine($"SMTP Server: {_configuration["EmailSettings:SmtpServer"]}");
            Console.WriteLine($"Port: {_configuration["EmailSettings:Port"]}");
            Console.WriteLine($"Username: {_configuration["EmailSettings:Username"]}");
            Console.WriteLine($"From: {_configuration["EmailSettings:From"]}");
            Console.WriteLine($"To: {recipientEmail}");
            Console.WriteLine($"Subject: {subject}");

            var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]
                ),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network, // Add this line
                UseDefaultCredentials = false // Explicitly disable default credentials
            };

            // Add this right before sending
            Console.WriteLine($"Attempting to send email to {recipientEmail} using {_configuration["EmailSettings:Username"]}");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(recipientEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email:");
                Console.WriteLine(ex.ToString()); // Print full exception details
                throw;
            }
        }

        public async Task SendOrderConfirmationEmail(string recipientEmail, string orderDetails)
        {
            var subject = "Order Confirmation - Kitab Sansar";
            var body = $"Thank you for your order!<br><br>{orderDetails}<br><br>We appreciate your business.";
            await SendEmailAsync(recipientEmail, subject, body);
        }
    }
}
