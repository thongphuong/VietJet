using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using DAL.Entities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Utils;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Cost;

namespace TrainingCenter.Controllers
{
    using System.Text;
    using TMS.Core.Services.Approves;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.Cost;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.Services.Subject;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.ViewModel;

    public class CostManagementController : BaseAdminController
    {
        #region MyRegion

        private readonly ICostService _repoCosts ;
        private readonly ICourseDetailService _repoCourseServiceDetail;
        private readonly ISubjectService _repoSubject;
        private readonly IApproveService _repoApprove;

        public CostManagementController(IConfigService configService, IUserContext userContext,
            INotificationService notificationService, ICourseMemberService courseMemberService,
            IEmployeeService employeeService, ICourseDetailService courseDetailService,
            IDepartmentService departmentService, ICourseService courseService, ICostService repoCosts, ISubjectService repoSubject, IApproveService repoApprove) : base(
            configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService,
            departmentService, courseService, repoApprove)
        {
            _repoCosts = repoCosts;
            _repoCourseServiceDetail = courseDetailService;
            _repoSubject = repoSubject;
            _repoApprove = repoApprove;
        }

        #endregion

        //
        // GET: /Admin/User/
        

        // return view
        public ActionResult Index(int id = 0)
        {
            return View();
        }
        public ActionResult ReTrainingFee(int? id = 0)
        {
            var model = new CostReTrainingModel();
            model.Course_Retraining =
                   CourseService.Get_BondAgreement(
                        a => a.IsDeleted == false && a.IsActive == true,
                        (int)UtilConstants.ApproveType.CourseResult,
                         (int)UtilConstants.EStatus.Approve
                        )
                        .OrderByDescending(b => b.StartDate).OrderByDescending(b => b.Id)
                        .ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name));
            return View(model);
        }
        public ActionResult BondAgreement(int? id = 0)
        {
            var model = new CostResultModel();
            model.Course_BondAgreement =
                    CourseService.Get_BondAgreement(a => a.IsDeleted == false && a.IsActive == true ,
                        (int)UtilConstants.ApproveType.CourseResult,
                         (int)UtilConstants.EStatus.Approve)
                        .OrderByDescending(b => b.StartDate).OrderByDescending(b => b.Id)
                        .ToDictionary(c => c.Id, c => string.Format("{0} - {1}", c.Code, c.Name));
            return View(model);
        }
        [HttpPost]
        public JsonResult ChangeSubjectReturnCourseResult(int SubjectID)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                int null_instructor = 1;
                var data = _repoCourseServiceDetail.Get(a => a.SubjectDetailId == SubjectID).OrderByDescending(a => a.Id);
               
                if (data.Any())
                {
                    null_instructor = 0;
                    foreach (var item in data)
                    {
                        var datafinal = _repoApprove.Get(a => a.int_Course_id == item.Course.Id  && item.Course.IsDeleted == false,
                            (int)UtilConstants.ApproveType.CourseResult

                            ).OrderByDescending(a => a.int_Course_id);
                        foreach(var itemfinal in datafinal)
                        {
                            html.AppendFormat("<option value='{0}'>{1}</option>", itemfinal.Course.Course_Detail.FirstOrDefault(a=>a.CourseId == itemfinal.int_Course_id).Id, itemfinal.Course.Name);
                        }
                        
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "CostManagement/ChangeSubjectReturnCourseResult", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ChangeCourseReturnSubjectResult(int courseId)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                int null_instructor = 1;
                var data = _repoCourseServiceDetail.Get(a => a.CourseId == courseId).OrderByDescending(a => a.CourseId); //
                if (data.Any())
                {
                    null_instructor = 0;
                    foreach (var item in data)
                    {
                        html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.SubjectDetail.Code+"-"+ item.SubjectDetail.Name);
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "CostManagement/ChangeCourseReturnSubjectResult", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }
        public ActionResult AjaxHandlResultHasInsert(jQueryDataTableParamModel param)
        {
            try
            {
                var value = 0;
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].Trim();
                var courseID = string.IsNullOrEmpty(Request.QueryString["CourseList1"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList1"].Trim());
                var ddlSubject = string.IsNullOrEmpty(Request.QueryString["ddl_subject1"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject1"].Trim());

                var member_list = CourseMemberService.Get(
                        a => (a.Trainee.non_working_day != null && a.Trainee.non_working_day > a.Course_Detail.dtm_time_to ) &&
                        a.Course_Detail.Commitment == true &&  a.Course_Detail.IsDeleted != true &&

                        ((ddlSubject == -1 || a.Course_Details_Id == ddlSubject) && (courseID == -1 || a.Course_Detail.CourseId == courseID))
                        && (string.IsNullOrEmpty(fStaffId) || a.Trainee.str_Staff_Id.Contains(fStaffId))
                        && (string.IsNullOrEmpty(fName) || (a.Trainee.FirstName.Contains(fName) || a.Trainee.LastName.Contains(fName)))
                    ).ToList();

                var member_id = member_list.Select(a => a.Member_Id);
                var course_member_id = member_list.Select(a=>a.Id);

                var list_trainee = EmployeeService.Get(a => a.non_working_day != null && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId))
                         && (string.IsNullOrEmpty(fName) || (a.FirstName.Contains(fName) || a.LastName.Contains(fName))) && member_id.Contains(a.Id)
                       ).ToList();

                var data_ = list_trainee.ToDictionary(a => a, a => a.TMS_Course_Member.Where(b=> course_member_id.Contains(b.Id)));

                var data = new List<TMS_Course_Member>();
                foreach (var item in data_)
                {
                    if (item.Value.Any())
                    {
                        var courseCost = item.Value.GroupBy(a => new { a.Trainee, a.Course_Detail})
                                .Select(a => new TMS_Course_Member
                                {
                                    Trainee = a.Key?.Trainee,
                                    Course_Detail = a.Key?.Course_Detail,
                                    Course_Details_Id = a.Key?.Course_Detail.Id,

                                }).Where(a => a.IsDelete != true).ToList();
                        if (courseCost.Any())
                        {
                            data.AddRange(courseCost);
                        }
                        else
                        {
                            data.Add(new TMS_Course_Member()
                            {
                                Member_Id = item.Key.Id,
                                Trainee = item.Key,
                            });
                        }
                    }
                    else
                    {
                        data.Add(new TMS_Course_Member()
                        {
                            Member_Id = item.Key.Id,
                            Trainee = item.Key,
                        });
                    }
                }



                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                         => sortColumnIndex == 1 ? c?.Trainee?.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c?.Trainee?.FirstName
                                                            : sortColumnIndex == 3 ? c?.Course_Detail?.Course?.Code
                                                            : sortColumnIndex == 4 ? c?.Course_Detail?.SubjectDetail?.Code
                                                            : sortColumnIndex == 6 ? c?.Course_Detail?.dtm_time_from
                                                            : sortColumnIndex == 7 ? c?.Course_Detail?.dtm_time_to
                                                            : (object)c?.Id);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                               c?.Trainee?.str_Staff_Id,
                                                //c?.Trainee.FirstName + " " + c?.Trainee.LastName,
                                                ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                                                c?.Course_Detail?.Course?.Code,
                                               c?.Course_Detail?.SubjectDetail?.Code,
                                                string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", c?.Course_Detail?.Allowance )!= null ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", c?.Course_Detail?.Allowance) : value.ToString(),

                                              commitment((int)c?.Course_Details_Id) == true ? "Yes" : "No",
                                                DateUtil.DateToString(c?.Course_Detail?.dtm_time_to,"dd/MM/yyyy"),
                                                 c?.Course_Detail?.CommitmetExpiredate != null ? DateUtil.DateToString(c?.Course_Detail?.dtm_time_to.Value.AddMonths((int)c?.Course_Detail?.CommitmetExpiredate),"dd/MM/yyyy") : "0",

                                               c?.Trainee?.non_working_day == null ? "Active": "Terminated",
                                                DateUtil.DateToString(c?.Trainee?.non_working_day,"dd/MM/yyyy"),//TotalDate(c?.To,c?.From) 

                                                 Return_days(c?.Course_Detail?.dtm_time_to,c?.Trainee?.non_working_day,c?.Course_Detail?.CommitmetExpiredate ?? 0),
                                               string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}",Compensation_Cost(commitment((int)c?.Course_Details_Id), (int)c?.Course_Details_Id, c?.Course_Detail?.dtm_time_to, c?.Trainee?.non_working_day, c?.Course_Detail?.dtm_time_from,c?.Course_Detail?.CommitmetExpiredate ?? 0))
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "CostManagement/AjaxHandlResultHasInsert", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new object[0]
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AjaxHandlResultCost(jQueryDataTableParamModel param)
        {
            try
            {
                var value = 0;
                var grade = "Fail";
                //var model = new CostTotalViewModel();
                var fName = string.IsNullOrEmpty(Request.QueryString["fName"]) ? "" : Request.QueryString["fName"].Trim();
                var fStaffId = string.IsNullOrEmpty(Request.QueryString["fStaffId"]) ? "" : Request.QueryString["fStaffId"].Trim();
                var courseID = string.IsNullOrEmpty(Request.QueryString["CourseList"]) ? -1 : Convert.ToInt32(Request.QueryString["CourseList"].Trim());
                var ddlSubject = string.IsNullOrEmpty(Request.QueryString["ddl_subject"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_subject"].Trim());

                var member_list = CourseMemberService.Get(
                     a => ((ddlSubject == -1 || a.Course_Details_Id == ddlSubject) && ((courseID == -1 || a.Course_Detail.CourseId == courseID) && a.Course_Detail.Course.IsDeleted == false))
                         && (string.IsNullOrEmpty(fStaffId) || a.Trainee.str_Staff_Id.Contains(fStaffId))
                         && (string.IsNullOrEmpty(fName) || (a.Trainee.FirstName.Contains(fName) || a.Trainee.LastName.Contains(fName)))
                         && a.Course_Detail.Course_Result.Any(c => !string.IsNullOrEmpty(c.Result) && c.CourseDetailId == a.Course_Details_Id && c.TraineeId == a.Member_Id) && a.Course_Detail.Course_Result_Summary.Any(d => d.Result.Equals(grade) && d.CourseDetailId == a.Course_Details_Id && d.TraineeId == a.Member_Id) ).ToList();

                var member_id = member_list.Select(a => a.Member_Id);
                var course_member_id = member_list.Select(a => a.Id);

                var list_trainee = EmployeeService.Get(a => (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId))
                         && (string.IsNullOrEmpty(fName) || (a.FirstName.Contains(fName) || a.LastName.Contains(fName))) && member_id.Contains(a.Id)
                       ).ToList();

                var data_ = list_trainee.ToDictionary(a => a, a => a.TMS_Course_Member.Where(b => course_member_id.Contains(b.Id)));

                var data__ = new List<TMS_Course_Member>();
                foreach (var item in data_)
                {
                    if (item.Value.Any())
                    {
                        var courseCost = item.Value.GroupBy(a => new { a.Trainee, a.Course_Detail })
                                .Select(a => new TMS_Course_Member
                                {
                                    Trainee = a.Key?.Trainee,
                                    Member_Id = a.Key?.Trainee?.Id,
                                    Course_Detail = a.Key?.Course_Detail,
                                    Course_Details_Id = a.Key?.Course_Detail?.Id,
                                   

                                }).Where(a => a.IsDelete != true).ToList();
                        if (courseCost.Any())
                        {
                            data__.AddRange(courseCost);
                        }
                        else
                        {
                            data__.Add(new TMS_Course_Member()
                            {
                                Member_Id = item.Key.Id,
                                Trainee = item.Key,
                            });
                        }
                    }
                    else
                    {
                        data__.Add(new TMS_Course_Member()
                        {
                            Member_Id = item.Key.Id,
                            Trainee = item.Key,
                        });
                    }
                }



                IEnumerable<TMS_Course_Member> filtered = data__;//.OrderByDescending(a => a.StaffId);
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Trainee.str_Staff_Id
                                                            : sortColumnIndex == 2 ? c.Trainee.FirstName
                                                            : sortColumnIndex == 3 ? c.Course_Detail.Course.Code
                                                            : sortColumnIndex == 4 ? c.Course_Detail?.SubjectDetail?.Code
                                                            : sortColumnIndex == 5 ? c.Course_Detail.dtm_time_from
                                                            : sortColumnIndex == 6 ? c.Course_Detail.dtm_time_to
                                                            //: sortColumnIndex == 8 ? (object)c.Grade
                                                            : (object)c.Id);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                                //string.Empty,

                                                c?.Trainee.str_Staff_Id,
                                                //c?.Trainee.FirstName + " " + c?.Trainee.LastName,
                                                ReturnDisplayLanguage(c?.Trainee?.FirstName,c?.Trainee?.LastName),
                                                c?.Course_Detail?.Course?.Code,
                                                c?.Course_Detail?.SubjectDetail?.Code,
                                                DateUtil.DateToString(c?.Course_Detail?.dtm_time_from,"dd/MM/yyyy") ,
                                                DateUtil.DateToString(c?.Course_Detail?.dtm_time_to,"dd/MM/yyyy"),
                                                c.Course_Detail?.Allowance != null ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", c?.Course_Detail?.Allowance) : value.ToString(),
                                                //string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", TotalCost((int)c?.Course_Details_Id)) !="" ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", TotalCost((int)c?.Course_Details_Id)) : value.ToString(),
                                                GetResultGrade(c?.Course_Detail?.SubjectDetailId, c?.Member_Id, c?.Course_Details_Id),

                                                
                                                string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", GetCostReTraining(c?.Course_Detail?.SubjectDetailId, c?.Member_Id, c?.Course_Details_Id)) !="" ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:#,###}", GetCostReTraining(c?.Course_Detail?.SubjectDetailId, c?.Member_Id, c?.Course_Details_Id)) : value.ToString(),
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "CostManagement/AjaxHandlResultCost", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new object[0]
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        private string GetPointRemark(UtilConstants.DetailResult type, int? traineeId, int? courseDetailId)
        {
            string result = "";

            switch (type)
            {
                case (int)UtilConstants.DetailResult.Score:
                    var data = CourseService.GetCourseResultSummaries(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId).FirstOrDefault();
                    result = (data?.point != null) ? data.point?.ToString() : result;
                    break;
                case UtilConstants.DetailResult.Remark:
                    var datar =
               CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId)
                   .OrderByDescending(a => a.Id)
                   .FirstOrDefault();
                    result = datar?.Remark ?? result;
                    break;


            }
            return result;
        }
        private string GetResultGrade(int? subjectDetailId, int? traineeId, int? courseDetailsId)
        {
            float _return = -1;
            var data = CourseService.GetCourseResultSummaries(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailsId).FirstOrDefault();
            _return = (data?.point != null) ? (float)data.point : _return;
            var result = CourseService.GetCourseResult(a=>a.CourseDetailId == courseDetailsId && a.TraineeId == traineeId);
            var grade = "Fail";
            if(_return != -1)
            {
                var subjectDetails = _repoSubject.GetScores(a => a.subject_id == subjectDetailId).OrderByDescending(a => a.point_from);
                foreach (var item in subjectDetails)
                {
                    if (_return >= item.point_from)
                    {
                        grade = item.grade;
                        break;
                    }
                }
            }
            else
            {
                grade = result?.FirstOrDefault()?.Result;
            }
            return grade;
        }
        private decimal? GetCostReTraining(int? subjectDetailId, int? traineeId, int? courseDetailsId)
        {
            decimal? recost = 0;
            var countTraine = CourseService.GetTraineemember(a => a.Course_Details_Id == courseDetailsId).ToList().Count();
            var grade = GetResultGrade(subjectDetailId, traineeId, courseDetailsId);
            if (grade == "Fail")
            {
                //recost = TotalCost((int)courseDetailsId) / countTraine;
                recost = TotalCost((int)courseDetailsId);
            };
            return recost;
        }
        public bool? commitment(int id)
        {
            bool? commit = true;
            var course_Detail = _repoCourseServiceDetail.GetById(id);
            if (course_Detail != null)
            {
                commit = course_Detail.Commitment;
            }
            return commit;
        }
       
        public decimal? CostCourse(int id)
        {
            var Coursecost = CourseService.GetCost(a => a.coursedetail_id == id);
            if(!Coursecost.Any()) return 0;
            var totalCostCourse = Coursecost.Sum(a => a.cost);
            return totalCostCourse;
        }
        public double? CostAlowance(int id)
        {
            var  get_Subject = _repoCourseServiceDetail.Get(a => a.Id == id).FirstOrDefault();
            var sunDuration = get_Subject.Course_Detail_Instructor
                   .Where(t => t.Trainee.bit_Internal==true && t.Duration.HasValue)
                   .Sum(a => a.Duration);
            var sumAllowance = get_Subject.Course_Detail_Instructor
                .Where(t => t.Trainee.bit_Internal==true && t.Duration.HasValue)
                .Sum(a => a.Allowance);
            if (sumAllowance != null)
            {
                var ccc = sunDuration * (float)sumAllowance;
                return ccc;
            }
            return 0;
        }   
        public decimal? CostPerson(int id)
        {
            var costAlowance = _repoCourseServiceDetail.GetById(id);
            var countTraine = CourseService.GetTraineemember(a => a.Course_Details_Id == id).ToList().Count();
            if (costAlowance != null && costAlowance.Allowance != null)
            {
                var totalCostPerson = costAlowance.Allowance*countTraine;
                return decimal.Parse(totalCostPerson.ToString());
            }
            else
            {
                return 0;
            }
        }
        public decimal? TotalCost(int id)
        {
            //var totalCost = CostCourse(id) + (decimal)CostAlowance(id) + CostPerson(id);
            var totalCost = CourseDetailService.GetById(id).Allowance ?? 0;
            //var countTraine = CourseService.GetTraineemember(a => a.Course_Details_Id == id).ToList().Count();
            //if (totalCost != 0)
            //{
            //    totalCost = totalCost / countTraine;
            //}
            return (decimal)totalCost;
        }
        public int? CourseTotalDate(DateTime? dateto, DateTime? datefrom)
        {
           // var total_date = (DateTime.Parse(dateto.ToString()).Date - DateTime.Parse(datefrom.ToString()).Date);

            var testt = (dateto.Value - datefrom.Value).TotalDays;



            return int.Parse(testt.ToString());
        }
        public int? WorkingTotalDate(DateTime? non_working_day, DateTime? datefrom)
        {
            var total_date = (DateTime.Parse(non_working_day.ToString()).Date - DateTime.Parse(datefrom.ToString()).Date);
            return int.Parse(total_date.TotalDays.ToString());
        }
        public int? Return_days(DateTime? dateto, DateTime? non_working_day, double? expire_date)
        {
            if (non_working_day != null && dateto.Value.AddMonths((int)expire_date) > non_working_day.Value.Date)    
            {
                var total_date = ((dateto.Value.AddMonths((int)expire_date)) - non_working_day.Value.Date);
                return int.Parse(total_date.TotalDays.ToString());                         
            }
            else
            {
                return null;
            }
           
        }
        public decimal? Compensation_Cost(bool? commitment,int id, DateTime? dateto, DateTime? non_working_day, DateTime? datefrom, double? expire_date)
        {
            decimal? compensation_cost  = 0;
            //var countTraine = CourseMemberService.Get(a => a.Course_Details_Id == id);
            
            //.ToList().Count();
            if (non_working_day != null && commitment == true && (dateto.Value.AddMonths((int)expire_date) > non_working_day.Value.Date)) // nghỉ việc trước ngày cam kết khóa học
            {   
                var Totalcost = TotalCost(id);
                var TotalCostPer = Totalcost;// (Totalcost / countTraine.Count());

                                             // thời gian kết thúc khóa học + thời hạn cam kết - thời gian kết thúc khóa học
                var TimeCommitment = (int)(dateto.Value.AddMonths((int)expire_date) - dateto.Value).TotalDays; // (int) expire_date;

                var Cost_One_Day_Of_Per = TotalCostPer / TimeCommitment;
                var Time_Return_Working = ((int)(dateto.Value.AddMonths((int)expire_date) - non_working_day.Value.Date).TotalDays);
                compensation_cost = Cost_One_Day_Of_Per * Time_Return_Working;
            }
            return compensation_cost;
        }
    }
}