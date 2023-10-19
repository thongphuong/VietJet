using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using DAL.Entities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.ViewModels.AjaxModels.AjaxGroupCourse;

namespace TrainingCenter.Controllers
{
    using System.Threading.Tasks;
    using TMS.Core.Services;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Subject;
    using TMS.Core.Services.Users;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Subjects;

    public class MCourseController : BaseAdminController
    {
        #region MyRegion

        private readonly ISubjectService _repoSubject;
        private readonly ICourseService _repoCouseService;


        #endregion
        //
        // GET: /Admin/User/
        // return view
        public MCourseController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ISubjectService repoSubject, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoSubject = repoSubject;
            _repoCouseService = courseService;
        }

        public ActionResult Index(int id = 0)
        {
            var groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;
            return View(groupsubject);
        }

        public ActionResult Modify(int? id)
        {
            if (!id.HasValue)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                {
                    result = false,
                    message = string.Format(Resource.INVALIDDATA, "Group user")
                };
                return RedirectToAction("Index", "MCourse");
            }
            try
            {
                var entity = _repoSubject.GetGroupSubjectById(id.Value);
                var model = new GroupSubjectViewModel
                {
                    Id = entity.id,
                    IsActive = entity.IsActive == true ? (int)UtilConstants.BoolEnum.Yes : (int)UtilConstants.BoolEnum.No,
                    Name = entity.Name,
                    Code = entity.Code,
                    Description = entity.Description,
                    AvailableSubjects = _repoSubject.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Name, a.Course_Type.str_Name)),
                    AssignedSubjects =
                        entity.CAT_GROUPSUBJECT_ITEM.Where(a => a.id_subject.HasValue).Select(a => (int)a.id_subject),
                    SubjectTypes = UtilConstants.CourseTypesDictionary(),
                    CertificateCode = entity.CertificateCode,
                    CertificateName = entity.CertificateName,
                };
                var AssignSubjectType = entity.CAT_GROUPSUBJECT_ITEM.Any() ? entity.CAT_GROUPSUBJECT_ITEM.FirstOrDefault().SubjectDetail.CourseTypeId : -1;
                model.ListSubjectType = _repoCouseService.GetCourseTypes().Select(a => new Subject_Types()
                {
                    ID = a.Course_Type_Id,
                    Name = a.str_Name,
                }).ToList();
                string listtype = string.Empty;
                foreach (var item in model.ListSubjectType)
                {
                    listtype += "<option value='" + item.ID + "'  " + (item.ID == AssignSubjectType ? "selected"


                                : "") + ">" + item.Name + "</option>";
                }
                model.HtmlSubjectType = listtype;
                return View(model);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Modify", ex.Message);
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                {
                    result = false,
                    message = ex.Message
                };
            }
            return RedirectToAction("Index", "MCourse");
        }
        [HttpPost]
        public async Task<ActionResult> Modify(GroupSubjectViewModel model)
        {
            var userId = CurrentUser.USER_ID.ToString();
            #region [VALIDATION]
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            try
            {

                _repoSubject.ModifyGroupSubject(model);

                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON CronGetListCategory)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_LIST_CATEGORY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Modify", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Code.Trim() + " - " + model.Name));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Code.Trim() + " - " + model.Name),
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                });
                var result = new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                };
                TempData[UtilConstants.NotifyMessageName] = result;
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Modify", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
        }
        //TODO:DELETE
        public ActionResult Editpage(int id = 0)
        {
            CAT_GROUPSUBJECT groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;

            return View(groupsubject);
        }
        //TODO:DELETE
        public ActionResult Detailpage(int id = 0)
        {
            CAT_GROUPSUBJECT groupsubject = new CAT_GROUPSUBJECT();
            if (id != 0)
            {
                groupsubject = _repoSubject.GetGroupSubjectById(id);
            }
            ViewBag.mModel = groupsubject;
            ViewBag.id = id;

            return View(groupsubject);
        }
        public ActionResult Create()
        {
            var model = new GroupSubjectViewModel
            {
                Id = 0,
                SubjectTypes = UtilConstants.CourseTypesDictionary(),
                AvailableSubjects = _repoSubject.GetSubjectDetail(a => a.IsActive == true && a.CourseTypeId.HasValue && a.CourseTypeId != 6).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.Code, a.Name))
            };
            model.ListSubjectType = _repoCouseService.GetCourseTypes().Select(a => new Subject_Types()
            {
                ID = a.Course_Type_Id,
                Name = a.str_Name,
            }).ToList();
            string listtype = string.Empty;
            foreach (var item in model.ListSubjectType)
            {
                listtype += "<option value='" + item.ID + "'>" + item.Name + "</option>";
            }
            model.HtmlSubjectType = listtype;
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> Create(GroupSubjectViewModel model)
        {
            var userId = CurrentUser.USER_ID.ToString();
            #region [VALIDATION]
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion
            try
            {

                _repoSubject.ModifyGroupSubject(model);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON CronGetListCategory)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_LIST_CATEGORY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Create", string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Code.Trim() + " - " + model.Name));
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Code.Trim() + " - " + model.Name),
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                });
                var result = new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                };
                TempData[UtilConstants.NotifyMessageName] = result;
                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Create", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult AjaxListSubjectInGroup(jQueryDataTableParamModel param, int? id)
        {
            if (id.HasValue)
            {
                var sub = _repoSubject.GetGroupSubjectById(id);
                var subs = sub?.CAT_GROUPSUBJECT_ITEM.Where(a => a.SubjectDetail.IsDelete == false /*&& a.SubjectDetail.IsActive==true*/).Select(a => a.SubjectDetail);

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SubjectDetail, object> orderingFunction = (c => sortColumnIndex == 1 ? c.Code
                                                          : sortColumnIndex == 2 ? c.Name
                                                          : sortColumnIndex == 3 ? c.Duration
                                                          : sortColumnIndex == 4 ? (object)c.RefreshCycle
                                                                        //: sortColumnIndex == 5 ? c?.Pass_Score.ToString(CultureInfo.InvariantCulture)
                                                                        : c.Name);

                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (subs != null && subs.Any())
                {
                    if (sortDirection != null)
                    {
                        subs = (sortDirection == "asc") ? subs.OrderBy(orderingFunction)
                                        : subs.OrderByDescending(orderingFunction);
                    }
                    var filtered = subs.ToList();
                    var filterRecords = filtered.Count();
                    var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                    var result = from c in displayed.ToArray()
                                 select new object[] {
                            string.Empty,
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.Code +"</span>",
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.Name +"</span>",
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.Duration +"</span>",
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +c.RefreshCycle +"</span>",
                             "<span "+(c?.IsActive != true ? "style='color:"+UtilConstants.String_DeActive_Color+";'":"")+">" +(c.IsAverageCalculate == true ? "Yes" : "No") +"</span>",
                            //c.Code,
                            //c.Name,
                            //c.Duration,
                            //c.RefreshCycle,
                            //c.Pass_Score,
                            //c.IsAverageCalculate == true ? "Yes" : "No",
                            };
                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = filterRecords,
                        iTotalDisplayRecords = filterRecords,
                        aaData = result
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(
                new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                    aaData = new List<object>()
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {

                var strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].Trim().ToLower();
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].Trim().ToLower();

                var model = _repoSubject.GetGroupSubject(a =>
                (string.IsNullOrEmpty(strCode) || a.Code.Trim().ToLower().Contains(strCode)) &&
                (string.IsNullOrEmpty(strName) || a.Name.Trim().ToLower().Contains(strName))).ToArray();
                var verticalBar = GetByKey("VerticalBar");
                var filtered = model.Select(a => new AjaxGroupCourse
                {
                    Code = a.Code ?? string.Empty,
                    Name = "<a href='javascript:void(0)'><span data-value='" + a.id + "' class='expand' style='cursor: pointer;'>" + a.Name ?? string.Empty + "</span></a>"  /*a.Name ?? string.Empty,*/,
                    Description = a.Description ?? string.Empty,
                    Status = (a.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_GroupSubject(0," + a.id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_GroupSubject(1," + a.id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                    Option = (
                     ((User.IsInRole("/MCourse/Modify")) ? "<a title='Edit' href='" + @Url.Action("Modify", new { a.id }) + "')' data-toggle='tooltip'><i class='fas fa-edit font-byhoa btnIcon_green' aria-hidden='true' ></i></a>" : "") +
                        ((User.IsInRole("/MCourse/Delete")) ? verticalBar + "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + a.id + ")' data-toggle='tooltip'><i class='fas fa-trash font-byhoa btnIcon_red' aria-hidden='true' ></i></a>" : "") + verticalBar +
                             ("<span data-value='" + a.id + "' class='expand' style='cursor: pointer;'><i class='fa fa-plus-circle btnIcon_gray font-byhoa' aria-hidden='true' ></i></span>")
                    )
                });

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxGroupCourse, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Code
                                                            : sortColumnIndex == 2 ? c.Name
                                                            : sortColumnIndex == 3 ? c.Description
                                                            : sortColumnIndex == 4 ? c.Status
                                                            : sortColumnIndex == 5 ? c.Option
                                                            : c.Code);

                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);

                var ajaxGroupCourses = filtered as AjaxGroupCourse[] ?? filtered.ToArray();
                var displayed = ajaxGroupCourses.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                         c?.Code,
                         c?.Name,
                         c?.Description,
                         c?.Status,
                         c?.Option
                         };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = ajaxGroupCourses.Count(),
                    iTotalDisplayRecords = ajaxGroupCourses.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/AjaxHandler", ex.Message);
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
        public async Task<ActionResult> Delete(int? id)
        {
            try
            {
                var model = _repoSubject.GetGroupSubjectById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var checkCourse = CourseService.Get(a => a.GroupSubjectId == id);
                if (checkCourse.Any(a => a.IsDeleted == false))
                {
                    return Json(new AjaxResponseViewModel()
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
                model.IsActive = false;
                model.IsDeleted = true;
                model.DeletedBy = CurrentUser.USER_ID.ToString();
                model.DeletedDate = DateTime.Now;
                model.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                _repoSubject.UpdateGroupSubject(model);


                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON CronGetListCategory)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_LIST_CATEGORY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Delete", string.Format(Messege.DELETE_SUCCESSFULLY, model.Name) + " " + Messege.ERROR_CALL_LMS);
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name) + "<br />" + Messege.ERROR_CALL_LMS,
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }


        [AllowAnonymous]
        public ActionResult ChangetypeReturnSubject(string id_type, int id = -1, int iscreate = 0)
        {
            try
            {
                if (!CMSUtils.IsNull(id_type))
                {
                    int id_type_ = Int32.Parse(id_type);
                    StringBuilder html = new StringBuilder();
                    var data = _repoSubject.GetSubjectDetail(a => a.IsDelete != true && a.CourseTypeId == id_type_);
                    var data_ = data.Select(b => b.Id);
                    var entity = _repoSubject.GetSubjectDetail(a => !data_.Contains(a.Id) && a.CourseTypeId == id_type_ && a.CAT_GROUPSUBJECT_ITEM.Any(b => b.id_groupsubject == id));
                    data = data.Concat(entity).OrderBy(a => a.Name.Trim());
                    if (data.Any())
                    {
                        foreach (var item in data)
                        {
                            var db = _repoSubject.GetGroupSubjectItem(a => a.id_subject == item.Id && a.id_groupsubject == id).FirstOrDefault();
                            if (db != null && db.SubjectDetail.CourseTypeId == id_type_)
                            {
                                html.AppendFormat("<option value='{0}' selected >{1}</option>", item.Id, item.Code + " - " + item.Name);
                            }
                            else
                            {
                                html.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.Code + " - " + item.Name);
                            }


                        }
                    }
                    return Json(new
                    {
                        htmlout = html.ToString()
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new
                {
                    htmlout = ""
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateGroupSubject(int isParticipate, string id)
        {
            try
            {
                int idgroupsubject = int.Parse(id);
                var removeGroupSubject = _repoSubject.GetGroupSubjectById(idgroupsubject);
                if (removeGroupSubject == null)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                if (isParticipate == 1)
                {
                    removeGroupSubject.IsActive = false;
                }
                else
                {
                    removeGroupSubject.IsActive = true;
                }
                removeGroupSubject.LmsStatus = (int)UtilConstants.ApiStatus.Modify;
                _repoSubject.UpdateGroupSubject(removeGroupSubject);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON CronGetListCategory)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_LIST_CATEGORY);
                    if (!callLms)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/SubmitSetParticipateGroupSubject", string.Format(Messege.SET_STATUS_SUCCESS, removeGroupSubject.Name) + " " + Messege.ERROR_CALL_LMS);
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SET_STATUS_SUCCESS, removeGroupSubject.Name) + "<br />" + Messege.ERROR_CALL_LMS,
                        //    result = false
                        //}, JsonRequestBehavior.AllowGet);
                    }
                    #endregion
                });

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, removeGroupSubject.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MCourse/SubmitSetParticipateGroupSubject", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }




        }
    }
}
