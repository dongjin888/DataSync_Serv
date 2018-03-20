using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using DataSyncServ.Dao;
using DataSyncServ.Utils;

namespace DataSyncServ.Utils
{
    public class DataService
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
                            path[0] = reader.GetString(0); // trial/
                            path[1] = reader.GetString(1); // trial/debug/ 
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

        #region 插入一条trial 记录
        public void insertTrial(TrialInfo trialInfo)
        {
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    string queryStr = @"insert into tabTrials values(null,@pltfm,@pdct,@actor,@date,
                                        @path,@bugPath,@info,@operat);";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;
                    cmd.Parameters.AddWithValue("@pltfm", trialInfo.Pltfm);
                    cmd.Parameters.AddWithValue("@pdct", trialInfo.Pdct);
                    cmd.Parameters.AddWithValue("@actor", trialInfo.Activator);
                    cmd.Parameters.AddWithValue("@date", trialInfo.Unique.Split('_')[1]);
                    string path = ContantInfo.Fs.path + trialInfo.Pltfm + "\\" + trialInfo.Pdct + "\\" +
                                  trialInfo.Unique + "\\";
                    cmd.Parameters.AddWithValue("@path", path);
                    cmd.Parameters.AddWithValue("@bugPath", path + "debug\\");
                    cmd.Parameters.AddWithValue("@info", trialInfo.Info);
                    cmd.Parameters.AddWithValue("@operat", trialInfo.Operator);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("插入Trial记录ok!");
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("执行插入trial记录时异常！" + ex.Message);
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception("数据插入异常!" + ex.Message);
                }
            }
        }
        #endregion

        //获取所有department 记录
        public List<DaoDepartment> getAllDepartments()
        {
            List<DaoDepartment> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<DaoDepartment>();

                    string queryStr = @"select * from tabdepartments;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DaoDepartment one = new DaoDepartment();
                            one.id = reader.GetInt16(0);
                            one.name = reader.GetString(1);
                            one.info = reader.GetString(2);
                            list.Add(one);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("getDepartment Error!\n" + ex.Message);
                }
            }
            return list;
        }

        //获取所有 platform 记录
        public List<DaoPlatform> getAllPltfms()
        {
            List<DaoPlatform> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<DaoPlatform>();

                    string queryStr = @"select * from tabplatforms;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DaoPlatform one = new DaoPlatform();
                            one.id = reader.GetInt16(0);
                            one.name = reader.GetString(1);
                            one.info = reader.GetString(2);
                            list.Add(one);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getpltfm Error!\n" + ex.Message);
                }
            }
            return list;
        }

        //获取所有product 记录
        public List<DaoProduct> getAllpdcts()
        {
            List<DaoProduct> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<DaoProduct>();

                    string queryStr = @"select * from tabproducts;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DaoProduct one = new DaoProduct();
                            one.id = reader.GetInt16(0);
                            one.pltfm = reader.GetString(1);
                            one.name = reader.GetString(2);
                            one.info = reader.GetString(3);
                            list.Add(one);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getProduct Error!\n" + ex.Message);
                }
            }
            return list;
        }

        //获取所有team 记录
        public List<DaoTeam> getAllTeams()
        {
            List<DaoTeam> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<DaoTeam>();

                    string queryStr = @"select * from tabteams;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DaoTeam one = new DaoTeam();
                            one.id = reader.GetInt16(0);
                            one.name = reader.GetString(1);
                            one.depart = reader.GetString(2);
                            one.info = reader.GetString(3);
                            list.Add(one);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getTeam Error!\n" + ex.Message);
                }
            }
            return list;
        }

        //获取所有user记录
        public List<DaoUser> getAllUsers()
        {
            List<DaoUser> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<DaoUser>();

                    string queryStr = @"select id,userid,username,teamname,usertel,userlevel,
                                               userinfo from tabusers;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DaoUser one = new DaoUser();
                            one.id = reader.GetInt16(0);
                            one.uid = reader.GetString(1);
                            one.name = reader.GetString(2);
                            one.team = reader.GetString(3);
                            one.tel = reader.GetString(4);
                            one.level = reader.GetString(5);
                            one.info = reader.GetString(6);
                            list.Add(one);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("getUser Error!\n"+ex.Message);
                }
            }
            return list;
        }

        //获取所有level 记录
        public List<DaoLevel> getAllLevels()
        {
            List<DaoLevel> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<DaoLevel>();

                    string queryStr = @"select * from tabuserlevels;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DaoLevel one = new DaoLevel();
                            one.id = reader.GetInt16(0);
                            one.name = reader.GetString(1);
                            one.info = reader.GetString(2);
                            list.Add(one);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getLevel Error!\n" + ex.Message);
                }
            }
            return list;
        }
        //获取所有trial 记录
        public List<DaoTrial> getAllTrials()
        {
            List<DaoTrial> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<DaoTrial>();

                    string queryStr = @"select id,trialPltfmName,trialPdctName,trialUserId,trialDate,
                                               trialSummaryPath,trialInfo,trialOperator from tabtrials;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DaoTrial one = new DaoTrial();
                            one.id = reader.GetInt32(0);
                            one.pltfm = reader.GetString(1);
                            one.pdct = reader.GetString(2);
                            one.activator = reader.GetString(3);
                            one.date = reader.GetString(4);
                            one.dbgPath = reader.GetString(5);
                            one.info = reader.GetString(6);
                            one.operater = reader.GetString(7);
                            list.Add(one);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getTrial Error!\n" + ex.Message);
                }
            }
            return list;
        }
        
        
        //获取department string
        public List<string> getDepartStr()
        {
            List<string> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<string>();

                    string queryStr = @"select departmentname from tabdepartments;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getDepartStr Error!\n" + ex.Message);
                }
            }
            return list;
        }
        //获取team string
        public List<string> getTeamStr(string depart)
        {
            List<string> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<string>();

                    string queryStr = @"select teamname from tabteams 
                                     where departname=@depart;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;
                    cmd.Parameters.AddWithValue("@depart", depart);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getLevelStr Error!\n" + ex.Message);
                }
            }
            return list;
        }
        //获取level string
        public List<string> getLevelStr()
        {
            List<string> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<string>();

                    string queryStr = @"select levelname from tabuserlevels;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getLevelStr Error!\n" + ex.Message);
                }
            }
            return list;
        }
        //获取 pltfm string
        public List<string> getPltfmStr()
        {
            List<string> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<string>();

                    string queryStr = @"select pltfmName from tabplatforms;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getPltfmStr Error!\n" + ex.Message);
                }
            }
            return list;
        }
        //获取 pdct string 
        public List<string> getPdctStr(string pltfm)
        {
            List<string> list = null;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    list = new List<string>();

                    string queryStr = @"select pdctName from tabproducts 
                                         where pltfmName=@pltfm ;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = queryStr;
                    cmd.Parameters.AddWithValue("@pltfm", pltfm);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader.GetString(0));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("getLevelStr Error!\n" + ex.Message);
                }
            }
            return list;
        }


        //删除
        public bool delete(string colName,int colVal,string table)
        {
            bool ret = false;

            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    string delStr = @"delete from "+table+" where "+colName+"=@colVal;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = delStr;
                    cmd.Parameters.AddWithValue("@colVal", colVal);

                    Console.WriteLine("delete string:" + delStr);

                    try
                    {
                        int res = cmd.ExecuteNonQuery();
                        ret = true;
                        Console.WriteLine("delete from tablle:" + table);

                    }catch(Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("delete Error!\n" + ex.Message);
                }
            }
            return ret;
        }

        //添加
        public bool add(Dictionary<string,string> maps,string table)
        {
            bool ret = false;
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    string addStr = @"insert into "+table+" values(null,";
                    int i = 0;
                    foreach(var entry in maps)
                    {
                        addStr += "@" + entry.Key;
                        if (i != maps.Count - 1)
                            addStr += ",";
                        else
                            addStr += ");";
                        i++;
                    }
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = addStr;
                    foreach(var entry in maps)
                    {
                        cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);

                    }

                    try
                    {
                        int res = cmd.ExecuteNonQuery();
                        ret = true;
                        Console.WriteLine("insert into table:" + table);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("insert Error!\n" + ex.Message);
                }
            }
            return ret;
        }

        //验证admin
        public bool judgeAdmin(string name,string originPass)
        {
            bool ret = false;
            string md5InputStr = MyMd5.getMd5EncryptedStr(originPass);
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    string query = @"select admPass from tabadmins where admName=@name;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.Parameters.AddRange(new MySqlParameter[] { new MySqlParameter("@name", name) });

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return ret;
                        }
                        else
                        {
                            string pass = reader.GetString(0); // 取出md5 经过我的encode 后的string
                            pass = PsEnDecode.decode(pass); // 用我的decode 得到md5 string
                            if (pass.Equals(md5InputStr)) //判断数据库得到的md5string 和 输入得到的md5 string 
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

        public bool chgAdmPass(string name,string passStr)
        {
            bool ret = false;
            string md5Str = MyMd5.getMd5EncryptedStr(passStr);
            string encode = PsEnDecode.encode(md5Str);
            if (con != null)
            {
                try
                {
                    if (con.State != ConnectionState.Open) { con.Open(); }

                    string query = @"update tabadmins set admPass=@pass where admName=@name;";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@pass", encode);
                    cmd.Parameters.AddWithValue("@name", name);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        ret = true;
                    }catch(Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return ret;
        }
    }
}
