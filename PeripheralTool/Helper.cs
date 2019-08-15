using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PeripheralTool
{
    public static class Helper
    {
        /// <summary>
        /// 把对象序列化并返回相应的字节
        /// </summary>
        /// <param name="pObj">需要序列化的对象</param>
        /// <returns>byte[]</returns>
        static public byte[] SerializeObject(object pObj)
        {
            if (pObj == null)
                return null;
            System.IO.MemoryStream _memory = new System.IO.MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(_memory, pObj);
            _memory.Position = 0;
            byte[] read = new byte[_memory.Length];
            _memory.Read(read, 0, read.Length);
            _memory.Close();
            return read;
        }
        /// <summary>
        /// 把字节反序列化成相应的对象
        /// </summary>
        /// <param name="pBytes">字节流</param>
                 /// <returns>object</returns>
        static public object DeserializeObject(byte[] pBytes)
        {
            object _newOjb = null;
            if (pBytes == null)
                return _newOjb;
            System.IO.MemoryStream _memory = new System.IO.MemoryStream(pBytes);
            _memory.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            _newOjb = formatter.Deserialize(_memory);
            _memory.Close();
            return _newOjb;
        }

        static public void SerializerXml<T>(object obj, string path)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            FileStream fs = File.Create(path);
            xs.Serialize(fs, obj);
            fs.Dispose();
        }
        static public T DeserializeXml<T>(string path)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            FileStream fs = File.OpenRead(path);
            T OBJ = (T)xs.Deserialize(fs);
            fs.Dispose();
            return OBJ;
        }


    }

}
