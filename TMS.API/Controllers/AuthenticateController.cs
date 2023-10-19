using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TMS.API.Models;
using TMS.API.Utilities;
using TMS.Core.Services.Users;
using TMS.Core.Utils;

namespace TMS.API.Controllers
{
    [RoutePrefix("api")]
    public class AuthenticateController : ApiController
    {
        private readonly IUserContext _userContext;
        public AuthenticateController(IUserContext userContext)
        {
            _userContext = userContext;
        }

        //[Route("Login")]
        //[HttpPost]
        //public HttpResponseMessage Login(LoginModel model)
        //{
        //    var pass = Command.EncryptString(model.Password);
        //    var usernamelower = model.Username.ToLower();
        //    var userLogin = _userContext.Login(usernamelower, pass);

        //    if (userLogin == null)
        //    {
        //        return new HttpResponseMessage(HttpStatusCode.BadRequest);
        //    }
        //    var expiredDate = UtilConstants.Limitdate();
        //    var data = new
        //    {
        //        userLogin.USERNAME,
        //        userLogin.PASSWORD,
        //        expiredDate
        //    };
        //    var token = Command.EncryptString(Newtonsoft.Json.JsonConvert.SerializeObject(data));
        //    token = Command.EncryptString(token);
        //    var resp = new HttpResponseMessage(HttpStatusCode.OK);
        //    resp.Content = new StringContent(token, System.Text.Encoding.UTF8, "text/plain");
        //    return resp;
        //}
        [Route("Login")]
        [HttpPost]
        public HttpResponseMessage Login(LoginModel model)
        {
            var userlogin = ConfigurationManager.AppSettings["userlogin"];
            var passwordlogin = ConfigurationManager.AppSettings["passwordlogin"];
            var pass = model.Password;
            var usernamelower = model.Username.ToLower();


            if (userlogin == usernamelower && passwordlogin == pass)
            {
                var expiredDate = UtilConstants.Limitdate();//DateTime.Now.AddMinutes(10);
                var data = new
                {
                    userlogin,
                    passwordlogin,
                    expiredDate
                };
                var token = Command.EncryptString(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                token = Command.EncryptString(token);
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new StringContent(token, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
