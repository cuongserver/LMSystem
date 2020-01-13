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

namespace LMSystem.Models
{
    public class UserIdentity
    {
        [Required(ErrorMessageResourceName = "Account_UsernameErr", ErrorMessageResourceType = typeof(RegionSetting))]
        public string userID { get; set; }

        [Required(ErrorMessageResourceName = "Account_PasswordErr", ErrorMessageResourceType = typeof(RegionSetting))]
        [DataType(DataType.Password)]
        public string userPassword { get; set; }

        //dùng cho việc đổi password
        [Required(ErrorMessageResourceName = "ChangePassword_Nonblank", ErrorMessageResourceType = typeof(RegionSetting))]
        [DataType(DataType.Password)]
        [NotMapped]
        public string newPassword { get; set; }

        [Required(ErrorMessageResourceName = "ChangePassword_Nonblank", ErrorMessageResourceType = typeof(RegionSetting))]
        [DataType(DataType.Password)]
        [NotMapped]
        [Compare("newPassword",
            ErrorMessageResourceType = typeof(RegionSetting),
            ErrorMessageResourceName = "ChangePassword_NewPWNotMatched")]
        public string newPasswordConfirm { get; set; }
        //dùng cho việc đổi password (hết)


        public string role { get; set; }

    }

    public class UserIdentityAuthResult
    {
        public int validationResult { get; set; }
        public string role { get; set; }
        public string name { get; set; }
    }

    public class UserIdentityAuthProcess
    {
        string con = GlobalObject.ConnectionStr();
        public UserIdentityAuthResult Auth(UserIdentity user)
        {
            UserIdentityAuthResult result = new UserIdentityAuthResult();
            using (SqlConnection conn = new SqlConnection(con))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandText = "spUserIdentityAuth",
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@username", user.userID);
                cmd.Parameters.AddWithValue("@password", user.userPassword);

                SqlParameter valRes = cmd.Parameters.Add("@validationResult", SqlDbType.NVarChar);
                valRes.Size = 100;
                valRes.Direction = ParameterDirection.Output;

                SqlParameter role = cmd.Parameters.Add("@role", SqlDbType.NVarChar);
                role.Size = 50;
                role.Direction = ParameterDirection.Output;

                SqlParameter name = cmd.Parameters.Add("@Name", SqlDbType.NVarChar);
                name.Size = 50;
                name.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();

                result.validationResult = Convert.ToInt32(valRes.Value);
                result.role = role.Value.ToString();
                result.name = name.Value.ToString();
            }
            return result;
        }
    }

    // data access layer dùng cho việc đổi password của user
    public class UserChangePasswordProcess
    {
        string connStr = GlobalObject.ConnectionStr();
        public int Result(UserIdentity user)
        {
            int processResult;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = conn,
                    CommandText = "spChangePassword",
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@userID", user.userID);
                cmd.Parameters.AddWithValue("@oldPassword", user.userPassword);
                cmd.Parameters.AddWithValue("@newPassword", user.newPassword);

                SqlParameter result = cmd.Parameters.Add("@result", SqlDbType.Int);
                result.Direction = ParameterDirection.Output;

                conn.Open();
                cmd.ExecuteNonQuery();
                processResult = Convert.ToInt32(result.Value);
            }
            return processResult;
        }
    }
}