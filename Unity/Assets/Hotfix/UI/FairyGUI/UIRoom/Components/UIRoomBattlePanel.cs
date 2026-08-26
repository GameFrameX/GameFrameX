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
    public sealed partial class UIRoomBattlePanel : FUI
    {
        public const string UIPackageName = "UIRoom";
        public const string UIResName = "UIRoomBattlePanel";
        public const string URL = "ui://rpsroom1battl1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_mask { get; private set; }
		public GGraph m_window_bg { get; private set; }
		public GGraph m_top_bg { get; private set; }
		public GTextField m_room_title_text { get; private set; }
		public GTextField m_room_state_text { get; private set; }
		public GTextField m_invite_text { get; private set; }
		public UIRoomButton m_invite_button { get; private set; }
		public UIRoomButton m_leave_button { get; private set; }
		public UIRoomButton m_close_button { get; private set; }
		public GGraph m_left_player_bg { get; private set; }
		public GGraph m_right_player_bg { get; private set; }
		public GLoader m_self_avatar { get; private set; }
		public GLoader m_opponent_avatar { get; private set; }
		public GTextField m_self_name_text { get; private set; }
		public GTextField m_opponent_name_text { get; private set; }
		public GTextField m_self_state_text { get; private set; }
		public GTextField m_opponent_state_text { get; private set; }
		public GTextField m_self_gesture_text { get; private set; }
		public GTextField m_opponent_gesture_text { get; private set; }
		public GGraph m_arena_bg { get; private set; }
		public GTextField m_result_text { get; private set; }
		public GTextField m_arena_tips_text { get; private set; }
		public UIGestureButton m_rock_button { get; private set; }
		public UIGestureButton m_scissors_button { get; private set; }
		public UIGestureButton m_paper_button { get; private set; }
		public UIRoomButton m_start_button { get; private set; }
		public UIRoomButton m_sync_button { get; private set; }
		public GTextField m_tips_text { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        private static void CreateGObjectAsync(UIPackage.CreateObjectCallback result)
        {
            UIPackage.CreateObjectAsync(UIPackageName, UIResName, result);
        }

        public static UIRoomBattlePanel CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UniTask<UIRoomBattlePanel> CreateInstanceAsync(Entity domain)
        {
            UniTaskCompletionSource<UIRoomBattlePanel> tcs = new UniTaskCompletionSource<UIRoomBattlePanel>();
            CreateGObjectAsync((go) =>
            {
                tcs.TrySetResult(Create(go));
            });
            return tcs.Task;
        }

        public static UIRoomBattlePanel Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIRoomBattlePanel>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIRoomBattlePanel GetFormPool(GObject go)
        {
            var fui = go.Get<UIRoomBattlePanel>();
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
				m_top_bg = (GGraph)com.GetChild("top_bg");
				m_room_title_text = (GTextField)com.GetChild("room_title_text");
				m_room_state_text = (GTextField)com.GetChild("room_state_text");
				m_invite_text = (GTextField)com.GetChild("invite_text");
				m_invite_button = UIRoomButton.Create(com.GetChild("invite_button"));
				m_leave_button = UIRoomButton.Create(com.GetChild("leave_button"));
				m_close_button = UIRoomButton.Create(com.GetChild("close_button"));
				m_left_player_bg = (GGraph)com.GetChild("left_player_bg");
				m_right_player_bg = (GGraph)com.GetChild("right_player_bg");
				m_self_avatar = (GLoader)com.GetChild("self_avatar");
				m_opponent_avatar = (GLoader)com.GetChild("opponent_avatar");
				m_self_name_text = (GTextField)com.GetChild("self_name_text");
				m_opponent_name_text = (GTextField)com.GetChild("opponent_name_text");
				m_self_state_text = (GTextField)com.GetChild("self_state_text");
				m_opponent_state_text = (GTextField)com.GetChild("opponent_state_text");
				m_self_gesture_text = (GTextField)com.GetChild("self_gesture_text");
				m_opponent_gesture_text = (GTextField)com.GetChild("opponent_gesture_text");
				m_arena_bg = (GGraph)com.GetChild("arena_bg");
				m_result_text = (GTextField)com.GetChild("result_text");
				m_arena_tips_text = (GTextField)com.GetChild("arena_tips_text");
				m_rock_button = UIGestureButton.Create(com.GetChild("rock_button"));
				m_scissors_button = UIGestureButton.Create(com.GetChild("scissors_button"));
				m_paper_button = UIGestureButton.Create(com.GetChild("paper_button"));
				m_start_button = UIRoomButton.Create(com.GetChild("start_button"));
				m_sync_button = UIRoomButton.Create(com.GetChild("sync_button"));
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
			m_top_bg = null;
			m_room_title_text = null;
			m_room_state_text = null;
			m_invite_text = null;
			m_invite_button = null;
			m_leave_button = null;
			m_close_button = null;
			m_left_player_bg = null;
			m_right_player_bg = null;
			m_self_avatar = null;
			m_opponent_avatar = null;
			m_self_name_text = null;
			m_opponent_name_text = null;
			m_self_state_text = null;
			m_opponent_state_text = null;
			m_self_gesture_text = null;
			m_opponent_gesture_text = null;
			m_arena_bg = null;
			m_result_text = null;
			m_arena_tips_text = null;
			m_rock_button = null;
			m_scissors_button = null;
			m_paper_button = null;
			m_start_button = null;
			m_sync_button = null;
			m_tips_text = null;
            self = null;            
        }

        private UIRoomBattlePanel(GObject gObject) : base(gObject)
        {
            // Awake(gObject);
        }
    }
}
#endif