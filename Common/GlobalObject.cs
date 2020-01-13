using System.Configuration;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Reflection;
using System.Web.Security;
using System;
using System.Text.RegularExpressions;
namespace LMSystem.Common
{
    public class GlobalObject
    {
        public static string ConnectionStr()
        {
            return ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
        }

        public static string GetModelErrorMessage(string vKey, ViewDataDictionary viewData)
        {
            var state = viewData.ModelState.FirstOrDefault(x => x.Key.Equals(vKey));
            if (state.Value != null && state.Value.Errors.Any(x => !string.IsNullOrEmpty(x.ErrorMessage)))
            {
                return state.Value.Errors.FirstOrDefault().ErrorMessage;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string DefaultDisplayLanguage() => "en";
        public static List<string> SupportedLanguage() => new List<string> { "vi", "en" };

        public static List<string> ControllerList()
        {
            List<string> controllers = new List<string>();
            var asmPath = HttpContext.Current.Server.MapPath("~/bin/LMSystem.dll");
            Assembly asm = Assembly.LoadFile(asmPath);
            var controllerTypes = asm.GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type));
            foreach (var controller in controllerTypes)
            {
                var fullName = controller.Name;
                controllers.Add(fullName.Substring(0, fullName.Length - "Controller".Length));
            }
            controllers.Remove("Base");
            return controllers;
        }

        public static string ConstraintBuilder(List<string> list)
        {
            string constraint = string.Empty;
            if (list.Count == 0) return constraint;

            constraint += @"";
            foreach (string value in list)
            {
                constraint = string.Concat(constraint, value + "|");
            }
            return constraint.Substring(0, constraint.Length - 1);

        }

        public static string getValueFromAuthTicket(int vKey, HttpContextBase context)
        {
            string result = "";
            HttpCookie authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if(authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                string[] userData = authTicket.UserData.Split(new Char[] { '|' });
                result = userData[vKey];
            }
            return result;
        }

        public static int pageSize() => 5;

        public static bool isTimeStringValid(string timeString)
        {
            if (timeString == null) return false;
            string pattern = @"^((\d\d\d\d)\/([0]{0,1}[1-9]|1[012])\/([1-9]|([012][0-9])|(3[01])))\s(([0-1]?[0-9]|2?[0-3]):([0-5]\d))$";
            Regex regex = new Regex(pattern);
            return regex.Match(timeString).Success;
        }
    }


}