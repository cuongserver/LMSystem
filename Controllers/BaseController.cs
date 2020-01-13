using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System.Diagnostics;
using LMSystem.Common;
using System.Collections.Generic;
using System.Web.Security;

namespace LMSystem.Controllers
{   
    [RequireHttps]
    public abstract class BaseController : Controller
    {
        private static List<string> supportedLang = GlobalObject.SupportedLanguage();
        private static string defLang = GlobalObject.DefaultDisplayLanguage();

        protected override void Initialize(RequestContext requestContext)
        {
            string URL = requestContext.HttpContext.Request.RawUrl;
            base.Initialize(requestContext);

            var routeLang = requestContext.RouteData.Values["lang"];
            var routeController = requestContext.RouteData.Values["controller"];
            var routeAction = requestContext.RouteData.Values["action"];
            var routeID = requestContext.RouteData.Values["id"];

            string controllerName = (routeController != null) ? (string)routeController : string.Empty;
            string actionName = (routeAction != null) ? (string)routeAction : string.Empty;

            string langURL = (routeLang == null) ? string.Empty : routeLang.ToString();
            var cookie = requestContext.HttpContext.Request.Cookies.Get("displayLanguage");
            string langCookie = (cookie != null) ? cookie.Value : defLang;
            string officialLang = (!string.IsNullOrEmpty(langURL) && supportedLang.Contains(langURL)) ?
                                    langURL : langCookie;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(officialLang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(officialLang);
            if (controllerName == "Account" && actionName == "Login")
            {
                if (!URL.Contains(controllerName) || !URL.Contains(actionName) || routeID != null)
                {
                    Response.Redirect(string.Format("/{0}/{1}/{2}", officialLang, controllerName, actionName), false);
                }
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string URL = filterContext.HttpContext.Request.RawUrl;
            Debug.WriteLine(URL);
            string culture = CultureInfo.CurrentCulture.ToString();
            var cookie = new HttpCookie("displayLanguage", culture);
            cookie.Expires = DateTime.MaxValue;
            filterContext.HttpContext.Response.Cookies.Add(cookie);

        }


        public ActionResult Logout()
        {
            string culture = CultureInfo.CurrentCulture.ToString();
            FormsAuthentication.SignOut();
            return Redirect("/" + culture + "/Account/Login");
        }

    }
}