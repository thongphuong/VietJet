using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TrainingCenter.Controllers
{
    using System.Data.Entity.SqlServer;
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
    using TMS.Core.Services.PostNews;
    using TMS.Core.Services.Subject;
    using TMS.Core.Services.Users;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Jobtitles;
    using TMS.Core.ViewModels.PostNews;
    public class PostNewsController : BaseAdminController
    {
        private readonly IPostNewsService _postNewsService;

        private readonly IPostCategoryService _categoryService;
        public PostNewsController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ISubjectService subjectService, IApproveService approveService, IPostCategoryService categoryService, IPostNewsService postNewsService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService,  approveService)
        {
            _categoryService = categoryService;
            _postNewsService = postNewsService;
        }
        // GET: PostNews
        public ActionResult Index()
        {
            var model = new PostNewsModel
            {
                Categories = _categoryService.GetAll(a => a.IsActive==true)
                    .ToDictionary(a => a.Id, a => a.Name),
                GroupTrainee = EmployeeService.GetAllGroupTrainees(a => a.IsActived == true)
                    .ToDictionary(a => a.Id, a => a.Name)
            };
            return View(model);
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                
                var title = string.IsNullOrEmpty(Request.QueryString["Title"]) ? string.Empty : Request.QueryString["Title"].ToLower().Trim();
                var content = string.IsNullOrEmpty(Request.QueryString["Content"]) ? string.Empty : Request.QueryString["Content"].ToLower().Trim();
                var fSearchDateFrom = string.IsNullOrEmpty(Request.QueryString["StartDate"]) ? string.Empty : Request.QueryString["StartDate"].Trim();
                var fSearchDateTo = string.IsNullOrEmpty(Request.QueryString["EndDate"]) ? string.Empty : Request.QueryString["EndDate"].Trim();
                var Type = string.IsNullOrEmpty(Request.QueryString["Type"]) ? -1 : Convert.ToInt32(Request.QueryString["Type"].Trim());
                var groupTrainee = string.IsNullOrEmpty(Request.QueryString["GroupTraineeID"]) ? -1 : Convert.ToInt32(Request.QueryString["GroupTraineeID"].Trim());

                DateTime dateFrom;
                DateTime dateTo;
                DateTime.TryParse(fSearchDateFrom, out dateFrom);
                DateTime.TryParse(fSearchDateTo, out dateTo);
                dateFrom = dateFrom != DateTime.MinValue ? dateFrom.Date : dateFrom;
                dateTo = dateTo != DateTime.MinValue ? dateTo.Date.AddHours(23).AddMinutes(59).AddSeconds(59) : dateTo;

                var model = _postNewsService.GetAllPostNews(a => a.IsDeleted == false && 
                                                              (Type == -1 || a.Type == Type) &&
                                                              (groupTrainee == -1 || a.PostNew_GroupTrainee.Any(b=>b.GroupTraineeID == groupTrainee)) &&
                                                              (string.IsNullOrEmpty(title) || a.Title.ToLower().Trim().Contains(title)) &&
                                                              (string.IsNullOrEmpty(content) || a.Content.ToLower().Trim().Contains(content)) &&
                                                              (dateFrom == DateTime.MinValue || SqlFunctions.DateDiff("day", dateFrom, a.StartDate) >= 0) &&
                                                              (dateTo == DateTime.MinValue || SqlFunctions.DateDiff("day", a.EndDate, dateTo) >= 0)
                ).ToArray();

                var filtered = model.Select(a => new AjaxPostNew
                {
                    Title = a.Title,
                    PostBy = a.PostBy,
                    StartDate = a?.StartDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                    EndDate = a.EndDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                    Type = a?.Type == 1 ? Resource.lblPostNews : a?.Type == 0 ? Resource.lblPostNotification : a?.Type == 2 ? Resource.lblPostWelcome : string.Empty,
                    Status = (a.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_PostNew(0," + a.Id + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_PostNew(1," + a.Id + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                    Option = (
                                (User.IsInRole("/PostNews/Modify") ? "<a href='/PostNews/Modify/" + a.Id + "' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' title='Edit' ></i></a>" : "") +
                                (User.IsInRole("/PostNews/Delete") ? "<a href='javascript:void(0)' onclick='callDelete(" + a.Id + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' title='Delete' ></i></a>" : "")
                              )
                });

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<AjaxPostNew, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Title
                                                          : sortColumnIndex == 2 ? c.Type
                                                          : sortColumnIndex == 3 ? c.StartDate
                                                          : sortColumnIndex == 4 ? c.EndDate
                                                          : sortColumnIndex == 5 ? c.PostBy
                                                          : sortColumnIndex == 6 ? c.Status
                                                          : sortColumnIndex == 7 ? c.Option
                                                          : c.Title);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var ajaxPostNews = filtered.ToArray();
                var displayed = ajaxPostNews.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                string.Empty,
                                c?.Title ,
                                c?.Type,
                                c?.StartDate,
                                c?.EndDate,
                                  c?.PostBy,
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNews/AjaxHandler", ex.Message);
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
            var model = new PostNewsModel();
            var entity = _postNewsService.GetPostNewById(id);
            model.Categories = _categoryService.GetAll(a => a.IsActive==true)
                .ToDictionary(a => a.Id, a => a.Name);
            model.GroupTrainee = EmployeeService.GetAllGroupTrainees(a=>a.IsActived == true)
               .ToDictionary(a => a.Id, a => a.Name);
            model.Id = -1;
            model.Type = -1;
            if (entity != null)
            {
                model.Id = entity.Id;
                model.Title = entity.Title;
                model.Content = entity.Content;
                //model.Sort = entity.Sort ?? 0;
                model.StartDate = entity.StartDate;
                model.EndDate = entity.EndDate;
                model.ImgName = entity.Image;
                model.Description = entity.Description;
                model.Type = entity.Type ?? -1;
                model.Category = entity.CategoryID;
                model.GroupTraineeListID = entity.PostNew_GroupTrainee.Select(a => (int)a.GroupTraineeID).ToArray();
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Modify(PostNewsModel model)
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
                if (model.Id != -1)
                {
                    //update avatar or not

                    if (model.ImgFile != null)
                    {
                        var newBanner = SaveImagePost(model.ImgFile, model.Type);
                        if (newBanner.result)
                        {
                            model.ImgName = newBanner.data.ToString();
                        }
                        else
                        {
                            return Json(newBanner);
                        }
                    }
                }
                else
                {
                    var newBanner = SaveImagePost(model.ImgFile, model.Type);
                    if (newBanner.result)
                    {
                        model.ImgName = newBanner.data.ToString();
                    }
                    else
                    {
                        return Json(newBanner);
                    }
                }
                var currentUser = GetUser();

                _postNewsService.ModifyPostNew(model, currentUser.Username, currentUser.FirstName.Trim() + " " + currentUser.LastName.Trim());
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON NEW)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_POSTNEWS);
                    if (!callLms)
                    {
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, (model.Id.HasValue ? Resource.lblModify : Resource.lblCreate), model.Title),
                        //    result = false
                        //});
                    }
                    #endregion
                });
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNews/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SubmitSetParticipatePostNew(int isParticipate, string id)
        {
            var idPostNew = int.Parse(id);
            var removePostNew = _postNewsService.GetPostNewById(idPostNew);
            if (removePostNew == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            removePostNew.IsActive = isParticipate != 1;
            var currentUser = GetUser();
            removePostNew.ModifyBy = currentUser.Username;
            removePostNew.ModifyDate = DateTime.Now;
            _postNewsService.UpdatePostNew(removePostNew);
            await Task.Run(() =>
            {
                #region [--------CALL LMS (CRON USER)----------]
                var callLms = CallServices(UtilConstants.CRON_GET_POSTNEWS);
                if (!callLms)
                {
                    //return Json(new AjaxResponseViewModel()
                    //{
                    //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblModify, removePostNew.Title),
                    //    result = false
                    //});
                }
                #endregion
            });
            return Json(new AjaxResponseViewModel
            {
                message = string.Format(Messege.SET_STATUS_SUCCESS, removePostNew.Title),
                result = true
            }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id = -1)
        {
            try
            {
                var model = _postNewsService.GetPostNewById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNews/Delete", Messege.ISVALID_DATA);
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
                _postNewsService.UpdatePostNew(model);
                await Task.Run(() =>
                {
                    #region [--------CALL LMS (CRON USER)----------]
                    var callLms = CallServices(UtilConstants.CRON_GET_POSTNEWS);
                    if (!callLms)
                    {
                        //return Json(new AjaxResponseViewModel()
                        //{
                        //    message = string.Format(Messege.SUCCESSFULLY_BUT_ERROR_LMS, Resource.lblDelete, model.Title),
                        //    result = false
                        //});
                    }
                    #endregion
                });
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Title),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "PostNews/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}