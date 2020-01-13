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
using System.Text;

namespace LMSystem.Controllers
{
    [CustomAuthorize]
    public class LeaveApplicationController : BaseController
    {
        LeaveApplicationDataAccessLayer appDAL = new LeaveApplicationDataAccessLayer();
        UserDataAccessLayer DAL = new UserDataAccessLayer();
        [AddHeader]
        [CustomAuthorize(Roles = "Admin, Manager, User")]
        [ImportModelState]
        public ActionResult New()
        {
            string userID = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            LeaveApplication model = appDAL.initLeaveApp(userID);
            ViewData["ReasonList"] = appDAL.getLeaveReasonList();
            ViewData["CommonParam"] = appDAL.getCommonParam();
            ViewData["WeeklyDayOff"] = appDAL.getWeeklyDayOff();
            ViewData["PublicHoliday"] = appDAL.getPublicHoliday();
            return PartialView("New", model);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin, Manager, User")]
        [ExportModelState]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendRequest(LeaveApplication model)
        {
            bool first_validate = true;
            model.userID = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            if (string.IsNullOrEmpty(model.reasonCode))
            {
                ModelState.AddModelError(nameof(model.reasonCode), RegionSetting.invalidInput);
                first_validate = false;
            }

            if (!GlobalObject.isTimeStringValid(model.timeStart))
            {
                ModelState.AddModelError(nameof(model.timeStart), RegionSetting.invalidInput);
                first_validate = false;
            }

            if (!GlobalObject.isTimeStringValid(model.timeEnd))
            {
                ModelState.AddModelError(nameof(model.timeEnd), RegionSetting.invalidInput);
                first_validate = false;
            }

            if (!first_validate)
            {
                Response.Headers.Add("executionResult", "0");
                ViewData["ReasonList"] = appDAL.getLeaveReasonList();
                ViewData["CommonParam"] = appDAL.getCommonParam();
                ViewData["WeeklyDayOff"] = appDAL.getWeeklyDayOff();
                ViewData["PublicHoliday"] = appDAL.getPublicHoliday();
                return PartialView("New", model);
            }
            else
            {
                int result = appDAL.submitLeaveApp(model);
                Response.Headers.Add("executionResult", "1");
                if (result == 1) ViewData["Message"] = RegionSetting.validationPassed;
                if (result == 0) ViewData["Message"] = RegionSetting.validationFailed;
                return PartialView("~/Views/Shared/GeneralMessage.cshtml");
            }
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin, Manager, Director")]
        [ImportModelState]
        public ActionResult PendingAppList()
        {
            string userid = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            List<LeaveApplication> list = appDAL.showPendingApp(userid).unApprovedLeaveApps;
            int recCount = appDAL.showPendingApp(userid).appCount;

            return PartialView("PendingAppList", list);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin, Manager, Director")]
        public ActionResult SelectAuthorizeCommand(string appID, int instruction)
        {
            ViewData["instruction"] = instruction.ToString();
            ViewData["appID"] = appID;
            return PartialView("AuthorizeApp");
        }
        [AddHeader]
        [CustomAuthorize(Roles = "Admin, Manager, Director")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AuthorizeApp(string appID, int instruction, string approverDesc)
        {
            if (string.IsNullOrEmpty(approverDesc))
            {
                approverDesc = "No comment";
            }
            string userID = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            int result = appDAL.authorizeLeaveApp(appID, userID, instruction, approverDesc);
            Response.Headers.Add("executionResult", result.ToString());
            Response.Headers.Add("authorizeAction", instruction.ToString());
            Response.Headers.Add("appID", appID);
            return PartialView("~/Views/Shared/GeneralMessage.cshtml");

        }
        [AddHeader]
        [CustomAuthorize(Roles = "User, Manager")]
        public ActionResult SeeBalance(string reportYear)
        {
            string userID = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            List<leaveAvailable> list = appDAL.ShowBalance(userID, reportYear);
            ViewData["ReportYear"] = reportYear;
            return PartialView("SeeBalance", list);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult ShowAll(string pageIndex,
                                    bool check0, bool check1, bool check2, bool check3, bool check4, bool check5,
                                    string op0, string op1, string op2, string op3, string op4, string op5,
                                    string criteria0, string criteria1, string criteria2,
                                    string criteria3, string criteria4, string criteria5, string x)
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

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(nameof(check0), check0.ToString());
            dict.Add(nameof(check1), check1.ToString());
            dict.Add(nameof(check2), check2.ToString());
            dict.Add(nameof(check3), check3.ToString());
            dict.Add(nameof(check4), check4.ToString());
            dict.Add(nameof(check5), check5.ToString());

            dict.Add(nameof(op0), op0.ToString());
            dict.Add(nameof(op1), op1.ToString());
            dict.Add(nameof(op2), op2.ToString());
            dict.Add(nameof(op3), op3.ToString());
            dict.Add(nameof(op4), op4.ToString());
            dict.Add(nameof(op5), op5.ToString());

            dict.Add(nameof(criteria0), criteria0.ToString());
            dict.Add(nameof(criteria1), criteria1.ToString());
            dict.Add(nameof(criteria2), criteria2.ToString());
            dict.Add(nameof(criteria3), criteria3.ToString());
            dict.Add(nameof(criteria4), criteria4.ToString());
            dict.Add(nameof(criteria5), criteria5.ToString());

            AllLeaveApps allApps = appDAL.showAll_1(_pageIndex, dict);
            List<LeaveApplication> apps = allApps.records;
            int recordCount = allApps.recordCount;
            int pageSize = allApps.pageSize;
            _pageIndex = (_pageIndex * pageSize <= recordCount)
                ? _pageIndex : (int)Math.Ceiling((double)recordCount / pageSize);

            ViewData["DeptList"] = DAL.getDeptList();
            ViewData["RankList"] = DAL.getRankList();
            ViewData["RecordCount"] = recordCount;
            ViewData["PageSize"] = pageSize;
            ViewData["PageIndex"] = _pageIndex;
            ViewData["FilterParam"] = dict;
            ViewData["NowDisplaying"] = RegionSetting.NowDisplaying +
                ((_pageIndex - 1) * pageSize + 1).ToString() + " - " +
                Math.Min(_pageIndex * pageSize, recordCount).ToString() +
                " / " + recordCount.ToString();
            return PartialView("ShowAll", apps);
        }
        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult ShowAllApp(string pageIndex)
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

            AllLeaveApps allApps = appDAL.showAll(_pageIndex);
            List<LeaveApplication> apps = allApps.records;
            int recordCount = allApps.recordCount;
            int pageSize = allApps.pageSize;
            _pageIndex = (_pageIndex * pageSize <= recordCount)
                ? _pageIndex : (int)Math.Ceiling((double)recordCount / pageSize);
            if (recordCount == 0)
            {
                ViewData["PageTitle"] = RegionSetting.Home_LeaveManageLink_ShowAll;
                return PartialView("NoResult");
            }

            ViewData["DeptList"] = DAL.getDeptList();
            ViewData["RankList"] = DAL.getRankList();
            ViewData["RecordCount"] = recordCount;
            ViewData["PageSize"] = pageSize;
            ViewData["PageIndex"] = _pageIndex;
            ViewData["FilterParam"] = null;
            ViewData["NowDisplaying"] = RegionSetting.NowDisplaying +
                ((_pageIndex - 1) * pageSize + 1).ToString() + " - " +
                Math.Min(_pageIndex * pageSize, recordCount).ToString() +
                " / " + recordCount.ToString();
            return PartialView("ShowAll", apps);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult TerminateApp(string appID)
        {
            string userRole = GlobalObject.getValueFromAuthTicket(0, this.HttpContext);
            Response.Headers.Add("appID", appID);
            if (userRole != "Admin")
            {
                Response.Headers.Add("executionResult", "-1");
                return PartialView("~/Views/Shared/Dummy.cshtml");
            }
            int result = appDAL.terminateApp(appID);
            Response.Headers.Add("executionResult", result.ToString());
            return PartialView("~/Views/Shared/Dummy.cshtml");
        }

        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ImportModelState]
        public ActionResult AppByAdmin()
        {
            LeaveApplication model = new LeaveApplication();
            ViewData["ActiveUserList"] = DAL.GetActiveUserList();
            ViewData["ReasonList"] = appDAL.getLeaveReasonList();
            ViewData["CommonParam"] = appDAL.getCommonParam();
            ViewData["WeeklyDayOff"] = appDAL.getWeeklyDayOff();
            ViewData["PublicHoliday"] = appDAL.getPublicHoliday();
            return PartialView("AppByAdmin", model);
        }
        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        [ExportModelState]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AppByAdminSubmit(LeaveApplication model)
        {
            bool first_validate = true;
            if (string.IsNullOrEmpty(model.reasonCode))
            {
                ModelState.AddModelError(nameof(model.reasonCode), RegionSetting.invalidInput);
                first_validate = false;
            }

            if (string.IsNullOrEmpty(model.userID))
            {
                ModelState.AddModelError(nameof(model.userID), RegionSetting.invalidInput);
                first_validate = false;
            }

            if (!GlobalObject.isTimeStringValid(model.timeStart))
            {
                ModelState.AddModelError(nameof(model.timeStart), RegionSetting.invalidInput);
                first_validate = false;
            }

            if (!GlobalObject.isTimeStringValid(model.timeEnd))
            {
                ModelState.AddModelError(nameof(model.timeEnd), RegionSetting.invalidInput);
                first_validate = false;
            }

            if (!first_validate)
            {
                Response.Headers.Add("executionResult", "0");
                ViewData["ActiveUserList"] = DAL.GetActiveUserList();
                ViewData["ReasonList"] = appDAL.getLeaveReasonList();
                ViewData["CommonParam"] = appDAL.getCommonParam();
                ViewData["WeeklyDayOff"] = appDAL.getWeeklyDayOff();
                ViewData["PublicHoliday"] = appDAL.getPublicHoliday();
                return PartialView("AppByAdmin", model);
            }
            else
            {
                int result = appDAL.submitLeaveAppByAdmin(model);
                Response.Headers.Add("executionResult", "1");
                if (result == 1) ViewData["Message"] = RegionSetting.appCreated;
                if (result == 0) ViewData["Message"] = RegionSetting.appFailed;
                if (result == -1) ViewData["Message"] = RegionSetting.appFailed;
                return PartialView("~/Views/Shared/GeneralMessage.cshtml");
            }
        }
        
        [AddHeader]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ExportToCsv(
                                    bool check0, bool check1, bool check2, bool check3, bool check4, bool check5,
                                    string op0, string op1, string op2, string op3, string op4, string op5,
                                    string criteria0, string criteria1, string criteria2,
                                    string criteria3, string criteria4, string criteria5, string x)
        {
            int _pageIndex = -1;
           
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(nameof(check0), check0.ToString());
            dict.Add(nameof(check1), check1.ToString());
            dict.Add(nameof(check2), check2.ToString());
            dict.Add(nameof(check3), check3.ToString());
            dict.Add(nameof(check4), check4.ToString());
            dict.Add(nameof(check5), check5.ToString());

            dict.Add(nameof(op0), op0.ToString());
            dict.Add(nameof(op1), op1.ToString());
            dict.Add(nameof(op2), op2.ToString());
            dict.Add(nameof(op3), op3.ToString());
            dict.Add(nameof(op4), op4.ToString());
            dict.Add(nameof(op5), op5.ToString());

            dict.Add(nameof(criteria0), criteria0.ToString());
            dict.Add(nameof(criteria1), criteria1.ToString());
            dict.Add(nameof(criteria2), criteria2.ToString());
            dict.Add(nameof(criteria3), criteria3.ToString());
            dict.Add(nameof(criteria4), criteria4.ToString());
            dict.Add(nameof(criteria5), criteria5.ToString());

            AllLeaveApps allApps = appDAL.showAll_2(_pageIndex, dict);
            List<LeaveApplication> apps = allApps.records;


            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("\"{0}\",", RegionSetting.appID));
            sb.Append(string.Format("\"{0}\",", RegionSetting.User_AddNew_fieldCaption_userName));
            sb.Append(string.Format("\"{0}\",", RegionSetting.deptName));
            sb.Append(string.Format("\"{0}\",", RegionSetting.rankDescription));
            sb.Append(string.Format("\"{0}\",", RegionSetting.reasonDetail));
            sb.Append(string.Format("\"{0}\",", RegionSetting.applicantDesc));
            sb.Append(string.Format("\"{0}\",", RegionSetting.timeStart));
            sb.Append(string.Format("\"{0}\",", RegionSetting.timeEnd));
            sb.Append(string.Format("\"{0}\",", RegionSetting.hourRequired));
            sb.Append(string.Format("\"{0}\",", RegionSetting.validationStatus));
            sb.Append(string.Format("\"{0}\",", RegionSetting.applicationProgress));
            sb.Append(string.Format("\"{0}\",", RegionSetting.approverAction));
            sb.Append(string.Format("\"{0}\",", RegionSetting.approverDesc));
            sb.Append(string.Format("\"{0}\",", RegionSetting.approverUserID));
            sb.Append(string.Format("\"{0}\",", RegionSetting.systemStatus));
            sb.Append("\r\n");
            for (int i = 0; i < apps.Count; i+=1)
            {
                sb.Append(string.Format("\"{0}\",", apps[i].appID));
                sb.Append(string.Format("\"{0}\",", apps[i].userName));
                sb.Append(string.Format("\"{0}\",", apps[i].deptName));
                sb.Append(string.Format("\"{0}\",", apps[i].rankDescription));
                sb.Append(string.Format("\"{0}\",", apps[i].reasonDetail));
                sb.Append(string.Format("\"{0}\",", apps[i].applicantDesc));
                sb.Append(string.Format("\"{0}\",", apps[i].timeStart));
                sb.Append(string.Format("\"{0}\",", apps[i].timeEnd));
                sb.Append(string.Format("\"{0}\",", apps[i].hourRequired));
                sb.Append(string.Format("\"{0}\",", apps[i].validationStatus));
                sb.Append(string.Format("\"{0}\",", apps[i].applicationProgress));
                sb.Append(string.Format("\"{0}\",", apps[i].approverAction));
                sb.Append(string.Format("\"{0}\",", apps[i].approverDesc));
                sb.Append(string.Format("\"{0}\",", apps[i].approverUserID));
                sb.Append(string.Format("\"{0}\",", apps[i].systemStatus));
                sb.Append("\r\n");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "allapps.csv");
        }

        [AddHeader]
        [CustomAuthorize(Roles = "User, Manager")]
        
        public ActionResult ShowMine(string pageIndex)
        {
            int _pageIndex;
            string userID = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
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

            AllLeaveApps allApps = appDAL.showAllMyRequest(_pageIndex, userID);
            List<LeaveApplication> apps = allApps.records;
            int recordCount = allApps.recordCount;
            int pageSize = allApps.pageSize;
            _pageIndex = (_pageIndex * pageSize <= recordCount)
                ? _pageIndex : (int)Math.Ceiling((double)recordCount / pageSize);
            if (recordCount == 0)
            {
                ViewData["PageTitle"] = RegionSetting.Home_LeaveManageLink_ShowAll;
                return PartialView("NoResult");
            }

            ViewData["DeptList"] = DAL.getDeptList();
            ViewData["RankList"] = DAL.getRankList();
            ViewData["RecordCount"] = recordCount;
            ViewData["PageSize"] = pageSize;
            ViewData["PageIndex"] = _pageIndex;
            ViewData["FilterParam"] = null;
            ViewData["NowDisplaying"] = RegionSetting.NowDisplaying +
                ((_pageIndex - 1) * pageSize + 1).ToString() + " - " +
                Math.Min(_pageIndex * pageSize, recordCount).ToString() +
                " / " + recordCount.ToString();
            return PartialView("ShowMyRequest", apps);
        }

        [AddHeader]
        [CustomAuthorize(Roles = "User, Manager")]
        public ActionResult ExportMyAppsToCsv()
        {
            int _pageIndex = -1;
            string userID = GlobalObject.getValueFromAuthTicket(2, this.HttpContext);
            AllLeaveApps allApps = appDAL.showAllMyRequest_2(_pageIndex, userID);
            List<LeaveApplication> apps = allApps.records;

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("\"{0}\",", RegionSetting.appID));
            sb.Append(string.Format("\"{0}\",", RegionSetting.User_AddNew_fieldCaption_userName));
            sb.Append(string.Format("\"{0}\",", RegionSetting.deptName));
            sb.Append(string.Format("\"{0}\",", RegionSetting.rankDescription));
            sb.Append(string.Format("\"{0}\",", RegionSetting.reasonDetail));
            sb.Append(string.Format("\"{0}\",", RegionSetting.applicantDesc));
            sb.Append(string.Format("\"{0}\",", RegionSetting.timeStart));
            sb.Append(string.Format("\"{0}\",", RegionSetting.timeEnd));
            sb.Append(string.Format("\"{0}\",", RegionSetting.hourRequired));
            sb.Append(string.Format("\"{0}\",", RegionSetting.validationStatus));
            sb.Append(string.Format("\"{0}\",", RegionSetting.applicationProgress));
            sb.Append(string.Format("\"{0}\",", RegionSetting.approverAction));
            sb.Append(string.Format("\"{0}\",", RegionSetting.approverDesc));
            sb.Append(string.Format("\"{0}\",", RegionSetting.approverUserID));
            sb.Append(string.Format("\"{0}\",", RegionSetting.systemStatus));
            sb.Append("\r\n");
            for (int i = 0; i < apps.Count; i += 1)
            {
                sb.Append(string.Format("\"{0}\",", apps[i].appID));
                sb.Append(string.Format("\"{0}\",", apps[i].userName));
                sb.Append(string.Format("\"{0}\",", apps[i].deptName));
                sb.Append(string.Format("\"{0}\",", apps[i].rankDescription));
                sb.Append(string.Format("\"{0}\",", apps[i].reasonDetail));
                sb.Append(string.Format("\"{0}\",", apps[i].applicantDesc));
                sb.Append(string.Format("\"{0}\",", apps[i].timeStart));
                sb.Append(string.Format("\"{0}\",", apps[i].timeEnd));
                sb.Append(string.Format("\"{0}\",", apps[i].hourRequired));
                sb.Append(string.Format("\"{0}\",", apps[i].validationStatus));
                sb.Append(string.Format("\"{0}\",", apps[i].applicationProgress));
                sb.Append(string.Format("\"{0}\",", apps[i].approverAction));
                sb.Append(string.Format("\"{0}\",", apps[i].approverDesc));
                sb.Append(string.Format("\"{0}\",", apps[i].approverUserID));
                sb.Append(string.Format("\"{0}\",", apps[i].systemStatus));
                sb.Append("\r\n");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "myapps.csv");
        }
    }
}