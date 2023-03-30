#if !BESTHTTP_DISABLE_SIGNALR_CORE

using System;
using UnityEngine;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using UnityEngine.UI;
using BestHTTP.Examples.Helpers;
#if CSHARP_7_OR_LATER
using System.Threading.Tasks;
#endif

namespace BestHTTP.Examples
{
    // Server side of this example can be found here:
    // https://github.com/Benedicht/BestHTTP_DemoSite/blob/master/BestHTTP_DemoSite/Hubs/TestHub.cs
    public class AsyncTestHubSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649
#pragma warning disable 0414
        [SerializeField]
        private string _path = "/TestHub";

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

#if !CSHARP_7_OR_LATER
            AddText("<color=red>This sample can work only when at least c# 7.3 is supported!</color>");
            SetButtons(false, false);
#else
            SetButtons(true, false);
#endif
        }

#if CSHARP_7_OR_LATER
        async void OnDestroy()
        {
            await hub?.CloseAsync();
        }
#endif

        /// <summary>
        /// GUI button callback
        /// </summary>
        public
#if CSHARP_7_OR_LATER
            async
#endif
        void OnConnectButton()
        {
#if CSHARP_7_OR_LATER
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
            hub = new HubConnection(new Uri(this.sampleSelector.BaseURL + this._path), protocol);

            // Subscribe to hub events
            hub.OnError += Hub_OnError;

            hub.OnTransportEvent += (hub, transport, ev) => AddText(string.Format("Transport(<color=green>{0}</color>) event: <color=green>{1}</color>", transport.TransportType, ev));

            // Set up server callable functions
            hub.On("Send", (string arg) => AddText(string.Format("On '<color=green>Send</color>': '<color=yellow>{0}</color>'", arg)).AddLeftPadding(20));
            hub.On<Person>("Person", (person) => AddText(string.Format("On '<color=green>Person</color>': '<color=yellow>{0}</color>'", person)).AddLeftPadding(20));
            hub.On<Person, Person>("TwoPersons", (person1, person2) => AddText(string.Format("On '<color=green>TwoPersons</color>': '<color=yellow>{0}</color>', '<color=yellow>{1}</color>'", person1, person2)).AddLeftPadding(20));

            AddText("StartConnect called");

            SetButtons(false, false);

            // And finally start to connect to the server
            await hub.ConnectAsync();

            SetButtons(false, true);
            AddText(string.Format("Hub Connected with <color=green>{0}</color> transport using the <color=green>{1}</color> encoder.", hub.Transport.TransportType.ToString(), hub.Protocol.Name));

            // Call a server function with a string param. We expect no return value.
            await hub.SendAsync("Send", "my message");

            // Call a parameterless function. We expect a string return value.
            try
            {
                string result = await hub.InvokeAsync<string>("NoParam");

                AddText(string.Format("'<color=green>NoParam</color>' returned: '<color=yellow>{0}</color>'", result))
                    .AddLeftPadding(20);
            }
            catch (Exception ex)
            {
                AddText(string.Format("'<color=green>NoParam</color>' error: '<color=red>{0}</color>'", ex.Message)).AddLeftPadding(20);
            }

            // Call a function on the server to add two numbers. OnSuccess will be called with the result and OnError if there's an error.
            var addResult = await hub.InvokeAsync<int>("Add", 10, 20);
            AddText(string.Format("'<color=green>Add(10, 20)</color>' returned: '<color=yellow>{0}</color>'", addResult)).AddLeftPadding(20);

            var nullabelTestResult = await hub.InvokeAsync<int?>("NullableTest", 10);
            AddText(string.Format("'<color=green>NullableTest(10)</color>' returned: '<color=yellow>{0}</color>'", nullabelTestResult)).AddLeftPadding(20);

            // Call a function that will return a Person object constructed from the function's parameters.
            var getPersonResult = await hub.InvokeAsync<Person>("GetPerson", "Mr. Smith", 26);
            AddText(string.Format("'<color=green>GetPerson(\"Mr. Smith\", 26)</color>' returned: '<color=yellow>{0}</color>'", getPersonResult)).AddLeftPadding(20);

            // To test errors/exceptions this call always throws an exception on the server side resulting in an OnError call.
            // OnError expected here!

            try
            {
                var singleResultFailureResult = await hub.InvokeAsync<int>("SingleResultFailure", 10, 20);
                AddText(string.Format("'<color=green>SingleResultFailure(10, 20)</color>' returned: '<color=yellow>{0}</color>'", singleResultFailureResult)).AddLeftPadding(20);
            }
            catch (Exception ex)
            {
                AddText(string.Format("'<color=green>SingleResultFailure(10, 20)</color>' error: '<color=red>{0}</color>'", ex.Message)).AddLeftPadding(20);
            }

            // This call demonstrates IEnumerable<> functions, result will be the yielded numbers.
            var batchedResult = await hub.InvokeAsync<int[]>("Batched", 10);
            AddText(string.Format("'<color=green>Batched(10)</color>' returned items: '<color=yellow>{0}</color>'", batchedResult.Length)).AddLeftPadding(20);

            // OnItem is called for a streaming request for every items returned by the server. OnSuccess will still be called with all the items.
            hub.GetDownStreamController<int>("ObservableCounter", 10, 1000)
                .OnItem(result => AddText(string.Format("'<color=green>ObservableCounter(10, 1000)</color>' OnItem: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnSuccess(result => AddText("'<color=green>ObservableCounter(10, 1000)</color>' OnSuccess.").AddLeftPadding(20))
                .OnError(error => AddText(string.Format("'<color=green>ObservableCounter(10, 1000)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // A stream request can be cancelled any time.
            var controller = hub.GetDownStreamController<int>("ChannelCounter", 10, 1000);

            controller.OnItem(result => AddText(string.Format("'<color=green>ChannelCounter(10, 1000)</color>' OnItem: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                      .OnSuccess(result => AddText("'<color=green>ChannelCounter(10, 1000)</color>' OnSuccess.").AddLeftPadding(20))
                      .OnError(error => AddText(string.Format("'<color=green>ChannelCounter(10, 1000)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // a stream can be cancelled by calling the controller's Cancel method
            controller.Cancel();

            // This call will stream strongly typed objects
            hub.GetDownStreamController<Person>("GetRandomPersons", 20, 2000)
                .OnItem(result => AddText(string.Format("'<color=green>GetRandomPersons(20, 1000)</color>' OnItem: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnSuccess(result => AddText("'<color=green>GetRandomPersons(20, 1000)</color>' OnSuccess.").AddLeftPadding(20));
#endif
        }

        /// <summary>
        /// GUI button callback
        /// </summary>
        public
#if CSHARP_7_OR_LATER
            async
#endif
        void OnCloseButton()
        {
#if CSHARP_7_OR_LATER
            if (this.hub != null)
            {
                AddText("Calling CloseAsync");
                SetButtons(false, false);

                await this.hub.CloseAsync();

                SetButtons(true, false);
                AddText("Hub Closed");
            }
#endif
        }

        /// <summary>
        /// Called when an unrecoverable error happen. After this event the hub will not send or receive any messages.
        /// </summary>
        private void Hub_OnError(HubConnection hub, string error)
        {
            SetButtons(true, false);
            AddText(string.Format("Hub Error: <color=red>{0}</color>", error));
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
