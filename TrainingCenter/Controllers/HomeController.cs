using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using System.Text;
using System.Threading;
using System.Globalization;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using DAL.Entities;
using TMS.Core.Services.CourseResultSummary;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.Mail;
using TMS.Core.Services.Subject;

namespace TrainingCenter.Controllers
{
    using System.Data.Entity.SqlServer;
    using TMS.Core.Services.Approves;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using CustomAuthorizes;
    using TMS.Core.Services.Department;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Notifications;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.App_GlobalResources;
    using System.Configuration;
    using Newtonsoft.Json.Linq;
    using System.Data;

    public class HomeController : BaseAdminController
    {
        private readonly IApproveService _repoTMS_APPROVES;
        private readonly ICourseDetailService CourseService_Detail;
        private readonly INotificationService _repoNotification;
        private readonly ISubjectService _repoSubjectService;
        private readonly IJobtitleService _reJobtitleService;
        private readonly ICourseResultSummaryService _resultSummaryService;
        private readonly IConfigService _configService;
        private readonly IMailService _mailService;
        public HomeController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IApproveService repoTmsApproves, ICourseService repoCourse, ICourseDetailService repoCourseDetail, INotificationService repoNotification, IDepartmentService departmentService, ISubjectService repoSubjectService, IJobtitleService reJobtitleService, ICourseResultSummaryService resultSummaryService, IMailService mailService)
            : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, repoCourse, repoTmsApproves)
        {
            _repoTMS_APPROVES = repoTmsApproves;
            CourseService_Detail = repoCourseDetail;
            _repoNotification = repoNotification;
            _repoSubjectService = repoSubjectService;
            _reJobtitleService = reJobtitleService;
            _resultSummaryService = resultSummaryService;
            this._mailService = mailService;
            _configService = configService;
            _mailService = mailService;
        }
        //[OutputCache(CacheProfile = "Cache_home_index")]
        public ActionResult Index()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);

            //var getallcourse = CourseService.GetListCourse(x => x.TMS_APPROVES.Any(a => a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course)).Select(a => a.Id);

            //var getallcourse = _repoTMS_APPROVES.Get(a => a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course
            //).Select(a => a.int_Course_id);
            var course_ = CourseService.Get(a => a.StartDate >= timenow && 
            a.TMS_APPROVES.Any(x=>x.int_id_status == (int)UtilConstants.EStatus.Approve && x.int_Type == (int)UtilConstants.ApproveType.Course)
            && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id)),true).Select(a => a.Id);

            var entity = CourseService_Detail.Get(a => a.Course.IsDeleted != true && a.dtm_time_from.Value.Year == DateTime.Now.Year && course_.Contains((int)a.CourseId));

            var course_detail_danghoc = entity.Count(a =>
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) <= 0)
            &&
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) >= 0)
            );


            var course_detail_saphoc = entity.Count(a =>
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) > 0)
            &&
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) > 0)
            );

            var course_detail_dahoc = entity.Count(a =>
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) < 0)
            &&
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) < 0)
            );



            ViewBag.count_danghoc = course_detail_danghoc;
            ViewBag.count_saphoc = course_detail_saphoc;
            ViewBag.count_dahoc = course_detail_dahoc;
            ViewBag.count_tong = entity.Count();

            ViewBag.percent_danghoc = Percent(entity.Count(), course_detail_danghoc);
            ViewBag.percent_saphoc = Percent(entity.Count(), course_detail_saphoc);
            ViewBag.percent_dahoc = Percent(entity.Count(), course_detail_dahoc);

            //ViewBag.CourseList = new SelectList(CourseService.Get().OrderBy(m => m.Name).ToList(), "Course_Id", "str_Name");
            ViewBag.RoomList = new SelectList(CourseService.GetRoom().OrderBy(m => m.str_Name), "Room_Id", "str_Name");
            ViewBag.InstructorList = new SelectList(EmployeeService.GetInstructors(true)?.OrderBy(m => m.FirstName).ToDictionary(a => a.Id, a => ReturnDisplayLanguage(a.FirstName, a.LastName)), "Key", "Value");



            //call webservice API
            //var countlms = 0;
            //if (Session["DataLms_NextTime"] != null && DateTime.Now < DateTime.Parse(Session["DataLms_NextTime"].ToString()))
            //{
            //    var data = JObject.Parse(GetByKey("DataLmsUser"));
            //    var date = (int)(DateTime.UtcNow.AddMinutes(-5).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            //    var postTitles =
            //        from p in data["users"]
            //        where (int)p["lastaccess"] > date
            //        select (string)p["lastaccess"];

            //    countlms = postTitles.Count();
            //}
            //else
            //{
            //    RequestWsLms.AddParameter("criteria[0][key]", "");
            //    RequestWsLms.AddParameter("criteria[0][value]", "");
            //    //var saApiResponse = CallServicesReturnJson(UtilConstants.core_user_get_users, RequestWsLms);
            //    //var da = _configService.Get(a => a.KEY.Equals("DataLmsUser")).FirstOrDefault();
            //    //if (da != null)
            //    //{
            //    //    da.VALUE = saApiResponse.ToString().Trim();
            //    //}
            //    //_configService.UpdateConfig(da);
            //    Session["DataLms_NextTime"] = DateTime.Now.AddMinutes(3);
            //}


            ViewBag.countOnline = 0;
            return View();
        }

        private static decimal Percent(int total, int curent)
        {
            var rr = 0.ToString("0.##");
            if (total != 0 && curent != 0)
            {
                rr = ((curent * 100) / total).ToString("0.##");
            }
            return decimal.Parse(rr);
        }
        public ActionResult AjaxHandlerCourseTraining(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var getallcourse = _repoTMS_APPROVES.Get(a => a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course
          ).Select(a => a.int_Course_id);//&& a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee
                var course_ = CourseService.Get(a => a.StartDate >= timenow && getallcourse.Contains(a.Id)
                && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id))).Select(a => a.Id);

                var course_detail_danghoc = CourseService_Detail.Get(a => a.dtm_time_from.Value.Year == DateTime.Now.Year
            && course_.Contains((int)a.CourseId)
            &&
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) <= 0)
            &&
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) >= 0)
            );

                IEnumerable<Course_Detail> filtered = course_detail_danghoc;


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Course?.Code
                                                          : sortColumnIndex == 2 ? c.SubjectDetail.Name
                                                            : sortColumnIndex == 3 ? c.dtm_time_from
                                                            : sortColumnIndex == 4 ? c.dtm_time_from
                                                            : sortColumnIndex == 5 ? c.dtm_time_to
                                                            : sortColumnIndex == 6 ? c.dtm_time_to
                                                            : sortColumnIndex == 7 ? c.Room?.str_Name
                                                            //: sortColumnIndex == 6 ? c.Trainee?.str_Fullname
                                                            : sortColumnIndex == 9 ? c.type_leaning
                                                            : sortColumnIndex == 10 ? (object)c.dtm_time_from
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
                var result = from c in displayed
                                 //let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details","Instructor", new { id = a.Trainee?.Id }) + "'>"+ a.Trainee?.FirstName + " " + a.Trainee?.LastName+"</a>").ToArray())
                             let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details", "Instructor", new { id = a.Trainee?.Id }) + "'>" + ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName) + "</a>").ToArray())
                             select new object[] {
                                 string.Empty,
                                c?.Course?.Code,
                                 //"<a title='View' href='"+@Url.Action("Details","Course",new{id = c.CourseId+"#SCHEDULES"})+"')' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                 "<a title='View' href='/course/details/"+c.CourseId+"#SCHEDULES"+"' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                DateUtil.DateToString( c?.dtm_time_from ,"dd/MM/yyyy")+"<br />"+
                               c?.time_from,
                                DateUtil.DateToString( c?.dtm_time_to ,"dd/MM/yyyy")+"<br />"+
                                c?.time_to
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Home/AjaxHandlerCourseTraining", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlerCourseUpcoming(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var getallcourse = _repoTMS_APPROVES.Get(a => a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course
           ).Select(a => a.int_Course_id);//&& a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee
                var course_ = CourseService.Get(a => a.StartDate >= timenow && getallcourse.Contains(a.Id)
                && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id))).Select(a => a.Id);
                var course_detail_saphoc = CourseService_Detail.Get(a => a.dtm_time_from.Value.Year == DateTime.Now.Year
            && course_.Contains((int)a.CourseId)
            &&
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) > 0)
            &&
            (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) > 0)
            );

                IEnumerable<Course_Detail> filtered = course_detail_saphoc;


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Course?.Code
                                                          : sortColumnIndex == 2 ? c.SubjectDetail.Name
                                                            : sortColumnIndex == 3 ? c.dtm_time_from
                                                            : sortColumnIndex == 4 ? c.dtm_time_from
                                                            : sortColumnIndex == 5 ? c.dtm_time_to
                                                            : sortColumnIndex == 6 ? c.dtm_time_to
                                                            : sortColumnIndex == 7 ? c.Room?.str_Name
                                                            //: sortColumnIndex == 6 ? c.Trainee?.str_Fullname
                                                            : sortColumnIndex == 9 ? c.type_leaning
                                                            : sortColumnIndex == 10 ? (object)c.dtm_time_from
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
                                 //let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details","Instructor", new { id = a.Trainee?.Id }) + "'>"+ a.Trainee?.FirstName + " " + a.Trainee?.LastName+"</a>").ToArray())
                             let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details", "Instructor", new { id = a.Trainee?.Id }) + "'>" + ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName) + "</a>").ToArray())
                             select new object[] {
                                 string.Empty,
                                c?.Course?.Code,
                                 //"<a title='View' href='"+@Url.Action("Details","Course",new{id = c.CourseId+"#SCHEDULES"})+"')' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                 "<a title='View' href='/course/details/"+c.CourseId+"#SCHEDULES"+"' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                DateUtil.DateToString( c?.dtm_time_from ,"dd/MM/yyyy")+"<br />"+
                               c?.time_from,
                                DateUtil.DateToString( c?.dtm_time_to ,"dd/MM/yyyy")+"<br />"+
                                c?.time_to
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Home/AjaxHandlerCourseUpcoming", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlerCourseComplete(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var getallcourse = _repoTMS_APPROVES.Get(a => a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course
                           ).Select(a => a.int_Course_id);//&& a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee
                var course_ = CourseService.Get(a => a.StartDate >= timenow &&  getallcourse.Contains(a.Id)
                && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id))).Select(a => a.Id);
                var course_detail_dahoc = CourseService_Detail.Get(a => a.dtm_time_from.Value.Year == DateTime.Now.Year
          && course_.Contains((int)a.CourseId)
         &&
         (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) < 0)
         &&
         (SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) < 0)
         );
                IEnumerable<Course_Detail> filtered = course_detail_dahoc;


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Course?.Code
                                                          : sortColumnIndex == 2 ? c.SubjectDetail.Name
                                                            : sortColumnIndex == 3 ? c.dtm_time_from
                                                            : sortColumnIndex == 4 ? c.dtm_time_from
                                                            : sortColumnIndex == 5 ? c.dtm_time_to
                                                            : sortColumnIndex == 6 ? c.dtm_time_to
                                                            : sortColumnIndex == 7 ? c.Room?.str_Name
                                                            //: sortColumnIndex == 6 ? c.Trainee?.str_Fullname
                                                            : sortColumnIndex == 9 ? c.type_leaning
                                                            : sortColumnIndex == 10 ? (object)c.dtm_time_from
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
                var result = from c in displayed
                                 //let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details","Instructor", new { id = a.Trainee?.Id }) + "'>"+ a.Trainee?.FirstName + " " + a.Trainee?.LastName+"</a>").ToArray())
                             let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details", "Instructor", new { id = a.Trainee?.Id }) + "'>" + ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName) + "</a>").ToArray())
                             select new object[] {
                                 string.Empty,
                                c?.Course?.Code,
                                 //"<a title='View' href='"+@Url.Action("Details","Course",new{id = c.CourseId+"#SCHEDULES"})+"')' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                 "<a title='View' href='/course/details/"+c.CourseId+"#SCHEDULES"+"' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                DateUtil.DateToString( c?.dtm_time_from ,"dd/MM/yyyy")+"<br />"+
                               c?.time_from,
                                DateUtil.DateToString( c?.dtm_time_to ,"dd/MM/yyyy")+"<br />"+
                                c?.time_to
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Home/AjaxHandlerCourseComplete", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        //[PartialCache("Cache_home_AjaxHandlerSchedule")]
        public ActionResult AjaxHandlerSchedule(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
               // CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                var str_Type = string.IsNullOrEmpty(Request.QueryString["str_Type"]) ? -1 : Convert.ToInt32(Request.QueryString["str_Type"].Trim());
                var str_Status = string.IsNullOrEmpty(Request.QueryString["str_Status"]) ? -1 : Convert.ToInt32(Request.QueryString["str_Status"].Trim());
                var RoomList = string.IsNullOrEmpty(Request.QueryString["RoomList"]) ? -1 : Convert.ToInt32(Request.QueryString["RoomList"].Trim());
                var InstructorList = string.IsNullOrEmpty(Request.QueryString["InstructorList"]) ? -1 : Convert.ToInt32(Request.QueryString["InstructorList"].Trim());
                var fSearchDate_from = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();

                var str_code = string.IsNullOrEmpty(Request.QueryString["str_code"]) ? "" : Request.QueryString["str_code"].Trim();
                var CourseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? "" : Request.QueryString["CourseList"].Trim();
                var ddl_subject = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? "" : Request.QueryString["ddl_subject"].Trim();
                DateTime datefrom;

                DateTime.TryParse(fSearchDate_from, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out datefrom);

                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var course_danghoc = CourseService.Get(a => a.StartDate >= timenow && a.Course_TrainingCenter.Any(x => CurrentUser.PermissionIds.Any(z => z == x.khoidaotao_id))).Select(a => a.Id);//&& a.dtm_StartDate > DateTime.Now
                var datatemp = CourseService_Detail.Get(a => course_danghoc.Contains((int)a.CourseId) &&
                    (str_Type == -1 || a.type_leaning == str_Type) &&
                    (string.IsNullOrEmpty(str_code) || a.Course.Code.Contains(str_code)) &&
                    (string.IsNullOrEmpty(CourseList) || a.Course.Name.Contains(CourseList)) &&
                    (string.IsNullOrEmpty(ddl_subject) || a.SubjectDetail.Name.Contains(ddl_subject)) &&
                    (string.IsNullOrEmpty(fSearchDate_from) ||
                    SqlFunctions.DateDiff("Day", datefrom, a.dtm_time_from) <= 0
                    &&
                    SqlFunctions.DateDiff("Day", datefrom, a.dtm_time_to) >= 0
                    ) &&
                    (RoomList == -1 || a.RoomId == RoomList) &&
                    (InstructorList == -1 || a.Course_Detail_Instructor.Any(q => q.Instructor_Id == InstructorList))
                &&
                 SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) >= 0
               && (str_Status == -1 || ((str_Status == (int)UtilConstants.ScheduleStatus.Upcoming) ?
               SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) > 0
               :
               SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) <= 0
               )));

                var data = datatemp.ToList().Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var tempcount = datatemp.Count();

                IEnumerable<Course_Detail> filtered = data;


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Course?.Code
                                                          : sortColumnIndex == 2 ? c.SubjectDetail.Name
                                                            : sortColumnIndex == 3 ? c.dtm_time_from
                                                            : sortColumnIndex == 4 ? c.dtm_time_from
                                                            : sortColumnIndex == 5 ? c.dtm_time_to
                                                            : sortColumnIndex == 6 ? c.dtm_time_to
                                                            : sortColumnIndex == 7 ? c.Room?.str_Name
                                                            //: sortColumnIndex == 6 ? c.Trainee?.str_Fullname
                                                            : sortColumnIndex == 9 ? c.type_leaning
                                                            : sortColumnIndex == 10 ? (object)c.dtm_time_from
                                                          : c.dtm_time_from);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var displayed = filtered;
                var result = from c in displayed
                                 //let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details","Instructor", new { id = a.Trainee?.Id }) + "'>"+ a.Trainee?.FirstName + " " + a.Trainee?.LastName+"</a>").ToArray())
                             let instructor = string.Join(", ", c?.Course_Detail_Instructor?.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => "<a href='" + @Url.Action("Details", "Instructor", new { id = a.Trainee?.Id }) + "'>" + ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName) + "</a>").ToArray())
                             select new object[] {
                                 string.Empty,
                                c?.Course?.Code,
                                 //"<a title='View' href='"+@Url.Action("Details","Course",new{id = c.CourseId+"#SCHEDULES"})+"')' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                 "<a title='View' href='/course/details/"+c.CourseId+"#SCHEDULES"+"' data-toggle='tooltip'>"+c?.SubjectDetail?.Name+"</a>",
                                DateUtil.DateToString( c?.dtm_time_from ,"dd/MM/yyyy")+"<br />"+
                                c?.time_from,
                                DateUtil.DateToString( c?.dtm_time_to ,"dd/MM/yyyy")+"<br />"+
                                c?.time_to,
                                c?.Room?.str_Name + "<input type='hidden' name='ids[]' id='ids[]' value='"+c.SubjectDetailId+"' />",
                                instructor,
                                TypeLearningIcon(c.type_leaning.Value),
                                (c?.dtm_time_from > DateTime.Now ? "<span class='label label-warning'>Upcoming</span>" : "<span class='label label-danger'>In process</span>")
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = tempcount,
                    iTotalDisplayRecords = tempcount,
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Home/AjaxHandlerSchedule", ex.Message);
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
        public ActionResult Change(string lang)
        {
            if (lang != null)
            {

                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

            }
            HttpCookie cookie = new HttpCookie("Language");
            cookie.Value = lang;
            Response.Cookies.Add(cookie);
            return Json(CMSUtils.alert("danger", "dd"));
        }

        [HttpPost]
        public JsonResult ChangeCourseReturnSubject(int id_course)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                int null_instructor = 1;
                var db = CourseService_Detail.Get(a => a.CourseId == id_course && a.SubjectDetail.IsDelete != true && a.SubjectDetail.IsDelete == true);
                if (db.Any())
                {
                    html.Append("<option value='-1'>--Subject List--</option>");
                    null_instructor = 0;
                    foreach (var item in db)
                    {
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
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }


        [HttpPost]
        public FileContentResult Export(FormCollection form)
        {
            int str_Type = string.IsNullOrEmpty(form["str_Type"]) ? -1 : Convert.ToInt32(form["str_Type"].Trim());
            int str_Status = string.IsNullOrEmpty(form["str_Status"]) ? -1 : Convert.ToInt32(form["str_Status"].Trim());
            //int str_code = string.IsNullOrEmpty(form["str_code"]) ? -1 : Convert.ToInt32(form["str_code"].Trim());
            //int CourseList = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"].Trim());
            //int ddl_subject = string.IsNullOrEmpty(form["ddl_subject"]) ? -1 : Convert.ToInt32(form["ddl_subject"].Trim());
            int RoomList = string.IsNullOrEmpty(form["RoomList"]) ? -1 : Convert.ToInt32(form["RoomList"].Trim());
            int InstructorList = string.IsNullOrEmpty(form["InstructorList"]) ? -1 : Convert.ToInt32(form["InstructorList"].Trim());
            string fSearchDate_from = string.IsNullOrEmpty(form["fSearchDate_from"]) ? "" : form["fSearchDate_from"].ToString();


            //string fSearchDate_to = string.IsNullOrEmpty(form["fSearchDate_to"]) ? "" : form["fSearchDate_to"].ToString();
            //DateTime? FromDate_from, ToDate_from;
            //AppUtils.StringToDateRange(fSearchDate_from, out FromDate_from, out ToDate_from);
            //DateTime? FromDate_to, ToDate_to;
            //AppUtils.StringToDateRange(fSearchDate_to, out FromDate_to, out ToDate_to);
            string str_code = string.IsNullOrEmpty(form["str_code"]) ? "" : form["str_code"].ToString();
            string CourseList = string.IsNullOrEmpty(form["CourseList"]) ? "" : form["CourseList"].ToString();
            string ddl_subject = string.IsNullOrEmpty(form["ddl_subject"]) ? "" : form["ddl_subject"].ToString();




            var datefrom = DateUtil.StringToDate(fSearchDate_from, DateUtil.DATE_FORMAT_OUTPUT);

            var getallcourse = _repoTMS_APPROVES.Get(a => a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course
      ).Select(a => a.int_Course_id);

            var course_danghoc = CourseService.Get(a => getallcourse.Contains(a.Id)).Select(a => a.Id);//&& a.dtm_StartDate > DateTime.Now



            var data = CourseService_Detail.Get(a => course_danghoc.Contains((int)a.CourseId) &&
                (str_Type == -1 || a.type_leaning == str_Type) &&
                (String.IsNullOrEmpty(str_code) || a.Course.Code.Contains(str_code)) &&
                (String.IsNullOrEmpty(CourseList) || a.Course.Name.Contains(CourseList)) &&
                (String.IsNullOrEmpty(ddl_subject) || a.SubjectDetail.Name.Contains(ddl_subject)) &&
                (String.IsNullOrEmpty(fSearchDate_from) ||
                SqlFunctions.DateDiff("Day", datefrom, a.dtm_time_from) <= 0
                &&
                SqlFunctions.DateDiff("Day", datefrom, a.dtm_time_to) >= 0
                //a.dtm_time_from <= datefrom && a.dtm_time_to >= datefrom
                ) &&
                (RoomList == -1 || a.RoomId == RoomList) &&
 (InstructorList == -1 || a.Course_Detail_Instructor.Any(q => q.Instructor_Id == InstructorList))
            &&
             SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_to) >= 0
           //a.dtm_time_to >= DateTime.Now
           && (str_Status == -1 || ((str_Status == (int)UtilConstants.ScheduleStatus.Upcoming) ?
           SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) > 0
           // a.dtm_time_from > DateTime.Now
           :
           SqlFunctions.DateDiff("Day", DateTime.Now, a.dtm_time_from) <= 0
           //a.dtm_time_from < DateTime.Now
           )))
            .OrderBy(p => p.dtm_time_from);




            byte[] filecontent = ExportExcelCourse(data);
            return File(filecontent, ExportUtils.ExcelContentType, "Schedule.xlsx");
        }
        private byte[] ExportExcelCourse(IEnumerable<Course_Detail> listCourseID)
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "ExcelFile/Schedule.xlsx");
            FileInfo template = new FileInfo(templateFilePath);

            //var data = CourseService_Detail.Get(a=>!a.bit_Deleted && (!listCourseID.Any() || listCourseID.Contains(a.Course_Detail_Id)));
            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                var worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                var startrow = 7;
                var groupHeader = 0;
                var grouptotal = 0;
                var row = 0;
                var count = 0;
                var noRow = 0;
                if (listCourseID.Count() == 0)
                {
                    noRow = 1;
                }
                foreach (var item1 in listCourseID)
                {
                    int col = 1;
                    count++;
                    ExcelRange cellNo = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col];
                    cellNo.Value = count;
                    cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellCourseCode = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 1];
                    cellCourseCode.Value = item1.Course?.Code;
                    cellCourseCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellCourseCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellsubjectname = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 2];
                    cellsubjectname.Value = item1.SubjectDetail?.Name;
                    cellsubjectname.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellsubjectname.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellsubjectname.Style.WrapText = true;

                    ExcelRange celldtm_time_from = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 3];
                    celldtm_time_from.Value = DateUtil.DateToString(item1.dtm_time_from, "dd/MM/yyyy");
                    celldtm_time_from.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celldtm_time_from.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celldtm_time_to = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 4];
                    celldtm_time_to.Value = DateUtil.DateToString(item1.dtm_time_to, "dd/MM/yyyy");
                    celldtm_time_to.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celldtm_time_to.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celltime_from = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 5];
                    celltime_from.Value = (item1.time_from != null ? (item1.time_from?.ToString().Substring(0, 2) + ":" + item1.time_from?.ToString().Substring(2)) : "");
                    celltime_from.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celltime_from.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celltime_to = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 6];
                    celltime_to.Value = (item1.time_to != null ? (item1.time_to?.ToString().Substring(0, 2) + ":" + item1.time_to?.ToString().Substring(2)) : "");// item1.Trainee?.str_Fullname;
                    celltime_to.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celltime_to.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellroom = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 7];
                    cellroom.Value = item1.Room?.str_Name;
                    cellroom.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellroom.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellroom.Style.WrapText = true;

                    string nameinstructor = "";
                    //var instructor = CourseService_Detail_Instructor.Get(a => a.Course_Detail_Id == item1.Course_Detail_Id);
                    var instructor = item1.Course_Detail_Instructor;
                    if (instructor.Any())
                    {
                        foreach (var item_ in instructor)
                        {
                            //nameinstructor += item_?.Trainee?.FirstName + " " + item_.Trainee?.LastName + " \n";
                            nameinstructor += ReturnDisplayLanguage(item_?.Trainee?.FirstName, item_?.Trainee?.LastName) + " \n";
                        }
                    }
                    ExcelRange cellInstructor = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 8];
                    cellInstructor.Value = nameinstructor;// item1.Trainee?.str_Fullname;
                    cellInstructor.Style.WrapText = true;
                    cellInstructor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellInstructor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celltype = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 9];
                    celltype.Value = TypeLearningName(item1.type_leaning.Value);
                    celltype.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celltype.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellstatus = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 10];
                    cellstatus.Value = (item1.dtm_time_from > DateTime.Now ? "Upcoming" : "In process");
                    cellstatus.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellstatus.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    row++;
                }
                using (ExcelRange r = worksheet.Cells[startrow + 1, 1, startrow + row + groupHeader + grouptotal + noRow, 11])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }



                worksheet.Cells[2, 3, 4, 10].Merge = true;
                ExcelRange cell9 = worksheet.Cells[2, 3];
                worksheet.Cells[2, 3].Style.Font.Bold = true;
                worksheet.Cells[2, 3].Style.Font.Size = 14;
                cell9.Value = "SCHEDULES";
                cell9.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell9.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                Bytes = xlPackage.GetAsByteArray();

            }
            return Bytes;
        }

        [HttpPost]
        //TODO : co thay doi iduser = iddata
        public ActionResult GetMessages(int pageIndex, int pageSize)
        {
            //var notificationDetail =
            //    _repoNotification.GetDetails(a => a.iddata == CurrentUser.USER_ID && a.IsActive == true && a.IsDeleted != true).OrderByDescending(n => n.idmessenge);
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var notificationDetail =
                _repoNotification.GetDetails(a => a.datesend >= timenow).OrderByDescending(n => n.idmessenge);



            var model = new NotificationModel()
            {
                NotificationDetail = notificationDetail.Skip(pageIndex * pageSize).Take(pageSize),
                ///Count = notificationDetail.Count(a => a.status == 0)


            };
            return PartialView("_MessagesList", model);
        }

        //public ActionResult GetMessages()
        //{
        //    var notificationDetail =
        //        _repoNotification.GetDetails(a => a.iddata == CurrentUser.USER_ID).OrderByDescending(n => n.idmessenge);

        //    var model = new NotificationModel()
        //    {
        //        NotificationDetail = notificationDetail,
        //        Count = notificationDetail.Count(a => a.status == 0)


        //    };
        //    return PartialView("_MessagesList", model);
        //}
        //TODO : co thay doi iduser = iddata
        public ActionResult GetMessages2_()
        {
            var notificationDetail =
               _repoNotification.GetDetails(a => a.iddata == CurrentUser.USER_ID).OrderByDescending(n => n.idmessenge);

            var model = new NotificationModel()
            {
                NotificationDetail = notificationDetail,
                Count = notificationDetail.Count(a => a.status == 0)
            };
            return PartialView("_MessagesList2", model);
        }
        private class EventViewModel
        {
            public Int64 id { get; set; }

            public String title { get; set; }

            public String start { get; set; }

            public String end { get; set; }

            public bool allDay { get; set; }
        }
        public JsonResult GetMessages2(DateTime start, DateTime end)
        {
            var viewModel = new EventViewModel();
            var events = new List<EventViewModel>();
            start = DateTime.Today.AddDays(-14);
            end = DateTime.Today.AddDays(-11);

            for (var i = 1; i <= 5; i++)
            {
                events.Add(new EventViewModel()
                {
                    id = i,
                    title = "Event " + i,
                    start = start.ToString(),
                    end = end.ToString(),
                    allDay = false
                });

                start = start.AddDays(7);
                end = end.AddDays(7);
            }


            return Json(events.ToArray(), JsonRequestBehavior.AllowGet);
        }
        public void viewnotification(int MessageID)
        {
            Notification_Detail models = _repoNotification.GetDetailById(MessageID);
            models.status = (int)UtilConstants.NotificationStatus.Hidden;
            _repoNotification.Update(models);
        }

        //TODO: co thay doi iduser = iddata
        public void MarkAllasRead()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            var notification = new Notification_Detail();
            var iduser = GetUser().USER_ID;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var models = _repoNotification.GetDetails(a => a.iddata == iduser && (a.status == null || a.status == 0) && a.datesend >= timenow);
            if (models.Any())
            {
                foreach (var item in models)
                {
                    notification = _repoNotification.GetDetailById(item.id);
                    if (notification.status == (int)UtilConstants.NotificationStatus.Show)
                    {
                        notification.status = (int)UtilConstants.NotificationStatus.Hidden;
                    }
                }
                _repoNotification.Update(notification);
            }
        }

        [HttpPost]
        public ActionResult Hidden(int? id)
        {
            if (!id.HasValue)
            {
                return Json(new AjaxResponseViewModel() { result = false, message = string.Format(Resource.INVALIDDATA) });
            }
            try
            {
                //var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                //var model = _repoNotification.GetDetails();
                Notification_Detail notification = _repoNotification.GetDetailById(id.Value);
                if (notification.IsActive == true)
                {
                    notification.IsActive = false;
                }
                _repoNotification.Update(notification);
                return Json(new AjaxResponseViewModel() { result = true, message = string.Format(Messege.SUCCESS) });
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel() { result = false, message = string.Format(Messege.UNSUCCESS) });
            }
        }

        public ActionResult FormFilter1()
        {
            var model = CourseService.Get().OrderByDescending(m => m.Id);
            return PartialView("PartialView/Filter/_Filter1", model);
        }


        public ActionResult GetCron()
        {
            var dicCron = new Dictionary<string, bool>();

            var trainees = EmployeeService.Get(a => a.LmsStatus == StatusModify || a.LmsStatus == null, true);
            dicCron.Add(UtilConstants.CRON_USER, trainees.Any());

            var subjects = _repoSubjectService.GetSubjectDetailApi(a => a.LmsStatus == StatusModify || a.LmsStatus == null);
            dicCron.Add(UtilConstants.CRON_SUBJECT, subjects.Any());
            var departments = DepartmentService.ApiGet(a => a.LmsStatus == StatusModify || a.LmsStatus == null);
            dicCron.Add(UtilConstants.CRON_DEPARTMENT, departments.Any());
            var jobtitles = _reJobtitleService.Get(a => a.LmsStatus == StatusModify || a.LmsStatus == null, true, true);
            dicCron.Add(UtilConstants.CRON_JOBTITLE, jobtitles.Any());


            var groupCourse = _repoSubjectService.GetAPIGroupSubject(a => (a.LmsStatus == StatusModify || a.LmsStatus == null) && a.CAT_GROUPSUBJECT_ITEM.Any());
            dicCron.Add(UtilConstants.CRON_GET_LIST_CATEGORY, groupCourse.Any());

            var traineeHistory = EmployeeService.GetAllTraineeHistories(a => a.LmsStatus == StatusModify || a.LmsStatus == null);
            dicCron.Add(UtilConstants.CRON_TRAINEE_HISTORY, traineeHistory.Any());

            var program = CourseService.ApiGet(a => a.LMSStatus == StatusModify || a.LMSStatus == null && a.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve));
            dicCron.Add(UtilConstants.CRON_PROGRAM, program.Any());

            var courseOfProgram = CourseDetailService.GetAllApi(a => (a.LmsStatus == StatusModify || a.LmsStatus == null) && (a.Course.LMSStatus == StatusIsSync && a.Course.IsDeleted != true));
            dicCron.Add(UtilConstants.CRON_COURSE, courseOfProgram.Any());

            var assignTrainee = CourseMemberService.GetApi(a => (a.LmsStatus == StatusModify || a.LmsStatus == null) &&
                                                             (a.Trainee.LmsStatus == StatusIsSync &&
                                                              a.Trainee.IsDeleted != true) &&
                                                             (a.Course_Detail.LmsStatus == StatusIsSync &&
                                                              a.Course_Detail.IsDeleted != true) &&
                                                             a.Course_Detail.Course.IsDeleted != true);
            dicCron.Add(UtilConstants.CRON_ASSIGN_TRAINEE, assignTrainee.Any());



            var courseResult = _resultSummaryService.Get(a =>
                            a.Course_Detail.IsDeleted != true && a.Course_Detail.IsActive == true &&
                            (a.LmsStatus == StatusModify || a.LmsStatus == null) && a.Trainee.LmsStatus == StatusIsSync &&
                            a.Trainee.IsDeleted != true);
            dicCron.Add(UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY, courseResult.Any());

            var certificate = CourseService.GetCourseResultFinal(a =>
                           a.LmsStatus == StatusModify && a.Course.LMSStatus == StatusIsSync &&
                           a.Course.IsDeleted != true && a.Trainee.LmsStatus == StatusIsSync && a.Trainee.IsDeleted != true);
            dicCron.Add(UtilConstants.CRON_GET_CERTIFICATE, certificate.Any());

            var model = new AjaxResponseViewModel()
            {
                result = true,
                data = dicCron
            };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult Notification()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult AjaxHandlerNotification(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();               
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                var data = _repoNotification.GetDetails(a => a.datesend >= timenow).OrderByDescending(n => n.idmessenge);
                IEnumerable<Notification_Detail> filtered = data;


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Notification_Detail, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Notification?.Message
                                                          : sortColumnIndex == 2 ? c?.Notification?.MessageContent
                                                            : sortColumnIndex == 3 ? c?.iduserfrom.ToString()
                                                            : sortColumnIndex == 4 ? c?.datesend.ToString()
                                                          : c?.datesend.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }

                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()                           
                             select new object[] {
                                 string.Empty,
                               c?.Notification?.URL != null ? "<a href='"+c?.Notification?.URL+"'>" + c?.Notification?.Message+"</a>" :"<a href='javascript:void(0)'>" + c?.Notification?.Message+"</a>"  ,
                                c?.Notification?.MessageContent,
                                c?.USER.FIRSTNAME + " " + c?.USER.LASTNAME,
                                DateUtil.DateToString(c?.datesend,"dd/MM/yyyy hh:ss")
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

    }
}
