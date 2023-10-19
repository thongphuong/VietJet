using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Web;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Data;
using System.Globalization;
using System.Net.Mail;
using System.Web.Hosting;
using System.Net;

namespace TMS.API.Utilities
{
    public static class Command
    {
        static object lock_statement = new Object();
   
        public static string TicksToString()
        {
            long ticks = DateTime.Now.Ticks;
            return string.Format("{0:X}", ticks).ToLower();
        }
        public static int CountWords(string text, string separator, int index)
        {
            var array = Regex.Split(text, separator);
            return array[index].Length;
        }
        public static IEnumerable<int> StringToIntList(string str)
        {
            if (String.IsNullOrEmpty(str))
                yield break;

            foreach (var s in str.Split(','))
            {
                int num;
                if (int.TryParse(s, out num))
                    yield return num;
            }
        }
        public static string ConvertRegular(string str)
        {
            return System.Text.RegularExpressions.Regex.Replace(str.ToString(), @"\t|\n|\r", "");
        }
        public static int Weeks(int year, int month)
        {
            DayOfWeek wkstart = DayOfWeek.Monday;

            DateTime first = new DateTime(year, month, 1);
            int firstwkday = (int)first.DayOfWeek;
            int otherwkday = (int)wkstart;

            int offset = ((otherwkday + 7) - firstwkday) % 7;

            double weeks = (double)(DateTime.DaysInMonth(year, month) - offset) / 7d;

            return (int)Math.Ceiling(weeks);
        }
        public static int GetWeekInMonth(DateTime date)
        {
            DateTime tempdate = date.AddDays(-date.Day + 1);

            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNumStart = ciCurr.Calendar.GetWeekOfYear(tempdate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum - weekNumStart + 1;

        }
        public static int GetWeeks(DateTime date)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            int week = weekNum % 4 == 0 ? 4 : weekNum % 4;
            return week;
        }
        public static string ConvertToUnSign(string text)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = text.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static string ConvertFaceToString(string text)
        {
            string temp = text.Replace(" ", "_");
            if (temp.Length > 32)
            {
                return temp.Substring(0, 32);
            }
            else
            {
                return temp;
            }

        }
        public static string ReplaceRealValue(string text)
        {
            string test = "";
            var lsList = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (lsList.Any())
            {
                test = lsList.Aggregate(test, (current, y) => string.Concat(current, y));
            }
            else
            {
                test = text;
            }

            return test;
        }
        public static string GetKyTuDau(string text)
        {
            var lsList = text.Replace(" ", ",").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string test = lsList.Aggregate("", (current, i) => current + (string.IsNullOrEmpty(i.Trim()) ? "" : i.Trim().Substring(0, 1)));
            return test.ToUpper();
        }

        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static bool CheckExitsFile(string file)
        {

            if (System.IO.File.Exists(file))
            {
                return true;
            }
            return false;
        }
        public static void DeleteFile(string path)
        {

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }



        }

        public static void SaveFile(string path, string name, HttpPostedFileBase input)
        {
            CreateFolder(path);//tao thu muc neu chua co
            input.SaveAs(path + "/" + name);

        }

        public static void SetFileDetails(HttpPostedFileBase f, out string fileName, out string filepath, out string fileExtension)
        {
            fileName = Path.GetFileName(f.FileName);
            fileExtension = Path.GetExtension(f.FileName);
            filepath = Path.GetFullPath(f.FileName);
        }

        //public static DataTable ExcelToTable(HttpPostedFileBase filePath)
        //{
        //    IExcelDataReader reader = null;

        //    try
        //    {
        //        if (filePath.FileName.ToLower().Trim().EndsWith(".xls"))
        //        {
        //            reader = ExcelReaderFactory.CreateBinaryReader(filePath.InputStream);
        //        }
        //        if (filePath.FileName.ToLower().Trim().EndsWith(".xlsx"))
        //        {
        //            reader = ExcelReaderFactory.CreateOpenXmlReader(filePath.InputStream);
        //        }
        //        reader.IsFirstRowAsColumnNames = true;
        //        DataSet result = reader.AsDataSet();
        //        DataTable dt = result.Tables[0];

        //        reader.Close();
        //        return dt;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }


        //}

        public static void SaveImage(string path, string imageSave, HttpPostedFileBase filePost)
        {

            CreateFolder(path);//tao thu muc neu chua co
            Image img = Image.FromStream(filePost.InputStream);
            Bitmap bitmapMasterImage = new System.Drawing.Bitmap(img);
            bitmapMasterImage.Save(path + "/" + imageSave);

        }
        public static bool GetFileCSV(HttpPostedFileBase input)
        {
            string type = input.FileName.Substring(input.FileName.LastIndexOf('.') + 1);
            if (type.ToLower() == "csv" || type.ToLower() == "txt")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string GetFileName(HttpPostedFileBase input)
        {
            string filename = input.FileName;
            if (input.FileName.Contains("\\"))
            {
                filename = input.FileName.Substring(input.FileName.LastIndexOf("\\") + 2);
            }
            string type = filename.Substring(filename.LastIndexOf('.'));
            string nameImgDefault = TextClearNormal(filename.Replace(type, ""), 50) + "_" + DateTime.Now.ToString("hhmmssffMMddyy");
            return nameImgDefault + type;
        }

        public static void DeleteImage(string path, string imageFile)
        {
            string paththumb = path + "thumb\\" + imageFile;
            string pathmedium = path + "medium\\" + imageFile;
            string pathlarge = path + imageFile;
            DeleteFile(paththumb);
            DeleteFile(pathmedium);
            DeleteFile(pathlarge);
        }

        public static string GetFormatFile(string contentType)
        {
            string format;
            switch (contentType)
            {
                case "image/png":
                    format = "png";
                    break;
                case "image/gif":
                    format = "gif";
                    break;
                case "application/x-shockwave-flash":
                    format = "swf";
                    break;
                default:
                    format = "jpg";
                    break;
            }
            return format;
        }

       

        public static string GetSiteRoot()
        {
            string port = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_PORT"];
            if (port == null || port == "80" || port == "443")
                port = "";
            else
                port = ":" + port;

            string protocol = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_PORT_SECURE"];
            if (protocol == null || protocol == "0")
                protocol = "http://";
            else
                protocol = "https://";

            string sOut = protocol + System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + port + System.Web.HttpContext.Current.Request.ApplicationPath;

            if (sOut.EndsWith("/"))
            {
                sOut = sOut.Substring(0, sOut.Length - 1);
            }

            return sOut;
        }



        public static string ToFriendly(string title, int maxLength)//bo dau
        {

            string titleUni = ToFriendly1(title);
            var match = Regex.Match(titleUni.ToLower(), "[\\w]+");
            StringBuilder result = new StringBuilder("");
            bool maxLengthHit = false;
            while (match.Success && !maxLengthHit)
            {
                if (result.Length + match.Value.Length <= maxLength)
                {
                    result.Append(match.Value + "-");
                }
                else
                {
                    maxLengthHit = true;
                    // Handle a situation where there is only one word and it is greater than the max length.
                    if (result.Length == 0) result.Append(match.Value.Substring(0, maxLength));
                }
                match = match.NextMatch();

            }
            // Remove trailing '-'
            if (result[result.Length - 1] == '-') result.Remove(result.Length - 1, 1);
            return result.ToString();
        }

        public static string ToFriendly1(string ip_str_change)//bo dau
        {
            Regex vRegRegex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string vStrFormD = ip_str_change.Normalize(NormalizationForm.FormD);
            vStrFormD = vStrFormD.Replace(",", "-");
            vStrFormD = vStrFormD.Replace(" ", "-");
            vStrFormD = vStrFormD.Replace("--", "-");
            return vRegRegex.Replace(vStrFormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').ToLower();
        }

        public static string ToFriendlyForsearch(string ipStrChange)//bo dau
        {
            Regex vRegRegex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string vStrFormD = ipStrChange.Normalize(NormalizationForm.FormD);
            return vRegRegex.Replace(vStrFormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').ToLower();
        }

        public static int PageTotal(int total, int size)
        {
            int result = 0;
            result = total / size;
            if (total % size > 0)
                result += 1;
            return result;
        }

        public static string TextClear(string text, int length)
        {
            if (!String.IsNullOrEmpty(text) && text.Length > length)
            {
                string cutString = text.Substring(0, length);
                var array = Regex.Split(cutString.Trim(), "\\s+").ToList();
                array.RemoveAt(array.Count() - 1);

                return string.Join(" ", array) + "...";
            }
            else
            {
                return text;
            }
        }
        public static string TextClearNormal(string text, int length)
        {
            string result = "";
            if (text != null && text.Length > length)
            {
                result = text;
                int l = text.Length;
                if (l > length)
                    result = text.Substring(0, length);
            }
            else
            {
                result = text;
            }
            return result;
        }



        public static string RandomCharecter()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }

        public static string EncryptString(string message)
        {
            byte[] results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(UTF8.GetBytes("@@@"));

            // Step 2. Create a new AesCryptoServiceProvider object
            AesCryptoServiceProvider tdesAlgorithm = new AesCryptoServiceProvider();

            // Step 3. Setup the encoder
            tdesAlgorithm.Key = tdesKey;
            tdesAlgorithm.Mode = CipherMode.ECB;
            tdesAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToEncrypt = UTF8.GetBytes(message);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(results);
        }

        public static string DecryptString(string message)
        {
            byte[] results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] tdesKey = hashProvider.ComputeHash(UTF8.GetBytes("@@@"));

            // Step 2. Create a new AesCryptoServiceProvider object
            AesCryptoServiceProvider tdesAlgorithm = new AesCryptoServiceProvider
            {
                Key = tdesKey,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            // Step 3. Setup the decoder

            // Step 4. Convert the input string to a byte[]
            byte[] dataToDecrypt = Convert.FromBase64String(message);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                tdesAlgorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(results);
        }

        public static string ConvertCurrency(string price)
        {
            var length = price.Length;
            if (!String.IsNullOrEmpty(price))
            {
                if (price.Equals("0"))
                {
                    return "Thỏa thuận";
                }
                else if (length > 6 && length <= 9)
                {
                    var result = Double.Parse(price) / 1000000;
                    return String.Format("{0}{1}", result, " triệu");
                }
                else if (length > 9)
                {
                    var result = Double.Parse(price) / 1000000000;
                    return String.Format("{0}{1}", result, " tỷ");
                }
                return String.Format("{0}{1}", Double.Parse(price).ToString("#,###"), " đ");
            }
            return "Thỏa thuận";
        }

        public static string ConvertYoutube(string videoLink)
        {
            string result = videoLink.Substring(videoLink.LastIndexOf("/watch?v=", StringComparison.Ordinal) + 9);
            return String.Format("//www.youtube.com/embed/{0}", result);
        }

        /// <summary>
        /// check valid email
        /// </summary>
        /// <param name="email">email (format: email id; email id;.....)</param>
        /// <returns>true if valid, false if not valid</returns>
        public static bool IsValidEmail(string email)
        {
            try
            {
                bool result = true;
                String[] arrEmail = email.Split(Char.Parse(";"));
                if ((from item in arrEmail let addr = new System.Net.Mail.MailAddress(item.Trim()) where addr.Address != item select item).Any())
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// check valid decimal
        /// </summary>
        /// <param name="inputString">decimal string</param>
        /// <returns>true if valid, false if not valid</returns>
        public static bool IsDecimal(string inputString)
        {
            try
            {
                Convert.ToDecimal(inputString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// check valid integer
        /// </summary>
        /// <param name="inputString">integer string</param>
        /// <returns>true if valid, false if not valid</returns>
        public static bool IsInteger(string inputString)
        {
            try
            {
                Convert.ToInt32(inputString);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string getAppSetting(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }

        public static class MailUtil
        {
            public static Boolean SendMail(String MailTo, String Subject, String Content)
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    mail.To.Add(MailTo);
                    mail.From = new MailAddress(getAppSetting("SendMailAddress"), "Training Center");
                    mail.Subject = Subject;
                    mail.Body = Content;
                    mail.IsBodyHtml = true;
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;// SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = getAppSetting("SMTP_Host");
                    smtp.Port = int.Parse(getAppSetting("SMTP_Port"));
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential
                    (getAppSetting("SendMailAddress"),getAppSetting("SendMailPassword"));// Enter senders User name and password
                    smtp.Send(mail);
                    return true;
                }
                catch (Exception ex)
                {
                    var pathForSaving = HostingEnvironment.MapPath("/Log.txt");
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine(ex.ToString());
                    System.IO.File.AppendAllText(pathForSaving, sb.ToString());
                    return false;
                }
            }
            public static Boolean SendMail(String MailTo, String Subject, String Content, byte[] AttachFile)
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    mail.To.Add(MailTo);
                    mail.From = new MailAddress(getAppSetting("SendMailAddress"), "Training Center");
                    mail.Subject = Subject;
                    mail.Body = Content;
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = getAppSetting("SMTP_Host");
                    smtp.Port = int.Parse(getAppSetting("SMTP_Port"));
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential
                        (getAppSetting("SendMailAddress"),getAppSetting("SendMailPassword"));// Enter senders User name and password
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

        //public static void sendEmailEx(string to, string cc, string subject, string content, string type, Employee em)
        //{


        //    Email mail = new Email();
        //    mail.To = to;
        //    mail.Cc = cc;
        //    mail.Subject = subject;
        //    mail.Content = content;
        //    mail.Content_Type = type;
        //    mail.Created_By = em == null ? -1 : -1;
        //    mail.Creation_Date = DateTime.Now;
        //    mail.Last_Updated_By = em == null ? -1 : -1;
        //    mail.Last_Updated_Date = DateTime.Now;

        //    try
        //    {
        //        UnitOfWork unitOfWork = new UnitOfWork();
        //        IRepository<Config> _repoConfig = null;
        //        IRepository<Email> _repoEmail = null;
        //        _repoConfig = unitOfWork.Repository<Config>();
        //        _repoEmail = unitOfWork.Repository<Email>();



        //        var lsConfig = _repoConfig.Get();
        //        string url = lsConfig.Where(x => x.Key == "KEY_EMAIL_URL").FirstOrDefault().Value.Trim();
        //        string usr = lsConfig.Where(x => x.Key == "KEY_EMAIL_USR").FirstOrDefault().Value.Trim();
        //        string pwd = lsConfig.Where(x => x.Key == "KEY_EMAIL_PWD").FirstOrDefault().Value.Trim();
        //        mail.From = usr;
        //        mail.Send_Date = DateTime.Now;
        //        /*
        //        if (usr.Contains('@'))
        //        {
        //            usr = usr.Substring(0, usr.IndexOf('@'));
        //        }
        //        */
        //        MailExchange email = new MailExchange(url, usr, pwd);
        //        if (email.SendEmail(mail.Subject, mail.Content, mail.To, mail.Cc))
        //        {
        //            mail.Send_Flag = "Y";
        //        }
        //        else
        //        {
        //            mail.Send_Flag = "N";
        //        }

        //        _repoEmail.Insert(mail);
        //        unitOfWork.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    finally
        //    {
        //    }
        //}


        //public static void SendEmailNM(string to, string cc, string subject, string content, string type, Employee em)
        //{

        //    Email mail = new Email();
        //    mail.To = to;
        //    mail.Cc = cc;
        //    mail.Subject = subject;
        //    mail.Content = content;
        //    mail.Content_Type = type;
        //    mail.Created_By = em == null ? -1 : -1;
        //    mail.Creation_Date = DateTime.Now;
        //    mail.Last_Updated_By = em == null ? -1 : -1;
        //    mail.Last_Updated_Date = DateTime.Now;

        //    try
        //    {
        //        UnitOfWork unitOfWork = new UnitOfWork();
        //        IRepository<Config> _repoConfig = null;
        //        IRepository<Email> _repoEmail = null;
        //        _repoConfig = unitOfWork.Repository<Config>();
        //        _repoEmail = unitOfWork.Repository<Email>();



        //        var lsConfig = _repoConfig.Get();
        //        string url = lsConfig.Where(x => x.Key == "KEY_EMAIL_URL").FirstOrDefault().Value.Trim();
        //        string usr = lsConfig.Where(x => x.Key == "KEY_EMAIL_USR").FirstOrDefault().Value.Trim();
        //        string pwd = lsConfig.Where(x => x.Key == "KEY_EMAIL_PWD").FirstOrDefault().Value.Trim();

        //        var configSSl = false;
        //        if (url.Contains("gmail"))
        //        {
        //            configSSl = true;
        //        }
        //        //var fromAddress = new MailAddress(em.Email); "epoaapp@pfizer.com","EPOA System"
        //        var fromAddress = new MailAddress(SystemEmailAddress, SystemEmailName);
        //        var toAddress = new MailAddress(to);
        //        //send mail to customer
        //        var smtp = new SmtpClient
        //        {
        //            Host = url,
        //            // Port = 587,
        //            EnableSsl = configSSl,
        //            DeliveryMethod = SmtpDeliveryMethod.Network,
        //            UseDefaultCredentials = false,
        //        };

        //        MailMessage mailsend = new MailMessage(fromAddress, toAddress);
        //        mailsend.IsBodyHtml = true;
        //        mailsend.Body = mail.Content;
        //        mailsend.Subject = mail.Subject;

        //        try
        //        {
        //            smtp.Send(mailsend);
        //            mail.Send_Flag = "Y";
        //        }
        //        catch (Exception)
        //        {
        //            mail.Send_Flag = "N";
        //        }

        //        _repoEmail.Insert(mail);
        //        unitOfWork.SaveChanges();

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}


    }
}