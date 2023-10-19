using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using DAL.Entities;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.Utils;
using TMS.Core.ViewModels.Common;
using TMS.Core.ViewModels.Cost;

namespace TrainingCenter.Controllers
{
    using TMS.Core.Services;
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.Cost;
    using TMS.Core.Services.CourseDetails;
    using TMS.Core.Services.CourseMember;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.Employee;
    using TMS.Core.Services.Notifications;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels;

    public class CostController : BaseAdminController
    {
        #region MyRegion

        private readonly ICostService _repoCosts;

        public CostController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, ICostService repoCosts,IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoCosts = repoCosts;
        }

        #endregion
        //
        // GET: /Admin/User/
        #region Index
        // return view
        public ActionResult Index(int id = 0)
        {
            CAT_COSTS Costs = new CAT_COSTS();
            if (id != 0)
            {
                Costs = _repoCosts.GetById(id);
            }
            // ViewBag.Cost = Costs;
            // ViewBag.id = id;
            return View(Costs);
        }
        public ActionResult Modify(int id = 0)
        {
            var model = new CostModel();
            model.GroupCosts = _repoCosts.GetGroupCost().OrderBy(a => a.Name).ToDictionary(a => a.Id, a => a.Code + " - " + a.Name);
            if (id != 0)
            {
                var cost = _repoCosts.GetById(id);
                model.Id = cost.id;
                model.Code = cost.str_Code;
                model.Name = cost.str_Name;
                model.Description = cost.str_Description;
                model.GroupCostId = (cost.GroupCostId ?? -1);

            }



            return View(model);
        }
        
    

        public ActionResult Editpage(int id = 0)
        {
            var model = new CostModel();

            if (id != 0)
            {
                var cost = _repoCosts.GetById(id);
                model.Id = cost.id;
                model.Code = cost.str_Code;
                model.Name = cost.str_Name;
                model.Description = cost.str_Description;
            }

            return View(model);
        }
        public ActionResult Detailpage(int id = 0)
        {
            var model = new CostModel();

            if (id != 0)
            {
                var cost = _repoCosts.GetById(id);
                model.Id = cost.id;
                model.Code = cost.str_Code;
                model.Name = cost.str_Name;
                model.Description = cost.str_Description;
            }

            return View(model);
        }
        public ActionResult Createpage(int id = 0)
        {
            var model = new CostModel();
            return View(model);
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var strCode = string.IsNullOrEmpty(Request.QueryString["Code"]) ? string.Empty : Request.QueryString["Code"].ToLower().Trim();
                var strName = string.IsNullOrEmpty(Request.QueryString["FullName"]) ? string.Empty : Request.QueryString["FullName"].ToLower().Trim();
                var url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().Trim();

                var data = _repoCosts.Get(a =>
                (string.IsNullOrEmpty(strCode) || a.str_Code.Contains(strCode)) &&
                (string.IsNullOrEmpty(strName) || a.str_Name.Contains(strName)));


                IEnumerable<CAT_COSTS> filtered = data;

                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<CAT_COSTS, string> orderingFunction = (c => sortColumnIndex == 1 ? c.str_Code
                                                          : sortColumnIndex == 2 ? c.str_Name
                                                         // : sortColumnIndex == 3 ? c.CAT_GROUPCOST?.Name
                                                          : sortColumnIndex == 3 ? c.str_Description
                                                          : c.str_Name);


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
                         c.str_Code,
                         c.str_Name,
                          //c.CAT_GROUPCOST?.Name,
                         c.str_Description,
                         //(c.str_Code.Equals("C001") || c.str_Code.Equals("C002")) ? string.Empty : 
                         (c.IsActive == false ? "&nbsp;<i class='fa fa-toggle-off' onclick='SetStatusCost(0,"+c.id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='SetStatusCost(1,"+c.id+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>"),
                         //c.dtm_Dreated_date?.ToString("dd/MM/yyyy"),
                         //c.dtm_Updated_date?.ToString("dd/MM/yyyy"),
                         ((Is_Edit(url)) ? "<a title='Edit' href='"+@Url.Action("Modify",new{id = c.id})+"' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : "") +
                        ((c.str_Code.Equals("C001") || c.str_Code.Equals("C002")) ? string.Empty : ((Is_Delete(url)) ? verticalBar +"<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : ""))
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region User-Modify

        [HttpPost]
        public ActionResult Modify(CostModel model)
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
                _repoCosts.Modify(model);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR,UtilConstants.LogEvent.Error, "Cost/Modify", ex.Message);
                
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            var result = new AjaxResponseViewModel
            {
                message = Messege.SUCCESS,
                result = true
            };
            TempData[UtilConstants.NotifyMessageName] = result;
            return Json(result, JsonRequestBehavior.AllowGet);
          
        }
        #endregion


        [HttpPost]
        public ActionResult delete(int id = -1)
        {
            try
            {
                var model = _repoCosts.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false

                    }, JsonRequestBehavior.AllowGet);
                }
                if (model.Course_Cost.Any(a => a.IsDeleted == true))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.str_Name),
                        result = false

                    }, JsonRequestBehavior.AllowGet);
                }
                model.IsDeleted = true;
                model.IsActive = false;
                model.dtm_Updated_date = DateTime.Now;
                model.int_Updated_by = CurrentUser.USER_ID;
                _repoCosts.Update(model);

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.str_Code),
                    result = true

                }, JsonRequestBehavior.AllowGet)
                ;

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false

                }, JsonRequestBehavior.AllowGet)
               ;
            }

        }

        [HttpPost]
        public ActionResult SubmitSetStatusCost(int isStatus, string id)
        {
            try
            {
                int idCost = int.Parse(id);
                var removeCost = _repoCosts.GetById(idCost);
                if (removeCost == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/SubmitSetStatusCost", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                if (isStatus == 1)
                {
                    removeCost.IsActive = false;
                }
                else
                {
                    removeCost.IsActive = true;
                }
                _repoCosts.Update(removeCost);


                return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeCost.str_Name), result = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/SubmitSetStatusCost", ex.Message);
                return Json(new AjaxResponseViewModel { message = ex.Message, result = false }, JsonRequestBehavior.AllowGet);

            }
           
        }

        #region CAT_GROUPCOST
        public ActionResult ModifyGroupCost(int? id = -1)
        {
            var model = new PartialGroupCostViewModify();
            if (id.HasValue && id != -1)
            {
                var groupCost = _repoCosts.GetGroupcostById(id);
                model.Id = groupCost.Id;
                model.Code = string.IsNullOrEmpty(groupCost.Code) ? string.Empty : groupCost.Code;  
                model.Name = string.IsNullOrEmpty(groupCost.Name) ? string.Empty : groupCost.Name;  
                model.Description = string.IsNullOrEmpty(groupCost.Description) ? string.Empty : groupCost.Description;  
            }
            return PartialView("_Partials/_GroupCostModify", model);
        }
        [HttpPost]
        public ActionResult ModifyGroupCost(PartialGroupCostViewModify model)
        {
            #region [VALIDATION]
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            #endregion

            try
            {
                _repoCosts.Modify(model);
                var result = new AjaxResponseViewModel()
                {
                    result = true,
                    message = Messege.SUCCESS
                };
                TempData[UtilConstants.NotifyMessageName] = result;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            { 
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/ModifyGroupCost", ex.Message);

                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult DeleteGroupCost(int? id)
        {
            try
            {
                var model = _repoCosts.GetGroupcostById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/DeleteGroupCost", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.NO_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }

                var name = model.Code + "-" + model.Name;
                if (model.CAT_COSTS.Any())
                {
                    var costName = string.Empty;
                    foreach (var cat in model.CAT_COSTS)
                    {
                        if (cat.Course_Cost.Any())
                        {
                            costName = cat.str_Code + "-" + cat.str_Name;
                            break;
                        }
                    }
                    var fullName = name + "-> " + costName;
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, fullName),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
              
                model.IsDeleted = true;
                model.IsActive = false;
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = CurrentUser.USER_ID;

                _repoCosts.Update(model);
                
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Cost/DeleteGroupCost", ex.Message);
                return Json(new
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult filterGroupCost()
        {
            var html = new StringBuilder();
            var groupCost = _repoCosts.GetGroupCost().OrderBy(a => a.Name);
            if (!groupCost.Any())
                return Json(new
                {
                    value_option = html.ToString()
                }, JsonRequestBehavior.AllowGet);
            html.Append("<option></option>");
            foreach (var item in groupCost)
            {
                html.AppendFormat("<option   value='{0}'>{1} - {2}</option>", item.Id, item.Code, item.Name);
            }
            return Json(new
            {
                value_option = html.ToString()
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
