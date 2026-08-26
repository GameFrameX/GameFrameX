using System.Text.RegularExpressions;

namespace GameFrameX.ProtoExport
{
    internal static class Utility
    {
        public static readonly char[] splitChars = { ' ', '\t' };

        public static readonly string[] splitNotesChars = { "//" };

        public const string CamelCasePattern = @"^(?![A-Z]+$)(?!.*[A-Z]{2})[A-Z][a-zA-Z0-9]*$";

        public static bool IsCamelCase(string str)
        {
            return Regex.IsMatch(str, CamelCasePattern);
        }
    }
}
