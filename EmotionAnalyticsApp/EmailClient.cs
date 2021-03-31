using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EmotionAnalyticsApp
{
    public class EMailClient
    {
        public static bool SendMail(string toEmailId, string subject, string body, string attachment)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.EnableSsl = true;
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("ultimaticindia@gmail.com", "ultimatic@2016");

            MailMessage msg = new MailMessage();
            msg.To.Add(toEmailId);
            //msg.From = new MailAddress("RtMinds-Face Analyst<rtminds.india@gmail.com>");
            msg.From = new MailAddress("RtMinds-Face Analyst<ultimaticindia@gmail.com>");
            msg.Subject = subject;
            msg.Body = body;
            msg.Attachments.Add(new Attachment(attachment));
            client.Send(msg);

            return true;
        }

        public static async Task SendMailAsync(string toEmailId, string subject, string body, string attachment)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.EnableSsl = true;
            client.Timeout = 100000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("ultimaticindia@gmail.com", "ultimatic@2016");

            MailMessage msg = new MailMessage();
            msg.To.Add(toEmailId);
            //msg.From = new MailAddress("RtMinds-Face Analyst<rtminds.india@gmail.com>");
            msg.From = new MailAddress("RtMinds-Face Analyst<ultimaticindia@gmail.com>");
            msg.Subject = subject;
            msg.Body = body;
            msg.Attachments.Add(new Attachment(attachment));
            await client.SendMailAsync(msg);
        }
    }
}
