#if (!UNITY_WEBGL || UNITY_EDITOR) && !BESTHTTP_DISABLE_ALTERNATE_SSL && !BESTHTTP_DISABLE_HTTP2

using System;
using System.Collections.Generic;

namespace BestHTTP.Connections.HTTP2
{
    // https://httpwg.org/specs/rfc7540.html#iana-settings
    public enum HTTP2Settings : ushort
    {
        /// <summary>
        /// Allows the sender to inform the remote endpoint of the maximum size of the
        /// header compression table used to decode header blocks, in octets.
        /// The encoder can select any size equal to or less than this value
        /// by using signaling specific to the header compression format inside a header block (see [COMPRESSION]).
        /// The initial value is 4,096 octets.
        /// </summary>
        HEADER_TABLE_SIZE = 0x01,

        /// <summary>
        /// This setting can be used to disable server push (Section 8.2).
        /// An endpoint MUST NOT send a PUSH_PROMISE frame if it receives this parameter set to a value of 0.
        /// An endpoint that has both set this parameter to 0 and had it acknowledged MUST treat the receipt of a
        /// PUSH_PROMISE frame as a connection error (Section 5.4.1) of type PROTOCOL_ERROR.
        /// 
        /// The initial value is 1, which indicates that server push is permitted.
        /// Any value other than 0 or 1 MUST be treated as a connection error (Section 5.4.1) of type PROTOCOL_ERROR.
        /// </summary>
        ENABLE_PUSH = 0x02,

        /// <summary>
        /// Indicates the maximum number of concurrent streams that the sender will allow. This limit is directional:
        /// it applies to the number of streams that the sender permits the receiver to create.
        /// Initially, there is no limit to this value. It is recommended that this value be no smaller than 100,
        /// so as to not unnecessarily limit parallelism.
        /// 
        /// A value of 0 for SETTINGS_MAX_CONCURRENT_STREAMS SHOULD NOT be treated as special by endpoints.
        /// A zero value does prevent the creation of new streams;
        /// however, this can also happen for any limit that is exhausted with active streams.
        /// Servers SHOULD only set a zero value for short durations; if a server does not wish to accept requests,
        /// closing the connection is more appropriate.
        /// </summary>
        MAX_CONCURRENT_STREAMS = 0x03,

        /// <summary>
        /// Indicates the sender's initial window size (in octets) for stream-level flow control.
        /// The initial value is 2^16-1 (65,535) octets.
        ///
        /// This setting affects the window size of all streams (see Section 6.9.2).
        ///
        /// Values above the maximum flow-control window size of 2^31-1 MUST be treated as a connection error
        /// (Section 5.4.1) of type FLOW_CONTROL_ERROR.
        /// </summary>
        INITIAL_WINDOW_SIZE = 0x04,

        /// <summary>
        /// Indicates the size of the largest frame payload that the sender is willing to receive, in octets.
        ///
        /// The initial value is 2^14 (16,384) octets.
        /// The value advertised by an endpoint MUST be between this initial value and the maximum allowed frame size
        /// (2^24-1 or 16,777,215 octets), inclusive.
        /// Values outside this range MUST be treated as a connection error (Section 5.4.1) of type PROTOCOL_ERROR.
        /// </summary>
        MAX_FRAME_SIZE = 0x05,

        /// <summary>
        /// This advisory setting informs a peer of the maximum size of header list that the sender is prepared to accept, in octets.
        /// The value is based on the uncompressed size of header fields,
        /// including the length of the name and value in octets plus an overhead of 32 octets for each header field.
        ///
        /// For any given request, a lower limit than what is advertised MAY be enforced. The initial value of this setting is unlimited.
        /// </summary>
        MAX_HEADER_LIST_SIZE = 0x06,

        RESERVED = 0x07,

        /// <summary>
        /// https://tools.ietf.org/html/rfc8441
        ///  Upon receipt of SETTINGS_ENABLE_CONNECT_PROTOCOL with a value of 1, a client MAY use the Extended CONNECT as defined in this document when creating new streams.
        ///  Receipt of this parameter by a server does not have any impact.
        ///  
        ///  A sender MUST NOT send a SETTINGS_ENABLE_CONNECT_PROTOCOL parameter with the value of 0 after previously sending a value of 1.
        /// </summary>
        ENABLE_CONNECT_PROTOCOL = 0x08
    }

    public sealed class HTTP2SettingsRegistry
    {
        public bool IsReadOnly { get; private set; }
        public Action<HTTP2SettingsRegistry, HTTP2Settings, UInt32, UInt32> OnSettingChangedEvent;

        private UInt32[] values;
        private bool[] changeFlags;

        public UInt32 this[HTTP2Settings setting]
        {
            get { return this.values[(ushort)setting]; }

            set
            {
                if (this.IsReadOnly)
                    throw new NotSupportedException("It's a read-only one!");

                ushort idx = (ushort)setting;

                // https://httpwg.org/specs/rfc7540.html#SettingValues
                // An endpoint that receives a SETTINGS frame with any unknown or unsupported identifier MUST ignore that setting.
                if (idx == 0 || idx >= this.values.Length)
                    return;

                UInt32 oldValue = this.values[idx];
                if (oldValue != value)
                {
                    this.values[idx] = value;
                    this.changeFlags[idx] = true;
                    IsChanged = true;

                    if (this.OnSettingChangedEvent != null)
                        this.OnSettingChangedEvent(this, setting, oldValue, value);
                }
            }
        }

        public bool IsChanged { get; private set; }

        private HTTP2SettingsManager _parent;

        public HTTP2SettingsRegistry(HTTP2SettingsManager parent, bool readOnly, bool treatItAsAlreadyChanged)
        {
            this._parent = parent;

            this.values = new UInt32[HTTP2SettingsManager.SettingsCount];

            this.IsReadOnly = readOnly;
            if (!this.IsReadOnly)
                this.changeFlags = new bool[HTTP2SettingsManager.SettingsCount];

            // Set default values (https://httpwg.org/specs/rfc7540.html#iana-settings)
            this.values[(UInt16)HTTP2Settings.HEADER_TABLE_SIZE] = 4096;
            this.values[(UInt16)HTTP2Settings.ENABLE_PUSH] = 1;
            this.values[(UInt16)HTTP2Settings.MAX_CONCURRENT_STREAMS] = 128;
            this.values[(UInt16)HTTP2Settings.INITIAL_WINDOW_SIZE] = 65535;
            this.values[(UInt16)HTTP2Settings.MAX_FRAME_SIZE] = 16384;
            this.values[(UInt16)HTTP2Settings.MAX_HEADER_LIST_SIZE] = UInt32.MaxValue; // infinite

            if (this.IsChanged = treatItAsAlreadyChanged)
            {
                this.changeFlags[(UInt16)HTTP2Settings.MAX_CONCURRENT_STREAMS] = true;
            }
        }

        public void Merge(List<KeyValuePair<HTTP2Settings, UInt32>> settings)
        {
            if (settings == null)
                return;

            for (int i = 0; i < settings.Count; ++i)
            {
                HTTP2Settings setting = settings[i].Key;
                UInt16 key = (UInt16)setting;
                UInt32 value = settings[i].Value;

                if (key > 0 && key <= HTTP2SettingsManager.SettingsCount)
                {
                    UInt32 oldValue = this.values[key];
                    this.values[key] = value;

                    if (oldValue != value && this.OnSettingChangedEvent != null)
                        this.OnSettingChangedEvent(this, setting, oldValue, value);

                    if (HTTPManager.Logger.Level <= Logger.Loglevels.All)
                        HTTPManager.Logger.Information("HTTP2SettingsRegistry", string.Format("Merge {0}({1}) = {2}", setting, key, value), this._parent.Parent.Context);
                }
            }
        }

        public void Merge(HTTP2SettingsRegistry from)
        {
            if (this.values != null)
                this.values = new uint[from.values.Length];

            for (int i = 0; i < this.values.Length; ++i)
                this.values[i] = from.values[i];
        }

        internal HTTP2FrameHeaderAndPayload CreateFrame()
        {
            List<KeyValuePair<HTTP2Settings, UInt32>> keyValuePairs = new List<KeyValuePair<HTTP2Settings, uint>>(HTTP2SettingsManager.SettingsCount);

            for (int i = 1; i < HTTP2SettingsManager.SettingsCount; ++i)
                if (this.changeFlags[i])
                {
                    keyValuePairs.Add(new KeyValuePair<HTTP2Settings, uint>((HTTP2Settings)i, this[(HTTP2Settings)i]));
                    this.changeFlags[i] = false;
                }

            this.IsChanged = false;

            return HTTP2FrameHelper.CreateSettingsFrame(keyValuePairs);
        }
    }

    public sealed class HTTP2SettingsManager
    {
        public static readonly int SettingsCount = Enum.GetNames(typeof(HTTP2Settings)).Length + 1;

        /// <summary>
        /// This is the ACKd or default settings that we sent to the server.
        /// </summary>
        public HTTP2SettingsRegistry MySettings { get; private set; }

        /// <summary>
        /// This is the setting that can be changed. It will be sent to the server ASAP, and when ACKd, it will be copied
        /// to MySettings.
        /// </summary>
        public HTTP2SettingsRegistry InitiatedMySettings { get; private set; }

        /// <summary>
        /// Settings of the remote peer
        /// </summary>
        public HTTP2SettingsRegistry RemoteSettings { get; private set; }

        public DateTime SettingsChangesSentAt { get; private set; }

        public HTTP2Handler Parent { get; private set; }

        public HTTP2SettingsManager(HTTP2Handler parentHandler)
        {
            this.Parent = parentHandler;

            this.MySettings = new HTTP2SettingsRegistry(this, readOnly: true, treatItAsAlreadyChanged: false);
            this.InitiatedMySettings = new HTTP2SettingsRegistry(this, readOnly: false, treatItAsAlreadyChanged: true);
            this.RemoteSettings = new HTTP2SettingsRegistry(this, readOnly: true, treatItAsAlreadyChanged: false);
            this.SettingsChangesSentAt = DateTime.MinValue;
        }

        internal void Process(HTTP2FrameHeaderAndPayload frame, List<HTTP2FrameHeaderAndPayload> outgoingFrames)
        {
            if (frame.Type != HTTP2FrameTypes.SETTINGS)
                return;

            HTTP2SettingsFrame settingsFrame = HTTP2FrameHelper.ReadSettings(frame);

            if (HTTPManager.Logger.Level <= Logger.Loglevels.Information)
                HTTPManager.Logger.Information("HTTP2SettingsManager", "Processing Settings frame: " + settingsFrame.ToString(), this.Parent.Context);

            if ((settingsFrame.Flags & HTTP2SettingsFlags.ACK) == HTTP2SettingsFlags.ACK)
            {
                this.MySettings.Merge(this.InitiatedMySettings);
                this.SettingsChangesSentAt = DateTime.MinValue;
            }
            else
            {
                this.RemoteSettings.Merge(settingsFrame.Settings);
                outgoingFrames.Add(HTTP2FrameHelper.CreateACKSettingsFrame());
            }
        }

        internal void SendChanges(List<HTTP2FrameHeaderAndPayload> outgoingFrames)
        {
            if (this.SettingsChangesSentAt != DateTime.MinValue && DateTime.UtcNow - this.SettingsChangesSentAt >= TimeSpan.FromSeconds(10))
            {
                HTTPManager.Logger.Error("HTTP2SettingsManager", "No ACK received for settings frame!", this.Parent.Context);
                this.SettingsChangesSentAt = DateTime.MinValue;
            }

            //  Upon receiving a SETTINGS frame with the ACK flag set, the sender of the altered parameters can rely on the setting having been applied.
            if (!this.InitiatedMySettings.IsChanged)
                return;

            outgoingFrames.Add(this.InitiatedMySettings.CreateFrame());
            this.SettingsChangesSentAt = DateTime.UtcNow;
        }
    }
}

#endif
