using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TrainingCenter
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });

            routes.MapRoute(
           name: "Register",
           url: "{controller}/{action}/{courseDetailid}/{staffid}",
           defaults: new
           {
               controller = "Course",
               action = "Register",
               courseDetailid = UrlParameter.Optional,
               staffid = UrlParameter.Optional
           });

            routes.MapRoute(
          name: "Result",
          url: "{controller}/{action}/{idCouresDetails}/{courseId}",
          defaults: new
          {
              controller = "Course",
              action = "Result",
              idCouresDetails = UrlParameter.Optional,
              courseId = UrlParameter.Optional
          });

            routes.MapRoute(
          name: "Modify",
          url: "{controller}/{action}/{id}/{type}",
          defaults: new
          {
              controller = "Employee",
              action = "Modify",
              type = UrlParameter.Optional,
              id = UrlParameter.Optional
          });
        }
    }

}