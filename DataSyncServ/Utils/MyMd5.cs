using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Utils
{
    public static class MyMd5
    {
        public static string getMd5EncryptedStr(string originStr)
        {
            string ret = null;
            byte[] result = Encoding.Default.GetBytes(originStr);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            ret = BitConverter.ToString(output).Replace("-", "");
            return ret;
        }
    }
}
