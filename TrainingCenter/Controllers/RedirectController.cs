using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TrainingCenter.Controllers
{
    public class RedirectController : Controller
    {
        //
        // GET: /Redirect/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ErrorPage()
        {
            return View();
        }
    }
}
