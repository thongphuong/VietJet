using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using DAL.Entities;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Roles;
using TMS.Core.Services.Users;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Role;

namespace TrainingCenter.Controllers
{
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.ViewModel.RoleMenus;
    using TMS.Core.App_GlobalResources;
    using DAL.Repositories;
    using Newtonsoft.Json;
    using TMS.Core.Services;

    public class LogController : BaseAdminController
    {
        #region Init

        private readonly IConfigService _configService;
        #endregion


        public LogController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _configService = configService;
        }

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {

                var ddl_LogEvent = string.IsNullOrEmpty(Request.QueryString["ddl_LogEvent"]) ? -1 : Convert.ToInt32(Request.QueryString["ddl_LogEvent"].Trim());

                string Content = string.IsNullOrEmpty(Request.QueryString["Content"]) ? string.Empty : Request.QueryString["Content"].Trim().ToLower();
                string Username = string.IsNullOrEmpty(Request.QueryString["Username"]) ? string.Empty : Request.QueryString["Username"].Trim().ToLower();
                string URL = string.IsNullOrEmpty(Request.QueryString["URL"]) ? string.Empty : Request.QueryString["URL"].Trim().ToLower();
                string Ip = string.IsNullOrEmpty(Request.QueryString["Ip"]) ? string.Empty : Request.QueryString["Ip"].Trim().ToLower();
                string ServerName = string.IsNullOrEmpty(Request.QueryString["ServerName"]) ? string.Empty : Request.QueryString["ServerName"].Trim().ToLower();
                string ClientInfo = string.IsNullOrEmpty(Request.QueryString["ClientInfo"]) ? string.Empty : Request.QueryString["ClientInfo"].Trim().ToLower();


                var data = _configService.GetSys_Log(a=> a.IsDeleted == false &&
                (ddl_LogEvent == -1 || a.LogEvent == ddl_LogEvent) &&
                (string.IsNullOrEmpty(Content) || a.Content.Contains(Content)) &&
                (string.IsNullOrEmpty(Username) || a.USER.USERNAME.Contains(Username)) &&
                (string.IsNullOrEmpty(URL) || a.FullUrl.Contains(URL)) &&
                (string.IsNullOrEmpty(Ip) || a.IP.Contains(Ip)) &&
                (string.IsNullOrEmpty(ServerName) || a.ServerName.Contains(ServerName)) &&
                (string.IsNullOrEmpty(ClientInfo) || a.ClientInfo.Contains(ClientInfo))
                ).OrderByDescending(a => a.CreateDay);

                IEnumerable<SYS_LogEvent> filtered = data;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<SYS_LogEvent, object> orderingFunction = (c => sortColumnIndex == 1 ? c.LogEvent
                                                          : sortColumnIndex == 2 ? c.Content
                                                          : sortColumnIndex == 3 ? c.USER.LASTNAME
                                                          : sortColumnIndex == 4 ? c.FullUrl
                                                          : sortColumnIndex == 5 ? c.IP
                                                          : sortColumnIndex == 6 ? c.ServerName
                                                          : sortColumnIndex == 7 ? c.ClientInfo
                                                           : sortColumnIndex == 7 ? (object)c.CreateDay
                                                          : c.CreateDay);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                    let logEvent = c.LogEvent
                    where logEvent != null
                    select new object[] {
                               string.Empty,
                               ReturnLogEvent((int)logEvent),
                               c.Source,
                                "<span data-value='"+ c?.Content+"' class='expand' style='cursor: pointer;'><i class='fa fa-search' aria-hidden='true'></i></span>",//.Replace(",","<br />").Replace('\"',' ').Replace('"',' ').Replace("{","").Replace("}","")
                              c?.USER?.USERNAME,
                               c?.FullUrl,
                               c?.IP,
                               c?.ServerName,
                               c?.ClientInfo,
                                c.CreateDay != null ? c.CreateDay.Value.ToString("dd/MM/yyyy HH:mm:ss") : string.Empty,
                               (User.IsInRole("/Log/delete") ? "<a href='javascript:void(0)' onclick='calldelete(" + c.Id + ")' data-toggle='tooltip' title='Delete'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : string.Empty),
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
            catch (Exception)
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

        private string ReturnLogEvent(int logEvent)
        {
            var _str = "";
            switch (logEvent)
            {
                case (int)UtilConstants.LogEvent.Insert:
                    _str = "Insert";
                    break;
                case (int)UtilConstants.LogEvent.Update:
                    _str = "Update";
                    break;
                case (int)UtilConstants.LogEvent.Delete:
                    _str = "Delete";
                    break;
                case (int)UtilConstants.LogEvent.Active:
                    _str = "Active";
                    break;
                case (int)UtilConstants.LogEvent.View:
                    _str = "View";
                    break;
                case (int)UtilConstants.LogEvent.Error:
                    _str = "Error";
                    break;
            }
            return _str;
        }
        private string ReturnLogSource(int logEvent)
        {
            var _str = "";
            switch (logEvent)
            {
                case (int)UtilConstants.LogSourse.Role:
                    _str = "Role";
                    break;
                case (int)UtilConstants.LogSourse.USER:
                    _str = "USER";
                    break;
                case (int)UtilConstants.LogSourse.Course:
                    _str = "Course";
                    break;
                case (int)UtilConstants.LogSourse.CourseCost:
                    _str = "Course Cost";
                    break;
            }
            return _str;
        }
        [HttpPost]
        public ActionResult DeleteAll(int? type)
        {
            try
            {
                var result = _configService.Delete(type);
                if (result)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.DELETE_SUCCESSFULLY, ""),
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.VALIDATION_SCHEDULE_TYPE,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

           
        }


        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                var model = _configService.GetLogById(id);
                if (model == null)
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.UNSUCCESS,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                model.IsDeleted = true;              
                model.DeletedBy = CurrentUser.USER_ID;
                model.DeletedAt = DateTime.Now;
                _configService.UpdateLog(model);

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY,""),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}
