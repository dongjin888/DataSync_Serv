using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace DataSyncServ.Utils
{
    class DataService
    {
        private MySqlConnection  con = null;

        public DataService()
        {
            if(con == null)
            {
                con = new MySqlConnection(ContantInfo.Database.CONSQLSTR);
            }
        }

        public bool closeCon()
        {
            bool ret = false;
            if (con != null && con.State == ConnectionState.Open)
            {
                try
                {
                    con.Close();
                    ret = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    ret = false;
                }
            }
            return false;
        }

        #region 验证用户
        public bool checkUser(string userId,string md5EncryptedStr)
        {
            bool ret = false;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    string query = @"select userpass from tabUsers where userId=@userId;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.Parameters.AddRange(new MySqlParameter[] { new MySqlParameter("@userId", userId) });

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return ret;
                        }
                        else
                        {
                            string pass = reader.GetString(0);
                            if (pass.Equals(md5EncryptedStr))
                            {
                                ret = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return ret;
        }
        #endregion

        #region 获取trial中的路径信息
        public string[] getTrialPath(string[] unique)
        {
            string[] path = null;

            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    string queryStr = @"select trialSummaryPath,trialDebugPath from tabTrials where 
                                        trialUserId=@userid and trialDate=@date;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;
                    cmd.Parameters.AddWithValue("@userid", unique[0]);
                    cmd.Parameters.AddWithValue("@date", unique[1]);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return path;
                        }
                        else
                        {
                            path = new string[2];
                            path[0] = reader.GetString(0);
                            path[1] = reader.GetString(1);
                        }
                    }

                }
                catch
                {

                }
            }
            return path;
        }
        #endregion
    }
}
