
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
using Excel;

namespace TrainingCenter.Utilities
{
    using DAL.Entities;

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

            foreach (var s in str.Split(new char[] { ',' }))
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
                temp.Substring(0, 32);
                return temp;
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

        #region resizeimage
        

        #endregion

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


        public static DataTable ExcelToTable(HttpPostedFileBase filePath)
        {
            IExcelDataReader reader = null;

            try
            {
                if (filePath.FileName.ToLower().Trim().EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(filePath.InputStream);
                }
                if (filePath.FileName.ToLower().Trim().EndsWith(".xlsx"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(filePath.InputStream);
                }
                reader.IsFirstRowAsColumnNames = true;
                DataSet result = reader.AsDataSet();
                DataTable dt = result.Tables[0];

                reader.Close();
                return dt;
            }
            catch (Exception)
            {

                throw;
            }


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
                //bool result = true;
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

        


    }
}