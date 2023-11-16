using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel;

namespace Server.Utility
{
    public static partial class Verifier
    {
        /// <summary>
        ///   Provides an implementation of the CRC-64 algorithm as described in ECMA-182, Annex B.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     For methods that return byte arrays or that write into spans of bytes,
        ///     this implementation emits the answer in the Big Endian byte order so that
        ///     the CRC residue relationship (CRC(message concat CRC(message))) is a fixed value) holds.
        ///     For CRC-64 this stable output is the byte sequence
        ///     <c>{ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }</c>.
        ///   </para>
        ///   <para>
        ///     There are multiple, incompatible, definitions of a 64-bit cyclic redundancy
        ///     check (CRC) algorithm. When interoperating with another system, ensure that you
        ///     are using the same definition. The definition used by this implementation is not
        ///     compatible with the cyclic redundancy check described in ISO 3309.
        ///   </para>
        /// </remarks>
        public sealed partial class Crc64 : NonCryptographicHashAlgorithm
        {
            private const ulong InitialState = 0UL;
            private const int Size = sizeof(ulong);

            private ulong _crc = InitialState;

            /// <summary>
            ///   Initializes a new instance of the <see cref="Crc64"/> class.
            /// </summary>
            public Crc64()
                : base(Size)
            {
            }

            /// <summary>
            ///   Appends the contents of <paramref name="source"/> to the data already
            ///   processed for the current hash computation.
            /// </summary>
            /// <param name="source">The data to process.</param>
            public override void Append(ReadOnlySpan<byte> source)
            {
                _crc = Update(_crc, source);
            }

            /// <summary>
            ///   Resets the hash computation to the initial state.
            /// </summary>
            public override void Reset()
            {
                _crc = InitialState;
            }

            /// <summary>
            ///   Writes the computed hash value to <paramref name="destination"/>
            ///   without modifying accumulated state.
            /// </summary>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            protected override void GetCurrentHashCore(Span<byte> destination)
            {
                BinaryPrimitives.WriteUInt64BigEndian(destination, _crc);
            }

            /// <summary>
            ///   Writes the computed hash value to <paramref name="destination"/>
            ///   then clears the accumulated state.
            /// </summary>
            protected override void GetHashAndResetCore(Span<byte> destination)
            {
                BinaryPrimitives.WriteUInt64BigEndian(destination, _crc);
                _crc = InitialState;
            }

            /// <summary>Gets the current computed hash value without modifying accumulated state.</summary>
            /// <returns>The hash value for the data already provided.</returns>
            [CLSCompliant(false)]
            public ulong GetCurrentHashAsUInt64() => _crc;

            /// <summary>
            ///   Computes the CRC-64 hash of the provided data.
            /// </summary>
            /// <param name="source">The data to hash.</param>
            /// <returns>The CRC-64 hash of the provided data.</returns>
            /// <exception cref="ArgumentNullException">
            ///   <paramref name="source"/> is <see langword="null"/>.
            /// </exception>
            public static byte[] Hash(byte[] source)
            {
                if (source is null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                return Hash(new ReadOnlySpan<byte>(source));
            }

            /// <summary>
            ///   Computes the CRC-64 hash of the provided data.
            /// </summary>
            /// <param name="source">The data to hash.</param>
            /// <returns>The CRC-64 hash of the provided data.</returns>
            public static byte[] Hash(ReadOnlySpan<byte> source)
            {
                byte[] ret = new byte[Size];
                ulong hash = HashToUInt64(source);
                BinaryPrimitives.WriteUInt64BigEndian(ret, hash);
                return ret;
            }

            /// <summary>
            ///   Attempts to compute the CRC-64 hash of the provided data into the provided destination.
            /// </summary>
            /// <param name="source">The data to hash.</param>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <param name="bytesWritten">
            ///   On success, receives the number of bytes written to <paramref name="destination"/>.
            /// </param>
            /// <returns>
            ///   <see langword="true"/> if <paramref name="destination"/> is long enough to receive
            ///   the computed hash value (8 bytes); otherwise, <see langword="false"/>.
            /// </returns>
            public static bool TryHash(ReadOnlySpan<byte> source, Span<byte> destination, out int bytesWritten)
            {
                if (destination.Length < Size)
                {
                    bytesWritten = 0;
                    return false;
                }

                ulong hash = HashToUInt64(source);
                BinaryPrimitives.WriteUInt64BigEndian(destination, hash);
                bytesWritten = Size;
                return true;
            }

            /// <summary>
            ///   Computes the CRC-64 hash of the provided data into the provided destination.
            /// </summary>
            /// <param name="source">The data to hash.</param>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <returns>
            ///   The number of bytes written to <paramref name="destination"/>.
            /// </returns>
            public static int Hash(ReadOnlySpan<byte> source, Span<byte> destination)
            {
                if (destination.Length < Size)
                {
                    ThrowDestinationTooShort();
                }

                ulong hash = HashToUInt64(source);
                BinaryPrimitives.WriteUInt64BigEndian(destination, hash);
                return Size;
            }

            /// <summary>Computes the CRC-64 hash of the provided data.</summary>
            /// <param name="source">The data to hash.</param>
            /// <returns>The computed CRC-64 hash.</returns>
            [CLSCompliant(false)]
            public static ulong HashToUInt64(ReadOnlySpan<byte> source) =>
                Update(InitialState, source);

            private static ulong Update(ulong crc, ReadOnlySpan<byte> source)
            {
                ReadOnlySpan<ulong> crcLookup = CrcLookup;
                for (int i = 0; i < source.Length; i++)
                {
                    ulong idx = (crc >> 56);
                    idx ^= source[i];
                    crc = crcLookup[(int)idx] ^ (crc << 8);
                }

                return crc;
            }

            /// <summary>CRC-64 transition table.</summary>
            private static ReadOnlySpan<ulong> CrcLookup => new ulong[256]
            {
                // Generated by GenerateTable(0x42F0E1EBA9EA3693):
                //
                //     static ulong[] GenerateTable(ulong polynomial)
                //     {
                //         var table = new ulong[256];
                //         for (int i = 0; i < table.Length; i++)
                //         {
                //             ulong val = (ulong)i << 56;
                //             for (int j = 0; j < 8; j++)
                //             {
                //                 if ((val & 0x8000_0000_0000_0000) == 0)
                //                 {
                //                     val <<= 1;
                //                 }
                //                 else
                //                 {
                //                     val = (val << 1) ^ polynomial;
                //                 }
                //             }
                //             table[i] = val;
                //         }
                //         return table;
                //     }

                0x0, 0x42F0E1EBA9EA3693, 0x85E1C3D753D46D26, 0xC711223CFA3E5BB5, 0x493366450E42ECDF, 0xBC387AEA7A8DA4C, 0xCCD2A5925D9681F9, 0x8E224479F47CB76A,
                0x9266CC8A1C85D9BE, 0xD0962D61B56FEF2D, 0x17870F5D4F51B498, 0x5577EEB6E6BB820B, 0xDB55AACF12C73561, 0x99A54B24BB2D03F2, 0x5EB4691841135847, 0x1C4488F3E8F96ED4,
                0x663D78FF90E185EF, 0x24CD9914390BB37C, 0xE3DCBB28C335E8C9, 0xA12C5AC36ADFDE5A, 0x2F0E1EBA9EA36930, 0x6DFEFF5137495FA3, 0xAAEFDD6DCD770416, 0xE81F3C86649D3285,
                0xF45BB4758C645C51, 0xB6AB559E258E6AC2, 0x71BA77A2DFB03177, 0x334A9649765A07E4, 0xBD68D2308226B08E, 0xFF9833DB2BCC861D, 0x388911E7D1F2DDA8, 0x7A79F00C7818EB3B,
                0xCC7AF1FF21C30BDE, 0x8E8A101488293D4D, 0x499B3228721766F8, 0xB6BD3C3DBFD506B, 0x854997BA2F81E701, 0xC7B97651866BD192, 0xA8546D7C558A27, 0x4258B586D5BFBCB4,
                0x5E1C3D753D46D260, 0x1CECDC9E94ACE4F3, 0xDBFDFEA26E92BF46, 0x990D1F49C77889D5, 0x172F5B3033043EBF, 0x55DFBADB9AEE082C, 0x92CE98E760D05399, 0xD03E790CC93A650A,
                0xAA478900B1228E31, 0xE8B768EB18C8B8A2, 0x2FA64AD7E2F6E317, 0x6D56AB3C4B1CD584, 0xE374EF45BF6062EE, 0xA1840EAE168A547D, 0x66952C92ECB40FC8, 0x2465CD79455E395B,
                0x3821458AADA7578F, 0x7AD1A461044D611C, 0xBDC0865DFE733AA9, 0xFF3067B657990C3A, 0x711223CFA3E5BB50, 0x33E2C2240A0F8DC3, 0xF4F3E018F031D676, 0xB60301F359DBE0E5,
                0xDA050215EA6C212F, 0x98F5E3FE438617BC, 0x5FE4C1C2B9B84C09, 0x1D14202910527A9A, 0x93366450E42ECDF0, 0xD1C685BB4DC4FB63, 0x16D7A787B7FAA0D6, 0x5427466C1E109645,
                0x4863CE9FF6E9F891, 0xA932F745F03CE02, 0xCD820D48A53D95B7, 0x8F72ECA30CD7A324, 0x150A8DAF8AB144E, 0x43A04931514122DD, 0x84B16B0DAB7F7968, 0xC6418AE602954FFB,
                0xBC387AEA7A8DA4C0, 0xFEC89B01D3679253, 0x39D9B93D2959C9E6, 0x7B2958D680B3FF75, 0xF50B1CAF74CF481F, 0xB7FBFD44DD257E8C, 0x70EADF78271B2539, 0x321A3E938EF113AA,
                0x2E5EB66066087D7E, 0x6CAE578BCFE24BED, 0xABBF75B735DC1058, 0xE94F945C9C3626CB, 0x676DD025684A91A1, 0x259D31CEC1A0A732, 0xE28C13F23B9EFC87, 0xA07CF2199274CA14,
                0x167FF3EACBAF2AF1, 0x548F120162451C62, 0x939E303D987B47D7, 0xD16ED1D631917144, 0x5F4C95AFC5EDC62E, 0x1DBC74446C07F0BD, 0xDAAD56789639AB08, 0x985DB7933FD39D9B,
                0x84193F60D72AF34F, 0xC6E9DE8B7EC0C5DC, 0x1F8FCB784FE9E69, 0x43081D5C2D14A8FA, 0xCD2A5925D9681F90, 0x8FDAB8CE70822903, 0x48CB9AF28ABC72B6, 0xA3B7B1923564425,
                0x70428B155B4EAF1E, 0x32B26AFEF2A4998D, 0xF5A348C2089AC238, 0xB753A929A170F4AB, 0x3971ED50550C43C1, 0x7B810CBBFCE67552, 0xBC902E8706D82EE7, 0xFE60CF6CAF321874,
                0xE224479F47CB76A0, 0xA0D4A674EE214033, 0x67C58448141F1B86, 0x253565A3BDF52D15, 0xAB1721DA49899A7F, 0xE9E7C031E063ACEC, 0x2EF6E20D1A5DF759, 0x6C0603E6B3B7C1CA,
                0xF6FAE5C07D3274CD, 0xB40A042BD4D8425E, 0x731B26172EE619EB, 0x31EBC7FC870C2F78, 0xBFC9838573709812, 0xFD39626EDA9AAE81, 0x3A28405220A4F534, 0x78D8A1B9894EC3A7,
                0x649C294A61B7AD73, 0x266CC8A1C85D9BE0, 0xE17DEA9D3263C055, 0xA38D0B769B89F6C6, 0x2DAF4F0F6FF541AC, 0x6F5FAEE4C61F773F, 0xA84E8CD83C212C8A, 0xEABE6D3395CB1A19,
                0x90C79D3FEDD3F122, 0xD2377CD44439C7B1, 0x15265EE8BE079C04, 0x57D6BF0317EDAA97, 0xD9F4FB7AE3911DFD, 0x9B041A914A7B2B6E, 0x5C1538ADB04570DB, 0x1EE5D94619AF4648,
                0x2A151B5F156289C, 0x4051B05E58BC1E0F, 0x87409262A28245BA, 0xC5B073890B687329, 0x4B9237F0FF14C443, 0x962D61B56FEF2D0, 0xCE73F427ACC0A965, 0x8C8315CC052A9FF6,
                0x3A80143F5CF17F13, 0x7870F5D4F51B4980, 0xBF61D7E80F251235, 0xFD913603A6CF24A6, 0x73B3727A52B393CC, 0x31439391FB59A55F, 0xF652B1AD0167FEEA, 0xB4A25046A88DC879,
                0xA8E6D8B54074A6AD, 0xEA16395EE99E903E, 0x2D071B6213A0CB8B, 0x6FF7FA89BA4AFD18, 0xE1D5BEF04E364A72, 0xA3255F1BE7DC7CE1, 0x64347D271DE22754, 0x26C49CCCB40811C7,
                0x5CBD6CC0CC10FAFC, 0x1E4D8D2B65FACC6F, 0xD95CAF179FC497DA, 0x9BAC4EFC362EA149, 0x158E0A85C2521623, 0x577EEB6E6BB820B0, 0x906FC95291867B05, 0xD29F28B9386C4D96,
                0xCEDBA04AD0952342, 0x8C2B41A1797F15D1, 0x4B3A639D83414E64, 0x9CA82762AAB78F7, 0x87E8C60FDED7CF9D, 0xC51827E4773DF90E, 0x20905D88D03A2BB, 0x40F9E43324E99428,
                0x2CFFE7D5975E55E2, 0x6E0F063E3EB46371, 0xA91E2402C48A38C4, 0xEBEEC5E96D600E57, 0x65CC8190991CB93D, 0x273C607B30F68FAE, 0xE02D4247CAC8D41B, 0xA2DDA3AC6322E288,
                0xBE992B5F8BDB8C5C, 0xFC69CAB42231BACF, 0x3B78E888D80FE17A, 0x7988096371E5D7E9, 0xF7AA4D1A85996083, 0xB55AACF12C735610, 0x724B8ECDD64D0DA5, 0x30BB6F267FA73B36,
                0x4AC29F2A07BFD00D, 0x8327EC1AE55E69E, 0xCF235CFD546BBD2B, 0x8DD3BD16FD818BB8, 0x3F1F96F09FD3CD2, 0x41011884A0170A41, 0x86103AB85A2951F4, 0xC4E0DB53F3C36767,
                0xD8A453A01B3A09B3, 0x9A54B24BB2D03F20, 0x5D45907748EE6495, 0x1FB5719CE1045206, 0x919735E51578E56C, 0xD367D40EBC92D3FF, 0x1476F63246AC884A, 0x568617D9EF46BED9,
                0xE085162AB69D5E3C, 0xA275F7C11F7768AF, 0x6564D5FDE549331A, 0x279434164CA30589, 0xA9B6706FB8DFB2E3, 0xEB46918411358470, 0x2C57B3B8EB0BDFC5, 0x6EA7525342E1E956,
                0x72E3DAA0AA188782, 0x30133B4B03F2B111, 0xF7021977F9CCEAA4, 0xB5F2F89C5026DC37, 0x3BD0BCE5A45A6B5D, 0x79205D0E0DB05DCE, 0xBE317F32F78E067B, 0xFCC19ED95E6430E8,
                0x86B86ED5267CDBD3, 0xC4488F3E8F96ED40, 0x359AD0275A8B6F5, 0x41A94CE9DC428066, 0xCF8B0890283E370C, 0x8D7BE97B81D4019F, 0x4A6ACB477BEA5A2A, 0x89A2AACD2006CB9,
                0x14DEA25F3AF9026D, 0x562E43B4931334FE, 0x913F6188692D6F4B, 0xD3CF8063C0C759D8, 0x5DEDC41A34BBEEB2, 0x1F1D25F19D51D821, 0xD80C07CD676F8394, 0x9AFCE626CE85B507,
            };
        }

        /// <summary>
        ///   Represents a non-cryptographic hash algorithm.
        /// </summary>
        public abstract class NonCryptographicHashAlgorithm
        {
            /// <summary>
            ///   Gets the number of bytes produced from this hash algorithm.
            /// </summary>
            /// <value>The number of bytes produced from this hash algorithm.</value>
            public int HashLengthInBytes { get; }

            /// <summary>
            ///   Called from constructors in derived classes to initialize the
            ///   <see cref="NonCryptographicHashAlgorithm"/> class.
            /// </summary>
            /// <param name="hashLengthInBytes">
            ///   The number of bytes produced from this hash algorithm.
            /// </param>
            /// <exception cref="ArgumentOutOfRangeException">
            ///   <paramref name="hashLengthInBytes"/> is less than 1.
            /// </exception>
            protected NonCryptographicHashAlgorithm(int hashLengthInBytes)
            {
                if (hashLengthInBytes < 1)
                    throw new ArgumentOutOfRangeException(nameof(hashLengthInBytes));

                HashLengthInBytes = hashLengthInBytes;
            }

            /// <summary>
            ///   When overridden in a derived class,
            ///   appends the contents of <paramref name="source"/> to the data already
            ///   processed for the current hash computation.
            /// </summary>
            /// <param name="source">The data to process.</param>
            public abstract void Append(ReadOnlySpan<byte> source);

            /// <summary>
            ///   When overridden in a derived class,
            ///   resets the hash computation to the initial state.
            /// </summary>
            public abstract void Reset();

            /// <summary>
            ///   When overridden in a derived class,
            ///   writes the computed hash value to <paramref name="destination"/>
            ///   without modifying accumulated state.
            /// </summary>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <remarks>
            ///   <para>
            ///     Implementations of this method must write exactly
            ///     <see cref="HashLengthInBytes"/> bytes to <paramref name="destination"/>.
            ///     Do not assume that the buffer was zero-initialized.
            ///   </para>
            ///   <para>
            ///     The <see cref="NonCryptographicHashAlgorithm"/> class validates the
            ///     size of the buffer before calling this method, and slices the span
            ///     down to be exactly <see cref="HashLengthInBytes"/> in length.
            ///   </para>
            /// </remarks>
            protected abstract void GetCurrentHashCore(Span<byte> destination);

            /// <summary>
            ///   Appends the contents of <paramref name="source"/> to the data already
            ///   processed for the current hash computation.
            /// </summary>
            /// <param name="source">The data to process.</param>
            /// <exception cref="ArgumentNullException">
            ///   <paramref name="source"/> is <see langword="null"/>.
            /// </exception>
            public void Append(byte[] source)
            {
                if (source is null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                Append(new ReadOnlySpan<byte>(source));
            }

            /// <summary>
            ///   Appends the contents of <paramref name="stream"/> to the data already
            ///   processed for the current hash computation.
            /// </summary>
            /// <param name="stream">The data to process.</param>
            /// <exception cref="ArgumentNullException">
            ///   <paramref name="stream"/> is <see langword="null"/>.
            /// </exception>
            /// <seealso cref="AppendAsync(Stream, CancellationToken)"/>
            public void Append(Stream stream)
            {
                if (stream is null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }

                byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);

                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);

                    if (read == 0)
                    {
                        break;
                    }

                    Append(new ReadOnlySpan<byte>(buffer, 0, read));
                }

                ArrayPool<byte>.Shared.Return(buffer);
            }

            /*/// <summary>
            ///   Asychronously reads the contents of <paramref name="stream"/>
            ///   and appends them to the data already
            ///   processed for the current hash computation.
            /// </summary>
            /// <param name="stream">The data to process.</param>
            /// <param name="cancellationToken">
            ///   The token to monitor for cancellation requests.
            ///   The default value is <see cref="CancellationToken.None"/>.
            /// </param>
            /// <returns>
            ///   A task that represents the asynchronous append operation.
            /// </returns>
            /// <exception cref="ArgumentNullException">
            ///   <paramref name="stream"/> is <see langword="null"/>.
            /// </exception>
            public Task AppendAsync(Stream stream, CancellationToken cancellationToken = default)
            {
                if (stream is null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }

                return AppendAsyncCore(stream, cancellationToken);
            }

            private async Task AppendAsyncCore(Stream stream, CancellationToken cancellationToken)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);

                while (true)
                {
#if NETCOREAPP
            int read = await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
#else
                    int read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
#endif

                    if (read == 0)
                    {
                        break;
                    }

                    Append(new ReadOnlySpan<byte>(buffer, 0, read));
                }

                ArrayPool<byte>.Shared.Return(buffer);
            }*/

            /// <summary>
            ///   Gets the current computed hash value without modifying accumulated state.
            /// </summary>
            /// <returns>
            ///   The hash value for the data already provided.
            /// </returns>
            public byte[] GetCurrentHash()
            {
                byte[] ret = new byte[HashLengthInBytes];
                GetCurrentHashCore(ret);
                return ret;
            }

            /// <summary>
            ///   Attempts to write the computed hash value to <paramref name="destination"/>
            ///   without modifying accumulated state.
            /// </summary>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <param name="bytesWritten">
            ///   On success, receives the number of bytes written to <paramref name="destination"/>.
            /// </param>
            /// <returns>
            ///   <see langword="true"/> if <paramref name="destination"/> is long enough to receive
            ///   the computed hash value; otherwise, <see langword="false"/>.
            /// </returns>
            public bool TryGetCurrentHash(Span<byte> destination, out int bytesWritten)
            {
                if (destination.Length < HashLengthInBytes)
                {
                    bytesWritten = 0;
                    return false;
                }

                GetCurrentHashCore(destination.Slice(0, HashLengthInBytes));
                bytesWritten = HashLengthInBytes;
                return true;
            }

            /// <summary>
            ///   Writes the computed hash value to <paramref name="destination"/>
            ///   without modifying accumulated state.
            /// </summary>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <returns>
            ///   The number of bytes written to <paramref name="destination"/>,
            ///   which is always <see cref="HashLengthInBytes"/>.
            /// </returns>
            /// <exception cref="ArgumentException">
            ///   <paramref name="destination"/> is shorter than <see cref="HashLengthInBytes"/>.
            /// </exception>
            public int GetCurrentHash(Span<byte> destination)
            {
                if (destination.Length < HashLengthInBytes)
                {
                    ThrowDestinationTooShort();
                }

                GetCurrentHashCore(destination.Slice(0, HashLengthInBytes));
                return HashLengthInBytes;
            }

            /// <summary>
            ///   Gets the current computed hash value and clears the accumulated state.
            /// </summary>
            /// <returns>
            ///   The hash value for the data already provided.
            /// </returns>
            public byte[] GetHashAndReset()
            {
                byte[] ret = new byte[HashLengthInBytes];
                GetHashAndResetCore(ret);
                return ret;
            }

            /// <summary>
            ///   Attempts to write the computed hash value to <paramref name="destination"/>.
            ///   If successful, clears the accumulated state.
            /// </summary>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <param name="bytesWritten">
            ///   On success, receives the number of bytes written to <paramref name="destination"/>.
            /// </param>
            /// <returns>
            ///   <see langword="true"/> and clears the accumulated state
            ///   if <paramref name="destination"/> is long enough to receive
            ///   the computed hash value; otherwise, <see langword="false"/>.
            /// </returns>
            public bool TryGetHashAndReset(Span<byte> destination, out int bytesWritten)
            {
                if (destination.Length < HashLengthInBytes)
                {
                    bytesWritten = 0;
                    return false;
                }

                GetHashAndResetCore(destination.Slice(0, HashLengthInBytes));
                bytesWritten = HashLengthInBytes;
                return true;
            }

            /// <summary>
            ///   Writes the computed hash value to <paramref name="destination"/>
            ///   then clears the accumulated state.
            /// </summary>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <returns>
            ///   The number of bytes written to <paramref name="destination"/>,
            ///   which is always <see cref="HashLengthInBytes"/>.
            /// </returns>
            /// <exception cref="ArgumentException">
            ///   <paramref name="destination"/> is shorter than <see cref="HashLengthInBytes"/>.
            /// </exception>
            public int GetHashAndReset(Span<byte> destination)
            {
                if (destination.Length < HashLengthInBytes)
                {
                    ThrowDestinationTooShort();
                }

                GetHashAndResetCore(destination.Slice(0, HashLengthInBytes));
                return HashLengthInBytes;
            }

            /// <summary>
            ///   Writes the computed hash value to <paramref name="destination"/>
            ///   then clears the accumulated state.
            /// </summary>
            /// <param name="destination">The buffer that receives the computed hash value.</param>
            /// <remarks>
            ///   <para>
            ///     Implementations of this method must write exactly
            ///     <see cref="HashLengthInBytes"/> bytes to <paramref name="destination"/>.
            ///     Do not assume that the buffer was zero-initialized.
            ///   </para>
            ///   <para>
            ///     The <see cref="NonCryptographicHashAlgorithm"/> class validates the
            ///     size of the buffer before calling this method, and slices the span
            ///     down to be exactly <see cref="HashLengthInBytes"/> in length.
            ///   </para>
            ///   <para>
            ///     The default implementation of this method calls
            ///     <see cref="GetCurrentHashCore"/> followed by <see cref="Reset"/>.
            ///     Overrides of this method do not need to call either of those methods,
            ///     but must ensure that the caller cannot observe a difference in behavior.
            ///   </para>
            /// </remarks>
            protected virtual void GetHashAndResetCore(Span<byte> destination)
            {
                GetCurrentHashCore(destination);
                Reset();
            }

            /// <summary>
            ///   This method is not supported and should not be called.
            ///   Call <see cref="GetCurrentHash()"/> or <see cref="GetHashAndReset()"/>
            ///   instead.
            /// </summary>
            /// <returns>This method will always throw a <see cref="NotSupportedException"/>.</returns>
            /// <exception cref="NotSupportedException">In all cases.</exception>
            [EditorBrowsable(EditorBrowsableState.Never)]
            [Obsolete("Use GetCurrentHash() to retrieve the computed hash code.", true)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
            public override int GetHashCode()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
            {
                throw new NotSupportedException();
            }


            private protected static void ThrowDestinationTooShort() =>
                throw new ArgumentException("destination");
        }
    }
}