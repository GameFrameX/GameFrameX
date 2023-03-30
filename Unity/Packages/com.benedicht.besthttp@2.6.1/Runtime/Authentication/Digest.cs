using System;
using System.Collections.Generic;

namespace BestHTTP.Authentication
{
    using BestHTTP.Extensions;
    using BestHTTP.PlatformSupport.Memory;
    using System.Text;

    /// <summary>
    /// Internal class that stores all information that received from a server in a WWW-Authenticate and need to construct a valid Authorization header. Based on rfc 2617 (http://tools.ietf.org/html/rfc2617).
    /// Used only internally by the plugin.
    /// </summary>
    public sealed class Digest
    {
        #region Public Properties

        /// <summary>
        /// The Uri that this Digest is bound to.
        /// </summary>
        public Uri Uri { get; private set; }

        public AuthenticationTypes Type { get; private set; }

        /// <summary>
        /// A string to be displayed to users so they know which username and password to use.
        /// This string should contain at least the name of the host performing the authentication and might additionally indicate the collection of users who might have access.
        /// </summary>
        public string Realm { get; private set; }

        /// <summary>
        /// A flag, indicating that the previous request from the client was rejected because the nonce value was stale.
        /// If stale is TRUE (case-insensitive), the client may wish to simply retry the request with a new encrypted response, without  the user for a new username and password.
        /// The server should only set stale to TRUE if it receives a request for which the nonce is invalid but with a valid digest for that nonce
        /// (indicating that the client knows the correct username/password).
        /// If stale is FALSE, or anything other than TRUE, or the stale directive is not present, the username and/or password are invalid, and new values must be obtained.
        /// </summary>
        public bool Stale { get; private set; }

        #endregion

        #region Private Properties

        /// <summary>
        /// A server-specified data string which should be uniquely generated each time a 401 response is made.
        /// Specifically, since the string is passed in the header lines as a quoted string, the double-quote character is not allowed.
        /// </summary>
        private string Nonce { get; set; }

        /// <summary>
        /// A string of data, specified by the server, which should be returned by the client unchanged in the Authorization header of subsequent requests with URIs in the same protection space.
        /// It is recommended that this string be base64 or data.
        /// </summary>
        private string Opaque { get; set; }

        /// <summary>
        /// A string indicating a pair of algorithms used to produce the digest and a checksum. If this is not present it is assumed to be "MD5".
        /// If the algorithm is not understood, the challenge should be ignored (and a different one used, if there is more than one).
        /// </summary>
        private string Algorithm { get; set; }

        /// <summary>
        /// List of URIs, as specified in RFC XURI, that define the protection space.
        /// If a URI is an abs_path, it is relative to the canonical root URL (see section 1.2 above) of the server being accessed.
        /// An absoluteURI in this list may refer to a different server than the one being accessed.
        /// The client can use this list to determine the set of URIs for which the same authentication information may be sent:
        /// any URI that has a URI in this list as a prefix (after both have been made absolute) may be assumed to be in the same protection space.
        /// If this directive is omitted or its value is empty, the client should assume that the protection space consists of all URIs on the responding server.
        /// </summary>
        public List<string> ProtectedUris { get; private set; }

        /// <summary>
        /// If present, it is a quoted string of one or more tokens indicating the "quality of protection" values supported by the server.
        /// The value "auth" indicates authentication. The value "auth-int" indicates authentication with integrity protection.
        /// </summary>
        private string QualityOfProtections { get; set; }

        /// <summary>
        /// his MUST be specified if a qop directive is sent (see above), and MUST NOT be specified if the server did not send a qop directive in the WWW-Authenticate header field.
        /// The nc-value is the hexadecimal count of the number of requests (including the current request) that the client has sent with the nonce value in this request.
        /// </summary>
        private int NonceCount { get; set; }

        /// <summary>
        /// Used to store the last HA1 that can be used in the next header generation when Algorithm is set to "md5-sess".
        /// </summary>
        private string HA1Sess { get; set; }

        #endregion

        internal Digest(Uri uri)
        {
            this.Uri = uri;
            this.Algorithm = "md5";
        }

        /// <summary>
        /// Parses a WWW-Authenticate header's value to retrive all information.
        /// </summary>
        public void ParseChallange(string header)
        {
            // Reset some values to its defaults.
            this.Type = AuthenticationTypes.Unknown;
            this.Stale = false;
            this.Opaque = null;
            this.HA1Sess = null;
            this.NonceCount = 0;
            this.QualityOfProtections = null;

            if (this.ProtectedUris != null)
                this.ProtectedUris.Clear();

            // Parse the header
            WWWAuthenticateHeaderParser qpl = new WWWAuthenticateHeaderParser(header);

            // Then process
            foreach (var qp in qpl.Values)
                switch (qp.Key)
                {
                    case "basic": this.Type = AuthenticationTypes.Basic; break;
                    case "digest": this.Type = AuthenticationTypes.Digest; break;
                    case "realm": this.Realm = qp.Value; break;
                    case "domain":
                    {
                        if (string.IsNullOrEmpty(qp.Value) || qp.Value.Length == 0)
                            break;

                        if (this.ProtectedUris == null)
                            this.ProtectedUris = new List<string>();

                        int idx = 0;
                        string val = qp.Value.Read(ref idx, ' ');
                        do
                        {
                            this.ProtectedUris.Add(val);
                            val = qp.Value.Read(ref idx, ' ');
                        } while (idx < qp.Value.Length);

                        break;
                    }
                    case "nonce": this.Nonce = qp.Value; break;
                    case "qop": this.QualityOfProtections = qp.Value; break;
                    case "stale": this.Stale = bool.Parse(qp.Value); break;
                    case "opaque": this.Opaque = qp.Value; break;
                    case "algorithm": this.Algorithm = qp.Value; break;
                }
        }

        /// <summary>
        /// Generates a string that can be set to an Authorization header.
        /// </summary>
        public string GenerateResponseHeader(HTTPRequest request, Credentials credentials, bool isProxy = false)
        {
            try
            {
                switch (Type)
                {
                    case AuthenticationTypes.Basic:
                        return string.Concat("Basic ", Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", credentials.UserName, credentials.Password))));

                    case AuthenticationTypes.Digest:
                    {
                        NonceCount++;

                        string HA1 = string.Empty;

                        // The cnonce-value is an opaque quoted string value provided by the client and used by both client and server to avoid chosen plaintext attacks, to provide mutual
                        //  authentication, and to provide some message integrity protection.
                        string cnonce = new System.Random(request.GetHashCode()).Next(int.MinValue, int.MaxValue).ToString("X8");

                        string ncvalue = NonceCount.ToString("X8");
                        switch (Algorithm.TrimAndLower())
                        {
                            case "md5":
                                HA1 = string.Format("{0}:{1}:{2}", credentials.UserName, Realm, credentials.Password).CalculateMD5Hash();
                                break;

                            case "md5-sess":
                                if (string.IsNullOrEmpty(this.HA1Sess))
                                    this.HA1Sess = string.Format("{0}:{1}:{2}:{3}:{4}", credentials.UserName, Realm, credentials.Password, Nonce, ncvalue).CalculateMD5Hash();
                                HA1 = this.HA1Sess;
                                break;

                            default: //throw new NotSupportedException("Not supported hash algorithm found in Web Authentication: " + Algorithm);
                                return string.Empty;
                        }

                        // A string of 32 hex digits, which proves that the user knows a password. Set according to the qop value.
                        string response = string.Empty;

                        // The server sent QoP-value can be a list of supported methodes(if sent at all - in this case it's null).
                        // The rfc is not specify that this is a space or comma separeted list. So it can be "auth, auth-int" or "auth auth-int".
                        // We will first check the longer value("auth-int") then the short one ("auth"). If one matches we will reset the qop to the exact value.
                        string qop = this.QualityOfProtections != null ? this.QualityOfProtections.TrimAndLower() : null;

                        // When we authenticate with a proxy and we want to tunnel the request, we have to use the CONNECT method instead of the 
                        //  request's, as the proxy will not know about the request itself.
                        string method = isProxy ? "CONNECT" : request.MethodType.ToString().ToUpper();

                        // When we authenticate with a proxy and we want to tunnel the request, the uri must match what we are sending in the CONNECT request's
                        //  Host header.
                        string uri = isProxy ? request.CurrentUri.Host + ":" + request.CurrentUri.Port : request.CurrentUri.GetRequestPathAndQueryURL();

                        if (qop == null)
                        {
                            string HA2 = string.Concat(request.MethodType.ToString().ToUpper(), ":", request.CurrentUri.GetRequestPathAndQueryURL()).CalculateMD5Hash();
                            response = string.Format("{0}:{1}:{2}", HA1, Nonce, HA2).CalculateMD5Hash();
                        }
                        else if (qop.Contains("auth-int"))
                        {
                            qop = "auth-int";

                            byte[] entityBody = request.GetEntityBody();

                            if (entityBody == null)
                                entityBody = BufferPool.NoData; //string.Empty.GetASCIIBytes();

                            string HA2 = string.Format("{0}:{1}:{2}", method, uri, entityBody.CalculateMD5Hash()).CalculateMD5Hash();

                            response = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", HA1, Nonce, ncvalue, cnonce, qop, HA2).CalculateMD5Hash();
                        }
                        else if (qop.Contains("auth"))
                        {
                            qop = "auth";
                            string HA2 = string.Concat(method, ":", uri).CalculateMD5Hash();

                            response = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", HA1, Nonce, ncvalue, cnonce, qop, HA2).CalculateMD5Hash();
                        }
                        else //throw new NotSupportedException("Unrecognized Quality of Protection value found: " + this.QualityOfProtections);
                            return string.Empty;

                        string result = string.Format("Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", cnonce=\"{4}\", response=\"{5}\"",
                                                                credentials.UserName, Realm, Nonce, uri, cnonce, response);

                        if (qop != null)
                            result += String.Concat(", qop=\"", qop, "\", nc=", ncvalue);

                        if (!string.IsNullOrEmpty(Opaque))
                            result = String.Concat(result, ", opaque=\"", Opaque, "\"");

                        return result;
                    }// end of case "digest":

                    default:
                        break;
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        public bool IsUriProtected(Uri uri)
        {
            // http://tools.ietf.org/html/rfc2617#section-3.2.1
            // An absoluteURI in this list may refer to
            // a different server than the one being accessed. The client can use
            // this list to determine the set of URIs for which the same
            // authentication information may be sent: any URI that has a URI in
            // this list as a prefix (after both have been made absolute) may be
            // assumed to be in the same protection space. If this directive is
            // omitted or its value is empty, the client should assume that the
            // protection space consists of all URIs on the responding server.

            if (string.CompareOrdinal(uri.Host, this.Uri.Host) != 0)
                return false;

            string uriStr = uri.ToString();

            if (ProtectedUris != null && ProtectedUris.Count > 0)
                for (int i = 0; i < ProtectedUris.Count; ++i)
                    if (uriStr.Contains(ProtectedUris[i]))
                        return true;


            return true;
        }
    }
}