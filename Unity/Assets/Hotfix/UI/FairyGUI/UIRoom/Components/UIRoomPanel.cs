/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using Cysharp.Threading.Tasks;
using FairyGUI.Utils;
using GameFrameX.Entity.Runtime;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    public sealed partial class UIRoomPanel : FUI
    {
        public const string UIPackageName = "UIRoom";
        public const string UIResName = "UIRoomPanel";
        public const string URL = "ui://rpsroom1panel1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_mask { get; private set; }
		public GGraph m_window_bg { get; private set; }
		public GTextField m_title_text { get; private set; }
		public GTextField m_summary_text { get; private set; }
		public UIRoomButton m_refresh_button { get; private set; }
		public UIRoomButton m_create_button { get; private set; }
		public UIRoomButton m_close_button { get; private set; }
		public GGraph m_room_list_bg { get; private set; }
		public GTextField m_room_list_title { get; private set; }
		public GList m_room_list { get; private set; }
		public GGraph m_room_detail_bg { get; private set; }
		public GTextField m_room_detail_title { get; private set; }
		public GTextField m_room_detail_text { get; private set; }
		public UIRoomButton m_start_button { get; private set; }
		public UIRoomButton m_leave_button { get; private set; }
		public UIRoomButton m_invite_button { get; private set; }
		public UIRoomButton m_sync_button { get; private set; }
		public UIRoomButton m_restart_button { get; private set; }
		public GTextField m_invite_text { get; private set; }
		public GGraph m_players_bg { get; private set; }
		public GTextField m_players_title { get; private set; }
		public GList m_player_list { get; private set; }
		public GGraph m_game_bg { get; private set; }
		public GTextField m_game_title { get; private set; }
		public GTextField m_result_text { get; private set; }
		public GTextField m_self_gesture_text { get; private set; }
		public GTextField m_opponent_gesture_text { get; private set; }
		public UIGestureButton m_rock_button { get; private set; }
		public UIGestureButton m_scissors_button { get; private set; }
		public UIGestureButton m_paper_button { get; private set; }
		public GTextField m_tips_text { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIRoomPanel CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIRoomPanel> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIRoomPanel> tcs = new UniTaskCompletionSource<UIRoomPanel>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIRoomPanel Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIRoomPanel>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIRoomPanel GetFormPool(GObject go)
        {
            var fui = go.Get<UIRoomPanel>();
            if (fui == null)
            {
                fui = Create(go);
            }

            fui.IsFromPool = true;
            return fui;
        }

        protected override void InitView()
        {
            if(GObject == null)
            {
                return;
            }

            self = (GComponent)GObject;
            self.Add(this);
            
            var com = GObject.asCom;
            if (com != null)
            {
				m_mask = (GGraph)com.GetChild("mask");
				m_window_bg = (GGraph)com.GetChild("window_bg");
				m_title_text = (GTextField)com.GetChild("title_text");
				m_summary_text = (GTextField)com.GetChild("summary_text");
				m_refresh_button = UIRoomButton.Create(com.GetChild("refresh_button"));
				m_create_button = UIRoomButton.Create(com.GetChild("create_button"));
				m_close_button = UIRoomButton.Create(com.GetChild("close_button"));
				m_room_list_bg = (GGraph)com.GetChild("room_list_bg");
				m_room_list_title = (GTextField)com.GetChild("room_list_title");
				m_room_list = (GList)com.GetChild("room_list");
				m_room_detail_bg = (GGraph)com.GetChild("room_detail_bg");
				m_room_detail_title = (GTextField)com.GetChild("room_detail_title");
				m_room_detail_text = (GTextField)com.GetChild("room_detail_text");
				m_start_button = UIRoomButton.Create(com.GetChild("start_button"));
				m_leave_button = UIRoomButton.Create(com.GetChild("leave_button"));
				m_invite_button = UIRoomButton.Create(com.GetChild("invite_button"));
				m_sync_button = UIRoomButton.Create(com.GetChild("sync_button"));
				m_restart_button = UIRoomButton.Create(com.GetChild("restart_button"));
				m_invite_text = (GTextField)com.GetChild("invite_text");
				m_players_bg = (GGraph)com.GetChild("players_bg");
				m_players_title = (GTextField)com.GetChild("players_title");
				m_player_list = (GList)com.GetChild("player_list");
				m_game_bg = (GGraph)com.GetChild("game_bg");
				m_game_title = (GTextField)com.GetChild("game_title");
				m_result_text = (GTextField)com.GetChild("result_text");
				m_self_gesture_text = (GTextField)com.GetChild("self_gesture_text");
				m_opponent_gesture_text = (GTextField)com.GetChild("opponent_gesture_text");
				m_rock_button = UIGestureButton.Create(com.GetChild("rock_button"));
				m_scissors_button = UIGestureButton.Create(com.GetChild("scissors_button"));
				m_paper_button = UIGestureButton.Create(com.GetChild("paper_button"));
				m_tips_text = (GTextField)com.GetChild("tips_text");
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            base.Dispose();
            self.Remove();
			m_mask = null;
			m_window_bg = null;
			m_title_text = null;
			m_summary_text = null;
			m_refresh_button = null;
			m_create_button = null;
			m_close_button = null;
			m_room_list_bg = null;
			m_room_list_title = null;
			m_room_list = null;
			m_room_detail_bg = null;
			m_room_detail_title = null;
			m_room_detail_text = null;
			m_start_button = null;
			m_leave_button = null;
			m_invite_button = null;
			m_sync_button = null;
			m_restart_button = null;
			m_invite_text = null;
			m_players_bg = null;
			m_players_title = null;
			m_player_list = null;
			m_game_bg = null;
			m_game_title = null;
			m_result_text = null;
			m_self_gesture_text = null;
			m_opponent_gesture_text = null;
			m_rock_button = null;
			m_scissors_button = null;
			m_paper_button = null;
			m_tips_text = null;
            self = null;            
        }

        private UIRoomPanel(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif