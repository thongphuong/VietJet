using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TMS.Core.Services.Companies;
using TMS.Core.Services.Subject;
using TMS.Core.Services.TraineeHis;
using TMS.Core.ViewModels.EmployeeModels;
using TMS.Core.ViewModels.Subjects;
using TMS.Core.ViewModels.TraineeHistory;
using TMS.Core.ViewModels.ViewModel;
using TrainingCenter.Utilities;


namespace TrainingCenter.Controllers
{
    using TMS.Core.Services.Approves;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.Services.Jobtitle;
    using TMS.Core.Services.Employee;
    using DAL.Entities;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Employee;
    using Newtonsoft.Json.Linq;
    using global::Utilities;
    using AppUtils = global::Utilities.AppUtils;
    using Newtonsoft.Json;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Globalization;

    public class EmployeeController : BaseAdminController
    {
        private readonly ISubjectService _repoSubjectService;
        private readonly ICompanyService _repoCompanyService;
        private readonly IJobtitleService _repoJobtitleService;
        private readonly ITraineeHistoryService _repotraineeHistoryService;
        private readonly IJobtitleService _repoJob_Tiltle;
        private readonly IDepartmentService _departmentService;
        private readonly IEmployeeService _repoEmployeeService;

        public EmployeeController(IConfigService configService,
            IJobtitleService repoJobTiltle,
            IEmployeeService repoEmployeeService,
        IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService repoApproveService, IJobtitleService jobtitleService, ICompanyService repoCompany, ISubjectService repoSubjectService, ITraineeHistoryService repotraineeHistoryService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, repoApproveService)
        {
            _repoJobtitleService = jobtitleService;
            _repoCompanyService = repoCompany;
            _departmentService = departmentService;
            _repoJob_Tiltle = repoJobTiltle;
            _repoSubjectService = repoSubjectService;
            _repotraineeHistoryService = repotraineeHistoryService;
            _repoEmployeeService = repoEmployeeService;
        }
        // GET: Employee
        public ActionResult Index(int? id)
        {
            if (
                (id == (int)UtilConstants.ROLE.Trainee ||
                 id == (int)UtilConstants.ROLE.Instructor
                    ))
            {
                //SyncUserCAS();
                var model = new EmployeeModelIndex();
                model.Departments = LoadDepartment();
                model.Genders = new SelectList(UtilConstants.GenderDictionary(), "Key", "Value");
                model.JobTitles = new SelectList(_repoJobtitleService.Get().OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Name), "Key", "Value");


                //model.Type = new SelectList(UtilConstants.ActiveStatusDictionary(), "Key", "Value");
                model.Type = new SelectList(UtilConstants.CourseCourseAreasDictionary(), "Key", "Value");
                model.Mentor = new SelectList(UtilConstants.SearchEmployee(), "Key", "Value");
                model.Control = (int)id;
                var value = GetByKey("SearchMentor");
                model.SearchMentor = value.Equals("1") ? true : false;

                return View(model);
            }
            TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
            return RedirectToAction("Index", "Home");
        }
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var userPermission = CurrentUser.PermissionIds;
                // xử lý param gửi lên
                var comOrDepId = string.IsNullOrEmpty(Request.QueryString["DepartmentList"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList"].Trim());
                var fStatus = string.IsNullOrEmpty(Request.QueryString["fStatus"]) ? -1 : Convert.ToInt32(Request.QueryString["fStatus"].Trim());
                var fJobTitle = string.IsNullOrEmpty(Request.QueryString["JobTitleList"]) ? -1 : Convert.ToInt32(Request.QueryString["JobTitleList"].Trim());
                var fGender = string.IsNullOrEmpty(Request.QueryString["Genders"]) ? -1 : Convert.ToInt32(Request.QueryString["Genders"].Trim());
                var ddlType = string.IsNullOrEmpty(Request.QueryString["Type"]) ? -1 : Convert.ToInt32(Request.QueryString["Type"].Trim());
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? string.Empty : Request.QueryString["fName"].Trim();
                var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? string.Empty : Request.QueryString["fEmail"].Trim();
                var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? string.Empty : Request.QueryString["fPhone"].Trim();
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? string.Empty : Request.QueryString["fStaffId"].Trim();
                var role = string.IsNullOrEmpty(Request.QueryString["control"]) ? -1 : Convert.ToInt32(Request.QueryString["control"].Trim());
                var mentor = string.IsNullOrEmpty(Request.QueryString["Mentor[]"]) ? string.Empty : Request.QueryString["Mentor[]"].Trim();

                var bitInternal = (ddlType == 1);
                var isActive = fStatus != 0;
                var lstDepartment = new List<int?>();

                if (comOrDepId != -1)
                {
                    var department = DepartmentService.GetById(comOrDepId);
                    if (department != null)
                    {
                        var departmentIds = DepartmentService.Get(a => a.Ancestor.Contains(department.Code)).OrderBy(b => b.Ancestor).Select(a => a.Id);
                        if (departmentIds.Any())
                        {
                            foreach (var id in departmentIds)
                            {
                                lstDepartment.Add(id);
                            }
                        }
                    }
                }



                var data = EmployeeService.Get(a => a.IsDeleted == false
                //&& userPermission.Any(x => a.Department_Id == x)
                && (role == -1 || (role != (int)UtilConstants.ROLE.Instructor || a.int_Role == role))
                && (comOrDepId == -1 || lstDepartment.Contains(a.Department_Id))
                  && (ddlType == -1 || a.bit_Internal == bitInternal)
                && (string.IsNullOrEmpty(fName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(fName.Trim())) || ((a.LastName.Trim() + " " + a.FirstName.Trim()).Contains(fName.Trim())))
                && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
                && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail.Trim()))
                && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone.Trim()))
                && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId.Trim()))
                && (fGender == -1 || a.Gender == fGender)
                && (fStatus == -1 || a.IsActive == isActive), true);


                if (!string.IsNullOrEmpty(mentor))
                {
                    var mentorCodes = mentor.Split(new char[] { ',' }).Select(int.Parse).ToList();

                    if (mentorCodes != null && mentorCodes.Any())
                    {
                        data = data.Where(a => a.Trainee_Type.Any(b => mentorCodes.Contains(b.Type.Value)));

                    }
                }

                IEnumerable<Trainee> filtered = data.OrderByDescending(p => p.Id);
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c
                                                         => sortColumnIndex == 1 ? c.str_Staff_Id
                                                           : sortColumnIndex == 2 ? c?.LastName
                                                           : sortColumnIndex == 3 ? UtilConstants.GenderDictionary()[c?.Gender ?? (int)UtilConstants.Gender.Others]
                                                           : sortColumnIndex == 4 ? c.str_Email
                                                           : sortColumnIndex == 5 ? c.str_Cell_Phone
                                                           : sortColumnIndex == 6 ? c.JobTitle?.Name ?? ""
                                                           : sortColumnIndex == 7 ? c.Department?.Code ?? ""
                                                           : sortColumnIndex == 8 ? c?.bit_Internal.ToString()
                                                         : c.str_Staff_Id.ToString());
                //Func<Trainee, object> orderingFunction = (c
                //                                          => sortColumnIndex == 1 ? c?.str_Staff_Id
                //                                            : sortColumnIndex == 2 ? c?.LastName
                //                                            : sortColumnIndex == 3 ? UtilConstants.GenderDictionary()[c?.Gender ?? (int)UtilConstants.Gender.Others]
                //                                            : sortColumnIndex == 4 ? c?.Department?.Ancestor
                //                                            : sortColumnIndex == 5 ? c?.bit_Internal
                //                                            : sortColumnIndex == 6 ? c?.Trainee_Type?.Any()
                //                                            : sortColumnIndex == 7 ? (object)c?.IsActive
                //                                            : c?.str_Staff_Id);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                 : filtered.OrderByDescending(orderingFunction);
                }

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = "";//GetByKey("VerticalBar");
                var result = from c in displayed
                             let gender = UtilConstants.GenderDictionary()[c?.Gender ?? (int)UtilConstants.Gender.Others]
                             let fullName = ReturnDisplayLanguage(c?.FirstName, c?.LastName)
                             select new object[] {
                                 string.Empty,
                                    c?.str_Staff_Id,
                                "<a href='"+@Url.Action("Details",new{id = c?.Id,type = role})+"'>"+ fullName +"</a>",
                                    gender,
                                    c.str_Email,
                                    c.str_Cell_Phone,
                                    c.JobTitle?.Name ?? "",
                                    c.Department?.Code ?? "",
                                    c?.bit_Internal == true ? UtilConstants.CourseAreas.Internal.ToString(): UtilConstants.CourseAreas.External.ToString(),
                                  //ReturnMentorHanna(c?.Trainee_Type),
                                    (c?.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Employee(0,"+c?.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Employee(1,"+c?.Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                                   ((User.IsInRole("/Employee/Details")) ? "<a title='View' href='"+@Url.Action("Details",new{id = c?.Id,type = role})+"' data-toggle='tooltip'><i class='fas fa-search btnIcon_blue font-byhoa' aria-hidden='true' ></i></a>" : "" ) +
                                    ((User.IsInRole("/Employee/Modify")) ? verticalBar +"<a title='Edit' href='"+@Url.Action("Modify",new{id = c?.Id,type = role})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>":"") +
                                    ((User.IsInRole("/Employee/Delete")) ? verticalBar +"<a title='Delete' href='javascript:void(0)'  onclick='calldelete(" + c?.Id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" :"")+verticalBar +
                                    "<a title='New Password' href='javascript:void(0)'  onclick='callrandom(\" "+fullName+" \"," + c?.Id  + ")' data-toggle='tooltip'><i class='fas fa-paper-plane btnIcon_darkorchid font-byhoa' aria-hidden='true' ></i></a>"
                        };
                var jsonResult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        private string ReturnMentorHanna(IEnumerable<Trainee_Type> trainee_Type = null)
        {
            var rt = "";
            if (trainee_Type.Any())
            {
                foreach (var item in trainee_Type)
                {
                    rt += string.Format("<span class='label label-primary'>{0}</span>", UtilConstants.SearchEmployee()[item.Type.Value]);
                }
            }
            return rt;

        }
        public ActionResult AjaxHandlerContract(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                var data = EmployeeService.GetContract(a => a.Trainee_Id == id).OrderBy(p => p.id);

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
                                    DateUtil.DateToString(c?.expire_date,Resource.lbl_FORMAT_DATE),
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerContract", ex.Message);
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
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());
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
                                    c.dtm_time_from?.ToString(Resource.lbl_FORMAT_DATE) +" - "+ c.dtm_time_to?.ToString(Resource.lbl_FORMAT_DATE),
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerEducation", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlerTrainingCourses(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                var datacourse = CourseService.Get(a=>a.IsDeleted != true && a.Course_Detail.Any(c=> c.IsDeleted != true && c.TMS_Course_Member.Any(x => x.Member_Id == id && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved))) && a.TMS_APPROVES.Any(v=>v.int_Type == (int)UtilConstants.ApproveType.Course && v.int_id_status == (int)UtilConstants.EStatus.Approve),true);
                //var datacourse = CourseDetailService.Get(a => a.Course.IsDeleted != true &&
                //             a.TMS_Course_Member.Any(x => x.Member_Id == id && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)) &&
                //            a.Course.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && x.int_id_status == (int)UtilConstants.EStatus.Approve)).Select(x => x.Course).Distinct();
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
                             let final = c.Course_Result_Final.FirstOrDefault(a => a.traineeid == id)
                             select new object[] {
                                   "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'>+</span>",
                                 string.Empty,
                                    c?.Code,
                                    "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+c.Name+"</a></span>",
                                    DateUtil.DateToString(c?.StartDate,Resource.lbl_FORMAT_DATE) +" - "+ DateUtil.DateToString(c?.EndDate,Resource.lbl_FORMAT_DATE),
                                    !string.IsNullOrEmpty(final?.certificatefinal) ? ( !string.IsNullOrEmpty(final?.Path) && final?.statusCertificate == 0 ? "<a  href='"+ ConfigurationSettings.AppSettings["AWSLinkS3"] + final?.Path +"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>"  : ""): "",
                                    //"<span data-value='" + c.Id + "' class='expand' style='cursor: pointer;'><a><i class='fa fa-plus-circle' aria-hidden='true' style=' font-size: 16px; '></i></a></span>"

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerTrainingCourses", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlerTrainingCompetecy(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());
                
                var listInstructorSubject =
                    EmployeeService.GetInstruc_Ability(a => a.InstructorId == id &&                  
                    a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General &&
                    a.SubjectDetail.IsDelete != true ).Select(a => a.SubjectDetailId);
                var data = _repoSubjectService.GetSubjectDetailApi(a => listInstructorSubject.Contains(a.Id)).GroupBy(a => a.Name).Select(b => b.OrderBy(p => p.Name).FirstOrDefault());
                IEnumerable<SubjectDetail> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                          : c.Id.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc                
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                 c?.IsActive != true ?"<span style='color:"+UtilConstants.String_DeActive_Color+";')>" + c?.Code + "</span>" : c?.Code,
                                c?.IsActive != true ?"<span style='color:"+UtilConstants.String_DeActive_Color+";')>"+ c?.Name + "</span>" : c?.Name
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerTrainingCompetecy", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlerConductedSubjects(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                var data = CourseDetailService.Get(a => a.Course_Detail_Instructor.Any(x => x.Instructor_Id == id) && a.Course.IsActive == true && a.Course.IsDeleted != true && a.SubjectDetail.IsDelete != true && a.SubjectDetail.CourseTypeId != 6).Select(a => a.SubjectDetailId);
                var dtsubject = _repoSubjectService.GetSubjectDetail(a => data.Contains(a.Id));

                IEnumerable<SubjectDetail> filtered = dtsubject;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                          : c.Id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 "<span data-value='"+c.Code +"' class='expand' style='cursor: pointer;'>+</span>",
                                string.Empty,
                                   c?.IsActive != true ?"<span style='color:"+UtilConstants.String_DeActive_Color+";')>"+ c?.Code + "</span>" : c?.Code,
                                c?.IsActive != true ?"<span style='color:"+UtilConstants.String_DeActive_Color+";')>"+ c?.Name + "</span>" : c?.Name
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerConductedSubjects", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlerSubjectsOfCourse(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var instructorId = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

               // var dataCourse_Detail_Id = CourseDetailService.Get(a => a.CourseId == id).ToList().Select(a => a.Id);
                var data = CourseMemberService.Get(a => a.Course_Detail.CourseId == id && a.Course_Detail.IsDeleted != true  && a.Member_Id == instructorId && a.IsActive == true && (a.Status == null || a.Status ==(int)UtilConstants.APIAssign.Approved), true);

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
                                  bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                  dtm_from = c?.Course_Detail?.dtm_time_from,
                                  dtm_from_to = DateUtil.DateToString(c?.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c?.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                                  subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                  TypeLearning = TypeLearningIcon(c?.Course_Detail?.type_leaning),
                                  firstCheck = ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == instructorId)),
                                  reCheck = ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == instructorId)),
                                  remark = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Type == true ? GetRemarkCheckFail(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Id) : (c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Remark != null ? c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Remark?.Replace("!!!!!", "<br />") : null),
                                  grade = returnpointgrade(2, c?.Member_Id, c?.Course_Details_Id),

                                  recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                                  courseDetails = c?.Course_Detail,
                                  memberId = c?.Member_Id,
                                  Status =
                                  (bool)c?.Course_Detail?.TMS_APPROVES?.Any(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve) ?
                                        "<span class='label label-success'>Success</span>"
                                    : (bool)c?.Course_Detail?.Course.TMS_APPROVES?.Any(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && a.int_id_status == (int)UtilConstants.EStatus.Approve) && c?.Course_Detail?.dtm_time_from > DateTime.Now
                                    ? "<span class='label label-warning'>Up Coming</span>" :
                                         "<span class='label label-danger'>Training</span>",

                              };

                resultA = resultA.OrderBy(a => a.dtm_from).ToList();
                for (int i = 0; i < resultA.Count(); i++)
                {
                    var name = resultA.ElementAt(i).subjectName;
                    var dataCourseDetail = CourseMemberService.Get(a => a.Course_Detail.SubjectDetail.Name.Equals(name) && a.Course_Detail.IsDeleted != true && a.Member_Id == instructorId && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).Where(a =>
                                 a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve));

                    if (dataCourseDetail.Any())
                    {
                        IEnumerable<TMS_Course_Member> filteredB = dataCourseDetail;
                        filteredB = filteredB.OrderBy(a => a?.Course_Detail?.dtm_time_from);
                        var displayedB = filteredB;
                        var resultB = (from c in displayedB.ToArray()
                                      select new ProfileSubjectModel
                                      {
                                          bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                          SubjectCode = c?.Course_Detail?.SubjectDetail?.Code,
                                          dtm_from = c?.Course_Detail?.dtm_time_from,
                                          subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                          courseDetails = c?.Course_Detail,
                                          memberId = c?.Member_Id,
                                      }).ToList();
                       // resultB = resultB.ToList();
                        for (int y = 0; y < resultB.Count(); y++)
                        {
                            if (y != 0 && resultB.ElementAt(y).subjectName == resultB.ElementAt(y - 1).subjectName)
                            {
                                resultB.ElementAt(y).ex_Date = ResturnExpiredate(resultB.ElementAt(y).courseDetails, resultB.ElementAt(y).memberId, resultB.ElementAt(y - 1).ex_Date);
                            }
                            else
                            {
                                resultB.ElementAt(y).ex_Date = ResturnExpiredate(resultB.ElementAt(y).courseDetails, resultB.ElementAt(y).memberId, resultB.ElementAt(y).ex_Date);
                            }
                            if (resultA.ElementAt(i).dtm_from == resultB.ElementAt(y).dtm_from)
                            {
                                resultA.ElementAt(i).ex_Date = resultB.ElementAt(y).ex_Date;
                            }
                        }
                    }
                    else
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
                }

                resultA = resultA.OrderByDescending(a => a.dtm_from);


                var result = from c in resultA
                             select new[] {
                                 string.Empty,
                                  "<span " + (c?.bit_Active != true ? "style='color:" + UtilConstants.String_DeActive_Color + ";'" : "") + ">" + c?.subjectName + "</span>",
                                 c?.dtm_from_to,
                                 c.firstCheck ,
                                 c.reCheck    ,
                                 c.remark     ,
                                 c.grade      ,
                                 c.recurrent  ,
                                 (c?.grade == "Fail" || string.IsNullOrEmpty(c?.grade)) ? string.Empty : c?.ex_Date?.ToString(Resource.lbl_FORMAT_DATE),
                                  c.Status

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerSubjectsOfCourse", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerSubjectsOfCourse2(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());
                var subjectType = (int?)UtilConstants.ApproveType.SubjectResult;
                var datacourse_ = CourseDetailService.Get(a => a.Course.IsDeleted != true && a.TMS_Course_Member.Any(x => x.Member_Id == id && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)) && a.TMS_APPROVES.Any(x => x.int_Type == subjectType));
                IEnumerable<int?> datacourse = new List<int?>();
                var listtemp = new List<int?>();
                foreach (var item in datacourse_)
                {
                    var item_ = item.TMS_APPROVES.LastOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                    if (item_ != null)
                    {
                        if (item_.int_id_status == (int)UtilConstants.EStatus.Approve)
                        {
                            listtemp.Add(item.Id);
                        }
                    }
                }
                datacourse = listtemp;
                var data = CourseMemberService.Get(a => datacourse.Contains((int)a.Course_Details_Id) && a.Member_Id == id && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true);

                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Course_Detail?.SubjectDetail?.Name
                                                            : sortColumnIndex == 2 ? c?.Course_Detail?.dtm_time_from
                                                            : sortColumnIndex == 6 ? (object)c?.Course_Detail?.SubjectDetail?.RefreshCycle
                                                            : c?.Course_Detail?.SubjectDetail?.Name);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                //if (sortDirection == null)
                //{
                //    sortDirection = "desc";
                //}
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                   : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                var resultA = from c in displayed
                              select new ProfileSubjectModel
                              {
                                  bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                  //SubjectCode = c?.Course_Detail?.SubjectDetail?.Code,
                                  dtm_from = c?.Course_Detail?.dtm_time_from,
                                  dtm_from_to = DateUtil.DateToString(c?.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c?.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                                  subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                  TypeLearning = TypeLearningIcon(c?.Course_Detail?.type_leaning),
                                  firstCheck = ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == id)),
                                  reCheck = ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == id)),

                                  remark = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Type == true ? GetRemarkCheckFail(c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Id) : (c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Remark != null ? c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Remark?.Replace("!!!!!", "<br />") : null),
                                  grade = returnpointgrade(2, c?.Member_Id, c?.Course_Details_Id),

                                  recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                                  courseDetails = c?.Course_Detail,
                                  memberId = c?.Member_Id,
                                  checkstatus = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.StatusCertificate,
                                  codecertificate = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id && x.StatusCertificate == 0)?.CertificateSubject,
                                  Path = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Path ?? "",
                                  //Status = (bool)c?.Course_Detail?.TMS_APPROVES?.Any(a => a.int_Type == (int)Constants.ApproveType.SubjectResult && a.int_id_status == (int)Constants.EStatus.Approve)
                                  //  ? "<span class='label label-success'>Success</span>"
                                  //  : "<span class='label label-danger'>In process</span>"

                              };

                resultA = resultA.ToList().OrderBy(a => a.subjectName.Trim()).ThenBy(a => a.dtm_from);
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

                //resultA = resultA.OrderBy(a => a.subjectName.Trim()).ThenByDescending(a => a.dtm_from);

                var result = from c in resultA
                             select new object[] {
                                                  //string.Empty,
                                "<span " + (c?.bit_Active != true ? "style='color:" + UtilConstants.String_DeActive_Color + ";'" : "") + ">" +  c?.subjectName + "</span>",
                                c?.dtm_from_to,
                                c?.firstCheck ,
                                c?.reCheck    ,
                                c?.remark     ,
                                c?.grade      ,
                                c?.recurrent  ,
                                (c?.firstCheck == null  && c?.reCheck == null) ? string.Empty : c?.ex_Date?.ToString(Resource.lbl_FORMAT_DATE),
                                !string.IsNullOrEmpty(c?.codecertificate)
                            ? (c?.checkstatus == 0 ? (!string.IsNullOrEmpty(c?.Path) ? ( "<a  href='" + ConfigurationSettings.AppSettings["AWSLinkS3"] +c?.Path +"' target='_blank'  data-toggle='tooltip'>"+c?.codecertificate+"</a>" ) : c?.codecertificate) : "") :""

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
        public string GetRemarkCheckFail(int? resultid)
        {
            var remark = "";
            var remarkresult = CourseService.GetCourseResultCheckFail(a => a.CourseResultID == resultid);
            remark = remarkresult.FirstOrDefault()?.RemarkContent;
            return remark;
        }
        public ActionResult AjaxHandlerConductedCourseOfSubject(jQueryDataTableParamModel param, string id = "")
        {
            try
            {
                var instructorId = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                var dataCourse_Detail = CourseDetailService.Get(a => a.SubjectDetail.Code.Equals(id) && a.Course_Detail_Instructor.Any(x => x.Instructor_Id == instructorId) && a.Course.IsActive == true && a.IsDeleted == false && a.SubjectDetail.IsDelete == false);


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
                                                  DateUtil.DateToString( c?.dtm_time_from ,Resource.lbl_FORMAT_DATE)+"<br />"+
                                                   (c?.time_from != null ? (c?.time_from?.Substring(0, 2)+c?.time_from?.Substring(2)) : ""),

                                                    DateUtil.DateToString( c?.dtm_time_to ,Resource.lbl_FORMAT_DATE)+"<br />"+
                                                    (c?.time_to != null ? (c?.time_to?.Substring(0, 2)+c?.time_to?.Substring(2)) : ""),
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
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerConductedCourseOfSubject", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Modify(int? id, int? type)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            if (
                 (type == (int)UtilConstants.ROLE.Trainee ||
                  type == (int)UtilConstants.ROLE.Instructor
                     ))
            {
                var model = new EmployeeModelModify();
                model.PathEducation = GetByKey("PathEducation");
                //type Trainee or Instructor
                model.Control = type.Value;
                model.Nations = new SelectList(ConfigService.GetNation(a=>a.isactive == true).OrderBy(a => a.Nation_Name).ToDictionary(a => a.Nation_Code, a => a.Nation_Name), "Key", "Value");
                model.PlacesOfBirths = new SelectList(ConfigService.GetProvince(), "Value", "Value");
                model.CourseTypes = new SelectList(UtilConstants.CourseCourseAreasDictionary(), "Key", "Value");
                model.Company = new SelectList(_repoCompanyService.Get(a=>a.IsActive == true).OrderBy(a => a.str_Name).ToDictionary(b => b.Company_Id, b => b.str_Name), "Key", "Value");
                // model.Departments = new SelectList(GetDepartmentModel().ToDictionary(a => a.Id, a => a.Code + " - " + a.DepartmentName),"Key","Value");
                model.Jobtitles = new SelectList(_repoJobtitleService.Get().OrderBy(a => a.Name).ToDictionary(b => b.Id, b => b.Name), "Key", "Value");
                model.Genders = new SelectList(UtilConstants.GenderDictionary(), "Key", "Value");
                model.InstructorTypes = UtilConstants.SearchEmployee();
                if (type == (int)UtilConstants.ROLE.Instructor)
                {
                    model.Subjects = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true)
                          .OrderBy(a => a.Code)
                          .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name));
                }

                var keyMentor = GetByKey("MENTOR");
                model.CheckHannahMentor = keyMentor.Equals("1") ? true : false;
                var entity = EmployeeService.GetById(id);
                //var xxx = string.IsNullOrEmpty(entity.Password) ? "" : Common.DecryptString(entity.Password);
                if (entity != null)
                {
                    //if (type != entity.int_Role)
                    //{
                    //    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                    //    return RedirectToAction("Index", "Home");
                    //}
                    model.Id = entity.Id;
                    model.NameImage = entity.avatar;
                    model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                    model.Role = entity.int_Role == (int)UtilConstants.ROLE.Instructor
                        ? (int)UtilConstants.ROLE.Instructor
                        : (int)UtilConstants.ROLE.Trainee;
                    model.Eid = entity.str_Staff_Id;
                    model.PersonalId = entity.PersonalId;
                    model.EmployeeType = entity.bit_Internal == true
                        ? (int)UtilConstants.CourseAreas.Internal
                        : (int)UtilConstants.CourseAreas.External;
                    model.CompanyId = entity.Company_Id;
                    model.DepartmentId = entity.Department_Id;
                    model.JobTitleId = entity.Job_Title_id;
                    model.dtm_Birthdate = entity.dtm_Birthdate.HasValue ? entity.dtm_Birthdate.Value.ToString("dd/MM/yyyy") : "";
                    model.Gender = entity.Gender;
                    model.PlaceOfBirth = entity.str_Place_Of_Birth;
                    model.Email = entity.str_Email;
                    model.Nation = entity.Nation;
                    model.Phone = entity.str_Cell_Phone;
                    model.dtm_Join_Date = entity.dtm_Join_Date.HasValue ? entity.dtm_Join_Date.Value.ToString("dd/MM/yyyy") : "";
                    model.ResignationDate = entity.non_working_day;
                    model.JoinedPartyDate = entity.Join_Party_Date;
                    model.InstructorType = entity.Trainee_Type.Select(a => a.Type);
                    model.Passport = entity.Passport;
                    model.IsExaminer = entity.IsExaminer;
                    model.RelevantDepartmentId = entity.Trainee_TrainingCenter.Select(a => a.khoidaotao_id).ToList();
                    model.InstructorSubjects =
                        entity.Instructor_Ability.Where(a =>a.SubjectDetail.Subject_TrainingCenter.Any(x=> model.RelevantDepartmentId.Contains(x.khoidaotao_id)) && a.SubjectDetail.IsActive == true && a.SubjectDetail.CourseTypeId.HasValue && a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General).Select(b => new EmployeeModelModify.EmployeeSubject()
                        {
                            Allowance = b.Allowance ?? 0,
                            Id = b.id,
                            Name = b.SubjectDetail.Name,
                            Code = b.SubjectDetail.Code,
                            SubjectId = (int)b.SubjectDetailId,
                            InstructorId = (int)b.InstructorId,
                            CreateDate = b.CreateDate
                        });
                    model.Educations = entity.Trainee_Record.Where(a => a.bit_Deleted == false).Select(b => new EmployeeModelModify.EmployeeEducation
                    {
                        //IsDelete sử dụng ngoài View
                        Id = b.Trainee_Record_Id,
                        TimeTo = b.dtm_time_to,
                        CourseName = b.str_Subject,
                        Note = b.str_note,
                        Organization = b.str_organization,
                        TimeFrom = b.dtm_time_from,
                        IsDeleted = b.bit_Deleted == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No,
                        FileUploads = b.Trainee_Upload_Files.Where(c => (c.IsDeleted == false || c.IsDeleted == null)).Select(d => new EmployeeModelModify.EmployeeEducation.FileUpload
                        {
                            Id = d.Id,
                            ModelNameImg = d.Name,
                            IsDeleted = d.IsDeleted == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No
                        }).ToList(),

                    });
                    
                    model.password = entity.Password;
                }
                var lstDepartment = new List<int?> { model.DepartmentId };
                model.RelevantDepartmentList = GetDepartmentAcestorModelCustom(CurrentUser.IsMaster, model.RelevantDepartmentId);
                model.Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster, lstDepartment);
                return View(model);
            }
            TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<ActionResult> Modify(EmployeeModelModify model, FormCollection form)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                if (!ModelState.IsValid)
                    return Json(new { result = false, message = MessageInvalidData(ModelState) });
                var datacheckEID = EmployeeService.GetEmp(a => a.str_Staff_Id.ToLower().Trim() == model.Eid.ToLower().Trim());
                if (model.Id.HasValue)
                {
                    //update avatar or not
                    if (model.ImgFile != null)
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
                    

                }
                else
                {
                    
                    if (datacheckEID.Any())
                    {
                        throw new Exception(string.Format(Messege.DataIsExists, Resource.TRAINEE_EID, model.Eid));
                    }
                    var newAvatar = SaveImage(model.ImgFile, model.Eid, UtilConstants.Upload.Trainee);
                    if (newAvatar.result)
                    {
                        model.ImgAvatar = newAvatar.data.ToString();
                    }
                    else
                    {
                        return Json(newAvatar);
                    }

                    if (string.IsNullOrEmpty(form["autuGenerate"]))
                    {
                        if (!string.IsNullOrEmpty(model.password))
                        {
                            model.password = model.password.Trim();
                            var checkPassword = IsPasswordAllowed(model.password);
                            if (!checkPassword)
                            {
                                return Json(new AjaxResponseViewModel { result = false, message = Messege.RegEx_PASSWORD });
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(model.PasswordConfirm))
                                {
                                    return Json(new AjaxResponseViewModel { result = false, message = Messege.VALIDATION_USER_CONFIRMPASSWORD_EXPRESSION });
                                }
                                else if (!string.IsNullOrEmpty(model.PasswordConfirm))
                                {
                                    if (!model.PasswordConfirm.Trim().Equals(model.password))
                                    {
                                        return Json(new AjaxResponseViewModel { result = false, message = Messege.VALIDATION_USER_CONFIRMPASSWORD_EXPRESSION });
                                    }
                                }
                            }

                        }
                        else
                        {
                            return Json(new AjaxResponseViewModel { result = false, message = Messege.RegEx_PASSWORD });
                        }

                    }
                    else
                    {
                        model.password = string.Empty;
                    }

                }

                // upload bằng
                if (model.Educations != null)
                {
                    foreach (var edu in model.Educations.Where(a => a.IsDeleted == (int)UtilConstants.BoolEnum.No))
                    {
                        if (edu.FileImage == null) continue;
                        var listNameImg = new List<string>();
                        foreach (var newImg in edu.FileImage.Select(item => SaveImage(item, model.Eid, UtilConstants.Upload.Education)))
                        {
                            if (newImg.result && !string.IsNullOrEmpty(newImg.data.ToString()))
                            {
                                listNameImg.Add(newImg.data.ToString());
                            }
                            //else
                            //{
                            //    return Json(newImg);
                            //}
                        }
                        edu.ListNameImage = listNameImg.ToArray();
                    }
                }
                //end upload bằng
                // var employee = EmployeeService.Modify(model, CurrentUser.USER_ID);
                

                // var entity = EmployeeService.Modify(model, Common.EncryptString(randomPass));
               
                var entity = EmployeeService.Modify(model, form, model.password);
                if (entity != null)
                {
                    //Chi gui mail Create , khong gui mail khi update
                    if (datacheckEID.IsNullOrEmpty())
                    {
                        if (entity.int_Role == (int)UtilConstants.ROLE.Instructor)
                        {
                            await Task.Run(() =>
                            {
                                Sent_Email_TMS(entity, null, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreateInstructor_Trainee);
                            });
                        }
                        else
                        {
                            await Task.Run(() =>
                            {
                                Sent_Email_TMS(null, entity, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreateInstructor_Trainee);
                            });
                        }   
                    }


                    // PostAnalyticAction(model, entity);

                    //await Task.Run(() =>
                    //{
                    //    var callLms = CallServices(UtilConstants.CRON_USER);
                    //    //if (!callLms)
                    //    //{
                    //    //    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/Modify", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), entity.str_Staff_Id));
                    //    //}
                    //});

                    #region [--------CALL LMS (CRON USER)----------]
                    //var callLms = CallServices(UtilConstants.CRON_USER);
                    //if (!callLms)
                    //{
                    //    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/Modify", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), entity.str_Staff_Id));
                    //    return Json(new AjaxResponseViewModel()
                    //    {
                    //        message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), entity.str_Staff_Id),
                    //        result = false
                    //    });
                    //}

                    //if (entity.int_Role == 1)
                    //{
                    //    List<Trainee> lstTrainee = new List<Trainee>();
                    //    lstTrainee.Add(entity);
                    //    //SyncTraineeToPortal(lstTrainee, Session["auth_token"].ToString());
                    //}
                    #endregion
                }



                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS };
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS });

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }
        [AllowAnonymous]
        public ActionResult Trainee_Point(int? id)
        {
            var entity = EmployeeService.GetById(id);
            if (entity != null)
            {
                var model = new EmployeeModelPoint();

                #region [Hannah Mentor]
                var keyHannahMentor = GetByKey("MENTOR");
                model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

                if (model.CheckHannahMentor)
                {
                    var hannahMentor = string.Empty;
                    if (entity.Trainee_Type.Any())
                    {
                        foreach (var item in entity.Trainee_Type)
                        {
                            hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                        }
                    }
                    model.HannahMentor = hannahMentor;

                }
                #endregion

                model.Id = entity.Id;
                model.Avatar = entity.avatar;
                model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                model.Eid = entity.str_Staff_Id;
                model.PersonId = entity.PersonalId;
                model.Passport = entity.Passport;
                model.Email = entity.str_Email;
                model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
                model.Type = entity.bit_Internal == true
                    ? UtilConstants.CourseAreas.Internal.ToString()
                    : UtilConstants.CourseAreas.External.ToString();
                model.PlaceOfBirth = entity.str_Place_Of_Birth;
                model.Department = entity.Department?.Code + " " + entity.Department?.Name;
                model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
                model.Jobtitle = entity.JobTitle?.Name;
                model.Nation = _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
                model.Company = entity.Company?.str_Name;
                model.Phone = entity.str_Cell_Phone;
                model.DateOfJoin = entity.dtm_Join_Date?.ToString(Resource.lbl_FORMAT_DATE);
                model.ResignationDate = entity.non_working_day?.ToString(Resource.lbl_FORMAT_DATE);
                model.Nation = entity.Nation;

                #region ----------- get value point trainee-------------------------

                var datamenberapproval = CourseMemberService.Get(a => a.Member_Id == id && a.IsActive == true);
                var datacourse = CourseDetailService.Get(a => datamenberapproval.Any(x => x.Course_Details_Id == a.Id), new[] { (int)UtilConstants.ApproveType.SubjectResult }).Select(x => x.CourseId).Distinct();
                var dataCourse_Detail_Id = CourseDetailService.Get(a => datacourse.Contains(a.CourseId)).Select(a => a.Id);
                var data = CourseMemberService.Get(a => dataCourse_Detail_Id.Contains((int)a.Course_Details_Id) && a.Member_Id == id).Where(a =>
                                a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve));

                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Course_Detail?.SubjectDetail?.Name
                                                            : sortColumnIndex == 2 ? c?.Course_Detail?.dtm_time_from
                                                            : sortColumnIndex == 6 ? (object)c?.Course_Detail?.SubjectDetail?.RefreshCycle
                                                            : c?.Course_Detail?.SubjectDetail?.Name);

                filtered = filtered.OrderByDescending(orderingFunction);
                var resultA = from c in filtered.ToArray()
                              select new ProfileSubjectModel1
                              {
                                  bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                  SubjectCode = c?.Course_Detail?.SubjectDetail?.Code,
                                  dtm_from = c?.Course_Detail?.dtm_time_from,
                                  dtm_from_to = c?.Course_Detail?.dtm_time_from.Value.ToString(Resource.lbl_FORMAT_DATE) ?? "" + " - " + c?.Course_Detail?.dtm_time_to.Value.ToString(Resource.lbl_FORMAT_DATE) ?? "",
                                  subjectName = c?.Course_Detail?.SubjectDetail?.Name,

                                  point = string.Format("{0:0.#}", GetPointRemark(UtilConstants.DetailResult.Score, c?.Member_Id, c?.Course_Details_Id)),
                                  remark = GetPointRemark(UtilConstants.DetailResult.Remark, c?.Member_Id, c?.Course_Details_Id),
                                  grade = c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.point != null ? ReturnResult(c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Course_Detail?.SubjectDetail?.Subject_Score, c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.point, c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Result) : c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Result,

                                  recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                                  memberId = c?.Member_Id,
                              };
                #endregion

                model.trainee_point = resultA.OrderBy(a => a.subjectName).ThenByDescending(a => a.dtm_from);
                return View(model);
            }
            TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
            return View();
        }
        public ActionResult Details(int? id, int? type)
        {
            if (
                 (type == (int)UtilConstants.ROLE.Trainee ||
                  type == (int)UtilConstants.ROLE.Instructor
                     ))
            {
                var entity = EmployeeService.GetById(id);
                if (entity != null)
                {
                    var model = new EmployeeModelDetails();
                    //if (type != entity.int_Role)
                    //{
                    //    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                    //    return RedirectToAction("Index", "Home");
                    //}

                    #region [Hannah Mentor]
                    var keyHannahMentor = GetByKey("MENTOR");
                    model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

                    if (model.CheckHannahMentor)
                    {
                        var hannahMentor = string.Empty;
                        if (entity.Trainee_Type.Any())
                        {
                            foreach (var item in entity.Trainee_Type)
                            {
                                hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                            }
                        }
                        model.HannahMentor = hannahMentor;

                    }
                    #endregion


                    model.Control = type.Value;
                    model.Id = entity.Id;
                    model.Avatar = entity.avatar;
                    model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                    model.Eid = entity.str_Staff_Id;
                    model.PersonId = entity.PersonalId;
                    model.Passport = entity.Passport;
                    model.Email = entity.str_Email;
                    model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
                    model.Type = entity.bit_Internal == true
                        ? UtilConstants.CourseAreas.Internal.ToString()
                        : UtilConstants.CourseAreas.External.ToString();
                    model.PlaceOfBirth = entity.str_Place_Of_Birth;
                    model.Department = entity.Department?.Name;
                    model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
                    model.Jobtitle = entity.JobTitle?.Name;
                    model.Nation = _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
                    model.Company = entity.Company?.str_Name;
                    model.Phone = entity.str_Cell_Phone;
                    model.DateOfJoin = entity.dtm_Join_Date?.ToString(Resource.lbl_FORMAT_DATE);
                    model.ResignationDate = entity.non_working_day?.ToString(Resource.lbl_FORMAT_DATE);
                    model.Nation = entity.Nation;
                    return View(model);
                }

            }
            TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
            return RedirectToAction("Index", "Home");
        }
        [AllowAnonymous]
        public ActionResult PartialListJobStandard(int? id, int type = (int)UtilConstants.Switch.Horizontal)
        {

            var traineeHistories =
                _repotraineeHistoryService.Get(a => a.Trainee_Id == id).OrderByDescending(b => b.Id).ToList();

            var model = new TraineeJobModel()
            {
                Type = type,
                TraineeHistories = traineeHistories,

            };
            return PartialView("_partials/_PartialTraineeJobStandard", model);
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
        public async Task<ActionResult> RandomResetPassword(int? id)
        {
            try
            {
                var model = EmployeeService.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/RandomResetPassword", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var newPass = TMS.Core.Utils.Common.RandomCharecter();
               // model.Password = newPass;
                //var token = Session["auth_token"].ToString();
                //var result = SyncTraineeToSSO(model, token);
                if (true)//result
                {
                    model.Password = Common.EncryptString(newPass);
                    model.LmsStatus = StatusModify;
                    EmployeeService.Update(model);

                    var model1 = UserContext.GetByUser(model.str_Staff_Id);
                    if (model1 != null)
                    {
                        model1.PASSWORD = Common.EncryptString(newPass);
                        UserContext.Update(model1);
                    }

                    await Task.Run(() =>
                    {
                        //var fullName = model.FirstName.Trim() + " " + model.LastName.Trim() + "";
                        var fullName = ReturnDisplayLanguage(model.FirstName, model.LastName) + "";
                        Sent_Email_TMS(null, model, null, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee);
                        #region [--------CALL LMS----------]
                        var callLms = CallServices(UtilConstants.CRON_USER);
                        if (!callLms)
                        {
                            LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/RandomResetPassword", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, fullName));
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
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/RandomResetPassword", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Delete(int? id)
        {
            try
            {
                var model = EmployeeService.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.NO_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //var fullName = model.FirstName.Trim() + " " + model.LastName.Trim();
                var fullName = ReturnDisplayLanguage(model.FirstName, model.LastName);

                if (model.TMS_Course_Member.Any(a => a.IsDelete != true))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, fullName),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //if (model.LmsStatus == StatusModify)
                //{
                //    return Json(new AjaxResponseViewModel()
                //    {
                //        message = string.Format(Messege.DELETED_UNSUCCESS_SENDLMS, fullName),
                //        result = false
                //    }, JsonRequestBehavior.AllowGet);

                //}
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
                        //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/Delete", string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, fullName));
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/Delete", ex.Message);
                return Json(new
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        public FileResult ExportEXCEL()
        {
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
            var role = string.IsNullOrEmpty(Request.QueryString["control"]) ? -1 : Convert.ToInt32(Request.QueryString["control"].Trim());
            var mentor = string.IsNullOrEmpty(Request.QueryString["Mentor[]"]) ? string.Empty : Request.QueryString["Mentor[]"].Trim();
            var filecontent = ExportEXCEL(comOrDepId, fStatus, fJobTitle, fGender, ddlType, fName, fEmail, fPhone, fStaffId, role, mentor);
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, (role == (int)UtilConstants.ROLE.Trainee ? UtilConstants.ROLE.Trainee.ToString() : UtilConstants.ROLE.Instructor.ToString()) + "List_" + DateTime.Now.ToString(Resource.lbl_FORMAT_DATE) + ".xlsx");
            }
            return null;
        }
        private byte[] ExportEXCEL(int comOrDepId, int fStatus, int fJobTitle, int fGender, int ddlType, string fName, string fEmail, string fPhone, string fStaffId, int role, string mentor)
        {
            try
            {
                #region [Phan lay Du lieu]
                List<int> userPermission = CurrentUser.PermissionIds;
                bool bitInternal = ddlType != 0;
                bool isActive = fStatus != 0;
                List<int?> lstDepartment = new List<int?>();
                Department department = DepartmentService.GetById(comOrDepId);
                if (department != null)
                {
                    var departmentIds = DepartmentService.Get(a => a.Ancestor.Contains(department.Code)).OrderBy(b => b.Ancestor).Select(a => a.Id);
                    if (departmentIds.Any())
                    {
                        foreach (var id in departmentIds)
                        {
                            lstDepartment.Add(id);
                        }
                    }
                }
                IQueryable<Trainee> data = EmployeeService.Get(a => a.IsDeleted == false
                //&& userPermission.Any(x => a.Department_Id == x)
                && (role == -1 || (role != (int)UtilConstants.ROLE.Instructor || a.int_Role == role))
                && (comOrDepId == -1 || lstDepartment.Contains(a.Department_Id))
                  && (ddlType == -1 || a.bit_Internal == bitInternal)
                && (string.IsNullOrEmpty(fName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(fName.Trim())) || ((a.LastName.Trim() + " " + a.FirstName.Trim()).Contains(fName.Trim())))
                && (fJobTitle == -1 || a.Job_Title_id == fJobTitle)
                && (string.IsNullOrEmpty(fEmail) || a.str_Email.Contains(fEmail.Trim()))
                && (string.IsNullOrEmpty(fPhone) || a.str_Cell_Phone.Contains(fPhone.Trim()))
                && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId.Trim()))
                && (fGender == -1 || a.Gender == fGender)
                && (fStatus == -1 || a.IsActive == isActive), true);
                if (!string.IsNullOrEmpty(mentor))
                {
                    var mentorCodes = mentor.Split(new char[] { ',' }).Select(int.Parse).ToList();

                    if (mentorCodes != null && mentorCodes.Any())

                    {
                        data = data.Where(a => a.Trainee_Type.Any(b => mentorCodes.Contains(b.Type.Value)));

                    }
                }
                #endregion

                #region [Xuat ra Excel]
                var templateFilePath = "";
                if (role == 1)
                {
                    templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/InstructorList.xlsx");
                }
                else
                {
                    templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/TraineeList.xlsx");
                }

                var template = new FileInfo(templateFilePath);
                var ms = new MemoryStream();
                byte[] bytes = null;

                ExcelPackage excelPackage;
                using (excelPackage = new ExcelPackage(template, false))
                {
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    if (data.Any())
                    {
                        var startRow = 11;
                        var count = 0;
                        foreach (var item in data)
                        {
                            var col = 2;
                            count++;

                            var cellNo = worksheet.Cells[startRow, col];
                            cellNo.Value = count;
                            cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellType = worksheet.Cells[startRow, ++col];
                            cellType.Value = item.bit_Internal == true ? "Internal" : "External";
                            cellType.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellType.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellType.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellCompany = worksheet.Cells[startRow, ++col];
                            cellCompany.Value = item.Company_Id.HasValue ? item?.Company?.str_Name?.ToString() : "";
                            cellCompany.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellCompany.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellCompany.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellDeparment = worksheet.Cells[startRow, ++col];
                            cellDeparment.Value = item?.Department?.Name?.ToString() ?? "";
                            cellDeparment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellDeparment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellDeparment.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellEid = worksheet.Cells[startRow, ++col];
                            cellEid.Value = item?.str_Staff_Id?.ToString() ?? "";
                            cellEid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellEid.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellEid.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellFullName = worksheet.Cells[startRow, ++col];
                            cellFullName.Value = ReturnDisplayLanguage(item?.FirstName, item?.LastName);
                            cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellJobtitleName = worksheet.Cells[startRow, ++col];
                            cellJobtitleName.Value = item?.JobTitle?.Name?.ToString() ?? "";
                            cellJobtitleName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellJobtitleName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellJobtitleName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellNationality = worksheet.Cells[startRow, ++col];
                            cellNationality.Value = item?.Nation?.ToString() ?? "";
                            cellNationality.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNationality.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellNationality.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var gender = item?.Gender ?? (int)UtilConstants.Gender.Others;
                            var cellGender = worksheet.Cells[startRow, ++col];
                            cellGender.Value = UtilConstants.GenderDictionary()[gender];
                            cellGender.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellGender.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellGender.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellDOB = worksheet.Cells[startRow, ++col];
                            cellDOB.Value = item?.dtm_Birthdate.HasValue == true ? string.Format("{0:dd/MM/yyyy}", item.dtm_Birthdate.Value) : "";
                            cellDOB.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellDOB.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellDOB.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellPlaceOfBirth = worksheet.Cells[startRow, ++col];
                            cellPlaceOfBirth.Value = item?.str_Place_Of_Birth?.ToString() ?? "";
                            cellPlaceOfBirth.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellPlaceOfBirth.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellPlaceOfBirth.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellJoiningDate = worksheet.Cells[startRow, ++col];
                            cellJoiningDate.Value = item?.dtm_Join_Date.HasValue == true ? string.Format("{0:dd/MM/yyyy}", item.dtm_Join_Date.Value) : "";
                            cellJoiningDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellJoiningDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellJoiningDate.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellPassport = worksheet.Cells[startRow, ++col];
                            cellPassport.Value = item?.Passport?.ToString() ?? "";
                            cellPassport.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellPassport.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellPassport.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var cellPhone = worksheet.Cells[startRow, ++col];
                            cellPhone.Value = item?.str_Cell_Phone?.ToString() ?? "";
                            cellPhone.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellPhone.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellPhone.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            startRow++;
                        }
                    }
                    bytes = excelPackage.GetAsByteArray();
                    return bytes;
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/ExportEXCEL", ex.Message);
                return null;
            }
        }
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateEmployee(int isParticipate, string id)
        {
            try
            {
                int idInstructor = int.Parse(id);
                var removeInstructor = EmployeeService.GetById(idInstructor);
                if (removeInstructor == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/SubmitSetParticipateEmployee", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false

                    }, JsonRequestBehavior.AllowGet);
                }

                if (isParticipate == 1)
                {
                    removeInstructor.IsActive = false;
                }
                else
                {
                    removeInstructor.IsActive = true;
                }
                removeInstructor.LmsStatus = StatusModify;
                EmployeeService.Update(removeInstructor);
                var fullName = ReturnDisplayLanguage(removeInstructor.FirstName, removeInstructor.LastName);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON USER)----------]
                    var callLms = CallServices(UtilConstants.CRON_USER);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/SubmitSetParticipateEmployee", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, fullName));
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
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/SubmitSetParticipateEmployee", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }


        }
        #region [----------------Group Trainee---------------]
        public ActionResult Group()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ModifyGroup(int? id)
        {
            var model = new EmployeeGroupModify();
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
                                   string.Format("{0} - {1}", a.str_Staff_Id, ReturnDisplayLanguage(a.FirstName, a.LastName)));
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
        public ActionResult DeleteGroup(int? id)
        {
            try
            {
                var model = EmployeeService.GetGroupById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/DeleteGroup", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/DeleteGroup", ex.Message);
                return Json(new
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult SetParticipateGroupEmployee(int isStatus, string id)
        {
            try
            {
                var idTrainee = int.Parse(id);
                var removeGroupTrainee = EmployeeService.GetGroupById(idTrainee);
                if (removeGroupTrainee == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/SetParticipateGroupEmployee", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
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
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/SetParticipateGroupEmployee", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult ModifyGroup(EmployeeGroupModify model)
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
                EmployeeService.Modify(model);
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/ModifyGroup", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }
        public ActionResult AjaxHandlerGroupEmployee(jQueryDataTableParamModel param)
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
                         "<a href='"+@Url.Action("ModifyGroup",new{id = c.Id})+"' title='Edit'  data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' ></i></a>"+
                         ((User.IsInRole("/GroupUser/Delete")) ? verticalBar +"<a title='Delete'  href='javascript:void(0)' onclick='calldelete(" + c.Id  + ")'  data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerGroupEmployee", ex.Message);
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
                var data = EmployeeService.Get(a => a.GroupTrainee_Item.Any(b => b.GroupTraineeId == id)).ToList().Select(c => new SubGroupEmployee()
                {
                    Code = c.str_Staff_Id,
                    //FullName = c.FirstName.Trim() + " " + c.LastName.Trim(),
                    FullName = ReturnDisplayLanguage(c.FirstName.Trim(), c.LastName.Trim()),

                    Job = c.JobTitle.Name,
                    Phone = c.str_Cell_Phone
                });

                IEnumerable<SubGroupEmployee> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

                Func<SubGroupEmployee, object> orderingFunction = (s =>
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

                var displayed = filtered.Skip(param.iDisplayStart)/*.Take(param.iDisplayLength)*/;
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
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Employee/AjaxHandlerGroupTraneeSub", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AjaxHandlerListSelectTrainee(string id, FormCollection form)
        {
            var filterCodeOrName = string.IsNullOrEmpty(form["FilterCodeOrName"])
                   ? string.Empty
                   : form["FilterCodeOrName"].Trim().ToLower();
            StringBuilder HTML = new StringBuilder();
            HTML.Append("<ul>");
            HTML.Append("<li><input class='assignedParentFunc-1' value='-1' multiple type='checkbox' id='checkAll'/><span>&nbspOther</span>");
            HTML.Append("<ul>");
            var list_trainee = EmployeeService.GetEmployeeList(filterCodeOrName);
            int GroupTraineeId = !string.IsNullOrEmpty(id) ? int.Parse(id) : -1;
            if (list_trainee.Count() > 0)
            {
                var listTraineeGroup = EmployeeService.GetGroupById(GroupTraineeId)?.GroupTrainee_Item?.Select(a => a.TraineeId);
                foreach (var item in list_trainee)
                {
                    HTML.Append("<li>");
                    HTML.AppendFormat(" <input  data-id='{0}' data-parentname='Other'  multiple value='{0}' data-name='{2}' class='TraineeIds' name='TraineeIds' id='trainee_" + item.traineeId + "' type='checkbox' " + ((listTraineeGroup != null && listTraineeGroup.Contains(item.traineeId)) ? "Checked" : "") + " /><input type='hidden' value='{1}' name='TraineeIds2'  /><label for='trainee_" + item.traineeId + "'>&nbsp{2}</label>", item.traineeId, item.traineeId, string.Format("{0} - {1}", item?.str_Staff_Id, ReturnDisplayLanguage(item?.FirstName, item?.LastName)));
                    HTML.Append("</li>");
                }
            }
            HTML.Append("</ul>");
            HTML.Append("</li>");
            HTML.Append("</ul>");
            var jsonresult = Json(HTML.ToString());
            jsonresult.MaxJsonLength = int.MaxValue;
            return jsonresult;
        }
        [HttpPost]
        #endregion
        #region Print Profile
        public ActionResult EmployeeProfilePrint(int? id, int? type)
        {

            if (
                (type == (int)UtilConstants.ROLE.Trainee ||
                 type == (int)UtilConstants.ROLE.Instructor)
                )
            {
                var model = new EmployeeModelPrint();

                var entity = EmployeeService.GetById(id);
                if (entity == null) return PartialView("EmployeeProfilePrint");
                #region [Hannah Mentor]
                var keyHannahMentor = GetByKey("MENTOR");
                model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

                if (model.CheckHannahMentor)
                {
                    var hannahMentor = string.Empty;
                    if (entity.Trainee_Type.Any())
                    {
                        foreach (var item in entity.Trainee_Type)
                        {
                            hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                        }
                    }
                    model.HannahMentor = hannahMentor;

                }
                #endregion
                model.Control = type.Value;
                model.Id = entity.Id;
                model.Avatar = entity.avatar;
                model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                model.Eid = entity.str_Staff_Id;
                model.PersonalId = entity.PersonalId;
                model.Passport = entity.Passport;
                model.Email = entity.str_Email;
                model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
                model.Type = entity.bit_Internal == true
                    ? UtilConstants.CourseAreas.Internal.ToString()
                    : UtilConstants.CourseAreas.External.ToString();
                model.PlaceOfBirth = entity.str_Place_Of_Birth;
                model.Department = entity.Department?.Code + " " + entity.Department?.Name;
                model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
                model.JobTitle = entity.JobTitle?.Name;
                model.Nation =
                    _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
                model.Company = entity.Company?.str_Name;
                model.Phone = entity.str_Cell_Phone;
                model.DateOfJoin = entity.dtm_Join_Date?.ToString(Resource.lbl_FORMAT_DATE);
                model.ResignationDate = entity.non_working_day?.ToString(Resource.lbl_FORMAT_DATE);
                model.Educations = entity.Trainee_Record?.OrderByDescending(a => a.Trainee_Record_Id).Select(a => new EmployeeModelPrint.Education()
                {
                    Time = a.dtm_time_from?.ToString(Resource.lbl_FORMAT_DATE) + " - " + a.dtm_time_to?.ToString(Resource.lbl_FORMAT_DATE),
                    Course = a.str_Subject,
                    Organization = a.str_organization,
                    Note = a.str_note

                });
                model.Contracts =
                    entity.Trainee_Contract?.OrderByDescending(a => a.id).Select(a => new EmployeeModelPrint.Contract()
                    {
                        ContractNo = a.contractno,
                        ExpiryDate = DateUtil.DateToString(a?.expire_date, Resource.lbl_FORMAT_DATE),
                        Description = a.description
                    });
                var course = CourseDetailService.Get(
                    a =>
                    a.TMS_Course_Member.Any(x => x.Member_Id == entity.Id && x.IsActive == true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)) &&
                         a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)).ToList()
                    .Where(a =>
                        a.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?
                            .int_id_status == (int)UtilConstants.EStatus.Approve).Select(x => x.Course).Distinct();

                // var datamenberapproval = entity.TMS_Course_Member?.Where(a => a.IsDelete == false && a.IsActive);
                model.TrainningCourses = course.Select(a => new EmployeeModelPrint.TrainningCourse()
                {
                    Code = a.Code,
                    Name = a.Name,
                    Time = DateUtil.DateToString(a.StartDate, Resource.lbl_FORMAT_DATE) + " - " + DateUtil.DateToString(a.EndDate, Resource.lbl_FORMAT_DATE),
                    Certificate = a.Course_Result_Final?.FirstOrDefault(b => b.IsDeleted == false && b.traineeid == entity.Id)?.SRNO,

                });
                var trainingCompetecy = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && a.Instructor_Ability.Any(x => x.InstructorId == entity.Id));
                model.TrainingCompetencies = trainingCompetecy.Select(a => new EmployeeModelPrint.TrainingCompetency()
                {
                    Code = a.Code,
                    Name = a.Name
                });
                var data = CourseDetailService.Get(a => a.Course_Detail_Instructor.Any(x => x.Instructor_Id == id) && a.Course.IsActive == true && a.Course.IsDeleted == false).Select(a => a.SubjectDetailId);
                var dtsubject = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && data.Contains(a.Id));
                model.ConductedCourses = dtsubject.Select(a => new EmployeeModelPrint.ConductedCourse()
                {
                    Code = a.Code,
                    Name = a.Name
                });

                return PartialView("EmployeeProfilePrint", model);
            }
            return PartialView("EmployeeProfilePrint");
        }
        #endregion
        #region [------Ham private--------]
        private string LoadDepartment(int? id = null)
        {
            var result = string.Empty;
            var data = DepartmentService.Get(a => CurrentUser.PermissionIds.Any(x => x == a.Id)).Select(x => new { x.Id, x.Ancestor, x.Name, x.Code });
            var lvl = 1;
            foreach (var item in data)
            {
                lvl = item.Ancestor.Count(x => x.Equals('!'));
                var khoangtrang = "";
                for (var i = 0; i < lvl; i++)
                {
                    khoangtrang += "&nbsp;&nbsp;&nbsp;";
                }
                result += "<option value='" + item.Id + "' style='font-size:" + (18 - (lvl + 2)) + "px;'" + (id == item.Id ? "Selected" : "") + ">" + khoangtrang + "+ " + item.Code + " - " + item.Name;
                result += "</option>";
            }
            return result;
        }
        public DateTime? ResturnExpiredate(Course_Detail CourseDetails, int? memberid, DateTime? prevExdate)//
        {

            DateTime? returnVal = null;
            if (CourseDetails == null) return null;
            var subjectCode = CourseDetails.SubjectDetail.Code;
            var enumerable = CourseService.GetCourseResult(a => a.TraineeId == memberid && a.Course_Detail.SubjectDetail.Code == subjectCode
                                                   && a.Course_Detail.dtm_time_from <= CourseDetails.dtm_time_from
                                                   && (a.Re_Check_Result == null ? (a.First_Check_Result != "F" && a.First_Check_Result != null) : a.Re_Check_Result != "F") && a.Course_Detail.TMS_APPROVES.OrderByDescending(x => x.id).FirstOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult).int_id_status == (int)UtilConstants.EStatus.Approve
                                                    );
            if (!enumerable.Any()) return null;
            if (enumerable.Count() > 1)
            {
                //lấy 2 phần tử cuối
                var courseResults = enumerable.OrderByDescending(a => a.Course_Detail.dtm_time_from).Take(2);
                if(courseResults.Any())
                {
                    var lastOrDefault = courseResults.AsEnumerable().LastOrDefault();
                    if (lastOrDefault?.Course_Detail?.dtm_time_to != null)
                    {
                        var fromdateLast = returnDateExpireTepm((DateTime)lastOrDefault?.Course_Detail?.dtm_time_to, (int)lastOrDefault?.Course_Detail?.SubjectDetail?.RefreshCycle);
                        if (prevExdate == null)
                        {
                            prevExdate = fromdateLast;
                        }
                    }
                    var firstOrDefault = courseResults?.FirstOrDefault();
                    if (firstOrDefault?.Course_Detail?.dtm_time_from == null) return returnVal;
                    //var fromdateFirst = firstOrDefault.Course_Detail.dtm_time_from;
                    var fromdateFirst = (DateTime)firstOrDefault?.Course_Detail?.dtm_time_to;
                    var expiredate = prevExdate;
                    var expiredate3Month = expiredate?.AddMonths(-3);
                    returnVal = returnDateExpireTepm(fromdateFirst, (int)firstOrDefault?.Course_Detail?.SubjectDetail?.RefreshCycle);

                    if (expiredate3Month < fromdateFirst && fromdateFirst <= expiredate)
                    {
                        returnVal = expiredate?.AddMonths((int)firstOrDefault?.Course_Detail?.SubjectDetail?.RefreshCycle);
                    }
                }
                //-- debug
                //// Kq lần đầu
               
            }
            else
            {
                var courseResult = enumerable.FirstOrDefault();
                if (courseResult?.Course_Detail?.dtm_time_to != null)
                    returnVal = returnDateExpireTepm((DateTime)courseResult?.Course_Detail?.dtm_time_to, (int)courseResult.Course_Detail.SubjectDetail.RefreshCycle);
            }
            return returnVal;
        }
        private DateTime? returnDateExpireTepm(DateTime fromdate, int cycle)
        {
            if (cycle == 0) return null;
            return (new DateTime(fromdate.Year, fromdate.Month, 1).AddMonths(1).AddDays(-1)).AddMonths(cycle);
        }
        #endregion
        #region [----------------Export bảng điểm-------------------------]
        [AllowAnonymous]
        public FileResult ExportTraineeTranscript(int id)
        {
            if (id == -1)
            {
                return null;
            }
            var filecontent = ExportExcelTraineeTranscript(id);
            return File(filecontent, ExportUtils.ExcelContentType, "TraineeTranscript.xlsx");

        }
        //TODO: process result
        private byte[] ExportExcelTraineeTranscript(int id)
        {
            var entity = EmployeeService.GetById(id);
            var model = new EmployeeModelPoint();

            #region [Hannah Mentor]
            var keyHannahMentor = GetByKey("MENTOR");
            model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

            if (model.CheckHannahMentor)
            {
                var hannahMentor = string.Empty;
                if (entity.Trainee_Type.Any())
                {
                    foreach (var item in entity.Trainee_Type)
                    {
                        hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                    }
                }
                model.HannahMentor = hannahMentor;
            }
            #endregion

            model.Id = entity.Id;
            model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
            model.Email = entity.str_Email;
            model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
            model.Department = entity.Department.Code + " " + entity.Department.Name;
            model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
            model.Jobtitle = entity.JobTitle?.Name;
            model.Nation = _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
            model.Phone = entity.str_Cell_Phone;
            var datamenberapproval = CourseMemberService.Get(a => a.Member_Id == id && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));
            var datacourse = CourseDetailService.Get(a => datamenberapproval.Any(x => x.Course_Details_Id == a.Id), new[] { (int)UtilConstants.ApproveType.SubjectResult }).Select(x => x.CourseId).Distinct();
            var dataCourse_Detail_Id = CourseDetailService.Get(a => datacourse.Contains(a.CourseId)).Select(a => a.Id);
            var data = CourseMemberService.Get(a => dataCourse_Detail_Id.Contains((int)a.Course_Details_Id) && a.Member_Id == id && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).Where(a =>
                            a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve));

            IEnumerable<TMS_Course_Member> filtered = data;
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            Func<TMS_Course_Member, object> orderingFunction = (c
                                                      => sortColumnIndex == 1 ? c?.Course_Detail?.SubjectDetail?.Name
                                                        : sortColumnIndex == 2 ? c?.Course_Detail?.dtm_time_from
                                                        : sortColumnIndex == 6 ? (object)c?.Course_Detail?.SubjectDetail?.RefreshCycle
                                                        : c?.Course_Detail?.SubjectDetail?.Name);
            var sortDirection = Request["sSortDir_0"]; // asc or desc

            if (sortDirection == null)
            {
                sortDirection = "desc";
            }
            filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                               : filtered.OrderByDescending(orderingFunction);
            var resultA = from c in filtered.ToArray()
                          select new ProfileSubjectModel1
                          {
                              bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                              SubjectCode = c?.Course_Detail?.SubjectDetail?.Code,
                              dtm_from = c?.Course_Detail?.dtm_time_from,
                              dtm_from_to = c?.Course_Detail?.dtm_time_from.Value.ToString(Resource.lbl_FORMAT_DATE) ?? "" + " - " + c?.Course_Detail?.dtm_time_to.Value.ToString(Resource.lbl_FORMAT_DATE) ?? "",
                              subjectName = c?.Course_Detail?.SubjectDetail?.Name,

                              point = string.Format("{0:0.#}", GetPointRemark(UtilConstants.DetailResult.Score, c?.Member_Id, c?.Course_Details_Id)),
                              remark = GetPointRemark(UtilConstants.DetailResult.Remark, c?.Member_Id, c?.Course_Details_Id),
                              grade = c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.point != null ? ReturnResult(c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Course_Detail?.SubjectDetail?.Subject_Score, c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.point, c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Result) : c?.Course_Detail?.Course_Result_Summary?.FirstOrDefault()?.Result,
                              recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                              memberId = c?.Member_Id,
                          };
            model.trainee_point = resultA.OrderBy(a => a.subjectName).ThenByDescending(a => a.dtm_from);
            ExcelPackage excelPackage;
            string templateFilePath = Server.MapPath(@"~/Template/ExcelFile/TraineeTranscript.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            using (excelPackage = new ExcelPackage(template, false))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();

                int startRow = 7;
                //var Row = 0;
                //string[] header = GetByKey("Report_SubjectResult").Split(',');
                //ExcelRange cellHeader = worksheet.Cells[3, 7];
                //cellHeader.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];

                //cellHeader.Style.Font.Size = 11;

                ExcelRange cellHeaderCourseName = worksheet.Cells[startRow, 3];
                cellHeaderCourseName.Value = model.FullName;

                ExcelRange cellHeaderCourseCode = worksheet.Cells[startRow, 6];
                cellHeaderCourseCode.Value = model.DateOfBirth;

                ExcelRange cellHeaderSubjectName = worksheet.Cells[startRow + 1, 3];
                cellHeaderSubjectName.Value = model.Phone;

                ExcelRange cellHeaderDuration = worksheet.Cells[startRow + 1, 6];
                cellHeaderDuration.Value = model.Email;

                ExcelRange cellHeaderVenue = worksheet.Cells[startRow + 2, 3];
                cellHeaderVenue.Value = model.Department;

                ExcelRange cellHeaderDate = worksheet.Cells[startRow + 2, 6];
                cellHeaderDate.Value = model.Jobtitle;

                startRow = 12;
                //int row = 1;
                int count = 0;

                foreach (var item in model.trainee_point)
                {
                    count++;
                    int col = 2;
                    ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                    cellNo.Value = count;
                    cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellSubjectCode = worksheet.Cells[startRow + 1, ++col];
                    cellSubjectCode.Value = item.SubjectCode;
                    cellSubjectCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellSubjectCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellSubjectCode.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellSubjectName = worksheet.Cells[startRow + 1, ++col];
                    cellSubjectName.Value = item.subjectName;
                    cellSubjectName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellSubjectName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellSubjectName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellTime = worksheet.Cells[startRow + 1, ++col];
                    cellTime.Value = item.dtm_from_to;
                    cellTime.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellTime.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellTime.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellPoint = worksheet.Cells[startRow + 1, ++col];
                    cellPoint.Value = item.point;
                    cellPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellPoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellGrace = worksheet.Cells[startRow + 1, ++col];
                    cellGrace.Value = item.grade;
                    cellGrace.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellGrace.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellGrace.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    startRow++;
                }
                ExcelRange cell1 = worksheet.Cells[startRow + 2, 3];
                cell1.Style.Font.Size = 12;
                cell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell1.Value = "Request by";

                ExcelRange cell2 = worksheet.Cells[startRow + 3, 3];
                cell2.Style.Font.Bold = true;
                cell2.Style.Font.Size = 12;
                cell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell2.Value = "TRAINEE";

                ExcelRange cell3 = worksheet.Cells[startRow + 4, 3];
                cell3.Style.Font.Size = 12;
                cell3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell3.Value = "Date .....................................";


                ExcelRange cell4 = worksheet.Cells[startRow + 2, 6];
                cell4.Style.Font.Size = 12;
                cell4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell4.Value = "Approved by";

                ExcelRange cell5 = worksheet.Cells[startRow + 3, 6];
                cell5.Style.Font.Bold = true;
                cell5.Style.Font.Size = 12;
                cell5.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell5.Value = "HEAD OF TRAINING";

                ExcelRange cell6 = worksheet.Cells[startRow + 4, 6];
                cell6.Style.Font.Size = 12;
                cell6.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell6.Value = "Date .....................................";
                bytes = excelPackage.GetAsByteArray();
            }
            return bytes;
        }
        #endregion
        [HttpPost]
        public ActionResult FilterSubject(FormCollection form)
        {
            var filterCodeOrName = string.IsNullOrEmpty(form["FilterCodeOrName"])
                ? string.Empty
                : form["FilterCodeOrName"].Trim().ToLower();
            var subjectId = !string.IsNullOrEmpty(form["Subjects[]"]) ? form["Subjects[]"].Split(new char[] { ',' }) : null;

            var html = new StringBuilder();
            var data = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && (string.IsNullOrEmpty(filterCodeOrName) || a.Code.Trim().ToLower().Contains(filterCodeOrName) || a.Name.Trim().ToLower().Contains(filterCodeOrName)));

            if (!data.Any())
            {
                return Json(new
                {
                    value = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in data.OrderBy(a => a.Code))
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
            return Json(new
            {
                value = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult AjaxHandlerListSelectSubject(string id, FormCollection form)
        {
            var filterCodeOrName = string.IsNullOrEmpty(form["form[FilterCodeOrName]"])
                ? string.Empty
                : form["form[FilterCodeOrName]"].Trim().ToLower();
            var checkselectvalue_ = !String.IsNullOrEmpty(form["form[int_khoidaotao][]"]) ? form["form[int_khoidaotao][]"].Split(new char[] { ',' }).Select(Int32.Parse).ToArray() : null;
            StringBuilder HTML = new StringBuilder();

            HTML.Append("<ul>");
            HTML.Append("<li><input class='assignedParentFunc-1' value='-1' multiple type='checkbox' id='checkAll'/><span>&nbspOther</span>");

            int traineeId = !string.IsNullOrEmpty(id) ? int.Parse(id) : -1;
            if(checkselectvalue_ != null)
            {
                foreach (var item_ in checkselectvalue_)
                {
                    var deparment = DepartmentService.GetById(item_);
                    if (deparment != null)
                    {
                        HTML.Append("<ul>");
                        HTML.Append("<li><input class='assignedParentFuncAll assignFuncAll' value='-1' multiple type='checkbox' id='checkAll_" + deparment.Id + "' data-index='" + deparment.Id + "'/><span>&nbsp" + deparment.Code + "</span>");
                        HTML.Append("<ul>");
                        var datakhoidaotao = _repoSubjectService.GetTrainingCenter(a => a.khoidaotao_id == deparment.Id).Select(a => a.subject_id);
                        var data = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && datakhoidaotao.Contains(a.Id) && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General && (string.IsNullOrEmpty(filterCodeOrName) || a.Code.Trim().ToLower().Contains(filterCodeOrName) || a.Name.Trim().ToLower().Contains(filterCodeOrName)));
                        foreach (var item in data.OrderBy(a => a.Name))
                        {
                            HTML.Append("<li>");
                            var listsubject = EmployeeService.GetInstruc_Ability(a => a.InstructorId == traineeId && a.SubjectDetailId == item.Id);
                            if (listsubject.Count() > 0)
                            {
                                HTML.AppendFormat(" <input data-id='{0}' data-parentname='Other' data-name='{2}' multiple value='{0}' class='InstructorAbility_" + deparment.Id + " assignFunc' name='InstructorAbility' id='InstructorAbility_" + item.Id + "' type='checkbox' Checked /><input type='hidden' value='{1}' name='InstructorAbility2' /><label for='InstructorAbility_" + item.Id + "'>&nbsp{2}</label>", item.Id, item.Id, item?.IsActive != true ? "<span style='color:" + UtilConstants.String_DeActive_Color + ";'>" + item.Name + " - " + UtilConstants.CourseTypesDictionary()[(item.CourseTypeId ?? 1)] + "</span>" : item.Name + " - " + UtilConstants.CourseTypesDictionary()[(item.CourseTypeId ?? 1)]);
                            }
                            else
                            {
                                HTML.AppendFormat(" <input data-id='{0}' data-parentname='Other' data-name='{2}' multiple value='{0}' class='InstructorAbility_" + deparment.Id + " assignFunc' name='InstructorAbility' id='InstructorAbility_" + item.Id + "' type='checkbox' /><input type='hidden' value='{1}' name='InstructorAbility2' /><label for='InstructorAbility_" + item.Id + "'>&nbsp{2}</label>", item.Id, item.Id, item?.IsActive != true ? "<span style='color:" + UtilConstants.String_DeActive_Color + ";'>" + item.Name + " - " + UtilConstants.CourseTypesDictionary()[(item.CourseTypeId ?? 1)] + "</span>" : item.Name + " - " + UtilConstants.CourseTypesDictionary()[(item.CourseTypeId ?? 1)]);
                            }
                            HTML.Append("</li>");
                        }
                        HTML.Append("</ul>");
                        HTML.Append("</li>");
                        HTML.Append("</ul>");
                    }
                }
            }
            

            HTML.Append("</li>");
            HTML.Append("</ul>");
            return Json(HTML.ToString());
        }
        [AllowAnonymous]
        public ActionResult CheckFile()
        {
            try
            {
                List<ImportFile> list = new List<ImportFile>();
                HttpFileCollectionBase files = Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];
                    var fname = file.FileName;
                    var fnameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                    var checkid = _repoEmployeeService.GetAll().Where(p => p.str_Staff_Id == fnameWithoutExtension && p.IsDeleted == false && p.IsActive == true).FirstOrDefault();
                    if (checkid != null)
                    {
                        var importfile = new ImportFile();
                        importfile.filename = fname + " - " + checkid.LastName + " " + checkid.FirstName;
                        importfile.status = "<p class='text-succes'>pass</p>";
                        list.Add(importfile);
                        var newAvatar = SaveImage(file, checkid.str_Staff_Id, UtilConstants.Upload.Trainee);
                        if (newAvatar.result)
                        {
                            checkid.avatar = newAvatar.data.ToString();
                            _repoEmployeeService.Update(checkid);
                        }

                    }
                    else
                    {
                        var importfile = new ImportFile();
                        importfile.filename = fname;
                        importfile.status = "<p class='text-danger'>Fail</p>";
                        list.Add(importfile);
                    }

                }
                return Json(new
                {
                    result = true,
                    data = JsonConvert.SerializeObject(list),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public class ImportFile
        {
            public string filename { get; set; }
            public string status { get; set; }
        }
        //LMS call
        #region LMS call
        [AllowAnonymous]
        public ActionResult PluginProFile(int? id, int? type)
        {
            if (
                 (type == (int)UtilConstants.ROLE.Trainee ||
                  type == (int)UtilConstants.ROLE.Instructor
                     ))
            {
                var entity = EmployeeService.GetById(id);
                if (entity != null)
                {
                    var model = new EmployeeModelDetails();
                    //if (type != entity.int_Role)
                    //{
                    //    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                    //    return RedirectToAction("Index", "Home");
                    //}

                    #region [Hannah Mentor]
                    var keyHannahMentor = GetByKey("MENTOR");
                    model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

                    if (model.CheckHannahMentor)
                    {
                        var hannahMentor = string.Empty;
                        if (entity.Trainee_Type.Any())
                        {
                            foreach (var item in entity.Trainee_Type)
                            {
                                hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                            }
                        }
                        model.HannahMentor = hannahMentor;

                    }
                    #endregion


                    model.Control = type.Value;
                    model.Id = entity.Id;
                    model.Avatar = entity.avatar;
                    model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                    model.Eid = entity.str_Staff_Id;
                    model.PersonId = entity.PersonalId;
                    model.Passport = entity.Passport;
                    model.Email = entity.str_Email;
                    model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
                    model.Type = entity.bit_Internal == true
                        ? UtilConstants.CourseAreas.Internal.ToString()
                        : UtilConstants.CourseAreas.External.ToString();
                    model.PlaceOfBirth = entity.str_Place_Of_Birth;
                    model.Department = entity.Department?.Code + " " + entity.Department?.Name;
                    model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
                    model.Jobtitle = entity.JobTitle?.Name;
                    model.Nation = _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
                    model.Company = entity.Company?.str_Name;
                    model.Phone = entity.str_Cell_Phone;
                    model.DateOfJoin = entity.dtm_Join_Date?.ToString(Resource.lbl_FORMAT_DATE);
                    model.ResignationDate = entity.non_working_day?.ToString(Resource.lbl_FORMAT_DATE);
                    model.Nation = entity.Nation;
                    return View(model);
                }

            }
            TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
            return RedirectToAction("Index", "Home");
        }
        #endregion
        public string returnpointgrade(int? type, int? Trainee_Id, int? Course_Details_Id)
        {
            string _return = "";
            var data = CourseService.GetCourseResult(a => a.TraineeId == Trainee_Id && a.CourseDetailId == Course_Details_Id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            if (data != null)
            {
                if (type == 1)
                {
                    #region [get điểm]
                    if (data.Re_Check_Score != null)
                    {
                        _return = data.Re_Check_Score.ToString();
                    }
                    else
                    {
                        if (data.First_Check_Score != null)
                        {
                            _return = data.First_Check_Score.ToString();
                        }
                    }
                    #endregion
                }
                else
                {
                    if (data.Re_Check_Result != null)
                    {
                        if (data.Re_Check_Result == null && data.First_Check_Result == null)
                            _return = "Fail";
                        if (data.Re_Check_Result == "P")
                        {
                            _return = "Pass";
                            var check_distintion = _repoSubjectService.GetScores(a => a.subject_id == data.Subject_Id).OrderByDescending(a => a.point_from);
                            foreach (var item in check_distintion)//.OrderBy(a => a.point_from)
                            {
                                if (data.Re_Check_Score >= item.point_from)
                                {
                                    _return = item.grade;
                                    if (data.First_Check_Result != "F")
                                    {
                                        break;
                                    }

                                }
                            }
                        }
                        else
                        {
                            _return = "Fail";
                        }
                    }
                    else
                    {
                        if (data.First_Check_Result != null)
                        {
                            if (data.First_Check_Result == "P")
                            {
                                _return = "Pass";
                                var check_distintion = _repoSubjectService.GetScores(a => a.subject_id == data.Subject_Id);
                                foreach (var item in check_distintion.OrderBy(a => a.point_from))
                                {
                                    if (data.First_Check_Score >= item.point_from)
                                    {
                                        _return = item.grade;
                                    }
                                }
                            }
                            else
                            {
                                _return = "Fail";
                            }
                        }
                        else
                        {
                            _return = "Fail";
                        }
                    }
                }
            }

            return _return;
        }
        [AllowAnonymous]
        public ActionResult Print(int? id)
        {
            var model = new EmployeeModelPrint();

            var entity = EmployeeService.GetById(id);
            if (entity == null) return PartialView("Print");
            #region [Hannah Mentor]
            var keyHannahMentor = GetByKey("MENTOR");
            model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

            if (model.CheckHannahMentor)
            {
                var hannahMentor = string.Empty;
                if (entity.Trainee_Type.Any())
                {
                    foreach (var item in entity.Trainee_Type)
                    {
                        hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                    }
                }
                model.HannahMentor = hannahMentor;

            }
            #endregion
            model.Id = entity.Id;
            model.Avatar = entity.avatar;
            model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
            model.Eid = entity.str_Staff_Id;
            model.PersonalId = entity.PersonalId;
            model.Passport = entity.Passport;
            model.Email = entity.str_Email;
            model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
            model.Type = entity.bit_Internal == true
                ? UtilConstants.CourseAreas.Internal.ToString()
                : UtilConstants.CourseAreas.External.ToString();
            model.PlaceOfBirth = entity.str_Place_Of_Birth;
            model.Department = entity.Department?.Name;
            model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
            model.JobTitle = entity.JobTitle?.Name;
            model.Nation =
                _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
            model.Company = entity.Company?.str_Name;
            model.Phone = entity.str_Cell_Phone;
            model.DateOfJoin = entity.dtm_Join_Date?.ToString(Resource.lbl_FORMAT_DATE);
            model.ResignationDate = entity.non_working_day?.ToString(Resource.lbl_FORMAT_DATE);
            model.Educations = entity.Trainee_Record?.OrderByDescending(a => a.Trainee_Record_Id).Select(a => new EmployeeModelPrint.Education()
            {
                Time = a.dtm_time_from?.ToString(Resource.lbl_FORMAT_DATE) + " - " + a.dtm_time_to?.ToString(Resource.lbl_FORMAT_DATE),
                Course = a.str_Subject,
                Organization = a.str_organization,
                Note = a.str_note

            });
            model.Contracts =
                entity.Trainee_Contract?.OrderByDescending(a => a.id).Select(a => new EmployeeModelPrint.Contract()
                {
                    ContractNo = a.contractno,
                    ExpiryDate = DateUtil.DateToString(a?.expire_date, Resource.lbl_FORMAT_DATE),
                    Description = a.description
                });
            var course = CourseDetailService.Get(
                a =>
                a.TMS_Course_Member.Any(x => x.Member_Id == entity.Id && x.IsActive == true &&(x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)) &&
                     a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)).ToList()
                .Where(a =>
                    a.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?
                        .int_id_status == (int)UtilConstants.EStatus.Approve).Select(x => x.Course).Distinct();

            // var datamenberapproval = entity.TMS_Course_Member?.Where(a => a.IsDelete == false && a.IsActive);
            model.TrainningCourses = course.Select(a => new EmployeeModelPrint.TrainningCourse()
            {
                Code = a.Code,
                Name = a.Name,
                Time = DateUtil.DateToString(a.StartDate, Resource.lbl_FORMAT_DATE) + " - " + DateUtil.DateToString(a.EndDate, Resource.lbl_FORMAT_DATE),
                Certificate = a.Course_Result_Final?.FirstOrDefault(b => b.IsDeleted == false && b.traineeid == entity.Id)?.SRNO,

            });
            var trainingCompetecy = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && a.Instructor_Ability.Any(x => x.InstructorId == entity.Id));
            model.TrainingCompetencies = trainingCompetecy.Select(a => new EmployeeModelPrint.TrainingCompetency()
            {
                Code = a.Code,
                Name = a.Name
            });
            var data = CourseDetailService.Get(a => a.Course_Detail_Instructor.Any(x => x.Instructor_Id == id) && a.Course.IsActive == true && a.Course.IsDeleted == false).Select(a => a.SubjectDetailId);
            var dtsubject = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && data.Contains(a.Id));
            model.ConductedCourses = dtsubject.Select(a => new EmployeeModelPrint.ConductedCourse()
            {
                Code = a.Code,
                Name = a.Name
            });
            var datacourse_ = CourseDetailService.Get(a => a.Course.IsDeleted != true && a.TMS_Course_Member.Any(x => x.Member_Id == id && x.IsActive == true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)) &&
                a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult));
            IEnumerable<int?> dataCourseDetailId = new List<int?>();
            var listtemp = new List<int?>();
            foreach (var item in datacourse_)
            {
                var item_ = item.TMS_APPROVES.LastOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                if (item_ != null)
                {
                    if (item_.int_id_status == (int)UtilConstants.EStatus.Approve)
                    {
                        listtemp.Add(item.Id);
                    }
                }
            }
            dataCourseDetailId = listtemp.Distinct();
            var data2 = CourseMemberService.Get(a => dataCourseDetailId.Contains((int)a.Course_Details_Id)  && a.Member_Id == id && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).AsEnumerable();
            var resultA = from c in data2
                          select new ProfileSubjectModel
                          {
                              dtm_from = c?.Course_Detail?.dtm_time_from,
                              dtm_from_to = DateUtil.DateToString(c?.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c?.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                              subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                              firstCheck = ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == id)),
                              reCheck = ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == id)),
                              remark = c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == id)?.Remark,
                              grade = returnpointgrade(2, c?.Member_Id, c?.Course_Details_Id),
                              recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                              courseDetails = c?.Course_Detail,
                              memberId = c?.Member_Id,
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
            var resultx = from c in resultA
                          select new ProfileSubjectModel
                          {
                              dtm_from_to = c.dtm_from_to,
                              subjectName = c.subjectName,
                              firstCheck = c.firstCheck,
                              reCheck = c.reCheck,
                              remark = c.remark,
                              grade = c.grade,
                              recurrent = c.recurrent,
                              ex_Date = c.ex_Date,
                              courseDetails = c.courseDetails
                          };
            model.TrainingCourseCustom = resultx;
            return PartialView("Print", model);
        }
        [AllowAnonymous]
        public ActionResult PartialDetailSimple()
        {
            //var model = EmployeeService.GetById(id);
            return PartialView("_partials/_PartialTraineeDetailSimple");
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult CreateEID(int valuetype = -1, int valuepartner = -1)//Create
        {
            #region[]
            string str_start = "TRN";
            var datapartner = _repoCompanyService.GetById(valuepartner);
            if (datapartner != null)
            {
                str_start = datapartner?.str_code;
            }
            #endregion
            string EID = "";
            if (valuetype == (int)UtilConstants.CourseAreas.External)
            {
                var data = EmployeeService.Get(a => a.str_Staff_Id.StartsWith(str_start),true);
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

                    var data_ = EmployeeService.Get(a => a.str_Staff_Id == EID,true);
                    if (data_.Count() == 0)
                    {
                        break;
                    }
                }
            }
            return Json(EID);
        }
    }
}