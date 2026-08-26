/** Hand-written for Room UI refactor. Do NOT regenerate. **/

#if ENABLE_UI_UGUI
using GameFrameX.UI.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.UGUI.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// UGUI 字段绑定：UIRoomResultPopup / UGUI field binding for UIRoomResultPopup.
    /// </summary>
    [DisallowMultipleComponent]
    [OptionUIConfig(null, "Assets/Bundles/UI/UIRoom")]
    public sealed partial class UIRoomResultPopup : UGUI
    {
        public GameObject self { get; private set; }

        #region Properties

        [SerializeField] [UGUIElementProperty("mask")]
        private UnityEngine.UI.Image mask;
        public UnityEngine.UI.Image m_mask { get { return mask; } }

        [SerializeField] [UGUIElementProperty("window_bg")]
        private UnityEngine.UI.Image window_bg;
        public UnityEngine.UI.Image m_window_bg { get { return window_bg; } }

        [SerializeField] [UGUIElementProperty("title_text")]
        private UnityEngine.UI.Text title_text;
        public UnityEngine.UI.Text m_title_text { get { return title_text; } }

        [SerializeField] [UGUIElementProperty("result_text")]
        private UnityEngine.UI.Text result_text;
        public UnityEngine.UI.Text m_result_text { get { return result_text; } }

        [SerializeField] [UGUIElementProperty("round_text")]
        private UnityEngine.UI.Text round_text;
        public UnityEngine.UI.Text m_round_text { get { return round_text; } }

        [SerializeField] [UGUIElementProperty("self_bg")]
        private UnityEngine.UI.Image self_bg;
        public UnityEngine.UI.Image m_self_bg { get { return self_bg; } }

        [SerializeField] [UGUIElementProperty("opponent_bg")]
        private UnityEngine.UI.Image opponent_bg;
        public UnityEngine.UI.Image m_opponent_bg { get { return opponent_bg; } }

        [SerializeField] [UGUIElementProperty("self_name_text")]
        private UnityEngine.UI.Text self_name_text;
        public UnityEngine.UI.Text m_self_name_text { get { return self_name_text; } }

        [SerializeField] [UGUIElementProperty("opponent_name_text")]
        private UnityEngine.UI.Text opponent_name_text;
        public UnityEngine.UI.Text m_opponent_name_text { get { return opponent_name_text; } }

        [SerializeField] [UGUIElementProperty("self_gesture_text")]
        private UnityEngine.UI.Text self_gesture_text;
        public UnityEngine.UI.Text m_self_gesture_text { get { return self_gesture_text; } }

        [SerializeField] [UGUIElementProperty("opponent_gesture_text")]
        private UnityEngine.UI.Text opponent_gesture_text;
        public UnityEngine.UI.Text m_opponent_gesture_text { get { return opponent_gesture_text; } }

        [SerializeField] [UGUIElementProperty("restart_button")]
        private UnityEngine.UI.Button restart_button;
        public UnityEngine.UI.Button m_restart_button { get { return restart_button; } }

        [SerializeField] [UGUIElementProperty("close_button")]
        private UnityEngine.UI.Button close_button;
        public UnityEngine.UI.Button m_close_button { get { return close_button; } }

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
            title_text = gameObject.transform.FindChildName("title_text").GetComponent<UnityEngine.UI.Text>();
            result_text = gameObject.transform.FindChildName("result_text").GetComponent<UnityEngine.UI.Text>();
            round_text = gameObject.transform.FindChildName("round_text").GetComponent<UnityEngine.UI.Text>();
            self_bg = gameObject.transform.FindChildName("self_bg").GetComponent<UnityEngine.UI.Image>();
            opponent_bg = gameObject.transform.FindChildName("opponent_bg").GetComponent<UnityEngine.UI.Image>();
            self_name_text = gameObject.transform.FindChildName("self_name_text").GetComponent<UnityEngine.UI.Text>();
            opponent_name_text = gameObject.transform.FindChildName("opponent_name_text").GetComponent<UnityEngine.UI.Text>();
            self_gesture_text = gameObject.transform.FindChildName("self_gesture_text").GetComponent<UnityEngine.UI.Text>();
            opponent_gesture_text = gameObject.transform.FindChildName("opponent_gesture_text").GetComponent<UnityEngine.UI.Text>();
            restart_button = gameObject.transform.FindChildName("restart_button").GetComponent<UnityEngine.UI.Button>();
            close_button = gameObject.transform.FindChildName("close_button").GetComponent<UnityEngine.UI.Button>();
        }
    }
}
#endif
