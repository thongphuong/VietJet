using System;
using System.Linq;
using System.Web.Mvc;
using TMS.Core.Services.Approves;

namespace TrainingCenter.Controllers
{
    using global::Utilities;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.IO;
    using System.Web;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TrainingCenter.Utilities;

    public class ExtensionController : BaseAdminController
    {
        #region init

        public ExtensionController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
        }

        #endregion
        // GET: Extension
        public ActionResult Index()
        {
            if (!Is_View())
            {
                return RedirectToAction("Index", "Redirect");
            }
            var configs = ConfigService.Get(m=>m.isExtension==true).ToList();
            var subConfig = configs.FirstOrDefault(m => m.KEY == "Reminder_Before_Template");
            var TemplateMail = ConfigService.GetMail();
            var value = Convert.ToInt32(subConfig.VALUE);
            ViewBag.VbTemplateMailList = new SelectList(TemplateMail, "ID", "Name", TemplateMail.Where(m => m.ID == value).FirstOrDefault()?.ID ?? 0);
            return  View(configs);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Modify(FormCollection form, HttpPostedFileBase ImgFile)
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
                    if(item.VALUE.Trim()!= ddlTemplateMail[0].Trim())
                    {
                        item.VALUE = ddlTemplateMail[0];
                        ConfigService.UpdateConfig(item);
                    }
                }

                if (ImgFile != null)
                {
                    {
                        var useAws = AppUtils.getAppSetting("UseAws");
                        if (useAws == "1")
                        {
                            string pic = "background_login.png";
                            string path = System.IO.Path.Combine(
                                                   "Uploads/Background/", pic);
                            if (!AWSUtils.AWS_CheckFolderExists("Uploads/Background/"))
                            {
                                AWSUtils.AWS_RunFolderCreationDemo(path);
                            }
                            else
                            {
                                AWSUtils.AWS_DeleteFile("Uploads/Background/");
                            }
                            AWSUtils.AWS_PutObject(path, ImgFile.InputStream);
                        }
                        else
                        {
                            //Doi ten Current Background File
                            //int Fcount = System.IO.Directory.GetFiles(Server.MapPath("~/Uploads/Background/")).Length;
                            //System.IO.File.Move(Server.MapPath("~/Uploads/Background/background_login.png"),Server.MapPath("~/Uploads/Background/")+Fcount.ToString() + ".png");


                            //string pic = System.IO.Path.GetFileName(ImgFile.FileName);
                            string pic = "background_login.png";
                            string path = System.IO.Path.Combine(
                                                   Server.MapPath("~/Uploads/Background/"), pic);
                            // file is uploaded
                            FileInfo temp = new FileInfo(path);
                            if (temp.Exists) temp.Delete();
                            ImgFile.SaveAs(path);
                        }
                    }
                }


                ViewBag.Messege = "Modify successfully";
                return RedirectToAction("Index", "Extension");
            }
            catch (Exception ex)
            {              
                ViewBag.Error = ex.ToString();
                return RedirectToAction("Index", "Extension");
            }

        }
    }
}