using System.Collections.Generic;
using DotLiquid;
using NLog;

namespace ExcelToCode.Excel
{

    public enum ExportType
    {
        Unknown,
        Client,
        Server,
        Both
    }

    public class SheetHeadInfo : Drop
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 表格id
        /// </summary>
        public int SheetId { set; get; }

        /// <summary>
        /// 表单名字
        /// </summary>
        public string SheetName { set; get; }

        /// <summary>
        /// 表单名字说明
        /// </summary>
        public string SheetNameDesc { set; get; }

        /// <summary>
        /// 导出类型
        /// </summary>
        public ExportType Etype = ExportType.Both;


        /// <summary>
        /// Bean类名字
        /// </summary>
        public string BeanClassName { set; get; }

        /// <summary>
        /// 容器类名字
        /// </summary>
        public string ContainerClassName { set; get; }

        /// <summary>
        /// 主键类型
        /// </summary>
        public string PrimaryKeyType { set; get; }

        /// <summary>
        /// 有效的字段
        /// </summary>
        public List<Field> Fields { get; private set; }

        /// <summary>
        /// 有效的字段
        /// </summary>
        public Dictionary<int, Field> ValidFileds { get; private set; }

        /// <summary>
        /// 字段数量
        /// </summary>
        public int FieldCount
        {
            get 
            {
                if(Fields == null)
                    return 0;
                return Fields.Count;
            }
        }

        public int ContentStartOffset { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> ValidFiledsName { get; private set; }

        public SheetHeadInfo()
        {
            ValidFileds = new Dictionary<int, Field>();
            ValidFiledsName = new Dictionary<string, string>();
            Fields = new List<Field>();
        }

        public void AddFiled(int key, Field field)
        {
            if (!ValidFileds.ContainsKey(key))
            {
                ValidFileds.Add(key, field);
                Fields.Add(field);
            }
            else
            {
                LOGGER.Info("重复添加key:{}", key);
            }
        }

    }
}
