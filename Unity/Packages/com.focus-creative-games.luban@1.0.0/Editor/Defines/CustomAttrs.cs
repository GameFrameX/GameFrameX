using System;

namespace Luban.Editor
{
    /// <summary>
    /// 命令属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class CommandAttribute : Attribute
    {
        /// <summary>
        /// 命令
        /// </summary>
        public readonly string Option;

        /// <summary>
        /// 是否换行
        /// </summary>
        public readonly bool NewLine;

        public CommandAttribute(string option, bool newLine = true)
        {
            this.Option = option;
            this.NewLine = newLine;
        }
    }
}