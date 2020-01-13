using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Diagnostics;

namespace LMSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                string[] userData = authTicket.UserData.Split(new Char[] { '|' });
                string[] roles = { userData[0] };
                GenericPrincipal userPrincipal = new GenericPrincipal(new GenericIdentity(authTicket.Name), roles);
                Context.User = userPrincipal;
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Debug.WriteLine(exception.Message);
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                try
                {
                    RequestContext requestContext = ((MvcHandler)httpContext.CurrentHandler).RequestContext;
                    Debug.WriteLine(requestContext.HttpContext.Request.Url);
                    var lang = requestContext.RouteData.Values["lang"];
                    var controller = requestContext.RouteData.Values["controller"].ToString();
                    var action = requestContext.RouteData.Values["action"].ToString();
                    Debug.WriteLine(controller + "--" + action);
                    Response.Redirect("/");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Response.Redirect("/");
                }

            }
        }
    }

}
