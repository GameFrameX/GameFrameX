using System;

namespace ExcelToCode.Excel
{
    public class DataIllegalException : Exception
    {

        public DataIllegalException(string message)
            :base(message)
        {
            
        }

    }
}
