using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LMSystem.Common;

namespace LMSystem
{
    public class RouteConfig
    {
        private static string langConstraint = GlobalObject.ConstraintBuilder(GlobalObject.SupportedLanguage());
        private static string controllerConstraint = GlobalObject.ConstraintBuilder(GlobalObject.ControllerList());
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "DefaultWithLang",
                url: "{lang}/{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional, lang = UrlParameter.Optional },
                constraints: new { lang = langConstraint, controller = controllerConstraint }
            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional, lang = UrlParameter.Optional },
                constraints: new { controller = controllerConstraint }
            );

            routes.MapRoute(
               name: "Catchall",
               url: "{*url}"
           );





        }
    }
}
