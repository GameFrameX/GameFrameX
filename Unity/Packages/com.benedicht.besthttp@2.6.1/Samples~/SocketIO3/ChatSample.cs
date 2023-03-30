using System;
using System.Collections.Generic;

using BestHTTP.Examples.Helpers;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Events;

using UnityEngine;
using UnityEngine.UI;

namespace BestHTTP.Examples.SocketIO3
{
#pragma warning disable 0649
    [PlatformSupport.IL2CPP.Preserve]
    class LoginData
    {
        [PlatformSupport.IL2CPP.Preserve] public int numUsers;
    }

    [PlatformSupport.IL2CPP.Preserve]
    sealed class NewMessageData
    {
        [PlatformSupport.IL2CPP.Preserve] public string username;
        [PlatformSupport.IL2CPP.Preserve] public string message;
    }

    [PlatformSupport.IL2CPP.Preserve]
    sealed class UserJoinedData : LoginData
    {
        [PlatformSupport.IL2CPP.Preserve] public string username;
    }

    [PlatformSupport.IL2CPP.Preserve]
    sealed class TypingData
    {
        [PlatformSupport.IL2CPP.Preserve] public string username;
    }

#pragma warning restore

    public sealed class ChatSample : BestHTTP.Examples.Helpers.SampleBase
    {
        private readonly TimeSpan TYPING_TIMER_LENGTH = TimeSpan.FromMilliseconds(700);

#pragma warning disable 0649, 0414

        [SerializeField]
        [Tooltip("The Socket.IO service address to connect to")]
        private string address = "https://socket-io-3-chat-5ae3v.ondigitalocean.app";

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

            this._userNameInput.text = PlayerPrefs.GetString("SocketIO3ChatSample_UserName");
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

        public void UpdateTyping()
        {
            if (!typing)
            {
                typing = true;
                Manager.Socket.Emit("typing");
            }

            lastTypingTime = DateTime.UtcNow;
        }

        public void OnMessageInput(string textToSend)
        {
            if ((!Input.GetKeyDown(KeyCode.KeypadEnter) && !Input.GetKeyDown(KeyCode.Return)) || string.IsNullOrEmpty(textToSend))
                return;

            Manager.Socket.Emit("new message", textToSend);

            AddText(string.Format("{0}: {1}", this._userNameInput.text, textToSend));
        }

        public void OnConnectButton()
        {
            SetPanels(false);

            PlayerPrefs.SetString("SocketIO3ChatSample_UserName", this._userNameInput.text);

            AddText("Connecting...");

            // Create the Socket.IO manager
            Manager = new SocketManager(new Uri(this.address));

            Manager.Socket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
            Manager.Socket.On(SocketIOEventTypes.Disconnect, OnDisconnected);

            Manager.Socket.On<LoginData>("login", OnLogin);
            Manager.Socket.On<NewMessageData>("new message", OnNewMessage);
            Manager.Socket.On<UserJoinedData>("user joined", OnUserJoined);
            Manager.Socket.On<UserJoinedData>("user left", OnUserLeft);
            Manager.Socket.On<TypingData>("typing", OnTyping);
            Manager.Socket.On<TypingData>("stop typing", OnStopTyping);

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

        #region SocketIO Events

        private void OnConnected(ConnectResponse resp)
        {
            AddText("Connected! Socket.IO SID: " + resp.sid);

            Manager.Socket.Emit("add user", this._userNameInput.text);
            this._input.interactable = true;
        }

        private void OnDisconnected()
        {
            AddText("Disconnected!");

            SetPanels(true);
            SetButtons(true, false);
        }

        private void OnLogin(LoginData data)
        {
            AddText("Welcome to Socket.IO Chat");

            if (data.numUsers == 1)
                this._participantsText.text = "there's 1 participant";
            else
                this._participantsText.text = "there are " + data.numUsers + " participants";
        }

        private void OnNewMessage(NewMessageData data)
        {
            AddText(string.Format("{0}: {1}", data.username, data.message));
        }

        private void OnUserJoined(UserJoinedData data)
        {
            AddText(string.Format("{0} joined", data.username));

            if (data.numUsers == 1)
                this._participantsText.text = "there's 1 participant";
            else
                this._participantsText.text = "there are " + data.numUsers + " participants";
        }

        private void OnUserLeft(UserJoinedData data)
        {
            AddText(string.Format("{0} left", data.username));

            if (data.numUsers == 1)
                this._participantsText.text = "there's 1 participant";
            else
                this._participantsText.text = "there are " + data.numUsers + " participants";
        }

        private void OnTyping(TypingData data)
        {
            int idx = typingUsers.FindIndex((name) => name.Equals(data.username));
            if (idx == -1)
                typingUsers.Add(data.username);

            SetTypingUsers();
        }

        private void OnStopTyping(TypingData data)
        {
            int idx = typingUsers.FindIndex((name) => name.Equals(data.username));
            if (idx != -1)
                typingUsers.RemoveAt(idx);

            SetTypingUsers();
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
