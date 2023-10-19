using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrainingCenter.Utilities;
using DAL.Entities;
using DocumentFormat.OpenXml.Wordprocessing;
using TMS.Core.Services.Configs;
using TMS.Core.Services.CourseDetails;
using TMS.Core.Services.CourseMember;
using TMS.Core.Services.Employee;
using TMS.Core.Services.Notifications;
using TMS.Core.Services.Roles;
using TMS.Core.Services.Companies;
using TMS.Core.Services.Users;
using TMS.Core.App_GlobalResources;
using TMS.Core.Services.Approves;
using TMS.Core.ViewModels;
using TMS.Core.ViewModels.AjaxModels;
using TMS.Core.ViewModels.UserModels;
using TMS.Core.ViewModels.ViewModel;


namespace TrainingCenter.Controllers
{
    using Newtonsoft.Json.Linq;
    using global::Utilities;
    using System.Configuration;
    using System.Web.Script.Serialization;
    using TMS.Core.Services;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.Department;
    using TMS.Core.Services.GroupUser;
    using TMS.Core.Utils;
    using TMS.Core.ViewModels.Common;
    using TMS.Core.ViewModels.Departments;

    //Tin's
    using TMS.Core.ViewModels.UserModels;

    public class UserController : BaseAdminController
    {
        #region MyRegion

        private readonly IUserService _repoUser;
        private readonly IRoleService _repoRole;
        private readonly IGroupUserService _repoGroupUser;
        private readonly IConfigService _configService;
        public UserController(IConfigService configService, IUserContext userContext, INotificationService notificationService, ICourseMemberService courseMemberService, IEmployeeService employeeService, ICourseDetailService courseDetailService, IDepartmentService departmentService, ICourseService courseService, IUserService repoUser, IRoleService repoRole, IGroupUserService repoGroupUser, IApproveService approveService) : base(configService, userContext, notificationService, courseMemberService, employeeService, courseDetailService, departmentService, courseService, approveService)
        {
            _repoUser = repoUser;
            _repoRole = repoRole;
            _repoGroupUser = repoGroupUser;
            _configService = configService;
        }

        #endregion
        //
        // GET: /Admin/User/
        #region Index
        // return view
        public ActionResult Index(int id = -1)
        {
            if (!Is_View())
            {
                return RedirectToAction("Index", "Redirect");
            }
            //1 : Supper Admin
            ViewBag.RoleList = new SelectList(_repoRole.Get()/*.Where(a => a.ID != 1)*/.OrderBy(m => m.NAME), "ID", "NAME");
            return View();
        }

        // fill data to datatable by ajax 

        public ActionResult AjaxHandler(jQueryDataTableParamModel param)
        {
            try
            {
                var fUsername = string.IsNullOrEmpty(Request.QueryString["fUsername"]) ? string.Empty : Request.QueryString["fUsername"].Trim();
                var fFirstname = string.IsNullOrEmpty(Request.QueryString["fFirstname"]) ? string.Empty : Request.QueryString["fFirstname"].Trim();
                var fLastname = string.IsNullOrEmpty(Request.QueryString["fLastname"]) ? string.Empty : Request.QueryString["fLastname"].Trim();
                //var fFullname = string.IsNullOrEmpty(Request.QueryString["fFullname"]) ? string.Empty : Request.QueryString["fFullname"].Trim();
                var fEmail = string.IsNullOrEmpty(Request.QueryString["fEmail"]) ? string.Empty : Request.QueryString["fEmail"].Trim();
                var fPhone = string.IsNullOrEmpty(Request.QueryString["fPhone"]) ? string.Empty : Request.QueryString["fPhone"].Trim();
                // var url = string.IsNullOrEmpty(Request.QueryString["url"]) ? string.Empty : Request.QueryString["url"].ToLower().Trim();
                var fRole = string.IsNullOrEmpty(Request.QueryString["fRole"]) ? -1 : Convert.ToInt32(Request.QueryString["fRole"].Trim());
                var isMaster = CurrentUser.IsMaster ? string.Empty : "Administrator";

                var model = _repoUser.GetAll(a =>
                     (string.IsNullOrEmpty(fUsername) || !a.USERNAME.Equals(isMaster) && a.USERNAME.Trim().ToLower().Contains(fUsername.Trim().ToLower()))
                    //&& (string.IsNullOrEmpty(fFullname) || (a.FIRSTNAME.Trim() + " " + a.LASTNAME.Trim()).ToLower().Contains(fFullname.Trim().ToLower()))
                    && (string.IsNullOrEmpty(fFirstname) || (a.FIRSTNAME.Trim()).ToLower().Contains(fFirstname.Trim().ToLower()))
                    && (string.IsNullOrEmpty(fLastname) || (a.LASTNAME.Trim()).ToLower().Contains(fLastname.Trim().ToLower()))
                    && (string.IsNullOrEmpty(fEmail) || a.EMAIL.Trim().ToLower().Contains(fEmail.Trim().ToLower()))
                    && (string.IsNullOrEmpty(fPhone) || a.PHONENO.Trim().ToLower().Contains(fPhone.Trim().ToLower()))
                    && (fRole == -1 || a.UserRoles.Any(x => x.RoleId == fRole))
                    && a.IsDeleted == false /*&& a.ISACTIVE==1*/, CurrentUser.PermissionIds, CurrentUser.IsMaster).ToArray();

                var verticalBar = GetByKey("VerticalBar");
                //var filtered = model.Select(a => new AjaxUser
                //{
                //    UserName = a.USERNAME.Trim(),
                //    FullName = ReturnDisplayLanguage(a.FIRSTNAME, a.LASTNAME),
                //    //UserState = a.USERSTATE == (int)UtilConstants.UserStateConstant.Online ? UtilConstants.UserStateConstant.Online.ToString() : UtilConstants.UserStateConstant.Offline.ToString(),
                //    LastOnline = a.LASTONLINEAT.ToString(),
                //    //Status = (a.ISACTIVE == 0 ? OnOffTextOption("User","Index",a.ID,Resource.INACTIVE, "fa fa-toggle-off","", "onclick='SetStatusUser(0," + a.ID + ")") : OnOffTextOption("User", "Index", a.ID, Resource.ACTIVE, "fa fa-toggle-on", "", "onclick='SetStatusUser(1," + a.ID + ")")),
                //    //Option = (a.ISACTIVE == 0 ? OnOffTextOption("User","Index",a.ID,Resource.lblEdit, "fa fa-pencil-square-o", @Url.Action("Modify", new {a.ID }),"") : string.Empty)
                //    //         + "|" +
                //    //         OnOffTextOption("User", "Index", a.ID, Resource.lblDelete, "fa fa-trash-o", "", " onclick='calldelete(" + a.ID + ")"),
                //    Status = (!a.USERNAME.ToLower().Equals("administrator") ? (a.ISACTIVE == 0 ? "&nbsp;<i class='fa fa-toggle-off' onclick='SetStatusUser(0," + a.ID + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='SetStatusUser(1," + a.ID + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>") : string.Empty),
                //    Option = ("<a href='" + @Url.Action("Modify", new { id = a.ID }) + "' data-toggle='tooltip'  title='Edit'><i class='fa fa-pencil-square-o btnIcon_green' style='font-size: 16px; '></i></a>") +
                //          (!a.USERNAME.ToLower().Equals("administrator") ? (verticalBar + "<a title='Delete'  href='javascript:void(0)' onclick='calldelete(" + a.ID + ")' data-toggle='tooltip'><i class='fa fa-trash-o btnIcon_red' aria-hidden='true' style=' font-size: 16px; '></i></a>") : string.Empty)
                //});
                IEnumerable<USER> filtered = model;
                var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
                Func<USER, object> orderingFunction = (s => sortColumnIndex == 1 ? s.USERNAME
                                                          : sortColumnIndex == 2 ? s.FIRSTNAME
                                                          : sortColumnIndex == 3 ? s.LASTNAME
                                                          : sortColumnIndex == 4 ? s.UserRoles.Any() ? s?.UserRoles?.FirstOrDefault()?.ROLE?.NAME : ""
                                                          : sortColumnIndex == 5 ? s.LASTONLINEAT
                                                           : sortColumnIndex == 6 ? s.ISACTIVE
                                                          : (object)s.USERNAME);

                var sortDirection = Request["sSortDir_0"] ?? "asc"; // asc or desc

                filtered = (sortDirection == "asc") ? filtered.OrderBy(orderingFunction)
                                  : filtered.OrderByDescending(orderingFunction);

                var displayed = filtered.Skip(param.iDisplayStart).Take(param.iDisplayLength);
                var result = from c in displayed.ToArray()
                             select new object[] {
                           string.Empty,
                           c?.USERNAME,
                           //ReturnDisplayLanguage(c?.FIRSTNAME,c?.LASTNAME),
                           c?.FIRSTNAME,
                           c?.LASTNAME,
                           c.UserRoles.Any() ? c?.UserRoles.FirstOrDefault().ROLE.NAME : "",
                           c.LASTONLINEAT != null ? c.LASTONLINEAT.Value.ToString("dd/MM/yyyy hh:mm:ss") : string.Empty,
                           ((bool) !c?.USERNAME.ToLower().Equals("administrator") ? (c?.ISACTIVE == 0 ? "&nbsp;<i class='fa fa-toggle-off' onclick='SetStatusUser(0," + c?.ID + ")' aria-hidden='true' title='Inactive' style='cursor: pointer;'></i>" : "&nbsp;<i class='fa fa-toggle-on'  onclick='SetStatusUser(1," + c?.ID + ")' aria-hidden='true' title='Active' style='cursor: pointer;'></i>") : string.Empty),
                           ("<a href='" + @Url.Action("Modify", new { id = c?.ID }) + "' data-toggle='tooltip'  title='Edit'><i class='fas fa-edit btnIcon_green font-byhoa' ></i></a>") +
                          ((bool) !c?.USERNAME.ToLower().Equals("administrator") ? (verticalBar + "<a title='Delete'  href='javascript:void(0)' onclick='calldelete(" + c?.ID + ")' data-toggle='tooltip'><i class='fas fa-trash btnIcon_red font-byhoa' aria-hidden='true' ></i></a>") : string.Empty)
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
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/AjaxHandler", ex.Message);
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
        public ActionResult Modify(int? id)
        {
            var checkPERMISSION_DATA = _configService.GetByKey("PERMISSION_DATA");
            var checkGUser = _configService.GetByKey("GroupUser");
            var checkRoleSupper = GetByKey("RoleSupperAdmin");
            var currentUser = GetUser();
            var user = new UserProfile()
            {
                Departments = GetDepartmentDictionary(null, null, null, 0, currentUser.PermissionIds),
                GroupUsers = _repoGroupUser.Get(a => a.IsActive == true).ToDictionary(a => a.Id, a => a.Name),
                Roles = CurrentUser.IsMaster ? _repoRole.Get().ToDictionary(a => a.ID, a => a.NAME) : _repoRole.Get(a => a.NAME.ToLower().Trim() != checkRoleSupper).ToDictionary(a => a.ID, a => a.NAME),
                //Instructors = EmployeeService.GetInstructors().OrderBy(a => a.FirstName).Where(a => !a.USERs.Any() && currentUser.PermissionIds.Any(x => x == a.Department_Id)).ToDictionary(a => a.Id, a => string.Format("{0} - {1} {2}", a.str_Staff_Id, a.FirstName, a.LastName)),
                Instructors = EmployeeService.GetInstructors(true).OrderBy(a => a.FirstName).Where(a => !a.USERs.Any()).ToDictionary(a => a.Id, a => string.Format("{0} - {1}", a.str_Staff_Id, ReturnDisplayLanguage(a.FirstName, a.LastName))),
                Role = new List<int>(),
                GroupUser = new List<int>(),
                Checksitepermissiondata = checkPERMISSION_DATA != null,
                CheckGroupUser = checkGUser != null
            };
            if (id.HasValue)
            {
                var session = GetSession();
                var entity = _repoUser.GetById(id);
                if (session.Username != "administrator" && entity.USERNAME == "administrator")
                {
                    TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel { result = false, message = Messege.ISVALID_DATA };
                    return RedirectToAction("Index");
                }
                user.Id = entity.ID;
                user.Address = entity.ADDRESS;
                user.Department = entity.DepartmentId;
                user.GroupUser = entity.GroupUserAccesses.Select(a => a.GroupUserId ?? 0).ToList();
                user.Email = entity.EMAIL;
                //user.FullName = ReturnDisplayLanguage(entity.FIRSTNAME, entity.LASTNAME);
                user.FirstName = entity.FIRSTNAME;
                user.LastName = entity.LASTNAME;
                user.Numbers = entity.PHONENO;
                user.UserName = entity.USERNAME;
                user.Role = entity.UserRoles.Select(a => (int)a.RoleId).ToList();
                user.InstructorId = entity.InstructorId;
                user.nameImage = entity.Avatar;

            }
            if (user.InstructorId.HasValue)
            {
                user.Instructors.Add((int)user.InstructorId, string.Format("{0} - {1}", user.UserName, ReturnDisplayLanguage(user.FirstName, user.LastName)));
            }
            user.Namesite = GetByKey("Namesite");
            return View(user);
        }

        private Dictionary<int, string> GetDepartmentDictionary(ICollection<Department> departments, int? id, Dictionary<int, string> dictionaryDepartment, int lvl = 0, List<int> permissions = null)
        {
            if (permissions == null) return null;
            var listDepartment = departments ?? DepartmentService.Get().ToList();
            var dictionary = dictionaryDepartment ?? new Dictionary<int, string>();
            var prefix = "";
            for (var i = 0; i < lvl; i++)
            {
                prefix += "--";
            }
            foreach (var department in listDepartment.Where(a => a.ParentId == id && permissions.Any(x => x == a.Id)))
            {
                dictionary.Add(department.Id, prefix + department.Name);
                if (listDepartment.Any(x => x.ParentId == department.Id))
                {
                    GetDepartmentDictionary(listDepartment, department.Id, dictionary, lvl + 1, permissions);
                }
            }
            return dictionary;
        }

        [HttpPost]
        public ActionResult Modify(UserProfile model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Id.HasValue)
                    {
                        //update avatar or not
                        if (model.ImgFile != null)
                        {
                            var newAvatar = SaveImage(model.ImgFile, model.UserName, UtilConstants.Upload.Trainee);
                            if (newAvatar.result)
                            {
                                model.ImgAvatar = newAvatar.data.ToString();
                            }
                            else
                            {
                                return Json(newAvatar);
                            }
                        }
                    }
                    else
                    {
                        var newAvatar = SaveImage(model.ImgFile, model.UserName, UtilConstants.Upload.Trainee);
                        if (newAvatar.result)
                        {
                            model.ImgAvatar = newAvatar.data.ToString();
                        }
                        else
                        {
                            return Json(newAvatar);
                        }
                    }
                    string pass = model.Password;
                    if (!string.IsNullOrEmpty(model.Password))
                    {

                        model.Password = Common.EncryptString(model.Password);
                        pass = model.Password;
                    }
                    //string token = Session["auth_token"].ToString();
                    var user = _repoUser.Modify(model, pass, "");

                    if (user != null)
                    {
                        #region [-------add content mail user-------]
                        var checkSentEmail = GetByKey(UtilConstants.KEY_SENT_EMAIL_CHANGE_PASSWORD);
                        if (checkSentEmail.Equals("1"))
                        {
                            Sent_Email_TMS(null, null, user, null, null, null, (int)UtilConstants.ActionTypeSentmail.CreatePasswordUser);
                        }
                        #endregion
                    }

                    var result = new AjaxResponseViewModel
                    {
                        message = Messege.SUCCESS,
                        result = true
                    };
                    TempData[UtilConstants.NotifyMessageName] = result;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/Modify", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                });
            }
            return Json(new AjaxResponseViewModel()
            {
                result = false,
                message = MessageInvalidData(ModelState)
            });
        }

        #endregion

        #region Active/Inactive User

        public JsonResult UserStatus(int id)
        {
            var user = _repoUser.GetById(id);
            user.ISACTIVE = (user.ISACTIVE == 0 ? 1 : 0);
            _repoUser.Update(user);
            return Json(user.ISACTIVE, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Changepass

        public ActionResult ChangePass()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ChangePass(UserChangePassword model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var passEncrypt = Common.EncryptString(model.OldPassword);
                    var newPassEncrypt = Common.EncryptString(model.NewPassword);
                    var user = UserContext.Login(CurrentUser.Username, passEncrypt);

                    if (user == null)
                    {
                        TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                        {
                            message = Messege.VALIDATION_PASSWORD,
                            result = true
                        };
                    }
                    else
                    {
                        user.PASSWORD = newPassEncrypt;
                        user.LAST_UPDATE_DATE = DateTime.Now;
                        user.LAST_UPDATED_BY = CurrentUser.Username;

                        _repoUser.Update(user);
                        TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                        {
                            message = Messege.CHANGEPASSWORD,
                            result = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                };
            }
            return View(model);
        }
        #endregion

        #region ChangeProfile
        public ActionResult ChangeProfile()
        {

            var entity = _repoUser.GetById(CurrentUser.USER_ID);

            var user = new ChangeUserProfile();


            var groupUsers = _repoGroupUser.Get(g => g.IsDeleted != true).ToDictionary(a => a.Id, a => a.Name);
            var groupUser = groupUsers.Where(g => entity.GroupUserAccesses.Any(a => a.GroupUserId == g.Key)).Aggregate("", (current, result) => current + " - " + result.Value + "<br/>");

            var roles = _repoRole.Get().ToDictionary(a => a.ID, a => a.NAME);
            var role = roles.Where(r => entity.UserRoles.Any(a => a.RoleId == r.Key)).Aggregate("", (current, result) => current + " - " + result.Value + "<br/>");

            //var instructors =
            //    EmployeeService.GetInstructors()
            //        .Where(a => currentUser.PermissionIds.Any(x => x == a.Department_Id))
            //        .ToDictionary(a => a.Id,
            //            a => string.Format("{0} - {1} {2}", a.str_Staff_Id, a.FirstName, a.LastName));
            var instructors =
                EmployeeService.GetInstructors()
                    .Where(a => CurrentUser.PermissionIds.Any(x => x == a.Department_Id))
                    .ToDictionary(a => a.Id,
                        a => string.Format("{0} - {1}", a.str_Staff_Id, ReturnDisplayLanguage(a.FirstName, a.LastName)));
            var instructor = instructors.Where(i => i.Key == entity.InstructorId)
                .Aggregate("", (current, resuslt) => current + " - " + resuslt.Value);

            var fullName = entity.FIRSTNAME.Trim() + " " + entity.LASTNAME.Trim();

            user.Id = entity.ID;
            user.Department = entity.DepartmentId;
            user.UserName = entity.USERNAME;
            user.FullName = fullName;
            user.Address = entity.ADDRESS;
            user.Numbers = entity.PHONENO;
            user.Email = entity.EMAIL;
            user.Role = role;
            user.nameImage = entity.Avatar;

            user.Instructor = instructor;
            user.GroupUser = groupUser;
            var checkPERMISSION_DATA = _configService.GetByKey("PERMISSION_DATA"); ;
            user.Checksitepermissiondata = checkPERMISSION_DATA == null ? false : true;

            user.Namesite = GetByKey("Namesite");
            return View(user);
        }
        [HttpPost]
        public ActionResult ChangProfileModel(ChangeUserProfile model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    if (model.Id.HasValue)
                    {
                        //update avatar or not
                        if (model.ImgFile != null)
                        {
                            var newAvatar = SaveImage(model.ImgFile, model.UserName, UtilConstants.Upload.Trainee);
                            if (newAvatar.result)
                            {
                                model.ImgAvatar = newAvatar.data.ToString();
                            }
                            else
                            {
                                return Json(newAvatar);
                            }
                        }
                    }
                    else
                    {
                        var newAvatar = SaveImage(model.ImgFile, model.UserName, UtilConstants.Upload.Trainee);
                        if (newAvatar.result)
                        {
                            model.ImgAvatar = newAvatar.data.ToString();
                        }
                        else
                        {
                            return Json(newAvatar);
                        }

                    }
                    var entity = _repoUser.ChangeProfile(model);
                    Session.Remove("UserA");
                    var configsite = _configService.Get();

                    var userModel = new UserModel(entity, configsite);
                    Session["UserA"] = userModel;

                    return Json(new AjaxResponseViewModel()
                    {
                        result = true,
                        message = Messege.SUCCESS
                    });
                }

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/ChangProfileModel", ex.Message);
                return Json(new AjaxResponseViewModel()
                {
                    message = ex.Message,
                    result = false
                });

            }
            return Json(new AjaxResponseViewModel()
            {
                result = false,
                message = MessageInvalidData(ModelState)
            });
        }
        #endregion
        [HttpPost]
        public ActionResult Delete(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/Delete", Messege.ISVALID_DATA);
                    return Json(new AjaxResponseViewModel { result = false, message = Messege.ISVALID_DATA });
                }
                var entity = _repoUser.GetById(id);
                entity.ISACTIVE = 0;
                entity.IsDeleted = true;
                _repoUser.Update(entity);
                var fullName = entity.FIRSTNAME.Trim() + " " + entity.LASTNAME.Trim();

                return Json(new AjaxResponseViewModel { result = true, message = string.Format(Messege.DELETE_SUCCESSFULLY, fullName) });
            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/Delete", ex.Message);
                return Json(new AjaxResponseViewModel { result = false, message = Messege.UNSUCCESS });
            }
        }
        public ActionResult AjaxhandlerChangeInstructor(int? id)
        {
            var profile = EmployeeService.GetById(id);
            if (profile == null)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/AjaxhandlerChangeInstructor", Messege.ISVALID_DATA);
                return Json(new AjaxResponseViewModel() { result = false, message = Messege.ISVALID_DATA });
            }
            else
            {
                return Json(new
                {
                    result = true,
                    data = new
                    {
                        eid = profile.str_Staff_Id,
                        firstName = profile.FirstName,
                        lastName = profile.LastName,
                        phone = profile.str_Cell_Phone,
                        mail = profile.str_Email,
                        password = !string.IsNullOrEmpty(profile.Password) ? Common.DecryptString(profile.Password) : Common.RandomCharecter(),
                    }
                });
            }
        }
        public ActionResult AjaxhandlerChangeGroupUser(string id)
        {
            if (id.Length > 0)
            {
                if (id.Contains(","))
                {
                    string cutstr = id.Substring(0, id.LastIndexOf(","));
                    int strId = int.Parse(cutstr);
                    var groupUserId = _repoGroupUser.GetById(strId);
                    if (groupUserId == null)
                    {
                        LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/AjaxhandlerChangeGroupUser", Messege.ISVALID_DATA);
                        return Json(new AjaxResponseViewModel() { result = false, message = Messege.ISVALID_DATA });
                    }
                    else
                    {
                        return Json(new
                        {
                            result = true,
                            data = new
                            {
                                id = groupUserId.Id,
                                departments = groupUserId.GroupUserPermissions.Select(a => a.DepartmentId)
                            }
                        });
                    }

                }
                else
                {
                    int pId = int.Parse(id);
                    var groupUserId = _repoGroupUser.GetById(pId);
                    return Json(new
                    {
                        result = true,
                        data = new
                        {
                            id = groupUserId.Id,
                            departments = groupUserId.GroupUserPermissions.Select(a => a.DepartmentId)
                        }
                    });
                }
            }
            else
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/AjaxhandlerChangeGroupUser", Messege.ISVALID_DATA);
                return Json(new AjaxResponseViewModel() { result = false, message = Messege.ISVALID_DATA });
            }

        }
        public ActionResult DepartmentListViewChangeProfile(int? id, int[] groupUsers)
        {
            var listPermissions = GetDepartmentModel(false);
            var groupUserPermission = groupUsers == null || !groupUsers.Any()
                ? new List<int?>()
                : _repoGroupUser.Get(a => groupUsers.Contains(a.Id)).SelectMany(a => a.GroupUserPermissions.Select(x => x.DepartmentId)).Distinct().ToList();
            var user = id.HasValue ? UserContext.GetById(id.Value) : null;
            var model = new UserPermissionViewModel()
            {
                GroupUserPermissions = groupUserPermission,
                DepartmentViewModels = listPermissions,
                IsMaster = true,
                PermissionIds = user?.UserPermissions.Select(a => (int)a.DepartmentId).ToList() ?? new List<int>(),
                UserId = user?.ID
            };

            return PartialView("_Partials/_UserChangeProfilePermission", model);
        }
        public ActionResult DepartmentListView(int? id, int[] groupUsers)
        {
            var listPermissions = GetDepartmentModel(CurrentUser.IsMaster);
            var groupUserPermission = groupUsers == null || !groupUsers.Any()
                ? new List<int?>()
                : _repoGroupUser.Get(a => groupUsers.Contains(a.Id)).SelectMany(a => a.GroupUserPermissions.Select(x => x.DepartmentId)).Distinct().ToList();
            var user = id.HasValue ? UserContext.GetById(id.Value) : null;
            var model = new UserPermissionViewModel()
            {
                GroupUserPermissions = groupUserPermission,
                DepartmentViewModels = listPermissions,
                IsMaster = true,
                PermissionIds = user?.UserPermissions.Select(a => (int)a.DepartmentId).ToList() ?? new List<int>(),
                UserId = user?.ID
            };
            //var listPermissions = GetDepartmentModel(CurrentUser.IsMaster);
            //var groupUserPermission =  _repoGroupUser.Get(a => groupUsers.Contains(a.Id)).SelectMany(a => a.GroupUserPermissions.Select(x => x.DepartmentId)).Distinct().ToList();
            //var user = id.HasValue ? UserContext.GetById(id.Value) : null;
            //var model = new UserPermissionViewModel()
            //{
            //    GroupUserPermissions = groupUserPermission,
            //    DepartmentViewModels = listPermissions,
            //    IsMaster = true,
            //    PermissionIds = user?.UserPermissions.Select(a => a.DepartmentId).ToList() ?? new List<int>(),
            //    UserId = user?.ID
            //};
            return PartialView("_Partials/_UserModifyPermission", model);
        }
        [HttpPost]
        public ActionResult GrantUserPermission(int id, int userId)
        {
            try
            {
                if (CurrentUser.IsMaster)
                {
                    _repoUser.GrantPermission(userId, id);
                    return Json(new AjaxResponseViewModel() { result = true, message = "Grant permission to user successfully" });
                }
                return Json(new AjaxResponseViewModel() { result = false, message = "You don't have role to mange this permission" });

            }
            catch (Exception ex)
            {
                LogEvent(UtilConstants.LogType.EVENT_TYPE_ERROR, UtilConstants.LogEvent.Error, "User/GrantUserPermission", ex.Message);
                return Json(new AjaxResponseViewModel() { result = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult SubmitSetParticipateUSER(int isStatus, string id)
        {
            var idUser = int.Parse(id);
            var removeUser = _repoUser.GetById(idUser);
            if (isStatus == 1)
            {
                removeUser.ISACTIVE = 0;
            }
            else
            {
                removeUser.ISACTIVE = 1;
            }

            _repoUser.Update(removeUser);
            var fullName = removeUser.FIRSTNAME.Trim() + " " + removeUser.LASTNAME.Trim();
            return Json(new AjaxResponseViewModel { message = string.Format(Messege.SET_STATUS_SUCCESS, fullName), result = true }, JsonRequestBehavior.AllowGet);


        }
    }
}
