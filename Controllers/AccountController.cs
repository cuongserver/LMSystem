using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LMSystem.Common;
using LMSystem.Models;
using System.Security;
using System.Web.Security;
using LMSystem.Resource;
using LMSystem.Attribute;

namespace LMSystem.Controllers
{
    //[HandleError]
    public class AccountController : BaseController
    {
        [AllowAnonymous]
        [AddHeader]
        [ImportModelState]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult Login()
        {
            var routeLang = Request.RequestContext.RouteData.Values["lang"];
            ViewBag.DisplayLanguage = (routeLang == null) ? string.Empty : routeLang.ToString();
            UserIdentity model = new UserIdentity();
            return View("Login", model);

        }

        [HttpPost]
        [AllowAnonymous]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserIdentity model)
        {
            var routeLang = Request.RequestContext.RouteData.Values["lang"];
            string lang = (routeLang == null) ? string.Empty : routeLang.ToString();
            ModelState.Remove(nameof(model.newPassword));
            ModelState.Remove(nameof(model.newPasswordConfirm));
            if (ModelState.IsValid)
            {
                UserIdentityAuthProcess process = new UserIdentityAuthProcess();
                UserIdentityAuthResult result = process.Auth(model);
                if (result.validationResult == 1)
                {
                    string ticketData = result.role + "|" + result.name + "|" + model.userID;
                    var authTicket = new FormsAuthenticationTicket(1, model.userID,
                        DateTime.Now, DateTime.Now.AddMinutes(30), false, ticketData);
                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName
                        , FormsAuthentication.Encrypt(authTicket));
                    cookie.HttpOnly = true;
                    cookie.Secure = true;
                    Response.Cookies.Add(cookie);

                    if (Request.Cookies["_selector"] != null)
                    {
                        var c = new HttpCookie("_selector");
                        c.Expires = DateTime.Now.AddDays(-100);
                        Response.Cookies.Add(c);
                    }

                    if (Request.Cookies["_index"] != null)
                    {
                        var c = new HttpCookie("_index");
                        c.Expires = DateTime.Now.AddDays(-100);
                        Response.Cookies.Add(c);
                    }

                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    ModelState.AddModelError("userID", RegionSetting.Account_UsernameErr);
                    return Redirect("/" + lang + "/Account/Login");
                }
            }
            return Redirect("/" + lang + "/Account/Login");
        }
        [AddHeader]
        [CustomAuthorize(Roles = "Director, User, Admin, Manager")]
        [ImportModelState]
        public ActionResult ChangePassWord()
        {
            UserIdentity model = new UserIdentity();
            return PartialView("ChangePassword", model);
        }

        [AddHeader]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Director, User, Admin, Manager")]
        [ExportModelState]
        public ActionResult ChangePassWord(UserIdentity model)
        {
            UserChangePasswordProcess changeProcess = new UserChangePasswordProcess();
            int result;
            ModelState.Remove(nameof(model.userID));
            model.userID = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            if (ModelState.IsValid)
            {
                result = changeProcess.Result(model);
                if (result == 1)
                {
                    FormsAuthentication.SignOut();
                    Response.Headers.Add("ChangePasswordResult", "1");
                    return PartialView("ChangePasswordSuccessful");
                }
                else
                {
                    ModelState.AddModelError("userPassword", RegionSetting.Account_UsernameErr);
                    Response.Headers.Add("ChangePasswordResult", "0");
                    return PartialView("ChangePassword");
                }
            }
            else
            {
                Response.Headers.Add("ChangePasswordResult", "0");
                return PartialView("ChangePassword");
            }
        }
    }
}