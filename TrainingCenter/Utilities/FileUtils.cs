using System;
using System.Web;
using System.IO;
using System.Web.Hosting;

namespace Utilities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using Excel;
    public static class FileUtils
    {

        public static FileInfo SaveFile(HttpPostedFileBase file, string filePath)
        {
            if (file != null)
            {
                string fileName = Path.GetFileName(file.FileName);
                fileName = checkFileName(fileName, filePath);
                if (!System.IO.Directory.Exists(filePath))
                    System.IO.Directory.CreateDirectory(filePath);
                var fullPath = Path.Combine(filePath, fileName);
                file.SaveAs(fullPath);
                FileInfo fi = new FileInfo(fullPath);
                return fi;
            }
            return null;
        }

        public static Boolean DeleteFile(string filePath)
        {
            File.Delete(filePath);
            return true;
        }

        private static string checkFileName(string fileName, string filePath)
        {

            while (filePath != "" && System.IO.File.Exists(Path.Combine(filePath, fileName)))
            {
                string name = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);
                int version;
                int _index = fileName.LastIndexOf("_");
                if (_index >= 0)
                {
                    if (int.TryParse(name.Substring(_index + 1, name.Length - _index - 1), out version))
                    {
                        version++;
                        fileName = name.Substring(0, _index) + "_" + version.ToString().PadLeft(3, '0') + extension;
                    }
                    else
                        fileName = name + "_001" + extension;
                }
                else
                    fileName = name + "_001" + extension;
            }
            return fileName;
        }

        //public static List<ImportUserViewModel> ReadProfileFromXlsx(Stream fileStream)
        //{
        //    TrainingCenterDBEntities db = new TrainingCenterDBEntities();
        //    var result = new List<ImportUserViewModel>();
        //    var workBook = new XLWorkbook(fileStream);

        //    foreach (var workSheet in workBook.Worksheets)
        //    {

        //        var firstDataCell = workSheet.Cell("A5");
        //        var lastDataCell = workSheet.LastCellUsed();
        //        var rangeData = workSheet.Range(firstDataCell.Address, lastDataCell.Address);

        //        for (int i = 1; i <= rangeData.RowCount(); i++)
        //        {
        //            var newUser = new ImportUserViewModel();
        //            newUser.user = new UserProfile();
        //            try
        //            {
        //                var row = rangeData.Row(i);
        //                DateTime tempDate=new DateTime();
        //                var tempString="";
        //                if (row.Cell(2).Value.ToString() != "")
        //                {

        //                    newUser.user.str_Email = row.Cell(2).Value.ToString();
        //                    newUser.user.UserName = row.Cell(2).Value.ToString();
        //                    newUser.user.str_File_No = row.Cell(1).Value.ToString();
        //                    newUser.user.str_Firstname = row.Cell(3).Value.ToString();
        //                    newUser.user.str_Lastname = row.Cell(4).Value.ToString();
        //                    newUser.user.str_Gender = row.Cell(5).Value.ToString();
        //                    newUser.user.str_Current_Title = row.Cell(6).Value.ToString();
        //                    if(DateTime.TryParseExact(row.Cell(7).Value.ToString(),"dd MMM yyyy",null,System.Globalization.DateTimeStyles.None,out tempDate))
        //                    {
        //                        newUser.user.dtm_Birthdate = tempDate;
        //                    }
        //                    newUser.user.str_Identity_Number = row.Cell(8).Value.ToString();
        //                    newUser.user.str_Address = row.Cell(9).Value.ToString();
        //                    newUser.user.str_Mobile_Phone = row.Cell(10).Value.ToString();
        //                    newUser.user.str_Home_Phone = row.Cell(11).Value.ToString();
        //                    if (DateTime.TryParseExact(row.Cell(12).Value.ToString(), "dd MMM yyyy", null, System.Globalization.DateTimeStyles.None, out tempDate))
        //                    {
        //                        newUser.user.dtm_Join_Date = tempDate;
        //                    }
        //                    tempString=row.Cell(13).Value.ToString();
        //                    newUser.user.int_Current_Department_Id = db.Department.Where(m => m.str_Short_Name == tempString).Select(m => m.Department_Id).FirstOrDefault();
        //                    newUser.user.str_Nationality = row.Cell(14).Value.ToString();
        //                    newUser.user.bit_Teacher = row.Cell(15).Value.ToString()=="Y";
        //                    newUser.status = "";

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                newUser.status = ex.Message;
        //            }
        //            result.Add(newUser);
        //        }

        //    }
        //    return result;
        //}


        //public static List<ImportQuestionModel> ReadQuestionFromXlsx(Stream fileStream)
        //{
        //    TrainingCenterDBEntities db = new TrainingCenterDBEntities();
        //    var result = new List<ImportQuestionModel>();
        //    var workBook = new XLWorkbook(fileStream);

        //    foreach (var workSheet in workBook.Worksheets)
        //    {

        //        var firstDataCell = workSheet.Cell("A2");
        //        var lastDataCell = workSheet.LastCellUsed();
        //        var rangeData = workSheet.Range(firstDataCell.Address, lastDataCell.Address);

        //        for (int i = 1; i <= rangeData.RowCount(); i++)
        //        {
        //            var newQuestion = new ImportQuestionModel();
        //            newQuestion.question = new Question();
        //            try
        //            {
        //                var row = rangeData.Row(i);

        //                if (row.Cell(1).Value.ToString() != "")
        //                {
        //                    newQuestion.question.str_Content = row.Cell(1).Value.ToString();
        //                    newQuestion.question.str_Type = "E";
        //                    for (int j = 2; j <= row.CellCount(); j=j+2)
        //                    {
        //                        if (!string.IsNullOrEmpty(row.Cell(j).Value.ToString()))
        //                        {
        //                            addOptionToQuestion(newQuestion.question, row.Cell(j).Value.ToString(), row.Cell(j + 1).Value.ToString());
        //                        }
        //                    }
        //                    newQuestion.status = "";

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                newQuestion.status = ex.Message;
        //            }
        //            result.Add(newQuestion);
        //        }

        //    }
        //    return result;
        //}

        //private static void addOptionToQuestion(Question question, string content, string answer)
        //{
        //    if(string.IsNullOrEmpty(question.str_Option_A_Content)){
        //        question.str_Option_A_Content=content;
        //        question.bit_Option_A_Answer=(answer=="1");
        //        question.str_Type = "S";
        //    }
        //    else if(string.IsNullOrEmpty(question.str_Option_B_Content)){
        //        question.str_Option_B_Content=content;
        //        question.bit_Option_B_Answer=(answer=="1");
        //        question.str_Type = "S";
        //    }
        //    else if (string.IsNullOrEmpty(question.str_Option_C_Content))
        //    {
        //        question.str_Option_C_Content = content;
        //        question.bit_Option_C_Answer = (answer == "1");
        //        question.str_Type = "S";
        //    }
        //    else if (string.IsNullOrEmpty(question.str_Option_D_Content))
        //    {
        //        question.str_Option_D_Content = content;
        //        question.bit_Option_D_Answer = (answer == "1");
        //        question.str_Type = "S";
        //    }
        //    else if (string.IsNullOrEmpty(question.str_Option_E_Content))
        //    {
        //        question.str_Option_E_Content = content;
        //        question.bit_Option_E_Answer = (answer == "1");
        //        question.str_Type = "S";
        //    }
        //    else if (string.IsNullOrEmpty(question.str_Option_F_Content))
        //    {
        //        question.str_Option_F_Content = content;
        //        question.bit_Option_F_Answer = (answer == "1");
        //        question.str_Type = "S";
        //    }
        //}

        public static string SaveFile(HttpPostedFileBase file, string folderName, string newName)
        {
            var filename = "";
            try
            {
                if (file != null)
                {
                    string filePath = HostingEnvironment.MapPath("~/" + folderName + "/");
                    //string fileName = Path.GetFileName(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    filename = newName + extension;
                    file.SaveAs(Path.Combine(filePath, filename));
                }
                return filename;
            }
            catch (Exception ex)
            {
                return filename;
            }
        }

        private static Dictionary<string, string> FormatPropertyName(IEnumerable<PropertyInfo> properties)
        {
            var listFields = new Dictionary<string, string>();
            foreach (var property in properties)
            {
                var varKey = property.GetCustomAttribute<DisplayAttribute>();
                listFields.Add((varKey != null ? varKey.GetName() : property.Name),property.Name);
            }
            return listFields;
        }

        public static List<T> ReadExcelToModel<T>(string filePath)
        {
            var properties = typeof (T).GetProperties();
            var colNames = FormatPropertyName(properties);
            var fileInfo = new FileInfo(filePath);
            var isXlsx = fileInfo.Extension.ToLower() == ".xlsx";
            using (var excelReader = isXlsx
                ? ExcelReaderFactory.CreateOpenXmlReader(File.Open(filePath, FileMode.Open, FileAccess.Read))
                : ExcelReaderFactory.CreateBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelTable = excelReader.AsDataSet().Tables[0];
                var excelCols = excelTable.Columns;
                if (colNames.Any(a => !excelCols.Contains(a.Key)))
                {
                    throw new Exception("Invalid template") {Data = { {"Code",510} }};
                }
                else
                {
                    var list = new List<T>();
                    foreach (DataRow row in excelTable.Rows)
                    {
                        var item = Activator.CreateInstance<T>();
                        var itemproperties = item.GetType();
                        for (var i = 0; i < colNames.Count; i++)
                        {
                            var cell = colNames.ElementAt(i);
                            var itemProperty = itemproperties.GetProperty(cell.Value);
                            var cellData = row[cell.Key];
                            if (itemProperty.PropertyType.IsArray)
                                cellData = cellData?.ToString().Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                            itemProperty.SetValue(item, cellData, null);
                        }
                        list.Add(item);
                    }
                    return list;
                }
            }
        }
    }
}