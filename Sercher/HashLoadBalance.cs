using NGenerics.DataStructures.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

//using System.Linq;
/// <summary>
/// 用于解决集合过多和分布式问题
/// </summary>
namespace Sercher
{
   public class ServerDB
    {
        string ip;
        int worldCount;
        string dbName;
        bool isOkey;//要是删除重复映射的时候词太多太占内存可以用这个字段和带过滤的findbigger方法实时更新可用映射
        public string Ip { get => ip; set => ip = value; }
        public int WorldCount { get => worldCount; set => worldCount = value; }
        public string DbName { get => dbName; set => dbName = value; }
    }

    public class ConsistentHashLoadBalance
    {
        private int serverDBTotalNum;
        public ConsistentHashLoadBalance()
        {
            List<ServerDB> serverlists = new List<ServerDB>();
            serverlists.Add(
                     new ServerDB {Ip="localhost",DbName="SercherIndexDatabaseA" }
                    );
            serverlists.Add(
                     new ServerDB {Ip="localhost",DbName="SercherIndexDatabaseB" }
                    );
            serverlists.Add(
                     new ServerDB {Ip="localhost",DbName="SercherIndexDatabaseC" }
                    );
            serverlists.Add(
                     new ServerDB {Ip="localhost",DbName="SercherIndexDatabaseD" }
                    );
            serverlists.Add(
                     new ServerDB {Ip="localhost",DbName="SercherIndexDatabaseE" }
                    );
            serverlists.Add(
         new ServerDB { Ip = "localhost", DbName = "SercherIndexDatabaseF" }
        );
            serverlists.Add(
         new ServerDB { Ip = "localhost", DbName = "SercherIndexDatabaseG" }
        );
            serverlists.Add(
         new ServerDB { Ip = "localhost", DbName = "SercherIndexDatabaseH" }
        );
            serverlists.Add(
         new ServerDB { Ip = "localhost", DbName = "SercherIndexDatabaseI" }
        );
            serverlists.Add(
         new ServerDB { Ip = "localhost", DbName = "SercherIndexDatabaseJ" }
        );
            serverlists.Add(
         new ServerDB { Ip = "localhost", DbName = "SercherIndexDatabaseK" }
        );

            hashTreeMap.Init(serverlists);

            ///remark:
            ///serverlists数组的每一项对应一个hash映射
            ///每次映射得到一个db的列表
            ///运行索引时会选择列表中最小负载的db进行
            ///【因为排序是在单个服务器上进行，这导致在单个服务上器数据量小的时候，相关性排序很糟糕】
            ///【所以像上面的代码都注释掉了，留下一个。
            ///  另外如果只是关于数据的均衡负载，可以使用mongodb的官方实现】
        }

        HashTreeMap<ServerDB> hashTreeMap = new HashTreeMap<ServerDB>();

        public int ServerDBTotalNum { get => hashTreeMap.Count(); }


        /// <summary>
        /// 根据单词获取hash映射
        /// </summary>
        /// <param name="world">world字符串</param>
        /// <returns></returns>
        public long GetHashByWorld(string world)
        {
            var md = MD5.Create();
            var by= md.ComputeHash(Encoding.UTF8.GetBytes(world));
            long result = 0;
            for (int i = 0; i < by.Length; i += 8)
            {
                long BigNum= Math.Abs(BitConverter.ToInt64(by, i));
                result = (Math.Abs(result * 10 + BigNum)) % hashTreeMap.HashSpace;
            }
            return result;
        }

        public ServerDB FindCloseServerDBsByHash(long hash)
        {
            return hashTreeMap.Findbigger(hash).Value;
        }
        public ServerDB FindCloseServerDBsByWorld(string world)
        {
            return hashTreeMap.Findbigger(GetHashByWorld(world)).Value;
        }
        /// <summary>
        /// 统计各个数据库的集合数
        /// </summary>
        public void SetServerDBCount()
        {
            HashSet<string> vs = new HashSet<string>();

            foreach (var serverdb in hashTreeMap)
            {//遍历hash键值对，其值是映射列表
                var m = serverdb.Value;
                    if (vs.Contains(m.Ip + m.DbName)) continue;
                    m.WorldCount = Helper.GetSercherIndexCollectionCount(m.Ip,m.DbName);
                    vs.Add(m.Ip + m.DbName);
            }
        }
        /// <summary>
        /// 加入一个hash映射（纵向扩展，分布集合）
        /// </summary>
        /// <param name="serverDB"></param>
        public void AddHashMap(ServerDB serverDB,bool ReSetServerDBCount=true)
        {
            //寻找集合最多的数据库的逆时针方向，增加一个节点
            if(ReSetServerDBCount)
                SetServerDBCount();
            long loadMaxhash = hashTreeMap
                 .OrderByDescending(x => x.Value.WorldCount).First().Key;
            long loadMinhash = hashTreeMap.FindSimler(loadMaxhash).Key;
            long curNodeHash = (loadMaxhash - loadMinhash) / 2;

            //数据迁移
            var loadMaxDB = hashTreeMap[loadMaxhash];
            HashSet<string> worldList=new HashSet<string>();
            loadMaxDB.GetSercherIndexCollectionNameList()
                .ForEach(y => { if (!worldList.Contains(y)) worldList.Add(y); });
            var waitDelList = new List<string>();
            foreach(var x in worldList)
            {
                if (curNodeHash > GetHashByWorld(x))
                {
                    var destdb = serverDB;
                    destdb.WorldCount++;
                    loadMaxDB.CopyCollection(x, destdb);
                    waitDelList.Add(x);
                }
            };

            //加入
            hashTreeMap[curNodeHash] = serverDB;

            //删除重映射集合
            waitDelList.ForEach(collection => loadMaxDB.DelCollectionAsync(collection));
            
        }

        /// <summary>
        /// 在一组配置的服务器组中平衡数据量
        /// </summary>
        /// <returns></returns>
        public int MakeBalance()
        {
            throw new NotImplementedException();
        }
        
        public void RemoveAllDBData()
        {
            var g = this.hashTreeMap.GetOrderedEnumerator();
            while (g.MoveNext())
            {
                g.Current.Value.DeleDb();
            }

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
///NOTE:一般来说虚拟节点需要配置成真实节点的10倍以上才能打到负载均衡
///