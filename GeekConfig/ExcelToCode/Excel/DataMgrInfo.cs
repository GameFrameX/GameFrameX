using DotLiquid;
using System.Collections.Generic;

namespace ExcelToCode.Excel
{
    public class DataMgrInfo : Drop
    {
        public List<string> Containers { get; set; }

        public DataMgrInfo()
        {
            Containers = new List<string>();
        }

    }
}
