using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using DAL.Entities;
using TMS.Core.Services.Approves;
using TMS.Core.Utils;
using TMS.Core.ViewModels.Reminder;

namespace TrainingCenter.Controllers
{
    using OfficeOpenXml;
    using OfficeOpenXml.Style;
    using System.IO;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Subject;
    using TMS.Core.Services.Users;
    using TMS.Core.Services.Jobtitle;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.ViewModel;
    using DAL.Repositories;
    using System.Globalization;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.SqlClient;
    using System.Configuration;
    using Dapper;

    public class ReminderController : BaseAdminController
    {
        #region MyRegion

        private readonly ISubjectService _repoSubject;
        private readonly IJobtitleService _repoJobtitle;
        private readonly IRepository<Course_Result> _repoCourseResult = null;
        public ReminderController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, ISubjectService repoSubject, ICourseService repoCourse, IDepartmentService departmentService, IApproveService approveService, IJobtitleService repoJobtitle, IRepository<Course_Result> repoCourseResult)
            : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, repoCourse, approveService)
        {
            _repoSubject = repoSubject;
            _repoJobtitle = repoJobtitle;
            _repoCourseResult = repoCourseResult;
        }


        public new class User
        {
            public string username { get; set; }
            public string score { get; set; }
            public string result { get; set; }
        }
        public class RootObject
        {
            public string courseid { get; set; }
            public List<User> users { get; set; }
        }
        #endregion
        //
        // GET: /Admin/User/
        #region Index
        // return view
        public ActionResult Index(int id = 0)
        {
            var model = new ReminderModel();
            var subjectList = _repoSubject.GetSubjectDetail(a => a.RefreshCycle > 0 && a.CourseTypeId.HasValue && a.CourseTypeId != (int)UtilConstants.CourseTypes.General);
           
            var subjectG = subjectList.GroupBy(a => a.Name.Trim())
                .Select(a => a.OrderByDescending(x => x.IsActive).FirstOrDefault()).OrderBy(a => a.Name.Trim());
            model.SubjectList = subjectG;
            //model.Subjects = subjectList.ToDictionary(a => a.Id, a => (a.IsActive != true ? "(DeActive) " : "") + a.Name);
            model.JobTitleList = new SelectList(_repoJobtitle.Get(true).OrderBy(m => m.Name.Trim()), "Id", "Name");
            model.Departments = LoadDepartment();
            return View(model);
        }

        //TODO: process result

        [Obsolete]
        public ActionResult AjaxHandlerListReminder2(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var ComOrDepId = string.IsNullOrEmpty(Request.QueryString["ComOrDepId"]) ? -1 : Convert.ToInt32(Request.QueryString["ComOrDepId"].Trim());
                List<int> listJobtitle = new List<int>();
                var lst = string.IsNullOrEmpty(Request.QueryString["fJobTitle[]"]) ? new List<string>() : Request.QueryString["fJobTitle[]"].Trim().Split(',').ToList();
                listJobtitle = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                var subjectCode = string.IsNullOrEmpty(Request.QueryString["Subject_Code"]) ? "" : Request.QueryString["Subject_Code"].Trim();
                var subjectName = string.IsNullOrEmpty(Request.QueryString["SubjectCode"]) ? "" : Request.QueryString["SubjectCode"].Trim();
                var fromDateLearning = string.IsNullOrEmpty(Request.QueryString["FromDate"]) ? "" : Request.QueryString["FromDate"];
                int nomout = 0;
                var nom = int.TryParse(Request.QueryString["NOM"], out nomout)
                    ? int.Parse(Request.QueryString["NOM"])
                    : 3;
                var departlist = new List<int>();
                departlist = GetDepartmentChild(ComOrDepId);
                if (ComOrDepId != -1)
                {
                    departlist.Add(ComOrDepId);
                }

                var dateFrom = DateUtil.StringToDate2(fromDateLearning, DateUtil.DATE_FORMAT_OUTPUT);
                var data = CourseService.GetCourseResult(a => a.Course_Detail.IsDeleted != true
                           && (string.IsNullOrEmpty(subjectName) || a.Course_Detail.SubjectDetail.Name.Trim() == subjectName.Trim())
                           && (string.IsNullOrEmpty(subjectCode) || a.Course_Detail.SubjectDetail.Code.Contains(subjectCode))
                           && (departlist.Count() == 0 || departlist.Contains((int)a.Trainee.Department_Id))
                           && (listJobtitle.Count() == 0 || listJobtitle.Contains((int)a.Trainee.Job_Title_id))
                          && (string.IsNullOrEmpty(a.Re_Check_Result) ? (a.First_Check_Result != "F" && !string.IsNullOrEmpty(a.First_Check_Result)) : a.Re_Check_Result != "F")
                           && (a.Course_Detail.TMS_APPROVES.OrderByDescending(b => b.id).FirstOrDefault(
                               c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult).int_id_status == (int)UtilConstants.EStatus.Approve)).OrderByDescending(p => p.Id);
                var filtered = data.AsEnumerable().Where(a => a.Course_Detail.SubjectDetail.RefreshCycle.HasValue && a.Course_Detail.SubjectDetail.RefreshCycle != 0 &&
                 (((new DateTime(a.Course_Detail.dtm_time_to.Value.Year, a.Course_Detail.dtm_time_to.Value.Month, 1).AddMonths(1).AddDays(-1)).AddMonths(a.Course_Detail.SubjectDetail.RefreshCycle.Value).AddDays(1) >= dateFrom.AddMonths(-nom))
                 &&
                 ((new DateTime(a.Course_Detail.dtm_time_to.Value.Year, a.Course_Detail.dtm_time_to.Value.Month, 1).AddMonths(1).AddDays(-1)).AddMonths(a.Course_Detail.SubjectDetail.RefreshCycle.Value).AddDays(1) <= dateFrom.AddMonths(nom)))
                 ).GroupBy(a => new { subjectName = a.Course_Detail.SubjectDetail.Name, subjectCode = a.Course_Detail.SubjectDetail.Code, traineeid = a.TraineeId })
                 .Select(g => new RecurrentModel
                 {
                     str_Staff_Id = g.FirstOrDefault()?.Trainee?.str_Staff_Id,
                     str_Fullname = ReturnDisplayLanguage(g.FirstOrDefault()?.Trainee?.FirstName, g.FirstOrDefault()?.Trainee?.LastName),
                     dept_Name = g.FirstOrDefault()?.Trainee?.Department?.Name,
                     subj_Code = g.Key?.subjectCode,
                     subj_Name = g.Key?.subjectName,
                     start_Date = g.FirstOrDefault()?.Course_Detail?.dtm_time_from,
                     ex_Date = ResturmExpiredate(g.ToList(), nom),
                     Validity = (ResturmExpiredate(g.ToList(), nom).Value - dateFrom).TotalDays.ToString("####")
                 });
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<RecurrentModel, object> orderingFunction = (c
                                                        => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                          : sortColumnIndex == 2 ? c?.str_Fullname
                                                          : sortColumnIndex == 3 ? c?.dept_Name
                                                          : sortColumnIndex == 4 ? c?.subj_Code
                                                          : sortColumnIndex == 5 ? c?.subj_Name
                                                          : sortColumnIndex == 6 ? c?.start_Date
                                                          : sortColumnIndex == 7 ? c?.ex_Date
                                                          : sortColumnIndex == 8 ? (object)c?.Validity
                                                          : c.ex_Date);
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
                                                c?.str_Staff_Id,
                                                c?.str_Fullname,
                                                c?.dept_Name,
                                                c?.subj_Code,
                                                c?.subj_Name,
                                                c?.start_Date.Value.ToString("dd/MM/yyyy"),
                                                c?.ex_Date.Value.ToString("dd/MM/yyyy"),
                                                returncolunm(double.Parse(c?.Validity))
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Reminder/AjaxHandlerListReminder", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public class CoureResultItem
        {
            public Nullable<int> IdResult { get; set; }
            public Nullable<int> TraineeId { get; set; }
            public Nullable<int> CourseDetailId { get; set; }
            public string SubjectCode { get; set; }
            public string SubjectName { get; set; }
            public Nullable<int> RefreshCycle { get; set; }
            public Nullable<System.DateTime> dtm_time_from { get; set; }
            public Nullable<System.DateTime> dtm_time_to { get; set; }
        }


        [Obsolete]
        public ActionResult AjaxHandlerListReminder(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                var ComOrDepId = string.IsNullOrEmpty(Request.QueryString["ComOrDepId"]) ? -1 : Convert.ToInt32(Request.QueryString["ComOrDepId"].Trim());
                List<int> listJobtitle = new List<int>();
                var lst = string.IsNullOrEmpty(Request.QueryString["fJobTitle[]"]) ? new List<string>() : Request.QueryString["fJobTitle[]"].Trim().Split(',').ToList();
                listJobtitle = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
                var subjectCode = string.IsNullOrEmpty(Request.QueryString["Subject_Code"]) ? "" : Request.QueryString["Subject_Code"].Trim();
                var subjectName = string.IsNullOrEmpty(Request.QueryString["SubjectCode"]) ? "" : Request.QueryString["SubjectCode"].Trim();
                var fromDateLearning = string.IsNullOrEmpty(Request.QueryString["FromDate"]) ? "" : Request.QueryString["FromDate"];
                int nomout = 0;
                var nom = int.TryParse(Request.QueryString["NOM"], out nomout)
                    ? int.Parse(Request.QueryString["NOM"])
                    : 3;
                var departlist = new List<int>();
                departlist = GetDepartmentChild(ComOrDepId);
                if (ComOrDepId != -1)
                {
                    departlist.Add(ComOrDepId);
                }

                var dateFrom = DateUtil.StringToDate2(fromDateLearning, DateUtil.DATE_FORMAT_OUTPUT);
                //var data = CourseService.GetCourseResult(a => a.Course_Detail.SubjectDetail.RefreshCycle.HasValue && a.Course_Detail.SubjectDetail.RefreshCycle != 0 && a.Course_Detail.IsDeleted != true
                //           && (string.IsNullOrEmpty(subjectName) || a.Course_Detail.SubjectDetail.Name.Trim() == subjectName.Trim())
                //           && (string.IsNullOrEmpty(subjectCode) || a.Course_Detail.SubjectDetail.Code.Contains(subjectCode))
                //           && (departlist.Count() == 0 || departlist.Contains((int)a.Trainee.Department_Id))
                //           && (listJobtitle.Count() == 0 || listJobtitle.Contains((int)a.Trainee.Job_Title_id))
                //          && (string.IsNullOrEmpty(a.Re_Check_Result) ? (a.First_Check_Result != "F" && !string.IsNullOrEmpty(a.First_Check_Result)) : a.Re_Check_Result != "F")
                //           && (a.Course_Detail.TMS_APPROVES.OrderByDescending(b => b.id).FirstOrDefault(
                //               c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult).int_id_status == (int)UtilConstants.EStatus.Approve)).OrderByDescending(p => p.Id);



                //var count1 = data.Count();
                //var filtered__ = data.AsEnumerable().Where(a =>
                // ((new DateTime(a.Course_Detail.dtm_time_to.Value.Year, a.Course_Detail.dtm_time_to.Value.Month, 1).AddMonths(a.Course_Detail.SubjectDetail.RefreshCycle.Value + 1) >= dateFrom.AddMonths(-nom))
                // &&
                // (new DateTime(a.Course_Detail.dtm_time_to.Value.Year, a.Course_Detail.dtm_time_to.Value.Month, 1).AddMonths(a.Course_Detail.SubjectDetail.RefreshCycle.Value + 1) <= dateFrom.AddMonths(nom)))
                // );
                //var count2 = filtered__.Count();
                var CourseResultList = new List<CoureResultItem>();
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    var sql = "select H.id as IdResult, H.TraineeId as TraineeId, H.CourseDetailId as CourseDetailId, SJ.Code as SubjectCode, Sj.Name as SubjectName, SJ.RefreshCycle as RefreshCycle, CD.dtm_time_from as dtm_time_from, Cd.dtm_time_to as dtm_time_to FROM Course_Result H INNER JOIN Course_Detail CD ON CD.Id = H.CourseDetailId INNER JOIN Trainee T ON T.Id = H.TraineeId INNER JOIN SubjectDetail SJ ON SJ.Id = CD.SubjectDetailId INNER JOIN TMS_APPROVES TA ON TA.id = (SELECT TOP 1 TA2.id FROM TMS_APPROVES AS TA2 WHERE TA2.int_courseDetails_Id = CD.Id order by id desc) where ((H.Re_Check_Result is not null AND H.Re_Check_Result <> 'F') OR (H.Re_Check_Result is null and H.First_Check_Result <> 'F')) AND ISNULL(CD.IsDeleted,0) <> 1";

                    if(!string.IsNullOrEmpty(subjectName))
                    {
                        sql += " AND SJ.Name = '" + subjectName + "'";
                    }
                    if (!string.IsNullOrEmpty(subjectCode))
                    {
                        sql += " AND SJ.Code like '%" + subjectCode + "%'";
                    }
                    CourseResultList = conn.Query<CoureResultItem>(sql).ToList();
                }




                var filtered__ = CourseService.GetReminder(string.Join(",", departlist), string.Join(",", listJobtitle), subjectName, subjectCode, dateFrom, nom);

                var filtered_ = filtered__.GroupBy(a => new { subjectName = a.subj_Name, subjectCode = a.subj_Code, traineeid = a.TraineeId }); 
                var displayed = filtered_.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var filtered = displayed.Select(g => new RecurrentModel
                {
                    str_Staff_Id = g.FirstOrDefault()?.str_Staff_Id,
                    str_Fullname = ReturnDisplayLanguage(g.FirstOrDefault()?.str_FirstName, g.FirstOrDefault()?.str_LastName),
                    dept_Name = g.FirstOrDefault()?.dept_Name,
                    subj_Code = g.Key?.subjectCode,
                    subj_Name = g.Key?.subjectName,
                    start_Date = g.FirstOrDefault()?.dtm_time_from,
                    ex_Date = ResturmExpiredate_Custom1(g.FirstOrDefault(), nom, CourseResultList),
                    Validity = ResturmExpiredate_Custom1(g.FirstOrDefault(), nom, CourseResultList).HasValue ? (ResturmExpiredate_Custom1(g.FirstOrDefault(), nom, CourseResultList).Value - dateFrom).TotalDays.ToString("####") : "",

                });
               
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<RecurrentModel, object> orderingFunction = (c
                                                        => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                          : sortColumnIndex == 2 ? c?.str_Fullname
                                                          : sortColumnIndex == 3 ? c?.dept_Name
                                                          : sortColumnIndex == 4 ? c?.subj_Code
                                                          : sortColumnIndex == 5 ? c?.subj_Name
                                                          : sortColumnIndex == 6 ? c?.start_Date
                                                          : sortColumnIndex == 7 ? c?.ex_Date
                                                          : sortColumnIndex == 8 ? (object)c?.Validity
                                                          : c.ex_Date);
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                     : filtered.OrderByDescending(orderingFunction);
                }
              
                var result = from c in filtered.ToArray()
                             select new object[] {
                                                string.Empty,
                                                c?.str_Staff_Id,
                                                c?.str_Fullname,
                                                c?.dept_Name,
                                                c?.subj_Code,
                                                c?.subj_Name,
                                                c?.start_Date.Value.ToString("dd/MM/yyyy"),
                                                c.ex_Date.HasValue ? c?.ex_Date.Value.ToString("dd/MM/yyyy") : "",
                                                !string.IsNullOrEmpty(c?.Validity) ? returncolunm(double.Parse(c?.Validity)) : ""
                        };
                var jsonresult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered_.Count(),
                    iTotalDisplayRecords = filtered_.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonresult.MaxJsonLength = int.MaxValue;
                return jsonresult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Reminder/AjaxHandlerListReminder", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        //TODO: process result
        public DateTime? ResturmExpiredate(List<Course_Result> listCourseResult, int nom)
        {
            var returnVal = new DateTime();

            if (listCourseResult.Count() > 1)
            {
                //lấy 2 phần tử cuối
                var courseResults = listCourseResult.OrderByDescending(a => a.Course_Detail.dtm_time_from).Take(2);
                var prevExdate = new DateTime();
                //-- debug
                //// Kq lần đầu
                var enumerable = courseResults as Course_Result[] ?? courseResults.ToArray();
                var lastOrDefault = enumerable.LastOrDefault();
                if (lastOrDefault?.Course_Detail.dtm_time_to != null)
                {
                    var fromdateLast = returnDateExpireTepm((DateTime)lastOrDefault.Course_Detail.dtm_time_to, (int)lastOrDefault.Course_Detail.SubjectDetail.RefreshCycle);
                    prevExdate = fromdateLast;
                }
                var firstOrDefault = enumerable.FirstOrDefault();
                if (firstOrDefault?.Course_Detail.dtm_time_from == null) return returnVal;
                //var fromdateFirst = firstOrDefault.Course_Detail.dtm_time_from;
                var fromdateFirst = (DateTime)firstOrDefault.Course_Detail.dtm_time_to;
                var expiredate = prevExdate;
                var expiredate3Month = expiredate.AddMonths(-3);
                returnVal = returnDateExpireTepm(fromdateFirst, (int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle);

                if (expiredate3Month < fromdateFirst && fromdateFirst <= expiredate)
                {
                    returnVal = expiredate.AddMonths((int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle);
                }
            }
            else
            {
                var courseResult = listCourseResult.FirstOrDefault();
                if (courseResult?.Course_Detail?.dtm_time_to != null)
                    returnVal = returnDateExpireTepm((DateTime)courseResult.Course_Detail.dtm_time_to, (int)courseResult.Course_Detail.SubjectDetail.RefreshCycle);
            }
            return returnVal;
        }
        public DateTime? ResturmExpiredate_Custom(List<sp_Reminder_TV_Result> listCourseResult, int nom)
        {
            var returnVal = new DateTime();

            if (listCourseResult.Count() > 1)
            {
                //lấy 2 phần tử cuối
                var courseResults = listCourseResult.OrderByDescending(a => a.dtm_time_from).Take(2);
                var prevExdate = new DateTime();
                //-- debug
                //// Kq lần đầu
                var enumerable = courseResults as sp_Reminder_TV_Result[] ?? courseResults.ToArray();
                var lastOrDefault = enumerable.LastOrDefault();
                if (lastOrDefault?.dtm_time_to != null)
                {
                    var fromdateLast = returnDateExpireTepm((DateTime)lastOrDefault.dtm_time_to, (int)lastOrDefault.RefreshCycle);
                    prevExdate = fromdateLast;
                }
                var firstOrDefault = enumerable.FirstOrDefault();
                if (firstOrDefault?.dtm_time_from == null) return returnVal;
                //var fromdateFirst = firstOrDefault.Course_Detail.dtm_time_from;
                var fromdateFirst = (DateTime)firstOrDefault.dtm_time_to;
                var expiredate = prevExdate;
                var expiredate3Month = expiredate.AddMonths(-3);
                returnVal = returnDateExpireTepm(fromdateFirst, (int)firstOrDefault.RefreshCycle);

                if (expiredate3Month < fromdateFirst && fromdateFirst <= expiredate)
                {
                    returnVal = expiredate.AddMonths((int)firstOrDefault.RefreshCycle);
                }
            }
            else
            {
                var courseResult = listCourseResult.FirstOrDefault();
                if (courseResult?.dtm_time_to != null)
                    returnVal = returnDateExpireTepm((DateTime)courseResult.dtm_time_to, (int)courseResult.RefreshCycle);
            }
            return returnVal;
        }
        public DateTime? ResturmExpiredate_Custom1(sp_Reminder_TV_Result listCourseResult, int nom, List<CoureResultItem> CoureResultItem)
        {
            var returnVal = new DateTime();
            try
            {
                var enumerable = CoureResultItem.Where(a => a.TraineeId == listCourseResult.TraineeId && a.SubjectCode == listCourseResult.subj_Code
                                                  && a.dtm_time_from <= listCourseResult.dtm_time_from);
              
                if (!enumerable.Any()) return null;
               
                if (enumerable.Count() > 1)
                {
                    var prevExdate = new DateTime();
                    var _listitem = enumerable.OrderBy(a => a.dtm_time_from).ToArray();
                    for (int y = 0; y < _listitem.Count(); y++)
                    {
                        //lấy 2 phần tử cuối
                        var courseResults = enumerable.Where(a=> a.dtm_time_from <= _listitem[y].dtm_time_from).OrderByDescending(a => a.dtm_time_from).Take(2);
                        if(courseResults.Count() > 1)
                        {
                            if (courseResults.Any())
                            {

                                var lastOrDefault = courseResults.AsEnumerable().LastOrDefault();
                                if (lastOrDefault?.dtm_time_to != null)
                                {
                                    var fromdateLast = returnDateExpireTepm((DateTime)lastOrDefault?.dtm_time_to, (int)lastOrDefault?.RefreshCycle);
                                    if (prevExdate == null)
                                    {
                                        prevExdate = fromdateLast;
                                    }                                   
                                }
                                var firstOrDefault = courseResults?.FirstOrDefault();
                                if (firstOrDefault?.dtm_time_from == null) return returnVal;
                                //var fromdateFirst = firstOrDefault.Course_Detail.dtm_time_from;
                                var fromdateFirst = (DateTime)firstOrDefault?.dtm_time_to;
                                var expiredate = prevExdate;
                                var expiredate3Month = expiredate.AddMonths(-3);
                                returnVal = returnDateExpireTepm(fromdateFirst, (int)firstOrDefault?.RefreshCycle);

                                if (expiredate3Month < fromdateFirst && fromdateFirst <= expiredate)
                                {
                                    returnVal = expiredate.AddMonths((int)firstOrDefault?.RefreshCycle);
                                }
                                prevExdate = returnVal;
                            }
                            //-- debug
                            //// Kq lần đầu
                        }
                        else
                        {
                            var courseResult = courseResults.FirstOrDefault();
                            if (courseResult?.dtm_time_to != null)
                            {
                                returnVal = returnDateExpireTepm((DateTime)courseResult?.dtm_time_to, (int)courseResult.RefreshCycle);
                                prevExdate = returnVal;
                            }
                               
                        }
                    }
                }
                else
                {
                    var courseResult = enumerable.FirstOrDefault();
                    if (courseResult?.dtm_time_to != null)
                        returnVal = returnDateExpireTepm((DateTime)courseResult?.dtm_time_to, (int)courseResult.RefreshCycle);
                }
                return returnVal;
            }
            catch (Exception ex)
            {

                return returnVal;
            }
         
           
        }
        private DateTime returnDateExpireTepm(DateTime fromdate, int cycle)
        {
            return (new DateTime(fromdate.Year, fromdate.Month, 1).AddMonths(1).AddDays(-1)).AddMonths(cycle);
        }

        #region SendMail



        public JsonResult SendMail(string subjectID, string TraineeID)
        {

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion
        [AllowAnonymous]
        public FileContentResult RecurrentTrainingExport(string ComOrDepId_, string fJobTitle_, string Subject_Code_, string SubjectCode_, string FromDate_, string NOM_)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            int ComOrDepId = string.IsNullOrEmpty(ComOrDepId_) ? -1 : Convert.ToInt32(ComOrDepId_.Trim());

            List<int> listJobtitle = new List<int>();
            var lst = string.IsNullOrEmpty(Request.QueryString["fJobTitle_[]"]) ? new List<string>() : Request.QueryString["fJobTitle_[]"].Trim().Split(',').ToList();
            listJobtitle = lst.Count > 0 ? lst.Select(x => Convert.ToInt32(x)).ToList() : new List<int>();
            var subjectName = string.IsNullOrEmpty(SubjectCode_) ? "" : SubjectCode_.Trim();
            string subjectCode = string.IsNullOrEmpty(Subject_Code_) ? string.Empty : Subject_Code_.ToLower();
            string fromDateLearning = string.IsNullOrEmpty(FromDate_) ? string.Empty : FromDate_.ToLower();
            int nomout = 0; 
            var nom = int.TryParse(NOM_, out nomout)
                 ? int.Parse(NOM_)
                 : 3;
            var departlist = new List<int>();
            departlist = GetDepartmentChild(ComOrDepId);
            if (ComOrDepId != -1)
            {
                departlist.Add(ComOrDepId);
            }
            var CourseResultList = new List<CoureResultItem>();
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                var sql = "select H.id as IdResult, H.TraineeId as TraineeId, H.CourseDetailId as CourseDetailId, SJ.Code as SubjectCode, Sj.Name as SubjectName, SJ.RefreshCycle as RefreshCycle, CD.dtm_time_from as dtm_time_from, Cd.dtm_time_to as dtm_time_to FROM Course_Result H INNER JOIN Course_Detail CD ON CD.Id = H.CourseDetailId INNER JOIN Trainee T ON T.Id = H.TraineeId INNER JOIN SubjectDetail SJ ON SJ.Id = CD.SubjectDetailId INNER JOIN TMS_APPROVES TA ON TA.id = (SELECT TOP 1 TA2.id FROM TMS_APPROVES AS TA2 WHERE TA2.int_courseDetails_Id = CD.Id order by id desc) where ((H.Re_Check_Result is not null AND H.Re_Check_Result <> 'F') OR (H.Re_Check_Result is null and H.First_Check_Result <> 'F')) AND ISNULL(CD.IsDeleted,0) <> 1";

                if (!string.IsNullOrEmpty(subjectName))
                {
                    sql += " AND SJ.Name = '" + subjectName + "'";
                }
                if (!string.IsNullOrEmpty(subjectCode))
                {
                    sql += " AND SJ.Code like '%" + subjectCode + "%'";
                }
                CourseResultList = conn.Query<CoureResultItem>(sql).ToList();
            }
            var dateFrom = DateUtil.StringToDate2(fromDateLearning, DateUtil.DATE_FORMAT_OUTPUT);
            var filtered__ = CourseService.GetReminder(string.Join(",", departlist), string.Join(",", listJobtitle), subjectName, subjectCode, dateFrom, nom);

            var filtered_ = filtered__.GroupBy(a => new { subjectName = a.subj_Name, subjectCode = a.subj_Code, traineeid = a.TraineeId });
            //var resultA = filtered_.Select(g => new RecurrentModel
            //                {
            //                    str_Staff_Id = g.FirstOrDefault()?.str_Staff_Id,
            //                    str_Fullname = ReturnDisplayLanguage(g.FirstOrDefault()?.str_FirstName, g.FirstOrDefault()?.str_LastName),
            //                    dept_Name = g.FirstOrDefault()?.dept_Name,
            //                    subj_Code = g.Key?.subjectCode,
            //                    subj_Name = g.Key?.subjectName,
            //                    start_Date = g.FirstOrDefault()?.dtm_time_from,
            //                    ex_Date = ResturmExpiredate_Custom(g.ToList(), nom),
            //                    Validity = (ResturmExpiredate_Custom(g.ToList(), nom).Value - dateFrom).TotalDays.ToString("####")
            //                });
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "/Template/ExcelFile/RecurrentTrainingReport.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            ExcelPackage xlPackage;
            MemoryStream MS = new MemoryStream();
            byte[] Bytes = null;
            using (xlPackage = new ExcelPackage(template, false))
            {
                var worksheet = xlPackage.Workbook.Worksheets["Sheet1"];
                int startrow = 7;
                int groupHeader = 0;
                int grouptotal = 0;
                int row = 0;
                int col = 1;
                int count = 0;
                foreach (var item1 in filtered_)
                {
                    count++;
                    ExcelRange cellNo = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col];
                    cellNo.Value = count;
                    cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellCourse = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 1];
                    cellCourse.Value = item1?.FirstOrDefault()?.str_Staff_Id;
                    cellCourse.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellMethod = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 2];
                    cellMethod.Value = ReturnDisplayLanguage(item1?.FirstOrDefault()?.str_FirstName, item1.FirstOrDefault()?.str_LastName);
                    cellMethod.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    cellMethod.Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    ExcelRange cellHours = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 3];
                    cellHours.Value = item1?.FirstOrDefault()?.dept_Name;
                    cellHours.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellHours.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange celljobs = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 4];
                    celljobs.Value = item1?.FirstOrDefault()?.job_Name;
                    celljobs.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    celljobs.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellDays = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 5];
                    cellDays.Value = item1?.Key?.subjectCode;
                    cellDays.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellDays.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellDate = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 6];
                    cellDate.Value = item1?.Key?.subjectName;
                    cellDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellstartdate = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 7];
                    cellstartdate.Value = item1?.FirstOrDefault()?.dtm_time_from.Value.ToString("dd/MM/yyyy");// item1?.Trainee?.str_Fullname;
                    cellstartdate.Style.WrapText = true;
                    cellstartdate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellstartdate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellInstructor = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 8];
                    cellInstructor.Value = ResturmExpiredate_Custom1(item1.FirstOrDefault(), nom, CourseResultList).HasValue ? ResturmExpiredate_Custom1(item1.FirstOrDefault(), nom, CourseResultList).Value.ToString("dd/MM/yyyy") : "";// item1?.Trainee?.str_Fullname;
                    cellInstructor.Style.WrapText = true;
                    cellInstructor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellInstructor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    ExcelRange cellNum = worksheet.Cells[startrow + row + 1 + groupHeader + grouptotal, col + 9];
                    cellNum.Value = ResturmExpiredate_Custom1(item1.FirstOrDefault(), nom, CourseResultList).HasValue ?  (ResturmExpiredate_Custom1(item1.FirstOrDefault(), nom, CourseResultList).Value - dateFrom).TotalDays.ToString("####") : "";
                    cellNum.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cellNum.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    row++;
                }
                using (ExcelRange r = worksheet.Cells[startrow, 1, startrow + row + 1 + groupHeader + grouptotal, 10])
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

                Bytes = xlPackage.GetAsByteArray();

            }

            return File(Bytes, ExportUtils.ExcelContentType, "RecurrentTrainingReport.xlsx");
        }
        private string LoadDepartment(int? id = null)
        {
            var result = string.Empty;
            var data = DepartmentService.Get().Select(x => new { x.Id, x.Ancestor, x.Name, x.Code });
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
        private readonly List<int> _deptChil = new List<int>();
        private List<int> GetDepartmentChild(int idParent)
        {
            var dept_Con = DepartmentService.Get(a => a.ParentId == idParent);
            if (dept_Con.Any())
            {
                foreach (var item in dept_Con)
                {
                    GetDepartmentChild(item.Id);
                    _deptChil.Add(item.Id);
                }
            }
            return _deptChil;
        }
        private string returncolunm(double input)
        {
            string html = "";
            if (input < 0 || input == 0)
            {
                html = "<p style='color:red;'>" + input + "</p>";
            }
            else
            {
                html = "<p>" + input + "</p>";
            }
            return html;
        }
    }
}
