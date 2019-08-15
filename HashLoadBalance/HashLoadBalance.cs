using NGenerics.DataStructures.Trees;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

//using System.Linq;
/// <summary>
/// 用于解决集合过多和分布式问题
/// </summary>
namespace HashLoadBalance
{
    class server
    {
        string ip;
        int worldCount;
        public string Ip { get => ip; set => ip = value; }
        public int WorldCount { get => worldCount; set => worldCount = value; }
    }

    public class ConsistentHashLoadBalance
    {
       public ConsistentHashLoadBalance()
        {
            serverlist[0] = new List<server>
            { new server {Ip="1.1.1.2" },new server{Ip="1.1.1.3" }};
            serverlist[1]=new List<server>
            { new server {Ip="1.1.1.2" },new server{Ip="1.1.1.4" }};
            serverlist[2] = new List<server>
            { new server {Ip="1.1.1.1" }};
            serverlist[3] = new List<server>
            { new server {Ip="1.1.1.1" }};

        }

        Dictionary<int, List<server>> serverlist = new Dictionary<int, List<server>>();

        /// <summary>
        /// 根据单词获取映射数据库
        /// </summary>
        /// <param name="world">world字符串</param>
        /// <returns></returns>
        public int GetServerByWorld(string world,int serverNum)
        {
            var md = MD5.Create();
            var by= md.ComputeHash(Encoding.UTF8.GetBytes(world));
            int result = 0;
            for (int i = 0; i < by.Length; i += 4)
            {
                int BigNum= Math.Abs(BitConverter.ToInt32(by, i));
                result = (Math.Abs(result * 10 + BigNum)) % serverNum;
            }
            return result;
        }

        public void SetServerDBCount()
        {
            foreach(var serverdb in serverlist)
            {
                foreach (var m in serverdb.Value)
                {
                    m.WorldCount = Help.GetCollectionCount(m.Ip);
                }
            }
        }

        
        /// <summary>
        /// 在一组配置的服务器组中平衡数据量
        /// </summary>
        /// <returns></returns>
        public int MakeBalance()
        {
            throw new NotImplementedException();
        }

        #region 之前的关于带虚拟节点的一致hash实现，弃用[因为一致性哈希所解决的伸缩问题和负载均衡问题在作为搜索寻址时并不需要]
        //private TreeMap<long, String> virtualNodes = new TreeMap<long, string>();
        //private LinkedList<String> nodes;
        ///// <summary>
        ///// 每个真实节点对应的虚拟节点数
        ///// </summary>
        //private int replicCnt;

        //public ConsistentHashLoadBalance(LinkedList<String> nodes, int replicCnt)
        //{
        //    this.nodes = nodes;
        //    this.replicCnt = replicCnt;
        //    initalization();
        //}

        ///**
        // * 初始化哈希环
        // * 循环计算每个node名称的哈希值，将其放入treeMap
        // */
        //private void initalization()
        //{
        //    foreach (String nodeName in nodes)
        //    {
        //        for (int i = 0; i < replicCnt / 4; i++)
        //        {
        //            String virtualNodeName = getNodeNameByIndex(nodeName, i);
        //            for (int j = 0; j < 4; j++)
        //            {
        //                virtualNodes.Add(hash(virtualNodeName, j), nodeName);
        //            }
        //        }
        //    }
        //}

        //private String getNodeNameByIndex(String nodeName, int index)
        //{
        //    return new StringBuilder(nodeName)
        //            .Append("&&")
        //            .Append(index)
        //            .ToString();
        //}

        ///**
        // * 根据资源key选择返回相应的节点名称
        // * @param key
        // * @return 节点名称
        // */
        //public String selectNode(String key)
        //{
        //    long hashOfKey = hash(key, 0);
        //    if (!virtualNodes.containsKey(hashOfKey))
        //    {
        //        var entry = virtualNodes.Findbiger(hashOfKey);
        //        if (entry != null)
        //            return entry;
        //        else
        //            return nodes.First.Value;
        //    }
        //    else
        //        return virtualNodes[hashOfKey];
        //}

        //private long hash(String nodeName, int number)
        //{
        //    byte[] digest = md5(nodeName);
        //    return (((long)(digest[3 + number * 4] & 0xFF) << 24)
        //            | ((long)(digest[2 + number * 4] & 0xFF) << 16)
        //            | ((long)(digest[1 + number * 4] & 0xFF) << 8)
        //            | (digest[number * 4] & 0xFF))
        //            & 0xFFFFFFFFL;
        //}

        ///**
        // * md5加密
        // *
        // * @param str
        // * @return
        // */
        //public byte[] md5(String str)
        //{
        //        var md = MD5.Create();
        //        return md.ComputeHash(UTF8Encoding.UTF8.GetBytes(str));
        //}

        //public void addNode(String node)
        //{
        //    nodes.AddLast(node);
        //    String virtualNodeName = getNodeNameByIndex(node, 0);
        //    for (int i = 0; i < replicCnt / 4; i++)
        //    {
        //        for (int j = 0; j < 4; j++)
        //        {
        //            virtualNodes.Add(hash(virtualNodeName, j), node);
        //        }
        //    }
        //}

        //public void removeNode(String node)
        //{
        //    nodes.Remove(node);
        //    String virtualNodeName = getNodeNameByIndex(node, 0);
        //    for (int i = 0; i < replicCnt / 4; i++)
        //    {
        //        for (int j = 0; j < 4; j++)
        //        {

        //            virtualNodes.Remove(hash(virtualNodeName, j));
        //        }
        //    }
        //}

        //public void printTreeNode()
        //{
        //    if (virtualNodes != null && !virtualNodes.isEmpty())
        //    {
        //        virtualNodes.InThreading();
        //        virtualNodes.forEach((hashKey, node)=>
        //               Debug.WriteLine(
        //                        new StringBuilder(node)
        //                                .Append(" ==> ")
        //                                .Append(hashKey)
        //                )
        //        );
        //    }
        //    else
        //        Console.WriteLine("Cycle is Empty");
        //}
        #endregion
    }

    class TreeMap<TKey, TValue>
    {
        BinarySearchTree<TKey, LinkedListNode<TValue>> bst=new BinarySearchTree<TKey, LinkedListNode<TValue>>();
        LinkedList<TValue> list=new LinkedList<TValue>();
        bool hasTread = false;

        public TValue this[TKey key]
        {
            get => bst[key].Value;
            set => bst[key].Value=value;
        }
        public void Add(TKey key,TValue value)
        {
            LinkedListNode<TValue> node = new LinkedListNode<TValue>(value);
            bst.Add(key, node);
            hasTread = false;
        }

        public void Remove(TKey key)
        {
            var node = bst[key];
            list.Remove(node);
            bst.Remove(key);

        }

        public void InThreading()
        {
            if (hasTread) return;
            list.Clear();
            var enumer = bst.GetOrderedEnumerator();
            while (enumer.MoveNext())
            {
                list.AddLast(enumer.Current.Value);

            } ;
            hasTread = true;
        }

        public TValue FindSimler(TKey key)
        {
            if (!hasTread) InThreading();
            return bst[key].Previous.Value;
        }
        public TValue Findbiger(TKey key)
        {
            if (!hasTread) InThreading();
            return bst[key].Next.Value;
        }

        public bool containsKey(TKey key)
        {
           return bst.ContainsKey(key);
        }

        public bool isEmpty()
        {
            return this.bst.Count == 0;
        }

        public void forEach(Action<TKey,TValue> action)
        {
            var enumer = bst.GetOrderedEnumerator();
            while (enumer.MoveNext())
            {
                action(enumer.Current.Key, enumer.Current.Value.Value);

            } ;
        }
    }


}
///NOTE:一般来说虚拟节点需要配置成真实节点的10倍以上才能打到负载均衡
///