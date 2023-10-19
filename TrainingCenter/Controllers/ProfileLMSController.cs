using DAL.Entities;
using Microsoft.ReportingServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Companies;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Subject;
using TMS.Core.Services.TraineeHis;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Employee;
using TMS.Core.ViewModels.Subjects;
using TMS.Core.ViewModels.TraineeHistory;

namespace TrainingCenter.Controllers
{
    public class ProfileLMSController : Controller
    {
        private readonly IConfigService _configService;
        private readonly IEmployeeService _EmployeeService;
        private readonly ICompanyService _repoCompanyService;
        private readonly ITraineeHistoryService _repotraineeHistoryService;
        private readonly ICourseMemberService _courseMemberService;
        private readonly ICourseDetailService _courseDetailService;
        private readonly ICourseService _CourseService;
        private readonly ISubjectService _repoSubjectService;
        public ProfileLMSController(IConfigService configService, IEmployeeService EmployeeService, ICompanyService repoCompanyService, ITraineeHistoryService repotraineeHistoryService, ICourseMemberService courseMemberService, ICourseDetailService courseDetailService, ICourseService CourseService, ISubjectService repoSubjectService)
        {
            _configService = configService;
            _EmployeeService = EmployeeService;
            _repoCompanyService = repoCompanyService;
            _repotraineeHistoryService = repotraineeHistoryService;
            _courseMemberService = courseMemberService;
            _courseDetailService = courseDetailService;
            _CourseService = CourseService;
            _repoSubjectService = repoSubjectService;
        }


        // GET: ProfileLMS
        public ActionResult Index()
        {
            return View();
        }
        #region LMS call

        public ActionResult PluginProFile(string k)
        {
            //var username = k;
            var username = TMS.Core.Utils.Common.DecryptURL(k);
            if (username.ToLower() == "admin")
            {
                username = "administrator";
            }



            var model = new EmployeeModelDetails();
            var entity = _EmployeeService.Get(a=>a.str_Staff_Id.ToLower() == username.ToLower(),true).FirstOrDefault();
            if (entity != null)
            {

                //if (type != entity.int_Role)
                //{
                //    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = Resource.INVALIDURL };
                //    return RedirectToAction("Index", "Home");
                //}

                #region [Hannah Mentor]
                var keyHannahMentor = GetByKey("MENTOR");
                model.CheckHannahMentor = keyHannahMentor.Equals("1") ? true : false;

                if (model.CheckHannahMentor)
                {
                    var hannahMentor = string.Empty;
                    if (entity.Trainee_Type.Any())
                    {
                        foreach (var item in entity.Trainee_Type)
                        {
                            hannahMentor += (item.Type.HasValue ? " " + UtilConstants.SearchEmployee()[item.Type.Value] : "");
                        }
                    }
                    model.HannahMentor = hannahMentor;

                }
                #endregion


                model.Control = entity.int_Role == (int)UtilConstants.TypeInstructor.Instructor ? 1 : 2;
                model.Id = entity.Id;
                model.Avatar = entity.avatar;
                model.FullName = ReturnDisplayLanguage(entity.FirstName, entity.LastName);
                model.Eid = entity.str_Staff_Id;
                model.PersonId = entity.PersonalId;
                model.Passport = entity.Passport;
                model.Email = entity.str_Email;
                model.DateOfBirth = entity.dtm_Birthdate?.ToString(Resource.lbl_FORMAT_DATE);
                model.Type = entity.bit_Internal == true
                    ? UtilConstants.CourseAreas.Internal.ToString()
                    : UtilConstants.CourseAreas.External.ToString();
                model.PlaceOfBirth = entity.str_Place_Of_Birth;
                model.Department = entity.Department?.Code + " " + entity.Department?.Name;
                model.Gender = UtilConstants.GenderDictionary()[entity.Gender ?? (int)UtilConstants.Gender.Others];
                model.Jobtitle = entity.JobTitle?.Name;
                //model.Nation = _repoCompanyService.Get(a => a.str_code.Equals(entity.Nation))?.FirstOrDefault()?.str_Name;
                model.Company = entity.Company?.str_Name;
                model.Phone = entity.str_Cell_Phone;
                model.DateOfJoin = entity.dtm_Join_Date?.ToString(Resource.lbl_FORMAT_DATE);
                model.ResignationDate = entity.non_working_day?.ToString(Resource.lbl_FORMAT_DATE);
                model.Nation = entity.Nation;
            }
            return View(model);
        }
        public string ReturnDisplayLanguage(string firstName, string lastName, string culture = null)
        {
            string fullName;
            fullName = lastName + " " + firstName;
            return fullName;
        }
        protected string GetByKey(string key)
        {
            return _configService.GetByKey(key);
        }
        public ActionResult PartialListJobStandard(int? id, int type = (int)UtilConstants.Switch.Horizontal)
        {

            var traineeHistories =
                _repotraineeHistoryService.Get(a => a.Trainee_Id == id).OrderByDescending(b => b.Id).ToList();

            var model = new TraineeJobModel()
            {
                Type = type,
                TraineeHistories = traineeHistories,

            };
            return PartialView("_partials/_PartialTraineeJobStandard", model);
        }
        
        public ActionResult AjaxHandlerContract(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                var data = _EmployeeService.GetContract(a => a.Trainee_Id == id).OrderBy(p => p.id);

                IEnumerable<Trainee_Contract> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee_Contract, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.contractno
                                                            : sortColumnIndex == 2 ? c?.expire_date.ToString()
                                                             : sortColumnIndex == 3 ? c?.description
                                                          : c.id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                 c?.contractno,
                                    DateUtil.DateToString(c?.expire_date,Resource.lbl_FORMAT_DATE),
                                    c?.description
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
        public ActionResult AjaxHandlerTrainingCourses(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                //var datamenberapproval = _courseMemberService.Get(a => a.Member_Id == id && a.IsActive == true);
                //var datacourse = _courseDetailService.Get(a => datamenberapproval.Any(x => x.Course_Details_Id == a.Id), new[] { (int)UtilConstants.ApproveType.AssignedTrainee }).Select(x => x.Course).Distinct();
                var datacourse = _CourseService.Get(a => a.IsDeleted != true && a.Course_Detail.Any(c => c.IsDeleted != true && c.TMS_Course_Member.Any(x => x.Member_Id == id && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved))) && a.TMS_APPROVES.Any(v => v.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && v.int_id_status == (int)UtilConstants.EStatus.Approve), true);
                IEnumerable<Course> filtered = datacourse;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course, object> orderingFunction = (c
                                                          => sortColumnIndex == 2 ? c.Code
                                                            : sortColumnIndex == 3 ? c.Name
                                                            : sortColumnIndex == 4 ? (object)c.StartDate
                                                          : c.StartDate);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let final = c.Course_Result_Final.FirstOrDefault(a => a.traineeid == id)
                             select new object[] {
                                   "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'>+</span>",
                                 string.Empty,
                                    c?.Code,
                                    "<span data-value='"+c.Id+"' class='expand' style='cursor: pointer;'><a>"+c.Name+"</a></span>",
                                    DateUtil.DateToString(c?.StartDate,Resource.lbl_FORMAT_DATE) +" - "+ DateUtil.DateToString(c?.EndDate,Resource.lbl_FORMAT_DATE),
                                    !string.IsNullOrEmpty(final?.certificatefinal) ? ( !string.IsNullOrEmpty(final?.Path) && final?.statusCertificate == 0 ? "<a  href='"+ ConfigurationSettings.AppSettings["AWSLinkS3"] + final?.Path +"' target='_blank'  data-toggle='tooltip'><i class='fa fa-print btnIcon_green' ></i></a>"  : ""): "",
                                    //"<span data-value='" + c.Id + "' class='expand' style='cursor: pointer;'><a><i class='fa fa-plus-circle' aria-hidden='true' style=' font-size: 16px; '></i></a></span>"

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
        public ActionResult AjaxHandlerSubjectsOfCourse2(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());
                var subjectType = (int?)UtilConstants.ApproveType.SubjectResult;
                var datamenberapproval = _courseMemberService.Get(a => a.Member_Id == id && a.IsActive == true);
                //var datacourse = _courseDetailService.Get(a => datamenberapproval.Any(x => x.Course_Details_Id == a.Id), new[] { (int)UtilConstants.ApproveType.SubjectResult }).Select(x => x.CourseId).Distinct();
                var datacourse_ = _courseDetailService.Get(a => a.Course.IsDeleted != true && a.TMS_Course_Member.Any(x => x.Member_Id == id && x.IsActive == true && x.IsDelete != true && (x.Status == null || x.Status == (int)UtilConstants.APIAssign.Approved)) && a.TMS_APPROVES.Any(x => x.int_Type == subjectType));
                IEnumerable<int?> datacourse = new List<int?>();
                var listtemp = new List<int?>();
                foreach (var item in datacourse_)
                {
                    var item_ = item.TMS_APPROVES.LastOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult);
                    if (item_ != null)
                    {
                        if (item_.int_id_status == (int)UtilConstants.EStatus.Approve)
                        {
                            listtemp.Add(item.Id);
                        }
                    }
                }
                datacourse = listtemp;
                //---------------------------------------------------
                //var dataCourse_Detail_Id = _repoCourse_Detail.Get(a => a.Course_Id == id && !a.bit_Deleted
                //&& a.TMS_APPROVES.Any(x => x.int_Type == (int)Constants.ApproveType.SubjectResult)).Where(a => a.TMS_APPROVES.LastOrDefault(x => x.int_Type == (int)Constants.ApproveType.SubjectResult)?.int_id_status == (int)Constants.EStatus.Approve).Select(a => a.Course_Detail_Id);

                //var dataCourse_Detail_Id = _courseDetailService.Get(a => datacourse.Contains(a.CourseId)).Select(a => a.Id);
                var data = _courseMemberService.Get(a => datacourse.Contains((int)a.Course_Details_Id) && a.Member_Id == id && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true);
                //var data = _courseMemberService.Get(a => dataCourse_Detail_Id.Contains((int)a.Course_Details_Id) && a.Member_Id == id).Where(a =>
                //                a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve));

                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Course_Detail?.SubjectDetail?.Name
                                                            : sortColumnIndex == 2 ? c?.Course_Detail?.dtm_time_from
                                                            : sortColumnIndex == 6 ? (object)c?.Course_Detail?.SubjectDetail?.RefreshCycle
                                                            : c?.Course_Detail?.SubjectDetail?.Name);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                   : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                var resultA = from c in displayed.ToArray()
                              select new ProfileSubjectModel
                              {
                                  bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                  //SubjectCode = c?.Course_Detail?.SubjectDetail?.Code,
                                  dtm_from = c?.Course_Detail?.dtm_time_from,
                                  dtm_from_to = DateUtil.DateToString(c?.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c?.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                                  subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                  TypeLearning = TypeLearningIcon(c?.Course_Detail?.type_leaning),
                                  firstCheck = ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == id)),
                                  reCheck = ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == id)),

                                  remark = GetPointRemark(UtilConstants.DetailResult.Remark, c?.Member_Id, c?.Course_Details_Id),
                                  grade = returnpointgrade(2, c?.Member_Id, c?.Course_Details_Id),

                                  recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                                  courseDetails = c?.Course_Detail,
                                  memberId = c?.Member_Id,
                                  checkstatus = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.StatusCertificate,
                                  codecertificate = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id && x.StatusCertificate == 0)?.CertificateSubject,
                                  Path = c?.Course_Detail?.Course_Result?.FirstOrDefault(x => x.TraineeId == c.Member_Id)?.Path ?? "",

                                  //Status = (bool)c?.Course_Detail?.TMS_APPROVES?.Any(a => a.int_Type == (int)Constants.ApproveType.SubjectResult && a.int_id_status == (int)Constants.EStatus.Approve)
                                  //  ? "<span class='label label-success'>Success</span>"
                                  //  : "<span class='label label-danger'>In process</span>"

                              };


                resultA = resultA.ToList().OrderBy(a => a.subjectName.Trim()).ThenBy(a => a.dtm_from);
                for (int i = 0; i < resultA.Count(); i++)
                {
                    if (i != 0 && resultA.ElementAt(i).subjectName == resultA.ElementAt(i - 1).subjectName)
                    {
                        resultA.ElementAt(i).ex_Date = ResturnExpiredate(resultA.ElementAt(i).courseDetails, resultA.ElementAt(i).memberId, resultA.ElementAt(i - 1).ex_Date);
                    }
                    else
                    {
                        resultA.ElementAt(i).ex_Date = ResturnExpiredate(resultA.ElementAt(i).courseDetails, resultA.ElementAt(i).memberId, resultA.ElementAt(i).ex_Date);
                    }


                }

                //resultA = resultA.OrderBy(a => a.subjectName).ThenByDescending(a => a.dtm_from);

                var result = from c in resultA
                             select new object[] {
                               "<span " + (c?.bit_Active != true ? "style='color:" + UtilConstants.String_DeActive_Color + ";'" : "") + ">" +  c?.subjectName + "</span>",
                                c?.dtm_from_to,
                                c?.firstCheck ,
                                c?.reCheck    ,
                                c?.remark     ,
                                c?.grade      ,
                                c?.recurrent  ,
                                (c?.firstCheck == null  && c?.reCheck == null) ? string.Empty : c?.ex_Date?.ToString(Resource.lbl_FORMAT_DATE),
                                !string.IsNullOrEmpty(c?.codecertificate)
                            ? (c?.checkstatus == 0 ? (!string.IsNullOrEmpty(c?.Path) ? ( "<a  href='" + ConfigurationSettings.AppSettings["AWSLinkS3"] +c?.Path +"' target='_blank'  data-toggle='tooltip'>"+c?.codecertificate+"</a>" ) : c?.codecertificate) : "") :""
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
        protected object ReturnTraineePoint(bool isFirstCheck, bool? isAvarage, Course_Result rs)
        {
            object point = null;
            try
            {
                CultureInfo customCulture = (CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
                if (rs == null) return null;
                if (isFirstCheck)
                {
                    if (isAvarage.HasValue && isAvarage == true)
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
                    if (isAvarage.HasValue && isAvarage == true)
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
            catch (Exception ex)
            {

                return point;
            }

        }
        public string returnpointgrade(int? type, int? Trainee_Id, int? Course_Details_Id)
        {
            string _return = "";
            var data = _CourseService.GetCourseResult(a => a.TraineeId == Trainee_Id && a.CourseDetailId == Course_Details_Id).OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            if (data != null)
            {
                if (type == 1)
                {
                    #region [get điểm]
                    if (data.Re_Check_Score != null)
                    {
                        _return = data.Re_Check_Score.ToString();
                    }
                    else
                    {
                        if (data.First_Check_Score != null)
                        {
                            _return = data.First_Check_Score.ToString();
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
                            var check_distintion = _repoSubjectService.GetScores(a => a.subject_id == data.Subject_Id).OrderByDescending(a => a.point_from);
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
                                var check_distintion = _repoSubjectService.GetScores(a => a.subject_id == data.Subject_Id);
                                foreach (var item in check_distintion.OrderBy(a => a.point_from))
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
        protected string ReturnResult(IEnumerable<Subject_Score> subjectScores, double? point, string result)
        {
            var _return = UtilConstants.Grade.Fail.ToString();
            if (point != -1)
            {
                foreach (var item in subjectScores.Where(item => point >= item.point_from))
                {
                    _return = item.grade;
                    break;
                }
            }
            else
            {
                _return = result;
            }


            return _return;
        }
        public DateTime? ResturnExpiredate(Course_Detail CourseDetails, int? memberid, DateTime? prevExdate)//
        {

            DateTime? returnVal = null;
            if (CourseDetails == null) return null;
            var subjectCode = CourseDetails.SubjectDetailId;
            var enumerable = _CourseService.GetCourseResult(a => a.TraineeId == memberid && a.Course_Detail.SubjectDetailId == subjectCode
                                                   && a.Course_Detail.dtm_time_from <= CourseDetails.dtm_time_from
                                                   && a.Course_Detail.Course_Result_Summary.FirstOrDefault(b => b.TraineeId == memberid && b.CourseDetailId == a.CourseDetailId).Result != "Fail" && a.Course_Detail.TMS_APPROVES.OrderByDescending(x => x.id).FirstOrDefault(c => c.int_Type == (int)UtilConstants.ApproveType.SubjectResult).int_id_status == (int)UtilConstants.EStatus.Approve
                                                    );

            if (enumerable.Any())
            {
                //lấy 2 phần tử cuối
                var courseResults = enumerable.OrderByDescending(a => a.Course_Detail.dtm_time_from).Take(2).ToList();

                //-- debug
                //// Kq lần đầu
                var lastOrDefault = courseResults.LastOrDefault();
                if (lastOrDefault?.Course_Detail.dtm_time_to != null)
                {
                    var fromdateLast = returnDateExpireTepm((DateTime)lastOrDefault.Course_Detail.dtm_time_to, (int)lastOrDefault.Course_Detail.SubjectDetail.RefreshCycle);
                    if (prevExdate == null)
                    {
                        prevExdate = fromdateLast;
                    }
                }
                var firstOrDefault = courseResults.FirstOrDefault();
                if (firstOrDefault?.Course_Detail.dtm_time_from == null) return returnVal;
                //var fromdateFirst = firstOrDefault.Course_Detail.dtm_time_from;
                var fromdateFirst = (DateTime)firstOrDefault.Course_Detail.dtm_time_to;
                var expiredate = prevExdate;
                var expiredate3Month = expiredate?.AddMonths(-3);
                returnVal = returnDateExpireTepm(fromdateFirst, (int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle);

                if (expiredate3Month < fromdateFirst && fromdateFirst <= expiredate)
                {
                    returnVal = expiredate?.AddMonths((int)firstOrDefault.Course_Detail.SubjectDetail.RefreshCycle);
                }
            }
            else
            {
                var courseResult = enumerable.FirstOrDefault();
                if (courseResult?.Course_Detail?.dtm_time_to != null)
                    returnVal = returnDateExpireTepm((DateTime)courseResult.Course_Detail.dtm_time_to, (int)courseResult.Course_Detail.SubjectDetail.RefreshCycle);
            }
            return returnVal;
        }


        private DateTime? returnDateExpireTepm(DateTime fromdate, int cycle)
        {
            if (cycle == 0) return null;
            return (new DateTime(fromdate.Year, fromdate.Month, 1).AddMonths(1).AddDays(-1)).AddMonths(cycle);
        }
        public ActionResult AjaxHandlerTrainingCompetecy(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                          : c.Code);
                var data = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && a.Instructor_Ability.Any(x => x.InstructorId == id));
                var filtered = Request["sSortDir_0"] == "asc"
                    ? data.OrderBy(orderingFunction)
                    : data.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    c.Code,
                                    c.Name
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
        public ActionResult AjaxHandlerConductedSubjects(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                var data = _courseDetailService.Get(a => a.Course_Detail_Instructor.Any(x => x.Instructor_Id == id) && a.Course.IsActive == true && a.Course.IsDeleted == false).Select(a => a.SubjectDetailId);
                var dtsubject = _repoSubjectService.GetSubjectDetail(a => a.IsActive == true && data.Contains(a.Id));

                IEnumerable<SubjectDetail> filtered = dtsubject;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                          : c.Code);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Select(a => new { a.Code, a.Name }).Distinct().Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                string.Empty,
                                   "<span data-value='"+c.Code +"' class='expand' style='cursor: pointer;'><a>"+c.Code+"</a></span>",
                                   "<span data-value='"+c.Code +"' class='expand' style='cursor: pointer;'><a>"+c.Name+"</a></span>",
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
        public ActionResult AjaxHandlerSubjectsOfCourse(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var instructorId = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());
                var data = _courseMemberService.Get(a => a.Course_Detail.CourseId == id && a.Course_Detail.IsDeleted != true && a.Member_Id == instructorId && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true);
                //var dataCourse_Detail_Id = _courseDetailService.Get(a => a.CourseId == id, new[] { (int)UtilConstants.ApproveType.Course }).ToList().Select(a => a.Id);
                //var data = _courseMemberService.Get(a => dataCourse_Detail_Id.Contains((int)a.Course_Details_Id) && a.Member_Id == instructorId);


                IEnumerable<TMS_Course_Member> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Course_Detail?.dtm_time_from
                                                            : sortColumnIndex == 2 ? c?.Course_Detail?.SubjectDetail?.Name
                                                            : sortColumnIndex == 3 ? c?.Trainee?.Department?.Name
                                                            : sortColumnIndex == 7 ? (object)c?.Course_Detail?.SubjectDetail?.RefreshCycle
                                                            : c?.Course_Detail?.dtm_time_from);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                   : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);


                var resultA = from c in displayed.ToArray()
                              select new ProfileSubjectModel
                              {

                                  bit_Active = c?.Course_Detail?.SubjectDetail?.IsActive,
                                  dtm_from = c?.Course_Detail?.dtm_time_from,
                                  dtm_from_to = DateUtil.DateToString(c?.Course_Detail?.dtm_time_from, "dd/MM/yyyy") + "-" + DateUtil.DateToString(c?.Course_Detail?.dtm_time_to, "dd/MM/yyyy"),
                                  subjectName = c?.Course_Detail?.SubjectDetail?.Name,
                                  TypeLearning = TypeLearningIcon(c?.Course_Detail?.type_leaning),
                                  firstCheck = ReturnTraineePoint(true, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == instructorId)),
                                  reCheck = ReturnTraineePoint(false, c?.Course_Detail?.SubjectDetail?.IsAverageCalculate, c?.Course_Detail?.Course_Result?.FirstOrDefault(a => a.TraineeId == instructorId)),
                                  remark = GetPointRemark(UtilConstants.DetailResult.Remark, c?.Member_Id, c?.Course_Details_Id),
                                  grade = returnpointgrade(2, c?.Member_Id, c?.Course_Details_Id),

                                  recurrent = c?.Course_Detail?.SubjectDetail?.RefreshCycle == 0 ? "Unlimit" : c?.Course_Detail?.SubjectDetail?.RefreshCycle.ToString(),
                                  courseDetails = c?.Course_Detail,
                                  memberId = c?.Member_Id,
                                  Status =
                                  (bool)c?.Course_Detail?.TMS_APPROVES?.Any(a => a.int_Type == (int)UtilConstants.ApproveType.SubjectResult && a.int_id_status == (int)UtilConstants.EStatus.Approve) ?
                                        "<span class='label label-success'>Success</span>"
                                    : (bool)c?.Course_Detail?.Course.TMS_APPROVES?.Any(a => a.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee && a.int_id_status == (int)UtilConstants.EStatus.Approve) && c?.Course_Detail?.dtm_time_from > DateTime.Now
                                    ? "<span class='label label-warning'>Up Coming</span>" :
                                         "<span class='label label-danger'>Training</span>",

                              };

                resultA = resultA.OrderBy(a => a.dtm_from).ToList();
                for (int i = 0; i < resultA.Count(); i++)
                {
                    var name = resultA.ElementAt(i).subjectName;
                    var dataCourseDetail = _courseMemberService.Get(a => a.Course_Detail.SubjectDetail.Name.Equals(name) && a.Course_Detail.IsDeleted == false && a.Approve_Id != null && a.IsDelete == null && a.Member_Id == instructorId).Where(a =>
                                 a.Course_Detail.TMS_APPROVES.Any(x => x.int_Type == (int)UtilConstants.ApproveType.SubjectResult && x.int_id_status == (int)UtilConstants.EStatus.Approve));

                    if (dataCourseDetail.Any())
                    {
                        IEnumerable<TMS_Course_Member> filteredB = dataCourseDetail;
                        filteredB = filteredB.OrderBy(a => a?.Course_Detail?.dtm_time_from);
                        var displayedB = filteredB;
                        var resultB = (from c in displayedB.ToArray()
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
                            if (resultA.ElementAt(i).dtm_from == resultB.ElementAt(y).dtm_from)
                            {
                                resultA.ElementAt(i).ex_Date = resultB.ElementAt(y).ex_Date;
                            }
                        }
                    }
                    else
                    {
                        if (i != 0 && resultA.ElementAt(i).subjectName == resultA.ElementAt(i - 1).subjectName)
                        {
                            resultA.ElementAt(i).ex_Date = ResturnExpiredate(resultA.ElementAt(i).courseDetails, resultA.ElementAt(i).memberId, resultA.ElementAt(i - 1).ex_Date);
                        }
                        else
                        {
                            resultA.ElementAt(i).ex_Date = ResturnExpiredate(resultA.ElementAt(i).courseDetails, resultA.ElementAt(i).memberId, resultA.ElementAt(i).ex_Date);
                        }
                    }
                }

                resultA = resultA.OrderByDescending(a => a.dtm_from);


                var result = from c in resultA
                             select new[] {
                                 string.Empty,
                                  "<span " + (c?.bit_Active != true ? "style='color:" + UtilConstants.String_DeActive_Color + ";'" : "") + ">" + c?.subjectName + "</span>",
                                 c?.dtm_from_to,
                                 c.firstCheck ,
                                 c.reCheck    ,
                                 c.remark     ,
                                 c.grade      ,
                                 c.recurrent  ,
                                 (c?.grade == "Fail" || string.IsNullOrEmpty(c?.grade)) ? string.Empty : c?.ex_Date?.ToString(Resource.lbl_FORMAT_DATE),
                                  c.Status

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
        public ActionResult AjaxHandlerConductedCourseOfSubject(jQueryDataTableParamModel param, string id = "")
        {
            try
            {
                var instructorId = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());

                var dataCourse_Detail = _courseDetailService.Get(a => a.SubjectDetail.Code.Equals(id) && a.Course_Detail_Instructor.Any(x => x.Instructor_Id == instructorId) && a.Course.IsActive == true && a.IsDeleted == false && a.SubjectDetail.IsDelete == false);


                IEnumerable<Course_Detail> filtered = dataCourse_Detail;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Course_Detail, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c?.Course?.Code
                                                            : sortColumnIndex == 2 ? c?.Course?.Name
                                                            : sortColumnIndex == 3 ? c?.dtm_time_from
                                                            : sortColumnIndex == 4 ? c?.dtm_time_to
                                                            : sortColumnIndex == 5 ? (object)c?.Room?.str_Name
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
                             select new object[] {
                                                 string.Empty,
                                                 c?.Course?.Code,
                                                 c?.Course?.Name,
                                                  DateUtil.DateToString( c?.dtm_time_from ,Resource.lbl_FORMAT_DATE)+"<br />"+
                                                   (c?.time_from != null ? (c?.time_from?.Substring(0, 2)+":"+c?.time_from?.Substring(2)) : ""),

                                                    DateUtil.DateToString( c?.dtm_time_to ,Resource.lbl_FORMAT_DATE)+"<br />"+
                                                    (c?.time_to != null ? (c?.time_to?.Substring(0, 2)+":"+c?.time_to?.Substring(2)) : ""),
                                                    c?.Room?.str_Name,
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
        public ActionResult AjaxHandlerEducation(jQueryDataTableParamModel param)
        {
            try
            {
                var id = string.IsNullOrEmpty(Request.QueryString["Id"]) ? -1 : Convert.ToInt32(Request.QueryString["Id"].Trim());
                var data = _EmployeeService.GetRecord(a => a.Trainee_Id == id).OrderBy(p => p.str_Subject);

                List<Trainee_Record> models = data.ToList();
                IEnumerable<Trainee_Record> filtered = models;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee_Record, string> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.dtm_time_from.ToString()
                                                            : sortColumnIndex == 2 ? c.str_Subject
                                                             : sortColumnIndex == 3 ? c.str_organization
                                                             : sortColumnIndex == 4 ? c.str_note
                                                          : c.Trainee_Record_Id.ToString());


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                    : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    c.dtm_time_from?.ToString(Resource.lbl_FORMAT_DATE) +" - "+ c.dtm_time_to?.ToString(Resource.lbl_FORMAT_DATE),
                                    c.str_Subject,
                                     c.str_organization,
                                      c.str_note
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
        #endregion
    }
}