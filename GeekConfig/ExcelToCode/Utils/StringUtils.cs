using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExcelConverter.Utils
{
    public class StringUtils
    {

        public static string Trim(string org)
        {
            return Regex.Replace(org, @"\s", "");
        }

    }
}
