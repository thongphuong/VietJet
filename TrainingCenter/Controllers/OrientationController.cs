using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DAL.Entities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Jobtitle;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Orientation;
using TMS.Core.Services.Users;
using TMS.Core.Services.TraineeHis;
using TMS.Core.Utils;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Orientation;
using System.Text;
using DAL.Repositories;
using TMS.Core.ViewModels.TraineeHistory;
using Microsoft.Reporting.WebForms;
using TrainingCenter.Utilities;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TMS.Core.Services;
using System.Data.Entity.SqlServer;
using System.Configuration;
using TMS.Core.App_GlobalResources.Orientation;
using TMS.Core.Services.Approves;
using TMS.Core.ViewModels.AjaxModels;
using TMS.Core.ViewModels.Courses;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TrainingCenter.Controllers
{
    public class OrientationController : BaseAdminController
    {
        private readonly IJobtitleService _jobtitleService;

        private readonly IOrientationService _orientationService;
        private readonly IEmployeeService _employeeService;

        // GET: Orientation
        public OrientationController(IJobtitleService jobtitleService, IOrientationService orientationService, IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService , IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService,  approveService)
        {
            _jobtitleService = jobtitleService;
            _orientationService = orientationService;
            _employeeService = employeeService;
        }


        #region [------------------------------ My PDP---------------------------------]
        
        public ActionResult Index()
        {
            var model = new OrientationListPDPView
            {
                Jobtitle = _jobtitleService.Get().ToDictionary(a => a.Id, a => a.Name)
            };
            return View(model);
        }

        public ActionResult AjaxHandlerPDP(jQueryDataTableParamModel param)
        {
            try
            {
                var traineeCode = string.IsNullOrEmpty(Request.QueryString["TraineeCode"]) ? string.Empty : Request.QueryString["TraineeCode"].ToLower().Trim();
                var traineeName = string.IsNullOrEmpty(Request.QueryString["TraineeName"]) ? string.Empty : Request.QueryString["TraineeName"].ToLower().Trim();
                var jobTitleId = string.IsNullOrEmpty(Request.QueryString["TraineeCode"]) ? -1 : Convert.ToInt32(Request.QueryString["TraineeCode"].Trim());


                var data = EmployeeService.GetAllPdp(a => a.Status == (int)UtilConstants.StatusApiApprove.Pending &&
                                                        (string.IsNullOrEmpty(traineeCode) || a.Trainee.str_Staff_Id.Trim().ToLower() == traineeCode) &&
                                                        (string.IsNullOrEmpty(traineeName) || a.Trainee.str_Staff_Id.Trim().ToLower() == traineeName) &&
                                                        (jobTitleId == -1 || jobTitleId == a.JobTitleId)
                                                        ).ToArray();
                var verticalBar = GetByKey("VerticalBar");
                var filtered = data.Select(a => new AjaxTraineeFuture
                {
                    Code = a.Trainee.str_Staff_Id,
                    //FullName = a.Trainee.FirstName.Trim() + " " + a.Trainee.LastName.Trim(),
                    FullName = ReturnDisplayLanguage(a.Trainee.FirstName,a.Trainee.LastName),
                    JobTitle = a.JobTitle.Name,
                    Position = a.Position.ToString(),
                    Date = a.CreationDate.ToString(),
                    Option = "<a href='javascript:void(0)' title='"+ Resource.ALERT_APPROVE + "'  onclick='actionModifyForm("+a.TraineeId+ ")' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i></a><input type='hidden' id='Trainee_" + a.TraineeId + "' value='" + a.TraineeId + "' /><input type='hidden' id='Jobtitle_" + a.TraineeId + "' value='" + a.JobTitleId + "' /><input type='hidden' id='Position_" + a.TraineeId + "' value='" + a.Position + "' />" + verticalBar +
                             "<a href='javascript:void(0)' title='" + Resource.ALERT_REJECT + "'  onclick='actionReject(" + a.Id+ ")' data-toggle='tooltip'><i class='fa fa-times' aria-hidden='true'></i></a>"
                });

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxTraineeFuture, string> orderingFunction = (
                    c =>
                    sortColumnIndex == 1 ? c.Code :
                    sortColumnIndex == 2 ? c.FullName :
                    sortColumnIndex == 3 ? c.JobTitle :
                    sortColumnIndex == 4 ? c.Position :
                    c.Date);
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
                              c?.Code,
                              c?.FullName,
                              c?.JobTitle,
                              c?.Position,
                              c?.Option

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandlerPDP", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                },
                    JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult RejectPdp(int? id)
        {
            try
            {
                var model = CourseService.GetTraineeFutureById(id);
                if (model == null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //var name = model.Trainee.str_Staff_Id.Trim() + " - " + (model.Trainee.FirstName.Trim() + " " + model.Trainee.LastName.Trim());
                var name = model.Trainee.str_Staff_Id.Trim() + " - " + (ReturnDisplayLanguage(model.Trainee.FirstName,model.Trainee.LastName));
                if (model.TraineeFuture_Item.Any())
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, name),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                model.Status = (int)UtilConstants.StatusApiApprove.Reject;
                model.ModifyDate = DateTime.Now;
                model.ModifyBy = CurrentUser.Username;
                CourseService.UpdateTraineeFuture(model);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(MessageOrientation.REJECT_SUCCESSFULLY, name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/RejectPdp", ex.Message);
                return Json(new
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult ModifyFormPdp(int? traineeId, int? jobFutureId, int? position)
        {
            var model = new OrientationPDPViewModel();
            var jobTitleFutureId = jobFutureId ?? -1;
            var trainee = _employeeService.GetById(traineeId);
            model.Avatar = trainee.avatar;
            //model.FullName = trainee.FirstName.Trim() + " " + trainee.LastName.Trim();
            model.FullName = ReturnDisplayLanguage(trainee.FirstName, trainee.LastName);
            model.TraineeId = trainee.Id;
            model.TraineeCode = trainee.str_Staff_Id;
            model.Passport = trainee.Passport;
            model.PersonId = trainee.PersonalId;
            model.Gender = UtilConstants.GenderDictionary()[trainee.Gender ?? 3];
            model.DateOfJoin = trainee.dtm_Birthdate?.ToString("dd/MM/yyyy") ?? string.Empty;
            model.PlaceOfBirth = trainee.str_Place_Of_Birth;
            model.Email = trainee.str_Email;
            model.Nationality = trainee.Nation;
            model.Phone = trainee.str_Cell_Phone;
            model.DateOfJoin = trainee.dtm_Join_Date?.ToString("dd/MM/yyyy") ?? string.Empty;
            model.JobTitleDefaultName = trainee.JobTitle.Name;
            model.JobTitleId = trainee.Job_Title_id ?? -1;
            model.Type =
                UtilConstants.CourseCourseAreasDictionary()[
                    (trainee.bit_Internal==true
                        ? (int)UtilConstants.CourseAreas.Internal
                        : (int)UtilConstants.CourseAreas.External)];
            model.DepartmentName = trainee.Department.Code + " - " + trainee.Department.Name;
            model.Company = trainee.Company != null ? trainee.Company.str_Name : string.Empty;

            model.JobFutureId = jobTitleFutureId;

            var jobfuture = _jobtitleService.GetById(jobTitleFutureId);

            model.JobTitleFutureName = jobfuture.Name ?? string.Empty;
            model.OrientationKindOfSuccessor = _orientationService.GetKind().ToDictionary(a => a.Id, a => a.Name);
            model.Position = position.ToString();

            return PartialView("_partials/_ModifyFormPDP", model);
        }

        [HttpPost]
        public ActionResult ModifyFormTraineeFuture(OrientationPDPViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = GetUser();
                    _orientationService.ModifyOrientationPDP(model,  model.TraineeId, model.JobTitleId, model.ExpectedDate, model.JobFutureId);
                    return Json(new AjaxResponseViewModel { result = true, message =  MessageOrientation.APPROVED_SUCCESSFULLY , data = "1" });
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/ModifyFormTraineeFuture", ex.Message);
                    return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
                }
            }
            var msg = MessageInvalidData(ModelState);
            return Json(new AjaxResponseViewModel { result = false, message = msg });
        }



        #endregion



        public ActionResult AjaxHandleListOrientation(jQueryDataTableParamModel param)
        {
            try
            {
                var name = string.IsNullOrEmpty(Request.QueryString["fullname"]) ? string.Empty : Request.QueryString["fullname"].ToLower().Trim();
                var jobcurent = string.IsNullOrEmpty(Request.QueryString["current"]) ? -1 : Convert.ToInt32(Request.QueryString["current"].Trim());
                var jobfuture = string.IsNullOrEmpty(Request.QueryString["furture"]) ? -1 : Convert.ToInt32(Request.QueryString["furture"].Trim());
                var kind = string.IsNullOrEmpty(Request.QueryString["KindOfSuccessor"]) ? -1 : Convert.ToInt32(Request.QueryString["KindOfSuccessor"].Trim());
                string fSearchDate = string.IsNullOrEmpty(Request.QueryString["fExpiryDate"]) ? string.Empty : Request.QueryString["fExpiryDate"].Trim();
                DateTime expirydate;
                DateTime.TryParse(fSearchDate, out expirydate);
                expirydate = expirydate != DateTime.MinValue ? expirydate.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : expirydate;

                var data = _orientationService.Get(
                    a =>
                      (string.IsNullOrEmpty(name) || ((a.Trainee.FirstName.Trim() + " " + a.Trainee.LastName.Trim()).Contains(name.Trim())))
                         && (jobfuture == -1 || a.JobFutureId == jobfuture)
                         && (jobcurent == -1 || a.JobHistoryId == jobcurent)
                         && (expirydate == DateTime.MinValue || SqlFunctions.DateDiff("day", a.ExpectedDate, expirydate) >= 0)
                         && (kind == -1 || a.Orientation_Kind_Of_Successor.Id == kind));

                IEnumerable<Orientation> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Orientation, string> orderingFunction = (
                    c =>
                    sortColumnIndex == 1 ? c.Trainee.LastName :
                    sortColumnIndex == 2 ? c.JobTitle.Name :
                    sortColumnIndex == 3 ? c.JobTitle.Name :
                    sortColumnIndex == 4 ? c.Orientation_Kind_Of_Successor.Name :

                    sortColumnIndex == 6 ? c.ExpectedDate.ToString() :
                    sortColumnIndex == 7 ? c.CreateDay.ToString() :
                    sortColumnIndex == 8 ? c.Remark :
                    c.CreateDay.ToString());
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
                       // "<a href='javascript:void(0)' onclick='clickchange("+c.Trainee.Id+")'>" + c.Trainee?.FirstName + " " + c.Trainee?.LastName +"</a>",
                        "<a href='javascript:void(0)' onclick='clickchange("+c.Trainee.Id+")'>" + (ReturnDisplayLanguage(c.Trainee?.FirstName,c.Trainee?.LastName)) +"</a>",
                        c.Trainee.JobTitle.Name,
                        c.JobTitle.Name,
                        c.Orientation_Kind_Of_Successor.Name,
                        loadSubject(c.Id),
                        DateTime.Parse(c?.ExpectedDate.ToString()).ToShortDateString() + "",
                        //c.CreateDay + "/" + c.USER.FIRSTNAME.Trim() +" " + c.USER.LASTNAME.Trim(),
                        c.CreateDay + "/" + ReturnDisplayLanguage(c.USER.FIRSTNAME,c.USER.LASTNAME),
                        c.Remark,
                        ""

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandleListOrientation", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AjaxHandleListOrientationInJob(jQueryDataTableParamModel param)
        {
            try
            {
                var name = string.IsNullOrEmpty(Request.QueryString["fullname"]) ? string.Empty : Request.QueryString["fullname"].ToLower().Trim();
                var job = string.IsNullOrEmpty(Request.QueryString["job_position"]) ? -1 : Convert.ToInt32(Request.QueryString["job_position"].Trim());
                string fSearchDate = string.IsNullOrEmpty(Request.QueryString["fExpiryDate"]) ? string.Empty : Request.QueryString["fExpiryDate"].Trim();
                DateTime expirydate;
                DateTime.TryParse(fSearchDate, out expirydate);
                expirydate = expirydate != DateTime.MinValue ? expirydate.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : expirydate;

                var data = _orientationService.Get(
                   a => (a.JobTitle.Id == job || job == -1)
                       &&
                     (string.IsNullOrEmpty(name) || ((a.Trainee.FirstName.Trim() + " " + a.Trainee.LastName.Trim()).Contains(name.Trim())))
                       && (expirydate == DateTime.MinValue || SqlFunctions.DateDiff("day", a.ExpectedDate, expirydate) >= 0))
                       .GroupBy(a => new { a.ExpectedDate, a.JobFutureId, a.IdKindOfSuccessor }).Select(b => b.FirstOrDefault());

                IEnumerable<Orientation> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Orientation, string> orderingFunction = (
                    c =>
                    sortColumnIndex == 0 ? c.Trainee.JobTitle.Code :
                    sortColumnIndex == 1 ? c.Trainee.JobTitle.Name :
                    sortColumnIndex == 5 ? c.ExpectedDate.ToString() :
                    sortColumnIndex == 6 ? c.CreateDay.ToString() :
                    sortColumnIndex == 7 ? c.Remark :
                    c.CreateDay.ToString());

                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection != null)
                {
                    filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                        : filtered.OrderByDescending(orderingFunction);
                }


                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);



                var result = from c in displayed
                             select new object[] {
                        c.JobTitle?.Code,
                        "<a href='javascript:void(0)' onclick='clickchange("+c.JobTitle.Id+")'>" + c.JobTitle?.Name+"</a>",
                        c.IdKindOfSuccessor == 1 ? loadEmp(c.JobFutureId,1,c.ExpectedDate) : "",
                        c.IdKindOfSuccessor == 2 ? loadEmp(c.JobFutureId,2,c.ExpectedDate) : "",
                        c.IdKindOfSuccessor == 3 ? loadEmp(c.JobFutureId,3,c.ExpectedDate) : "",
                        DateTime.Parse(c.ExpectedDate.ToString()).ToShortDateString() + "",
                        c.CreateDay + "/" + c.USER.FIRSTNAME.Trim() +" " + c.USER.LASTNAME.Trim(),
                        c.Remark,
                        "",
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandleListOrientationInJob", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult ModifyOrientation(FormCollection form)
        {
            try
            {
                var user = GetUser();
                var Remark = form.GetValues("Remark");
                var OrientationId = string.IsNullOrEmpty(form["OrientationId"]) ? -1 : Convert.ToInt32(form["OrientationId"]);
                var TraineeId = string.IsNullOrEmpty(form["TraineeId"]) ? -1 : Convert.ToInt32(form["TraineeId"]);
                var JobHistoryId = string.IsNullOrEmpty(form["JobHistoryId"]) ? -1 : Convert.ToInt32(form["JobHistoryId"]);
                var JobFutureId = string.IsNullOrEmpty(form["JobFutureId"]) ? -1 : Convert.ToInt32(form["JobFutureId"]);

                if (TraineeId != -1 && JobHistoryId != -1 && JobFutureId != -1)
                {
                    Orientation entity = new Orientation
                    {
                        TraineeId = TraineeId,
                        JobHistoryId = JobHistoryId,
                        JobFutureId = JobFutureId,
                        CreateDay = DateTime.Now,
                        CreateBy = user.USER_ID,
                        ExpectedDate = DateTime.Now,
                        Remark = "ssss"
                    };
                    _orientationService.Insert(entity);

                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.SUCCESS,
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/ModifyOrientation", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.UNSUCCESS + ": " + ex,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ModifyForm(int? traineeId, int? jobFID)
        {
            ViewBag.JobID = jobFID;
            var model = new OrientationModifyViewModel
            {
                Trainee = _employeeService.GetById(traineeId),
                OrientationKindOfSuccessorList = _orientationService.GetKind().Select(a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Name, Value = a.Name })
            };
            _orientationService.Get();
            return PartialView("_partials/_ModifyForm", model);
        }

       
        [HttpPost]
        public ActionResult ModifyForm(OrientationModifyViewModel model, FormCollection f)
        {
            var user = GetUser();
            var idtrainee = f["empID"].ToString();
            var idjob = f["jobID"].ToString();
            var expectDate = f["ExpectedDate"].ToString();
            var data = new OrientationModifyViewModel
            {
                Trainee = _employeeService.GetById(int.Parse(idtrainee)),
            };
            if (ModelState.IsValid)
            {
                try
                {
                    var headerid = _orientationService.ModifyOrientation(model, int.Parse(idtrainee), data.Trainee.Job_Title_id, DateTime.Parse(expectDate.ToString()), int.Parse(idjob));
                    return Json(new AjaxResponseViewModel() { result = true, message = "Insert success", data = "1" });
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/ModifyForm", ex.Message);
                    return Json(new { result = false, message = ex.Message });
                }
            }
            var msg = MessageInvalidData(ModelState);
            return Json(new { result = false, message = msg });
        }

        [HttpGet]
        public ActionResult ModifyForms(int idJob)
        {
            ViewBag.JobID = idJob;
            var data = _jobtitleService.GetById(idJob);
            var model = new OrientationModifyViewModel()
            {
                Trainee = _employeeService.GetById(),
                Subjects = data.Title_Standard.Select(a => new CustomDataListModel() { Key = a.SubjectDetail.Id, Value = a.SubjectDetail.Name }),
                OrientationKindOfSuccessorList = _orientationService.GetKind().Select(a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Name, Value = a.Name })
            };
            var subjects = model.Subjects.Select(a => (int)a.Key);

            var trainee = CourseService.GetCourseResultFinal(
                a => a.IsDeleted==false && (a.grade == null || a.grade > 0) && a.Course.IsActive == true && a.Course.IsDeleted != true && a.Course.Course_Detail.Any(x => x.IsDeleted==false && subjects.Any(z => z == x.SubjectDetailId))).AsEnumerable().Select(a => new
                {
                    a.traineeid,
                   // FullName = GetByKey("DisplayLanguage") == "vi" ? a.Trainee.FirstName + " " + a.Trainee.LastName : a.Trainee.LastName + " " + a.Trainee.FirstName,
                    FullName = ReturnDisplayLanguage(a.Trainee.FirstName,a.Trainee.LastName),
                    subjects = a.Course.Course_Detail.Where(
                    x => x.IsDeleted==false).Select(x => x.SubjectDetailId),
                    a.Trainee
                });
            model.TrainedEmployee = trainee
                .GroupBy(
                    a => new
                    {
                        Id = a.traineeid,
                        FullName = a.FullName,
                        a.Trainee,
                    },
                    b => b.subjects.Distinct(),
                    (a, b) => new OrientationJobCoincideTrainee()
                    {
                        FullName = a.FullName,
                        Id = a.Id,
                        AmountSubjects = b.Sum(z => z.Count()),
                        Employee = a.Trainee,
                    }
                ).OrderByDescending(a => a.AmountSubjects);

            _orientationService.Get();
            return PartialView("_partials/_ModifyForms", model);
        }

        [HttpPost]
        public ActionResult ModifyForms(OrientationModifyViewModel model, FormCollection f)
        {
            var idjob = f["jobID"];
            var expectDate = f["ExpectedDate"];

            var AbilitiesList = f.AllKeys
                                    .Where(k => k.StartsWith("Abilities_"))
                                    .Select(k => f[k]);

            if (ModelState.IsValid)
            {
                try
                {
                    if (AbilitiesList != null) { }
                    foreach (var ability in AbilitiesList)
                    {
                        var id = int.Parse(ability);
                        var jobid = _employeeService.Get(a => a.Id == id).Select(a => a.Job_Title_id).FirstOrDefault();
                        var headerid = _orientationService.ModifyOrientation(model,  id, jobid, DateTime.Parse(expectDate), int.Parse(idjob));
                    }
                    return Json(new AjaxResponseViewModel() { result = true, message = "Insert success", data = "1" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/ModifyForms", ex.Message);
                    return Json(new { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var msg = MessageInvalidData(ModelState);
            return Json(new { result = false, message = msg },JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult ModifyForm_New(List<OrientationModify> model,int? id, int? jobfuture)
        {
            try
            {
                _orientationService.ModifySuccessor(model, id, jobfuture);
                return Json(new AjaxResponseViewModel() { result = true, message = "Insert success" });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/ModifyForm", ex.Message);
                return Json(new { result = false, message = ex.Message });
            }
            
        }
        #region Orientation Emp

        public ActionResult OrientationEmp(int? id)
        {
            var model = new OrientationViewModel
            {
                //EmpList =
                //    EmployeeService.Get().OrderByDescending(a => a.str_Staff_Id)
                //        .ToDictionary(a => a.Id, a => a.str_Staff_Id + " - " + a.FirstName + " " + a.LastName),
                EmpList =
                    EmployeeService.Get(true)
                        .ToDictionary(a => a.Id, a => a.str_Staff_Id + " - " + (ReturnDisplayLanguage(a.FirstName, a.LastName))),
                OrientationKindOfSuccessorList = _orientationService.GetKind().OrderBy(a => a.Name).Select(a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Name, Value = a.Name }),
                GroupTrainees = EmployeeService.GetAllGroupTrainees(a => a.IsActived == true).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name)),
                Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster),
                JobTitles = _jobtitleService.Get().OrderBy(a => a.Name.Trim()).ToDictionary(a => a.Id, a => a.Name),
            };
            if(id.HasValue)
            {
                model.Id = id;
                var record = _orientationService.GetbyId(id);
                if(record != null)
                {
                    model.OrientationModify = record.PotentialSuccessors_Item.Select(a => new OrientationModify() {
                        EmployeeID = a.TraineeId ?? 0,
                        EmployeeName = a.TraineeId.HasValue ? ReturnDisplayLanguage(a.Trainee?.FirstName,a.Trainee?.LastName) : string.Empty,
                        EmployeeEID = a.TraineeId.HasValue ? a.Trainee?.str_Staff_Id : string.Empty,
                        JobTitleName = a.JobHistoryId.HasValue ? a.JobTitle?.Name: string.Empty,
                        JobTitleID = a.JobHistoryId ?? 0,
                        FuturePositionID = a.PotentialSuccessor?.JobtitleId ?? 0,
                        JobTitleFutureName = a.SuccessorsID.HasValue ? a.PotentialSuccessor?.JobTitle?.Name : string.Empty,
                        SelectedValue = a.IsActive ?? false,
                        Status = a.Status,
                    });
                    model.JobFuture = record.JobtitleId ?? 0;
                }

               
            }
            else
            {
                model.Id = 0;
            }
            StringBuilder html = new StringBuilder();
            html.Append("<option value=''></option>");
            var list = EmployeeService.Get(true);
            foreach (var item in list)
            {
                var fullName = ReturnDisplayLanguage(item.FirstName, item.LastName);
                html.AppendFormat(
                                    "<option  value='{0}'>{1}</option>",
                                    item.Id, fullName);
            }
            model.EmpListCustom = html.ToString();
            return View(model);
        }
        [HttpPost]
        public ActionResult OrientationEmpFilter(int? idEmp)
        {
            try
            {
                var model = new OrientationViewModel
                {
                    JobList = _jobtitleService.Get(),
                    Employee = EmployeeService.GetById(idEmp)

                };
                var jobId = EmployeeService.GetById(idEmp)?.Job_Title_id;
                return Json(new AjaxResponseViewModel { data = jobId, result = true });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/OrientationEmpFilter", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false });
            }
        }
        public ActionResult OrientationEmpListJob(int? jobFutureId, int? idsuccessor)
        {
            var model = new OrientationViewModel
            {
                JobList = _jobtitleService.Get().OrderBy(a=>a.Name.Trim()),
                JobFuture = jobFutureId ?? 0,
                Id = idsuccessor ?? 0,
            };
            return PartialView("_Partials/_OrientationEmpListJob", model);
        }
        public ActionResult OrientationEmpListsubject(int? idJob, int? idEmp)
        {
            var final = CourseService.GetCourseResultFinal(a => a.traineeid == idEmp && a.Course.IsDeleted != true && a.Course.IsActive == true);
            var getCourseid = final.Select(a => a.courseid);
            var detail = CourseDetailService.Get(a => getCourseid.Contains(a.CourseId));
            var courseidAssign = detail.Where(a => a.Course.TMS_APPROVES.Any(b => b.int_Course_id == a.CourseId && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.AssignedTrainee)).Select(a => (int)a.SubjectDetailId).ToList();
            var getCourseidFinal = final.Where(a => a.grade != null && a.grade > 0).Select(a => a.courseid);
            var courseidFinal = detail.Where(a => a.Course.TMS_APPROVES.Any(b => b.int_Course_id == a.CourseId && b.int_id_status == (int)UtilConstants.EStatus.Approve && b.int_Type == (int)UtilConstants.ApproveType.CourseResult)).Select(a => (int)a.SubjectDetailId).ToList();

            var model = new OrientationViewModel
            {
                TitleStandard = _jobtitleService.GetById((int)idJob)?.Title_Standard,
                CourseResultFinals = CourseService.GetCourseResultFinal(),
                ListSubjectAssign = courseidAssign,
                ListSubjectFinal = courseidFinal
            };
            return PartialView("_Partials/_OrientationEmpListsubject", model);
        }

        public FileResult ExportOrientationEmpEXCEL(FormCollection form)
        {
            var filecontent = ExportOrientationEmpEXCEL();
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "PotentialSuccessors.xlsx");
            }
            return null;
        }

        private byte[] ExportOrientationEmpEXCEL()
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "ExcelFile/PotentialSuccessors.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            //var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);

            var data = _orientationService.Get();
            // _courseServiceMember.Get(a => a.Course_Details_Id == ddlSubject && a.DeleteApprove == null && a.IsActive && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approve)).ToList();
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            if (data != null)
            {
                ExcelPackage excelPackage;
                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 9;
                    //var Row = 0;
                    int count = 0;
                    foreach (var item in data)
                    {
                        int col = 2;
                        count++;
                        ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                        cellNo.Value = count;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);





                        //var fullName = item?.Trainee?.FirstName + " " + item?.Trainee?.LastName;
                        var fullName = ReturnDisplayLanguage(item?.Trainee?.FirstName,item?.Trainee?.LastName);
                        ExcelRange cellFullName = worksheet.Cells[startRow + 1, ++col];
                        cellFullName.Value = fullName;
                        cellFullName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellFullName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellFullName.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        ExcelRange cellTraineeJobtitleName = worksheet.Cells[startRow + 1, ++col];
                        cellTraineeJobtitleName.Value = item?.Trainee?.JobTitle?.Name;
                        cellTraineeJobtitleName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTraineeJobtitleName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellTraineeJobtitleName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellJobtitleName = worksheet.Cells[startRow + 1, ++col];
                        cellJobtitleName.Value = item?.JobTitle.Name;
                        cellJobtitleName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellJobtitleName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellJobtitleName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellKindOfSuccessor = worksheet.Cells[startRow + 1, ++col];
                        cellKindOfSuccessor.Value = item?.Orientation_Kind_Of_Successor?.Name;
                        cellKindOfSuccessor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellKindOfSuccessor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellKindOfSuccessor.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellLoadSubject = worksheet.Cells[startRow + 1, ++col];
                        cellLoadSubject.Value = loadSubjectExcel(item.JobTitle.Id, item.Trainee.Id);
                        cellLoadSubject.Style.WrapText = true;
                        cellLoadSubject.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellLoadSubject.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellLoadSubject.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellExpectedDate = worksheet.Cells[startRow + 1, ++col];
                        cellExpectedDate.Value = DateTime.Parse(item?.ExpectedDate?.ToString()).ToShortDateString() + "";
                        cellExpectedDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellExpectedDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellExpectedDate.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellCreateDay = worksheet.Cells[startRow + 1, ++col];
                        cellCreateDay.Value = item?.CreateDay + "/" + item?.USER?.FIRSTNAME + " " + item?.USER?.LASTNAME;
                        cellCreateDay.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCreateDay.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellCreateDay.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellRemark = worksheet.Cells[startRow + 1, ++col];
                        cellRemark.Value = item?.Remark;
                        cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        startRow++;

                    }
                    bytes = excelPackage.GetAsByteArray();
                }
            }
            return bytes;

        }

        private string loadSubjectExcel(int idJob, int? idEmp)
        {
            StringBuilder HTML = new StringBuilder();



            var getCourseid = CourseService.GetCourseResultFinal(a => a.traineeid == idEmp && a.Course.IsDeleted != true && a.Course.IsActive == true).Select(a => a.courseid);

            var subject = CourseDetailService.Get(a => getCourseid.Contains(a.CourseId)).Select(a => a.SubjectDetailId).Distinct();
            var datajobtitle = _jobtitleService.GetById(idJob);
            if (datajobtitle == null)
                return HTML.ToString();

            var data_ = _jobtitleService.GetTitleStandard(a => a.Job_Title_Id == idJob && !subject.Contains(a.Subject_Id));



            foreach (var item in data_)
            {
                HTML.AppendFormat("{0} - {1} \n\r", item.SubjectDetail.Code, item.SubjectDetail.Name);
            }
            return HTML.ToString();
        }
        private string loadSubject(int idOrien)
        {
            StringBuilder HTML = new StringBuilder();

            var dataOrien = _orientationService.GetItem(a => a.OrientationId == idOrien);
            if (dataOrien.Any())
            {
                foreach (var item in dataOrien)
                {
                    HTML.AppendFormat("{0} - {1} </br>", item.SubjectDetail.Code, item.SubjectDetail.Name);
                }
            }
            return HTML.ToString();
        }

        #endregion

        #region Orientation Job

        public ActionResult OrientationJob()
        {
            var model = new OrientationJobViewModel()
            {
                JobList = _jobtitleService.Get().OrderBy(a => a.Name.Trim()),
                JobHeaders = _jobtitleService.GetJobHeader().Select(a => new CustomDataListModel() { Key = a.JobtitleLevelId.HasValue ? a.JobtitleLevelId.ToString() : "", Title = a.Name, Value = a.Id.ToString() }),
                JobLevels = _jobtitleService.GetJobLevel().ToDictionary(a => a.Id, a => a.Name),
                JobPositions = _jobtitleService.GetJobPosition().Select(a => new CustomDataListModel() { Title = a.Name, Value = a.Id.ToString() }),
            };
            return View(model);
        }

        public ActionResult AjaxHandlerListJobTitle(int? jobLevel, int? jobHeader, int? jobPosition)
        {
            try
            {
                var data = _jobtitleService.Get(a =>
                    (jobPosition == null || a.JobPositionId == jobPosition) && (jobHeader == null || a.JobHeaderId == jobHeader) /*&& (jobLevel == null || a.JobtitleHeader.JobtitleLevelId == jobLevel)*/).Select(a => new { a.Id, a.Name });
                return Json(new AjaxResponseViewModel() { result = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandlerListJobTitle", ex.Message);
                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult AjaxHandlerListValidEmployee(int? id)
        {
            try
            {
                if (id != null)
                {
                    var data = _jobtitleService.GetById(id.Value);
                    if (data.IsActive==false)
                    {
                        return Json(new AjaxResponseViewModel() { result = false, message = "Jobtitle is inactive" }, JsonRequestBehavior.AllowGet);
                    }
                    var model = new OrientationJobRouteEmployeeViewModel()
                    {
                        Subjects = data.Title_Standard.Select(a => new CustomDataListModel() { Key = a.SubjectDetail.Id, Value = a.SubjectDetail.Name }),
                    };
                    var subjects = model.Subjects.Select(a => (int)a.Key);



                    var trainee = CourseService.GetCourseResultFinal(
                        a => a.IsDeleted==false && (a.grade == null || a.grade > 0) && a.Course.IsActive == true && a.Course.IsDeleted != true && a.Course.Course_Detail.Any(x => x.IsDeleted==false && subjects.Any(z => z == x.SubjectDetailId))).Select(a => new
                        {
                            a.traineeid,
                            FullName = a.Trainee.FirstName + " " + a.Trainee.LastName,
                            subjects = a.Course.Course_Detail.Where(
                                    x => x.IsDeleted==false).Select(x => x.SubjectDetailId)
                        });
                    model.TrainedEmployee = trainee
                        .GroupBy(
                            a => new
                            {
                                Id = a.traineeid,
                                FullName = a.FullName,
                            },
                           b => b.subjects.Distinct(),
                            (a, b) => new OrientationJobCoincideTrainee()
                            {
                                FullName = a.FullName,
                                Id = a.Id,
                                AmountSubjects = b.Sum(z => z.Count())
                            }
                        ).OrderByDescending(a => a.AmountSubjects);

                    return Json(new AjaxResponseViewModel() { result = true, data = model }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandlerListValidEmployee", Messege.DATA_ISNOTFOUND);
                    return Json(new AjaxResponseViewModel() { result = false, data = null, message = string.Format(Messege.DATA_ISNOTFOUND, Resource.lblJobTitle, "<font color='red' >" + id.Value + "</font>") }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandlerListValidEmployee", ex.Message);
                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        public ActionResult AjaxHandlerTrainedSubject(int? id, int? jobtitleId)//selected Employee Id
        {
            try
            {
                if (id.HasValue && jobtitleId.HasValue)
                {
                    var data = _jobtitleService.GetById(jobtitleId.Value);
                    if (data.IsActive==false)
                    {
                        return Json(new AjaxResponseViewModel() { result = false, message = "Jobtitle is inactive" }, JsonRequestBehavior.AllowGet);
                    }


                    var jobTitleSubjects = data.Title_Standard.Select(a => a.SubjectDetail.Id);

                    var getCourseid = CourseService.GetCourseResultFinal(a => a.traineeid == id && a.IsDeleted==false && a.Course.IsDeleted != true && a.Course.IsActive == true).Select(a => a.courseid).ToArray();

                    var trainingSubject = CourseDetailService.Get(a => a.IsDeleted==false && a.IsActive==true && jobTitleSubjects.Any(x => a.SubjectDetailId == x) && getCourseid.Contains(a.CourseId)).Select(a => new OrientationJobAvalibleSubject()
                    {
                        SubjectId = (int)a.SubjectDetailId,
                        Status = a.Course.Course_Result_Final.Where(b => b.courseid == a.CourseId).Select(c => c.grade.HasValue).FirstOrDefault()
                    }).Distinct();

                    // var counttest = trainingSubject.Count();


                    //var trainingSubject = CourseService.GetCourseResultFinal(a => !a.IsDeleted && a.Course.IsActive == true && (a.grade == null || a.grade > 0) &&
                    //        a.traineeid == id && jobTitleSubjects.Any(x => a.Course.Course_Detail.Any(z => z.IsActive && !z.IsDeleted && x == z.SubjectDetailId)))
                    //    .Select(a => new OrientationJobAvalibleSubject() { SubjectId = a.Course.Course_Detail.Select(x => x.SubjectDetailId).FirstOrDefault(), Status = a.grade.HasValue });

                    return Json(new AjaxResponseViewModel() { result = true, data = trainingSubject }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandlerTrainedSubject", Messege.DATA_ISNOTFOUND);
                    return Json(new AjaxResponseViewModel() { result = false, data = null, message = string.Format(Messege.DATA_ISNOTFOUND, Resource.lblJobTitle, "<font color='red' >" + id.Value + "</font>") }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/AjaxHandlerTrainedSubject", ex.Message);

                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult OrientationJobListEmployee(int idJob, int? idEmp)
        {
            var data = _jobtitleService.GetById(idJob);
            var model = new OrientationJobRouteEmployeeViewModel()
            {
                Subjects = data.Title_Standard.Select(a => new CustomDataListModel() { Key = a.SubjectDetail.Id, Value = a.SubjectDetail.Name }),
            };
            var subjects = model.Subjects.Select(a => (int)a.Key);

            var trainee = CourseService.GetCourseResultFinal(
                a => a.IsDeleted==false && (a.grade == null || a.grade > 0) && a.Course.IsActive == true && a.Course.IsDeleted != true && a.Course.Course_Detail.Any(x => x.IsDeleted==false && subjects.Contains((int)x.SubjectDetailId))
              ,new[] { (int)UtilConstants.ApproveType.Course, (int)UtilConstants.ApproveType.AssignedTrainee}
                ).Select(a => new
                {
                    a.traineeid,
                    FullName = a.Trainee.FirstName + " " + a.Trainee.LastName,
                    subjects = a.Course.Course_Detail.Where(
                    x => x.IsDeleted==false && x.IsActive==true && subjects.Contains((int)x.SubjectDetailId)).Select(x => x.SubjectDetailId),
                    a.Trainee
                });
            model.TrainedEmployee = trainee
                .GroupBy(
                    a => new
                    {
                        Id = a.traineeid,
                        FullName = a.FullName,
                        a.Trainee
                    },
                    b => b.subjects,
                    (a, b) => new OrientationJobCoincideTrainee()
                    {
                        FullName = a.FullName,
                        Id = a.Id,
                        AmountSubjects = b.SelectMany(z => z).Distinct().Count(),//b.Sum(z => z.Count()),
                        Employee = a.Trainee
                    }
                ).OrderByDescending(a => a.AmountSubjects);
            return PartialView("_Partials/_OrientationJobListEmployee", model);
        }


        public FileResult ExportOrientationJobEXCEL(FormCollection form)
        {
            var filecontent = ExportOrientationJobEXCEL();
            if (filecontent != null)
            {
                return File(filecontent, ExportUtils.ExcelContentType, "SuccessionPositions.xlsx");
            }
            return null;
        }

        private byte[] ExportOrientationJobEXCEL()
        {
            string templateFilePath = Server.MapPath(@"" + GetByKey("PrivateTemplate") + "ExcelFile/SuccessionPositions.xlsx");
            FileInfo template = new FileInfo(templateFilePath);
            //var courseDetail = _repoCourseServiceDetail.GetById(ddlSubject);

            var data = _orientationService.Get().GroupBy(a => new { a.ExpectedDate, a.JobFutureId, a.IdKindOfSuccessor }).Select(b => b.FirstOrDefault());
            // _courseServiceMember.Get(a => a.Course_Details_Id == ddlSubject && a.DeleteApprove == null && a.IsActive && (a.Status == null || a.Status == (int)UtilConstants.APIAssign.Approve)).ToList();
            MemoryStream ms = new MemoryStream();
            byte[] bytes = null;
            if (data != null)
            {
                ExcelPackage excelPackage;
                using (excelPackage = new ExcelPackage(template, false))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.First();
                    int startRow = 9;
                    //var Row = 0;
                    int count = 0;
                    foreach (var item in data)
                    {
                        int col = 2;
                        count++;
                        ExcelRange cellNo = worksheet.Cells[startRow + 1, col];
                        cellNo.Value = count;
                        cellNo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellNo.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellNo.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellTraineeJobtitleName = worksheet.Cells[startRow + 1, ++col];
                        cellTraineeJobtitleName.Value = item?.JobTitle?.Name;
                        cellTraineeJobtitleName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellTraineeJobtitleName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellTraineeJobtitleName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellJobtitleName = worksheet.Cells[startRow + 1, ++col];
                        cellJobtitleName.Value = item?.IdKindOfSuccessor == 1 ? loadEmpExcel(item?.JobFutureId, 1, item?.ExpectedDate) : "";
                        cellJobtitleName.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellJobtitleName.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellJobtitleName.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellKindOfSuccessor = worksheet.Cells[startRow + 1, ++col];
                        cellKindOfSuccessor.Value = item?.IdKindOfSuccessor == 2 ? loadEmpExcel(item?.JobFutureId, 2, item?.ExpectedDate) : "";
                        cellKindOfSuccessor.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellKindOfSuccessor.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellKindOfSuccessor.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellLoadSubject = worksheet.Cells[startRow + 1, ++col];
                        cellLoadSubject.Value = item?.IdKindOfSuccessor == 3 ? loadEmpExcel(item?.JobFutureId, 3, item?.ExpectedDate) : "";
                        cellLoadSubject.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellLoadSubject.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellLoadSubject.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellExpectedDate = worksheet.Cells[startRow + 1, ++col];
                        cellExpectedDate.Value = DateTime.Parse(item?.ExpectedDate.ToString()).ToShortDateString() + "";
                        cellExpectedDate.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellExpectedDate.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellExpectedDate.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellCreateDay = worksheet.Cells[startRow + 1, ++col];
                        cellCreateDay.Value = item?.CreateDay + "/" + item?.USER?.FIRSTNAME + " " + item?.USER?.LASTNAME;
                        cellCreateDay.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellCreateDay.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellCreateDay.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        ExcelRange cellRemark = worksheet.Cells[startRow + 1, ++col];
                        cellRemark.Value = item?.Remark;
                        cellRemark.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        cellRemark.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cellRemark.Style.Border.BorderAround(ExcelBorderStyle.Thin);


                        startRow++;

                    }
                    bytes = excelPackage.GetAsByteArray();
                }
            }
            return bytes;

        }
        private string loadEmpExcel(int? idJob, int? idKind, DateTime? expectdate)
        {
            StringBuilder HTML = new StringBuilder();
            var model = _orientationService.Get(a => a.IdKindOfSuccessor == idKind && a.ExpectedDate == expectdate && a.JobFutureId == idJob).OrderByDescending(a => a.Id);//.Select(a => a.TraineeId).Distinct();
            //var data = _jobtitleService.GetById((int)idJob).Orientations;

            foreach (var item in model)//data.Where(a => model.Contains(a.TraineeId) && a.IdKindOfSuccessor == idKind)
            {
                //HTML.AppendFormat("{0} - {1} \n\r ", @item.Trainee.str_Staff_Id, @item.Trainee.FirstName + " " + @item.Trainee.LastName);
                HTML.AppendFormat("{0} - {1} \n\r ", @item.Trainee.str_Staff_Id, ReturnDisplayLanguage(@item.Trainee.FirstName ,@item.Trainee.LastName));
            }
            return HTML.ToString();
        }
        private string loadEmp(int? idJob, int? idKind, DateTime? expectdate)
        {
            StringBuilder HTML = new StringBuilder();
            var model = _orientationService.Get(a => a.IdKindOfSuccessor == idKind && a.ExpectedDate == expectdate && a.JobFutureId == idJob).OrderByDescending(a => a.Id).Select(a => a.Trainee).Distinct();
            foreach (var item in model)
            {
                //HTML.AppendFormat("{0} - {1} </br>", @item.str_Staff_Id, @item.FirstName + " " + @item.LastName);
                HTML.AppendFormat("{0} - {1} </br>", @item.str_Staff_Id, ReturnDisplayLanguage(@item.FirstName,@item.LastName));
            }
            return HTML.ToString();
        }

        #endregion
        [AllowAnonymous]
        public ActionResult AjaxHandlerAvailableSubject2(jQueryDataTableParamModel param)
        {
            try
            {
                var departmentId = string.IsNullOrEmpty(Request.QueryString["DepartmentList2"]) ? -1 : Convert.ToInt32(Request.QueryString["DepartmentList2"].Trim());
                var jobtitleId = string.IsNullOrEmpty(Request.QueryString["JobtitleList2"]) ? -1 : Convert.ToInt32(Request.QueryString["JobtitleList2"].Trim());
                var traineeCode = string.IsNullOrEmpty(Request.QueryString["EID2"]) ? string.Empty : Request.QueryString["EID2"].Trim().ToLower();
                var fullName = string.IsNullOrEmpty(Request.QueryString["FullName2"]) ? string.Empty : Request.QueryString["FullName2"].Trim().ToLower();
                var groupTraineeId = string.IsNullOrEmpty(Request.QueryString["GroupTrainee2"]) ? -1 : Convert.ToInt32(Request.QueryString["GroupTrainee2"].Trim());
                var valueparent = string.IsNullOrEmpty(Request["valuecurrent"]) ? string.Empty : Request["valuecurrent"].Trim(',');
                var listvalue = String2Array(valueparent);
                //////////////////////////////////////////////////////////////
                var lstDepartment = new List<int?>();
                var department = DepartmentService.GetById(departmentId);
                if (department != null)
                {
                    var departmentIds = DepartmentService.Get(a => a.Ancestor.Contains(department.Code)).OrderBy(b => b.Ancestor).Select(a => a.Id);
                    if (departmentIds.Any())
                    {
                        foreach (var id in departmentIds)
                        {
                            lstDepartment.Add(id);
                        }
                    }
                }
                var EID_ = new List<string>();
                var FullName_ = new List<string>();
                if (!string.IsNullOrEmpty(traineeCode))
                {
                    EID_ = traineeCode.Split(',').ToList();
                }
                if (!string.IsNullOrEmpty(fullName))
                {
                    FullName_ = fullName.Split(',').ToList();
                }
                var data = EmployeeService.Get(a =>
                                 (((departmentId != -1 || jobtitleId != -1 || !string.IsNullOrEmpty(fullName) || !string.IsNullOrEmpty(traineeCode))
                                 && (departmentId == -1 || lstDepartment.Contains(a.Department_Id))
                                 && (jobtitleId == -1 || a.Job_Title_id == jobtitleId)
                                  && (string.IsNullOrEmpty(traineeCode) || EID_.Any(t => a.str_Staff_Id.Contains(t)))
                                 && (string.IsNullOrEmpty(fullName) || FullName_.Any(t => (string.IsNullOrEmpty(a.LastName) || (a.LastName == a.FirstName) ? a.FirstName : a.LastName + " " + a.FirstName).Contains(t))))
                                 ||  (groupTraineeId != -1 && (groupTraineeId == -1 || a.GroupTrainee_Item.Any(b => b.GroupTraineeId == groupTraineeId))))
                                  && !listvalue.Contains(a.Id)).ToList();

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Trainee, string> orderingFunction = (c
                                                           => sortColumnIndex == 1 ? c?.str_Staff_Id
                                                             : sortColumnIndex == 2 ? c?.LastName
                                                             : sortColumnIndex == 3 ? c?.Department?.Ancestor
                                                              : sortColumnIndex == 4 ? c?.JobTitle?.Name
                                                           : c?.Department?.Ancestor);

                var filtered = (Request["sSortDir_0"] == "asc")
                   ? data.OrderBy(orderingFunction)
                   : data.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                 string.Empty,
                                    "<input class='EmployeeID' type='hidden' name='EmployeeID' id='EmployeeID' value='"+c.Id+"' />" +
                                   c?.str_Staff_Id,
                                   //c?.FirstName.Trim() + " " +c?.LastName.Trim(),
                                   ReturnDisplayLanguage(c?.FirstName,c?.LastName),
                                   c?.Department?.Name,
                                   "<input class='JobTitleID' type='hidden' name='JobTitleID' id='JobTitleID' value='"+c?.JobTitle?.Id+"' />" +
                                   c?.JobTitle?.Name,
                                   "<input type='checkbox' name='id2[]' value='"+c?.Id+"'>",

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Course/AjaxHandlerAvailableSubject2", ex.Message);
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
        public ActionResult AjaxHandlerApprovePotentialSuccessors(jQueryDataTableParamModel param)
        {
            try
            {
                var fName = string.IsNullOrEmpty(Request.QueryString["fullname"]) ? "" : Request.QueryString["fullname"].Trim();
                var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
                DateTime dateFrom;
                DateTime.TryParse(fSearchDateFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateFrom);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;


                var data = _orientationService.Get(a =>
                            (string.IsNullOrEmpty(fName) || a.JobTitle.Name.Contains(fName)) &&
                            (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.CreatedAt) >= 0) && a.PotentialSuccessors_Item.Any(x=>x.IsActive == true));
                IEnumerable<PotentialSuccessor> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PotentialSuccessor, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.JobTitle.Name
                                                            : sortColumnIndex == 2 ? c.CreatedAt
                                                            : sortColumnIndex == 3 ? (object)c.Status                                                    
                                                            : c.JobTitle.Name);


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
                             select new object[] {
                                    string.Empty,
                                    "<span data-value='"+c?.Id+"' class='expand' style='cursor: pointer;'><a>"+ c?.JobTitle?.Name +"</a></span><br />",
                                    c.CreatedAt.HasValue ? c?.CreatedAt.Value.ToString("dd/MM/yyyy"):"",
                                    c?.Status == 0 ? "Approval" : (c?.Status == 2 ? "Reject": "Pending"),
                                   "<a title='Approve' href='"+@Url.Action("Appprove",new{id = c?.Id})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>",
                             };


                var jsonResult = Json(new
                {
                    param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
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
        [AllowAnonymous]
        public ActionResult AjaxHandlerRequestPotentialSuccessors(jQueryDataTableParamModel param)
        {
            try
            {
                var fName = string.IsNullOrEmpty(Request.QueryString["fullname"]) ? "" : Request.QueryString["fullname"].Trim();
                var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["fSearchDate_from"]) ? "" : Request.QueryString["fSearchDate_from"].Trim();
                DateTime dateFrom;
                DateTime.TryParse(fSearchDateFrom, CultureInfo.CreateSpecificCulture("vi-VN"), DateTimeStyles.None, out dateFrom);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;


                var data = _orientationService.Get(a =>
                            (string.IsNullOrEmpty(fName) || a.JobTitle.Name.Contains(fName)) &&
                            (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.CreatedAt) >= 0) && a.CreatedBy == CurrentUser.USER_ID);
                IEnumerable<PotentialSuccessor> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PotentialSuccessor, object> orderingFunction = (c
                                                          => sortColumnIndex == 1 ? c.JobTitle.Name
                                                            : sortColumnIndex == 2 ? c.CreatedAt
                                                            : sortColumnIndex == 3 ? (object)c.Status
                                                            : c.JobTitle.Name);


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
                             select new object[] {
                                    string.Empty,
                                    "<span data-value='"+c?.Id+"' class='expand' style='cursor: pointer;'><a>"+ c?.JobTitle?.Name +"</a></span><br />",
                                    c.CreatedAt.HasValue ? c.CreatedAt.Value.ToString("dd/MM/yyyy"):"",
                                    c?.Status == 0 ? "Approval" : (c?.Status == 2 ? "Reject": "Pending"),
                                    !string.IsNullOrEmpty(c?.Note) ? "<a title='View Remark' onclick='viewremark("+c?.Id+")' data-toggle='tooltip'><i class='fas fa-sticky-note btnIcon_green font-byhoa' aria-hidden='true'></i></a>" : "",
                                   "<a title='Edit' href='"+@Url.Action("OrientationEmp",new{id = c?.Id})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>",
                                   
                             };


                var jsonResult = Json(new
                {
                    param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
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

        [AllowAnonymous]
        public ActionResult AjaxHandlerApprovePotentialSuccessors_Item(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var data = _orientationService.GetItembyId(id).Where(a=>a.IsActive == true);

                IEnumerable<PotentialSuccessors_Item> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<PotentialSuccessors_Item, object> orderingFunction = c
                                                          => sortColumnIndex == 1 ? c.Trainee.str_Staff_Id
                                                            : sortColumnIndex == 2 ? ReturnDisplayLanguage(c.Trainee.FirstName,c.Trainee.LastName)
                                                            : sortColumnIndex == 3 ? c.JobTitle.Name
                                                            : sortColumnIndex == 4 ? (object)c.Status
                                                            : ReturnDisplayLanguage(c.Trainee.FirstName, c.Trainee.LastName);


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
                             select new object[] {
                                    string.Empty,
                                    c.Trainee.str_Staff_Id,
                                   ReturnDisplayLanguage(c.Trainee.FirstName,c.Trainee.LastName),
                                   c.JobTitle.Name,
                                   c.Status == 0 ? "Approval" : (c.Status == 2 ? "Reject": "Pending"),
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
        [AllowAnonymous]
        public ActionResult ListApprove()
        {
            
            return View();
        }
        [AllowAnonymous]
        public ActionResult ListRequest()
        {

            return View();
        }

        [AllowAnonymous]
        public ActionResult Appprove(int? id)
        {
            var model = new OrientationViewModel
            {
                EmpList =
                    EmployeeService.Get(true)
                        .ToDictionary(a => a.Id, a => a.str_Staff_Id + " - " + (ReturnDisplayLanguage(a.FirstName, a.LastName))),
                OrientationKindOfSuccessorList = _orientationService.GetKind().OrderBy(a => a.Name).Select(a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Name, Value = a.Name }),
                GroupTrainees = EmployeeService.GetAllGroupTrainees(a => a.IsActived == true).OrderBy(a => a.Name).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name)),
                Departments = GetDepartmentAcestorModel(CurrentUser.IsMaster),
                JobTitles = _jobtitleService.Get().OrderBy(a => a.Name.Trim()).ToDictionary(a => a.Id, a => a.Name),
            };
            if (id.HasValue)
            {
                model.Id = id;
                var record = _orientationService.GetbyId(id);
                if (record != null)
                {
                    model.OrientationModify = record.PotentialSuccessors_Item.Where(a=>a.IsActive == true ).Select(a => new OrientationModify()
                    {
                        EmployeeID = a.TraineeId ?? 0,
                        EmployeeName = a.TraineeId.HasValue ? ReturnDisplayLanguage(a.Trainee?.FirstName, a.Trainee?.LastName) : string.Empty,
                        EmployeeEID = a.TraineeId.HasValue ? a.Trainee?.str_Staff_Id : string.Empty,
                        JobTitleName = a.JobHistoryId.HasValue ? a.JobTitle?.Name : string.Empty,
                        JobTitleID = a.JobHistoryId ?? 0,
                        FuturePositionID = a.PotentialSuccessor?.JobtitleId ?? 0,
                        JobTitleFutureName = a.SuccessorsID.HasValue ? a.PotentialSuccessor?.JobTitle?.Name : string.Empty,
                        SelectedValue = a.IsActive ?? false,
                        Status = a.Status, 
                    });
                    model.JobFuture = record.JobtitleId ?? 0;
                }


            }
            else
            {
                model.Id = 0;
            }
            StringBuilder html = new StringBuilder();
            html.Append("<option value=''></option>");
            var list = EmployeeService.Get(true);
            foreach (var item in list)
            {
                var fullName = ReturnDisplayLanguage(item.FirstName, item.LastName);
                html.AppendFormat(
                                    "<option  value='{0}'>{1}</option>",
                                    item.Id, fullName);
            }
            model.EmpListCustom = html.ToString();
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Appprove(List<OrientationModify> model, int? id, int type)
        {
            try
            {
                _orientationService.ApproveySuccessor(model, id, type);
                var message = type == 0 ? "Aproval successfully! " : "Reject successfully!";
                return Json(new AjaxResponseViewModel() { result = true, message = message });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Orientation/ModifyForm", ex.Message);
                return Json(new { result = false, message = ex.Message });
            }

        }
        [AllowAnonymous]
        public string ShowRemark(int id)
        {
            var remark = "";
            var check = _orientationService.GetbyId(id)?.Note;
            remark = string.IsNullOrEmpty(check) ? "" : Regex.Replace(check, "[\r\n]", "<br/>");
            return remark;
        }
    }
}