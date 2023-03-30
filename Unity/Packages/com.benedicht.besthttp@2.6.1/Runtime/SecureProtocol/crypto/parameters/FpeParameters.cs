#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

using BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Parameters
{
public sealed class FpeParameters
    : ICipherParameters
{
    private readonly KeyParameter key;
    private readonly int radix;
    private readonly byte[] tweak;
    private readonly bool useInverse;

    public FpeParameters(KeyParameter key, int radix, byte[] tweak): this(key, radix, tweak, false)
    {
        
    }

    public FpeParameters(KeyParameter key, int radix, byte[] tweak, bool useInverse)
    {
        this.key = key;
        this.radix = radix;
        this.tweak = Arrays.Clone(tweak);
        this.useInverse = useInverse;
    }

    public KeyParameter Key
    {
        get { return key; }
    }

    public int Radix
    {
        get { return radix; }
    }

    public bool UseInverseFunction
    {
        get { return useInverse; }
    }

    public byte[] GetTweak()
    {
        return Arrays.Clone(tweak);
    }
}
}
#pragma warning restore
#endif
