using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Core.Services.Charset
{
    public class ResponseCharset : ActionFilterAttribute
    {
        private string Charset;

        public ResponseCharset(string charset = "utf-8")
        {
            Charset = charset;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.Headers["Content-Type"] += ";charset=utf-8";
        }
    }
}
