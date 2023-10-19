using System.Data;
using System.Linq;
using System.Web.Mvc;
using TMS.Core.Services.Approves;
using TrainingCenter.Utilities;
using DataTable = System.Data.DataTable;


namespace TrainingCenter.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using DAL.Entities;
    using TMS.Core.Services.Companies;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Jobtitle;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Subject;
    using TMS.Core.Services.Users;
    using TMS.Core.Services.TraineeHis;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.ViewModel;
    using global::Utilities;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.ViewModels.Common;
    using System.Configuration;
    using System.Globalization;
    using System.Threading.Tasks;

    public class ImportController : BaseAdminController
    {

        private const string REPORT_PDF = "PDF";
        private const string REPORT_EXCEL = "EXCELOPENXML";
        #region Intructor
        private readonly IJobtitleService _repoJobtitle;
        private readonly IDepartmentService _repoDepartment;
        private readonly ISubjectService _repoSubject;
        private readonly ICompanyService _repoCompany;
        private readonly ITraineeHistoryService _repoTranineeHis;
        private readonly IConfigService _configService;
        private readonly IApproveService _approveService;
        public ImportController(IApproveService approveService, IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IJobtitleService repoJobtitle, IDepartmentService repoDepartment, ISubjectService repoSubject, ICompanyService repoCompany, ITraineeHistoryService repoTraineeHis) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoJobtitle = repoJobtitle;
            _repoDepartment = repoDepartment;
            _repoSubject = repoSubject;
            _repoCompany = repoCompany;
            _repoTranineeHis = repoTraineeHis;
            _configService = configService;
            _approveService = approveService;

        }
        #endregion



        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Modify(FormCollection form)
        {
            string html = "";
            string txt_input = form["txt_input"].Trim();
            if (txt_input == "")
            {
                return Json(CMSUtils.alert("success", "vui long nhap"));
            }
            DataTable data = CMSUtils.GetQuery(txt_input);
            if (data.Rows.Count > 0)
            {
                string[] columnNames = data.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();
                html += "<table class=\"table table-striped table-bordered\" style=\"width:100%\">";


                html += "<tr>";
                foreach (var itemcol in columnNames)
                {
                    html += "<th>" + itemcol + "</th>";
                }
                html += "</tr>";
                foreach (DataRow itemrow in data.Rows)
                {
                    html += "<tr>";
                    foreach (var itemcol in columnNames)
                    {
                        html += "<td>" + itemrow[itemcol] + "</td>";
                    }
                    html += "</tr>";
                }
                html += "</table>";
            }


            return Json(html);
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        #region[import User code cũ]


        //public JsonResult InsertEmployees(ImportEmplyeeModel model)
        //{
        //    //var currentUser = GetUser();
        //    model.ErrorFlag = "";
        //    model.ErrorNote = "";
        //    bool isUpdate = false;
        //    Trainee employee = new Trainee();
        //    TraineeHistory employeehistory = new TraineeHistory();

        //    //var countEID = EmployeeService.Get(m => m.str_Staff_Id == model.EID).Count();
        //    //if (countEID > 0)
        //    //{
        //    //    model.ErrorFlag = "1";
        //    //    model.ErrorNote += "<font color='red'>- EID is exist in system ! </br></font>";
        //    //}
        //    if (string.IsNullOrEmpty(model.EID))
        //    {
        //        model.ErrorFlag = "1";
        //        model.ErrorNote = "<font color='red'>EID can not be emty!  </br> </font>";
        //    }

        //    var checkid = EmployeeService.Get(a => a.str_Staff_Id == model.EID.Trim());
        //    if (checkid.Any())
        //    {
        //        employee = EmployeeService.Get(a => a.str_Staff_Id == model.EID.Trim()).FirstOrDefault();
        //        isUpdate = true;
        //        //model.ErrorFlag = "1";
        //    }

        //    else if (string.IsNullOrEmpty(model.FULL_NAME))
        //    {
        //        model.ErrorFlag = "1";
        //        model.ErrorNote += "<font color='red'>Full Name can not be null! </br></font>";
        //    }
        //    employee.str_Staff_Id = model.EID;
        //    employee.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
        //    if (!string.IsNullOrEmpty(model.PLACE_OF_BIRTH))
        //    {
        //        var nation = _configService.GetProvinceByName(model.PLACE_OF_BIRTH.Trim().ToLower());
        //        employee.str_Place_Of_Birth = nation?.name;
        //    }
        //    //employee.str_Place_Of_Birth = model.PLACE_OF_BIRTH ?? "";
        //    model.DATE_OF_BIRTH = model.DATE_OF_BIRTH ?? "";
        //    if (model.DATE_OF_BIRTH != "")
        //    {
        //        employee.dtm_Birthdate = DateUtil.StringToDate(model.DATE_OF_BIRTH, DateUtil.DATE_FORMAT_OUTPUT);
        //    }
        //    // var firstName = model.FULL_NAME.Split(' ')[0];
        //    //  var lastName = model.FULL_NAME.Substring(model.FULL_NAME.Split(' ')[0].Length);

        //    var checkBOM = model.FULL_NAME.Replace('\u00A0', ' ');
        //    var firstName = checkBOM.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim();
        //    var lastName = model.FULL_NAME.Replace(firstName, "").Trim();


        //    employee.FirstName = firstName;
        //    employee.LastName = lastName;

        //    employee.Passport = model.ID_PASSPORT_NO ?? "";
        //    if (!string.IsNullOrEmpty(model.TYPE))
        //    {
        //        employee.bit_Internal = model.TYPE.Trim().ToLower() == "internal";
        //        if (employee.bit_Internal)
        //        {
        //            if (string.IsNullOrEmpty(model.JOB_TITLE))
        //            {
        //                model.ErrorFlag = "1";
        //                model.ErrorNote += "<font color='red'>Jobtitle can not be null! </br></font>";
        //            }
        //            else
        //            {
        //                var jobTiltle = _repoJobtitle.Get(a => a.Code.Trim() == model.JOB_TITLE_CODE.Trim()).FirstOrDefault();
        //                if (jobTiltle != null)
        //                {
        //                    employee.Job_Title_id = jobTiltle.Id;
        //                    employeehistory.Job_Title_Id = jobTiltle.Id;
        //                }
        //                else
        //                {
        //                    model.ErrorFlag = "1";
        //                    model.ErrorNote += "<font color='red'>Jobtitle is not exist in system! </br></font>";
        //                }
        //            }
        //            if (string.IsNullOrEmpty(model.DEPARTMENT))
        //            {
        //                model.ErrorFlag = "1";
        //                model.ErrorNote += "<font color='red'>Department can not be null! </br></font>";
        //            }
        //            else
        //            {
        //                var department = _repoDepartment.Get(a => a.Code.Trim() == model.DEPARTMENT_CODE.Trim()).FirstOrDefault();
        //                if (department != null)
        //                {
        //                    employee.Department_Id = department.Id;
        //                }
        //                else
        //                {
        //                    model.ErrorFlag = "1";
        //                    model.ErrorNote += "<font color='red'>Department is not exist in system! </br></font>";
        //                }
        //            }
        //        }
        //        else if (!employee.bit_Internal)
        //        {
        //            if (!string.IsNullOrEmpty(model.JOB_TITLE))
        //            {
        //                var jobTiltle = _repoJobtitle.Get(a => a.Code.Trim() == model.JOB_TITLE_CODE.Trim()).FirstOrDefault();
        //                if (jobTiltle != null)
        //                {
        //                    employee.Job_Title_id = jobTiltle.Id;
        //                    employeehistory.Job_Title_Id = jobTiltle.Id;
        //                    //var jobTiltlelog = _repoTranineeHis.Get(a => a.Job_Title_Id == jobTiltle.Id).FirstOrDefault();
        //                    //if (jobTiltlelog != null)
        //                    //{
        //                    //    employeehistory.Job_Title_Id = jobTiltlelog.Id;
        //                    //}
        //                    //else
        //                    //{
        //                    //    isUpdate = true;
        //                    //    model.ErrorNote += "<font color='red'>- LogJobtitle is exist in system! </br></font>";

        //                    //}
        //                }
        //                else
        //                {
        //                    model.ErrorFlag = "1";
        //                    model.ErrorNote += "<font color='red'>Jobtitle is not exist in system! </br></font>";
        //                }
        //            }
        //            if (!string.IsNullOrEmpty(model.DEPARTMENT))
        //            {
        //                var department = _repoDepartment.Get(a => a.Code == model.DEPARTMENT_CODE.Trim()).FirstOrDefault();
        //                if (department != null)
        //                {
        //                    employee.Department_Id = department.Id;
        //                }
        //                else
        //                {
        //                    model.ErrorFlag = "1";
        //                    model.ErrorNote += "<font color='red'>Department is not exist in system! </br></font>";
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        model.ErrorFlag = "1";
        //        model.ErrorNote += "<font color='red'>Full Name can not be null! </br></font>";
        //    }
        //    //if (!string.IsNullOrEmpty(model.DEPARTMENT))
        //    //{
        //    //    var department = _repoDepartment.Get(a => a.Code == model.DEPARTMENT.Trim()).FirstOrDefault();
        //    //    employee.Department_Id = department?.Id;
        //    //}
        //    if (!string.IsNullOrEmpty(model.COMPANY))
        //    {
        //        var partner = _repoCompany.GetByCode(model.COMPANY.Trim().ToLower());
        //        employee.Company_Id = partner?.Company_Id;
        //    }

        //    model.JOB_TITLE = model.JOB_TITLE ?? "";


        //    if (!string.IsNullOrEmpty(model.NATIONALITY))
        //    {
        //        var nation = _configService.GetNationByCode(model.NATIONALITY.Trim().ToLower());
        //        employee.Nation = nation?.Nation_Code;
        //    }
        //    //employee.Nation = model.NATIONALITY ?? "";

        //    if (!string.IsNullOrEmpty(model.GENDER) && (model.GENDER.Trim().ToLower() == "m" || model.GENDER.Trim().ToLower() == "f"))
        //    {
        //        employee.Gender = model.GENDER.Trim().ToLower() == "m" ? 1 : 2;
        //    }
        //    var countEmail = EmployeeService.Get(m => m.str_Email == model.EMAIL).Count();
        //    if (countEmail > 0)
        //    {
        //        model.ErrorFlag = "1";
        //        model.ErrorNote += "<font color='red'>Email is exist in system ! </br></font>";
        //    }
        //    if (!string.IsNullOrEmpty(model.EMAIL))
        //    {
        //        if (IsValidEmail(model.EMAIL))
        //        {
        //            employee.str_Email = model.EMAIL;
        //        }
        //        else
        //        {
        //            model.ErrorFlag = "1";
        //            model.ErrorNote += "<font color='red'>Please enter correct the email type! </br></font>";
        //        }

        //    }
        //    else
        //    {
        //        employee.str_Email = employee.str_Staff_Id + "@" + employee.str_Staff_Id + ".com";
        //    }
        //    employee.str_Cell_Phone = model.PHONE ?? "";
        //    //if (!string.IsNullOrEmpty(model.TRAINING_ALLOWANCE))
        //    //{
        //    //    employee.Allowance = int.Parse(model.TRAINING_ALLOWANCE);
        //    //}
        //    model.DATE_OF_JOIN = model.DATE_OF_JOIN ?? "";

        //    if (!string.IsNullOrEmpty(model.DATE_OF_JOIN))
        //    {
        //        employee.dtm_Join_Date = DateUtil.StringToDate(model.DATE_OF_JOIN, DateUtil.DATE_FORMAT_OUTPUT);
        //    }
        //    if (!string.IsNullOrEmpty(model.IS_INSTRUCTOR) && model.IS_INSTRUCTOR.ToLower().Trim() == "false")
        //    {
        //        employee.int_Role = (int)UtilConstants.ROLE.Trainee;
        //    }
        //    else if (!string.IsNullOrEmpty(model.IS_INSTRUCTOR) && model.IS_INSTRUCTOR.ToLower().Trim() == "true")
        //    {
        //        employee.int_Role = (int)UtilConstants.ROLE.Instructor;
        //    }
        //    if (model.ErrorFlag == "1")
        //    {
        //        return Json(model, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        //var traineetocallservices = EmployeeService.Get(a => a.Id == employee.Id);
        //        if (employee.int_Role == (int)UtilConstants.ROLE.Instructor)
        //        {
        //            var contract = new Trainee_Contract();
        //            if (!string.IsNullOrEmpty(model.CONTRACT_NO) && !string.IsNullOrEmpty(model.CONTRACT_EXPIRE_DATE))
        //            {
        //                contract.expire_date = DateUtil.StringToDate(model.CONTRACT_EXPIRE_DATE,
        //                    DateUtil.DATE_FORMAT_OUTPUT);
        //                contract.contractno = model.CONTRACT_NO ?? "";
        //                contract.description = model.CONTRACT_DESCRIPTION ?? "";
        //                employee.Trainee_Contract.Add(contract);
        //            }

        //        }
        //        if (!string.IsNullOrEmpty(model.COURSE))
        //        {
        //            var traineeEdu = new Trainee_Record();
        //            traineeEdu.str_Subject = model.COURSE;
        //            if (!string.IsNullOrEmpty(model.EDUCATION_FROM))
        //            {
        //                traineeEdu.dtm_time_from = DateUtil.StringToDate(model.EDUCATION_FROM,
        //                    DateUtil.DATE_FORMAT_OUTPUT);
        //            }
        //            if (!string.IsNullOrEmpty(model.EDUCATION_TO))
        //            {
        //                traineeEdu.dtm_time_to = DateUtil.StringToDate(model.EDUCATION_TO,
        //                    DateUtil.DATE_FORMAT_OUTPUT);
        //            }
        //            traineeEdu.str_organization = model.ORGANIZATION ?? "";
        //            traineeEdu.str_note = model.NOTE ?? "";
        //            employee.Trainee_Record.Add(traineeEdu);
        //        }


        //        if (!string.IsNullOrEmpty(model.EX_EDUCATION_COURSE))
        //        {
        //            var traineeEdu1 = new Trainee_Record();
        //            traineeEdu1.str_Subject = model.EX_EDUCATION_COURSE ?? "";
        //            if (!string.IsNullOrEmpty(model.EX_EDUCATION_FROM))
        //            {
        //                traineeEdu1.dtm_time_from = DateUtil.StringToDate(model.EX_EDUCATION_FROM,
        //                    DateUtil.DATE_FORMAT_OUTPUT);
        //            }
        //            if (!string.IsNullOrEmpty(model.EX_EDUCATION_TO))
        //            {
        //                traineeEdu1.dtm_time_to = DateUtil.StringToDate(model.EX_EDUCATION_TO,
        //                    DateUtil.DATE_FORMAT_OUTPUT);
        //            }
        //            traineeEdu1.str_organization = model.EX_EDUCATION_ORGANIZATION ?? "";
        //            traineeEdu1.str_note = model.NOTE ?? "";
        //            employee.Trainee_Record.Add(traineeEdu1);
        //        }
        //        employeehistory.dtm_Create_At = DateTime.Now;
        //        if (isUpdate)
        //        {

        //            if (model.FlagImport != "1")
        //            {
        //                EmployeeService.Update(employee);
        //                model.ErrorNote += "<font color='red'>Update</br></font>";
        //                var check = EmployeeService.Get(a => a.str_Staff_Id == model.EID).FirstOrDefault();
        //                employeehistory.Trainee_Id = check.Id;
        //                EmployeeService.Insert1(employeehistory);
        //                model.ErrorNote += "<font color='red'>CreateLog</br></font>";
        //            }
        //        }
        //        else
        //        {
        //            if (model.FlagImport != "1")
        //            {
        //                var randomPass = Common.RandomCharecter();
        //                employee.IsActive = true;
        //                employee.IsDeleted = false;
        //                employee.dtm_Created_At = DateTime.Now;
        //                employee.Password = Common.EncryptString(randomPass);
        //                EmployeeService.Insert(employee);
        //                model.ErrorNote += "<font color='red'>Create</br></font>";
        //                var check = EmployeeService.Get();
        //                if (check.Any())
        //                {
        //                    var checkEmployee = check.OrderByDescending(a => a.Id).First();
        //                    employeehistory.Trainee_Id = checkEmployee.Id;
        //                    EmployeeService.Insert1(employeehistory);

        //                    #region [-----------------Sent Mail Trainee------------------]
        //                    var checksite = GetByKey(UtilConstants.KEY_SENT_EMAIL_CHANGE_PASSWORD);
        //                    if (checksite.Equals("1"))
        //                    {
        //                        Sent_Email_TMS(null, checkEmployee, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee);
        //                    }
        //                    #endregion
        //                }
        //                model.ErrorNote += "<font color='red'>CreateLog</br></font>";
        //            }
        //        }
        //        if (model.ErrorFlag != "1")
        //        {
        //            model.ErrorNote += "<font color='red'>No Error</br></font>";
        //        }
        //        return Json(model, JsonRequestBehavior.AllowGet);
        //    }
        //}
        //public ActionResult GetTraineeTemplate()
        //{
        //    return View();
        //}
        #endregion
        [Custom("Ajax")]
        public ActionResult GetTraineeTemplate()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var sheet1Name = "Job_title";
            var sheet2Name = "Department";
            var sheet3Name = "Company";
            var sheet4Name = "Nationaly";
            var sheet5Name = "Place_Of_Birth";
            var sheet6Name = "Course";
            var startRowIndex = 3;
            var startColIndex = 1;
            var filePathTemplate = Server.MapPath(@"~/Template/TMS-Intructor-DHNN.xlsx");
            try
            {
                using (var excel = new ExcelPackage(new FileInfo(filePathTemplate)))
                {
                    var sheetJobdepartment = excel.Workbook.Worksheets[sheet1Name];
                    var sheetdepartment = excel.Workbook.Worksheets[sheet2Name];
                    var sheetCompany = excel.Workbook.Worksheets[sheet3Name];
                    var sheetNationaly = excel.Workbook.Worksheets[sheet4Name];
                    var sheetPlaceofbirth = excel.Workbook.Worksheets[sheet5Name];
                    var sheetCourse = excel.Workbook.Worksheets[sheet6Name];

                    var departments = _repoDepartment.Get(a => a.IsActive == true).OrderBy(a => a.Ancestor.Trim()).ThenBy(a => a.Code.Trim());
                    var jobtitles = _repoJobtitle.Get(a => a.IsActive == true);
                    var companys = _repoCompany.Get(a => a.IsActive == true);
                    var nationalys = _configService.GetNations();
                    var placeofbirths = ConfigService.GetProvince();
                    var _course = _repoSubject.GetSubjectDetail(a => a.IsActive == true)
                        .OrderBy(a => a.Code);

                    var rowIndex = startRowIndex;
                    var jobRows = startRowIndex;
                    var comRows = startRowIndex;
                    var natRows = startRowIndex;
                    var placeRows = startRowIndex;
                    var courseRows = startRowIndex;
                    if (departments.Any())
                    {
                        foreach (var department in departments)
                        {

                            sheetdepartment.Cells[rowIndex, startColIndex].Value = department.Name.Replace(" ", string.Empty);
                            sheetdepartment.Cells[rowIndex, startColIndex + 1].Value = department.Code;
                            sheetdepartment.Cells[rowIndex, startColIndex + 2].Value = department.Name;
                            rowIndex++;
                        }
                        var departmentCells = sheetdepartment.Cells[startRowIndex, startColIndex, rowIndex - 1, startColIndex];
                        departmentCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    if (jobtitles.Any())
                    {
                        foreach (var job in jobtitles)
                        {
                            sheetJobdepartment.Cells[jobRows, startColIndex].Value = job.Name.Replace(" ", string.Empty);
                            sheetJobdepartment.Cells[jobRows, startColIndex + 1].Value = job.Code;
                            sheetJobdepartment.Cells[jobRows, startColIndex + 2].Value = job.Name;
                            jobRows++;
                        }
                        var jobdepartmentCells = sheetJobdepartment.Cells[startRowIndex, startColIndex, jobRows - 1, startColIndex + 2];
                        jobdepartmentCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    if (companys.Any())
                    {
                        foreach (var company in companys)
                        {
                            sheetCompany.Cells[comRows, startColIndex].Value = company.str_code;
                            sheetCompany.Cells[comRows, startColIndex + 1].Value = company.str_Name;
                            comRows++;
                        }
                        var companyCells = sheetCompany.Cells[startRowIndex, startColIndex, comRows - 1, startColIndex + 2];
                        companyCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    if (nationalys.Any())
                    {
                        foreach (var nationaly in nationalys)
                        {
                            sheetNationaly.Cells[natRows, startColIndex].Value = nationaly.Nation_Code;
                            sheetNationaly.Cells[natRows, startColIndex + 1].Value = nationaly.Nation_Name;

                            natRows++;
                        }
                        var nationalymentCells = sheetNationaly.Cells[startRowIndex, startColIndex, natRows - 1, startColIndex + 1];
                        nationalymentCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    if (placeofbirths.Any())
                    {
                        foreach (var placeofbirth in placeofbirths)
                        {
                            sheetPlaceofbirth.Cells[placeRows, startColIndex].Value = placeofbirth.Value;
                            placeRows++;
                        }
                        var placeofbirthCells = sheetPlaceofbirth.Cells[startRowIndex, startColIndex, placeRows - 1, startColIndex];
                        placeofbirthCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    if (_course.Any())
                    {
                        foreach (var item in _course)
                        {
                            sheetCourse.Cells[courseRows, startColIndex].Value = item.Code;
                            sheetCourse.Cells[courseRows, startColIndex + 1].Value = item.Name;
                            courseRows++;
                        }
                        var courseCells = sheetCourse.Cells[startRowIndex, startColIndex, courseRows - 1, startColIndex + 1];
                        courseCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }


                    // var departmentNameCells = sheetJobdepartment.Cells[startRowIndex, startColIndex, jobRows - 1, startColIndex + 1];



                    var streamBytes = new MemoryStream(excel.GetAsByteArray());
                    return File(streamBytes, excel.File.Extension, "TMS_Employee" + "_" + DateTime.Now.ToString("ddMMyyyy") + excel.File.Extension);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string[] DepartmentCols = new[] { "Code", "Name", "IsTraining", "Parent" };
        private string[] AssignTraineeCols = new[] { "EID", "Full Name" };
        private string[] CompanyCols = new[] { "Code", "Name", "Description" };
        private string[] JobtitleCols = new[] { "Code", "Name", "Department Code", "Description", "Subject Codes" };
        private string[] ValidValue = new[] { "1", "true" };
        private string SaveFile(HttpPostedFileWrapper postedFile)
        {
            var folder = Server.MapPath("/imports");
            if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
            var file = folder + "/" + postedFile.FileName + DateTime.Now.ToString("yyyyMMddhhmmss") + ".xlsx";
            postedFile.SaveAs(file);
            return file;
        }

        public ActionResult ImportDepartments()
        {
            return View();
        }
        [HttpPost]

        public ActionResult ImportDepartments(HttpPostedFileWrapper postedFile)
        {
            try
            {
                using (var readingFile = new ExcelPackage(postedFile.InputStream))
                {
                    var worksheet = readingFile.Workbook.Worksheets.First();
                    var exceCols = worksheet.Tables[0].Columns.Select(a => a.Name).Where(a => !string.IsNullOrEmpty(a));
                    if (!exceCols.Any(a => DepartmentCols.Contains(a))) //todo: valid template
                        return null;
                    var listDepartments = new Dictionary<string, DepartmentImportViewModel>();
                    for (var i = 2; i <= worksheet.Tables[0].Address.Rows; i++)
                    {
                        var code = worksheet.Cells[i, 1].Text.Trim();
                        if (!string.IsNullOrEmpty(code))
                        {
                            var cellIsTraining = worksheet.Cells[i, 3].Text;
                            var isTraining = ValidValue.Contains(cellIsTraining.ToLower());
                            code = code.ToUpper();
                            if (!listDepartments.ContainsKey(code))
                            {
                                listDepartments.Add(worksheet.Cells[i, 1].Text, new DepartmentImportViewModel()
                                {
                                    Code = code,
                                    IsTraining = isTraining,
                                    Name = worksheet.Cells[i, 2].Text,
                                    ParentCode = worksheet.Cells[i, 4].Text,
                                    Description = worksheet.Cells[i, 5].Text,
                                });
                            }
                            else
                            {
                                listDepartments[code].Code = worksheet.Cells[i, 1].Text;
                                listDepartments[code].IsTraining = isTraining;
                                listDepartments[code].Name = worksheet.Cells[i, 2].Text;
                                listDepartments[code].ParentCode = worksheet.Cells[i, 4].Text;
                                listDepartments[code].Description = worksheet.Cells[i, 5].Text;
                            }
                        }
                    }
                    if (listDepartments.Any())
                    {
                        _repoDepartment.Import(listDepartments);
                        return RedirectToAction("Index", "Department");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ActionResult ImportAssignTrainees()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ImportAssignTrainees(HttpPostedFileBase postedFile, int? courrseId, int submitType)
        {
            try
            {
                postedFile = Request.Files["postedFile"];
                //int hd_courseID = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"]);

                if (courrseId.HasValue)
                {

                    using (var readingFile = new ExcelPackage(postedFile.InputStream))
                    {
                        var course = CourseService.GetById((int)courrseId);
                        var courseDetail = course.Course_Detail.Where(a => a.IsDeleted != true);
                        var trainee = EmployeeService.Get(a=>a.IsActive == true ,true);
                        var worksheet = readingFile.Workbook.Worksheets.First();
                        var exceCols = worksheet.Tables[0].Columns.Select(a => a.Name).Where(a => !string.IsNullOrEmpty(a));
                        if (!exceCols.Any(a => AssignTraineeCols.Contains(a)))
                        {//todo: valid template
                            return Json(new AjaxResponseViewModel { message = Messege.VALIDATION_FILE, result = false }, JsonRequestBehavior.AllowGet);
                        }
                        var listDepartments = new Dictionary<string, AssignTraineeImportModel>();
                        List<string> list_code = new List<string>();
                        for (var j = 2; j <= worksheet.Tables[0].Address.Rows; j++)
                        {
                            var code = worksheet.Cells[j, 1].Text.Trim();

                            if (!string.IsNullOrEmpty(code))
                            {
                                var traineeID = trainee.FirstOrDefault(a => a.str_Staff_Id == code);
                                if (traineeID != null)
                                {
                                    //var fullname = worksheet.Cells[j, 2].Text;
                                    code = code.ToUpper();
                                    if (!listDepartments.ContainsKey(code))
                                    {
                                        listDepartments.Add(code, new AssignTraineeImportModel()
                                        {
                                            TraineeID = traineeID.Id,
                                            EID = code,
                                            Remark = worksheet.Cells[j, 2].Text,
                                            //FullName = fullname,
                                        });
                                    }
                                    else
                                    {
                                        listDepartments[code].EID = code;
                                        listDepartments[code].TraineeID = traineeID.Id;
                                        listDepartments[code].Remark = worksheet.Cells[j, 2].Text;
                                        //listDepartments[code].FullName = worksheet.Cells[j, 2].Text;
                                    }
                                }
                                else
                                {
                                    list_code.Add(code);
                                    //return Json(new AjaxResponseViewModel { message = code + " " + Messege.VALIDATION_IMPORT_ASSIGN, result = false }, JsonRequestBehavior.AllowGet);
                                }
                            }
                        }
                        foreach (var item in courseDetail)
                        {
                            if (listDepartments.Any())
                            {
                                var entity_ = item.TMS_Course_Member;
                                foreach (var list in listDepartments)
                                {
                                    var id = list.Value.TraineeID;
                                    var remark = list.Value.Remark;
                                    var entity = entity_.FirstOrDefault(a => a.Member_Id == id && a.Course_Details_Id == item.Id);
                                    if (entity == null)
                                    {
                                        Dictionary<string, object> dic = new Dictionary<string, object>();
                                        dic.Add("Member_Id", id);
                                        dic.Add("Course_Details_Id", item.Id);
                                        dic.Add("IsDelete", 0);
                                        dic.Add("IsActive", 1);
                                        dic.Add("AssignBy", CurrentUser.USER_ID + "");
                                        dic.Add("Status", 1); // chưa gửi request phê duyệt
                                        dic.Add("LmsStatus", 99);// chưa approval
                                        if (CMSUtils.InsertDataSQLNoLog("TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                        {

                                        }
                                    }
                                    else
                                    {
                                        Dictionary<string, object> dic = new Dictionary<string, object>();
                                        dic.Add("IsDelete", 0);
                                        dic.Add("IsActive", 1);
                                        dic.Add("AssignBy", CurrentUser.USER_ID + "");
                                        dic.Add("Status", 1); // chưa gửi request phê duyệt
                                        dic.Add("LmsStatus", 99);// chưa approval
                                        if (CMSUtils.UpdateDataSQLNoLog("Id", entity.Id + "", "TMS_Course_Member", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                        {

                                        }
                                    }
                                    if (!string.IsNullOrEmpty(remark))
                                    {
                                        TMS_Course_Member_Remark entity_remark = CourseMemberService.GetRemark(a => a.TraineeId == id && a.CourseId == course.Id).FirstOrDefault();
                                        if (entity_remark == null)
                                        {
                                            Dictionary<string, object> dic = new Dictionary<string, object>();
                                            dic.Add("TraineeId", id);
                                            dic.Add("CourseId", course.Id);
                                            dic.Add("remark", remark);
                                            if (CMSUtils.InsertDataSQLNoLog("TMS_Course_Member_Remark", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                            {

                                            }
                                        }
                                        else
                                        {
                                            Dictionary<string, object> dic = new Dictionary<string, object>();
                                            dic.Add("remark", remark);
                                            if (CMSUtils.UpdateDataSQLNoLog("Id", entity_remark.Id + "", "TMS_Course_Member_Remark", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                                            {

                                            }
                                        }
                                    }
                                    //var removeTrainee = new TMS_Course_Member() { Member_Id = id, Remark = remark};
                                    //CourseService.Insert(removeTrainee, -1, (int)courrseId, UtilConstants.ApproveType.AssignedTrainee, 1,0);
                                }
                            }
                        }


                        if (course != null)
                        {
                            var maxtrainee = course.NumberOfTrainee.HasValue ? course.NumberOfTrainee : 0;
                            var courseName = course.Code.Trim() + " - " + course.Name.Trim();
                            var result = string.Empty;
                            var data = string.Empty;
                            
                            if (courseDetail.Any())
                            {
                                if (list_code.Count() > 0)
                                {
                                    result += "List Trainee assign fail: " + string.Join(",",list_code);
                                }
                                result += "<ul>";
                                foreach (var item in courseDetail.Where(a => a.IsDeleted == false))
                                {
                                    var count = item.TMS_Course_Member.Count(a => a.IsDelete == false);
                                    result += "<li>" + item.SubjectDetail.Name + "&nbsp;:&nbsp;<font " + (count > maxtrainee ? "color='red'" : "") + ">" + count + "&nbsp;</font>/&nbsp;" + maxtrainee + "</li>";
                                }
                                result += "</ul>";
                                var countResult = course.Course_Result_Final.Count(a => a.IsDeleted == false);
                                data += "&nbsp;:&nbsp;<font " + (countResult > maxtrainee ? "color='red'" : "") + " >&nbsp;" + countResult + "&nbsp;</font>/&nbsp;" + maxtrainee;
                            }
                            return Json(new AjaxResponseViewModel()
                            {
                                message = string.Format(Messege.ALERT_MAXTRAINEE, courseName, result),
                                result = true
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    //InsertOrUpdateStatusApi((int)courrseId, UtilConstants.ApiStatus.Modify, UtilConstants.LMSStatus.AssignTrainee);                   
                }
                return Json(new AjaxResponseViewModel { message = Messege.UNSUCCESS_APPROVAL_ASSIGN_TRAINEE, result = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ImportCompany()
        {
            return View();
        }
        [HttpPost]

        public ActionResult ImportCompany(HttpPostedFileWrapper postedFile)
        {
            try
            {
                using (var readingFile = new ExcelPackage(postedFile.InputStream))
                {
                    var worksheet = readingFile.Workbook.Worksheets.First();
                    var exceCols = worksheet.Tables[0].Columns.Select(a => a.Name).Where(a => !string.IsNullOrEmpty(a));
                    if (!exceCols.Any(a => CompanyCols.Contains(a))) //todo: valid template
                        return null;
                    var listDepartments = new Dictionary<string, Company>();
                    for (var i = 2; i <= worksheet.Tables[0].Address.Rows; i++)
                    {
                        var code = worksheet.Cells[i, 1].Text.Trim();
                        if (!string.IsNullOrEmpty(code))
                        {
                            code = code.ToUpper();
                            if (!listDepartments.ContainsKey(code))
                            {
                                listDepartments.Add(worksheet.Cells[i, 1].Text, new Company()
                                {
                                    str_code = code,
                                    str_Name = worksheet.Cells[i, 2].Text,
                                    dicsription = worksheet.Cells[i, 3].Text,
                                });
                            }
                            else
                            {
                                listDepartments[code].str_Name = worksheet.Cells[i, 2].Text;
                                listDepartments[code].dicsription = worksheet.Cells[i, 3].Text;
                            }
                        }
                    }
                    if (listDepartments.Any())
                    {
                        var now = DateTime.Now;
                        foreach (var dept in listDepartments)
                        {
                            var dateKey = dept.Key;
                            var data = dept.Value;
                            var Company = _repoCompany.GetByCode(dateKey);
                            if (Company == null)
                            {
                                Company = new Company()
                                {
                                    str_Name = data.str_Name,
                                    str_code = data.str_code,
                                    bit_Deleted = false,
                                    dicsription = data.dicsription,
                                    dtm_Created_At = now
                                };
                                _repoCompany.Insert(Company);
                            }
                            else
                            {
                                Company.str_Name = data.str_Name;
                                Company.dtm_Modified_At = now;
                                Company.dicsription = data.dicsription;
                                _repoCompany.Update(Company);
                            }
                        }
                    }

                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ActionResult ImportJobtitle()
        {
            return View();
        }
        [HttpPost]

        public ActionResult ImportJobtitle(HttpPostedFileWrapper postedFile)
        {
            try
            {
                using (var readingFile = new ExcelPackage(postedFile.InputStream))
                {
                    var worksheet = readingFile.Workbook.Worksheets.First();
                    var exceCols = worksheet.Tables[0].Columns.Select(a => a.Name).Where(a => !string.IsNullOrEmpty(a));
                    if (!exceCols.Any(a => JobtitleCols.Contains(a))) //todo: valid template
                        return null;
                    //var fileLocation = SaveFile(postedFile);
                    //var readngData = FileUtils.ReadExcelToModel<JobTitleImportViewModel>(fileLocation);
                    var listDepartments = new Dictionary<string, JobTitleImportViewModel>();
                    for (var i = 2; i <= worksheet.Tables[0].Address.Rows; i++)
                    {
                        var code = worksheet.Cells[i, 1].Text.Trim();
                        if (!string.IsNullOrEmpty(code))
                        {
                            code = code.ToUpper();
                            var subjectCode = worksheet.Cells[i, 5].Value;
                            var codes = !string.IsNullOrWhiteSpace(subjectCode?.ToString())
                                ? subjectCode.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : null;
                            if (!listDepartments.ContainsKey(code))
                            {
                                //"Code", "Name", "Department Code","Description","Subject Codes"
                                listDepartments.Add(worksheet.Cells[i, 1].Text, new JobTitleImportViewModel()
                                {
                                    Code = code,
                                    Name = worksheet.Cells[i, 2].Text,
                                    Description = worksheet.Cells[i, 4].Text,
                                    DepartmentCode = worksheet.Cells[i, 3].Text,
                                    SubjectCodes = codes,
                                });
                            }
                            else
                            {
                                listDepartments[code].Code = code;
                                listDepartments[code].Name = worksheet.Cells[i, 2].Text;
                                listDepartments[code].Description = worksheet.Cells[i, 2].Text;
                                listDepartments[code].DepartmentCode = worksheet.Cells[i, 4].Text;
                                listDepartments[code].SubjectCodes = codes;
                            }
                        }
                    }
                    if (listDepartments.Any())
                    {
                        var now = DateTime.Now;
                        foreach (var dept in listDepartments)
                        {
                            var dateKey = dept.Key;
                            var data = dept.Value;
                            var jobtitle = _repoJobtitle.Get(a => a.Code.Equals(dateKey)).FirstOrDefault();
                            List<SubjectDetail> subjectCodes = null;
                            var subCodes = dept.Value.SubjectCodes?.Select(a => a.Trim().ToUpper()).ToList();
                            var deptCode = _repoDepartment.Get(a => a.Code == dept.Value.DepartmentCode).FirstOrDefault();
                            subjectCodes = subCodes != null && subCodes.Any() ? _repoSubject.GetSubjectDetail(a => a.IsActive == true && subCodes.Contains(a.Code)).ToList() : null;
                            if (jobtitle == null)
                            {
                                jobtitle = new JobTitle()
                                {
                                    Name = data.Name,
                                    Code = data.Code,
                                    IsDelete = false,
                                    Description = data.Description,
                                    CreatedDate = now,
                                };
                                if (subjectCodes != null && subjectCodes.Any())
                                {
                                    var subjectStandard = subjectCodes.Select(a => new Title_Standard()
                                    {
                                        SubjectDetail = a,
                                        JobTitle = jobtitle
                                    }).ToList();
                                    subjectStandard.ForEach(a => _repoJobtitle.UpdateTitleStandard(a));
                                }
                                _repoJobtitle.Insert(jobtitle);
                            }
                            else
                            {
                                jobtitle.Name = data.Name;
                                jobtitle.Code = data.Code;
                                jobtitle.UpdatedDate = now;
                                jobtitle.UpdatedBy = CurrentUser.Username;
                                jobtitle.Description = data.Description;
                                if (subjectCodes != null && subjectCodes.Any())
                                {
                                    var jobStandards = _repoJobtitle.GetTitleStandard(a => a.Job_Title_Id == jobtitle.Id).ToList();
                                    if (jobStandards.Any())
                                    {
                                        _repoJobtitle.DeleteTitleStandard(jobStandards);
                                        //jobStandards.ForEach(a => _repoTitleStandard.Delete(a));
                                    }
                                    var subjectStandard = subjectCodes.Select(a => new Title_Standard()
                                    {
                                        SubjectDetail = a,
                                        JobTitle = jobtitle
                                    }).ToList();
                                    subjectStandard.ForEach(a => _repoJobtitle.UpdateTitleStandard(a));
                                }
                                _repoJobtitle.Update(jobtitle);
                            }
                        }
                    }

                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Ham import 9
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<JsonResult> InsertEmployeesCM(IEnumerable<ImportEmplyeeModel> result)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                IFormatProvider cultures = new System.Globalization.CultureInfo("vi-VN", true);

                if (!result.Any())
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.NO_DATA
                    }, JsonRequestBehavior.AllowGet);
                }
                var courselist = result.ToList();
                bool checkInsert = false;
                var employees = EmployeeService.Get(true).ToList();
                var provinces = ConfigService.GetProvince();
                var jobTiltles = _repoJobtitle.Get();
                var departments = DepartmentService.Get();
                var companies = _repoCompany.Get();
                var nations = ConfigService.GetNations();
                string formatSQL = "yyyy-MM-dd";
                var call = false;
                var body = ConfigService.GetMail(a => a.Type == (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp).FirstOrDefault();
                var body_ = body?.Content;
                var cat_mail_ID = body?.ID ?? -1;
                foreach (var model in courselist)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    if (model.FlagImport == "1")
                    {
                        model.ErrorFlag = "";
                        model.ErrorNote = "";
                    }
                    else if (model.FlagImport == "0")
                    {
                        call = true;
                    }
                    var isUpdate = false;
                    var employee = new Trainee();
                    //var employeehistory = new TraineeHistory();

                    if (string.IsNullOrEmpty(model.EID))
                    {
                        model.ErrorFlag = "1";
                        model.ErrorNote = "<font color='red'>EID can not be emty!  </br> </font>";
                    }
                    if (model.PASSWORD != null && !string.IsNullOrEmpty(model.PASSWORD))
                    {
                        var checkPassword = IsPasswordAllowed(model.PASSWORD ?? string.Empty);
                        if (!checkPassword)
                        {
                            model.ErrorFlag = "1";
                            model.ErrorNote = "<font color='red'>" + Messege.RegEx_PASSWORD + " </br> </font>";
                        }

                    }
                    var randomPass = Common.RandomCharecter();


                    var checkEmployee = employees.FirstOrDefault(a => a.str_Staff_Id.ToLower().Trim() == model.EID.ToLower().Trim());
                    if (checkEmployee != null)
                    {
                        employee = checkEmployee;
                        isUpdate = true;
                        //model.ErrorFlag = "1";
                    }

                    else if (string.IsNullOrEmpty(model.EID))
                    {
                        model.ErrorFlag = "1";
                        model.ErrorNote += "<font color='red'>EID can not be null! </br></font>";
                    }
                    //if (string.IsNullOrEmpty(model.DEPARTMENT_CODE))
                    //{
                    //    model.ErrorFlag = "1";
                    //    model.ErrorNote = "<font color='red'>Department not found!</font>";
                    //}
                    dic.Add("str_Staff_Id", model.EID);
                    dic.Add("LmsStatus", (int)UtilConstants.ApiStatus.Modify);
                    //employee.str_Staff_Id = model.EID;
                    //employee.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                    if (!string.IsNullOrEmpty(model.PLACE_OF_BIRTH))
                    {
                        var nation = provinces.FirstOrDefault(a => a.Value.Contains(model.PLACE_OF_BIRTH));
                        //employee.str_Place_Of_Birth = nation.Value;
                        dic.Add("str_Place_Of_Birth", nation.Value);
                    }
                    //employee.str_Place_Of_Birth = model.PLACE_OF_BIRTH ?? "";
                    if (!string.IsNullOrEmpty(model.DATE_OF_BIRTH))
                    {
                        //employee.dtm_Birthdate = DateTime.Parse(model.DATE_OF_BIRTH, cultures, System.Globalization.DateTimeStyles.AssumeLocal);
                        dic.Add("dtm_Birthdate", DateTime.Parse(model.DATE_OF_BIRTH, cultures, System.Globalization.DateTimeStyles.AssumeLocal));
                    }
                    // var firstName = model.FULL_NAME.Split(' ')[0];
                    //  var lastName = model.FULL_NAME.Substring(model.FULL_NAME.Split(' ')[0].Length);

                    if (!string.IsNullOrEmpty(model.FULL_NAME))
                    {
                        var checkBOM = model.FULL_NAME.Trim().Replace('\u00A0', ' ');
                        var culture = "en";
                        var firstName = " ";
                        if (checkBOM.Contains(" "))
                        {
                            firstName = culture == "vi"
                           ? checkBOM.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last().Trim()
                           : checkBOM.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First().Trim();
                        }
                        var lastName = checkBOM.Replace(firstName, "").Trim();
                        //employee.FirstName = lastName;
                        //employee.LastName = firstName;
                        dic.Add("FirstName", lastName);
                        dic.Add("LastName", firstName);
                    }
                    else
                    {
                        if (isUpdate != true)
                        {
                            model.ErrorFlag = "1";
                            model.ErrorNote += "<font color='red'>Full Name can not be null! </br></font>";
                        }

                    }
                    //employee.Passport = model.ID_PASSPORT_NO ?? "";                  
                    //employee.bit_Internal = string.IsNullOrEmpty(model.TYPE) ? true : (model.TYPE.Trim().ToLower() == "internal");
                    dic.Add("Passport", model.ID_PASSPORT_NO ?? "");
                    var bit_Internal = string.IsNullOrEmpty(model.TYPE) ? true : (model.TYPE.Trim().ToLower() == "internal");
                    dic.Add("bit_Internal", bit_Internal);

                    if (bit_Internal == true)
                    {
                        if (string.IsNullOrEmpty(model.JOB_TITLE_CODE))
                        {
                            if (false)//isUpdate != true)
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Jobtitle can not be null! </br></font>";
                            }

                        }
                        else
                        {
                            var jobTitleCode = model.JOB_TITLE_CODE.Replace(Environment.NewLine, "");
                            var jobTiltle = jobTiltles.FirstOrDefault(a => a.Code.ToLower().Trim() == jobTitleCode.ToLower().Trim());
                            if (jobTiltle != null)
                            {
                                //employee.Job_Title_id = jobTiltle.Id;

                                //employeehistory.Job_Title_Id = jobTiltle.Id;
                                dic.Add("Job_Title_id", jobTiltle.Id);
                            }
                            else
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Jobtitle is not exist in system! </br></font>";
                            }
                        }
                        if (string.IsNullOrEmpty(model.DEPARTMENT_CODE))
                        {
                            if (isUpdate != true)
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Department can not be null! </br></font>";
                            }

                        }
                        else
                        {
                            var departmentCode = model.DEPARTMENT_CODE.Replace(Environment.NewLine, "");
                            var department = departments.FirstOrDefault(a => a.Code.ToLower().Trim() == departmentCode.ToLower().Trim());
                            if (department != null)
                            {
                                //employee.Department_Id = department.Id;
                                dic.Add("Department_Id", department.Id);
                            }
                            else
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Department is not exist in system! </br></font>";
                            }
                        }
                    }
                    else if (bit_Internal == false)
                    {
                        if (!string.IsNullOrEmpty(model.JOB_TITLE))
                        {
                            var jobTitleCode = model.JOB_TITLE_CODE?.Replace(Environment.NewLine, "");
                            var jobTiltle = jobTiltles.FirstOrDefault(a => a.Code.ToLower().Trim() == jobTitleCode.ToLower().Trim());
                            if (jobTiltle != null)
                            {
                                //employee.Job_Title_id = jobTiltle.Id;
                                //employeehistory.Job_Title_Id = jobTiltle.Id;  
                                dic.Add("Job_Title_id", jobTiltle.Id);
                            }
                            else
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Jobtitle is not exist in system! </br></font>";
                            }
                        }
                        if (!string.IsNullOrEmpty(model.DEPARTMENT))
                        {
                            var departmentCode = model.DEPARTMENT_CODE.Replace(Environment.NewLine, "");
                            var department = departments.FirstOrDefault(a => a.Code.ToLower().Trim() == departmentCode.ToLower().Trim());
                            if (department != null)
                            {
                                //employee.Department_Id = department.Id;
                                dic.Add("Department_Id", department.Id);
                            }
                            else
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Department is not exist in system! </br></font>";
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(model.COMPANY))
                    {
                        var partner =
                            companies.FirstOrDefault(a => a.str_code.ToLower().Trim() == model.COMPANY.Trim().ToLower());

                        //employee.Company_Id = partner?.Company_Id;
                        dic.Add("Company_Id", partner?.Company_Id);
                    }

                    model.JOB_TITLE = model.JOB_TITLE ?? "";


                    if (!string.IsNullOrEmpty(model.NATIONALITY))
                    {
                        var nation =
                            nations.FirstOrDefault(a => a.Nation_Code.ToLower().Trim() == model.NATIONALITY.ToLower().Trim());
                        //employee.Nation = nation?.Nation_Code;
                        dic.Add("Nation", nation?.Nation_Code);
                    }
                    //employee.Nation = model.NATIONALITY ?? "";

                    if (!string.IsNullOrEmpty(model.GENDER) && (model.GENDER.Trim().ToLower() == "m" || model.GENDER.Trim().ToLower() == "f" || model.GENDER.Trim().ToLower() == "female" || model.GENDER.Trim().ToLower() == "male"))
                    {
                        var gender = (model.GENDER.Trim().ToLower() == "m" || model.GENDER.Trim().ToLower() == "male") ? 1 : 2;
                        //employee.Gender = model.GENDER.Trim().ToLower() == "m" ? 1 : 2;
                        dic.Add("Gender", gender);
                    }
                    else
                    {
                        if (isUpdate != true)
                        {
                            dic.Add("Gender", 3);
                        }

                    }
                    bool checkEmail = false;

                    if (!string.IsNullOrEmpty(model.EMAIL))
                    {
                        if (checkEmployee != null)
                        {
                            checkEmail = employees.Any(a => a.str_Email == model.EMAIL.Trim() && a.Id != checkEmployee.Id);
                        }
                        else
                        {
                            var check = employees.Any(a => a.str_Email == model.EMAIL.Trim() && a.str_Staff_Id.ToLower().Trim() != model.EID.ToLower().Trim());
                            if (check)
                            {
                                checkEmail = check;
                            }
                        }

                        if (checkEmail)
                        {
                            model.ErrorFlag = "1";
                            model.ErrorNote += "<font color='red'>Email is exist in system ! </br></font>";
                        }

                        if (IsValidEmail(model.EMAIL))
                        {
                            //employee.str_Email = model.EMAIL.Trim();
                            dic.Add("str_Email", model.EMAIL.Trim());
                        }
                        else
                        {
                            model.ErrorFlag = "1";
                            model.ErrorNote += "<font color='red'>Please enter correct the email type! </br></font>";
                        }

                    }
                    else
                    {
                        if (isUpdate != true)
                        {
                            var mail = model.EID + "@" + model.EID + ".com";
                            //employee.str_Email = employee.str_Staff_Id + "@" + employee.str_Staff_Id + ".com";
                            dic.Add("str_Email", mail);
                        }

                    }
                    //employee.str_Cell_Phone = model.PHONE ?? "";
                    if (!string.IsNullOrEmpty(model.PHONE))
                    {
                        if (IsNumberAllowed(model.PHONE))
                        {
                            dic.Add("str_Cell_Phone", model.PHONE);
                        }

                    }


                    if (!string.IsNullOrEmpty(model.DATE_OF_JOIN))
                    {
                        var dtm_Join_Date = DateTime.Parse(model.DATE_OF_JOIN, cultures, System.Globalization.DateTimeStyles.AssumeLocal);
                        //employee.dtm_Join_Date = DateTime.Parse(model.DATE_OF_JOIN, cultures, System.Globalization.DateTimeStyles.AssumeLocal); 
                        dic.Add("dtm_Join_Date", dtm_Join_Date);
                    }
                    var int_Role = (int)UtilConstants.ROLE.Trainee;
                    if (!string.IsNullOrEmpty(model.IS_INSTRUCTOR) && model.IS_INSTRUCTOR.ToLower().Trim() == "false")
                    {
                        //employee.int_Role = (int)UtilConstants.ROLE.Trainee;
                        dic.Add("int_Role", (int)UtilConstants.ROLE.Trainee);
                    }
                    else if (!string.IsNullOrEmpty(model.IS_INSTRUCTOR) && model.IS_INSTRUCTOR.ToLower().Trim() == "true")
                    {
                        //employee.int_Role = (int)UtilConstants.ROLE.Instructor;
                        dic.Add("int_Role", (int)UtilConstants.ROLE.Instructor);
                        int_Role = (int)UtilConstants.ROLE.Instructor;
                    }
                    if (!string.IsNullOrEmpty(model.IS_EXAMINER) && model.IS_EXAMINER.ToLower().Trim() == "true")
                    {
                        //employee.IsExaminer = true;
                        dic.Add("IsExaminer", 1);
                    }


                    //employeehistory.dtm_Create_At = DateTime.Now;
                    var traineeID = -1;
                    if (isUpdate)
                    {

                        if (model.FlagImport != "1" && model.ErrorFlag != "1")
                        {
                            var Password = !string.IsNullOrEmpty(model.PASSWORD) ? Common.EncryptString(model.PASSWORD) : employee.Password;
                            //employee.Password = !string.IsNullOrEmpty(model.PASSWORD) ? Common.EncryptString(model.PASSWORD) : employee.Password;
                            dic.Add("Password", Password);
                            if (CMSUtils.UpdateDataSQLNoLog("Id", employee.Id + "", "Trainee", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                            traineeID = employee.Id;
                            // EmployeeService.Update(employee);
                            model.ErrorNote += "<font color='green'>Update</br></font>";
                            if (checkEmployee != null)
                            {
                                //employeehistory.Trainee_Id = checkEmployee.Id;
                                //EmployeeService.Insert1(employeehistory);
                                model.ErrorNote += "<font color='green'>CreateLog</br></font>";
                                //await Task.Run(() =>
                                //{
                                //    var mail_receiver = checkEmployee.str_Email;
                                //    var bodysendmail = BodySendMail_Custom(body_, null, checkEmployee, null, null, 2);
                                //    if (!string.IsNullOrEmpty(mail_receiver) && cat_mail_ID != -1 && !string.IsNullOrEmpty(bodysendmail))
                                //    {
                                //        InsertSentMail_Custom(cat_mail_ID, mail_receiver, (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp, bodysendmail, null, "");
                                //    }

                                //});

                            }
                            else
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Data is empty! </br></font>";
                            }

                        }
                    }
                    else
                    {
                        if (model.FlagImport != "1" && model.ErrorFlag != "1")
                        {

                            //var randomPass = Common.RandomCharecter();
                            //employee.IsActive = true;
                            //employee.IsDeleted = false;
                            //employee.dtm_Created_At = DateTime.Now;
                            //employee.Password = !string.IsNullOrEmpty(model.PASSWORD) ? Common.EncryptString(model.PASSWORD) : Common.EncryptString(randomPass);

                            var Password = !string.IsNullOrEmpty(model.PASSWORD) ? Common.EncryptString(model.PASSWORD) : Common.EncryptString(randomPass);
                            dic.Add("Password", Password);
                            dic.Add("IsActive", 1);
                            dic.Add("IsDeleted", 0);
                            dic.Add("dtm_Created_At", DateTime.Now);
                            if (CMSUtils.InsertDataSQLNoLog("Trainee", dic.Keys.ToArray(), CMSUtils.SetDBNullobject(dic.Values.ToArray())) > 0)
                            {

                            }
                            var trainee = EmployeeService.GetByCode(model.EID);
                            if (trainee != null)
                            {
                                checkInsert = true;
                                employees.Add(trainee);
                                traineeID = trainee.Id;
                            }

                            model.ErrorNote += "<font color='green'>Create</br></font>";

                            if (checkInsert)
                            {
                                //employeehistory.Trainee_Id = trainee.Id;
                                //EmployeeService.Insert1(employeehistory);
                                model.ErrorNote += "<font color='green'>CreateLog</br></font>";
                                #region [-----------------Sent Mail Trainee------------------]
                                await Task.Run(() =>
                                {
                                    var mail_receiver = trainee.str_Email;
                                    var bodysendmail = BodySendMail_Custom(body_, null, trainee, null, null, 2);
                                    if (!string.IsNullOrEmpty(mail_receiver) && cat_mail_ID != -1 && !string.IsNullOrEmpty(bodysendmail))
                                    {
                                        InsertSentMail_Custom(cat_mail_ID, mail_receiver, (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp, bodysendmail, null, "");
                                    }
                                    //Sent_Email_TMS(null, trainee, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee);
                                });

                                #endregion
                            }
                            else
                            {
                                model.ErrorFlag = "1";
                                model.ErrorNote += "<font color='red'>Data is empty! </br></font>";
                            }
                        }
                    }
                    if (traineeID != -1)
                    {
                        if (int_Role == (int)UtilConstants.ROLE.Instructor)
                        {


                            if (!string.IsNullOrEmpty(model.CONTRACT_NO) && !string.IsNullOrEmpty(model.CONTRACT_EXPIRE_DATE))
                            {
                                // var contract = new Trainee_Contract();
                                //contract.expire_date = DateUtil.StringToDate2(model.CONTRACT_EXPIRE_DATE,
                                //    DateUtil.DATE_FORMAT_OUTPUT);
                                //contract.contractno = model.CONTRACT_NO ?? "";
                                //contract.description = model.CONTRACT_DESCRIPTION ?? "";
                                Dictionary<string, object> dic_contract = new Dictionary<string, object>();
                                dic_contract.Add("Trainee_Id", traineeID);
                                dic_contract.Add("expire_date", DateUtil.StringToDate2(model.CONTRACT_EXPIRE_DATE,
                                   DateUtil.DATE_FORMAT_OUTPUT));
                                dic_contract.Add("contractno", model.CONTRACT_NO);
                                dic_contract.Add("description", model.CONTRACT_DESCRIPTION);
                                dic_contract.Add("dtm_Created_At", DateTime.Now);
                                dic_contract.Add("IsActive", 1);
                                dic_contract.Add("IsDeleted", 0);
                                if (CMSUtils.InsertDataSQLNoLog("Trainee_Contract", dic_contract.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_contract.Values.ToArray())) > 0)
                                {

                                }
                                //employee.Trainee_Contract.Add(contract);
                            }

                        }
                        if (!string.IsNullOrEmpty(model.EDUCATION_COURSE))
                        {
                            //var traineeEdu = new Trainee_Record();
                            //traineeEdu.str_Subject = model.EDUCATION_COURSE;
                            //if (!string.IsNullOrEmpty(model.EDUCATION_FROM))
                            //{
                            //    traineeEdu.dtm_time_from = DateUtil.StringToDate2(model.EDUCATION_FROM,
                            //        DateUtil.DATE_FORMAT_OUTPUT);
                            //}
                            //if (!string.IsNullOrEmpty(model.EDUCATION_TO))
                            //{
                            //    traineeEdu.dtm_time_to = DateUtil.StringToDate2(model.EDUCATION_TO,
                            //        DateUtil.DATE_FORMAT_OUTPUT);
                            //}
                            //traineeEdu.str_organization = model.ORGANIZATION ?? "";
                            //traineeEdu.str_note = model.NOTE ?? "";

                            Dictionary<string, object> dic_edu = new Dictionary<string, object>();
                            dic_edu.Add("Trainee_Id", traineeID);
                            dic_edu.Add("str_Subject", model.EDUCATION_COURSE);
                            if (!string.IsNullOrEmpty(model.EDUCATION_FROM))
                            {
                                dic_edu.Add("dtm_time_from", DateUtil.StringToDate2(model.EDUCATION_FROM,
                                    DateUtil.DATE_FORMAT_OUTPUT));
                            }
                            if (!string.IsNullOrEmpty(model.EDUCATION_TO))
                            {
                                dic_edu.Add("dtm_time_to", DateUtil.StringToDate2(model.EDUCATION_TO,
                                    DateUtil.DATE_FORMAT_OUTPUT));
                            }

                            dic_edu.Add("str_organization", model.ORGANIZATION);
                            dic_edu.Add("str_note", model.NOTE);
                            dic_edu.Add("isActive", 1);
                            dic_edu.Add("bit_Deleted", 0);
                            if (CMSUtils.InsertDataSQLNoLog("Trainee_Record", dic_edu.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_edu.Values.ToArray())) > 0)
                            {

                            }
                            //employee.Trainee_Record.Add(traineeEdu);
                        }

                        if (!string.IsNullOrEmpty(model.EX_EDUCATION_COURSE))
                        {
                            //var traineeEdu1 = new Trainee_Record();
                            //traineeEdu1.str_Subject = model.EX_EDUCATION_COURSE ?? "";
                            //if (!string.IsNullOrEmpty(model.EX_EDUCATION_FROM))
                            //{
                            //    traineeEdu1.dtm_time_from = DateUtil.StringToDate2(model.EX_EDUCATION_FROM,
                            //        DateUtil.DATE_FORMAT_OUTPUT);
                            //}
                            //if (!string.IsNullOrEmpty(model.EX_EDUCATION_TO))
                            //{
                            //    traineeEdu1.dtm_time_to = DateUtil.StringToDate2(model.EX_EDUCATION_TO,
                            //        DateUtil.DATE_FORMAT_OUTPUT);
                            //}
                            //traineeEdu1.str_organization = model.EX_EDUCATION_ORGANIZATION ?? "";
                            //traineeEdu1.str_note = model.NOTE ?? "";
                            //employee.Trainee_Record.Add(traineeEdu1);

                            Dictionary<string, object> dic_edu_ex = new Dictionary<string, object>();
                            dic_edu_ex.Add("Trainee_Id", traineeID);
                            dic_edu_ex.Add("str_Subject", model.EX_EDUCATION_COURSE);
                            if (!string.IsNullOrEmpty(model.EX_EDUCATION_FROM))
                            {
                                dic_edu_ex.Add("dtm_time_from", DateUtil.StringToDate2(model.EX_EDUCATION_FROM,
                                    DateUtil.DATE_FORMAT_OUTPUT));
                            }
                            if (!string.IsNullOrEmpty(model.EDUCATION_TO))
                            {
                                dic_edu_ex.Add("dtm_time_to", DateUtil.StringToDate2(model.EX_EDUCATION_TO,
                                    DateUtil.DATE_FORMAT_OUTPUT));
                            }

                            dic_edu_ex.Add("str_organization", model.EX_EDUCATION_ORGANIZATION);
                            dic_edu_ex.Add("str_note", model.NOTE);
                            dic_edu_ex.Add("isActive", 1);
                            dic_edu_ex.Add("bit_Deleted", 0);
                            if (CMSUtils.InsertDataSQLNoLog("Trainee_Record", dic_edu_ex.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_edu_ex.Values.ToArray())) > 0)
                            {

                            }
                        }
                    }

                    //if (checkInsert)
                    //{
                    //    if (employee.int_Role == (int)UtilConstants.ROLE.Instructor)
                    //    {
                    //        if (model.FlagImport != "1")
                    //        {
                    //            var checkEmp = employees.FirstOrDefault(a => a.str_Staff_Id == model.EID);
                    //            if (!string.IsNullOrEmpty(model.COURSE))
                    //            {
                    //                var courseItem = model.COURSE.Split(new char[] { ',' });
                    //                foreach (var item in courseItem)
                    //                {
                    //                    var checkCode = _repoSubject.GetSubjectDetailByCode(item);
                    //                    if (checkCode.Any())
                    //                    {
                    //                        var subid = checkCode.FirstOrDefault().Id;
                    //                        var checkExist = EmployeeService.GetInstruc_Ability(a => a.SubjectDetailId == subid && a.InstructorId == checkEmp.Id);
                    //                        if (!checkExist.Any())
                    //                        {
                    //                            var instructorAbility = new Instructor_Ability
                    //                            {
                    //                                InstructorId = checkEmp.Id,
                    //                                SubjectDetailId = subid,
                    //                                CreateBy = CurrentUser.USER_ID,
                    //                                CreateDate = DateTime.Now
                    //                            };
                    //                            EmployeeService.InsertAbility(instructorAbility);

                    //                            var instructorAbilityLog = new Instructor_Ability_LOG
                    //                            {
                    //                                InstructorId = checkEmp.Id,
                    //                                SubjectDetailId = subid,
                    //                                CreateBy = CurrentUser.USER_ID,
                    //                                CreateDate = DateTime.Now,
                    //                                IsDeleted = false
                    //                            };
                    //                            EmployeeService.InsertAbilityLog(instructorAbilityLog);
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    if (model.ErrorFlag != "1")
                    {
                        model.ErrorNote += "<font color='green'>No Error</br></font>";
                    }
                }

                var jsonResult = Json(new AjaxResponseViewModel()
                {
                    result = true,
                    message = Messege.SUCCESS,
                    data = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = ex.ToString(),
                    data = result
                }, JsonRequestBehavior.AllowGet);
            }

        }


    }
}
