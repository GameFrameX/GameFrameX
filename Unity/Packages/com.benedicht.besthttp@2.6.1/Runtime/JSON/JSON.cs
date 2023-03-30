using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BestHTTP.JSON
{
	/// <summary>
    /// Based on the download from http://techblog.procurios.nl/k/news/view/14605/14863/how-do-i-write-my-own-parser-%28for-json%29.html
	/// This class encodes and decodes JSON strings.
	/// Spec. details, see http://www.json.org/
	///
    /// JSON uses Arrays and Objects. These correspond here to the datatypes List and Dictionary.
	/// All numbers are parsed to doubles.
	/// </summary>
	public class Json
	{
		private const int TOKEN_NONE = 0;
        private const int TOKEN_CURLY_OPEN = 1;
        private const int TOKEN_CURLY_CLOSE = 2;
        private const int TOKEN_SQUARED_OPEN = 3;
        private const int TOKEN_SQUARED_CLOSE = 4;
        private const int TOKEN_COLON = 5;
        private const int TOKEN_COMMA = 6;
        private const int TOKEN_STRING = 7;
        private const int TOKEN_NUMBER = 8;
        private const int TOKEN_TRUE = 9;
        private const int TOKEN_FALSE = 10;
        private const int TOKEN_NULL = 11;

		private const int BUILDER_CAPACITY = 2000;

		/// <summary>
		/// Parses the string json into a value
		/// </summary>
		/// <param name="json">A JSON string.</param>
        /// <returns>A List, a Dictionary, a double, a string, null, true, or false</returns>
		public static object Decode(string json)
		{
			bool success = true;

			return Decode(json, ref success);
		}

		/// <summary>
		/// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <param name="success">Successful parse?</param>
        /// <returns>A List, a Dictionary, a double, a string, null, true, or false</returns>
		public static object Decode(string json, ref bool success)
		{
			success = true;
			if (json != null) {
				char[] charArray = json.ToCharArray();
				int index = 0;
				object value = ParseValue(charArray, ref index, ref success);
				return value;
			} else {
				return null;
			}
		}

		/// <summary>
        /// Converts a Dictionary / List object into a JSON string
		/// </summary>
		/// <param name="json">A Dictionary / List</param>
		/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
		public static string Encode(object json)
		{
			StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
			bool success = SerializeValue(json, builder);
			return (success ? builder.ToString() : null);
		}

		protected static Dictionary<string, object> ParseObject(char[] json, ref int index, ref bool success)
		{
            Dictionary<string, object> table = new Dictionary<string, object>();
			int token;

			// {
			NextToken(json, ref index);

			bool done = false;
			while (!done) {
				token = LookAhead(json, index);
				if (token == Json.TOKEN_NONE) {
					success = false;
					return null;
				} else if (token == Json.TOKEN_COMMA) {
					NextToken(json, ref index);
				} else if (token == Json.TOKEN_CURLY_CLOSE) {
					NextToken(json, ref index);
					return table;
				} else {

					// name
					string name = ParseString(json, ref index, ref success);
					if (!success) {
						success = false;
						return null;
					}

					// :
					token = NextToken(json, ref index);
					if (token != Json.TOKEN_COLON) {
						success = false;
						return null;
					}

					// value
					object value = ParseValue(json, ref index, ref success);
					if (!success) {
						success = false;
						return null;
					}

					table[name] = value;
				}
			}

			return table;
		}

		protected static List<object> ParseArray(char[] json, ref int index, ref bool success)
		{
            List<object> array = new List<object>();

			// [
			NextToken(json, ref index);

			bool done = false;
			while (!done) {
				int token = LookAhead(json, index);
				if (token == Json.TOKEN_NONE) {
					success = false;
					return null;
				} else if (token == Json.TOKEN_COMMA) {
					NextToken(json, ref index);
				} else if (token == Json.TOKEN_SQUARED_CLOSE) {
					NextToken(json, ref index);
					break;
				} else {
					object value = ParseValue(json, ref index, ref success);
					if (!success) {
						return null;
					}

					array.Add(value);
				}
			}

			return array;
		}

		protected static object ParseValue(char[] json, ref int index, ref bool success)
		{
			switch (LookAhead(json, index)) {
				case Json.TOKEN_STRING:
					return ParseString(json, ref index, ref success);
				case Json.TOKEN_NUMBER:
					return ParseNumber(json, ref index, ref success);
				case Json.TOKEN_CURLY_OPEN:
					return ParseObject(json, ref index, ref success);
				case Json.TOKEN_SQUARED_OPEN:
					return ParseArray(json, ref index, ref success);
				case Json.TOKEN_TRUE:
					NextToken(json, ref index);
					return true;
				case Json.TOKEN_FALSE:
					NextToken(json, ref index);
					return false;
				case Json.TOKEN_NULL:
					NextToken(json, ref index);
					return null;
				case Json.TOKEN_NONE:
					break;
			}

			success = false;
			return null;
		}

		protected static string ParseString(char[] json, ref int index, ref bool success)
		{
			StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
			char c;

			EatWhitespace(json, ref index);

			// "
			c = json[index++];

			bool complete = false;
			while (!complete) {

				if (index == json.Length) {
					break;
				}

				c = json[index++];
				if (c == '"') {
					complete = true;
					break;
				} else if (c == '\\') {

					if (index == json.Length) {
						break;
					}
					c = json[index++];
					if (c == '"') {
						s.Append('"');
					} else if (c == '\\') {
						s.Append('\\');
					} else if (c == '/') {
						s.Append('/');
					} else if (c == 'b') {
						s.Append('\b');
					} else if (c == 'f') {
						s.Append('\f');
					} else if (c == 'n') {
						s.Append('\n');
					} else if (c == 'r') {
						s.Append('\r');
					} else if (c == 't') {
						s.Append('\t');
					} else if (c == 'u') {
						int remainingLength = json.Length - index;
						if (remainingLength >= 4) {
							// parse the 32 bit hex into an integer codepoint
							uint codePoint;
							if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint))) {
								return "";
							}
							// convert the integer codepoint to a unicode char and add to string
							s.Append(Char.ConvertFromUtf32((int)codePoint));
							// skip 4 chars
							index += 4;
						} else {
							break;
						}
					}

				} else {
					s.Append(c);
				}

			}

			if (!complete) {
				success = false;
				return null;
			}

			return s.ToString();
		}

		protected static double ParseNumber(char[] json, ref int index, ref bool success)
		{
			EatWhitespace(json, ref index);

			int lastIndex = GetLastIndexOfNumber(json, index);
			int charLength = (lastIndex - index) + 1;

			double number;
			success = Double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

			index = lastIndex + 1;
			return number;
		}

		protected static int GetLastIndexOfNumber(char[] json, int index)
		{
			int lastIndex;

			for (lastIndex = index; lastIndex < json.Length; lastIndex++) {
				if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1) {
					break;
				}
			}
			return lastIndex - 1;
		}

		protected static void EatWhitespace(char[] json, ref int index)
		{
			for (; index < json.Length; index++) {
				if (" \t\n\r".IndexOf(json[index]) == -1) {
					break;
				}
			}
		}

		protected static int LookAhead(char[] json, int index)
		{
			int saveIndex = index;
			return NextToken(json, ref saveIndex);
		}

		protected static int NextToken(char[] json, ref int index)
		{
			EatWhitespace(json, ref index);

			if (index == json.Length) {
				return Json.TOKEN_NONE;
			}

			char c = json[index];
			index++;
			switch (c) {
				case '{':
					return Json.TOKEN_CURLY_OPEN;
				case '}':
					return Json.TOKEN_CURLY_CLOSE;
				case '[':
					return Json.TOKEN_SQUARED_OPEN;
				case ']':
					return Json.TOKEN_SQUARED_CLOSE;
				case ',':
					return Json.TOKEN_COMMA;
				case '"':
					return Json.TOKEN_STRING;
				case '0': case '1': case '2': case '3': case '4':
				case '5': case '6': case '7': case '8': case '9':
				case '-':
					return Json.TOKEN_NUMBER;
				case ':':
					return Json.TOKEN_COLON;
			}
			index--;

			int remainingLength = json.Length - index;

			// false
			if (remainingLength >= 5) {
				if (json[index] == 'f' &&
					json[index + 1] == 'a' &&
					json[index + 2] == 'l' &&
					json[index + 3] == 's' &&
					json[index + 4] == 'e') {
					index += 5;
					return Json.TOKEN_FALSE;
				}
			}

			// true
			if (remainingLength >= 4) {
				if (json[index] == 't' &&
					json[index + 1] == 'r' &&
					json[index + 2] == 'u' &&
					json[index + 3] == 'e') {
					index += 4;
					return Json.TOKEN_TRUE;
				}
			}

			// null
			if (remainingLength >= 4) {
				if (json[index] == 'n' &&
					json[index + 1] == 'u' &&
					json[index + 2] == 'l' &&
					json[index + 3] == 'l') {
					index += 4;
					return Json.TOKEN_NULL;
				}
			}

			return Json.TOKEN_NONE;
		}

		protected static bool SerializeValue(object value, StringBuilder builder)
		{
			bool success = true;

			if (value is string) {
				success = SerializeString((string)value, builder);
			} else if (value is IDictionary) {
				success = SerializeObject((IDictionary)value, builder);
			} else if (value is IList) {
				success = SerializeArray(value as IList, builder);
			} else if ((value is Boolean) && ((Boolean)value == true)) {
				builder.Append("true");
			} else if ((value is Boolean) && ((Boolean)value == false)) {
				builder.Append("false");
			} else if (value is ValueType) {
				// thanks to ritchie for pointing out ValueType to me
				success = SerializeNumber(Convert.ToDouble(value), builder);
			} else if (value == null) {
				builder.Append("null");
			} else {
				success = false;
			}
			return success;
		}

		protected static bool SerializeObject(IDictionary anObject, StringBuilder builder)
		{
			builder.Append("{");

			IDictionaryEnumerator e = anObject.GetEnumerator();
			bool first = true;
			while (e.MoveNext()) {
				string key = e.Key.ToString();
				object value = e.Value;

				if (!first) {
					builder.Append(", ");
				}

				SerializeString(key, builder);
				builder.Append(":");
				if (!SerializeValue(value, builder)) {
					return false;
				}

				first = false;
			}

			builder.Append("}");
			return true;
		}

		protected static bool SerializeArray(IList anArray, StringBuilder builder)
		{
			builder.Append("[");

			bool first = true;
			for (int i = 0; i < anArray.Count; i++) {
				object value = anArray[i];

				if (!first) {
					builder.Append(", ");
				}

				if (!SerializeValue(value, builder)) {
					return false;
				}

				first = false;
			}

			builder.Append("]");
			return true;
		}

		protected static bool SerializeString(string aString, StringBuilder builder)
		{
			builder.Append("\"");

			char[] charArray = aString.ToCharArray();
			for (int i = 0; i < charArray.Length; i++) {
				char c = charArray[i];
				if (c == '"') {
					builder.Append("\\\"");
				} else if (c == '\\') {
					builder.Append("\\\\");
				} else if (c == '\b') {
					builder.Append("\\b");
				} else if (c == '\f') {
					builder.Append("\\f");
				} else if (c == '\n') {
					builder.Append("\\n");
				} else if (c == '\r') {
					builder.Append("\\r");
				} else if (c == '\t') {
					builder.Append("\\t");
				} else {
					int codepoint = Convert.ToInt32(c);
					if ((codepoint >= 32) && (codepoint <= 126)) {
						builder.Append(c);
					} else {
						builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
					}
				}
			}

			builder.Append("\"");
			return true;
		}

		protected static bool SerializeNumber(double number, StringBuilder builder)
		{
			builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
			return true;
		}
	}
}