using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Users;
using TMS.Core.ViewModels.ReportModels;
using TrainingCenter.Utilities;

namespace TrainingCenter.Controllers
{
    using DAL.Repositories;
    using OfficeOpenXml.Style;
    using System.Data.Entity.SqlServer;
    using System.Globalization;
    using System.Text;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.UserModels;
    using TMS.Core.ViewModels.ViewModel;

    public class TrainingTimeController : BaseAdminController
    {
        #region Init


        #endregion
        //
        // GET: /Nationality/
        private readonly IRepository<Trainee_TrainingCenter> _repoTrainee_TrainingCenter = null;
        private readonly IRepository<TMS_APPROVES_HISTORY> _repoTMS_APPROVES_HISTORY = null;
        private readonly IRepository<Course_Detail_Instructor> _repoCourseDetailInstructor = null;
        private readonly IRepository<Trainee> _repoTrainee = null;
        private readonly IRepository<Department> _repoDepartment = null;
        public TrainingTimeController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService, IRepository<Trainee_TrainingCenter> repoTrainee_TrainingCenter, IRepository<TMS_APPROVES_HISTORY> repoTMS_APPROVES_HISTORY, IRepository<Course_Detail_Instructor> repoCourseDetailInstructor, IRepository<Trainee> repoTrainee, IRepository<Department> repoDepartment) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoTrainee_TrainingCenter = repoTrainee_TrainingCenter;
            _repoTMS_APPROVES_HISTORY = repoTMS_APPROVES_HISTORY;
            _repoCourseDetailInstructor = repoCourseDetailInstructor;
            _repoTrainee = repoTrainee;
            _repoDepartment = repoDepartment;
        }

        public ActionResult Index(int id = 0)
        {
            var timenow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var model = new TrainingAllowanceModel()
            {
                From = timenow,
                To = timenow.AddMonths(1).AddDays(-1),
                Department = DepartmentService.Get(a => a.is_training == true).OrderBy(a => a.Id)
            };

            return View(model);
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {

                var strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().Trim();
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].ToLower().Trim();
                var strFromdate = string.IsNullOrEmpty(Request.QueryString["from"]) ? string.Empty : Request.QueryString["from"].Trim();
                var strTodate = string.IsNullOrEmpty(Request.QueryString["to"]) ? string.Empty : Request.QueryString["to"].Trim();
                DateTime dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                DateTime dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                List<string> lst = string.IsNullOrEmpty(Request.QueryString["DepartmentId[]"]) ? new List<string>() : Request.QueryString["DepartmentId[]"].Trim().Split(new char[] { ',' }).ToList();
                List<int> listDepartmentID = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                var listCourseDetails = CourseDetailService.Get(a => (a.dtm_time_to >= dateFrom && a.dtm_time_to <= dateTo) && a.TMS_APPROVES.Any(t => t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve)).Select(a => a.Id).ToList();
                var coursedetailsInstructor = CourseDetailService.GetDetailInstructors(a => a.Course_Detail_Id.HasValue && listCourseDetails.Contains(a.Course_Detail_Id.Value)).GroupBy(a => a.Instructor_Id).Select(t => t.Key);
                IEnumerable<Trainee> filtered =
                    EmployeeService.Get(
                        a => coursedetailsInstructor.Contains(a.Id) && a.int_Role == (int)UtilConstants.ROLE.Instructor && (string.IsNullOrEmpty(strCode) || a.str_Staff_Id.Contains(strCode)) && (String.IsNullOrEmpty(strName) || ((a.FirstName.Trim() + " " + a.LastName.Trim()).Contains(strName.Trim()))));
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                          : sortColumnIndex == 2 ? c?.FirstName
                                                          : c.FirstName);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                   : filtered.OrderByDescending(orderingFunction);


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let sum = c.Course_Detail_Instructor.Where(a => a.Course_Detail_Id.HasValue && listCourseDetails.Contains(a.Course_Detail_Id.Value)).Sum(a => a.Duration ?? 0)
                             let courseIntrucdetail = c.Course_Detail_Instructor.Where(a => a.Course_Detail_Id.HasValue && listCourseDetails.Contains(a.Course_Detail_Id.Value)).GroupBy(a => new { a.Course_Detail_Id }).Select(g => new Course_Detail_Instructor
                             {
                                 Allowance = g.FirstOrDefault()?.Allowance != null ? (decimal.Parse(g.Sum(a => a.Duration).ToString()) * g.FirstOrDefault()?.Allowance) : 0
                             })
                             select new object[] {
                        string.Empty,
                        c.str_Staff_Id,
                        //c.FirstName + " " +c.LastName,
                        ReturnDisplayLanguage(c.FirstName ,c.LastName),
                        sum > 0 ? sum.ToString() : "",
                        "Huy ơi móc dữ liệu ra dùm nhoa.",
                        "Huy ơi móc dữ liệu ra dùm nhoa.",
                        "Huy ơi móc dữ liệu ra dùm nhoa.",
                        courseIntrucdetail.Sum(b => b.Allowance)?.ToString("#,###"),
                        "<span data-value='"+ c.Id  +"' class='expand' style='cursor: pointer;'><i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; '></i></span>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "TrainingTime/AjaxHandler", ex.Message);
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
        public ActionResult AjaxHandlerPaymentRequest(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower();
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].ToLower();
                var strFromdate = string.IsNullOrEmpty(Request.QueryString["from"]) ? string.Empty : Request.QueryString["from"].ToLower();
                var strTodate = string.IsNullOrEmpty(Request.QueryString["to"]) ? string.Empty : Request.QueryString["to"].ToLower();
               
                var dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                var dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                List<int> listCourseID = new List<int>();
                var lst = string.IsNullOrEmpty(Request.QueryString["DepartmentId[]"]) ? new List<string>() : Request.QueryString["DepartmentId[]"].Trim().Split(',').ToList();
                listCourseID = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                var data = _repoTrainee_TrainingCenter.GetAll(a => (!listCourseID.Any() || listCourseID.Contains((int)a.khoidaotao_id))).Select(a => a.instructor_id);


                //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                List<int> listCourseDetails = CourseDetailService.GetAllApi(a => a.IsDeleted != true && a.TMS_APPROVES.Any(t => t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve &&
              ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, t.dtm_requested_date) >= 0) &&
                  (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", t.dtm_requested_date, dateTo) >= 0))
                )).Select(a => a.Id).ToList();
                var coursedetailsInstructor = _repoCourseDetailInstructor.GetAll(a => data.Contains(a.Instructor_Id) && a.Course_Detail_Id.HasValue && listCourseDetails.Contains(a.Course_Detail_Id.Value) && a.Type == (int)UtilConstants.TypeInstructor.Instructor).GroupBy(a => a.Instructor_Id).Select(t => t.Key);
                IEnumerable<Trainee> filtered = _repoTrainee.GetAll(a => coursedetailsInstructor.Contains(a.Id) && a.int_Role == (int)UtilConstants.ROLE.Instructor &&
                a.IsDeleted != true &&
                (string.IsNullOrEmpty(strCode) || a.str_Staff_Id.Contains(strCode)) &&
                (string.IsNullOrEmpty(strName) || a.FirstName.Contains(strName) || a.LastName.Contains(strName) || (a.LastName + " " + a.FirstName).Contains(strName))).OrderBy(m => m.LastName + " " + m.FirstName);


                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                          : sortColumnIndex == 2 ? ReturnDisplayLanguage(c?.FirstName, c?.LastName)
                                                          : c.Id.ToString());
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let sum = c?.Course_Detail_Instructor.Where(a => a.Course_Detail_Id.HasValue && listCourseDetails.Contains(a.Course_Detail_Id.Value)).Sum(a => a.Duration ?? 0)
                             let courseIntrucdetail = c?.Course_Detail_Instructor.Where(a => a.Course_Detail_Id.HasValue && listCourseDetails.Contains(a.Course_Detail_Id.Value)).GroupBy(a => new { a.Course_Detail_Id }).Select(g => new Course_Detail_Instructor
                             {
                                 Allowance = g.FirstOrDefault(a => a.Instructor_Id == c?.Id)?.Allowance != null ? (decimal.Parse(g.Sum(a => a.Duration).ToString()) * g.FirstOrDefault(a => a.Instructor_Id == c?.Id)?.Allowance) : 0
                             })
                             let paid = c.Payments.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Paid && a.Is_Cancel == null && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null && listCourseDetails.Contains((int)a.CourseDetail_Id)).Sum(a => a.Amount)
                             let pending = c.Payments.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Pending && a.Is_Cancel == null && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null && listCourseDetails.Contains((int)a.CourseDetail_Id)).Sum(a => a.Amount)
                             select new object[] {
                                string.Empty,
                                c?.str_Staff_Id,
                                ReturnDisplayLanguage(c.FirstName,c.LastName),
                                sum > 0 ? sum.ToString():"",
                                courseIntrucdetail.Sum(b => b.Allowance)?.ToString("#,###"),
                                paid?.ToString("#,###"),
                                (courseIntrucdetail.Sum(b => b.Allowance) - paid - pending)?.ToString("#,###"),
                                 pending?.ToString("#,###"),
                                "<span data-value='"+ c?.Id  +"' class='expand' style='cursor: pointer;'><i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; color: black; '></i></span>" + "<input type='hidden' value='"+c?.Id+"_"+((courseIntrucdetail.Sum(b => b.Allowance) - paid) ?? 0)+ "' name='InstructorId' /><input type='hidden' value='"+c?.Id+ "' name='id' />" +
                                "<span onclick='goPayment("+c?.Id+",0)'><i class='fa fa-credit-card' aria-hidden='true' style='cursor: pointer;' style=' font-size: 16px; color: black; '></i></span>"
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
        public ActionResult AjaxHandlerPaymentApproval(jQueryDataTableParamModel param)
        {
            try
            {
                var strCode = string.IsNullOrEmpty(Request.QueryString["Code2"]) ? string.Empty : Request.QueryString["Code2"].ToLower();
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName2"]) ? string.Empty : Request.QueryString["FullName2"].ToLower();
                var strFromdate = string.IsNullOrEmpty(Request.QueryString["from2"]) ? string.Empty : Request.QueryString["from2"].ToLower();
                var strTodate = string.IsNullOrEmpty(Request.QueryString["to2"]) ? string.Empty : Request.QueryString["to2"].ToLower();
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(strFromdate, out dateFrom);
                DateTime.TryParse(strTodate, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                List<int> listCourseID = new List<int>();
                var lst = string.IsNullOrEmpty(Request.QueryString["DepartmentId2[]"]) ? new List<string>() : Request.QueryString["DepartmentId2[]"].Trim().Split(',').Where(p => !string.IsNullOrEmpty(p)).ToList();
                listCourseID = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();

                IEnumerable<int?> data = _repoTrainee_TrainingCenter.GetAll(a => listCourseID.Count() == 0 || listCourseID.Contains((int)a.khoidaotao_id)).Select(b => b.instructor_id);
                IEnumerable<int> getid = _repoTMS_APPROVES_HISTORY.GetAll(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_courseDetails_Id.HasValue).OrderByDescending(x => x.id).GroupBy(a => a.int_courseDetails_Id).Select(c => c.OrderByDescending(x => x.id).FirstOrDefault()).Select(a => a.id);
                List<int?> listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(a => getid.Contains(a.id)
                &&
                 ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.dtm_requested_date) >= 0) &&
                  (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.dtm_requested_date, dateTo) >= 0))
                ).Select(a => a.int_courseDetails_Id).ToList();
                var coursedetailsInstructor = _repoCourseDetailInstructor.GetAll(a => data.Contains(a.Instructor_Id) && listCourseDetails.Contains(a.Course_Detail_Id) && a.Type == (int)UtilConstants.TypeInstructor.Instructor).GroupBy(a => a.Instructor_Id).Select(t => t.Key);
                IEnumerable<Trainee> filtered = _repoTrainee.GetAll(a => coursedetailsInstructor.Contains(a.Id) && a.int_Role == (int)UtilConstants.ROLE.Instructor &&
                a.IsDeleted != true &&
                (string.IsNullOrEmpty(strCode) || a.str_Staff_Id.Contains(strCode)) &&
                (string.IsNullOrEmpty(strName) || a.FirstName.Contains(strName) || a.LastName.Contains(strName) || (a.LastName + " " + a.FirstName).Contains(strName))
                ).OrderBy(m => m.LastName + " " + m.FirstName);

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                          : sortColumnIndex == 2 ? ReturnDisplayLanguage(c?.FirstName, c?.LastName)
                                                          : c.Id.ToString());
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                 : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let sum = c.Course_Detail_Instructor.Where(a => listCourseDetails.Contains(a.Course_Detail_Id)).Sum(a => a.Duration ?? 0)
                             let courseIntrucdetail = c.Course_Detail_Instructor.Where(a => listCourseDetails.Contains(a.Course_Detail_Id)).GroupBy(a => new { a.Course_Detail_Id }).Select(g => new Course_Detail_Instructor
                             {
                                 Allowance = g.FirstOrDefault(a => a.Instructor_Id == c.Id)?.Allowance != null ? (decimal.Parse(g.Sum(a => a.Duration).ToString()) * g.FirstOrDefault(a => a.Instructor_Id == c.Id)?.Allowance) : 0
                             })
                             let paid = c.Payments.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Paid && a.Is_Cancel == null && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null && listCourseDetails.Contains(a.CourseDetail_Id)).Sum(a => a.Amount)
                             let pending = c.Payments.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Pending && a.Is_Cancel == null && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null && listCourseDetails.Contains(a.CourseDetail_Id)).Sum(a => a.Amount)
                             select new object[] {
                                string.Empty,
                                c?.str_Staff_Id,
                                ReturnDisplayLanguage(c?.FirstName,c?.LastName),
                                 sum > 0 ? sum.ToString() : "",
                                 courseIntrucdetail.Sum(b => b.Allowance)?.ToString("#,###"),
                                paid?.ToString("#,###"),
                                (courseIntrucdetail.Sum(b => b.Allowance) - paid - pending)?.ToString("#,###"),
                                 pending?.ToString("#,###"),

                                "<span data-value='"+ c.Id  +"' class='expand' style='cursor: pointer;'><i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; '></i></span>"+ "<input type='hidden' value='"+c?.Id+"_"+((courseIntrucdetail.Sum(b => b.Allowance) - paid) ?? 0)+ "' name='InstructorId2' /><input type='hidden' value='"+c?.Id+ "' name='id2' />&nbsp;"+
                                "<span onclick='goPayment("+c?.Id+",1)'><i class='fa fa-credit-card' aria-hidden='true' style='cursor: pointer;' style=' font-size: 16px; color: black; '></i></span>"
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

        public ActionResult AjaxHandlerSubject(jQueryDataTableParamModel param)
        {
            try
            {
                string strFromdate = string.IsNullOrEmpty(Request.QueryString["from"]) ? string.Empty : Request.QueryString["from"].ToLower().ToString();
                string strTodate = string.IsNullOrEmpty(Request.QueryString["to"]) ? string.Empty : Request.QueryString["to"].ToLower().ToString();
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].ToLower().ToString());
                DateTime dtmFromdate = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                DateTime dtmTodate = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);

                var data = CourseDetailService.GetDetailInstructors(a => a.Instructor_Id == id && a.Course_Detail.IsDeleted == false && (a.Course_Detail.dtm_time_to >= dtmFromdate && a.Course_Detail.dtm_time_to <= dtmTodate) && a.Course_Detail.TMS_APPROVES.Any(t => t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve)).GroupBy(g => g.Course_Detail).ToList()
                    .Select(t => new TrainingTimeModel
                    {
                        strTotal = t.Key.Id.ToString(),
                        strName = t.Key.SubjectDetail.Name,
                        CourseName = t.Key.Course.Name,
                        strCode = t.Key.Course.Code,
                        duration = t.Any(a => a.Duration.HasValue) ? t.Sum(a => a.Duration).Value : 0,
                        dateFrom = t.Key?.dtm_time_from,
                        dateTo = t.Key?.dtm_time_to,
                        //date = DateUtil.DateToString(t.Key?.dtm_time_from, "dd/MM/yyyy") + " - " + DateUtil.DateToString(t.Key?.dtm_time_to, "dd/MM/yyyy"),
                        timeFrom = t.Key?.time_from,
                        timeTo = t.Key?.time_from,
                        strTrainingAlow = t.FirstOrDefault().Allowance.HasValue ? t.FirstOrDefault().Allowance.Value : 0, /*t.FirstOrDefault()?.Trainee.Instructor_Ability.Where(a=>a.SubjectDetailId == t.Key.SubjectDetail.Id).FirstOrDefault()?.Allowance?.ToString("#,###")*/
                    });




                IEnumerable<TrainingTimeModel> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TrainingTimeModel, object> orderingFunction = (c => sortColumnIndex == 1 ? c.strCode
                                                                        : sortColumnIndex == 2 ? c.CourseName
                                                                        : sortColumnIndex == 3 ? c.strName
                                                                        : sortColumnIndex == 4 ? c.dateFrom
                                                                        : sortColumnIndex == 5 ? c.timeFrom
                                                                        : sortColumnIndex == 6 ? (object)c.duration
                                                                        : c.strCode);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let date_string = DateUtil.DateToString(c.dateFrom, "dd/MM/yyyy") + " - " + DateUtil.DateToString(c.dateTo, "dd/MM/yyyy")
                             let allowance = c.strTrainingAlow.ToString("#,###")
                             select new object[] {
                                string.Empty,
                                 c.strCode ,
                                 c.CourseName ,
                                 c.strName ,
                                c.date,
                                c.timeFrom +" - "+ c.timeTo,
                                c.duration,
                                string.Format("{0:#,###}",allowance),
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "TrainingTime/AjaxHandlerSubject", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        private string GenStatus(int? status, string name, bool isHtml = false)
        {
            var html = "";
            var result = name;
            switch (status)
            {
                case (int)UtilConstants.PaymentStatus.Paid:
                    html = "<span class='label label-success'>" + name + "</span>";
                    break;
                case (int)UtilConstants.PaymentStatus.Pending:
                    html = "<span class='label label-default'>" + name + "</span>";
                    break;
                case (int)UtilConstants.PaymentStatus.NoPayment:
                    html = "<span class='label label-danger'>" + name + "</span>";
                    break;
                default:
                    html = "<span class='label label-danger'>" + name + "</span>";
                    break;
            }

            return isHtml ? result : html;
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerDetailSubjectPaymentRequest(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                string strFromdate = string.IsNullOrEmpty(Request.QueryString["from"]) ? string.Empty : Request.QueryString["from"].ToLower().ToString();
                string strTodate = string.IsNullOrEmpty(Request.QueryString["to"]) ? string.Empty : Request.QueryString["to"].ToLower().ToString();
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].ToLower().ToString());
                
                DateTime dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                DateTime dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                var data = _repoCourseDetailInstructor.GetAll(a => a.Instructor_Id == id && a.Course_Detail.IsDeleted != true && a.Course_Detail.Course.IsDeleted != true && a.Course_Detail.TMS_APPROVES.Any(t =>
              ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, t.dtm_requested_date) >= 0) &&
                  (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", t.dtm_requested_date, dateTo) >= 0))
              && t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve)).AsEnumerable().Select(t => new TrainingTimeModel
              {
                  CourseDetailId = t.Course_Detail?.Id,
                  strTotal = t.Course_Detail?.Id.ToString(),
                  strName = t.Course_Detail?.SubjectDetail?.Name,
                  CourseName = t.Course_Detail?.Course?.Name,
                  strCode = t.Course_Detail?.Course?.Code,
                  duration = t.Duration ?? 0,
                  dateFrom = t.Course_Detail?.dtm_time_from,
                  dateTo = t.Course_Detail?.dtm_time_to,
                  //date = DateUtil.DateToString(t.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + " - " + DateUtil.DateToString(t.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                  time = (t.Course_Detail?.time_from != null ? t.Course_Detail?.time_from?.ToString().Substring(0, 2) + ":" + t.Course_Detail?.time_from?.ToString().Substring(2) : null) + " - " + (t.Course_Detail?.time_to != null ? t.Course_Detail?.time_to?.ToString().Substring(0, 2) + ":" + t.Course_Detail?.time_to?.ToString().Substring(2) : null),
                  strTrainingAlow = t.Allowance.HasValue ? t.Allowance.Value : 0,
                  dateRequest = t.Course_Detail?.TMS_APPROVES?.FirstOrDefault()?.dtm_requested_date,
                  //PaymentStatus = t.Course_Detail?.Payments?.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null)?.Method_Id,
                  //PaymentStatusName = t.Course_Detail?.Payments?.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null)?.Payment_Status?.Name,
                  PaymentStatus = t.Course_Detail?.Payments?.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null)?.Method_Id,
                  PaymentStatusName = t.Course_Detail?.Payments?.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null)?.Payment_Status?.Name,
              });




                IEnumerable<TrainingTimeModel> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TrainingTimeModel, object> orderingFunction = (c => sortColumnIndex == 1 ? c.strCode
                                                                        : sortColumnIndex == 2 ? c.CourseName
                                                                        : sortColumnIndex == 3 ? c.strName
                                                                        : sortColumnIndex == 4 ? c.dateFrom
                                                                        : sortColumnIndex == 5 ? c.timeFrom
                                                                        : sortColumnIndex == 6 ? (object)c.duration
                                                                        : c.strCode);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let daterequest_string = DateUtil.DateToString(c.dateRequest, "dd/MM/yyyy")
                             let date = DateUtil.DateToString(c?.dateFrom, "dd/MM/yyyy") + " - " + DateUtil.DateToString(c?.dateTo, "dd/MM/yyyy")
                             let allowance = c.strTrainingAlow.ToString("#,###")
                             select new object[] {
                                string.Empty,
                                 c.strCode ,
                                 c.CourseName ,
                                 c.strName ,
                                 date,
                                 c?.time,
                                 c.duration,
                                 daterequest_string,
                                 allowance,
                                 GenStatus(c?.PaymentStatus,c?.PaymentStatusName)
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
        public ActionResult AjaxHandlerDetailSubjectPaymentApproval(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                string strFromdate = string.IsNullOrEmpty(Request.QueryString["from2"]) ? string.Empty : Request.QueryString["from2"].ToLower().ToString();
                string strTodate = string.IsNullOrEmpty(Request.QueryString["to2"]) ? string.Empty : Request.QueryString["to2"].ToLower().ToString();
                int id = string.IsNullOrEmpty(Request.QueryString["id"]) ? -1 : Convert.ToInt32(Request.QueryString["id"].ToLower().ToString());
               
                DateTime dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                DateTime dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                // List<int> getid_ = _repoTMS_APPROVES_HISTORY.GetAll(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_courseDetails_Id.HasValue).OrderByDescending(a => a.id ).GroupBy(a => a.int_courseDetails_Id).Select(c => c.OrderByDescending(x => x.id).FirstOrDefault()).Select(a => a.id).ToList();
                // var getid = _repoTMS_APPROVES_HISTORY.GetAll(a => getid_ .Contains(a.id) &&
                //((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.dtm_requested_date) >= 0) &&
                // (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.dtm_requested_date, dateTo) >= 0)));
                var getid = _repoTMS_APPROVES_HISTORY.GetAll(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_courseDetails_Id.HasValue).OrderByDescending(a => a.id).GroupBy(a => a.int_courseDetails_Id).Select(c => c.OrderByDescending(x => x.id).FirstOrDefault()).Where(a => ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.dtm_requested_date) >= 0) && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.dtm_requested_date, dateTo) >= 0)));
                var listCourseDetails = getid.Select(a => a.int_courseDetails_Id);
                var listCourseDetails2 = getid;
                //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                //  var data = _repoCourseDetailInstructor.GetAll(a => a.Instructor_Id == id && a.Course_Detail.IsDeleted != true && a.Course_Detail.Course.IsDeleted != true && listCourseDetails.Contains(a.Course_Detail_Id) && a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(t => new TrainingTimeModel
                //{
                //    CourseDetailId = t.Course_Detail_Id,
                //    strName = t.Course_Detail.SubjectDetail.Name.ToString() ?? "",
                //    CourseName = t.Course_Detail.Course.Name.ToString() ?? "",
                //    strCode = t.Course_Detail.Course.Code.ToString() ?? "",
                //    duration = t.Duration ?? 0,
                //    dateFrom = t.Course_Detail.dtm_time_from,
                //    dateTo = t.Course_Detail.dtm_time_to,
                //    timeFrom = t.Course_Detail.time_from ?? "",
                //    timeTo = t.Course_Detail.time_to ?? "",
                //    strTrainingAlow = t.Allowance.HasValue ? t.Allowance.Value : 0,
                //    //dateRequest = listCourseDetails2.FirstOrDefault(a => a.int_courseDetails_Id == t.Course_Detail.Id).dtm_requested_date ?? DateTime.Now,
                //    PaymentStatus = t.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null).Method_Id ?? 0,
                //    PaymentStatusName = t.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null).Payment_Status.Name ?? "",
                //});

                var data = _repoCourseDetailInstructor.GetAll(a => a.Instructor_Id == id && a.Course_Detail.IsDeleted != true && a.Course_Detail.Course.IsDeleted != true && listCourseDetails.Contains(a.Course_Detail_Id) && a.Type == (int)UtilConstants.TypeInstructor.Instructor);

                IEnumerable<Course_Detail_Instructor> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail_Instructor, object> orderingFunction = (c => sortColumnIndex == 1 ? c.Course_Detail.Course.Code
                                                                        : sortColumnIndex == 2 ? c.Course_Detail.Course.Name
                                                                        : sortColumnIndex == 3 ? c.Course_Detail.SubjectDetail.Name
                                                                        : sortColumnIndex == 4 ? c.Course_Detail.dtm_time_from
                                                                        : sortColumnIndex == 5 ? (c.Course_Detail.time_from != null ? c.Course_Detail.time_from.Substring(0, 2) + "" + c.Course_Detail.time_from.Substring(2) : null) + " - " + (c.Course_Detail.time_to != null ? c.Course_Detail.time_to.Substring(0, 2) + "" + c.Course_Detail.time_to.Substring(2) : null)
                                                                        : sortColumnIndex == 6 ? (object)c.Duration
                                                                        : c.Course_Detail.Course.Code);





                //IEnumerable<TrainingTimeModel> filtered = data;
                //var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                //Func<TrainingTimeModel, object> orderingFunction = (c => sortColumnIndex == 1 ? c.strCode
                //                                                        : sortColumnIndex == 2 ? c.CourseName
                //                                                        : sortColumnIndex == 3 ? c.strName
                //                                                        : sortColumnIndex == 4 ? c.dateFrom
                //                                                        : sortColumnIndex == 5 ? (c.timeFrom != null ? c.timeFrom.Substring(0, 2) + "" + c.timeFrom.Substring(2) : null) + " - " + (c.timeTo != null ? c.timeTo.Substring(0, 2) + "" + c.timeTo.Substring(2) : null)
                //                                                        : sortColumnIndex == 6 ? (object)c.duration
                //                                                        : c.strCode);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let daterequest = DateUtil.DateToString(listCourseDetails2.FirstOrDefault(a => a.int_courseDetails_Id == c.Course_Detail_Id)?.dtm_requested_date, "dd/MM/yyyy")
                             let allowance = c.Allowance.HasValue ? c.Allowance.Value.ToString("#,###") : "0"
                             let PaymentStatus = c.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted != true && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null)?.Method_Id ?? 0
                             let PaymentStatusName = c.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == id && a.Is_Cancel == null && a.Is_Deleted != true && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null)?.Payment_Status?.Name
                             select new object[] {
                                string.Empty,
                                 c.Course_Detail.Course.Code ,
                                 c.Course_Detail.Course.Name ,
                                 c.Course_Detail.SubjectDetail.Name ,
                                 DateUtil.DateToString(c.Course_Detail.dtm_time_from, "dd/MM/yyyy") + " - " + DateUtil.DateToString(c.Course_Detail.dtm_time_to, "dd/MM/yyyy"),
                                (c.Course_Detail.time_from != null ? c.Course_Detail.time_from.Substring(0, 2) + c.Course_Detail.time_from.Substring(2) : null) + " - " + (c.Course_Detail.time_to != null ? c.Course_Detail.time_to.Substring(0, 2) + c.Course_Detail.time_to.Substring(2) : null),
                                c.Duration,
                                daterequest,
                                allowance,
                                 GenStatus(PaymentStatus,PaymentStatusName)
                                //payment_courseDetailId.Contains(c?.CourseDetailId) ? "<span class='label label-success'>Paid</span>" :  "<span class='label label-danger'>No Payment</span>"
                        };
                var jsonResult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
             JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
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
        [HttpPost]
        public FileContentResult ExportPaymentRequest(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            string strCode = string.IsNullOrEmpty(form["code"]) ? string.Empty : form["Code"].ToLower().ToString();
            string strName = string.IsNullOrEmpty(form["fullname"]) ? string.Empty : form["fullname"].ToLower().ToString();
            string strFromdate = string.IsNullOrEmpty(form["from"]) ? string.Empty : form["from"].ToLower().ToString();
            string strTodate = string.IsNullOrEmpty(form["to"]) ? string.Empty : form["to"].ToLower().ToString();
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(strFromdate, out dateFrom);
            DateTime.TryParse(strTodate, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

            //dtmFromdate = dtmFromdate.Date;
            //dtmTodate = dtmTodate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            var lst = string.IsNullOrEmpty(form["int_khoidaotao"]) ? new List<int>() : form["int_khoidaotao"].Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a => Convert.ToInt32(a)).ToList();

            byte[] filecontent = ExportExcelPaymentRequest(strCode, strName, dateFrom, dateTo, lst);
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "TrainingAllowance.xlsx");
            }
            return null;
        }
        private byte[] ExportExcelPaymentRequest(string strCode, string strName, DateTime? dtmFromdate, DateTime? dtmTodate, List<int> listCourseID)
        {
            string templateFilePath = Server.MapPath(
@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/TrainingAllow.xlsx");
            System.IO.FileInfo template = new System.IO.FileInfo(templateFilePath);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var data = _repoTrainee_TrainingCenter.GetAll(a => (!listCourseID.Any() || listCourseID.Contains((int)a.khoidaotao_id))).Select(a => a.instructor_id);
            var dateFromTo = string.Format("\t Date: from {0} to {1}", dtmFromdate?.ToString("dd/MM/yyyy"), dtmTodate?.ToString("dd/MM/yyyy"));

            List<int> listCourseDetails = CourseDetailService.Get(a => a.IsDeleted != true && a.TMS_APPROVES.Any(t => t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve &&

            (((dtmFromdate == DateTime.MinValue || SqlFunctions.DateDiff("day", dtmFromdate, t.dtm_requested_date) >= 0) &&
                    (dtmTodate == DateTime.MinValue || SqlFunctions.DateDiff("day", t.dtm_requested_date, dtmTodate) >= 0))) &&
            (t.dtm_requested_date >= dtmFromdate && t.dtm_requested_date <= dtmTodate)
            )).Select(a => a.Id).ToList();

            IEnumerable<Course_Detail_Instructor> detail_i = _repoCourseDetailInstructor.GetAll(a => data.Contains(a.Instructor_Id) && a.Course_Detail_Id.HasValue && listCourseDetails.Contains(a.Course_Detail_Id.Value) && a.Course_Detail.IsDeleted != true && a.Course_Detail.Course.IsDeleted != true && (string.IsNullOrEmpty(strCode) || a.Trainee.str_Staff_Id.Contains(strCode)) && a.Trainee.int_Role == (int)UtilConstants.ROLE.Instructor && a.Trainee.IsDeleted != true && (string.IsNullOrEmpty(strName) || a.Trainee.FirstName.Contains(strName) || a.Trainee.LastName.Contains(strName) || (a.Trainee.LastName + " " + a.Trainee.FirstName).Contains(strName))
                ).OrderByDescending(a => a.Course_Detail.dtm_time_from);

            var result = from c in detail_i.ToArray()
                         let payment = c?.Course_Detail?.Payments?.FirstOrDefault(a => a.Instructor_Id == c?.Instructor_Id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null)
                         let str_Fullname = ReturnDisplayLanguage(c?.Trainee.FirstName, c?.Trainee.LastName)
                         let learningType = TypeLearningName((int)c?.Course_Detail.type_leaning)
                         select new
                         {
                             string.Empty,
                             str_Fullname,
                             c?.Trainee.str_Staff_Id,
                             Coursecode = c?.Course.Code,
                             Course = c?.Course.Name,
                             Subject = c?.Course_Detail?.SubjectDetail?.Name,
                             dtm_Date = (c?.Course_Detail?.dtm_time_from.Value.ToString("dd/MM/yyyy") ?? "") + "-" + (c?.Course_Detail?.dtm_time_to.Value.ToString("dd/MM/yyyy") ?? ""),
                             tm_Time = c?.Course_Detail?.time_from?.ToString().Substring(0, 2) + ":" + c?.Course_Detail?.time_from?.ToString().Substring(2, 2) + " - "
                                      + c?.Course_Detail?.time_to?.Substring(0, 2) + ":" + c?.Course_Detail?.time_to?.Substring(2, 2),
                             c?.Duration,
                             c?.Allowance,
                             Customer = c?.Course.CustomerType,
                             learningType,
                             PaymentStatus = payment?.Payment_Status?.Name, //PaymentStatusName
                             MethodId = payment?.Method_Id, //PaymentStatus
                             Remarks = string.Empty,
                             Date = payment?.CreationDate,
                             Payees = c?.Trainee.bit_Internal,
                         };

            OfficeOpenXml.ExcelPackage xlPackage;
            System.IO.MemoryStream MS = new System.IO.MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new OfficeOpenXml.ExcelPackage(template, false))
            {
                if (result.Any())
                {
                    foreach (OfficeOpenXml.ExcelWorksheet aworksheet in xlPackage.Workbook.Worksheets)
                    {
                        OfficeOpenXml.ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                        int startrow = 5;
                        int rowheader = 0;
                        int rowDetail = 0;
                        int count = 1;
                        double sum = 0, sumDuration = 0, sumRateAllowance = 0;
                        int col = 1;
                        foreach (var item1 in result.OrderBy(a => a.str_Fullname))
                        {
                            col = 1;
                            var allowance = item1.Allowance.HasValue ? (double)item1.Allowance.Value : 0;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, col].Value = count;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.str_Fullname;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.str_Staff_Id;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Payees == true ? "Internal" : "External";
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.Coursecode;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.Course;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.Customer == true ? "Internal" : "External";//Customer
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.Subject;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.learningType;//Type (cR, cRo, eL)
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.dtm_Date;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.tm_Time;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.Duration;

                            var allowanceCell = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            allowanceCell.Value = allowance.ToString("#,###");
                            allowanceCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            var sumAllowance = (allowance) * (item1.Duration ?? 0);
                            var sumallowance = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            sumallowance.Value = sumAllowance.ToString("#,###");
                            sumallowance.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = (item1.MethodId != null && item1.MethodId != (int)UtilConstants.PaymentStatus.NoPayment) ? item1.PaymentStatus : "No Payment";//Payment Status (Pending, Paid, No Payment) 
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1.Remarks;//Remarks
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = ((item1.MethodId != null && item1.MethodId != (int)UtilConstants.PaymentStatus.NoPayment) ? DateUtil.DateToString(item1.Date, "dd/MM/yyyy") : "");
                            rowDetail++;
                            sumRateAllowance = sumRateAllowance + allowance;
                            sum += sumAllowance;
                            sumDuration = sumDuration + (item1.Duration ?? 0);

                            count++;
                        }
                        #region Custom row
                        var daterow = worksheet.Cells[4, 1, 4, col];
                        daterow.Merge = true;
                        daterow.Value = dateFromTo;
                        daterow.Style.Font.Bold = true;

                        rowDetail++;
                        var totalRow = worksheet.Cells[startrow + rowheader + rowDetail, 1, startrow + rowheader + rowDetail, 11];
                        totalRow.Merge = true;
                        totalRow.Value = "Total";
                        totalRow.Style.Font.Bold = true;
                        totalRow.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        var sumDurationCell = worksheet.Cells[startrow + rowheader + rowDetail, 12];//11
                        sumDurationCell.Value = sumDuration;
                        sumDurationCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        var sumRateAllowanceCell = worksheet.Cells[startrow + rowheader + rowDetail, 13];//12
                        sumRateAllowanceCell.Value = sumRateAllowance.ToString("#,###");
                        sumRateAllowanceCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        var summaryCell = worksheet.Cells[startrow + rowheader + rowDetail, 14];//13
                        summaryCell.Value = sum.ToString("#,###");
                        summaryCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        #endregion



                        using (OfficeOpenXml.ExcelRange r = worksheet.Cells[1, 1, startrow + rowheader + rowDetail, 17])//15
                        {
                            r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        }

                        Bytes = xlPackage.GetAsByteArray();

                    }
                }

                return Bytes;
            }

        }
        [AllowAnonymous]
        [HttpPost]
        public FileContentResult ExportPaymentApproval(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var strCode = string.IsNullOrEmpty(form["code2"]) ? string.Empty : form["code2"].ToLower().Trim();
            var strName = string.IsNullOrEmpty(form["fullname2"]) ? string.Empty : form["fullname2"].ToLower().Trim();
            var strFromdate = string.IsNullOrEmpty(form["from2"]) ? string.Empty : form["from2"].Trim();
            var strTodate = string.IsNullOrEmpty(form["to2"]) ? string.Empty : form["to2"].Trim();
            DateTime dateFrom;
            DateTime dateTo;
            DateTime.TryParse(strFromdate, out dateFrom);
            DateTime.TryParse(strTodate, out dateTo);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
            var listCourseID = new List<int>();
            var lst = string.IsNullOrEmpty(form["int_khoidaotao2"]) ? new List<string>() : form["int_khoidaotao2"].Trim().Split(new char[] { ',' }).ToList();
            listCourseID = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();


            byte[] filecontent = ExportExcelPaymentApproval(strCode, strName, dateFrom, dateTo, listCourseID);
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "TrainingAllowance.xlsx");
            }
            return null;
        }

        private byte[] ExportExcelPaymentApproval(string strCode, string strName, DateTime? dtmFromdate, DateTime? dtmTodate, List<int> listCourseID)
        {
            string templateFilePath = Server.MapPath(
@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/TrainingAllow.xlsx");
            System.IO.FileInfo template = new System.IO.FileInfo(templateFilePath);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var data = _repoTrainee_TrainingCenter.GetAll(a => listCourseID.Count() == 0 || listCourseID.Contains((int)a.khoidaotao_id)).Select(b => b.instructor_id);
            var dateFromTo = string.Format("\t Date: from {0} to {1}", dtmFromdate?.ToString("dd/MM/yyyy"), dtmTodate?.ToString("dd/MM/yyyy"));
            var getid = _repoTMS_APPROVES_HISTORY.GetAll(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_courseDetails_Id.HasValue).OrderByDescending(a => a.id).GroupBy(a => a.int_courseDetails_Id).Select(c => c.OrderByDescending(x => x.id).FirstOrDefault()).Select(a => a.id);
            var listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(a => getid.Contains(a.id)
                                                                       &&
                                                                       ((dtmFromdate == DateTime.MinValue || SqlFunctions.DateDiff("day", dtmFromdate, a.dtm_requested_date) >= 0) &&
                                                                        (dtmTodate == DateTime.MinValue || SqlFunctions.DateDiff("day", a.dtm_requested_date, dtmTodate) >= 0))
           ).Select(a => a.int_courseDetails_Id);
            IEnumerable<Course_Detail_Instructor> detail_i = CourseDetailService.GetDetailInstructors(a => a.Course_Detail.IsDeleted != true && data.Contains(a.Instructor_Id) && listCourseDetails.Contains(a.Course_Detail_Id) && a.Course_Detail.Course.IsDeleted != true && a.Type == (int)UtilConstants.TypeInstructor.Instructor &&
                (string.IsNullOrEmpty(strCode) || a.Trainee.str_Staff_Id.Contains(strCode)) && a.Trainee.IsDeleted != true &&
                 (string.IsNullOrEmpty(strName) || (a.Trainee.FirstName.Contains(strName) || a.Trainee.LastName.Contains(strName) || (a.Trainee.LastName + " " + a.Trainee.FirstName).Contains(strName))));

            var result = from c in detail_i.ToArray()
                         let payment = c?.Course_Detail?.Payments?.FirstOrDefault(a => a.Instructor_Id == c?.Instructor_Id && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null)
                         let str_Fullname = ReturnDisplayLanguage(c?.Trainee.FirstName, c?.Trainee.LastName)
                         let learningType = TypeLearningName((int)c?.Course_Detail.type_leaning)
                         select new
                         {
                             string.Empty,
                             str_Fullname,
                             c?.Trainee.str_Staff_Id,
                             Coursecode = c?.Course.Code,
                             Course = c?.Course.Name,
                             Subject = c?.Course_Detail?.SubjectDetail?.Name,
                             dtm_Date = (c?.Course_Detail?.dtm_time_from.Value.ToString("dd/MM/yyyy") ?? "") + "-" + (c?.Course_Detail?.dtm_time_to.Value.ToString("dd/MM/yyyy") ?? ""),
                             tm_Time = c?.Course_Detail?.time_from?.ToString() + " - "
                                      + c?.Course_Detail?.time_to?.ToString(),
                             c?.Duration,
                             c?.Allowance,
                             Customer = c?.Course.CustomerType,
                             learningType,
                             PaymentStatus = payment?.Payment_Status?.Name, //PaymentStatusName
                             MethodId = payment?.Method_Id, //PaymentStatus
                             Remarks = string.Empty,
                             Date = payment?.CreationDate,
                             Payees = c?.Trainee.bit_Internal,
                             Numberoftrainee = c?.Course.Course_Result_Final.Count(x => x.courseid == c.Course_Id && x.IsDeleted != true) ?? 0
                         };
            OfficeOpenXml.ExcelPackage xlPackage;
            System.IO.MemoryStream MS = new System.IO.MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new OfficeOpenXml.ExcelPackage(template, false))
            {
                OfficeOpenXml.ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                string[] header = GetByKey("Report_TrainingTime").Split(new char[] { ',' });
                worksheet.Cells[1, 10].Value = header[0] + "\r\n" + header[1] + "\r\n" + header[2];
                worksheet.Cells[1, 10].Style.Font.Size = 11;
                if (result.Any())
                {
                    foreach (OfficeOpenXml.ExcelWorksheet aworksheet in xlPackage.Workbook.Worksheets)
                    {

                        int startrow = 5;
                        int rowheader = 0;
                        int rowDetail = 0;
                        int count = 1;
                        double sum = 0, sumDuration = 0, sumRateAllowance = 0;
                        int col = 1;
                        var result_ = result.OrderBy(a => a.str_Fullname).ToList();
                        foreach (var item1 in result_)
                        {
                            col = 1;
                            var allowance = item1.Allowance.HasValue ? (double)item1.Allowance : 0;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, col].Value = count;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.str_Fullname;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.str_Staff_Id;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Payees == true ? "Internal" : "External";
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Coursecode;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Course;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Customer == true ? "Internal" : "External";//Customer
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Subject;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.learningType;//Type (cR, cRo, eL)
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.dtm_Date;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.tm_Time;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Numberoftrainee;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Duration;
                            var allowanceCell = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            allowanceCell.Value = allowance.ToString("#,###");
                            allowanceCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                            var sumAllowance = (allowance) * (item1.Duration ?? 0);
                            var sumallowance = worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col];
                            sumallowance.Value = sumAllowance.ToString("#,###");
                            sumallowance.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = (item1?.MethodId != null && item1.MethodId != (int)UtilConstants.PaymentStatus.NoPayment) ? item1.PaymentStatus : "No Payment";//Payment Status (Pending, Paid, No Payment) 
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = item1?.Remarks;//Remarks
                            worksheet.Cells[rowheader + startrow + rowDetail + 1, ++col].Value = ((item1?.MethodId != null && item1?.MethodId != (int)UtilConstants.PaymentStatus.NoPayment) ? DateUtil.DateToString(item1?.Date, "dd/MM/yyyy") : "");



                            rowDetail++;
                            sumRateAllowance = sumRateAllowance + allowance;
                            sum += sumAllowance;
                            sumDuration = sumDuration + (item1.Duration ?? 0);
                            count++;
                        }
                        #region Custom row
                        var daterow = worksheet.Cells[4, 1, 4, col];
                        daterow.Merge = true;
                        daterow.Value = dateFromTo;
                        daterow.Style.Font.Bold = true;

                        rowDetail++;
                        var totalRow = worksheet.Cells[startrow + rowheader + rowDetail, 1, startrow + rowheader + rowDetail, 12];
                        totalRow.Merge = true;
                        totalRow.Value = "Total";
                        totalRow.Style.Font.Bold = true;
                        totalRow.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        var sumDurationCell = worksheet.Cells[startrow + rowheader + rowDetail, 13];
                        sumDurationCell.Value = sumDuration;
                        sumDurationCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        sumDurationCell.Style.Font.Bold = true;
                        var sumRateAllowanceCell = worksheet.Cells[startrow + rowheader + rowDetail, 14];
                        sumRateAllowanceCell.Value = sumRateAllowance.ToString("#,###");
                        sumRateAllowanceCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        sumRateAllowanceCell.Style.Font.Bold = true;
                        var summaryCell = worksheet.Cells[startrow + rowheader + rowDetail, 15];
                        summaryCell.Value = sum.ToString("#,###");
                        summaryCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                        summaryCell.Style.Font.Bold = true;
                        #endregion



                        using (OfficeOpenXml.ExcelRange r = worksheet.Cells[1, 1, startrow + rowheader + rowDetail, 18])
                        {
                            r.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            r.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            r.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                            r.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                            r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        }

                        Bytes = xlPackage.GetAsByteArray();

                    }
                }

                return Bytes;
            }

        }
        [AllowAnonymous]
        public ActionResult Payment(int? id, int? type, string from, string to)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var model = new TrainingPayment();
            var fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var toDate = (fromDate.AddMonths(1).AddDays(-1));
            model.DateFrom = !string.IsNullOrEmpty(from) ? from : fromDate.ToString("dd/MM/yyyy");
            model.DateTo = !string.IsNullOrEmpty(to) ? to : toDate.ToString("dd/MM/yyyy");
            model.DepartmentIds = null;
            var instructor = EmployeeService.GetById(id);
            if (instructor != null)
            {
                model.Id = instructor.Id;
                model.DepartmentIds = instructor.Trainee_TrainingCenter.Any()
                    ? instructor.Trainee_TrainingCenter.Select(a => a.khoidaotao_id).ToList()
                    : null;
            }
            model.Type = type;
            model.Departments = loaddepartment(null, 1, model.DepartmentIds);
            model.RequestOrApproval = new Dictionary<int, string>
            {
                { (int)UtilConstants.RequestOrApproval.Request,UtilConstants.RequestOrApproval.Request.ToString()},
                { (int)UtilConstants.RequestOrApproval.Approval,UtilConstants.RequestOrApproval.Approval.ToString()}
            };
            return View(model);
        }
        private string loaddepartment(int? parentid, int level, List<int?> department_id)
        {
            string result = string.Empty;
            bool parent = false;
            var data = _repoDepartment.GetAll(a => a.IsDeleted == false);
            if (parentid == null)
            {
                data = data.Where(a => a.IsDeleted == false && a.ParentId == null);
                parent = true;
            }
            else
            {
                data = data.Where(a => a.IsDeleted == false && a.ParentId == parentid);
            }

            if (data.Count() == 0)
                return result;
            else
            {
                foreach (var item in data)
                {
                    string selected = "";
                    if (department_id != null && department_id.Count > 0 && department_id.Contains(item.Id))
                    {
                        selected = "selected";
                    }
                    string khoangtrang = "";
                    for (int i = 0; i < level; i++)
                    {
                        khoangtrang += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                    }

                    result += "<option value='" + item.Id + "' style='font-size:" + (18 - (level + 2)) + "px;' " + selected + " >" + khoangtrang + "+ " + item.Name;
                    result += "</option>";
                    result += loaddepartment(item.Id, level + 1, department_id);
                }
            }
            return result;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Payment(FormCollection form)
        {
            #region Get value form
            var type = string.IsNullOrEmpty(form["typeRequestOrApproval"]) ? -1 : Convert.ToInt32(form["typeRequestOrApproval"]);
            var instructorId = string.IsNullOrEmpty(form["InstructorId"]) ? -1 : Convert.ToInt32(form["InstructorId"]);
            var courseDetailIds = form.GetValues("course-detail-id");
            var paymentId = form.GetValues("payment-id");
            var strFromdate = string.IsNullOrEmpty(form["DateFrom"]) ? string.Empty : form["DateFrom"].Trim();
            var strTodate = string.IsNullOrEmpty(form["DateTo"]) ? string.Empty : form["DateTo"].Trim();
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            DateTime dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
            DateTime dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
            #endregion

            try
            {
                if (courseDetailIds == null)
                    return Json(new
                    {
                        result = false,
                        message = CMSUtils.alert("danger", Messege.NO_DATA),
                    }, JsonRequestBehavior.AllowGet);

                var user = GetUser();
                var now = DateTime.Now;
                for (var i = 0; i < courseDetailIds.Length; i++)
                {
                    var courseDetail = CourseDetailService.GetById(int.Parse(courseDetailIds[i]));
                    if (courseDetail != null)
                    {
                        #region debug get value id

                        //var valueId2 = string.IsNullOrEmpty(form.GetValue("cb-" + courseDetail.Course_Detail_Id + "[]").AttemptedValue) ? -1 : Convert.ToInt32(form.GetValue("cb-" + courseDetail.Course_Detail_Id + "[]").AttemptedValue);
                        //return Json(new
                        //{
                        //    result = false,
                        //    message = CMSUtils.alert("danger", valueId2.ToString()),
                        //}, JsonRequestBehavior.AllowGet);

                        #endregion


                        var payment = courseDetail.Payments.FirstOrDefault(a =>
                            (string.IsNullOrEmpty(paymentId[i]) || a.Id == int.Parse(paymentId[i])) &&
                            a.Instructor_Id == instructorId && a.Is_Cancel == null && a.Is_Deleted == false &&
                            (
                                type == (int)UtilConstants.RequestOrApproval.Request
                                ? a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null
                                : a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null
                             )
                            );
                        var valueId = string.IsNullOrEmpty(form.GetValue("cb-" + courseDetail.Id + "[]").AttemptedValue) ? -1 : Convert.ToInt32(form.GetValue("cb-" + courseDetail.Id + "[]").AttemptedValue);

                        if (payment != null)
                        {
                            payment.Is_Cancel = true;
                            payment.Is_Deleted = true;
                            payment.Updated_By = (int?)user.USER_ID;
                            payment.Updated_Date = now;
                            ConfigService.UpdatePayments(payment);
                        }


                        if (valueId != -1 /*&& valueId != (int)Constants.PaymentStatus.NoPayment*/)
                        {
                            var duration = courseDetail.Course_Detail_Instructor.Where(a => a.Instructor_Id == instructorId).Any(b => b.Duration.HasValue)
                                ? (decimal?)courseDetail.Course_Detail_Instructor?.Where(a => a.Instructor_Id == instructorId).Sum(b => b.Duration)
                                : 0;
                            var trainingAllow = courseDetail.Course_Detail_Instructor.Any(a => a.Instructor_Id == instructorId)
                                ? courseDetail.Course_Detail_Instructor?.FirstOrDefault(a => a.Instructor_Id == instructorId)?.Allowance
                                : 0;
                            payment = new Payment()
                            {
                                Instructor_Id = instructorId,
                                CourseDetail_Id = courseDetail.Id,
                                Duration = (double?)duration,
                                Allowance = trainingAllow,
                                Created_By = (int?)user.USER_ID,
                                CreationDate = now,
                                Is_Deleted = false,


                            };

                            var paymentStatus = ConfigService.GetPaymentStatusById(valueId);
                            payment.Method_Id = paymentStatus?.Id;
                            var amount = (decimal?)(duration * trainingAllow);
                            payment.Amount = amount;
                            if (type == (int)UtilConstants.RequestOrApproval.Request)
                            {
                                if (dateFrom != DateTime.MinValue)
                                {
                                    payment.Pay_DateFrom_DateRequest = dateFrom;
                                    payment.DateFrom = dateFrom;
                                }

                                if (dateTo != DateTime.MinValue)
                                {
                                    payment.Pay_DateTo_Date_Request = dateTo;

                                    payment.DateTo = dateTo; //ngay search
                                }

                            }
                            else
                            {

                                if (dateFrom != DateTime.MinValue)
                                {
                                    payment.Pay_DateFrom_DateApproval = dateFrom;
                                    payment.DateFrom = dateFrom;
                                }
                                if (dateTo != DateTime.MinValue)
                                {
                                    payment.Pay_DateTo_DateApproval = dateTo;
                                    payment.DateTo = dateTo; //ngay search
                                }


                            }
                            ConfigService.InsertPayments(payment);
                        }
                    }

                }

                return Json(new
                {
                    result = true,
                    message = CMSUtils.alert("success", Messege.SUCCESS),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = CMSUtils.alert("danger", Messege.FAIL),
                    result = false,
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult FilterInstructor(FormCollection form)
        {
            var html = new StringBuilder();
            var strFromdate = string.IsNullOrEmpty(form["DateFrom"]) ? string.Empty : form["DateFrom"].ToLower().Trim();
            var strTodate = string.IsNullOrEmpty(form["DateTo"]) ? string.Empty : form["DateTo"].ToLower().Trim();
            var instructorId = string.IsNullOrEmpty(form["Id"]) ? -1 : Int32.Parse(form["Id"]);
            var dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
            var dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

            var listCourseId = new List<int>();
            var lst = string.IsNullOrEmpty(form["int_khoidaotao[]"]) ? new List<string>() : form["int_khoidaotao[]"].Trim().Split(',').ToList();
            listCourseId = lst.Count > 0 ? lst.Where(a => a != "").Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
            var listInstructorId = EmployeeService.GetTraineeCenter(a => (!listCourseId.Any() || listCourseId.Contains((int)a.khoidaotao_id))).Select(a => a.instructor_id);
            var listCourseDetailId = CourseDetailService.Get(a => a.IsDeleted == false && a.TMS_APPROVES.Any(t => t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve &&
              ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, t.dtm_requested_date) >= 0) &&
                  (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", t.dtm_requested_date, dateTo) >= 0)))).Select(a => a.Id);
            var coursedetailsInstructor = CourseDetailService.GetDetailInstructors(a => listInstructorId.Contains(a.Instructor_Id) && listCourseDetailId.Contains((int)a.Course_Detail_Id)).GroupBy(a => a.Instructor_Id).Select(t => t.Key);

            var data = EmployeeService.Get(a => coursedetailsInstructor.Contains(a.Id) && a.int_Role == (int)UtilConstants.ROLE.Instructor &&
                a.IsDeleted == false, true /*&&
                (string.IsNullOrEmpty(strCode) || a.str_Staff_Id.Contains(strCode)) &&
                (string.IsNullOrEmpty(strName) || a.str_Fullname.Contains(strName))*/).OrderBy(b => b.FirstName);
            html.Append("<option></option>");
            if (!data.Any())
            {
                return Json(new
                {
                    result = false,
                    value = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
            foreach (var item in data)
            {
                if (item.Id == instructorId)
                {
                    html.AppendFormat("<option value='{0}' selected >{1} - {2}</option>", item.Id, item.str_Staff_Id, ReturnDisplayLanguage(item.FirstName, item.LastName));
                }
                else
                {
                    html.AppendFormat("<option value='{0}' >{1} - {2}</option>", item.Id, item.str_Staff_Id, ReturnDisplayLanguage(item.FirstName, item.LastName));
                }
            }
            return Json(new
            {
                result = true,
                value = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetAmount(FormCollection form)
        {

            var strFromdate = string.IsNullOrEmpty(form["DateFrom"]) ? string.Empty : form["DateFrom"].ToLower();
            var strTodate = string.IsNullOrEmpty(form["DateTo"]) ? string.Empty : form["DateTo"].ToLower();
            var id = string.IsNullOrEmpty(form["InstructorId"]) ? -1 : Convert.ToInt32(form["InstructorId"]);
            var type = string.IsNullOrEmpty(form["typeRequestOrApproval"]) ? -1 : Convert.ToInt32(form["typeRequestOrApproval"]);
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            DateTime dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
            DateTime dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);
            dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
            dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

            var listCourseDetails = new List<int?>();

            if (type == (int)UtilConstants.RequestOrApproval.Request)
            {
                //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                listCourseDetails = CourseDetailService.Get(a => (id == -1 || a.Course_Detail_Instructor.Any(b => b.Instructor_Id == id)) && a.IsDeleted == false && a.TMS_APPROVES.Any(t => t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve &&
                                                                                                                                                                                                                ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, t.dtm_requested_date) >= 0) &&
                                                                                                                                                                                                                 (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", t.dtm_requested_date, dateTo) >= 0))
                                                               )).AsEnumerable().Select(a => a?.Id).ToList();
            }
            else
            {
                var getid = _repoTMS_APPROVES_HISTORY.GetAll(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve
                ).OrderByDescending(a => a.id).GroupBy(a => a.int_courseDetails_Id).Select(c => c.OrderByDescending(x => x.id).FirstOrDefault()).Select(a => a.id);

                listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(a => getid.Contains(a.id)
                                                                       &&
                                                                       ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.dtm_requested_date) >= 0) &&
                                                                        (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.dtm_requested_date, dateTo) >= 0))
                ).Select(a => a.int_courseDetails_Id).ToList();
            }
            var model = EmployeeService.GetById(id);
            if (model == null)
            {
                return Json(new
                {
                    totalDuration = "",
                    totalMoney = "",
                    paid = "",
                    unpaid = "",
                    result = false
                },
         JsonRequestBehavior.AllowGet);
            }
            var totalDuration = model.Course_Detail_Instructor?.Where(a => listCourseDetails.Contains(a.Course_Detail_Id) && a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Course_Detail_Id.HasValue && a.Course_Detail.IsDeleted == false)
                    .Sum(a => a.Duration) ?? 0;
            var totalMoney =
                model.Course_Detail_Instructor.Where(a => listCourseDetails.Contains(a.Course_Detail_Id) && a.Type == (int)UtilConstants.TypeInstructor.Instructor && a.Course_Detail_Id.HasValue && a.Course_Detail.IsDeleted == false)
                    .GroupBy(a => new { a.Course_Detail_Id })
                    .Select(g => new
                    {
                        Allowance =
                            g.FirstOrDefault()?.Allowance != null
                                ? (decimal.Parse(g.Sum(a => a.Duration).ToString()) * g.FirstOrDefault()?.Allowance)
                                : 0
                    }).Sum(c => c.Allowance);
            var paid = type == (int)UtilConstants.RequestOrApproval.Approval
                ? model.Payments?.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Paid && a.Is_Deleted == false && a.Is_Cancel == null && dateFrom <= a.Pay_DateFrom_DateApproval && a.Pay_DateTo_DateApproval <= dateTo).GroupBy(a => a.CourseDetail_Id).Select(b => b.FirstOrDefault()).Sum(a => a.Amount)
                : model.Payments?.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Paid && a.Is_Deleted == false && a.Is_Cancel == null && dateFrom <= a.Pay_DateFrom_DateRequest && a.Pay_DateTo_Date_Request <= dateTo).GroupBy(a => a.CourseDetail_Id).Select(b => b.FirstOrDefault()).Sum(a => a.Amount);

            //var paid = type == (int)Constants.RequestOrApproval.Approval ? model.Payments?.Where(a => a.Is_Deleted == false && a.Is_Cancel == null && dateFrom <= a.Pay_DateTo_DateApproval && a.Pay_DateFrom_DateApproval <= dateTo).Sum(a => a.Amount) : model.Payments?.Where(a => a.Is_Deleted == false && a.Is_Cancel == null && dateFrom <= a.Pay_DateTo_Date_Request && a.Pay_DateFrom_DateRequest <= dateTo).Sum(a => a.Amount);

            var pending = type == (int)UtilConstants.RequestOrApproval.Approval
                ? model.Payments?.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Pending && a.Is_Deleted == false && a.Is_Cancel == null && dateFrom <= a.Pay_DateFrom_DateApproval && a.Pay_DateTo_DateApproval <= dateTo).Sum(a => a.Amount)
                : model.Payments?.Where(a => a.Method_Id == (int)UtilConstants.PaymentStatus.Pending && a.Is_Deleted == false && a.Is_Cancel == null && dateFrom <= a.Pay_DateFrom_DateRequest && a.Pay_DateTo_Date_Request <= dateTo).Sum(a => a.Amount);
            var unpaid = totalMoney - paid - pending;
            return Json(new
            {
                totalDuration = totalDuration > 0 ? totalDuration.ToString() : "",
                totalMoney = totalMoney?.ToString("#,###"),
                paid = paid?.ToString("#,###"),
                unpaid = unpaid?.ToString("#,###"),
                pending = pending?.ToString("#,###"),
                result = true
            },
         JsonRequestBehavior.AllowGet);

        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerSubjectPayment(jQueryDataTableParamModel param)
        {
            try
            {
                var strFromdate = string.IsNullOrEmpty(Request.QueryString["DateFrom"]) ? string.Empty : Request.QueryString["DateFrom"].ToLower();
                var strTodate = string.IsNullOrEmpty(Request.QueryString["DateTo"]) ? string.Empty : Request.QueryString["DateTo"].ToLower();
                var instructorId = string.IsNullOrEmpty(Request.QueryString["InstructorId"])
                    ? -1
                    : Convert.ToInt32(Request.QueryString["InstructorId"]);
                var type = string.IsNullOrEmpty(Request.QueryString["typeRequestOrApproval"])
                    ? -1
                    : Convert.ToInt32(Request.QueryString["typeRequestOrApproval"]);
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                DateTime dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                DateTime dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);


                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                IEnumerable<TrainingTimeModel> data;
                if (type == (int)UtilConstants.RequestOrApproval.Approval)
                {
                    var historyId_ = _repoTMS_APPROVES_HISTORY.GetAll(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve && a.int_courseDetails_Id.HasValue).OrderByDescending(a => a.id).GroupBy(a => a.int_courseDetails_Id).Select(c => c.OrderByDescending(x => x.id).FirstOrDefault()).Select(a => a.id);
                    var historyId = _repoTMS_APPROVES_HISTORY.GetAll(a => historyId_.Contains(a.id) &&
                ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.dtm_requested_date) >= 0) &&
                 (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.dtm_requested_date, dateTo) >= 0)));
                    var listCourseDetails = historyId.Select(a => a.int_courseDetails_Id);
                    var listCourseDetails2 = historyId;

                    //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                    data = _repoCourseDetailInstructor.GetAll(a => a.Instructor_Id == instructorId && a.Course_Detail.IsDeleted != true && a.Course_Detail.Course.IsDeleted != true && listCourseDetails.Contains(a.Course_Detail_Id) && a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(t => new TrainingTimeModel
                    {
                        CourseDetailId = t.Course_Detail.Id,
                        strTotal = t.Course_Detail.Id.ToString(),
                        strName = t.Course_Detail.SubjectDetail.Name,
                        CourseName = t.Course_Detail.Course.Name,
                        strCode = t.Course_Detail.Course.Code,
                        duration = t.Duration ?? 0,
                        dateFrom = t.Course_Detail.dtm_time_from,
                        dateTo = t.Course_Detail.dtm_time_to,
                        //date = DateUtil.DateToString(t.Course_Detail.dtm_time_from, "dd/MM/yyyy") + " - " + DateUtil.DateToString(t.Course_Detail.dtm_time_to, "dd/MM/yyyy"),
                        time = (t.Course_Detail.time_from != null ? t.Course_Detail.time_from.ToString().Substring(0, 2) + ":" + t.Course_Detail.time_from.ToString().Substring(2) : null) + " - " + (t.Course_Detail.time_to != null ? t.Course_Detail.time_to.ToString().Substring(0, 2) + ":" + t.Course_Detail.time_to.ToString().Substring(2) : null),
                        strTrainingAlow = t.Allowance.HasValue ? t.Allowance.Value : 0,
                        dateRequest =
                           t.Course_Detail.TMS_APPROVES.FirstOrDefault().dtm_requested_date,
                        PaymentStatus = t.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == instructorId && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null).Method_Id,
                        PaymentId = t.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == instructorId && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateApproval != null && a.Pay_DateTo_DateApproval != null).Id,
                    });
                }
                else
                {
                    //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                    var data_ = _repoCourseDetailInstructor.GetAll(a => a.Instructor_Id == instructorId && a.Course_Detail.IsDeleted == false && a.Course_Detail.TMS_APPROVES.Any(t =>
                 ((dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, t.dtm_requested_date) >= 0) &&
                     (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", t.dtm_requested_date, dateTo) >= 0))
                 && t.int_Type == (int)UtilConstants.ApproveType.SubjectResult && t.int_id_status == (int)UtilConstants.EStatus.Approve));
                    data = data_.Select(t => new TrainingTimeModel
                    {
                        CourseDetailId = t.Course_Detail.Id,
                        strTotal = t.Course_Detail.Id.ToString(),
                        strName = t.Course_Detail.SubjectDetail.Name,
                        CourseName = t.Course_Detail.Course.Name,
                        strCode = t.Course_Detail.Course.Code,
                        duration = t.Duration ?? 0,
                        dateFrom = t.Course_Detail.dtm_time_from,
                        dateTo = t.Course_Detail.dtm_time_to,
                        //date = DateUtil.DateToString(t.Course_Detail.dtm_time_from, "dd/MM/yyyy") + " - " + DateUtil.DateToString(t.Course_Detail.dtm_time_to, "dd/MM/yyyy"),
                        time = (t.Course_Detail.time_from != null ? t.Course_Detail.time_from.ToString().Substring(0, 2) + ":" + t.Course_Detail.time_from.ToString().Substring(2) : null) + " - " + (t.Course_Detail.time_to != null ? t.Course_Detail.time_to.ToString().Substring(0, 2) + ":" + t.Course_Detail.time_to.ToString().Substring(2) : null),
                        strTrainingAlow = t.Allowance.HasValue ? t.Allowance.Value : 0,
                        dateRequest = t.Course_Detail.TMS_APPROVES.FirstOrDefault().dtm_requested_date,
                        //PaymentStatus = loadPaymentStatus(paymentStatus, t.Key?.Payments?.FirstOrDefault(b => b.Is_Cancel == null && b.Pay_DateFrom_DateRequest != null && b.Pay_DateTo_Date_Request != null)?.Method_Id)
                        PaymentStatus = t.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == instructorId && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null).Method_Id ?? 0,
                        PaymentId = t.Course_Detail.Payments.FirstOrDefault(a => a.Instructor_Id == instructorId && a.Is_Cancel == null && a.Is_Deleted == false && a.Pay_DateFrom_DateRequest != null && a.Pay_DateTo_Date_Request != null).Id,
                    });

                }



                IEnumerable<TrainingTimeModel> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TrainingTimeModel, object> orderingFunction = (c => sortColumnIndex == 1 ? c.strCode
                                                                        : sortColumnIndex == 2 ? c.CourseName
                                                                        : sortColumnIndex == 3 ? c.strName
                                                                        : sortColumnIndex == 4 ? c.dateFrom
                                                                        : sortColumnIndex == 5 ? c.time
                                                                        : sortColumnIndex == 6 ? (object)c.duration
                                                                        : c.strCode);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                                 //let amount = (c?.Duration * c?.Amount)?.ToString("#,###")
                             let allowance = c.strTrainingAlow.ToString("#,###")
                             let date_string = DateUtil.DateToString(c.dateFrom, "dd/MM/yyyy") + " - " + DateUtil.DateToString(c.dateTo, "dd/MM/yyyy")
                             let dateRequest_string = DateUtil.DateToString(c.dateRequest, "dd/MM/yyyy")
                             select new object[] {
                                string.Empty ,
                                 c?.strCode + "<input type='hidden' id='course-detail-id' name='course-detail-id' value='"+c?.CourseDetailId+"' />",
                                 c?.CourseName + "<input type='hidden' id='payment-id' name='payment-id' value='"+c?.PaymentId+"' />",
                                 c?.strName ,
                                 date_string,
                                 c?.time,
                                 c?.duration,
                                 dateRequest_string,
                                 allowance,
                                 //checkbox .PaymentStatus.Paid
                                 "<input id='select-all-paid' class='select-all-paid' onchange='cbChecked(this)' type='checkbox' name='cb-" + c?.CourseDetailId+"[]' value='"+(int)UtilConstants.PaymentStatus.Paid+"' "
                                 +(c?.PaymentStatus ==(int)UtilConstants.PaymentStatus.Paid ? "checked" : "" ) +" />",
                                 //checkbox .PaymentStatus.NoPayment
                                 "<input id='select-all-no-payment' class='select-all-paid' onchange='cbChecked(this)' type='checkbox' name='cb-"+c?.CourseDetailId+"[]' value='"+(int)UtilConstants.PaymentStatus.NoPayment+"' "
                                 +((c?.PaymentStatus == (int)UtilConstants.PaymentStatus.NoPayment || c?.PaymentId == null) ? "checked" : "" )
                                 +"/>",
                                 //checkbox .PaymentStatus.Pending
                                 "<input  id='select-all-pending' class='select-all-paid' onchange='cbChecked(this)' type='checkbox' name='cb-"+c?.CourseDetailId+"[]' value='"+(int)UtilConstants.PaymentStatus.Pending+"' "
                                 +(c?.PaymentStatus ==(int)UtilConstants.PaymentStatus.Pending ? "checked" : "" ) +"/>",

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
        [HttpPost]
        public ActionResult PaymentAllRequest(FormCollection form)
        {
            try
            {
                var strFromdate = string.IsNullOrEmpty(form["from"]) ? string.Empty : form["from"].ToLower();
                var strTodate = string.IsNullOrEmpty(form["to"]) ? string.Empty : form["to"].ToLower();
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                var dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);

                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;


                var fInstructors = form["InstructorId"] != null ? form.GetValues("InstructorId") : null;
                var typeRequestOrApproval = string.IsNullOrEmpty(form["RequestOrApproval"]) ? -1 : Convert.ToInt32(form["RequestOrApproval"]);

                var user = GetUser();
                if (fInstructors == null)
                    return Json(new
                    {
                        result = false,
                        message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + Messege.NO_DATA),
                    }, JsonRequestBehavior.AllowGet);
                foreach (var fInstructor in fInstructors)
                {
                    var sInstructor = fInstructor.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    var instructor = EmployeeService.GetById(int.Parse(sInstructor[0]));
                    //var paid = sInstructor[1]; // số tiền đã thanh toán
                    var unpaid = sInstructor[1] != null ? Convert.ToDecimal(sInstructor[1]) : 0; // tổng số tiền chưa search ngày bắt đầu -> ngày kết thúc
                    if (unpaid > 0) // so sánh if khác 0 thì vào hàm
                    {
                        #region [biến tạm]

                        var dateStar = DateTime.MinValue;
                        var dateEnd = DateTime.MinValue;
                        #endregion

                        var payments = instructor.Payments.Where(a => a.Is_Cancel == null && dateFrom <= a.Pay_DateTo_Date_Request && a.Pay_DateFrom_DateRequest <= dateTo).OrderBy(a => a.Pay_DateFrom_DateRequest)
                               /*.Where(a => dateFrom <= a.Pay_DateTo && a.Pay_DateFrom <= dateTo)*/;
                        var paymentIds = payments.Select(a => a.CourseDetail_Id);

                        if (payments.Any())
                        {
                            var midPayments = payments.GroupBy(b => b.Pay_DateFrom_DateRequest).Select(g => g.First());
                            if (midPayments.Count() > 1)
                            {
                                #region// lấy record khoảng giữa 
                                //danh sach các khoảng giữa chưa thanh toán
                                var lstUnpaid = new List<NewFromToPayment>();
                                var tempDateFrom = DateTime.MinValue;
                                var tempDateTo = DateTime.MinValue;
                                var count = 0;
                                foreach (var item in midPayments)
                                {

                                    count++;
                                    if (count % 2 == 0)
                                    {
                                        tempDateTo = item.Pay_DateFrom_DateRequest.Value.Date.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);
                                    }
                                    else
                                    {
                                        tempDateFrom = item.Pay_DateTo_Date_Request.Value.AddDays(1);
                                    }
                                    if (/*fromtoPayment.DateFrom.HasValue && fromtoPayment.DateTo.HasValue*/ tempDateFrom != DateTime.MinValue && tempDateTo != DateTime.MinValue)
                                    {
                                        lstUnpaid.Add(new NewFromToPayment()
                                        {
                                            DateFrom = tempDateFrom,
                                            DateTo = tempDateTo
                                        });
                                        tempDateFrom = DateTime.MinValue;
                                        tempDateTo = DateTime.MinValue;
                                    }
                                }
                                #endregion

                                #region //insert các khoảng giữa chưa thanh toán
                                if (lstUnpaid.Any())
                                {
                                    foreach (var item in lstUnpaid)
                                    {

                                        dateStar = item.DateFrom.Value;
                                        dateEnd = item.DateTo.Value;
                                        if (typeRequestOrApproval == (int)UtilConstants.RequestOrApproval.Request)
                                        {
                                            //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                            var courseDetailInstructors = _repoCourseDetailInstructor.GetAll(
                                                a => a.Instructor_Id == instructor.Id &&
                                                     a.Course_Detail.IsDeleted == false &&
                                                     a.Course_Detail.TMS_APPROVES.Any(t =>
                                                         ((dateStar == DateTime.MinValue ||
                                                           SqlFunctions.DateDiff("day", dateStar, t.dtm_requested_date) >=
                                                           0) &&
                                                          (dateEnd == DateTime.MinValue ||
                                                           SqlFunctions.DateDiff("day", t.dtm_requested_date, dateEnd) >=
                                                           0)) &&
                                                         t.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                         t.int_id_status == (int)UtilConstants.EStatus.Approve)).GroupBy(b => b.Course_Detail).Select(c => c.Key);
                                            if (courseDetailInstructors.Any())
                                            {
                                                ModifyPayment(courseDetailInstructors, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                            }
                                        }
                                        else
                                        {
                                            var getid = _repoTMS_APPROVES_HISTORY.GetAll(
                                                a =>
                                                    a.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                    a.int_id_status == (int)UtilConstants.EStatus.Approve
                                                )
                                                .OrderByDescending(a => a.id)
                                                .GroupBy(a => a.int_courseDetails_Id)
                                                .Select(c => c.FirstOrDefault())
                                                .Select(a => a.id);
                                            var listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(
                                                a => getid.Contains(a.id)
                                                     &&
                                                     ((dateStar == DateTime.MinValue ||
                                                       SqlFunctions.DateDiff("day", dateStar, a.dtm_requested_date) >= 0) &&
                                                      (dateEnd == DateTime.MinValue ||
                                                       SqlFunctions.DateDiff("day", a.dtm_requested_date, dateEnd) >= 0))
                                                ).Select(a => a.int_courseDetails_Id);
                                            //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                            var courseDetailInstructors = _repoCourseDetailInstructor.GetAll(
                                                a => a.Instructor_Id == instructor.Id &&
                                                     a.Course_Detail.IsDeleted != true &&
                                                    listCourseDetails.Contains(a.Course_Detail_Id)).GroupBy(b => b.Course_Detail).Select(c => c.Key);

                                            if (courseDetailInstructors.Any())
                                            {
                                                ModifyPayment(courseDetailInstructors, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                            }
                                        }



                                    }
                                }
                                #endregion

                            }

                            else
                            {
                                if (typeRequestOrApproval == (int)UtilConstants.RequestOrApproval.Request)
                                {
                                    #region//TH 1 lấy từ đầu tới ngày bắt đầu
                                    var firstDateFromPayment = payments.FirstOrDefault();
                                    if (firstDateFromPayment != null &&
                                        dateFrom < firstDateFromPayment.Pay_DateFrom_DateRequest &&
                                        dateTo >= firstDateFromPayment.Pay_DateFrom_DateRequest)
                                    {
                                        #region [Nếu ngày dateTo search > dateFrom DB DATE REQUEST]

                                        dateStar = dateFrom;
                                        // ngày kết thúc (lấy ngày bắt đầu db - 1)
                                        dateEnd =
                                            firstDateFromPayment.Pay_DateFrom_DateRequest.Value.Date.AddDays(-1)
                                                .AddHours(23)
                                                .AddMinutes(59)
                                                .AddSeconds(59);
                                        //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                        var courseDetailInstructorsTH1 = _repoCourseDetailInstructor.GetAll(
                                            a => a.Instructor_Id == instructor.Id &&
                                                 !a.Course_Detail.IsDeleted == true &&
                                                 a.Course_Detail.TMS_APPROVES.Any(t =>
                                                     ((dateStar == DateTime.MinValue ||
                                                       SqlFunctions.DateDiff("day", dateStar, t.dtm_requested_date) >=
                                                       0) &&
                                                      (dateEnd == DateTime.MinValue ||
                                                       SqlFunctions.DateDiff("day", t.dtm_requested_date, dateEnd) >=
                                                       0)) &&
                                                     t.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                     t.int_id_status == (int)UtilConstants.EStatus.Approve)).GroupBy(b => b.Course_Detail).Select(c => c.Key);
                                        if (courseDetailInstructorsTH1.Any())
                                        {
                                            ModifyPayment(courseDetailInstructorsTH1, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                        }

                                        #endregion
                                    }
                                    #endregion
                                    #region//TH6 lấy từ ngày kết thúc tới ngày search
                                    var lastDateFromPayment = payments.LastOrDefault();
                                    if (lastDateFromPayment != null &&
                                        lastDateFromPayment.Pay_DateTo_Date_Request < dateTo)
                                    {
                                        #region [ngày bắt đầu form search < ngày kết thúc DB]

                                        // ngày bắt đầu (lấy ngày kết thúc + 1)
                                        dateStar = lastDateFromPayment.Pay_DateTo_Date_Request.Value.AddDays(1);
                                        // ngày kết thúc (lấy ngày đến from search )
                                        dateEnd = dateTo;
                                        //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                        var courseDetailInstructorsTH6 = _repoCourseDetailInstructor.GetAll(
                                            a => a.Instructor_Id == instructor.Id &&
                                                 a.Course_Detail.IsDeleted != true &&
                                                  a.Course_Detail.TMS_APPROVES.Any(t =>
                                                     ((dateStar == DateTime.MinValue ||
                                                       SqlFunctions.DateDiff("day", dateStar, t.dtm_requested_date) >=
                                                       0) &&
                                                      (dateEnd == DateTime.MinValue ||
                                                       SqlFunctions.DateDiff("day", t.dtm_requested_date, dateEnd) >=
                                                       0)) &&
                                                     t.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                     t.int_id_status == (int)UtilConstants.EStatus.Approve)).GroupBy(b => b.Course_Detail).Select(c => c.Key);
                                        if (courseDetailInstructorsTH6.Any())
                                        {
                                            ModifyPayment(courseDetailInstructorsTH6, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                        }

                                        #endregion
                                    }
                                    if (dateStar == DateTime.MinValue && dateEnd == DateTime.MinValue)
                                    {
                                        #region [Insert bình thường]
                                        //ngày bắt dầu  form search
                                        dateStar = dateFrom;
                                        //lấy ngày kết thúc form search
                                        dateEnd = dateTo/*.AddHours(23).AddMinutes(59).AddSeconds(59)*/;
                                        if (typeRequestOrApproval == (int)UtilConstants.RequestOrApproval.Request)
                                        {
                                            //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                            var courseDetailInstructors = _repoCourseDetailInstructor.GetAll(
                                                a => a.Instructor_Id == instructor.Id &&
                                                     a.Course_Detail.IsDeleted != true &&
                                                     a.Course_Detail.TMS_APPROVES.Any(t =>
                                                         ((dateStar == DateTime.MinValue ||
                                                           SqlFunctions.DateDiff("day", dateStar, t.dtm_requested_date) >=
                                                           0) &&
                                                          (dateEnd == DateTime.MinValue ||
                                                           SqlFunctions.DateDiff("day", t.dtm_requested_date, dateEnd) >=
                                                           0)) &&
                                                         t.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                         t.int_id_status == (int)UtilConstants.EStatus.Approve)).GroupBy(b => b.Course_Detail).Select(c => c.Key);
                                            if (courseDetailInstructors.Any())
                                            {
                                                ModifyPayment(courseDetailInstructors, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                            }
                                        }
                                        #endregion
                                    }
                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            #region [Insert lần đầu]
                            dateStar = dateFrom;
                            dateEnd = dateTo;
                            if (typeRequestOrApproval == (int)UtilConstants.RequestOrApproval.Request)
                            {
                                //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                var courseDetailInstructors = _repoCourseDetailInstructor.GetAll(
                                    a => a.Instructor_Id == instructor.Id &&
                                         a.Course_Detail.IsDeleted != true &&
                                          a.Course_Detail.TMS_APPROVES.Any(t =>
                                             ((dateStar == DateTime.MinValue ||
                                               SqlFunctions.DateDiff("day", dateStar, t.dtm_requested_date) >=
                                               0) &&
                                              (dateEnd == DateTime.MinValue ||
                                               SqlFunctions.DateDiff("day", t.dtm_requested_date, dateEnd) >=
                                               0)) &&
                                             t.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                             t.int_id_status == (int)UtilConstants.EStatus.Approve)).GroupBy(b => b.Course_Detail).Select(c => c.Key);
                                if (courseDetailInstructors.Any())
                                {
                                    ModifyPayment(courseDetailInstructors, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                }
                            }
                            #endregion
                        }

                    }
                }

                return Json(new
                {
                    result = true,
                    message = CMSUtils.alert("success", Messege.SUCCESS),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + ex),
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        private void ModifyPayment(IEnumerable<Course_Detail> courseDetailInstructors, DateTime dateFromSearch, DateTime dateToSearch, DateTime dateStar, DateTime dateEnd, Trainee instructor, UserModel user, int requestOrApproval, IEnumerable<int?> paymentIds)
        {
            foreach (var detail in courseDetailInstructors.Where(a => (!paymentIds.Any() || !paymentIds.Contains(a.Id))))
            {
                var entity = new Payment()
                {
                    Instructor_Id = instructor.Id,
                    DateFrom = dateFromSearch,
                    DateTo = dateToSearch,
                    Is_Deleted = false,
                    Created_By = (int)user.USER_ID,
                    CreationDate = DateTime.Now,
                    Allowance = detail.Course_Detail_Instructor?.FirstOrDefault(a => a.Instructor_Id == instructor.Id)?.Allowance,
                    Duration = detail.Course_Detail_Instructor?.Where(a => a.Instructor_Id == instructor.Id).Sum(a => a.Duration) ?? 0,
                    // logic ngày thanh toán 
                    //Pay_DateFrom = dateStar, 
                    //Pay_DateTo = dateEnd,t.Any(a => a.Duration.HasValue) ? t.Sum(a => a.Duration).Value : 0
                    CourseDetail_Id = detail.Id,
                    Amount = detail.Course_Detail_Instructor?.FirstOrDefault(a => a.Instructor_Id == instructor.Id)?.Allowance * (decimal)(detail.Course_Detail_Instructor?.Where(a => a.Instructor_Id == instructor.Id).Sum(a => a.Duration) ?? 0)
                };
                if (requestOrApproval == (int)UtilConstants.RequestOrApproval.Request)
                {
                    entity.Pay_DateFrom_DateRequest = dateStar;
                    entity.Pay_DateTo_Date_Request = dateEnd;
                }
                else
                {
                    entity.Pay_DateFrom_DateApproval = dateStar;
                    entity.Pay_DateTo_DateApproval = dateEnd;
                }
                ConfigService.InsertPayments(entity);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult PaymentAllApproval(FormCollection form)
        {
            try
            {
                var strFromdate = string.IsNullOrEmpty(form["from2"]) ? string.Empty : form["from2"].ToLower();
                var strTodate = string.IsNullOrEmpty(form["to2"]) ? string.Empty : form["to2"].ToLower();
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                var dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);

                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                var fInstructors = form["InstructorId2"] != null ? form.GetValues("InstructorId2") : null;
                var typeRequestOrApproval = string.IsNullOrEmpty(form["RequestOrApproval2"]) ? -1 : Convert.ToInt32(form["RequestOrApproval2"]);

                var user = GetUser();
                if (fInstructors == null)
                    return Json(new
                    {
                        result = false,
                        message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + Messege.NO_DATA),
                    }, JsonRequestBehavior.AllowGet);
                foreach (var fInstructor in fInstructors)
                {
                    var sInstructor = fInstructor.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    var instructor = EmployeeService.GetById(int.Parse(sInstructor[0]));
                    //var paid = sInstructor[1]; // số tiền đã thanh toán
                    var unpaid = sInstructor[1] != null ? Convert.ToDecimal(sInstructor[1]) : 0; // tổng số tiền chưa search ngày bắt đầu -> ngày kết thúc
                    if (unpaid > 0) // so sánh if khác 0 thì vào hàm
                    {
                        #region [biến tạm]

                        var dateStar = DateTime.MinValue;
                        var dateEnd = DateTime.MinValue;
                        #endregion

                        var payments = instructor.Payments.Where(a => dateFrom <= a.Pay_DateTo_DateApproval && a.Pay_DateFrom_DateApproval <= dateTo).OrderBy(a => a.Pay_DateFrom_DateApproval)
                              /*.Where(a => dateFrom <= a.Pay_DateTo && a.Pay_DateFrom <= dateTo)*/;
                        var paymentIds = payments.Select(a => a.CourseDetail_Id);
                        if (payments.Any())
                        {
                            var midPayments = payments.GroupBy(b => b.Pay_DateFrom_DateApproval).Select(g => g.First());
                            if (midPayments.Count() > 1)
                            {
                                #region// lấy record khoảng giữa 
                                //danh sach các khoảng giữa chưa thanh toán
                                var lstUnpaid = new List<NewFromToPayment>();
                                var tempDateFrom = DateTime.MinValue;
                                var tempDateTo = DateTime.MinValue;
                                var count = 0;
                                foreach (var item in midPayments)
                                {

                                    count++;
                                    if (count % 2 == 0)
                                    {
                                        tempDateTo = item.Pay_DateFrom_DateApproval.Value.Date
                                                .AddDays(-1)
                                                .AddHours(23)
                                                .AddMinutes(59)
                                                .AddSeconds(59);
                                    }
                                    else
                                    {
                                        tempDateFrom = item.Pay_DateTo_DateApproval.Value.AddDays(1);
                                    }
                                    if (/*fromtoPayment.DateFrom.HasValue && fromtoPayment.DateTo.HasValue*/ tempDateFrom != DateTime.MinValue && tempDateTo != DateTime.MinValue)
                                    {
                                        lstUnpaid.Add(new NewFromToPayment()
                                        {
                                            DateFrom = tempDateFrom,
                                            DateTo = tempDateTo
                                        });
                                        tempDateFrom = DateTime.MinValue;
                                        tempDateTo = DateTime.MinValue;
                                    }
                                }
                                #endregion

                                #region //insert các khoảng giữa chưa thanh toán
                                if (lstUnpaid.Any())
                                {
                                    foreach (var item in lstUnpaid)
                                    {

                                        dateStar = item.DateFrom.Value;
                                        dateEnd = item.DateTo.Value;

                                        var getid = _repoTMS_APPROVES_HISTORY.GetAll(
                                            a =>
                                                a.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                a.int_id_status == (int)UtilConstants.EStatus.Approve
                                            )
                                            .OrderByDescending(a => a.id)
                                            .GroupBy(a => a.int_courseDetails_Id)
                                            .Select(c => c.FirstOrDefault())
                                            .Select(a => a.id);
                                        var listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(
                                            a => getid.Contains(a.id)
                                                 &&
                                                 ((dateStar == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", dateStar, a.dtm_requested_date) >= 0) &&
                                                  (dateEnd == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", a.dtm_requested_date, dateEnd) >= 0))
                                            ).Select(a => a.int_courseDetails_Id);
                                        //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                        var courseDetailInstructors = _repoCourseDetailInstructor.GetAll(
                                            a => a.Instructor_Id == instructor.Id &&
                                                 a.Course_Detail.IsDeleted != true &&
                                                 listCourseDetails.Contains(a.Course_Detail_Id)).GroupBy(b => b.Course_Detail).Select(c => c.Key);

                                        if (courseDetailInstructors.Any())
                                        {
                                            ModifyPayment(courseDetailInstructors, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                        }
                                    }
                                }
                                #endregion

                            }

                            else
                            {

                                if (typeRequestOrApproval == (int)UtilConstants.RequestOrApproval.Approval)
                                {
                                    #region//TH 1 lấy từ đầu tới ngày bắt đầu

                                    var firstDateFromPayment = payments.FirstOrDefault();
                                    if (firstDateFromPayment != null &&
                                        dateFrom < firstDateFromPayment.Pay_DateFrom_DateApproval &&
                                        dateTo >= firstDateFromPayment.Pay_DateFrom_DateApproval)
                                    {
                                        #region [Nếu ngày dateTo search > dateFrom DB DATE APPROVAL]

                                        dateStar = dateFrom;
                                        // ngày kết thúc (lấy ngày bắt đầu db - 1)
                                        dateEnd =
                                            firstDateFromPayment.Pay_DateFrom_DateApproval.Value.Date.AddDays(-1)
                                                .AddHours(23)
                                                .AddMinutes(59)
                                                .AddSeconds(59);
                                        var getid = _repoTMS_APPROVES_HISTORY.GetAll(
                                                 a =>
                                                     a.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                     a.int_id_status == (int)UtilConstants.EStatus.Approve
                                                 )
                                                 .OrderByDescending(a => a.id)
                                                 .GroupBy(a => a.int_courseDetails_Id)
                                                 .Select(c => c.FirstOrDefault())
                                                 .Select(a => a.id);
                                        var listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(
                                            a => getid.Contains(a.id)
                                                 &&
                                                 ((dateStar == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", dateStar, a.dtm_requested_date) >= 0) &&
                                                  (dateEnd == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", a.dtm_requested_date, dateEnd) >= 0))
                                            ).Select(a => a.int_courseDetails_Id);

                                        //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                        var courseDetailInstructorsTH1 = _repoCourseDetailInstructor.GetAll(
                                            a => a.Instructor_Id == instructor.Id &&
                                                 a.Course_Detail.IsDeleted != true &&
                                                 listCourseDetails.Contains(a.Course_Detail_Id)).GroupBy(b => b.Course_Detail).Select(c => c.Key);

                                        if (courseDetailInstructorsTH1.Any())
                                        {
                                            ModifyPayment(courseDetailInstructorsTH1, dateFrom, dateTo, dateStar,
                                                dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                        }

                                        #endregion
                                    }
                                    #endregion

                                    #region//TH6 lấy từ ngày kết thúc tới ngày search
                                    var lastDateFromPayment = payments.LastOrDefault();
                                    if (lastDateFromPayment != null &&
                                   lastDateFromPayment.Pay_DateTo_DateApproval < dateTo)
                                    {
                                        #region [ngày bắt đầu form search < ngày kết thúc DB]
                                        // ngày bắt đầu (lấy ngày kết thúc + 1)
                                        dateStar = lastDateFromPayment.Pay_DateTo_DateApproval.Value.AddDays(1);
                                        // ngày kết thúc (lấy ngày đến from search )
                                        dateEnd = dateTo.AddHours(23).AddMinutes(59).AddSeconds(59);
                                        var getid = _repoTMS_APPROVES_HISTORY.GetAll(
                                                      a =>
                                                          a.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                          a.int_id_status == (int)UtilConstants.EStatus.Approve
                                                      )
                                                      .OrderByDescending(a => a.id)
                                                      .GroupBy(a => a.int_courseDetails_Id)
                                                      .Select(c => c.FirstOrDefault())
                                                      .Select(a => a.id);
                                        var listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(
                                            a => getid.Contains(a.id)
                                                 &&
                                                 ((dateStar == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", dateStar, a.dtm_requested_date) >= 0) &&
                                                  (dateEnd == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", a.dtm_requested_date, dateEnd) >= 0))
                                            ).Select(a => a.int_courseDetails_Id);
                                        //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                        var courseDetailInstructorsTH6 = _repoCourseDetailInstructor.GetAll(
                                            a => a.Instructor_Id == instructor.Id &&
                                                 a.Course_Detail.IsDeleted != true &&
                                                  listCourseDetails.Contains(a.Course_Detail_Id)).GroupBy(b => b.Course_Detail).Select(c => c.Key);

                                        if (courseDetailInstructorsTH6.Any())
                                        {
                                            ModifyPayment(courseDetailInstructorsTH6, dateFrom, dateTo, dateStar, dateEnd, instructor, user, typeRequestOrApproval, paymentIds);
                                        }
                                        #endregion
                                    }
                                    if (dateStar == DateTime.MinValue && dateEnd == DateTime.MinValue)
                                    {
                                        #region [Insert bình thường]
                                        //ngày bắt dầu  form search
                                        dateStar = dateFrom;
                                        //lấy ngày kết thúc form search
                                        dateEnd = dateTo.AddHours(23).AddMinutes(59).AddSeconds(59);
                                        var getid = _repoTMS_APPROVES_HISTORY.GetAll(
                                                    a =>
                                                        a.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                        a.int_id_status == (int)UtilConstants.EStatus.Approve
                                                    )
                                                    .OrderByDescending(a => a.id)
                                                    .GroupBy(a => a.int_courseDetails_Id)
                                                    .Select(c => c.FirstOrDefault())
                                                    .Select(a => a.id);
                                        var listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(
                                            a => getid.Contains(a.id)
                                                 &&
                                                 ((dateStar == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", dateStar, a.dtm_requested_date) >= 0) &&
                                                  (dateEnd == DateTime.MinValue ||
                                                   SqlFunctions.DateDiff("day", a.dtm_requested_date, dateEnd) >= 0))
                                            ).Select(a => a.int_courseDetails_Id);
                                        //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                                        var courseDetailInstructors = _repoCourseDetailInstructor.GetAll(
                                            a => a.Instructor_Id == instructor.Id &&
                                                 !a.Course_Detail.IsDeleted == true &&
                                                 listCourseDetails.Contains(a.Course_Detail_Id)).GroupBy(b => b.Course_Detail).Select(c => c.Key);
                                        if (courseDetailInstructors.Any())
                                        {
                                            ModifyPayment(courseDetailInstructors, dateFrom, dateTo, dateStar, dateEnd,
                                                instructor, user, typeRequestOrApproval, paymentIds);
                                        }

                                        #endregion
                                    }
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            #region [Insert lần đầu]
                            dateStar = dateFrom;
                            dateEnd = dateTo;
                            var getid = _repoTMS_APPROVES_HISTORY.GetAll(
                                            a =>
                                                a.int_Type == (int)UtilConstants.ApproveType.SubjectResult &&
                                                a.int_id_status == (int)UtilConstants.EStatus.Approve
                                            )
                                            .OrderByDescending(a => a.id)
                                            .GroupBy(a => a.int_courseDetails_Id)
                                            .Select(c => c.FirstOrDefault())
                                            .Select(a => a.id);
                            var listCourseDetails = _repoTMS_APPROVES_HISTORY.GetAll(
                                a => getid.Contains(a.id)
                                     &&
                                     ((dateStar == DateTime.MinValue ||
                                       SqlFunctions.DateDiff("day", dateStar, a.dtm_requested_date) >= 0) &&
                                      (dateEnd == DateTime.MinValue ||
                                       SqlFunctions.DateDiff("day", a.dtm_requested_date, dateEnd) >= 0))
                                ).Select(a => a.int_courseDetails_Id);
                            //TODO:tindt trạng thái khóa học a.Course.int_Status == 0
                            var courseDetailInstructors = _repoCourseDetailInstructor.GetAll(
                                a => a.Instructor_Id == instructor.Id &&
                                     a.Course_Detail.IsDeleted != true &&
                                      listCourseDetails.Contains(a.Course_Detail_Id)).GroupBy(b => b.Course_Detail).Select(c => c.Key);
                            if (courseDetailInstructors.Any())
                            {
                                ModifyPayment(courseDetailInstructors, dateFrom, dateTo, dateStar, dateEnd,
                                    instructor, user, typeRequestOrApproval, paymentIds);
                            }
                            #endregion
                        }
                    }
                }

                return Json(new
                {
                    result = true,
                    message = CMSUtils.alert("success", Messege.SUCCESS),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + ex),
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult CancelPaymentRequest(FormCollection form)
        {
            try
            {
                var strFromdate = string.IsNullOrEmpty(form["from"]) ? string.Empty : form["from"].ToLower();
                var strTodate = string.IsNullOrEmpty(form["to"]) ? string.Empty : form["to"].ToLower();
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                var dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);

                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;


                var fInstructors = form["InstructorId"] != null ? form.GetValues("InstructorId") : null;

                var user = GetUser();
                if (fInstructors == null)
                    return Json(new
                    {
                        result = false,
                        message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + Messege.NO_DATA),
                    }, JsonRequestBehavior.AllowGet);
                foreach (var fInstructor in fInstructors)
                {
                    var sInstructor = fInstructor.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    var instructor = EmployeeService.GetById(int.Parse(sInstructor[0]));

                    var payments =
                        instructor.Payments.Where(
                            a => a.Is_Cancel == null && dateFrom >= a.Pay_DateFrom_DateRequest && a.Pay_DateTo_Date_Request <= dateTo &&
                            dateFrom <= a.Pay_DateTo_Date_Request && a.Pay_DateFrom_DateRequest <= dateTo).ToList();

                    if (payments.Any())
                    {
                        foreach (var item in payments)
                        {
                            item.Is_Cancel = true;
                            ConfigService.UpdatePayments(item);
                        }
                    }
                }
                return Json(new
                {
                    result = true,
                    message = CMSUtils.alert("success", Messege.SUCCESS),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + ex),
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult CancelPaymentApproval(FormCollection form)
        {
            try
            {
                var strFromdate = string.IsNullOrEmpty(form["from2"]) ? string.Empty : form["from2"].ToLower();
                var strTodate = string.IsNullOrEmpty(form["to2"]) ? string.Empty : form["to2"].ToLower();
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var dateFrom = DateUtil.StringToDate(strFromdate, DateUtil.DATE_FORMAT_OUTPUT);
                var dateTo = DateUtil.StringToDate(strTodate, DateUtil.DATE_FORMAT_OUTPUT);

                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;


                var fInstructors = form["InstructorId2"] != null ? form.GetValues("InstructorId2") : null;

                var user = GetUser();
                if (fInstructors == null)
                    return Json(new
                    {
                        result = false,
                        message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + Messege.NO_DATA),
                    }, JsonRequestBehavior.AllowGet);
                foreach (var fInstructor in fInstructors)
                {
                    var sInstructor = fInstructor.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                    var instructor = EmployeeService.GetById(int.Parse(sInstructor[0]));

                    var payments =
                        instructor.Payments.Where(
                            a => a.Is_Cancel == null && dateFrom >= a.Pay_DateFrom_DateApproval && a.Pay_DateTo_DateApproval <= dateTo &&
                            dateFrom <= a.Pay_DateTo_DateApproval && a.Pay_DateFrom_DateApproval <= dateTo).ToList();
                    if (payments.Any())
                    {
                        foreach (var item in payments)
                        {
                            item.Is_Cancel = true;
                            ConfigService.UpdatePayments(item);
                        }
                    }
                }
                return Json(new
                {
                    result = true,
                    message = CMSUtils.alert("success", Messege.SUCCESS),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    result = false,
                    message = CMSUtils.alert("danger", Messege.FAIL + "<br />" + ex),
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
