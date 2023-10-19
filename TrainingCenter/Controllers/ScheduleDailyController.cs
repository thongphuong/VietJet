using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
//using TrainingCenter.Serveices;
using System.Data;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Drawing;
using System.Text;
using TrainingCenter.Utilities;
using System.Linq.Expressions;
using System.Globalization;
using System.IO;
using System.Linq.Dynamic;
using System.Net;
using System.util;
using System.Web.Helpers;
using System.Web.UI;
using TMS.Core.App_GlobalResources;
using DataTable = System.Data.DataTable;
using DAL.Entities;

using Microsoft.Ajax.Utilities;
using RestSharp.Extensions;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Companies;
using TMS.Core.Services.Cost;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Subject;
using TMS.Core.Services.Users;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseResultSummary;
using TMS.Core.Services.Orientation;
using TMS.Core.ViewModels.AjaxModels.AjaxAssignMember;
using TMS.Core.ViewModels.AjaxModels.AjaxCourse;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.ReportModels;
using System.Web.Script.Serialization;

namespace TrainingCenter.Controllers
{
    using System.Data.Entity.SqlServer;
    using DAL.UnitOfWork;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Courses;
    using TMS.Core.ViewModels.ViewModel;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.Configuration;
    using Newtonsoft.Json.Linq;
    using System.Data.Entity;
    using Newtonsoft.Json;
    using System.Web;
    using RestSharp;
    using TMS.Core.ViewModels.Subjects;
    using System.Threading.Tasks;
    using System.Text.RegularExpressions;

    public class ScheduleDailyController : BaseAdminController
    {
        private readonly IDepartmentService _repoDepartment;
        private readonly ISubjectService _repoSubject;
        private readonly ICostService _courseServiceCost;
        private readonly IApproveService _repoTmsApproves;
        private readonly IJobtitleService _repoJobTiltle;
        private readonly ICompanyService _repoCompany;
        private readonly IUserService _repoUser;
        private readonly ICourseDetailService _courseDetailService;

        private readonly ICourseResultSummaryService _repoCourseResultSummaryService;
        private readonly IOrientationService _repoOrientationService;
        private readonly ICourseDetailService _repoCourseDetail;
        private readonly IEmployeeService _repoEmployeeService;
        private readonly ICourseService _repoCourse;
        public static string ConfigDayShow = ConfigurationManager.AppSettings["ConfigDayShow"];
        // GET: ScheduleDaily
        public ScheduleDailyController(IConfigService configService,
            IUserContext userContext,
            INotificationService notificationService,
            ICourseMemberService courseMemberService,
            IEmployeeService employeeService,
            ICourseDetailService courseDetailService,
            ICourseService repoCourse,
            IDepartmentService repoDepartment,

            ISubjectService repoSubject,
            ICostService repoCourseCost,
            IJobtitleService repoJobTiltle,
            IApproveService repoTmsApproves,
            ICompanyService repoCompany,
            IUserService repoUser,
            ICourseResultSummaryService repoCourseResultSummaryService,
            IOrientationService repoOrientationService,
            IEmployeeService repoEmployeeService,
            ICourseDetailService repoCourseDetail
            ) : base(configService,
                userContext,
                notificationService,
                courseMemberService,
                employeeService,
                courseDetailService,
                repoDepartment,
                repoCourse,
                repoTmsApproves)
        {
            _courseDetailService = courseDetailService;
            _repoDepartment = repoDepartment;
            _repoSubject = repoSubject;
            _courseServiceCost = repoCourseCost;
            _repoJobTiltle = repoJobTiltle;
            _repoTmsApproves = repoTmsApproves;
            _repoCompany = repoCompany;
            _repoUser = repoUser;
            //_configService = configService;
            _repoCourseResultSummaryService = repoCourseResultSummaryService;
            _repoOrientationService = repoOrientationService;
            _repoCourseDetail = repoCourseDetail;
            _repoEmployeeService = repoEmployeeService;
            _repoCourse = repoCourse;
        }
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
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");

                var _ConfigDayShow = string.IsNullOrEmpty(ConfigDayShow) ? 1 : Convert.ToInt32(ConfigDayShow);
                var datefrom = DateTime.Now.Date;
                DateTime datetotal;
                var dates = new List<DateTime>();
                if (_ConfigDayShow > 1)
                {
                    datetotal = datefrom.Date.AddDays(_ConfigDayShow - 1);
                    for (var dt = datefrom; dt <= datetotal; dt = dt.AddDays(1))
                    {
                        dates.Add(dt.Date);
                    }
                }
                else
                {
                    dates.Add(datefrom);
                }
                var dateto = dates.LastOrDefault();
                var data = CourseDetailService.GetAllApi(a=>a.IsDeleted != true && SqlFunctions.DateDiff("day", datefrom, a.dtm_time_from) >= 0 && SqlFunctions.DateDiff("day", a.dtm_time_from, dateto) >= 0 && dates.Any(x => SqlFunctions.DateDiff("day", x, a.dtm_time_from) >= 0 && SqlFunctions.DateDiff("day", a.dtm_time_to, x) <= 0));
                var selectlist = data.Select(a => a.dtm_time_from);
                IEnumerable<Course_Detail> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Course.Code
                                                            : sortColumnIndex == 2 ? c.SubjectDetail.Name
                                                            : sortColumnIndex == 3 ? (object)c.dtm_time_from
                                                            : sortColumnIndex == 4 ? (object)c.time_from
                                                            : (object)c.dtm_time_from);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = "";//UtilConstants.VerticalBar");
                var result = from c in displayed
                             let room = c.Course_Detail_Room.FirstOrDefault(a=>a.CourseDetailID == c.Id && c.dtm_time_from.Value.Date == a.LearningDate.Value.Date)
                             let instructor = string.Join("<br /> ", c.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).OrderBy(a => a.Trainee.FirstName).Select(a => ReturnDisplayLanguage(a.Trainee.FirstName, a.Trainee.LastName)).ToArray())
                             select new object[] {
                                    string.Empty,
                                    c.Course.Code,
                                    c.SubjectDetail.Name,
                                     room != null ? (room.RoomId.HasValue ? room.Room.str_Name : room.RoomOther) : "",
                                    c.dtm_time_from.HasValue ? c.dtm_time_from.Value.ToString("dd/MM/yyyy") : "",
                                    (c.time_from!= null ? c.time_from.ToString().Substring(0, 2) + c.time_from.ToString().Substring(2) : null) + " - " + (c.time_to != null ? c.time_to.ToString().Substring(0, 2) +  c.time_to.ToString().Substring(2) : null),
                                   
                                    instructor,
                                    c.SubjectDetail.Duration,
                                    string.IsNullOrEmpty(c.str_remark) ? "" : Regex.Replace(c.str_remark, "[\r\n]", "<br/>"),

                             };


                var jsonResult = Json(new
                {
                    param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandler", ex.Message);
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