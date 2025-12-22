using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace LapTrinhWeb_Nhom_12.Models
{
    public class MailHelper
    {
        public void SendMail(string toEmailAddress, string subject, string content)
        {
            var fromEmailAddress = "phamanhnhatminh1809@gmail.com";
            var fromEmailPassword = "pqzl sevx yzlp fnyn";
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587;

            bool enabledSsl = true;

            string body = content;
            MailMessage message = new MailMessage(new MailAddress(fromEmailAddress, "MedForAll System"), new MailAddress(toEmailAddress));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;

            var client = new SmtpClient();
            client.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword);
            client.Host = smtpHost;
            client.EnableSsl = enabledSsl;
            client.Port = smtpPort;

            client.Send(message);
        }
    }
}