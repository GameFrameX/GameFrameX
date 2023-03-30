#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO.Pem
{
	public class PemHeader
	{
		private string name;
		private string val;

		public PemHeader(string name, string val)
		{
			this.name = name;
			this.val = val;
		}

		public virtual string Name
		{
			get { return name; }
		}

		public virtual string Value
		{
			get { return val; }
		}

		public override int GetHashCode()
		{
			return GetHashCode(this.name) + 31 * GetHashCode(this.val);
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
				return true;

			if (!(obj is PemHeader))
				return false;

			PemHeader other = (PemHeader)obj;

			return BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.Equals(this.name, other.name)
				&& BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.Equals(this.val, other.val);
		}

		private int GetHashCode(string s)
		{
			if (s == null)
			{
				return 1;
			}

			return s.GetHashCode();
		}

        public override string ToString()
        {
			return name + ":" + val;
        }
    }
}
#pragma warning restore
#endif
