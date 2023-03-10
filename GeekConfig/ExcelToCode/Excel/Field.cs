using DotLiquid;
using System;

namespace ExcelToCode.Excel
{
    public class Field : Drop
    {
        public int Col;

        public int Row;

        /// <summary>
        /// 字段名
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// 数据类型(驼峰命名会被模板拆分为DataType==>Data_Type)
        /// (为保证模板和类属性一致，故用小写)
        /// </summary>
        public string Datatype { set; get; }

        /// <summary>
        /// 字段描述
        /// </summary>
        public string Desc { get; set; }

    }

}
