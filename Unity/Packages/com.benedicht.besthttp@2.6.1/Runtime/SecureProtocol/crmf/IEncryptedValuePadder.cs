#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
#pragma warning disable
using System;

namespace BestHTTP.SecureProtocol.Org.BouncyCastle.Crmf
{
     /// <summary>
     /// An encrypted value padder is used to make sure that prior to a value been
     /// encrypted the data is padded to a standard length.
     /// </summary>
    public interface IEncryptedValuePadder
    {
        ///
        /// <summary>Return a byte array of padded data.</summary>
        ///
        /// <param name="data">the data to be padded.</param>
        /// <returns>a padded byte array containing data.</returns>
        ///
        byte[] GetPaddedData(byte[] data);

        ///
        /// <summary>Return a byte array of with padding removed.</summary>
        ///
        /// <param name="paddedData">the data to be padded.</param>
        /// <returns>an array containing the original unpadded data.</returns>
        ///
        byte[] GetUnpaddedData(byte[] paddedData);
    }
}
#pragma warning restore
#endif
