using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Data.Entity.Validation;

namespace Utilities
{
    public static class ExceptionUtils
    {
        /// <summary>
        /// Insert a string into text files in folder /LogFile/. The files are split by Date
        /// </summary>
        /// <param name="FunctionName">Title of the Log</param>
        /// <param name="ErrorMessage">Content of the log</param>
        public static void logError(String FunctionName, String ErrorMessage)
        {
            DateTime today = DateTime.Now;
            String fileName = today.ToString("yyyyMMdd");
            //String filePath=HttpContext.Current.Server.MapPath("~/LogFile/"+fileName+".txt");
            String filePath = HostingEnvironment.MapPath("~/LogFile/" + fileName + ".txt");

            FileInfo file = new FileInfo(filePath);
            StreamWriter fileStream;
            if (file.Exists)
            {
                fileStream = new StreamWriter(filePath, true);
            }
            else
            {
                fileStream = new StreamWriter(filePath, false);
            }

            fileStream.WriteLine("**********  " + FunctionName + "  **********");
            fileStream.WriteLine("----------" + today.ToString("dd MMM yyyy HH:mm") + "---------\r\n");
            fileStream.WriteLine(ErrorMessage);
            fileStream.WriteLine("\r\n\r\n\r\n");
            fileStream.Close();

        }

        /// <summary>
        /// Get Error String of a DbEntityValidationException
        /// </summary>
        /// <param name="ex">DbEntityValidationException</param>
        /// <returns>Error String</returns>
        public static string getEntityValidationErrors(DbEntityValidationException ex)
        {
            String Error = "";
            foreach (var eve in ex.EntityValidationErrors)
            {
                Error += "Entity of type \"" + eve.Entry.Entity.GetType().Name + "\" in state \"" + eve.Entry.State + "\" has the following validation errors:" + Environment.NewLine;

                foreach (var ve in eve.ValidationErrors)
                {
                    Error += "   - Property: \"" + ve.PropertyName + "\", Error: \"" + ve.ErrorMessage + "\"";

                }
            }
            return Error;
        }
    }


}