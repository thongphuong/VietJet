using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Utilities
{
    using TrainingCenter.Utilities;

    public static class MailUtil
    {
        public static Boolean SendMail(String MailTo, String Subject, String Content)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(MailTo);
                mail.From = new MailAddress(AppUtils.getAppSetting("SendMailAddress"), "Training Center");
                mail.Subject = Subject;
                mail.Body = Content;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = AppUtils.getAppSetting("SMTP_Host");
                smtp.Port = int.Parse(AppUtils.getAppSetting("SMTP_Port"));
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                (AppUtils.getAppSetting("SendMailAddress"), AppUtils.getAppSetting("SendMailPassword"));// Enter senders User name and password
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                var pathForSaving = HostingEnvironment.MapPath("/Log.txt");
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(ex.ToString());
                System.IO.File.WriteAllText(pathForSaving, sb.ToString());
                return false;
            }
        }
        public static Boolean SendMail(String MailTo, String Subject, String Content,byte[] AttachFile)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(MailTo);
                mail.From = new MailAddress(AppUtils.getAppSetting("SendMailAddress"), "Training Center");
                mail.Subject = Subject;
                mail.Body = Content;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = AppUtils.getAppSetting("SMTP_Host");
                smtp.Port = int.Parse(AppUtils.getAppSetting("SMTP_Port"));
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                    (AppUtils.getAppSetting("SendMailAddress"), AppUtils.getAppSetting("SendMailPassword"));// Enter senders User name and password
                System.IO.MemoryStream ms = new System.IO.MemoryStream(AttachFile);

                //create the attachment from a stream. Be sure to name the data with a file and
                //media type that is respective of the data
                mail.Attachments.Add(new System.Net.Mail.Attachment(ms, "example.xls", "application/vnd.ms-excel"));
                smtp.Send(mail);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool IsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}