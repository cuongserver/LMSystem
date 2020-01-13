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
    public class PublicHoliday
    {
        public string holiday { get; set; }
        public string description { get; set; }
    }

    public static class SystemSettingDAL
    {
        public static int InsertNewHoliday(PublicHoliday model)
        {
            int result;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "InsertPublicHoliday"
                };

                cmd.Parameters.AddWithValue("@holiday", model.holiday);
                cmd.Parameters.AddWithValue("@description", model.description ?? string.Empty);

                SqlParameter prm = cmd.Parameters.Add("@result", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                result = Convert.ToInt32(prm.Value);
                con.Close();
            }
            return result;
        }
        public static List<PublicHoliday> GetPublicHolidays()
        {
            List<PublicHoliday> list = new List<PublicHoliday>();
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
                    PublicHoliday publicHoliday = new PublicHoliday();
                    publicHoliday.holiday = dr[nameof(publicHoliday.holiday)].ToString();
                    publicHoliday.description = dr[nameof(publicHoliday.description)].ToString();
                    list.Add(publicHoliday);
                }
            }
            return list;
        }
        public static int DisablePublicHoliday(string holiday)
        {
            int result;
            string connStr = GlobalObject.ConnectionStr();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "DisablePublicHoliday"
                };

                cmd.Parameters.AddWithValue("@holiday", holiday);

                SqlParameter prm = cmd.Parameters.Add("@result", SqlDbType.Int);
                prm.Direction = ParameterDirection.Output;

                con.Open();
                cmd.ExecuteNonQuery();
                result = Convert.ToInt32(prm.Value);
                con.Close();
            }
            return result;
        }
    }
}