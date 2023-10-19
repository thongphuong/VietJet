﻿using DAL.Entities;
using Microsoft.Reporting.WebForms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.Cost;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Subject;
using TMS.Core.Services.Users;
using TMS.Core.ViewModels.Common;
using TrainingCenter.Template.Report;
using TrainingCenter.Utilities;
using TMS.Core.Services.Jobtitle;

namespace TrainingCenter.Controllers
{
    using DAL.Repositories;
    using global::Utilities;
    using Microsoft.Ajax.Utilities;
    using System.Drawing;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using TMS.Core.Services.Department;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.ReportModels;
    using TMS.Core.ViewModels.ViewModel;

    public class ReportController : BaseAdminController
    {
        #region Variables
        private const string REPORT_PDF = "PDF";
        private const string REPORT_EXCEL = "EXCELOPENXML";

        private readonly ICourseDetailService _repoCourseServiceDetail;
        private readonly ICostService _repoCourseServiceCost;
        private readonly IApproveService _repoTmsApproves;
        private readonly ICourseMemberService _courseServiceMember;
        private readonly IEmployeeService _repoEmployee;
        private readonly ISubjectService _repoSubject;
        private readonly ICourseService _CourseService;
        private readonly ICourseDetailService _CourseDetailService;
        private readonly IRepository<Course> _repoCourse = null;
        private readonly IJobtitleService _repoJobTiltle;
        public ReportController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ICourseDetailService repoCourseDetail, ICostService repoCourseCost, IApproveService repoTmsApproves, ICourseMemberService repoCourseMember, IEmployeeService repoEmployee, ISubjectService repoSubject, ICourseDetailService CourseDetailService, IRepository<Course> repoCourse, IJobtitleService repoJobTiltle) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, repoTmsApproves)
        {
            _repoCourseServiceDetail = repoCourseDetail;
            _repoCourseServiceCost = repoCourseCost;
            _repoTmsApproves = repoTmsApproves;
            _courseServiceMember = repoCourseMember;
            _repoEmployee = repoEmployee;
            _repoSubject = repoSubject;
            _CourseService = CourseService;
            _CourseDetailService = CourseDetailService;
            _repoCourse = repoCourse;
            _repoJobTiltle = repoJobTiltle;
        }
        #endregion
        public ActionResult Index()
        {

            return View();
        }

        #region [cost] 
        public ActionResult Cost()
        {
            return View();
        }
        #region[Chart]
        public class SeriesItem
        {
            public string name { get; set; }
            public float[] data { get; set; }
        }
        public class SeriesChartDept
        {
            public string id { get; set; }
            public string name { get; set; }
            public bool colorByPoint { get; set; }
            public SeriesChartDeptData[] data { get; set; }

        }
        public class SeriesChartDeptData
        {
            public string name { get; set; }
            public int y { get; set; }
            public string drilldown { get; set; }
        }

        private IEnumerable<DateTime> Diff(int type, DateTime filtertodate, DateTime filterfromdate)
        {
            var start = new DateTime();
            start = filterfromdate;
            var end = filtertodate;
            end = new DateTime(end.Year, end.Month, DateTime.DaysInMonth(end.Year, end.Month));
            var diff =
                (type == 1) ?
                (Enumerable.Range(0, int.MaxValue)
                .Select(e => start.AddDays(e))
                .TakeWhile(e => e <= end)
                .Select(e => e)) :
                (type == 2) ?
                (Enumerable.Range(0, int.MaxValue)
                .Select(e => start.AddMonths(e))
                .TakeWhile(e => e <= end)
                .Select(e => e)) :
                (Enumerable.Range(0, int.MaxValue)
                .Select(e => start.AddYears(e))
                .TakeWhile(e => e <= end)
                .Select(e => e));
            return diff;
        }

        public JsonResult ChartCost(int type, DateTime filtertodate, DateTime filterfromdate)
        {
            var diff = Diff(type, filtertodate, filterfromdate);

            //Get course approval
            var datacourseapproval = CourseService.Get().Select(a => a.Id);
            var dataGroupCost = _repoCourseServiceCost.GetGroupCost();
            var datacoursedetail = _repoCourseServiceDetail.Get(a => datacourseapproval.Contains(a.Course.Id));
            float Totalcost = 0;
            var series = new List<SeriesItem>();
            if (dataGroupCost.Any())
            {

                //For Groupcourse
                foreach (var itemGroupCost in dataGroupCost)
                {
                    var costArrayList = new List<float>();

                    foreach (var itemtimecost in diff)
                    {
                        var dataCourseDetail = datacoursedetail.Where(a =>
                               ((type == 1) ?
                         (a.dtm_time_from.Value.Day <= itemtimecost.Day && itemtimecost.Day <= a.dtm_time_to.Value.Day
                         && a.dtm_time_from.Value.Month <= itemtimecost.Month && itemtimecost.Month <= a.dtm_time_to.Value.Month
                         && a.dtm_time_from.Value.Year <= itemtimecost.Year && itemtimecost.Year <= a.dtm_time_to.Value.Year) :
                         ((type == 2) ?
                         (a.dtm_time_from.Value.Month <= itemtimecost.Month && itemtimecost.Month <= a.dtm_time_to.Value.Month
                         && a.dtm_time_from.Value.Year <= itemtimecost.Year && itemtimecost.Year <= a.dtm_time_to.Value.Year) :
                        (a.dtm_time_from.Value.Year <= itemtimecost.Year && itemtimecost.Year <= a.dtm_time_to.Value.Year)))
                        &&
                        (itemGroupCost.Code == "GC001" ? a.Course_Detail_Instructor.Any() : true)
                           );
                        float cost = 0;
                        foreach (var item in dataCourseDetail)
                        {
                            cost += CostByTime(item, itemtimecost, itemGroupCost);

                        }
                        costArrayList.Add(cost);
                        Totalcost += cost;
                    }
                    float[] arr = costArrayList.ToArray();
                    series.Add(new SeriesItem { name = itemGroupCost.Name, data = arr });
                }
            }



            var pointStartY = filterfromdate.Year;
            var pointStartM = filterfromdate.Month;
            var pointStartD = filterfromdate.Day;
            var pointIntervalUnit = "";
            var subtitle = "";
            var totalcostconvert = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", Totalcost);
            switch (type)
            {
                case 1:
                    pointIntervalUnit = "day";
                    subtitle = filterfromdate.ToString("dd/MM/yyyy") + " - " + filtertodate.ToString("dd/MM/yyyy") + " Total cost: " + totalcostconvert;
                    break;
                case 2:
                    pointStartD = 1;
                    pointIntervalUnit = "month";
                    subtitle = filterfromdate.ToString("MM/yyyy") + " - " + filtertodate.ToString("MM/yyyy") + " Total cost: " + totalcostconvert;
                    break;
                case 3:
                    pointStartD = 1;
                    pointStartM = 1;
                    pointIntervalUnit = "year";
                    subtitle = filterfromdate.ToString("yyyy") + " - " + filtertodate.ToString("yyyy") + " Total cost: " + totalcostconvert;
                    break;
            }

            return Json(new
            {
                series,
                pointStartY,
                pointStartM,
                pointStartD,
                pointIntervalUnit,
                subtitle
            }, JsonRequestBehavior.AllowGet);
        }
        private static float CostByTime(Course_Detail itemCourseDetail, DateTime time, CAT_GROUPCOST type = null, bool chartdept = false)
        {
            float totalcost = 0;
            float costoneday = 0;

            DateTime courseStart = (DateTime)itemCourseDetail.dtm_time_from;
            DateTime courseEnd = (DateTime)itemCourseDetail.dtm_time_to;
            var firstDayOfMonth = new DateTime(time.Year, time.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            // get cost one day
            var span = courseEnd.Subtract(courseStart);

            var isAllowanceInstructor = false;
            var isCostPerPerson = false;
            if (chartdept)
            {
                var cost = itemCourseDetail.Course_Cost.Where(a => a.IsDeleted == false).Sum(a => a.cost);
                if (cost != null) costoneday = span.Days != 0 ? (float)cost / span.Days : (float)cost;
                isAllowanceInstructor = true;
                isCostPerPerson = true;

            }
            else
            {
                var cost = itemCourseDetail.Course_Cost.Where(a => type != null && a.IsDeleted == false && a.CAT_COSTS.CAT_GROUPCOST.Id == type.Id).Sum(a => a.cost);
                if (cost != null) costoneday = span.Days != 0 ? (float)cost / span.Days : (float)cost;
                switch (type.Code)
                {
                    case "GC001":
                        isAllowanceInstructor = true;
                        break;
                    case "GC002":
                        isCostPerPerson = true;
                        break;
                }
            }

            if (isAllowanceInstructor)
            {
                var sunDuration = itemCourseDetail.Course_Detail_Instructor
                    .Where(t => t.Trainee.bit_Internal == true && t.Duration.HasValue)
                    .Sum(a => a.Duration);
                var sumAllowance = itemCourseDetail.Course_Detail_Instructor
                    .Where(t => t.Trainee.bit_Internal == true && t.Duration.HasValue)
                    .Sum(a => a.Allowance);
                if (sumAllowance != null)
                {
                    var ccc = sunDuration * (float)sumAllowance;
                    if (ccc != null) costoneday += span.Days != 0 ? (float)ccc / span.Days : (float)ccc;
                }
            }
            if (isCostPerPerson)
            {
                //TODO: son
            }

            if (chartdept)
            {
                var deptInCourse = itemCourseDetail.Course.Course_TrainingCenter.Count;
                costoneday = costoneday / deptInCourse;
            }


            if (courseStart <= firstDayOfMonth
                && lastDayOfMonth <= courseEnd)
            {
                var th1 = lastDayOfMonth.Subtract(firstDayOfMonth);
                totalcost += costoneday * th1.Days;
            }
            else if (firstDayOfMonth <= courseStart
                && courseEnd <= lastDayOfMonth)
            {
                var th2 = courseEnd.Subtract(firstDayOfMonth);
                totalcost += costoneday * th2.Days;
            }
            else if (courseStart < firstDayOfMonth && firstDayOfMonth < courseEnd && firstDayOfMonth < courseEnd && courseEnd < lastDayOfMonth
                )
            {
                var th3 = courseEnd.Subtract(firstDayOfMonth);
                totalcost += costoneday * th3.Days;
            }
            else if (courseStart <= lastDayOfMonth || lastDayOfMonth <= courseEnd || firstDayOfMonth <= courseStart ||
                courseStart <= lastDayOfMonth)
            {
                var th4 = lastDayOfMonth.Subtract(courseStart);
                totalcost += costoneday * th4.Days;
            }

            return totalcost;
        }

        private float totalcostdep = 0;
        private int CostByDept(IEnumerable<DateTime> diff, IEnumerable<int> deptlist, int type, DateTime filtertodate, DateTime filterfromdate)
        {
            float cost = 0;
            var datacourseapproval = CourseService.Get(a => a.Course_TrainingCenter.Any(c => deptlist.Contains((int)c.khoidaotao_id))).Select(a => a.Id);
            var dataCourseDetail = _repoCourseServiceDetail.Get(a =>
                    datacourseapproval.Contains((int)a.Course.Id)
                );

            //(int)Math.Ceiling(cost)
            foreach (var item in dataCourseDetail)
            {
                foreach (var itemtimecost in diff)
                {
                    switch (type)
                    {
                        case 1:
                            if ((item.dtm_time_from.Value.Day <= itemtimecost.Day && itemtimecost.Day <= item.dtm_time_to.Value.Day
                && item.dtm_time_from.Value.Month <= itemtimecost.Month && itemtimecost.Month <= item.dtm_time_to.Value.Month
                && item.dtm_time_from.Value.Year <= itemtimecost.Year && itemtimecost.Year <= item.dtm_time_to.Value.Year))
                            {
                                cost += CostByTime(item, itemtimecost, null, true);
                            }
                            break;
                        case 2:
                            if ((item.dtm_time_from.Value.Month <= itemtimecost.Month && itemtimecost.Month <= item.dtm_time_to.Value.Month
                && item.dtm_time_from.Value.Year <= itemtimecost.Year && itemtimecost.Year <= item.dtm_time_to.Value.Year))
                            {
                                cost += CostByTime(item, itemtimecost, null, true);
                            }
                            break;
                        case 3:
                            if (item.dtm_time_from.Value.Year <= itemtimecost.Year && itemtimecost.Year <= item.dtm_time_to.Value.Year)
                            {
                                cost += CostByTime(item, itemtimecost, null, true);
                            }
                            break;
                    }






                }
            }
            totalcostdep += cost;
            return (int)Math.Ceiling(cost);
        }
        public JsonResult ChartCostDepartment(int type, DateTime filtertodate, DateTime filterfromdate)
        {
            totalcostdep = 0;
            float totalcost = 0;
            var diff = Diff(type, filtertodate, filterfromdate);
            #region [series]
            var series = new SeriesChartDept
            {
                id = "root",
                colorByPoint = true,
                name = "root"
            };
            var serieslistdata = new List<SeriesChartDeptData>();
            var datadept = DepartmentService.Get();
            if (datadept.Any())
            {
                //For Groupcourse
                foreach (var itemGroupCost in datadept.Where(a => a.ParentId == null))
                {
                    var list = datadept.Where(a => a.Ancestor.StartsWith(itemGroupCost.Ancestor)).Select(a => a.Id);
                    serieslistdata.Add(new SeriesChartDeptData() { name = itemGroupCost.Name, y = CostByDept(diff, list, type, filtertodate, filterfromdate), drilldown = (itemGroupCost.Department1.Any() ? itemGroupCost.Id.ToString() : null) });
                }
                series.data = serieslistdata.ToArray();
            }
            #endregion
            totalcost = totalcostdep;
            #region [drilldown]
            var drilldown = new List<SeriesChartDept>();
            drilldown = GenerateDrilldown(diff, drilldown, datadept, type, filtertodate, filterfromdate);
            #endregion
            var subtitle = "";
            var totalcostconvert = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", totalcost);
            switch (type)
            {
                case 1:
                    subtitle = filterfromdate.ToString("dd/MM/yyyy") + " - " + filtertodate.ToString("dd/MM/yyyy") + " Total cost: " + totalcostconvert;
                    break;
                case 2:
                    subtitle = filterfromdate.ToString("MM/yyyy") + " - " + filtertodate.ToString("MM/yyyy") + " Total cost: " + totalcostconvert;
                    break;
                case 3:
                    subtitle = filterfromdate.ToString("yyyy") + " - " + filtertodate.ToString("yyyy") + " Total cost: " + totalcostconvert;
                    break;
            }

            return Json(new
            {
                series,
                drilldown,
                subtitle
            }, JsonRequestBehavior.AllowGet);
        }
        private List<SeriesChartDept> GenerateDrilldown(IEnumerable<DateTime> diff, List<SeriesChartDept> drilldown, IEnumerable<Department> datadept, int type, DateTime filtertodate, DateTime filterfromdate)
        {
            var listid = new List<int>();
            foreach (var itemGroupCost in datadept)
            {
                var drilldownlistdata = new List<SeriesChartDeptData>();
                var datadeptchild = datadept.Where(a => a.ParentId == itemGroupCost.Id);
                if (datadeptchild.Any())
                {
                    foreach (var itemGroupCostChild in datadeptchild)
                    {
                        listid.Add(itemGroupCostChild.Id);
                        var list = datadept.Where(a => a.Ancestor.StartsWith(itemGroupCostChild.Ancestor)).Select(a => a.Id);
                        drilldownlistdata.Add(new SeriesChartDeptData { name = itemGroupCostChild.Name, y = CostByDept(diff, list, type, filtertodate, filterfromdate), drilldown = itemGroupCostChild.Department1.Any() ? itemGroupCostChild.Id.ToString() : null });
                    }
                }
                drilldown.Add(new SeriesChartDept { id = itemGroupCost.Id.ToString(), colorByPoint = true, name = itemGroupCost.Name, data = drilldownlistdata.ToArray() });
            }
            if (listid.Count <= 0) return drilldown;
            {
                var datadeptchild = datadept.Where(a => listid.Contains(a.Id));
                drilldown = GenerateDrilldown(diff, drilldown, datadeptchild, type, filtertodate, filterfromdate);
            }
            return drilldown;
        }
        #endregion
        public ActionResult AjaxHandlerCost(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var name = string.IsNullOrEmpty(Request.QueryString["coursename"]) ? string.Empty : Request.QueryString["coursename"].Trim();
                var code = string.IsNullOrEmpty(Request.QueryString["coursecode"]) ? string.Empty : Request.QueryString["coursecode"].Trim();
                string searchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
                string searchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();

                DateTime dateFrom;
                DateTime dateTo;

                DateTime.TryParse(searchDateFrom, out dateFrom);
                DateTime.TryParse(searchDateTo, out dateTo);

                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                //var data = ctx.sp_GetCostHeader(lst, FromDate_from, ToDate_from, FromDate_to, ToDate_to).ToList<sp_GetCostHeader_Result>();
                var course = _repoCourse.GetAll(a => a.IsDeleted != true &&
                    (string.IsNullOrEmpty(name) || a.Name.Contains(name)) &&
                    (string.IsNullOrEmpty(code) || a.Code.Contains(code)) &&
                    (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                    (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) &&
                 a.TMS_APPROVES.FirstOrDefault(z => z.int_Type == (int)UtilConstants.ApproveType.Course).int_id_status == (int)UtilConstants.EStatus.Approve).ToList();

                List<Course> list_course = new List<Course>();
                foreach (var item in course)
                {
                    var item_ = item.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.Course);
                    if (item_ != null)
                    {
                        if (item_.int_id_status == (int)UtilConstants.EStatus.Approve)
                        {
                            list_course.Add(item);
                        }
                    }
                }
                var data = list_course.Select(a => new CourseCostReportViewModel()
                {
                    Cost = decimal.Parse(a.Course_Detail_Instructor.Where(x => x.Type == (int)UtilConstants.TypeInstructor.Instructor && x.Course_Id == a.Id && x.Course?.TMS_APPROVES?.LastOrDefault(z => z.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?.int_id_status == (int)UtilConstants.EStatus.Approve).GroupBy(x => x.Instructor_Id).Select(g => new
                    {
                        Allowance = (decimal.Parse(g.Where(t => t.Duration.HasValue).Sum(x => x.Duration).ToString()) * g.FirstOrDefault()?.Allowance)
                    }).Sum(b => b.Allowance).ToString()),
                    Course_Id = a.Id,
                }).ToList();
                data = list_course.Select(a => new CourseCostReportViewModel()
                {
                    Cost = (data.Any(x => x.Course_Id == a.Id) ? data.Where(x => x.Course_Id == a.Id).Sum(x => x.Cost) : 0) + a.Course_Cost.Where(x => x.IsDeleted == false).Sum(x => x.cost),
                    Course_Id = a.Id,
                    dtm_StartDate = a.StartDate,
                    dtm_EndDate = a.EndDate,
                    str_Name = a.Name,
                    CourseCode = a.Code
                    //ExpectedCost = a.Course_Cost?.OrderByDescending(b => b.id)?.FirstOrDefault()?.ExpectedCost
                }).ToList();
                IEnumerable<CourseCostReportViewModel> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CourseCostReportViewModel, object> orderingFunction = (c => sortColumnIndex == 2 ? c?.str_Name
                                                          : sortColumnIndex == 3 ? c?.dtm_StartDate
                                                          : sortColumnIndex == 4 ? (object)c?.Cost
                                                          : c.dtm_StartDate);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = sortDirection == "asc" ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                c?.Course_Id,
                                c?.CourseCode,
                                "<span data-value='"+c?.Course_Id+"' class='expand' style='cursor: pointer;'><a>"+c?.str_Name+"</a></span>",
                                DateUtil.DateToString(c?.dtm_StartDate,"dd/MM/yyyy") +" - "+ DateUtil.DateToString(c?.dtm_EndDate,"dd/MM/yyyy"),
                                string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", c?.Cost),
                                //string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", c?.ExpectedCost)
                        };
                var jsonresult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonresult.MaxJsonLength = int.MaxValue;
                return jsonresult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlerCost", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CostPrint(/*int id = 0*/)
        {
            var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course);
            // xử lý param gửi lên 
            var name = string.IsNullOrEmpty(Request.QueryString["coursename"]) ? string.Empty : Request.QueryString["coursename"].Trim();
            var code = string.IsNullOrEmpty(Request.QueryString["coursecode"]) ? string.Empty : Request.QueryString["coursecode"].Trim();
            string fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["dateFrom"]) ? string.Empty : Request.QueryString["dateFrom"].Trim();
            string fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["dateTo"]) ? string.Empty : Request.QueryString["dateTo"].Trim();
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFrom, out dateFrom);
            DateTime.TryParse(fSearchDateTo, out dateTo);
            dateFrom = dateFrom.Date;
            dateTo = dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            System.Linq.Expressions.Expression<Func<Course_Cost, bool>> expression = (a =>
                     (string.IsNullOrEmpty(name) || a.Course.Name.Contains(name)) &&
                     (string.IsNullOrEmpty(code) || a.Course.Code.Contains(code)) &&
                     (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.Course.StartDate) >= 0) &&
                     (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.Course.EndDate, dateTo) >= 0));
            var courseCost = _repoCourseServiceCost.GetCourseCost(expression).ToList().Where(a => a.IsDeleted == false &&
            dataCourseProccess.LastOrDefault(x => x.Course == a.Course)?.int_id_status == (int)UtilConstants.EStatus.Approve).ToList();
            var model = new CostModelRp()
            {
                CourseCosts = courseCost

            };

            return PartialView("CostPrint", model);
        }
        [HttpPost]
        public FileContentResult ExportCost(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            //var lst = string.IsNullOrEmpty(Request.QueryString["id_course"]) ? "" : Request.QueryString["id_course"].ToString(); 
            var lst = string.IsNullOrEmpty(form["id[]"]) ? string.Empty : form["id[]"].Trim();

            var Name = string.IsNullOrEmpty(Request["coursename"]) ? string.Empty : Request["coursename"].Trim();
            var Code = string.IsNullOrEmpty(Request["coursecode"]) ? string.Empty : Request["coursecode"].Trim();
            var searchDateFrom = string.IsNullOrEmpty(Request["fSearchDate_from"]) ? "" : Request["fSearchDate_from"].Trim();
            var searchDateTo = string.IsNullOrEmpty(Request["fSearchDate_to"]) ? "" : Request["fSearchDate_to"].Trim();

            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(searchDateFrom, out dateFrom);
            DateTime.TryParse(searchDateTo, out dateTo);
            //dateFrom = dateFrom.Date;
            //dateTo = dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            byte[] filecontent = ExportExcelCost(Name, Code, dateFrom, dateTo);
            return File(filecontent, ExportUtils.ExcelContentType, "Cost.xlsx");
        }
        private byte[] ExportExcelCost(string Name, string Code, DateTime? dateFrom, DateTime? dateTo)
        {
            //return null;
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/Cost.xlsx");
            FileInfo template = new FileInfo(templateFilePath);

            var Courses = CourseService.Get(a => ((string.IsNullOrEmpty(Name) || a.Name.Contains(Name)) && (string.IsNullOrEmpty(Code) || a.Code.Contains(Code)) && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0)));
            //var cat_cost = _repoCourseServiceCost.Get(p => p.IsDeleted == false && p.IsActive == true);

            ExcelPackage excelPackage;
            MemoryStream memoryStream = new MemoryStream();
            byte[] bytes = null;
            using (excelPackage = new ExcelPackage(template, false))
            {
                var worksheet = excelPackage.Workbook.Worksheets["Sheet1"];
                int starRow = 6;
                int starCol = 0;
                int count = 0;
                int starRowPlus = 5;
                int starRowCost = 2;
                int startColCost = 12;
                int countRow = 0;
                decimal? Total = 0;
                int TotalAll = 0;
                var coltotal = starCol + starRowCost + 12;
                var starRowCostplus = 1;
                decimal? total_internal = 0;
                decimal? total_external = 0;
                var count_num_cost = Courses.Max(a => a.Course_Cost.Count(b => b.IsActive == true && b.IsDeleted == false));
                var listCATCOST = new List<int?>();
                foreach (var item in Courses.Where(x => x.Course_Cost.Any(a => a.IsActive == true && a.IsDeleted == false)))
                {
                    listCATCOST.AddRange(item.Course_Cost.Select(a => a.cost_id));
                }
                listCATCOST = listCATCOST.Distinct().ToList();
                Dictionary<int, string> catcostname = new Dictionary<int, string>();
                Dictionary<int, decimal?> total_cost = new Dictionary<int, decimal?>();
                foreach (var item in Courses)
                {
                    Total = 0;
                    count++;
                    ExcelRange cellNo = worksheet.Cells[starRow + countRow, starCol + starRowCost];
                    cellNo.Value = count;
                    cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellCode = worksheet.Cells[starRow + countRow, starCol + starRowCost + 1];
                    cellCode.Value = item.Code;
                    cellCode.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellName = worksheet.Cells[starRow + countRow, starCol + starRowCost + 2, starRow + countRow, starCol + starRowCost + 3];
                    cellName.Merge = true;
                    cellName.Value = item.Name;
                    cellName.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    ExcelRange cellRoom = worksheet.Cells[starRow + countRow, starCol + starRowCost + 4];
                    cellRoom.Value = item.Course_Detail?.FirstOrDefault()?.Room?.str_Name;
                    cellRoom.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellRoom.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellRoom.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celTypeOfTraining = worksheet.Cells[starRow + countRow, starCol + starRowCost + 5];
                    celTypeOfTraining.Value = GetCourseType(item.CourseTypeId);
                    celTypeOfTraining.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celTypeOfTraining.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celTypeOfTraining.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celDept = worksheet.Cells[starRow + countRow, starCol + starRowCost + 6];
                    celDept.Value = item.Course_TrainingCenter?.FirstOrDefault()?.Department?.Code;
                    celDept.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celCustomer = worksheet.Cells[starRow + countRow, starCol + starRowCost + 7];
                    celCustomer.Value = item.CustomerType == true ? "Internal" : "External";
                    celCustomer.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celCustomer.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celCustomer.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celVenue = worksheet.Cells[starRow + countRow, starCol + starRowCost + 8];
                    celVenue.Value = item.Venue;
                    celVenue.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celVenue.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celVenue.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celDateFrom = worksheet.Cells[starRow + countRow, starCol + starRowCost + 9];
                    celDateFrom.Value = item.StartDate.Value.ToString("dd/MM/yyyy") ?? "";
                    celDateFrom.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celDateFrom.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celDateFrom.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celDateTo = worksheet.Cells[starRow + countRow, starCol + starRowCost + 10];
                    celDateTo.Value = item.EndDate.Value.ToString("dd/MM/yyyy") ?? "";
                    celDateTo.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celDateTo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celDateTo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    var instructor = item.Course_Detail_Instructor.Where(x => x.Course_Id == item.Id && x.Course?.TMS_APPROVES.LastOrDefault(z => z.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?.int_id_status == (int)UtilConstants.EStatus.Approve).GroupBy(a => a.Instructor_Id);

                    ExcelRange cellCostNameInstructor1 = worksheet.Cells[starRow + countRow, starCol + starRowCost + 11];
                    var costInternal = decimal.Parse(instructor.Select(g => new Course_Detail_Instructor
                    {
                        Allowance = (decimal.Parse(g.Where(t => t.Trainee.bit_Internal == true && t.Duration.HasValue).Sum(a => a.Duration).ToString()) * g.FirstOrDefault()?.Allowance)
                    }).Sum(b => b.Allowance).ToString());
                    cellCostNameInstructor1.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", costInternal);
                    cellCostNameInstructor1.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellCostNameInstructor1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellCostNameInstructor1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellCostNameInstructor1.AutoFitColumns();

                    Total += costInternal;
                    total_internal += costInternal;

                    ExcelRange cellCostNameInstructor2 = worksheet.Cells[starRow + countRow, starCol + starRowCost + 12];
                    var costEnternal = decimal.Parse(instructor.Select(g => new Course_Detail_Instructor
                    {
                        Allowance = (decimal.Parse(g.Where(t => t.Trainee.bit_Internal == false && t.Duration.HasValue).Sum(a => a.Duration).ToString()) * g.FirstOrDefault()?.Allowance)
                    }).Sum(b => b.Allowance).ToString());
                    cellCostNameInstructor2.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", costEnternal);
                    cellCostNameInstructor2.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellCostNameInstructor2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellCostNameInstructor2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellCostNameInstructor2.AutoFitColumns();
                    Total += costEnternal;
                    total_external += costEnternal;
                    if (item.Course_Cost.Any())
                    {
                        var Costs = item.Course_Cost.GroupBy(a => a.CAT_COSTS, a => a.cost, (a, b) => new CostViewModel
                        {
                            NameCost = a.str_Name,
                            CodeCost = a.str_Code,
                            Cost = b.Sum(),
                        }).Where(a => a.Cost > 0).ToList();
                        Total += Costs.Sum(x => x.Cost);
                        foreach (var catcost in Costs)
                        {
                            var col = starRowCostplus;
                            var colname = catcost.CodeCost + "-" + catcost.NameCost;
                            if (catcostname.Any(a => a.Value == colname))
                            {
                                var col_temp = catcostname.FirstOrDefault(a => a.Value.Equals(colname)).Key;
                                col = col_temp;
                            }
                            else
                            {
                                catcostname.Add(starRowCostplus, colname);
                                starRowCostplus++;
                            }
                            var value_cost = catcost.Cost.HasValue && catcost.Cost > 0 ? catcost.Cost.Value : 0;
                            ExcelRange cellCostName = worksheet.Cells[starRowPlus, coltotal + col];
                            cellCostName.Value = colname;
                            cellCostName.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellCostName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellCostName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellCostName.AutoFitColumns();

                            ExcelRange cellCatCost = worksheet.Cells[starRow + countRow, coltotal + col];
                            cellCatCost.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", value_cost);
                            cellCatCost.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellCatCost.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellCatCost.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellCatCost.AutoFitColumns();

                            if (total_cost.Any(a => a.Key == (coltotal + col)))
                            {
                                total_cost[coltotal + col] += value_cost;
                            }
                            else
                            {
                                total_cost.Add(coltotal + col, value_cost);
                            }
                        }
                    }
                    #region Total
                    ExcelRange celTotal = worksheet.Cells[starRow + countRow, coltotal + listCATCOST.Count() + 1];
                    celTotal.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", Total);
                    celTotal.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celTotal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celTotal.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    celTotal.AutoFitColumns();
                    #endregion
                    countRow++;

                }
                ExcelRange cellTotalCol = worksheet.Cells[starRowPlus, coltotal + listCATCOST.Count() + 1];
                cellTotalCol.Value = "Total";
                cellTotalCol.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellTotalCol.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                cellTotalCol.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellTotalCol.AutoFitColumns();

                ExcelRange celNameTotal = worksheet.Cells[starRow + countRow, 2, starRow + countRow, starCol + starRowCost + 10];
                celNameTotal.Merge = true;
                celNameTotal.Value = "Total";
                celNameTotal.Style.Font.Bold = true;
                celNameTotal.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                celNameTotal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                celNameTotal.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                celNameTotal.AutoFitColumns();

                ExcelRange celTotalAll = worksheet.Cells[starRow + countRow, coltotal + listCATCOST.Count() + 1];
                var sum = total_internal + total_external + total_cost.Sum(a => a.Value);
                celTotalAll.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", sum);
                celTotalAll.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                celTotalAll.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                celTotalAll.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                celTotalAll.AutoFitColumns();

                ExcelRange celTotalinternal = worksheet.Cells[starRow + countRow, starCol + starRowCost + 11];
                celTotalinternal.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", total_internal);
                celTotalinternal.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                celTotalinternal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                celTotalinternal.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                celTotalinternal.AutoFitColumns();

                ExcelRange celTotalexternal = worksheet.Cells[starRow + countRow, starCol + starRowCost + 12];
                celTotalexternal.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", total_external);
                celTotalexternal.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                celTotalexternal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                celTotalexternal.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                celTotalexternal.AutoFitColumns();

                foreach (var item_total in total_cost)
                {
                    ExcelRange celTotalcost = worksheet.Cells[starRow + countRow, item_total.Key];
                    celTotalcost.Value = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", item_total.Value);
                    celTotalcost.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    celTotalcost.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celTotalcost.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    celTotalcost.AutoFitColumns();
                }

                ExcelRange celNameReport = worksheet.Cells[3, starCol + starRowCost + 3, 3, coltotal + listCATCOST.Count() + 1];
                celNameReport.Merge = true;
                celNameReport.Value = " COST REPORT";
                celNameReport.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                celNameReport.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                celNameReport.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                celNameReport.AutoFitColumns();


                ExcelRange cell = worksheet.Cells[starRowPlus, 2, starRow + countRow, coltotal + listCATCOST.Count() + 1];
                //cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                bytes = excelPackage.GetAsByteArray();
            }
            return bytes;
        }


        public ActionResult AjaxHandlerCostDetail(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {

                var db = _repoCourseServiceCost.GetCourseCost(a => a.course_id == id && a.Course.IsActive == true && a.Course.IsDeleted == false && a.CAT_COSTS.IsActive == true && a.CAT_COSTS.IsDeleted == false)
                    .GroupBy(a => a.CAT_COSTS, a => a.cost, (a, b) => new CostViewModel
                    {
                        NameCost = a.str_Name,
                        CodeCost = a.str_Code,
                        Cost = b.Sum(),
                    }).Where(a => a.Cost > 0).ToList();
                var isApproveSubjectResult = CourseService.GetById(id).TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?.int_id_status == (int)UtilConstants.EStatus.Approve;
                if (isApproveSubjectResult)
                {
                    db.Add(new CostViewModel()
                    {
                        CodeCost = "C001",
                        NameCost = "Training allowance(Internal)",
                        Cost = decimal.Parse((CourseService.GetById(id).Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).GroupBy(a => a.Instructor_Id).Select(g => new Course_Detail_Instructor
                        {
                            Allowance = (decimal.Parse(g.Where(t => t.Trainee.bit_Internal == true && t.Duration.HasValue).Sum(a => a.Duration).ToString()) * g.FirstOrDefault().Allowance)
                        }).Sum(b => b.Allowance)).ToString()),
                        createday = DateTime.Now
                    });
                    db.Add(new CostViewModel()
                    {
                        CodeCost = "C002",
                        NameCost = "Training allowance(Outsource)",
                        Cost = decimal.Parse((CourseService.GetById(id).Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).GroupBy(a => a.Instructor_Id).Select(g => new Course_Detail_Instructor
                        {
                            Allowance = (decimal.Parse(g.Where(t => t.Trainee.bit_Internal == false && t.Duration.HasValue).Sum(a => a.Duration).ToString()) * g.FirstOrDefault().Allowance)
                        }).Sum(b => b.Allowance)).ToString()),
                        createday = DateTime.Now
                    });

                }

                IEnumerable<CostViewModel> filtered = db.OrderBy(a => a.CodeCost);
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CostViewModel, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.NameCost
                                                            : sortColumnIndex == 2 ? c?.Cost.ToString()
                                                          : c.createday.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                  c?.CodeCost+" - "+  c?.NameCost,
                                  string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", c?.Cost)
                             };




                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = db.Count(),
                    iTotalDisplayRecords = db.Count(),
                    aaData = result
                },
             JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlerCostDetail", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        private CostReportDataSet.Course_CostRow AddAllowanceCost(Course item, CostReportDataSet.Course_CostDataTable courseCost, int courseCount)
        {
            #region[khai bao]
            var trainingcenter = item.Course_TrainingCenter.FirstOrDefault()?.Department?.Code.ToUpper();
            var courseTypes = UtilConstants.CourseTypesDictionary();
            string GroupCourse = "";
            var CheckGroupCourse = CourseDetailService.Get(a => a.CourseId == item.Id && a.IsDeleted == false);
            if (CheckGroupCourse.Any())
            {
                if (CheckGroupCourse.Count() > 1)
                {
                    if (item.Name.ToLower().IndexOf('-') != -1)
                    {
                        string[] Cut1 = CMSUtils.SplitString(item.Name, '-');
                        GroupCourse = Cut1[0].ToString();
                    }

                }
            }

            #endregion
            var detailRow = courseCost.NewCourse_CostRow();
            detailRow.DataColumn1 = item.Code;
            detailRow.DataColumn2 = GroupCourse;
            detailRow.DataColumn3 = item.CourseTypeId.HasValue
                                    ? courseTypes[item.CourseTypeId.Value]
                                    : "";
            detailRow.DataColumn4 = trainingcenter;
            detailRow.DataColumn5 = item?.Company?.str_Name;
            detailRow.DataColumn6 = item?.Venue;
            detailRow.DataColumn7 = DateUtil.DateToString(item.StartDate, "dd/MM/yyyy");
            detailRow.DataColumn8 = DateUtil.DateToString(item.EndDate, "dd/MM/yyyy");
            detailRow.cost_id = -1;
            detailRow.course_id = item.Id;
            detailRow.No = courseCount.ToString();
            detailRow.courseName = item.Name;

            //Expected Cost
            detailRow.DataColumn13 = item.Course_Cost?.OrderByDescending(a => a.id)?.FirstOrDefault()?.ExpectedCost ?? 0;


            detailRow.costName = "C001 Training allowance (Internal)";
            //detailRow.cost = decimal.Parse((item.Course_Detail_Instructor.Where(a => a.Course_Detail_Id != null).GroupBy(a => a.Instructor_Id).Select(g => new Course_Detail_Instructor
            //{
            //    Allowance = (decimal.Parse(g.Where(t => t.Trainee.bit_Internal).Sum(a => a.Duration).ToString()) * g.FirstOrDefault().Allowance)
            //}).Sum(b => b.Allowance)).ToString());
            detailRow.cost = decimal.Parse((item.Course_Detail_Instructor.GroupBy(a => a.Instructor_Id).Select(g => new Course_Detail_Instructor
            {
                Allowance = (decimal.Parse(g.Where(t => t.Trainee.bit_Internal == true && t.Duration.HasValue).Sum(a => a.Duration).ToString()) * g.FirstOrDefault().Allowance)
            }).Sum(b => b.Allowance)).ToString());

            courseCost.AddCourse_CostRow(detailRow);
            detailRow = courseCost.NewCourse_CostRow();
            detailRow.DataColumn1 = item.Code;
            detailRow.DataColumn2 = GroupCourse;
            detailRow.DataColumn3 = item.CourseTypeId.HasValue
                                    ? courseTypes[item.CourseTypeId.Value]
                                    : "";
            detailRow.DataColumn4 = trainingcenter;
            detailRow.DataColumn5 = item?.Company?.str_Name;
            detailRow.DataColumn6 = item?.Venue;
            detailRow.DataColumn7 = DateUtil.DateToString(item.StartDate, "dd/MM/yyyy");
            detailRow.DataColumn8 = DateUtil.DateToString(item.EndDate, "dd/MM/yyyy");

            detailRow.cost_id = 0;
            detailRow.course_id = item.Id;
            detailRow.courseName = item.Name;
            detailRow.No = courseCount.ToString();
            detailRow.costName = "C002 Training allowance (Outsource)";
            //detailRow.cost = decimal.Parse((item.Course_Detail_Instructor.Where(a => a.Course_Detail_Id != null).GroupBy(a => a.Instructor_Id).Select(g => new Course_Detail_Instructor
            //{
            //    Allowance = (decimal.Parse(g.Where(t => !t.Trainee.bit_Internal).Sum(a => a.Duration).ToString()) * g.FirstOrDefault().Allowance)
            //}).Sum(b => b.Allowance)).ToString());
            detailRow.cost = decimal.Parse((item.Course_Detail_Instructor.GroupBy(a => a.Instructor_Id).Select(g => new Course_Detail_Instructor
            {
                Allowance = (decimal.Parse(g.Where(t => t.Trainee.bit_Internal == false && t.Duration.HasValue).Sum(a => a.Duration).ToString()) * g.FirstOrDefault().Allowance)
            }).Sum(b => b.Allowance)).ToString());

            return detailRow;
        }


        [HttpPost]

        public ActionResult Stockreport(FormCollection form)//int[] arr_checked
        {
            try
            {
                //int[] arr_checked = new Array();
                var browserInformation = Request.Browser;
                //Set as private if current browser type is IE
                Response.AppendHeader("cache-control", browserInformation.Browser == "IE" ? "private" : "no-cache");


                //var data = ctx.sp_GetCostHeader(lst, FromDate_from, ToDate_from, FromDate_to, ToDate_to).ToList<sp_GetCostHeader_Result>();
                var Name = string.IsNullOrEmpty(form["coursename"]) ? string.Empty : form["coursename"].ToLower().Trim();
                var Code = string.IsNullOrEmpty(form["coursecode"]) ? string.Empty : form["coursecode"].ToLower().Trim();
                var searchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? "" : form["fSearchDate_from"].Trim();
                var searchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? "" : form["fSearchDate_to"].Trim();

                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(searchDateFrom, out dateFrom);
                DateTime.TryParse(searchDateTo, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted == false, (int)UtilConstants.ApproveType.Course);

                System.Linq.Expressions.Expression<Func<Course, bool>> expression = (a =>
                   (string.IsNullOrEmpty(Name) || a.Name.Contains(Name)) &&
                   (string.IsNullOrEmpty(Code) || a.Code.Contains(Code)) &&
                   (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                   (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0));
                var course = CourseService.Get(expression).Where(a =>
                dataCourseProccess.FirstOrDefault(z => z.int_Course_id == a.Id && z.int_Type == (int)UtilConstants.ApproveType.Course).int_id_status == (int)UtilConstants.EStatus.Approve).ToList();

                var data =
                    course.Where(
                        a =>
                            a.TMS_APPROVES.LastOrDefault(
                                x => x.int_Type == (int)UtilConstants.ApproveType.Course)?.int_id_status ==
                            (int)UtilConstants.EStatus.Approve);

                CostReportDataSet.Course_CostDataTable courseCost = new CostReportDataSet.Course_CostDataTable();

                var courseTypes = UtilConstants.CourseTypesDictionary();
                var courseCount = 1;
                if (data.Any())
                {
                    foreach (var item in data)
                    {
                        #region[khai bao]

                        if (item.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult)?.int_id_status == (int)UtilConstants.EStatus.Approve)
                        {
                            courseCost.AddCourse_CostRow(AddAllowanceCost(item, courseCost, courseCount));
                        }
                        string trainingcenter = "";
                        trainingcenter = item.Course_TrainingCenter.FirstOrDefault()?.Department?.Code?.ToUpper();


                        string GroupCourse = "";
                        var courseDetails = item.Course_Detail;
                        if (courseDetails.Any())
                        {
                            if (item.Name.ToLower().IndexOf('-') != -1)
                            {
                                string[] Cut1 = CMSUtils.SplitString(item.Name, '-');
                                GroupCourse = Cut1[0];

                            }

                        }

                        #endregion

                        if (item.Course_Cost.Any(a => a.IsDeleted == false))
                        {
                            foreach (var cost in item.Course_Cost.Where(a => a.IsDeleted == false))
                            {

                                var detailRow = courseCost.NewCourse_CostRow();

                                detailRow.DataColumn1 = item.Code;
                                detailRow.DataColumn2 = GroupCourse;
                                detailRow.DataColumn3 = item.CourseTypeId.HasValue
                                    ? courseTypes[item.CourseTypeId.Value]
                                    : "";
                                detailRow.DataColumn4 = trainingcenter;
                                detailRow.DataColumn5 = item.Company == null ? "" : item.Company.str_Name;
                                detailRow.DataColumn6 = item.Venue;
                                detailRow.DataColumn7 = DateUtil.DateToString(item.StartDate, Resource.lbl_FORMAT_DATE);
                                detailRow.DataColumn8 = DateUtil.DateToString(item.EndDate, Resource.lbl_FORMAT_DATE);

                                detailRow.No = courseCount.ToString();
                                detailRow.cost_id = cost.cost_id ?? -1; //item.cost_id.Value;
                                detailRow.course_id = item.Id;
                                detailRow.courseName = item.Name;
                                detailRow.costName = cost.CAT_COSTS?.str_Code + "-" +
                                                     cost.CAT_COSTS?.str_Name;
                                detailRow.cost = cost.cost ?? 0;
                                courseCost.AddCourse_CostRow(detailRow);
                            }

                        }
                        courseCount++;
                    }
                }

                ReportDataSource source = new ReportDataSource();
                source.Name = "DataSet1";
                source.Value = courseCost;

                ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();

                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.ReportPath = Server.MapPath(GetByKey("PrivateTemplate") + "/Report/Cost.rdlc");
                //rv.Attributes. = "TransporterIssueTransaction";
                rv.LocalReport.DataSources.Clear();
                rv.LocalReport.DataSources.Add(source);

                //ReportParameter param_1 = new ReportParameter("name", "Cost", false);
                //ReportParameter param_2 = new ReportParameter("param_2", toDate.ToString("dd/MM/yyyy"), false);
                //ReportParameter param_3 = new ReportParameter("param_3", "OrderList", false);

                List<ReportParameter> lsParam = new List<ReportParameter>();
                //lsParam.Add(param_1);
                //lsParam.Add(param_2);
                //lsParam.Add(param_3);
                //rv.LocalReport.SetParameters(lsParam);
                rv.LocalReport.Refresh();

                byte[] streamBytes = null;
                string mimeType = "";
                string encoding = "";
                string filenameExtension = "";
                string[] streamids = null;
                Warning[] warnings = null;
                string deviceInfo = "<DeviceInfo><OutputFormat>EXCEL</OutputFormat></DeviceInfo>";
                string type = Request["type"];

                streamBytes = rv.LocalReport.Render(REPORT_EXCEL, deviceInfo, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

                return File(streamBytes, mimeType, "Cost" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                //return fl;
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }
        #endregion
        #region[TrainingPlan]
        public ActionResult TrainingPlan()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new TrainingPlanModel()
            {
                DictionaryCourses = CourseService.Get(a => a.StartDate >= timenow, true).OrderBy(a => a.Code).ToDictionary(a => a.Code, a => string.Format("{0} _ {1}", a.Code, a.Name)),
            };

            return View(model);
        }

        public ActionResult TrainingPlanPrint(/*int id = 0*/)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            // xử lý param gửi lên 
            var courseListCode = string.IsNullOrEmpty(Request.QueryString["CourseList[]"]) ? string.Empty : Request.QueryString["CourseList[]"].Trim();
            var name = string.IsNullOrEmpty(Request.QueryString["coursename"]) ? string.Empty : Request.QueryString["coursename"].Trim();
            var code = string.IsNullOrEmpty(Request.QueryString["coursecode"]) ? string.Empty : Request.QueryString["coursecode"].Trim();
            string fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? string.Empty : Request.QueryString["fSearchDate_from"].Trim();
            string fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? string.Empty : Request.QueryString["fSearchDate_to"].Trim();
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFrom, out dateFrom);
            DateTime.TryParse(fSearchDateTo, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            IEnumerable<Course> data = null;

            string[] courseCodes = null;
            if (!string.IsNullOrEmpty(courseListCode))
            {
                courseCodes = courseListCode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (courseCodes != null && courseCodes.Any())
            {
                data = CourseService.Get(a => courseCodes.Contains(a.Code) && a.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course), true);

            }
            else
            {
                data = CourseService.Get(a =>
                  (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(code) && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                (string.IsNullOrEmpty(name) || a.Name.Contains(name)) && (string.IsNullOrEmpty(code) || a.Code.Contains(code)) && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) && a.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course), true);
            }

            #region [Lay Title cua Report]
            string[] header = GetByKey("Training_PLan_Header").Split(new char[] { ',' });
            #endregion

            var model = new TrainingPlanPrint()
            {
                Courses = data.OrderBy(a => a.StartDate),
                Title = header
            };

            return PartialView("TrainingPlanPrint", model);
        }


        [HttpPost]
        public JsonResult ChangeCourseReturnSubject(string id_course)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                int null_instructor = 1;
                DataTable db = CMSUtils.GetDataSQL("", "Course_Detail c LEFT JOIN Subject s ON s.Subject_Id = c.Subject_Id", "s.Subject_Id AS Subject_Id,s.str_Name AS str_Name", String.Format("c.Course_Id={0}", id_course), "s.str_Name");
                if (db.Rows.Count > 0)
                {
                    html.Append("<option value='-1'>--Subject List--</option>");
                    null_instructor = 0;
                    foreach (DataRow dr in db.Rows)
                    {
                        html.AppendFormat("<option value='{0}'>{1}</option>", dr["Subject_Id"], dr["str_Name"]);
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/ChangeCourseReturnSubject", ex.Message);
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult TrainingPlan(FormCollection form)
        {
            int txt_Deparment = string.IsNullOrEmpty(form["txt_Deparment"]) ? -1 : Convert.ToInt32(form["txt_Deparment"].Trim());
            string txt_fromdate = string.IsNullOrEmpty(form["txt_fromdate"]) ? "" : form["txt_fromdate"].ToString();
            string txt_todate = string.IsNullOrEmpty(form["txt_todate"]) ? "" : form["txt_todate"].ToString();


            Boolean validate = true;
            string messenge = "";
            if (string.IsNullOrEmpty(txt_fromdate) || string.IsNullOrEmpty(txt_todate))
            {
                messenge += Messege.VALIDATION_FROMDATE_TODATE + "<br/>";
                validate = false;
            }
            //if (txt_Deparment == -1)
            //{
            //    messenge += Messege.VALIDATION_DEPARTMENT + "<br/>";
            //    validate = false;
            //}

            if (validate == false)
                return Json(new { Message = CMSUtils.alert("warning", messenge) });

            StringBuilder HTML = new StringBuilder();

            DateTime fromdate = DateTime.Parse(txt_fromdate);

            // datatemp
            var data_get_Course_Detail = _repoCourseServiceDetail.Get(a => a.dtm_time_from >= fromdate || fromdate <= a.dtm_time_to);//.Select(a=>a.Course_Detail_Id);
            DataTable table = new DataTable();
            table.Columns.Add("User", typeof(int));
            table.Columns.Add("Subject", typeof(int));
            table.Columns.Add("fromdate", typeof(DateTime));
            table.Columns.Add("todate", typeof(DateTime));
            foreach (var item in data_get_Course_Detail)
            {
                var datagetmenber = _courseServiceMember.Get(a => a.Course_Details_Id == item.Id && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));
                if (datagetmenber != null)
                {
                    foreach (var item_ in datagetmenber)
                    {
                        table.Rows.Add(
                               item_.Member_Id
                               , item.SubjectDetailId
                               , item.dtm_time_from,
                               item.dtm_time_to
                               );
                    }
                }

            }
            DataView dv = table.DefaultView;
            dv.Sort = "User desc";
            DataTable table_ = dv.ToTable();
            //

            DateTime start = DateTime.Parse(txt_fromdate);
            DateTime end = DateTime.Parse(txt_todate);

            HTML.Append("<table class='table table-striped table-bordered  table-responsive' cellspacing='1' style='width: 100 % '>");
            HTML.Append("<thead class=\"m-gray\"><tr role=\"row\">");
            HTML.Append("<th style=\"width: 20 %;\"></th>");
            for (DateTime counter = start; counter <= end; counter = counter.AddDays(1))
            {
                HTML.AppendFormat("<th>{0}</th>", DateUtil.DateToString(counter, "dd/MM/yyyy"));
            }
            HTML.Append("</tr></thead>");

            HTML.Append("<tbody>");

            foreach (DataRow item in table_.Rows)
            {
                var trainee = _repoEmployee.GetById(int.Parse(item["User"].ToString()));
                //var traineename = trainee?.FirstName + " " + trainee?.LastName;
                var traineename = ReturnDisplayLanguage(trainee?.FirstName, trainee?.LastName);
                HTML.Append("<tr>");
                HTML.AppendFormat("<td>{0}</td>", traineename);
                var tempcolunm = "";
                var countTempcolunm = 1;
                var strOutput = "";
                var full = true;
                for (DateTime counter = start; counter <= end; counter = counter.AddDays(1))
                {
                    if ((DateTime)item["fromdate"] <= counter && (DateTime)item["todate"] >= counter)
                    {
                        var subjectname_ = "";
                        subjectname_ = _repoSubject.GetById(int.Parse(item["Subject"].ToString())).str_Name;
                        if (countTempcolunm == 1)
                        {
                            tempcolunm = subjectname_;
                            countTempcolunm++;
                        }
                        else
                        {
                            if (tempcolunm == subjectname_)
                            {
                                countTempcolunm++;
                            }
                        }
                    }
                    else
                    {
                        full = false;
                        // print khi ngoai khoản thời gian
                        if (countTempcolunm == 1)
                        {
                            strOutput += "<td></td>";
                        }
                        else
                        {
                            strOutput += ("<td colspan=" + countTempcolunm + ">");
                            strOutput += "<a class='fc-day-grid-event fc-h-event fc-event fc-start fc-end fc-draggable fc-resizable' style='background-color:#5CB71A;border-color:#5CB71A'><div class='fc-content'> <span class='fc-title'>" + tempcolunm + "</span></div><div class='fc-resizer fc-end-resizer'></div></a>" + "</td>";
                            countTempcolunm = 1;
                            tempcolunm = "";
                        }

                    }
                }
                if (full)
                {
                    strOutput += ("<td colspan=" + countTempcolunm + ">");
                    strOutput += "<a class='fc-day-grid-event fc-h-event fc-event fc-start fc-end fc-draggable fc-resizable' style='background-color:#5CB71A;border-color:#5CB71A'><div class='fc-content'> <span class='fc-title'>" + tempcolunm + "</span></div><div class='fc-resizer fc-end-resizer'></div></a>" + "</td>";
                }
                HTML.AppendFormat(strOutput);
                HTML.Append("</tr>");
            }


            //DataView view = new DataView(table_);
            //DataTable distinctUser = view.ToTable(true, "User");
            //foreach (DataRow item in distinctUser.Rows)
            //{
            //    HTML.Append("<tr>");
            //    var traineename = _repoTrainee.GetById(item["User"]).str_Fullname;
            //    HTML.AppendFormat("<td>{0}</td>", traineename);
            //    for (DateTime counter = start; counter <= end; counter = counter.AddDays(1))
            //    {

            //        DataRow[] result = table_.Select("User ='" + item["User"] + "'");
            //        HTML.Append("<td>");
            //        foreach (DataRow row in result)
            //        {
            //            var subjectname_ = "";
            //            if ((DateTime)row["fromdate"] <= counter && (DateTime)row["todate"] >= counter)
            //            {
            //                subjectname_ = _repoSubject.GetById(row["Subject"]).str_Name;
            //            }
            //            HTML.AppendFormat("{0}<hr />", subjectname_);
            //        }
            //        HTML.Append("</td>");
            //    }
            //    HTML.Append("</tr>");
            //}

            HTML.Append("</tbody>");

            HTML.Append("</table>");
            return Json(new { Result = HTML.ToString() });
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                // xử lý param gửi lên 
                var courseListCode = string.IsNullOrEmpty(Request.QueryString["CourseList[]"]) ? string.Empty : Request.QueryString["CourseList[]"].Trim();
                var name = string.IsNullOrEmpty(Request.QueryString["coursename"]) ? string.Empty : Request.QueryString["coursename"].Trim();
                var code = string.IsNullOrEmpty(Request.QueryString["coursecode"]) ? string.Empty : Request.QueryString["coursecode"].Trim();
                string fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? string.Empty : Request.QueryString["fSearchDate_from"].Trim();
                string fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? string.Empty : Request.QueryString["fSearchDate_to"].Trim();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDateFrom, out dateFrom);
                DateTime.TryParse(fSearchDateTo, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);

                IEnumerable<Course> filtered = null;

                string[] courseCodes = null;
                if (!string.IsNullOrEmpty(courseListCode))
                {
                    courseCodes = courseListCode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                if (courseCodes != null && courseCodes.Any())
                {
                    filtered = CourseService.Get(a => courseCodes.Contains(a.Code) && a.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course), true);

                }
                else
                {
                    filtered = CourseService.Get(a => 
                    (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(code) && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                    (string.IsNullOrEmpty(name) || a.Name.Contains(name))
                    && (string.IsNullOrEmpty(code) || a.Code.Contains(code))
                    && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0)
                    && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) && a.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course), true);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                            : sortColumnIndex == 3 ? c?.StartDate
                                                            : sortColumnIndex == 4 ? c?.EndDate
                                                            
                                                          : (object)c?.StartDate);


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc")
                        ? filtered.OrderBy(orderingFunction)
                        : filtered.OrderByDescending(orderingFunction);
                }
                else
                {
                    filtered = filtered.OrderByDescending(a => a.StartDate);
                }
                // var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                  // ReturnColumncheck(c.Course_Id) ,
                                    string.Empty,
                                    c.Code,
                                    "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+c.Name+"</a></span>",
                                    c.StartDate?.ToString("dd/MM/yyyy") ??"",
                                    c.EndDate?.ToString("dd/MM/yyyy") ??"",
                                   //c?.Course_Type?.str_Name,
                                    c.TMS_APPROVES.Any() ?   ReturnColumnStatus(c.Id) : "",

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        private int ReturnColumncheck(int? Course_Id)
        {
            int status = -1;

            //var dd = _repoTmsApproves.Get(a => a.int_Course_id == Course_Id && a.int_Type == (int)UtilConstants.ApproveType.Course && a.int_id_status == (int)UtilConstants.EStatus.Approve).FirstOrDefault();
            var dd = CourseService.Get(a => a.Id == Course_Id).FirstOrDefault();
            if (dd != null)
            {
                status = (int)Course_Id;
            }
            return status;
        }
        private string ReturnColumnStatus(int? Course_Id)
        {
            int status = -1;
            var approveCourse = (int)UtilConstants.ApproveType.Course;
            var dd = _repoTmsApproves.Get(a => a.int_Course_id == Course_Id && a.int_Type == approveCourse).FirstOrDefault();
            if (dd != null)
            {
                status = (int)dd.int_id_status;
            }
            string _return = "";
            switch (status)
            {
                case (int)UtilConstants.EStatus.Approve:
                    _return = "Approved";
                    break;
                case (int)UtilConstants.EStatus.Reject:
                    _return = "Reject";
                    break;
                case (int)UtilConstants.EStatus.Block:
                    _return = "Block";
                    break;
                case (int)UtilConstants.EStatus.Pending:
                    _return = "Pending";
                    break;
            }
            return _return;
        }


        public ActionResult AjaxHandlerSubject(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var model = _repoCourseServiceDetail.Get(a => a.CourseId == id && a.SubjectDetail.IsDelete == false && a.SubjectDetail.int_Parent_Id != null && a.SubjectDetail.CourseTypeId.HasValue && a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General);

                IEnumerable<Course_Detail> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.SubjectDetail?.Name
                                                                        : sortColumnIndex == 2 ? c?.Trainee?.str_Staff_Id
                                                                        : sortColumnIndex == 3 ? TypeLearning(c.type_leaning.Value)
                                                                        : sortColumnIndex == 4 ? ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName)
                                                                        : sortColumnIndex == 5 ? c?.dtm_time_from?.ToString()
                                                                        : sortColumnIndex == 6 ? c?.dtm_time_to?.ToString()
                                                                        : sortColumnIndex == 7 ? c?.Room?.str_Name
                                                                        : sortColumnIndex == 8 ? c?.SubjectDetail?.Course_Type?.str_Name
                                                                        : c?.dtm_time_from.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";

                }

                if (sortColumnIndex == 0)
                {
                    filtered = filtered.OrderByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Offline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Online)
                                .ThenBy(a => a.dtm_time_from).ThenBy(a => (object)a?.time_from).ThenBy(a => (object)a?.time_to);
                }
                else
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction).ThenBy(c => (object)c?.time_from).ThenBy(c => (object)c?.time_to)
                   : filtered.OrderByDescending(orderingFunction).ThenBy(c => (object)c?.time_from).ThenBy(c => (object)c?.time_to);

                }
                var courseTypes = UtilConstants.CourseTypesDictionary();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                                 //let instructor = string.Join(",", c.Course_Detail_Instructor.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor)?.Select(a => a.Trainee.FirstName + " " + a.Trainee.LastName).ToArray())
                             let instructor = string.Join(",", c.Course_Detail_Instructor.Where(b => b.Type == (int)UtilConstants.TypeInstructor.Instructor)?.Select(a => ReturnDisplayLanguage(a.Trainee.FirstName, a.Trainee.LastName)).ToArray())
                             select new object[] {
                                string.Empty,
                                 "<span "+(c?.SubjectDetail?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.SubjectDetail?.Name +"</span>",
                                TypeLearning(c.type_leaning.Value),
                                c?.SubjectDetail?.Duration,
                                instructor,
                                (c?.dtm_time_from != null ? DateUtil.DateToString(c?.dtm_time_from,"dd/MM/yyyy") : ""),
                                (c?.dtm_time_to != null ? DateUtil.DateToString(c?.dtm_time_to,"dd/MM/yyyy") : ""),
                                c?.Room?.str_Name,
                                c.SubjectDetail.CourseTypeId.HasValue ? courseTypes[c.SubjectDetail.CourseTypeId.Value] : "",
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlerSubject", ex.Message);
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
        public FileContentResult Export(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var courseListCode = string.IsNullOrEmpty(form["CourseList"]) ? string.Empty : form["CourseList"].Trim();
            var name = string.IsNullOrEmpty(form["coursename"]) ? string.Empty : form["coursename"].Trim();
            var code = string.IsNullOrEmpty(form["coursecode"]) ? string.Empty : form["coursecode"].Trim();
            string fSearchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? string.Empty : form["fSearchDate_from"].Trim();
            string fSearchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? string.Empty : form["fSearchDate_to"].Trim();

            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDateFrom, out dateFrom);
            DateTime.TryParse(fSearchDateTo, out dateTo);



            var filecontent = ExportExcelCourse(name, code, dateFrom, dateTo, courseListCode);

            return File(filecontent, ExportUtils.ExcelContentType, "TraningPlan.xlsx");
        }
        private byte[] ExportExcelCourse(string name, string code, DateTime? dateFrom, DateTime? dateTo, string courseListCode)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/TraingPlan.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            IEnumerable<Course> data = null;
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            string[] courseCodes = null;
            if (!string.IsNullOrEmpty(courseListCode))
            {
                courseCodes = courseListCode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (courseCodes != null && courseCodes.Any())
            {
                data = CourseService.Get(a => courseCodes.Contains(a.Code) && a.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course), true).OrderBy(a => a.StartDate);

            }
            else
            {
                data = CourseService.Get(a =>
                 (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(code) && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue ? a.StartDate >= timenow : true) &&
                (string.IsNullOrEmpty(name) || a.Name.Contains(name)) && (string.IsNullOrEmpty(code) || a.Code.Contains(code)) && (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) && a.TMS_APPROVES.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.Course), true).OrderBy(a => a.StartDate);
            }

            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                var worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                var startrow = 9;
                var groupHeader = 0;
                var grouptotal = 0;
                var row = 0;
                int CountMerge = 0;
                int CountMergeTotal = 0;
                int count = 0;
                int start_header = 2;
                // string[] header = GetByKey("Report_TrainingPlan").Split(',');
                string[] header = GetByKey("Training_PLan_Header").Split(new char[] { ',' });

                ExcelRange cellHeader = worksheet.Cells[2, 12];
                cellHeader.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                cellHeader.Style.Font.Size = 11;
                var count_course = 0;
                var name_course = "";
                foreach (var item in data)
                {
                    if (count_course == 0)
                    {
                        name_course = string.Format("TRAINING PLAN FOR {0} REV...", item.Code.Split('-')[0]);
                    }
                    ExcelRange cellTitle = worksheet.Cells[2, 3];
                    cellTitle.Value = name_course;
                    cellTitle.Style.Font.Size = 14;
                    cellTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    double? totalHours = 0;
                    double? totalDays = 0;
                    const int col = 1;

                    worksheet.Cells[
                        startrow + row + groupHeader + grouptotal + CountMergeTotal, 1, startrow + row + groupHeader + CountMergeTotal + grouptotal, 12]
                        .Merge = true;
                    ExcelRange cell = worksheet.Cells[startrow + row + groupHeader + grouptotal + CountMergeTotal, 1];
                    cell.Value = item.Code.ToUpper() + " - " + item.Name.ToUpper();
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGray);
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 14;

                    foreach (
                        var item1 in
                            item.Course_Detail.Where(a => a.IsDeleted != true).OrderByDescending(a=>a.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Offline).ThenByDescending(a => a.type_leaning == (int)UtilConstants.LearningTypes.Online)
                                .ThenBy(a => a.dtm_time_from).ThenBy(a => (object)a?.time_from).ThenBy(a => (object)a?.time_to))
                    {
                        if (item1 != null)
                        {
                            ++count;
                            CountMerge = 0;
                            string nameinstructor = "";
                            string nameExaminer = "";
                            // var total_duration_cro = item1.Course_Blended_Learning.Where(a => a.Course_Detail_Id == item1.Id && a.IsActive == true && a.IsDeleted != true).Sum(a => a.Duration);
                            //totalHours = totalHours + (item1.type_leaning != (int)UtilConstants.LearningTypes.OfflineOnline ? ((item1.SubjectDetail?.Duration != null) ? (int)item1.SubjectDetail?.Duration : 0) : total_duration_cro);

                            //totalDays = totalDays + (item1.type_leaning != (int)UtilConstants.LearningTypes.OfflineOnline ? ((item1.SubjectDetail.Duration != null) ? ((float)item1.SubjectDetail.Duration) / (float)8 : 0): ((total_duration_cro != null) ? ((float)total_duration_cro) / (float)8 : 0));
                            //string date = item1.dtm_time_from.Value.ToString("dd/MM/yyyy") ?? "" + " - " +
                            //              item1.dtm_time_to.Value.ToString("dd/MM/yyyy") ?? "";
                            var instructor = item1.Course_Detail_Instructor.Where(a => a.Course_Detail_Id == item1.Id && a.Type == (int)UtilConstants.TypeInstructor.Instructor).Distinct();
                            var examiner = item1.Course_Detail_Instructor.FirstOrDefault(a => a.Course_Detail_Id == item1.Id && a.Type == (int)UtilConstants.TypeInstructor.Hannah);
                            if (examiner != null)
                            {
                                nameExaminer = ReturnDisplayLanguage(examiner?.Trainee?.FirstName, examiner?.Trainee?.LastName) + Environment.NewLine;
                            }
                            if (instructor.Any())
                            {
                                foreach (var item2 in instructor)
                                {
                                    // nameinstructor += item2?.Trainee?.FirstName + " " + item2?.Trainee?.LastName + Environment.NewLine;
                                    nameinstructor += ReturnDisplayLanguage(item2?.Trainee?.FirstName, item2?.Trainee?.LastName) + Environment.NewLine;
                                }
                            }
                            // Bien CountMerge dung de dem so row phai Merge theo Status (Online, Offline, Examination)

                            CountMerge = (int)item1?.Course_Blended_Learning?.Where(a => a.Course_Detail_Id == item1.Id && a.IsActive == true && a.IsDeleted != true).ToList().Count;
                            if (CountMerge > 0) CountMerge--;
                            ExcelRange cellNo = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col];
                            cellNo.Value = count;
                            cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellNo.Merge = true;

                            ExcelRange cellCourse =
                            worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 1, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col + 1];
                            cellCourse.Value = item1?.SubjectDetail?.Name;
                            cellCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellCourse.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellCourse.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellCourse.Merge = true;

                            ExcelRange cellMethod =
                           worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 2, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col + 2];
                            cellMethod.Value = TypeLearningName(item1.type_leaning.Value);
                            cellMethod.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellMethod.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellMethod.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellMethod.Style.Font.Color.SetColor(Color.Black);
                            cellMethod.Merge = true;


                            if (item1?.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline && item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted != true).Count() > 0)
                            {
                                int StatusRow = 0;
                                foreach (var item2 in item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted == false))
                                {
                                    ExcelRange cellStatus =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + StatusRow + CountMergeTotal, col + 3];
                                    cellStatus.Value = item2?.LearningType;
                                    cellStatus.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    cellStatus.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    cellStatus.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    StatusRow++;
                                }
                            }
                            else
                            {
                                ExcelRange cellStatus =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 3];
                                cellStatus.Value = item1?.type_leaning == (int)UtilConstants.LearningTypes.Online ? "Online" : "Classroom";
                                cellStatus.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellStatus.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellStatus.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            }
                            if (item1?.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline && item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted != true).Count() > 0)
                            {
                                int StatusRow = 0;
                                foreach (var item2 in item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted == false))
                                {
                                    ExcelRange cellHours =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + StatusRow + CountMergeTotal, col + 4];
                                    cellHours.Value = item2?.Duration ?? 0;
                                    cellHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    cellHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    cellHours.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    totalHours += item2?.Duration ?? 0;
                                    StatusRow++;
                                }
                            }
                            else
                            {
                                ExcelRange cellHours =
                                 worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 4, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col + 4];
                                cellHours.Value = item1?.SubjectDetail?.Duration ?? 0;
                                cellHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellHours.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                totalHours += item1?.SubjectDetail?.Duration ?? 0;
                                cellHours.Merge = true;
                            }
                            if (item1?.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline && item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted != true).Count() > 0)
                            {
                                int StatusRow = 0;
                                foreach (var item2 in item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted == false))
                                {
                                    ExcelRange cellDays =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + StatusRow + CountMergeTotal, col + 5];
                                    var dration_cro = (float)(item2?.Duration ?? 0) / (float)8;
                                    cellDays.Value = (dration_cro % 1 == 0 ? Math.Round(dration_cro, 2).ToString() : ConvertDot(Math.Round(dration_cro, 2), 2));
                                    cellDays.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    cellDays.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    totalDays += Math.Round(dration_cro, 2);
                                    cellDays.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    StatusRow++;
                                }
                            }
                            else
                            {
                                ExcelRange cellDays =
                               worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 5, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col + 5];
                                var dration = (float)(item1.SubjectDetail?.Duration ?? 0) / (float)8;
                                cellDays.Value = (dration % 1 == 0 ? Math.Round(dration, 2).ToString() : ConvertDot(Math.Round(dration, 2), 2));
                                cellDays.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellDays.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellDays.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                totalDays += Math.Round(dration, 2);
                                cellDays.Merge = true;
                            }




                            if (item1?.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline && item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted != true).Count() > 0)
                            {
                                int DateRow = 0;
                                foreach (var item2 in item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted == false))
                                {
                                    string date = item2?.DateFrom != null && item2?.DateTo != null ? item2?.DateFrom.Value.ToString("dd/MM/yyyy") + " - " +
                                            item2?.DateTo.Value.ToString("dd/MM/yyyy") : "";

                                    //string date = item2?.DateFrom.Value.ToString("dd/MM/yyyy") + " - " +
                                    //         item2?.DateTo.Value.ToString("dd/MM/yyyy");
                                    ExcelRange cellDate =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + DateRow + CountMergeTotal, col + 6];
                                    cellDate.Value = date;
                                    cellDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    cellDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    cellDate.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    DateRow++;
                                }
                            }
                            else
                            {
                                string date = item1?.dtm_time_from != null && item1?.dtm_time_to != null ? item1?.dtm_time_from.Value.ToString("dd/MM/yyyy") + " - " +
                                            item1?.dtm_time_to.Value.ToString("dd/MM/yyyy") : "";

                                //string date = item1?.dtm_time_from.Value.ToString("dd/MM/yyyy") + " - " +
                                //            item1?.dtm_time_to.Value.ToString("dd/MM/yyyy");
                                ExcelRange cellDate =
                                worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 6];
                                cellDate.Value = date;
                                cellDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellDate.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            }
                            if (item1?.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline && item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted != true).Count() > 0)
                            {
                                ExcelRange cellInstructor = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 7, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col + 7];
                                cellInstructor.Merge = true;
                                cellInstructor.Style.WrapText = true;
                                cellInstructor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellInstructor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellInstructor.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                foreach (var item2 in item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted == false))
                                {

                                    //var Examiner_cell = "";
                                    //if (item2?.LearningType != "Online" && item2?.LearningType != "Offline")
                                    //{
                                    //    Examiner_cell = ReturnDisplayLanguage(item2?.Trainee?.FirstName, item2?.Trainee?.LastName);
                                    //}
                                    cellInstructor.Value = nameinstructor;
                                    //cellInstructor.Value = nameinstructor + (string.IsNullOrEmpty(Examiner_cell) ? "" : "/" + Environment.NewLine + Examiner_cell);
                                }
                            }
                            else
                            {
                                ExcelRange cellInstructor =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 7];
                                cellInstructor.Style.WrapText = true;
                                cellInstructor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellInstructor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellInstructor.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                cellInstructor.Value = nameinstructor;
                                //cellInstructor.Value = nameinstructor + (string.IsNullOrEmpty(nameExaminer) ? "" : "/" + Environment.NewLine + nameExaminer); // item1?.Trainee?.str_Fullname;
                            }


                            if (item1?.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline && item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted != true).Count() > 0)
                            {
                                int ExaminerRow = 0;
                                foreach (var item2 in item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted == false))
                                {
                                    ExcelRange cellExaminer =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + ExaminerRow + CountMergeTotal, col + 8];
                                    cellExaminer.Style.WrapText = true;
                                    cellExaminer.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    cellExaminer.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    cellExaminer.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    //cellExaminer.Value = ReturnDisplayLanguage(item2?.Trainee?.FirstName, item2?.Trainee?.LastName);
                                    if (item2?.LearningType != "Online" && item2?.LearningType != "Classroom")
                                    {
                                        cellExaminer.Value = ReturnDisplayLanguage(item2?.Trainee?.FirstName, item2?.Trainee?.LastName);
                                    }
                                    else
                                    {
                                        cellExaminer.Value = " - ";
                                    }
                                    
                                    ExaminerRow++;
                                }
                            }
                            else
                            {
                                ExcelRange cellExaminer =
                                    worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 8];
                                cellExaminer.Style.WrapText = true;
                                cellExaminer.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellExaminer.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellExaminer.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                cellExaminer.Value = string.IsNullOrEmpty(nameExaminer) ? " - " : nameExaminer;
                            }

                            ExcelRange cellNum = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 9, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col + 9];
                            cellNum.Value = item1?.Course?.NumberOfTrainee;
                            cellNum.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellNum.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellNum.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellNum.Merge = true;

                            if (item1?.type_leaning == (int)UtilConstants.LearningTypes.OfflineOnline && item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted != true).Count() > 0)
                            {
                                var roomrow = 0;
                                foreach (var item2 in item1?.Course_Blended_Learning?.Where(a => a.IsActive == true && a.IsDeleted == false))
                                {
                                    ExcelRange cellVenue =
                              worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + roomrow + CountMergeTotal, col + 10];
                                    cellVenue.Value = item2.Room?.str_Name ?? string.Empty;
                                    cellVenue.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    cellVenue.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    cellVenue.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    cellVenue.Merge = true;
                                    roomrow++;
                                }

                            }
                            else
                            {
                                ExcelRange cellVenue =
                               worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 10];
                                cellVenue.Value = item1.Room?.str_Name ?? string.Empty;
                                cellVenue.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellVenue.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellVenue.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                cellVenue.Merge = true;
                            }


                            ExcelRange cellRemark =
                                 worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal + CountMergeTotal, col + 11, startrow + row + 1 + groupHeader + grouptotal + CountMerge + CountMergeTotal, col + 11];
                            cellRemark.Value = item1?.str_remark ?? "";
                            //cellRemark.Value = item1.Course_Result?.FirstOrDefault()?.Remark ?? string.Empty;
                            cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellRemark.Merge = true;
                            cellRemark.Style.WrapText = true;
                        }
                        groupHeader++;

                        //  worksheet.Cells[startrow + row + groupHeader + grouptotal, 1, startrow + row + groupHeader + grouptotal, 3].Merge = true;
                        CountMergeTotal += CountMerge;
                    }
                    // totalHours += (int)item?.Course_Detail?.FirstOrDefault()?.SubjectDetail?.Duration;
                    //totalDays += ((double)item?.Course_Detail?.FirstOrDefault()?.SubjectDetail?.Duration / (double)8);
                    row++;
                    ExcelRange cellTotal = worksheet.Cells[startrow + row + groupHeader + grouptotal + CountMergeTotal, 1, startrow + row + groupHeader + grouptotal + CountMergeTotal, 4];
                    cellTotal.Style.Font.Bold = true;
                    cellTotal.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTotal.Value = "TOTAL:";
                    cellTotal.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellTotal.Merge = true;

                    ExcelRange cellTotalHours = worksheet.Cells[startrow + row + groupHeader + grouptotal + CountMergeTotal, 5];
                    cellTotalHours.Value = totalHours;
                    cellTotalHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTotalHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellTotalHours.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellTotalDays = worksheet.Cells[startrow + row + groupHeader + grouptotal + CountMergeTotal, 6];
                    cellTotalDays.Value = (totalDays % 1 == 0 ? Math.Round(totalDays.Value, 2).ToString() : ConvertDot(Math.Round(totalDays.Value, 2), 2));
                    cellTotalDays.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTotalDays.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellTotalDays.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                    ExcelRange cellMerge = worksheet.Cells[startrow + row + groupHeader + grouptotal + CountMergeTotal, 7, startrow + row + groupHeader + grouptotal + CountMergeTotal, 12];
                    cellMerge.Merge = true;
                    cellMerge.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    grouptotal++;
                    count_course++;

                }
                int lastRow = startrow + row + groupHeader + grouptotal + CountMergeTotal;
                worksheet.Cells[lastRow + 1, 1, lastRow + 3, 12].Merge = true;
                ExcelRange cell1 = worksheet.Cells[lastRow + 1, 1];
                cell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                cell1.Style.Font.Bold = true;
                cell1.Style.Font.Size = 14;
                cell1.Value = "Notes: eL (Online): e-Learning, cR (Offline): ClassRoom Learning, Exam: Examination. Classroom training will start from 08:30 to 12:00 in the morning, and from 13:15 to 16:45 in the afternoon (Local time: UTC+7).";
                cell1.Style.WrapText = true;

                worksheet.Cells[lastRow + 4, 1, lastRow + 4, 2].Merge = true;
                ExcelRange cell3 = worksheet.Cells[lastRow + 4, 1];
                worksheet.Cells[lastRow + 4, 1].Style.Font.Bold = true;
                worksheet.Cells[lastRow + 4, 1].Style.Font.Size = 14;
                worksheet.Cells[lastRow + 4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell3.Value = "Approved by";

                worksheet.Cells[lastRow + 5, 1, lastRow + 5, 2].Merge = true;
                ExcelRange cell4 = worksheet.Cells[lastRow + 5, 1];
                worksheet.Cells[lastRow + 5, 1].Style.Font.Bold = true;
                worksheet.Cells[lastRow + 5, 1].Style.Font.Size = 14;
                worksheet.Cells[lastRow + 5, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell4.Value = "HEAD OF TRAINING";

                worksheet.Cells[lastRow + 6, 1, lastRow + 6, 2].Merge = true;
                ExcelRange cell4_ = worksheet.Cells[lastRow + 6, 1];
                worksheet.Cells[lastRow + 6, 1].Style.Font.Size = 14;
                worksheet.Cells[lastRow + 6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell4_.Value = "Date .....................................";

                worksheet.Cells[lastRow + 4, 11, lastRow + 4, 12].Merge = true;
                ExcelRange cell7 = worksheet.Cells[lastRow + 4, 11];
                worksheet.Cells[lastRow + 4, 11].Style.Font.Bold = true;
                worksheet.Cells[lastRow + 4, 11].Style.Font.Size = 14;
                worksheet.Cells[lastRow + 4, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell7.Value = "Prepared by";

                worksheet.Cells[lastRow + 5, 11, lastRow + 5, 12].Merge = true;
                ExcelRange cell8 = worksheet.Cells[lastRow + 5, 11];
                worksheet.Cells[lastRow + 5, 11].Style.Font.Bold = true;
                worksheet.Cells[lastRow + 5, 11].Style.Font.Size = 14;
                worksheet.Cells[lastRow + 5, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell8.Value = "PLANNING TEAM";

                worksheet.Cells[lastRow + 6, 11, lastRow + 6, 12].Merge = true;
                ExcelRange cell9 = worksheet.Cells[lastRow + 6, 11];
                worksheet.Cells[lastRow + 6, 11].Style.Font.Size = 14;
                worksheet.Cells[lastRow + 6, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell9.Value = "Date .....................................";

                Bytes = xlPackage.GetAsByteArray();

            }

            return Bytes;
        }
        #endregion
        #region[Subject Result]
        public ActionResult SubjectResult(int? id = 0)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new SubjectResultModel
            {
                Courseses =
                    CourseService.Get(a => a.StartDate >= timenow,true)
                        .OrderByDescending(b => b.Id)
                        .ToDictionary(a => a.Id, a => a.Name)
            };

            //ViewBag.CourseList = CourseService.Get().OrderByDescending(m => m.Id);
            return View(model);
        }
        public ActionResult SubjectResultPrint(int id = 0)
        {
            //var ddlSubject = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());
            //var courseDetail = _repoCourseServiceDetail.GetDetailInstructorBySubject(id);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            var courseDetail = CourseDetailService.GetById(id);
            var model = new CourseDetailModelRp();
            model.header = GetByKey("Subject_Result_Header").Split(new char[] { ',' });
            if (courseDetail != null)
            {
                var data = _courseServiceMember.GetSubjectResult(id);
                //var data = _courseServiceMember.Get(a => a.Course_Details_Id == id && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderBy(m => m.Member_Id).ToList();
                model.CourseCode = courseDetail.Course.Code;
                model.CourseName = courseDetail.Course.Name;
                model.SubjectDetailName = courseDetail.SubjectDetail.Name;
                model.SubjectDetailDuration = (double)courseDetail.SubjectDetail.Duration;
                model.RoomName = courseDetail?.Room?.str_Name;
                model.TimeFrom = courseDetail.dtm_time_from.HasValue ? courseDetail.dtm_time_from.Value.ToString("dd/MM/yyyy") : "";
                model.TimeTo = courseDetail.dtm_time_to.HasValue ? courseDetail.dtm_time_to.Value.ToString("dd/MM/yyyy") : "";
                model.TypeLearning = TypeLearning(courseDetail.type_leaning ?? (int)UtilConstants.LearningTypes.Offline);
                model.SubjectActive = courseDetail.SubjectDetail.IsActive;
                model.TraineeRps = data.Select(a => new CourseDetailModelRp.TraineeRp
                {
                    bit_Average_Calculate = a?.bit_Average_Calculate,
                    FullName = a?.FullName,
                    StaffId = a?.StaffId,
                    DepartmentCode = a?.DeptCode,
                    FirstCheck = ReturnTraineePoint_Huy_Custom(a?.bit_Average_Calculate, a?.First_Check_Score, a?.First_Check_Result),
                    Recheck = ReturnTraineePoint_Huy_Custom(a?.bit_Average_Calculate, a?.Re_Check_Score, a?.Re_Check_Result),
                    ReMark = a?.Type_fail == true ? GetRemarkCheckFail(a.Course_Result_id) : (a?.Remark != null ? a?.Remark?.Replace("!!!!!", "<br />") : ""),
                    Grace = returnpointgrade(2, a?.Traineeid, a?.CourseDetailId),
                }).OrderByDescending(p => p.Grace == "Distinction").ThenByDescending(p => p.Grace == "Pass").ThenByDescending(p => p.Grace == "Fail").ThenByDescending(a => a.FirstCheck).ThenByDescending(a => a.Recheck).ThenBy(a => a.StaffId);

            }
            //ViewBag.returnreport = data;
            return PartialView("SubjectResultPrint", model);
        }

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
                        //if ((bool)_repoCourseServiceDetail.checkapproval(item, new[] { (int)UtilConstants.ApproveType.SubjectResult }))

                        html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, (item?.SubjectDetail?.IsActive != true ? "(" + UtilConstants.String_DeActive + ") " : "") + item.SubjectDetail.Name);
                    }
                }
                else
                {
                    null_instructor = 1;
                }

                var objectReturn = Json(new
                {
                    value_option = html.ToString(),
                    value_null = null_instructor
                }, JsonRequestBehavior.AllowGet);

                return objectReturn;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/ChangeCourseReturnSubjectResult", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }
        public ActionResult AjaxHandlResultHasInsert_(jQueryDataTableParamModel param)
        {
            try
            {
                int courseDetailId = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());

                //var data = _CourseService.GetCourseResultSummaries(a => a.CourseDetailId == courseDetailId).ToList().Select(a => new ResultViewModels


                var data_ =
                   _courseServiceMember.Get(
                       a => a.Course_Details_Id == courseDetailId && a.IsActive == true).AsEnumerable();

                var data =
                  data_.Select(b => new ResultViewModels
                  {
                      Traineeid = b?.Trainee?.Id,
                      CourseDetailId = b?.Course_Details_Id,
                      bit_Average_Calculate = b?.Course_Detail?.SubjectDetail?.IsAverageCalculate,
                      StaffId = b?.Trainee?.str_Staff_Id?.ToString() ?? "",
                      FullName = ReturnDisplayLanguage(b?.Trainee?.FirstName, b?.Trainee?.LastName),
                      DeptCode = b?.Trainee?.Department?.Code?.ToString() ?? "",
                      from = b?.Course_Detail?.dtm_time_from.Value,
                      to = b?.Course_Detail?.dtm_time_to.Value,
                      FirstCheck = ReturnTraineePoint(true, b?.Course_Detail?.SubjectDetail?.IsAverageCalculate, b?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == b.Member_Id)),
                      Recheck = ReturnTraineePoint(false, b?.Course_Detail?.SubjectDetail?.IsAverageCalculate, b?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == b.Member_Id)),
                      Grade = returnpointgrade(2, b?.Member_Id, b?.Course_Details_Id),
                      SubjectDetailId = b?.Course_Detail?.SubjectDetailId,
                      Remark = b?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == b.Member_Id)?.Remark != null ? b?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == b.Member_Id)?.Remark.Replace("!!!!!", "<br />") : null,

                  });
                IEnumerable<ResultViewModels> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ResultViewModels, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.StaffId
                                                            : sortColumnIndex == 2 ? c.FullName
                                                            : sortColumnIndex == 3 ? c.DeptCode
                                                            : sortColumnIndex == 4 ? c.from
                                                            : sortColumnIndex == 5 ? c.FirstCheck
                                                            : sortColumnIndex == 6 ? c.Recheck
                                                            : sortColumnIndex == 7 ? c.Remark
                                                            : sortColumnIndex == 8 ? (object)c.Grade
                                                            : c.FirstCheck);


                var sortDirection = Request["sSortDir_0"] ?? "desc"; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenBy(orderingFunction).ThenByDescending(p => p.FirstCheck).ThenByDescending(p => p.Recheck).ThenBy(a => a.StaffId)
                                   : filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(orderingFunction).ThenByDescending(p => p.FirstCheck).ThenByDescending(p => p.Recheck).ThenBy(a => a.StaffId);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                string.Empty,
                                                c?.StaffId,
                                                c?.FullName,
                                                c?.DeptCode,
                                                DateUtil.DateToString(c.@from,"dd/MM/yyyy")  +"-"+ DateUtil.DateToString(c?.to,"dd/MM/yyyy"),
                                                c?.FirstCheck,
                                                c?.Recheck,
                                                c?.Grade,
                                                c?.Remark
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlResultHasInsert", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlResultHasInsert(jQueryDataTableParamModel param)
        {
            try
            {
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                int courseDetailId = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());
                var data_ = _courseServiceMember.GetSubjectResult(courseDetailId);
                var data =
                  data_.Select(b => new ResultViewModels
                  {
                      Traineeid = b?.Traineeid,
                      CourseDetailId = b?.CourseDetailId,
                      bit_Average_Calculate = b?.bit_Average_Calculate,
                      StaffId = b?.StaffId,
                      FullName = b.FullName,
                      DeptCode = b?.DeptCode,
                      from = b?.dtm_time_from.Value,
                      to = b?.dtm_time_to.Value,
                      FirstCheck = ReturnTraineePoint_Huy_Custom(b?.bit_Average_Calculate, b?.First_Check_Score, b?.First_Check_Result),
                      Recheck = ReturnTraineePoint_Huy_Custom(b?.bit_Average_Calculate, b?.Re_Check_Score, b?.Re_Check_Result),
                      Grade = returnpointgrade(2, b?.Traineeid, b?.CourseDetailId),
                      SubjectDetailId = b?.SubjectDetailId,
                      Remark = b?.Type_fail == true ? GetRemarkCheckFail(b.Course_Result_id) : (b?.Remark != null ? b?.Remark.Replace("!!!!!", "<br />") : ""),
                  });

                //foreach (var item in data_)
                //{
                //    item.Grade = returnpointgrade(2, item?.Traineeid, item?.CourseDetailId);
                //}
                IEnumerable<ResultViewModels> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ResultViewModels, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.StaffId
                                                            : sortColumnIndex == 2 ? c.FullName
                                                            : sortColumnIndex == 3 ? c.DeptCode
                                                            : sortColumnIndex == 4 ? c.from
                                                            : sortColumnIndex == 5 ? c.FirstCheck
                                                            : sortColumnIndex == 6 ? c.Recheck
                                                            : sortColumnIndex == 7 ? c.Remark
                                                            : sortColumnIndex == 8 ? c.Grade
                                                            : c.FirstCheck);


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(p => p.FirstCheck).ThenByDescending(p => p.Recheck).ThenBy(a => a.StaffId).ThenBy(orderingFunction)
                                   : filtered.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(p => p.FirstCheck).ThenByDescending(p => p.Recheck).ThenBy(a => a.StaffId).ThenByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                string.Empty,
                                                c?.StaffId,
                                                c?.FullName,
                                                c?.DeptCode,
                                                DateUtil.DateToString(c.@from,"dd/MM/yyyy")  +"-"+ DateUtil.DateToString(c?.to,"dd/MM/yyyy"),
                                                c?.bit_Average_Calculate == true ? c?.FirstCheck?.ToString()?.Replace("-1", "0") : c?.FirstCheck,
                                                c?.bit_Average_Calculate == true ? c?.Recheck?.ToString()?.Replace("-1", "0") : c?.Recheck,
                                                c?.Grade,
                                                c?.Remark
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = data_.Count(),
                    iTotalDisplayRecords = data_.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlResultHasInsert", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        protected object ReturnTraineePoint_Huy_Custom(bool? isAvarage, double? point, string grade)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            object result = null;
            if (isAvarage == true)
            {
                result = point != -1 ? point : 0;
            }
            else
            {
                result = grade;
            }
            return result;
        }
        public string returnpointgrade(int? type, int? Trainee_Id, int? Course_Details_Id)
        {
            //CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            //customCulture.NumberFormat.NumberDecimalSeparator = ".";
            //System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            string _return = "";
            var data = CourseService.GetCourseResult(a => a.TraineeId == Trainee_Id && a.CourseDetailId == Course_Details_Id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            if (data != null)
            {
                if (type == 1)
                {
                    #region [get điểm]
                    if (data.Re_Check_Score != null)
                    {
                        _return = data.Re_Check_Score.ToString().Replace("-1", "0");
                    }
                    else
                    {
                        if (data.First_Check_Score != null)
                        {
                            _return = data.First_Check_Score.ToString().Replace("-1", "0");
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
                            var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Course_Detail.SubjectDetailId).OrderByDescending(a => a.point_from);
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
                                var check_distintion = _repoSubject.GetScores(a => a.subject_id == data.Course_Detail.SubjectDetailId).OrderBy(a => a.point_from);
                                foreach (var item in check_distintion)
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
        public object ReturnTraineePoint(bool isFirstCheck, bool? isAvarage, Course_Result rs = null)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            object point = null;
            if (rs == null) return null;
            if (isFirstCheck)
            {
                if (isAvarage.HasValue && isAvarage.Value)
                {
                    point = rs.First_Check_Score != -1 ? rs.First_Check_Score : 0;
                }
                else
                {
                    switch (rs.First_Check_Result)
                    {
                        case "P":
                            point = "Pass";
                            break;
                        case "F":
                            point = "Fail";
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (isAvarage.HasValue && isAvarage.Value)
                {
                    point = rs.Re_Check_Score != -1 ? rs.Re_Check_Score : 0;
                }
                else
                {
                    switch (rs.Re_Check_Result)
                    {
                        case "P":
                            point = "Pass";
                            break;
                        case "F":
                            point = "Fail";
                            break;
                        default:
                            break;
                    }
                }
            }
            return point;
        }

        [HttpPost]
        public FileContentResult ExportSubjectResult(FormCollection form)
        {

            var ddlSubject = string.IsNullOrEmpty(form["ddl_subject"]) ? -1 : Convert.ToInt32(form["ddl_subject"].Trim());
            // ViewBag.course_details = _repoCourseServiceDetail.GetById(ddlSubject);
            var filecontent = ExportExcelSubjectResult(ddlSubject);
            return File(filecontent, ExportUtils.ExcelContentType, "SubjectResult.xlsx");
        }
        //TODO: process result
        private byte[] ExportExcelSubjectResult(int ddlSubject)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/SubjectResult.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);
            var data_ = _courseServiceMember.GetSubjectResult(ddlSubject);
            var data =
              data_.Select(b => new ResultViewModels
              {
                  Traineeid = b?.Traineeid,
                  CourseDetailId = b?.CourseDetailId,
                  bit_Average_Calculate = b?.bit_Average_Calculate,
                  StaffId = b?.StaffId,
                  FullName = b.FullName,
                  DeptCode = b?.DeptCode,
                  from = b?.dtm_time_from.Value,
                  to = b?.dtm_time_to.Value,
                  FirstCheck = ReturnTraineePoint_Huy_Custom(b?.bit_Average_Calculate, b?.First_Check_Score, b?.First_Check_Result),
                  Recheck = ReturnTraineePoint_Huy_Custom(b?.bit_Average_Calculate, b?.Re_Check_Score, b?.Re_Check_Result),
                  Grade = returnpointgrade(2, b?.Traineeid, b?.CourseDetailId),
                  SubjectDetailId = b?.SubjectDetailId,
                  Remark = b?.Type_fail == true ? GetRemarkCheckFail(b.Course_Result_id) : (b?.Remark != null ? b?.Remark.Replace("!!!!!", "<br />") : ""),
              });
            ExcelPackage excelPackage;
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            using (excelPackage = new ExcelPackage(template, false))
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets["Sheet1"];

                string[] header = GetByKey("Subject_Result_Header").Split(new char[] { ',' });
                ExcelRange cellHeader = worksheet.Cells[3, 9];
                cellHeader.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                cellHeader.Style.Font.Size = 11;
                cellHeader.Style.WrapText = true;

                ExcelRange cellHeaderCourseName = worksheet.Cells[7, 4];
                cellHeaderCourseName.Value = courseDetail?.Course?.Name?.ToString() ?? "";

                ExcelRange cellHeaderCourseCode = worksheet.Cells[7, 8];
                cellHeaderCourseCode.Value = courseDetail?.Course?.Code?.ToString() ?? "";

                ExcelRange cellHeaderSubjectName = worksheet.Cells[8, 4];
                cellHeaderSubjectName.Value = (courseDetail?.SubjectDetail?.IsActive != true ? UtilConstants.String_DeActive : "") + courseDetail?.SubjectDetail?.Name;

                ExcelRange cellHeaderDuration = worksheet.Cells[8, 8];
                cellHeaderDuration.Value = courseDetail?.SubjectDetail?.Duration;

                ExcelRange cellHeaderVenue = worksheet.Cells[9, 4];
                cellHeaderVenue.Value = courseDetail?.Room?.str_Name?.ToString() ?? "";

                ExcelRange cellHeaderDate = worksheet.Cells[9, 8];
                cellHeaderDate.Value = courseDetail?.dtm_time_from.Value.ToString("dd/MM/yyyy") + " -" + courseDetail?.dtm_time_to.Value.ToString("dd/MM/yyyy");

                ExcelRange cellHeaderType = worksheet.Cells[10, 4];
                cellHeaderType.Value = TypeLearningName((int)courseDetail.type_leaning);
                int startRow = 14;
                int count = 0;

                foreach (var item in data.OrderByDescending(p => p.Grade == "Distinction").ThenByDescending(p => p.Grade == "Pass").ThenByDescending(p => p.Grade == "Fail").ThenByDescending(a => a.FirstCheck).ThenByDescending(a => a.Recheck).ThenBy(a => a.StaffId))
                {
                    int col = 2;
                    ExcelRange cellNo = worksheet.Cells[startRow, col];
                    cellNo.Value = ++count;
                    cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellFullName = worksheet.Cells[startRow, ++col];
                    cellFullName.Value = item.FullName;
                    cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellStaffId = worksheet.Cells[startRow, ++col];
                    cellStaffId.Value = item?.StaffId ?? "";
                    cellStaffId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellStaffId.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellStaffId.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellDepartment = worksheet.Cells[startRow, ++col];
                    cellDepartment.Value = item?.DeptCode ?? "";
                    cellDepartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellDepartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellDepartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellFirstPoint = worksheet.Cells[startRow, ++col];
                    cellFirstPoint.Value = item.FirstCheck != null ? (item.bit_Average_Calculate == true ? ((double)item?.FirstCheck % 1 == 0 ? ((double)item?.FirstCheck).ToString() : ConvertDot((double)item?.FirstCheck)) : item.FirstCheck?.ToString()?.Replace("-1", "0")) : "";
                    cellFirstPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellFirstPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellFirstPoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                    ExcelRange cellRePoint = worksheet.Cells[startRow, ++col];
                    cellRePoint.Value = item.Recheck != null ? (item.bit_Average_Calculate == true ? ((double)item?.Recheck % 1 == 0 ? ((double)item?.Recheck).ToString() : ConvertDot((double)item?.Recheck)) : item?.Recheck?.ToString()?.Replace("-1", "0")) : "";
                    cellRePoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellRePoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellRePoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    //ExcelRange cellPoint = worksheet.Cells[startRow, ++col];
                    //cellPoint.Value = GetPointRemark(UtilConstants.DetailResult.Score, item.Member_Id, item.Course_Details_Id);
                    //cellPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //cellPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //cellPoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellGrade = worksheet.Cells[startRow, ++col];
                    cellGrade.Value = item.Grade;
                    cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellGrade.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    ExcelRange cellRemark = worksheet.Cells[startRow, ++col];
                    cellRemark.Value = item.type_checkfail == true ? GetRemarkCheckFail(item.ResultId) : (!string.IsNullOrEmpty(item.Remark) ? item.Remark.Replace("!!!!!", Environment.NewLine) : "");
                    cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellRemark.Style.WrapText = true;
                    startRow++;
                }

                ExcelRange cell1 = worksheet.Cells[++startRow, 2];
                cell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                cell1.Style.Font.Bold = true;
                cell1.Style.Font.Size = 11;
                cell1.Value = "Note: eL: e-Learning, cR: Classroom Learning, cRo: Classroom and Online Learning.";
                cell1.Style.WrapText = false;
                startRow++;

                worksheet.Cells[++startRow, 2, startRow, 3].Merge = true;
                ExcelRange cell2 = worksheet.Cells[startRow, 2];
                cell2.Style.Font.Bold = true;
                cell2.Style.Font.Size = 11;
                cell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell2.Value = "Approved by";

                worksheet.Cells[startRow, 8, startRow, 9].Merge = true;
                ExcelRange cell3 = worksheet.Cells[startRow, 8];
                cell3.Style.Font.Bold = true;
                cell3.Style.Font.Size = 11;
                cell3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell3.Value = "Prepared by";

                worksheet.Cells[++startRow, 2, startRow, 3].Merge = true;
                ExcelRange cell4 = worksheet.Cells[startRow, 2];
                cell4.Style.Font.Bold = true;
                cell4.Style.Font.Size = 12;
                cell4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell4.Value = "HEAD OF TRAINING";

                worksheet.Cells[startRow, 8, startRow, 9].Merge = true;
                ExcelRange cell5 = worksheet.Cells[startRow, 8];
                cell5.Style.Font.Bold = true;
                cell5.Style.Font.Size = 12;
                cell5.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell5.Value = "INSTRUCTOR";

                worksheet.Cells[++startRow, 2, startRow, 3].Merge = true;
                ExcelRange cell6 = worksheet.Cells[startRow, 2];
                cell6.Style.Font.Size = 11;
                cell6.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell6.Value = "Date .....................................";

                worksheet.Cells[startRow, 8, startRow, 9].Merge = true;
                ExcelRange cell7 = worksheet.Cells[startRow, 8];
                cell7.Style.Font.Size = 11;
                cell7.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell7.Value = "Date .....................................";


                bytes = excelPackage.GetAsByteArray();
            }
            return bytes;

            //DataTable dt = ExportUtils.ConvertToDatatable(data.ToArray());
            //byte[] Bytes = null;
            //int startrow = 7;
            //string templateFilePath = Server.MapPath(@"~/Template/ExcelFile/SubjectResult.xlsx");
            //FileInfo template = new FileInfo(templateFilePath);

            //MemoryStream MS = new MemoryStream();
            //using (xlPackage = new ExcelPackage(template, false))
            //{
            //    ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
            //    #region Write Header
            //    ExcelRange cellHeaderCourseName = worksheet.Cells[startrow, 3];
            //    cellHeaderCourseName.Value = courseDetails.Course?.Name;

            //    ExcelRange cellHeaderCourseCode = worksheet.Cells[startrow, 8];
            //    cellHeaderCourseCode.Value = courseDetails.Course?. Code;

            //    ExcelRange cellHeaderSubjectName = worksheet.Cells[startrow + 1, 3];
            //    cellHeaderSubjectName.Value = courseDetails.SubjectDetail?.Name;

            //    ExcelRange cellHeaderDuration = worksheet.Cells[startrow + 1, 8];
            //    cellHeaderDuration.Value = courseDetails.SubjectDetail?.Duration;

            //    ExcelRange cellHeaderVenue = worksheet.Cells[startrow + 2, 3];
            //    cellHeaderVenue.Value = courseDetails.Room?.str_Name;

            //    ExcelRange cellHeaderDate = worksheet.Cells[startrow + 2, 8];
            //    cellHeaderDate.Value = courseDetails.dtm_time_from.ToString("dd/MM/yyyy") + " -" + courseDetails.dtm_time_to.ToString("dd/MM/yyyy");
            //    #endregion

            //    #region Write Details
            //    startrow = 12;
            //    int row = 1;


            //    int rowheader = 0;
            //    int rowDetail = 0;
            //    int count = 0;
            //    foreach (var item1 in data.OrderByDescending(a => a.score))
            //    {
            //        count++;
            //        int col = 2;

            //        var courseresult = CourseService.GetCourseResult(item1.traineeid ,item1.course_detail_id);


            //        ExcelRange cellNo = worksheet.Cells[rowheader + startrow + rowDetail + 1, col];
            //        cellNo.Value = count;
            //        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        //Full name
            //        ExcelRange cellFullName = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
            //        cellFullName.Value = item1?.fullName;
            //        //EID
            //        ExcelRange cellEID = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
            //        cellEID.Value = item1?.staffId;
            //        cellEID.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        cellEID.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        //dept
            //        ExcelRange celdept = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
            //        celdept.Value = item1?.deptCode;
            //        celdept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        celdept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        //first check
            //        ExcelRange celfirstcheck = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
            //        celfirstcheck.Value = (courseresult != null ? (courseDetails.SubjectDetail?.bit_Average_Calculate == true ? courseresult?.First_Check_Score.ToString() : (courseresult?.First_Check_Result == "P" ? "Pass" : "Fail")) : "");
            //        celfirstcheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        celfirstcheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        //re check
            //        ExcelRange celrecheck = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
            //        celrecheck.Value = (courseresult != null ? (courseDetails.SubjectDetail?.bit_Average_Calculate == true ? courseresult?.Re_Check_Score.ToString() : (courseresult?.Re_Check_Result == "P" ? "Pass" : "Fail")) : "");
            //        celrecheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        celrecheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        //grade
            //        ExcelRange celgrade = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
            //        celgrade.Value = item1?.grade;
            //        celgrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        celgrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        //remark
            //        ExcelRange celremark = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
            //        celremark.Value = (courseresult != null ? courseresult?.Remark : "");
            //        celremark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            //        rowDetail++;

            //    }


            //    //if (dt.Rows.Count > 0)
            //    //{
            //    using (ExcelRange r = worksheet.Cells[startrow - 1, 2, startrow + dt.Rows.Count, dt.Columns.Count - 5])
            //    {
            //        r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //        r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //        r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //        r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //        r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
            //        r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
            //        r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
            //        r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
            //    }
            //    //}
            //    #endregion

            //    #region Write Footer

            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 2, startrow + dt.Rows.Count + 2, 3].Merge = true;
            //    ExcelRange cell2 = worksheet.Cells[startrow + dt.Rows.Count + 2, 2];
            //    cell2.Value = "Approved by";
            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 2].Style.Font.Bold = true;
            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 2].Style.Font.Size = 11;
            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 2, startrow + dt.Rows.Count + 3, 3].Merge = true;
            //    ExcelRange cell3 = worksheet.Cells[startrow + dt.Rows.Count + 3, 2];
            //    cell3.Value = "HEAD OF TRAINING";
            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 2].Style.Font.Bold = true;
            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 2].Style.Font.Size = 12;
            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //    worksheet.Cells[startrow + dt.Rows.Count + 4, 2, startrow + dt.Rows.Count + 4, 3].Merge = true;
            //    ExcelRange cell5_ = worksheet.Cells[startrow + dt.Rows.Count + 4, 2];
            //    cell5_.Value = "Date .....................................";
            //    worksheet.Cells[startrow + dt.Rows.Count + 4, 2].Style.Font.Size = 11;
            //    worksheet.Cells[startrow + dt.Rows.Count + 4, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //    ///////////////////////////////

            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 8, startrow + dt.Rows.Count + 2, 9].Merge = true;
            //    ExcelRange cell5 = worksheet.Cells[startrow + dt.Rows.Count + 2, 8];
            //    cell5.Value = "Prepared by";
            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 8].Style.Font.Bold = true;
            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 8].Style.Font.Size = 11;
            //    worksheet.Cells[startrow + dt.Rows.Count + 2, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 8, startrow + dt.Rows.Count + 3, 9].Merge = true;
            //    ExcelRange cell6 = worksheet.Cells[startrow + dt.Rows.Count + 3, 8];
            //    cell6.Value = "INSTRUCTOR";
            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 8].Style.Font.Bold = true;
            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 8].Style.Font.Size = 12;
            //    worksheet.Cells[startrow + dt.Rows.Count + 3, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //    worksheet.Cells[startrow + dt.Rows.Count + 4, 8, startrow + dt.Rows.Count + 4, 9].Merge = true;
            //    ExcelRange cell6_ = worksheet.Cells[startrow + dt.Rows.Count + 4, 8];
            //    cell6_.Value = "Date .....................................";
            //    worksheet.Cells[startrow + dt.Rows.Count + 4, 8].Style.Font.Size = 11;
            //    worksheet.Cells[startrow + dt.Rows.Count + 4, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //    #endregion
            //    Bytes = xlPackage.GetAsByteArray();
            //}

            //return Bytes;
        }

        public string GetRemarkCheckFail(int? resultid)
        {
            var remark = "";
            var remarkresult = CourseService.GetCourseResultCheckFail(a => a.CourseResultID == resultid);
            remark = remarkresult.FirstOrDefault()?.RemarkContent;
            return remark;
        }
        //private string GetRemark(int? traineeId, int? courseDetailId)
        //{
        //    var result = "";
        //    var data =
        //        CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId)
        //            .OrderByDescending(a => a.Id)
        //            .FirstOrDefault();
        //    result = data?.Remark ?? result;
        //    return result;
        //}

        //private string GetPoint(int? traineeId, int? courseDetailId)
        //{
        //    var _return = "";
        //    var data = CourseService.GetCourseResultSummaries(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId).FirstOrDefault();
        //    _return = (data?.point != null) ? data.point?.ToString() : _return;
        //    return _return;
        //}

        #endregion
        #region[Final Course Result]
        public ActionResult FinalCourseResult(int? id = 0)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new FinalCourseResultModel()
            {
                Courses = CourseService.Get(a => a.StartDate >= timenow && a.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.CourseResult && x.int_id_status == (int)UtilConstants.EStatus.Approve) && a.IsDeleted != true).OrderByDescending(b => b.StartDate).ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name)),
                //Courses =
                //    _repoTmsApproves.Get(
                //        a => a.Course.IsDeleted == false,
                //        (int)UtilConstants.ApproveType.CourseResult,
                //        (int)UtilConstants.EStatus.Approve
                //        )
                //        .OrderByDescending(b => b.Course.StartDate).OrderByDescending(a => a.int_Course_id)
                //        .ToDictionary(c => c.Course.Id, c => string.Format("{0} - {1}", c.Course.Code, c.Course.Name))
            };

            return View(model);
        }
        public ActionResult FinalCourseResultPrint(int id = 0)
        {
            var courseId = string.IsNullOrEmpty(Request.QueryString["courseId"]) ? -1 : Convert.ToInt32(Request.QueryString["courseId"].Trim());
            var course = CourseService.GetById(courseId);
            IEnumerable<Course_Detail> course_Details = CourseDetailService.Get(a => a.CourseId == course.Id);
            var course_Result_Final = CourseService.GetCourseResultFinal(a => a.courseid == course.Id).OrderByDescending(a => a.grade == (int)UtilConstants.Grade.Distinction).ThenByDescending(a => a.grade == (int)UtilConstants.Grade.Pass).ThenByDescending(a => a.grade == (int)UtilConstants.Grade.Fail).ThenByDescending(a => a.point).GroupBy(v => new { v.traineeid, v.point, v.grade }).Select(c => c.FirstOrDefault());
            var data_ = course_Details.Select(b => b.Id);
            var data_course_member = CourseMemberService.Get(a => data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true);
            if (data_course_member.Count() > 0)
            {
                course_Result_Final = course_Result_Final.Where(a => data_course_member.Any(b => b.Member_Id == a.traineeid && b.Course_Detail.CourseId == a.courseid));
            }
            int No_Certificate = course_Result_Final.Count(a => a.grade != (int)UtilConstants.Grade.Fail);

            var model = new FinalCourseResultModeltModelRp();
            model.header = GetByKey("Final_Course_Result_Header").Split(',');
            var subjectResult = CourseService.GetCourseResult(a => a.CourseDetailId.HasValue && data_.Contains(a.CourseDetailId.Value));

            var data = course_Result_Final;
            if (course != null)
            {
                model.CourseName = course.Name;
                model.CourseCode = course.Code;
                model.Venue = course.Venue;
                model.TimeFrom = course.StartDate.Value.ToString("dd/MM/yyyy") ?? "";
                model.TimeTo = course.EndDate.Value.ToString("dd/MM/yyyy") ?? "";
                model.Trainees = data.OrderByDescending(p => p.grade).ThenByDescending(p => p.point).AsEnumerable().Select(a => new FinalCourseResultModeltModelRp.TraineeModelRp()
                {
                    //FullName = a?.Trainee?.FirstName + " " + a?.Trainee?.LastName,
                    FullName = ReturnDisplayLanguage(a?.Trainee?.FirstName, a?.Trainee?.LastName),
                    Eid = a?.Trainee?.str_Staff_Id,
                    DepartmentCode = a?.Trainee?.Department.Code,
                    Point = a?.point.ToString(),
                    Grace = GetGrace(a?.grade),
                    Id = a?.traineeid,
                    ReMark = a?.remark
                });

            }
            ViewBag.course_Details = course_Details;
            ViewBag.Course = course;
            ViewBag.No_Certificate = No_Certificate;
            ViewBag.subjectResult = subjectResult;
            //ViewBag.trainingcenter = trainingcenter;
            return PartialView("FinalCourseResultPrint", model);
        }

        public ActionResult AjaxHandleListResultFinal(jQueryDataTableParamModel param)
        {
            try
            {
                var courseList = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());

                var data_ = CourseDetailService.Get(a => a.CourseId == courseList && a.IsDeleted != true).Select(b => b.Id);
                var data = CourseService.GetCourseResultFinal(a => a.courseid == courseList && a.Trainee.TMS_Course_Member.Any(x => data_.Contains((int)x.Course_Details_Id) && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved))).OrderByDescending(a => a.grade == (int)UtilConstants.Grade.Distinction).ThenByDescending(a => a.grade == (int)UtilConstants.Grade.Pass).ThenByDescending(a => a.grade == (int)UtilConstants.Grade.Fail).ThenByDescending(a => a.point).GroupBy(v => new { v.traineeid, v.point, v.grade }).Select(c => c.FirstOrDefault());
                IEnumerable<Course_Result_Final> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Result_Final, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName)
                                                            : sortColumnIndex == 3 ? c?.Trainee?.JobTitle?.Name
                                                            : sortColumnIndex == 4 ? c?.Trainee?.Department?.Name
                                                            : sortColumnIndex == 5 ? (object)c?.point
                                                            : sortColumnIndex == 6 ? (object)c?.grade
                                                            : (object)c.grade);



                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction).ThenByDescending(p => p.point)
                                : filtered.OrderByDescending(orderingFunction).ThenByDescending(p => p.point);
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                 string.Empty,
                                                c?.Trainee?.str_Staff_Id,
                                                //c?.Trainee?.FirstName + " " + c?.Trainee?.LastName,
                                                ReturnDisplayLanguage(c?.Trainee?.FirstName ,c?.Trainee?.LastName),
                                                c?.Trainee?.JobTitle?.Name,
                                                c?.Trainee?.Department?.Name,
                                                (c?.point != null && (double)c?.point != -1) ? string.Format("{0:0.#}", c?.point) : string.Empty,
                                                GetGrade(c?.grade),
                                                c?.remark ?? "",
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandleListResultFinal", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        private string GetGrade(int? grade)
        {
            var result = "Fail";
            switch (grade)
            {
                case (int)UtilConstants.Grade.Fail:
                    result = "Fail";
                    break;
                case (int)UtilConstants.Grade.Pass:
                    result = "Pass";
                    break;
                case (int)UtilConstants.Grade.Distinction:
                    result = "Distinction";
                    break;
                default:
                    result = string.Empty;
                    break;
            }
            return result;
        }

        [HttpPost]
        public FileContentResult ExportFinalCourseResult(FormCollection form)
        {
            int courseId = string.IsNullOrEmpty(form["CourseList"]) ? -1 : Convert.ToInt32(form["CourseList"].Trim());
            byte[] filecontent = ExportExcelFinalCourseResult(courseId);
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "FinalCourseResult.xlsx");
            }
            return null;
        }

        private byte[] ExportExcelFinalCourseResult1(int ddlSubject)
        {
            //Viet lai
            byte[] bytes = null;
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/FinalCourseResult.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            using (xlPackage = new ExcelPackage(template, false))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                Course course = CourseService.GetById(ddlSubject);
                var course_Result_Final = CourseService.GetCourseResultFinal(a => a.courseid == ddlSubject && a.IsDeleted == false, new int[] { (int)UtilConstants.ApproveType.CourseResult }, (int)UtilConstants.EStatus.Approve).OrderByDescending(a => a.courseid).OrderByDescending(a => a.point).ToList();

                #region [Xuat thong tin Course Name (row 6), Course Code (row 7), Venue (row 8), Date(row 9)]
                ExcelRange cellCourseName = worksheet.Cells[6, 4];
                cellCourseName.Value = course?.Name;
                cellCourseName.Style.Font.Bold = true;

                ExcelRange cellCourseCode = worksheet.Cells[7, 4];
                cellCourseCode.Value = course?.Code;
                cellCourseCode.Style.Font.Bold = true;

                ExcelRange cellVenue = worksheet.Cells[8, 4];
                cellVenue.Value = course?.Venue;
                cellVenue.Style.Font.Bold = true;

                ExcelRange cellDate = worksheet.Cells[9, 4];
                cellDate.Value = course?.StartDate.Value.ToString("dd/MM/yyyy") + " - " + course?.EndDate.Value.ToString("dd/MM/yyyy");
                cellDate.Style.Font.Bold = true;
                #endregion

                List<Course_Detail> course_Details = CourseDetailService.Get(a => a.CourseId == course.Id, new[] { (int)UtilConstants.ApproveType.AssignedTrainee, (int)UtilConstants.ApproveType.CourseResult }).ToList();
                int countSubject = course_Details.Count;

                #region [Tạo các tiêu đề cột từ tên Subject cho đến Remark (phụ thuộc vào số lượng môn)]
                ExcelRange cellPoints = worksheet.Cells[11, 6, 11, 6 + (countSubject * 2) - 1];
                cellPoints.Merge = true;
                cellPoints.Value = "Points";
                cellPoints.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellPoints.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellPoints.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellPoints.Style.Font.Bold = true;

                ExcelRange cellAverage = worksheet.Cells[11, 6 + (countSubject * 2), 13, 6 + (countSubject * 2)];
                cellAverage.Merge = true;
                cellAverage.Value = "Average Point";
                cellAverage.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellAverage.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellAverage.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellAverage.Style.Font.Bold = true;

                ExcelRange cellGrade = worksheet.Cells[11, 6 + (countSubject * 2) + 1, 13, 6 + (countSubject * 2) + 1];
                cellGrade.Merge = true;
                cellGrade.Value = "Grade";
                cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellGrade.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellGrade.Style.Font.Bold = true;

                ExcelRange cellRemarks = worksheet.Cells[11, 6 + (countSubject * 2) + 2, 13, 6 + (countSubject * 2) + 2];
                cellRemarks.Merge = true;
                cellRemarks.Value = "Remarks";
                cellRemarks.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellRemarks.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellRemarks.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellRemarks.Style.Font.Bold = true;

                for (int i = 1; i <= countSubject; i++)
                {
                    ExcelRange cellSubjectName = worksheet.Cells[12, 6 + (i * 2) - 2, 12, 6 + (i * 2) - 1];
                    cellSubjectName.Merge = true;
                    cellSubjectName.Value = course_Details[i - 1]?.SubjectDetail?.Name ?? "";
                    cellSubjectName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellSubjectName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellSubjectName.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellSubjectName.Style.Font.Bold = true;

                    ExcelRange cellFirstCheck = worksheet.Cells[13, 6 + (i * 2) - 2];
                    cellFirstCheck.Value = "First Check";
                    cellFirstCheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellFirstCheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellFirstCheck.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellFirstCheck.Style.Font.Bold = true;

                    ExcelRange cellReCheck = worksheet.Cells[13, 6 + (i * 2) - 1];
                    cellReCheck.Value = "ReCheck";
                    cellReCheck.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellReCheck.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellReCheck.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    cellReCheck.Style.Font.Bold = true;
                }
                #endregion

                int startRow = 14; // Dong bat dau do du lieu vo file Excel
                foreach (var item in course_Details)
                {
                    List<Course_Result> course_Results = item.Course_Result.Where(a => a.CourseDetailId == item.Id).ToList();
                    foreach (var item1 in course_Results)
                    {
                        if (course_Result_Final.Any(a => a.traineeid == item1.TraineeId))
                        {
                            #region [Chay vong lap xuong nhap du lieu vo file excel]
                            ExcelRange cellNo = worksheet.Cells[startRow, 2];
                            cellNo.Value = startRow - 13;
                            cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellTraineeName = worksheet.Cells[startRow, 3];
                            cellTraineeName.Value = ReturnDisplayLanguage(item1?.Trainee?.FirstName, item1?.Trainee?.LastName);
                            cellTraineeName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellTraineeName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellTraineeName.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            cellTraineeName.Style.WrapText = true;

                            ExcelRange cellEID = worksheet.Cells[startRow, 4];
                            cellEID.Value = item1?.Trainee?.str_Staff_Id ?? "";
                            cellEID.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellEID.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellEID.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellDept = worksheet.Cells[startRow, 5];
                            cellDept.Value = item1?.Trainee?.Department?.Name ?? "";
                            cellDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            cellDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellDept.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                            for (int i = 1; i <= countSubject; i++) // Vong lap chay ngang, de show diem cua tung Subject
                            {
                                ExcelRange cellFirstCheckScore = worksheet.Cells[startRow, 6 + (i * 2) - 2];
                                cellFirstCheckScore.Value = item1?.First_Check_Score == null ? item1?.First_Check_Result : item1?.First_Check_Score.ToString();
                                cellFirstCheckScore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellFirstCheckScore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellFirstCheckScore.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                                ExcelRange cellReCheckScore = worksheet.Cells[startRow, 6 + (i * 2) - 1];
                                cellReCheckScore.Value = item1?.Re_Check_Score == null ? item1?.Re_Check_Result : item1?.Re_Check_Score.ToString();
                                cellReCheckScore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellReCheckScore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellReCheckScore.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            }

                            ExcelRange cellAveragePoint = worksheet.Cells[startRow, 6 + (countSubject * 2)];
                            cellAveragePoint.Value = course_Result_Final.FirstOrDefault(a => a.traineeid == item1?.Trainee?.Id)?.point.HasValue == true ? float.Parse(course_Result_Final.FirstOrDefault(a => a.traineeid == item1?.Trainee?.Id)?.point?.ToString()).ToString() : "";
                            cellAveragePoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellAveragePoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellAveragePoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellGradeValue = worksheet.Cells[startRow, 6 + (countSubject * 2) + 1];
                            cellGradeValue.Value = GetGrade(course_Result_Final.FirstOrDefault(a => a.traineeid == item1?.Trainee?.Id)?.grade ?? 0).ToString() ?? "";
                            cellGradeValue.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellGradeValue.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellGradeValue.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            ExcelRange cellRemarkValue = worksheet.Cells[startRow, 6 + (countSubject * 2) + 2];
                            cellRemarkValue.Value = course_Result_Final.FirstOrDefault(a => a.traineeid == item1?.Trainee?.Id)?.remark ?? "";
                            cellRemarkValue.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellRemarkValue.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellRemarkValue.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            #endregion

                            startRow++;
                        }
                    }
                }
                int No_Certificates = course_Result_Final.Where(a => !string.IsNullOrEmpty(a.certificatefinal) && a.statusCertificate == 0).ToList().Count;
                ExcelRange cellNoCertificates = worksheet.Cells[startRow + 2, 3, startRow + 2, 4];
                cellNoCertificates.Merge = true;
                cellNoCertificates.Value = "Number of certificates issued: " + No_Certificates.ToString() ?? "";
                cellNoCertificates.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                cellNoCertificates.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellNoCertificates.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                //Merge Title
                ExcelRange cellMergeTitle = worksheet.Cells[2, 4, 4, 6 + (countSubject * 2)];
                cellMergeTitle.Merge = true;
                cellMergeTitle.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                string[] header = GetByKey("Final_Course_Result_Header").Split(',');
                ExcelRange cellTextNumber = worksheet.Cells[2, 6 + (countSubject * 2) + 1, 4, 6 + (countSubject * 2) + 2];
                cellTextNumber.Merge = true;
                cellTextNumber.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                cellTextNumber.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                cellTextNumber.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellTextNumber.AutoFitColumns();
                cellTextNumber.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellTextNumber.Style.WrapText = true;

                #region [Xuat thong tin co dinh o phia duoi cung file Excel]
                ExcelRange cellApprovedby = worksheet.Cells[startRow + 4, 2, startRow + 4, 3];
                cellApprovedby.Merge = true;
                cellApprovedby.Value = "Approved by";
                cellApprovedby.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellApprovedby.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ExcelRange cellHEADOFTRAINING = worksheet.Cells[startRow + 5, 2, startRow + 5, 3];
                cellHEADOFTRAINING.Merge = true;
                cellHEADOFTRAINING.Value = "HEAD OF TRAINING";
                cellHEADOFTRAINING.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellHEADOFTRAINING.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellHEADOFTRAINING.Style.Font.Bold = true;

                ExcelRange cellDateLeft = worksheet.Cells[startRow + 6, 2, startRow + 6, 3];
                cellDateLeft.Merge = true;
                cellDateLeft.Value = "Date .....................................";
                cellDateLeft.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellDateLeft.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                ExcelRange cellPreparedby = worksheet.Cells[startRow + 4, 6 + (countSubject * 2), startRow + 4, 6 + (countSubject * 2) + 1];
                cellPreparedby.Merge = true;
                cellPreparedby.Value = "Prepared by";
                cellPreparedby.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellPreparedby.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                ExcelRange cellVTCTRAININGMANAGER = worksheet.Cells[startRow + 5, 6 + (countSubject * 2), startRow + 5, 6 + (countSubject * 2) + 1];
                cellVTCTRAININGMANAGER.Merge = true;
                cellVTCTRAININGMANAGER.Value = "VTC TRAINING MANAGER";
                cellVTCTRAININGMANAGER.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellVTCTRAININGMANAGER.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellVTCTRAININGMANAGER.Style.Font.Bold = true;

                ExcelRange cellDateRight = worksheet.Cells[startRow + 6, 6 + (countSubject * 2), startRow + 6, 6 + (countSubject * 2) + 1];
                cellDateRight.Merge = true;
                cellDateRight.Value = "Date .....................................";
                cellDateRight.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellDateRight.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                #endregion

                bytes = xlPackage.GetAsByteArray();
            }

            // Code Cu
            //var course = CourseService.GetById(ddlSubject);
            //var data = CourseService.GetCourseResultFinal(a => a.courseid == ddlSubject, new[] { (int)UtilConstants.ApproveType.AssignedTrainee, (int)UtilConstants.ApproveType.CourseResult }).OrderByDescending(a => a.point).ToList();


            //byte[] bytes = null;
            //string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/FinalCourseResult.xlsx");
            //FileInfo template = new FileInfo(templateFilePath);
            //ExcelPackage xlPackage;
            //MemoryStream MS = new MemoryStream();

            //using (xlPackage = new ExcelPackage(template, false))
            //{

            //    ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
            //    #region Write Header

            //    int startRow = 6;

            //    ExcelRange cellHeaderCourseName = worksheet.Cells[startRow, 3];
            //    cellHeaderCourseName.Value = course?.Name;
            //    cellHeaderCourseName.Style.Font.Bold = true;

            //    ExcelRange cellHeaderSubjectName = worksheet.Cells[++startRow, 3];
            //    cellHeaderSubjectName.Value = course?.Course_Detail?.FirstOrDefault()?.SubjectDetail.Name;
            //    cellHeaderSubjectName.Style.Font.Bold = true;

            //    ExcelRange cellHeaderVenue = worksheet.Cells[++startRow, 3];
            //    cellHeaderVenue.Value = course?.Venue;
            //    cellHeaderVenue.Style.Font.Bold = true;

            //    var date = course?.StartDate.Value.ToString("dd/MM/yyyy") ?? "" + " - " + course?.EndDate.Value.ToString("dd/MM/yyyy") ?? "";
            //    ExcelRange cellHeaderDuration = worksheet.Cells[++startRow, 3];
            //    cellHeaderDuration.Value = date;
            //    cellHeaderDuration.Style.Font.Bold = true;
            //    #endregion

            //    int count = 0;
            //    startRow = 12;

            //    foreach (var item in data)
            //    {
            //        int col = 2;
            //        count++;
            //        ExcelRange cellNo = worksheet.Cells[startRow, col];
            //        cellNo.Value = count;
            //        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        ExcelRange celStaffId = worksheet.Cells[startRow, ++col];
            //        celStaffId.Value = item.Trainee.str_Staff_Id;
            //        celStaffId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //        celStaffId.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        celStaffId.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        //var fullName = item.Trainee.FirstName + " " + item.Trainee.LastName;
            //        var fullName = ReturnDisplayLanguage(item.Trainee.FirstName, item.Trainee.LastName);
            //        ExcelRange cellFullName = worksheet.Cells[startRow, ++col];
            //        cellFullName.Value = fullName;
            //        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        ExcelRange cellJobTitle = worksheet.Cells[startRow, ++col];
            //        cellJobTitle.Value = item?.Trainee?.JobTitle?.Name.Trim();
            //        cellJobTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //        cellJobTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        cellJobTitle.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        ExcelRange cellDeppartment = worksheet.Cells[startRow, ++col];
            //        cellDeppartment.Value = item.Trainee.Department.Code;
            //        cellDeppartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        cellDeppartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        cellDeppartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        ExcelRange cellPoint = worksheet.Cells[startRow, ++col];
            //        cellPoint.Value = (item?.point != 0 && item?.point != null) ? string.Format("{0:0.#}", item?.point) : string.Empty;
            //        cellPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        cellPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        cellPoint.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        ExcelRange cellGrade = worksheet.Cells[startRow, ++col];
            //        cellGrade.Value = GetGrace(item.grade);
            //        cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        cellGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        cellGrade.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        ExcelRange cellRemark = worksheet.Cells[startRow, ++col];
            //        cellRemark.Value = item?.remark ?? "";
            //        cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //        cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //        cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            //        startRow++;
            //    }

            //    //#region Write Footer
            //    //ExcelRange cell = worksheet.Cells[startrow + num + 1, endDynamicColumn + 2];
            //    //cell.Value = "Number of certificates issued: " + count.ToString();
            //    //cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            //    //worksheet.Cells[startrow + num + 3, 1 + 1, startrow + num + 3, 1 + 2].Merge = true;
            //    //ExcelRange cellfooterleft1 = worksheet.Cells[startrow + num + 3, 1 + 1];
            //    //cellfooterleft1.Value = "Approved by";
            //    //worksheet.Cells[startrow + num + 3, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //    //worksheet.Cells[startrow + num + 3, 1 + 1].Style.Font.Size = 11;

            //    //worksheet.Cells[startrow + num + 4, 1 + 1, startrow + num + 4, 1 + 2].Merge = true;
            //    //ExcelRange cellfooterleft2 = worksheet.Cells[startrow + num + 4, 1 + 1];
            //    //cellfooterleft2.Value = "HEAD OF TRAINING";
            //    //worksheet.Cells[startrow + num + 4, 1 + 1].Style.Font.Bold = true;
            //    //worksheet.Cells[startrow + num + 4, 1 + 1].Style.Font.Size = 12;
            //    //worksheet.Cells[startrow + num + 4, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //    //worksheet.Cells[startrow + num + 5, 1 + 1, startrow + num + 5, 1 + 2].Merge = true;
            //    //ExcelRange cellfooterleft3 = worksheet.Cells[startrow + num + 5, 1 + 1];
            //    //cellfooterleft3.Value = "Date .....................................";
            //    //worksheet.Cells[startrow + num + 5, 1 + 1].Style.Font.Size = 12;
            //    //worksheet.Cells[startrow + num + 5, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            //    //worksheet.Cells[startrow + num + 3, endDynamicColumn + 1, startrow + num + 3, endDynamicColumn + 2].Merge = true;
            //    //ExcelRange cellfooterright1 = worksheet.Cells[startrow + num + 3, endDynamicColumn + 1];
            //    //cellfooterright1.Value = "Prepared by";
            //    //worksheet.Cells[startrow + num + 3, endDynamicColumn + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //    //worksheet.Cells[startrow + num + 3, endDynamicColumn + 1].Style.Font.Size = 11;

            //    //worksheet.Cells[startrow + num + 4, endDynamicColumn + 1, startrow + num + 4, endDynamicColumn + 2].Merge = true;
            //    //ExcelRange cellfooterright2 = worksheet.Cells[startrow + num + 4, endDynamicColumn + 1];
            //    //cellfooterright2.Value = trainingcenter + " TRAINING MANAGER";
            //    //worksheet.Cells[startrow + num + 4, endDynamicColumn + 1].Style.Font.Bold = true;
            //    //worksheet.Cells[startrow + num + 4, endDynamicColumn + 1].Style.Font.Size = 12;
            //    //worksheet.Cells[startrow + num + 4, endDynamicColumn + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //    //worksheet.Cells[startrow + num + 5, endDynamicColumn + 1, startrow + num + 5, endDynamicColumn + 2].Merge = true;
            //    //ExcelRange cellfooterright3 = worksheet.Cells[startrow + num + 5, endDynamicColumn + 1];
            //    //cellfooterright3.Value = " Date .....................................";
            //    //worksheet.Cells[startrow + num + 5, endDynamicColumn + 1].Style.Font.Size = 12;
            //    //worksheet.Cells[startrow + num + 5, endDynamicColumn + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //    //#endregion

            //    //#region[header]
            //    //using (ExcelRange r = worksheet.Cells[2, 1 + 1, 4, dt.Columns.Count - 1 + 1])
            //    //{
            //    //    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //    //    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //    //    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //    //    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //    //    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
            //    //    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
            //    //    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
            //    //    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
            //    //}
            //    //using (ExcelRange r = worksheet.Cells[row_1, 1 + 1, startrow + num - 1, dt.Columns.Count - 1])
            //    //{
            //    //    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //    //    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //    //    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //    //    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //    //    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
            //    //    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
            //    //    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
            //    //    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
            //    //}
            //    //worksheet.Cells[2, 1 + 1, 4, 1 + 2].Merge = true;
            //    //worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //    //worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //    //worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            //    //worksheet.Cells[2, 4, 4, endDynamicColumn].Merge = true;
            //    //worksheet.Cells[2, 4, 4, endDynamicColumn].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //    //worksheet.Cells[2, 4, 4, endDynamicColumn].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //    //worksheet.Cells[2, 4, 4, endDynamicColumn].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //    //worksheet.Cells[2, 4, 4, endDynamicColumn].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //    //worksheet.Cells[2, endDynamicColumn + 1, 4, endDynamicColumn + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //    //worksheet.Cells[2, endDynamicColumn + 1, 4, endDynamicColumn + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //    //worksheet.Cells[2, endDynamicColumn + 1, 4, endDynamicColumn + 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //    //ExcelRange celltitleheader = worksheet.Cells[2, 4];
            //    //celltitleheader.Value = "FINAL COURSE RESULT";
            //    //celltitleheader.Style.Font.Bold = true;
            //    //celltitleheader.Style.Font.Size = 15;
            //    //worksheet.Cells[2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //    //worksheet.Cells[2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            //    //ExcelRange celltitleheader1 = worksheet.Cells[2, 1 + columnMerge1 + 3];
            //    //celltitleheader1.Value = "FORM: VJC­VTC­F­011";
            //    //celltitleheader1.Style.Font.Size = 10;
            //    //worksheet.Cells[2, 1 + columnMerge1 + 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            //    //ExcelRange celltitleheader2 = worksheet.Cells[3, 1 + columnMerge1 + 3];
            //    //celltitleheader2.Value = "Iss01/Rev01";
            //    //celltitleheader2.Style.Font.Size = 10;
            //    //worksheet.Cells[3, 1 + columnMerge1 + 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            //    //ExcelRange celltitleheader3 = worksheet.Cells[4, 1 + columnMerge1 + 3];
            //    //celltitleheader3.Value = "1/1/2017";
            //    //celltitleheader3.Style.Font.Size = 10;
            //    //worksheet.Cells[4, 1 + columnMerge1 + 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            //    //#endregion
            //    bytes = xlPackage.GetAsByteArray();
            //}

            return bytes;
        }

        private byte[] ExportExcelFinalCourseResult(int ddl_subject)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            //DataTable dt = CMSUtils.GetDocumentsByStore(new string[] { "ListCourseID" }, new object[] { ddl_subject.ToString() }, "sp_GetFinalCourseResult");
            Course data = CourseService.GetById(ddl_subject);
            var courseDetails = CourseDetailService.Get(a => a.CourseId == ddl_subject);
            var data_coursedetail = courseDetails.Select(b => b.Id);
            //var data_course_member = CourseMemberService.Get(a => data_coursedetail.Contains((int)a.Course_Details_Id) && a.IsActive == true && a.IsDelete == false && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderByDescending(p => p.Id);

            var courseResultFinal = CourseService.GetCourseResultFinal(a => a.courseid == ddl_subject && a.IsDeleted != true && a.Trainee.TMS_Course_Member.Any(x => data_coursedetail.Contains((int)x.Course_Details_Id) && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved))).OrderByDescending(a => a.grade == (int)UtilConstants.Grade.Distinction).ThenByDescending(a => a.grade == (int)UtilConstants.Grade.Pass).ThenByDescending(a => a.grade == (int)UtilConstants.Grade.Fail).ThenByDescending(a => a.point).GroupBy(v => new { v.traineeid, v.point, v.grade }).Select(c => c.FirstOrDefault());
            //if (data_course_member.Any())
            //{
            //    courseResultFinal = courseResultFinal.Where(a => data_course_member.Any(b => b.Member_Id == a.traineeid && b.Course_Detail.CourseId == a.courseid));
            //}
            //var subjectResult = CourseService.GetCourseResult(a => a.CourseDetailId.HasValue && data_coursedetail.Contains(a.CourseDetailId.Value));

            byte[] Bytes = null;
            int startrow = 13;
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/FinalCourseResult.xlsx");//Server.MapPath(@"~/ExcelFile/FinalCourseResult.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            //int startDynamicColumn = dt.Columns["DepartmentCode"].Ordinal;
            //int endDynamicColumn = (dt.Columns["point"].Ordinal);
            //int columnMerge = endDynamicColumn - (startDynamicColumn + 2);
            //if (dt.Rows.Count == 0)
            //    columnMerge = 0;
            //int count = 0;

            //string[] header = GetConfig.ByKey("Course_Result_Header").Split(',');

            using (xlPackage = new ExcelPackage(template, false))
            {
                //int columnMerge1 = 5 + ((columnMerge <= 0 ? 1 : columnMerge) - 1);

                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                #region Write Header

                ExcelRange cellHeaderCourseName = worksheet.Cells[5, 3];
                cellHeaderCourseName.Value = data?.Name;
                cellHeaderCourseName.Style.Font.Bold = true;

                ExcelRange cellHeaderCourseCode = worksheet.Cells[6, 3];
                cellHeaderCourseCode.Value = data?.Code;
                cellHeaderCourseCode.Style.Font.Bold = true;

                ExcelRange cellHeaderSubjectName = worksheet.Cells[7, 3];
                cellHeaderSubjectName.Value = data?.Venue;
                cellHeaderSubjectName.Style.Font.Bold = true;

                ExcelRange cellHeaderDuration = worksheet.Cells[8, 3];
                cellHeaderDuration.Value = data?.StartDate.Value.ToString("dd/MM/yyyy") + " - " + data?.EndDate.Value.ToString("dd/MM/yyyy");
                cellHeaderDuration.Style.Font.Bold = true;
                #endregion

                #region Write Details

                #region Wirte Captions 

                int row_1 = 10;
                int row_1_point_5 = 11;
                int row_2 = 12;
                int col_default = 1;
                worksheet.Cells[row_1, col_default + 1, row_2, col_default + 1].Merge = true;
                ExcelRange cellNO = worksheet.Cells[row_1, col_default + 1];
                cellNO.Value = "No.";
                cellNO.Style.Font.Bold = true;
                cellNO.Style.Font.Size = 11;
                cellNO.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellNO.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + 2, row_2, col_default + 2].Merge = true;
                ExcelRange cellName = worksheet.Cells[row_1, col_default + 2];
                cellName.Value = "Name";
                cellName.Style.Font.Bold = true;
                cellName.Style.Font.Size = 11;
                cellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + 3, row_2, col_default + 3].Merge = true;
                ExcelRange cellStaff = worksheet.Cells[row_1, col_default + 3];
                cellStaff.Value = "EID";
                cellStaff.Style.Font.Bold = true;
                cellStaff.Style.Font.Size = 11;
                cellStaff.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellStaff.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + 4, row_2, col_default + 4].Merge = true;
                ExcelRange cellDept = worksheet.Cells[row_1, col_default + 4];
                cellDept.Value = "Dept";
                cellDept.Style.Font.Bold = true;
                cellDept.Style.Font.Size = 11;
                cellDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                //
                int genDynamicColumn = 5;

                foreach (var item in courseDetails)
                {
                    worksheet.Cells[row_1_point_5, col_default + genDynamicColumn, row_1_point_5, col_default + genDynamicColumn + 1].Merge = true;
                    ExcelRange CellName = worksheet.Cells[row_1_point_5, col_default + genDynamicColumn];
                    CellName.Value = item?.SubjectDetail?.Name;
                    CellName.Style.Font.Bold = true;
                    CellName.Style.Font.Size = 11;
                    CellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    CellName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellpointdynamic = worksheet.Cells[row_2, col_default + genDynamicColumn];
                    cellpointdynamic.Value = "First Attempt";
                    //cellpointdynamic.Style.Font.Bold = true;
                    cellpointdynamic.Style.Font.Size = 11;
                    cellpointdynamic.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellpointdynamic.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    genDynamicColumn++;

                    ExcelRange cellpointdynamicRe = worksheet.Cells[row_2, col_default + genDynamicColumn];
                    cellpointdynamicRe.Value = "Second Attempt";
                    //cellpointdynamicRe.Style.Font.Bold = true;
                    cellpointdynamicRe.Style.Font.Size = 11;
                    cellpointdynamicRe.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellpointdynamicRe.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genDynamicColumn++;
                }

                genDynamicColumn--;


                worksheet.Cells[row_1, col_default + 5, row_1, col_default + genDynamicColumn].Merge = true;
                ExcelRange cellpoint = worksheet.Cells[row_1, col_default + 5];
                cellpoint.Value = "Result";
                cellpoint.Style.Font.Bold = true;
                cellpoint.Style.Font.Size = 11;
                cellpoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellpoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + genDynamicColumn + 1, row_2, col_default + genDynamicColumn + 1].Merge = true;
                ExcelRange cellAVG = worksheet.Cells[row_1, col_default + genDynamicColumn + 1];
                cellAVG.Value = "Average Score";
                cellAVG.Style.Font.Bold = true;
                cellAVG.Style.Font.Size = 11;
                cellAVG.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellAVG.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + genDynamicColumn + 2, row_2, col_default + genDynamicColumn + 2].Merge = true;
                ExcelRange cellGrade = worksheet.Cells[row_1, col_default + genDynamicColumn + 2];
                cellGrade.Value = "Grade";
                cellGrade.Style.Font.Bold = true;
                cellGrade.Style.Font.Size = 11;
                cellGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[row_1, col_default + genDynamicColumn + 3, row_2, col_default + genDynamicColumn + 3].Merge = true;
                ExcelRange cellRemark = worksheet.Cells[row_1, col_default + genDynamicColumn + 3];
                cellRemark.Value = "Remarks";
                cellRemark.Style.Font.Bold = true;
                cellRemark.Style.Font.Size = 11;
                cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                #endregion
                int row = 1;

                int x = 0;
                foreach (var item in courseResultFinal.OrderByDescending(a => a.grade).ThenByDescending(p => p.point))
                {
                    row = startrow + x;
                    int genCols = 1;
                    ExcelRange cellRowNo = worksheet.Cells[row, col_default + genCols];
                    cellRowNo.Value = x + 1;
                    cellRowNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellRowNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;

                    //Trainee Name
                    ExcelRange cellTraineeName = worksheet.Cells[row, col_default + genCols];
                    cellTraineeName.Value = ReturnDisplayLanguage(item?.Trainee?.FirstName, item?.Trainee?.LastName);
                    cellTraineeName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellTraineeName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;


                    //Trainee ID
                    ExcelRange cellEid = worksheet.Cells[row, col_default + genCols];
                    cellEid.Value = item?.Trainee?.str_Staff_Id;
                    cellEid.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellEid.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;


                    //Trainee Department
                    ExcelRange cellTraineeDept = worksheet.Cells[row, col_default + genCols];
                    cellTraineeDept.Value = item?.Trainee?.Department?.Code;
                    cellTraineeDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTraineeDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;


                    //Result
                    int dynamicCol = 5;
                    //var traineeResult = subjectResult.Where(a => a.TraineeId == item.traineeid);
                    foreach (var detail in courseDetails)
                    {
                        var resultbycourseDetail = detail.Course_Result.FirstOrDefault(a => a.CourseDetailId == detail.Id && a.TraineeId == item.traineeid);
                        ExcelRange cellTraineeScore = worksheet.Cells[row, col_default + dynamicCol];
                        cellTraineeScore.Value = detail.SubjectDetail.IsAverageCalculate == false ? resultbycourseDetail?.First_Check_Result : resultbycourseDetail?.First_Check_Score?.ToString().Replace("-1", "0");
                        cellTraineeScore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTraineeScore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        dynamicCol++;
                        genCols++;

                        ExcelRange cellTraineeRescore = worksheet.Cells[row, col_default + dynamicCol];
                        cellTraineeRescore.Value = detail.SubjectDetail.IsAverageCalculate == false ? resultbycourseDetail?.Re_Check_Result : resultbycourseDetail?.Re_Check_Score?.ToString().Replace("-1", "0");
                        cellTraineeRescore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTraineeRescore.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        dynamicCol++;
                        genCols++;

                    }

                    //Trainee Average Point
                    ExcelRange cellTraineeAvgPoint = worksheet.Cells[row, col_default + genCols];
                    cellTraineeAvgPoint.Value = item.point.HasValue && item.point != 0 ? Math.Round(item.point.Value, 1).ToString() : "0"; //String.Format("{0:0.#}", item.point)
                    cellTraineeAvgPoint.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTraineeAvgPoint.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;

                    //Trainee Grade
                    ExcelRange cellTraineeGrade = worksheet.Cells[row, col_default + genCols];
                    cellTraineeGrade.Value = GetGrade(item.grade).ToString() ?? "";
                    cellTraineeGrade.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellTraineeGrade.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    genCols++;

                    //Trainee Remarks
                    //ExcelRange cellTraineeRemarks = worksheet.Cells[row, col_default + genCols];
                    //cellTraineeRemarks.Value = item.rema = ExcelHorizontalAlignment.Center;
                    //cellTraineeRemarks.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //genCols++;

                    x++;
                }

                #endregion

                #region Write Footer
                ExcelRange cell = worksheet.Cells[startrow + x + 1, genDynamicColumn + 3];
                cell.Value = "Number of certificates to be issued: " + courseResultFinal.Count(a => a.grade != (int)UtilConstants.Grade.Fail);//count.ToString();
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                // Approve  by
                worksheet.Cells[startrow + x + 3, 1 + 1, startrow + x + 3, 1 + 2].Merge = true;
                ExcelRange cellfooterleft1 = worksheet.Cells[startrow + x + 3, 1 + 1];
                cellfooterleft1.Value = "Approved by";
                worksheet.Cells[startrow + x + 3, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[startrow + x + 3, 1 + 1].Style.Font.Size = 11;

                worksheet.Cells[startrow + x + 4, 1 + 1, startrow + x + 4, 1 + 2].Merge = true;
                ExcelRange cellfooterleft2 = worksheet.Cells[startrow + x + 4, 1 + 1];
                cellfooterleft2.Value = "HEAD OF TRAINING";
                worksheet.Cells[startrow + x + 4, 1 + 1].Style.Font.Bold = true;
                worksheet.Cells[startrow + x + 4, 1 + 1].Style.Font.Size = 12;
                worksheet.Cells[startrow + x + 4, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[startrow + x + 8, 1 + 1, startrow + x + 8, 1 + 2].Merge = true;
                ExcelRange cellfooterleft3 = worksheet.Cells[startrow + x + 8, 1 + 1];
                cellfooterleft3.Value = "Date .....................................";
                worksheet.Cells[startrow + x + 5, 1 + 1].Style.Font.Size = 12;
                worksheet.Cells[startrow + x + 5, 1 + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Verified by
                worksheet.Cells[startrow + x + 3, genDynamicColumn/2 + 2, startrow + x + 3, genDynamicColumn/2 + 3].Merge = true;
                ExcelRange cellfootercenter1 = worksheet.Cells[startrow + x + 3, genDynamicColumn/2 + 2];
                cellfootercenter1.Value = "Verified by";
                cellfootercenter1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellfootercenter1.Style.Font.Size = 11;

                worksheet.Cells[startrow + x + 4, genDynamicColumn/2 + 2, startrow + x + 4, genDynamicColumn/2 + 3].Merge = true;
                ExcelRange cellfootercenter2 = worksheet.Cells[startrow + x + 4, genDynamicColumn/2 + 2];
                cellfootercenter2.Value = "VTC TRAINING MANAGER";
                cellfootercenter2.Style.Font.Bold = true;
                cellfootercenter2.Style.Font.Size = 12;
                cellfootercenter2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[startrow + x + 8, genDynamicColumn/2 + 2, startrow + x + 8, genDynamicColumn/2 + 3].Merge = true;
                ExcelRange cellfootercenter3 = worksheet.Cells[startrow + x + 8, genDynamicColumn/2 + 2];
                cellfootercenter3.Value = " Date .....................................";
                cellfootercenter3.Style.Font.Size = 12;
                cellfootercenter3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Prepared by
                worksheet.Cells[startrow + x + 3, genDynamicColumn + 2, startrow + x + 3, genDynamicColumn + 3].Merge = true;
                ExcelRange cellfooterright1 = worksheet.Cells[startrow + x + 3, genDynamicColumn + 2];
                cellfooterright1.Value = "Prepared by";
                cellfooterright1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellfooterright1.Style.Font.Size = 11;

                worksheet.Cells[startrow + x + 4, genDynamicColumn + 2, startrow + x + 4, genDynamicColumn + 3].Merge = true;
                ExcelRange cellfooterright2 = worksheet.Cells[startrow + x + 4, genDynamicColumn + 2];
                cellfooterright2.Value = "ATM DEPARTMENT";
                cellfooterright2.Style.Font.Bold = true;
                cellfooterright2.Style.Font.Size = 12;
                cellfooterright2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[startrow + x + 8, genDynamicColumn + 2, startrow + x + 8, genDynamicColumn + 3].Merge = true;
                ExcelRange cellfooterright3 = worksheet.Cells[startrow + x + 8, genDynamicColumn + 2];
                cellfooterright3.Value = " Date .....................................";
                cellfooterright3.Style.Font.Size = 12;
                cellfooterright3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                #endregion

                #region[header]
                //using (ExcelRange r = worksheet.Cells[2, 1 + 1, 4, dt.Columns.Count - 1 + 1])
                //{
                //    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                //    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                //    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                //}
                using (ExcelRange r = worksheet.Cells[row_1, 1 + 1, startrow + x - 1, col_default + genDynamicColumn + 3])
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
                //worksheet.Cells[2, 1 + 1, 4, 1 + 2].Merge = true;
                worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 1 + 1, 4, 1 + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Merge = true;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, 4, 4, genDynamicColumn + 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;


                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Merge = true;
                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[2, genDynamicColumn + 3, 4, genDynamicColumn + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //worksheet.Cells[2, genDynamicColumn + 1, 4, genDynamicColumn + 4].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ExcelRange celltitleheader = worksheet.Cells[2, 4];
                celltitleheader.Value = "FINAL COURSE RESULT";
                celltitleheader.Style.Font.Bold = true;
                celltitleheader.Style.Font.Size = 15;
                worksheet.Cells[2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                string[] header = GetByKey("Final_Course_Result_Header").Split(',');
                ExcelRange celltitleheader3 = worksheet.Cells[2, genDynamicColumn + 3];
                celltitleheader3.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                celltitleheader3.Style.Font.Size = 10;
                celltitleheader3.Style.WrapText = true;
                celltitleheader3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                celltitleheader3.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                #endregion
                Bytes = xlPackage.GetAsByteArray();
            }

            return Bytes;
        }
        #endregion
        #region[Training]
        public ActionResult Training()
        {
            var instructorId = (int)UtilConstants.ROLE.Instructor;
            var model = new TrainingActivityModel()
            {
                Departments = LoadDepartment(),
                Status = new SelectList(UtilConstants.StatusDictionary(), "Key", "Value")
            };

            return View(model);
        }
        private string LoadDepartment(int? id = null)
        {
            var result = string.Empty;
            var data = DepartmentService.Get().OrderBy(a => a.Name).Select(x => new { x.Id, x.Ancestor, x.Name, x.Code });
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
        public ActionResult AjaxHandlerTraining(jQueryDataTableParamModel param)
        {
            try
            {
                //xử lý param gửi lên 
                string departmentCode = string.IsNullOrEmpty(Request.QueryString["departmentCode"]) ? string.Empty : Request.QueryString["departmentCode"].Trim();
                var das = new List<int>();
                if (!string.IsNullOrEmpty(departmentCode))
                {
                    das = TinhGiaiThua(int.Parse(departmentCode));
                    if (int.Parse(departmentCode) != -1)
                    {
                        das.Add(int.Parse(departmentCode));
                    }
                }
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");

                var courseName = string.IsNullOrEmpty(Request.QueryString["coursename"]) ? string.Empty : Request.QueryString["coursename"].Trim();
                var courseCode = string.IsNullOrEmpty(Request.QueryString["coursecode"]) ? string.Empty : Request.QueryString["coursecode"].Trim();
                // string status = string.IsNullOrEmpty(Request.QueryString["Status"]) ? string.Empty : Request.QueryString["Status"].Trim();
                string searchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
                string searchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();
                string venue = string.IsNullOrEmpty(Request.QueryString["venue"]) ? string.Empty : Request.QueryString["venue"].Trim();
                string status = string.IsNullOrEmpty(Request.QueryString["status"]) ? string.Empty : Request.QueryString["status"].Trim();
                var sss = "";
              
                if (!string.IsNullOrEmpty(courseName) || !string.IsNullOrEmpty(courseCode) || !string.IsNullOrEmpty(venue))
                {
                    var listCourse = CourseService.Get(
                        a => a.IsDeleted != true &&
                            (string.IsNullOrEmpty(courseCode) || a.Code.Contains(courseCode)) &&
                            (string.IsNullOrEmpty(courseName) || a.Name.Contains(courseName)) &&
                            (string.IsNullOrEmpty(venue) || a.Venue.Contains(venue)), true)
                            .Select(a => a.Id);
                    if (listCourse.Any())
                        sss = string.Join(",", listCourse);
                }
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                if (string.IsNullOrEmpty(courseName) && string.IsNullOrEmpty(courseCode) && string.IsNullOrEmpty(venue) && string.IsNullOrEmpty(status) && string.IsNullOrEmpty(searchDateFrom) && string.IsNullOrEmpty(searchDateTo))
                {
                    var listCourse = CourseService.Get(
                        a => a.IsDeleted != true && a.StartDate >= timenow, true).Select(a => a.Id);
                    if (listCourse.Any())
                        sss = string.Join(",", listCourse);
                }
                DateTime? FromDate_from, ToDate_from;
                AppUtils.StringToDateRange2(searchDateFrom, out FromDate_from, out ToDate_from);
                DateTime? FromDate_to, ToDate_to;
                AppUtils.StringToDateRange2(searchDateTo, out FromDate_to, out ToDate_to);
                var data = CourseService.GetTrainingHeader(sss, string.Join(",", das), FromDate_from, ToDate_from, FromDate_to, ToDate_to, status);
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<sp_GetTrainingHeaderTV_Result, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.str_Code
                                                          : sortColumnIndex == 2 ? c.str_Name
                                                            : sortColumnIndex == 3 ? c.TypeName
                                                            : sortColumnIndex == 4 ? c.DepartmentCode
                                                            : sortColumnIndex == 5 ? c?.Duration
                                                            : sortColumnIndex == 6 ? c?.dtm_StartDate
                                                            : sortColumnIndex == 7 ? c?.dtm_EndDate
                                                            : sortColumnIndex == 8 ? c?.Paticipants
                                                            : sortColumnIndex == 9 ? c?.Distinction
                                                             : sortColumnIndex == 10 ? c?.Pass
                                                             : sortColumnIndex == 11 ? (object)c?.NoOfCerti
                                                            : c.dtm_StartDate);
                // var sortDirection = (Request["sSortDir_0"] == "asc"); // asc or desc

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                var filtered = (sortDirection == "asc") ? data.OrderBy(orderingFunction)
                                   : data.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                    c.Course_Id,
                                    c.str_Code,
                                    "<span data-value='"+c.Course_Id+"' class='expand' style='cursor: pointer;'><a>"+c.str_Name+"</a></span>",
                                    c.TypeName,
                                    c.DepartmentCode,
                                    c.Duration,
                                    c.dtm_StartDate?.ToString("dd/MM/yyyy") ??"",
                                    c.dtm_EndDate?.ToString("dd/MM/yyyy") ??"",
                                    c.Paticipants,
                                    c.Distinction,
                                    c.Pass,
                                    c.NoOfCerti
                                   //c?.Course_Type?.str_Name,
                                    //c?.is_data_new == null ? "Approve" :  ReturnColumnStatus(c.Course_Id),

                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
                    aaData = result
                },
            JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlerTraining", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        private GradeModel TotalCountGrade(int? courseId)
        {
            var model = new GradeModel();
            var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId);
            model.Paticipant = data.Count();
            model.Distinction = data.Count(a => a.grade == (int)UtilConstants.Grade.Distinction);
            model.Pass = data.Count(a => a.grade == (int)UtilConstants.Grade.Pass);
            model.Fail = data.Count(a => a.grade == (int)UtilConstants.Grade.Fail);
            model.Certificate = data.Count(a => a.SRNO != null);
            return model;
        }


        protected int TotalCount(UtilConstants.Grade type, int? courseId)
        {
            var count = 0;
            IQueryable<Course_Result_Final> data;
            switch (type)
            {
                case UtilConstants.Grade.Paticipant:
                    data = CourseService.GetCourseResultFinal(a => a.courseid == courseId);
                    count = data.Count();
                    break;
                case UtilConstants.Grade.Distinction:
                    data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && (int)type == a.grade);
                    count = data.Count();
                    break;
                case UtilConstants.Grade.Pass:
                    data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && (int)type == a.grade);
                    count = data.Count();
                    break;
                case UtilConstants.Grade.Fail:
                    data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && (int)type == a.grade);
                    count = data.Count();
                    break;
                case UtilConstants.Grade.Certificate:
                    data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && a.SRNO != null);
                    count = data.Count();
                    break;
            }
            return count;
        }
        //private string CountPaticipant(int? courseId)
        //{
        //    var count = string.Empty;
        //    var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId);
        //    count = data.Count().ToString();
        //    return count;
        //}

        //private string CountCertificate(int? courseId)
        //{
        //    var count = string.Empty;
        //    var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && a.SRNO != null);
        //    count = data.Count().ToString();
        //    return count;
        //}



        protected string CountGrace(int? courseId, UtilConstants.Grade type)
        {
            var count = string.Empty;
            var data = CourseService.GetCourseResultFinal(a => a.courseid == courseId && (int)type == a.grade);
            count = data.Count().ToString();
            return count;
        }


        public ActionResult TrainingPrint(int id = 0)
        {

            var courseName = string.IsNullOrEmpty(Request.QueryString["coursename"]) ? string.Empty : Request.QueryString["coursename"].Trim();
            var courseCode = string.IsNullOrEmpty(Request.QueryString["coursecode"]) ? string.Empty : Request.QueryString["coursecode"].Trim();
            string searchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
            string searchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();
            string venue = string.IsNullOrEmpty(Request.QueryString["venue"]) ? string.Empty : Request.QueryString["venue"].Trim();

            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(searchDateFrom, out dateFrom);
            DateTime.TryParse(searchDateTo, out dateTo);

            var data =
              CourseService.Get(
                  a =>
                      a.Course_TrainingCenter.Any(b => CurrentUser.PermissionIds.Any(c => c == b.khoidaotao_id)) &&
                      (string.IsNullOrEmpty(courseCode) || a.Code.Contains(courseCode)) &&
                      (string.IsNullOrEmpty(courseName) || a.Name.Contains(courseName)) &&
                      (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                      (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0) &&
                      (string.IsNullOrEmpty(venue) || a.Venue.Contains(venue))).ToList();

            var model = new TrainingModelRp
            {
                CourseRps = data.Select(a => new CourseRp()
                {
                    CourseCode = a.Code,
                    CourseName = a.Name,
                    TypeOfTraining = GetCourseType(a.CourseTypeId),
                    DateFrom = a.StartDate.Value.ToString(Resource.lbl_FORMAT_DATE) ?? string.Empty,
                    DateTo = a.EndDate.Value.ToString(Resource.lbl_FORMAT_DATE) ?? string.Empty,
                    Participants = TotalCount(UtilConstants.Grade.Paticipant, a.Id),
                    Distinction = TotalCount(UtilConstants.Grade.Distinction, a.Id),
                    Pass = TotalCount(UtilConstants.Grade.Pass, a.Id),
                    Certificate = TotalCount(UtilConstants.Grade.Distinction, a.Id)
                }).OrderByDescending(b => Convert.ToDateTime(b.DateFrom))
            };

            return PartialView("TrainingPrint", model);

        }


        public ActionResult AjaxHandlerTrainingSubject(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                //var model = _repoCourseServiceDetail.GetByCourse(id).ToList();

                var data = GetTrainningData(null, null, null, null, null, null, null, id);
                IEnumerable<TrainingActivityReportViewModel> filtered = data;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TrainingActivityReportViewModel, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.SubjectName
                                                                                        : sortColumnIndex == 2 ? c?.Customer
                                                                                        : sortColumnIndex == 3 ? c?.Duration.ToString()
                                                                                        : sortColumnIndex == 4 ? c?.Participants.ToString()
                                                                                        : c.Course_Detail_Id.ToString());
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                            string.Empty,

                             "<span "+(c?.bit_Active != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c?.SubjectName +"</span>",

                            c?.Method,
                            c?.Duration,
                            c?.Participants,
                            c?.Distinction,
                            c?.Pass
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlerTrainingSubject", ex.Message);
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
        public FileContentResult ExportTraining(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var courseName = string.IsNullOrEmpty(form["coursename"]) ? string.Empty : form["coursename"].Trim();
            var courseCode = string.IsNullOrEmpty(form["coursecode"]) ? string.Empty : form["coursecode"].Trim();
            var departmentCode = string.IsNullOrEmpty(form["DepartmentCode"]) ? null : (int?)Convert.ToInt32(form["DepartmentCode"].Trim().ToString());
            var searchDateFrom = string.IsNullOrEmpty(form["fSearchDate_from"]) ? string.Empty : form["fSearchDate_from"].Trim();
            var status = string.IsNullOrEmpty(form["status"]) ? null : (int?)Convert.ToInt32(form["status"].Trim().ToString());
            var searchDateTo = string.IsNullOrEmpty(form["fSearchDate_to"]) ? string.Empty : form["fSearchDate_to"].Trim();
            var venue = string.IsNullOrEmpty(form["venue"]) ? string.Empty : form["venue"].Trim();

            DateTime? FromDate_from, ToDate_from;
            AppUtils.StringToDateRange2(searchDateFrom, out FromDate_from, out ToDate_from);
            byte[] filecontent = ExportExcelTraining(courseCode, courseName, departmentCode, FromDate_from, ToDate_from, status, venue);
            //byte[] filecontent = ExportExcelTraining(courseName, courseCode, dateFrom, dateTo, venue);
            return File(filecontent, ExportUtils.ExcelContentType, "TRAINING ACTIVITY REPORT.xlsx");

        }
        //TODO: process result
        private IEnumerable<TrainingActivityReportViewModel> GetTrainningData(string courseCode, string courseName, int? departmentid, DateTime? from, DateTime? to, int? statusCode, string venue, int? CourseID)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            var courids = new List<int>();

            if (statusCode.HasValue)
            {
                if (statusCode.Value == 0)
                {
                    var list = CourseService.Get(
                        a => a.IsDeleted != true && a.TMS_APPROVES.Any(x=>x.int_id_status == (int)UtilConstants.EStatus.Approve && x.int_Type == (int)UtilConstants.ApproveType.Course) && a.EndDate > DateTime.Now && a.GroupSubjectId > 0, true).Select(a => a.Id);
                    //var list = _repoTmsApproves.Get(a => a.Course.IsDeleted != true && a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course && a.Course.EndDate > DateTime.Now && a.Course.GroupSubjectId > 0).Select(a => (int)a.int_Course_id);
                    courids.AddRange(list);
                }
                else if (statusCode.Value == 1)
                {
                    var list = CourseService.Get(
                       a => a.IsDeleted != true && a.TMS_APPROVES.Any(x => x.int_id_status == (int)UtilConstants.EStatus.Approve && x.int_Type == (int)UtilConstants.ApproveType.Course) && a.EndDate < DateTime.Now && a.GroupSubjectId > 0, true).Select(a => a.Id);
                    //var list_ = _repoTmsApproves.Get(a => a.Course.IsDeleted != true && a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_Type == (int)UtilConstants.ApproveType.Course && a.Course.EndDate < DateTime.Now && a.Course.GroupSubjectId > 0).Select(a => (int)a.int_Course_id);
                    courids.AddRange(list);
                }
                else
                {
                    var list = CourseService.Get(a => !a.TMS_APPROVES.Any() && a.GroupSubjectId > 0, true).Select(a => a.Id);
                    courids.AddRange(list);
                }
            }
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            if (string.IsNullOrEmpty(courseName) && string.IsNullOrEmpty(courseCode) && string.IsNullOrEmpty(venue) && !departmentid.HasValue && (from == DateTime.MinValue || !from.HasValue) && (to == DateTime.MinValue || !to.HasValue) && !statusCode.HasValue && !CourseID.HasValue)
            {
                var listCourse = CourseService.Get(
                        a => a.IsDeleted != true && a.StartDate >= timenow, true).Select(a => a.Id);
                if (listCourse.Any())
                    courids.AddRange(listCourse);
            }

            var course = "";
            if (courids.Count() > 0)
            {
                course = string.Join(",", courids);
            }

            var courseDetails = CourseDetailService.GetTraining(venue, courseCode, courseName, departmentid, from, to, course, CourseID);

            return
                courseDetails.GroupBy(
                    a =>
                        new
                        {
                            a.Company,
                            a.CourseCode,
                            a.CourseName,
                            a.bit_Active,
                            a.SubjectName,
                            a.TypeOfTraining,
                            a.Dept,
                            a.Venue,
                            a.Method,
                            a.Customer,
                            a.DateFrom,
                            a.DateTo,
                            a.Duration
                        }, a => new { a.Participants, a.Pass, a.Fail, a.Distinction }, (a, b)
                              => new TrainingActivityReportViewModel()
                              {
                                  Company = a.Company,
                                  CourseCode = a.CourseCode,
                                  CourseName = a.CourseName,
                                  bit_Active = a.bit_Active,
                                  SubjectName = a.SubjectName,
                                  TypeOfTraining = a.TypeOfTraining,
                                  Dept = a.Dept,
                                  Method = a.Method,
                                  Customer = a.Customer,
                                  DateFrom = a.DateFrom,
                                  DateTo = a.DateTo,
                                  Venue = a.Venue,
                                  Duration = a.Duration,
                                  Participants = b.Where(x => x.Participants.HasValue).Sum(x => x.Participants.Value),
                                  Distinction = b.Where(x => x.Distinction.HasValue).Sum(x => x.Distinction.Value),
                                  Pass = b.Where(x => x.Pass.HasValue).Sum(x => x.Pass.Value),
                                  Fail = b.Where(x => x.Fail.HasValue).Sum(x => (int)x.Fail)
                              }).OrderByDescending(a => a.DateFrom);
        }
        private byte[] ExportExcelTraining(string courseCode, string courseName, int? departmentCode, DateTime? dateFrom, DateTime? dateTo, int? status, string venue)
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/Training.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            var data = GetTrainningData(courseCode, courseName, departmentCode, dateFrom, dateTo, status, venue, null);
            ExcelPackage xlPackage;
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                var startrow = 8;
                var worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                var rowheader = 0;
                var rowDetail = 0;
                var count = 1;
                var columns = 17;
                using (ExcelRange r = worksheet.Cells[startrow - 1, 1, startrow - 1 + data.Count() + 1, columns])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    r.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    r.Style.WrapText = true;
                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }
                int pass = 0, distinction = 0, fail = 0, participants = 0;
                double duration = 0;
                foreach (var item in data)
                {
                    var col = 1;
                    worksheet.Cells[rowheader + startrow + rowDetail, col].Value = count;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Company;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.CourseCode;
                    var col2 = worksheet.Cells[rowheader + startrow + rowDetail, ++col];
                    col2.Value = item.CourseName;
                    col2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    var col3 = worksheet.Cells[rowheader + startrow + rowDetail, ++col];
                    col3.Value = (item.bit_Active != true ? "(DeAcive) " : "") + item.SubjectName;
                    col3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.TypeOfTraining;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Dept;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Method;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Customer;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Duration;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Venue;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.DateFrom?.ToString("dd/MM/yyyy");
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.DateTo?.ToString("dd/MM/yyyy");
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Participants;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Distinction;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Pass;
                    worksheet.Cells[rowheader + startrow + rowDetail, ++col].Value = item.Fail;
                    pass += item.Pass;
                    distinction += item.Distinction;
                    fail += item.Fail;
                    participants += item.Participants;
                    duration += item.Duration ?? 0;

                    rowDetail++;
                    count++;
                }
                var totalRow = rowheader + startrow + rowDetail;
                var totalcols = worksheet.Cells[totalRow, 1, totalRow, 9];
                totalcols.Merge = true;
                totalcols.Value = "Total";
                totalcols.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[totalRow, 10].Value = duration;
                worksheet.Cells[totalRow, 17].Value = fail;
                worksheet.Cells[totalRow, 16].Value = pass;
                worksheet.Cells[totalRow, 15].Value = distinction;
                worksheet.Cells[totalRow, 14].Value = participants;
                var rowExtend = worksheet.Cells[totalRow + 1, 14, totalRow + 1, 17];
                rowExtend.Merge = true;
                rowExtend.Value = "Prepared by ";
                rowExtend = worksheet.Cells[totalRow + 2, 14, totalRow + 2, 17];
                rowExtend.Merge = true;
                rowExtend.Value = "Date .....................";
                Bytes = xlPackage.GetAsByteArray();
            }
            return Bytes;
        }
        #endregion
        #region Attendance Sheet
        // page 
        public ActionResult AttendanceSheet()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            var model = new AttendanceSheetModel()
            {
                Courses = CourseService.Get(a => a.StartDate >= timenow).OrderByDescending(a => a.Id).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name))
            };

            return View(model);
        }


        public ActionResult AttendanceSheetPrint(int id = 0)
        {
            var ddlSubject = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());
            var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);
            var model = new AttendanceSheetModelRp();

            IEnumerable<TMS_Course_Member> data = new List<TMS_Course_Member>();
            if (courseDetail != null)
            {
                data = courseDetail.TMS_Course_Member.Where(a => /*a.Approve_Id != null &&*/ a.IsActive == true && a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).OrderBy(a => a.Trainee?.str_Staff_Id).AsEnumerable();
            }

            //var data =
            //    _courseServiceMember.Get(a => a.Course_Details_Id == ddlSubject && a.DeleteApprove == null && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved));
            if (courseDetail != null)
            {
                model.header = GetByKey("Attendance_Sheet_Header").Split(new char[] { ',' });
                model.CourseCode = courseDetail.Course.Code;
                model.CourseName = courseDetail.Course.Name;
                model.SubjectDetailName = courseDetail.SubjectDetail.Name;
                model.SubjectDetailDuration = (float)courseDetail.SubjectDetail.Duration;
                model.Venue = courseDetail?.Room?.str_Name;
                model.TimeFrom = (courseDetail.dtm_time_from.Value.ToString("dd/MM/yyyy") ?? "");
                model.TimeTo = (courseDetail.dtm_time_to.Value.ToString("dd/MM/yyyy") ?? "");
                model.Trainees = data.Select(a => new AttendanceSheetModelRp.TraineeModelRp
                {
                    FullName = GetByKey("DisplayLanguage") == "vi" ? a.Trainee.FirstName + " " + a.Trainee.LastName : a.Trainee.LastName + " " + a.Trainee.FirstName,
                    //FullName = GetByKey(a.Trainee.FirstName , a.Trainee.LastName),
                    Eid = a.Trainee.str_Staff_Id,
                    DepartmentCode = a.Trainee.Department.Code
                });

            }
            return PartialView("AttendanceSheetPrint", model);
        }

        public ActionResult AjaxHandlAttendanceSheet(jQueryDataTableParamModel param)
        {
            try
            {
                int ddlSubject = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());

                var data = _repoCourseServiceDetail.GetById(ddlSubject);
                IEnumerable<TMS_Course_Member> filtered = new List<TMS_Course_Member>();
                if (data != null)
                {
                    filtered = data.TMS_Course_Member.Where(a => /*a.Approve_Id != null &&*/ a.IsActive == true && a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).OrderBy(a => a.Trainee?.str_Staff_Id);
                }

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 3 ? c?.Trainee?.Department?.Code
                                                            : c?.Trainee?.FirstName.ToString());
                var sortDirection = Request["sSortDir_0"]; // asc or desc


                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                    string.Empty,
                                                   c?.Trainee?.str_Staff_Id,
                                                //c?.Trainee?.FirstName + " " + c?.Trainee?.LastName,
                                                ReturnDisplayLanguage(c?.Trainee?.FirstName , c?.Trainee?.LastName),
                                                c?.Trainee?.Department?.Code,
                                                // PercentDate(c.Course_Detail, c.Trainee),
                                                "<span data-valuetrainee='" + c.Trainee.Id + "' data-valuecoursedetail='" + c.Course_Details_Id + "' class='expand' style='cursor: pointer;'><a><i class='fa fa-search'></i></a></span>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlAttendanceSheet", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlerAttendanceSheetDetail(jQueryDataTableParamModel param)
        {
            try
            {
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].Trim());
                int cdid = string.IsNullOrEmpty(Request.QueryString["cdid"]) ? -1 : Convert.ToInt32(Request.QueryString["cdid"].Trim());
                //var model = CourseDetailService.GetByCourse(id).Where(a=> !a.bit_Deleted);


                var model = new List<DateTime>();
                Expression<Func<Course_Detail, bool>> expression = a => a.SubjectDetail.IsDelete == false;
                if (cdid != null)
                {
                    expression =
                        a => a.Id == cdid && a.SubjectDetail.IsDelete == false;
                }
                var data = CourseDetailService.Get(expression).Select(a => new { a.dtm_time_from, a.dtm_time_to }).FirstOrDefault();
                if (data != null)
                {
                    for (DateTime d = (DateTime)data.dtm_time_from; d <= (DateTime)data.dtm_time_to; d = d.AddDays(1))
                    {
                        model.Add(d.Date);
                    }
                }





                IEnumerable<DateTime> filtered = model;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<DateTime, object> orderingFunction = (c => c.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var courseTypes = UtilConstants.CourseTypesDictionary();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                            string.Empty,
                            c.ToString("dd/MM/yyyy"),
                            AttendedDate(cdid,id,UtilConstants.Attendance.Present,c),
                            AttendedDate(cdid,id,UtilConstants.Attendance.Absent,c),
                            AttendedDate(cdid,id,UtilConstants.Attendance.Late,c),
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Report/AjaxHandlerAttendanceSheetDetail", ex.Message);
                return Json(new
                {

                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        private string PercentDate(Course_Detail course_Detail, Trainee trainee)
        {
            var counttotal = 0;
            var countPresent = 0;
            Expression<Func<Course_Detail, bool>> expression = a => a.SubjectDetail.IsDelete == false;
            if (course_Detail != null)
            {
                expression =
                    a => a.Id == course_Detail.Id && a.SubjectDetail.IsDelete == false;
            }
            var data = CourseDetailService.Get(expression).Select(a => new { a.dtm_time_from, a.dtm_time_to }).FirstOrDefault();
            if (data != null)
            {
                for (DateTime d = (DateTime)data.dtm_time_from; d <= (DateTime)data.dtm_time_to; d = d.AddDays(1))
                {
                    counttotal++;
                }
            }
            countPresent = _CourseService.GetAllTraineeAttendance(a => a.CourseDetailId == course_Detail.Id && a.TraineeId == trainee.Id && (a.Type == (int)UtilConstants.Attendance.Present || a.Type == (int)UtilConstants.Attendance.Absent)).Count();
            var Percent = (countPresent * 100) / counttotal;
            return Percent.ToString() + "%";
        }
        private string AttendedDate(int course_Detailid, int traineeid, UtilConstants.Attendance attendance, DateTime dateTime)
        {

            var data = _CourseService.GetTraineeAttendance(a => a.CourseDetailId == course_Detailid && a.TraineeId == traineeid && a.Type == (int)attendance && a.AttendedDate == dateTime);
            if (data != null)
                return "<i class='fa fa-check' aria-hidden='true'></i>";
            return "";
        }
        private string AttendedDateExcel(IEnumerable<Course_Attendance> course_Attendance, int course_Detailid, int traineeid, object dateTime)
        {
            var Date = DateTime.Parse(dateTime.ToString());
            var data = course_Attendance.FirstOrDefault(a => a.CourseDetailId == course_Detailid && a.TraineeId == traineeid && a.AttendedDate == Date);
            if (data != null)
                switch (data.Type)
                {
                    case (int)UtilConstants.Attendance.Present:
                        return "P";
                    case (int)UtilConstants.Attendance.Absent:
                        return "A";
                    case (int)UtilConstants.Attendance.Late:
                        return "L";
                    default:
                        return "";
                }
            return "";
        }


        [HttpPost]
        public FileResult ExportAttendanceSheet(FormCollection form)
        {

            var ddlSubject = string.IsNullOrEmpty(form["ddl_subject"]) ? -1 : Convert.ToInt32(form["ddl_subject"].Trim());

            var filecontent = ExportExcelAttendanceSheet(ddlSubject);
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "AttendanceSheet" + string.Format("{0:dd/MM/yyyy}", DateTime.Now) + ".xlsx");
            }
            else
            {
                return null;
            }

        }

        private byte[] ExportExcelAttendanceSheet(int ddlSubject)
        {
            
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/AttendanceSheet.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);

            //var data =
            //    _courseServiceMember.Get(a => a.Course_Details_Id == ddlSubject && a.DeleteApprove == null && a.IsActive && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approve)).ToList();
            //var data =
            //    _courseServiceMember.Get(a => a.Course_Details_Id == ddlSubject && a.IsDelete == false).ToList();
            //IEnumerable<TMS_Course_Member> datax = new List<TMS_Course_Member>();
            //if (courseDetail != null)
            //{
            //    datax = courseDetail.TMS_Course_Member.Where(a => /*a.Approve_Id != null &&*/a.IsActive == true && a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).OrderBy(a => a.Trainee?.str_Staff_Id);
            //}

            ExcelPackage excelPackage;
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            if (courseDetail != null)
            {
                var data = CourseMemberService.Get(a => a.Course_Details_Id == courseDetail.Id && a.IsActive == true && a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).ToList().OrderBy(a => a.Trainee?.str_Staff_Id);
                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 7;
                    string[] header = GetByKey("Attendance_Sheet_Header").Split(new char[] { ',' });

                    ExcelRange cellHeaderNoForm = worksheet.Cells[3, 10];
                    cellHeaderNoForm.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                    cellHeaderNoForm.Style.Font.Size = 11;

                    ExcelRange cellHeader = worksheet.Cells[startRow, 3];
                    cellHeader.Value = courseDetail.Course?.Name;
                    cellHeader.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeader.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeader.Style.Font.Size = 12;
                    cellHeader.Style.Font.Bold = true;

                    ExcelRange cellHeaderCourseCode = worksheet.Cells[startRow, 8];
                    cellHeaderCourseCode.Value = courseDetail.Course?.Code;
                    cellHeaderCourseCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeaderCourseCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeaderCourseCode.Style.Font.Size = 12;
                    cellHeaderCourseCode.Style.Font.Bold = true;

                    ExcelRange cellHeaderSubjectName = worksheet.Cells[startRow + 1, 3];
                    cellHeaderSubjectName.Value = courseDetail.SubjectDetail?.Name;
                    cellHeaderSubjectName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeaderSubjectName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeaderSubjectName.Style.Font.Size = 12;
                    cellHeaderSubjectName.Style.Font.Bold = true;

                    ExcelRange cellHeaderDuration = worksheet.Cells[startRow + 1, 8];
                    cellHeaderDuration.Value = courseDetail.SubjectDetail?.Duration;
                    cellHeaderDuration.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeaderDuration.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeaderDuration.Style.Font.Size = 12;
                    cellHeaderDuration.Style.Font.Bold = true;

                    ExcelRange cellHeaderVenue = worksheet.Cells[startRow + 2, 3];
                    cellHeaderVenue.Value = courseDetail?.Room?.str_Name;
                    cellHeaderVenue.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeaderVenue.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeaderVenue.Style.Font.Size = 12;
                    cellHeaderVenue.Style.Font.Bold = true;

                    var date = (courseDetail.dtm_time_from.Value.ToString("dd/MM/yyyy") ?? "") + " - " +
                               (courseDetail.dtm_time_to.Value.ToString("dd/MM/yyyy") ?? "");
                    ExcelRange cellHeaderDate = worksheet.Cells[startRow + 2, 8];
                    cellHeaderDate.Value = date;
                    cellHeaderDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cellHeaderDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellHeaderDate.Style.Font.Size = 12;
                    cellHeaderDate.Style.Font.Bold = true;

                    int count = 0;
                    startRow = 13;
                    foreach (var item in data)
                    {
                        int col = 2;
                        count++;
                        ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                        cellNo.Value = count;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        var fullName = ReturnDisplayLanguage(item.Trainee.FirstName, item.Trainee.LastName);
                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, ++col];
                        cellFullName.Value = fullName;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ExcelRange cellStaffId = worksheet.Cells[startRow + 1, ++col];
                        cellStaffId.Value = item.Trainee.str_Staff_Id;
                        cellStaffId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellStaffId.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellStaffId.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellDepartment = worksheet.Cells[startRow + 1, ++col];
                        cellDepartment.Value = item.Trainee.Department.Code;
                        cellDepartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellDepartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellDepartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange day1Am = worksheet.Cells[startRow + 1, ++col];
                        day1Am.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ExcelRange day1Pm = worksheet.Cells[startRow + 1, ++col];
                        day1Pm.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange day2Am = worksheet.Cells[startRow + 1, ++col];
                        day2Am.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ExcelRange day2Pm = worksheet.Cells[startRow + 1, ++col];
                        day2Pm.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange day3Am = worksheet.Cells[startRow + 1, ++col];
                        day3Am.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ExcelRange day3Pm = worksheet.Cells[startRow + 1, ++col];
                        day3Pm.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        startRow++;

                    }
                    int endrow = startRow + 2;

                    var total = worksheet.Cells[startRow + 1, 2, startRow + 1, 11];
                    total.Merge = true;
                    //ExcelRange cellcount = worksheet.Cells[startRow + 1, 11];
                    total.Value = "Records:  " + count;
                    total.Style.Font.Size = 10;
                    total.Style.Font.Bold = true;
                    total.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    total.Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                    ExcelRange instructor = worksheet.Cells[endrow, 2];
                    instructor.Value = "Instructor:";
                    instructor.Style.Font.Bold = true;
                    instructor.Style.Font.Size = 10;
                    instructor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    instructor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange signature1 = worksheet.Cells[endrow + 2, 2];
                    signature1.Value = "NAME: ..............................................................................................................SIGNATURE: ..............................................................................................................DATE: ..............................................................................................................";
                    signature1.Style.Font.Bold = true;
                    signature1.Style.Font.Size = 9;
                    signature1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    signature1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange signature2 = worksheet.Cells[endrow + 4, 2];
                    signature2.Value = "NAME: ..............................................................................................................SIGNATURE: ..............................................................................................................DATE: ..............................................................................................................";
                    signature2.Style.Font.Bold = true;
                    signature2.Style.Font.Size = 9;
                    signature2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    signature2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    bytes = excelPackage.GetAsByteArray();
                }

            }
            return bytes;


        }


        public ActionResult AttendanceSheetPDF(int id = -1)
        {
            try
            {

                var CourseDetails = _repoCourseServiceDetail.GetById(id);
                var browserInformation = Request.Browser;
                // int count = 0;
                Response.AppendHeader("cache-control", browserInformation.Browser == "IE" ? "private" : "no-cache");
                //DateTime? fromDate = null;
                //DateTime? toDate = null;

                ReportDataSet.ReportModelRow detailRow;
                ReportDataSet.ReportModelDataTable coursemmber = new ReportDataSet.ReportModelDataTable();

                foreach (var item in CourseDetails.TMS_Course_Member)
                {
                    detailRow = coursemmber.NewReportModelRow();
                    //detailRow.DataColumn1 = item?.Trainee?.FirstName + " " + item?.Trainee?.LastName;
                    detailRow.DataColumn1 = ReturnDisplayLanguage(item?.Trainee?.FirstName, item?.Trainee?.LastName);
                    detailRow.DataColumn2 = item?.Trainee?.str_Staff_Id;
                    detailRow.DataColumn3 = item?.Trainee?.Department?.Name;
                    coursemmber.AddReportModelRow(detailRow);
                }

                ReportDataSource source = new ReportDataSource();
                source.Name = "DataSet1";
                source.Value = coursemmber;

                ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();

                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.ReportPath = Server.MapPath(GetByKey("PrivateTemplate") + "ReportFile/AttendanceSheet.rdlc");
                //rv.Attributes. = "TransporterIssueTransaction";
                rv.LocalReport.DataSources.Clear();
                rv.LocalReport.DataSources.Add(source);

                ReportParameter param_1 = new ReportParameter("pCourseName", CourseDetails.Course.Name, false);
                ReportParameter param_2 = new ReportParameter("pSubjectName", CourseDetails.SubjectDetail.Name, false);
                ReportParameter param_3 = new ReportParameter("pDuration", CourseDetails.Duration?.ToString(), false);
                ReportParameter param_4 = new ReportParameter("pVenue", CourseDetails.Course.Venue, false);
                ReportParameter param_5 = new ReportParameter("pDateFrom", CourseDetails.dtm_time_from.Value.ToString("dd/MM/yyyy"), false);
                ReportParameter param_6 = new ReportParameter("pDateTo", CourseDetails.dtm_time_to.Value.ToString("dd/MM/yyyy"), false);
                ReportParameter param_7 = new ReportParameter("pCourseCode", CourseDetails.Course.Code, false);

                List<ReportParameter> lsParam = new List<ReportParameter>();
                lsParam.Add(param_1);
                lsParam.Add(param_2);
                lsParam.Add(param_3);
                lsParam.Add(param_4);
                lsParam.Add(param_5);
                lsParam.Add(param_6);
                lsParam.Add(param_7);
                rv.LocalReport.SetParameters(lsParam);
                rv.LocalReport.Refresh();

                byte[] streamBytes = null;
                string mimeType = "application/pdf";
                string encoding = "";
                string filenameExtension = ".pdf";
                string[] streamids = null;
                Warning[] warnings = null;
                string deviceInfo = "<DeviceInfo><OutputFormat>PDF</OutputFormat></DeviceInfo>";
                //string type = REPORT_PDF;

                streamBytes = rv.LocalReport.Render(REPORT_PDF, deviceInfo, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

                return File(streamBytes, mimeType, "AttendanceSheet" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".pdf");
                //return fl;
            }
            catch (Exception ex)
            {
                return View("AttendanceSheet");
            }
            finally
            {
            }
        }



        public ActionResult AttendanceSheetEXCEL(int id = -1)
        {
            try
            {

                var CourseDetails = _repoCourseServiceDetail.GetById(id);
                var browserInformation = Request.Browser;
                // int count = 0;
                Response.AppendHeader("cache-control", browserInformation.Browser == "IE" ? "private" : "no-cache");
                //DateTime? fromDate = null;
                // DateTime? toDate = null;

                ReportDataSet.ReportModelRow detailRow;
                ReportDataSet.ReportModelDataTable coursemmber = new ReportDataSet.ReportModelDataTable();

                foreach (var item in CourseDetails.TMS_Course_Member)
                {
                    detailRow = coursemmber.NewReportModelRow();
                    //detailRow.DataColumn1 = item?.Trainee?.FirstName + " " + item?.Trainee?.LastName;
                    detailRow.DataColumn1 = ReturnDisplayLanguage(item?.Trainee?.FirstName, item?.Trainee?.LastName);
                    detailRow.DataColumn2 = item?.Trainee?.str_Staff_Id;
                    detailRow.DataColumn3 = item?.Trainee?.Department?.Name;
                    coursemmber.AddReportModelRow(detailRow);
                }

                ReportDataSource source = new ReportDataSource();
                source.Name = "DataSet1";
                source.Value = coursemmber;

                ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();

                rv.ProcessingMode = ProcessingMode.Local;
                rv.LocalReport.ReportPath = Server.MapPath(GetByKey("PrivateTemplate") + "ReportFile/AttendanceSheet.rdlc");
                //rv.Attributes. = "TransporterIssueTransaction";
                rv.LocalReport.DataSources.Clear();
                rv.LocalReport.DataSources.Add(source);

                var param_1 = new ReportParameter("pCourseName", CourseDetails.Course.Name, false);
                var param_2 = new ReportParameter("pSubjectName", CourseDetails.SubjectDetail?.Name, false);
                var param_3 = new ReportParameter("pDuration", CourseDetails.Duration?.ToString(), false);
                var param_4 = new ReportParameter("pVenue", CourseDetails.Course.Venue, false);
                var param_5 = new ReportParameter("pDateFrom", CourseDetails.dtm_time_from.Value.ToString("dd/MM/yyyy"), false);
                var param_6 = new ReportParameter("pDateTo", CourseDetails.dtm_time_to.Value.ToString("dd/MM/yyyy"), false);
                var param_7 = new ReportParameter("pCourseCode", CourseDetails.Course.Code, false);

                var lsParam = new List<ReportParameter> { param_1, param_2, param_3, param_4, param_5, param_6, param_7 };
                rv.LocalReport.SetParameters(lsParam);
                rv.LocalReport.Refresh();

                byte[] streamBytes = null;
                string mimeType = "";
                string encoding = "";
                string filenameExtension = "";
                string[] streamids = null;
                Warning[] warnings = null;
                string deviceInfo = "<DeviceInfo><OutputFormat>EXCEL</OutputFormat></DeviceInfo>";
                // string type = REPORT_PDF;

                streamBytes = rv.LocalReport.Render(REPORT_EXCEL, deviceInfo, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

                return File(streamBytes, mimeType, "AttendanceSheet" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                //return fl;
            }
            catch (Exception ex)
            {
                return View("AttendanceSheet");
            }
            finally
            {
            }
        }

        #endregion
        #region Instructor
        public ActionResult Instructor()
        {
            List<Department> ltsDepartment = DepartmentService.Get().ToList();
            ViewBag.ltsDepartment = ltsDepartment;
            List<JobTitle> ltsJobtitle = _repoJobTiltle.Get().ToList();
            ViewBag.ltsJobtitle = ltsJobtitle;
            return View();
        }

        public ActionResult AjaxHandlerInstructor_(jQueryDataTableParamModel param)
        {
            try
            {
                var subjectName = string.IsNullOrEmpty(Request.QueryString["subjectName"]) ? string.Empty : Request.QueryString["subjectName"].Trim();
                var subjectCode = string.IsNullOrEmpty(Request.QueryString["subjectCode"]) ? string.Empty : Request.QueryString["subjectCode"].Trim();
                var traineeName = string.IsNullOrEmpty(Request.QueryString["traineeName"]) ? string.Empty : Request.QueryString["traineeName"].Trim();
                var traineeCode = string.IsNullOrEmpty(Request.QueryString["traineeCode"]) ? string.Empty : Request.QueryString["traineeCode"].Trim();
                var departmentID = string.IsNullOrEmpty(Request.QueryString["department"]) ? -1 : Convert.ToInt32(Request.QueryString["department"].Trim());
                var jobtitleID = string.IsNullOrEmpty(Request.QueryString["jobtitle"]) ? -1 : Convert.ToInt32(Request.QueryString["jobtitle"].Trim());
                var data =
                    _repoEmployee.GetAbility(
                        a =>
                            a.Trainee != null && a.Trainee.IsDeleted != true &&
                            a.Trainee.int_Role == (int)UtilConstants.ROLE.Instructor &&
                            (string.IsNullOrEmpty(traineeName) || a.Trainee.FirstName.Contains(traineeName.Trim()) || a.Trainee.LastName.Contains(traineeName.Trim())) &&
                            (string.IsNullOrEmpty(traineeCode) || a.Trainee.str_Staff_Id.Contains(traineeCode)) &&
                            a.SubjectDetailId.HasValue && a.SubjectDetail.IsDelete != true &&
                             a.SubjectDetail.int_Parent_Id != null && a.SubjectDetail.CourseTypeId.HasValue &&
                             a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General &&
                             a.SubjectDetail.int_Parent_Id != a.SubjectDetail.Id && a.SubjectDetail.SubjectDetail2.IsDelete != true &&
                            (string.IsNullOrEmpty(subjectName) || a.SubjectDetail.Name.Contains(subjectName)) &&
                            (string.IsNullOrEmpty(subjectCode) || a.SubjectDetail.Code.Contains(subjectCode)) &&
                            (departmentID == -1 || a.Trainee.Department_Id == departmentID) &&
                            (jobtitleID == -1 || a.Trainee.Job_Title_id == jobtitleID));
                IEnumerable<Instructor_Ability> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Instructor_Ability, string> orderingFunction = (c
                                          => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                            : sortColumnIndex == 2 ? ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName)
                                            : sortColumnIndex == 3 ? c?.Trainee?.JobTitle?.Name
                                            : sortColumnIndex == 4 ? c?.Trainee?.Department?.Name
                                            : sortColumnIndex == 5 ? c?.SubjectDetail?.Name
                                            : sortColumnIndex == 5 ? c?.SubjectDetail?.CourseTypeId.ToString()
                                            : c?.Trainee?.str_Staff_Id);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }
                var count = filtered.Count();
                var records = filtered.Select(c =>
                    new
                    {
                        EID = string.Empty,
                        str_Staff_Id = c?.Trainee?.str_Staff_Id,
                        str_Fullname = ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName),
                        Job_Tiltle = c?.Trainee?.JobTitle?.Name,
                        Department = c?.Trainee?.Department?.Name,
                        Code = c?.SubjectDetail?.Code,
                        Subject = "<span " + (c?.SubjectDetail?.IsActive != true ? "style='color:" + UtilConstants.String_DeActive_Color + ";'" : "") + ">" + c?.SubjectDetail?.Name + "</span>",

                    }).Distinct().ToList();
                var cscost = records.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in cscost
                             select new object[] {
                                    c.EID,
                                    c.str_Staff_Id,
                                    c.str_Fullname,
                                    c.Job_Tiltle,
                                    c.Department,
                                    c.Code,
                                    c.Subject,
                        }; ;
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = records.Count(),
                    iTotalDisplayRecords = records.Count(),
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

        public ActionResult AjaxHandlerInstructor(jQueryDataTableParamModel param)
        {
            try
            {
                var subjectName = string.IsNullOrEmpty(Request.QueryString["subjectName"]) ? string.Empty : Request.QueryString["subjectName"].Trim();
                var subjectCode = string.IsNullOrEmpty(Request.QueryString["subjectCode"]) ? string.Empty : Request.QueryString["subjectCode"].Trim();
                var traineeName = string.IsNullOrEmpty(Request.QueryString["traineeName"]) ? string.Empty : Request.QueryString["traineeName"].Trim();
                var traineeCode = string.IsNullOrEmpty(Request.QueryString["traineeCode"]) ? string.Empty : Request.QueryString["traineeCode"].Trim();
                var departmentID = string.IsNullOrEmpty(Request.QueryString["department"]) ? -1 : Convert.ToInt32(Request.QueryString["department"].Trim());
                var jobtitleID = string.IsNullOrEmpty(Request.QueryString["jobtitle"]) ? -1 : Convert.ToInt32(Request.QueryString["jobtitle"].Trim());
                var sortDirection = string.IsNullOrEmpty(Request["sSortDir_0"]) ? "" : Request["sSortDir_0"]; // asc or desc

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                var orderingFunction = (sortColumnIndex == 1 ? "str_Staff_Id"
                                            : sortColumnIndex == 2 ? "str_Fullname"
                                            : sortColumnIndex == 3 ? "Job_Tiltle"
                                            : sortColumnIndex == 4 ? "Department"
                                            : sortColumnIndex == 5 ? "SubjectCode"
                                            : "str_Staff_Id");
                var data =
                    _repoEmployee.GetInstructorReport(subjectName, subjectCode, traineeName, traineeCode, departmentID, jobtitleID, sortDirection, orderingFunction).GroupBy(c => new { str_Staff_Id = c.str_Staff_Id, str_Fullname = c.str_Fullname, Job_Tiltle = c.Job_Tiltle, Department = c.Department, SubjectCode = c.SubjectCode, Subject = c.Subject });
                IEnumerable<sp_GetInstructorReport_TV_Result> filtered = data.Select(g => g.FirstOrDefault());

                var cscost = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in cscost.ToArray()
                             select new object[] {
                                    string.Empty,
                                    c.str_Staff_Id,
                                    c.str_Fullname,
                                    c.Job_Tiltle,
                                    c.Department,
                                    c.SubjectCode,
                                    c.Subject,
                        };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
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

        public ActionResult InstructorPrint()
        {
            var subjectName = string.IsNullOrEmpty(Request.QueryString["subjectName"]) ? string.Empty : Request.QueryString["subjectName"].Trim();
            var subjectCode = string.IsNullOrEmpty(Request.QueryString["subjectCode"]) ? string.Empty : Request.QueryString["subjectCode"].Trim();
            var traineeName = string.IsNullOrEmpty(Request.QueryString["traineeName"]) ? string.Empty : Request.QueryString["traineeName"].Trim();
            var traineeCode = string.IsNullOrEmpty(Request.QueryString["traineeCode"]) ? string.Empty : Request.QueryString["traineeCode"].Trim();
            var data =
                    _repoEmployee.GetAbility(
                        a =>
                            a.Trainee != null && a.Trainee.IsDeleted != true &&
                            a.Trainee.int_Role == (int)UtilConstants.ROLE.Instructor &&
                            (string.IsNullOrEmpty(traineeName) || a.Trainee.FirstName.Contains(traineeName.Trim()) || a.Trainee.LastName.Contains(traineeName.Trim())) &&
                            (string.IsNullOrEmpty(traineeCode) || a.Trainee.str_Staff_Id.Contains(traineeCode)) &&
                            a.SubjectDetailId.HasValue && a.SubjectDetail.IsDelete != true &&
                             a.SubjectDetail.int_Parent_Id != null && a.SubjectDetail.CourseTypeId.HasValue &&
                             a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General &&
                             a.SubjectDetail.int_Parent_Id != a.SubjectDetail.Id && a.SubjectDetail.SubjectDetail2.IsDelete != true &&
                            (string.IsNullOrEmpty(subjectName) || a.SubjectDetail.Name.Contains(subjectName)) &&
                            (string.IsNullOrEmpty(subjectCode) || a.SubjectDetail.Code.Contains(subjectCode)));
            var model = new InstructorSubjectModel();
            model.InstructorSubjectModelRps = data.AsEnumerable().Select(a => new InstructorSubjectModel.InstructorSubjectModelRp()
            {
                Eid = a?.Trainee?.str_Staff_Id,
                Name = ReturnDisplayLanguage(a?.Trainee?.FirstName, a?.Trainee?.LastName),
                Job = a?.Trainee?.JobTitle?.Name,
                Department = a?.Trainee?.Department?.Name,
                SubjectName = a?.SubjectDetail?.Name,
                Type = a?.Trainee?.bit_Internal == true ? "Internal" : "External",
                Relevant_Department = a?.Trainee?.Trainee_TrainingCenter?.ToList(),
            });

            return PartialView("InstructorPrint", model);
        }

        [HttpPost]
        public FileContentResult ExportInstructor(FormCollection form)
        {
            var subjectName = string.IsNullOrEmpty(form["subjectName"]) ? string.Empty : form["subjectName"].Trim();
            var subjectCode = string.IsNullOrEmpty(form["subjectCode"]) ? string.Empty : form["subjectCode"].Trim();
            var traineeName = string.IsNullOrEmpty(form["traineeName"]) ? string.Empty : form["traineeName"].Trim();
            var traineeCode = string.IsNullOrEmpty(form["traineeCode"]) ? string.Empty : form["traineeCode"].Trim();
            var departmentID = string.IsNullOrEmpty(Request.QueryString["department"]) ? -1 : Convert.ToInt32(Request.QueryString["department"].Trim());
            var jobtitleID = string.IsNullOrEmpty(Request.QueryString["jobtitle"]) ? -1 : Convert.ToInt32(Request.QueryString["jobtitle"].Trim());

            byte[] filecontent = ExportExcelInstructor(traineeCode, traineeName, subjectCode, subjectName, departmentID, jobtitleID);
            return File(filecontent, ExportUtils.ExcelContentType, "Instructor.xlsx");
        }

        private byte[] ExportExcelInstructor(string traineeCode, string traineeName, string subjectCode, string subjectName, int departmentID, int jobtitleID)
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/Trainee.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            var data =
                   _repoEmployee.GetAbility(
                        a =>
                            a.Trainee != null && a.Trainee.IsDeleted != true &&
                            a.Trainee.int_Role == (int)UtilConstants.ROLE.Instructor &&
                            (string.IsNullOrEmpty(traineeName) || a.Trainee.FirstName.Contains(traineeName.Trim()) || a.Trainee.LastName.Contains(traineeName.Trim())) &&
                            (string.IsNullOrEmpty(traineeCode) || a.Trainee.str_Staff_Id.Contains(traineeCode)) &&
                            a.SubjectDetailId.HasValue && a.SubjectDetail.IsDelete != true &&
                             a.SubjectDetail.int_Parent_Id != null && a.SubjectDetail.CourseTypeId.HasValue &&
                             a.SubjectDetail.CourseTypeId != (int)UtilConstants.CourseTypes.General &&
                             a.SubjectDetail.int_Parent_Id != a.SubjectDetail.Id && a.SubjectDetail.SubjectDetail2.IsDelete != true &&
                            (string.IsNullOrEmpty(subjectName) || a.SubjectDetail.Name.Contains(subjectName)) &&
                            (string.IsNullOrEmpty(subjectCode) || a.SubjectDetail.Code.Contains(subjectCode)) &&
                            (departmentID == -1 || a.Trainee.Department_Id == departmentID) &&
                            (jobtitleID == -1 || a.Trainee.Job_Title_id == jobtitleID)).AsEnumerable().Select(c =>
                        new
                        {
                            EID = string.Empty,
                            str_Staff_Id = c?.Trainee?.str_Staff_Id,
                            str_Fullname = ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName),
                            Job_Tiltle = c?.Trainee?.JobTitle?.Name,
                            Department = c?.Trainee?.Department?.Name,
                            Code = c?.SubjectDetail?.Code,
                            Subject = (c?.SubjectDetail?.IsActive != true ? "(" + UtilConstants.String_DeActive + ") " : "") + c?.SubjectDetail?.Name,
                            Type = c?.Trainee?.bit_Internal == true ? "Internal" : "External",
                            traineeObj = c?.Trainee,
                        }).Distinct();


            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                //string[] header = GetByKey("Report_Instructor").Split(new char[] { ',' });

                //ExcelRange cellHeader = worksheet.Cells[2, 8];
                //cellHeader.Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                //cellHeader.Style.Font.Size = 11;

                foreach (ExcelWorksheet aworksheet in xlPackage.Workbook.Worksheets)
                {

                    int startRow = 9;
                    int count = 0;
                    var maxcol = 1;
                    foreach (var item in data)
                    {
                        maxcol = 1;
                        ExcelRange cellNo = worksheet.Cells[startRow + count, maxcol];
                        cellNo.Value = count + 1;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellStaffId = worksheet.Cells[startRow + count, ++maxcol];
                        cellStaffId.Value = item.str_Staff_Id;
                        cellStaffId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellStaffId.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellStaffId.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellName = worksheet.Cells[startRow + count, ++maxcol];
                        cellName.Value = item.str_Fullname;
                        cellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellType = worksheet.Cells[startRow + count, ++maxcol];
                        cellType.Value = item.Type;
                        cellType.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellType.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellType.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellJob = worksheet.Cells[startRow + count, ++maxcol];
                        cellJob.Value = item.Job_Tiltle;
                        cellJob.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellJob.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellJob.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellRelevantTrainingDepartment = worksheet.Cells[startRow + count, ++maxcol];
                        cellRelevantTrainingDepartment.Value = ListRelevantTraining(item.traineeObj, item.Code);
                        cellRelevantTrainingDepartment.Style.WrapText = true;
                        cellRelevantTrainingDepartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellRelevantTrainingDepartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellRelevantTrainingDepartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellDepartment = worksheet.Cells[startRow + count, ++maxcol];
                        cellDepartment.Value = item.Department;
                        cellDepartment.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellDepartment.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellDepartment.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange celsCode = worksheet.Cells[startRow + count, ++maxcol];
                        celsCode.Value = item.Code;
                        celsCode.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        celsCode.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        celsCode.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange celsSubject = worksheet.Cells[startRow + count, ++maxcol];
                        celsSubject.Value = item.Subject;
                        celsSubject.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        celsSubject.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        celsSubject.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        celsSubject.Style.WrapText = true;

                        count++;
                    }
                }
                Bytes = xlPackage.GetAsByteArray();
            }
            return Bytes;
        }
        public string ListRelevantTraining(Trainee instructor, string subject)
        {
            var listString = "";
            var list = instructor.Trainee_TrainingCenter.Where(a => a.Department.Subject_TrainingCenter.Any(x => x.SubjectDetail.Code.Contains(subject)));
            foreach (var item in list)
            {
                listString += item?.Department?.Name + "\r\n";
            }

            return listString;
        }
        #endregion
        [AllowAnonymous]
        public FileContentResult ExportParticipants(FormCollection form)
        {
            var multiCourseCode = form.GetValues("multiCourseCode");
            var name = form["coursename"];
            var code = form["coursecode"];
            string fSearchDate_from = form["fSearchDate_from"];
            string fSearchDate_to = form["fSearchDate_to"];

            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDate_from, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateFrom);
            DateTime.TryParse(fSearchDate_to, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;


            var filecontent = ExportExcelParticipant(name, code, dateFrom, dateTo, multiCourseCode);
            return File(filecontent, ExportUtils.ExcelContentType, "Participant" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
        }
        private byte[] ExportExcelParticipant(string name, string code, DateTime? dateFrom, DateTime? dateTo, string[] multiCourseCode)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var templateFilePath = Server.MapPath(@"~/Template/ExcelFile/Participant.xlsx");
            var template = new FileInfo(templateFilePath);
            var data = (multiCourseCode != null && multiCourseCode.Any())
                ? CourseService.Get(a => multiCourseCode.Contains(a.Code), true).OrderBy(a => a.StartDate)
                : CourseService.Get(a =>
                    (string.IsNullOrEmpty(name) || a.Name.Contains(name)) &&
                    (string.IsNullOrEmpty(code) || a.Code.Contains(code)) &&
                    (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                            (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0)).OrderBy(a => a.StartDate);

            ExcelPackage xlPackage;
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                const int startRow = 9;
                var worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                const int rowHeader = 0;
                var count = 0;
                const int endcolumns = 5;
                const int column = 1;
                var row = 0;
                using (ExcelRange r = worksheet.Cells[startRow - 1, 1, startRow - 1 + data.Count() + 1, endcolumns])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    r.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    r.Style.WrapText = true;
                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                }



                var header = GetByKey("List_Of_Participants_Header").Split(',');
                if (header.Length == 3)
                {
                    var cellHeader = worksheet.Cells["E2"];
                    cellHeader.Value = string.IsNullOrEmpty(header[0]) ? string.Empty : header[0];
                    cellHeader.Style.Font.Size = 11;
                    var cellHeader1 = worksheet.Cells["E3"];
                    cellHeader1.Value = string.IsNullOrEmpty(header[1]) ? string.Empty : header[1];
                    cellHeader1.Style.Font.Size = 11;
                    var cellHeader2 = worksheet.Cells["E4"];
                    cellHeader2.Value = string.IsNullOrEmpty(header[2]) ? string.Empty : header[2];
                    cellHeader2.Style.Font.Size = 11;

                }
                var arrCourseCode = string.Empty;

                if (data.Any())
                {
                    foreach (var item in data)
                    {
                        count = 0;
                        arrCourseCode += " " + item.Code + ",";
                        worksheet.Cells[startRow + row + rowHeader, 1, startRow + row + rowHeader, endcolumns].Merge = true;
                        var cell = worksheet.Cells[startRow + row + rowHeader, 1];
                        cell.Value = item.Code.ToUpper().Trim() + " - " + item.Name.ToUpper().Trim();
                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGray);
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.Font.Bold = true;
                        cell.Style.Font.Size = 14;
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        var data_ = CourseDetailService.Get(a => a.CourseId == item.Id).Select(b => b.Id);
                        var members = CourseMemberService.Get(a => data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderByDescending(p => p.Id);
                        var trainees = members.GroupBy(a => a.Member_Id).Select(b => b.FirstOrDefault()).OrderByDescending(p => p.Id);
                        if (trainees.Any())
                        {
                            foreach (var trainee in trainees)
                            {
                                row++;
                                count++;
                                var cellNo = worksheet.Cells[startRow + row + rowHeader, column];
                                cellNo.Value = count;
                                cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                                var cellName = worksheet.Cells[startRow + row + rowHeader, column + 1];
                                cellName.Value = ReturnDisplayLanguage(trainee.Trainee?.FirstName, trainee.Trainee?.LastName);
                                cellName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                cellName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                                var cellStaff = worksheet.Cells[startRow + row + rowHeader, column + 2];
                                cellStaff.Value = trainee.Trainee?.str_Staff_Id.Trim();
                                cellStaff.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                cellStaff.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellStaff.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                                var cellDept = worksheet.Cells[startRow + row + rowHeader, column + 3];
                                cellDept.Value = trainee.Trainee?.Department?.Name.Trim();
                                cellDept.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                cellDept.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellDept.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                                var cellRemark = worksheet.Cells[startRow + row + rowHeader, column + 4];
                                cellRemark.Value = string.Empty;
                                cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            }
                            row++;
                        }
                    }
                }

                var title = "LIST OF PARTICIPANTS - REV... ";
                var cellTitle = worksheet.Cells["C2:C4,D2:D4"];
                cellTitle.Value = title + "\r\n" + "Attached with Trainning Plan " + arrCourseCode.TrimEnd(',');
                cellTitle.Style.Font.Bold = true;
                cellTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellTitle.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                cellTitle.Style.WrapText = true;

                var cellfooter = worksheet.Cells[startRow + row + rowHeader + 2, endcolumns - 1, startRow + row + rowHeader + 2, endcolumns];
                cellfooter.Value = "Date:......................................................";
                cellfooter.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellfooter.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellfooter.Merge = true;
                var cellfooter2 = worksheet.Cells[startRow + row + rowHeader + 3, endcolumns - 1, startRow + row + rowHeader + 3, endcolumns];
                cellfooter2.Value = "Verified by";
                cellfooter2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cellfooter2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cellfooter2.Merge = true;

                Bytes = xlPackage.GetAsByteArray();

            }
            return Bytes;
        }
        [AllowAnonymous]
        public ActionResult Participant()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
            ViewBag.CourseList = new SelectList(CourseService.Get(a => a.StartDate >= timenow,true).ToDictionary(a => a.Code, a => a.Code + "_" + a.Name).OrderBy(m => m.Key), "Key", "Value");
            return View();
        }
        [AllowAnonymous]
        public ActionResult ParticipantsPrint()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var name = Request.QueryString["coursename"];
            var code = Request.QueryString["coursecode"];
            var multiCourseCode = Request.QueryString["multiCourseCode[]"];

            var fSearchDate_from = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].ToString();
            var fSearchDate_to = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].ToString();

            //DateTime? FromDate_from, ToDate_from;
            //AppUtils.StringToDateRange(fSearchDate_from, out FromDate_from, out ToDate_from);
            //DateTime? FromDate_to, ToDate_to;
            //AppUtils.StringToDateRange(fSearchDate_to, out FromDate_to, out ToDate_to);

            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(fSearchDate_from, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateFrom);
            DateTime.TryParse(fSearchDate_to, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

            IEnumerable<Course> model = null;
            string[] courseCodes = null;
            if (!string.IsNullOrEmpty(multiCourseCode))
            {
                courseCodes = multiCourseCode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            if (courseCodes != null && courseCodes.Any())
            {
                model = CourseService.Get(a => courseCodes.Contains(a.Code), true);
            }
            else
            {
                model =
                    CourseService.Get(a =>
                    (string.IsNullOrEmpty(name) || a.Name.Contains(name)) &&
                    (string.IsNullOrEmpty(code) || a.Code.Contains(code)) &&
                    (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                            (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0)).OrderBy(a => a.StartDate);
            }
            var courseCode = string.Empty;
            var entitymodel = new ParticipantsModel();
            entitymodel.CourseCode = model.Aggregate(courseCode, (current, a) => current + (a.Code + ","))?.TrimEnd(',');
            entitymodel.header = GetByKey("List_Of_Participants_Header").Split(',');
            Dictionary<string, List<EmployeeView>> listemployee = new Dictionary<string, List<EmployeeView>>();
            foreach (var item in model)
            {
                var data_ = CourseDetailService.Get(a => a.CourseId == item.Id).Select(b => b.Id);
                var members = CourseMemberService.Get(a => data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderByDescending(p => p.Id);
                var trainees = members.GroupBy(a => a.Member_Id).Select(b => b.FirstOrDefault()).OrderByDescending(p => p.Id);
                var traineeslist = trainees.AsEnumerable().Select(a => new EmployeeView
                {
                    FullName = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                    str_Staff_Id = a.Trainee?.str_Staff_Id,
                    DepartmentName = a.Trainee?.Department?.Name,
                });
                listemployee.Add(item.Code, traineeslist.ToList());

            }
            entitymodel.courses = model.Select(a => new CourseView
            {
                Code = a.Code,
                Name = a.Name,
                trainees = listemployee[a.Code],
            });


            return PartialView("ParticipantsPrint", entitymodel);
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerParticipants(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                // xử lý param gửi lên 
                var name = Request.QueryString["coursename"].Trim();
                var code = Request.QueryString["coursecode"].Trim();
                var multiCourseCode = Request.QueryString["multiCourseCode[]"];

                //int coursename = string.IsNullOrEmpty(Request.QueryString["coursename"]) ? -1 : Convert.ToInt32(Request.QueryString["coursename"].Trim());
                string fSearchDate_from = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].ToString();
                string fSearchDate_to = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].ToString();

                //DateTime? FromDate_from, ToDate_from;
                //AppUtils.StringToDateRange(fSearchDate_from, out FromDate_from, out ToDate_from);
                //DateTime? FromDate_to, ToDate_to;
                //AppUtils.StringToDateRange(fSearchDate_to, out FromDate_to, out ToDate_to);

                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDate_from, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateFrom);
                DateTime.TryParse(fSearchDate_to, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var timenow = DateTime.Now.Date.AddDays(-365).AddHours(23).AddMinutes(59).AddSeconds(59);
                IEnumerable<Course> filtered = null;
                string[] courseCodes = null;
                if (!string.IsNullOrEmpty(multiCourseCode))
                {
                    courseCodes = multiCourseCode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                if (courseCodes != null && courseCodes.Any())
                {
                    filtered = CourseService.Get(a => courseCodes.Contains(a.Code), true);
                }
                else
                {
                    filtered = CourseService.Get(a =>
                    (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(code) && dateFrom == DateTime.MinValue && dateTo == DateTime.MinValue ? a.StartDate >= timenow :  true) &&
                    (string.IsNullOrEmpty(name) || a.Name.Contains(name)) &&
                    (string.IsNullOrEmpty(code) || a.Code.Contains(code)) &&
                    (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                            (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0));
                }
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                            : sortColumnIndex == 3 ? c?.StartDate
                                                            : sortColumnIndex == 4 ? c?.EndDate
                                                            : sortColumnIndex == 5 ? c?.CourseTypeId
                                                            : sortColumnIndex == 6 ? (object)c?.is_data_new
                                                          : c?.StartDate);


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc")
                        ? filtered.OrderBy(orderingFunction)
                        : filtered.OrderByDescending(orderingFunction);
                }
                else
                {
                    filtered = filtered.OrderByDescending(a => a.StartDate);
                }
                // var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                  // ReturnColumncheck(c.Course_Id) ,
                                    string.Empty,
                                    c.Code,
                                    "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+c.Name+"</a></span>",
                                    c.StartDate.HasValue ? c.StartDate.Value.ToString("dd/MM/yyyy") : "",
                                    c.EndDate.HasValue ? c.EndDate.Value.ToString("dd/MM/yyyy") :"",
                                   //c?.Course_Type?.str_Name,
                                    c.TMS_APPROVES.Any() ?   ReturnColumnStatus(c.Id) : "",

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
        [AllowAnonymous]
        public ActionResult AjaxHandlerTrainee(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {

                var data_ = CourseDetailService.Get(a => a.CourseId == id).Select(b => b.Id);
                var data = CourseMemberService.Get(a => data_.Contains((int)a.Course_Details_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).OrderByDescending(p => p.Id);

                IEnumerable<TMS_Course_Member> filtered = data.GroupBy(a => a.Member_Id).Select(b => b.FirstOrDefault());

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, string> orderingFunction = (c => sortColumnIndex == 1 ? ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName)
                                                                        : sortColumnIndex == 2 ? c?.Trainee?.str_Staff_Id
                                                                        : sortColumnIndex == 3 ? c?.Trainee?.Department?.Name
                                                                        : ReturnDisplayLanguage(c?.Trainee?.FirstName, c?.Trainee?.LastName));

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
                                    string.Empty,
                                    ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                                    c?.Trainee?.str_Staff_Id,
                                    c?.Trainee?.Department?.Name
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
        private readonly List<int> _deptChil = new List<int>();
        private List<int> TinhGiaiThua(int idParent)
        {
            var dept_Con = DepartmentService.Get(a => a.ParentId == idParent);
            if (dept_Con.Any())
            {
                foreach (var item in dept_Con)
                {
                    TinhGiaiThua(item.Id);
                    _deptChil.Add(item.Id);
                }
            }
            return _deptChil;
        }
        private string ConvertDot(double value, int round = 1)
        {
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

            nfi.CurrencyDecimalSeparator = ".";
            nfi.CurrencyGroupSeparator = ",";
            nfi.CurrencySymbol = "";
            var answer = Convert.ToDecimal(value).ToString(round == 1 ? "f1" : "f2",
                  nfi);
            return answer;
        }

        public object ConvertValue(bool? isAvarage, double? value_point, string value_result)
        {

            object result = null;
            if (isAvarage.HasValue && isAvarage.Value)
            {
                result = value_point;
            }
            else
            {
                if (!string.IsNullOrEmpty(value_result))
                {
                    result = value_result;
                }
            }
            return result;
        }
    }
}
