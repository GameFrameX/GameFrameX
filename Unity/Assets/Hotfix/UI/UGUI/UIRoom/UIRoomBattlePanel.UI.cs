/** Hand-written for Room UI refactor. Do NOT regenerate. **/

#if ENABLE_UI_UGUI
using GameFrameX.UI.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.UGUI.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// UGUI 字段绑定：UIRoomBattlePanel / UGUI field binding for UIRoomBattlePanel.
    /// </summary>
    [DisallowMultipleComponent]
    [OptionUIConfig(null, "Assets/Bundles/UI/UIRoom")]
    public sealed partial class UIRoomBattlePanel : UGUI
    {
        public GameObject self { get; private set; }

        #region Properties

        [SerializeField] [UGUIElementProperty("mask")]
        private UnityEngine.UI.Image mask;
        public UnityEngine.UI.Image m_mask { get { return mask; } }

        [SerializeField] [UGUIElementProperty("window_bg")]
        private UnityEngine.UI.Image window_bg;
        public UnityEngine.UI.Image m_window_bg { get { return window_bg; } }

        [SerializeField] [UGUIElementProperty("top_bg")]
        private UnityEngine.UI.Image top_bg;
        public UnityEngine.UI.Image m_top_bg { get { return top_bg; } }

        [SerializeField] [UGUIElementProperty("room_title_text")]
        private UnityEngine.UI.Text room_title_text;
        public UnityEngine.UI.Text m_room_title_text { get { return room_title_text; } }

        [SerializeField] [UGUIElementProperty("room_state_text")]
        private UnityEngine.UI.Text room_state_text;
        public UnityEngine.UI.Text m_room_state_text { get { return room_state_text; } }

        [SerializeField] [UGUIElementProperty("invite_text")]
        private UnityEngine.UI.Text invite_text;
        public UnityEngine.UI.Text m_invite_text { get { return invite_text; } }

        [SerializeField] [UGUIElementProperty("invite_button")]
        private UnityEngine.UI.Button invite_button;
        public UnityEngine.UI.Button m_invite_button { get { return invite_button; } }

        [SerializeField] [UGUIElementProperty("leave_button")]
        private UnityEngine.UI.Button leave_button;
        public UnityEngine.UI.Button m_leave_button { get { return leave_button; } }

        [SerializeField] [UGUIElementProperty("close_button")]
        private UnityEngine.UI.Button close_button;
        public UnityEngine.UI.Button m_close_button { get { return close_button; } }

        [SerializeField] [UGUIElementProperty("left_player_bg")]
        private UnityEngine.UI.Image left_player_bg;
        public UnityEngine.UI.Image m_left_player_bg { get { return left_player_bg; } }

        [SerializeField] [UGUIElementProperty("right_player_bg")]
        private UnityEngine.UI.Image right_player_bg;
        public UnityEngine.UI.Image m_right_player_bg { get { return right_player_bg; } }

        [SerializeField] [UGUIElementProperty("self_avatar")]
        private GameFrameX.UI.UGUI.Runtime.UIImage self_avatar;
        public GameFrameX.UI.UGUI.Runtime.UIImage m_self_avatar { get { return self_avatar; } }

        [SerializeField] [UGUIElementProperty("opponent_avatar")]
        private GameFrameX.UI.UGUI.Runtime.UIImage opponent_avatar;
        public GameFrameX.UI.UGUI.Runtime.UIImage m_opponent_avatar { get { return opponent_avatar; } }

        [SerializeField] [UGUIElementProperty("self_name_text")]
        private UnityEngine.UI.Text self_name_text;
        public UnityEngine.UI.Text m_self_name_text { get { return self_name_text; } }

        [SerializeField] [UGUIElementProperty("opponent_name_text")]
        private UnityEngine.UI.Text opponent_name_text;
        public UnityEngine.UI.Text m_opponent_name_text { get { return opponent_name_text; } }

        [SerializeField] [UGUIElementProperty("self_state_text")]
        private UnityEngine.UI.Text self_state_text;
        public UnityEngine.UI.Text m_self_state_text { get { return self_state_text; } }

        [SerializeField] [UGUIElementProperty("opponent_state_text")]
        private UnityEngine.UI.Text opponent_state_text;
        public UnityEngine.UI.Text m_opponent_state_text { get { return opponent_state_text; } }

        [SerializeField] [UGUIElementProperty("self_gesture_text")]
        private UnityEngine.UI.Text self_gesture_text;
        public UnityEngine.UI.Text m_self_gesture_text { get { return self_gesture_text; } }

        [SerializeField] [UGUIElementProperty("opponent_gesture_text")]
        private UnityEngine.UI.Text opponent_gesture_text;
        public UnityEngine.UI.Text m_opponent_gesture_text { get { return opponent_gesture_text; } }

        [SerializeField] [UGUIElementProperty("arena_bg")]
        private UnityEngine.UI.Image arena_bg;
        public UnityEngine.UI.Image m_arena_bg { get { return arena_bg; } }

        [SerializeField] [UGUIElementProperty("result_text")]
        private UnityEngine.UI.Text result_text;
        public UnityEngine.UI.Text m_result_text { get { return result_text; } }

        [SerializeField] [UGUIElementProperty("arena_tips_text")]
        private UnityEngine.UI.Text arena_tips_text;
        public UnityEngine.UI.Text m_arena_tips_text { get { return arena_tips_text; } }

        [SerializeField] [UGUIElementProperty("rock_button")]
        private UnityEngine.UI.Button rock_button;
        public UnityEngine.UI.Button m_rock_button { get { return rock_button; } }

        [SerializeField] [UGUIElementProperty("scissors_button")]
        private UnityEngine.UI.Button scissors_button;
        public UnityEngine.UI.Button m_scissors_button { get { return scissors_button; } }

        [SerializeField] [UGUIElementProperty("paper_button")]
        private UnityEngine.UI.Button paper_button;
        public UnityEngine.UI.Button m_paper_button { get { return paper_button; } }

        [SerializeField] [UGUIElementProperty("start_button")]
        private UnityEngine.UI.Button start_button;
        public UnityEngine.UI.Button m_start_button { get { return start_button; } }

        [SerializeField] [UGUIElementProperty("sync_button")]
        private UnityEngine.UI.Button sync_button;
        public UnityEngine.UI.Button m_sync_button { get { return sync_button; } }

        [SerializeField] [UGUIElementProperty("tips_text")]
        private UnityEngine.UI.Text tips_text;
        public UnityEngine.UI.Text m_tips_text { get { return tips_text; } }

        #endregion

        private bool _isInitView = false;

        protected override void InitView()
        {
            if (_isInitView)
            {
                return;
            }

            _isInitView = true;
            this.self = this.gameObject;
            mask = gameObject.transform.FindChildName("mask").GetComponent<UnityEngine.UI.Image>();
            window_bg = gameObject.transform.FindChildName("window_bg").GetComponent<UnityEngine.UI.Image>();
            top_bg = gameObject.transform.FindChildName("top_bg").GetComponent<UnityEngine.UI.Image>();
            room_title_text = gameObject.transform.FindChildName("room_title_text").GetComponent<UnityEngine.UI.Text>();
            room_state_text = gameObject.transform.FindChildName("room_state_text").GetComponent<UnityEngine.UI.Text>();
            invite_text = gameObject.transform.FindChildName("invite_text").GetComponent<UnityEngine.UI.Text>();
            invite_button = gameObject.transform.FindChildName("invite_button").GetComponent<UnityEngine.UI.Button>();
            leave_button = gameObject.transform.FindChildName("leave_button").GetComponent<UnityEngine.UI.Button>();
            close_button = gameObject.transform.FindChildName("close_button").GetComponent<UnityEngine.UI.Button>();
            left_player_bg = gameObject.transform.FindChildName("left_player_bg").GetComponent<UnityEngine.UI.Image>();
            right_player_bg = gameObject.transform.FindChildName("right_player_bg").GetComponent<UnityEngine.UI.Image>();
            self_avatar = gameObject.transform.FindChildName("self_avatar").GetComponent<GameFrameX.UI.UGUI.Runtime.UIImage>();
            opponent_avatar = gameObject.transform.FindChildName("opponent_avatar").GetComponent<GameFrameX.UI.UGUI.Runtime.UIImage>();
            self_name_text = gameObject.transform.FindChildName("self_name_text").GetComponent<UnityEngine.UI.Text>();
            opponent_name_text = gameObject.transform.FindChildName("opponent_name_text").GetComponent<UnityEngine.UI.Text>();
            self_state_text = gameObject.transform.FindChildName("self_state_text").GetComponent<UnityEngine.UI.Text>();
            opponent_state_text = gameObject.transform.FindChildName("opponent_state_text").GetComponent<UnityEngine.UI.Text>();
            self_gesture_text = gameObject.transform.FindChildName("self_gesture_text").GetComponent<UnityEngine.UI.Text>();
            opponent_gesture_text = gameObject.transform.FindChildName("opponent_gesture_text").GetComponent<UnityEngine.UI.Text>();
            arena_bg = gameObject.transform.FindChildName("arena_bg").GetComponent<UnityEngine.UI.Image>();
            result_text = gameObject.transform.FindChildName("result_text").GetComponent<UnityEngine.UI.Text>();
            arena_tips_text = gameObject.transform.FindChildName("arena_tips_text").GetComponent<UnityEngine.UI.Text>();
            rock_button = gameObject.transform.FindChildName("rock_button").GetComponent<UnityEngine.UI.Button>();
            scissors_button = gameObject.transform.FindChildName("scissors_button").GetComponent<UnityEngine.UI.Button>();
            paper_button = gameObject.transform.FindChildName("paper_button").GetComponent<UnityEngine.UI.Button>();
            start_button = gameObject.transform.FindChildName("start_button").GetComponent<UnityEngine.UI.Button>();
            sync_button = gameObject.transform.FindChildName("sync_button").GetComponent<UnityEngine.UI.Button>();
            tips_text = gameObject.transform.FindChildName("tips_text").GetComponent<UnityEngine.UI.Text>();
        }
    }
}
#endif
