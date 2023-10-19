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
using TMS.Core.ViewModels.ViewModel.RoleMenus;

namespace TrainingCenter.Controllers
{
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels;
    using TMS.Core.App_GlobalResources;

    public class RoleController : BaseAdminController
    {
        #region Init

        private readonly IRoleService _repoRole;
        private const string RoleCheckError = "Supper Admin";
        #endregion

        #region Role - Index

        public RoleController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IRoleService repoRole,IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoRole = repoRole;
        }

        public ActionResult Index()
        {
            if (!Is_View())
            {
                return RedirectToAction("Index", "Redirect");
            }

            //var roles = _repoRole.Get(a => a.NAME.ToLower().Trim() != RoleCheckError.ToLower().Trim()).ToList();
            var model = new RoleModel();
          
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {

            var name = form["NAME"].Trim();
            var check = _repoRole.Get();

            if (check.Any(a => a.NAME == name))
            {
                ViewBag.Error = "ten da ton tai";
                var roles = _repoRole.Get();
                ViewBag.Duprole = name;
                return View("Index", roles);
            }
            var role = new ROLE();
            role.NAME = name;
            role.CREATED_BY = GetUser().USER_ID;
            role.CREATION_DATE = DateTime.Now;
            role.LAST_UPDATED_BY = role.CREATED_BY;
            role.LAST_UPDATE_DATE = role.CREATION_DATE;
            role.IsActive = true;
            role.IsDeleted = false;
            _repoRole.Insert(role);
            return RedirectToAction("Modify", new { roleId = role.ID });
        }
        #endregion
        #region [Create Role]
        [HttpPost]
        public ActionResult Create(RoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    result = false,
                    message = MessageInvalidData(ModelState)
                   
                }, JsonRequestBehavior.AllowGet);

            }
            try
            {
                
                var entity = _repoRole.Get(a => a.NAME.ToLower().Trim() == model.Name.ToLower());
                if (entity.Any())
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        result = false,
                        message = Messege.NAME_EXIST
                    },JsonRequestBehavior.AllowGet);
                }
                var role = new ROLE
                {
                    NAME = model.Name,
                    DESCRIPTION = model.Description,
                    CREATED_BY = GetUser().USER_ID,
                    CREATION_DATE = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };
                _repoRole.Insert(role);
                return Json(new AjaxResponseViewModel()
                {
                    result = true,
                    message = string.Format(Messege.CREATE_SUCCESSFULLY, model.Name)
                },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/Create", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = string.Format(Messege.CREATE_UNSUCCESSFULLY, model.Name)
                }, JsonRequestBehavior.AllowGet);
            }
           
        }
            
        #endregion

        public ActionResult AjaxHandlerRole(jQueryDataTableParamModel param)
        {
            try
            {
                var name = string.IsNullOrEmpty(Request.QueryString["rName"]) ? string.Empty : Request.QueryString["rName"].Trim();

                var isMaster = CurrentUser.IsMaster ? string.Empty : RoleCheckError;

                IEnumerable<ROLE> filtered = _repoRole.GetAjaxHandler(a => a.NAME.ToLower().Trim() != isMaster.ToLower().Trim() && (a.NAME.Contains(name) || a.DESCRIPTION.Contains(name)));
              
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<ROLE, object> orderingFunction = (a =>
                 a.ID);
                var sortDirection = Request["sSortDir_0"]; // asc or desc

                if (sortDirection == null)
                {
                    sortDirection = "asc";
                }
                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);
                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var verticalBar = GetByKey("VerticalBar");
                var result = from c in displayed.ToArray()
                    select new object[]
                    {
                        string.Empty,
                       // c.ID,
                        c.NAME ?? string.Empty,
                        c.DESCRIPTION ?? string.Empty,
                         !c.NAME.Equals(RoleCheckError) ? (c.IsActive==false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_Role(0,"+c.ID+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_Role(1,"+c.ID+")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>") : string.Empty,
                        ((User.IsInRole("/Role/Modify")) ? "<a title='Edit' href='"+@Url.Action("Modify",new{id = c.ID})+"')' data-toggle='tooltip'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" : "")+ 
                        ((User.IsInRole("/Role/Delete") && !c.NAME.Equals(RoleCheckError)) ? verticalBar  +"<a title='Delete' href='javascript:void(0)' onclick='calldelete(" + c.ID  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/AjaxHandlerRole", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        #region Role-Modify
        public ActionResult Modify(int? id)
        {
            var model = new RoleModel();
            if (id.HasValue)
            {
                var role = _repoRole.GetById(id);
                var session = GetSession();
                if (session.Username != "administrator" && role.ID == 1)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/Modify", Messege.ISVALID_DATA);
                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.ISVALID_DATA)
                    };
                    return RedirectToAction("Index");
                }
            
                model.Id = role.ID;
                model.Name = role.NAME;
                model.Description = role.DESCRIPTION;
            }
                return View(model);

        }
        [HttpPost]
        public ActionResult Modify(RoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new AjaxResponseViewModel
                {
                    result = false,
                    message = MessageInvalidData(ModelState)

                }, JsonRequestBehavior.AllowGet);

            }
            try
            {
                var entity = new ROLE();
                if (model.Id != 0)
                {
                    var check = _repoRole.Get(a => a.NAME.ToLower().Trim() == model.Name.ToLower() && a.ID != model.Id);
                    if (check.Any())
                    {
                        return Json(new AjaxResponseViewModel()
                        {
                            result = false,
                            message = Messege.NAME_EXIST
                        }, JsonRequestBehavior.AllowGet);
                    }
                     entity = _repoRole.GetById(model.Id);
                    if (entity == null)
                    {
                        return Json(new AjaxResponseViewModel()
                        {
                            result = false,
                            message = Messege.NO_DATA
                        }, JsonRequestBehavior.AllowGet);
                    }
                    entity.LAST_UPDATED_BY = GetUser().USER_ID;
                    entity.LAST_UPDATE_DATE = DateTime.Now;
                }
                entity.NAME = model.Name;
                entity.DESCRIPTION = model.Description;
               
               
                _repoRole.Update(entity,UtilConstants.LogEvent.Update);
                return Json(new AjaxResponseViewModel()
                {
                    result = true,
                    message = string.Format(Messege.UPDATE_SUCCESSFULLY, model.Name)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/Modify", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = string.Format(Messege.UPDATE_UNSUCCESSFULLY, model.Name)//ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        [AllowAnonymous]
        public ActionResult MenuListView(int? id)
        {
            var ss = ConfigService.GetMenu();
            var menu = from a in ss.ToArray()
                let permissionMenu = a.GroupFunctionId.HasValue
                    ? a.GroupFunction.GroupPermissionFunctions.Where(x => x.Function.ActionType.HasValue)
                        .Select(x => (int) x.Function.ActionType).Distinct().ToArray()
                    : null
                select new MenuViewModel()
                {
                    Id = a.ID,
                    ParentId = a.PARENT_ID,
                    IsMenu = a.ISMENU > 0,
                    MenuIndex = a.SHOWORDER,
                    MenuTitle = a.TITLE,
                    Icon = a.ICON,
                    Url = a.URL,
                    Function = a.GroupFunctionId,
                    Ancestor = a.Ancestor,
                    Functionlist = a.GroupFunction,
                    Checkbox = 
                            (a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction, id, UtilConstants.ROLE_FUNCTION.FullOption, permissionMenu)) +
                            ( a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction, id,UtilConstants.ROLE_FUNCTION.View, permissionMenu)) + 
                            (a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction, id, UtilConstants.ROLE_FUNCTION.CreateEdit, permissionMenu)) + 
                            (a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction, id, UtilConstants.ROLE_FUNCTION.Delete, permissionMenu)) + 
                            (a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction, id, UtilConstants.ROLE_FUNCTION.Export, permissionMenu))
                };
            return PartialView("_Partials/_MenuListView", menu);
        }
        public ActionResult AjaxHandler(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var data = ConfigService.GetMenu(a => a.PARENT_ID == null);
                
                var result = from c in data.ToArray()
                          let permissionMenu = c.GroupFunctionId.HasValue ? c.GroupFunction.GroupPermissionFunctions.Where(a => a.Function.ActionType.HasValue).Select(a => (int)a.Function.ActionType).Distinct().ToArray() : null
                             select new object[] {
                            string.Empty,
                            c.GroupFunctionId == null ?string.Format("<a href='javascript:void(0)' data-id ='{0}' onclick='role.showSubItem(this)' >{1}</a> ",c.ID,c.TITLE) : c.TITLE,
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.FullOption,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.View,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.CreateEdit,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.Delete,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.Export,permissionMenu),

                        };

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = data.Count(),
                    iTotalDisplayRecords = data.Count(),
                    aaData = result
                },
             JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AjaxHandlerSubmenu(jQueryDataTableParamModel param, int menuId, int id = 0)
        {
            var data = ConfigService.GetMenuById(menuId);
            var display = from c in data.MENU1.Where(a=>a.ISACTIVE == 1).OrderBy(a=>a.SHOWORDER).ToArray()
                          let permissionMenu = c.GroupFunctionId.HasValue ? c.GroupFunction.GroupPermissionFunctions.Where(a => a.Function.ActionType.HasValue).Select(a => (int)a.Function.ActionType).Distinct().ToArray() : null
                          select new object[] {
                            string.Empty,
                            c.MENU1.Any() ? string.Format("<a href='javascript:void(0)' data-id ='{0}' onclick='menu.showSubItem(this)' >{1}</a> ",c.ID,c.TITLE) : c.TITLE,
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.FullOption,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.View,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.CreateEdit,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.Delete,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,UtilConstants.ROLE_FUNCTION.Export,permissionMenu),
                            //ReturnColumnCheck(c.ID,idrole,(int)UtilConstants.ROLE_FUNCTION.Active_Deactive)
                    };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = 0,
                iTotalDisplayRecords = 0,
                    aaData = display
            },
                    JsonRequestBehavior.AllowGet);
        }

        private string ReturnColumnCheck(GroupFunction menu, int? idRole, UtilConstants.ROLE_FUNCTION? type,int[] allPermission)
        {
            if(type == (int)UtilConstants.ROLE_FUNCTION.FullOption && menu.GroupPermissionFunctions.Any())
            {
                var rolePermission = menu.ROLEMENUs.Where(a => a.ROLE_ID == idRole && a.IsActive == true).Select(a=>a.MenuType);
                var icon = allPermission.All(a=> rolePermission.Any(x=> x == a)) ? "far fa-check-square" : "far fa-square";
                var isCheck = allPermission.All(a => rolePermission.Any(x => x == a))  ? 0 : 1;

                var _return = " <span class='action'><i class='" + icon + " byhoa' aria-hidden='true' onclick='role.updateRole(" + idRole + "," + menu.Id + "," + (int)type + "," + isCheck + ",this)'></i></span> ";
                return _return + ((int)type == (int)UtilConstants.ROLE_FUNCTION.CreateEdit ? "Create/Edit" : type.ToString());
            }
            if (menu.GroupPermissionFunctions.Any(a => type != null && a.Function.ActionType == (int) type))
            {
                var permission = menu.ROLEMENUs.FirstOrDefault(a => type != null && (a.ROLE_ID == idRole && a.MenuType == (int) type));
                var icon = permission?.IsActive == true ? "far fa-check-square" : "far fa-1x fa-square";
                var isCheck = permission?.IsActive == true ? 0 : 1;
                var _return = "<span class='action'><i class='" + icon + " byhoa' aria-hidden='true' onclick='role.updateRole(" + idRole + "," + menu.Id + "," + (int)type + "," + isCheck + ",this)'></i></span> ";
                return _return + ((int)type == (int)UtilConstants.ROLE_FUNCTION.CreateEdit ? "Create/Edit" : type.ToString());
            }
            return "";
        }

        
        public JsonResult click_option(int idrole, int idpage, int optiontype, int onoff)
        {
            try
            {
                _repoRole.Update(idrole, idpage, optiontype, onoff);
                return Json(new AjaxResponseViewModel()
                {
                    result =  true,
                    message = Messege.SUCCESS
                },JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/click_option", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    result = false,
                    message = Messege.UNSUCCESS
                }, JsonRequestBehavior.AllowGet);
                throw;
            }
        }

        [HttpPost]
        public ActionResult Delete(int id = -1)
        {
            try
            {
                var model = _repoRole.GetById(id);
                if (model == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/Delete", Messege.NO_DATA);
                    return Json(new AjaxResponseViewModel
                    {
                        message = Messege.NO_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                if (model.ROLEMENUs.Any(a => a.IsActive == true))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, model.NAME),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                model.IsActive = false;
                model.IsDeleted = true;
                model.LAST_UPDATE_DATE = DateTime.Now;
                model.LAST_UPDATED_BY = CurrentUser.USER_ID;
                _repoRole.Update(model, UtilConstants.LogEvent.Delete);

                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.DELETE_SUCCESSFULLY, model.NAME),
                    result = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/Delete", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public ActionResult SetParticipateRole(int isParticipate, string id)
        {
            int roleId = int.Parse(id);
            var removeRole = _repoRole.GetById(roleId);
            if (removeRole == null) {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "Role/Delete", Messege.NO_DATA);
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.NO_DATA,
                    result = false

                }, JsonRequestBehavior.AllowGet);
            }
                
            removeRole.IsActive = isParticipate != 1;
            _repoRole.Update(removeRole, UtilConstants.LogEvent.Active);

            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, removeRole.NAME), result = true }, JsonRequestBehavior.AllowGet);

        }
    }
}
