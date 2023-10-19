using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TrainingCenter.Utilities
{
    public class ExportUtils
    {
        public ExportUtils()
        {
        }

        public static string ExcelContentType
        {
            get
            { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        }
        public void WriteFile(DataTable dtSource, string strPathToTableTemplate, string strPathToRowTemplate,
                              string strFileName, string[] strArrWordToBeReplaced,
                              string[] strArrWordReplace, string[] strArrWordInRowToBeReplaced,
                              string[] strArrColumnTitle, string strDateTimeFormat, string strContentType,
                              string strFileExtension, HttpResponse Response, HttpRequest Request)
        {
            //Đường dẫn Template
            string strFileTemplate = Request.PhysicalApplicationPath + strPathToTableTemplate;
            string strRowsTemplate = Request.PhysicalApplicationPath + strPathToRowTemplate;

            #region //Khai báo header trả về cho client

            Response.Buffer = true;
            Response.Expires = 0;
            Response.Clear();
            HttpContext.Current.Response.ContentType = strContentType;
            HttpContext.Current.Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            HttpContext.Current.Response.Charset = "utf-8";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + strFileName + strFileExtension);
            #endregion
            //đọc file template
            string strContent = ReadContentDataFromFile(strFileTemplate);
            string strRows = ReadContentDataFromFile(strRowsTemplate);
            string strRowsContent = string.Empty;

            #region Ghi dữ liệu vào content

            //ghi tiêu đề
            //replace cac mang trong tableTemplate
            for (int wordIndex = 0; wordIndex < strArrWordToBeReplaced.Count(); wordIndex++)
            {
                strContent = strContent.Replace(strArrWordToBeReplaced[wordIndex], strArrWordReplace[wordIndex]);
            }
            foreach (DataRow drSource in dtSource.Rows)
            {
                string strRowsTmp = strRows;
                //replace mang trong rowTemplate
                for (int wordIndex = 0; wordIndex < strArrWordInRowToBeReplaced.Count(); wordIndex++)
                {
                    //string strReplace = Convert.ToString(drSource[strArrColumnTitle[wordIndex]]).Replace("&", "và");
                    string strReplace = Convert.ToString(drSource[strArrColumnTitle[wordIndex]]);
                    //nếu kiểu dữ liệu là date time thì format lại theo kiểu định dạng được truyền vào
                    if (drSource[strArrColumnTitle[wordIndex]].GetType() == typeof(DateTime))
                    {
                        strReplace = Convert.ToDateTime(drSource[strArrColumnTitle[wordIndex]]).ToString("dd/MM/yyyy");
                    }
                    strRowsTmp = strRowsTmp.Replace(strArrWordInRowToBeReplaced[wordIndex], strReplace);
                }
                strRowsContent += strRowsTmp;
            }
            //Ghi dữ liệu vào content
            strContent = strContent.Replace("[ROWS]", strRowsContent);
            #endregion Ghi dữ liệu vào content
            //write content
            Response.Write(strContent);
            Response.Flush();
            Response.Close();
            Response.End();
        }

        protected string ReadContentDataFromFile(string strFileName)
        {
            FileStream objFileStream = null;
            try
            {
                objFileStream = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
                StreamReader objReader = new StreamReader(objFileStream);
                string s = objReader.ReadToEnd();
                return s;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objFileStream != null)
                    objFileStream.Close();
            }
        }

        public void Export(string strPath, DataTable dtData)
        {

        }

        public static DataTable ConvertToDatatable(Object[] array)
        {
            PropertyInfo[] properties = array.GetType().GetElementType().GetProperties();
            DataTable dt = CreateDataTable(properties);
            if (array.Length != 0)
            {
                foreach (object o in array)
                    FillData(properties, dt, o);
            }
            return dt;
        }

        private static DataTable CreateDataTable(PropertyInfo[] properties)
        {
            DataTable dt = new DataTable();
            DataColumn dc = null;
            foreach (PropertyInfo pi in properties)
            {
                dc = new DataColumn();
                dc.ColumnName = pi.Name;
                dt.Columns.Add(dc);
            }
            return dt;
        }

        private static void FillData(PropertyInfo[] properties, DataTable dt, Object o)
        {
            DataRow dr = dt.NewRow();
            foreach (PropertyInfo pi in properties)
            {
                dr[pi.Name] = pi.GetValue(o, null);
            }
            dt.Rows.Add(dr);
        }
    }
}