using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Utils;

namespace TrainingCenter.Controllers
{
    using DAL.Entities;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.Services.Nationality;
    using TMS.Core.ViewModels;
    using TMS.Core.ViewModels.Nation;
    using System.Globalization;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.Services;

    public class NationalityController : BaseAdminController
    {
        #region Variables

        private readonly IConfigService _repoNationality;
        private readonly INationalityService _nationalityService;
        #endregion

        public NationalityController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService,  IConfigService repoNation, IApproveService approveService, INationalityService nationalityService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoNationality = repoNation;
            _nationalityService = nationalityService;
        }

        public ActionResult Index(int id = 0)
        {
            return View();
        }

        public ActionResult Modify(int? id)
        {
            var nation = new NationModels();

            if (id.HasValue)
            {
                var entity = CourseService.GetNationById(id);
                nation.Id = entity.id;
                nation.Code = entity.Nation_Code;
                nation.Name = entity.Nation_Name;
                nation.Description = entity.description;
                nation.isActive = (bool)entity.isactive;
            }
            return View(nation);
        }

        [HttpPost]
        public ActionResult Modify(NationModels model)
        {
            if (ModelState.IsValid)
            {
                var codeHasSpaceMessage = string.Format(Messege.WARNING_CODE_HAS_SPACE, model.Code);
                if (model.Code.Contains(" "))
                {
                    return Json(new AjaxResponseViewModel { result = false, message = codeHasSpaceMessage }, JsonRequestBehavior.AllowGet);
                }
                var entity = CourseService.GetNationById(model.Id);
                if (entity == null)
                {
                    if (CourseService.GetNation(a => a.Nation_Code.ToLower() == model.Code.ToLower()).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE },JsonRequestBehavior.AllowGet);
                    }

                    CourseService.InsertNation(model);
                }
                else
                {
                    if (CourseService.GetNation(a => a.Nation_Code.ToLower() == model.Code.ToLower() && a.id != model.Id).Any())
                    {
                        return Json(new AjaxResponseViewModel { result = false, message = Messege.EXISTING_CODE },JsonRequestBehavior.AllowGet);
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
                var strCode = string.IsNullOrEmpty(Request.QueryString["strCode"]) ? string.Empty : Request.QueryString["strCode"].ToLower();
                var strName = string.IsNullOrEmpty(Request.QueryString["strName"]) ? string.Empty : Request.QueryString["strName"].ToLower();
                var url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower();

                var data = _repoNationality.GetNation(a => a.bit_Deleted==false &&
                    (string.IsNullOrEmpty(strCode) || a.Nation_Code.Contains(strCode)) &&
                    (string.IsNullOrEmpty(strName) || a.Nation_Name.Contains(strName)));


                List<Nation> models = data.ToList();
                IEnumerable<Nation> filtered = models;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<Nation, string> orderingFunction = (c => sortColumnIndex == 1 ? c?.Nation_Code
                                                          : sortColumnIndex == 2 ? c?.Nation_Name
                                                          : sortColumnIndex == 3 ? c?.description
                                                          : c.Nation_Code);


                var sortDirection = Request["sSortDir_0"]; // asc or desc
                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                                       : filtered.OrderByDescending(orderingFunction);

                //var cscost = filtered.ToArray();
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                             select new object[] {
                               string.Empty,
                         c.Nation_Code,
                         "<a href='" + @Url.Action("Modify",new {id = c.id}) + "'>"+c.Nation_Name+"</a>",                        
                         //c.createuser,
                         //c.createday.ToString(),
                         c.description,
                         c.isactive == false ? "<i class='fa fa-toggle-off' onclick='Set_Participate_Nation(0,"+c.id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "<i class='fa fa-toggle-on'  onclick='Set_Participate_Nation(1,"+c.id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>",
                        ((Is_Edit(url)) ? "<a title='Edit' href='"+@Url.Action("Modify",new{id = c.id})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : "") +
                        ((Is_Delete(url)) ? verticalBar + "<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Nationality/AjaxHandler", ex.Message);
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
        public ActionResult delete(int id = -1)
        {
            try
            {
                //var model = CourseService.GetNationById(id);           
                var model = _nationalityService.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Nationality/delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                

                model.bit_Deleted = true;
                model.isactive = false;
                CourseService.UpdateNation(model);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY,model.Nation_Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Nationality/delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult SubmitSetParticipateNation(int isParticipate, string id)
        {
            try
            {
                int idnational = int.Parse(id);
                var removeNational = CourseService.GetNationById(idnational);
                if (removeNational == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Nationality/SubmitSetParticipateNation", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                  
                if (isParticipate == 1)
                {
                    removeNational.isactive = false;
                }
                else
                {
                    removeNational.isactive = true;
                }
                CourseService.UpdateNation(removeNational);

                return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeNational.Nation_Name), result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Nationality/SubmitSetParticipateNation", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);
            }
           
        }
    }
}
