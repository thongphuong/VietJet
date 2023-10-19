using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac.Core;
using DAL.Entities;
using TMS.API.Models;
using TMS.API.Utilities;
using TMS.Core.Services.Companies;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.CourseResultSummary;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Subject;
using TMS.Core.Services.Users;
using TMS.Core.Utils;
using TMS.Core.ViewModels.APIEmployeeProfile;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.Courses;
using TMS.Core.ViewModels.ViewModel;
using DAL.UnitOfWork;
using Microsoft.Ajax.Utilities;
using TMS.API.Areas.HelpPage.Controllers;
using TMS.Core.Services.Approves;
using TMS.Core.Services.TraineeHis;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.TraineeHistory;
using TMS.Core.Services.Configs;
using TMS.Core.Services.PostNews;
using TMS.Core.ViewModels.Subjects;

namespace TMS.API.Controllers
{
    [System.Web.Http.RoutePrefix("api")]
    public class EmployeeController : ApiController
    {
        private readonly IUserContext _userContext;
        private readonly IEmployeeService _employeeService;
        protected readonly IDepartmentService DepartmentService;
        protected readonly IJobtitleService JobtitleService;
        protected readonly ICompanyService CompanyService;
        protected readonly ICourseService CourseService;
        protected readonly IConfigService ConfigService;
        protected readonly ICourseDetailService CourseDetailService;
        protected readonly ICourseMemberService CourseMemberService;
        protected readonly ISubjectService SubjectService;
        protected readonly ICourseResultSummaryService CourseResultSummaryService;
        protected readonly INotificationService NotificationService;
        protected readonly IApproveService ApproveService;
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly IUserService _userService;
        protected readonly ITraineeHistoryService TraineeHistoryService;
        protected readonly IPostNewsService PostNewsService;
        protected readonly IPostCategoryService PostCategoryService;
        private const int statusModify = (int)UtilConstants.ApiStatus.Modify;
        private const int statusIsSync = (int)UtilConstants.ApiStatus.Synchronize;
        protected const int StatusUnSuccessfully = (int)UtilConstants.ApiStatus.UnSuccessfully;
        private const int statusIsNoResponse = (int)UtilConstants.ApiStatus.NoResponse;
        public EmployeeController(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IUserContext userContext,
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IJobtitleService jobtitleService,
            ICompanyService companyService,
            ICourseService courseService,
            ICourseDetailService courseDetailService,
            IConfigService _ConfigService,
            ICourseMemberService courseMemberService,
            ISubjectService subjectService,
            ICourseResultSummaryService courseResultSummaryService,
            INotificationService notificationService,
            ITraineeHistoryService traineeHistoryService,
            IApproveService approveService,
            IPostNewsService postNewsService,
            IPostCategoryService postCategoryService)
        {
            _userContext = userContext;
            _userService = userService;
            _employeeService = employeeService;
            DepartmentService = departmentService;
            this.JobtitleService = jobtitleService;
            this.CompanyService = companyService;
            this.CourseService = courseService;
            this.CourseDetailService = courseDetailService;
            this.CourseMemberService = courseMemberService;
            this.SubjectService = subjectService;
            this.CourseResultSummaryService = courseResultSummaryService;
            this.NotificationService = notificationService;
            this.ApproveService = approveService;
            this.TraineeHistoryService = traineeHistoryService;
            ConfigService = _ConfigService;
            UnitOfWork = unitOfWork;
            this.PostNewsService = postNewsService;
            this.PostCategoryService = postCategoryService;
        }

        private class UserData
        {
            public string userlogin { get; set; }
            public string passwordlogin { get; set; }
            public string expiredDate { get; set; }
        }

        #region[Login get Token]

        private string CheckVTokenValidation()
        {
            //return "";
            var header = Request.Headers.Contains("Authorization")
                ? Request.Headers.GetValues("Authorization").FirstOrDefault()
                : null;
            if (header != null)
            {
                try
                {
                    header = header.Replace("token ", "");
                    var jsondata = Command.DecryptString(Command.DecryptString(header));
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<UserData>(jsondata);



                    var userlogin = ConfigurationManager.AppSettings["userlogin"];
                    var passwordlogin = ConfigurationManager.AppSettings["passwordlogin"];
                    var pass = data.passwordlogin;
                    var usernamelower = data.userlogin.ToLower();
                    if (userlogin == usernamelower && passwordlogin == pass)
                    {
                        var datetime = DateTime.MinValue;
                        if (DateTime.TryParse(data.expiredDate, out datetime))
                        {
                            var now = DateTime.Now;
                            if (DateTime.Compare(now, datetime) > 0)
                                return "Token expired";
                            return "";
                        }
                        return "Invalid token. My dream: token value";
                    }
                }
                catch (Exception ex)
                {
                    return "Invalid token. My dream: token value";
                }
            }
            return "Missing token. My dream: token value";
        }

        private string Username()
        {
            //return "sontt";
            var header = Request.Headers.Contains("Authorization")
                ? Request.Headers.GetValues("Authorization").FirstOrDefault()
                : null;
            header = header.Replace("token ", "");
            var jsondata = Command.DecryptString(Command.DecryptString(header));
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<UserData>(jsondata);
            return data.userlogin;
        }

        #endregion

        #region [B1 User]

        [System.Web.Http.Route("GetListEmployee")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetListEmployee(string EID = null)
        {
            try
            {
                #region [ checkvalidation ]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }

                #endregion

                var fStaffId = string.IsNullOrEmpty(EID) ? "" : EID;
                var entity = _employeeService.Get(a =>
                    (a.LmsStatus == statusModify || a.LmsStatus == null /*|| a.LmsStatus == statusIsNoResponse*/)
                    && (string.IsNullOrEmpty(fStaffId) || a.str_Staff_Id.Contains(fStaffId)), true).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetListEmployee"] ?? "10")).ToList();

                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    _employeeService.Update(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APITrainee(a)) : null;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }


        //API List trainee chat MENTOR HANNAH
        [System.Web.Http.Route("GetUserByUsername")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetListEmployee2(string username = null)
        {
            try
            {

                var fStaffId = string.IsNullOrEmpty(username) ? "" : username;
                var entity = _employeeService.GetByCode(fStaffId);

                var model = new APIUserMentorHannah();
                if (entity != null)
                {
                    model.Username = entity.str_Staff_Id;
                    model.FirstName = entity.LastName;
                    model.LastName = entity.FirstName;
                    model.Gender = entity.Gender ?? (int)UtilConstants.Gender.Others; ;
                    model.Email = entity.str_Email;
                    model.Role = entity.int_Role ?? (int)UtilConstants.ROLE.Trainee;
                    model.Avatar =
                        (UtilConstants.PathImage +
                         ((string.IsNullOrEmpty(entity.avatar) || entity.avatar.StartsWith("NoAvata"))
                             ? "NoAvata.png"
                             : entity.avatar)) + "";
                    var roles = new List<APIUserMentorHannah.Target>();
                    if (entity.Trainee_Type != null && entity.int_Role == (int)UtilConstants.ROLE.Instructor)
                    {
                        foreach (var type in entity.Trainee_Type)
                        {
                            roles.Add(new APIUserMentorHannah.Target()
                            {
                                Type = type.Type
                            });
                        }
                    }
                    model.Targets = roles;
                    if (entity.TMS_Course_Member.Any())
                    {
                        model.CourseOfMentees = entity.TMS_Course_Member
                            .Where(a => a.IsActive == true && a.IsDelete == false).Select(a =>
                                new APIUserMentorHannah.CourseOfMentee()
                                {
                                    CourseId = a.Course_Details_Id.Value,
                                    CourseName = a.Course_Detail.SubjectDetail.Name
                                });
                    }
                    if (entity.Course_Detail_Instructor.Any())
                    {
                        model.CourseOfMentors = entity.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Mentor).DistinctBy(a => a.Course_Detail_Id).Select(a =>
                                new APIUserMentorHannah.CourseOfMentor()
                                {
                                    CourseId = a.Course_Detail_Id.Value,
                                    CourseName = a.Course_Detail.SubjectDetail.Name
                                });
                        model.CourseOfHannahs = entity.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Hannah).DistinctBy(a => a.Course_Detail_Id).Select(a =>
                            new APIUserMentorHannah.CourseOfHannah()
                            {
                                CourseId = a.Course_Detail_Id.Value,
                                CourseName = a.Course_Detail.SubjectDetail.Name
                            });
                    }

                }

                var result = model;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        #endregion

        #region [B2 Course]

        [System.Web.Http.Route("GetCourseById")]
        [System.Web.Http.HttpGet]
        //API List CourseDetail chat MENTOR HANNAH
        public IHttpActionResult GetListCourseOfProgram(int courseId = -1)
        {
            try
            {
                #region checkvalidation
                var entity = CourseDetailService.GetById(courseId);

                var model = new APICourseMentorHannah();
                if (entity != null)
                {
                    model.CourseId = entity.Id;
                    model.CourseName = entity.SubjectDetail.Name;
                    model.CourseCode = entity.SubjectDetail.Code;
                    model.DateFrom = (int)DateUtil.ConvertToUnixTime((DateTime)entity.dtm_time_from);
                    model.DateTo = (int)DateUtil.ConvertToUnixTime((DateTime)entity.dtm_time_to);
                    model.TimeFrom = entity.time_from ?? "";
                    model.TimeTo = entity.time_to ?? "";
                    model.TimeBlock = entity.TimeBlock ?? 5;
                    model.Time = entity.Time ?? 1;
                    model.SubjectInstructors = entity.Course_Detail_Instructor.Select(a =>
                        new APICourseMentorHannah.SubjectInstructor()
                        {
                            Username = a.Trainee.str_Staff_Id,
                            FullName = a.Trainee.FirstName + " " + a.Trainee.LastName,
                            Type = a.Type ?? (int)UtilConstants.TypeInstructor.Instructor

                        });
                    model.Mentees = entity.TMS_Course_Member.Where(a => a.IsActive == true && a.IsDelete == false)
                        .Select(a => new APICourseMentorHannah.Mentee()
                        {
                            Username = a.Trainee.str_Staff_Id,
                            FullName = a.Trainee.FirstName + " " + a.Trainee.LastName,
                        });
                }
                #endregion

                var result = model;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("GetListProgram")]
        [System.Web.Http.HttpGet]
        //GetListCourse VJT
        //GetListProgram new
        public IHttpActionResult GetListProgram()
        {
            try
            {
                #region checkvalidation

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }

                #endregion

                var entity = CourseService.ApiGet(a => (a.LMSStatus == statusModify || a.LMSStatus == null /*|| a.LMSStatus == statusIsNoResponse*/)).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetListProgram"] ?? "10")).ToList();
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LMSStatus = statusIsNoResponse;
                    CourseService.Update(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APICourse(a)) : null;
                //var result = entity.Select(a => new APICourse(a));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("GetListCourseOfProgram")]
        [System.Web.Http.HttpGet]
        //GetListSubject TVJ
        //GetListCourseOfProgram new
        public IHttpActionResult GetListCourseOfProgram(string codeprogram = "")
        {
            try
            {
                #region checkvalidation

                var msg = CheckVTokenValidation();
                var entity = new List<Course_Detail>();
                var displayName = ConfigService.GetByKey("DisplayLanguage");
                var validCode = string.IsNullOrEmpty(codeprogram) ? "" : codeprogram.ToLower();
                if (!string.IsNullOrEmpty(msg))
                {
                    entity =
                       CourseDetailService.GetAllApi(
                           a => (string.IsNullOrEmpty(validCode) ||
                                 a.Course.Code.ToLower().Contains(validCode)) &&
                                (a.LmsStatus == statusModify || a.LmsStatus == null /*|| a.LmsStatus == statusIsNoResponse*/) && a.Course.LMSStatus == statusIsSync && a.Course.IsDeleted == false && a.Course.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve)).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetListCourseOfProgram"] ?? "10")).ToList();
                }
                else
                {
                    entity =
                        CourseDetailService.GetAllApi(
                            a =>
                                 (a.LmsStatus == statusModify || a.LmsStatus == null /*|| a.LmsStatus == statusIsNoResponse*/) && a.Course.LMSStatus == statusIsSync && a.Course.IsDeleted == false && a.Course.TMS_APPROVES.Any(b => b.int_Type == (int)UtilConstants.ApproveType.Course && b.int_id_status == (int)UtilConstants.EStatus.Approve)).Take(int.Parse( ConfigurationManager.AppSettings["Take_GetListCourseOfProgram"] ?? "0")).ToList();
                }
                #endregion

                //var result = entity.Select(a => new APISubject(a, displayName));

                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    CourseDetailService.Update(item);
                    counttemp++;
                }
                var result = counttemp == count ? entity.Select(a => new APISubject(a, displayName)) : null;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("GetListCategories")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetListCategories(string code = null)
        {
            try
            {
                #region checkvalidation

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var validCode = string.IsNullOrEmpty(code) ? "" : code;
                var entity = SubjectService.GetAPIGroupSubject(a => (a.LmsStatus == statusModify || a.LmsStatus == null /*|| a.LmsStatus == statusIsNoResponse*/) && a.CAT_GROUPSUBJECT_ITEM.Any() && (string.IsNullOrEmpty(validCode) || a.Code.Contains(validCode))).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetListCategories"] ?? "10")).ToList();
                //var result = entity.Select(a => new APIGroupCourse(a));

                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    SubjectService.UpdateGroupSubject(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APIGroupCourse(a)) : null;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        #endregion

        #region [B3 AssignTrainee]

        [System.Web.Http.Route("GetAssignTraineeOld")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAssignTrainee_Old(string CourseCode = null)
        {
            try
            {
                // checkvalidation
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }

                var status = (int)UtilConstants.LMSStatus.AssignTrainee;
                var code = string.IsNullOrEmpty(CourseCode) ? "" : CourseCode;



                var entity = CourseMemberService.Get(a =>
                    (a.Course_Detail.Course.LMSStatus == status || a.Course_Detail.Course.LMSStatus == null)
                    && (String.IsNullOrEmpty(code) || a.Course_Detail.Course.Code.Contains(code))).Take(10).ToList();

                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    CourseMemberService.Update(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APICourseTraineeViewModel(a)) : null;

                #region[update flag]

                if (entity.Any())
                {

                    var courseDetailId = entity.Select(a => a.Course_Details_Id).Distinct();
                    foreach (var item in courseDetailId)
                    {
                        var coursedetail =
                            CourseDetailService.GetById(item);
                        var course = CourseService.GetById(coursedetail.CourseId);
                        course.LMSStatus = (int)UtilConstants.LMSStatus.Synchronize;
                        CourseService.Update(course);
                    }
                }

                #endregion

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("GetAssignTrainee")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAssignTrainee()
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                ;

                #endregion

                var entity = CourseMemberService.GetApi(a => (a.LmsStatus == statusModify || a.LmsStatus == null /*|| a.LmsStatus == statusIsNoResponse*/) &&
                                    ((a.Trainee.LmsStatus == statusIsSync || a.Trainee.LmsStatus == null) &&
                                    a.Trainee.IsDeleted != true) &&
                                    (a.Course_Detail.LmsStatus == statusIsSync &&
                                    a.Course_Detail.IsDeleted != true) &&
                                    a.Course_Detail.Course.IsDeleted != true).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetAssignTrainee"] ?? "10")).ToList();
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    CourseMemberService.Update(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APICourseTraineeViewModel(a)) : null;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("PostAssignTrainee")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostAssignTrainee_Old([FromBody] APICourseTraineeLMSViewModel[] data)
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }

                #endregion

                var username = Username();
                var entity = CourseDetailService.AssignTrainee(data, username);
                if (!string.IsNullOrEmpty(entity))
                {
                    return BadRequest(entity);
                }

                return Ok("Assign employee In LMS successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("PostApplyAssignTrainee")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostApplyAssignTrainee([FromBody] APIAssignLMS[] data)
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                ;

                #endregion

                var username = Username();
                var result = CourseService.ApplyAssignTrainee(data, username);

                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [System.Web.Http.Route("PostAssign")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostAssign([FromBody] APIAssignLMS[] data)
        {
            try
            {
                // checkvalidation
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                var username = Username();
                var result = CourseDetailService.Assign(data, username);

                return Ok(result); //true or false
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region [B4 result]

        [System.Web.Http.Route("GetListCourseResultSummary")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetListCourseResultSummary_Old()
        {
            try
            {
                var str = "c1750s_13013";
                var cutstr = str.Split('_').LastOrDefault();
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                var entity = CourseResultSummaryService.Get().ToList();
                //var result = entity.Select();
                return Ok(cutstr);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }


        [System.Web.Http.Route("GetCourseResultSumary")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetCourseResultSumary()
        {
            try
            {
                #region [ checkvalidation ]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity =
                    CourseResultSummaryService.Get(
                        a =>
                            a.Course_Detail.IsDeleted != true && a.Course_Detail.IsActive == true &&
                            (a.LmsStatus == statusModify || a.LmsStatus == null /*|| a.LmsStatus == statusIsNoResponse*/) && a.Trainee.LmsStatus == statusIsSync &&
                            a.Trainee.IsDeleted != true).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetCourseResultSumary"] ?? "10")).ToList();
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    CourseResultSummaryService.Update(item);
                    counttemp++;
                }

                var result = counttemp == count ? entity.Select(a => new APICourseResultSummary()
                {
                    CourseDetailId = a.CourseDetailId ?? -1,
                    CourseCode = a.Course_Detail?.Course?.Code ?? "",
                    TraineeCode = a.Trainee?.str_Staff_Id ?? "",
                    SubjectCode = a.Course_Detail?.SubjectDetail?.Code ?? "",
                    Grade = !string.IsNullOrEmpty(a.Result) ? a.Result : UtilConstants.Grade.Fail.ToString(),
                    Point = a.point ?? -1,
                    Remark = a.Remark ?? "",
                    IsCompleted = a.Course_Detail?.TMS_APPROVES?.Any(b => b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.SubjectResult) ?? false,
                    StartDate = a.Course_Detail.dtm_time_from.HasValue ? (int)DateUtil.ConvertToUnixTime(a.Course_Detail.dtm_time_from.Value) : 0,
                    EndDate = a.Course_Detail.dtm_time_to.HasValue ? (int)DateUtil.ConvertToUnixTime(a.Course_Detail.dtm_time_to.Value) : 0,
                    ExpireDate = !string.IsNullOrEmpty(a.Result) && a.Result != UtilConstants.Grade.Fail.ToString() ? FunctionReturnExpire(a.Course_Detail, a.TraineeId) : 0,
                }) : null;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        public int FunctionReturnExpire(Course_Detail CourseDetails, int? memberid)
        {
            var valu_ex = 0;
            DateTime? ex_date = null;
            var dataCourseDetail = CourseMemberService.Get(a => a.Course_Detail.SubjectDetail.Name == CourseDetails.SubjectDetail.Name && a.Course_Detail.IsDeleted != true && a.IsActive == true && a.Member_Id == memberid && (a.Status == 0 || a.Status == null), true).Where(a => a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve));
            if (dataCourseDetail.Any())
            {
                IEnumerable<TMS_Course_Member> filteredB = dataCourseDetail;
                filteredB = filteredB.OrderBy(a => a?.Course_Detail?.dtm_time_from);
                var resultB = (from c in filteredB.ToArray()
                               select new ProfileSubjectModel
                               {
                                   bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                   SubjectCode = c?.Course_Detail?.SubjectDetail?.Code,
                                   dtm_from = c?.Course_Detail?.dtm_time_from,
                                   subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                   courseDetails = c?.Course_Detail,
                                   memberId = c?.Member_Id,
                               }).ToList();
                //resultB = resultB.ToList();
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
                    if (CourseDetails?.dtm_time_from == resultB.ElementAt(y).dtm_from)
                    {
                        ex_date = resultB.ElementAt(y).ex_Date;
                    }
                }
            }
            else
            {
                ex_date = ResturnExpiredate(CourseDetails, memberid, ex_date);
            }
            return valu_ex = ex_date != null ? (int)DateUtil.ConvertToUnixTime(ex_date.Value) : 0;
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
                if (courseResults.Any())
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
        [System.Web.Http.Route("PostResult")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostResult([FromBody] APICourseResultViewModel[] data)
        {
            try
            {
                #region [ checkvalidation ]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion
                var username = Username();
                var result = ApproveService.Result(data, username);
                return Ok(result); //true or false
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        #endregion

        #region [B5 Certificate]

        [System.Web.Http.Route("GetCertificateCategory")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetCertificateFinal()
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity =
                    CourseService.GetCourseResultFinal(
                        a =>
                            (a.LmsStatus == statusModify || a.LmsStatus == null) && !string.IsNullOrEmpty(a.certificatefinal))
                        .Take(int.Parse(ConfigurationManager.AppSettings["Take_GetCertificateFinal"] ?? "10"))
                        .ToList();
                // var result = entity.Select(a => new APICertificate(a));
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    CourseService.UpdateCourseResultFinal(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APICertificate(a)) : null;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [System.Web.Http.Route("GetCertificateCourse")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetCertificateCourse()
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity =
                    CourseService.GetCourseResult(
                        a =>
                            (a.LmsStatus == statusModify || a.LmsStatus == statusIsNoResponse || a.LmsStatus == null) && !string.IsNullOrEmpty(a.CertificateSubject))
                        .Take(int.Parse(ConfigurationManager.AppSettings["Take_GetCertificateCourse"] ?? "10"))
                        .ToList();
                //var result = entity.Select(a => new APICertificateResult(a));
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    CourseService.UpdateCourseResult(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APICertificateResult(a)) : null;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region [Notification]

        [System.Web.Http.Route("GetNotification")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetNotification()
        {
            try
            {
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                var entity = NotificationService.GetNotification().ToList();
                var result = entity.Select(a => new APINotification(a));

                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        #endregion

        #region Department
        [System.Web.Http.Route("GetListDepartment")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetListDepartment(string code = null)
        {
            try
            {
                #region [ checkvalidation ]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }

                #endregion

                var entity =
                    DepartmentService.ApiGet(a => (a.LmsStatus == statusModify || a.LmsStatus == null /*|| a.LmsStatus == statusIsNoResponse*/)).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetListDepartment"] ?? "10")).ToList();
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    DepartmentService.Update(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new Core.ViewModels.Departments.APIDepartment(a)) : null;
                //var result = entity.Select(a => new Core.ViewModels.Departments.APIDepartment(a));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        #endregion

        #region [----------------------------LMS My PDP-------------------------------------]
        [System.Web.Http.Route("PostMyPDP")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostMyPDP([FromBody] APITraineeFuture[] data)
        {
            try
            {
                #region [ checkvalidation ]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var userName = Username();

                var result = CourseService.InsertTraineeFuture(data, userName);
                return Ok(result);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region [----------------------Job Title an neu la ThaiVietjet-----------------------------------]

        [System.Web.Http.Route("GetJobTitle")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetJobTitle()
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity = JobtitleService.Get(a => a.LmsStatus == null || a.LmsStatus == statusModify /*|| a.LmsStatus == statusIsNoResponse*/, true, true).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetJobTitle"] ?? "10")).ToList();
                //var result = data.Select(a => new APIJobTitle(a));
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    JobtitleService.Update(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APIJobTitle(a)) : null;
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [System.Web.Http.Route("GetTraineeHistory")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetTraineeHistory()
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                //var data = CourseService.GetCourseResultFinal(
                //    a => !a.IsDeleted && (a.LmsStatus == null || a.LmsStatus == statusModify)).Take(10).ToList();
                //var result = data.DistinctBy(a => a.traineeid).Select(a => new APIJobTitleStandard(a));
                var entity =
                    _employeeService.GetAllTraineeHistories(a => a.LmsStatus == null || a.LmsStatus == statusModify /*|| a.LmsStatus == statusIsNoResponse*/).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetTraineeHistory"] ?? "10")).ToList();
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    _employeeService.Update1(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APITraineeHistory(a)) : null;
                // var result = traineeHistories.Select(a => new APITraineeHistory(a));
                //foreach (var trainee in distinctTraineeids)
                //{
                //    var courseIdAssign = trainee.Course.Course_Detail.Select(a => a.SubjectDetailId);
                //    var traineeId = trainee.traineeid;
                //    var TraineeHistories =
                //        TraineeHistoryService.Get(a => a.Trainee_Id == traineeId).OrderByDescending(a => a.Id).ToList();

                //    if (TraineeHistories.Any())
                //    {
                //        foreach (var jobHistory in TraineeHistories)
                //        {
                //            var subjects = jobHistory.JobTitle.Title_Standard.Select(a => a.SubjectDetail);
                //        }
                //    }
                //}
                return Ok(result);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }


        [System.Web.Http.Route("GetTraineePDP")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetTraineePDP()
        {
            try
            {
                #region [checkvalidation]

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity = _employeeService.GetAllPdp(a => a.LmsStatus == statusModify /*|| a.LmsStatus == statusIsNoResponse*/ || a.LmsStatus == null).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetTraineePDP"] ?? "10")).ToList();
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    _employeeService.Update2(item);
                    counttemp++;
                }


                var result = counttemp == count ? entity.Select(a => new APITraineeFuture(a)) : null;
                //  var result = traineeFuture.Select(a => new APITraineeFuture(a));
                return Ok(result);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region [---------------------------Get List Subject  (Course) an neu la ThaiVietjet--------------------]


        //// GetListSubject new
        [System.Web.Http.Route("GetListSubject")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetListSubject()
        {
            try
            {
                #region checkvalidation

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity = SubjectService.GetSubjectDetailApi(a => (a.LmsStatus == null || a.LmsStatus == statusModify /*|| a.LmsStatus == statusIsNoResponse*/) && a.CourseTypeId.HasValue && a.CourseTypeId != 6).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetListSubject"] ?? "10")).ToList();
                var count = entity.Count();
                var counttemp = 0;
                foreach (var item in entity)
                {
                    item.LmsStatus = statusIsNoResponse;
                    SubjectService.Update(item);
                    counttemp++;
                }
                var result = counttemp == count ? entity.Select(a => new APISubjectDetail(a)) : null;
                //var result = data.Select(a => new APISubjectDetail(a));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region [--------------------------------------Update khi LMS insert success------------------------------]

        [System.Web.Http.Route("UpdateFlagLms")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult UpdateFlagLms([FromBody] APIFlagLMS[] data)
        {
            try
            {
                // checkvalidation
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                var username = Username();
                var result = CourseService.UpdateFlagLMSReturnInt(data, username);

                return Ok(result); //true or false
            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("UpdateFlagLms2")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult UpdateFlagLms2([FromBody] APIFlagLMS[] data)
        {
            try
            {
                // checkvalidation
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                var username = Username();
                var result = CourseService.UpdateFlagLMSReturnModel(data, username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        #endregion

        #region [------------------Update Password  tu LMS----------------------]

        [System.Web.Http.Route("UpdatePassword")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult UpdatePassword([FromBody] APIChangePasswordTrainee data)
        {
            try
            {
                // checkvalidation
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                var currentUser = "LMS" + Username();
                var result = _employeeService.UpdateApi(data, currentUser);
                return Ok(result); //true or false
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("UpdateUser")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult UpdateUser([FromBody] APIChangePasswordFromLMS data)
        {
            try
            {
                // checkvalidation
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                // var currentUser = "LMS" + Username();

                var result = _employeeService.UpdateApiEmployee(data);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        #endregion

        #region [---------------DH DA NANG---------------------------]

        [System.Web.Http.Route("InsertEmployee")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult InsertEmployee([FromBody] APIEmployeeDHDaNang[] data)
        {
            try
            {
                //// checkvalidation
                ////var msg = CheckVTokenValidation();
                ////if (!string.IsNullOrEmpty(msg)) { return BadRequest(msg); }
                //const string username = "DHDaNang";
                //var type = _employeeService.InsertEmployee_DHDaNang(data, username);
                //var result = new AjaxResponseViewModel();
                //switch (type)
                //{
                //    case (int) UtilConstants.StatusApiDHDaNang.Null:
                //        result = new AjaxResponseViewModel()
                //        {
                //            result = false,
                //            message = "data is Null !",
                //            data = data
                //        };
                //        break;
                //    case (int) UtilConstants.StatusApiDHDaNang.Undefined:
                //        result = new AjaxResponseViewModel()
                //        {
                //            result = false,
                //            message = "Undefined !",
                //            data = data
                //        };
                //        break;
                //    case (int) UtilConstants.StatusApiDHDaNang.Insert:
                //        result = new AjaxResponseViewModel()
                //        {
                //            result = true,
                //            message = "Insert Successfully !",
                //            data = data
                //        };
                //        break;
                //    case (int) UtilConstants.StatusApiDHDaNang.Modify:
                //        result = new AjaxResponseViewModel()
                //        {
                //            result = true,
                //            message = "Update Successfully !",
                //            data = data
                //        };
                //        break;
                //    case (int) UtilConstants.StatusApiDHDaNang.UsernameIsNull:
                //        result = new AjaxResponseViewModel()
                //        {
                //            result = false,
                //            message = "Username(Email) is not found !",
                //            data = data
                //        };
                //        break;
                //    case (int) UtilConstants.StatusApiDHDaNang.PasswordIsNull:
                //        result = new AjaxResponseViewModel()
                //        {
                //            result = false,
                //            message = "Password is not found !",
                //            data = data
                //        };
                //        break;
                //}
                //return Ok(result);
                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("GetPostNews")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetPostNews()
        {
            try
            {
                #region checkvalidation

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity = PostNewsService.GetAllPostNews(a => (a.LMSStatus == null || a.LMSStatus == statusModify), true).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetPostNews"] ?? "10")).ToList();
                var result = entity.Select(a => new APIPostNews(a));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [System.Web.Http.Route("GetCategoryPostNews")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetCategoryPostNews()
        {
            try
            {
                #region checkvalidation

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity = PostCategoryService.GetAll(a => (a.LMSStatus == null || a.LMSStatus == statusModify), true).Take(10).ToList();
                var result = entity.Select(a => new APIPostCategoryModel(a));

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }


        [System.Web.Http.Route("InsertContact")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult InsertContact([FromBody] APIContact data)
        {
            try
            {
                // checkvalidation
                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg))
                {
                    return BadRequest(msg);
                }
                var currentUser = "LMS" + Username();
                var result = _employeeService.InsertContact(data, currentUser);
                return Ok(result); //true or false
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("GetSurveyGlobal")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetSurveyGlobal()
        {
            try
            {
                #region checkvalidation

                var msg = CheckVTokenValidation();
                if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);

                #endregion

                var entity = ConfigService.GetSurvey_API(a => (a.LMSStatus == null || a.LMSStatus == statusModify), true).Take(int.Parse(ConfigurationManager.AppSettings["Take_GetSurveyGlobal"] ?? "10")).ToList();
                var result = entity.Select(a => new APISurvey(a));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        #endregion

        #region [----------------------Render partial view Job--------------------------]

        [System.Web.Http.Route("RenderViewJob/{traineeCode}")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult RenderViewJob(string traineeCode = null)
        {

            if (traineeCode != null)
            {
                var getCourseid = CourseService.GetCourseResultFinal(
                    a =>
                        (string.IsNullOrEmpty(traineeCode) ||
                         a.Trainee.str_Staff_Id.ToLower().Trim() == traineeCode.ToLower().Trim())
                        && a.Course.IsDeleted != true && a.Course.IsActive == true).Select(a => a.courseid).ToArray();
                var courseidAssign =
                    CourseDetailService.Get(a => getCourseid.Contains(a.CourseId))
                        .Select(a => a.SubjectDetailId)
                        .Distinct()
                        .ToList();
                var model = new TraineeJobModel()
                {
                    TraineeHistories =
                        TraineeHistoryService.Get(
                            a => a.Trainee.str_Staff_Id.ToLower().Trim() == traineeCode.ToLower().Trim())
                            .OrderByDescending(b => b.Id)
                            .ToList(),
                    //Trainees = _employeeService.GetByCode(traineeCode),
                    //ListSubjectAssign = courseidAssign

                };
                var result = ReturnJobToHtml(model);


                return Json(new
                {
                    data = result.ToString()
                });
                ;
            }

            return NotFound();
        }


        private StringBuilder ReturnJobToHtml(TraineeJobModel model)
        {
            var html = new StringBuilder();

            html.Append("<div class='panel-group' id='accordion'>");
            var count = 0;
            if (model.TraineeHistories != null)
            {

                foreach (var jobHistory in model.TraineeHistories)
                {
                    count++;
                    var subjects = jobHistory.JobTitle.Title_Standard.Select(a => a.SubjectDetail);
                    html.Append("<div class='panel panel-default'>");
                    html.Append("<div class='panel-heading'>");
                    html.Append("<h4 class='panel-title'>");
                    html.Append("<a data-toggle='collapse' data-parent='#accordion' href='panels.html#collapse" +
                                jobHistory.Id + "' aria-expanded='false' class='collapsed'>"
                                + jobHistory.JobTitle.Name + "</a>");
                    html.Append("</h4>");
                    html.Append("</div>");
                    html.Append("<div id='collapse" + jobHistory.Id + "' class='panel-collapse " +
                                (count == 1 ? "in" : "collapse") + "' aria-expanded='true'>");
                    html.Append("<div class='panel-body'>");
                    html.Append("<div class='panel-body'>");
                    html.Append("<div class='col-md-4'>");
                    html.Append("<p>Subject Jobtitle </p>");
                    html.Append("<div class='subjectcontent'>");
                    foreach (var subject in subjects)
                    {
                        if (subject.Title_Standard.Any(b => b.Job_Title_Id == jobHistory.Job_Title_Id))
                        {
                            html.Append("<p>" + subject.Name + "</p>");
                        }
                    }
                    html.Append("</div>");
                    html.Append("</div>");
                    html.Append("<div class='col-md-4'>");
                    html.Append("<p>Training </p>");
                    html.Append("<div class='trainningcontent' >");
                    foreach (var subject in subjects)
                    {
                        if (
                            subject.Title_Standard.Any(
                                b =>
                                    b.Job_Title_Id == jobHistory.Job_Title_Id /*&&
                                    model.ListSubjectAssign.Contains(b.Subject_Id)*/))
                        {
                            html.Append("<p>" + subject.Name + "</p>");
                        }
                    }
                    html.Append("</div>");
                    html.Append("</div>");
                    html.Append("<div class='col-md-4'>");
                    html.Append("<p>Missing Training </p>");
                    html.Append("<div class='missingcontent'>");
                    foreach (var subject in subjects)
                    {
                        if (
                            subject.Title_Standard.Any(
                                b =>
                                    b.Job_Title_Id == jobHistory.Job_Title_Id /*&&
                                    !model.ListSubjectAssign.Contains(b.Subject_Id)*/))
                        {
                            html.Append("<p>" + subject.Name + "</p>");
                        }
                    }
                    html.Append("</div>");
                    html.Append("</div>");
                    html.Append("</div>");
                    html.Append("</div>");
                    html.Append("</div>");
                    html.Append("</div>");

                }
            }
            html.Append("</div>");
            return html;
        }


        #endregion

        #region [----------------Check điều kiện Program Course--------------------]
        [System.Web.Http.Route("BindToSubject")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult BindToSubject([FromBody] APICheckUserBindToSubject data)
        {
            try
            {
                // checkvalidation
                //var msg = CheckVTokenValidation();
                //if (!string.IsNullOrEmpty(msg))
                //{
                //    return BadRequest(msg);
                //}

                var status = "";
                var program_course = CourseService.ApiGet(a => a.Code == data.Program_code).ToList();
                var listBindSubject = program_course.Count() != 0 ? program_course.FirstOrDefault()?.Course_Subject_Item.ToList() : new List<Course_Subject_Item>();
                var resultItem = CourseService.GetCourseResultSummaries(a => a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve) && a.Result == "Pass").ToList();

                var listItem = new List<Course_Result_Summary>();
                var listTrainee = new List<string>();
                var count = 1;

                foreach (var item in resultItem.OrderBy(a => a.TraineeId))
                {
                    foreach (var _item in listBindSubject)
                    {
                        if (item.Course_Detail.SubjectDetailId == _item.SubjectId)
                        {
                            listItem.Add(item);
                        }
                    }
                }
                if (listItem.Count() != 0)
                {
                    for (var i = 1; i < listItem.Count(); i++)
                    {
                        if (listItem.ElementAt(i).TraineeId == listItem.ElementAt(i - 1).TraineeId)
                        {
                            count++;
                        }
                        if (count == listBindSubject.Count())
                        {
                            listTrainee.Add(listItem.ElementAt(i).Trainee.str_Staff_Id);
                            count = 1;
                        }
                    }
                }
                if (listBindSubject.Count() == 0)
                {
                    status = "Allow";
                }
                else if (listTrainee.Any(a => a.Contains(data.Eid)))
                {
                    status = "Allow";
                }
                else
                {
                    status = "Not Allow";
                }
                var result = new APIBindToSubject(data?.Course_code, listBindSubject, status);
                return Ok(result); //true or false
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        #endregion

        #region [Approve From Email]
        [System.Web.Http.Route("AprroveFromEmail")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult AprroveFromEmail([FromUri]QueryData queryData)
        {
            try
            {
                ApproveService.ApproveFromEmail(queryData.id, queryData.type, queryData.strStatus, queryData.strSendMail, queryData.note);
                return Ok(); //true or false
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        #endregion

        //public IHttpActionResult SubmitContent()
        //{
        //    try
        //    {
        //        // checkvalidation
        //        var msg = CheckVTokenValidation();
        //        if (!string.IsNullOrEmpty(msg))
        //        {
        //            return BadRequest(msg);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var currentUser = "LMS" + Username();

        //        return BadRequest(ex.ToString());
        //    }
        //}
        [System.Web.Http.Route("PostSentMail")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult PostSentMail([FromBody] APISentMailModel[] data)
        {
            try
            {
                var result = new APIReturn();
                #region [ checkvalidation ]

                //var msg = CheckVTokenValidation();
                //if (!string.IsNullOrEmpty(msg)) return BadRequest(msg);
                #endregion
                if (data.Any())
                {
                    result = CourseService.InsertSentmailAPI(data[0]);
                }
                return Ok(result); //true or false
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [System.Web.Http.Route("TestConnection")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult TestConnection()
        {
            try
            {
                return Ok("Successfuly Connection");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}



