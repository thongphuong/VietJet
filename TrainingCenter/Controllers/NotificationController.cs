using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Courses;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.GroupUser;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Users;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Departments;
using TMS.Core.ViewModels.UserModels;
using TMS.Core.ViewModels.Notifications;
using TMS.Core.App_GlobalResources;
using TMS.Core.Utils;
using TrainingCenter.Utilities;

namespace TrainingCenter.Controllers
{
    public class NotificationController : BaseAdminController
    {
    
        private readonly IEmployeeService _repoEmployee;
     
        private readonly IDepartmentService _repoDepartmentService;

 
        public NotificationController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, ICourseService repoCourse, IEmployeeService repoEmployee , IDepartmentService departmentService, IApproveService approveService)
            : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, repoCourse,  approveService)
        {
    
            _repoEmployee = repoEmployee;
         
            _repoDepartmentService = departmentService;
         

        }
        public ActionResult Notification()
        {
        
            var departments = _repoDepartmentService.Get(a => a.IsActive == true).OrderBy(a => a.Ancestor).Select(a => new DepartmentModifyViewModel()
            {
                Id = a.Id,
                ParentId = a.ParentId,
                Name = a.Name,
            });
            var model = new DepartmentViewModel
            {
                DepartmentViewModels = _repoDepartmentService.Get(a => a.ParentId == null && a.IsActive == true).OrderBy(a => a.Ancestor).Select(a => new DepartmentModel()
                {
                    Id = a.Id,
                    DepartmentName = a.Name,
                    Code = a.Code
                }),
                DepartList = _repoDepartmentService.Get(a => a.ParentId != null && a.IsActive == true).ToDictionary(x => x.Id, x => string.Format("{0} - {1}",x.Code, x.Name)),
                ListSelect = departments.Select(a => a.Id).ToList(),
                //EmployeeList = _repoEmployee.GetEmp().ToDictionary(a => a.Id, a => string.Format("{0} - {1} {2}", a.str_Staff_Id, a.FirstName, a.LastName)),
                EmployeeList = _repoEmployee.GetEmp().ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.str_Staff_Id, ReturnDisplayLanguage(a.FirstName,a.LastName))),

            };
            
            return View(model);
        }
        [HttpPost]

        public ActionResult Notification(FormCollection form, DepartmentViewModel model)
        {
            try
            {
                var user = GetUser();
                var title_depart = model.Notification.Message;
                var content_depart = model.Notification.MessageContent;
                string[] keyvalue_depart = form["selectDepart"].Split(new char[] { ',' });   
                if (keyvalue_depart != null || content_depart != null || title_depart != null)
                {
                    foreach (var item in keyvalue_depart)
                    {
                        var Key = int.Parse(item);
                        SendNotification(UtilConstants.NotificationType.Department, null, null, Key, DateTime.Now, title_depart, content_depart, title_depart, content_depart);
                     
                    }
                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                    {
                        message = Messege.SUCCESS,
                        result = true
                    };
                }
                else
                {
                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                    {
                        message = "Object reference not set to an instance of an object.",
                        result = false,
                    };
                }
            }
            catch (Exception ex)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = false, message = ex.Message };
            }
            return RedirectToAction("Notification");
        }


        [HttpPost]
        public ActionResult SendTrainee(FormCollection form, DepartmentViewModel model)
        {
            try
            {
                var title_trainee = model.Notification.Message;
                var content_trainee = model.Notification.MessageContent;
                string[] keyvalue_trainee = form["selecttrainee"].Split(new char[] { ',' });
                if (keyvalue_trainee != null )
                {
                    if (content_trainee != null || title_trainee != null)
                    {
                        foreach (var item in keyvalue_trainee)
                        {
                            var Key = int.Parse(item);
                            SendNotification(UtilConstants.NotificationType.Trainee, null, null, Key, DateTime.Now, title_trainee, content_trainee, title_trainee, content_trainee);
                         
                        }
                        TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                        {
                            message = Messege.SUCCESS,
                            result = true
                        };
                    }
                    else
                    {
                        TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                        {
                            message = "Object reference not set to an instance of an object.",
                            result = false,
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = false, message = ex.Message };
            }
            return RedirectToAction("Notification");
        }

        public JsonResult GetValueDepartment(int value_department)
        {
            try
            {
                var html = new StringBuilder();
                var null_departmant = 1;
                var data = _repoDepartmentService.Get(a => a.ParentId == value_department);
                var data_parent = _repoDepartmentService.GetById(value_department);
                if (data.Any())
                {
                    null_departmant = 0;
                    foreach (var dr in data)
                    {
                        html.AppendFormat("<option value='{0}'>{1}-{2}</option>", dr.Id, dr.Code, dr.Name);
                    }
                }
                else
                {
                    null_departmant = 1;
                    
                    html.AppendFormat("<option value='{0}'>{1}-{2}</option>", data_parent.Id, data_parent.Code, data_parent.Name);

                }

                return Json(new
                {
                    result = true,
                    value_option = html.ToString(),
                    value_null = null_departmant
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message });
            }
        }

    }

}
