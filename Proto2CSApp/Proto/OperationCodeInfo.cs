namespace Proto2CS.Editor
{
    public class OperationCodeInfoList
    {
        public OperationCodeInfoList()
        {
            OperationCodeInfos = new List<OperationCodeInfo>();
        }

        public int Start { get; set; }
        public List<OperationCodeInfo> OperationCodeInfos { get; set; }
        public string OutputPath { get; set; }
    }

    /// <summary>
    /// 消息码信息
    /// </summary>
    public class OperationCodeInfo
    {
        public OperationCodeInfo(bool isEnum = false) : this()
        {
            IsEnum = isEnum;
        }

        private OperationCodeInfo()
        {
            Fields = new List<OperationField>();
            Description = string.Empty;
        }

        public bool IsRequest
        {
            get { return Name.StartsWith("Req") || Name.StartsWith("C2S_"); }
        }

        public string ParentClass
        {
            get
            {
                if (IsEnum)
                {
                    return string.Empty;
                }

                if (Name.StartsWith("Req") || Name.StartsWith("C2S_"))
                {
                    return "IRequestMessage";
                }

                if (Name.StartsWith("Res") || Name.StartsWith("S2C_"))
                {
                    return "IResponseMessage";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 操作码
        /// </summary>
        public int Opcode { get; set; }

        /// <summary>
        /// 是否是枚举
        /// </summary>
        public bool IsEnum { get; set; }

        /// <summary>
        /// 字段
        /// </summary>
        public List<OperationField> Fields { get; set; } = new List<OperationField>();

        /// <summary>
        /// 注释
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    public class OperationField
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Type); }
        }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 注释
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 成员编码
        /// </summary>
        public int Members { get; set; }

        /// <summary>
        /// 是否是重复
        /// </summary>
        public bool IsRepeated { get; set; }
    }
}