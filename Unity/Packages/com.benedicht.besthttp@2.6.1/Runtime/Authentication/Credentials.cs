namespace BestHTTP.Authentication
{
    /// <summary>
    /// Authentication types that supported by BestHTTP.
    /// The authentication is defined by the server, so the Basic and Digest are not interchangeable. If you don't know what to use, the preferred way is to choose Unknow.
    /// </summary>
    public enum AuthenticationTypes
    {
        /// <summary>
        /// If the authentication type is not known this will do a challenge turn to receive what methode should be choosen.
        /// </summary>
        Unknown,

        /// <summary>
        /// The most basic authentication type. It's easy to do, and easy to crack. ;)
        /// </summary>
        Basic,

        /// <summary>
        /// 
        /// </summary>
        Digest
    }

    /// <summary>
    /// Hold all information that required to authenticate to a remote server.
    /// </summary>
    public sealed class Credentials
    {
        /// <summary>
        /// The type of the Authentication. If you don't know what to use, the preferred way is to choose Unknow.
        /// </summary>
        public AuthenticationTypes Type { get; private set; }

        /// <summary>
        /// The username to authenticate on the remote server.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// The password to use in the authentication process. The password will be stored only in this class.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Set up the authentication credentials with the username and password. The Type will be set to Unknown.
        /// </summary>
        public Credentials(string userName, string password)
            :this(AuthenticationTypes.Unknown, userName, password)
        {
        }

        /// <summary>
        /// Set up the authentication credentials with the given authentication type, username and password.
        /// </summary>
        public Credentials(AuthenticationTypes type, string userName, string password)
        {
            this.Type = type;
            this.UserName = userName;
            this.Password = password;
        }
    }
}