using System;
using System.Collections.Generic;

namespace Sercher
{
    public interface IConsistentHashLoadBalance<T>
    {
        /// <summary>
        /// 服务节点数
        /// </summary>
        int ServerDBTotalNum { get; }
        /// <summary>
        /// 选择最大负载的节点，在其后的区域加入一个新节点
        /// </summary>
        /// <param name="serverDB"></param>
        /// <param name="MaxKeySelector"></param>
        void AddHashMap(T serverDB, Func<KeyValuePair<long, T>, long> MaxKeySelector);
        /// <summary>
        /// 选择最大负载的节点，在其后的区域加入一个新节点
        /// </summary>
        /// <typeparam name="Key">新的节点的类型</typeparam>
        /// <param name="serverDB">新的节点</param>
        /// <param name="MaxKeySelector">指定提取的排序字段</param>
        /// <param name="EmigrationSouceMap">待迁出数据。T是待迁出的节点，返回List为表名的集合</param>
        /// <param name="ImmigrationAction">对迁出数据做出的迁入行为。x是待迁出的每个表名</param>
        /// <returns>迁移的表名集合</returns>
        Tuple<long,T> AddHashMap(T serverDB, Func<KeyValuePair<long, T>, long> MaxKeySelector, Func<T, List<string>> EmigrationSouceMap, Action<T, List<string>> ImmigrationAction);
        /// <summary>
        /// 根据值寻找最近的hash节点
        /// </summary>
        /// <param name="value">值</param>
        /// <returns></returns>
        T FindCloseServerDBsByTableName(string value);
        /// <summary>
        /// 获取hash环上的所有节点
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetServerNodes();
        IEnumerable<KeyValuePair<long,T>> GetHashNodes();
        /// <summary>
        /// 移除hash节点
        /// </summary>
        /// <param name="serverDB"></param>
        /// <param name="ReSetServerDBCount">是否重新计算服务节点数目</param>
        void RemoveHashMap(T serverDB);
    }
}