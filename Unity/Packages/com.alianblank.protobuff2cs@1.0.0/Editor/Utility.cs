namespace Proto2CS.Editor
{
    internal static class Utility
    {
        public static readonly char[] splitChars = { ' ', '\t' };

        public static readonly string[] splitNotesChars = { "//" };

        public static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";

                    break;
                case "int32":
                    typeCs = "int";

                    break;
                case "bytes":
                    typeCs = "byte[]";

                    break;
                case "uint32":
                    typeCs = "uint";

                    break;
                case "long":
                    typeCs = "long";

                    break;
                case "int64":
                    typeCs = "long";

                    break;
                case "uint64":
                    typeCs = "ulong";

                    break;
                case "uint16":
                    typeCs = "ushort";

                    break;
                case "map<int32,long>":
                    typeCs = "Dictionary<int, long>";

                    break;
                default:
                    typeCs = type;

                    break;
            }

            return typeCs;
        }
    }
}