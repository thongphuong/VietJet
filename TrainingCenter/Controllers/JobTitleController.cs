using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TMS.Core.App_GlobalResources;
using System.Text;
using System.Web.Script.Serialization;
using DAL.Entities;
using Newtonsoft.Json;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Department;
using TMS.Core.Services.Jobtitle;
using TMS.Core.ViewModels.AjaxModels;

namespace TrainingCenter.Controllers
{
    using System.Configuration;
    using System.Threading.Tasks;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Subject;
    using TMS.Core.Services.Users;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Jobtitles;

    public class JobTitleController : BaseAdminController
    {
        //
        // GET: /Trainee/
        #region ctor

        private readonly IJobtitleService _jobtitleService;
        private readonly ISubjectService _subjectService;
        public JobTitleController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IJobtitleService jobtitleService, ISubjectService subjectService, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _jobtitleService = jobtitleService;
            _subjectService = subjectService;
        }

        #endregion

        public ActionResult Index()
        {
            var model =
                _jobtitleService.Get()
                    .Select(a => new JobtitleViewModel { Code = a.Code, Id = a.Id, Name = a.Name });
            return View();
        }


        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = "Invalid params" };
                return RedirectToAction("Index");
            }
            var entity = _jobtitleService.GetById(id.Value);
            if (entity == null)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = false, message = "Jobtitle is not found" };
                return RedirectToAction("Index");
            }
            var model = new JobtitleModifyViewModel()
            {
                Subjects = _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name)),
                Id = entity.Id,
                Name = entity.Name,
                //                Code = entity.Code,
                Description = entity.Description,
                AssignedSubjects = entity.Title_Standard.Select(a => (int)a.Subject_Id).ToArray(),
            };
            return View(model);
        }
        public ActionResult ListJobs()
        {
            return PartialView("_ListJobs");
        }


        public ActionResult Create()
        {
            var check = ConfigurationManager.AppSettings["HiddenLevelPosition"];
            var jobtitleModifymodel = new JobtitleModifyViewModel()
            {
                Subjects = _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name),
                YesNoDictionary = UtilConstants.YesNoDictionary(),
                IsActive = (int)UtilConstants.BoolEnum.Yes,
                check_hidden_level_position = string.IsNullOrEmpty(check) || int.Parse(check) != 1 ? false : true,
            };
            return View(jobtitleModifymodel);
        }
        [HttpPost]
        public async Task<ActionResult> Create(JobtitleModifyViewModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                model.Subjects = _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name);
                model.YesNoDictionary = UtilConstants.YesNoDictionary();
                return View(model);
            }
            try
            {
                var value = form.GetValues("AssignedSubjectsList");
                if (value.Length > 0)
                {
                    var listint = value.FirstOrDefault().Split(new char[] { ',' });
                    List<int> termsList = new List<int>();
                    foreach (var item in listint)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            termsList.Add(Convert.ToInt32(item));
                        }
                    }
                    model.AssignedSubjects = termsList.ToArray();
                }
                var check = ConfigurationManager.AppSettings["HiddenLevelPosition"];

                model.check_hidden_level_position = string.IsNullOrEmpty(check) || int.Parse(check) != 1 ? false : true;
                _jobtitleService.Modify(model);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON JOBTITLE)----------]
                    var callLms = CallServices(UtilConstants.CRON_JOBTITLE);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/Create", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name));
                        TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = true, message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name) };
                        //return RedirectToAction("Index");
                    }
                    #endregion
                });
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = true, message = Messege.SUCCESS };
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/Create", ex.Message);
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = false, message = ex.Message };
            }
            model.Subjects = _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name);
            model.YesNoDictionary = UtilConstants.YesNoDictionary();
            return View(model);
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].ToLower().Trim();

                var model = _jobtitleService.Get(a => (string.IsNullOrEmpty(strName) || (a.Name.Trim().ToLower().Contains(strName) /*|| (a.JobHeaderId.HasValue && a.JobtitleHeader.Name.Trim().ToLower().Contains(strName)) || (a.JobPositionId.HasValue && a.JobtitlePosition.Name.Trim().ToLower().Contains(strName))*/)), true).ToArray();
                var verticalBar = GetByKey("VerticalBar");
                var getvalue = ConfigurationManager.AppSettings["HiddenLevelPosition"];
                bool check = true; //string.IsNullOrEmpty(getvalue) || int.Parse(getvalue) != 1 ? false : true;
                var filtered = model.Select(a => new AjaxJobTitle
                {
                    //Level = a.JobtitleHeader?.Name ?? string.Empty,
                    //Position = a.JobtitlePosition?.Name ?? string.Empty,
                    Name = a.Name ?? string.Empty,
                    Status = (a.IsActive == false || a.IsActive == null? "<i class='fa fa-toggle-off' onclick='Set_Participate_Jobtitle(0," + a.Id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Jobtitle(1," + a.Id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                    Option = (
                                (User.IsInRole("/JobTitle/Modify") ? "<a href='/JobTitle/Modify/" + a.Id + "' data-toggle='tooltip' title='Edit' ><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : string.Empty) +
                                (User.IsInRole("/JobTitle/delete") ? verticalBar + "<a href='javascript:void(0)' onclick='calldelete(" + a.Id + ")' data-toggle='tooltip' title='Delete'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : string.Empty)
                              ),
                });

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxJobTitle, string> orderingFunction = (c =>
                                                          sortColumnIndex == 1 ? c.Name
                                                          : sortColumnIndex == 2 ? c.Status
                                                          : sortColumnIndex == 3 ? c.Option
                                                          : c.Name);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var ajaxJobTitles = filtered.ToArray();
                var displayed = ajaxJobTitles.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                IEnumerable<object[]> result;
                if (check == true)
                {
                    result = from c in displayed
                             select new object[] {
                                string.Empty,
                                c?.Name,
                                c?.Status,
                                c?.Option
                             };
                }
                else
                {
                    result = from c in displayed
                             select new object[] {
                               string.Empty,
                               c?.Level,
                               c?.Position,
                               c?.Name,
                               c?.Status,
                               c?.Option
                             };
                }
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = ajaxJobTitles.Count(),
                    iTotalDisplayRecords = ajaxJobTitles.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/AjaxHandler", ex.Message);
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

        public ActionResult AjaxHandlerLevel(jQueryDataTableParamModel param)
        {
            try
            {
                //string strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().ToString().Trim();

                var strName = string.IsNullOrEmpty(Request.QueryString["levelname"]) ? string.Empty : Request.QueryString["levelname"].ToLower().Trim();

                var model = _jobtitleService.GetJobHeader(a => (string.IsNullOrEmpty(strName) || (a.Name.Trim().ToLower().Contains(strName))), true).ToArray();
                var verticalBar = GetByKey("VerticalBar");
                var filtered = model.Select(a => new AjaxJobtitleHeader
                {
                    LevelName = a.Name,
                    Description = a.Description,
                    Status = (a.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_LevelJobtitle(0," + a.Id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_LevelJobtitle(1," + a.Id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                    Option = (
                                (User.IsInRole("/JobTitle/Modify") ? "<a href='javascript:void(0)' onclick='ModifyHeader(" + a.Id + ")' data-toggle='tooltip' title='Edit'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i></a>" : string.Empty) +
                                (User.IsInRole("/JobTitle/delete") ? verticalBar + "<a href='javascript:void(0)' onclick='calldelete1(" + a.Id + ")' data-toggle='tooltip'  title='Delete'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true'></i></a>" : string.Empty)
                              )

                });



                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxJobtitleHeader, string> orderingFunction = (c => sortColumnIndex == 1 ? c.LevelName
                                                                                : sortColumnIndex == 2 ? c.Description
                                                                                : sortColumnIndex == 3 ? c.Status
                                                                                : sortColumnIndex == 4 ? c.Option
                                                                                : c.LevelName);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);


                //var cscost = filtered.ToArray();
                var ajaxJobtitleHeaders = filtered.ToArray();
                var displayed = ajaxJobtitleHeaders.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                string.Empty,
                                c?.LevelName ,
                                c?.Description,
                                c?.Status,
                                c?.Option
                                /* UtilConstants.ActiveStatusDictionary()[UtilConstants.BoolEnum.Yes]:UtilConstants.ActiveStatusDictionary()[UtilConstants.BoolEnum.No],*/
                             };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = ajaxJobtitleHeaders.Count(),
                    iTotalDisplayRecords = ajaxJobtitleHeaders.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/AjaxHandlerLevel", ex.Message);
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
        public ActionResult AjaxHandlerPosition(jQueryDataTableParamModel param)
        {
            try
            {
                //string strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().ToString().Trim();
                var strName = string.IsNullOrEmpty(Request.QueryString["positionname"]) ? string.Empty : Request.QueryString["positionname"].ToLower().Trim();

                var model = _jobtitleService.GetJobPosition(a => (string.IsNullOrEmpty(strName) || (a.Name.Trim().ToLower().Contains(strName))), true).ToArray();
                var verticalBar = GetByKey("VerticalBar");
                var filtered = model.Select(a => new AjaxJobtitlePosition()
                {
                    PositionName = a.Name,
                    Description = a.Description,
                    Status = (a.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_PositionJobtitle(0," + a.Id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_PositionJobtitle(1," + a.Id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                    Option = (User.IsInRole("/JobTitle/Modify") ? "<a href='javascript:void(0)' onclick='ModifyPosition(" + a.Id + ")' data-toggle='tooltip' title='Edit'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : string.Empty) +
                             (User.IsInRole("/JobTitle/delete") ? verticalBar + "<a href='javascript:void(0)' onclick='calldelete2(" + a.Id + ")' data-toggle='tooltip' title='Delete'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : string.Empty)

                });

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

                Func<AjaxJobtitlePosition, string> orderingFunction = (c => sortColumnIndex == 1 ? c.PositionName
                                                                                : sortColumnIndex == 2 ? c.Description
                                                                                : sortColumnIndex == 3 ? c.Status
                                                                                : sortColumnIndex == 4 ? c.Option
                                                                                : c.PositionName);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc
                filtered = (sortDirection == "asc")
                    ? filtered.OrderBy(orderingFunction)
                    : filtered.OrderByDescending(orderingFunction);
                //var cscost = filtered.ToArray();
                var ajaxJobtitlePositions = filtered.ToArray();
                var displayed = ajaxJobtitlePositions.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                string.Empty,
                                c?.PositionName ,
                                c?.Description,
                                c?.Status,
                                c?.Option/* UtilConstants.ActiveStatusDictionary()[UtilConstants.BoolEnum.Yes]:UtilConstants.ActiveStatusDictionary()[UtilConstants.BoolEnum.No],*/
                             };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = ajaxJobtitlePositions.Count(),
                    iTotalDisplayRecords = ajaxJobtitlePositions.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/AjaxHandlerPosition", ex.Message);
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
        private List<string> loaddepartment(int? parentid, int level)
        {
            var result = new List<string>();
            var data = DepartmentService.Get(a => a.Id == parentid);
            if (data.Count() == 0)
                return result;
            else
            {
                foreach (var item in data)
                {
                    var arr = loaddepartment(item.ParentId, level + 1);
                    if (arr.Any())
                    {
                        result.AddRange(arr);
                    }
                    result.Add(item.Name);
                }
            }
            return result;
        }
        private string ReturnDepartment(int id_department = -1)
        {
            StringBuilder HTML_ = new StringBuilder();
            List<string> result = loaddepartment(id_department, 1);
            var khoangtrang = "";
            foreach (string item in result)
            {
                khoangtrang += "&nbsp;&nbsp;&nbsp;&nbsp;";
                HTML_.AppendFormat("{0} +{1} <br />", khoangtrang, item);
            }
            return HTML_.ToString();
        }

        [HttpGet]
        public ActionResult CreateHeader()
        {
            var model = new JobtitleHeaderViewModel()
            {
                JobLvlDictionary = _jobtitleService.GetJobLevel().Select(a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Description, Value = a.Name })
            };
            return PartialView("_partials/_JobHeaderModify", model);
        }
        [HttpGet]
        public ActionResult ModifyHeader(int? id)
        {

            var entity = _jobtitleService.GetByIdHeader((int)id);
            var model = new JobtitleHeaderViewModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                //JobLvlDictionary = _jobtitleService.GetJobLevel().Select(a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Description, Value = a.Name })
            };
            return PartialView("_partials/_JobHeaderLevelModify", model);
        }
        [HttpGet]
        public ActionResult CreatePosition()
        {
            return PartialView("_partials/_JobPositionModify", new JobtitlePositionViewModel());
        }
        [HttpGet]
        public ActionResult ModifyPosition(int? id)
        {
            var entity = _jobtitleService.GetByIdPosition((int)id);
            var model = new JobtitlePositionViewModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                //JobLvlDictionary = _jobtitleService.GetJobLevel().Select(a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Description, Value = a.Name })
            };
            return PartialView("_partials/_JobPositionEdit", model);
        }
        [HttpPost]
        public ActionResult CreateHeader(JobtitleHeaderViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var headerid = _jobtitleService.ModifyJobHeader(model);
                    return Json(new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS, data = headerid }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/CreateHeader", ex.Message);
                    return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var msg = MessageInvalidData(ModelState);
            return Json(new { result = false, message = msg });
        }
        [HttpPost]
        public async Task<ActionResult> ModifyHeader(JobtitleHeaderViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var headerid = _jobtitleService.ModifyJobHeader(model);
                    await Task.Run(() =>
                    {
                        #region [--------CALL LMS (CRON JOBTITLE)----------]
                        var callLms = CallServices(UtilConstants.CRON_JOBTITLE);
                        if (!callLms)
                        {
                            LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/ModifyHeader", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name));
                            //return Json(new AjaxResponseViewModel()
                            //{
                            //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name),
                            //    data = headerid,
                            //    result = false
                            //});
                        }
                        #endregion
                    });
                    return Json(new AjaxResponseViewModel() { result = true, message = Messege.SUCCESS, data = headerid });
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/ModifyHeader", ex.Message);
                    return Json(new { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var msg = MessageInvalidData(ModelState);
            return Json(new AjaxResponseViewModel { result = false, message = msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreatePosition(JobtitlePositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var position = _jobtitleService.ModifyJobPosition(model);
                    return Json(new AjaxResponseViewModel { result = true, data = position, message = "Insert success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/CreatePosition", ex.Message);
                    return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var msg = MessageInvalidData(ModelState);
            return Json(new AjaxResponseViewModel { result = false, message = msg }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> ModifyPosition(JobtitlePositionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var headerid = _jobtitleService.ModifyJobPosition(model);
                    await Task.Run(() =>
                    {
                        #region [--------CALL LMS (CRON JOBTITLE)----------]
                        var callLms = CallServices(UtilConstants.CRON_JOBTITLE);
                        if (!callLms)
                        {
                            LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/ModifyPosition", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name));
                            //return Json(new AjaxResponseViewModel()
                            //{
                            //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name),
                            //    data = headerid,
                            //    result = false
                            //});
                        }
                        #endregion
                    });
                    return Json(new AjaxResponseViewModel() { result = true, message = "Update success", data = headerid }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/ModifyPosition", ex.Message);
                    return Json(new { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            var msg = MessageInvalidData(ModelState);
            return Json(new { result = false, message = msg }, JsonRequestBehavior.AllowGet);
        }
        #region User-Modify

        public ActionResult Modify(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }
            var entity = _jobtitleService.GetById(id.Value);
            var check = ConfigurationManager.AppSettings["HiddenLevelPosition"];
            if (entity == null)
            {
                return RedirectToAction("Index");
            }
            var model = new JobtitleModifyViewModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive == true ? 1 : 0,
                JobPositionId = entity.JobPositionId,
                JobHeaderId = entity.JobHeaderId,
                AssignedSubjects = entity.Title_Standard.Select(a => (int)a.Subject_Id).ToArray(),
                Subjects = _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name),
                YesNoDictionary = UtilConstants.YesNoDictionary(),
                check_hidden_level_position = string.IsNullOrEmpty(check) || int.Parse(check) != 1 ? false : true,
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult Modify(JobtitleModifyViewModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                model.Subjects = _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name);
                model.YesNoDictionary = UtilConstants.YesNoDictionary();
                return View(model);
            }
            try
            {
                var value = form.GetValues("AssignedSubjectsList");
                if (value.Length > 0)
                {
                    var listint = value.FirstOrDefault().Split(new char[] { ',' });
                    List<int> termsList = new List<int>();
                    foreach (var item in listint)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            termsList.Add(Convert.ToInt32(item));
                        }
                    }
                    model.AssignedSubjects = termsList.ToArray();
                }
                var check = ConfigurationManager.AppSettings["HiddenLevelPosition"];
                model.check_hidden_level_position = string.IsNullOrEmpty(check) || int.Parse(check) != 1 ? false : true;
                _jobtitleService.Modify(model);
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = true, message = Messege.SUCCESS };
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/Modify", ex.Message);
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = false, message = ex.Message };
            }
            model.Subjects = _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name);
            model.YesNoDictionary = UtilConstants.YesNoDictionary();
            return View(model);
        }
        #endregion

        [HttpPost]
        public ActionResult ListHeader()
        {
            try
            {
                var data = _jobtitleService.GetJobHeader().OrderBy(a => a.Name.Trim())
                    .Select(
                        a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Description, Value = a.Name });

                return
                    Json(new AjaxResponseViewModel() { data = data, result = true });
            }
            catch (Exception ex)
            {
                return
                   Json(new AjaxResponseViewModel() { data = ex.Message, result = false });
            }
        }
        [HttpPost]
        public ActionResult ListPosition()
        {
            try
            {
                var data = _jobtitleService.GetJobPosition().OrderBy(a => a.Name.Trim())
                    .Select(
                        a => new CustomDataListModel() { Key = a.Id.ToString(), Title = a.Description, Value = a.Name });

                return
                    Json(new AjaxResponseViewModel() { data = data, result = true });
            }
            catch (Exception ex)
            {
                return
                   Json(new AjaxResponseViewModel() { data = ex.Message, result = false });
            }
        }

        [AllowAnonymous]
        public ActionResult RouteEmployee()
        {
            var model = new JobTitleOptionsViewModel()
            {
                JobHeaders = _jobtitleService.GetJobHeader().Select(a => new CustomDataListModel() { Key = a.JobtitleLevelId.HasValue ? a.JobtitleLevelId.ToString() : "", Title = a.Name, Value = a.Id.ToString() }),
                JobLevels = _jobtitleService.GetJobLevel().ToDictionary(a => a.Id, a => a.Name),
                JobPositions = _jobtitleService.GetJobPosition().Select(a => new CustomDataListModel() { Title = a.Name, Value = a.Id.ToString() }),
            };
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult AjaxHandlerListJobTitle(int? jobLevel, int? jobHeader, int? jobPosition)
        {
            try
            {
                var data = _jobtitleService.Get(a =>
                    (jobPosition == null || a.JobPositionId == jobPosition) && (jobHeader == null || a.JobHeaderId == jobHeader)/* && (jobLevel == null || a.JobtitleHeader.JobtitleLevelId == jobLevel)*/).Select(a => new { a.Id, a.Name });
                return Json(new AjaxResponseViewModel() { result = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        [AllowAnonymous]
        public ActionResult AjaxHandlerListValidEmployee(int? id)
        {
            try
            {
                if (id != null)
                {
                    var data = _jobtitleService.GetById(id.Value);
                    if (data.IsActive == false)
                    {
                        return Json(new AjaxResponseViewModel() { result = false, message = "Jobtitle is inactive" }, JsonRequestBehavior.AllowGet);
                    }
                    var model = new JobtitleRouteEmployeeViewModel()
                    {
                        Subjects = data.Title_Standard.Select(a => new CustomDataListModel() { Key = a.SubjectDetail.Id, Value = a.SubjectDetail.Name }),
                    };
                    var subjects = model.Subjects.Select(a => (int)a.Key);
                    model.TrainedEmployee = CourseService.GetCourseResultFinal(
                        a => a.IsDeleted == false && (a.grade == null || a.grade > 0) && a.Course.IsActive == true && a.Course.IsDeleted != true && a.Course.Course_Detail.Any(x => x.IsDeleted == false && subjects.Any(z => z == x.SubjectDetailId)))
                        .GroupBy(
                            a => new
                            {
                                Id = a.traineeid,
                                FullName = GetByKey("DisplayLanguage") == "vi" ? a.Trainee.FirstName + " " + a.Trainee.LastName : a.Trainee.LastName + " " + a.Trainee.FirstName,
                                //FullName = GetByKey(a.Trainee.FirstName,a.Trainee.LastName)
                            },
                            a =>
                                a.Course.Course_Detail.Where(
                                    x => x.IsDeleted == false && subjects.Any(z => z == x.SubjectDetailId))
                                    .Select(x => x.SubjectDetailId).Distinct(),
                            (a, b) => new JobTitleCoincideTrainee()
                            {
                                FullName = a.FullName,
                                // FullName = "" + GetByKey(a.FirstName,a.LastName),
                                Id = a.Id,
                                AmountSubjects = b.Count()
                            }
                        ).OrderByDescending(a => a.AmountSubjects);

                    return Json(new AjaxResponseViewModel() { result = true, data = model }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AjaxResponseViewModel() { result = false, data = null, message = string.Format(Messege.DATA_ISNOTFOUND, Resource.lblJobTitle, "<font color='red' >" + id.Value + "</font>") }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [AllowAnonymous]
        public ActionResult AjaxHandlerTrainedSubject(int? id, int? jobtitleId)//selected Employee Id
        {
            try
            {
                if (id.HasValue && jobtitleId.HasValue)
                {
                    var data = _jobtitleService.GetById(jobtitleId.Value);
                    if (data.IsActive == false)
                    {
                        return Json(new AjaxResponseViewModel() { result = false, message = "Jobtitle is inactive" }, JsonRequestBehavior.AllowGet);
                    }

                    var jobTitleSubjects = data.Title_Standard.Select(a => a.SubjectDetail.Id);
                    var trainingSubject = CourseService.GetCourseResultFinal(a => a.IsDeleted == false && a.Course.IsActive == true && (a.grade == null || a.grade > 0) &&
                            a.traineeid == id && jobTitleSubjects.Any(x => a.Course.Course_Detail.Any(z => z.IsActive == true && z.IsDeleted == false && x == z.SubjectDetailId)))
                        .Select(a => new JobTitleAvalibleSubject { SubjectId = a.Course.Course_Detail.Select(x => (int)x.SubjectDetailId).FirstOrDefault(), Status = a.grade.HasValue });

                    return Json(new AjaxResponseViewModel() { result = true, data = trainingSubject }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new AjaxResponseViewModel() { result = false, data = null, message = string.Format(Messege.DATA_ISNOTFOUND, Resource.lblJobTitle, "<font color='red' >" + id.Value + "</font>") }, JsonRequestBehavior.AllowGet);

                }
            }
            catch (Exception ex)
            {

                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult Delete(int id = -1)
        {
            try
            {
                var model = _jobtitleService.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                if (model.Trainees.Any(a => a.IsDeleted == false))
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.Name),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                if (model.LmsStatus == StatusModify)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_SENDLMS, model.Name),
                        result = false
                    }, JsonRequestBehavior.AllowGet);

                }
                model.IsDelete = true;
                model.IsActive = false;
                model.DeletedBy = CurrentUser.USER_ID.ToString();
                model.DeletedDate = DateTime.Now;
                // Cập nhật Status để gửi LMS
                model.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                _jobtitleService.Update(model);

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult DeleteLevel(int id = -1)
        {
            try
            {
                var model = _jobtitleService.GetByIdHeader(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/DeleteLevel", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //if (model.JobTitles.Any(a => a.Trainees.Any(b=>!b.IsDeleted)))
                //{
                //    return Json(new AjaxResponseViewModel
                //    {
                //        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.Name),
                //        result = false
                //    }, JsonRequestBehavior.AllowGet);
                //}
                model.IsDelete = true;
                model.IsActive = false;
                model.DeletedBy = CurrentUser.USER_ID.ToString();
                model.DeletedDate = DateTime.Now;
                _jobtitleService.Update(model);

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/DeleteLevel", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult DeletePosition(int id = -1)
        {
            try
            {
                var model = _jobtitleService.GetByIdPosition(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/DeletePosition", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                //if (model.JobTitles.Any(a => a.Trainees.Any(b => !b.IsDeleted)))
                //{
                //    return Json(new AjaxResponseViewModel
                //    {
                //        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.Name),
                //        result = false
                //    }, JsonRequestBehavior.AllowGet);
                //}
                model.IsDelete = true;
                model.IsActive = false;
                model.DeletedBy = CurrentUser.USER_ID.ToString();
                model.DeletedDate = DateTime.Now;
                _jobtitleService.Update(model);

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/DeletePosition", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateJobtitle(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idjobtitle = int.Parse(id);
                var removeJobtitle = _jobtitleService.GetById(idjobtitle);
                if (removeJobtitle == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipateJobtitle", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }


                if (isParticipate == 1)
                {
                    removeJobtitle.IsActive = false;
                }
                else
                {
                    removeJobtitle.IsActive = true;
                }
                // Cập nhật Status để gửi LMS
                removeJobtitle.LmsStatus = StatusModify;

                _jobtitleService.Update(removeJobtitle);

                await Task.Run(() =>
                {
                    #region [--------CALL LMS CronJobtitle----------]
                    var callLms = CallServices(UtilConstants.CRON_JOBTITLE);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipateJobtitle", string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, removeJobtitle.Name));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, removeJobtitle.Name),
                        //    result = false
                        //});
                    }
                    #endregion
                });
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, removeJobtitle.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipateJobtitle", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, ex.Message),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }


        }
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateLevelJob(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idjobtitle = int.Parse(id);
                var removeJobtitle = _jobtitleService.GetByIdHeader(idjobtitle);
                if (removeJobtitle == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipateLevelJob", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                if (isParticipate == 1)
                {
                    removeJobtitle.IsActive = false;
                }
                else
                {
                    removeJobtitle.IsActive = true;
                }
                //removeJobtitle.JobTitles.ToList().ForEach(a => a.LmsStatus = StatusModify);
                _jobtitleService.Update(removeJobtitle);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS CronJobtitle----------]
                    var callLms = CallServices(UtilConstants.CRON_JOBTITLE);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipateLevelJob", string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, removeJobtitle.Name));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, removeJobtitle.Name),
                        //    result = false
                        //});
                    }
                    #endregion
                });

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, removeJobtitle.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipateLevelJob", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, ex.Message),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }


        }
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipatePositionJob(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idjobtitle = int.Parse(id);
                var removeJobtitle = _jobtitleService.GetByIdPosition(idjobtitle);
                if (removeJobtitle == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipatePositionJob", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }


                if (isParticipate == 1)
                {
                    removeJobtitle.IsActive = false;
                }
                else
                {
                    removeJobtitle.IsActive = true;
                }


                //removeJobtitle.JobTitles.ToList().ForEach(a => a.LmsStatus = StatusModify);
                _jobtitleService.Update(removeJobtitle);

                await Task.Run(() =>
                {
                    #region [--------CALL LMS CronJobtitle----------]
                    var callLms = CallServices(UtilConstants.CRON_JOBTITLE);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipatePositionJob", string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, removeJobtitle.Name));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.DELETE_SUCCESSFULLY_BUT_ERROR_LMS, removeJobtitle.Name),
                        //    result = false
                        //});
                    }
                    #endregion
                });


                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, removeJobtitle.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "JobTitle/SubmitSetParticipatePositionJob", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, ex.Message),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult FilterSubject(string filterCodeOrName, string[] listSubjects)
        {
            var html = new StringBuilder();
            html.Append("<ul>");
            html.Append("<li id='liCheckAll'><input value='-1' multiple='' type='checkbox' id='checkAll' /><label class='label_thuan' for='checkAll'>&nbsp;Check All</label></li>");
            html.Append("<ul id='available'>");


            var data =
                _subjectService.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6 &&
                     (string.IsNullOrEmpty(filterCodeOrName) || (a.Code.ToLower().Contains(filterCodeOrName.ToLower()) || a.Name.ToLower().Contains(filterCodeOrName.ToLower())))).ToList();

            var list = JsonConvert.SerializeObject(listSubjects);

            if (!data.Any())
            {
                return Json(new
                {
                    value = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            }
            if (data.Any())
            {

                foreach (var item in data.OrderBy(a => a.Code + " - " + a.Name))
                {
                    if (!list.Contains(item.Id.ToString()))
                    {
                        html.Append("<li>");
                        html.AppendFormat("<input class='availableFunc' data-id='{0}' data-value='{1}' multiple='' value='{0}' name='subject' id='subject_{0}' type='checkbox' /><label class='label_thuan' for='subject_{0}'>{1}</label>", item.Id, item.Code + " - " + item.Name);
                        //html.AppendFormat("<option   value='{0}'>{1} - {2}</option>", item.Id, item.Code, item.Name);
                        html.Append("</li>");
                    }

                }
            }
            html.Append("</ul>");
            html.Append("</ul>");
            return Json(new
            {
                value = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
