using NGenerics.DataStructures.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

//using System.Linq;

namespace Sercher
{
    /// <summary>
    /// 基于hash映射的分布式的实现
    /// </summary>
    /// <remarks>此类暂定和SercherDb类型绑在一起</remarks>
    public class ConsistentHashLoadBalance<T> : IConsistentHashLoadBalance<T>
    {
        public ConsistentHashLoadBalance(List<T> serverlists)
        {
            hashTreeMap.Init(serverlists);
        }

        HashTreeMap<T> hashTreeMap = new HashTreeMap<T>();

        public int ServerDBTotalNum { get => hashTreeMap.Count(); }

        public IEnumerable<T> GetServerNodes()
        {
            return this.hashTreeMap.Select(x => x.Value);
        }

        /// <summary>
        /// 根据单词获取hash映射
        /// </summary>
        /// <param name="world">world字符串</param>
        /// <returns></returns>
        protected long GetHashByWorld(string world)
        {
            var md = MD5.Create();
            var by = md.ComputeHash(Encoding.UTF8.GetBytes(world));
            long result = 0;
            for (int i = 0; i < by.Length; i += 8)
            {
                long BigNum = Math.Abs(BitConverter.ToInt64(by, i));
                result = (Math.Abs(result * 10 + BigNum)) % hashTreeMap.HashSpace;
            }
            return result;
        }
        public T FindCloseServerDBsByValue(string world)
        {
            return hashTreeMap.Findbigger(GetHashByWorld(world)).Value;
        }

        /// <summary>
        /// 选择最大负载的节点，在其后的区域加入一个新节点
        /// </summary>
        /// <typeparam name="Key">新的节点的类型</typeparam>
        /// <param name="serverDB">新的节点</param>
        /// <param name="MaxKeySelector">指定提取的排序字段</param>
        /// <param name="EmigrationSouceMap">待迁出数据。T是待迁出的节点，返回List为表名的集合</param>
        /// <param name="ImmigrationAction">对待每个迁出数据做出的迁入行为。x是待迁出的每个表名</param>
        /// <returns>迁移的表名集合</returns>
        public List<string> AddHashMap(T serverDB,
            Func<KeyValuePair<long, T>, long> MaxKeySelector,
            Func<T, List<string>> EmigrationSouceMap,
            Action<T,List<string>> ImmigrationAction
            )
        {
            //寻找集合最多的数据库的逆时针方向，增加一个节点
            // SetServerDBCount();
            long loadMaxhash = hashTreeMap
                 .OrderByDescending(x => MaxKeySelector).First().Key;
            long loadMinhash = hashTreeMap.FindSimler(loadMaxhash).Key;
            long curNodeHash = (loadMaxhash - loadMinhash) / 2;

            //数据迁移[因涉及非常多的稀疏表，于是采用select into后分离数据库的方式]
            var loadMaxDB = hashTreeMap[loadMaxhash];

            var EmigrationSouceMapResult = EmigrationSouceMap(loadMaxDB)
                 .Where(x => curNodeHash > GetHashByWorld(x)).ToList();

            ImmigrationAction(loadMaxDB, EmigrationSouceMapResult);

            //加入
            hashTreeMap[curNodeHash] = serverDB;

            //删除重映射集合
            //waitDelList.ForEach(collection => loadMaxDB.DelCollectionAsync(collection));
            return EmigrationSouceMapResult;
        }

        public void AddHashMap(T serverDB, Func<KeyValuePair<long, T>, long> MaxKeySelector)
        {
            long loadMaxhash = hashTreeMap
             .OrderByDescending(x => MaxKeySelector).First().Key;
            long loadMinhash = hashTreeMap.FindSimler(loadMaxhash).Key;
            long curNodeHash = (loadMaxhash - loadMinhash) / 2;
            hashTreeMap[curNodeHash] = serverDB;
        }

        public void RemoveHashMap(T serverDB, bool ReSetServerDBCount = true)
        {
            throw new NotImplementedException();
        }

    }

    class HashTreeMap<TValue>:RedBlackTree<long,TValue>
    {
        public long HashSpace {get=> long.MaxValue; }
        public void Init(List<TValue> values)
        {//初始化很多节点，使单词均匀的落到节点上
            if (values.Count() < 1) throw new InvalidOperationException("Initialize at least 1 parameters for boundary");
            var splitSpace = HashSpace / values.Count();
            int i = 0;
            while (i < values.Count())
            {
                this.Add(splitSpace * i, values[i]);
                i++;
            }
            //注意：这里的初始化范围只有最小值0，并没有终结点，其界限在findbigger方法进行处理
        }

        /// <summary>
        /// 寻找相对于k的较小值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<long, TValue> FindSimler(long key, Func<TValue, bool> filter)
        {
            var node = Tree;
            KeyValuePair<long, TValue> temp;
            while (node != null)
            {
                if (node.Data.Key > key && filter(node.Data.Value)) temp = node.Data;
                if (node.Data.Key > key) node = node.Left;
                else node = node.Right;
            }
            if (temp.Value == null) throw new KeyNotFoundException();
            return temp;
        }
        /// <summary>
        /// 寻找相对于k的较大值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<long, TValue> Findbigger(long key,Func<TValue,bool> filter)
        {
            var node = Tree;
            KeyValuePair<long, TValue> temp;
            while (node != null)
            {
                if (node.Data.Key < key&& filter(node.Data.Value)) temp = node.Data;
                if (node.Data.Key < key) node = node.Left;
                else node = node.Right;
            }
            if (temp.Value == null)
            {
                var kvnode = FindNode(0);
                if (kvnode == null) throw new KeyNotFoundException();
                else temp = kvnode.Data;
            }
            return temp;
        }
        /// <summary>
        /// 寻找相对于k的较小值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<long, TValue> FindSimler(long key)
        {
            var node = Tree;
            KeyValuePair<long, TValue> temp;
            while (node != null)
            {
                if (node.Data.Key < key) temp = node.Data;
                if (node.Data.Key < key) node = node.Left;
                else node = node.Right;
            }
            if (temp.Value == null) throw new KeyNotFoundException();
            return temp;
        }
        /// <summary>
        /// 寻找相对于k的较大值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public KeyValuePair<long, TValue> Findbigger(long key)
        {
            var node = Tree;
            KeyValuePair<long, TValue> temp;
            while (node != null)
            {
                if (node.Data.Key > key) temp = node.Data;
                if (node.Data.Key > key) node = node.Left;
                else node = node.Right;
            }
            if (temp.Value == null)
            {
                var kvnode = FindNode(0);
                if (kvnode == null) throw new KeyNotFoundException();
                else temp = kvnode.Data;
            }
            return temp;
        }

        private BinaryTree<KeyValuePair<long, TValue>> FindNode(long key)
        {
            BinaryTree<KeyValuePair<long, TValue>> left = base.Tree;
            KeyValuePair<long, TValue> pair = new KeyValuePair<long, TValue>(key, default(TValue));
            while (left != null)
            {
                int num = base.Comparer.Compare(pair, left.Data);
                if (num == 0)
                {
                    return left;
                }
                if (num < 0)
                {
                    left = left.Left;
                }
                else
                {
                    left = left.Right;
                }
            }
            return null;
        }

        public bool isEmpty()
        {
            return this.Count == 0;
        }

        public void forEach(Action<long, TValue> action)
        {
            var enumer = this.GetOrderedEnumerator();
            while (enumer.MoveNext())
            {
                action(enumer.Current.Key, enumer.Current.Value);

            };
        }

    }


}
