using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using LMSystem.Attribute;

namespace LMSystem.Controllers
{
    [CustomAuthorize]
    public class HomeController : BaseController
    {
        [CustomAuthorize(Roles = "Director, User, Admin, Manager")]
        public ActionResult Index()
        {
            return View();
        }
        [CustomAuthorize(Roles = "Director, User, Admin, Manager")]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Dashboard()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

    }
}