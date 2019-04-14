using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Utils
{
    public class LogEx
    {
        public static bool ifShowScreen = false;
        public static StringBuilder buf = null;

        static LogEx()
        {
            buf = new StringBuilder("----------------- " +
                                  "exception " + DateTime.Now.ToLongTimeString()+
                                  "-------------------\r\n");
        }
        public static void log(string message)
        {
            if (!message.Contains("position"))
            {
                buf.Append(message + "\r\n");
            }
            if (ifShowScreen)
            {
                Console.WriteLine(message);
            }
        }
    }
}
