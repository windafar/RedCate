using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeripheralTool
{
   public class FileSys
    {
        /// <summary>
                /// 遍历 rootdir目录下的所有文件
                /// </summary>
                /// <param name="rootdir">目录名称</param>
                /// <returns>该目录下的所有文件</returns>
       public static  List<string> GetAllFiles(string rootdir)
        {//方法有问题
            List<string> result = new List<string>();
            GetAllFiles(rootdir, result);
            return result;
        }
        /// <summary>
        /// 作为遍历文件函数的子函数
        /// </summary>
        /// <param name="parentDir">目录名称</param>
        /// <param name="result">该目录下的所有文件</param>
        static void  GetAllFiles(string parentDir, List<string> result)
        {
            //获取目录parentDir下的所有的子文件夹
            string[] dir = Directory.GetDirectories(parentDir);
            for (int i = 0; i < dir.Length; i++)
                GetAllFiles(dir[i], result);

            //获取目录parentDir下的所有的文件，并过滤得到所有的文本文件
            string[] file = Directory.GetFiles(parentDir,"*.txt");
            for (int i = 0; i < file.Length; i++)
            {
                //FileInfo fi = new FileInfo(file[i]);
                //if (fi.Extension.ToLower() == "txt")
                //{
                result.Add(file[i]);
                //}                
            }
        }
    }
}
