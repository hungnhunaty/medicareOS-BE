using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BE.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var portStr = _configuration["EmailSettings:Port"] ?? "587";
            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            var senderName = _configuration["EmailSettings:SenderName"] ?? "Medicare System";
            var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";

            int port = int.TryParse(portStr, out int p) ? p : 587;

            // Kiểm tra xem cấu hình có đang ở dạng mặc định (placeholder) hay không
            bool isPlaceholder = string.IsNullOrEmpty(senderEmail) || 
                                 string.IsNullOrEmpty(senderPassword) || 
                                 senderEmail.Contains("YOUR_GMAIL") || 
                                 senderPassword.Contains("YOUR_GMAIL");

            if (isPlaceholder)
            {
                Console.WriteLine("\n========================================================");
                Console.WriteLine("[DEBUG EMAIL SERVICE] PHÁT HIỆN CẤU HÌNH GMAIL MẶC ĐỊNH.");
                Console.WriteLine($"Gửi tới: {toEmail}");
                Console.WriteLine($"Tiêu đề: {subject}");
                Console.WriteLine($"Nội dung:\n{body}");
                Console.WriteLine("========================================================\n");
                return;
            }

            try
            {
                using (var smtpClient = new SmtpClient(smtpServer))
                {
                    smtpClient.Port = port;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 3000; // Giới hạn thời gian kết nối tối đa 3 giây

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(senderEmail, senderName);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.To.Add(toEmail);

                        smtpClient.Send(mailMessage);
                    }
                }
                Console.WriteLine($"[EmailService] Đã gửi email thành công tới {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n========================================================");
                Console.WriteLine($"[LỖI GỬI EMAIL THỰC TẾ]: {ex.Message}");
                Console.WriteLine("FALLBACK: HIỂN THỊ NỘI DUNG EMAIL Ở CONSOLE DƯỚI ĐÂY:");
                Console.WriteLine($"Gửi tới: {toEmail}");
                Console.WriteLine($"Tiêu đề: {subject}");
                Console.WriteLine($"Nội dung:\n{body}");
                Console.WriteLine("========================================================\n");
            }
        }
    }
}
