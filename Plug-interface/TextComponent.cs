using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
/// <summary>
/// 组件暂缓...
/// </summary>
namespace Component
{
    /// <summary>
    /// 文本相关的搜索和索引外围主键
    /// </summary>
    class TextComponent
    {
        public TextComponent(string inputData)
        {
            if (string.IsNullOrWhiteSpace(inputData))
            {
                throw new ArgumentException("message", nameof(inputData));
            }
        }

        public TextComponent(FileStream inputFs)
        {

        }

        private object inputData;

        private object outputData;

        public object InputData { get => inputData; set => inputData = value; }
        public object OutputData { get => outputData; set => outputData = value; }

        protected virtual TextComponent Process()
        {
            this.OutputData = true;
            return this;
        }

        public T ToResult<T>()
        {
            return (T)InputData;
        }
    }
}
