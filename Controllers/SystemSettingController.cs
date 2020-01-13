using LMSystem.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LMSystem.Models;
using LMSystem.Common;
using LMSystem.Resource;
using System.Diagnostics;
using System.Threading;

namespace LMSystem.Controllers
{
    [CustomAuthorize]
    public class SystemSettingController : BaseController
    {
        // GET: SystemSetting
        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult NewPublicHoliday()
        {
            PublicHoliday model = new PublicHoliday();
            return PartialView("NewPublicHoliday", model);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ExportModelState]
        [HttpPost]
        public ActionResult AddPublicHoliday(PublicHoliday model)
        {
            bool first_validate = true;
            string timeString = !string.IsNullOrEmpty(model.holiday) ? model.holiday + " 00:00" : string.Empty;
            if (!GlobalObject.isTimeStringValid(timeString))
            {
                ModelState.AddModelError(nameof(model.holiday), RegionSetting.invalidInput);
                first_validate = false;
            }
            if (!first_validate)
            {
                Response.Headers.Add("executionResult", "0");
                return PartialView("NewPublicHoliday", model);
            }
            else
            {
                int result = SystemSettingDAL.InsertNewHoliday(model);
                Response.Headers.Add("executionResult", "1");
                if (result == -1) ViewData["Message"] = RegionSetting.dateInvalid;
                if (result == 1) ViewData["Message"] = RegionSetting.recordInserted;
                if (result == 0) ViewData["Message"] = RegionSetting.recordExists;
                return PartialView("~/Views/Shared/GeneralMessage.cshtml");
            }

        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ShowPublicHoliday()
        {
            List<PublicHoliday> models = SystemSettingDAL.GetPublicHolidays();
            return PartialView("ShowPublicHoliday", models);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DisablePublicHoliday(string holiday)
        {
            int result = SystemSettingDAL.DisablePublicHoliday(holiday);
            Response.Headers.Add("executionResult", result.ToString());
            if (result == -1)
            {
                ViewData["Message"] = RegionSetting.dateInvalid;
                return PartialView("~/Views/Shared/GeneralMessage.cshtml");
            }
            if (result == 1)
            {
                ViewData["Message"] = RegionSetting.recordDisabled;
                Response.Headers.Add("id", holiday.Replace("/", "-"));
                return PartialView("~/Views/Shared/GeneralMessage.cshtml");
            }

            return PartialView("~/Views/Shared/Dummy.cshtml");
        }

    }
}