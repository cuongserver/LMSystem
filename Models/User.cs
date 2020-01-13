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
    public class User
    {
        [Required(ErrorMessageResourceName = "User_AddNew_Validation_null_userID", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userID), ResourceType = typeof(RegionSetting))]
        public string userID { get; set; }

        [Required(ErrorMessageResourceName = "User_AddNew_Validation_null_userName", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userName), ResourceType = typeof(RegionSetting))]
        public string userName { get; set; }

        [Required(ErrorMessageResourceName = "User_AddNew_Validation_null_passWord", ErrorMessageResourceType = typeof(RegionSetting))]
        public string userPassword { get; set; }

        [Required(ErrorMessageResourceName = "User_AddNew_Validation_null_deptCode", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_deptCode), ResourceType = typeof(RegionSetting))]
        public string deptCode { get; set; }

        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_deptCode), ResourceType = typeof(RegionSetting))]
        public string deptName { get; set; }

        [Required(ErrorMessageResourceName = "User_AddNew_Validation_null_rankCode", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_rankCode), ResourceType = typeof(RegionSetting))]
        public string rankCode { get; set; }

        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_rankCode), ResourceType = typeof(RegionSetting))]
        public string rankDescription { get; set; }

        [Required(ErrorMessageResourceName = "User_AddNew_Validation_null_userEmail", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userEmail), ResourceType = typeof(RegionSetting))]
        public string userEmail { get; set; }

        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_isuserActive), ResourceType = typeof(RegionSetting))]
        public bool userIsActive { get; set; }

        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userFailedLoginAttempt), ResourceType = typeof(RegionSetting))]
        public int userFailedLoginAttempt { get; set; }
    }

    public class Department
    {
        public string deptCode { get; set; }
        public string deptName { get; set; }
    }

    public class Rank
    {
        public string rankCode { get; set; }
        public string rankDescription { get; set; }
    }

    public class UserCollection
    {
        public int pageSize { get; set; }
        public int recordCount { get; set; }
        public List<User> userRecords { get; set; }
    }

    public class UserDataAccessLayer
    {
        string connStr = GlobalObject.ConnectionStr();
        public List<Department> getDeptList()
        {
            List<Department> list = new List<Department>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText = "select * from deptList order by deptCode"
                };
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Department dept = new Department()
                    {
                        deptCode = rdr[0].ToString(),
                        deptName = rdr[1].ToString()
                    };
                    list.Add(dept);
                }
            }
            return list;
        }
        public List<Rank> getRankList()
        {
            List<Rank> list = new List<Rank>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.Text,
                    CommandText = "select * from rankList where rankCode <> 'Admin' order by rankCode"
                };
                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Rank rank = new Rank()
                    {
                        rankCode = rdr[0].ToString(),
                        rankDescription = rdr[1].ToString()
                    };
                    list.Add(rank);
                }
            }
            return list;
        }
        public List<User> getUserList()
        {
            List<User> list = new List<User>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spListAllUsers"
                };
                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    User user = new User();
                    user.userID = Convert.ToString(dr[nameof(user.userID)]);
                    user.userName = Convert.ToString(dr[nameof(user.userName)]);
                    user.deptCode = Convert.ToString(dr[nameof(user.deptCode)]);
                    user.deptName = Convert.ToString(dr[nameof(user.deptName)]);

                    user.rankCode = Convert.ToString(dr[nameof(user.rankCode)]);
                    user.rankDescription = Convert.ToString(dr[nameof(user.rankDescription)]);
                    user.userEmail = Convert.ToString(dr[nameof(user.userEmail)]);
                    user.userIsActive = Convert.ToBoolean(dr[nameof(user.userIsActive)]);
                    user.userFailedLoginAttempt = Convert.ToInt32(dr[nameof(user.userFailedLoginAttempt)]);
                    list.Add(user);
                }
            }
            return list;
        }

        public List<User> GetActiveUserList()
        {
            List<User> list = new List<User>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "ListActiveUser"
                };
                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    User user = new User();
                    user.userID = Convert.ToString(dr[nameof(user.userID)]);
                    user.userName = Convert.ToString(dr[nameof(user.userName)]);
                    user.deptCode = Convert.ToString(dr[nameof(user.deptCode)]);
                    user.deptName = Convert.ToString(dr[nameof(user.deptName)]);

                    user.rankCode = Convert.ToString(dr[nameof(user.rankCode)]);
                    user.rankDescription = Convert.ToString(dr[nameof(user.rankDescription)]);
                    user.userEmail = Convert.ToString(dr[nameof(user.userEmail)]);
                    user.userIsActive = Convert.ToBoolean(dr[nameof(user.userIsActive)]);
                    user.userFailedLoginAttempt = Convert.ToInt32(dr[nameof(user.userFailedLoginAttempt)]);
                    list.Add(user);
                }
            }
            return list;
        }
        public UserCollection getUserListWithPagination(int pageIndex)
        {
            int pageSize = GlobalObject.pageSize();
            UserCollection collection = new UserCollection();
            collection.pageSize = pageSize;
            List<User> list = new List<User>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spListAllUsersWithPagination"
                };

                cmd.Parameters.AddWithValue("@pageSize", collection.pageSize);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);

                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                collection.recordCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    User user = new User();
                    user.userID = Convert.ToString(dr[nameof(user.userID)]);
                    user.userName = Convert.ToString(dr[nameof(user.userName)]);
                    user.deptCode = Convert.ToString(dr[nameof(user.deptCode)]);
                    user.deptName = Convert.ToString(dr[nameof(user.deptName)]);

                    user.rankCode = Convert.ToString(dr[nameof(user.rankCode)]);
                    user.rankDescription = Convert.ToString(dr[nameof(user.rankDescription)]);
                    user.userEmail = Convert.ToString(dr[nameof(user.userEmail)]);
                    user.userIsActive = Convert.ToBoolean(dr[nameof(user.userIsActive)]);
                    user.userFailedLoginAttempt = Convert.ToInt32(dr[nameof(user.userFailedLoginAttempt)]);
                    list.Add(user);
                }
                collection.userRecords = list;
            }
            return collection;

        }
        public int createNewUser(User model)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spCreateNewUser"
                };
                cmd.Parameters.AddWithValue("@userID", model.userID);
                cmd.Parameters.AddWithValue("@userName", model.userName);
                cmd.Parameters.AddWithValue("@userPassword", model.userPassword);
                cmd.Parameters.AddWithValue("@deptCode", model.deptCode);
                cmd.Parameters.AddWithValue("@rankCode", model.rankCode);
                cmd.Parameters.AddWithValue("@userEmail", model.userEmail);

                SqlParameter procResult = cmd.Parameters.Add("@procResult", SqlDbType.Int);
                procResult.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(procResult.Value);
            }
        }
        public int editUserInfo(User model)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spEditUserInfo"
                };
                cmd.Parameters.AddWithValue("@userID", model.userID);
                cmd.Parameters.AddWithValue("@userName", model.userName);
                cmd.Parameters.AddWithValue("@deptCode", model.deptCode);
                cmd.Parameters.AddWithValue("@rankCode", model.rankCode);
                cmd.Parameters.AddWithValue("@userEmail", model.userEmail);
                cmd.Parameters.AddWithValue("@userIsActive", model.userIsActive);
                cmd.Parameters.AddWithValue("@userFailedLoginAttempt", model.userFailedLoginAttempt);

                SqlParameter procResult = cmd.Parameters.Add("@result", SqlDbType.Int);
                procResult.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(procResult.Value);
            }
        }

        public int updatePwByAdmin(User model, string newPW, string adminPW, string adminUSer)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "[spUpdatePasswordByAdmin]"
                };
                cmd.Parameters.AddWithValue("@targetAccount", model.userID);
                cmd.Parameters.AddWithValue("@adminUser", adminUSer);
                cmd.Parameters.AddWithValue("@adminPassword", adminPW);
                cmd.Parameters.AddWithValue("@newPassword", newPW);

                SqlParameter procResult = cmd.Parameters.Add("@result", SqlDbType.Int);
                procResult.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(procResult.Value);
            }
        }
    }
    //leave quota section
    public class UserLeaveQuota
    {
        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userID), ResourceType = typeof(RegionSetting))]
        public string userID { get; set; }

        [Display(Name = nameof(RegionSetting.User_AddNew_fieldCaption_userName), ResourceType = typeof(RegionSetting))]
        public string userName { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlyANNL), ResourceType = typeof(RegionSetting))]
        public string yearlyANNL { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlyBRML), ResourceType = typeof(RegionSetting))]
        public string yearlyBRML { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlyCPSL), ResourceType = typeof(RegionSetting))]
        public string yearlyCPSL { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlyFMRL), ResourceType = typeof(RegionSetting))]
        public string yearlyFMRL { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlySMRL), ResourceType = typeof(RegionSetting))]
        public string yearlySMRL { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlySCKL), ResourceType = typeof(RegionSetting))]
        public string yearlySCKL { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlyUPDL), ResourceType = typeof(RegionSetting))]
        public string yearlyUPDL { get; set; }

        [Required(ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [RegularExpression(@"^([1-9][0-9]*$)|(Unlimited$)",
                ErrorMessageResourceName = "Quota_Regex", ErrorMessageResourceType = typeof(RegionSetting))]
        [Display(Name = nameof(RegionSetting.Caption_yearlySPKL), ResourceType = typeof(RegionSetting))]
        public string yearlySPCL { get; set; }
    }

    public class LeaveQuotaSummary
    {
        public int pageSize { get; set; }
        public int recordCount { get; set; }
        public List<UserLeaveQuota> records { get; set; }
    }

    public class UserLeaveQuota_DataAccessLayer
    {
        string connStr = GlobalObject.ConnectionStr();
        public LeaveQuotaSummary quotaSummary(int pageIndex)
        {
            int pageSize = GlobalObject.pageSize();
            LeaveQuotaSummary summary = new LeaveQuotaSummary();
            summary.pageSize = pageSize;
            List<UserLeaveQuota> list = new List<UserLeaveQuota>();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spLeaveQuotaSummary"
                };

                cmd.Parameters.AddWithValue("@pageSize", summary.pageSize);
                cmd.Parameters.AddWithValue("@pageIndex", pageIndex);

                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.BigInt);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                summary.recordCount = Convert.ToInt32(prm.Value);
                con.Close();
                da.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    UserLeaveQuota quota = new UserLeaveQuota();
                    quota.userID = Convert.ToString(dr[nameof(quota.userID)]);
                    quota.userName = Convert.ToString(dr[nameof(quota.userName)]);
                    quota.yearlyANNL = Convert.ToString(dr[nameof(quota.yearlyANNL)]);
                    quota.yearlyBRML = Convert.ToString(dr[nameof(quota.yearlyBRML)]);

                    quota.yearlyCPSL = Convert.ToString(dr[nameof(quota.yearlyCPSL)]);
                    quota.yearlyFMRL = Convert.ToString(dr[nameof(quota.yearlyFMRL)]);
                    quota.yearlySCKL = Convert.ToString(dr[nameof(quota.yearlySCKL)]);
                    quota.yearlySMRL = Convert.ToString(dr[nameof(quota.yearlySMRL)]);
                    quota.yearlySPCL = Convert.ToString(dr[nameof(quota.yearlySPCL)]);
                    quota.yearlyUPDL = Convert.ToString(dr[nameof(quota.yearlyUPDL)]);
                    list.Add(quota);
                }
                summary.records = list;
            }
            return summary;

        }
        public UserLeaveQuota quotaDetail(string userID)
        {
            UserLeaveQuota quota = new UserLeaveQuota();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spLeaveQuotaDetail"
                };

                cmd.Parameters.AddWithValue("@userID", userID);
                SqlParameter prm = cmd.Parameters.Add("@recordCount", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                con.Close();
                da.Dispose();
                if (Convert.ToInt32(prm.Value) > 0)
                {
                    DataRow dr = dt.Rows[0];
                    quota.userID = Convert.ToString(dr[nameof(quota.userID)]);
                    quota.userName = Convert.ToString(dr[nameof(quota.userName)]);
                    quota.yearlyANNL = Convert.ToString(dr[nameof(quota.yearlyANNL)]);
                    quota.yearlyBRML = Convert.ToString(dr[nameof(quota.yearlyBRML)]);
                    quota.yearlyCPSL = Convert.ToString(dr[nameof(quota.yearlyCPSL)]);
                    quota.yearlyFMRL = Convert.ToString(dr[nameof(quota.yearlyFMRL)]);
                    quota.yearlySCKL = Convert.ToString(dr[nameof(quota.yearlySCKL)]);
                    quota.yearlySMRL = Convert.ToString(dr[nameof(quota.yearlySMRL)]);
                    quota.yearlySPCL = Convert.ToString(dr[nameof(quota.yearlySPCL)]);
                    quota.yearlyUPDL = Convert.ToString(dr[nameof(quota.yearlyUPDL)]);
                }
            }
            return quota;
        }

        public int quotaUpdate(UserLeaveQuota model)
        {
            int result;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "spUpdateLeaveQuota"
                };

                cmd.Parameters.AddWithValue("@userID", model.userID);
                cmd.Parameters.AddWithValue("@yearlyANNL", model.yearlyANNL);
                cmd.Parameters.AddWithValue("@yearlyBRML", model.yearlyBRML);
                cmd.Parameters.AddWithValue("@yearlyCPSL", model.yearlyCPSL);
                cmd.Parameters.AddWithValue("@yearlyFMRL", model.yearlyFMRL);
                cmd.Parameters.AddWithValue("@yearlySCKL", model.yearlySCKL);
                cmd.Parameters.AddWithValue("@yearlySMRL", model.yearlySMRL);
                cmd.Parameters.AddWithValue("@yearlySPCL", model.yearlySPCL);
                cmd.Parameters.AddWithValue("@yearlyUPDL", model.yearlyUPDL);


                SqlParameter prm = cmd.Parameters.Add("@result", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                result = Convert.ToInt32(prm.Value);
            }
            return result;
        }
    }

}