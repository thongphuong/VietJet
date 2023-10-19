using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Core.Services.Approves;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Department;
using TMS.Core.Services.Employee;
using TMS.Core.Services.GroupUser;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Users;
using TMS.Core.Utils;
using TMS.Core.ViewModels;

namespace TrainingCenter.Controllers
{
    using TMS.Core.ViewModels.GroupUserModels;
    using TMS.Core.ViewModels.Departments;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.Courses;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.Services;

    public class GroupUserController : BaseAdminController
    {
        private readonly IGroupUserService _groupUserService;
        public GroupUserController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IGroupUserService groupUserService,IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService,  approveService)
        {
            _groupUserService = groupUserService;
        }

        // GET: GroupUser
        public ActionResult Index()
        {
            ViewBag.Namesite = GetByKey("Namesite");
            return View();
        }

        //View Modify
        public ActionResult Modify(int? id)
        {
            var groupUser = new GroupUserModel();
            if (!id.HasValue)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                {
                    message = Resource.INVALIDURL
                };
                return RedirectToAction("Index");
            }
            var entity = _groupUserService.GetById(id);
            if (entity == null)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                {
                    message = string.Format(Messege.DATA_ISNOTFOUND,Resource.lblGroupUser , "<font color='red' >" + id.Value + "</font>")
                };
                return RedirectToAction("Index");
            }
            groupUser.Id = entity.Id;
            groupUser.Name = entity.Name;
            groupUser.Title = entity.Title;

            return View(groupUser);
        }

        [HttpPost]
        public ActionResult Modify(GroupUserModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Title))
                    {
                        _groupUserService.Modify(model);
                        TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel(){ result = true, message = Messege.SUCCESS};
                    }
                   
                    return RedirectToAction("Index");
                }
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = MessageInvalidData(ModelState) };
                return View(model);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/Modify", ex.Message);
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel() { result = false, message = ex.Message };
                return View(model);
            }
        }

        public ActionResult AjaxHandlerMenu(jQueryDataTableParamModel param, int id = 0)
        {
            try
            {
                var data = ConfigService.GetMenu(a => a.PARENT_ID == null);

                var result = from c in data.ToArray()
                             let permissionMenu = c.GroupFunctionId.HasValue ? c.GroupFunction.GroupPermissionFunctions.Where(a => a.Function.ActionType.HasValue).Select(a => (int)a.Function.ActionType).Distinct().ToArray() : null
                             select new object[] {
                            string.Empty,
                            c.GroupFunctionId == null ?string.Format("<a href='javascript:void(0)' data-id ='{0}' onclick='groupuser.showSubItem(this)' >{1}</a> ",c.ID,c.TITLE) : c.TITLE,
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.FullOption,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.View,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.CreateEdit,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.Delete,permissionMenu),
                            c.GroupFunctionId == null ? "" : ReturnColumnCheck(c.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.Export,permissionMenu),

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/AjaxHandlerMenu", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        //Load SubMenu
        public ActionResult AjaxHandlerSubMenu(jQueryDataTableParamModel param, int menuId, int id = 0)
        {

            var data = ConfigService.GetMenuById(menuId);
            var display = from a in data.MENU1.Where(b => b.ISACTIVE == 1).OrderBy(b => b.SHOWORDER).ToArray()
                          let permissionMenu = a.GroupFunctionId.HasValue ? a.GroupFunction.GroupPermissionFunctions.Where(b => b.Function.ActionType.HasValue).Select(b => (int)b.Function.ActionType).Distinct().ToArray() : null
                          select new object[]
                            {
                                  string.Empty, a.MENU1.Any() ? string.Format("<a href='javascript:void(0)' data-id ='{0}' onclick='groupuser.showSubItem(this)' >{1}</a> ",a.ID,a.TITLE) : a.TITLE ,
                                    a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.FullOption,permissionMenu),
                                    a.GroupFunctionId == null ? "" :ReturnColumnCheck(a.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.View,permissionMenu),
                                    a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.CreateEdit,permissionMenu),
                                    a.GroupFunctionId == null ? "" :
                                  ReturnColumnCheck(a.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.Delete,permissionMenu),
                                    a.GroupFunctionId == null ? "" : ReturnColumnCheck(a.GroupFunction,id,(int)UtilConstants.ROLE_FUNCTION.Export,permissionMenu)
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

        private string ReturnColumnCheck(GroupFunction menu, int? idGroupUser, int? type, int[] allPermission)
        {
            if (type == (int)UtilConstants.ROLE_FUNCTION.FullOption && menu.GroupPermissionFunctions.Any())
            {
                var menuPermission = menu.GroupUserMenus.Where(a => a.GroupUserId == idGroupUser && a.IsActive == true).Select(a => a.FunctionType);

                var icon = allPermission.All(a => menuPermission.Any(x => x == a)) ? "far fa-check-square" : "far fa-square";
                var isCheck = allPermission.All(a => menuPermission.Any(x => x == a)) ? 0 : 1;

                var _return = "<i class='" + icon + " byhoa' aria-hidden='true' onclick='groupuser.updateGroupUser(" + idGroupUser + "," + menu.Id + "," + type + "," + isCheck + ",this)'></i>";
                return _return;
            }
            if (menu.GroupPermissionFunctions.Any(a => a.Function.ActionType == type))
            {
                var permission = menu.GroupUserMenus.FirstOrDefault(a => a.GroupUserId == idGroupUser && a.FunctionType == type);

                var icon = permission?.IsActive == true ? "far fa-check-square" : "far fa-1x fa-square";
                var isCheck = permission?.IsActive == true ? 0 : 1;
                var _return = "<i class='" + icon + " byhoa' aria-hidden='true' onclick='groupuser.updateGroupUser(" + idGroupUser + "," + menu.Id + "," + type + "," + isCheck + ",this)'></i>";
                return _return;
            }
            return "";
        }

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var name = string.IsNullOrEmpty(Request.QueryString["Name"]) ? "" : Request.QueryString["Name"].Trim();
                var title = string.IsNullOrEmpty(Request.QueryString["Title"]) ? "" : Request.QueryString["Title"].Trim();

                var model = _groupUserService.Get(a => a.Name.Contains(name)
                      && (string.IsNullOrEmpty(title) || a.Title.Contains(title))
                );


                IEnumerable<GroupUser> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);

                Func<GroupUser, object> orderingFunction = (g => sortColumnIndex == 1 ? g.Name
                                                                : sortColumnIndex == 2 ? g.Title 
                                                                : sortColumnIndex == 3 ? g.IsActive
                                                                : (object)g.Name);



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
                             select new[] {
                                string.Empty,
                         "<a href='"+@Url.Action("Modify", new {id = c.Id})+"'>"+c.Name+" </a>",
                         c.Title,
                         c.IsActive==false ? "&nbsp;<i class='fa fa-toggle-off' onclick='Set_Participate_GroupUser(0,"+c.Id+")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='Set_Participate_GroupUser(1,"+c.Id+")' aria-hidden='true'  title='Active' style='cursor: pointer;' style='font-size:16px;'></i>",
                         "<a href='"+@Url.Action("Modify",new{id = c.Id})+"' data-toggle='tooltip' title='Edit'><i class='fas fa-edit btnIcon_green font-byhoa' aria-hidden='true' ></i></a>" +
                         ((User.IsInRole("/GroupUser/Delete")) ? verticalBar + "<a title='Delete'  href='javascript:void(0)' onclick='calldelete(" + c.Id  + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>" : "")

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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/AjaxHandler", ex.Message);
                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = 0,
                    iTotalDisplayRecords = 0,
                },
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Delete(int id)
        {
            try
            {
                var guser = _groupUserService.GetById(id);
                if (guser == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                if (guser.GroupUserAccesses.Any(a => a.USER.IsDeleted==false))
                {
                    return Json(new AjaxResponseViewModel()
                    {
                        message = string.Format(Messege.DELETED_UNSUCCESS_AVAILABLE, guser.Name),
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                guser.IsDeleted = true;
                _groupUserService.Update(guser);
                return Json(new AjaxResponseViewModel
                {
                    message = Messege.SUCCESS,
                    result = true
                }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/Delete", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public ActionResult ClickOption(int idGroupUser, int idMenu, int type, int onoff)
        {
            try
            {
                _groupUserService.Update(idGroupUser, idMenu, type, onoff);
                return Json(new { code = "Success" });
            }
            catch
            {
                return Json(new { ResultExecutedContext = "An error occur" });
            }
        }

        public ActionResult UserModifyPermission(int? id)
        {
            var listPermissions = GetDepartmentModel(CurrentUser.IsMaster);
            var groupUser = id.HasValue ? _groupUserService.GetById(id.Value) : null;
            var model = new GroupUserPermissionViewModel()
            {
                DepartmentViewModels = listPermissions,
                IsMaster = true,
                GroupUserId = groupUser?.Id,
                GroupUserPermissionIds = groupUser?.GroupUserPermissions.Select(a => (int)a.DepartmentId).ToList() ?? new List<int>()

            };
            return PartialView("_Partials/_UserModifyPermission", model);
        }


        [HttpPost]
        public ActionResult GrantGroupUserPermission(int id, int groupUserId)
        {
            try
            {
                if (CurrentUser.IsMaster)
                {
                    _groupUserService.GrantPermission(groupUserId, id);
                    return Json(new AjaxResponseViewModel { result = true, message = "Grant permission to user successfully !" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel { result = false, message = "You don't have role to mange this permission" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/GrantGroupUserPermission", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GrantGroupUserPermissionAll(int groupUserId, bool typecheck)
        {
            try
            {
                if (CurrentUser.IsMaster)
                {
                    var listPermissions = GetParentDepartmentModel(CurrentUser.IsMaster);
                    _groupUserService.GrantPermissionAll(groupUserId, listPermissions.Select(a => a.Id).ToArray(), typecheck);
                    return Json(new AjaxResponseViewModel { result = true, message = "Grant permission to user successfully !" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new AjaxResponseViewModel { result = false, message = "You don't have role to mange this permission" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/GrantGroupUserPeGrantGroupUserPermissionAllrmission", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CreateGroupUser()
        {
            return PartialView("_Partials/_CreateGroupUser");
        }
        [HttpPost]
        public ActionResult CreateGroupUser(GroupUserModel model)
        {
            if (!ModelState.IsValid)
            {
                //return Json(new AjaxResponseViewModel { result = false, message = Resource.INVALIDDATA });
                return Json(new AjaxResponseViewModel()
                {
                    message = Messege.FAIL + "<br />" + MessageInvalidData(ModelState),
                    result = false
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var id = _groupUserService.Insert(model);
                return Json(new AjaxResponseViewModel { result = true, data = id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/CreateGroupUser", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = ex.Message },JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult SubmitSetParticipateGroupUser(int isParticipate, string id, FormCollection form)
        {
            try
            {
                int idGroupUser = int.Parse(id);
                var removeGroupUser = _groupUserService.GetById(idGroupUser);
                if (removeGroupUser == null)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/SubmitSetParticipateGroupUser", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel()
                    {
                        message = Messege.ISVALID_DATA,
                        result = false
                    }, JsonRequestBehavior.AllowGet);
                }
                    
                if (isParticipate == 1)
                {
                    removeGroupUser.IsActive = false;
                }
                else
                {
                    removeGroupUser.IsActive = true;
                }
                _groupUserService.Update(removeGroupUser);
                return Json(new AjaxResponseViewModel
                {
                    message = string.Format(Messege.SET_STATUS_SUCCESS, removeGroupUser.Name),
                    result = true
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "GroupUser/SubmitSetParticipateGroupUser", ex.Message);
                return Json(new AjaxResponseViewModel
                {
                    message = ex.Message,
                    result = false
                }, JsonRequestBehavior.AllowGet);

            }
            
        }
    }
}