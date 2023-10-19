using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Entities;
using DAL.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Users;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Survey;
using TMS.Core.ViewModels.Courses;
using TMS.Core.ViewModels.Room;
using Utilities;
using TrainingCenter.Serveices;
using static TrainingCenter.Controllers.SurveyController;
using static TrainingCenter.Controllers.ReminderController;
using DAL.UnitOfWork;
using TrainingCenter.Utilities;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using AppUtils = Utilities.AppUtils;
using System.Threading.Tasks;
using System.Globalization;
//using TrainingCenter.Serveices;

namespace TrainingCenter.Controllers
{
    public class SurveyController : BaseAdminController
    {
        // GET: Survey
        #region init
        private readonly IRepository<MENU> _repoMenu = null;
        private readonly IRepository<Course> _repoCourse = null;
        private readonly IRepository<Room> _repoRoom = null;
        private readonly CallLmsServices _sLmsServices = null;
        private readonly IRepository<Trainee> _repoTrainee = null;
        private readonly IRepository<TMS_Course_Member> _repoCourse_Member = null;
        private readonly IRepository<Course_Detail_Instructor> _repoCourse_detail_instructor;

        private readonly IRepository<Course_Detail> _repoCourse_Detaillist = null;

        public SurveyController(IRepository<Course_Detail_Instructor> repoCourse_detail_instructor, IRepository<TMS_Course_Member> repoCourse_Member, IRepository<Trainee> repoTrainee, IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService, IRepository<MENU> repoMenu, IRepository<Course> repoCourse, IRepository<Room> repoRoom, /*CallLmsServices sLmsServices,*/ IRepository<Course_Detail> repoCourse_Detaillist) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoCourse_Member = repoCourse_Member;
            _repoTrainee = repoTrainee;
            _repoMenu = repoMenu;
            _repoCourse = repoCourse;
            _repoRoom = repoRoom;
            _repoCourse_detail_instructor = repoCourse_detail_instructor;
            _sLmsServices = new CallLmsServices();
            _repoCourse_Detaillist = repoCourse_Detaillist;
        }

        public class ServicesResultModel
        {
            public string TenHam { get; set; }
            public List<RootObject> Lms { get; set; }

        }
        public class UserSurvey
        {
            public string userid { get; set; }
            public string username { get; set; }
            public int idTrainee { get; set; }
            public int IdcCourse { get; set; }
            public string score { get; set; }
            public string result { get; set; }
            //
            public string fullname { get; set; }
            public string department { get; set; }

            //public string remark { get; set; }
            public List<Response_rank> response_rank { get; set; }
            public List<Response_text> response_text { get; set; }

        }
        public class Choise
        {
            public string content { get; set; }
        }
        public class Question
        {
            public string name { get; set; }
            public string name_type { get; set; }
            public string content { get; set; }
            public List<Choise> choise { get; set; }
        }
        public class Response_rank
        {
            public string response_id { get; set; }
            public string question_id { get; set; }
            public string choice_id { get; set; }
            public string rank { get; set; }
        }

        public class Response_text
        {
            public string response_id { get; set; }
            public string question_id { get; set; }
            public string response { get; set; }
        }

        public class RootObject
        {
            public string course_code { get; set; }
            public string code_survey { get; set; }
            public string name { get; set; }
            public string intro { get; set; }
            public List<UserSurvey> users { get; set; }
            public List<Question> question { get; set; }
            public int? id_survey { get; set; }

        }
        public class user
        {
            public string userid { get; set; }
            public string username { get; set; }
            public int idTrainee { get; set; }
            public int IdcCourse { get; set; }
            public string score { get; set; }
            public string result { get; set; }
            //
            public string fullname { get; set; }
            public string department { get; set; }

            //public string remark { get; set; }
            public List<Response_rank> response_rank { get; set; }
            public List<Response_text> response_text { get; set; }

        }
        public class SurveyViewModel
        {
            public int? Survey_ID { get; set; }
            public int? CourseDetail_ID { get; set; }
            public string Course_Name { get; set; }
            public string Course_Code { get; set; }
            public string Survey_Name { get; set; }
            public string Subject_Code { get; set; }
            public string Subject_Name { get; set; }
            public string Date_From { get; set; }
            public string Date_To { get; set; }
            public DateTime Date_FromDT { get; set; }
            public DateTime Date_ToDT { get; set; }
        }
        [AllowAnonymous]
        public ActionResult AjaxHandleListSurvey(jQueryDataTableParamModel param)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
                IFormatProvider culture = new System.Globalization.CultureInfo("vi-VN", true);
                #region [Lấy dữ liệu LMS]

                var suurvey_model = new List<SurveyViewModel>();
                var model = new List<RootObject>();
                var type = UtilConstants.CRON_POST_SURVEY;
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
                //model = JsonConvert.DeserializeObject<List<RootObject>>(response.Content);
                IEnumerable<UserSurvey> filtesred = new List<UserSurvey>();

                if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                {
                    var num = responseContent.IndexOf("###################################################");
                    responseContent = responseContent.Remove(num);
                    var model_ = JsonConvert.DeserializeObject<List<ServicesResultModel>>(responseContent);
                    model = model_?.FirstOrDefault()?.Lms;
                    if (model != null)
                    {
                        foreach (var item in model)
                        {
                            var survey_item = new SurveyViewModel();
                            survey_item.Survey_ID = item?.id_survey ?? -1;
                            survey_item.Survey_Name = "<span data-value='" + survey_item.Survey_ID + "' class='expand' style='cursor: pointer;'><a>" + item?.name + "</a></span>";
                            if (!string.IsNullOrEmpty(item?.course_code))
                            {
                                //var courseDetailID = item?.course_code?.Split('_');
                                var courseDetail_value = ""; //courseDetailID[1];
                                var checkcode = item?.course_code?.LastIndexOf("::");
                                if (checkcode != -1)
                                {
                                    courseDetail_value = item?.course_code?.Substring((int)checkcode + 2);
                                }

                                var record = _repoCourse_Detaillist.Get(Convert.ToInt32(courseDetail_value));
                                survey_item.CourseDetail_ID = record?.Id;
                                survey_item.Subject_Name = record?.SubjectDetail?.Name;
                                survey_item.Subject_Code = record?.SubjectDetail?.Code;
                                survey_item.Date_From = DateUtil.DateToString(record?.dtm_time_from, "dd/MM/yyyy");
                                survey_item.Date_To = DateUtil.DateToString(record?.dtm_time_to, "dd/MM/yyyy");
                                survey_item.Date_FromDT = (DateTime)record?.dtm_time_from;
                                survey_item.Date_ToDT = (DateTime)record?.dtm_time_to.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                                survey_item.Course_Code = record?.Course?.Code;
                                survey_item.Course_Name = record?.Course?.Name;
                            }
                            suurvey_model.Add(survey_item);
                        }
                    }
                }
                #endregion
                #region [list dữ liệu]

                string code = string.IsNullOrEmpty(Request.QueryString["subjectcode"]) ? "" : Request.QueryString["subjectcode"].Trim();
                string name = string.IsNullOrEmpty(Request.QueryString["subjectname"]) ? "" : Request.QueryString["subjectname"].Trim();

                string fSearchDate_from = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
                string fSearchDate_to = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? "" : Request.QueryString["fSearchDate_to"].Trim();


                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDate_from, out dateFrom);
                DateTime.TryParse(fSearchDate_to, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;
                var data = suurvey_model.Where(a =>
                                            (string.IsNullOrEmpty(name) || a.Course_Name.Contains(name)) &&
                                            (string.IsNullOrEmpty(code) || a.Course_Code.Contains(code)) &&
                                           (dateFrom == DateTime.MinValue || dateFrom <= a.Date_FromDT) &&
                            (dateTo == DateTime.MinValue || a.Date_ToDT <= dateTo)
                                            );
                IEnumerable<SurveyViewModel> filtered = data;
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                if (sortColumnIndex != 5 && sortColumnIndex != 6)
                {
                    Func<SurveyViewModel, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.Course_Code
                                                              : sortColumnIndex == 2 ? c?.Course_Name
                                                               : sortColumnIndex == 3 ? c?.Subject_Name
                                                                : sortColumnIndex == 4 ? c?.Survey_Name
                                                                : c?.Course_Name);
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                   : filtered.OrderByDescending(orderingFunction);
                }
                else
                {
                    Func<SurveyViewModel, DateTime> orderingFunction_Datetime = (c => sortColumnIndex == 5 ? (DateTime)c?.Date_FromDT.Date : (DateTime)c?.Date_ToDT.Date);
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction_Datetime)
                                   : filtered.OrderByDescending(orderingFunction_Datetime);
                }

                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);

                var result = from c in displayed.ToArray()
                             select new object[] {
                                                 string.Empty,
                                                 c?.Course_Code,
                                                 c?.Course_Name,
                                                 c?.Subject_Name,
                                                 c?.Survey_Name,
                                                 c?.Date_From,
                                                  c?.Date_To,
                                                  "<a href='/Survey/SurveyTotalDetail?idsurvey="+ c?.Survey_ID +"&coursedetailId="+c?.CourseDetail_ID+"' style='cursor: pointer;'><i class='fa fa-search' aria-hidden='true' style=' font-size: 16px; color: black;'></i></a>&nbsp;" +
                                                  "<span data-value='" + c?.Survey_ID + "' class='expand' style='cursor: pointer;'><i class='fa fa-plus-circle' aria-hidden='true' style=' font-size: 16px; color: black; '></i></span>",
            };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                },
            JsonRequestBehavior.AllowGet);
                #endregion
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
        public ActionResult AjaxHandlerTrainee1(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                #region [Lấy dữ liệu LMS]

                var suurvey_List_Trainee = new List<int>();
                var suurvey_List_Detail = new List<int>();
                var suurvey_model = new List<SurveyViewModel>();
                var model = new List<RootObject>();
                var type = UtilConstants.CRON_POST_SURVEY;
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
                //model = JsonConvert.DeserializeObject<List<RootObject>>(response.Content);
                IEnumerable<UserSurvey> filtesred = new List<UserSurvey>();
                if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                {
                    var num = responseContent.IndexOf("###################################################");
                    responseContent = responseContent.Remove(num);
                    var model_ = JsonConvert.DeserializeObject<List<ServicesResultModel>>(responseContent);
                    model = model_?.FirstOrDefault()?.Lms;

                    foreach (var item in model)
                    {
                        if (item.id_survey == id)
                        {
                            var survey_item = new SurveyViewModel();
                            survey_item.Survey_ID = item?.id_survey ?? -1;

                            if (!string.IsNullOrEmpty(item?.course_code))
                            {
                                var courseDetail_value = ""; //courseDetailID[1];
                                var checkcode = item?.course_code?.LastIndexOf("::");
                                if (checkcode != -1)
                                {
                                    courseDetail_value = item?.course_code?.Substring((int)checkcode + 2);
                                }
                                suurvey_List_Detail.Add(Convert.ToInt32(courseDetail_value));
                            }
                            if (item.users != null)
                            {
                                foreach (var item1 in item.users)
                                {
                                    var eid = item1.userid;
                                    var traineeId = _repoTrainee.Get(a => a.str_Staff_Id.ToLower() == item1.userid && a.IsDeleted != true);
                                    if (traineeId != null)
                                    {
                                        suurvey_List_Trainee.Add(traineeId.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region [list dữ liệu]   
                var data = CourseMemberService.Get(a => suurvey_List_Detail.Contains((int)a.Course_Details_Id) && suurvey_List_Trainee.Contains((int)a.Member_Id) && a.IsActive == true && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approved), true).OrderByDescending(p => p.Id);

                IEnumerable<TMS_Course_Member> filtered = data.GroupBy(a => a.Member_Id).Select(b => b.FirstOrDefault());

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<TMS_Course_Member, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.Trainee?.LastName
                                                                        : sortColumnIndex == 2 ? c?.Trainee?.str_Staff_Id
                                                                        : sortColumnIndex == 3 ? c?.Trainee?.Department?.Name
                                                                        : "");


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
                                    c?.Trainee?.LastName + " " + c?.Trainee.FirstName,
                                    c?.Trainee?.str_Staff_Id,
                                    c?.Trainee?.Department?.Name,
                                   "<a href='/Survey/SurveyDetail?idsurvey="+ id +"&idmember="+c?.Member_Id+"&coursedetailId="+c?.Course_Details_Id+"' style='cursor: pointer;'><i class='fa fa-search'></i></a>"
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
            #endregion

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
        public ActionResult Index()
        {
            var model = new SurveyModels();

            // model.CourseList = new SelectList(db.Course.Where(a => a.int_Status == 0).ToDictionary(a => a.str_Code, a => a.str_Code + "_" + a.str_Name).OrderBy(m => m.Key), "Key", "Value");
            model.CourseList = CourseService.Get(a => a.IsActive == true && a.IsDeleted == false).OrderBy(m => m.Code).ToDictionary(a => a.Code, a => a.Code + "_" + a.Name);

            model.RoomList = _repoRoom.GetAll(a => a.bit_Deleted == false).OrderBy(m => m.str_Name).Select(a => new RoomModels()
            {
                Id = a.Room_Id,
                Name = a.str_Name,
            }).ToList();

            //model.InstructorList = new SelectList(db.Trainee.Where(a => a.int_Role == (int)Constants.ROLE.Instructor && !a.bit_Deleted).OrderBy(m => m.str_Fullname).ToList(), "Trainee_Id", "str_Fullname");
            //model.Typecourse = new SelectList(db.Course_Type.OrderBy(m => m.str_Name).ToList(), "Course_Type_Id", "str_Name");
            model.Id = _repoMenu.Get(a => a.NAME.Equals("Report")).ID;
            return View(model);
        }
        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var strCode = string.IsNullOrEmpty(Request.QueryString["str_Code"]) ? string.Empty : Request.QueryString["str_Code"].Trim().ToLower();
                var strName = string.IsNullOrEmpty(Request.QueryString["str_Name"]) ? string.Empty : Request.QueryString["str_Name"].Trim().ToLower();
                string fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? string.Empty : Request.QueryString["fSearchDate_from"].Trim();
                string fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["fSearchDate_to"]) ? string.Empty : Request.QueryString["fSearchDate_to"].Trim();
                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDateFrom, out dateFrom);
                DateTime.TryParse(fSearchDateTo, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                var data = ConfigService.GetSurvey(a => (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.OpenDate) >= 0) && (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.CloseDate, dateTo) >= 0) && (string.IsNullOrEmpty(strCode) || a.Code.Contains(strCode)) && (string.IsNullOrEmpty(strName) || a.Name.Contains(strName)));

                IEnumerable<Survey> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Survey, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Code
                                                          : sortColumnIndex == 2 ? c.Name
                                                          : c.Code);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);

                var ajaxRooms = filtered.ToArray();

                var displayed = ajaxRooms.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                          c?.Code,//"<span data-value='" + c?.Code + "' class='expand' style='cursor: pointer;'>"+c?.Code+"</span>",
                         c?.Name, //"<span data-value='" + c?.Code + "' class='expand' style='cursor: pointer;'>"+c?.Name+"</span>",
                         c?.OpenDate.Value.ToString("dd/MM/yyyy") ?? string.Empty,
                         c?.CloseDate.Value.ToString("dd/MM/yyyy") ?? string.Empty,
                         c?.Description,
                         (c?.Is_Active == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Survey(0," + c?.Id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Survey(1," + c?.Id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                          "<a href='/Survey/SurveyTotalDetail?code="+ c?.Code +"' title='View' style='cursor: pointer;' data-toggle='tooltip'><i class='fas fa-search btnIcon_blue font-byhoa' aria-hidden='true' ></i></a>&nbsp;" +
                          ((User.IsInRole("/Survey/Modify")) ? "<a title='Edit' href='" + @Url.Action("Modify", new { id = c?.Id }) + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : string.Empty) +

                        ((User.IsInRole("/Survey/delete")) ? "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c?.Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : string.Empty),
                         };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = ajaxRooms.Count(),
                    iTotalDisplayRecords = ajaxRooms.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Survey/AjaxHandler", ex.Message);
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
        public ActionResult AjaxHandlerSurveyItem(jQueryDataTableParamModel param)
        {
            try
            {
                var code = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].Trim();
                #region [Lấy dữ liệu LMS]
                var suurvey_List_Trainee = new List<int>();
                var suurvey_List_Detail = new List<int>();
                var model = new List<ServicesResultModel>();
                var type = UtilConstants.CRON_POST_SURVEY;
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

                IEnumerable<UserSurvey> filtesred = new List<UserSurvey>();

                if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
                {
                    var num = responseContent.IndexOf("###################################################");
                    responseContent = responseContent.Remove(num);

                    model = JsonConvert.DeserializeObject<List<ServicesResultModel>>(responseContent);
                    var _model = model?.FirstOrDefault().Lms;
                    foreach (var item in _model)
                    {

                        if (item.users != null)
                        {
                            foreach (var item1 in item.users)
                            {
                                var eid = item1.userid;
                                var traineeId = EmployeeService.Get(a => a.str_Staff_Id.ToLower() == item1.userid && a.IsDeleted == false)?.FirstOrDefault();
                                if (traineeId != null)
                                {
                                    suurvey_List_Trainee.Add(traineeId.Id);
                                }
                            }
                        }

                    }

                }

                #endregion

                var data = EmployeeService.Get(a => suurvey_List_Trainee.Contains(a.Id) && a.IsDeleted == false);

                IEnumerable<Trainee> filtered = data;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.FirstName
                                                                        : sortColumnIndex == 2 ? c?.str_Staff_Id
                                                                        : sortColumnIndex == 3 ? c?.Department?.Name
                                                                        : c?.FirstName);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                    : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             let fullName = ReturnDisplayLanguage(c?.FirstName, c?.LastName)
                             select new object[] {
                                    string.Empty,
                                    fullName,
                                    c?.str_Staff_Id,
                                    c?.Department?.Name,
                                   "<a href='/Survey/SurveyDetail?code="+ code +"&idemployee="+c?.Id+"' style='cursor: pointer;'><i class='fa fa-search'></i></a>"
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Survey/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Modify(int id = 0)
        {
            var model = new SurveyModels();
            if (id != 0)
            {
                var entity = ConfigService.GetSurveyById(id);
                model.Id = entity.Id;
                model.Code = entity.Code;
                model.Name = entity.Name;
                model.Description = entity.Description;
                model.StartDate = entity.OpenDate;
                model.EndDate = entity.CloseDate;
            }
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Modify(SurveyModels model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { result = false, message = MessageInvalidData(ModelState) });

                if (model.StartDate > model.EndDate)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        result = false,
                        message = Messege.VALIDATION_COURSE_FROM_THAN_TO

                    });
                }
                var entity = ConfigService.GetSurveyById(model.Id);
                if (entity != null)
                {
                    if (ConfigService.GetSurvey(a => a.Code.ToLower() == model.Code.ToLower() && a.Id != model.Id).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE }, JsonRequestBehavior.AllowGet);
                    }
                    ConfigService.UpdateSurvey(model);
                }
                else
                {
                    if (ConfigService.GetSurvey(a => a.Code.ToLower() == model.Code.ToLower()).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE }, JsonRequestBehavior.AllowGet);
                    }
                    ConfigService.InsertSurvey(model);
                }
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON SURVEY)----------]
                    var callLms = CallServices(UtilConstants.CRON_SURVEY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Survey/Modify", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate)));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name),
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }

                    #endregion
                });
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS };
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Survey/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        public async Task<ActionResult> SubmitSetParticipateSurvey(int isParticipate, string id)
        {
            int idsurvey = int.Parse(id);
            var removeSurvey = ConfigService.GetSurveyById(idsurvey);
            if (removeSurvey == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            if (isParticipate == 1)
            {
                removeSurvey.Is_Active = false;
            }
            else
            {
                removeSurvey.Is_Active = true;
            }
            removeSurvey.LMSStatus = 1;
            ConfigService.UpdateSurvey(removeSurvey);
            await Task.Run(() =>
            {
                #region [--------CALL LMS (CRON PROGRAM)----------]
                var callLms = CallServices(UtilConstants.CRON_SURVEY);
                if (!callLms)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Survey/Modify", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (removeSurvey.Id != null ? Resource.lblModify : Resource.lblCreate)));
                    //return Json(new AjaxResponseViewModel()
                    //{
                    //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (removeSurvey.Id != null ? Resource.lblModify : Resource.lblCreate), removeSurvey.Name),
                    //    result = false
                    //}, JsonRequestBehavior.AllowGet);
                }

                #endregion
            });
            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.SET_STATUS_SUCCESS, removeSurvey.Name),
                result = true
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> delete(int id = -1)
        {
            try
            {
                var model = ConfigService.GetSurveyById(id);
                model.Is_Deleted = true;
                model.Is_Active = false;
                model.Deleted_At = DateTime.Now;
                model.Deleted_By = GetUser().USER_ID;
                model.LMSStatus = 1;
                ConfigService.UpdateSurvey(model);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON PROGRAM)----------]
                    var callLms = CallServices(UtilConstants.CRON_SURVEY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Survey/delete", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id != null ? Resource.lblModify : Resource.lblCreate)));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblDelete, model.Name),
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }

                    #endregion
                });
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Survey/deleted", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [AllowAnonymous]
        public ActionResult SurveyDetail(int idsurvey, int idmember, int coursedetailId)
        {
            var trainee = _repoTrainee.Get(p => p.Id == idmember);
            var model = new List<ServicesResultModel>();
            var type = UtilConstants.CRON_POST_SURVEY;
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
            if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
            {
                var num = responseContent.IndexOf("###################################################");
                responseContent = responseContent.Remove(num);

                JObject rss = JObject.Parse(@"{ 'item' :" + responseContent + "}");

                var data_ = rss.SelectTokens("$..Lms[?(@.id_survey == '" + idsurvey + "')]").FirstOrDefault();
                if (data_ != null)
                {
                    var member = data_["users"];
                    var data = new List<Newtonsoft.Json.Linq.JToken>();
                    foreach (var item in member)
                    {
                        var result = item["userid"].ToObject<string>();
                        if (result == trainee.str_Staff_Id.ToLower())
                        {
                            data.Add(item);
                        }
                    }
                    var userItem = data.FirstOrDefault();
                    ViewBag.DataQuestion = data_;
                    ViewBag.Idsurvey = idsurvey;
                    ViewBag.Idmember = idmember;
                    ViewBag.courseDetailID = coursedetailId;
                    ViewBag.DataUser = userItem;
                }

            }

            return View();
        }
        public ActionResult SurveyTotalDetail(int idsurvey, int coursedetailId)
        {
            var type = UtilConstants.CRON_POST_SURVEY;
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
            if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
            {
                var num = responseContent.IndexOf("###################################################");
                responseContent = responseContent.Remove(num);
                JObject rss = JObject.Parse(@"{ 'item' :" + responseContent + "}");
                var data_ = rss.SelectTokens("$..Lms[?(@.id_survey == '" + idsurvey + "')]").FirstOrDefault();
                ViewBag.DataQuestion = data_;
                ViewBag.Idsurvey = idsurvey;
                ViewBag.courseDetailID = coursedetailId;
            }

            return View();
        }
        [AllowAnonymous]
        #region Export Excel
        public FileResult ExportSurveyTotal(string idsurvey, string coursedetailId, string type, string idmember)
        {

            int surveyId_ = string.IsNullOrEmpty(idsurvey) ? -1 : Convert.ToInt32(idsurvey);
            int coursedetailid_ = string.IsNullOrEmpty(coursedetailId) ? -1 : Convert.ToInt32(coursedetailId);
            int _type = string.IsNullOrEmpty(type) ? -1 : Convert.ToInt32(type);
            int _idmember = string.IsNullOrEmpty(idmember) ? -1 : Convert.ToInt32(idmember);
            if (coursedetailid_ != -1 && surveyId_ != -1 && _type != -1)
            {
                byte[] filecontent = ExportExcelSurveyTotal(surveyId_, coursedetailid_, _type, _idmember);
                if (filecontent != null)
                {
                    return File(filecontent, ExportUtils.ExcelContentType, "Survey" + "_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx");
                }
            }
            return null;
        }

        private byte[] ExportExcelSurveyTotal(int idsurvey, int coursedetailId, int type, int idmember)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
            var typefunction = UtilConstants.CRON_POST_SURVEY;
            var server = GetByKey("API_LMS_SERVER");
            var token = GetByKey("API_LMS_TOKEN");
            var function = GetByKey("FUNCTION");
            var moodlewsrestformat = GetByKey("moodlewsrestformat") ?? "";
            var restClient = new RestClient(server);
            var request = new RestRequest(Method.POST);
            request.AddParameter("wstoken", token);

            request.AddParameter("wsfunction", function);
            request.AddParameter("moodlewsrestformat", moodlewsrestformat);
            request.AddParameter("type", typefunction);
            var response = restClient.Execute(request);
            var responseContent = response.Content;
            byte[] Bytes = null;
            if (responseContent.Contains("\"warnings\":[]") || responseContent.Contains("\"exception\":[]"))
            {
                var numm = responseContent.IndexOf("###################################################");
                responseContent = responseContent.Remove(numm);
                JObject rss = JObject.Parse(@"{ 'item' :" + responseContent + "}");
                var data_ = rss.SelectTokens("$..Lms[?(@.id_survey == '" + idsurvey + "')]").FirstOrDefault();
                if (data_ != null)
                {
                    var qs = data_?["question"];
                    var list_trainee = data_?["users"];
                    var courseDetailItem = _repoCourse_Detaillist.Get(p => p.Id == coursedetailId);
                    var member = data_["users"];
                    var trainee = idmember != -1 ? _repoTrainee.Get(p => p.Id == idmember) : null;
                    var data = new List<Newtonsoft.Json.Linq.JToken>();
                    foreach (var item in member)
                    {
                        var result = item["userid"].ToObject<string>();
                        if (result == trainee?.str_Staff_Id.ToLower())
                        {
                            data.Add(item);
                        }
                    }
                    var userItem = data.FirstOrDefault();


                    var response_rank = userItem?["response_rank"];
                    var response_text = userItem?["response_text"];


                    int startrow = 7;
                    string templateFilePath = type == 1 ? Server.MapPath(@"~/Template/ExcelFile/SurveyResult.xlsx") : Server.MapPath(@"~/Template/ExcelFile/SurveyDetail.xlsx");
                    FileInfo template = new FileInfo(templateFilePath);
                    MemoryStream MS = new MemoryStream();
                    if (qs != null)
                    {
                        string[] header = GetConfig.ByKey("Survey_Header") != null ? GetConfig.ByKey("Survey_Header").Split(new char[] { ',' }) : new string[] { };
                        ExcelPackage xlPackage;
                        using (xlPackage = new ExcelPackage(template, false))
                        {
                            ExcelWorksheet worksheet = xlPackage.Workbook.Worksheets.First();

                            ExcelRange cellHeaderCourseCode = worksheet.Cells[startrow, 3];
                            cellHeaderCourseCode.Value = courseDetailItem?.Course?.Code;

                            ExcelRange cellHeaderCourseName = worksheet.Cells[startrow, type == 1 ? 7 : 6];
                            cellHeaderCourseName.Value = courseDetailItem?.Course?.Name;

                            ExcelRange cellHeaderSubjectCode = worksheet.Cells[startrow + 1, 3];
                            cellHeaderSubjectCode.Value = courseDetailItem?.SubjectDetail?.Code;

                            ExcelRange cellHeaderSubjectName = worksheet.Cells[startrow + 1, type == 1 ? 7 : 6];
                            cellHeaderSubjectName.Value = courseDetailItem?.SubjectDetail?.Name;

                            ExcelRange cellHeaderForm = worksheet.Cells[3, type == 1 ? 10 : 9];
                            cellHeaderForm.Value = header.Any() ? header[0] : string.Empty;
                            ExcelRange cellHeaderForm1 = worksheet.Cells[4, type == 1 ? 10 : 9];
                            cellHeaderForm1.Value = header?.Count() > 1 ? header[1] : string.Empty;
                            ExcelRange cellHeaderForm2 = worksheet.Cells[5, type == 1 ? 10 : 9];
                            cellHeaderForm2.Value = header?.Count() > 2 ? header[2] : string.Empty;


                            var detai_instructor = _repoCourse_detail_instructor.GetAll(a => a.Course_Detail_Id == courseDetailItem.Id);
                            var instruc = "";
                            if (detai_instructor.Count() != 0)
                            {
                                foreach (var item in detai_instructor)
                                {
                                    instruc += item?.Trainee?.LastName + " " + item?.Trainee?.FirstName + ", ";
                                }
                            }
                            instruc = instruc.Remove(instruc.Length - 2);
                            ExcelRange cellHeaderInstructoor = worksheet.Cells[startrow + 2, 3];
                            cellHeaderInstructoor.Value = instruc;

                            var datefrom = DateUtil.DateToString(courseDetailItem?.dtm_time_from, "dd/MM/yyyy");
                            var dateto = DateUtil.DateToString(courseDetailItem?.dtm_time_to, "dd/MM/yyyy");
                            ExcelRange cellHeaderDate = worksheet.Cells[startrow + 2, type == 1 ? 7 : 6];
                            cellHeaderDate.Value = datefrom + "-" + dateto;

                            ExcelRange cellHeaderVenue = worksheet.Cells[startrow + 3, 3];
                            cellHeaderVenue.Value = courseDetailItem?.Course?.Venue;

                            #region Write Details
                            startrow = 14;
                            int row = 1;


                            int rowheader = 0;
                            int rowDetail = 0;
                            int countplus = 0;
                            var no = 1;
                            int col = 2;
                            var count_rank = 0;
                            int collimit = 0;
                            collimit = type == 1 ? 11 : 10;
                            #region [-- response_rank ---]
                            foreach (var item in qs)
                            {

                                var name_type = item["name_type"].ToObject<string>();
                                var no_header = no + ".";
                                if (name_type == "response_rank")
                                {
                                    countplus++;

                                    worksheet.Cells[rowheader + startrow + rowDetail + 1, col, rowheader + startrow + rowDetail + 1, collimit].Merge = true;
                                    ExcelRange cellNo1 = worksheet.Cells[rowheader + startrow + rowDetail + 1, col];
                                    cellNo1.Value = no_header + " " + item["name"];
                                    cellNo1.Style.Font.Bold = true;
                                    cellNo1.Style.Font.Size = 11;
                                    var num = 1;
                                    startrow++;
                                    foreach (var item1 in item["choise"])
                                    {
                                        var no_header1 = no + "." + num;
                                        if (type == 1)
                                        {
                                            var count = 0;
                                            var count1 = 0;
                                            var count2 = 0;
                                            var count3 = 0;
                                            var count4 = 0;
                                            double pecent = 0;
                                            double pecent1 = 0;
                                            double pecent2 = 0;
                                            double pecent3 = 0;
                                            double pecent4 = 0;
                                            var avg = "";

                                            if (list_trainee != null)
                                            {
                                                foreach (var item2 in list_trainee)
                                                {
                                                    foreach (var item3 in item2["response_rank"])
                                                    {
                                                        var xxx = item3["choice_id"].Value<int>();
                                                        var yyy = item1["id_choice"].Value<int>();
                                                        var zzz = item["id_question"].Value<int>();
                                                        var iii = item3["question_id"].Value<int>();

                                                        if (xxx == yyy && zzz == iii)
                                                        {
                                                            int value_rank = Convert.ToInt32(item3["rank"].ToString());
                                                            switch (value_rank)
                                                            {
                                                                case 4:
                                                                    count++;
                                                                    break;
                                                                case 3:
                                                                    count1++;
                                                                    break;
                                                                case 2:
                                                                    count2++;
                                                                    break;
                                                                case 1:
                                                                    count3++;
                                                                    break;
                                                                case 0:
                                                                    count4++;
                                                                    break;
                                                                default:
                                                                    count = 0;
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                    double sum = count + count1 + count2 + count3 + count4;
                                                    double total = (count * 5) + (count1 * 4) + (count2 * 3) + (count3 * 2) + count4;
                                                    avg = (total / sum).ToString("N1");
                                                    pecent = Math.Round(count * (100 / sum));
                                                    pecent1 = Math.Round(count1 * (100 / sum));
                                                    pecent2 = Math.Round(count2 * (100 / sum));
                                                    pecent3 = Math.Round(count3 * (100 / sum));
                                                    pecent4 = Math.Round(count4 * (100 / sum));
                                                }
                                            }

                                            worksheet.Cells[
                                                rowheader + startrow + rowDetail + 1, col, rowheader + startrow + rowDetail + 1,
                                                col + 3].Merge = true;
                                            ExcelRange cellquestion = worksheet.Cells[rowheader + startrow + rowDetail + 1, col];
                                            cellquestion.Value = no_header1 + " " + item1["content"];

                                            ExcelRange cellanswer1 =
                                                worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 4];
                                            cellanswer1.Value = count == 0 ? "" : count + " (" + pecent + "%" + ")";
                                            cellanswer1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                            cellanswer1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                            ExcelRange cellanswer2 =
                                                worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 5];
                                            cellanswer2.Value = count1 == 0 ? "" : count1 + " (" + pecent1 + "%" + ")";
                                            cellanswer2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                            cellanswer2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                            ExcelRange cellanswer3 =
                                                worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 6];
                                            cellanswer3.Value = count2 == 0 ? "" : count2 + " (" + pecent2 + "%" + ")";
                                            cellanswer3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                            cellanswer3.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                            ExcelRange cellanswer4 =
                                                worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 7];
                                            cellanswer4.Value = count3 == 0 ? "" : count3 + " (" + pecent3 + "%" + ")";
                                            cellanswer4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                            cellanswer4.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                            ExcelRange cellanswer5 =
                                                worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 8];
                                            cellanswer5.Value = count4 == 0 ? "" : count4 + " (" + pecent4 + "%" + ")";
                                            cellanswer5.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                            cellanswer5.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                                            ExcelRange cellrank = worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 9];
                                            cellrank.Value = avg;
                                            cellrank.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                            cellrank.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                        }
                                        else if (type == 2)
                                        {
                                            worksheet.Cells[
                                               rowheader + startrow + rowDetail + 1, col, rowheader + startrow + rowDetail + 1,
                                               col + 3].Merge = true;
                                            ExcelRange cellquestion = worksheet.Cells[rowheader + startrow + rowDetail + 1, col];
                                            cellquestion.Value = no_header1 + " " + item1["content"];
                                            var check_rank = response_rank?[count_rank];
                                            var xxx = item1["id_question"].Value<int>();
                                            var yyy = item1["id_choice"].Value<int>();
                                            var zzz = check_rank["question_id"].Value<int>();
                                            var iii = check_rank["choice_id"].Value<int>();
                                            if (xxx == zzz && yyy == iii)
                                            {


                                                var cvt = new FontConverter();
                                                string s = cvt.ConvertToString("");
                                                Font f = cvt.ConvertFromString(s) as Font;

                                                var value_rank = check_rank?["rank"].Value<int>() ?? -1;
                                                switch (value_rank)
                                                {
                                                    case 4:
                                                        ExcelRange cellanswer1 = worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 4];
                                                        cellanswer1.Value = "x";
                                                        cellanswer1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                                        cellanswer1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                                        cellanswer1.Style.Font.Bold = true;
                                                        break;
                                                    case 3:
                                                        ExcelRange cellanswer2 = worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 5];
                                                        cellanswer2.Value = "x";
                                                        cellanswer2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                                        cellanswer2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                                        cellanswer2.Style.Font.Bold = true;
                                                        break;
                                                    case 2:
                                                        ExcelRange cellanswer3 = worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 6];
                                                        cellanswer3.Value = "x";
                                                        cellanswer3.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                                        cellanswer3.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                                        cellanswer3.Style.Font.Bold = true;
                                                        break;
                                                    case 1:
                                                        ExcelRange cellanswer4 = worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 7];
                                                        cellanswer4.Value = "x";
                                                        cellanswer4.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                                        cellanswer4.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                                        cellanswer4.Style.Font.Bold = true;
                                                        break;
                                                    case 0:
                                                        ExcelRange cellanswer5 = worksheet.Cells[rowheader + startrow + rowDetail + 1, col + 8];
                                                        cellanswer5.Value = "x";
                                                        cellanswer5.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                                        cellanswer5.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                                        cellanswer5.Style.Font.Bold = true;
                                                        break;
                                                    default:

                                                        break;
                                                }
                                            }

                                        }
                                        num++;
                                        rowDetail++;
                                        count_rank++;
                                    }
                                    no++;

                                }

                            }
                            #endregion


                            ExcelRange r = worksheet.Cells[15, 2, rowheader + startrow + rowDetail, collimit];
                            r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                            r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                            r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);


                            #region [-- response_text ---]
                            foreach (var item in qs)
                            {
                                var name_type = item["name_type"].ToObject<string>();
                                if (name_type == "response_text")
                                {

                                    var html = item["content"].Value<string>();
                                    worksheet.Cells[rowheader + startrow + rowDetail + 2, col, rowheader + startrow + rowDetail + 2, collimit].Merge = true;
                                    ExcelRange cellquestion = worksheet.Cells[rowheader + startrow + rowDetail + 2, col];
                                    cellquestion.Value = StripTagsCharArray(html);
                                    cellquestion.Style.Font.Bold = true;
                                    cellquestion.Style.Font.Size = 11;
                                    startrow++;
                                    if (type == 1)
                                    {
                                        if (list_trainee != null)
                                        {
                                            foreach (var item2 in list_trainee)
                                            {
                                                var traineeEId = item2["userid"].ToObject<string>();
                                                var traineeId = _repoTrainee.Get(a => a.str_Staff_Id.ToLower() == traineeEId && !(bool)a.IsDeleted);
                                                if (traineeId != null)
                                                {
                                                    foreach (var item3 in item2["response_text"])
                                                    {

                                                        var zzz = item["id_question"].Value<int>();
                                                        var iii = item3["question_id"].Value<int>();
                                                        if (zzz == iii)
                                                        {
                                                            ExcelRange cellanswer1 = worksheet.Cells[rowheader + startrow + rowDetail + 2, col];
                                                            cellanswer1.Value = traineeId?.LastName + " " + traineeId?.FirstName;

                                                            ExcelRange cellanswer2 = worksheet.Cells[rowheader + startrow + rowDetail + 2, col + 1];
                                                            cellanswer2.Value = StripTagsCharArray(item3["response"].Value<string>());
                                                            rowDetail++;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (type == 2)
                                    {
                                        if (response_text.Any())
                                        {
                                            ExcelRange cellanswer1 = worksheet.Cells[rowheader + startrow + rowDetail + 2, col];
                                            cellanswer1.Value = StripTagsCharArray(response_text[0]["response"].Value<string>());
                                            rowDetail++;
                                        }

                                    }

                                }
                            }
                            #endregion

                            worksheet.Cells[rowheader + startrow + rowDetail + 3, col, rowheader + startrow + rowDetail + 3, collimit].Merge = true;
                            ExcelRange cellfooter = worksheet.Cells[rowheader + startrow + rowDetail + 3, col];
                            cellfooter.Value = "Thank you for taking the time to help us improve our training.";
                            cellfooter.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cellfooter.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cellfooter.Style.Font.Bold = true;
                            cellfooter.Style.Font.Italic = true;

                            #endregion
                            Bytes = xlPackage.GetAsByteArray();
                        }
                    }
                }

            }



            return Bytes;
        }

        public static string StripTagsCharArray(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        #endregion
    }
}