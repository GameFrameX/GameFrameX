/** Hand-written for Room UI refactor. Do NOT regenerate. **/

#if ENABLE_UI_UGUI
using GameFrameX.UI.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.UGUI.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// UGUI 字段绑定：UIRoomListItem / UGUI field binding for UIRoomListItem.
    /// </summary>
    [DisallowMultipleComponent]
    [OptionUIConfig(null, "Assets/Bundles/UI/UIRoom")]
    public sealed partial class UIRoomListItem : UGUI
    {
        public GameObject self { get; private set; }

        #region Properties

        [SerializeField] [UGUIElementProperty("bg")]
        private UnityEngine.UI.Image bg;
        public UnityEngine.UI.Image m_bg { get { return bg; } }

        [SerializeField] [UGUIElementProperty("room_name_text")]
        private UnityEngine.UI.Text room_name_text;
        public UnityEngine.UI.Text m_room_name_text { get { return room_name_text; } }

        [SerializeField] [UGUIElementProperty("room_status_text")]
        private UnityEngine.UI.Text room_status_text;
        public UnityEngine.UI.Text m_room_status_text { get { return room_status_text; } }

        [SerializeField] [UGUIElementProperty("join_button")]
        private UnityEngine.UI.Button join_button;
        public UnityEngine.UI.Button m_join_button { get { return join_button; } }

        #endregion

        public static UIRoomListItem Create(GameObject go)
        {
            var ui = go.GetOrAddComponent<UIRoomListItem>();
            ui?.InitView();
            return ui;
        }

        private bool _isInitView = false;

        protected override void InitView()
        {
            if (_isInitView)
            {
                return;
            }

            _isInitView = true;
            this.self = this.gameObject;
            bg = gameObject.transform.FindChildName("bg").GetComponent<UnityEngine.UI.Image>();
            room_name_text = gameObject.transform.FindChildName("room_name_text").GetComponent<UnityEngine.UI.Text>();
            room_status_text = gameObject.transform.FindChildName("room_status_text").GetComponent<UnityEngine.UI.Text>();
            join_button = gameObject.transform.FindChildName("join_button").GetComponent<UnityEngine.UI.Button>();
        }
    }
}
#endif
