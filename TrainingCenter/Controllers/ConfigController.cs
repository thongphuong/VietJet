using System;
using System.Linq;
using System.Web.Mvc;
using TMS.Core.Services.Approves;

namespace TrainingCenter.Controllers
{
    using System.Configuration;
    using System.Data.SqlClient;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;

    public class ConfigController : BaseAdminController
    {
        #region init

        public ConfigController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService ,IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
        }

        #endregion
        //
        // GET: /Admin/Config/
        public ActionResult Index()
        {
            if (!Is_View())
            {
                return RedirectToAction("Index", "Redirect");
            }
            var configs = ConfigService.Get().ToList();
            var subConfig = configs.FirstOrDefault(m => m.KEY == "Reminder_Before_Template");
            var TemplateMail = ConfigService.GetMail();
            var value = Convert.ToInt32(subConfig.VALUE);
            if ((TemplateMail.Where(x => x.ID == value).FirstOrDefault()) != null)
            {
                ViewBag.VbTemplateMailList = new SelectList(TemplateMail, "ID", "Name", TemplateMail.Where(m => m.ID == value).FirstOrDefault().ID);
            }
            else
            {
                ViewBag.VbTemplateMailList = new SelectList(TemplateMail, "ID", "Name");
            }
            return View(configs);
        }

        [HttpPost,ValidateInput(false)]
        public ActionResult Modify(FormCollection form)
        {
            try
            {
                var listId = form.GetValues("Id");
                var listValue = form.GetValues("Value");
                var TemplateId = form.GetValues("TemplateId");
                var ddlTemplateMail = form.GetValues("ddlTemplateMail");
                if (listId != null && true & listId.Any())
                {
                    //todo: move to service
                    for (int i = 0; i < listId.Count(); i++)
                    {
                        int id = Int32.Parse(listId[i]);
                        var item = ConfigService.GetById(id);
                        if (listValue != null) item.VALUE = listValue[i];
                        ConfigService.UpdateConfig(item);
                    }
                }
                if (!string.IsNullOrEmpty(ddlTemplateMail[0]))
                {
                    var item = ConfigService.GetById(Int32.Parse(TemplateId[0]));
                    if (item.VALUE.Trim() != ddlTemplateMail[0].Trim())
                    {
                        item.VALUE = ddlTemplateMail[0];
                        ConfigService.UpdateConfig(item);
                    }
                }
                ViewBag.Messege = "Modify successfully";
                return RedirectToAction("Index", "Config");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.ToString() ;
                return RedirectToAction("Index", "Config");
            }
           
        }


	}

}