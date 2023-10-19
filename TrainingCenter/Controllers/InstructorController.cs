using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;
using ClientMonitor.Filters;
using System.Data;
using System.IO;
using System.Globalization;
using TrainingCenter.Utilities;
//using TrainingCenter.Serveices;
using TMS.Core.App_GlobalResources;
using System.Text;
using System.Web.UI;
using Microsoft.Reporting.WebForms;
using DAL.Entities;
using Newtonsoft.Json;
using TMS.Core.Services.Approves;
using TMS.Core.ViewModels.Subjects;

namespace TrainingCenter.Controllers
{
    using DAL.UnitOfWork;
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
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.ViewModel;
    using TMS.Core.ViewModels.Instructor;
    using TMS.Core.ViewModels.EmployeeModels;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.Configuration;
    using System.Threading.Tasks;

    [ClientMonitor]
    public class InstructorController : BaseAdminController
    {

        #region MyRegion

        private readonly ISubjectService _repoSubject;
        private readonly ICourseMemberService _repoCoursemember;
        private readonly ICompanyService _repoCompany;
        private readonly ICourseDetailService _repoCourse_Detail;
        private readonly IJobtitleService _repoJob_Tiltle;
        private readonly IConfigService _repoConfig;

        private const int statusModify = (int)UtilConstants.ApiStatus.Modify;

        public InstructorController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, ISubjectService repoSubject, ICourseMemberService repoCoursemember, ICompanyService repoCompany, ICourseDetailService repoCourseDetail, IJobtitleService repoJobTiltle, IDepartmentService repoDepartment, IConfigService repoConfig, ICourseService repoCourse, IApproveService approveService)
            : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, repoDepartment, repoCourse, approveService)
        {
            _repoSubject = repoSubject;
            _repoCoursemember = repoCoursemember;
            _repoCompany = repoCompany;
            _repoCourse_Detail = repoCourseDetail;
            _repoJob_Tiltle = repoJobTiltle;
            _repoConfig = repoConfig;
        }

        #endregion

        #region Print Profile
        public ActionResult EmployeeProfilePrint(int? id)
        {
            var subjectType = (int?)UtilConstants.ApproveType.SubjectResult;
            var model = new EmployeeViewModel()
            {
                Employees = EmployeeService.GetById(id),
                Training_Education =  EmployeeService.GetRecord(a => a.Trainee_Id == id).OrderBy(p => p.str_Subject),
                Training_Contracts =  EmployeeService.GetContract(a => a.Trainee_Id == id).OrderBy(p => p.id),
                Training_Competency =  _repoSubject.GetSubjectDetail(a => a.IsActive==true && a.Instructor_Ability.Any(x => x.InstructorId == id)).OrderBy(a => a.Id),
                Training_Course = _repoCourse_Detail.Get(
                    a => a.TMS_Course_Member.Any(x => x.Member_Id == id) &&
                        a.TMS_APPROVES.Any(x => x.int_Type == subjectType)).ToList()
                        .Where(a =>
                            a.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?.int_id_status == (int)UtilConstants.EStatus.Approve).Select(x => x.Course).Distinct()
            };
            var entity = EmployeeService.GetById(id);
            return PartialView("EmployeeProfilePrint", model);
        }

        #endregion

        #region Export PDF
        // Export PDF
        public ActionResult GetDataPDF()
        {
            int ComOrDepId = string.IsNullOrEmpty(Request.QueryString["ComOrDepId"]) ? -1 : Convert.ToInt32(Request.QueryString["ComOrDepId"].Trim());
            int fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"]);
            int ddl_TYPE = string.IsNullOrEmpty(Request.QueryString["ddl_TYPE"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_TYPE"]);
            int fJobTitle = string.IsNullOrEmpty(Request.QueryString["fJobTitle"]) ? -1 : Convert.ToInt32(Request.QueryString["fJobTitle"].Trim());
            int fGender = string.IsNullOrEmpty(Request.QueryString["fGender"]) ? -1 : Convert.ToInt32(Request.QueryString["fGender"].Trim());
            //string url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().ToString();
            string fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].ToString();
            string fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? "" : Request.QueryString["fEmail"].ToString();
            string fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? "" : Request.QueryString["fPhone"].ToString();
            string fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].ToString();
            bool ss = true;
            if (ddl_TYPE == 0)
            {
                ss = false;
            }
            var data = new List<Trainee>();

            data = EmployeeService.Get(a => a.int_Role == (int)UtilConstants.ROLE.Instructor
          && (ddl_TYPE == -1 || a.bit_Internal == ss)
          && (ComOrDepId == -1 || a.Department_Id == ComOrDepId)
          && (string.IsNullOrEmpty(fName) || (a.FirstName.Contains(fName) || a.LastName.Contains(fName)))
          && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
          && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail))
          && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone))
          && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId))
          && (fGender == -1 || a.Gender == fGender)
          && (fStatus == -1 || a.Suspended == fStatus)).ToList();
            return View(data);
        }

        #endregion

        #region Export EXCEL
        public FileResult ExportEXCEL(FormCollection form)
        {
            var filecontent = ExportEXCEL();
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "InstructorList.xlsx");
            }
            return null;
        }
        private byte[] ExportEXCEL()
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "ExcelFile/InstructorList.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            //var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);
            var departmentId = string.IsNullOrEmpty(Request.QueryString["DepartmentList"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList"].Trim());
            var fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"].Trim());
            var type = string.IsNullOrEmpty(Request.QueryString["Type"]) ? -1 : Convert.ToInt32(Request.QueryString["Type"].Trim());
            var fJobTitle = string.IsNullOrEmpty(Request.QueryString["JobTitleList"]) ? -1 : Convert.ToInt32(Request.QueryString["JobTitleList"].Trim());
            var fGender = string.IsNullOrEmpty(Request.QueryString["Genders"]) ? -1 : Convert.ToInt32(Request.QueryString["Genders"].Trim());

            var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
            var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? "" : Request.QueryString["fEmail"].Trim();
            var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? "" : Request.QueryString["fPhone"].Trim();
            var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].Trim();
            var ss = true;
            if (type == 0)
            {
                ss = false;
            }

            var data = EmployeeService.Get(a => a.int_Role == (int)UtilConstants.ROLE.Instructor
            && (type == -1 || a.bit_Internal == ss)
            && (departmentId == -1 || a.Department_Id == departmentId)
            && (string.IsNullOrEmpty(fName) || (a.FirstName.Contains(fName) || a.LastName.Contains(fName)))
            && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
            && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail))
            && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone))
            && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId))
            && (fGender == -1 || a.Gender == fGender)
            && (fStatus == -1 || a.Suspended == fStatus));

            ExcelPackage excelPackage;
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            if (data != null)
            {
                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 9;
                    //var Row = 0;
                    int count = 0;
                    foreach (var item in data)
                    {
                        int col = 2;
                        count++;
                        ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                        cellNo.Value = count;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellEid = worksheet.Cells[startRow + 1, ++col];
                        cellEid.Value = item?.str_Staff_Id;
                        cellEid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellEid.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellEid.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, ++col];
                        //cellFullName.Value = item?.FirstName + " " + item?.LastName;
                        cellFullName.Value = ReturnDisplayLanguage(item?.FirstName,item?.LastName);

                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        var gender = item?.Gender ?? (int)UtilConstants.Gender.Others;
                        ExcelRange cellGender = worksheet.Cells[startRow + 1, ++col];
                        cellGender.Value = UtilConstants.GenderDictionary()[gender];
                        cellGender.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellGender.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellGender.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellEmail = worksheet.Cells[startRow + 1, ++col];
                        cellEmail.Value = item?.str_Email;
                        cellEmail.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellEmail.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellEmail.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellPhone = worksheet.Cells[startRow + 1, ++col];
                        cellPhone.Value = item?.str_Cell_Phone;
                        cellPhone.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellPhone.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellPhone.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellJobtitleName = worksheet.Cells[startRow + 1, ++col];
                        cellJobtitleName.Value = item?.JobTitle?.Name;
                        cellJobtitleName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellJobtitleName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellJobtitleName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellDeparment = worksheet.Cells[startRow + 1, ++col];
                        cellDeparment.Value = item?.Department?.Name;
                        cellDeparment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellDeparment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellDeparment.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellRemark = worksheet.Cells[startRow + 1, ++col];
                        cellRemark.Value = item.bit_Internal==true ? "Internal" : "External";
                        cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        startRow++;

                    }
                    bytes = excelPackage.GetAsByteArray();
                }

            }
            return bytes;

        }

        #endregion
        public ActionResult Index()
        {
            var model = new InstructorModel
            {
                DepartmentList = loaddepartment(null, 1, null),
                JobTitles = new SelectList(_repoJob_Tiltle.Get().OrderBy(m => m.Name), "Id", "Name"),
                Status = new SelectList(new[]
                {
                    new {Id = "0", Name = "Active"},
                    new {Id = "1", Name = "Deactive"}
                }, "Id", "Name"),
                Type = new SelectList(new[]
                {
                    new {Id = "1", Name = "Internal"},
                    new {Id = "0", Name = "External"}
                }, "Id", "Name"),
                Genders = new SelectList(new[]
                {
                    new {Id = "1", Name = "Male"},
                    new {Id = "2", Name = "Female"},
                    new {Id = "3", Name = "Others"}
                }, "Id", "Name"),
                Mentor = new SelectList(new[]
                {
                    new {Id = (int)UtilConstants.TypeInstructor.Mentor,Name=UtilConstants.TypeInstructor.Mentor.ToString()},
                    new {Id = (int)UtilConstants.TypeInstructor.Instructor,Name=UtilConstants.TypeInstructor.Instructor.ToString()}
                })
                
            };
            return View(model);
        }
        private string loaddepartment(int? parentid, int level, int? department_id)
        {
            string result = string.Empty;
            bool parent = false;
            var data = DepartmentService.Get(null, CurrentUser.PermissionIds);
            if (parentid == null)
            {
                data = data.Where(a => a.ParentId == null);
                parent = true;
            }
            else
            {
                data = data.Where(a => a.ParentId == parentid);
            }

            if (data.Count() == 0)
                return result;
            else
            {
                foreach (var item in data)
                {
                    string selected = "";
                    if (item.Id == department_id)
                    {
                        selected = "selected";
                    }
                    string khoangtrang = "";
                    for (int i = 0; i < level; i++)
                    {
                        khoangtrang += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                    }

                    result += "<option value='" + item.Id + "' style='font-size:" + (18 - (level + 2)) + "px;' " + selected + " >" + khoangtrang + item.Code + " - " + item.Name;
                    result += "</option>";
                    result += loaddepartment(item.Id, level + 1, department_id);
                }
            }
            return result;
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                // xử lý param gửi lên 

                var departmentId = string.IsNullOrEmpty(Request.QueryString["DepartmentList"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList"].Trim());
                var fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"].Trim());
                var type = string.IsNullOrEmpty(Request.QueryString["Type"]) ? -1 : Convert.ToInt32(Request.QueryString["Type"].Trim());
                var fJobTitle = string.IsNullOrEmpty(Request.QueryString["JobTitleList"]) ? -1 : Convert.ToInt32(Request.QueryString["JobTitleList"].Trim());
                var fGender = string.IsNullOrEmpty(Request.QueryString["Genders"]) ? -1 : Convert.ToInt32(Request.QueryString["Genders"].Trim());

                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
                var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? "" : Request.QueryString["fEmail"].Trim();
                var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? "" : Request.QueryString["fPhone"].Trim();
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].Trim();
                var ss = true;
                if (type == 0)
                {
                    ss = false;
                }

                var data = EmployeeService.Get(a => a.int_Role == (int)UtilConstants.ROLE.Instructor
                && (type == -1 || a.bit_Internal == ss)
                && (departmentId == -1 || a.Department_Id == departmentId)
                  && (string.IsNullOrEmpty(fName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(fName.Trim())))
                && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
                && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail.Trim()))
                && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone.Trim()))
                && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId.Trim()))
                && (fGender == -1 || a.Gender == fGender)
                && (fStatus == -1 || a.Suspended == fStatus));

                IEnumerable<Trainee> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c.FirstName
                                                            : sortColumnIndex == 3 ? c?.Gender.ToString()
                                                            : sortColumnIndex == 4 ? c.str_Email
                                                            : sortColumnIndex == 5 ? c.str_Cell_Phone
                                                            : sortColumnIndex == 6 ? c.JobTitle?.Name
                                                            : sortColumnIndex == 7 ? c.Department?.Code
                                                            : sortColumnIndex == 8 ? c?.bit_Internal.ToString()
                                                          : c.LastName);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             let gender = c.Gender ?? (int)UtilConstants.Gender.Others
                             let fullName = ReturnDisplayLanguage(c.FirstName, c.LastName)
                             select new object[] {
                                 string.Empty,
                                    c.str_Staff_Id,
                                    //"<a href='"+@Url.Action("Details",new{id = c.Id})+"')'>"+c.FirstName + " " + c.LastName+"</a>",
                                     "<a href='"+@Url.Action("Details",new{id = c.Id})+"')'>"+ ReturnDisplayLanguage(c.FirstName,c.LastName) +"</a>",
                                    UtilConstants.GenderDictionary()[gender],
                                    c.str_Email,
                                    c.str_Cell_Phone,
                                    c.JobTitle?.Name ?? "",
                                    c.Department?.Code ?? "",
                                    c.bit_Internal==true ? "Internal": "External",
                                     c.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Instructor(0,"+c.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Instructor(1,"+c.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",

                                     ((User.IsInRole("/Instructor/Details")) ? "<a  title='View' href='"+@Url.Action("Details",new{id = c.Id})+"')'><i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; '></i></a>":"") + 

                                     ((User.IsInRole("/Instructor/Create")) ? verticalBar +"<a  title='Edit' href='"+@Url.Action("Create",new{id = c.Id})+"')'><i class='fa fa-pencil-square-o' aria-hidden='true' style=' font-size: 16px; '></i></a>":"") + 

                                     ((User.IsInRole("/Instructor/Create")) ? verticalBar +"<a title='New Password' href='javascript:void(0)'  onclick='callrandom(\" "+fullName+" \"," + c.Id  + ")'><i class='fa fa-paper-plane' aria-hidden='true' style=' font-size: 16px; '></i></a>" : "") + 

                                    ((User.IsInRole("/Instructor/Delete")) ? verticalBar +"<a title='Delete' href='javascript:void(0)'  onclick='calldelete(" + c.Id  + ")'><i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px; '></i></a>" :"")


            };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

      
        [HttpPost]
        public JsonResult CreateEmployee(Trainee trainee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var countEmail = EmployeeService.Get(m => m.str_Email == trainee.str_Email).Count();
                    if (countEmail > 0)
                    {
                        return Json(new
                        {
                            Result = "ERROR",
                            Message = Messege.EMAIL_EXIST /*"The email you input is exist in the system!"*/
                        });
                    }
                    else
                    {
                        trainee.dtm_Created_At = DateTime.Now;
                        trainee.str_Created_By = User.Identity.Name;
                        trainee.bit_Internal = true;
                        EmployeeService.Insert(trainee);
                        return Json(new { Result = "OK", Record = trainee });
                    }
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID /*"Form is not valid! Please correct it and try again."*/
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Create", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
        }
        [HttpPost]
        public JsonResult CreatePartner(Trainee trainee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var countEmail = EmployeeService.Get(m => m.str_Email == trainee.str_Email).Count();
                    if (countEmail > 0)
                    {
                        return Json(new
                        {
                            Result = "ERROR",
                            Message = Messege.EMAIL_EXIST // "The email you input is exist in the system!"
                        });
                    }
                    else
                    {
                        trainee.dtm_Created_At = DateTime.Now;
                        trainee.str_Created_By = User.Identity.Name;
                        trainee.bit_Internal = false;
                        EmployeeService.Insert(trainee);
                        return Json(new { Result = "OK", Record = trainee });
                    }
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID // "Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Create", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID // "Form is not valid! Please correct it and try again."
                });
            }
        }

        [HttpPost]
        public JsonResult Update(Trainee trainee)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dbTrainee = EmployeeService.GetById(trainee.Id);
                    var countEmail = EmployeeService.Get(m => m.str_Email == trainee.str_Email && m.IsDeleted==false);
                    if (dbTrainee.str_Email != trainee.str_Email && countEmail.Any())
                    {
                        return Json(new
                        {
                            Result = "ERROR",
                            Message = Messege.EMAIL_EXIST // "The email you input is exist in the system!"
                        });
                    }
                    else
                    {
                        dbTrainee.Company_Id = trainee.Company_Id;
                        dbTrainee.Department_Id = trainee.Department_Id;
                        dbTrainee.str_Staff_Id = trainee.str_Staff_Id;
                        dbTrainee.FirstName = trainee.FirstName;
                        dbTrainee.LastName = trainee.LastName;
                        dbTrainee.str_Email = trainee.str_Email;
                        dbTrainee.str_Cell_Phone = trainee.str_Cell_Phone;
                        dbTrainee.Job_Title_id = trainee.Job_Title_id;
                        dbTrainee.str_Station = trainee.str_Station;
                        dbTrainee.dtm_Birthdate = trainee.dtm_Birthdate;
                        dbTrainee.str_Place_Of_Birth = trainee.str_Place_Of_Birth;
                        dbTrainee.dtm_Last_Modified_At = DateTime.Now;
                        dbTrainee.str_Last_Modified_By = User.Identity.Name;
                        dbTrainee.dtm_Join_Date = trainee.dtm_Join_Date;
                        EmployeeService.Update(dbTrainee);
                        return Json(new { Result = "OK" });
                    }
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID // "Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Edit", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
        }

        public JsonResult Delete(Trainee trainee)
        {
            try
            {
                var item = EmployeeService.GetById(trainee.Id);
                item.IsDeleted = true;
                EmployeeService.Update(item);
                return Json(new { Result = "OK" });
            }

            catch (Exception ex)
            {
                ExceptionUtils.logError("Trainee - Delete", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
        }


        [HttpPost]
        public JsonResult GetDepartmentOption()
        {
            try
            {
                var deptList = DepartmentService.Get(null, CurrentUser.PermissionIds).ToList();
                return Json(new { Result = "OK", Options = deptList.Select(m => new { DisplayText = m.Name, Value = m.Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GetCompanyOption()
        {
            try
            {

                var deptList = _repoCompany.Get(m => m.bit_Deleted==false).ToList();
                return Json(new { Result = "OK", Options = deptList.Select(m => new { DisplayText = m.str_Name, Value = m.Company_Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult GetJobTitleOption()
        {
            try
            {
                var titleList = _repoJob_Tiltle.Get().OrderBy(m => m.Name).ToList();
                return Json(new { Result = "OK", Options = titleList.Select(m => new { DisplayText = m.Name, Value = m.Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult TraineeRecordList(int Trainee_Id, int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                var query = EmployeeService.GetRecordByTraineeId(Trainee_Id).ToList();

                return Json(new
                {
                    Result = "OK",
                    Records = query.Select(m => new { m.Trainee_Record_Id, m.str_Subject, m.str_Duration, m.str_Location, m.str_Trainer, m.str_Result }),

                });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        public JsonResult DeleteRecord(Trainee_Record traineeRecord)
        {
            try
            {
                var item = EmployeeService.GetRecordId(traineeRecord.Trainee_Record_Id);
                item.bit_Deleted = true;
                EmployeeService.UpdateRecord(item);
                return Json(new { Result = "OK" });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Delete", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
        }

        [HttpPost]
        public JsonResult CreateRecord(Trainee_Record traineeRecord)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    traineeRecord.dtm_Created_At = DateTime.Now;
                    traineeRecord.str_Created_By = User.Identity.Name;
                    traineeRecord.bit_Deleted = false;
                    traineeRecord.isActive = true;
                    EmployeeService.InsertRecord(traineeRecord);
                    return Json(new { Result = "OK", Record = traineeRecord });
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Create", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
        }

        [HttpPost]
        public JsonResult UpdateRecord(Trainee_Record traineeRecord)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dbRecord = EmployeeService.GetRecordId(traineeRecord.Trainee_Record_Id);
                    dbRecord.str_Subject = traineeRecord.str_Subject;
                    dbRecord.str_Duration = traineeRecord.str_Duration;
                    dbRecord.str_Location = traineeRecord.str_Location;
                    dbRecord.str_Trainer = traineeRecord.str_Trainer;
                    dbRecord.str_Result = traineeRecord.str_Result;

                    dbRecord.dtm_Modified_At = DateTime.Now;
                    dbRecord.str_Modified_By = User.Identity.Name;
                    EmployeeService.UpdateRecord(dbRecord);
                    return Json(new { Result = "OK" });
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Edit", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
        }

        public ActionResult Import()
        {
            return View();
        }
        public ActionResult ImportPartner()
        {
            return View();
        }



        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadEmloyeeList(HttpPostedFileBase staffFile)
        {
            var importModel = new List<ImportTraineeViewModel>();
            importModel = DataUtils.ReadEmployeeXlsx(staffFile.InputStream);

            return View("ImportEmployee", importModel);
        }

        [HttpPost]
       
        public ActionResult Import(List<ImportTraineeViewModel> model)
        {
            //string test = "";
            try
            {

                foreach (var item in model)
                {
                    if (String.IsNullOrEmpty(item.status))
                    {
                        var newTrainee = new Trainee();
                        newTrainee.str_Staff_Id = item.str_Staff_Id;
                        newTrainee.FirstName = item.str_Fullname;
                        newTrainee.str_Email = item.str_Email;
                        newTrainee.str_Station = item.str_Station;
                        newTrainee.str_Cell_Phone = item.str_Cell_Phone;
                        newTrainee.str_Place_Of_Birth = item.str_BirthPlace;
                        newTrainee.dtm_Birthdate = item.dtm_Birthdate;
                        newTrainee.bit_Internal = true;
                        newTrainee.dtm_Created_At = DateTime.Now;
                        newTrainee.str_Created_By = User.Identity.Name;
                        if (!String.IsNullOrEmpty(item.Department))
                        {
                            newTrainee.Department_Id = getDepartmentId(item.Department);
                        }

                        if (!String.IsNullOrEmpty(item.Job_Title))
                        {
                            newTrainee.Job_Title_id = getJobTitleId(item.Job_Title);
                        }
                        EmployeeService.Insert(newTrainee);
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                String Error = ExceptionUtils.getEntityValidationErrors(ex);
                ExceptionUtils.logError("ImportEmployee - Create", Error);
                ViewBag.Message = Error;
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("ImportEmployee - Create", ex.ToString());
                ViewBag.Message = ex.Message;
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadPartnerList(HttpPostedFileBase staffFile)
        {

            var importModel = new List<ImportTraineeViewModel>();
            importModel = DataUtils.ReadPartnerXlsx(staffFile.InputStream);

            return View("ImportPartner", importModel);
        }

        [HttpPost]
        public ActionResult ImportPartner(List<ImportTraineeViewModel> model)
        {
            string test = "";
            var sdb = new EFDbContext();
            try
            {
                foreach (var item in model)
                {
                    if (string.IsNullOrEmpty(item.status))
                    {
                        var newTrainee = new Trainee();
                        newTrainee.str_Staff_Id = item.str_Staff_Id;
                        newTrainee.FirstName = item.str_Fullname;
                        newTrainee.str_Email = item.str_Email;
                        newTrainee.str_Station = item.str_Station;
                        newTrainee.str_Cell_Phone = item.str_Cell_Phone;
                        newTrainee.dtm_Birthdate = item.dtm_Birthdate;
                        newTrainee.bit_Internal = false;
                        newTrainee.dtm_Created_At = DateTime.Now;
                        newTrainee.str_Created_By = User.Identity.Name;
                        newTrainee.str_Place_Of_Birth = item.str_BirthPlace;
                        if (!string.IsNullOrEmpty(item.Company))
                        {
                            newTrainee.Company_Id = getCompanyId(item.Company);
                        }


                        if (!string.IsNullOrEmpty(item.Job_Title))
                        {
                            newTrainee.Job_Title_id = getJobTitleId(item.Job_Title);
                        }
                        sdb.Trainees.Add(newTrainee);
                        //EmployeeService.Insert(newTrainee);
                        //test = newTrainee.FirstName;
                    }
                }
                sdb.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var Error = ExceptionUtils.getEntityValidationErrors(ex);
                ExceptionUtils.logError("ImportEmployee - Create", Error);
                ViewBag.Message = Error;
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("ImportEmployee - Create", ex.ToString());
                ViewBag.Message = ex.Message;
            }
            return RedirectToAction("Index");
        }

        private int getDepartmentId(String DepartmentName)
        {
            var dbDep = DepartmentService.Get(m => m.Name == DepartmentName, CurrentUser.PermissionIds).FirstOrDefault();
            if (dbDep == null)
            {
                dbDep = new Department { Name = DepartmentName, IsDeleted = false, IsActive = true };
                DepartmentService.Insert(dbDep);
            }
            return dbDep.Id;
        }

        private int getCompanyId(String CompanyName)
        {
            var dbCom = _repoCompany.Get(m => m.str_Name == CompanyName && m.bit_Deleted==false).FirstOrDefault();
            if (dbCom == null)
            {
                dbCom = new Company();
                dbCom.str_Name = CompanyName;
                dbCom.bit_Deleted = false;
                _repoCompany.Insert(dbCom);
            }
            return dbCom.Company_Id;
        }

        private int getJobTitleId(String JobTitleName)
        {
            var dbJobTitle = _repoJob_Tiltle.Get(m => m.Name == JobTitleName).FirstOrDefault();
            if (dbJobTitle == null)
            {
                dbJobTitle = new JobTitle();
                dbJobTitle.Name = JobTitleName;
                dbJobTitle.IsDelete = false;
                _repoJob_Tiltle.Insert(dbJobTitle);
            }
            return dbJobTitle.Id;
        }

        [HttpPost]
        public JsonResult TraineeResultList(int Trainee_Id, int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            return null;
            //try
            //{
            //    var query = _repoCourse.GetCourseResult(m => m.TraineeId == Trainee_Id && !m.bit_Deleted && m.SubjectDetail.IsDelete != true).ToList();

            //    return Json(new
            //    {
            //        Result = "OK",
            //        Records = query.Select(m => new TraineeResultViewModel
            //        {
            //            Course_Result_Id = m.Course_Result_Id,
            //            Subject_Id = m.SubjectDetailId,
            //            First_Check_Result = (m.SubjectDetail.bit_ScoreOrResult == true ? (m.First_Check_Score == null ? "" : m.First_Check_Score.Value.ToString()) : m.First_Check_Result),
            //            Re_Check_Result = (m.SubjectDetail.bit_ScoreOrResult == true ? (m.Re_Check_Score == null ? "" : m.Re_Check_Score.Value.ToString()) : m.Re_Check_Result),
            //            Remark = m.Remark,
            //            ResultType = (m.SubjectDetail.bit_ScoreOrResult == true ? "Score" : "Pass/Fail"),
            //            Editable = (m.Course_Detail_Id == null),
            //            Learning_From = m.Learning_From,
            //            Learning_To = m.Learning_To
            //        }),

            //    });
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { Result = "ERROR", Message = ex.Message });
            //}
        }

        public JsonResult DeleteResult(TraineeResultViewModel result)
        {
            try
            {

                var item = CourseService.GetCourseResultById(result.Course_Result_Id);
                if (item.CourseDetailId != null)
                {
                    return Json(new
                    {
                        Result = "ERROR",
                        Message = Messege.CANNOTDELETE_RESULT //"Can not delete Result from course."
                    });
                }
                CourseService.UpdateCourseResult(item);
                return Json(new { Result = "OK" });

            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("DeleteResult", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = Messege.FORM_NOTVALID //"Form is not valid! Please correct it and try again."
                });
            }
        }
        [HttpPost]
        public JsonResult GetSubjectOption()
        {
            try
            {
                var subjectList = _repoSubject.Get(m => m.bit_Deleted==false).ToList();

                return Json(new { Result = "OK", Options = subjectList.Select(m => new { DisplayText = m.str_Name, Value = m.Subject_Id }) });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Create(int? id)
        {
            if (!id.HasValue)
            {
                loaddata();
                var groupSubject = _repoSubject.GetGroupSubject();

                // check data instructor old & new ---------------
                ViewBag.groupSubject = groupSubject;
                //ViewBag.ParentSubjectlist = parentSubjectlist;
                ViewBag.LoadDepartment = loaddepartment(null, 1, null);
                return View(new InstructorValidation() { Departments = GetDepartmentModel().ToDictionary(a => a.Id, a => a.Code +" - "+ a.DepartmentName), Jobtitles = _repoJob_Tiltle.Get().OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name), Subjects = _repoSubject.GetSubjectDetail( a => a.IsActive==true).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => string.Format("{0} - {1}",a.Code,a.Name)), });
            }
            else
            {
                var entity = EmployeeService.GetById(id);
                if (entity == null)
                {
                    TempData["Notification"] = new { result = false, message = "Instructor is not found" };
                    return RedirectToAction("Index");
                }

                ViewBag.ddl_DEPARTMENT = new SelectList(DepartmentService.Get(null, CurrentUser.PermissionIds).OrderBy(m => m.Name), "Id", "Name", entity.Department_Id);
                ViewBag.ddl_PARTNER = new SelectList(_repoCompany.Get().OrderBy(m => m.str_Name), "Company_Id", "str_Name", entity.Company_Id);
                ViewBag.ddl_NATIONALITY = new SelectList(_repoConfig.GetNations().OrderBy(m => m.Nation_Name), "Nation_Code", "Nation_Name", entity.Nation);
                ViewBag.ddl_GENDER = new SelectList(UtilConstants.GenderDictionary(), "Key", "Value", entity.Gender);

                ViewBag.ddl_PLACEOFBIRTH = new SelectList(ConfigService.GetProvince(), "Value", "Value", entity.str_Place_Of_Birth);
                ViewBag.ddl_TYPE = new SelectList(UtilConstants.CourseCourseAreasDictionary(), "Key", "Value", entity.bit_Internal==true ? (int)UtilConstants.CourseAreas.Internal : (int)UtilConstants.CourseAreas.External);

                var model = new InstructorValidation()
                {
                    PathEducation = GetByKey("PathEducation"),
                    Id = entity.Id,
                    Eid = entity.str_Staff_Id,
                    //FirstName = entity.FirstName,
                    //LastName = entity.LastName,
                    //FullName = entity.FirstName.Trim() + " " + entity.LastName.Trim(),
                    FullName = ReturnDisplayLanguage(entity.FirstName,entity.LastName),
                    Mail = entity.str_Email,
                    Phone = entity.str_Cell_Phone,
                    Birthdate = entity.dtm_Birthdate,
                    CompanyId = entity.Company_Id,
                    EmployeeType = entity.bit_Internal==true ? (int)UtilConstants.CourseAreas.Internal : (int)UtilConstants.CourseAreas.External,
                    Contracts = entity.Trainee_Contract.Select(a =>
                       new InstructorContract()
                       {
                           ContractNo = a.contractno,
                           Description = a.description,
                           ExpireDate = a.expire_date,
                           Id = a.id
                       }),
                    Department_Id = entity.Department_Id,
                    Departments = GetDepartmentModel().ToDictionary(a => a.Id, a => a.Code + " - " + a.DepartmentName),
                    Jobtitles = _repoJob_Tiltle.Get().OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name),
                    TrainingAllowance = entity.Allowance?.ToString(),
                    Passport = entity.Passport,
                    PersonalId = entity.PersonalId,
                    JoinedDate = entity.dtm_Join_Date,
                    JobTitleId = entity.Job_Title_id,
                    Subjects = _repoSubject.GetSubjectDetail(a => a.IsActive==true).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => string.Format("{0} - {1}",a.Code,a.Name)),
                    NameImage = entity.avatar,
                    Educations =
                    entity.Trainee_Record.Select(
                        a =>
                            new TraineeEducation()
                            {
                                Id = a.Trainee_Record_Id,
                                TimeTo = a.dtm_time_to,
                                CourseName = a.str_Subject,
                                Note = a.str_note,
                                Organization = a.str_organization,
                                TimeFrom = a.dtm_time_from,
                                FileUploads = a.Trainee_Upload_Files.Where(b => (b.IsDeleted == false || b.IsDeleted == null)).Select(c => new TraineeEducation.FileUpload
                                {
                                    Id = c.Id,
                                    ModelNameImg = c.Name,
                                    IsDeleted = c.IsDeleted == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No

                                }).ToList()
                            }),
                    InstructorSubject = entity.Instructor_Ability.Select(x => new InstructorSubject
                    {
                        Allowance = x.Allowance ?? 0,
                        Id = x.id,
                        Name = x.SubjectDetail.Name,
                        Code = x.SubjectDetail.Code,
                        SubjectId = (int)x.SubjectDetailId,
                        InstructorId = (int)x.InstructorId,
                        CreateDate = x.CreateDate
                    }),
                    Abilities = entity.Instructor_Ability.Select(a => (int)a.SubjectDetailId)
                };


                var groupSubject = _repoSubject.GetGroupSubject();

                var instructorId = id;
                ViewBag.groupSubject = groupSubject;
                ViewBag.Instructer = EmployeeService.GetById(instructorId);
                ViewBag.LoadDepartment = loaddepartment(null, 1, EmployeeService.GetById(instructorId)?.Department_Id);
                return View(model);
            }
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                return RedirectToAction("Index");
            }
            var entity = EmployeeService.GetById(id);
            if (entity == null)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = string.Format(Messege.DATA_ISNOTFOUND, Resource.lblInstructor, "<font color='red' >" + id.Value + "</font>") };
                return RedirectToAction("Index");
            }
            return View(entity);
        }

        public ActionResult AjaxHandlerTrainingCompetecy(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].Trim());
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                          : c.Code);

                var filtered = Request["sSortDir_0"] == "asc"
                    ? _repoSubject.GetSubjectDetail(a => a.IsActive==true && a.Instructor_Ability.Any(x => x.InstructorId == id)).OrderBy(orderingFunction)
                    : _repoSubject.GetSubjectDetail(a => a.IsActive==true && a.Instructor_Ability.Any(x => x.InstructorId == id)).OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    c.Code,
                                    c.Name
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult AjaxHandlerTrainee_Courses(jQueryDataTableParamModel param)
        {
            try
            {
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].Trim());

                var datamenberapproval = _repoCoursemember.Get(a=>a.Member_Id == id);
                var datacourse = _repoCourse_Detail.Get(a => datamenberapproval.Any(x => x.Course_Details_Id == a.Id),new[] { (int)UtilConstants.ApproveType.SubjectResult }).Select(x => x.Course).Distinct();
                IEnumerable<Course> filtered = datacourse;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course, object> orderingFunction = (c
                                                          => sortColumnIndex == 2 ? c.Code
                                                            : sortColumnIndex == 3 ? c.Name
                                                            : sortColumnIndex == 4 ? (object)c.StartDate
                                                          : c.StartDate);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                     "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+c.Code+"</a></span>",
                                    "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+c.Name+"</a></span>",
                                    DateUtil.DateToString(c?.StartDate,"dd/MM/yyyy") +" - "+ DateUtil.DateToString(c?.EndDate,"dd/MM/yyyy"),
                                    c.Course_Result_Final.FirstOrDefault(a=>a.traineeid == id)?.SRNO,
                                    "<span data-value='" + c.Id + "' class='expand' style='cursor: pointer;'><a><i class='fa fa-plus-circle' aria-hidden='true' style=' font-size: 16px; '></i></a></span>"

                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult AjaxHandlerSubject2(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                int id_ = string.IsNullOrEmpty(Request.QueryString["id_"]) ? -1 : Convert.ToInt32(Request.QueryString["id_"].Trim());
                int instructorId = id_;

                var dataCourse_Detail_Id = _repoCourse_Detail.Get(a => a.CourseId == id, new[] { (int)UtilConstants.ApproveType.SubjectResult }).ToList().Select(a => a.Id);
                var data = _repoCoursemember.Get(a => dataCourse_Detail_Id.Contains((int)a.Course_Details_Id) && a.IsDelete == false && a.Member_Id == instructorId);


                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Course_Detail?.dtm_time_from
                                                            : sortColumnIndex == 2 ? c?.Course_Detail?.SubjectDetail?.Name
                                                            : sortColumnIndex == 3 ? c?.Trainee?.Department?.Name
                                                            : sortColumnIndex == 7 ? (object)c?.Course_Detail?.SubjectDetail?.RefreshCycle
                                                            : c?.Course_Detail?.dtm_time_from);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                   : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);


                var resultA = from c in displayed.ToArray()
                              select new ProfileSubjectModel
                              {
                                  dtm_from = c?.Course_Detail?.dtm_time_from,
                                  dtm_from_to = c?.Course_Detail?.dtm_time_from.Value.ToString("dd/MM/yyyy")??"" + " - " +c?.Course_Detail?.dtm_time_to.Value.ToString("dd/MM/yyyy")??"",
                                  subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                  TypeLearning = TypeLearningIcon(c?.Course_Detail?.type_leaning),
                                  point = string.Format("{0:0.#}", GetPointRemark(UtilConstants.DetailResult.Score, c?.Member_Id, c?.Course_Details_Id)),
                                  remark = GetPointRemark(UtilConstants.DetailResult.Remark, c?.Member_Id, c?.Course_Details_Id),
                                  grade = c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.point != null ? ReturnResult(c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Course_Detail?.SubjectDetail?.Subject_Score, c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.point, c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Result) : string.Empty,

                                  recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                                  courseDetails = c?.Course_Detail,
                                  memberId = c?.Member_Id

                              };

                resultA = resultA.OrderBy(a => a.dtm_from).ToList();
                for (int i = 0; i < resultA.Count(); i++)
                {
                    if (i != 0 && resultA.ElementAt(i).subjectName == resultA.ElementAt(i - 1).subjectName)
                    {
                        resultA.ElementAt(i).ex_Date = ResturnExpiredate(resultA.ElementAt(i).courseDetails, resultA.ElementAt(i).memberId, resultA.ElementAt(i - 1).ex_Date);
                    }
                    else
                    {
                        resultA.ElementAt(i).ex_Date = ResturnExpiredate(resultA.ElementAt(i).courseDetails, resultA.ElementAt(i).memberId, resultA.ElementAt(i).ex_Date);
                    }


                }

                resultA = resultA.OrderByDescending(a => a.dtm_from);


                var result = from c in resultA
                             select new [] {
                                 string.Empty,
                                 c?.dtm_from_to,
                                 c?.subjectName,
                                 c?.TypeLearning,
                                 c?.point     ,
                                 c?.grade      ,
                                 c?.remark     ,
                                 c?.recurrent  ,
                                 (c?.grade == "Fail" || string.IsNullOrEmpty(c?.grade)) ? string.Empty : c?.ex_Date?.ToString("dd/MM/yyyy")

                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
      
      

        public ActionResult AjaxHandlerCourse2(jQueryDataTableParamModel param, string id = "")
        {
            try
            {
                int id_instructor = string.IsNullOrEmpty(Request.QueryString["id_"]) ? -1 : Convert.ToInt32(Request.QueryString["id_"].Trim());

                var dataCourse_Detail = _repoCourse_Detail.Get(a => a.SubjectDetail.Code.Equals(id) && a.Course_Detail_Instructor.Any(x => x.Instructor_Id == id_instructor) && a.Course.IsActive == true && a.IsDeleted==false && a.SubjectDetail.IsDelete==false);


                IEnumerable<Course_Detail> filtered = dataCourse_Detail;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Course?.Code
                                                            : sortColumnIndex == 2 ? c?.Course?.Name
                                                            : sortColumnIndex == 3 ? c?.dtm_time_from
                                                            : sortColumnIndex == 4 ? c?.dtm_time_to
                                                            : sortColumnIndex == 5 ? (object)c?.Room?.str_Name
                                                            : c.dtm_time_from);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                 string.Empty,
                                                 c?.Course?.Code,
                                                 c?.Course?.Name,
                                                  DateUtil.DateToString( c?.dtm_time_from ,"dd/MM/yyyy")+"<br />"+
                                                   (c?.time_from != null ? (c?.time_from?.ToString().Substring(0, 2)+":"+c?.time_from?.ToString().Substring(2)) : ""),

                                                    DateUtil.DateToString( c?.dtm_time_to ,"dd/MM/yyyy")+"<br />"+
                                                    (c?.time_to != null ? (c?.time_to?.ToString().Substring(0, 2)+":"+c?.time_to?.ToString().Substring(2)) : ""),
                                                    c?.Room?.str_Name,
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public string returnSubjectResult(Course_Detail courseDetail)
        {
            return null;
            //var date =
            // ((courseDetail.Course_Result.All(x => x.Re_Check_Result == null) ? courseDetail.Course_Result.All(x => x.First_Check_Result == "P") : courseDetail.Course_Result.All(x => x.Re_Check_Result == "P"))
            //    && courseDetail.TMS_APPROVES.FirstOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?.int_id_status == (int)UtilConstants.EStatus.Approve) ? courseDetail?.SubjectDetail?.RefreshCycle : null;
            //if (date == 0 || date == null) return "";
            //return courseDetail.Course.EndDate.AddMonths(date.Value).ToString("dd/MM/yyyy");
        }
        public ActionResult AjaxHandlerTable_Subjects(jQueryDataTableParamModel param)
        {
            try
            {
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].Trim());
                int instructorId = id;

                var dt = _repoCourse_Detail.Get(a => a.Course_Detail_Instructor.Any(x => x.Instructor_Id == instructorId) && a.Course.IsActive == true && a.Course.IsDeleted == false).Select(a => a.SubjectDetailId);
                var dtsubject = _repoSubject.GetSubjectDetail(a => a.IsActive==true && dt.Contains(a.Id));

                IEnumerable<SubjectDetail> filtered = dtsubject;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                          : c.Code);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Select(a => new { a.Code, a.Name }).Distinct().Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 "<span data-value='"+c.Code +"' class='expand' style='cursor: pointer;'>+</span>",
                                 string.Empty,
                                    c.Code,
                                    c.Name
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }



        public ActionResult AjaxHandlerEducation(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].Trim());
                var data = EmployeeService.GetRecord(a => a.Trainee_Id == id).OrderBy(p => p.str_Subject);

                List<Trainee_Record> models = data.ToList();
                IEnumerable<Trainee_Record> filtered = models;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee_Record, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.dtm_time_from.ToString()
                                                            : sortColumnIndex == 2 ? c.str_Subject
                                                             : sortColumnIndex == 3 ? c.str_organization
                                                             : sortColumnIndex == 4 ? c.str_note
                                                          : c.Trainee_Record_Id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    c.dtm_time_from?.ToString("dd/MM/yyyy") +" - "+ c.dtm_time_to?.ToString("dd/MM/yyyy"),
                                    c.str_Subject,
                                     c.str_organization,
                                      c.str_note
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult AjaxHandlerContract(jQueryDataTableParamModel param)
        {
            try
            {
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].Trim());
                int instructorId = id;

                var data = EmployeeService.GetContract(a => a.Trainee_Id == instructorId).OrderBy(p => p.id);

                IEnumerable<Trainee_Contract> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee_Contract, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.contractno
                                                            : sortColumnIndex == 2 ? c?.expire_date.ToString()
                                                             : sortColumnIndex == 3 ? c?.description
                                                          : c.id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                 c?.contractno,
                                    DateUtil.DateToString(c?.expire_date,"dd/MM/yyyy"),
                                    c?.description
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        #region[loadata]
        private void loaddata()
        {
            ViewBag.ddl_DEPARTMENT = new SelectList(DepartmentService.Get(null, CurrentUser.PermissionIds), "Id", "Name");
            ViewBag.ddl_PARTNER = new SelectList(_repoCompany.Get(), "Company_Id", "str_Name");
            ViewBag.ddl_NATIONALITY = new SelectList(_repoConfig.GetNations(), "Nation_Code", "Nation_Name");
            ViewBag.ddl_GENDER = new SelectList(UtilConstants.GenderDictionary(), "Key", "Value");
            ViewBag.ddl_PLACEOFBIRTH = new SelectList(_repoConfig.GetProvince(), "Value", "Value");
            ViewBag.ddl_TYPE = new SelectList(new[]
           {
                new { Id = "1", Name = "Internal" },
                new { Id = "2", Name = "External" },
            }, "Id", "Name");
        }

        #endregion
        [HttpPost]
        public ActionResult CreateEID(int valuetype = -1, int valuepartner = -1)//Create
        {
            #region[]
            string str_start = "TRN";
            var datapartner = _repoCompany.GetById(valuepartner);
            if (datapartner != null)
            {
                str_start = datapartner?.str_code?.ToString();
            }
            #endregion
            string EID = "";
            if (valuetype == (int)UtilConstants.CourseAreas.External)
            {
                var data = EmployeeService.Get(a => a.str_Staff_Id.StartsWith(str_start));
                for (int i = data.Count() + 1; ; i++)
                {
                    #region[10 ki tu]
                    //if (i <= 9)
                    //{
                    //    EID = str_start + "000000000" + i.ToString();
                    //}
                    //else if (i > 9 && i <= 99)
                    //{
                    //    EID = str_start + "00000000" + i.ToString();

                    //}
                    //else if (i > 99 && i <= 999)
                    //{
                    //    EID = str_start + "0000000" + i.ToString();
                    //}
                    //else if (i > 999 && i <= 9999)
                    //{
                    //    EID = str_start + "000000" + i.ToString();
                    //}
                    //else if (i > 9999 && i <= 99999)
                    //{
                    //    EID = str_start + "00000" + i.ToString();
                    //}
                    //else if (i > 99999 && i <= 999999)
                    //{
                    //    EID = str_start + "0000" + i.ToString();
                    //}
                    //else if (i > 999999 && i <= 9999999)
                    //{
                    //    EID = str_start + "000" + i.ToString();
                    //}
                    //else if (i > 9999999 && i <= 99999999)
                    //{
                    //    EID = str_start + "00" + i.ToString();
                    //}
                    #endregion 4 ki ty
                    if (i <= 9)
                    {
                        EID = str_start + "0000" + i.ToString();
                    }
                    else if (i > 9 && i <= 99)
                    {
                        EID = str_start + "000" + i.ToString();
                    }
                    else if (i > 99 && i <= 999)
                    {
                        EID = str_start + "00" + i.ToString();
                    }
                    else if (i > 999 && i <= 9999)
                    {
                        EID = str_start + "0" + i.ToString();
                    }
                    else if (i > 9990 && i <= 99999)
                    {
                        EID = str_start + i.ToString();
                    }

                    var data_ = EmployeeService.Get(a => a.str_Staff_Id == EID);
                    if (data_.Count() == 0)
                    {
                        break;
                    }
                }
            }
            return Json(EID);
        }

        [HttpPost]
        public ActionResult checkselectsubject(string id, FormCollection form)//Create
        {
            var checkselectvalue_ = !String.IsNullOrEmpty(form["int_khoidaotao"]) ? form["int_khoidaotao"].Split(new char[] { ',' }).Select(Int32.Parse).ToArray() : null;
            StringBuilder HTML = new StringBuilder();
            HTML.Append("<ul>");
            HTML.Append("<li><input class='assignedParentFunc-1' value='-1' multiple type='checkbox' id='checkAll'/><span>Other</span>");
            HTML.Append("<ul>");
            if (checkselectvalue_ != null)
            {
                var datakhoidaotao = _repoSubject.Get().Select(a => a.Subject_Id);
                var data = _repoSubject.Get(a => datakhoidaotao.Contains(a.Subject_Id) && a.bit_Deleted==false
               && a.int_Parent_Id != a.Subject_Id
                && a.int_Parent_Id != null).GroupBy(a => a.str_Name).Select(b => b.First());
                foreach (var item in data)
                {
                    HTML.Append("<li>");
                    if (string.IsNullOrEmpty(id))
                    {
                        HTML.AppendFormat(" <input data-id='-1' data-parentname='Other' multiple value='{0}' class='assignFunc' name='assignFunc' type='checkbox' /><input type='hidden' value='{1}' name='assignFunc2' /><span>{2}</span>", item.Subject_Id, item.Subject_Id, item?.str_Name);
                    }
                    else
                    {
                        int instructorId = Int32.Parse(id);
                        var listInstructorSubject =
                            EmployeeService.GetAbility(a => a.InstructorId == instructorId && a.SubjectDetailId == item.Subject_Id);
                        if (listInstructorSubject.Count() > 0)
                        {
                            HTML.AppendFormat(" <input data-id='-1' data-parentname='Other'  multiple value='{0}' class='assignFunc' name='assignFunc' type='checkbox' Checked /><input type='hidden' value='{1}' name='assignFunc2' /><span>{2}</span>", item.Subject_Id, item.Subject_Id, item?.str_Name);
                        }
                        else
                        {
                            HTML.AppendFormat(" <input data-id='-1' data-parentname='Other'  multiple value='{0}' class='assignFunc' name='assignFunc' type='checkbox' /><input type='hidden' value='{1}' name='assignFunc2' /><span>{2}</span>", item.Subject_Id, item.Subject_Id, item?.str_Name);
                        }

                    }

                    HTML.Append("</li>");
                }
            }

            HTML.Append("</ul>");
            HTML.Append("</li>");
            HTML.Append("</ul>");
            return Json(HTML.ToString());
        }


        [HttpPost]
        public JsonResult ChangeDepartmentReturnJobtitle(string id_department, string id)
        {
            try
            {
                if (!CMSUtils.IsNull(id_department))
                {
                    int id_department_ = Int32.Parse(id_department);
                    int id_ = -1;
                    if (id != null)
                    {
                        id_ = Int32.Parse(id);
                    }
                    StringBuilder html = new StringBuilder();
                    var data = _repoJob_Tiltle.Get();
                    var datauser = EmployeeService.GetById(id_);
                    int jobtitleid = -1;
                    if (datauser != null)
                    {
                        jobtitleid = (int)datauser.Job_Title_id;
                    }
                    if (data != null)
                    {
                        //html.Append("<select name='Job_Title_id' id='ddl_JOBTITLE' class='form-control' data-placeholder='-- JOB TITLE --'> <option></option>");
                        foreach (var item in data)
                        {
                            if (jobtitleid == item.Id)
                            {
                                html.AppendFormat("<option value='{0}' selected>{1}</option>", item.Id, item?.Name);
                            }
                            else
                            {
                                html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item?.Name);
                            }

                        }
                        //html.Append("</select>");
                    }
                    return Json(new
                    {
                        htmlout = html.ToString()
                    });
                }
                return Json(new
                {
                    htmlout = ""
                });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


        [HttpPost]
        public ActionResult RemoveContract(string id)
        {
            if (CMSUtils.DeleteDataSQLNoLog("id", id, "Trainee_Contract") >= 1)
            {
                return Json(CMSUtils.alert("success", Messege.SUCCESS_REMOVE_CONTRACT));
            }
            return Json(CMSUtils.alert("danger", Messege.UNSUCCESS_REMOVE_CONTRACT));
        }
        [HttpPost]
        public ActionResult RemoveEdu(string id)
        {
            if (CMSUtils.DeleteDataSQLNoLog("Trainee_Record_Id", id, "Trainee_Record") >= 1)
            {
                return Json(CMSUtils.alert("success", Messege.SUCCESS_REMOVE_EDU));
            }
            return Json(CMSUtils.alert("danger", Messege.UNSUCCESS_REMOVE_EDU));
        }
        [HttpPost]
        public async Task<ActionResult> delete(int id = -1)
        {
            try
            {
                var model = EmployeeService.GetById(id);
                if (model == null)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //var fullName = model.FirstName.Trim() + " " + model.LastName.Trim();
                var fullName = ReturnDisplayLanguage(model.FirstName, model.LastName);
                if (model.Course_Detail_Instructor.Any(a => a.Course_Detail.IsDeleted==false))
                {

                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, fullName),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                model.IsActive = false;
                model.IsDeleted = true;
                model.LmsStatus = statusModify;
                model.dtm_Deleted_At = DateTime.Now;
                model.str_Deleted_By = CurrentUser.USER_ID.ToString();
                EmployeeService.Update(model);

                await Task.Run(() =>
                {
                    #region [--------CALL LMS----------]
                    var callLms = CallServices(UtilConstants.CRON_USER);
                    if (!callLms)
                    {
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, fullName),
                        //    result = false
                        //});
                    }
                    #endregion
                });

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, fullName),
                    result = true
                }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public async Task<ActionResult> Create(InstructorValidation model)//Create
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { result = false, message = MessageInvalidData(ModelState) });
                if (model.Id.HasValue)
                {
                    //update avatar or not
                    if (model.ImgFile != null)
                    {
                        var newAvatar = SaveImage(model.ImgFile,model.Eid,UtilConstants.Upload.Trainee);
                        if (newAvatar.result)
                        {
                            model.ImgAvatar = newAvatar.data.ToString();
                        }
                        else
                        {
                            return Json(newAvatar);
                        }
                    }
                }
                else
                {
                    var newAvatar = SaveImage(model.ImgFile, model.Eid, UtilConstants.Upload.Trainee);
                    if (newAvatar.result)
                    {
                        model.ImgAvatar = newAvatar.data.ToString();
                    }
                    else
                    {
                        return Json(newAvatar);
                    }

                }

                // upload bằng
                if (model.Educations.Any() || model.Educations != null)
                {
                    foreach (var edu in model.Educations)
                    {
                        if (edu.FileUploads == null) continue;
                        var listNameImg = new List<string>();
                        foreach (var img in edu.FileUploads.Where(img => img.FIleImage?[0] != null))
                        {
                            foreach (var newImg in img.FIleImage.Select(item => SaveImage(item, model.Eid, UtilConstants.Upload.Education)))
                            {
                                if (newImg.result)
                                {
                                    listNameImg.Add(newImg.data.ToString());

                                }
                                else
                                {
                                    return Json(newImg);
                                }
                            }
                            img.ListNameImage = listNameImg.ToArray();
                        }
                    }
                }
                //end upload bằng
                // var employee = EmployeeService.Modify(model, CurrentUser.USER_ID);
                var randomPass = Common.RandomCharecter();
                var entity = EmployeeService.Modify(model,  Common.EncryptString(randomPass));
                await Task.Run(() =>
                {
                    if (entity != null)
                    {


                        //gui mail
                        if (1 == 1)
                        {
                            var checksite = GetByKey(UtilConstants.KEY_SENT_EMAIL_CHANGE_PASSWORD);
                            if (checksite.Equals("1"))
                            {
                                Sent_Email_TMS(entity, null, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee);
                            }

                        }

                        #region [--------CALL LMS (CRON USER)----------]
                        var callLms = CallServices(UtilConstants.CRON_USER);
                        if (!callLms)
                        {

                            //return Json(new AjaxResponseViewModel()
                            //{
                            //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), entity.str_Staff_Id),
                            //    result = false
                            //});
                        }
                        #endregion
                    }
                });
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS };
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS });

            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
            return Json(new AjaxResponseViewModel()
            {
                result = false,
                message = MessageInvalidData(ModelState)
            });
        }



        #region helper


        public DateTime? ResturnExpiredate(Course_Detail CourseDetails, int? memberid, DateTime? prevExdate)//
        {

            DateTime? returnVal = null;
            if (CourseDetails == null) return null;
            string subjectCode = CourseDetails.SubjectDetail.Code;
            var enumerable = CourseService.GetCourseResult(a => a.TraineeId == memberid && a.Course_Detail.SubjectDetail.Code == subjectCode
                                                   && a.Course_Detail.dtm_time_from <= CourseDetails.dtm_time_from
                                                   && a.Course_Detail.Course_Result_Summary.FirstOrDefault(b => b.TraineeId == memberid && b.CourseDetailId == a.CourseDetailId).Result != "Fail" && a.Course_Detail.TMS_APPROVES.OrderByDescending(x => x.id).FirstOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult)
                                                                                   .int_id_status == (int)UtilConstants.EStatus.Approve
                                                    );

            if (enumerable.Any())
            {
                //lấy 2 phần tử cuối
                var courseResults = enumerable.OrderByDescending(a => a.Course_Detail.dtm_time_from).Take(2).ToList();
                //-- debug
                var lastOrDefault = courseResults.LastOrDefault();
                if (lastOrDefault?.Course_Detail.dtm_time_from != null)
                {
                    var fromdateLast = lastOrDefault.Course_Detail.dtm_time_from;
                }
                var firstOrDefault = courseResults.FirstOrDefault();
                if (firstOrDefault?.Course_Detail.dtm_time_from != null)
                {
                    var fromdateFirst = (DateTime)firstOrDefault.Course_Detail.dtm_time_from;
                    var expiredate = prevExdate;
                    var expiredate3Month = expiredate?.AddMonths(-3);
                    returnVal = returnDateExpireTepm(fromdateFirst, (int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle);

                    if (expiredate3Month < fromdateFirst && fromdateFirst <= expiredate)
                    {
                        returnVal = expiredate?.AddMonths((int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle);
                    }
                }
            }
            else
            {
                var courseResult = enumerable.FirstOrDefault();
                if (courseResult?.Course_Detail?.dtm_time_to != null)
                    returnVal = returnDateExpireTepm((DateTime)courseResult.Course_Detail.dtm_time_to, (int)courseResult.Course_Detail.SubjectDetail.RefreshCycle);
            }
            return returnVal;
        }

        private DateTime? returnDateExpireTepm(DateTime fromdate, int cycle)
        {
            if (cycle == 0) return null;
            return (new DateTime(fromdate.Year, fromdate.Month, 1).AddMonths(1).AddDays(-1)).AddMonths(cycle);
        }

        #endregion

        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateInstructor(int isParticipate, string id)
        {
            int idInstructor = int.Parse(id);
            var removeInstructor = EmployeeService.GetById(idInstructor);
            if (removeInstructor == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false

                }, JsonRequestBehavior.AllowGet);
            if (isParticipate == 1)
            {
                removeInstructor.IsActive = false;
            }
            else
            {
                removeInstructor.IsActive = true;
            }
            removeInstructor.LmsStatus = statusModify;
            EmployeeService.Update(removeInstructor);
            //var fullName = removeInstructor.FirstName.Trim() + " " + removeInstructor.LastName.Trim();
            var fullName = ReturnDisplayLanguage(removeInstructor.FirstName, removeInstructor.LastName);
            await Task.Run(() =>
            {
                #region [--------CALL LMS (CRON USER)----------]
                var callLms = CallServices(UtilConstants.CRON_USER);
                if (!callLms)
                {
                    //return Json(new AjaxResponseViewModel()
                    //{
                    //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, fullName),
                    //    result = false
                    //});
                }
                #endregion
            });
            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, fullName), result = true }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult FilterSubject(FormCollection form)
        {
            var filterCodeOrName = string.IsNullOrEmpty(form["FilterCodeOrName"])
                ? string.Empty
                : form["FilterCodeOrName"].Trim().ToLower();
            var subjectId = !string.IsNullOrEmpty(form["Subjects[]"]) ? form["Subjects[]"].Split(new char[] { ',' }) : null;
            
            var html = new StringBuilder();
            var data = _repoSubject.GetSubjectDetail(a => a.IsActive==true && (string.IsNullOrEmpty(filterCodeOrName) || a.Code.Trim().ToLower().Contains(filterCodeOrName) || a.Name.Trim().ToLower().Contains(filterCodeOrName))).ToList();

            if (!data.Any())
            {
                return Json(new
                {
                    value = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
            if (data.Any())
            {

                foreach (var item in data)
                {
                    if (subjectId != null && subjectId.Contains(item.Id.ToString()))
                    {
                        html.AppendFormat("<option selected   value='{0}'>{1} - {2}</option>", item.Id, item.Code, item.Name);
                    }
                    else
                    {
                        html.AppendFormat("<option   value='{0}'>{1} - {2}</option>", item.Id, item.Code, item.Name);
                    }
              
                }
            }
            return Json(new
            {
                value = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
