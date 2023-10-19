using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Entities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.CourseResultSummary;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.PostNews;
using TMS.Core.Services.Subject;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Cron;

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
    public class CronController : BaseAdminController
    {
        #region init
        private readonly ISubjectService _repoSubjectService;
        private readonly IJobtitleService _reJobtitleService;
        private readonly ICourseResultSummaryService _resultSummaryService;
        private readonly IPostNewsService _postNewsService;
        private readonly IPostCategoryService _postCategoryService;
        private const int statusIsNoResponse = (int)UtilConstants.ApiStatus.NoResponse;

        public CronController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService, ISubjectService repoSubjectService, IJobtitleService reJobtitleService, ICourseResultSummaryService resultSummaryService, IPostNewsService postNewsService, IPostCategoryService postCategoryService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoSubjectService = repoSubjectService;
            _reJobtitleService = reJobtitleService;
            _resultSummaryService = resultSummaryService;
            _postNewsService = postNewsService;
            _postCategoryService = postCategoryService;
        }
        #endregion
        // GET: Cron
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var name = string.IsNullOrEmpty(Request.QueryString["name"]) ? string.Empty : Request.QueryString["name"].Trim().ToLower();
                var state = string.IsNullOrEmpty(Request.QueryString["state"]) ? -1 : Convert.ToInt32(Request.QueryString["state"].Trim());
                var data = GetCron();
                var boolState = state == 1;
                var filtered = data.Where(a => (string.IsNullOrEmpty(name) || a.Name.Trim().ToLower().Contains(name)) && (state == -1 || a.IsSync == boolState));
                var sortColumnIdex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CronView, object> orderingFunction = (c =>
                                sortColumnIdex == 1 ? c.Name :
                                sortColumnIdex == 2 ? c.IsSync
                                : (object)c.IsSync);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[]
                             {
                                 string.Empty,
                                 c.Name,
                                 (c.IsSync ? "<span class='label label-warning'>"+Resource.lblAwaitSync+"</span>" : "<span class='label label-success'>"+Resource.lblSynchronized+"</span>") + " ("+c.count+") " +  "<input type='hidden' class='form-control' name='"+c.Name+"' value='"+c.count_noreponse+"'/>",
                                 c.IsSync ? "<a title='"+Resource.lblAwaitSync+"(10)' href='javascript:void(0);' data-toggle='tooltip' onclick='ClickCronReturnJson(\""+c.Name+"\")'><i class='zmdi zmdi-refresh  btnIcon_orange '></i></a>" : "<a title='"+Resource.lblSynchronized+"' href='javascript:void(0);' data-toggle='tooltip'><i class='zmdi zmdi-refresh  btnIcon_green '></i></a>"
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
        [AllowAnonymous]
        public ActionResult ClickCron(string key)
        {

            var result = CallServicesReturnJson(key);
            if (result.Result)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Active, "Home/ClickCron",
                    result);
            }
            else
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Home/ClickCron",
                      result);

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<CronView> GetCron()
        {
            var cron = new CronView();
            var list = new List<CronView>();

            var trainees = EmployeeService.Get(a => a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse, true);
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_USER,
                IsSync = trainees.Any(a => a.LmsStatus == 1),
                count = trainees.Count(a => a.LmsStatus == 1),
                count_noreponse = trainees.Count(a => a.LmsStatus == 3),
            });
            var subjects = _repoSubjectService.GetSubjectDetailApi(a => (a.LmsStatus == null || a.LmsStatus == StatusModify || a.LmsStatus == statusIsNoResponse) && a.CourseTypeId.HasValue && a.CourseTypeId != 6);
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_SUBJECT,
                IsSync = subjects.Any(a => a.LmsStatus == 1),
                count = subjects.Count(a => a.LmsStatus == 1),
                count_noreponse = subjects.Count(a => a.LmsStatus == 3),
            });
            var departments = DepartmentService.ApiGet(a => a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse);
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_DEPARTMENT,
                IsSync = departments.Any(a => a.LmsStatus == 1),
                count = departments.Count(a => a.LmsStatus == 1),
                count_noreponse = departments.Count(a => a.LmsStatus == 3),
            });
            var jobtitles = _reJobtitleService.Get(a => a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse, true, true);
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_JOBTITLE,
                IsSync = jobtitles.Any(a => a.LmsStatus == 1),
                count = jobtitles.Count(a => a.LmsStatus == 1),
                count_noreponse = jobtitles.Count(a => a.LmsStatus == 3),
            });

            var groupCourse = _repoSubjectService.GetAPIGroupSubject(a => (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) && a.CAT_GROUPSUBJECT_ITEM.Any());
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_GET_LIST_CATEGORY,
                IsSync = groupCourse.Any(a => a.LmsStatus == 1),
                count = groupCourse.Count(a => a.LmsStatus == 1),
                count_noreponse = groupCourse.Count(a => a.LmsStatus == 3),
            });
            var program = CourseService.ApiGet(a => (a.LMSStatus == StatusModify || a.LMSStatus == null || a.LMSStatus == statusIsNoResponse) && a.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve));
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_PROGRAM,
                IsSync = program.Any(a => a.LMSStatus == 1),
                count = program.Count(a => a.LMSStatus == 1),
                count_noreponse = program.Count(a => a.LMSStatus == 3),
            });
            var courseOfProgram = CourseDetailService.GetAllApi(a => (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) && (a.Course.LMSStatus == StatusIsSync && a.Course.IsDeleted == false && a.Course.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve)) );
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_COURSE,
                IsSync = courseOfProgram.Any(a => a.LmsStatus == 1),
                count = courseOfProgram.Count(a => a.LmsStatus == 1),
                count_noreponse = courseOfProgram.Count(a => a.LmsStatus == 3),
            });
            var assignTrainee = CourseMemberService.GetApi(a => (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) &&
                                                                ((a.Trainee.LmsStatus == StatusIsSync || a.Trainee.LmsStatus == null) &&
                                                                 a.Trainee.IsDeleted != true ) &&
                                                                (a.Course_Detail.LmsStatus == StatusIsSync && 
                                                                 a.Course_Detail.IsDeleted != true) &&
                                                                a.Course_Detail.Course.IsDeleted != true);

            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_ASSIGN_TRAINEE,
                IsSync = assignTrainee.Any(a => a.LmsStatus == 1),
                count = assignTrainee.Count(a => a.LmsStatus == 1),
                count_noreponse = assignTrainee.Count(a => a.LmsStatus == 3),
            });

            var courseResult = _resultSummaryService.Get(a =>
                            a.Course_Detail.IsDeleted==false && a.Course_Detail.IsActive==true &&
                            (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) && a.Trainee.LmsStatus == StatusIsSync &&
                            a.Trainee.IsDeleted==false);
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY,
                IsSync = courseResult.Any(a => a.LmsStatus == 1),
                count = courseResult.Count(a => a.LmsStatus == 1),
                count_noreponse = courseResult.Count(a => a.LmsStatus == 3),
            });
            var certificateCategory = CourseService.GetCourseResultFinal(
                        a =>
                            (a.LmsStatus == StatusModify || a.LmsStatus == statusIsNoResponse || a.LmsStatus == null) && a.Course.LMSStatus == StatusIsSync &&
                            a.Course.IsDeleted == false && a.Trainee.LmsStatus == StatusIsSync && a.Trainee.IsDeleted == false);
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_GET_CERTIFICATE_CATEGORY,
                IsSync = certificateCategory.Any(a => a.LmsStatus == 1),
                count = certificateCategory.Count(a => a.LmsStatus == 1),
                count_noreponse = certificateCategory.Count(a => a.LmsStatus == 3),
            });
            var certificateCourse = CourseService.GetCourseResult(
                        a =>
                            (a.LmsStatus == StatusModify || a.LmsStatus == statusIsNoResponse || a.LmsStatus == null) && a.Course_Detail.LmsStatus == StatusIsSync &&
                            a.Course_Detail.IsDeleted == false && a.Trainee.LmsStatus == StatusIsSync && a.Trainee.IsDeleted == false && a.Course_Detail.Course.LMSStatus == StatusIsSync && a.Course_Detail.Course.IsDeleted == false && !string.IsNullOrEmpty(a.CertificateSubject));
            list.Add(new CronView()
            {
                Name = UtilConstants.CRON_GET_CERTIFICATE_COURSE,
                IsSync = certificateCourse.Any(a => a.LmsStatus == 1),
                count = certificateCourse.Count(a => a.LmsStatus == 1),
                count_noreponse = certificateCourse.Count(a => a.LmsStatus == 3),
            });
            return list;
        }

        private CronView GetCronByKey(string key)
        {
            var list = new CronView();
            switch (key)
            {
                case "CRON_GET_SURVEY_GLOBAL":
                    var surveys = ConfigService.GetSurvey_API(a => a.LMSStatus == StatusModify || a.LMSStatus == null || a.LMSStatus == statusIsNoResponse, true);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_SURVEY,
                        IsSync = surveys.Any(),
                        count = surveys.Count()
                    };
                    break;
                case "CRON_USER":
                    var trainees = EmployeeService.Get(a => a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse, true);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_USER,
                        IsSync = trainees.Any(),
                        count = trainees.Count()
                    };
                    break;
                case "CRON_SUBJECT":
                    var subjects = _repoSubjectService.GetSubjectDetailApi(a => (a.LmsStatus == null || a.LmsStatus == StatusModify || a.LmsStatus == statusIsNoResponse) && a.CourseTypeId.HasValue && a.CourseTypeId != 6);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_SUBJECT,
                        IsSync = subjects.Any(),
                        count = subjects.Count()
                    } ;
                    break;
                case "CRON_DEPARTMENT":
                    var departments = DepartmentService.ApiGet(a => a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_DEPARTMENT,
                        IsSync = departments.Any(),
                        count = departments.Count()
                    };
                    break;
                case "CRON_JOBTITLE":
                    var jobtitles = _reJobtitleService.Get(a => a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse, true, true);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_JOBTITLE,
                        IsSync = jobtitles.Any(),
                        count = jobtitles.Count()
                    };
                    break;
                case "CRON_GET_LIST_CATEGORY":
                    var groupCourse = _repoSubjectService.GetAPIGroupSubject(a => (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) && a.CAT_GROUPSUBJECT_ITEM.Any());
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_GET_LIST_CATEGORY,
                        IsSync = groupCourse.Any(),
                        count = groupCourse.Count()
                    };
                    break;
                case "CRON_PROGRAM":
                    var program = CourseService.ApiGet(a => (a.LMSStatus == StatusModify || a.LMSStatus == null || a.LMSStatus == statusIsNoResponse) && a.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve));
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_PROGRAM,
                        IsSync = program.Any(),
                        count = program.Count()
                    };
                    break;
                case "CRON_COURSE":
                    var courseOfProgram = CourseDetailService.GetAllApi(a => (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) && (a.Course.LMSStatus == StatusIsSync && a.Course.IsDeleted == false && a.Course.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve)));
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_COURSE,
                        IsSync = courseOfProgram.Any(),
                        count = courseOfProgram.Count()
                    };
                    break;
                case "CRON_ASSIGN_TRAINEE":
                    var assignTrainee = CourseMemberService.GetApi(a => (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) &&
                                                                 ((a.Trainee.LmsStatus == StatusIsSync || a.Trainee.LmsStatus == null) &&
                                                                  a.Trainee.IsDeleted == false) &&
                                                                 (a.Course_Detail.LmsStatus == StatusIsSync &&
                                                                  a.Course_Detail.IsDeleted == false) &&
                                                                 a.Course_Detail.Course.IsDeleted == false);

                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_ASSIGN_TRAINEE,
                        IsSync = assignTrainee.Any(),
                        count = assignTrainee.Count()
                    };
                    break;
                case "CRON_GET_COURSE_RESULT_SUMMARY":
                    var courseResult = _resultSummaryService.Get(a =>
                            a.Course_Detail.IsDeleted == false && a.Course_Detail.IsActive == true &&
                            (a.LmsStatus == StatusModify || a.LmsStatus == null || a.LmsStatus == statusIsNoResponse) && a.Trainee.LmsStatus == StatusIsSync &&
                            a.Trainee.IsDeleted == false);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_GET_COURSE_RESULT_SUMMARY,
                        IsSync = courseResult.Any(),
                        count = courseResult.Count()
                    };
                    break;
                case "CRON_GET_CERTIFICATE":
                    var certificate = CourseService.GetCourseResultFinal(a =>
                          (a.LmsStatus == StatusModify || a.LmsStatus == statusIsNoResponse) && a.Course.LMSStatus == StatusIsSync &&
                          a.Course.IsDeleted == false && a.Trainee.LmsStatus == StatusIsSync && a.Trainee.IsDeleted == false);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_GET_CERTIFICATE,
                        IsSync = certificate.Any(),
                        count = certificate.Count()
                    };
                    break;
                case "CRON_GET_CERTIFICATE_CATEGORY":
                    var certificateCategory = CourseService.GetCourseResultFinal(
                      a =>
                          (a.LmsStatus == StatusModify || a.LmsStatus == statusIsNoResponse || a.LmsStatus == null) && a.Course.LMSStatus == StatusIsSync &&
                          a.Course.IsDeleted == false && a.Trainee.LmsStatus == StatusIsSync && a.Trainee.IsDeleted == false);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_GET_CERTIFICATE_CATEGORY,
                        IsSync = certificateCategory.Any(),
                        count = certificateCategory.Count()
                    };
                    break;
                case "CRON_GET_CERTIFICATE_COURSE":
                    var certificateCourse = CourseService.GetCourseResult(
                       a =>
                           (a.LmsStatus == StatusModify || a.LmsStatus == statusIsNoResponse || a.LmsStatus == null) && a.Course_Detail.LmsStatus == StatusIsSync &&
                           a.Course_Detail.IsDeleted == false && a.Trainee.LmsStatus == StatusIsSync && a.Trainee.IsDeleted == false && a.Course_Detail.Course.LMSStatus == StatusIsSync && a.Course_Detail.Course.IsDeleted == false);
                    list = new CronView()
                    {
                        Name = UtilConstants.CRON_GET_CERTIFICATE_COURSE,
                        IsSync = certificateCourse.Any(),
                        count = certificateCourse.Count()
                    };
                    break;
            }
            return list;
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult ClickCronAllByKey(string key)
        {
            var check = CallServicesReturnJson(key);
            if (check.Result)
            {
                var list = GetCronByKey(key);
                var result = new AjaxResponseViewModel()
                {
                    result = true,
                    count = list.count,
                    message = Messege.SUCCESS
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var result = new AjaxResponseViewModel()
                {
                    result = false,
                    count = 0,
                    message = Messege.UNSUCCESS
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
    }
}