using System;

namespace Component.Default
{
    /// <summary>
    /// 此标记类主要用于标志组件的配置参数
    /// </summary>
    internal class ComponentPramAttribute : Attribute
    {
        private string fileType;

        public ComponentPramAttribute(string FileType)
        {
            this.FileType = FileType;
        }

        public string FileType { get => fileType; set => fileType = value; }

        public static ComponentPramAttribute GetAttribute(Type item)
        {
            var excludeFieldAttribute = (ComponentPramAttribute)GetCustomAttribute(item, typeof(ComponentPramAttribute));
            return excludeFieldAttribute;
        }
    }
}