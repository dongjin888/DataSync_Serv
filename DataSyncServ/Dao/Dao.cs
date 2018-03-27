using System;
using System.Collections.Generic;

using System.Text;


namespace DataSyncServ.Dao
{
    class Dao
    {
    }

    public class DaoDepartment
    {
        public int id;
        public string name;
        public string info;
        public DaoDepartment()
        {
            name = "";
            info = "";
        }
    }

    public class DaoPlatform
    {
        public int id;
        public string name;
        public string info;
        public DaoPlatform()
        {
            name = "";
            info = "";
        }
    }

    public class DaoProduct
    {
        public int id;
        public string pltfm;
        public string name;
        public string info;

        public DaoProduct()
        {
            pltfm = "";
            name = "";
            info = "";
        }
    }

    public class DaoTeam
    {
        public int id;
        public string name;
        public string depart;
        public string info;
        public DaoTeam()
        {
            name = "";
            depart = "";
            info = "";
        }
    }

    public class DaoUser
    {
        public int id;
        public string uid;
        public string name;
        public string team;
        public string tel;
        public string level;
        public string info;
        public string pass;
        public DaoUser()
        {
            uid = "";
            name = "";
            team = "";
            tel = "";
            level = "";
            info = "";
            pass = "";
        }
    }

    public class DaoLevel
    {
        public int id;
        public string name;
        public string info;
        public DaoLevel()
        {
            name = "";
            info = "";
        }
    }
    public class DaoTrial
    {
        public int id;
        public string pltfm;
        public string pdct;
        public string activator;
        public string date;
        public string path;
        public string dbgPath;
        public string info;
        public string operater;
        public DaoTrial()
        {
            pltfm = "";
            pdct = "";
            activator = "";
            date = "";
            path = "";
            dbgPath = "";
            info = "";
            operater = "";
        }
    }


}
