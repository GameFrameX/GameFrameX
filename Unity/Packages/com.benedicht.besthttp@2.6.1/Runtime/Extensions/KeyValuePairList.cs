using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestHTTP.Extensions
{
    /// <summary>
    /// Base class for specialized parsers
    /// </summary>
    public class KeyValuePairList
    {
        public List<HeaderValue> Values { get; protected set; }

        public bool TryGet(string valueKeyName, out HeaderValue @param)
        {
            @param = null;
            for (int i = 0; i < Values.Count; ++i)
                if (string.CompareOrdinal(Values[i].Key, valueKeyName) == 0)
                {
                    @param = Values[i];
                    return true;
                }
            return false;
        }
    }
}