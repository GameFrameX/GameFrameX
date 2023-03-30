#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;
using System.Collections;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.CryptoPro
{
    public class Gost28147Parameters
        : Asn1Encodable
    {
        private readonly Asn1OctetString iv;
        private readonly DerObjectIdentifier paramSet;

		public static Gost28147Parameters GetInstance(
            Asn1TaggedObject	obj,
            bool				explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

		public static Gost28147Parameters GetInstance(
            object obj)
        {
            if (obj == null || obj is Gost28147Parameters)
            {
                return (Gost28147Parameters) obj;
            }

            if (obj is Asn1Sequence)
            {
                return new Gost28147Parameters((Asn1Sequence) obj);
            }

            throw new ArgumentException("Invalid GOST3410Parameter: " + BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Platform.GetTypeName(obj));
        }

        private Gost28147Parameters(
            Asn1Sequence seq)
        {
			if (seq.Count != 2)
				throw new ArgumentException("Wrong number of elements in sequence", "seq");

			this.iv = Asn1OctetString.GetInstance(seq[0]);
			this.paramSet = DerObjectIdentifier.GetInstance(seq[1]);
        }

		/**
         * <pre>
         * Gost28147-89-Parameters ::=
         *               SEQUENCE {
         *                       iv                   Gost28147-89-IV,
         *                       encryptionParamSet   OBJECT IDENTIFIER
         *                }
         *
         *   Gost28147-89-IV ::= OCTET STRING (SIZE (8))
         * </pre>
         */
        public override Asn1Object ToAsn1Object()
        {
			return new DerSequence(iv, paramSet);
        }
    }
}
#pragma warning restore
#endif
