using System;
using System.Web.Mvc;
using ClientMonitorLibrary.Web;
namespace ClientMonitor.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ClientMonitorAttribute : ActionFilterAttribute
    {   
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            WebServer monitor = WebServer.GetInstance();
            monitor.CountRequest();
        }

    }
}
