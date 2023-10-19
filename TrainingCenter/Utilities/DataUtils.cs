using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Utilities
{
    using TMS.Core.ViewModels.ViewModel;
    using TrainingCenter.Helpers;

    public static class DataUtils
    {
        public static List<ImportTraineeViewModel> ReadEmployeeXlsx(Stream fileStream)
        {
            var result = new List<ImportTraineeViewModel>();
            var workBook = new XLWorkbook(fileStream);
            CultureInfo provider = CultureInfo.InvariantCulture;
            //double tempDouble = 0;
            foreach (var workSheet in workBook.Worksheets)
            {

                var firstDataCell = workSheet.Cell("B4");
                var lastDataCell = workSheet.LastCellUsed();
                var rangeData = workSheet.Range(firstDataCell.Address, lastDataCell.Address);

                for (int i = 1; i <= rangeData.RowCount(); i++)
                {
                    try
                    {
                        var row = rangeData.Row(i);
                        if (row.Cell(1).Value.ToString() != "")
                        {
                            var StaffInform = new ImportTraineeViewModel();
                            StaffInform.str_Staff_Id = row.Cell(9).Value.ToString().Trim();
                            StaffInform.str_BirthPlace = row.Cell(8).Value.ToString().Trim();
                            StaffInform.str_Fullname = row.Cell(1).Value.ToString().Trim();
                            StaffInform.Department = row.Cell(2).Value.ToString().Trim();
                            StaffInform.Job_Title = row.Cell(3).Value.ToString().Trim();
                            StaffInform.str_Station = row.Cell(4).Value.ToString().Trim();
                            StaffInform.str_Email = row.Cell(5).Value.ToString().Trim();
                            StaffInform.str_Cell_Phone = row.Cell(6).Value.ToString().Trim();
                            //if (Double.TryParse(row.Cell(7).Value.ToString().Trim(),  out tempDouble))
                            //{
                            //    StaffInform.dtm_Birthdate = DateTime.FromOADate(tempDouble);
                            //}
                            try
                            {
                                StaffInform.dtm_Birthdate = (DateTime)row.Cell(7).Value;
                            }
                            catch (Exception ex)
                            {
                                ex.ToString();
                            }

                            //if (db.Trainee.Where(m => m.str_Staff_Id == StaffInform.str_Staff_Id).Count() > 0)
                            //{
                            //    StaffInform.status += "Staff ID Duplicate; ";
                            //}
                            if (String.IsNullOrEmpty(StaffInform.str_Fullname))
                            {
                                StaffInform.status += "Fullname is empty; ";
                            }

                            if (!String.IsNullOrEmpty(StaffInform.str_Email) && !MailUtil.IsValidEmail(StaffInform.str_Email))
                            {
                                StaffInform.status += "Invalid email; ";
                            }

                            result.Add(StaffInform);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
                break;
            }
            return result;
        }


        public static List<ImportTraineeViewModel> ReadPartnerXlsx(Stream fileStream)
        {
            var result = new List<ImportTraineeViewModel>();
            var workBook = new XLWorkbook(fileStream);
            CultureInfo provider = CultureInfo.InvariantCulture;
            double tempDouble = 0;
            foreach (var workSheet in workBook.Worksheets)
            {

                var firstDataCell = workSheet.Cell("B4");
                var lastDataCell = workSheet.LastCellUsed();
                var rangeData = workSheet.Range(firstDataCell.Address, lastDataCell.Address);

                for (int i = 1; i <= rangeData.RowCount(); i++)
                {
                    try
                    {
                        var row = rangeData.Row(i);
                        if (row.Cell(1).Value.ToString() != "")
                        {
                            var StaffInform = new ImportTraineeViewModel();
                            StaffInform.str_Staff_Id = row.Cell(9).Value.ToString().Trim();
                            StaffInform.str_BirthPlace = row.Cell(8).Value.ToString().Trim();
                            StaffInform.str_Fullname = row.Cell(1).Value.ToString().Trim();
                            StaffInform.Company = row.Cell(2).Value.ToString().Trim();
                            StaffInform.Job_Title = row.Cell(3).Value.ToString().Trim();
                            StaffInform.str_Station = row.Cell(4).Value.ToString().Trim();
                            StaffInform.str_Email = row.Cell(5).Value.ToString().Trim();
                            StaffInform.str_Cell_Phone = row.Cell(6).Value.ToString().Trim();
                            if (Double.TryParse(row.Cell(7).Value.ToString().Trim(), out tempDouble))
                            {
                                StaffInform.dtm_Birthdate = DateTime.FromOADate(tempDouble);
                            }


                            //if (db.Trainee.Where(m => m.str_Staff_Id == StaffInform.str_Staff_Id).Count() > 0)
                            //{
                            //    StaffInform.status += "Staff ID Duplicate; ";
                            //}
                            if (String.IsNullOrEmpty(StaffInform.str_Fullname))
                            {
                                StaffInform.status += "Fullname is empty; ";
                            }

                            if (!String.IsNullOrEmpty(StaffInform.str_Email) && !MailUtil.IsValidEmail(StaffInform.str_Email))
                            {
                                StaffInform.status += "Invalid email; ";
                            }

                            result.Add(StaffInform);
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
                break;
            }
            return result;
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            try
            {
                return !source.Any();
            }
            catch (Exception)
            {
                return true;
            }
        }
        public static string Compare_Two_string(string After = "", string Before = "")
        {
            HtmlDiff diffHelper = new HtmlDiff(Before, After);
            return diffHelper.Build().ToString();
        }

    }
}