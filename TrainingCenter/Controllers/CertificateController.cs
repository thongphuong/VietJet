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
    using TMS.Core.ViewModels.Certificate;
    using System.Text;

    public class CertificateController : BaseAdminController
    {
        public CertificateController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService,IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {

        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Modify(int? id)
        {
            var model = new CertificateViewModels();
            if (id.HasValue)
            {
                var entity = ConfigService.GetCertificateById(id);
                model.Id = entity.ID;
                model.Name = entity.Name;
                model.Template = entity.Content;
                model.TypeCertificateID = entity.Type.HasValue ? entity.Type.Value : 0;
            }
            else
            {
                model.Template = "<div style='background: no - repeat;position: relative;' id='Template' > </div>";
            }

            return View(model);
        }
        private string ReturnKey(int? ID)
        {
            StringBuilder HTML = new StringBuilder();
            var data_ = ConfigService.GetCertificateById(ID);
            var key = data_.KeyList.Split(new char[] { ',' });
            foreach (var item in key)
            {
                HTML.Append("<p>&nbsp;&nbsp; - &nbsp " + item + "</p>");
            }
            return HTML.ToString();
        }



        [HttpPost]
        public ActionResult Modify(CertificateViewModels model)
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
                    ConfigService.UpdateCertificate(model);
                }
                else
                {
                    ConfigService.InsertCertificate(model);
                }

                return Json(new AjaxResponseViewModel { result = true, message = Messege.SUCCESS },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Certificate/Modify", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }


        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                //string strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().ToString().Trim();
                string strName = string.IsNullOrEmpty(Request.QueryString["Name"]) ? string.Empty : Request.QueryString["Name"].ToLower().Trim();

                var model = ConfigService.GetCertificate(a => (string.IsNullOrEmpty(strName) || (a.Name.Contains(strName))) && a.IsDelete == false);
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                IEnumerable<CAT_CERTIFICATE> filtered = model;
                Func<CAT_CERTIFICATE, string> orderingFunction = (c => sortColumnIndex == 1 ? c.Name : c.Name);


                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction).ThenBy(a => a.Name)
                                  : filtered.OrderByDescending(orderingFunction).ThenBy(a => a.Name);

                var verticalBar = GetByKey("VerticalBar");
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed
                             select new object[] {
                                string.Empty,
                               
                                c?.Name,
                                c?.IsActive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Certificate(0,"+c?.ID+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Certificate(1,"+c?.ID+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",
                                "<a onclick='Blank_Review("+c?.ID+")' title='View' data-toggle='tooltip'><i class='fa fa-search btnIcon_blue font-byhoa' aria-hidden='true'></i></a>"+ verticalBar+
                                 ((User.IsInRole("/Certificate/Modify")) ? "<a  title='Edit' href='"+@Url.Action("Modify",new{id = c?.ID})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true'></i></a>":"") +
                                  ((User.IsInRole("/Trainee/Delete")) ? (verticalBar +"<a title='Delete' href='javascript:void(0)'  onclick='calldelete(" + c?.ID  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>") :"")  +"<div id='cer_body"+c?.ID+"' style='display:none;'>"+c?.Content+"</div>"
    };
                var jsonResult = Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = filtered.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = result
                }, JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Certificate/AjaxHandler", ex.Message);
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
        public ActionResult SubmitSetParticipateCertificate(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idCertificate = int.Parse(id);
                var removeCertificate = ConfigService.GetCertificateById(idCertificate);
                if (removeCertificate == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Certificate/SubmitSetParticipateCertificate", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                if (isParticipate == 1)
                {
                    removeCertificate.IsActive = false;
                }
                else
                {
                    removeCertificate.IsActive = true;
                }
                ConfigService.UpdateCertificate(removeCertificate);
                return Json(new AjaxResponseViewModel
                {

                    message = string.Format(Messege.SET_STATUS_SUCCESS, removeCertificate.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Certificate/SubmitSetParticipateCertificate", ex.Message);
                return Json(new AjaxResponseViewModel
                {

                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
           

        }
        [HttpPost]
        public ActionResult Delete(int? id)
        {
            try
            {
                var model = ConfigService.GetCertificateById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Certificate/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                var name = model.Name;
                if (model.Course_Result_Final.Any(a => a.IsDeleted == false))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, name),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                model.IsDelete = true;
                model.IsActive = false;
                model.UpdateAt = DateTime.Now;
                model.UpdateBy = CurrentUser.USER_ID;
                ConfigService.UpdateCertificate(model);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Certificate/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }

    }
}
