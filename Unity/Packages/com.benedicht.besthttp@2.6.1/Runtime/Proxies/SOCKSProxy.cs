#if !BESTHTTP_DISABLE_PROXY
using System;
using System.IO;
using System.Text;
using BestHTTP.Authentication;
using BestHTTP.Extensions;
using BestHTTP.PlatformSupport.Memory;

namespace BestHTTP
{
    internal enum SOCKSVersions : byte
    {
        Unknown = 0x00,
        V5 = 0x05
    }

    /// <summary>
    /// https://tools.ietf.org/html/rfc1928
    ///   The values currently defined for METHOD are:
    ///     o  X'00' NO AUTHENTICATION REQUIRED
    ///     o  X'01' GSSAPI
    ///     o  X'02' USERNAME/PASSWORD
    ///     o  X'03' to X'7F' IANA ASSIGNED
    ///     o  X'80' to X'FE' RESERVED FOR PRIVATE METHODS
    ///     o  X'FF' NO ACCEPTABLE METHODS
    /// </summary>
    internal enum SOCKSMethods : byte
    {
        NoAuthenticationRequired = 0x00,
        GSSAPI = 0x01,
        UsernameAndPassword = 0x02,
        NoAcceptableMethods = 0xFF
    }

    internal enum SOCKSReplies : byte
    {
        Succeeded = 0x00,
        GeneralSOCKSServerFailure = 0x01,
        ConnectionNotAllowedByRuleset = 0x02,
        NetworkUnreachable = 0x03,
        HostUnreachable = 0x04,
        ConnectionRefused = 0x05,
        TTLExpired = 0x06,
        CommandNotSupported = 0x07,
        AddressTypeNotSupported = 0x08
    }

    internal enum SOCKSAddressTypes
    {
        IPV4 = 0x00,
        DomainName = 0x03,
        IPv6 = 0x04
    }

    public sealed class SOCKSProxy : Proxy
    {
        public SOCKSProxy(Uri address, Credentials credentials)
            : base(address, credentials)
        { }

        internal override string GetRequestPath(Uri uri)
        {
            return uri.GetRequestPathAndQueryURL();
        }

        internal override bool SetupRequest(HTTPRequest request)
        {
            return false;
        }

        internal override void Connect(Stream stream, HTTPRequest request)
        {
            var buffer = BufferPool.Get(1024, true);
            try
            {
                int count = 0;

                // https://tools.ietf.org/html/rfc1928
                //   The client connects to the server, and sends a version
                //   identifier/method selection message:
                //
                //                   +----+----------+----------+
                //                   |VER | NMETHODS | METHODS  |
                //                   +----+----------+----------+
                //                   | 1  |    1     | 1 to 255 |
                //                   +----+----------+----------+
                //
                //   The VER field is set to X'05' for this version of the protocol.  The
                //   NMETHODS field contains the number of method identifier octets that
                //   appear in the METHODS field.
                //

                buffer[count++] = (byte)SOCKSVersions.V5;
                if (this.Credentials != null)
                {
                    buffer[count++] = 0x02; // method count
                    buffer[count++] = (byte)SOCKSMethods.UsernameAndPassword;
                    buffer[count++] = (byte)SOCKSMethods.NoAuthenticationRequired;
                }
                else
                {
                    buffer[count++] = 0x01; // method count
                    buffer[count++] = (byte)SOCKSMethods.NoAuthenticationRequired;
                }

                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Information("SOCKSProxy", string.Format("Sending method negotiation - count: {0} buffer: {1} ", count.ToString(), BufferToHexStr(buffer, count)), request.Context);

                // Write negotiation
                stream.Write(buffer, 0, count);
                // Read result
                count = stream.Read(buffer, 0, buffer.Length);

                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Information("SOCKSProxy", string.Format("Negotiation response - count: {0} buffer: {1} ", count.ToString(), BufferToHexStr(buffer, count)), request.Context);

                //   The server selects from one of the methods given in METHODS, and
                //   sends a METHOD selection message:
                //
                //                         +----+--------+
                //                         |VER | METHOD |
                //                         +----+--------+
                //                         | 1  |   1    |
                //                         +----+--------+
                //
                //   If the selected METHOD is X'FF', none of the methods listed by the
                //   client are acceptable, and the client MUST close the connection.
                //
                //   The values currently defined for METHOD are:
                //
                //          o  X'00' NO AUTHENTICATION REQUIRED
                //          o  X'01' GSSAPI
                //          o  X'02' USERNAME/PASSWORD
                //          o  X'03' to X'7F' IANA ASSIGNED
                //          o  X'80' to X'FE' RESERVED FOR PRIVATE METHODS
                //          o  X'FF' NO ACCEPTABLE METHODS
                //
                //   The client and server then enter a method-specific sub-negotiation.

                SOCKSVersions version = (SOCKSVersions)buffer[0];
                SOCKSMethods method = (SOCKSMethods)buffer[1];

                // Expected result:
                //  1.) Received bytes' count is 2: version + preferred method
                //  2.) Version must be 5
                //  3.) Preferred method must NOT be 0xFF
                if (count != 2)
                    throw new Exception(string.Format("SOCKS Proxy - Expected read count: 2! count: {0} buffer: {1}" + count.ToString(), BufferToHexStr(buffer, count)));
                else if (version != SOCKSVersions.V5)
                    throw new Exception("SOCKS Proxy - Expected version: 5, received version: " + buffer[0].ToString("X2"));
                else if (method == SOCKSMethods.NoAcceptableMethods)
                    throw new Exception("SOCKS Proxy - Received 'NO ACCEPTABLE METHODS' (0xFF)");
                else
                {
                    HTTPManager.Logger.Information("SOCKSProxy", "Method negotiation over. Method: " + method.ToString(), request.Context);
                    switch (method)
                    {
                        case SOCKSMethods.NoAuthenticationRequired:
                            // nothing to do
                            break;

                        case SOCKSMethods.UsernameAndPassword:
                            if (this.Credentials.UserName.Length > 255)
                                throw new Exception(string.Format("SOCKS Proxy - Credentials.UserName too long! {0} > 255", this.Credentials.UserName.Length.ToString()));
                            if (this.Credentials.Password.Length > 255)
                                throw new Exception(string.Format("SOCKS Proxy - Credentials.Password too long! {0} > 255", this.Credentials.Password.Length.ToString()));

                            // https://tools.ietf.org/html/rfc1929 : Username/Password Authentication for SOCKS V5
                            //   Once the SOCKS V5 server has started, and the client has selected the
                            //   Username/Password Authentication protocol, the Username/Password
                            //   subnegotiation begins.  This begins with the client producing a
                            //   Username/Password request:
                            //
                            //           +----+------+----------+------+----------+
                            //           |VER | ULEN |  UNAME   | PLEN |  PASSWD  |
                            //           +----+------+----------+------+----------+
                            //           | 1  |  1   | 1 to 255 |  1   | 1 to 255 |
                            //           +----+------+----------+------+----------+

                            HTTPManager.Logger.Information("SOCKSProxy", "starting sub-negotiation", request.Context);
                            count = 0;
                            buffer[count++] = 0x01; // version of sub negotiation

                            WriteString(buffer, ref count, this.Credentials.UserName);
                            WriteString(buffer, ref count, this.Credentials.Password);

                            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                                HTTPManager.Logger.Information("SOCKSProxy", string.Format("Sending username and password sub-negotiation - count: {0} buffer: {1} ", count.ToString(), BufferToHexStr(buffer, count)), request.Context);

                            // Write negotiation
                            stream.Write(buffer, 0, count);
                            // Read result
                            count = stream.Read(buffer, 0, buffer.Length);

                            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                                HTTPManager.Logger.Information("SOCKSProxy", string.Format("Username and password sub-negotiation response - count: {0} buffer: {1} ", count.ToString(), BufferToHexStr(buffer, count)), request.Context);

                            //   The server verifies the supplied UNAME and PASSWD, and sends the
                            //   following response:
                            //
                            //                        +----+--------+
                            //                        |VER | STATUS |
                            //                        +----+--------+
                            //                        | 1  |   1    |
                            //                        +----+--------+

                            // A STATUS field of X'00' indicates success. If the server returns a
                            // `failure' (STATUS value other than X'00') status, it MUST close the
                            // connection.
                            bool success = buffer[1] == 0;

                            if (count != 2)
                                throw new Exception(string.Format("SOCKS Proxy - Expected read count: 2! count: {0} buffer: {1}" + count.ToString(), BufferToHexStr(buffer, count)));
                            else if (!success)
                                throw new Exception("SOCKS proxy: username+password authentication failed!");

                            HTTPManager.Logger.Information("SOCKSProxy", "Authenticated!", request.Context);
                            break;

                        case SOCKSMethods.GSSAPI:
                            throw new Exception("SOCKS proxy: GSSAPI not supported!");

                        case SOCKSMethods.NoAcceptableMethods:
                            throw new Exception("SOCKS proxy: No acceptable method");
                    }
                }

                //   The SOCKS request is formed as follows:
                //
                //        +----+-----+-------+------+----------+----------+
                //        |VER | CMD |  RSV  | ATYP | DST.ADDR | DST.PORT |
                //        +----+-----+-------+------+----------+----------+
                //        | 1  |  1  | X'00' |  1   | Variable |    2     |
                //        +----+-----+-------+------+----------+----------+
                //
                //     Where:
                //
                //          o  VER    protocol version: X'05'
                //          o  CMD
                //             o  CONNECT X'01'
                //             o  BIND X'02'
                //             o  UDP ASSOCIATE X'03'
                //          o  RSV    RESERVED
                //          o  ATYP   address type of following address
                //             o  IP V4 address: X'01'
                //             o  DOMAINNAME: X'03'
                //             o  IP V6 address: X'04'
                //          o  DST.ADDR       desired destination address
                //          o  DST.PORT desired destination port in network octet
                //             order

                count = 0;
                buffer[count++] = (byte)SOCKSVersions.V5; // version: 5
                buffer[count++] = 0x01; // command: connect
                buffer[count++] = 0x00; // reserved, bust be 0x00

                if (request.CurrentUri.IsHostIsAnIPAddress())
                {
                    bool isIPV4 = Extensions.Extensions.IsIpV4AddressValid(request.CurrentUri.Host);
                    buffer[count++] = isIPV4 ? (byte)SOCKSAddressTypes.IPV4 : (byte)SOCKSAddressTypes.IPv6;

                    var ipAddress = System.Net.IPAddress.Parse(request.CurrentUri.Host);
                    var ipBytes = ipAddress.GetAddressBytes();
                    WriteBytes(buffer, ref count, ipBytes); // destination address
                }
                else
                {
                    buffer[count++] = (byte)SOCKSAddressTypes.DomainName;

                    // The first octet of the address field contains the number of octets of name that
                    // follow, there is no terminating NUL octet.
                    WriteString(buffer, ref count, request.CurrentUri.Host);
                }

                // destination port in network octet order
                buffer[count++] = (byte)((request.CurrentUri.Port >> 8) & 0xFF);
                buffer[count++] = (byte)(request.CurrentUri.Port & 0xFF);

                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Information("SOCKSProxy", string.Format("Sending connect request - count: {0} buffer: {1} ", count.ToString(), BufferToHexStr(buffer, count)), request.Context);

                stream.Write(buffer, 0, count);
                count = stream.Read(buffer, 0, buffer.Length);

                if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                    HTTPManager.Logger.Information("SOCKSProxy", string.Format("Connect response - count: {0} buffer: {1} ", count.ToString(), BufferToHexStr(buffer, count)), request.Context);

                //   The SOCKS request information is sent by the client as soon as it has
                //   established a connection to the SOCKS server, and completed the
                //   authentication negotiations.  The server evaluates the request, and
                //   returns a reply formed as follows:
                //
                //        +----+-----+-------+------+----------+----------+
                //        |VER | REP |  RSV  | ATYP | BND.ADDR | BND.PORT |
                //        +----+-----+-------+------+----------+----------+
                //        | 1  |  1  | X'00' |  1   | Variable |    2     |
                //        +----+-----+-------+------+----------+----------+
                //
                //     Where:
                //          o  VER    protocol version: X'05'
                //          o  REP    Reply field:
                //             o  X'00' succeeded
                //             o  X'01' general SOCKS server failure
                //             o  X'02' connection not allowed by ruleset
                //             o  X'03' Network unreachable
                //             o  X'04' Host unreachable
                //             o  X'05' Connection refused
                //             o  X'06' TTL expired
                //             o  X'07' Command not supported
                //             o  X'08' Address type not supported
                //             o  X'09' to X'FF' unassigned
                //          o  RSV    RESERVED
                //          o  ATYP   address type of following address
                //             o  IP V4 address: X'01'
                //             o  DOMAINNAME: X'03'
                //             o  IP V6 address: X'04'
                //          o  BND.ADDR       server bound address
                //          o  BND.PORT       server bound port in network octet order
                //
                //   Fields marked RESERVED (RSV) must be set to X'00'.

                version = (SOCKSVersions)buffer[0];
                SOCKSReplies reply = (SOCKSReplies)buffer[1];

                // at least 10 bytes expected as a result
                if (count < 10)
                    throw new Exception(string.Format("SOCKS proxy: not enough data returned by the server. Expected count is at least 10 bytes, server returned {0} bytes! content: {1}", count.ToString(), BufferToHexStr(buffer, count)));
                else if (reply != SOCKSReplies.Succeeded)
                    throw new Exception("SOCKS proxy error: " + reply.ToString());

                HTTPManager.Logger.Information("SOCKSProxy", "Connected!", request.Context);
            }
            finally
            {
                BufferPool.Release(buffer);
            }
        }

        private void WriteString(byte[] buffer, ref int count, string str)
        {
            // Get the bytes
            int byteCount = Encoding.UTF8.GetByteCount(str);
            if (byteCount > 255)
                throw new Exception(string.Format("SOCKS Proxy - String is too large ({0}) to fit in 255 bytes!", byteCount.ToString()));

            // number of bytes
            buffer[count++] = (byte)byteCount;

            // and the bytes itself
            Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, count);

            count += byteCount;
        }

        private void WriteBytes(byte[] buffer, ref int count, byte[] bytes)
        {
            Array.Copy(bytes, 0, buffer, count, bytes.Length);
            count += bytes.Length;
        }

        private string BufferToHexStr(byte[] buffer, int count)
        {
            StringBuilder sb = new StringBuilder(count * 2);
            for (int i = 0; i < count; ++i)
                sb.AppendFormat("0x{0} ", buffer[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
#endif
