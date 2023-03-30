#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using System;
using System.Collections.Generic;

namespace BestHTTP.Connections.HTTP2
{
    sealed class HeaderTable
    {
        // https://http2.github.io/http2-spec/compression.html#static.table.definition
        // Valid indexes starts with 1, so there's an empty entry.
        static string[] StaticTableValues = new string[] { string.Empty, string.Empty, "GET", "POST", "/", "/index.html", "http", "https", "200", "204", "206", "304", "400", "404", "500", string.Empty, "gzip, deflate" };

        // https://http2.github.io/http2-spec/compression.html#static.table.definition
        // Valid indexes starts with 1, so there's an empty entry.
        static string[] StaticTable = new string[62]
        {
            string.Empty,
            ":authority",
            ":method", // GET
            ":method", // POST
            ":path", // /
            ":path", // index.html
            ":scheme", // http
            ":scheme", // https
            ":status", // 200
            ":status", // 204
            ":status", // 206
            ":status", // 304
            ":status", // 400
            ":status", // 404
            ":status", // 500
            "accept-charset",
            "accept-encoding", // gzip, deflate
            "accept-language",
            "accept-ranges",
            "accept",
            "access-control-allow-origin",
            "age",
            "allow",
            "authorization",
            "cache-control",
            "content-disposition",
            "content-encoding",
            "content-language",
            "content-length",
            "content-location",
            "content-range",
            "content-type",
            "cookie",
            "date",
            "etag",
            "expect",
            "expires",
            "from",
            "host",
            "if-match",
            "if-modified-since",
            "if-none-match",
            "if-range",
            "if-unmodified-since",
            "last-modified",
            "link",
            "location",
            "max-forwards",
            "proxy-authenticate",
            "proxy-authorization",
            "range",
            "referer",
            "refresh",
            "retry-after",
            "server",
            "set-cookie",
            "strict-transport-security",
            "transfer-encoding",
            "user-agent",
            "vary",
            "via",
            "www-authenticate",
        };

        public UInt32 DynamicTableSize { get; private set; }
        public UInt32 MaxDynamicTableSize {
            get { return this._maxDynamicTableSize; }
            set
            {
                this._maxDynamicTableSize = value;
                EvictEntries(0);
            }
        }
        private UInt32 _maxDynamicTableSize;

        private List<KeyValuePair<string, string>> DynamicTable = new List<KeyValuePair<string, string>>();
        private HTTP2SettingsRegistry settingsRegistry;

        public HeaderTable(HTTP2SettingsRegistry registry)
        {
            this.settingsRegistry = registry;
            this.MaxDynamicTableSize = this.settingsRegistry[HTTP2Settings.HEADER_TABLE_SIZE];
        }

        public KeyValuePair<UInt32, UInt32> GetIndex(string key, string value)
        {
            for (int i = 0; i < DynamicTable.Count; ++i)
            {
                var kvp = DynamicTable[i];

                // Exact match for both key and value
                if (kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase) && kvp.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
                    return new KeyValuePair<UInt32, UInt32>((UInt32)(StaticTable.Length + i), (UInt32)(StaticTable.Length + i));
            }

            KeyValuePair<UInt32, UInt32> bestMatch = new KeyValuePair<UInt32, UInt32>(0, 0);
            for (int i = 0; i < StaticTable.Length; ++i)
            {
                if (StaticTable[i].Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    if (i < StaticTableValues.Length && !string.IsNullOrEmpty(StaticTableValues[i]) && StaticTableValues[i].Equals(value, StringComparison.OrdinalIgnoreCase))
                        return new KeyValuePair<UInt32, UInt32>((UInt32)i, (UInt32)i);
                    else
                        bestMatch = new KeyValuePair<UInt32, UInt32>((UInt32)i, 0);
                }
            }

            return bestMatch;
        }

        public string GetKey(UInt32 index)
        {
            if (index < StaticTable.Length)
                return StaticTable[index];

            return this.DynamicTable[(int)(index - StaticTable.Length)].Key;
        }

        public KeyValuePair<string, string> GetHeader(UInt32 index)
        {
            if (index < StaticTable.Length)
                return new KeyValuePair<string, string>(StaticTable[index],
                                                        index < StaticTableValues.Length ? StaticTableValues[index] : null);

            return this.DynamicTable[(int)(index - StaticTable.Length)];
        }

        public void Add(KeyValuePair<string, string> header)
        {
            // https://http2.github.io/http2-spec/compression.html#calculating.table.size
            // The size of an entry is the sum of its name's length in octets (as defined in Section 5.2),
            // its value's length in octets, and 32.
            UInt32 newHeaderSize = CalculateEntrySize(header);

            EvictEntries(newHeaderSize);

            // If the size of the new entry is less than or equal to the maximum size, that entry is added to the table.
            // It is not an error to attempt to add an entry that is larger than the maximum size;
            //  an attempt to add an entry larger than the maximum size causes the table to be
            //  emptied of all existing entries and results in an empty table.
            if (this.DynamicTableSize + newHeaderSize <= this.MaxDynamicTableSize)
            {
                this.DynamicTable.Insert(0, header);
                this.DynamicTableSize += (UInt32)newHeaderSize;
            }
        }

        private UInt32 CalculateEntrySize(KeyValuePair<string, string> entry)
        {
            return 32 + (UInt32)System.Text.Encoding.UTF8.GetByteCount(entry.Key) +
                        (UInt32)System.Text.Encoding.UTF8.GetByteCount(entry.Value);
        }

        private void EvictEntries(uint newHeaderSize)
        {
            // https://http2.github.io/http2-spec/compression.html#entry.addition
            // Before a new entry is added to the dynamic table, entries are evicted from the end of the dynamic
            //  table until the size of the dynamic table is less than or equal to (maximum size - new entry size) or until the table is empty.
            while (this.DynamicTableSize + newHeaderSize > this.MaxDynamicTableSize && this.DynamicTable.Count > 0)
            {
                KeyValuePair<string, string> entry = this.DynamicTable[this.DynamicTable.Count - 1];
                this.DynamicTable.RemoveAt(this.DynamicTable.Count - 1);
                this.DynamicTableSize -= CalculateEntrySize(entry);
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder("[HeaderTable ");
            sb.AppendFormat("DynamicTable count: {0}, DynamicTableSize: {1}, MaxDynamicTableSize: {2}, ", this.DynamicTable.Count, this.DynamicTableSize, this.MaxDynamicTableSize);

            foreach(var kvp in this.DynamicTable)
                sb.AppendFormat("\"{0}\": \"{1}\", ", kvp.Key, kvp.Value);

            sb.Append("]");
            return sb.ToString();
        }
    }
}

#endif
