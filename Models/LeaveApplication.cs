using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using LMSystem.Common;
using LMSystem.Resource;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace LMSystem.Models
{
    public class LeaveApplication
    {
        public string appID { get; set; }

        public DateTime initDate { get; set; }

        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userID), ResourceType = typeof(RegionSetting))]
        public string userID { get; set; }

        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userName), ResourceType = typeof(RegionSetting))]
        public string userName { get; set; }

        public string deptCode { get; set; }

        public string deptName { get; set; }

        public string rankCode { get; set; }

        public string rankDescription { get; set; }

        public string reasonCode { get; set; }

        public string reasonDetail { get; set; }

        public string applicantDesc { get; set; }

        public string timeStart { get; set; }

        public string timeEnd { get; set; }

        public string hourRequired { get; set; }

        public string validationStatus { get; set; }

        public string approverUserID { get; set; }

        public string approverAction { get; set; }

        public string approverDesc { get; set; }

        public string applicationProgress { get; set; }

        public string systemStatus { get; set; }

        public string recordChangeLog { get; set; }

    }

    public class LeaveReason
    {
        public string reasonCode { get; set; }
        public string reasonDetail { get; set; }
    }

    public class UnapprovedLeaveApps
    {
        public List<LeaveApplication> unApprovedLeaveApps { get; set; }
        public int appCount { get; set; }
    }

    public class AllLeaveApps
    {
        public int pageSize { get; set; }
        public int recordCount { get; set; }
        public List<LeaveApplication> records { get; set; }
    }

    public class leaveAvailable
    {
        public string reasonCode { get; set; }
        public string reasonDetail { get; set; }
        public string leaveQuota { get; set; }
        public string leaveUsed { get; set; }
        public string leavePending { get; set; }
        public string leaveBalance { get; set; }
    }

    //public class LeaveApplicationUI
    //{
    //    public LeaveApplication Application { get; set; }
    //    public List<LeaveReason> reasonSet { get; set; }
    //}

    public class LeaveApplicationDataAccessLayer
    {
        public LeaveApplication initLeaveApp(string userID)
        {
            LeaveApplication app = new LeaveApplication();
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spInitLeaveApp"
                };
                cmd.Parameters.AddWithValue("@userID", userID);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                con.Open();
                da.Fill(dt);
                con.Close();
                da.Dispose();
                DataRow dr = dt.Rows[0];
                app.appID = Convert.ToString(dr[nameof(app.appID)]);
                app.userID = Convert.ToString(dr[nameof(app.userID)]);
                app.userName = Convert.ToString(dr[nameof(app.userName)]);
                app.initDate = Convert.ToDateTime(dr[nameof(app.initDate)]);
                app.applicationProgress = Convert.ToString(dr[nameof(app.applicationProgress)]);

                app.deptCode = Convert.ToString(dr[nameof(app.deptCode)]);
                app.deptName = Convert.ToString(dr[nameof(app.deptName)]);
                app.rankCode = Convert.ToString(dr[nameof(app.rankCode)]);
                app.rankDescription = Convert.ToString(dr[nameof(app.rankDescription)]);

                app.timeStart = Convert.ToString(dr[nameof(app.timeStart)]);
                app.timeEnd = Convert.ToString(dr[nameof(app.timeEnd)]);
                app.hourRequired = Convert.ToString(dr[nameof(app.hourRequired)]);
                app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                app.applicantDesc = Convert.ToString(dr[nameof(app.applicantDesc)]);
            }
            return app;
        }
        public List<LeaveReason> getLeaveReasonList()
        {
            List<LeaveReason> list = new List<LeaveReason>();
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText = "select * from [mData-LeaveReason]"
                };

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                con.Open();
                da.Fill(dt);
                con.Close();
                da.Dispose();
                foreach (DataRow dr in dt.Rows)
                {
                    LeaveReason reason = new LeaveReason
                    {
                        reasonCode = Convert.ToString(dr[nameof(reason.reasonCode)]),
                        reasonDetail = Convert.ToString(dr[nameof(reason.reasonDetail)])
                    };
                    list.Add(reason);
                }
            }
            return list;
        }
        public Dictionary<string, string> getCommonParam()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText = "select * from [commonParam]"
                };

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                con.Open();
                da.Fill(dt);
                con.Close();
                da.Dispose();
                foreach (DataRow dr in dt.Rows)
                {
                    dict.Add(dr["param"].ToString(), dr["fValue"].ToString());
                }
            }
            return dict;
        }

        public List<string> getWeeklyDayOff()
        {
            List<string> list = new List<string>();
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText = "select * from [weeklyDayOff]"
                };

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                con.Open();
                da.Fill(dt);
                con.Close();
                da.Dispose();
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr["weekDay"].ToString());
                }
            }
            return list;
        }
        public List<string> getPublicHoliday()
        {
            List<string> list = new List<string>();
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "GetPublicHolidayList"
                };

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);

                con.Open();
                da.Fill(dt);
                con.Close();
                da.Dispose();
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(dr["holiday"].ToString());
                }
            }
            return list;
        }

        public int submitLeaveApp(LeaveApplication model)
        {
            string connStr = GlobalObject.ConnectionStr();
            int result = -1000;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "submitLeaveApplication"
                };
                cmd.Parameters.AddWithValue("@appID", model.appID);
                cmd.Parameters.AddWithValue("@userID", model.userID);
                cmd.Parameters.AddWithValue("@reasonCode", model.reasonCode);
                cmd.Parameters.AddWithValue("@applicantDesc", model.applicantDesc);
                cmd.Parameters.AddWithValue("@timeStart", model.timeStart);
                cmd.Parameters.AddWithValue("@timeEnd", model.timeEnd);

                SqlParameter prm = cmd.Parameters.Add("@result", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                result = Convert.ToInt32(prm.Value);
            }
            Debug.WriteLine(result.ToString());
            return result;
        }

        public UnapprovedLeaveApps showPendingApp(string userid)
        {
            UnapprovedLeaveApps collection = new UnapprovedLeaveApps();
            List<LeaveApplication> list = new List<LeaveApplication>();
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "appWaitingApproval"
                };
                cmd.Parameters.AddWithValue("@userId", userid);

                SqlParameter prm = cmd.Parameters.Add("@recCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                collection.appCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    LeaveApplication app = new LeaveApplication();
                    app.appID = Convert.ToString(dr[nameof(app.appID)]);
                    app.userID = Convert.ToString(dr[nameof(app.userID)]);
                    app.userName = Convert.ToString(dr[nameof(app.userName)]);
                    app.deptCode = Convert.ToString(dr[nameof(app.deptCode)]);
                    app.deptName = Convert.ToString(dr[nameof(app.deptName)]);
                    app.rankCode = Convert.ToString(dr[nameof(app.rankCode)]);
                    app.rankDescription = Convert.ToString(dr[nameof(app.rankDescription)]);
                    app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                    app.reasonDetail = Convert.ToString(dr[nameof(app.reasonDetail)]);
                    app.applicantDesc = Convert.ToString(dr[nameof(app.applicantDesc)]);
                    app.timeStart = Convert.ToString(dr[nameof(app.timeStart)]);
                    app.timeEnd = Convert.ToString(dr[nameof(app.timeEnd)]);
                    app.hourRequired = Convert.ToString(dr[nameof(app.hourRequired)]);
                    list.Add(app);
                }
                collection.unApprovedLeaveApps = list;
            }

            return collection;
        }

        public int authorizeLeaveApp(string appID, string userID, int instruction, string approverDesc)
        {
            int result;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "authorizeApp"
                };

                cmd.Parameters.AddWithValue("@appID", appID);
                cmd.Parameters.AddWithValue("@userId", userID);
                cmd.Parameters.AddWithValue("@instruction", instruction);
                cmd.Parameters.AddWithValue("@approverDesc", approverDesc);

                SqlParameter prm = cmd.Parameters.Add("@result", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                result = Convert.ToInt32(prm.Value);
                con.Close();
            }
            return result;
        }

        public List<leaveAvailable> ShowBalance(string userid, string reportYear)
        {
            List<leaveAvailable> list = new List<leaveAvailable>();
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "[showBalance]"
                };

                cmd.Parameters.AddWithValue("@userID", userid);
                cmd.Parameters.AddWithValue("@reportYear", reportYear);
                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
                da.Dispose();
                foreach (DataRow dr in dt.Rows)
                {
                    leaveAvailable app = new leaveAvailable();
                    app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                    app.reasonDetail = Convert.ToString(dr[nameof(app.reasonDetail)]);
                    app.leaveQuota = Convert.ToString(dr[nameof(app.leaveQuota)]);
                    app.leaveUsed = Convert.ToString(dr[nameof(app.leaveUsed)]);
                    app.leavePending = Convert.ToString(dr[nameof(app.leavePending)]);
                    app.leaveBalance = Convert.ToString(dr[nameof(app.leaveBalance)]);
                    list.Add(app);
                }
            }
            return list;
        }
        public AllLeaveApps showAll(int pageIndex)
        {
            int pageSize = GlobalObject.pageSize();
            AllLeaveApps apps = new AllLeaveApps();
            List<LeaveApplication> list = new List<LeaveApplication>();
            apps.pageSize = pageSize;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spListAllApplication"
                };

                cmd.Parameters.AddWithValue("@pageSize", apps.pageSize);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);

                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                apps.recordCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    LeaveApplication app = new LeaveApplication();
                    app.appID = Convert.ToString(dr[nameof(app.appID)]);
                    app.userID = Convert.ToString(dr[nameof(app.userID)]);
                    app.userName = Convert.ToString(dr[nameof(app.userName)]);
                    app.deptCode = Convert.ToString(dr[nameof(app.deptCode)]);
                    app.deptName = Convert.ToString(dr[nameof(app.deptName)]);
                    app.rankCode = Convert.ToString(dr[nameof(app.rankCode)]);
                    app.rankDescription = Convert.ToString(dr[nameof(app.rankDescription)]);
                    app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                    app.reasonDetail = Convert.ToString(dr[nameof(app.reasonDetail)]);
                    app.applicantDesc = Convert.ToString(dr[nameof(app.applicantDesc)]);
                    app.timeStart = Convert.ToString(dr[nameof(app.timeStart)]);
                    app.timeEnd = Convert.ToString(dr[nameof(app.timeEnd)]);
                    app.hourRequired = Convert.ToString(dr[nameof(app.hourRequired)]);
                    app.systemStatus = Convert.ToString(dr[nameof(app.systemStatus)]);
                    app.approverAction = Convert.ToString(dr[nameof(app.approverAction)]);
                    app.approverUserID = Convert.ToString(dr[nameof(app.approverUserID)]);
                    app.approverDesc = Convert.ToString(dr[nameof(app.approverDesc)]);
                    app.applicationProgress = Convert.ToString(dr[nameof(app.applicationProgress)]);
                    app.validationStatus = Convert.ToString(dr[nameof(app.validationStatus)]);

                    list.Add(app);
                }
                apps.records = list;
            }
            return apps;
        }

        public AllLeaveApps showAll_1(int pageIndex, Dictionary<string, string> dict)
        {

            int pageSize = GlobalObject.pageSize();
            AllLeaveApps apps = new AllLeaveApps();
            List<LeaveApplication> list = new List<LeaveApplication>();
            apps.pageSize = pageSize;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spListAllApplicationWithFilter"
                };

                cmd.Parameters.AddWithValue("@pageSize", apps.pageSize);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);

                cmd.Parameters.AddWithValue("@check0", dict["check0"]);
                cmd.Parameters.AddWithValue("@check1", dict["check1"]);
                cmd.Parameters.AddWithValue("@check2", dict["check2"]);
                cmd.Parameters.AddWithValue("@check3", dict["check3"]);
                cmd.Parameters.AddWithValue("@check4", dict["check4"]);
                cmd.Parameters.AddWithValue("@check5", dict["check5"]);

                cmd.Parameters.AddWithValue("@op0", dict["op0"]);
                cmd.Parameters.AddWithValue("@op1", dict["op1"]);
                cmd.Parameters.AddWithValue("@op2", dict["op2"]);
                cmd.Parameters.AddWithValue("@op3", dict["op3"]);
                cmd.Parameters.AddWithValue("@op4", dict["op4"]);
                cmd.Parameters.AddWithValue("@op5", dict["op5"]);

                cmd.Parameters.AddWithValue("@criteria0", dict["criteria0"]);
                cmd.Parameters.AddWithValue("@criteria1", dict["criteria1"]);
                cmd.Parameters.AddWithValue("@criteria2", dict["criteria2"]);
                cmd.Parameters.AddWithValue("@criteria3", dict["criteria3"]);
                cmd.Parameters.AddWithValue("@criteria4", dict["criteria4"]);
                cmd.Parameters.AddWithValue("@criteria5", dict["criteria5"]);

                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                apps.recordCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    LeaveApplication app = new LeaveApplication();
                    app.appID = Convert.ToString(dr[nameof(app.appID)]);
                    app.userID = Convert.ToString(dr[nameof(app.userID)]);
                    app.userName = Convert.ToString(dr[nameof(app.userName)]);
                    app.deptCode = Convert.ToString(dr[nameof(app.deptCode)]);
                    app.deptName = Convert.ToString(dr[nameof(app.deptName)]);
                    app.rankCode = Convert.ToString(dr[nameof(app.rankCode)]);
                    app.rankDescription = Convert.ToString(dr[nameof(app.rankDescription)]);
                    app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                    app.reasonDetail = Convert.ToString(dr[nameof(app.reasonDetail)]);
                    app.applicantDesc = Convert.ToString(dr[nameof(app.applicantDesc)]);
                    app.timeStart = Convert.ToString(dr[nameof(app.timeStart)]);
                    app.timeEnd = Convert.ToString(dr[nameof(app.timeEnd)]);
                    app.hourRequired = Convert.ToString(dr[nameof(app.hourRequired)]);
                    app.systemStatus = Convert.ToString(dr[nameof(app.systemStatus)]);
                    app.approverAction = Convert.ToString(dr[nameof(app.approverAction)]);
                    app.approverUserID = Convert.ToString(dr[nameof(app.approverUserID)]);
                    app.approverDesc = Convert.ToString(dr[nameof(app.approverDesc)]);
                    app.applicationProgress = Convert.ToString(dr[nameof(app.applicationProgress)]);
                    app.validationStatus = Convert.ToString(dr[nameof(app.validationStatus)]);

                    list.Add(app);
                }
                apps.records = list;
            }
            return apps;
        }

        public int terminateApp(string appID)
        {
            string connStr = GlobalObject.ConnectionStr();
            int success;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "terminateApp"
                };

                cmd.Parameters.AddWithValue("@appID", appID);
                SqlParameter prm = cmd.Parameters.Add("@result", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                success = Convert.ToInt32(prm.Value);
            }
            return success;

        }

        public int submitLeaveAppByAdmin(LeaveApplication model)
        {
            string connStr = GlobalObject.ConnectionStr();
            int result = -1000;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "submitLeaveApplicationByAdmin"
                };
                cmd.Parameters.AddWithValue("@userID", model.userID);
                cmd.Parameters.AddWithValue("@reasonCode", model.reasonCode);
                cmd.Parameters.AddWithValue("@applicantDesc", model.applicantDesc);
                cmd.Parameters.AddWithValue("@timeStart", model.timeStart);
                cmd.Parameters.AddWithValue("@timeEnd", model.timeEnd);

                SqlParameter prm = cmd.Parameters.Add("@result", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                result = Convert.ToInt32(prm.Value);
            }
            Debug.WriteLine(result.ToString());
            return result;
        }

        public AllLeaveApps showAll_2(int pageIndex, Dictionary<string, string> dict)
        {

            int pageSize = -1;
            AllLeaveApps apps = new AllLeaveApps();
            List<LeaveApplication> list = new List<LeaveApplication>();
            apps.pageSize = pageSize;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spListAllApplicationWithFilterForDownload"
                };

                cmd.Parameters.AddWithValue("@pageSize", apps.pageSize);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);

                cmd.Parameters.AddWithValue("@check0", dict["check0"]);
                cmd.Parameters.AddWithValue("@check1", dict["check1"]);
                cmd.Parameters.AddWithValue("@check2", dict["check2"]);
                cmd.Parameters.AddWithValue("@check3", dict["check3"]);
                cmd.Parameters.AddWithValue("@check4", dict["check4"]);
                cmd.Parameters.AddWithValue("@check5", dict["check5"]);

                cmd.Parameters.AddWithValue("@op0", dict["op0"]);
                cmd.Parameters.AddWithValue("@op1", dict["op1"]);
                cmd.Parameters.AddWithValue("@op2", dict["op2"]);
                cmd.Parameters.AddWithValue("@op3", dict["op3"]);
                cmd.Parameters.AddWithValue("@op4", dict["op4"]);
                cmd.Parameters.AddWithValue("@op5", dict["op5"]);

                cmd.Parameters.AddWithValue("@criteria0", dict["criteria0"]);
                cmd.Parameters.AddWithValue("@criteria1", dict["criteria1"]);
                cmd.Parameters.AddWithValue("@criteria2", dict["criteria2"]);
                cmd.Parameters.AddWithValue("@criteria3", dict["criteria3"]);
                cmd.Parameters.AddWithValue("@criteria4", dict["criteria4"]);
                cmd.Parameters.AddWithValue("@criteria5", dict["criteria5"]);

                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                apps.recordCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    LeaveApplication app = new LeaveApplication();
                    app.appID = Convert.ToString(dr[nameof(app.appID)]);
                    app.userID = Convert.ToString(dr[nameof(app.userID)]);
                    app.userName = Convert.ToString(dr[nameof(app.userName)]);
                    app.deptCode = Convert.ToString(dr[nameof(app.deptCode)]);
                    app.deptName = Convert.ToString(dr[nameof(app.deptName)]);
                    app.rankCode = Convert.ToString(dr[nameof(app.rankCode)]);
                    app.rankDescription = Convert.ToString(dr[nameof(app.rankDescription)]);
                    app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                    app.reasonDetail = Convert.ToString(dr[nameof(app.reasonDetail)]);
                    app.applicantDesc = Convert.ToString(dr[nameof(app.applicantDesc)]);
                    app.timeStart = Convert.ToString(dr[nameof(app.timeStart)]);
                    app.timeEnd = Convert.ToString(dr[nameof(app.timeEnd)]);
                    app.hourRequired = Convert.ToString(dr[nameof(app.hourRequired)]);
                    app.systemStatus = Convert.ToString(dr[nameof(app.systemStatus)]);
                    app.approverAction = Convert.ToString(dr[nameof(app.approverAction)]);
                    app.approverUserID = Convert.ToString(dr[nameof(app.approverUserID)]);
                    app.approverDesc = Convert.ToString(dr[nameof(app.approverDesc)]);
                    app.applicationProgress = Convert.ToString(dr[nameof(app.applicationProgress)]);
                    app.validationStatus = Convert.ToString(dr[nameof(app.validationStatus)]);

                    list.Add(app);
                }
                apps.records = list;
            }
            return apps;
        }

        public AllLeaveApps showAllMyRequest(int pageIndex, string userID)
        {
            int pageSize = GlobalObject.pageSize();
            AllLeaveApps apps = new AllLeaveApps();
            List<LeaveApplication> list = new List<LeaveApplication>();
            apps.pageSize = pageSize;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spListMyApplication"
                };

                cmd.Parameters.AddWithValue("@pageSize", apps.pageSize);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@userID", userID);


                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                apps.recordCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    LeaveApplication app = new LeaveApplication();
                    app.appID = Convert.ToString(dr[nameof(app.appID)]);
                    app.userID = Convert.ToString(dr[nameof(app.userID)]);
                    app.userName = Convert.ToString(dr[nameof(app.userName)]);
                    app.deptCode = Convert.ToString(dr[nameof(app.deptCode)]);
                    app.deptName = Convert.ToString(dr[nameof(app.deptName)]);
                    app.rankCode = Convert.ToString(dr[nameof(app.rankCode)]);
                    app.rankDescription = Convert.ToString(dr[nameof(app.rankDescription)]);
                    app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                    app.reasonDetail = Convert.ToString(dr[nameof(app.reasonDetail)]);
                    app.applicantDesc = Convert.ToString(dr[nameof(app.applicantDesc)]);
                    app.timeStart = Convert.ToString(dr[nameof(app.timeStart)]);
                    app.timeEnd = Convert.ToString(dr[nameof(app.timeEnd)]);
                    app.hourRequired = Convert.ToString(dr[nameof(app.hourRequired)]);
                    app.systemStatus = Convert.ToString(dr[nameof(app.systemStatus)]);
                    app.approverAction = Convert.ToString(dr[nameof(app.approverAction)]);
                    app.approverUserID = Convert.ToString(dr[nameof(app.approverUserID)]);
                    app.approverDesc = Convert.ToString(dr[nameof(app.approverDesc)]);
                    app.applicationProgress = Convert.ToString(dr[nameof(app.applicationProgress)]);
                    app.validationStatus = Convert.ToString(dr[nameof(app.validationStatus)]);

                    list.Add(app);
                }
                apps.records = list;
            }
            return apps;
        }

        public AllLeaveApps showAllMyRequest_2(int pageIndex, string userID)
        {
            int pageSize = -1;
            AllLeaveApps apps = new AllLeaveApps();
            List<LeaveApplication> list = new List<LeaveApplication>();
            apps.pageSize = pageSize;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spListMyApplicationForDownload"
                };

                cmd.Parameters.AddWithValue("@pageSize", apps.pageSize);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);
                cmd.Parameters.AddWithValue("@userID", userID);


                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                apps.recordCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    LeaveApplication app = new LeaveApplication();
                    app.appID = Convert.ToString(dr[nameof(app.appID)]);
                    app.userID = Convert.ToString(dr[nameof(app.userID)]);
                    app.userName = Convert.ToString(dr[nameof(app.userName)]);
                    app.deptCode = Convert.ToString(dr[nameof(app.deptCode)]);
                    app.deptName = Convert.ToString(dr[nameof(app.deptName)]);
                    app.rankCode = Convert.ToString(dr[nameof(app.rankCode)]);
                    app.rankDescription = Convert.ToString(dr[nameof(app.rankDescription)]);
                    app.reasonCode = Convert.ToString(dr[nameof(app.reasonCode)]);
                    app.reasonDetail = Convert.ToString(dr[nameof(app.reasonDetail)]);
                    app.applicantDesc = Convert.ToString(dr[nameof(app.applicantDesc)]);
                    app.timeStart = Convert.ToString(dr[nameof(app.timeStart)]);
                    app.timeEnd = Convert.ToString(dr[nameof(app.timeEnd)]);
                    app.hourRequired = Convert.ToString(dr[nameof(app.hourRequired)]);
                    app.systemStatus = Convert.ToString(dr[nameof(app.systemStatus)]);
                    app.approverAction = Convert.ToString(dr[nameof(app.approverAction)]);
                    app.approverUserID = Convert.ToString(dr[nameof(app.approverUserID)]);
                    app.approverDesc = Convert.ToString(dr[nameof(app.approverDesc)]);
                    app.applicationProgress = Convert.ToString(dr[nameof(app.applicationProgress)]);
                    app.validationStatus = Convert.ToString(dr[nameof(app.validationStatus)]);

                    list.Add(app);
                }
                apps.records = list;
            }
            return apps;
        }

    }
}