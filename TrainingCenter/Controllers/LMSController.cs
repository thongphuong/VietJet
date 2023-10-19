using DAL.Entities;
using DAL.Repositories;
using DAL.UnitOfWork;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Companies;
using TMS.Core.Services.Configs;
using TMS.Core.Services.Cost;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.CourseResultSummary;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Orientation;
using TMS.Core.Services.Subject;
using TMS.Core.Services.Users;
using TMS.Core.Utils;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Courses;
using TMS.Core.ViewModels.ReportModels;
using TMS.Core.ViewModels.UserModels;
using TMS.Core.ViewModels.ViewModel;
using TrainingCenter.Utilities;

namespace TrainingCenter.Controllers
{
    public class LMSController : Controller
    {
        private readonly IApproveService _repoTmsApproves;
        private readonly ICourseService _CourseService;
        private readonly ICourseDetailService _CourseDetailService;
        private readonly IUserContext _userContext;
        private readonly IConfigService _configService;
        private readonly IDepartmentService _departmentService;
        private readonly ISubjectService _repoSubject;
        private readonly IEmployeeService _EmployeeService;
        private readonly ICourseResultSummaryService _repoCourseResultSummaryService;
        private readonly INotificationService _NotificationService;
        private readonly ICourseMemberService _courseMemberService;
        protected int StatusIsSync = (int)UtilConstants.ApiStatus.Synchronize;
        protected int StatusModify = (int)UtilConstants.ApiStatus.Modify;
        protected int StatusUnSuccessfully = (int)UtilConstants.ApiStatus.UnSuccessfully;
        protected string keyCodeStart = UtilConstants.KEY_CORE_START;
        protected string keyCodeEnd = UtilConstants.KEY_CORE_END;
        public LMSController(IApproveService repoTmsApproves, ICourseService courseService, ICourseDetailService courseDetailService, IUserContext userContext, IConfigService configService, IDepartmentService departmentService, ISubjectService repoSubject, IEmployeeService EmployeeService, ICourseResultSummaryService repoCourseResultSummaryService, INotificationService NotificationService, ICourseMemberService courseMemberService)
        {
            _repoTmsApproves = repoTmsApproves;
            _CourseService = courseService;
            _CourseDetailService = courseDetailService;
            _userContext = userContext;
            _configService = configService;
            _departmentService = departmentService;
            _repoSubject = repoSubject;
            _EmployeeService = EmployeeService;
            _repoCourseResultSummaryService = repoCourseResultSummaryService;
            _NotificationService = NotificationService;
            _courseMemberService = courseMemberService;
        }


        // GET: LMS
        public ActionResult Index()
        {


            return View();
        }

        private bool Login(string username)
        {
            var usernamelower = username.ToLower();
            USER userLogin = null;
            userLogin = _userContext.GetByUser(usernamelower);
            if (userLogin != null)
            {
                //var configsite = _configService.Get();
                var userModel = new UserModel(userLogin, null);

                if (userModel.IsMaster)
                {
                    var userPermissions = _departmentService.Get(a => a.IsDeleted != true && a.IsActive == true).Select(a => (int)a.Id);
                    // var userPermissions = datauser.UserPermissions.Select(a => a.DepartmentId).ToList();
                    userModel.PermissionIds = userPermissions.ToList();
                }

                if (userModel.IsActive == (int)UtilConstants.UserActive.On)
                {
                    Session.Clear();

                    Session["UserA"] = userModel;
                    string timeout = ConfigurationManager.AppSettings["SessionTimeout"];
                    Session.Timeout = int.Parse(timeout);

                    System.Web.HttpCookie cookie = Request.Cookies["EPOA_culture"];
                    if (cookie != null)
                    {
                        userModel.LanguageAbbreviation = cookie.Value;
                        Session["Lang"] = cookie.Value;
                    }


                    //userLogin.ONLINEAT = DateTime.Now;
                    //// userLogin.LASTONLINEAT = DateTime.Now;
                    //userLogin.USERSTATE = (int)UtilConstants.UserStateConstant.Online;

                    ////return so lan dang nhap = 0
                    //userLogin.Attempts = 0;

                    //_userContext.Update(userLogin);

                }
                return true;
            }
            return false;
        }
        protected UserModel GetUser()
        {
            var userModel = System.Web.HttpContext.Current.Session?["UserA"];

            return (UserModel)userModel;
        }
        private UserModel _currentUser = null;
        protected UserModel CurrentUser
        {
            get
            {
                if (_currentUser == null) _currentUser = GetUser();
                return _currentUser;
            }
        }
        public class CourseResultLMS
        {
            public string TraineeCode { get; set; }
            public string Score { get; set; }
            public string Time { get; set; }
            public string CourseDetailId { get; set; }

        }
        public ActionResult PluginInPutresult(string k)
        {
            try
            {
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                //var username = k;
                //var idCouresDetails = 27300;
                var paramUrl = TMS.Core.Utils.Common.DecryptURL(k);
                var arr = paramUrl.Split(new string[] { "uio" }, StringSplitOptions.None);
                if (arr.Length != 3)
                {
                    return Redirect("~/Redirect/ErrorPage");
                }
                var idCouresDetails = int.Parse(arr[0].ToString());
                var username = arr[1].ToString();
                if (username.ToLower() == "admin" || username.ToLower() == "administrator")
                {
                    username = "administrator";
                }
                if (Login(username) == false)
                {
                    return Redirect("~/Redirect/ErrorPage");
                }


                var dataCourseProccess = _repoTmsApproves.Get(a => a.Course.IsDeleted != true && a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending), (int)UtilConstants.ApproveType.SubjectResult);

                var model = new CourseResultViewModel();
                model.ProcessStep = _repoTmsApproves.ProcessStep((int)UtilConstants.ApproveType.SubjectResult);
                var courseDetail = _CourseDetailService.GetById(idCouresDetails);
                model.TypeLearningId = courseDetail.type_leaning ?? (int)UtilConstants.LearningTypes.Offline;
                if (model.TypeLearningId == (int)UtilConstants.LearningTypes.Online)
                {
                    model.MarkType = (int)UtilConstants.MarkTypes.Auto;
                }
                else if (model.TypeLearningId == (int)UtilConstants.LearningTypes.Offline || model.TypeLearningId == (int)UtilConstants.LearningTypes.OfflineOnline)
                {
                    if (courseDetail.mark_type.HasValue)
                    {
                        if (courseDetail.mark_type == (int)UtilConstants.MarkTypes.Auto) // Auto không áp dụng cho khóa Pass/Fail
                        {
                            model.MarkType = courseDetail.SubjectDetail.IsAverageCalculate == true ? (int)UtilConstants.MarkTypes.Auto : (int)UtilConstants.MarkTypes.Manual;
                        }
                        else
                        {
                            model.MarkType = (int)UtilConstants.MarkTypes.Manual;
                        }
                    }
                    else
                    {
                        model.MarkType = model.TypeLearningId == (int)UtilConstants.LearningTypes.OfflineOnline ? (int)UtilConstants.MarkTypes.Auto : (int)UtilConstants.MarkTypes.Manual;
                    }
                }
                model.checkSuccessCron = false;
                #region Call service
                if (model.MarkType == (int)UtilConstants.MarkTypes.Auto)
                {
                    var type = UtilConstants.CRON_POST_RESULT + idCouresDetails;
                    var server = ConfigurationManager.AppSettings["API_LMS_SERVER"] ?? "";
                    var token = ConfigurationManager.AppSettings["API_LMS_TOKEN"] ?? "";
                    var function = ConfigurationManager.AppSettings["FUNCTION"] ?? "";
                    var moodlewsrestformat = ConfigurationManager.AppSettings["moodlewsrestformat"] ?? "";
                    var restClient = new RestClient(server);
                    var request = new RestRequest(Method.POST);
                    request.AddParameter("wstoken", token);

                    request.AddParameter("wsfunction", function);
                    request.AddParameter("moodlewsrestformat", moodlewsrestformat);
                    request.AddParameter("type", type);
                    var response = restClient.Execute(request);
                    var responseContent = response.Content;
                    var model_ = new List<CourseResultLMS>();
                    if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                    {
                        model.checkSuccessCron = true;
                    }
                }
                #endregion
                model.CourseId = (int)courseDetail.CourseId;
                model.CourseDetailId = idCouresDetails;
                model.SubjectDetailId = (int)courseDetail.SubjectDetailId;
                model.CourseCode = courseDetail.Course.Code;
                model.CourseName = courseDetail.Course.Name;
                model.SubjectCode = courseDetail.SubjectDetail.Code;
                model.SubjectName = courseDetail.SubjectDetail.Name;
                model.RoomName = courseDetail.Room?.str_Name;
                model.Duration = courseDetail.SubjectDetail.Duration.ToString();
                model.DateFromTo = courseDetail.dtm_time_from?.ToString("dd/MM/yyy") + " - " +
                courseDetail.dtm_time_to?.ToString("dd/MM/yyyy");
                model.MaxGrade = courseDetail.Course?.MaxGrade == null ? "100" : courseDetail.Course?.MaxGrade.ToString();
                var instructorAbility = courseDetail.Course_Detail_Instructor.ToList();
                var instructors = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor);
                var hannah = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Hannah);
                var mentor = instructorAbility.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Mentor);
                model.Instructors = instructors.Select(b => ReturnDisplayLanguageCustom(b.Trainee.FirstName, b.Trainee.LastName, null, b.Duration.HasValue ? b.Duration.Value : 0)).Aggregate(model.Instructors, (current, fullName) => current + (fullName + "<br />"));
                model.Hannah = hannah.Select(b => ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName)).Aggregate(model.Hannah, (current, fullName) => current + (fullName + "<br />"));
                model.Mentor = mentor.Select(b => ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName)).Aggregate(model.Mentor, (current, fullName) => current + (fullName + "<br />"));
                //iscaculate
                model.IsCalculate = (bool)courseDetail.SubjectDetail.IsAverageCalculate;
                model.IsApproved =
                    dataCourseProccess.Any(
                        a =>
                            a.int_courseDetails_Id == idCouresDetails &&
                            (a.int_id_status == (int)UtilConstants.EStatus.Approve || a.int_id_status == (int)UtilConstants.EStatus.Pending));
                model.Members = courseDetail.TMS_Course_Member.Where(a => a.IsActive == true && a.IsDelete != true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved)).Select(a => new CourseResultViewModel.Member()
                {
                    Id = a.Id,

                    TraineeId = a.Trainee?.Id,
                    Code = a.Trainee?.str_Staff_Id,
                    Name = ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName),
                    DepartmentCode = a.Trainee?.Department?.Code,
                    DepartmentName = a.Trainee?.Department?.Name,
                    LearningTime = a.Course_Detail?.dtm_time_from?.ToString(Resource.lbl_FORMAT_DATE) + " - " + a.Course_Detail?.dtm_time_to?.ToString(Resource.lbl_FORMAT_DATE),
                    Score = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Score?.ToString().Replace("-1", "0"),
                    Result = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Result,
                    Score_Re = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Score?.ToString().Replace(",", ".").Replace("-1", "0"),
                    Result_Re = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Result,
                    Score_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Score?.ToString().Replace(",", "."),
                    Result_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.First_Check_Result,
                    Score_Re_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Score?.ToString().Replace(",", "."),
                    Result_Re_temp = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Re_Check_Result,
                    Course_Result_Id = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Id,
                    RealReResult = a.Course_Detail?.Course_Result_Temp?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Score?.ToString(),
                    Remark = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Remark?.Replace("!!!!!", Environment.NewLine),
                    Type = a.Course_Detail?.Course_Result?.FirstOrDefault(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id)?.Type ?? false,
                    CheckFail = "<a title='Checking Fail'  href='javascript:void(0)' onclick='RemarkComment(" + a.Id + ",this)'><i class='fas fa-edit " + (a.Course_Detail?.Course_Result?.Any(b => b.CourseDetailId == a.Course_Details_Id && b.TraineeId == a.Member_Id && b.Type == true) == true ? "highlight" : "") + " ' aria-hidden='true' ></i></a>"
                ,
                    CheckBox = "<input onclick='CheckBoxClick(this)' type='CheckBox' id='" + a.Id + "' value='" + a.Id + "' />"
                });
                return View(model);
            }
            catch (Exception)
            {
                
                return Redirect("~/Redirect/ErrorPage");
            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> ReSync(int? idCouresDetails)
        {
            try
            {
                if (idCouresDetails.HasValue && idCouresDetails > 0)
                {
                    var type = UtilConstants.CRON_POST_RESULT + idCouresDetails;
                    var server = GetByKey("API_LMS_SERVER");
                    var token = GetByKey("API_LMS_TOKEN");
                    var function = GetByKey("FUNCTION");
                    var moodlewsrestformat = GetByKey("moodlewsrestformat") ?? "";
                    var restClient = new RestClient(server);
                    var request = new RestRequest(Method.POST);
                    request.AddParameter("wstoken", token);

                    request.AddParameter("wsfunction", function);
                    request.AddParameter("moodlewsrestformat", moodlewsrestformat);
                    request.AddParameter("type", type);
                    var response = restClient.Execute(request);
                    var responseContent = response.Content;
                    var model_ = new List<CourseResultLMS>();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {

                        return Json(new AjaxResponseViewModel { message = "Re-Sync Result Unsuccessfully !!!", result = false }, JsonRequestBehavior.AllowGet);
                    }
                    if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                    {
                        return Json(new AjaxResponseViewModel { message = "Re-Sync Result Successfully !!!", result = true }, JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(new AjaxResponseViewModel { message = "Re-Sync Result Unsuccessfully !!!", result = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel { message = ex.ToString(), result = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public string ReturnDisplayLanguage(string firstName, string lastName, string culture = null)
        {
            string fullName;
            //culture = "en";
            //HttpCookie cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            //if (cultureCookie != null)
            //{
            //    culture = cultureCookie.Value;
            //}
            //switch (culture)
            //{
            //    case "vi":
            //        fullName = firstName + " " + lastName;
            //        break;
            //    default:
            //        fullName = lastName + " " + firstName;
            //        break;
            //}
            fullName = lastName + " " + firstName;
            return fullName;
        }
        protected string ReturnDisplayLanguageCustom(string firstName, string lastName, string culture = null, double duration = 0)
        {
            string fullName;
            culture = "en";
            System.Web.HttpCookie cultureCookie = System.Web.HttpContext.Current.Request.Cookies["language"];
            if (cultureCookie != null)
            {
                //culture = GetByKey("DisplayLanguage");
                culture = cultureCookie.Value;
            }
            switch (culture)
            {
                case "vi":
                    fullName = firstName + " " + lastName + " - Duration (Hours) : " + duration;
                    break;
                default:
                    fullName = lastName + " " + firstName + " - Duration (Hours) : " + duration;
                    break;
            }
            return fullName;
        }
        protected string TypeLearningIcon(int? type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "<a href='javascript:void(0)'  data-toggle='tooltip' title='Class'><i class='fas fa-chalkboard-teacher'  style='color:green;'></i></a>";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "<a href='javascript:void(0)'  data-toggle='tooltip' title='E-Learning'><i class='fa fa-desktop' style='color:red;'></i></a>";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "<a href='javascript:void(0)'  data-toggle='tooltip' title='cRo'><i class='fas fa-book-reader' style='color:royalblue;'></i></a>";
                    break;
            }
            return str_type;
        }
        protected string TypeLearning(int type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "cR";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "eL";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "cRo";
                    break;
            }
            return str_type;
        }
        [HttpPost]
        public async Task<ActionResult> InsertResult(FormCollection form)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            try
            {
                #region [get value form]
                var user = GetUser();
                var memberId = form.GetValues("MemberId");
                var traineeIds = form.GetValues("TraineeId");
                var traineeCode = form.GetValues("TraineeCode");
                var traineeName = form.GetValues("TraineeName");
                var departmentCode = form.GetValues("DepartmentCode");
                var txtRemark = form.GetValues("txt_Remark");
                var countIngre = form.GetValues("CountIngre");
                var txtResult = form.GetValues("result");
                var txtFirstCheckResult = form.GetValues("First_Check_Result");
                var txtFirstCheckScore = form.GetValues("First_Check_Score");
                var txtReCheckResult = form.GetValues("Re_Check_Result");
                var txtReCheckScore = form.GetValues("Re_Check_Score");
                var courseId = string.IsNullOrEmpty(form["fCourseId"]) ? -1 : Convert.ToInt32(form["fCourseId"]);
                var courseDetailId = string.IsNullOrEmpty(form["fCourseDetailId"])
                    ? -1
                    : Convert.ToInt32(form["fCourseDetailId"]);
                var subjectDetailId = string.IsNullOrEmpty(form["fSubjectDetailId"])
                    ? -1
                    : Convert.ToInt32(form["fSubjectDetailId"]);
                var submitType = string.IsNullOrEmpty(form["fsubmitType"]) ? -1 : Convert.ToInt32(form["fsubmitType"]);
                //var note = string.IsNullOrEmpty(form["fNote"]) ? string.Empty : form["fNote"].Trim();
                var isCalculate = string.IsNullOrEmpty(form["fIsCalculate"])
                    ? -1
                    : Convert.ToInt32(form["fIsCalculate"]);
                var maxGade = string.IsNullOrEmpty(form["MaxGrade"])
                    ? -1
                    : double.Parse(form["MaxGrade"]);
                #endregion

                if (traineeIds != null)
                {
                    var i = 0;
                    foreach (var id in traineeIds)
                    {
                        var appSettings = form.AllKeys.Where(k => k.StartsWith("TraineeId_" + id + "_")).ToDictionary(k => k, k => form[k]);
                        foreach (var key in appSettings)
                        {
                            var separators = new[] { "_", "," };
                            var words = key.ToString().Split(separators, StringSplitOptions.RemoveEmptyEntries);
                            //  var traineeId = words;
                            var traineeId = int.Parse(words.GetValue(1).ToString());
                            var score = words.GetValue(3).ToString();


                            var result = string.Empty;
                            var remark = string.Empty;
                            //var newScore = txtResult[i];
                            var newFirstCheckResult = txtFirstCheckResult[i];
                            var newFirstCheckScore = txtFirstCheckScore[i];
                            if (!string.IsNullOrEmpty(newFirstCheckScore))
                            {
                                newFirstCheckScore = Regex.Replace(newFirstCheckScore, "[.,]", separator);
                            }
                            var newReCheckResult = txtReCheckResult[i];
                            var newReCheckScore = txtReCheckScore[i];
                            if (!string.IsNullOrEmpty(newReCheckScore))
                            {
                                newReCheckScore = Regex.Replace(newReCheckScore, "[.,]", separator);
                            }
                            #region Modify CourResult
                            var entity = _CourseService.GetCourseResult(traineeId, courseDetailId);
                            if (entity != null)
                            {
                                remark = txtRemark[i];
                                entity.Result = result;
                                if (!string.IsNullOrEmpty(newFirstCheckScore) && newFirstCheckScore != "-1")
                                {
                                    entity.First_Check_Score = !string.IsNullOrEmpty(newFirstCheckScore) ? Math.Round(double.Parse(newFirstCheckScore), 1) : -1;
                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newFirstCheckScore) ? double.Parse(newFirstCheckScore) : -1);
                                }
                                entity.Remark = remark;
                                if (!string.IsNullOrEmpty(newReCheckScore) && newReCheckScore != "-1")
                                {
                                    entity.Re_Check_Score = !string.IsNullOrEmpty(newReCheckScore) ? Math.Round(double.Parse(newReCheckScore), 1) : -1;
                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newReCheckScore) ? double.Parse(newReCheckScore) : -1);
                                }
                                entity.ModifiedAt = DateTime.Now;
                                if (!string.IsNullOrEmpty(newFirstCheckResult) && newFirstCheckResult != "-1")
                                {

                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : newFirstCheckResult;
                                }
                                entity.ModifiedBy = CurrentUser.USER_ID.ToString();
                                if (!string.IsNullOrEmpty(newReCheckResult) && newReCheckResult != "-1")
                                {

                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : newReCheckResult;
                                }
                                entity.inCourseMemberId = int.Parse(memberId[i]);
                                entity.IsDelete = false;
                                entity.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;
                                _CourseService.UpdateCourseResult(entity);
                            }
                            else
                            {
                                remark = txtRemark[i];
                                entity = new Course_Result()
                                {
                                    CourseDetailId = courseDetailId,
                                    TraineeId = traineeId,
                                    Result = result,
                                    Remark = remark,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = CurrentUser.USER_ID.ToString(),
                                    IsDelete = false,
                                    LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS
                                };
                                if (!string.IsNullOrEmpty(newFirstCheckScore) && newFirstCheckScore != "-1")
                                {
                                    entity.First_Check_Score = !string.IsNullOrEmpty(newFirstCheckScore) ? Math.Round(double.Parse(newFirstCheckScore), 1) : -1;
                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newFirstCheckScore) ? double.Parse(newFirstCheckScore) : -1);
                                }
                                else
                                {
                                    entity.First_Check_Score = -1;
                                    entity.First_Check_Result = "F";
                                }

                                if (!string.IsNullOrEmpty(newReCheckScore) && newReCheckScore != "-1")
                                {
                                    entity.Re_Check_Score = !string.IsNullOrEmpty(newReCheckScore) ? Math.Round(double.Parse(newReCheckScore), 1) : -1;
                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : GetGrade(subjectDetailId, !string.IsNullOrEmpty(newReCheckScore) ? double.Parse(newReCheckScore) : -1);
                                }
                                if (!string.IsNullOrEmpty(newFirstCheckResult) && newFirstCheckResult != "-1")
                                {

                                    entity.First_Check_Result = entity.Type == true
                                        ? "F"
                                        : newFirstCheckResult;
                                }
                                entity.ModifiedBy = CurrentUser.USER_ID.ToString();
                                if (!string.IsNullOrEmpty(newReCheckResult) && newReCheckResult != "-1")
                                {

                                    entity.Re_Check_Result = entity.Type == true
                                    ? "F"
                                    : newReCheckResult;
                                }
                                entity.inCourseMemberId = int.Parse(memberId[i]);
                                _CourseService.InsertCourseResult(entity);
                            }
                            #endregion

                            #region modify Course Summary

                            var entitySummary = _repoCourseResultSummaryService.GetById(traineeId, courseDetailId);
                            var subject = _repoSubject.GetSubjectDetailById(subjectDetailId);
                            if (entitySummary != null)
                            {
                                if (entity != null && subject?.IsAverageCalculate == true)
                                {
                                    if (entity.Re_Check_Score.HasValue)
                                    {
                                        entitySummary.point = entity.Re_Check_Score;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, (int)entity.Re_Check_Score);

                                    }
                                    else
                                    {
                                        entitySummary.point = entity.First_Check_Score.HasValue ? entity.First_Check_Score : -1;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, entity.First_Check_Score.HasValue ? (int)entity.First_Check_Score : -1);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(entity.Re_Check_Result))
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.Re_Check_Result == "F" ? "Fail" : "Pass";

                                    }
                                    else
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.First_Check_Result == "P" ? "Pass" : "Fail";
                                    }
                                }

                                entitySummary.Remark = remark;
                                if (isCalculate == (int)UtilConstants.BoolEnum.Yes)
                                {
                                    entitySummary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;//submitType == (int)UtilConstants.EStatus.Approve ? StatusIsSync : StatusModify;
                                }
                                else
                                {
                                    entitySummary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;//StatusIsSync;
                                }
                                _repoCourseResultSummaryService.Update(entitySummary);
                            }
                            else
                            {
                                entitySummary = new Course_Result_Summary()
                                {
                                    TraineeId = traineeId,
                                    CourseDetailId = courseDetailId,
                                };
                                if (entity != null && subject?.IsAverageCalculate == true)
                                {
                                    if (entity.Re_Check_Score.HasValue)
                                    {
                                        entitySummary.point = entity.Re_Check_Score;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, (int)entity.Re_Check_Score);

                                    }
                                    else
                                    {
                                        entitySummary.point = entity.First_Check_Score.HasValue ? entity.First_Check_Score : -1;
                                        entitySummary.Result = GetGradeSummary(subjectDetailId, entity.First_Check_Score.HasValue ? (int)entity.First_Check_Score : -1);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(entity.Re_Check_Result))
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.Re_Check_Result == "F" ? "Fail" : "Pass";

                                    }
                                    else
                                    {
                                        entitySummary.point = -1;
                                        entitySummary.Result = entity.First_Check_Result == "P" ? "Pass" : "Fail";
                                    }
                                }
                                entitySummary.Remark = remark;
                                entitySummary.LmsStatus = (int)UtilConstants.ApiStatus.AddNewTMS;// StatusModify;//submitType == (int)UtilConstants.EStatus.Approve ? StatusIsSync : StatusModify;
                                _repoCourseResultSummaryService.Insert(entitySummary);
                            }
                            #endregion
                            i++;

                        }

                    }
                }


                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                //LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/InsertResult", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        private string GetGrade(int subjectDetailId, double score = -1)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            var getsubjectDetails = _repoSubject.GetSubjectDetail(a => a.IsActive == true && a.Id == subjectDetailId).Select(a => a.Id);
            if (getsubjectDetails.Any())
            {
                var getsubjectScore = _repoSubject.GetScores(a => getsubjectDetails.Contains((int)a.subject_id)).OrderByDescending(a => a.point_from);
                if (getsubjectScore.Any())
                {
                    foreach (var item in getsubjectScore)
                    {
                        if (score >= item.point_from)
                        {
                            result = item.grade;
                            break;
                        }

                    }

                }
            }
            var _return = result.Contains(UtilConstants.Grade.Fail.ToString())
                ? "F"
                : "P";
            return _return;

        }
        private string GetGradeSummary(int subjectDetailId, double score = -1)
        {
            var result = UtilConstants.Grade.Fail.ToString();
            var getsubjectDetails = _repoSubject.GetSubjectDetail(a => a.IsActive == true && a.Id == subjectDetailId).Select(a => a.Id);
            if (getsubjectDetails.Any())
            {
                var getsubjectScore = _repoSubject.GetScores(a => getsubjectDetails.Contains((int)a.subject_id)).OrderByDescending(a => a.point_from);
                if (getsubjectScore.Any())
                {
                    foreach (var item in getsubjectScore)
                    {
                        if (score >= item.point_from)
                        {
                            result = item.grade;
                            break;
                        }

                    }

                }
                else
                {
                    result = string.Empty;
                }
            }

            return result;

        }
        protected void Modify_TMS(bool? isApprove, Course course, int type, int status, UtilConstants.ActionType actionType, string note = "", int? courseDetailId = -1, int? approveId = -1)
        {
            courseDetailId = courseDetailId ?? -1;
            var approveType = UtilConstants.ApproveType.Course;
            var eStatus = UtilConstants.EStatus.Pending;
            var notiTemplate = string.Empty;
            var notiTemplateVn = string.Empty;
            var notiContent = string.Empty;
            var notiContentVn = string.Empty;
            #region [Request]
            if (actionType == UtilConstants.ActionType.Request)
            {
                switch (type)
                {
                    case (int)UtilConstants.ApproveType.Course:
                        approveType = UtilConstants.ApproveType.Course;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_Course;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_Course_VN;
                        notiContent = UtilConstants.NotificationContent.Request_Course;
                        notiContentVn = UtilConstants.NotificationContent.Request_Course_VN;
                        break;
                    case (int)UtilConstants.ApproveType.AssignedTrainee:
                        approveType = UtilConstants.ApproveType.AssignedTrainee;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_AssignTrainee;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_AssignTrainee_VN;
                        notiContent = UtilConstants.NotificationContent.Request_AssignTrainee;
                        notiContentVn = UtilConstants.NotificationContent.Request_AssignTrainee_VN;
                        break;
                    case (int)UtilConstants.ApproveType.SubjectResult:
                        approveType = UtilConstants.ApproveType.SubjectResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_SubjectResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_SubjectResult_VN;
                        notiContent = UtilConstants.NotificationContent.Request_SubjectResult;
                        notiContentVn = UtilConstants.NotificationContent.Request_SubjectResult_VN;
                        break;
                    case (int)UtilConstants.ApproveType.CourseResult:
                        approveType = UtilConstants.ApproveType.CourseResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Request_CourseResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Request_CourseResult_VN;
                        notiContent = UtilConstants.NotificationContent.Request_CourseResult;
                        notiContentVn = UtilConstants.NotificationContent.Request_CourseResult_VN;
                        break;
                }
                if (status == (int)UtilConstants.EStatus.Approve)
                {
                    eStatus = UtilConstants.EStatus.Approve;
                }

                if (status == (int)UtilConstants.EStatus.CancelRequest)
                {
                    eStatus = UtilConstants.EStatus.CancelRequest;
                    notiTemplate = UtilConstants.NotificationTemplate.CancelRequest;
                    notiTemplateVn = UtilConstants.NotificationTemplate.CancelRequestVN;
                    notiContent = UtilConstants.NotificationContent.CancelRequest;
                    notiContentVn = UtilConstants.NotificationContent.CancelRequestVN;
                }
            }

            #endregion
            #region [Approve]

            if (actionType == UtilConstants.ActionType.Approve)
            {
                switch (type)
                {
                    case (int)UtilConstants.ApproveType.Course:
                        approveType = UtilConstants.ApproveType.Course;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_Course;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_Course_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_Course;
                        notiContentVn = UtilConstants.NotificationContent.Approval_Course_VN;
                        break;
                    case (int)UtilConstants.ApproveType.AssignedTrainee:
                        approveType = UtilConstants.ApproveType.AssignedTrainee;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_AssignTrainee;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_AssignTrainee_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_AssignTrainee;
                        notiContentVn = UtilConstants.NotificationContent.Approval_AssignTrainee_VN;
                        break;
                    case (int)UtilConstants.ApproveType.SubjectResult:
                        approveType = UtilConstants.ApproveType.SubjectResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_SubjectResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_SubjectResult_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_SubjectResult;
                        notiContentVn = UtilConstants.NotificationContent.Approval_SubjectResult_VN;
                        break;
                    case (int)UtilConstants.ApproveType.CourseResult:
                        approveType = UtilConstants.ApproveType.CourseResult;
                        notiTemplate = UtilConstants.NotificationTemplate.Approval_CourseResult;
                        notiTemplateVn = UtilConstants.NotificationTemplate.Approval_CourseResult_VN;
                        notiContent = UtilConstants.NotificationContent.Approval_CourseResult;
                        notiContentVn = UtilConstants.NotificationContent.Approval_CourseResult_VN;
                        break;
                }

                #region [status]
                switch (status)
                {
                    case (int)UtilConstants.EStatus.Pending:
                        eStatus = UtilConstants.EStatus.Pending;
                        break;
                    case (int)UtilConstants.EStatus.Approve:
                        eStatus = UtilConstants.EStatus.Approve;
                        break;
                    case (int)UtilConstants.EStatus.Reject:
                        eStatus = UtilConstants.EStatus.Reject;
                        break;
                    case (int)UtilConstants.EStatus.Block:
                        eStatus = UtilConstants.EStatus.Block;
                        break;
                }
                #endregion

                #region [Course]
                if (approveType == UtilConstants.ApproveType.Course && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockCourse;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockCourseVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockCourse;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockCourseVn;
                }
                if (approveType == UtilConstants.ApproveType.Course && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_Course;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_Course_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_Course;
                    notiContentVn = UtilConstants.NotificationContent.Reject_Course_VN;
                }
                #endregion
                #region [Assign]
                if (approveType == UtilConstants.ApproveType.AssignedTrainee && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockAssignTrainee;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockAssignTraineeVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockAssign;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockAssignVn;
                }
                if (approveType == UtilConstants.ApproveType.AssignedTrainee && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_AssignTrainee;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_AssignTrainee_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_AssignTrainee;
                    notiContentVn = UtilConstants.NotificationContent.Reject_AssignTrainee_VN;
                }
                #endregion
                #region [Subject]
                if (approveType == UtilConstants.ApproveType.SubjectResult && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockSubjectResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockSubjectResultVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockSubjectResult;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockSubjectResultVn;
                }
                if (approveType == UtilConstants.ApproveType.SubjectResult && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_SubjectResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_SubjectResult_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_SubjectResult;
                    notiContentVn = UtilConstants.NotificationContent.Reject_SubjectResult_VN;
                }
                #endregion
                #region [Final]
                if (approveType == UtilConstants.ApproveType.CourseResult && eStatus == UtilConstants.EStatus.Block)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.UnBlockFinal;
                    notiTemplateVn = UtilConstants.NotificationTemplate.UnBlockFinalVn;
                    notiContent = UtilConstants.NotificationContent.UnBlockFinal;
                    notiContentVn = UtilConstants.NotificationContent.UnBlockFinalVn;
                }
                if (approveType == UtilConstants.ApproveType.CourseResult && eStatus == UtilConstants.EStatus.Reject)
                {
                    notiTemplate = UtilConstants.NotificationTemplate.Reject_CourseResult;
                    notiTemplateVn = UtilConstants.NotificationTemplate.Reject_CourseResult_VN;
                    notiContent = UtilConstants.NotificationContent.Reject_CourseResult;
                    notiContentVn = UtilConstants.NotificationContent.Reject_CourseResult_VN;
                }
                #endregion


            }
            #endregion


            var approve = _repoTmsApproves.Modify(false, course, approveType, eStatus, actionType, courseDetailId, note);
            if (approve == null) throw new Exception(Messege.WARNING_SENT_REQUEST_ERROR);
            var approver = _repoTmsApproves.GetApprover(approveType);
            //if(approver == null) throw new Exception(Messege.WARNING_NOT_FOUND_APPROVER);
            var approverId = approver?.UserId ?? 7; // admin = 7
            var toUser = actionType == UtilConstants.ActionType.Request ? approverId : approve.int_Requested_by;

            SendNotification((int)UtilConstants.NotificationType.AutoProcess, (int)approveType, approve.id, toUser, DateTime.Now, notiTemplate,
           (
           approveType == UtilConstants.ApproveType.SubjectResult
           ? string.Format(notiContent, approve.Course_Detail.SubjectDetail.Code, approve.Course.Code + " - " + approve.Course.Name, note)
           : string.Format(notiContent, approve.Course.Code + " - " + approve.Course.Name, note)),
           notiTemplateVn,
           (
           approveType == UtilConstants.ApproveType.SubjectResult
           ? string.Format(notiContentVn, approve.Course_Detail.SubjectDetail.Code, approve.Course.Code + " - " + approve.Course.Name, note)
           : string.Format(notiContentVn, approve.Course.Code + " - " + approve.Course.Name, note)));

            #region [Send email GV]

            var checkcsendEmail = _configService.GetByKey("SENDEMAILGV");
            if (checkcsendEmail.Equals("1") && actionType == UtilConstants.ActionType.Request)
            {
                var currentUser = CurrentUser;
                var tmsApprove = course.TMS_APPROVES?.FirstOrDefault(a => a.int_Type == type);

                Sent_Email_TMS(null, null, approver?.USER, course, null, currentUser.USER_ID, type, false, tmsApprove, (int)UtilConstants.ActionTypeSentmail.SendMailApprove);
            }
            #endregion


        }
        protected bool CallServices(string type)
        {
            var server = _configService.GetByKey("API_LMS_SERVER");
            var token = _configService.GetByKey("API_LMS_TOKEN");
            var function = _configService.GetByKey("FUNCTION");
            var moodlewsrestformat = _configService.GetByKey("moodlewsrestformat") ?? "";
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);

            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            request.AddParameter("type", type);
            var response = restClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {

                return false;
            }

            var responseContent = response.Content;
            if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
            {
                return true;
            }
            return false;
        }
        #region [------------------SENT EMAIL------------------]
        protected void Sent_Email_TMS(Trainee instructor, Trainee trainee, USER user, Course course, Course_Detail_Instructor details, int? int_Requested_by, int? actionType = -1, bool? LMSAssign = false, TMS_APPROVES tmsApprove = null, int? sendApproveMail = -1)
        {//, UtilConstants.EStatus status
            #region [CodeNEw]

            var body_Ins = string.Empty;
            var mail_receiver = string.Empty;
            var TypeSentEmail = -1;
            //var checkHANNAH = CheckSiteConfig(UtilConstants.KEY_HANNAH);
            //var UserHannal = checkHANNAH ? UserContext.Get(a => a.UserRoles.Any(c => c.RoleId == 9) && !a.IsDeleted && a.ISACTIVE == 1, GetUser().PermissionIds).ToDictionary(a => a.ID, a => ReturnDisplayLanguage(a.FIRSTNAME, a.LASTNAME)) : null;
            var checkHANNAH = _configService.GetByKey("HANNAH");
            var UserHannal = checkHANNAH.Equals("1")
                ? _EmployeeService.Get(a => a.Trainee_Type.Any(b => b.Type == (int)UtilConstants.TypeInstructor.Hannah))
                    .ToDictionary(a => a.Id, a => ReturnDisplayLanguage(a.FirstName, a.LastName))
                : null;

            #region [------Approve---------]
            #region [ApproveCourse]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor--------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedGV;
                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Mentor:

                            #region [--------------------------Mentor------------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedTeachingAssistant;


                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Hannah:

                            #region [--------------------------Hannah------------------------------------]
                            if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            {
                                body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedHannal, null, instructor, course);
                                mail_receiver = instructor.str_Email;
                                TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal;


                            }
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedHannal_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedHannal_User;


                            //}
                            break;
                            #endregion

                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    var user_create = _userContext.GetById((int)int_Requested_by);
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApprovedCourse, user_create, null, course);
                    mail_receiver = user_create.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApprovedCourse;

                }
            }
            #endregion

            #region [Assign Trainee]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.AssignTrainee)
            {

                if (trainee != null && course != null)
                {
                    if (LMSAssign == false)
                    {
                        body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTrainee, null, trainee, course);
                        mail_receiver = trainee.str_Email;
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTrainee;

                    }
                    else
                    {
                        body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS, null, trainee, course);
                        mail_receiver = trainee.str_Email;
                        TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailAssignTraineeLMS;


                    }

                }
            }
            #endregion

            #region [Approved Final Program]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.ApprovedFinalProgram)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:

                            #region [--------------------------Instructor-------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalGV;

                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Mentor:

                            #region [--------------------------Mentor------------------------------------]
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalMantor, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalMantor;


                            break;
                        #endregion

                        case (int)UtilConstants.TypeInstructor.Hannah:

                            #region [--------------------------Hannah------------------------------------]
                            if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            {
                                body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalHannah, null, instructor, course);
                                mail_receiver = instructor.str_Email;
                                TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah;

                            }
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalHannah_User;


                            //}
                            break;
                            #endregion

                    }
                }

                if (trainee != null && course != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailApproveFinalCourse, null, trainee, course);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailApproveFinalCourse;


                }
            }

            #endregion
            #endregion

            #region [Create Password]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordUser)
            {
                if (user != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordUser, user, null, null);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordUser;


                }
            }
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CreatePasswordEmployee)
            {
                if (instructor != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, instructor, null);
                    mail_receiver = instructor.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;


                }
                if (trainee != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailPasswordEmp, null, trainee, null);
                    mail_receiver = trainee.str_Email;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailPasswordEmp;

                }
            }
            #endregion


            #region [------Reject---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.Reject)
            {
                if (instructor != null && course != null)
                {
                    switch (details.Type)
                    {
                        case (int)UtilConstants.TypeInstructor.Instructor:
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectGV, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectGV;


                            break;
                        case (int)UtilConstants.TypeInstructor.Mentor:
                            body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectMantor, null, instructor, course);
                            mail_receiver = instructor.str_Email;
                            TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectMantor;


                            break;
                        case (int)UtilConstants.TypeInstructor.Hannah:
                            if (!UserHannal.Any(a => a.Key == details.Instructor_Id))
                            {
                                body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectHannah, null, instructor, course);
                                mail_receiver = instructor.str_Email;
                                TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectHannah;


                            }
                            //else
                            //{
                            //    var userId = UserContext.GetById((int)details.Instructor_Id);
                            //    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectHannah_User, userId, null, course);
                            //    mail_receiver = userId.EMAIL;
                            //    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectHannah_User;

                            //}
                            break;
                    }
                }

                if (course != null && int_Requested_by.HasValue)
                {
                    var user_create = _userContext.GetById((int)int_Requested_by);
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailRejectCourse, user_create, null, course);
                    mail_receiver = user_create.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailRejectCourse;

                }
            }
            #endregion

            #region [------Cancel Request---------]
            if (actionType == (int)UtilConstants.ActionTypeSentmail.CancelRequest)
            {
                if (user != null && course != null)
                {
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SentMailCancelRequest, user, null, course);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SentMailCancelRequest;

                }

            }
            #endregion
            #region [Send Email Approve]
            if (sendApproveMail == (int)UtilConstants.ActionTypeSentmail.SendMailApprove)
            {
                if (user != null && course != null)
                {
                    // type lấy khi nào config bật , ko thì lấy UtilConstants.ActionTypeSentmail
                    body_Ins = BodySendMail(UtilConstants.TypeSentEmail.SendMailApproveToMail, user, null, course, tmsApprove, actionType);
                    mail_receiver = user.EMAIL;
                    TypeSentEmail = (int)UtilConstants.TypeSentEmail.SendMailApproveToMail;

                }

            }
            #endregion

            if (!string.IsNullOrEmpty(mail_receiver) && TypeSentEmail != -1 && !string.IsNullOrEmpty(body_Ins))
            {
                InsertSentMail(mail_receiver, TypeSentEmail, body_Ins, course?.Id);
            }
            #endregion

        }
        protected TMS_SentEmail InsertSentMail(string mail_receiver, int typeSentMail, string bodySentMail, int? courseId, string subjectName = "")
        {
            try
            {
                var entity = new TMS_SentEmail();

                entity.mail_receiver = mail_receiver;
                entity.type_sent = typeSentMail;
                entity.content_body = bodySentMail;
                entity.flag_sent = 0;
                entity.cat_mail_ID = _configService.GetMail(a => a.Type == typeSentMail)?.FirstOrDefault()?.ID;
                entity.id_course = courseId;
                entity.Is_Deleted = false;
                entity.Is_Active = true;
                // entity.subjectname = subjectName;

                _CourseService.InsertSentMail(entity);

                return entity;
            }
            catch (Exception ex)
            {
                // LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogSourse.SendMail, UtilConstants.LogEvent.Insert, ex.Message);
                return null;
            }

        }

        #endregion
        protected string BodySendMail(UtilConstants.TypeSentEmail cAT_MAIL, USER uSER = null, Trainee trainee = null, Course Program = null, TMS_APPROVES tmsApprove = null, int? type = -1)
        {
            var body = _configService.GetMail(a => a.Type == (int)cAT_MAIL).FirstOrDefault()?.Content;
            if (body != null)
            {
                if (uSER != null)
                {
                    var pass = uSER.PASSWORD != null ? TMS.Core.Utils.Common.DecryptString(uSER.PASSWORD) : "";
                    body = body.Replace(UtilConstants.MAIL_USER_USERNAME, uSER.USERNAME)
                        //.Replace(UtilConstants.MAIL_USER_FULLNAME, uSER.FIRSTNAME.Trim() + " " + uSER.LASTNAME.Trim())
                        .Replace(UtilConstants.MAIL_USER_FULLNAME, ReturnDisplayLanguage(uSER.FIRSTNAME.Trim(), uSER.LASTNAME.Trim()))

                        .Replace(UtilConstants.MAIL_USER_PASSWORD, pass)
                        .Replace(UtilConstants.MAIL_USER_EMAIL, uSER.EMAIL)
                        .Replace(UtilConstants.MAIL_USER_PHONE, uSER.PHONENO);
                }
                if (trainee != null)
                {
                    int? grade;
                    if (Program != null)
                    {
                        grade = trainee.Course_Result_Final?.Where(a => a.courseid == Program.Id).FirstOrDefault()?.grade;
                    }
                    else
                    {
                        grade = 0;
                    }
                    var pass = trainee.Password != null ? TMS.Core.Utils.Common.DecryptString(trainee?.Password) : "";
                    body = body.Replace(UtilConstants.MAIL_TRAINEE_USERNAME, trainee.str_Staff_Id)
                       .Replace(UtilConstants.MAIL_TRAINEE_PASSWORD, pass)
                       //.Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, trainee.FirstName + " " + trainee.LastName)
                       .Replace(UtilConstants.MAIL_TRAINEE_FULLNAME, (cAT_MAIL == UtilConstants.TypeSentEmail.SendMailApproveToMail ? ReturnDisplayLanguage(uSER.FIRSTNAME, uSER.LASTNAME) : ReturnDisplayLanguage(trainee.FirstName, trainee.LastName)))
                       .Replace(UtilConstants.MAIL_TRAINEE_EMAIL, trainee.str_Email)
                       .Replace(UtilConstants.MAIL_TRAINEE_PHONE, trainee.str_Cell_Phone)
                       .Replace(UtilConstants.MAIL_TRAINEE_GRADE, GetGrace(grade));
                }
                if (Program != null)
                {
                    var strKey = "";
                    if (tmsApprove != null)
                    {
                        strKey = "id=" + tmsApprove.id + "&type=" + tmsApprove.int_Type;

                    }
                    body = body.Replace(UtilConstants.MAIL_PROGRAM_NAME, Program.Name)
                       .Replace(UtilConstants.MAIL_PROGRAM_CODE, Program.Code)
                       .Replace(UtilConstants.MAIL_PROGRAM_STARTDATE, Program.StartDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_ENDDATE, Program.EndDate?.ToString("dd/MM/yyyy"))
                       .Replace(UtilConstants.MAIL_PROGRAM_VENUE, Program.Venue)
                       .Replace(UtilConstants.MAIL_PROGRAM_MAXTRAINEE, Program.NumberOfTrainee.ToString())
                       .Replace(UtilConstants.MAIL_PROGRAM_NOTE, Program.Note)
                       .Replace(UtilConstants.MAIL_CODE, EncryptKeyEmail(strKey));

                    var itemcourse = "";
                    var listCourse = Program.Course_Detail.Where(a => a.IsDeleted == false && a.IsActive == true);
                    if (listCourse.Count() > 0)
                    {
                        var count = 0;
                        itemcourse = "<table border='1' cellpadding='1' cellspacing='1' stype='width:500px;'";
                        itemcourse += "<tbody>";
                        itemcourse += "<tr>";
                        itemcourse += "<th>" + @Resource.lblStt + "</th>";
                        itemcourse += "<th>" + @Resource.lblCode + "</th>";
                        itemcourse += "<th>" + @Resource.lblName + "</th>";
                        itemcourse += "<th>" + @Resource.lblStartDate + "</th>";
                        itemcourse += "<th>" + @Resource.lblEndDate + "</th>";
                        itemcourse += "<th>" + @Resource.lblMethod + "</th>";
                        itemcourse += "<th>" + @Resource.lblRoom + "</th>";
                        itemcourse += "<th>" + @Resource.lblInstructor + "</th>";
                        itemcourse += "</tr>";
                        foreach (var item in listCourse)
                        {
                            var instructor = "";
                            var InstructorIds = item.Course_Detail_Instructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(a => a.Instructor_Id).ToList();
                            if (InstructorIds.Any())
                            {
                                foreach (var teacherId in InstructorIds)
                                {
                                    var teacher = _EmployeeService.GetById(teacherId);
                                    if (teacher == null) continue;
                                    var fullName = ReturnDisplayLanguage(teacher.FirstName, teacher.LastName) + "<br />";
                                    instructor += fullName;
                                }
                            }


                            //var dbInstructor = item.Course_Detail_Instructor.ToList();
                            //if (dbInstructor.Any())
                            //{
                            //    //instructor = dbInstructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(b => b.Trainee.FirstName + " " + b.Trainee.LastName).Aggregate(instructor, (current, fullName) => current + (fullName + "<br />"));
                            //    instructor = dbInstructor.Where(a => a.Type == (int)UtilConstants.TypeInstructor.Instructor).Select(b => ReturnDisplayLanguage(b.Trainee.FirstName, b.Trainee.LastName)).Aggregate(instructor, (current, fullName) => current + (fullName + "<br />"));
                            //}
                            count++;
                            itemcourse += "<tr>";
                            itemcourse += "<td>" + count + "</td>";
                            itemcourse += "<td>" + item.SubjectDetail.Code + "</td>";
                            itemcourse += "<td>" + item.SubjectDetail.Name + "</td>";
                            itemcourse += "<td>" + item.dtm_time_from?.ToString("dd/MM/yyyy") + "<br />" + (item.time_from.Substring(0, 2) + "" + item.time_from.Substring(2)) + "</td>";
                            itemcourse += "<td>" + item.dtm_time_to?.ToString("dd/MM/yyyy") + "<br />" + (item.time_to.Substring(0, 2) + "" + item.time_to.Substring(2)) + "</td>";
                            itemcourse += "<td>" + TypeLearningName((int)item.type_leaning) + "</td>";
                            itemcourse += "<td>" + (item.Room == null ? "" : item.Room.str_Name) + "</td>";
                            itemcourse += "<td>" + instructor + "</td>";
                            itemcourse += "</tr>";
                        }
                        itemcourse += "</tbody>";
                        itemcourse += "</table>";

                    }
                    body = body.Replace(UtilConstants.MAIL_LIST_COURSE, itemcourse);
                }

            }
            return body;
        }
        protected void SendNotification(UtilConstants.NotificationType type, int? typelog, int? idApproval, int? to, DateTime? datesend, string messenge, string messengeContent, string messengeVn, string messengeContentVn)
        {
            _NotificationService.Notification_Insert((int)type, typelog, idApproval, to ?? -1, datesend, messenge, messengeContent, messengeVn, messengeContentVn);

        }
        protected string GetGrace(int? grade)
        {
            var strGrade = "";
            switch (grade)
            {
                case (int)UtilConstants.Grade.Fail:
                    strGrade = "Fail";
                    break;
                case (int)UtilConstants.Grade.Pass:
                    strGrade = "Pass";
                    break;
                case (int)UtilConstants.Grade.Distinction:
                    strGrade = "Distinction";
                    break;
            }
            return strGrade;
        }
        #region [EncryptKeyEmail]
        private string EncryptKeyEmail(string input)
        {
            var _return = "";
            foreach (char c in input)
            {
                switch (c.ToString())
                {
                    case "q": _return += "="; break;
                    case "w": _return += ";"; break;
                    case "e": _return += "0"; break;
                    case "r": _return += "1"; break;
                    case "t": _return += "2"; break;
                    case "y": _return += "3"; break;
                    case "u": _return += "4"; break;
                    case "i": _return += "9"; break;
                    case "o": _return += "8"; break;
                    case "p": _return += "7"; break;
                    case "a": _return += "6"; break;
                    case "s": _return += "5"; break;
                    case "d": _return += "M"; break;
                    case "f": _return += "n"; break;
                    case "g": _return += "B"; break;
                    case "h": _return += "v"; break;
                    case "j": _return += "c"; break;
                    case "k": _return += "x"; break;
                    case "l": _return += "Z"; break;
                    case "z": _return += "l"; break;
                    case "x": _return += "k"; break;
                    case "c": _return += "j"; break;
                    case "v": _return += "h"; break;
                    case "b": _return += "g"; break;
                    case "n": _return += "F"; break;
                    case "m": _return += "d"; break;
                    case "1": _return += "s"; break;
                    case "2": _return += "a"; break;
                    case "3": _return += "q"; break;
                    case "4": _return += "W"; break;
                    case "5": _return += "e"; break;
                    case "6": _return += "r"; break;
                    case "7": _return += "T"; break;
                    case "8": _return += "y"; break;
                    case "9": _return += "u"; break;
                    case "0": _return += "i"; break;
                    case "=": _return += "O"; break;
                    case ";": _return += "P"; break;
                    default: _return += c.ToString(); break;
                }
            }
            return keyCodeStart + _return.Trim() + keyCodeEnd;
        }
        #endregion
        protected string TypeLearningName(int type)
        {
            var str_type = "";
            switch (type)
            {
                case (int)UtilConstants.LearningTypes.Offline:
                    str_type = "cR";
                    break;
                case (int)UtilConstants.LearningTypes.Online:
                    str_type = "eL";
                    break;
                case (int)UtilConstants.LearningTypes.OfflineOnline:
                    str_type = "cRo";
                    break;
            }
            return str_type;
        }

        public ActionResult RemarkCommentMany(string listid)
        {
            var list = listid.Split(new char[] { ',' }).Select(p => Convert.ToInt32(p)).ToList();
            var Model = new List<RemarkComment>();
            foreach (var item in list)
            {
                var courseMemberId = item;
                var courseMember = _courseMemberService.GetById(courseMemberId);
                if (courseMember == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.ISVALID_DATA
                    }, JsonRequestBehavior.AllowGet);
                }
                var courseResult =
               _CourseService.GetCourseResult(
                   a => a.CourseDetailId == courseMember.Course_Details_Id && a.TraineeId == courseMember.Member_Id)?.FirstOrDefault();
                var fullName = ReturnDisplayLanguage(courseMember.Trainee.FirstName,
              courseMember.Trainee.LastName);
                var model = new RemarkComment
                {
                    FullName = fullName,
                    EId = courseMember.Trainee.str_Staff_Id,
                    Id = courseMember.Id,
                    CourseDetailId = courseMember.Course_Details_Id ?? -1,
                    TraineeId = courseMember.Member_Id ?? -1,
                    Remark = courseResult?.CourseRemarkCheckFails?.FirstOrDefault()?.RemarkContent ?? "Incomplete",
                    Result = courseResult?.Result ?? string.Empty,
                    Type = courseResult?.Type ?? true
                };
                Model.Add(model);
            }
            return PartialView("_Partials/_ReMarkCommentMany", Model);
        }
        [HttpPost]
        public ActionResult RemarkCommentMany(List<RemarkComment> model)
        {
            var listsuccess = new List<int>();
            var listfail = new List<int>();
            foreach (var item in model)
            {
                if (item.Type && string.IsNullOrEmpty(item.Remark))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.VALIDATION_REMARKCOMMENT_REMARK
                    }, JsonRequestBehavior.AllowGet);
                }


            }
            foreach (var item in model)
            {
                try
                {

                    var tmsMember = _courseMemberService.Get(a =>
                        a.Course_Details_Id == item.CourseDetailId && a.Member_Id == item.TraineeId &&
                        a.IsActive == true && a.IsDelete != true).FirstOrDefault();
                    if (tmsMember != null)
                    {
                        var courseResult =
                            _CourseService.GetCourseResult(
                                a => a.CourseDetailId == item.CourseDetailId && a.TraineeId == item.TraineeId).ToList();
                        if (courseResult.Any())
                        {
                            foreach (var result in courseResult)
                            {
                                //result.Type = item.Type;
                                ////result.Remark = model.Remark;
                                //result.Result = item.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty;
                                //result.ModifiedBy = CurrentUser.USER_ID.ToString();
                                //result.ModifiedAt = DateTime.Now;
                                //result.inCourseMemberId = tmsMember.Id;
                                //_CourseService.UpdateCourseResult(result);

                                Dictionary<string, object> dic_result = new Dictionary<string, object>();
                                dic_result.Add("Type", item.Type ? 1 : 0);
                                dic_result.Add("Result", item.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty);
                                dic_result.Add("ModifiedBy", CurrentUser.USER_ID + "");
                                dic_result.Add("ModifiedAt", DateTime.Now);
                                dic_result.Add("inCourseMemberId", tmsMember.Id);
                                if (CMSUtils.UpdateDataSQLNoLog("Id", result.Id + "", "Course_Result", dic_result.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_result.Values.ToArray())) > 0)
                                {

                                }

                                var remarkresult = _CourseService.GetCourseResultCheckFail(a => a.CourseResultID == result.Id);
                                if (remarkresult.Any())
                                {
                                    var recordremarkitem = remarkresult.FirstOrDefault();
                                    //recordremarkitem.RemarkContent = item.Remark;
                                    //recordremarkitem.CreatedAt = DateTime.Now;
                                    //_CourseService.UpdateCourseResultCheckFail(recordremarkitem);

                                    Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                    dic_remark.Add("RemarkContent", item.Remark);
                                    dic_remark.Add("CreatedAt", DateTime.Now);
                                    if (CMSUtils.UpdateDataSQLNoLog("Id", recordremarkitem.Id + "", "CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                    {

                                    }
                                }
                                else
                                {
                                    //var remark = new CourseRemarkCheckFail();
                                    //remark.CourseResultID = result.Id;
                                    //remark.RemarkContent = item.Remark;
                                    //remark.CreatedAt = DateTime.Now;
                                    //_CourseService.InsertCourseResultCheckFail(remark);

                                    Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                    dic_remark.Add("CourseResultID", result.Id);
                                    dic_remark.Add("RemarkContent", item.Remark);
                                    dic_remark.Add("CreatedAt", DateTime.Now);
                                    if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                    {

                                    }
                                }

                                if (item.Type == true)
                                {
                                    listsuccess.Add(item.Id);
                                }
                                else
                                {
                                    listfail.Add(item.Id);
                                }
                            }
                        }
                        else
                        {
                            var entity = new Course_Result
                            {
                                TraineeId = item.TraineeId,
                                CourseDetailId = item.CourseDetailId,
                                Score = item.Score ?? 0,
                                CreatedAt = DateTime.Now,
                                CreatedBy = CurrentUser.USER_ID.ToString(),
                                Type = item.Type,
                                //Remark = model.Remark,
                                Result = item.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty,
                                inCourseMemberId = tmsMember.Id
                            };
                            _CourseService.InsertCourseResult(entity);

                            //var remark = new CourseRemarkCheckFail();
                            //remark.CourseResultID = entity.Id;
                            //remark.RemarkContent = item.Remark;
                            //remark.CreatedAt = DateTime.Now;
                            //_CourseService.InsertCourseResultCheckFail(remark);

                            Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                            dic_remark.Add("CourseResultID", entity.Id);
                            dic_remark.Add("RemarkContent", item.Remark);
                            dic_remark.Add("CreatedAt", DateTime.Now);
                            if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                            {

                            }
                            if (item.Type == true)
                            {
                                listsuccess.Add(item.Id);
                            }
                            else
                            {
                                listfail.Add(item.Id);
                            }
                        }



                    }

                }
                catch (Exception ex)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.UNSUCCESS + ": " + ex,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                result = true,
                data = string.Join(",", listsuccess),
                data1 = string.Join(",", listfail),
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemarkComment(string id)
        {
            var courseMemberId = int.Parse(id);
            var courseMember = _courseMemberService.GetById(courseMemberId);

            if (courseMember == null)
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.ISVALID_DATA
                }, JsonRequestBehavior.AllowGet);
            }

            var courseResult =
                _CourseService.GetCourseResult(
                    a => a.CourseDetailId == courseMember.Course_Details_Id && a.TraineeId == courseMember.Member_Id)?.FirstOrDefault();

            //var fullName = courseMember.Trainee.FirstName.Trim() + " " + courseMember.Trainee.LastName.Trim();
            var fullName = ReturnDisplayLanguage(courseMember.Trainee.FirstName,
                courseMember.Trainee.LastName);
            var model = new RemarkComment
            {
                FullName = fullName,
                EId = courseMember.Trainee.str_Staff_Id,
                Id = courseMember.Id,
                CourseDetailId = courseMember.Course_Details_Id ?? -1,
                TraineeId = courseMember.Member_Id ?? -1,
                Remark = courseResult?.CourseRemarkCheckFails?.FirstOrDefault()?.RemarkContent ?? "Incomplete",
                Result = courseResult?.Result ?? string.Empty,
                Type = courseResult?.Type ?? true
            };
            return PartialView("_Partials/_ReMarkComment", model);
        }
        [HttpPost]
        public ActionResult RemarkComment(RemarkComment model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            if (model.Type && string.IsNullOrEmpty(model.Remark))
            {
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.VALIDATION_REMARKCOMMENT_REMARK
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {

                var tmsMember = _courseMemberService.Get(a =>
                    a.Course_Details_Id == model.CourseDetailId && a.Member_Id == model.TraineeId &&
                    a.IsActive == true && a.IsDelete != true).FirstOrDefault();
                if (tmsMember != null)
                {
                    var courseResult =
                        _CourseService.GetCourseResult(
                            a => a.CourseDetailId == model.CourseDetailId && a.TraineeId == model.TraineeId).ToList();
                    if (courseResult.Count() > 0)
                    {
                        foreach (var result in courseResult)
                        {
                            //result.Type = model.Type;
                            ////result.Remark = model.Remark;
                            //result.Result = model.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty;
                            //result.ModifiedBy = CurrentUser.USER_ID.ToString();
                            //result.ModifiedAt = DateTime.Now;
                            //result.inCourseMemberId = tmsMember.Id;
                            //_CourseService.UpdateCourseResult(result);

                            Dictionary<string, object> dic_result = new Dictionary<string, object>();
                            dic_result.Add("Type", model.Type ? 1 : 0);
                            dic_result.Add("Result", model.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty);
                            dic_result.Add("ModifiedBy", CurrentUser.USER_ID + "");
                            dic_result.Add("ModifiedAt", DateTime.Now);
                            dic_result.Add("inCourseMemberId", tmsMember.Id);
                            if (CMSUtils.UpdateDataSQLNoLog("Id", result.Id + "", "Course_Result", dic_result.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_result.Values.ToArray())) > 0)
                            {

                            }

                            var remarkresult = _CourseService.GetCourseResultCheckFail(a => a.CourseResultID == result.Id);
                            if (remarkresult.Any())
                            {
                                var recordremarkitem = remarkresult.FirstOrDefault();
                                //recordremarkitem.RemarkContent = model.Remark;
                                //recordremarkitem.CreatedAt = DateTime.Now;
                                //_CourseService.UpdateCourseResultCheckFail(recordremarkitem);

                                Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                dic_remark.Add("RemarkContent", model.Remark);
                                dic_remark.Add("CreatedAt", DateTime.Now);
                                if (CMSUtils.UpdateDataSQLNoLog("Id", recordremarkitem.Id + "", "CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                {

                                }
                            }
                            else
                            {
                                //var remark = new CourseRemarkCheckFail();
                                //remark.CourseResultID = result.Id;
                                //remark.RemarkContent = model.Remark;
                                //remark.CreatedAt = DateTime.Now;
                                //_CourseService.InsertCourseResultCheckFail(remark);

                                Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                                dic_remark.Add("CourseResultID", result.Id);
                                dic_remark.Add("RemarkContent", model.Remark);
                                dic_remark.Add("CreatedAt", DateTime.Now);
                                if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                                {

                                }

                            }


                        }
                    }
                    else
                    {
                        var entity = new Course_Result
                        {
                            TraineeId = model.TraineeId,
                            CourseDetailId = model.CourseDetailId,
                            Score = model.Score ?? 0,
                            CreatedAt = DateTime.Now,
                            CreatedBy = CurrentUser.USER_ID.ToString(),
                            Type = model.Type,
                            //Remark = model.Remark,
                            Result = model.Type ? UtilConstants.Grade.Fail.ToString() : string.Empty,
                            inCourseMemberId = tmsMember.Id
                        };
                        _CourseService.InsertCourseResult(entity);

                        //var remark = new CourseRemarkCheckFail();
                        //remark.CourseResultID = entity.Id;
                        //remark.RemarkContent = model.Remark;
                        //remark.CreatedAt = DateTime.Now;
                        //_CourseService.InsertCourseResultCheckFail(remark);

                        Dictionary<string, object> dic_remark = new Dictionary<string, object>();
                        dic_remark.Add("CourseResultID", entity.Id);
                        dic_remark.Add("RemarkContent", model.Remark);
                        dic_remark.Add("CreatedAt", DateTime.Now);
                        if (CMSUtils.InsertDataSQLNoLog("CourseRemarkCheckFail", dic_remark.Keys.ToArray(), CMSUtils.SetDBNullobject(dic_remark.Values.ToArray())) > 0)
                        {

                        }
                    }

                }


                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true,
                    data = model.Type
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        protected string MessageInvalidData(ModelStateDictionary modelState)
        {
            var errors = modelState.Where(a => a.Value.Errors.Any());

            var msg = new StringBuilder("<ul>");

            foreach (var error in errors)
            {

                msg.AppendLine(string.Format("<li class='text-danger'>{0}:<ul>", error.Key));
                foreach (var msgError in error.Value.Errors)
                {
                    msg.AppendLine(string.Format("<li>{0}</li><hr>", msgError.ErrorMessage));
                }
                msg.AppendLine("</ul></li>");
            }
            msg.AppendLine("</ul>");
            return msg.ToString();
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

            var courseDetail = _CourseDetailService.GetById(ddlSubject);
            var data_ = _courseMemberService.GetSubjectResult(ddlSubject);
            //var courseDetail = _CourseDetailService.GetById(ddlSubject);
            //var data_ =
            //       _courseMemberService.Get(
            //           a => a.Course_Details_Id == ddlSubject && a.DeleteApprove == null && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderBy(m => m.Member_Id);
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
                cellHeaderSubjectName.Value = courseDetail?.SubjectDetail?.Name?.ToString() ?? "";

                ExcelRange cellHeaderDuration = worksheet.Cells[8, 8];
                cellHeaderDuration.Value = courseDetail?.SubjectDetail?.Duration.HasValue == true ? ((float)courseDetail?.SubjectDetail?.Duration.Value).ToString() : "";

                ExcelRange cellHeaderVenue = worksheet.Cells[9, 4];
                cellHeaderVenue.Value = courseDetail?.Room?.str_Name?.ToString() ?? "";

                ExcelRange cellHeaderDate = worksheet.Cells[9, 8];
                cellHeaderDate.Value = courseDetail?.dtm_time_from.Value.ToString("dd/MM/yyyy") + " -" + courseDetail?.dtm_time_to.Value.ToString("dd/MM/yyyy");

                ExcelRange cellHeaderType = worksheet.Cells[10, 4];
                cellHeaderType.Value = TypeLearningName(courseDetail != null ? courseDetail.type_leaning.Value : -1);

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


        }
        public string GetRemarkCheckFail(int? resultid)
        {
            var remark = "";
            var remarkresult = _CourseService.GetCourseResultCheckFail(a => a.CourseResultID == resultid);
            remark = remarkresult.FirstOrDefault()?.RemarkContent;
            return remark;
        }
        protected string GetResultGrade(int? subjectDetailId, int? traineeId, int? courseDetailsId)
        {

            float _return = -1;
            var data = _CourseService.GetCourseResultSummary(traineeId, courseDetailsId);


            var grade = "Fail";
            var coursedetail = _CourseDetailService.GetById(courseDetailsId);
            if (coursedetail.SubjectDetail.IsAverageCalculate == true)
            {
                _return = (data?.point != null) ? (float)data.point : _return;
                var subjectDetails = _CourseService.GetScores(a => a.subject_id == subjectDetailId).OrderByDescending(a => a.point_from);
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
                grade = data.Result == "F" ? "Fail" : "Pass";

            }

            return grade;
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
        protected string GetPointRemark(UtilConstants.DetailResult type, int? traineeId, int? courseDetailId)
        {
            var result = "";

            switch (type)
            {
                case (int)UtilConstants.DetailResult.Score:
                    var data = _CourseService.GetCourseResultSummaries(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId).FirstOrDefault();
                    result = (data?.point != null && data?.point != -1) ? data.point?.ToString() : result;
                    break;
                case UtilConstants.DetailResult.Remark:
                    var datar =
               _CourseService.GetCourseResult(a => a.TraineeId == traineeId && a.CourseDetailId == courseDetailId)
                   .OrderByDescending(a => a.Id)
                   .FirstOrDefault();
                    result = datar?.Remark ?? result;
                    break;


            }
            return result;
        }
        protected string GetByKey(string key)
        {
            return _configService.GetByKey(key);
        }
        public ActionResult SubjectResultPrint(int id = 0)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            var courseDetail = _CourseDetailService.GetById(id);
            var model = new CourseDetailModelRp();

            model.header = GetByKey("Subject_Result_Header").Split(new char[] { ',' });
            if (courseDetail != null)
            {
                var data = _courseMemberService.GetSubjectResult(id);
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
        public string returnpointgrade(int? type, int? Trainee_Id, int? Course_Details_Id)
        {
            CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            string _return = "";
            var data = _CourseService.GetCourseResult(a => a.TraineeId == Trainee_Id && a.CourseDetailId == Course_Details_Id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
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
        private string ConvertDot(double value)
        {
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

            nfi.CurrencyDecimalSeparator = ".";
            nfi.CurrencyGroupSeparator = ",";
            nfi.CurrencySymbol = "";
            var answer = Convert.ToDecimal(value).ToString("f1",
                  nfi);
            return answer;
        }
    }
}