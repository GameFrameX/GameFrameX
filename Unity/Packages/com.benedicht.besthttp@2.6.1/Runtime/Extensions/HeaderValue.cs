using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BestHTTP.Extensions
{
    /// <summary>
    /// Used in string parsers. Its Value is optional.
    /// </summary>
    public sealed class HeaderValue
    {
        #region Public Properties

        public string Key { get; set; }
        public string Value { get; set; }
        public List<HeaderValue> Options { get; set; }

        public bool HasValue { get { return !string.IsNullOrEmpty(this.Value); } }

        #endregion

        #region Constructors

        public HeaderValue()
        { }

        public HeaderValue(string key)
        {
            this.Key = key;
        }

        #endregion

        #region Public Helper Functions

        public void Parse(string headerStr, ref int pos)
        {
            ParseImplementation(headerStr, ref pos, true);
        }

        public bool TryGetOption(string key, out HeaderValue option)
        {
            option = null;

            if (Options == null || Options.Count == 0)
                return false;

            for (int i = 0; i < Options.Count; ++i)
                if (String.Equals(Options[i].Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    option = Options[i];
                    return true;
                }

            return false;
        }

        #endregion

        #region Private Helper Functions

        private void ParseImplementation(string headerStr, ref int pos, bool isOptionIsAnOption)
        {
            // According to https://tools.ietf.org/html/rfc7234#section-5.2.1.1
            // Max-Age has a form "max-age=5", but some (imgur.com specifically) sends it as "max-age:5"
            string key = headerStr.Read(ref pos, (ch) => ch != ';' && ch != '=' && ch != ':' && ch != ',', true);
            this.Key = key;

            char? skippedChar = headerStr.Peek(pos - 1);
            bool isValue = skippedChar == '=' || skippedChar == ':';
            bool isOption = isOptionIsAnOption && skippedChar == ';';

            while ((skippedChar != null && isValue || isOption) && pos < headerStr.Length)
            {

                if (isValue)
                {
                    string value = headerStr.ReadPossibleQuotedText(ref pos);
                    this.Value = value;
                }
                else if (isOption)
                {
                    HeaderValue option = new HeaderValue();
                    option.ParseImplementation(headerStr, ref pos, false);

                    if (this.Options == null)
                        this.Options = new List<HeaderValue>();

                    this.Options.Add(option);
                }

                if (!isOptionIsAnOption)
                    return;

                skippedChar = headerStr.Peek(pos - 1);
                isValue = skippedChar == '=';
                isOption = isOptionIsAnOption && skippedChar == ';';
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            if (this.Options != null && this.Options.Count > 0)
            {
                StringBuilder sb = new StringBuilder(4);
                sb.Append(Key);
                sb.Append("=");
                sb.Append(Value);

                foreach(var option in Options)
                {
                    sb.Append(";");
                    sb.Append(option.ToString());
                }

                return sb.ToString();
            }
            else if (!string.IsNullOrEmpty(Value))
                return Key + '=' + Value;
            else
                return Key;
        }

        #endregion
    }
}
