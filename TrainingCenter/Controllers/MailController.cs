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
using TMS.Core.ViewModels.Mail;

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
    using System.Text;
    using static TMS.API.Utilities.Command;

    public class MailController : BaseAdminController
    {
        public MailController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {

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
                string strName = string.IsNullOrEmpty(Request.QueryString["MailName"]) ? string.Empty : Request.QueryString["MailName"].ToLower().Trim();
                string strSubject = string.IsNullOrEmpty(Request.QueryString["MailSubject"]) ? string.Empty : Request.QueryString["MailSubject"].ToLower().Trim();

                var model = ConfigService.GetMail(a => (string.IsNullOrEmpty(strName) || (a.Name.Contains(strName))) && (string.IsNullOrEmpty(strSubject) || (a.SubjectMail.Contains(strSubject))));
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                IEnumerable<CAT_MAIL> filtered = model;
                Func<CAT_MAIL, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name
                                                          : sortColumnIndex == 2 ? c.Code
                                                          : c.SubjectMail);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "desc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction).ThenBy(a => a.Name)
                                  : filtered.OrderByDescending(a=>a.ID);


                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                string.Empty,
                                c?.Name ,
                                c?.SubjectMail,
                                c?.Content,
                                 ((User.IsInRole("/Mail/Modify")) ? "<a  title='Edit' href='"+@Url.Action("Modify",new{id = c.ID})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i></a>&nbsp;":"")
                                 +
                                  (User.IsInRole("/Mail/Modify") ? "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.ID + ")' data-toggle='tooltip' ><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : string.Empty)
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Mail/AjaxHandler", ex.Message);
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

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(MailViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                ConfigService.InsertMail(model);
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Mail/Create", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Modify(int? id)
        {

            var entity = ConfigService.GetMailById(id);
            var model = new MailViewModels();

            if (entity != null)
            {
                model.Id = entity.ID;
                model.Name = entity.Name;
                model.Code = entity.Code;
                model.Subject_Mail = entity.SubjectMail;
                model.TemplateMail = entity.Content;
            }
            model.KeyTagMail = ReturnKeyMail();
            return View(model);
        }
        private string ReturnKeyMail()
        {
            StringBuilder HTML = new StringBuilder();
            /* ----  Cach cu. Danh Sach Key lay theo MailID. Con bau gio doi lai Danh sach Key mac dinh Show het
            var data_ = ConfigService.GetMailById(mailID);
            if (data_.KeyList == null)
            {
                return null;
            }
            else
            {
                var keymail = data_.KeyList.Split(',');
                foreach (var item in keymail)
                {
                    HTML.Append("<p>&nbsp;&nbsp; - &nbsp " + item + "</p>");
                }
                return HTML.ToString();
            }
            */
            var data_ = ConfigService.GetAllCAT_MAIL_KEYs();
            if (data_.Count() < 0)
            {
                return null;
            }
            else
            {
                foreach (var item in data_)
                {
                    var keymail = item.Key_Name.Split(new char[] { ',' });
                    foreach (var item1 in keymail)
                    {
                        HTML.Append("<p>&nbsp;&nbsp; - &nbsp " + item1 + "</p>");
                    }
                }

                return HTML.ToString();
            }
        }
        [HttpPost]
        public ActionResult Modify(MailViewModels model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                if (model.Id.HasValue)
                {
                    ConfigService.UpdateMail(model);
                }
                else
                {
                    var checkCode = CodeExists(model.Code);
                    if (checkCode != null)
                    {
                        return checkCode;
                    }
                    else
                    {
                        ConfigService.InsertMail(model);
                    }
                }
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Mail/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        #region Quan ly TMS Send Mail
        [AllowAnonymous]
        public ActionResult SendMail()
        {
            var data = ConfigService.GetMemberMail();
            ViewBag.chuagui = data.Count(a => a.flag_sent == 0);
            ViewBag.dagui = data.Count(a => a.flag_sent == 1);
            ViewBag.loi = data.Count(a => a.flag_sent == 2);
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult AjaxHandlerSendMail(jQueryDataTableParamModel param)
        {
            try
            {
                //string strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().ToString().Trim();
                string strName = string.IsNullOrEmpty(Request.QueryString["MailName"]) ? string.Empty : Request.QueryString["MailName"].ToLower().Trim();
                string strSubject = string.IsNullOrEmpty(Request.QueryString["MailSubject"]) ? string.Empty : Request.QueryString["MailSubject"].ToLower().Trim();
                var status = string.IsNullOrEmpty(Request.QueryString["status"]) ? -1 : Convert.ToInt32(Request.QueryString["status"].Trim());
                var boolStatus = status == 1;
                var model = ConfigService.GetMemberMail(a => (string.IsNullOrEmpty(strName) || (a.mail_receiver.Contains(strName))) && (string.IsNullOrEmpty(strSubject) || (a.CAT_MAIL != null && a.CAT_MAIL.Name.Contains(strSubject))) && (status == -1 || a.flag_sent == status));

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                IEnumerable<TMS_SentEmail> filtered = model;
                Func<TMS_SentEmail, string> orderingFunction = (c => sortColumnIndex == 1 ? c.mail_receiver
                                                          : sortColumnIndex == 2 ? c.CAT_MAIL?.Name
                                                          : sortColumnIndex == 3 ? c.content_body
                                                          : sortColumnIndex == 4 ? c.flag_sent.ToString()
                                                          : c.type_sent.ToString());
                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "desc")
                    ? filtered.OrderBy(orderingFunction)
                    : filtered.OrderByDescending(x => x.Id);

                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                                string.Empty,
                                c?.mail_receiver ,
                                c?.CAT_MAIL?.Name,
                                (c?.content_body.Length > 50 ? c?.content_body.Substring(0,100) +  "<a data-toggle='tooltip' title='view more'  href='"+@Url.Action("SendMailModify",new{id = c?.Id}) + "')'>[...]</a>"  : c?.content_body),
                                ReturnStatus(c?.flag_sent),
                                "<a  title='View Detail' href='"+@Url.Action("SendMailModify",new{id = c?.Id}) + "')' data-toggle='tooltip'><i class='fas fa-search btnIcon_blue font-byhoa' aria-hidden='true'></i></a>&nbsp;" +
                                 "<a  title='Resend' onclick='callupdate("+c?.Id+")' data-toggle='tooltip'><i class='fas fa-paper-plane btnIcon_darkorchid font-byhoa' aria-hidden='true'></i></a>&nbsp;"
                                 //((User.IsInRole("/Mail/Modify")) ? "<a  title='Edit' href='"+@Url.Action("Modify",new{id = c.ID})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i></a>&nbsp;":"")
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Mail/AjaxHandler", ex.Message);
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
        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateStatus_Old(SendMailViewModels model)
        {
            try
            {
                var entity = ConfigService.GetSendMailById(model.Id);
                if (entity == null) return Json(new AjaxResponseViewModel { result = false, message = Messege.NO_DATA }, JsonRequestBehavior.AllowGet);
                entity.flag_sent = 0;

                ConfigService.UpdateStatusTMS_SentEmail(entity);
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Mail/UpdateStatus", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateStatus(SendMailViewModels model)
        {
            try
            {
                var entity = ConfigService.GetSendMailById(model.Id);
                if (entity == null) return Json(new AjaxResponseViewModel { result = false, message = Messege.NO_DATA }, JsonRequestBehavior.AllowGet);

                var mailTo = ReplaceChar(entity.mail_receiver);
                if (MailUtil.SendMail(mailTo, entity.subjectname ?? entity.CAT_MAIL?.SubjectMail, entity.content_body))
                {
                    entity.flag_sent = 1;
                }
                else
                {
                    entity.flag_sent = 2;
                }
                ConfigService.UpdateStatusTMS_SentEmail(entity);
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Mail/UpdateStatus", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SendMailModify(SendMailViewModels model)
        {
            try
            {
                ConfigService.ModifyTMS_SentEmail(model);
                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Mail/UpdateStatus", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult SendMailModify(int? id)
        {

            var entity = ConfigService.GetSendMailById(id);
            var model = new SendMailViewModels();
            if (entity != null)
            {
                model.Id = entity.Id;
                model.Email = entity.mail_receiver;
                model.Subject = entity.CAT_MAIL != null ? entity.CAT_MAIL.SubjectMail : entity.subjectname;
                model.TemplateMail = entity.content_body;
                model.FlagSend = entity.flag_sent ?? 0;
            }
            return View(model);
        }
        private static string ReturnStatus(int? type)
        {
            var return_ = "";
            switch (type)
            {
                case 0:
                    return_ = "<span class='label label-pending' style='background - color: #8B9999;'>Waiting</span>";
                    break;
                case 1:
                    return_ = "<span class='label label-success'>Done</span>";
                    break;
                case 2:
                    return_ = "<span  class='expand label label-danger' style='cursor: pointer;font-size: 13px;'>Error</span>";
                    break;
            }
            return return_;
        }
        #endregion

        public JsonResult CodeExists(string Code)
        {
            if (!string.IsNullOrEmpty(Code))
            {
                var CodeExistsObject = ConfigService.GetMail(a => a.Code == Code).FirstOrDefault();
                if (CodeExistsObject != null)
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState) + "The Code is exists",
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Delete(int id = -1)
        {
            try
            {
                var model = ConfigService.GetMailById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MailTemplate/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                else if (model.TMS_SentEmail.Any(a => a.cat_mail_ID == model.ID))
                {
                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.Name),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    model.IsDelete = true;
                    model.IsActive = false;
                    model.UpdateBy = CurrentUser.USER_ID;
                    model.UpdateAt = DateTime.Now;
                    ConfigService.DeleteMailTemplate(model);

                    return Json(new AjaxResponseViewModel
                    {
                        message = string.Format(Messege.DELETE_SUCCESSFULLY, model.Name),
                        result = true
                    }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "MailTemplate/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        public static string ReplaceChar(string result)
        {
            if (!string.IsNullOrEmpty(result))
            {
                result.Replace(" ", string.Empty);
                result.Replace(Environment.NewLine, string.Empty);
                result.Replace("\\t", string.Empty);
                result.Replace(";", string.Empty);
                result.Replace(",", ".");
                return result;
            }
            return string.Empty;
        }
    }
}
