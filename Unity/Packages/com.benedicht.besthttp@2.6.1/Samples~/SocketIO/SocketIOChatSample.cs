#if !BESTHTTP_DISABLE_SOCKETIO

using System;
using System.Collections.Generic;

using UnityEngine;
using BestHTTP.SocketIO;
using UnityEngine.UI;
using BestHTTP.Examples.Helpers;

namespace BestHTTP.Examples
{
    public sealed class SocketIOChatSample : BestHTTP.Examples.Helpers.SampleBase
    {
        private readonly TimeSpan TYPING_TIMER_LENGTH = TimeSpan.FromMilliseconds(700);

#pragma warning disable 0649, 0414

        [SerializeField]
        [Tooltip("The WebSocket address to connect")]
        private string address = "https://socketio-chat-h9jt.herokuapp.com/socket.io/";

        [Header("Login Details")]
        [SerializeField]
        private RectTransform _loginRoot;

        [SerializeField]
        private InputField _userNameInput;

        [Header("Chat Setup")]

        [SerializeField]
        private RectTransform _chatRoot;

        [SerializeField]
        private Text _participantsText;

        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private RectTransform _contentRoot;

        [SerializeField]
        private TextListItem _listItemPrefab;

        [SerializeField]
        private int _maxListItemEntries = 100;

        [SerializeField]
        private Text _typingUsersText;

        [SerializeField]
        private InputField _input;

        [Header("Buttons")]

        [SerializeField]
        private Button _connectButton;

        [SerializeField]
        private Button _closeButton;

#pragma warning restore
        
        /// <summary>
        /// The Socket.IO manager instance.
        /// </summary>
        private SocketManager Manager;

        /// <summary>
        /// True if the user is currently typing
        /// </summary>
        private bool typing;

        /// <summary>
        /// When the message changed.
        /// </summary>
        private DateTime lastTypingTime = DateTime.MinValue;

        /// <summary>
        /// Users that typing.
        /// </summary>
        private List<string> typingUsers = new List<string>();

        #region Unity Events

        protected override void Start()
        {
            base.Start();

            this._userNameInput.text = PlayerPrefs.GetString("SocketIOChatSample_UserName");
            SetButtons(!string.IsNullOrEmpty(this._userNameInput.text), false);
            SetPanels(true);
        }

        void OnDestroy()
        {
            if (this.Manager != null)
            {
                // Leaving this sample, close the socket
                this.Manager.Close();
                this.Manager = null;
            }
        }

        public void OnUserNameInputChanged(string userName)
        {
            SetButtons(!string.IsNullOrEmpty(userName), false);
        }

        public void OnUserNameInputSubmit(string userName)
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                OnConnectButton();
        }

        public void OnConnectButton()
        {
            SetPanels(false);
            
            PlayerPrefs.SetString("SocketIOChatSample_UserName", this._userNameInput.text);

            AddText("Connecting...");

            // Create the Socket.IO manager
            Manager = new SocketManager(new Uri(address));

            Manager.Socket.On(SocketIOEventTypes.Connect, (s, p, a) =>
            {
                AddText("Connected!");

                Manager.Socket.Emit("add user", this._userNameInput.text);
                this._input.interactable = true;
            });

            Manager.Socket.On(SocketIOEventTypes.Disconnect, (s, p, a) =>
            {
                AddText("Disconnected!");

                SetPanels(true);
                SetButtons(true, false);
            });

            // Set up custom chat events
            Manager.Socket.On("login", OnLogin);
            Manager.Socket.On("new message", OnNewMessage);
            Manager.Socket.On("user joined", OnUserJoined);
            Manager.Socket.On("user left", OnUserLeft);
            Manager.Socket.On("typing", OnTyping);
            Manager.Socket.On("stop typing", OnStopTyping);

            // The argument will be an Error object.
            Manager.Socket.On(SocketIOEventTypes.Error, (socket, packet, args) => {
                Debug.LogError(string.Format("Error: {0}", args[0].ToString()));
            });

            SetButtons(false, true);
        }

        public void OnCloseButton()
        {
            SetButtons(false, false);
            this.Manager.Close();
        }

        void Update()
        {
            if (typing)
            {
                var typingTimer = DateTime.UtcNow;
                var timeDiff = typingTimer - lastTypingTime;
                if (timeDiff >= TYPING_TIMER_LENGTH)
                {
                    Manager.Socket.Emit("stop typing");
                    typing = false;
                }
            }
        }
        
        #endregion

        #region Chat Logic
        
        public void OnMessageInput(string textToSend)
        {
            if ((!Input.GetKeyDown(KeyCode.KeypadEnter) && !Input.GetKeyDown(KeyCode.Return)) || string.IsNullOrEmpty(textToSend))
                return;

            Manager.Socket.Emit("new message", textToSend);

            AddText(string.Format("{0}: {1}", this._userNameInput.text, textToSend));
        }

        public void UpdateTyping(string textToSend)
        {
            if (!typing)
            {
                typing = true;
                Manager.Socket.Emit("typing");
            }

            lastTypingTime = DateTime.UtcNow;
        }

        void addParticipantsMessage(Dictionary<string, object> data)
        {
            int numUsers = Convert.ToInt32(data["numUsers"]);

            if (numUsers == 1)
                this._participantsText.text = "there's 1 participant";
            else
                this._participantsText.text = "there are " + numUsers + " participants";
        }

        void addChatMessage(Dictionary<string, object> data)
        {
            var username = data["username"] as string;
            var msg = data["message"] as string;

            AddText(string.Format("{0}: {1}", username, msg));
        }

        void AddChatTyping(Dictionary<string, object> data)
        {
            var username = data["username"] as string;

            int idx = typingUsers.FindIndex((name) => name.Equals(username));
            if (idx == -1)
                typingUsers.Add(username);

            SetTypingUsers();
        }

        void RemoveChatTyping(Dictionary<string, object> data)
        {
            var username = data["username"] as string;

            int idx = typingUsers.FindIndex((name) => name.Equals(username));
            if (idx != -1)
                typingUsers.RemoveAt(idx);

            SetTypingUsers();
        }

        #endregion

        #region Custom SocketIO Events

        void OnLogin(Socket socket, Packet packet, params object[] args)
        {
            AddText("Welcome to Socket.IO Chat");

            addParticipantsMessage(args[0] as Dictionary<string, object>);
        }

        void OnNewMessage(Socket socket, Packet packet, params object[] args)
        {
            addChatMessage(args[0] as Dictionary<string, object>);
        }

        void OnUserJoined(Socket socket, Packet packet, params object[] args)
        {
            var data = args[0] as Dictionary<string, object>;

            var username = data["username"] as string;

            AddText(string.Format("{0} joined", username));

            addParticipantsMessage(data);
        }

        void OnUserLeft(Socket socket, Packet packet, params object[] args)
        {
            var data = args[0] as Dictionary<string, object>;

            var username = data["username"] as string;

            AddText(string.Format("{0} left", username));

            addParticipantsMessage(data);
        }

        void OnTyping(Socket socket, Packet packet, params object[] args)
        {
            AddChatTyping(args[0] as Dictionary<string, object>);
        }

        void OnStopTyping(Socket socket, Packet packet, params object[] args)
        {
            RemoveChatTyping(args[0] as Dictionary<string, object>);
        }

        #endregion

        private void AddText(string text)
        {
            GUIHelper.AddText(this._listItemPrefab, this._contentRoot, text, this._maxListItemEntries, this._scrollRect);
        }

        private void SetTypingUsers()
        {
            if (this.typingUsers.Count > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder(this.typingUsers[0], this.typingUsers.Count + 1);

                for (int i = 1; i < this.typingUsers.Count; ++i)
                    sb.AppendFormat(", {0}", this.typingUsers[i]);

                if (this.typingUsers.Count == 1)
                    sb.Append(" is typing!");
                else
                    sb.Append(" are typing!");

                this._typingUsersText.text = sb.ToString();
            }
            else
                this._typingUsersText.text = string.Empty;
        }

        private void SetPanels(bool login)
        {
            if (login)
            {
                this._loginRoot.gameObject.SetActive(true);
                this._chatRoot.gameObject.SetActive(false);
                this._input.interactable = false;
            }
            else
            {
                this._loginRoot.gameObject.SetActive(false);
                this._chatRoot.gameObject.SetActive(true);
                this._input.interactable = true;
            }
        }

        private void SetButtons(bool connect, bool close)
        {
            if (this._connectButton != null)
                this._connectButton.interactable = connect;

            if (this._closeButton != null)
                this._closeButton.interactable = close;
        }
    }
}

#endif
