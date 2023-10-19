using System;
using System.Security.Cryptography;
using System.Text;

namespace TMS.Core.Utils
{
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;

    public static class Common
    {
        public static string SaveFile(HttpPostedFileWrapper postedFile, string directory)
        {
            if (string.IsNullOrEmpty(directory)) return null;
            if (!System.IO.Directory.Exists(directory)) System.IO.Directory.CreateDirectory(directory);
            var file = directory + "/" + postedFile.FileName + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx";
            postedFile.SaveAs(file);
            return file;
        }

        //============================ Using TMS =====================================================================//
        #region[Using TMS]
        public static string alert(string type, string content)
        {
            StringBuilder html = new StringBuilder();
            switch (type)
            {
                case "success":
                    html.AppendFormat("<div class='alert alert-success'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                    break;
                case "info":
                    html.AppendFormat("<div class='alert alert-info'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                    break;
                case "warning":
                    html.AppendFormat("<div class='alert alert-warning'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                    break;
                case "danger":
                    html.AppendFormat("<div class='alert alert-danger'><a href='javascript:void(0)' class='close' data-dismiss='alert' aria-label='close'>&times;</a>{0}</div>", content);
                    break;
            }
            return html.ToString();
        }
        public static object[] SetDBNullobject(object[] input)
        {
            object[] values = new object[] { };

            for (int i = 0; i < input.Length; i++)
            {
                object temp = DBNull.Value;
                if (!Common.IsNull(input[i]))
                {
                    temp = input[i];
                }
                input[i] = temp;
            }
            return input;
        }
        public static object[] SetObjectNull(object[] arr_coursefrom)
        {
            if (!Common.IsNull(arr_coursefrom))
            {
                for (int i = 0; i < arr_coursefrom.Length; i++)
                {
                    if (Common.IsNull(arr_coursefrom[i]))
                    {
                        arr_coursefrom[i] = null;
                    }
                    else
                    {
                        if (arr_coursefrom[i].ToString() == "-1")
                        {
                            arr_coursefrom[i] = null;
                        }
                    }
                }
            }
            return arr_coursefrom;
        }
        #endregion
        //============================ IsNull =====================================================================//
        #region [public static bool IsNull(object value)]
        public static bool IsNull(object value)
        {
            bool result = true;

            if (value != DBNull.Value && value != null && !String.IsNullOrEmpty(value.ToString()))
            {
                result = false;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNull(string value)]
        public static bool IsNull(string value)
        {
            bool result = true;

            if (value != null && !String.IsNullOrEmpty(value.ToString()))
            {
                result = false;
            }

            return result;
        }
        #endregion
        //============================ Cut String =====================================================================//
        #region [public static string[] SplitString(string value, char split)]
        public static string[] SplitString(string value, char split)
        {
            if (value != string.Empty)
            {
                string[] getValue = new string[3];
                if (value.Contains(split.ToString()))
                {
                    getValue = value.Split(new Char[] { split });
                }
                else
                {
                    getValue = new string[1];
                    getValue.SetValue(value, 0);
                }
                return getValue;
            }
            return null;
        }
        #endregion
        //============================ Validation Number ===============================================================//
        #region [public static bool IsNumber(object value, Type type)]
        public static bool IsNumber(object value, Type type)
        {
            bool result = false;

            if (!Common.IsNull(value))
            {
                switch (type.Name)
                {
                    case "SByte":
                        SByte sSByte = 0;
                        if (SByte.TryParse(value.ToString(), out sSByte)) result = true;
                        break;
                    case "Byte":
                        Byte bByte = 0;
                        if (Byte.TryParse(value.ToString(), out bByte)) result = true;
                        break;
                    case "Int16":
                        Int16 iInt16 = 0;
                        if (Int16.TryParse(value.ToString(), out iInt16)) result = true;
                        break;
                    case "Int32":
                        int iInt32 = 0;
                        if (int.TryParse(value.ToString(), out iInt32)) result = true;
                        break;
                    case "Int64":
                        Int64 iInt64 = 0;
                        if (Int64.TryParse(value.ToString(), out iInt64)) result = true;
                        break;
                    case "UInt16":
                        UInt16 uUint16 = 0;
                        if (UInt16.TryParse(value.ToString(), out uUint16)) result = true;
                        break;
                    case "UInt32":
                        uint uInt = 0;
                        if (uint.TryParse(value.ToString(), out uInt)) result = true;
                        break;
                    case "UInt64":
                        UInt64 uUInt64 = 0;
                        if (UInt64.TryParse(value.ToString(), out uUInt64)) result = true;
                        break;
                    case "Double":
                        Double dDouble = 0;
                        if (Double.TryParse(value.ToString(), out dDouble)) result = true;
                        break;
                    case "Single":
                        float fFloat = 0;
                        if (float.TryParse(value.ToString(), out fFloat)) result = true;
                        break;
                    case "Decimal":
                        decimal dDecimal = 0;
                        if (decimal.TryParse(value.ToString(), out dDecimal)) result = true;
                        break;
                }
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out int number)]
        public static bool IsNumber(object value, out int number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(int), out oNumber);
            number = int.Parse(oNumber.ToString());

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out Int16 number)]
        public static bool IsNumber(object value, out Int16 number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(Int16), out oNumber);
            number = (Int16)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out sbyte number)]
        public static bool IsNumber(object value, out sbyte number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(sbyte), out oNumber);
            number = (sbyte)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out byte number)]
        public static bool IsNumber(object value, out byte number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(byte), out oNumber);
            number = (byte)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out long number)]
        public static bool IsNumber(object value, out long number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(long), out oNumber);
            number = (long)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out ulong number)]
        public static bool IsNumber(object value, out ulong number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(ulong), out oNumber);
            number = (ulong)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out uint number)]
        public static bool IsNumber(object value, out uint number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(uint), out oNumber);
            number = (uint)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out UInt16 number)]
        public static bool IsNumber(object value, out UInt16 number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(UInt16), out oNumber);
            number = (UInt16)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out float number)]
        public static bool IsNumber(object value, out float number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(float), out oNumber);
            number = (float)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out double number)]
        public static bool IsNumber(object value, out double number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(double), out oNumber);
            number = (double)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out decimal number)]
        public static bool IsNumber(object value, out decimal number)
        {
            bool result = false;
            number = 0;
            object oNumber = 0;
            result = IsNumber(value, typeof(decimal), out oNumber);
            number = (decimal)oNumber;

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out sbyte? returnValue)]
        public static bool IsNumber(object value, out sbyte? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                sbyte temp = 0;
                if (sbyte.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out int? returnValue)]
        public static bool IsNumber(object value, out int? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                int temp = 0;
                if (int.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out Int16? returnValue)]
        public static bool IsNumber(object value, out Int16? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                Int16 temp = 0;
                if (Int16.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out long? returnValue)]
        public static bool IsNumber(object value, out long? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                long temp = 0;
                if (long.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out uint? returnValue)]
        public static bool IsNumber(object value, out uint? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                uint temp = 0;
                if (uint.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out UInt16? returnValue)]
        public static bool IsNumber(object value, out UInt16? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                UInt16 temp = 0;
                if (UInt16.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out ulong? returnValue)]
        public static bool IsNumber(object value, out ulong? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                ulong temp = 0;
                if (ulong.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out decimal? returnValue)]
        public static bool IsNumber(object value, out decimal? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                decimal temp = 0;
                if (decimal.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out float? returnValue)]
        public static bool IsNumber(object value, out float? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                float temp = 0;
                if (float.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, out double? returnValue)]
        public static bool IsNumber(object value, out double? returnValue)
        {
            bool result = false;
            returnValue = null;

            if (!Common.IsNull(value))
            {
                double temp = 0;
                if (double.TryParse(value.ToString(), out temp)) result = true;
                returnValue = temp;
            }

            return result;
        }
        #endregion
        #region [public static bool IsNumber(object value, Type type, out object number)]
        public static bool IsNumber(object value, Type type, out object number)
        {
            bool result = false;
            number = 0;

            if (!Common.IsNull(value))
            {
                switch (type.Name)
                {
                    case "SByte":
                        SByte sSByte = 0;
                        if (SByte.TryParse(value.ToString(), out sSByte)) result = true;
                        number = sSByte;
                        break;
                    case "Byte":
                        Byte bByte = 0;
                        if (Byte.TryParse(value.ToString(), out bByte)) result = true;
                        number = bByte;
                        break;
                    case "Int16":
                        Int16 iInt16 = 0;
                        if (Int16.TryParse(value.ToString(), out iInt16)) result = true;
                        number = iInt16;
                        break;
                    case "Int32":
                        int iInt32 = 0;
                        if (int.TryParse(value.ToString(), out iInt32)) result = true;
                        number = iInt32;
                        break;
                    case "Int64":
                        Int64 iInt64 = 0;
                        if (Int64.TryParse(value.ToString(), out iInt64)) result = true;
                        number = iInt64;
                        break;
                    case "UInt16":
                        UInt16 uUint16 = 0;
                        if (UInt16.TryParse(value.ToString(), out uUint16)) result = true;
                        number = uUint16;
                        break;
                    case "UInt32":
                        uint uInt = 0;
                        if (uint.TryParse(value.ToString(), out uInt)) result = true;
                        number = uInt;
                        break;
                    case "UInt64":
                        UInt64 uUInt64 = 0;
                        if (UInt64.TryParse(value.ToString(), out uUInt64)) result = true;
                        number = uUInt64;
                        break;
                    case "Double":
                        Double dDouble = 0;
                        if (Double.TryParse(value.ToString(), out dDouble)) result = true;
                        number = dDouble;
                        break;
                    case "Single":
                        float fFloat = 0;
                        if (float.TryParse(value.ToString(), out fFloat)) result = true;
                        number = fFloat;
                        break;
                    case "Decimal":
                        decimal dDecimal = 0;
                        if (decimal.TryParse(value.ToString(), out dDecimal)) result = true;
                        number = dDecimal;
                        break;
                }
            }

            return result;
        }
        #endregion
        //============================ Validation Boolean =============================================================//
        #region [public static bool IsBoolean(object value)]
        public static bool IsBoolean(object value)
        {
            bool result = false;

            if (!Common.IsNull(value))
            {
                bool bBoolean = false;
                if (bool.TryParse(value.ToString(), out bBoolean)) result = true;
            }

            return result;
        }
        #endregion
        #region [public static bool IsBoolean(object value, out bool boolean)]
        public static bool IsBoolean(object value, out bool boolean)
        {
            bool result = false;
            boolean = false;

            if (!Common.IsNull(value))
            {
                bool bBoolean = false;
                if (bool.TryParse(value.ToString(), out bBoolean)) result = true;
                boolean = bBoolean;
            }

            return result;
        }
        #endregion
        #region [public static bool IsBoolean(object value, out bool? boolean)]
        public static bool IsBoolean(object value, out bool? boolean)
        {
            bool result = false;
            boolean = null;

            if (!Common.IsNull(value))
            {
                bool bBoolean = false;
                if (bool.TryParse(value.ToString(), out bBoolean)) result = true;
                boolean = bBoolean;
            }

            return result;
        }
        #endregion
        //============================ Validation DateTime ============================================================//
        #region [public static bool IsDateTime(object value)]
        public static bool IsDateTime(object value)
        {
            bool result = false;

            if (!Common.IsNull(value))
            {
                DateTime dDateTime = DateTime.Now;
                if (DateTime.TryParse(value.ToString(), out dDateTime)) result = true;
            }

            return result;
        }
        #endregion
        #region [public static bool IsDateTime(object value, out DateTime dateTime)]
        public static bool IsDateTime(object value, out DateTime dateTime)
        {
            bool result = false;
            dateTime = DateTime.Now;

            if (!Common.IsNull(value))
            {
                DateTime dDateTime = DateTime.Now;
                if (DateTime.TryParse(value.ToString(), out dDateTime)) result = true;
                dateTime = dDateTime;
            }

            return result;
        }
        #endregion
        #region [public static bool IsDateTime(object value, out DateTime? dateTime)]
        public static bool IsDateTime(object value, out DateTime? dateTime)
        {
            bool result = false;
            dateTime = null;

            if (!Common.IsNull(value))
            {
                DateTime dDateTime = DateTime.Now;
                if (DateTime.TryParse(value.ToString(), out dDateTime)) result = true;
                dateTime = dDateTime;
            }

            return result;
        }
        #endregion
        //============================ Convert Text To Date  ==========================================================//
        #region[private static string ConvertToText(object number)]
        private static string ConvertToText(object number)
        {
            string s = number.ToString();
            string[] so = new string[] { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
            string[] hang = new string[] { "", "ngàn", "triệu", "tỷ" };
            int i, j, donvi, chuc, tram;
            string str = " ";
            bool booAm = false;
            decimal decS = 0;
            //Tung addnew
            try
            {
                decS = Convert.ToDecimal(s.ToString());
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            if (decS < 0)
            {
                decS = -decS;
                s = decS.ToString();
                booAm = true;
            }
            i = s.Length;
            if (i == 0)
                str = so[0] + str;
            else
            {
                j = 0;
                while (i > 0)
                {
                    donvi = Convert.ToInt32(s.Substring(i - 1, 1));
                    i--;
                    if (i > 0)
                        chuc = Convert.ToInt32(s.Substring(i - 1, 1));
                    else
                        chuc = -1;
                    i--;
                    if (i > 0)
                        tram = Convert.ToInt32(s.Substring(i - 1, 1));
                    else
                        tram = -1;
                    i--;
                    if ((donvi > 0) || (chuc > 0) || (tram > 0) || (j == 3))
                        str = hang[j] + str;
                    j++;
                    if (j > 3) j = 1;
                    if ((donvi == 1) && (chuc > 1))
                    {
                        if (chuc == 10)
                            str = "một " + str;
                        else
                            str = "mốt " + str;
                    }
                    else
                    {
                        if ((donvi == 5) && (chuc > 0))
                            str = "lăm " + str;
                        else if (donvi > 0)
                            str = so[donvi] + " " + str;
                    }
                    if (chuc < 0)
                        break;
                    else
                    {
                        if ((chuc == 0) && (donvi > 0)) str = "linh " + str;
                        if (chuc == 1) str = "mười " + str;
                        if (chuc > 1) str = so[chuc] + " mươi " + str;
                    }
                    if (tram < 0) break;
                    else
                    {
                        if ((tram > 0) || (chuc > 0) || (donvi > 0)) str = so[tram] + " trăm " + str;
                    }
                    str = " " + str;
                }
            }
            if (booAm) str = "Âm " + str;
            return str;
        }
        #endregion
        #region[public static string ConvertDateToText(int year, int month, int day)]
        public static string ConvertDateToText(int year, int month, int day)
        {
            string str = "";
            string sYear = ConvertToText(year.ToString("#"));
            string sMonth = ConvertToText(month.ToString("#"));
            string sDay = ConvertToText(day.ToString("#"));

            str = String.Format("Ngày {0} tháng {1} năm {2}", sDay, sMonth, sYear);

            return str;
        }
        #endregion
        #region[public static string ConvertNumberDecimalToText(decimal number)]
        public static string ConvertNumberDecimalToText(decimal number)
        {
            string str = ConvertToText(number.ToString("#"));
            return str;
        }
        #endregion
        #region[public static string ConvertNumberIntToText(int number)]
        public static string ConvertNumberIntToText(int number)
        {
            string str = ConvertToText(number.ToString("#"));
            return str;
        }
        #endregion
        #region[public static string ConvertNumberDoubleToText(double number)]
        public static string ConvertNumberDoubleToText(double number)
        {
            string str = ConvertToText(number.ToString("#"));
            return str;
        }
        #endregion

        public static bool CheckImgFile(string fileExtension)
        {

            var aaaa = fileExtension.ToLower();
            var check = !string.IsNullOrEmpty(fileExtension) &&
                        UtilConstants.ImageExtensions.Contains(fileExtension.ToLower());

            return check;
        }


        public static string RandomCharecter()
        {
            const string CharHoa = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string so = "0123456789";
            const string kytu = "!@#";
            const string charrandom = "!@#0123456789QWERTYUIOPLKJHGFDSAZXCVBNM";
            var random = new Random();
            var resultHoa = new string(
                Enumerable.Repeat(CharHoa, 2)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var resultThuong = new string(
                Enumerable.Repeat(CharHoa.ToLower(), 2)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            var resultso = new string(
               Enumerable.Repeat(so, 2)
                         .Select(s => s[random.Next(s.Length)])
                         .ToArray());
            var resultkytu = new string(
               Enumerable.Repeat(kytu.ToLower(), 2)
                         .Select(s => s[random.Next(s.Length)])
                         .ToArray());
            var resultcharrandom = new string(
               Enumerable.Repeat(charrandom, 2)
                         .Select(s => s[random.Next(s.Length)])
                         .ToArray());

            return resultcharrandom + resultso + resultThuong + resultkytu + resultHoa;
        }
        public static string RandomCode()
        {
            const string CharHoa = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string so = "0123456789";
            const string charrandom = "0123456789QWERTYUIOPLKJHGFDSAZXCVBNM";
            var random = new Random();
            var resultHoa = new string(
                Enumerable.Repeat(CharHoa, 2)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            var resultThuong = new string(
                Enumerable.Repeat(CharHoa.ToLower(), 2)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            var resultso = new string(
                Enumerable.Repeat(so, 2)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
            var resultcharrandom = new string(
                Enumerable.Repeat(charrandom, 2)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());

            return resultcharrandom + resultso + resultThuong + resultHoa;
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

        public static string EncryptURL(string input)
        {
            var _return = "";
            foreach (char c in input)
            {
                switch (c.ToString())
                {
                    case "1": _return += "n"; break;
                    case "2": _return += "m"; break;
                    case "3": _return += "/"; break;
                    case "4": _return += "."; break;
                    case "5": _return += "1"; break;
                    case "6": _return += "2"; break;
                    case "7": _return += "3"; break;
                    case "8": _return += "4"; break;
                    case "9": _return += "5"; break;
                    case "0": _return += "6"; break;
                    case "q": _return += "7"; break;
                    case "w": _return += "8"; break;
                    case "e": _return += "9"; break;
                    case "r": _return += "0"; break;
                    case "t": _return += "q"; break;
                    case "y": _return += "w"; break;
                    case "u": _return += "e"; break;
                    case "i": _return += "r"; break;
                    case "o": _return += "t"; break;
                    case "p": _return += "y"; break;
                    case "a": _return += "u"; break;
                    case "s": _return += "i"; break;
                    case "d": _return += "o"; break;
                    case "f": _return += "p"; break;
                    case "g": _return += "a"; break;
                    case "h": _return += "s"; break;
                    case "j": _return += "d"; break;
                    case "k": _return += "f"; break;
                    case "l": _return += "g"; break;
                    case "z": _return += "h"; break;
                    case "x": _return += "j"; break;
                    case "c": _return += "k"; break;
                    case "v": _return += "l"; break;
                    case "b": _return += "z"; break;
                    case "n": _return += "x"; break;
                    case "m": _return += "c"; break;
                    case "/": _return += "v"; break;
                    case ".": _return += "b"; break;
                    case " ": _return += "$"; break;
                    default: _return += c.ToString(); break;
                }
            }
            return _return.Trim();
        }
        public static string DecryptURL(string input)
        {
            var _return = "";
            foreach (char c in input)
            {
                switch (c.ToString())
                {
                    case "n": _return += "1"; break;
                    case "m": _return += "2"; break;
                    case "/": _return += "3"; break;
                    case ".": _return += "4"; break;
                    case "1": _return += "5"; break;
                    case "2": _return += "6"; break;
                    case "3": _return += "7"; break;
                    case "4": _return += "8"; break;
                    case "5": _return += "9"; break;
                    case "6": _return += "0"; break;
                    case "7": _return += "q"; break;
                    case "8": _return += "w"; break;
                    case "9": _return += "e"; break;
                    case "0": _return += "r"; break;
                    case "q": _return += "t"; break;
                    case "w": _return += "y"; break;
                    case "e": _return += "u"; break;
                    case "r": _return += "i"; break;
                    case "t": _return += "o"; break;
                    case "y": _return += "p"; break;
                    case "u": _return += "a"; break;
                    case "i": _return += "s"; break;
                    case "o": _return += "d"; break;
                    case "p": _return += "f"; break;
                    case "a": _return += "g"; break;
                    case "s": _return += "h"; break;
                    case "d": _return += "j"; break;
                    case "f": _return += "k"; break;
                    case "g": _return += "l"; break;
                    case "h": _return += "z"; break;
                    case "j": _return += "x"; break;
                    case "k": _return += "c"; break;
                    case "l": _return += "v"; break;
                    case "z": _return += "b"; break;
                    case "x": _return += "n"; break;
                    case "c": _return += "m"; break;
                    case "v": _return += "/"; break;
                    case "b": _return += "."; break;
                    case "$": _return += " "; break;
                    default: _return += c.ToString(); break;
                }
            }
            return _return.Trim();
        }
    }
}
