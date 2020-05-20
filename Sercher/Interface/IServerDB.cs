using System;

namespace Sercher
{
    public interface IServerDB
    {
        /// <summary>
        /// 数据库名词
        /// </summary>
        string DbName { get; set; }
        /// <summary>
        /// 数据库IP或者是名称
        /// </summary>
        string Ip { get; set; }
        /// <summary>
        /// 数据库状态
        /// </summary>
        int Status { get; set; }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <returns></returns>
        bool CreateDB(string FileDir = null);
        /// <summary>
        /// 删除数据库
        /// </summary>
        void DeleDb();
        /// <summary>
        /// 判断数据库是否存在
        /// </summary>
        /// <returns></returns>
        bool GetdbStatus();
        /// <summary>
        /// 获取数据库文件本地位置
        /// </summary>
        /// <returns></returns>
        Tuple<string, string> GetDbFilePath();
        /// <summary>
        /// 备份数据库到指定位置
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool BackupTo(string path);
        /// <summary>
        /// 恢复数据库到指定位置
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool RestoreFrom(string path);
    }
}