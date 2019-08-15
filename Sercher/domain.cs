using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sercher
{
    /// <summary>
    /// 文档索引数据
    /// </summary>
    /// <remarks>一个单词对应一个文档，一次文档索引产生多个documentindex</remarks>
    [Serializable]
    public class DocumentIndex
    {
        public ObjectId DocId { set; get; }
        public int DocumentWorldTotal { set; get; }
        /// <summary>
        /// 对应一个句子
        /// </summary>
        public DateTime IndexTime { set; get; }
        public ObjectId _id { set; get; }

       // public string Word { set; get; }
        /// <summary>
        /// 当前单词出现在文中的次数
        /// </summary>
        public int WordFrequency { set; get; }
        /// <summary>
        /// 索引词语在文中的位置
        /// </summary>
        public List<int> BeginIndex { set; get; }

    }


    public class Document
    {
        public string Name { set; get; }
        public string Url { set; get; }
        public ObjectId _id { set; get; }
        //public long WorldTotal { set; get; }
        public enum HasIndexed { none, Indexed, Indexing }

        public HasIndexed hasIndexed { set; get; }
    }

    /// <summary>
    /// 此类统计单词数据用于分词
    /// </summary>
    [Serializable]
    public class WorldStatistics
    {
        public enum SourceType { FromDic, FromStady }

        string word;
        int frequency;
        public SourceType WordSource { set; get; }
        public string Word { get => word; set => word = value; }
        public int Frequency { get => frequency; set => frequency = value; }
        public ObjectId _id { set; get; }
    }

    public enum OPTION
    {
        /// <summary>
        /// 字典来自数据库
        /// </summary>
        DicIsDB,
        /// <summary>
        /// 字典来自dictionary
        /// </summary>
        DicIsByte,
        /// <summary>
        /// 字典来自xml
        /// </summary>
        DicIsXml,
        /// <summary>
        /// 字典来自文本文件
        /// </summary>
        DicIsTextResource
    }

    public class TrieTree
    {
        TrieNode _root = null;
        private TrieTree()
        {
            _root = new TrieNode(char.MaxValue, 0);
            charCount = 0;
        }
        static TrieTree _instance = null;
        public static TrieTree GetInstance()
        {
            if (_instance == null)
            {
                _instance = new TrieTree();
            }
            return _instance;
        }
        public TrieNode Root
        {
            get
            {
                return _root;
            }
        }
        public void AddWord(char ch)
        {
            TrieNode newnode = _root.AddChild(ch);
            newnode.IncreaseFrequency();
            newnode.WordEnded = true;
        }
        int charCount;
        public void AddWord(string word)
        {
            if (word.Length == 1)
            {
                AddWord(word[0]);
                charCount++;
            }
            else
            {
                char[] chars = word.ToCharArray();
                TrieNode node = _root;
                charCount += chars.Length;
                for (int i = 0; i < chars.Length; i++)
                {
                    TrieNode newnode = node.AddChild(chars[i]);
                    newnode.IncreaseFrequency();
                    node = newnode;
                }
                node.WordEnded = true;
            }
        }
        public int GetFrequency(char ch)
        {
            TrieNode matchedNode = _root.Children.FirstOrDefault(n => n.Character == ch);
            if (matchedNode == null)
            {
                return 0;
            }
            return matchedNode.Frequency;
        }
        public int GetFrequency(string word)
        {
            if (word.Length == 1)
            {
                return GetFrequency(word[0]);
            }
            else
            {
                char[] chars = word.ToCharArray();
                TrieNode node = _root;
                for (int i = 0; i < chars.Length; i++)
                {
                    if (node.Children == null)
                        return 0;
                    TrieNode matchednode = node.Children.FirstOrDefault(n => n.Character == chars[i]);
                    if (matchednode == null)
                    {
                        return 0;
                    }
                    node = matchednode;
                }
                if (node.WordEnded == true)
                    return node.Frequency;
                else
                    return -1;
            }
        }
    }

    public class TrieNode
    {
        public TrieNode(char ch, int depth)
        {
            this.Character = ch;
            this._depth = depth;
        }
        public char Character;
        int _depth;
        public int Depth
        {
            get
            {
                return _depth;
            }
        }
        TrieNode _parent = null;
        public TrieNode Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }
        public bool WordEnded = false;
        HashSet<TrieNode> _children = null;
        public HashSet<TrieNode> Children
        {
            get
            {
                return _children;
            }
        }
        public TrieNode GetChildNode(char ch)
        {
            if (_children != null)
                return _children.FirstOrDefault(n => n.Character == ch);
            else
                return null;
        }
        public TrieNode AddChild(char ch)
        {
            TrieNode matchedNode = null;
            if (_children != null)
            {
                matchedNode = _children.FirstOrDefault(n => n.Character == ch);
            }
            if (matchedNode != null)
            //found the char in the list   
            {
                //matchedNode.IncreaseFrequency();      
                return matchedNode;
            }
            else
            {
                //not found       
                TrieNode node = new TrieNode(ch, this.Depth + 1);
                node.Parent = this;
                //node.IncreaseFrequency();            
                if (_children == null)
                    _children = new HashSet<TrieNode>();
                _children.Add(node);
                return node;
            }
        }
        int _frequency = 0;
        public int Frequency
        {
            get
            {
                return _frequency;
            }
        }
        public void IncreaseFrequency()
        {
            _frequency++;
        }
        public string GetWord()
        {
            TrieNode tmp = this;
            string result = string.Empty;
            while (tmp.Parent != null) //until root node  
            {
                result = tmp.Character + result;
                tmp = tmp.Parent;
            }
            return result;
        }
        public override string ToString()
        {
            return Convert.ToString(this.Character);
        }
    }
}
