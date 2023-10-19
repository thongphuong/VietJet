using Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.Mail;
using TMS.Core.Utils;
using TMS.Core.ViewModels.AjaxModels;
using TMS.Core.ViewModels.Schedule;
using Utilities;

namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.Services.Nationality;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Nation;
    using System.Globalization;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.Services;
    using System.Text;
    using TMS.Core.Services.GroupUser;
    public class ScheduleController : BaseAdminController
    {
        #region Variables
        private readonly IJobtitleService _repoJobTiltle;
        private readonly IMailService _repoMailService;
        private readonly ICourseDetailService _repoCourseServiceDetail;
        private readonly IGroupUserService _groupUserService;
        #endregion

        public ScheduleController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService, IJobtitleService repoJobTiltle, IMailService repoMailService, ICourseDetailService repoCourseDetail, IGroupUserService groupUserService)
            : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoJobTiltle = repoJobTiltle;
            _repoMailService = repoMailService;
            _repoCourseServiceDetail = repoCourseDetail;
            _groupUserService = groupUserService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var name = string.IsNullOrEmpty(Request.QueryString["Name"]) ? string.Empty : Request.QueryString["Name"].Trim().ToLower();

                var data = ConfigService.GetAllSchedule(a =>
                    (string.IsNullOrEmpty(name) || a.Name.ToLower().Contains(name)));
                IEnumerable<Schedule> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Schedule, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.Name
                                                          : sortColumnIndex == 2 ? c?.Content
                                                          : sortColumnIndex == 3 ? (object)c?.IsActive
                                                          : c.Name);
                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                                       : filtered.OrderByDescending(orderingFunction);

                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                         "<a href='" + @Url.Action("Modify",new {id = c.id}) + "'>"+c.Name+"</a>",
                         c.Content,
                         c.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Schedule(0,"+c.id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Schedule(1,"+c.id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",
                                 ReturnStatusSchedule(c),
                       c.IsDefault.HasValue ? "" : "<a title='Edit' href='"+@Url.Action("Modify",new{id = c.id})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" 
                       +
                        ((User.IsInRole("/Schedule/Delete"))  ? verticalBar + "<a title='Delete' href='javascript:void(0);' onclick='calldelete(" + c.id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Schedule/AjaxHandler", ex.Message);
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
        public ActionResult AjaxHandlerListMail(jQueryDataTableParamModel param, string id = null)
        {
            try
            {
                var myInts = id?.Split(new char[] { ',' }).Select(int.Parse).ToArray();
                var data = ConfigService.GetMemberMail(a => myInts.Contains(a.Id));
                IEnumerable<TMS_SentEmail> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_SentEmail, object> orderingFunction = (c => sortColumnIndex == 1 ? c?.mail_receiver
                                                          : c.mail_receiver);
                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                                       : filtered.OrderByDescending(orderingFunction);

                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                                c.mail_receiver,
                                c.content_body,
                                 "<a title='Re-Send' href='javascript:void(0);' onclick='ReSendMail(" + c.Id  + ")' data-toggle='tooltip'><i class='fa fa-paper-plane btnIcon_blue font-byhoa' aria-hidden='true' ></i></a>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Schedule/AjaxHandlerListMail", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        private static string ReturnStatusSchedule(Schedule schedule)
        {
            //var return_ = "";
            //var das = schedule.CAT_MAIL?.TMS_SentEmail?.Count(a => a.flag_sent == false);
            //if (das != 0)
            //{
            //    return_ = das.ToString();
            //}
            //return return_;

            var das = schedule.IdTemplateMail != null ?
                schedule.CAT_MAIL?.TMS_SentEmail?.Where(a => a.flag_sent == 0).Select(a => a.Id) :
                schedule.TMS_SentEmail?.Where(a => a.flag_sent == 0).Select(a => a.Id);

            var return_ = das.Any() ? "<span data-value='" + string.Join(",", das) + "' class='expand label label-danger' style='cursor: pointer;font-size: 13px;'>Have " + das.Count() + " mail not sent</span>" : "<span class='label label-success'>Done</span>";
            return return_;
        }

        public ActionResult AjaxHandlerAvailable(jQueryDataTableParamModel param)
        {
            try
            {
                var sss = "";
                var departmentId = string.IsNullOrEmpty(Request.QueryString["DepartmentList"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList"].Trim());
                var fullName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].Trim().ToLower();
                var groupTraineeId = string.IsNullOrEmpty(Request.QueryString["GroupTrainee"]) ? -1 : Convert.ToInt32(Request.QueryString["GroupTrainee"].Trim());
                var prerequisiteId = string.IsNullOrEmpty(Request.QueryString["Prerequisite"]) ? -1 : Convert.ToInt32(Request.QueryString["Prerequisite"].Trim());
                var userTypeId = string.IsNullOrEmpty(Request.QueryString["UserTypeId"]) ? -1 : Convert.ToInt32(Request.QueryString["UserTypeId"].Trim());
                var user = string.IsNullOrEmpty(Request.QueryString["NUserId[]"]) ? new List<int>() : Request.QueryString["NUserId[]"].Split(new char[] { ',' }).Select(int.Parse).ToList();
                var emp = string.IsNullOrEmpty(Request.QueryString["NEmployeeId[]"]) ? new List<int>() : Request.QueryString["NEmployeeId[]"].Split(new char[] { ',' }).Select(int.Parse).ToList();

                // Params of User     
                string userName = string.IsNullOrEmpty(Request.QueryString["userName"]) ? string.Empty : Request.QueryString["userName"].Trim().ToLower();
                string firstName = string.IsNullOrEmpty(Request.QueryString["firstName"]) ? string.Empty : Request.QueryString["firstName"].Trim().ToLower();
                string lastName = string.IsNullOrEmpty(Request.QueryString["lastName"]) ? string.Empty : Request.QueryString["lastName"].Trim().ToLower();
                string emailUser = string.IsNullOrEmpty(Request.QueryString["emailUser"]) ? string.Empty : Request.QueryString["emailUser"].Trim().ToLower();
                string phoneUser = string.IsNullOrEmpty(Request.QueryString["phoneUser"]) ? string.Empty : Request.QueryString["phoneUser"].Trim();
                var groupUserList = Request.QueryString["GroupUsersID"].Trim();
                List<int> groupUserListId = new List<int>();
                if (!string.IsNullOrEmpty(groupUserList))
                {
                    groupUserListId = groupUserList.Split(new char[] { ',' }).Select(Int32.Parse).ToList();
                }


                //Params of Instructor and Trainee
                List<int> subjectListId = new List<int>();
                List<int> GroupTraineeListId = new List<int>();
                int courseId = string.IsNullOrEmpty(Request.QueryString["CourseListID"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseListID"].Trim());
                if (!string.IsNullOrEmpty(Request.QueryString["ddl_subject"]))
                {
                    string subjectList = Request.QueryString["ddl_subject"].Trim();
                    subjectListId = subjectList.Split(new char[] { ',' }).Select(Int32.Parse).ToList();
                }
                if (!string.IsNullOrEmpty(Request.QueryString["GroupTraineesID"]))
                {
                    string GroupTraineeList = Request.QueryString["GroupTraineesID"].Trim();
                    GroupTraineeListId = GroupTraineeList.Split(new char[] { ',' }).Select(Int32.Parse).ToList();
                }
                var jobtitleId = string.IsNullOrEmpty(Request.QueryString["JobtitleList"]) ? -1 : Convert.ToInt32(Request.QueryString["JobtitleList"].Trim());
                var traineeCode = string.IsNullOrEmpty(Request.QueryString["EID"]) ? string.Empty : Request.QueryString["EID"].Trim().ToLower();


                const int pass = (int)UtilConstants.Grade.Pass;
                const int distinction = (int)UtilConstants.Grade.Distinction;

                List<AjaxUserSchedule> data = new List<AjaxUserSchedule>();

                if (userTypeId == (int)UtilConstants.UserType.UserSystem) //Trường hợp UserSystem
                {
                    data =
                       UserContext.GetByUserName_FirstName_LastName_Email_Phone_GroupUser(userName, firstName, lastName, emailUser, phoneUser, groupUserListId).Where(x => !user.Contains(x.ID) && !emp.Contains(x.ID)).ToArray().Select(a => new AjaxUserSchedule()
                       {
                           Id = a.ID,
                           FullName = ReturnDisplayLanguage(a.FIRSTNAME, a.LASTNAME),
                           Department =
                                      string.Join(",", a.UserPermissions?.Select(b => b.Department.Name))
                       }).Distinct().ToList();
                }
                else // Trường hợp Employee
                {
                    List<Trainee> listTraineeTotal = EmployeeService.Get(a => a.IsDeleted == false && a.IsActive == true && userTypeId != -1,true).Where(x => !user.Contains(x.Id) && !emp.Contains(x.Id)).ToList();

                    if (courseId != -1 && subjectListId.Count == 0) // Chỉ Có Field search Course
                    {
                        // Lấy danh sách Course Detail theo Course ID
                        List<int> courseDetailsID = _repoCourseServiceDetail.GetByCourse(courseId).Where(a => a.IsActive == true).Select(b => b.Id).ToList();
                        // Lấy danh sách học viên đã được Approve vào khóa học ở trên
                        var memberID = CourseMemberService.GetApi(a => a.IsDelete != true && a.IsActive == true && courseDetailsID.Contains((int)a.Course_Details_Id)).Select(c => c.Member_Id);
                        listTraineeTotal = listTraineeTotal.Where(a => memberID.Contains((int)a.Id)).ToList();
                    }
                    if (courseId != -1 && subjectListId.Count > 0) // Có Field search Course và Có Filed Search Subject
                    {
                        // Lấy danh sách Course Detail theo Course ID
                        List<Course_Detail> courseDetailsID = _repoCourseServiceDetail.GetByCourse(courseId).Where(a => a.IsActive == true).ToList();
                        // Lọc lai danh sach course Detail theo Subject
                        List<int> courseDetailsID_New = courseDetailsID.Where(a => subjectListId.Contains((int)a.SubjectDetailId)).Select(b => b.Id).ToList();
                        // Lấy danh sách học viên đã được Approve vào khóa học ở trên
                        var memberID = CourseMemberService.GetApi(a => a.IsDelete != true && a.IsActive == true && courseDetailsID_New.Contains((int)a.Course_Details_Id)).Select(c => c.Member_Id);
                        listTraineeTotal = listTraineeTotal.Where(a => memberID.Contains((int)a.Id)).ToList();
                    }
                    if (jobtitleId != -1) // Có Field Search Jobtitle
                    {
                        listTraineeTotal = listTraineeTotal.Where(a => a.Job_Title_id == jobtitleId).ToList();
                    }
                    if (!string.IsNullOrEmpty(traineeCode)) // Có Field Search EID
                    {
                        listTraineeTotal = listTraineeTotal.Where(a => a.str_Staff_Id.ToLower().Trim() == traineeCode).ToList();
                    }
                    if (GroupTraineeListId.Count > 0) // Trường hợp Field Search có Group Trainee
                    {
                        List<Trainee> listTraineeByGroup = EmployeeService.GetEmp(a => a.IsActive == true && a.GroupTrainee_Item.Any(b => b.GroupTraineeId == a.Id)).ToList();
                        listTraineeTotal.AddRange(listTraineeByGroup);
                    }
                    // Tổng kết lại list data
                    data = listTraineeTotal.Select(a => new AjaxUserSchedule()
                    {
                        Id = a.Id,
                        FullName = ReturnDisplayLanguage(a?.FirstName, a?.LastName),
                        Department = a.Department?.Name ??""
                    }).Distinct().ToList();
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxUserSchedule, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.FullName
                                                            : sortColumnIndex == 2 ? c?.Department
                                                          : c.FullName);

                var filtered = (Request["sSortDir_0"] == "asc")
                   ? data.OrderBy(orderingFunction)
                   : data.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                   c?.FullName,
                                   c?.Department,
                                   "<input type='checkbox' name='id2[]' class='frmFilter' value='"+c?.Id+"'>"

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Schedule/AjaxHandlerAvailable", ex.Message);
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
        public ActionResult Modify(ScheduleModify model)
        {
            model.MethodId = 2; // Chi gui mail, khong su dung SMS
            if (!ModelState.IsValid)
                return Json(new AjaxResponseViewModel { result = false, message = MessageInvalidData(ModelState) });

            try
            {
                var schedule = ConfigService.GetAllSchedule(a=>a.Name==model.Name && a.id != model.Id);
                if (schedule.Any())
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.EXISTING_ReminderTitle
                    });
                }
                else
                {
                    var result = ConfigService.Modify(model);
                    if (result != null)
                    {
                        var ajaxResult = new AjaxResponseViewModel()
                        {
                            result = true,
                            message = Messege.SUCCESS
                        };
                        TempData[UtilConstants.NotifyMessageName] = ajaxResult;
                        return Json(ajaxResult, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.ISVALID_DATA
                    });
                }


            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Schedule/Modify", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = ex.Message
                });
            }


        }
        [HttpGet]
        public ActionResult Modify(int? id)
        {
            var schedule = ConfigService.GetScheduleById(id);

            var model = new ScheduleModify();
            model.Methods = UtilConstants.ScheduleMethodDictionary();
            model.Types = UtilConstants.ScheduleTypeDictionary();
            model.DayOfWeek = UtilConstants.DateOfWeekDictionary();
            model.UserTypes = UtilConstants.UserTypeDictionary();
            model.TimeMark = UtilConstants.ScheduleTimeMarkDictionary();
            //TODO: chỉ lấy những template xử dụng trong dhdn
            var arrCode = new string[] { "SendMailScheduleCourseMissing", "SendMailScheduleReportTrainingPlan", "SendMailReminderLogin", "SendMailReminderCourse", "SendMailReminderFinalCourse" };
            model.TemplateMails = ConfigService.GetMail(/*a => arrCode.Contains(a.Code)*/).ToDictionary(a => a.ID, a => a.Name);
            //model.Departments = GetDepartmentModel().OrderBy(a => a.Ancestor).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.DepartmentName));
            model.Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster);
            model.JobTitles = _repoJobTiltle.Get(a => a.IsDelete == false)
                .OrderBy(a => a.Name)
                .ToDictionary(a => a.Id, a => a.Name);
            model.CourseList = CourseService.Get(a => a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)), true)
                .OrderByDescending(a => a.Code)
                .ToDictionary(a => a.Id, a => a.Name);
            model.GroupTrainees =
                EmployeeService.GetAllGroupTrainees(a => a.IsActived == true)
                    .OrderBy(a => a.Name)
                    .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name));
            model.GroupUsers =
                 _groupUserService.Get()
                    .OrderBy(a => a.Name)
                    .ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Id, a.Name));

            //khoa hoan thanh
            var dataCourseResultProccess = ApproveService.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.CourseResult, (int)UtilConstants.EStatus.Approve);
            model.Prerequisite =
                CourseService.Get(a => dataCourseResultProccess.Any(b => b.Course == a))
                    .OrderBy(a => a.Id)
                    .ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name));
            if (schedule != null)
            {
                model.Id = schedule.id;
                model.Name = schedule.Name;
                model.Content = schedule.Content;
                model.TeamplateContent = schedule.CAT_MAIL != null ? Regex.Replace(schedule.CAT_MAIL.Content, "<.*?>", string.Empty) : "";
                model.TemplateId = schedule.IdTemplateMail;
                var schedulesMethod = schedule.Schedules_Method.FirstOrDefault();
                var schedulesDestination = schedule.Schedules_destination?.FirstOrDefault();
                var schedulesType = schedule.Schedules_Type.FirstOrDefault();

                model.MethodId = schedulesMethod?.IdMethod ?? -1;
                model.TypeId = schedule.Schedules_Type?.FirstOrDefault()?.IdType ?? -1;
                model.TimeRemind = schedulesType.TimeRemind != null ? (int)schedulesType?.TimeRemind : 0;
                model.Catmail_code = schedule.CAT_MAIL.Code;
                model.UserTypeId = schedulesDestination?.IsUser == true
                    ? (int)UtilConstants.UserType.UserSystem
                    : (int)UtilConstants.UserType.Employee;
                model.IsAll = schedulesDestination?.IsAll == true ? "on" : "";

                model.GetUsers = schedulesDestination?.IsUser == true
                    ? schedule.Schedules_destination?.Where(a => a.IdUser.HasValue).Select(a => new ScheduleModify.GetUser()
                    {
                        Id = a.IdUser,
                        FullName = ReturnDisplayLanguage(a.USER.FIRSTNAME, a.USER.LASTNAME),
                        Department = string.Join(",", a.USER?.UserPermissions?.Select(b => b.Department.Name))

                    })
                    : schedule.Schedules_destination.Where(a => a.IdEmp.HasValue).Select(a => new ScheduleModify.GetUser()
                    {
                        Id = a.IdEmp,
                        FullName = ReturnDisplayLanguage(a.Trainee.FirstName, a.Trainee.LastName),
                        Department = a.Trainee?.Department?.Name
                    });


                if (model.TypeId == (int)UtilConstants.ScheduleType.Repeat)
                {
                    if (schedulesType != null)
                    {
                        var value = 0;
                        var intValue = int.Parse(schedulesType?.Value);
                        switch (schedulesType?.IdTimeMark)
                        {
                            //case (int)UtilConstants.ScheduleTimeMark.Second:
                            //    value = intValue;
                            //    break;
                            case (int)UtilConstants.ScheduleTimeMark.Hour:
                                value = (intValue / 60 / 60);
                                break;
                            case (int)UtilConstants.ScheduleTimeMark.Day:
                                value = (intValue / 60 / 60 / 24);
                                break;
                            case (int)UtilConstants.ScheduleTimeMark.Month:
                                value = (intValue / 60 / 60 / 24 / 30);
                                break;
                        }
                        model.TimeRepeat = value;
                        model.TimeMarkId = schedulesType?.IdTimeMark;
                    }
                }
                else if (model.TypeId == (int)UtilConstants.ScheduleType.SetCalendar)
                {
                    if (schedulesType != null)
                    {
                        //var csResult = schedulesType.Value.Split(new char[] { '|' },
                        //    StringSplitOptions.RemoveEmptyEntries);
                        model.DateCalendar = DateTime.Parse(schedulesType.Value);
                        // model.TimeCalendar = csResult[1];
                    }
                }
                else if (model.TypeId == (int)UtilConstants.ScheduleType.Periodic)
                {
                    var csResult = schedulesType.Value.Split(new char[] { '|' },
                           StringSplitOptions.RemoveEmptyEntries);
                    model.DayValues = csResult[0].Split(new char[] { ',' },
                           StringSplitOptions.RemoveEmptyEntries).Select(n => Convert.ToInt32(n)).ToArray();
                    model.TimePeriodic = csResult[1];
                }


            }
            return View(model);
        }
        [HttpPost]
        public ActionResult SubmitSetParticipateSchedule(int isParticipate, string id)
        {
            int idschedule = int.Parse(id);
            var removeSchedule = ConfigService.GetScheduleById(idschedule);
            if (removeSchedule == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            if (isParticipate == 1)
            {
                removeSchedule.IsActive = false;
            }
            else
            {
                removeSchedule.IsActive = true;
            }
            ConfigService.UpdateSchedule(removeSchedule);

            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeSchedule.Name), result = true }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult ReSendMail(string id)
        {
            var inId = int.Parse(id);
            var item = _repoMailService.GetById(inId);
            if (item != null)
            {
                if (MailUtil.SendMail(item.mail_receiver, item.subjectname ?? item.CAT_MAIL.SubjectMail, item.content_body))
                {
                    item.flag_sent = 1;
                }
                else
                {
                    item.flag_sent = 2;
                }
            }
            _repoMailService.Update(item);


            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, item.mail_receiver), result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int id = -1)
        {
            try
            {
                var model = ConfigService.GetScheduleById(id);
                if (model == null)
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);

                model.IsDelete = true;
                model.IsActive = false;
                ConfigService.UpdateSchedule(model);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Schedule/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult TemplateMail(int? id)
        {
            var result = string.Empty;
            string code = string.Empty;
            var model = ConfigService.GetMailById(id);
            if (model != null)
            {
                code = model.Code;
                result = Regex.Replace(model.Content, "<.*?>", string.Empty);
            }
            return Json(new
            {
                value = result,
                code = code,
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public JsonResult ChangeCourseReturnSubjectResult(int courseId, int? reportsubjectresult)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                int null_instructor = 1;
                var data = _repoCourseServiceDetail.Get(a => a.CourseId == courseId);//
                if (data.Any())
                {
                    null_instructor = 0;
                    foreach (var item in data.Where(a => (reportsubjectresult == 1 ? true : true)))
                    {
                        if ((bool)_repoCourseServiceDetail.checkapproval(item, new[] { (int)UtilConstants.ApproveType.AssignedTrainee }))

                            html.AppendFormat("<option value='{0}'>{1}</option>", item.SubjectDetailId, item.SubjectDetail.Name);
                    }
                }
                else
                {
                    null_instructor = 1;
                }

                return Json(new
                {
                    value_option = html.ToString(),
                    value_null = null_instructor
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Schedule/ChangeCourseReturnSubjectResult", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }
    }
}
