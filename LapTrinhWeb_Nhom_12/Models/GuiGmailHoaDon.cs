using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class GuiGmailHoaDon
    {
        private readonly string _fromEmail = "phamanhnhatminh1809@gmail.com";
        private readonly string _password = "pqzl sevx yzlp fnyn"; // App Password

        public async Task SendInvoiceEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, "Nhà Thuốc MedForAll"); // Tên hiển thị
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true; // Quan trọng: Cho phép nội dung là HTML

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(_fromEmail, _password);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi mail: " + ex.Message);
            }
        }
    }
}