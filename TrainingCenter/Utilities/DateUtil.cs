using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TrainingCenter.Utilities
{
    public static class DateUtil 
    {
        public const string DATE_FORMAT_OUTPUT = @"dd\/MM\/yyyy";
        public const string DATE_FORMAT_OUTPUT_YYYY_MM_DD = @"yyyy-MM-dd";
        public const string DATE_FORMAT_OUTPUT_YYYYMMDD = @"yyyyMMdd";
        public const string DATE_FORMAT_FULL_YYYYMMDDHHMMSS = @"yyyyMMddHHmmss";
        public const string KEY_FROM_DATE = "FROM";
        public const string KEY_TO_DAE = "TO";
        public const string KEY_WEEK = "W";

        /// <summary>
        /// <author>MinhNT</author>
        /// <remarks>convert string to date</remarks>
        /// <param name="strDate">date string</param>
        /// <param name="format">format date</param>
        /// <returns>return datetime of strDate if correct, return datetime now if not correct</returns>
        /// </summary>
        public static DateTime StringToDate(string strDate, string format)
        {
            DateTime result;
            try
            {
                // If this line is reached, no exception was thrown
                result = DateTime.ParseExact(strDate, format, null);
            }
            catch (Exception)
            {
                // If this line is reached, an exception was thrown
                result = DateTime.Now;
            }

            return result;
        }

        /// <summary>
        /// <author>MinhNT</author>
        /// <remarks>convert date to string</remarks>
        /// <param name="strDate">date Datetime?</param>
        /// <param name="format">format string</param>
        /// <returns>return datetime string of date if correct, return empty if not correct</returns>
        /// </summary>
        public static string DateToString(DateTime? date, string format)
        {
            string result;

            try
            {
                // If this line is reached, no exception was thrown
                result = date.Value.ToString(format);
            }
            catch (Exception)
            {
                // If this line is reached, an exception was thrown
                result = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Convert a date time object to Unix time representation.
        /// </summary>
        /// <param name="datetime">The datetime object to convert to Unix time stamp.</param>
        /// <returns>Returns a numerical representation (Unix time) of the DateTime object.</returns>
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ConvertToUnixTime(DateTime datetime)
        {
            //DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //return (long)(datetime - sTime).TotalSeconds;
            TimeSpan elapsedTime = datetime - Epoch;
            return (long)elapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Convert Unix time value to a DateTime object.
        /// </summary>
        /// <param name="unixtime">The Unix time stamp you want to convert to DateTime.</param>
        /// <returns>Returns a DateTime object that represents value of the Unix time.</returns>
        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return sTime.AddSeconds(unixtime);
        }


        /// <summary>
        /// <author>MinhNT</author>
        /// <remarks>check string is datetime</remarks>
        /// <param name="strDate">date string</param>
        /// <param name="format">format date</param>
        /// <returns>return true if correct, return false if not correct</returns>
        /// </summary>
        public static bool IsDateTime(string strDate, string format)
        {
            bool result;

            try
            {
                // If this line is reached, no exception was thrown
                DateTime dtParse = DateTime.ParseExact(strDate, format, null);
                result = true;
            }
            catch (Exception)
            {
                // If this line is reached, an exception was thrown
                result = false;
            }

            return result;
        }

        /// <summary>
        /// get week by date (week: monday --> saturday)
        /// </summary>
        /// <param name="date">date DateTime</param>
        /// <param name="from_to">from of Date, to of Date</param>
        /// <returns>week sring</returns>
        public static string getWeek(DateTime date, string from_to)
        {
            if (date == null)
            {
                return string.Empty;
            }

            int week;

            week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            return KEY_WEEK + week.ToString();
        }
    }
}
