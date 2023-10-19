using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Utilities
{
    public static class AppUtils
    {
        public static string getAppSetting(string key)
        {
            return System.Configuration.ConfigurationManager.AppSettings[key];
        }



        public static string GenerateRandomPassword(int numOfChar)
        {
            Random randomClass = new Random();
            String result = "";
            for (int i = 0; i < numOfChar; i++)
            {
                result += randomClass.Next(0, 9).ToString();
            }
            return result;
        }

        public static double MilliTimeStamp(DateTime TheDate)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = TheDate.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);

            return ts.TotalMilliseconds;
        }

        public static void StringToDateRange(string DateString, out DateTime? FromDate, out DateTime? ToDate)
        {
            try
            {
                DateTime FDate = new DateTime();
                DateTime TDate = new DateTime();
                var DateList = DateString.Split(new char[] { '-' });
                if (DateTime.TryParseExact(DateList[0].ToString().Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out FDate))
                {
                    FromDate = FDate;
                }
                else
                {
                    FromDate = null;
                }
                if (DateTime.TryParseExact(DateList[1].ToString().Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out TDate))
                {
                    ToDate = TDate;
                }
                else
                {
                    ToDate = null;
                }

            }
            catch (Exception)
            {
                FromDate = null;
                ToDate = null;
            }
        }
        public static void StringToDateRange2(string DateString, out DateTime? FromDate, out DateTime? ToDate)
        {
            try
            {
                IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
                DateTime FDate = new DateTime();
                DateTime TDate = new DateTime();
                var DateList = DateString.Split(new char[] { '-' });
                if (DateTime.TryParseExact(DateList[0].ToString().Trim(), "dd/MM/yyyy", culture, System.Globalization.DateTimeStyles.None, out FDate))
                {
                    FromDate = FDate;
                }
                else
                {
                    FromDate = null;
                }
                if (DateTime.TryParseExact(DateList[1].ToString().Trim(), "dd/MM/yyyy", culture, System.Globalization.DateTimeStyles.None, out TDate))
                {
                    ToDate = TDate;
                }
                else
                {
                    ToDate = null;
                }

            }
            catch (Exception)
            {
                FromDate = null;
                ToDate = null;
            }
        }
        public static string NullableDate2String(DateTime? item)
        {
            if (item == null)
            {
                return "";
            }
            else
            {
                return item.Value.ToString("dd MMM yyyy");
            }
        }
    }
}