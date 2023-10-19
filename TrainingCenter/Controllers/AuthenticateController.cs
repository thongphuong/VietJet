using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DAL.Entities;
using TMS.Core.ViewModels.APIModels;
using TMS.Core.ViewModels.Common;
using TrainingCenter.Utilities;
using Utilities;

namespace TrainingCenter.Controllers
{
    using TMS.Core.Services.Configs;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels.UserModels;
    using TMS.Core.Utils;
    using TMS.Core.App_GlobalResources;
    using TMS.Core.Services.Employee;
    using System.Collections.Generic;
    using TMS.Core.Services.Department;
    using Newtonsoft.Json.Linq;
    using TMS.Core.Services.Courses;
    using TMS.Core.Services.TraineeHis;
    using TMS.Core.Services.Subject;
    using System.Net.Http;
    using System.IO;
    using System.Text;

    public class AuthenticateController : Controller
    {
        //
        // GET: /Admin/Login/
        #region Init

        private readonly ITraineeHistoryService _traineeHistoryService;
        private readonly ICourseService CourseService;
        private readonly IUserService _userService;
        private readonly IUserContext _userContext;
        private readonly IConfigService _configService;
        private readonly IDepartmentService _departmentService;
        private readonly ISubjectService _repoSubject;
        private readonly IEmployeeService _employeeService;

        public AuthenticateController(IConfigService configService, IUserContext userContext,
            ITraineeHistoryService traineeHistoryService,
            IUserService userService,
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            ISubjectService repoSubject,
            ICourseService _CourseService)
        {
            _userContext = userContext;
            _configService = configService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            CourseService = _CourseService;
            _traineeHistoryService = traineeHistoryService;
            _repoSubject = repoSubject;
            _userService = userService;
        }

        #endregion

        #region Login
        public ActionResult Index()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Session.Clear();
            ApplyCulture();
            
            ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
            ViewBag.PrivateLogoLogin = UtilConstants.PrivateLogoLogin;
            return View();
        }
        //TODO:fix lại
        //public ActionResult Index_BK()
        //{
        //    string token = "";
        //    try
        //    {
        //        token = GetAndSetCommonCookie();
        //    }
        //    catch
        //    {

        //    }

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        Session.Clear();
        //        ApplyCulture();
        //        ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
        //        return View();
        //    }
        //    else
        //    {
        //        string userName = Common.GetUsernameByToken_Tho(token);
        //        var user = _userService.GetByName(userName);

        //        if (user != null)
        //        {
        //            var check = LoginUseCookie_Tho(user.USERNAME, user.PASSWORD, token);
        //            switch (check.ToLower())
        //            {
        //                case "true":
        //                    return RedirectToAction("Index", "Home");
        //                case "fail":

        //                    TempData["fail"] = "Đăng nhập thất bại"; break;
        //                case "lock":

        //                    TempData["fail"] = "Tài khoản của bạn tạm thời bị khóa"; break;
        //                case "online":

        //                    TempData["fail"] = "Tài khoản của bạn đang đăng nhập từ trình duyệt khác. Hãy thử lại sau 30 giây"; break;
        //                default:

        //                    TempData["fail"] = "Username hoặc password nhập không chính xác!"; break;
        //            }
        //        }
        //        else
        //        {

        //            Session.Clear();
        //            ApplyCulture();
        //            ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
        //            return View();
        //        }
        //    }


        //    Session.Clear();
        //    ApplyCulture();
        //    ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
        //    return View();
        //}

        [HttpPost]
        public ActionResult Index(UserModel model)
        {
            //try
            //{
           // Response.Cache.SetCacheability(HttpCacheability.NoCache);
            ApplyCulture();
            var result = new AjaxResponseViewModel();
            var check = Userlogin(model.Username, model.Password, "", 1); // 1: cần pass , 2 : ko cần pass
            switch (check.ToLower())
            {
                case "true":
                    //FormsAuthentication.SetAuthCookie(model.Username, false);
                    return RedirectToAction("Index", "Home");
                case "fail":
                    result.result = false;
                    result.message = Messege.LOGIN_FAIL;
                    //TempData[UtilConstants.NotifyMessageName] = "Đăng nhập thất bại";
                    Session.Clear();
                    ApplyCulture();
                    ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
                    break;
                case "lock":
                    result.result = false;
                    result.message = Messege.LOGIN_BLOCK;
                    // TempData[UtilConstants.NotifyMessageName] = "Tài khoản của bạn tạm thời bị khóa";
                    Session.Clear();
                    ApplyCulture();
                    ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
                    break;
                case "online":
                    result.result = false;
                    result.message = Messege.LOGIN_BROWSER;
                    //   TempData[UtilConstants.NotifyMessageName] = "Tài khoản của bạn đang đăng nhập từ trình duyệt khác. Hãy thử lại sau 30 giây";
                    Session.Clear();
                    ApplyCulture();
                    ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
                    break;
                case "lock1p":
                    result.result = false;
                    result.message = Messege.LOGIN_FAIL_1MINUTE;
                    // TempData[UtilConstants.NotifyMessageName] = "Tài khoản của bạn đang đăng nhập từ trình duyệt khác. Hãy thử lại sau 30 giây";
                    Session.Clear();
                    ApplyCulture();
                    ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
                    break;
                case "lock30p":
                    result.result = false;
                    result.message = Messege.LOGIN_FAIL_30MINUTES;
                    //TempData[UtilConstants.NotifyMessageName] = "Tài khoản của bạn đang đăng nhập từ trình duyệt khác. Hãy thử lại sau 30 giây";
                    break;
                default:
                    result.result = false;
                    result.message = Messege.LOGIN_INCORRECT;
                    //TempData[UtilConstants.NotifyMessageName] = "Username hoặc password nhập không chính xác!";
                    Session.Clear();
                    ApplyCulture();
                    ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
                    break;
            }

            TempData[UtilConstants.NotifyMessageName] = result;
            return View();
        }

        public string Userlogin(string username, string password, string token, int type)
        {

            var pass = Common.EncryptString(password);
            var usernamelower = username.ToLower();

            USER userLogin = null;
            if (type == 1)
            {
                userLogin = _userContext.Login(usernamelower, pass); // get user by username and password
            }
            else
            {
                userLogin = _userContext.GetByUser(usernamelower); // Duy Tho Le write here
            }

            //var checkLogin = _configService.GetByKey("CheckLogin");
            //var ipClient = System.Web.HttpContext.Current.Request.UserHostAddress;
            if (false)//(checkLogin.Equals("1"))
            {
                var userAtempt = _userContext.Attempt(usernamelower);
                if (userAtempt != null)
                {
                    var dt = DateTime.Now;
                    // var aaaa = dt.AddMinutes(-30);
                    //if (userAtempt.Attempts >= 5 && dt.AddMinutes(-30) <= userAtempt.LASTLOGINFAIL && (!string.IsNullOrEmpty(userAtempt.IP) && userAtempt.IP.Equals(ipClient)))
                    //{
                    //    if ((!string.IsNullOrEmpty(userAtempt.IP) && !userAtempt.IP.Equals(ipClient)))
                    //    {
                    //        userAtempt.Attempts = 1;
                    //    }
                    //    else
                    //    {
                    //        userAtempt.Attempts = userAtempt.Attempts.HasValue ? userAtempt.Attempts + 1 : 1;
                    //    }
                    //    userAtempt.LASTLOGINFAIL = DateTime.Now;
                    //    _userContext.Update(userAtempt);
                    //    return "lock30p";
                    //}
                    //if (userAtempt.Attempts >= 3 && dt.AddMinutes(-1) <= userAtempt.LASTLOGINFAIL && !string.IsNullOrEmpty(userAtempt.IP) && userAtempt.IP.Equals(ipClient))
                    //{
                    //    if ((!string.IsNullOrEmpty(userAtempt.IP) && !userAtempt.IP.Equals(ipClient)))
                    //    {
                    //        userAtempt.Attempts = 1;
                    //    }
                    //    else
                    //    {
                    //        userAtempt.Attempts = userAtempt.Attempts.HasValue ? userAtempt.Attempts + 1 : 1;
                    //    }
                    //    userAtempt.LASTLOGINFAIL = DateTime.Now;
                    //    _userContext.Update(userAtempt);
                    //    return "lock1p";
                    //}
                }
            }

            if (userLogin == null)
            {
                if(false)// (checkLogin.Equals("1"))
                {

                    var userAtempt = _userContext.Attempt(usernamelower);
                    if (userAtempt != null)
                    {

                        //if (!string.IsNullOrEmpty(userAtempt.IP) && !userAtempt.IP.Equals(ipClient))
                        //{
                        //    userAtempt.Attempts = 1;
                        //}
                        //else
                        //{
                        //    userAtempt.Attempts = userAtempt.Attempts.HasValue ? userAtempt.Attempts + 1 : 1;
                        //}
                        //userAtempt.LASTLOGINFAIL = DateTime.Now;
                        //userAtempt.IP = ipClient;
                        //_userContext.Update(userAtempt);
                    }
                }

                return false+"";
            }

            //var configsite = _configService.Get();
            var userModel = new UserModel(userLogin, null);

            //var checkPERMISSION_DATA = _configService.UtilConstants.PERMISSION_DATA");

            //if(1!=1)//if(checkPERMISSION_DATA == "0")
            //{
            //    // nếu khong chọn groupuser thì cho full data
            //    var datauser = _userService.GetById(userModel.USER_ID);
            //    if(datauser != null)
            //    {
            //        if (!datauser.GroupUserAccesses.Any())
            //        {
            //            //var userPermissions = _departmentService.Get(a => !a.IsDeleted && a.IsActive).Select(a => (int)a.Id).ToList();
            //            var userPermissions = datauser.UserPermissions.Select(a => a.DepartmentId).ToList();

            //            userModel.PermissionIds = userPermissions;
            //        }

            //    }
            //}

            if (userModel.IsMaster)
            {
                var userPermissions = _departmentService.Get(a => a.IsDeleted == false && a.IsActive == true).Select(a => (int)a.Id);
                // var userPermissions = datauser.UserPermissions.Select(a => a.DepartmentId).ToList();
                userModel.PermissionIds = userPermissions.ToList();
            }

            if (userModel.IsActive == (int)UtilConstants.UserActive.On)
            {
                //var p = Common.DecryptString(userModel.Password);
                Session.Clear();

                Session["UserA"] = userModel;
                string timeout = ConfigurationManager.AppSettings["SessionTimeout"];
                Session.Timeout = int.Parse(timeout);

                HttpCookie cookie = Request.Cookies["EPOA_culture"];
                if (cookie != null)
                {
                    userModel.LanguageAbbreviation = cookie.Value;
                    Session["Lang"] = cookie.Value;
                }

                Session["auth_token"] = token;

                userLogin.ONLINEAT = DateTime.Now;
                // userLogin.LASTONLINEAT = DateTime.Now;
                userLogin.USERSTATE = (int)UtilConstants.UserStateConstant.Online;

                //return so lan dang nhap = 0
                userLogin.Attempts = 0;

                _userContext.Update(userLogin);

                return true+"";
            }
            else
            {
                return "lock";
            }
            //return false.ToString();
        }
        #endregion

        #region Login - Logout
        public ActionResult Logout()
        {

            try
            {
                string token = Session["auth_token"].ToString();
                //AddCookie("expried");
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            if (Session["UserA"] != null)
            {
                var currentUser = (UserModel)Session["UserA"];
                var user = _userContext.GetById(currentUser.USER_ID);
                user.LASTONLINEAT = DateTime.Now;
                user.USERSTATE = (int)UtilConstants.UserStateConstant.Offline;
                _userContext.Update(user);

                //var cache = MyRedisConnectorHelper.Connection.GetDatabase();

                //var value = cache.KeyExists($"MENU_USER:{currentUser.USER_ID}");

                //if(value == true)
                //{
                //    cache.KeyDelete($"MENU_USER:{ currentUser.USER_ID}");
                //}
            }
           
            Session.Abandon();
            return RedirectToAction("Index", "Authenticate");
        }
        #endregion
        public ActionResult CheckUserOnline()
        {
            if (Session["UserA"] != null)
            {
                var currentUser = (UserModel)Session["UserA"];
                var user = _userContext.GetById(currentUser.USER_ID);
                user.LASTONLINEAT = DateTime.Now;
                user.USERSTATE = (int)UtilConstants.UserStateConstant.Offline;
                _userContext.Update(user);
            }
            else
            {
                Session.Abandon();
                return RedirectToAction("Index", "Authenticate");
            }
            return Json(new AjaxResponseViewModel()
            {
                result = true,
                message = "check successfully !"
            }, JsonRequestBehavior.AllowGet);
        }
        #region Login - ForgotPass
        public ActionResult ForgotPass()
        {
            Session.Clear();
            ApplyCulture();
            ViewBag.PrivateLogo = UtilConstants.PrivateLogo;
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPass(string email)
        {

            var usercheck = _userContext.GetByEmail(email);

            if (usercheck == null)
            {


                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                {
                    message = Messege.FORGOT_EMAIL,
                    result = true
                };
                return View();
            }
            else
            {
                var randomPass = Common.RandomCharecter();

                usercheck.PASSWORD = Common.EncryptString(randomPass);
                var loadTemplate = System.IO.File.ReadAllText(Server.MapPath(ConfigurationManager.AppSettings["PrivateTemplate"] + "ForgotPassword.html"));

                var to = usercheck.EMAIL;
                var content = String.Format(loadTemplate, usercheck.USERNAME, randomPass);
                //TODO:SMS
                MailUtil.SendMail(to, "New password", content);

                _userContext.Update(usercheck);

                TempData[UtilConstants.NotifyMessageName] = new AjaxResponseViewModel()
                {
                    message = Messege.SEND_SUCCESS_EMAIL,
                    result = true
                };
                return RedirectToAction("Index", "Authenticate");
            }
        }
        #endregion

        //public ConfigData GetConfigData()
        //{
        //    var config = new ConfigData();
        //    var models = unitOfWork.ConfigRepository.GetConfigs();
        //    config.EmailSite = models.SingleOrDefault(p => p.Id.Equals("EmailSite")).Value;
        //    config.EmailSend = models.SingleOrDefault(p => p.Id.Equals("EmailSend")).Value;
        //    config.EmailPassword = models.SingleOrDefault(p => p.Id.Equals("PasswordEmail")).Value;
        //    config.Server = models.SingleOrDefault(p => p.Id.Equals("Server")).Value;
        //    config.Logo = models.SingleOrDefault(p => p.Id.Equals("Logo")).Value;
        //    return config;
        //}


        public ActionResult ChangePass()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePass(string pass, string newpass, string newpasscom)
        {
            var user = Session["Customer"] as USER;
            user = _userContext.GetById(user.ID);

            var Pass = Common.EncryptString(pass);
            if (!user.PASSWORD.Equals(Pass))
            {
                return Json(new { result = false, messege = "Your password incorrect!" }, JsonRequestBehavior.AllowGet);
            }
            if (!newpasscom.Equals(newpass))
            {
                return Json(new { result = false, messege = "Mật khẩu xác nhận ko trùng khớp với mật khẩu!" }, JsonRequestBehavior.AllowGet);
            }
            Pass = Common.EncryptString(newpass);
            user.PASSWORD = Pass;
            user.LASTONLINEAT = DateTime.Now;
            _userContext.Update(user);

            return Json(new { result = true, messege = "Your password has been changed successfully!" }, JsonRequestBehavior.AllowGet);
        }

        private void SetCulture(string culture)
        {
            // TO-DO: Validate input
            var cookie = Request.Cookies["UGMS_culture"];
            if (cookie != null)
            {
                cookie.Value = culture;
            }
            else
            {
                cookie = new HttpCookie("UGMS_culture")
                {
                    HttpOnly = false,
                    Value = culture,
                    Expires = DateTime.Now.AddDays(1)
                };
                // Not accessible by JS.
            }
            Response.Cookies.Add(cookie);
        }


        private void ApplyCulture()
        {
            var culture = "vi-VN";
            var cultureCookie = Request.Cookies["UGMS_culture"];
            if (cultureCookie != null)
            {
                culture = cultureCookie.Value;
            }
            else
            {
                SetCulture(culture);
            }
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
        }

        public ActionResult ChangeLanguage(string culture)
        {
            SetCulture(culture);

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;

            Session["lang"] = culture;

            return View("Index");
        }    
        public void AddCookie(string cookie)
        {
            HttpCookie ulis_Cookie = new HttpCookie("tms_access_token");

            ulis_Cookie.Value = cookie;
            ulis_Cookie.Expires = DateTime.Now.AddHours(24);
            ulis_Cookie.Domain = UtilConstants.TMS_Cookie;
            Response.SetCookie(ulis_Cookie);
        }
        public UserModel GetUser()
        {
            var userModel = System.Web.HttpContext.Current.Session?["UserA"];

            return (UserModel)userModel;
        }
        public string GenerateMenu()
        {
            var user = GetUser();
            if (user == null) return null;
            var menuHtml = new StringBuilder();
            //HttpCookie cookie = System.Web.HttpContext.Current.Request.Cookies["language"];
          // var cache = MyRedisConnectorHelper.Connection.GetDatabase();

            //var value = cache.StringGet($"MENU_USER:{user.USER_ID}");
            if(false)//(value.HasValue)
            {
                //menuHtml.Append(value.ToString());
            }
            else
            {
                var menus = _configService.GetMenu();
                string code = null;
                var lvl = 1;
                var currentlvl = 1;

                foreach (var menu in menus)
                {
                    var menuItems = new StringBuilder();
                    if (menu.GroupFunction != null && menu.GroupFunction.IsActive == true && !user.FunctionIds.ContainsKey(menu.GroupFunction.Id))
                    { continue; }

                    var span = "<span class='zmdi zmdi-plus'></span>";
                    #region

                    var checkremovemenu = menus.Where(a => a.Ancestor.StartsWith(menu.Ancestor + "_"));
                    if (checkremovemenu.Any())
                    {
                        var checkreturn = true; // gan fail mac dinh
                        foreach (var item in checkremovemenu)
                        {

                            if (item.GroupFunction != null && item.GroupFunction.IsActive == true &&
                                !user.FunctionIds.ContainsKey(item.GroupFunction.Id))
                            {
                                // true
                            }
                            else
                            {
                                checkreturn = false;
                            }


                        }
                        if (checkreturn) // true thi dung
                        {
                            continue;
                        }
                    }
                    else
                    {
                        span = "";
                    }
                    #endregion


                    if (string.IsNullOrEmpty(code) || !menu.Ancestor.StartsWith(code))
                    {
                        code = menu.Code;
                        for (var i = 1; i < currentlvl; i++)
                        {
                            menuItems.Append("</ul>");
                        }
                        currentlvl = lvl = 1;
                    }

                    lvl = menu.Ancestor.Split(new char[] { '_' }).Count();

                    if (lvl > currentlvl)
                    {
                        menuItems.AppendFormat("<ul class='nav' style='margin-left:{0}px;'>", lvl * 5);
                    }
                    else if (lvl < currentlvl)
                    {
                        menuItems.Append("</ul>");
                    }
                    currentlvl = lvl;

                    //var menuname = ConfigService.GetMenuNameById(menu.ID);
                    menuItems.Append("<li class='line_" + menu.ID + "'>");
                    menuItems.Append("<a title ='" + menu.TITLE + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menu.TITLE + span + "</a>");
                    //if (menuname == null)
                    //{
                    //    menuItems.Append("<a title ='" + menu.TITLE + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menu.TITLE + span + "</a>");
                    //}
                    //else
                    //{
                    //    if (true)//cookie.Value != menuname.Type)
                    //    {
                    //        menuItems.Append("<a title ='" + menu.TITLE + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menu.TITLE + span + "</a>");
                    //    }
                    //    else
                    //    {
                    //        menuItems.Append("<a title ='" + menuname.Name + "' href='" + (menu.GroupFunctionId == null ? "javascript:void(0);" : menu.GroupFunction.DefaultUrl) + "'><i class='" + menu.ICON + "' style='margin-right:5px;'></i> " + menuname.Name + span + "</a>");
                    //    }
                    //}


                    menuHtml.Append(menuItems);
                }
                for (var i = 1; i < currentlvl; i++)
                {
                    menuHtml.Append("</ul>");
                }
                //cache.StringSet($"MENU_USER:{user.USER_ID}", menuHtml.ToString());
            }

            return menuHtml.ToString();
        }

        [AllowAnonymous]
        public void ReadData()
        {
            var cache = MyRedisConnectorHelper.Connection.GetDatabase();
            var devicesCount = 10000;
            var texxt = "";
            for (int i = 0; i < devicesCount; i++)
            {
                var value = cache.StringGet($"Device_Status:{i}");
                texxt += value + Environment.NewLine;
                //Console.WriteLine($"Valor={value}");
            }
        }
        [AllowAnonymous]
        public void SaveBigData()
        {
            var devicesCount = 10000;
            var rnd = new Random();
            var cache = MyRedisConnectorHelper.Connection.GetDatabase();

            for (int i = 1; i < devicesCount; i++)
            {
                var value = rnd.Next(0, 10000);
                cache.StringSet($"Device_Status:{i}", value);
            }
        }
    }
}
