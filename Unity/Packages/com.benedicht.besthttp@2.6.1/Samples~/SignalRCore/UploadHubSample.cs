#if !BESTHTTP_DISABLE_SIGNALR_CORE

using BestHTTP;
using BestHTTP.Examples.Helpers;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples
{
    /// <summary>
    /// This sample demonstrates redirection capabilities. The server will redirect a few times the client before
    /// routing it to the final endpoint.
    /// </summary>
    public sealed class UploadHubSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649

        [SerializeField]
        private string _path = "/uploading";

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

        [SerializeField]
        private float _yieldWaitTime = 0.1f;

#pragma warning restore

        // Instance of the HubConnection
        private HubConnection hub;

        protected override void Start()
        {
            base.Start();

            SetButtons(true, false);
        }

        void OnDestroy()
        {
            if (hub != null)
            {
                hub.StartClose();
            }
        }

        public void OnConnectButton()
        {
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
            if (this.hub != null)
            {
                this.hub.StartClose();

                AddText("StartClose called");
                SetButtons(false, false);
            }
        }

        private void Hub_Redirected(HubConnection hub, Uri oldUri, Uri newUri)
        {
            AddText(string.Format("Hub connection redirected to '<color=green>{0}</color>'!", hub.Uri));
        }

        /// <summary>
        /// This callback is called when the plugin is connected to the server successfully. Messages can be sent to the server after this point.
        /// </summary>
        private void Hub_OnConnected(HubConnection hub)
        {
            AddText(string.Format("Hub Connected with <color=green>{0}</color> transport using the <color=green>{1}</color> encoder.", hub.Transport.TransportType.ToString(), hub.Protocol.Name));

            StartCoroutine(UploadWord());

            SetButtons(false, true);
        }

        private IEnumerator UploadWord()
        {
            AddText("<color=green>UploadWord</color>:");

            var controller = hub.GetUpStreamController<string, string>("UploadWord");
            controller.OnSuccess(result =>
            {
                AddText(string.Format("UploadWord completed, result: '<color=yellow>{0}</color>'", result))
                    .AddLeftPadding(20);
                AddText("");

                StartCoroutine(ScoreTracker());
            });

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.UploadParam("Hello ");

            AddText("'<color=green>Hello </color>' uploaded!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.UploadParam("World");

            AddText("'<color=green>World</color>' uploaded!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.UploadParam("!!");

            AddText("'<color=green>!!</color>' uploaded!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);

            controller.Finish();

            AddText("Sent upload finished message.")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
        }

        private IEnumerator ScoreTracker()
        {
            AddText("<color=green>ScoreTracker</color>:");
            var controller = hub.GetUpStreamController<string, int, int>("ScoreTracker");

            controller.OnSuccess(result =>
            {
                AddText(string.Format("ScoreTracker completed, result: '<color=yellow>{0}</color>'", result))
                    .AddLeftPadding(20);
                AddText("");

                StartCoroutine(ScoreTrackerWithParameterChannels());
            });

            const int numScores = 5;
            for (int i = 0; i < numScores; i++)
            {
                yield return new WaitForSeconds(_yieldWaitTime);

                int p1 = UnityEngine.Random.Range(0, 10);
                int p2 = UnityEngine.Random.Range(0, 10);
                controller.UploadParam(p1, p2);

                AddText(string.Format("Score({0}/{1}) uploaded! p1's score: <color=green>{2}</color> p2's score: <color=green>{3}</color>", i + 1, numScores, p1, p2))
                    .AddLeftPadding(20);
            }

            yield return new WaitForSeconds(_yieldWaitTime);
            controller.Finish();

            AddText("Sent upload finished message.")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
        }

        private IEnumerator ScoreTrackerWithParameterChannels()
        {
            AddText("<color=green>ScoreTracker using upload channels</color>:");

            using (var controller = hub.GetUpStreamController<string, int, int>("ScoreTracker"))
            {
                controller.OnSuccess(result =>
                {
                    AddText(string.Format("ScoreTracker completed, result: '<color=yellow>{0}</color>'", result))
                        .AddLeftPadding(20);
                    AddText("");

                    StartCoroutine(StreamEcho());
                });

                const int numScores = 5;

                // While the server's ScoreTracker has two parameters, we can upload those parameters separately
                // So here we 

                using (var player1param = controller.GetUploadChannel<int>(0))
                {
                    for (int i = 0; i < numScores; i++)
                    {
                        yield return new WaitForSeconds(_yieldWaitTime);

                        int score = UnityEngine.Random.Range(0, 10);
                        player1param.Upload(score);

                        AddText(string.Format("Player 1's score({0}/{1}) uploaded! Score: <color=green>{2}</color>", i + 1, numScores, score))
                            .AddLeftPadding(20);
                    }
                }

                AddText("");

                using (var player2param = controller.GetUploadChannel<int>(1))
                {
                    for (int i = 0; i < numScores; i++)
                    {
                        yield return new WaitForSeconds(_yieldWaitTime);

                        int score = UnityEngine.Random.Range(0, 10);
                        player2param.Upload(score);

                        AddText(string.Format("Player 2's score({0}/{1}) uploaded! Score: <color=green>{2}</color>", i + 1, numScores, score))
                            .AddLeftPadding(20);
                    }
                }

                AddText("All scores uploaded!")
                    .AddLeftPadding(20);
            }
            yield return new WaitForSeconds(_yieldWaitTime);
        }

        private IEnumerator StreamEcho()
        {
            AddText("<color=green>StreamEcho</color>:");
            using (var controller = hub.GetUpAndDownStreamController<string, string>("StreamEcho"))
            {
                controller.OnSuccess(result =>
                {
                    AddText("StreamEcho completed!")
                        .AddLeftPadding(20);
                    AddText("");

                    StartCoroutine(PersonEcho());
                });

                controller.OnItem(item =>
                {
                    AddText(string.Format("Received from server: '<color=yellow>{0}</color>'", item))
                        .AddLeftPadding(20);
                });

                const int numMessages = 5;
                for (int i = 0; i < numMessages; i++)
                {
                    yield return new WaitForSeconds(_yieldWaitTime);

                    string message = string.Format("Message from client {0}/{1}", i + 1, numMessages);
                    controller.UploadParam(message);

                    AddText(string.Format("Sent message to the server: <color=green>{0}</color>", message))
                        .AddLeftPadding(20);
                }

                yield return new WaitForSeconds(_yieldWaitTime);
            }

            AddText("Upload finished!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
        }

        /// <summary>
        /// This is basically the same as the previous StreamEcho, but it's streaming a complex object (Person
        /// </summary>
        private IEnumerator PersonEcho()
        {
            AddText("<color=green>PersonEcho</color>:");

            using (var controller = hub.GetUpAndDownStreamController<Person, Person>("PersonEcho"))
            {
                controller.OnSuccess(result =>
                {
                    AddText("PersonEcho completed!")
                        .AddLeftPadding(20);
                    AddText("");
                    AddText("All Done!");
                });

                controller.OnItem(item =>
                {
                    AddText(string.Format("Received from server: '<color=yellow>{0}</color>'", item))
                        .AddLeftPadding(20);
                });

                const int numMessages = 5;
                for (int i = 0; i < numMessages; i++)
                {
                    yield return new WaitForSeconds(_yieldWaitTime);

                    Person person = new Person()
                    {
                        Name = "Mr. Smith",
                        Age = 20 + i * 2
                    };

                    controller.UploadParam(person);

                    AddText(string.Format("Sent person to the server: <color=green>{0}</color>", person))
                        .AddLeftPadding(20);
                }

                yield return new WaitForSeconds(_yieldWaitTime);
            }
            AddText("Upload finished!")
                .AddLeftPadding(20);

            yield return new WaitForSeconds(_yieldWaitTime);
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
