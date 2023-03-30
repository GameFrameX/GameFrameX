#if !BESTHTTP_DISABLE_SIGNALR_CORE

using BestHTTP.Examples;
using BestHTTP.Examples.Helpers;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
    /// <summary>
    /// A sample to demonstrate Bearer token authorization on the server. The client will connect to the /redirect route
    /// where it will receive the token and will receive the new url (/HubWithAuthorization) to connect to.
    /// HubWithAuthorization without the token would throw an error.
    /// </summary>
    public sealed class HubWithAuthorizationSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649

        [SerializeField]
        private string _path = "/redirect";

        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private RectTransform _contentRoot;

        [SerializeField]
        private TextListItem _listItemPrefab;

        [SerializeField]
        private int _maxListItemEntries = 100;

        [SerializeField]
        private Button _connectButton;

        [SerializeField]
        private Button _closeButton;

#pragma warning restore

        // Instance of the HubConnection
        HubConnection hub;

        protected override void Start()
        {
            base.Start();

            SetButtons(true, false);
        }

        void OnDestroy()
        {
            if (hub != null)
                hub.StartClose();
        }

        public void OnConnectButton()
        {
            // Server side of this example can be found here:
            // https://github.com/Benedicht/BestHTTP_DemoSite/blob/master/BestHTTP_DemoSite/Hubs/

#if BESTHTTP_SIGNALR_CORE_ENABLE_MESSAGEPACK_CSHARP
            try
            {
                MessagePack.Resolvers.StaticCompositeResolver.Instance.Register(
                    MessagePack.Resolvers.DynamicEnumAsStringResolver.Instance,
                    MessagePack.Unity.UnityResolver.Instance,
                    //MessagePack.Unity.Extension.UnityBlitWithPrimitiveArrayResolver.Instance,
                    //MessagePack.Resolvers.StandardResolver.Instance,
                    MessagePack.Resolvers.ContractlessStandardResolver.Instance
                );

                var options = MessagePack.MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.StaticCompositeResolver.Instance);
                MessagePack.MessagePackSerializer.DefaultOptions = options;
            }
            catch
            { }
#endif

            IProtocol protocol = null;
#if BESTHTTP_SIGNALR_CORE_ENABLE_MESSAGEPACK_CSHARP
            protocol = new MessagePackCSharpProtocol();
#elif BESTHTTP_SIGNALR_CORE_ENABLE_GAMEDEVWARE_MESSAGEPACK
            protocol = new MessagePackProtocol();
#else
            protocol = new JsonProtocol(new LitJsonEncoder());
#endif

            // Crete the HubConnection
            hub = new HubConnection(new Uri(base.sampleSelector.BaseURL + this._path), protocol);

            // Subscribe to hub events
            hub.OnConnected += Hub_OnConnected;
            hub.OnError += Hub_OnError;
            hub.OnClosed += Hub_OnClosed;

            hub.OnRedirected += Hub_Redirected;

            hub.OnTransportEvent += (hub, transport, ev) => AddText(string.Format("Transport(<color=green>{0}</color>) event: <color=green>{1}</color>", transport.TransportType, ev));

            // And finally start to connect to the server
            hub.StartConnect();

            AddText("StartConnect called");
            SetButtons(false, false);
        }

        public void OnCloseButton()
        {
            if (hub != null)
            {
                hub.StartClose();

                AddText("StartClose called");
                SetButtons(false, false);
            }
        }

        private void Hub_Redirected(HubConnection hub, Uri oldUri, Uri newUri)
        {
            AddText(string.Format("Hub connection redirected to '<color=green>{0}</color>' with Access Token: '<color=green>{1}</color>'", hub.Uri, hub.NegotiationResult.AccessToken));
        }

        /// <summary>
        /// This callback is called when the plugin is connected to the server successfully. Messages can be sent to the server after this point.
        /// </summary>
        private void Hub_OnConnected(HubConnection hub)
        {
            AddText(string.Format("Hub Connected with <color=green>{0}</color> transport using the <color=green>{1}</color> encoder.", hub.Transport.TransportType.ToString(), hub.Protocol.Name));
            SetButtons(false, true);

            // Call a parameterless function. We expect a string return value.
            hub.Invoke<string>("Echo", "Message from the client")
                .OnSuccess(ret => AddText(string.Format("'<color=green>Echo</color>' returned: '<color=yellow>{0}</color>'", ret)).AddLeftPadding(20));

            AddText("'<color=green>Message from the client</color>' sent!")
                .AddLeftPadding(20);
        }
        
        /// <summary>
        /// This is called when the hub is closed after a StartClose() call.
        /// </summary>
        private void Hub_OnClosed(HubConnection hub)
        {
            AddText("Hub Closed");
            SetButtons(true, false);
        }

        /// <summary>
        /// Called when an unrecoverable error happen. After this event the hub will not send or receive any messages.
        /// </summary>
        private void Hub_OnError(HubConnection hub, string error)
        {
            AddText(string.Format("Hub Error: <color=red>{0}</color>", error));
            SetButtons(true, false);
        }

        private void SetButtons(bool connect, bool close)
        {
            if (this._connectButton != null)
                this._connectButton.interactable = connect;

            if (this._closeButton != null)
                this._closeButton.interactable = close;
        }

        private TextListItem AddText(string text)
        {
            return GUIHelper.AddText(this._listItemPrefab, this._contentRoot, text, this._maxListItemEntries, this._scrollRect);
        }
    }
}

#endif
