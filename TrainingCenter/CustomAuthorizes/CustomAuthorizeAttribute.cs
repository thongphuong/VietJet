using System.Net;
using System.Web.Security;
using TMS.Core.App_GlobalResources;
using TMS.Core.Utils;
using TMS.Core.ViewModels.Common;

namespace TrainingCenter.CustomAuthorizes
{
    using System.Web;
    using System.Web.Mvc;
    using TMS.Core.Services.Users;
    using TMS.Core.ViewModels.UserModels;

    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public IUserContext EFUserContext { get; set; }
        private UserModel _userModel;
        public UserModel UserModel
        {
            get
            {
                if (_userModel == null)
                {
                    _userModel = GetUser();
                }
                return _userModel;
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = UserModel;
            if (user == null) return false;
            httpContext.User = new CustomPrincipal(user.Username, user.FunctionIds) { UserContext = EFUserContext };
            return true;
        }
        private UserModel GetUser()
        {
            var userModel = HttpContext.Current.Session?["UserA"];
            return (UserModel)userModel;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var request = httpContext.Request;
            var response = httpContext.Response;
            if (!AuthorizeCore(httpContext))
            {
                if (request.IsAjaxRequest())
                {
                    response.StatusCode = (int)HttpStatusCode.Redirect;
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    filterContext.Result = new JsonResult
                    {
                        Data = new
                        {
                            Error = "NotAuthorized",
                            LogOnUrl = urlHelper.Action("Index", "Authenticate")
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectResult("/Authenticate/index");
                }
            }
            else if (!filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), false) &&
   !filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), false))
            {
                var controller = filterContext.RouteData.Values["controller"].ToString();
                var action = filterContext.RouteData.Values["action"].ToString();
                if (string.IsNullOrEmpty(action)) action = "Index";
                if (string.IsNullOrEmpty(controller)) action = "Home";
                var path = string.Format("/{0}/{1}", controller, action);
                if (!filterContext.HttpContext.User.IsInRole(path))
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        //filterContext.HttpContext.Response.StatusCode = 401;
                        var jsonResult = new JsonResult
                        {
                            Data = new { message = Messege.WARNING_401, result = false },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                        filterContext.Result = jsonResult;
                    }
                    else
                    {
                        filterContext.HttpContext.Response.StatusCode = 401;
                        filterContext.Result = new RedirectResult("/redirect/index");
                    }
                }
            }

        }




        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}