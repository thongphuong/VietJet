using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using System.Globalization;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Utils;

namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using TMS.Core.Services;
    using TMS.Core.Services.Companies;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Company;

    public class PartnerController : BaseAdminController
    {
        #region Variables
        private readonly ICompanyService _repoPartner;


        #endregion

        public PartnerController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ICompanyService repoPartner, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoPartner = repoPartner;
        }

        public ActionResult Index(int id = 0)
        {
            return View();
        }

        public ActionResult Modify(int? id)
        {
            var company = new CompanyModels();

            if (id.HasValue)
            {
                var entity = CourseService.GetCompanyById(id);
                company.Id = entity.Company_Id;
                company.Code = entity.str_code;
                company.Name = entity.str_Name;
                company.Description = entity.dicsription;
            }
            return View(company);
        }

        [HttpPost]
        public ActionResult Modify(CompanyModels model)
        {
            if (ModelState.IsValid)
            {
                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
                if (model.Code.Contains(" "))
                {
                    return Json(new AjaxResponseViewModel { result = false, message = codeHasSpaceMessage }, JsonRequestBehavior.AllowGet);
                }
                var entity = CourseService.GetCompanyById(model.Id);

                if (entity == null)
                {
                    if (CourseService.GetCompany(a => a.str_code.ToLower() == model.Code.ToLower()).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE }, JsonRequestBehavior.AllowGet);
                    }

                    CourseService.InsertCompany(model);
                }
                else
                {
                    if (CourseService.GetCompany(a => a.str_code.ToLower() == model.Code.ToLower() && a.Company_Id != model.Id).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE }, JsonRequestBehavior.AllowGet);
                    }

                    CourseService.Update(model);
                }
                var ajaxResult = new AjaxResponseViewModel()
                {
                    result = true,
                    message = Messege.SUCCESS
                };
                TempData[UtilConstants.NotifyMessageName] = ajaxResult;
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                string strCode = string.IsNullOrEmpty(Request.QueryString["strCode"]) ? string.Empty : Request.QueryString["strCode"].ToLower().Trim();
                string strName = string.IsNullOrEmpty(Request.QueryString["strName"]) ? string.Empty : Request.QueryString["strName"].ToLower().Trim();
                string url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().Trim();
                string strDesciption = string.IsNullOrEmpty(Request.QueryString["strDescription"]) ? string.Empty : Request.QueryString["strDescription"].ToLower().Trim();
                var data = _repoPartner.Get(a => a.bit_Deleted==false &&
                    (string.IsNullOrEmpty(strCode) || a.str_code.Contains(strCode)) &&
                    (string.IsNullOrEmpty(strName) || a.str_Name.Contains(strName)) &&
                    (string.IsNullOrEmpty(strDesciption) || a.dicsription.Contains(strDesciption)));


                List<Company> models = data.ToList();
                IEnumerable<Company> filtered = models;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Company, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.str_code
                                                          : sortColumnIndex == 2 ? c?.str_Name
                                                          : sortColumnIndex == 3 ? c?.dicsription
                                                          : c.str_Name);


                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                                       : filtered.OrderByDescending(orderingFunction);
                var verticalBar = GetByKey("VerticalBar");
                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                         c.str_code,
                         "<a href='" + @Url.Action("Modify",new {id = c.Company_Id}) + "'>"+c.str_Name+"</a>",
                         c.dicsription,

                         c.IsActive != true ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Partner(0,"+c.Company_Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Partner(1,"+c.Company_Id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",

                        ((Is_Edit(url)) ? "<a title='Edit' href='"+@Url.Action("Modify",new{id = c.Company_Id})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : "") +
                        ((Is_Delete(url)) ? verticalBar +"<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.Company_Id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Partner/AjaxHandler", ex.Message);
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
        public ActionResult SubmitSetStatusPartner(int isStatus, string id)
        {
            int idPartner = int.Parse(id);
            var removeCost = CourseService.GetCompanyById(idPartner);
            if (removeCost == null)
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.UNSUCCESS,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            if (isStatus == 1)
            {
                removeCost.IsActive = false;
            }
            else
            {
                removeCost.IsActive = true;
            }
            CourseService.UpdateCompany(removeCost);


            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeCost.str_Name), result = true }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                var currentUser = GetUser().Username.ToString(CultureInfo.CurrentCulture);
                var model = CourseService.GetCompanyById(id);
                model.bit_Deleted = true;
                model.dtm_Deleted_At = DateTime.Now;
                model.str_Deleted_By = currentUser;
                CourseService.UpdateCompany(model);

                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Partner/delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}