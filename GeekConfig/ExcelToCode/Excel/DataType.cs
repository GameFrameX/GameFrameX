namespace ExcelToCode.Excel
{
    public static class DataType
    {

        public const string Int = "int";

        public const string String = "string";

        public const string Long = "long";

        public const string Float = "float";

        public const string Text = "text";

        public const string TextMult = "textmult";

        public static bool IsLegal(string type)
        {
            //默认为int类型
            if (string.IsNullOrEmpty(type))
                type = "int";
            return (type == Int || type == Text || type == TextMult
                || type == Long || type == String);
        }

        public static string GetTrueTyped(string type)
        {
            //默认为int类型
            if (string.IsNullOrEmpty(type))
                type = "int";
            if (type == Text || type == String)
                return String;
            return type;
        }

    }
}
