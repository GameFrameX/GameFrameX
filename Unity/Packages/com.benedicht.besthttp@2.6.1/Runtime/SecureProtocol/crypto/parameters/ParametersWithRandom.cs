#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
    public class ParametersWithRandom
		: ICipherParameters
    {
        private readonly ICipherParameters	parameters;
		private readonly SecureRandom		random;

		public ParametersWithRandom(
            ICipherParameters	parameters,
            SecureRandom		random)
        {
			if (parameters == null)
				throw new ArgumentNullException("parameters");
			if (random == null)
				throw new ArgumentNullException("random");

			this.parameters = parameters;
			this.random = random;
		}

		public ParametersWithRandom(
            ICipherParameters parameters)
			: this(parameters, new SecureRandom())
        {
		}


		public SecureRandom GetRandom()
		{
			return Random;
		}

		public SecureRandom Random
        {
			get { return random; }
        }

		public ICipherParameters Parameters
        {
            get { return parameters; }
        }
    }
}
#pragma warning restore
#endif
