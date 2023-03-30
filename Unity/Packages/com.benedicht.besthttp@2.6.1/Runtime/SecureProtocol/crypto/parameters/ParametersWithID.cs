#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
    public class ParametersWithID
        : ICipherParameters
    {
        private readonly ICipherParameters parameters;
        private readonly byte[] id;

        public ParametersWithID(ICipherParameters parameters,
            byte[] id)
            : this(parameters, id, 0, id.Length)
        {
        }

        public ParametersWithID(ICipherParameters parameters,
            byte[] id, int idOff, int idLen)
        {
            this.parameters = parameters;
            this.id = Arrays.CopyOfRange(id, idOff, idOff + idLen);
        }

        public byte[] GetID()
        {
            return id;
        }

        public ICipherParameters Parameters
        {
            get { return parameters; }
        }
    }
}
#pragma warning restore
#endif
