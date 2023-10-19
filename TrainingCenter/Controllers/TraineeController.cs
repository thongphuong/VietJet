using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utilities;
using System.Data;
using System.IO;
using TMS.Core.App_GlobalResources;
using System.Text;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using DAL.Entities;
using DotNetOpenAuth.Messaging;
using TMS.Core.Services.Approves;
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
using TMS.Core.Services.TraineeHis;
using TMS.Core.Services.Users;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.TraineeHistory;
using TMS.Core.ViewModels.Trainees;
using TMS.Core.ViewModels.ViewModel;
using TMS.Core.ViewModels.EmployeeModels;


namespace TrainingCenter.Controllers
{
    using Newtonsoft.Json.Linq;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.Configuration;
    using System.Threading.Tasks;
    using TMS.Core.Services;
    using TMS.Core.ViewModels.Common;
    using TrainingCenter.Template.Report;
    using Utilities;

    public class TraineeController : BaseAdminController
    {
        #region MyRegion

        private readonly IJobtitleService _jobtitleService;
        private readonly ICompanyService _companyService;
        private readonly ISubjectService _subjectService;
        private readonly ITraineeHistoryService _traineeHistoryService;
        private readonly IDepartmentService _departmentService;

        public TraineeController(IConfigService configService, IUserContext userContext,
            INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IJobtitleService jobtitleService, ICompanyService companyService, ISubjectService subjectService, ITraineeHistoryService traineeHistoryService, IApproveService repoApproveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, repoApproveService)
        {
            _jobtitleService = jobtitleService;
            _companyService = companyService;
            _subjectService = subjectService;
            _traineeHistoryService = traineeHistoryService;
            _departmentService = departmentService;
        }

        #endregion
        public ActionResult Index()
        {            
            var model = new TraineeIndexModel();
            model.Departments = LoadDepartment();
            model.Genders = UtilConstants.GenderDictionary();
            model.JobTitles = _jobtitleService.Get().OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name);
            model.Status = new SelectList(new[]
            {
                new { Id = "0", Name = "Active" },
                new { Id = "1", Name = "Deactive" },
            }, "Id", "Name");
            model.Type = new SelectList(new[]
            {
                new {Id = "1", Name = "Internal"},
                new {Id = "0", Name = "External"}
            }, "Id", "Name");

            return View(model);
        }
        #region Print Profile
        public ActionResult EmployeeProfilePrint(int? id)
        {
            var subjectType = (int?)UtilConstants.ApproveType.SubjectResult;
            var model = new EmployeeViewModel()
            {
                Employees = EmployeeService.GetById(id),
                Training_Education = EmployeeService.GetRecordByTraineeId((int)id).OrderBy(p => p.str_Subject),
                Training_Contracts = EmployeeService.GetContract(a => a.Trainee_Id == id).OrderBy(p => p.id),
                Training_Course = CourseDetailService.Get(
                        a => a.TMS_Course_Member.Any(x => x.Member_Id == id) &&
                            a.TMS_APPROVES.Any(x => x.int_Type == subjectType)).ToList()
                            .Where(a =>
                                a.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?.int_id_status == (int)UtilConstants.EStatus.Approve).Select(x => x.Course).Distinct(),
            };
            var entity = EmployeeService.GetById(id);
            return PartialView("EmployeeProfilePrint", model);
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
            string templateFilePath = Server.MapPath(
@"" + GetByKey("PrivateTemplate") + "ExcelFile/InstructorList.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            //var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);
            var userPermission = CurrentUser.PermissionIds;
            // xử lý param gửi lên
            var ComOrDepId = string.IsNullOrEmpty(Request.QueryString["ComOrDepId"]) ? -1 : Convert.ToInt32(Request.QueryString["ComOrDepId"].Trim());
            var fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"].Trim());
            var fJobTitle = string.IsNullOrEmpty(Request.QueryString["fJobTitle"]) ? -1 : Convert.ToInt32(Request.QueryString["fJobTitle"].Trim());
            var fGender = string.IsNullOrEmpty(Request.QueryString["fGender"]) ? -1 : Convert.ToInt32(Request.QueryString["fGender"].Trim());
            var ddl_TYPE = string.IsNullOrEmpty(Request.QueryString["ddl_TYPE"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_TYPE"].Trim());
            var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
            var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? "" : Request.QueryString["fEmail"].Trim();
            var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? "" : Request.QueryString["fPhone"].Trim();
            var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].Trim();
            var ss = true;
            if (ddl_TYPE == 0)
            {
                ss = false;
            }
            var tt = true;
            if (fStatus == 0)
            {
                tt = false;
            }
            var data = EmployeeService.Get(a =>
            userPermission.Any(x => a.Department_Id == x)
            && (ComOrDepId == -1 || a.Department_Id == ComOrDepId)
              && (ddl_TYPE == -1 || a.bit_Internal == ss)
            && (string.IsNullOrEmpty(fName) || (a.FirstName.Contains(fName) || a.LastName.Contains(fName)))
            && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
            && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail))
            && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone))
            && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId))
            && (fGender == -1 || a.Gender == fGender)
            && (fStatus == -1 || a.IsActive == tt));

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
                        cellFullName.Value = ReturnDisplayLanguage(item?.FirstName, item?.LastName);
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
                        cellRemark.Value = item.bit_Internal == true ? "Internal" : "External";
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
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var userPermission = CurrentUser.PermissionIds;
                // xử lý param gửi lên
                var comOrDepId = string.IsNullOrEmpty(Request.QueryString["ComOrDepId"]) ? -1 : Convert.ToInt32(Request.QueryString["ComOrDepId"].Trim());
                var fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"].Trim());
                var fJobTitle = string.IsNullOrEmpty(Request.QueryString["fJobTitle"]) ? -1 : Convert.ToInt32(Request.QueryString["fJobTitle"].Trim());
                var fGender = string.IsNullOrEmpty(Request.QueryString["fGender"]) ? -1 : Convert.ToInt32(Request.QueryString["fGender"].Trim());
                var ddlType = string.IsNullOrEmpty(Request.QueryString["ddl_TYPE"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_TYPE"].Trim());
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? string.Empty : Request.QueryString["fName"].Trim();
                var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? string.Empty : Request.QueryString["fEmail"].Trim();
                var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? string.Empty : Request.QueryString["fPhone"].Trim();
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? string.Empty : Request.QueryString["fStaffId"].Trim();

                var bitInternal = ddlType != 0;
                var isActive = fStatus != 0;

                var data = EmployeeService.Get(a =>
                userPermission.Any(x => a.Department_Id == x)
                && (comOrDepId == -1 || a.Department_Id == comOrDepId)
                  && (ddlType == -1 || a.bit_Internal == bitInternal)
                && (string.IsNullOrEmpty(fName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(fName.Trim())))
                && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
                && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail.Trim()))
                && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone.Trim()))
                && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId.Trim()))
                && (fGender == -1 || a.Gender == fGender)
                && (fStatus == -1 || a.IsActive == isActive), CurrentUser.IsMaster);

                IEnumerable<Trainee> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c.FirstName
                                                            : sortColumnIndex == 3 ? c.Gender
                                                            : sortColumnIndex == 4 ? c.str_Email
                                                            : sortColumnIndex == 5 ? c.str_Cell_Phone
                                                            : sortColumnIndex == 6 ? c.JobTitle?.Name
                                                            : sortColumnIndex == 7 ? c.Department?.Code
                                                            : sortColumnIndex == 8 ? (object)c?.bit_Internal
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
                             //let fullName = c.FirstName.Trim() + " " + c.LastName.Trim()
                             let fullName = ReturnDisplayLanguage(c.FirstName, c.LastName)
                             select new object[] {
                                 string.Empty,
                                    c.str_Staff_Id ?? "",
                                "<a href='"+@Url.Action("Details",new{id = c.Id})+"'>"+ fullName +"</a>",
                                    UtilConstants.GenderDictionary()[gender],
                                    c.str_Email ?? "",
                                    c.str_Cell_Phone ?? "",
                                    c.JobTitle?.Name ?? "",
                                    c.Department?.Code ?? "",
                                    c.bit_Internal==true ? UtilConstants.CourseAreas.Internal.ToString(): UtilConstants.CourseAreas.External.ToString(),
                                    (c.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Trainee(0,"+c.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Trainee(1,"+c.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>") 
                                   /* + ((User.IsInRole("/Trainee/AjaxHandler")) ? "<a href='javascript:void(0)' class='btn btn-primary legitRipple' data-type='primary' onclick='callsendApi(\""+c.str_Email.Trim()+"\",\"" + ( !string.IsNullOrEmpty(c.Password) ?  Command.DecryptString(c.Password.Trim()) : c.Password )+ "\",\""+c.LastName.Trim()+"\",\""+c.FirstName.Trim()+"\")'          ><i class='fa fa-eye' aria-hidden='true' title='Send to DH Da Nang'></i> Send to DH Da Nang</a>&nbsp;" : "")*/,

                                   ((User.IsInRole("/Trainee/Details")) ? "<a title='View' href='"+@Url.Action("Details",new{id = c.Id})+"'><i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; '></i></a>" : "<i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; '></i>" ) +
                                    ((User.IsInRole("/Trainee/Create")) ? verticalBar + "<a title='Edit' href='"+@Url.Action("Create",new{id = c.Id})+"'><i class='fa fa-pencil-square-o' aria-hidden='true' style=' font-size: 16px; '></i></a>":"<i class='fa fa-pencil-square-o' aria-hidden='true' style=' font-size: 16px; '></i>") + verticalBar +
                                    "<a title='New Password' href='javascript:void(0)'  onclick='callrandom(\" "+fullName+" \"," + c.Id  + ")'><i class='fa fa-paper-plane' aria-hidden='true' style=' font-size: 16px; '></i></a>" +
                                    ((User.IsInRole("/Trainee/Delete")) ? verticalBar +"<a title='Delete' href='javascript:void(0)'  onclick='calldelete(" + c.Id  + ")'><i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px; '></i></a>" :"<i class='fa fa-trash-o' aria-hidden='true' style=' font-size: 16px; '></i>")


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
        public async Task<ActionResult> RandomResetPassword(int? id)
        {
            try
            {
                var model = EmployeeService.GetById(id);
                if (model == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var newPass = TMS.Core.Utils.Common.RandomCharecter();
                model.Password = TMS.Core.Utils.Common.EncryptString(newPass);
                model.LmsStatus = StatusModify;
                EmployeeService.Update(model);
                await Task.Run(() =>
                {
                    //var fullName = model.FirstName.Trim() + " " + model.LastName.Trim() + "";
                    var fullName = ReturnDisplayLanguage(model.FirstName, model.LastName) + "";
                    Sent_Email_TMS(null, model, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee);
                    #region [--------CALL LMS----------]
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


                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SUCCESS),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> Delete(int? id)
        {
            try
            {
                var model = EmployeeService.GetById(id);
                if (model == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //var fullName = model.FirstName.Trim() + " " + model.LastName.Trim();
                var fullName = ReturnDisplayLanguage(model.FirstName, model.LastName);

                if (model.TMS_Course_Member.Any(a => a.IsDelete == false))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, fullName),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                if (model.LmsStatus == StatusModify)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_SENDLMS, fullName),
                        result = false
                    }, JsonRequestBehavior.AllowGet);

                }
                model.IsDeleted = true;
                model.IsActive = false;
                model.dtm_Deleted_At = DateTime.Now;
                model.str_Deleted_By = CurrentUser.USER_ID.ToString();

                // Cập nhật Status để gửi LMS
                model.LmsStatus = StatusModify;

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
        public JsonResult GetSubjectOption()
        {
            try
            {
                var subjectList = _subjectService.Get(m => m.bit_Deleted == false).ToList();

                return Json(new AjaxResponseViewModel() { result = true, data = subjectList.Select(m => new { DisplayText = m.str_Name, Value = m.Subject_Id }) });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Create(int? id)
        {
            var model = new Trainee_Validation()
            {
                Educations = new List<TraineeEducation>()
            };
            if (!id.HasValue)
            {
                ViewBag.Trainee = null;
                ViewBag.LoadDepartment = LoadDepartment();
            }
            else
            {
                var entity = EmployeeService.GetById(id, CurrentUser.IsMaster);
                model = new Trainee_Validation()
                {
                    PathEducation = GetByKey("PathEducation"),
                    Id = entity.Id,
                    eid = entity.str_Staff_Id,
                    Company_Id = entity.Company_Id,
                    Department_Id = entity.Department_Id,
                    type = entity.bit_Internal == true ? (int)UtilConstants.CourseAreas.Internal : (int)UtilConstants.CourseAreas.External,
                    dtm_Birthdate = entity.dtm_Birthdate,
                    dtm_Join_Date = entity.dtm_Join_Date,
                    //FirstName = entity.FirstName,
                    // LastName = entity.LastName,
                    //FullName = entity.FirstName.Trim() + " " + entity.LastName.Trim(),
                    FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName),

                    gender = entity.Gender,
                    nameImage = entity.avatar,
                    Job_Title_id = entity.Job_Title_id,
                    mail = entity.str_Email,
                    nation = entity.Nation,
                    passport = entity.Passport,
                    phone = entity.str_Cell_Phone,
                    str_Place_Of_Birth = entity.str_Place_Of_Birth,
                    str_id = entity.PersonalId,
                    JobtitleDictionary = _jobtitleService.Get().OrderBy(m => m.Name).ToDictionary(a => a.Id, a => a.Name),
                    Role = entity.int_Role == (int)UtilConstants.ROLE.Instructor ? (int)UtilConstants.ROLE.Instructor : (int)UtilConstants.ROLE.Trainee,
                    Resignation_Date = entity.non_working_day,
                    Educations = entity.Trainee_Record.Where(a => a.bit_Deleted == false).Select(a => new TraineeEducation()
                    {
                        Id = a.Trainee_Record_Id,
                        IsDeleted = a.bit_Deleted == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No,
                        CourseName = a.str_Subject,
                        Note = a.str_note,
                        Organization = a.str_organization,
                        TimeFrom = a.dtm_time_from,
                        TimeTo = a.dtm_time_to,
                        FileUploads = a.Trainee_Upload_Files.Where(b => (b.IsDeleted == false || b.IsDeleted == null)).Select(c => new TraineeEducation.FileUpload
                        {
                            Id = c.Id,
                            ModelNameImg = c.Name,
                            IsDeleted = c.IsDeleted == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No

                        }).ToList()
                    }).ToList()
                };

                ViewBag.ddl_PARTNER = new SelectList(_companyService.Get(m => m.bit_Deleted == false).OrderBy(m => m.str_Name), "Company_Id", "str_Name", model.Company_Id);
                ViewBag.ddl_NATIONALITY = new SelectList(ConfigService.GetNations().OrderBy(m => m.Nation_Name), "Nation_Code", "Nation_Name", model.nation);
                ViewBag.ddl_GENDER = new SelectList(UtilConstants.GenderDictionary(), "Key", "Value", model.gender);
                ViewBag.ddl_PLACEOFBIRTH = new SelectList(ConfigService.GetProvince(), "Value", "Value", model.str_Place_Of_Birth);
                ViewBag.ddl_TYPE = new SelectList(UtilConstants.CourseCourseAreasDictionary(), "Key", "Value", model.type);

                ViewBag.Trainee = EmployeeService.GetById(id);
                ViewBag.LoadDepartment = LoadDepartment(model.Department_Id);
                return View(model);

            }
            ViewBag.ddl_DEPARTMENT = new SelectList(DepartmentService.Get(null, CurrentUser.PermissionIds).OrderBy(m => m.Name), "Department_Id", "str_Name");
            model.JobtitleDictionary = _jobtitleService.Get().OrderBy(m => m.Name).ToDictionary(a => a.Id, a => a.Name);

            ViewBag.ddl_PARTNER = new SelectList(_companyService.Get(m => m.bit_Deleted == false).OrderBy(m => m.str_Name), "Company_Id", "str_Name");
            ViewBag.ddl_NATIONALITY = new SelectList(ConfigService.GetNations().OrderBy(m => m.Nation_Name), "Nation_Name", "Nation_Name");
            ViewBag.ddl_GENDER = new SelectList(UtilConstants.GenderDictionary(), "Key", "Value");
            ViewBag.ddl_PLACEOFBIRTH = new SelectList(ConfigService.GetProvince(), "Value", "Value");
            ViewBag.ddl_TYPE = new SelectList(UtilConstants.CourseCourseAreasDictionary(), "Key", "Value");

            ViewBag.Trainee = EmployeeService.GetById(id);
            ViewBag.LoadDepartment = LoadDepartment();
            return View(model);
        }
        public void WriteHtmlTable<T>(IEnumerable<T> data, TextWriter output)
        {
            //Writes markup characters and text to an ASP.NET server control output stream. This class provides formatting capabilities that ASP.NET server controls use when rendering markup to clients.
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {

                    //  Create a form to contain the List
                    Table table = new Table();
                    TableRow row = new TableRow();
                    PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
                    foreach (PropertyDescriptor prop in props)
                    {
                        TableHeaderCell hcell = new TableHeaderCell();
                        hcell.Text = prop.Name;
                        hcell.BackColor = System.Drawing.Color.Yellow;
                        row.Cells.Add(hcell);
                    }

                    table.Rows.Add(row);

                    //  add each of the data item to the table
                    foreach (T item in data)
                    {
                        row = new TableRow();
                        foreach (PropertyDescriptor prop in props)
                        {
                            TableCell cell = new TableCell();
                            cell.Text = prop.Converter.ConvertToString(prop.GetValue(item));
                            row.Cells.Add(cell);
                        }
                        table.Rows.Add(row);
                    }

                    //  render the table into the htmlwriter
                    table.RenderControl(htw);

                    //  render the htmlwriter into the response
                    output.Write(sw.ToString());
                }
            }


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
        [HttpGet]
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
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = string.Format(Messege.DATA_ISNOTFOUND, Resource.lblTRAINEE, "<font color='red' >" + id.Value + "</font>") };
                return RedirectToAction("Index");
            }

            return View(entity);
        }
        [AllowAnonymous]
        public ActionResult PartialListJobStandard(int? id, int type = (int)UtilConstants.Switch.Horizontal)
        {

            var traineeHistories =
                _traineeHistoryService.Get(a => a.Trainee_Id == id).OrderByDescending(b => b.Id).ToList();

            var model = new TraineeJobModel()
            {
                Type = type,
                TraineeHistories = traineeHistories,
                //Trainees = EmployeeService.GetById(id),
                //ListSubjectAssign = courseidAssign,
                //SubjectCompleted = completed
            };

            //var getCourseid = CourseService.GetCourseResultFinal(a => a.traineeid == id && a.Course.IsDeleted != true && a.Course.IsActive == true).Select(a => a.courseid).ToArray();

            //var courseidAssign = CourseDetailService.Get(a => getCourseid.Contains(a.CourseId)).Select(a => a.SubjectDetailId).Distinct().ToList();
            //var completed =
            //    _repoApproveService.Get(
            //        a =>
            //            getCourseid.Contains(a.int_Course_id) &&
            //            a.int_Type == (int) UtilConstants.ApproveType.SubjectResult &&
            //            a.int_id_status == (int) UtilConstants.EStatus.Approve)
            //        .Select(b => b.Course_Detail.SubjectDetailId).Distinct()
            //        .ToList();
            //var model = new TraineeJobModel()
            //{
            //    TraineeHistories =
            //        _traineeHistoryService.Get(a => a.Trainee_Id == id).OrderByDescending(b => b.Id).ToList(),
            //    Trainees = EmployeeService.GetById(id),
            //    ListSubjectAssign = courseidAssign,
            //    //SubjectCompleted = completed
            //};
            return PartialView("_partials/_PartialTraineeJobStandard", model);
        }
        [AllowAnonymous]
        public ActionResult PartialDetailSimple(int? id)
        {
            var model = EmployeeService.GetById(id);
            return PartialView("_partials/_PartialTraineeDetailSimple", model);
        }
        public ActionResult AjaxHandlerEducation(jQueryDataTableParamModel param)
        {
            try
            {
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].Trim());
                int instructorId = id;

                var data = EmployeeService.GetRecordByTraineeId(instructorId).OrderBy(p => p.str_Subject);

                List<Trainee_Record> models = data.ToList();
                IEnumerable<Trainee_Record> filtered = models;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee_Record, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.dtm_time_from.ToString()
                                                            : sortColumnIndex == 2 ? c?.str_Subject
                                                             : sortColumnIndex == 3 ? c?.str_organization
                                                             : sortColumnIndex == 4 ? c?.str_note
                                                          : c.Trainee_Record_Id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    c?.dtm_time_from?.ToString("dd/MM/yyyy") +" - "+ c?.dtm_time_to?.ToString("dd/MM/yyyy"),
                                    c?.str_Subject,
                                     c?.str_organization,
                                      c?.str_note
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
            ViewBag.avatar = "";
            ViewBag.hiden_avatar = "0";
            ViewBag.ddl_DEPARTMENT = new SelectList(DepartmentService.Get(null, CurrentUser.PermissionIds).OrderBy(m => m.Name), "Department_Id", "str_Name");
            ViewBag.ddl_JOBTITLE = new SelectList(_jobtitleService.Get().OrderBy(m => m.Name), "Id", "Name");
            ViewBag.ddl_PARTNER = new SelectList(_companyService.Get(m => m.bit_Deleted == false).OrderBy(m => m.str_Name), "Company_Id", "str_Name");
            ViewBag.ddl_NATIONALITY = new SelectList(ConfigService.GetNations().OrderBy(m => m.Nation_Name), "Nation_Code", "Nation_Name");
            DataTable db_ = CMSUtils.GetDataSQL("", "CAT_USER_GENDER", "id,name", "", "");
            ViewBag.ddl_GENDER = new SelectList(db_.AsDataView(), "id", "name");
            DataTable db_CAT_PROVINCES = CMSUtils.GetDataSQL("", "CAT_PROVINCES", "id,name", "", "name");
            ViewBag.ddl_PLACEOFBIRTH = new SelectList(db_CAT_PROVINCES.AsDataView(), "name", "name");
            ViewBag.ddl_TYPE = new SelectList(new[]
           {
                new { Id = "1", Name = "Internal" },
                new { Id = "0", Name = "External" },
            }, "Id", "Name");
        }
        private void loaddataedit(string id)
        {

        }
        #endregion

        [HttpPost]
        public ActionResult CreateEID(int valuetype = -1, int valuepartner = -1)//Create
        {
            #region[]
            string str_start = "TRN";
            var datapartner = _companyService.GetById(valuepartner);
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
                    #endregion
                    if (i <= 9)
                    {
                        EID = str_start + "000" + i.ToString();
                    }
                    else if (i > 9 && i <= 99)
                    {
                        EID = str_start + "00" + i.ToString();

                    }
                    else if (i > 99 && i <= 999)
                    {
                        EID = str_start + "0" + i.ToString();
                    }
                    else if (i > 999 && i <= 9999)
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
        #region Export Excel
        //controller Action

        public ActionResult Excel(string ComOrDepId, string fStatus, string ddl_TYPE, string fJobTitle, string fGender, string fName, string fEmail, string fPhone, string fStaffId)
        {

            var deviceInfo = "<DeviceInfo><OutputFormat>EXCEL</OutputFormat></DeviceInfo>";
            var REPORT_EXCEL = "EXCELOPENXML";

            int ComOrDepId_ = string.IsNullOrEmpty(ComOrDepId) ? -1 : Convert.ToInt32(ComOrDepId);
            int fStatus_ = string.IsNullOrEmpty(fStatus) ? -1 : Convert.ToInt32(fStatus);
            int ddl_TYPE_ = string.IsNullOrEmpty(ddl_TYPE) ? -1 : Convert.ToInt32(ddl_TYPE);
            int fJobTitle_ = string.IsNullOrEmpty(fJobTitle) ? -1 : Convert.ToInt32(fJobTitle);
            int fGender_ = string.IsNullOrEmpty(fGender) ? -1 : Convert.ToInt32(fGender);
            bool ss = true;
            if (ddl_TYPE_ == 0)
            {
                ss = false;
            }

            var model1 = EmployeeService.Get(a =>
              (ddl_TYPE_ == -1 || a.bit_Internal == ss)
              && (ComOrDepId_ == -1 || a.Department_Id == ComOrDepId_)
              && (string.IsNullOrEmpty(fName) || (a.FirstName.Contains(fName) || a.LastName.Contains(fName)))
              && (fJobTitle_ == -1 || a.Job_Title_id == fJobTitle_)
              && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail))
              && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone))
              && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId))
              && (fGender_ == -1 || a.Gender == fGender_)
              && (fStatus_ == -1 || a.Suspended == fStatus_));
            ReportDataSet.ReportModelRow detailRow;
            ReportDataSet.ReportModelDataTable coursemmber = new ReportDataSet.ReportModelDataTable();

            int i = 1;
            foreach (var item in model1)
            {
                detailRow = coursemmber.NewReportModelRow();
                detailRow.DataColumn1 = (i++).ToString();
                detailRow.DataColumn2 = (bool)item.bit_Internal ? "Internal" : "External";
                detailRow.DataColumn3 = item.Company?.str_Name;
                detailRow.DataColumn4 = item.Department?.Code ?? "";
                detailRow.DataColumn5 = item.str_Staff_Id;
                //detailRow.DataColumn6 = item.FirstName + " " + item.LastName;
                detailRow.DataColumn6 = ReturnDisplayLanguage(item.FirstName, item.LastName);

                detailRow.DataColumn7 = item.JobTitle?.Name ?? "";
                detailRow.DataColumn8 = item.Nation;
                detailRow.DataColumn9 = item?.Gender.ToString();
                detailRow.DataColumn10 = item.dtm_Birthdate.HasValue ? item.dtm_Birthdate.Value.ToString("dd MMM yyyy") : "";
                detailRow.DataColumn11 = item.str_Place_Of_Birth;
                detailRow.DataColumn12 = item.dtm_Join_Date.HasValue ? item.dtm_Join_Date.Value.ToString("dd MMM yyyy") : "";
                detailRow.DataColumn13 = item.Passport;
                detailRow.DataColumn14 = item.str_Cell_Phone;
                coursemmber.AddReportModelRow(detailRow);
            }
            var source = new ReportDataSource();
            source.Name = "DataSet1";
            source.Value = coursemmber;

            var rv = new Microsoft.Reporting.WebForms.ReportViewer();

            rv.ProcessingMode = ProcessingMode.Local;
            rv.LocalReport.ReportPath = Server.MapPath(GetByKey("PrivateTemplate") + "ReportFile/Instructor.rdlc");
            //rv.Attributes. = "TransporterIssueTransaction";
            rv.LocalReport.DataSources.Clear();
            rv.LocalReport.DataSources.Add(source);

            var title = "TRAINEE REPORT";
            var param_1 = new ReportParameter("Title", title, false);

            rv.LocalReport.SetParameters(param_1);
            rv.LocalReport.Refresh();

            byte[] streamBytes = null;
            var mimeType = "";
            var encoding = "";
            var filenameExtension = "";
            string[] streamids = null;
            Warning[] warnings = null;

            streamBytes = rv.LocalReport.Render(REPORT_EXCEL, deviceInfo, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
            return File(streamBytes, mimeType, "TRAINEE REPORT" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");

        }

        //helper class
        public class Export
        {
            public void ToExcel(HttpResponseBase Response, object clientsList)
            {
                var grid = new System.Web.UI.WebControls.GridView();
                grid.DataSource = clientsList;
                grid.DataBind();
                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=TraineeList" + DateUtil.DateToString(DateTime.Now, "ddMMyyyy_hhmm") + ".xls");
                Response.ContentType = "application/excel";

                StringWriter sw = new StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter(sw);

                htw.WriteLine("<meta http-equiv=\"Content-Type\" content=\"application/vnd.ms-excel; charset=utf-8\">");


                grid.RenderControl(htw);
                Response.Write(sw.ToString());
                Response.End();
            }
        }

        ///----------///
        #endregion

        [HttpPost]
        public async Task<ActionResult> Create(Trainee_Validation model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    if (model.Id.HasValue)
                    {
                        //update avatar or not
                        if (model.ImgFile != null)
                        {
                            var newAvatar = SaveImage(model.ImgFile, model.eid, UtilConstants.Upload.Trainee);
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
                        var newAvatar = SaveImage(model.ImgFile, model.eid, UtilConstants.Upload.Trainee);
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
                                foreach (var newImg in img.FIleImage.Select(item => SaveImage(item, model.eid, UtilConstants.Upload.Education)))
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

                    var randomPass = TMS.Core.Utils.Common.RandomCharecter();

                    var entity = EmployeeService.Modify(model, TMS.Core.Utils.Common.EncryptString(randomPass));


                    await Task.Run(() =>
                    {
                        if (entity != null)
                        {
                            #region [insert content body]
                            var checksite = GetByKey(UtilConstants.KEY_SENT_EMAIL_CHANGE_PASSWORD);
                            if (checksite.Equals("1"))
                            {
                                Sent_Email_TMS(null, entity, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee);
                            }
                            #endregion


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
                    return Json(new AjaxResponseViewModel { result = true, message = string.Format(Messege.SUCCESS) });
                }
                catch (Exception ex)
                {
                    return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
                }
            }
            else
            {
                return Json(new AjaxResponseViewModel { result = false, message = MessageInvalidData(ModelState) });
            }

        }

        public void WriteTsv<T>(IEnumerable<T> data, TextWriter output)
        {
            var props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                output.Write(prop.DisplayName); // header
                output.Write("\t");
            }
            output.WriteLine();
            foreach (T item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    output.Write(prop.Converter.ConvertToString(
                         prop.GetValue(item)));
                    output.Write("\t");
                }
                output.WriteLine();
            }
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
                    var data = _jobtitleService.Get();
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
        public async Task<ActionResult> SubmitSetParticipateTrainee(int isStatus, string id)
        {
            int idTrainee = int.Parse(id);
            var removeTrainee = EmployeeService.GetById(idTrainee);
            if (removeTrainee == null)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (isStatus == 1)
            {
                removeTrainee.IsActive = false;
            }
            else
            {
                removeTrainee.IsActive = true;
            }
            removeTrainee.LmsStatus = StatusModify;
            removeTrainee.dtm_Last_Modified_At = DateTime.Now;
            removeTrainee.str_Last_Modified_By = CurrentUser.USER_ID.ToString();
            EmployeeService.Update(removeTrainee);
            //var fullName = removeTrainee.FirstName.Trim() + " " + removeTrainee.LastName.Trim();
            var fullName = ReturnDisplayLanguage(removeTrainee.FirstName, removeTrainee.LastName);
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
            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, "<font color='red'>" + fullName + "</font>"), result = true }, JsonRequestBehavior.AllowGet);
        }


        #region [-------------------------------- Group Trainee ---------------------------------------]

        public ActionResult Group()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ModifyGroup(int? id)
        {

            var model = new GroupTraineeModel();
            //model.GroupTrainees =
            //           EmployeeService.Get()
            //               .ToDictionary(a => a.Id,
            //                   a =>
            //                       string.Format("{0} - {1}", a.str_Staff_Id,
            //                           a.FirstName.Trim() + " " + a.LastName.Trim()));
            model.GroupTrainees =
                       EmployeeService.Get()
                           .ToDictionary(a => a.Id,
                               a =>
                                   string.Format("{0} - {1}", a.str_Staff_Id,
                                       ReturnDisplayLanguage(a.FirstName, a.LastName)));
            var entity = EmployeeService.GetGroupById(id);
            if (entity != null)
            {

                model.Id = entity.Id;
                model.Code = entity.Code;
                model.Name = entity.Name;
                model.Description = entity.Description;
                model.IsActived = entity.IsActived ?? false;

                model.TraineeIds = entity.GroupTrainee_Item.Select(a => a.TraineeId).ToList();

            }
            return View(model);
        }
        [HttpPost]
        public ActionResult ModifyGroup(GroupTraineeModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                // EmployeeService.Modify(model);
                var result = new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                };
                TempData[UtilConstants.NotifyMessageName] = result;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }


        public ActionResult AjaxHandlerGroupTrainee(jQueryDataTableParamModel param)
        {
            try
            {
                var name = string.IsNullOrEmpty(Request.QueryString["NameS"]) ? string.Empty : Request.QueryString["NameS"].ToLower().Trim();
                var code = string.IsNullOrEmpty(Request.QueryString["CodeS"]) ? string.Empty : Request.QueryString["CodeS"].ToLower().Trim();

                var data = EmployeeService.GetAllGroupTrainees(a =>
                         (string.IsNullOrEmpty(name) || a.Name.Contains(name))
                      && (string.IsNullOrEmpty(code) || a.Code.Contains(code))
                );

                IEnumerable<GroupTrainee> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

                Func<GroupTrainee, object> orderingFunction = (g => sortColumnIndex == 1 ? g.Name
                                                                : sortColumnIndex == 2 ? g.Code : g.Name);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);


                var verticalBar = GetByKey("VerticalBar");
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new[] {
                                string.Empty,
                                 c.Code,
                         "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+c.Name+"</a></span>",

                         c.IsActived == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='SetParticipateGroupTrainee(0,"+c.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='SetParticipateGroupTrainee(1,"+c.Id+")' aria-hidden='true'  title='Active' style='cursor: pointer;' ></i>",
                         "<a href='"+@Url.Action("ModifyGroup",new{id = c.Id})+"' ><i class='fa fa-pencil-square-o' style='font-size:16px' title='Edit' ></i></a>"+
                         ((User.IsInRole("/GroupUser/Delete")) ? verticalBar +"<a title='Delete'  href='javascript:void(0)' onclick='calldelete(" + c.Id  + ")'><i class='fa fa-trash-o' aria-hidden='true' style='font-size:16px'></i></a>" : "")

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

        public ActionResult AjaxHandlerGroupTraneeSub(jQueryDataTableParamModel param, int id)
        {
            try
            {
                var data = EmployeeService.Get(a => a.GroupTrainee_Item.Any(b => b.GroupTraineeId == id)).ToList().Select(c => new SubGroupTraineeModel()
                {
                    Code = c.str_Staff_Id,
                    //FullName = c.FirstName.Trim() + " " + c.LastName.Trim(),
                    FullName = ReturnDisplayLanguage(c.FirstName.Trim(), c.LastName.Trim()),

                    Job = c.JobTitle.Name,
                    Phone = c.str_Cell_Phone
                });

                IEnumerable<SubGroupTraineeModel> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

                Func<SubGroupTraineeModel, object> orderingFunction = (s =>
                                          sortColumnIndex == 1 ? s.Code
                                        : sortColumnIndex == 2 ? s.FullName
                                        : sortColumnIndex == 3 ? s.Job
                                        : sortColumnIndex == 4 ? s.Phone
                                                                : s.FullName);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new[] {
                                string.Empty,
                                c.Code,
                                c.FullName,
                                c.Phone,
                                c.Job
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
        [HttpPost]
        public ActionResult DeleteGroup(int? id)
        {
            try
            {
                var model = EmployeeService.GetGroupById(id);
                if (model == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var name = model.Code.Trim() + " " + model.Name.Trim();
                if (model.GroupTrainee_Item.Any(a => a.IsDeleted == false))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, name),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                model.IsDeleted = true;
                model.IsActived = false;
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = CurrentUser.USER_ID;
                EmployeeService.Update(model);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, name),
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
        public ActionResult SetParticipateGroupTrainee(int isStatus, string id)
        {
            int idTrainee = int.Parse(id);
            var removeGroupTrainee = EmployeeService.GetGroupById(idTrainee);
            if (removeGroupTrainee == null)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

            removeGroupTrainee.IsActived = isStatus != 1;

            removeGroupTrainee.ModifiedDate = DateTime.Now;
            removeGroupTrainee.ModifiedBy = CurrentUser.USER_ID;
            EmployeeService.Update(removeGroupTrainee);
            var name = removeGroupTrainee.Code.Trim() + " " + removeGroupTrainee.Name.Trim();
            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, name), result = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        private string LoadDepartment(int? id = null)
        {
            var result = string.Empty;
            var data = DepartmentService.Get(a => CurrentUser.PermissionIds.Any(x => x == a.Id)).Select(x => new { x.Id, x.Ancestor, x.Name, x.Code });
            var lvl = 1;
            foreach (var item in data)
            {
                lvl = item.Ancestor.Count(x => x.Equals('_'));
                var khoangtrang = "";
                for (var i = 1; i < lvl; i++)
                {
                    khoangtrang += "&nbsp;&nbsp;&nbsp;";
                }
                result += "<option value='" + item.Id + "' " + (id == item.Id ? "Selected" : "") + ">" + khoangtrang + item.Code + " - " + item.Name;
                result += "</option>";
            }
            return result;
        }

        [HttpPost]
        public ActionResult DeleteImg(int? id)
        {
            try
            {
                var model = EmployeeService.FileGetById(id);
                if (model == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }


                model.IsDeleted = true;
                model.ModifiedDate = DateTime.Now;
                EmployeeService.Update(model);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
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



        #region [-----------------Hàm không xài----------------]

        [HttpPost]
        public JsonResult CreatePartner(Trainee model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var countEmail = EmployeeService.Get(m => m.str_Email == model.str_Email).Count();
                    if (countEmail > 0)
                    {
                        return Json(new
                        {
                            Result = "ERROR",
                            Message = "The email you input is exist in the system!"
                        });
                    }
                    else
                    {
                        model.dtm_Created_At = DateTime.Now;
                        model.str_Created_By = User.Identity.Name;
                        model.bit_Internal = false;
                        EmployeeService.Insert(model);
                        return Json(new { Result = "OK", Record = model });
                    }
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = "Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Create", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = "Form is not valid! Please correct it and try again."
                });
            }
        }


        [HttpPost]
        public JsonResult Update(Trainee model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dbTrainee = EmployeeService.GetById(model.Id);
                    var countEmail = EmployeeService.Get(m => m.str_Email == model.str_Email);
                    if (dbTrainee.str_Email != model.str_Email && countEmail.Any())
                    {
                        return Json(new
                        {
                            Result = "ERROR",
                            Message = "The email you input is exist in the system!"
                        });
                    }
                    else
                    {
                        dbTrainee.Company_Id = model.Company_Id;
                        dbTrainee.Department_Id = model.Department_Id;
                        dbTrainee.str_Staff_Id = model.str_Staff_Id;
                        dbTrainee.FirstName = model.FirstName;
                        dbTrainee.LastName = model.LastName;
                        dbTrainee.str_Email = model.str_Email;
                        dbTrainee.str_Cell_Phone = model.str_Cell_Phone;
                        dbTrainee.Job_Title_id = model.Job_Title_id;
                        dbTrainee.str_Station = model.str_Station;
                        dbTrainee.dtm_Birthdate = model.dtm_Birthdate;
                        dbTrainee.str_Place_Of_Birth = model.str_Place_Of_Birth;
                        dbTrainee.dtm_Last_Modified_At = DateTime.Now;
                        dbTrainee.str_Last_Modified_By = User.Identity.Name;
                        dbTrainee.dtm_Join_Date = model.dtm_Join_Date;
                        EmployeeService.Update(dbTrainee);
                        return Json(new { Result = "OK" });
                    }
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = "Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Edit", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = "Form is not valid! Please correct it and try again."
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

                var deptList = _companyService.Get(m => m.bit_Deleted == false).ToList();
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
                var titleList = _jobtitleService.Get().OrderBy(m => m.Name).ToList();
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
                var query = EmployeeService.GetRecord(m => m.bit_Deleted == false && m.Trainee_Id == Trainee_Id).ToList();

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
                    Message = "Form is not valid! Please correct it and try again."
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

                    EmployeeService.InsertRecord(traineeRecord);
                    return Json(new { Result = "OK", Record = traineeRecord });
                }
                return Json(new
                {
                    Result = "ERROR",
                    Message = "Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Create", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = "Form is not valid! Please correct it and try again."
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
                    Message = "Form is not valid! Please correct it and try again."
                });
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("Department - Edit", ex.ToString());
                ViewBag.Message = ex.Message;
                return Json(new
                {
                    Result = "ERROR",
                    Message = "Form is not valid! Please correct it and try again."
                });
            }
        }
        public ActionResult Import()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Import(List<ImportTraineeViewModel> model)
        {
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
                        newTrainee.str_Place_Of_Birth = item.str_BirthPlace;
                        newTrainee.dtm_Birthdate = item.dtm_Birthdate;
                        newTrainee.bit_Internal = true;
                        newTrainee.IsActive = true;
                        newTrainee.dtm_Created_At = DateTime.Now;
                        newTrainee.str_Created_By = User.Identity.Name;
                        if (!string.IsNullOrEmpty(item.Department))
                        {
                            newTrainee.Department_Id = getDepartmentId(item.Department);
                        }

                        if (!string.IsNullOrEmpty(item.Job_Title))
                        {
                            newTrainee.Job_Title_id = getJobTitleId(item.Job_Title);
                        }
                        EmployeeService.Insert(newTrainee);
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                string error = ExceptionUtils.getEntityValidationErrors(ex);
                ExceptionUtils.logError("ImportEmployee - Create", error);
                ViewBag.Message = error;
            }
            catch (Exception ex)
            {
                ExceptionUtils.logError("ImportEmployee - Create", ex.ToString());
                ViewBag.Message = ex.Message;
            }
            return RedirectToAction("Index");
        }
        public ActionResult ImportEmployee()
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
        public ActionResult ImportEmployee(List<ImportTraineeViewModel> model)
        {
            //string test = "";
            var sdb = new EFDbContext();
            try
            {
                var Yearend = new DateTime(DateTime.Now.Year, 12, 31);

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
                    sdb.SaveChanges();
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
                        newTrainee.dtm_Birthdate = item.dtm_Birthdate;
                        newTrainee.bit_Internal = false;
                        newTrainee.dtm_Created_At = DateTime.Now;
                        newTrainee.str_Created_By = User.Identity.Name;
                        newTrainee.str_Place_Of_Birth = item.str_BirthPlace;
                        if (!String.IsNullOrEmpty(item.Company))
                        {
                            newTrainee.Company_Id = getCompanyId(item.Company);
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

        private int getDepartmentId(String DepartmentName)
        {
            var dbDep = DepartmentService.Get(m => m.Name == DepartmentName && m.IsDeleted == false, CurrentUser.PermissionIds).FirstOrDefault();
            if (dbDep == null)
            {
                dbDep = new Department();
                dbDep.Name = DepartmentName;
                dbDep.IsDeleted = false;
                dbDep.IsActive = true;
                DepartmentService.Insert(dbDep);
            }
            return dbDep.Id;
        }

        private int getCompanyId(String CompanyName)
        {
            var dbCom = _companyService.Get(m => m.str_Name == CompanyName && m.bit_Deleted == false).FirstOrDefault();
            if (dbCom == null)
            {
                dbCom = new Company();
                dbCom.str_Name = CompanyName;
                dbCom.bit_Deleted = false;
                _companyService.Insert(dbCom);
            }
            return dbCom.Company_Id;
        }

        private int getJobTitleId(String JobTitleName)
        {
            var dbJobTitle = _jobtitleService.Get(m => m.Name == JobTitleName && m.IsDelete == false).FirstOrDefault();
            if (dbJobTitle == null)
            {
                dbJobTitle = new JobTitle();
                dbJobTitle.Name = JobTitleName;
                dbJobTitle.IsDelete = false;
                dbJobTitle.IsActive = true;
                dbJobTitle.CreatedBy = CurrentUser.Username;
                dbJobTitle.CreatedDate = DateTime.Now;

                _jobtitleService.Insert(dbJobTitle);
            }
            return dbJobTitle.Id;
        }
        [HttpPost]
        //TODO: process result
        public JsonResult TraineeResultList(int Trainee_Id, int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            return null;
            //try
            //{
            //    var query = _courseService.GetCourseResult(Trainee_Id).ToList();

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
                        Message = "Can not delete Result from course."
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
                    Message = "Form is not valid! Please correct it and try again."
                });
            }
        }
        //TODO: process result
        [HttpPost]
        public JsonResult CreateResult(TraineeResultViewModel model)
        {
            return null;
            //try
            //{
            //    if (ModelState.IsValid)
            //    {
            //        var subject = _subjectService.GetById(model.Subject_Id);
            //        if (subject != null)
            //        {
            //            var newResult = new Course_Result();
            //            newResult.Created_At = DateTime.Now;
            //            newResult.Created_By = User.Identity.Name;
            //            newResult.Learning_From = model.Learning_From;
            //            newResult.Learning_To = model.Learning_To;
            //            //Kiểm tra nếu loại kết quả là điểm thì convert sang kiểu Int và chèn vào kết quả điểm
            //            if (subject.bit_ScoreOrResult)
            //            {
            //                newResult.First_Check_Score = int.Parse(model.First_Check_Result);
            //                if (newResult.First_Check_Score != null && newResult.First_Check_Score.Value >= subject.Pass_Score)
            //                {
            //                    newResult.First_Check_Result = "P";
            //                }
            //                else
            //                {
            //                    newResult.First_Check_Result = "F";
            //                }
            //                var temInt = 0;
            //                if (int.TryParse(model.Re_Check_Result, out temInt))
            //                {
            //                    newResult.Re_Check_Score = int.Parse(model.Re_Check_Result);
            //                    if (newResult.Re_Check_Score.Value >= subject.Pass_Score)
            //                    {
            //                        newResult.Re_Check_Result = "P";
            //                    }
            //                    else
            //                    {
            //                        newResult.Re_Check_Result = "F";
            //                    }
            //                }
            //            }
            //            //Nếu kết quả là Pass/Fail thì kiểm tra giá trị nhập vào có đúng không
            //            else
            //            {
            //                if (model.First_Check_Result.ToUpper() != "P" && model.First_Check_Result.ToUpper() != "F" && model.Re_Check_Result.ToUpper() != "P" && model.Re_Check_Result.ToUpper() != "F")
            //                {
            //                    return Json(new
            //                    {
            //                        Result = "ERROR",
            //                        Message = "Please input P or F."
            //                    });
            //                }
            //                else
            //                {
            //                    newResult.First_Check_Result = model.First_Check_Result;
            //                    newResult.Re_Check_Result = model.Re_Check_Result;
            //                }
            //            }
            //            newResult.Remark = model.Remark;
            //            newResult.SubjectDetailId = model.Subject_Id;
            //            newResult.Trainee_Id = model.Trainee_Id;
            //            _courseService.InsertCourseResult(newResult);
            //            return Json(new
            //            {
            //                Result = "OK",
            //                Record = new TraineeResultViewModel
            //                {
            //                    Course_Result_Id = newResult.Course_Result_Id,
            //                    Subject_Id = newResult.SubjectDetailId,
            //                    First_Check_Result = (newResult.SubjectDetail.bit_ScoreOrResult == true ? (newResult.First_Check_Score == null ? "" : newResult.First_Check_Score.Value.ToString()) : newResult.First_Check_Result),
            //                    Re_Check_Result = (newResult.SubjectDetail.bit_ScoreOrResult == true ? (newResult.Re_Check_Score == null ? "" : newResult.Re_Check_Score.Value.ToString()) : newResult.Re_Check_Result),
            //                    Remark = newResult.Remark,
            //                    ResultType = (newResult.SubjectDetail.bit_ScoreOrResult == true ? "Score" : "Pass/Fail"),
            //                    Editable = (newResult.Course_Detail_Id == null),
            //                    Learning_From = newResult.Learning_From,
            //                    Learning_To = newResult.Learning_To
            //                }
            //            });
            //        }
            //    }
            //    return Json(new
            //    {
            //        Result = "ERROR",
            //        Message = "Form is not valid! Please correct it and try again."
            //    });
            //}
            //catch (Exception ex)
            //{
            //    ExceptionUtils.logError("CreateResult", ex.ToString());
            //    ViewBag.Message = ex.Message;
            //    return Json(new
            //    {
            //        Result = "ERROR",
            //        Message = "Form is not valid! Please correct it and try again."
            //    });
            //}
        }

        [HttpPost]
        public JsonResult UpdateResult(TraineeResultViewModel model)
        {
            return null;
            //try
            //{
            //    if (ModelState.IsValid)
            //    {
            //        var dbRecord = _courseService.GetCourseResultById(model.Course_Result_Id);

            //        var subject = _subjectService.GetById(model.Subject_Id);
            //        if (subject != null)
            //        {

            //            dbRecord.Modified_At = DateTime.Now;
            //            dbRecord.Modified_By = User.Identity.Name;
            //            //Kiểm tra nếu loại kết quả là điểm thì convert sang kiểu Int và chèn vào kết quả điểm
            //            if (subject.bit_ScoreOrResult)
            //            {
            //                dbRecord.First_Check_Score = int.Parse(model.First_Check_Result);
            //                if (dbRecord.First_Check_Score != null)
            //                {
            //                    if (dbRecord.First_Check_Score.Value >= subject.Pass_Score)
            //                    {
            //                        dbRecord.First_Check_Result = "P";
            //                    }
            //                    else
            //                    {
            //                        dbRecord.First_Check_Result = "F";
            //                    }
            //                }

            //                var temInt = 0;
            //                if (int.TryParse(model.Re_Check_Result, out temInt))
            //                {
            //                    dbRecord.Re_Check_Score = int.Parse(model.Re_Check_Result);
            //                    if (dbRecord.Re_Check_Score.Value >= subject.Pass_Score)
            //                    {
            //                        dbRecord.Re_Check_Result = "P";
            //                    }
            //                    else
            //                    {
            //                        dbRecord.Re_Check_Result = "F";
            //                    }
            //                }
            //                else
            //                {
            //                    dbRecord.Re_Check_Result = "F";
            //                    dbRecord.Re_Check_Score = null;
            //                }
            //            }
            //            //Nếu kết quả là Pass/Fail thì kiểm tra giá trị nhập vào có đúng không
            //            else
            //            {
            //                if (model.First_Check_Result.ToUpper() != "P" && model.First_Check_Result.ToUpper() != "F" && model.Re_Check_Result.ToUpper() != "P" && model.Re_Check_Result.ToUpper() != "F")
            //                {
            //                    return Json(new
            //                    {
            //                        Result = "ERROR",
            //                        Message = "Please input P or F."
            //                    });
            //                }
            //                else
            //                {
            //                    dbRecord.First_Check_Result = model.First_Check_Result;
            //                    dbRecord.Re_Check_Result = model.Re_Check_Result;

            //                }
            //            }
            //            dbRecord.Remark = model.Remark;
            //            dbRecord.SubjectDetailId = model.Subject_Id;
            //            dbRecord.Learning_From = model.Learning_From;
            //            dbRecord.Learning_To = model.Learning_To;
            //            _courseService.UpdateCourseResult(dbRecord);
            //            return Json(new
            //            {
            //                Result = "OK"
            //            });
            //        }
            //    }
            //    return Json(new
            //    {
            //        Result = "ERROR",
            //        Message = "Form is not valid! Please correct it and try again."
            //    });
            //}
            //catch (Exception ex)
            //{
            //    ExceptionUtils.logError("UpdateResult", ex.ToString());
            //    ViewBag.Message = ex.Message;
            //    return Json(new
            //    {
            //        Result = "ERROR",
            //        Message = "Form is not valid! Please correct it and try again."
            //    });
            //}
        }

        #endregion
    }
}
