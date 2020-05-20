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
        bool CreateDB();
        /// <summary>
        /// 删除数据库
        /// </summary>
        void DeleDb();
        /// <summary>
        /// 判断数据库是否存在
        /// </summary>
        /// <returns></returns>
        bool GetdbStatus();
    }
}