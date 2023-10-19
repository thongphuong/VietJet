using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Core.Services.PostNews;

namespace TrainingCenter.Controllers
{
    using System.Configuration;
    using System.Threading.Tasks;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.Approves;
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
    using TMS.Core.ViewModels.Jobtitles;
    using TMS.Core.ViewModels.PostNews;

    public class PostNewsCategoryController : BaseAdminController
    {
        // GET: PostNewsCategory

        private readonly IPostCategoryService _categoryService;
        private const string CatePrefix = "C";

        public PostNewsCategoryController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ISubjectService subjectService, IApproveService approveService, IPostCategoryService categoryService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService,  approveService)
        {
            _categoryService = categoryService;
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                //string strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().ToString().Trim();
                var name = string.IsNullOrEmpty(Request.QueryString["Name"]) ? string.Empty : Request.QueryString["Name"].ToLower().Trim();


                var model = _categoryService.GetAll(a => a.IsDeleted == false &&
                                                            (string.IsNullOrEmpty(name) || a.Name.ToLower().Contains(name.Trim()))

                ).ToArray();
                var filtered = model.Select(a => new AjaxCategory
                {
                    Code = a.Code,
                    Name = a.Name,
                    //NameParent = a.ParentId != null ? a.INTRO_CATEGORY2.Name : string.Empty,
                    Description = a.Description,
                    Ancestor = a.Ancestor,
                    Status = (a.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Category(0," + a.Id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Category(1," + a.Id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                    Option = (
                                "<a title='Edit' href='/PostNewsCategory/Modify/" + a.Id + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" +
                                 "<a title='Delete' href='javascript:void(0)' onclick='callDelete(" + a.Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>"
                              )
                });



                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxCategory, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name
                                                          //: sortColumnIndex == 2 ? c.NameParent
                                                          : sortColumnIndex == 2 ? c.Description
                                                          : sortColumnIndex == 3 ? c.Status
                                                          : sortColumnIndex == 4 ? c.Option
                                                          : c.Name);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var ajaxPostNews = filtered.ToArray();
                var displayed = ajaxPostNews.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                string.Empty,

                                c?.Name,
                                //c?.NameParent,
                                c?.Description,
                                c?.Status,
                                c?.Option
                             };
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = ajaxPostNews.Count(),
                    iTotalDisplayRecords = ajaxPostNews.Count(),
                    aaData = result
                },
           JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNewsCategory/AjaxHandler", ex.Message);
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

        public ActionResult Modify(int id = -1)
        {
            var model = new PostNewsCategoryModel {Id = -1};
            var data = _categoryService.GetById(id);
            //model.Parent = _categoryServiceService.GetAll(a => a.IsActive && a.Id != id)
            //    .ToDictionary(a => a.Id, a =>  a.Name);
            if (data != null)
            {
                model.Id = data.Id;
                model.Code = data.Code;
                model.Name = data.Name;
                //model.Sort = data.Sort;
                model.Icon = data.Icon;
                model.BackgroundColor = data.Background;
                model.Description = data.Description;
                //model.ParentId = data.ParentId ?? -1;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Modify(PostNewsCategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var currentUser = GetUser();
                _categoryService.Modify(model, currentUser.Username);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON USER)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_CATEGORY_POSTNEWS);
                    if (!callLms)
                    {
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Name),
                        //    result = false
                        //});
                    }
                    #endregion
                });
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNewsCategory/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }


        }


        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipateCategory(int isParticipate, string id)
        {
            var idPostNew = int.Parse(id);
            var removeCate = _categoryService.GetById(idPostNew);
            if (removeCate == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            if (isParticipate == 1)
            {
                removeCate.IsActive = false;
            }
            else
            {
                removeCate.IsActive = true;
            }
            var currentUser = GetUser();
            removeCate.ModifyBy = currentUser.Username;
            removeCate.ModifyDate = DateTime.Now;
            _categoryService.Update(removeCate);
            await Task.Run(() =>
            {
                #region [--------CALL LMS (CRON USER)----------]
                var callLms = CallServices(UtilConstants.CRON_GET_CATEGORY_POSTNEWS);
                if (!callLms)
                {
                    //return Json(new AjaxResponseViewModel()
                    //{
                    //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, removeCate.Name),
                    //    result = false
                    //});
                }
                #endregion
            });
            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.SET_STATUS_SUCCESS, removeCate.Name),
                result = true
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id = -1)
        {
            try
            {
                var model = _categoryService.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNewsCategory/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                model.IsDeleted = true;
                model.IsActive = false;
                model.ModifyBy = CurrentUser.Username;
                model.ModifyDate = DateTime.Now;
                _categoryService.Update(model);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON USER)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_CATEGORY_POSTNEWS);
                    if (!callLms)
                    {
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblDelete, model.Name),
                        //    result = false
                        //});
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNewsCategory/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }

    }
}