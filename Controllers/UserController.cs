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
namespace LMSystem.Controllers
{
    [CustomAuthorize]
    public class UserController : BaseController
    {
        UserDataAccessLayer DAL = new UserDataAccessLayer();
        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult AddNew()
        {
            ViewData["DeptList"] = DAL.getDeptList();
            ViewData["RankList"] = DAL.getRankList();
            User model = new User();
            return PartialView("AddNew", model);
        }
        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ExportModelState]
        [HttpPost]
        public ActionResult AddNew(User model)
        {
            if (ModelState.IsValid)
            {
                ViewData["DeptList"] = DAL.getDeptList();
                ViewData["RankList"] = DAL.getRankList();
                int res = DAL.createNewUser(model);
                switch (res)
                {
                    case 0:
                        Response.Headers.Add("Result", "0");
                        ModelState.AddModelError("userEmail", RegionSetting.User_AddNew_Unique_userEmail);
                        return PartialView("AddNew");
                    case 2627:
                        Response.Headers.Add("Result", "0");
                        ModelState.AddModelError("userID", RegionSetting.User_AddNew_Unique_userID);
                        return PartialView("AddNew");

                }
                Response.Headers.Add("Result", "1");
                ViewData["Message"] = RegionSetting.User_AddNew_SuccessMessg_param0 + model.userID
                                        + RegionSetting.User_AddNew_SuccessMessg_param1;
                return PartialView("~/Views/Shared/GeneralMessage.cshtml");

            }
            else
            {
                Response.Headers.Add("Result", "0");
                UserDataAccessLayer DAL = new UserDataAccessLayer();
                ViewData["DeptList"] = DAL.getDeptList();
                ViewData["RankList"] = DAL.getRankList();
                return PartialView("AddNew", model);
            }
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Overview(string pageIndex)
        {
            int _pageIndex;
            if (pageIndex == null)
            {
                _pageIndex = 1;
            }
            else
            {
                decimal Dec;
                var isNumeric = decimal.TryParse(pageIndex, out Dec);
                if (isNumeric == false)
                {
                    _pageIndex = 1;
                }
                else
                {
                    _pageIndex = (Dec <= 0) ? 1 : (int)Dec;
                }
            }
            List<User> Users = DAL.getUserListWithPagination(_pageIndex).userRecords;
            int recordCount = DAL.getUserListWithPagination(_pageIndex).recordCount;
            int pageSize = DAL.getUserListWithPagination(_pageIndex).pageSize;
            _pageIndex = (_pageIndex * pageSize <= recordCount)
                ? _pageIndex : (int)Math.Ceiling((double)recordCount / pageSize);
            if (recordCount == 0)
            {
                ViewData["PageTitle"] = RegionSetting.Home_UserMaint_Link2;
                return PartialView("NoResult");
            }
            ViewData["RecordCount"] = recordCount;
            ViewData["PageSize"] = pageSize;
            ViewData["PageIndex"] = _pageIndex;
            ViewData["NowDisplaying"] = RegionSetting.NowDisplaying +
                ((_pageIndex - 1) * pageSize + 1).ToString() + " - " +
                Math.Min(_pageIndex * pageSize, recordCount).ToString() +
                " / " + recordCount.ToString();
            return PartialView("Overview", Users);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult Details(string userid)
        {
            if (userid == null)
            {
                ViewData["NotFound"] = RegionSetting.NoResult;
                return PartialView("Details");
            }
            List<User> Users = DAL.getUserList();

            User user = Users.AsEnumerable().Where(x => x.userID == userid).First();
            if (user != null)
            {
                ViewData["DeptList"] = DAL.getDeptList();
                ViewData["RankList"] = DAL.getRankList();
                return PartialView("Details", user);
            }
            else
            {
                ViewData["NotFound"] = RegionSetting.NoResult;
                return PartialView("Details");
            }
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Details([Bind(Exclude = "userPassword")] User user)
        {
            List<User> Users = DAL.getUserList();
            ViewData["DeptList"] = DAL.getDeptList();
            ViewData["RankList"] = DAL.getRankList();
            ModelState.Remove(nameof(user.userPassword));
            User _user;
            if (!ModelState.IsValid)
            {
                Response.Headers.Add("EditUserInfoResult", "-1");
                var errors = from item in ModelState
                             where item.Value.Errors.Any()
                             select item.Key;
                foreach (var err in errors)
                {
                    ModelState[err].Errors.Clear();
                    ModelState.AddModelError(err, RegionSetting.User_Edit_Common_Invalid);
                }
                _user = Users.AsEnumerable().Where(x => x.userID == user.userID).First();
                return PartialView("Details", _user);
            }
            else
            {
                int result = DAL.editUserInfo(user);
                switch (result)
                {
                    case 0:
                        Response.Headers.Add("EditUserInfoResult", "0");
                        ModelState.AddModelError("userName", RegionSetting.User_Edit_Invalid_ID);
                        _user = Users.AsEnumerable().Where(x => x.userID == user.userID).First();
                        return PartialView("Details", _user);
                    case 1:
                        Response.Headers.Add("EditUserInfoResult", "1");
                        ModelState.AddModelError("userEmail", RegionSetting.User_Edit_Email_Accquired);
                        _user = Users.AsEnumerable().Where(x => x.userID == user.userID).First();
                        return PartialView("Details", _user);
                    default:
                        Response.Headers.Add("EditUserInfoResult", "2");
                        _user = Users.AsEnumerable().Where(x => x.userID == user.userID).First();
                        return PartialView("Details", _user);
                }
            }
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult UpdatePassword(string userid)
        {
            if (userid == null)
            {
                ViewData["NotFound"] = RegionSetting.NoResult;
                return PartialView("UpdatePassword");
            }
            List<User> Users = DAL.getUserList();
            User user = Users.AsEnumerable().Where(x => x.userID == userid).First();
            if (user != null)
            {
                return PartialView("UpdatePassword", user);
            }
            else
            {
                ViewData["NotFound"] = RegionSetting.NoResult;
                return PartialView("UpdatePassword");
            }
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdatePassword(User model, string newPW, string cfNewPW, string adminPW)
        {
            string adminUSer = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            ModelState.Remove(nameof(model.userName));
            ModelState.Remove(nameof(model.userPassword));
            ModelState.Remove(nameof(model.deptCode));
            ModelState.Remove(nameof(model.rankCode));
            ModelState.Remove(nameof(model.userEmail));
            if (newPW == null || newPW.Length == 0)
            {

                ModelState.AddModelError("newPW", RegionSetting.ChangePassword_Nonblank);
                Response.Headers.Add("executionResult", "-1");
                return PartialView("UpdatePassword", model);
            }

            if (newPW != cfNewPW)
            {
                ModelState.AddModelError("cfNewPW", RegionSetting.ChangePassword_NewPWNotMatched);
                Response.Headers.Add("executionResult", "-1");
                return PartialView("UpdatePassword", model);
            }

            if (adminPW == null || adminPW.Length == 0)
            {

                ModelState.AddModelError("adminPW", RegionSetting.ChangePassword_Nonblank);
                Response.Headers.Add("executionResult", "-1");
                return PartialView("UpdatePassword", model);
            }

            int executionResult = DAL.updatePwByAdmin(model, newPW, adminPW, adminUSer);
            switch (executionResult)
            {
                case 0:
                    ModelState.AddModelError("newPW", RegionSetting.User_Edit_Invalid_ID);
                    Response.Headers.Add("executionResult", "0");
                    return PartialView("UpdatePassword", model);
                case 1:

                    ModelState.AddModelError("adminPW", RegionSetting.User_Edit_Common_Invalid);
                    Response.Headers.Add("executionResult", "1");
                    return PartialView("UpdatePassword", model);
                default:
                    Response.Headers.Add("executionResult", "2");
                    Response.Headers.Add("userID", model.userID);
                    ViewData["Message"] = RegionSetting.User_UpdatePassword_SuccessMessg_param0
                        + model.userID + RegionSetting.User_UpdatePassword_SuccessMessg_param1;
                    return PartialView("~/Views/Shared/GeneralMessage.cshtml");
            }
        }


        // leave summary

        UserLeaveQuota_DataAccessLayer leaveQuota_DAL = new UserLeaveQuota_DataAccessLayer();
        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult QuotaSummary(string pageIndex)
        {
            int _pageIndex;
            if (pageIndex == null)
            {
                _pageIndex = 1;
            }
            else
            {
                decimal Dec;
                var isNumeric = decimal.TryParse(pageIndex, out Dec);
                if (isNumeric == false)
                {
                    _pageIndex = 1;
                }
                else
                {
                    _pageIndex = (Dec <= 0) ? 1 : (int)Dec;
                }
            }
            List<UserLeaveQuota> quotas = leaveQuota_DAL.quotaSummary(_pageIndex).records;
            int recordCount = leaveQuota_DAL.quotaSummary(_pageIndex).recordCount;
            int pageSize = leaveQuota_DAL.quotaSummary(_pageIndex).pageSize;
            _pageIndex = (_pageIndex * pageSize <= recordCount)
                ? _pageIndex : (int)Math.Ceiling((double)recordCount / pageSize);
            if (recordCount == 0)
            {
                ViewData["PageTitle"] = RegionSetting.Home_UserMaint_Link3;
                return PartialView("NoResult");
            }
            ViewData["RecordCount"] = recordCount;
            ViewData["PageSize"] = pageSize;
            ViewData["PageIndex"] = _pageIndex;
            ViewData["NowDisplaying"] = RegionSetting.NowDisplaying +
                ((_pageIndex - 1) * pageSize + 1).ToString() + " - " +
                Math.Min(_pageIndex * pageSize, recordCount).ToString() +
                " / " + recordCount.ToString();
            return PartialView("QuotaSummary", quotas);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult QuotaDetail(string userID)
        {
            UserLeaveQuota quota = leaveQuota_DAL.quotaDetail(userID);
            if (quota.userID == null)
            {
                ViewData["NotFound"] = RegionSetting.NoResult;
                return PartialView("QuotaDetail");
            }
            else
            {
                return PartialView("QuotaDetail", quota);
            }
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult QuotaDetail(UserLeaveQuota model)
        {
            if (!ModelState.IsValid)
            {
                Response.Headers.Add("EditQuotaResult", "-1");
                UserLeaveQuota quota = leaveQuota_DAL.quotaDetail(model.userID);
                return PartialView("QuotaDetail", quota);
            }
            else
            {
                int result = leaveQuota_DAL.quotaUpdate(model);
                if (result == 0)
                {
                    Response.Headers.Add("EditQuotaResult", "0");
                    ViewData["NotFound"] = RegionSetting.NoResult;
                    return PartialView("QuotaDetail");
                }
                Response.Headers.Add("EditQuotaResult", "1");
                UserLeaveQuota quota = leaveQuota_DAL.quotaDetail(model.userID);
                return PartialView("QuotaDetail", quota);
            }

        }
    }
}