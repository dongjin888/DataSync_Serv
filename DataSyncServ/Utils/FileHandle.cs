using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataSyncServ.Utils
{
    class FileHandle
    {
        //遍历所有的目录名
        public static void traceFolder(DirectoryInfo root,List<DirectoryInfo> sonFolder)
        {
            if (root.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo d in root.GetDirectories())
                {
                    traceFolder(d,sonFolder);
                }
            }
            else
            {
                sonFolder.Add(root);
            }
        }


        //遍历所有文件
        public static void traceAllFile(DirectoryInfo root, List<FileInfo> files)
        {
            if (root.GetFiles().Length > 0)
            {
                foreach (FileInfo f in root.GetFiles())
                {
                    files.Add(f);
                }
            }

            if (root.GetDirectories().Length > 0)
            {
                foreach (DirectoryInfo d in root.GetDirectories())
                {
                    traceAllFile(d, files);
                }
            }
        }
    }
}
