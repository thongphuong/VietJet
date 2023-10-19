using System;
using System.Net;
using Microsoft.Exchange.WebServices.Data;

namespace TrainingCenter.Utilities
{
    public class MailExchange
    {
        private readonly string _url;
        private readonly string _username;
        private readonly string _password;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public MailExchange(string url, string username, string password)
        {
            this._url = url;
            this._username = username;
            this._password = password;
        }

        public bool SendEmail(string subject, string body, string toRecipients, string ccRecipients = "")
        {
            try
            {
                ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2010_SP1);
                ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
                service.Credentials = new WebCredentials(this._username, this._password);
                service.TraceEnabled = true;
                service.TraceFlags = TraceFlags.All;
                service.Url = new Uri(this._url);
                EmailMessage email = new EmailMessage(service);
                if (!String.IsNullOrEmpty(toRecipients))
                {
                    string[] lsEmail = toRecipients.Split(new char[] { ';' });
                    foreach (string item in lsEmail)
                    {
                        email.ToRecipients.Add(item.Trim());
                    }
                }
                if (!String.IsNullOrEmpty(ccRecipients))
                {
                    string[] lsEmail = ccRecipients.Split(new char[] { ';' });
                    foreach (string item in lsEmail)
                    {
                        email.CcRecipients.Add(item.Trim());
                    }
                }
                email.Subject = subject;
                email.Body = new MessageBody(body);
                email.Send();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ValidEmail(string email)
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool CertificateValidationCallBack(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            bool result = false;
            Uri redirectionUri = new Uri("");
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }
    }
}