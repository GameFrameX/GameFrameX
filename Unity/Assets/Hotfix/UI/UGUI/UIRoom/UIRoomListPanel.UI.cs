/** Hand-written for Room UI refactor. Do NOT regenerate. **/

#if ENABLE_UI_UGUI
using GameFrameX.UI.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.UGUI.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// UGUI 字段绑定：UIRoomListPanel / UGUI field binding for UIRoomListPanel.
    /// </summary>
    [DisallowMultipleComponent]
    [OptionUIConfig(null, "Assets/Bundles/UI/UIRoom")]
    public sealed partial class UIRoomListPanel : UGUI
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

        [SerializeField] [UGUIElementProperty("summary_text")]
        private UnityEngine.UI.Text summary_text;
        public UnityEngine.UI.Text m_summary_text { get { return summary_text; } }

        [SerializeField] [UGUIElementProperty("refresh_button")]
        private UnityEngine.UI.Button refresh_button;
        public UnityEngine.UI.Button m_refresh_button { get { return refresh_button; } }

        [SerializeField] [UGUIElementProperty("create_button")]
        private UnityEngine.UI.Button create_button;
        public UnityEngine.UI.Button m_create_button { get { return create_button; } }

        [SerializeField] [UGUIElementProperty("close_button")]
        private UnityEngine.UI.Button close_button;
        public UnityEngine.UI.Button m_close_button { get { return close_button; } }

        [SerializeField] [UGUIElementProperty("list_bg")]
        private UnityEngine.UI.Image list_bg;
        public UnityEngine.UI.Image m_list_bg { get { return list_bg; } }

        [SerializeField] [UGUIElementProperty("list_title")]
        private UnityEngine.UI.Text list_title;
        public UnityEngine.UI.Text m_list_title { get { return list_title; } }

        [SerializeField] [UGUIElementProperty("room_list")]
        private UnityEngine.UI.ScrollRect room_list;
        public UnityEngine.UI.ScrollRect m_room_list { get { return room_list; } }

        [SerializeField] [UGUIElementProperty("room_list/Viewport/Content")]
        private UnityEngine.Transform room_list_content;
        public UnityEngine.Transform m_room_list_content { get { return room_list_content; } }

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
            title_text = gameObject.transform.FindChildName("title_text").GetComponent<UnityEngine.UI.Text>();
            summary_text = gameObject.transform.FindChildName("summary_text").GetComponent<UnityEngine.UI.Text>();
            refresh_button = gameObject.transform.FindChildName("refresh_button").GetComponent<UnityEngine.UI.Button>();
            create_button = gameObject.transform.FindChildName("create_button").GetComponent<UnityEngine.UI.Button>();
            close_button = gameObject.transform.FindChildName("close_button").GetComponent<UnityEngine.UI.Button>();
            list_bg = gameObject.transform.FindChildName("list_bg").GetComponent<UnityEngine.UI.Image>();
            list_title = gameObject.transform.FindChildName("list_title").GetComponent<UnityEngine.UI.Text>();
            room_list = gameObject.transform.FindChildName("room_list").GetComponent<UnityEngine.UI.ScrollRect>();
            room_list_content = gameObject.transform.FindChildName("room_list/Viewport/Content").GetComponent<UnityEngine.Transform>();
            tips_text = gameObject.transform.FindChildName("tips_text").GetComponent<UnityEngine.UI.Text>();
        }
    }
}
#endif
