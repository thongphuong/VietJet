using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace TrainingCenter.Serveices
{
    public class EmailServives
    {
        public void SendSmtpMail(string toAddress, string subject, string body,string imgPath)
        {
            var fromAddress = ConfigurationManager.AppSettings["SendMailAddress"];
            var fromPassword = ConfigurationManager.AppSettings["SendMailPassword"];

            var eFromAddress = new MailAddress(fromAddress, "no-reply@vietjetair.com");
            var eToAddress = new MailAddress(toAddress, "");
            var inlineLogo = new Attachment(imgPath);
            var contentID = "Image";
            inlineLogo.ContentId = contentID;

            var smtp = new SmtpClient
            {
                Host =ConfigurationManager.AppSettings["SMTP_Host"],
                Port = int.Parse(ConfigurationManager.AppSettings["SMTP_Port"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(eFromAddress.Address, fromPassword)
            };
            var message = new MailMessage(eFromAddress, eToAddress);
            message.IsBodyHtml = true;
            message.Subject = subject;
            message.CC.Add("khoavd@tinhvan.com");
            message.Body = body;

            inlineLogo.ContentDisposition.Inline = true;
            inlineLogo.ContentDisposition.DispositionType = DispositionTypeNames.Inline;

            smtp.Send(message);
        }
    }
}