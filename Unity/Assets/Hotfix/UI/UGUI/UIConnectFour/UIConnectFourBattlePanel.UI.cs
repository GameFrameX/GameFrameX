/** Hand-written for ConnectFour UI. Do NOT regenerate. **/

#if ENABLE_UI_UGUI
using GameFrameX.UI.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.UGUI.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    /// <summary>
    /// UGUI 字段绑定：UIConnectFourBattlePanel / UGUI field binding for UIConnectFourBattlePanel.
    /// </summary>
    [DisallowMultipleComponent]
    [OptionUIConfig(null, "Assets/Bundles/UI/UIConnectFour")]
    public sealed partial class UIConnectFourBattlePanel : UGUI
    {
        public GameObject self { get; private set; }

        #region Properties

        [SerializeField] [UGUIElementProperty("mask")]
        private UnityEngine.UI.Image mask;
        public UnityEngine.UI.Image m_mask { get { return mask; } }

        [SerializeField] [UGUIElementProperty("window_bg")]
        private UnityEngine.UI.Image window_bg;
        public UnityEngine.UI.Image m_window_bg { get { return window_bg; } }

        [SerializeField] [UGUIElementProperty("title")]
        private UnityEngine.UI.Text title;
        public UnityEngine.UI.Text m_title { get { return title; } }

        [SerializeField] [UGUIElementProperty("room_state_text")]
        private UnityEngine.UI.Text room_state_text;
        public UnityEngine.UI.Text m_room_state_text { get { return room_state_text; } }

        [SerializeField] [UGUIElementProperty("turn_text")]
        private UnityEngine.UI.Text turn_text;
        public UnityEngine.UI.Text m_turn_text { get { return turn_text; } }

        [SerializeField] [UGUIElementProperty("board_grid")]
        private UnityEngine.UI.Image board_grid;
        public UnityEngine.UI.Image m_board_grid { get { return board_grid; } }

        [SerializeField] [UGUIElementProperty("cell_0")]
        private UnityEngine.UI.Image cell_0;
        public UnityEngine.UI.Image m_cell_0 { get { return cell_0; } }

        [SerializeField] [UGUIElementProperty("cell_1")]
        private UnityEngine.UI.Image cell_1;
        public UnityEngine.UI.Image m_cell_1 { get { return cell_1; } }

        [SerializeField] [UGUIElementProperty("cell_2")]
        private UnityEngine.UI.Image cell_2;
        public UnityEngine.UI.Image m_cell_2 { get { return cell_2; } }

        [SerializeField] [UGUIElementProperty("cell_3")]
        private UnityEngine.UI.Image cell_3;
        public UnityEngine.UI.Image m_cell_3 { get { return cell_3; } }

        [SerializeField] [UGUIElementProperty("cell_4")]
        private UnityEngine.UI.Image cell_4;
        public UnityEngine.UI.Image m_cell_4 { get { return cell_4; } }

        [SerializeField] [UGUIElementProperty("cell_5")]
        private UnityEngine.UI.Image cell_5;
        public UnityEngine.UI.Image m_cell_5 { get { return cell_5; } }

        [SerializeField] [UGUIElementProperty("cell_6")]
        private UnityEngine.UI.Image cell_6;
        public UnityEngine.UI.Image m_cell_6 { get { return cell_6; } }

        [SerializeField] [UGUIElementProperty("cell_7")]
        private UnityEngine.UI.Image cell_7;
        public UnityEngine.UI.Image m_cell_7 { get { return cell_7; } }

        [SerializeField] [UGUIElementProperty("cell_8")]
        private UnityEngine.UI.Image cell_8;
        public UnityEngine.UI.Image m_cell_8 { get { return cell_8; } }

        [SerializeField] [UGUIElementProperty("cell_9")]
        private UnityEngine.UI.Image cell_9;
        public UnityEngine.UI.Image m_cell_9 { get { return cell_9; } }

        [SerializeField] [UGUIElementProperty("cell_10")]
        private UnityEngine.UI.Image cell_10;
        public UnityEngine.UI.Image m_cell_10 { get { return cell_10; } }

        [SerializeField] [UGUIElementProperty("cell_11")]
        private UnityEngine.UI.Image cell_11;
        public UnityEngine.UI.Image m_cell_11 { get { return cell_11; } }

        [SerializeField] [UGUIElementProperty("cell_12")]
        private UnityEngine.UI.Image cell_12;
        public UnityEngine.UI.Image m_cell_12 { get { return cell_12; } }

        [SerializeField] [UGUIElementProperty("cell_13")]
        private UnityEngine.UI.Image cell_13;
        public UnityEngine.UI.Image m_cell_13 { get { return cell_13; } }

        [SerializeField] [UGUIElementProperty("cell_14")]
        private UnityEngine.UI.Image cell_14;
        public UnityEngine.UI.Image m_cell_14 { get { return cell_14; } }

        [SerializeField] [UGUIElementProperty("cell_15")]
        private UnityEngine.UI.Image cell_15;
        public UnityEngine.UI.Image m_cell_15 { get { return cell_15; } }

        [SerializeField] [UGUIElementProperty("cell_16")]
        private UnityEngine.UI.Image cell_16;
        public UnityEngine.UI.Image m_cell_16 { get { return cell_16; } }

        [SerializeField] [UGUIElementProperty("cell_17")]
        private UnityEngine.UI.Image cell_17;
        public UnityEngine.UI.Image m_cell_17 { get { return cell_17; } }

        [SerializeField] [UGUIElementProperty("cell_18")]
        private UnityEngine.UI.Image cell_18;
        public UnityEngine.UI.Image m_cell_18 { get { return cell_18; } }

        [SerializeField] [UGUIElementProperty("cell_19")]
        private UnityEngine.UI.Image cell_19;
        public UnityEngine.UI.Image m_cell_19 { get { return cell_19; } }

        [SerializeField] [UGUIElementProperty("cell_20")]
        private UnityEngine.UI.Image cell_20;
        public UnityEngine.UI.Image m_cell_20 { get { return cell_20; } }

        [SerializeField] [UGUIElementProperty("cell_21")]
        private UnityEngine.UI.Image cell_21;
        public UnityEngine.UI.Image m_cell_21 { get { return cell_21; } }

        [SerializeField] [UGUIElementProperty("cell_22")]
        private UnityEngine.UI.Image cell_22;
        public UnityEngine.UI.Image m_cell_22 { get { return cell_22; } }

        [SerializeField] [UGUIElementProperty("cell_23")]
        private UnityEngine.UI.Image cell_23;
        public UnityEngine.UI.Image m_cell_23 { get { return cell_23; } }

        [SerializeField] [UGUIElementProperty("cell_24")]
        private UnityEngine.UI.Image cell_24;
        public UnityEngine.UI.Image m_cell_24 { get { return cell_24; } }

        [SerializeField] [UGUIElementProperty("cell_25")]
        private UnityEngine.UI.Image cell_25;
        public UnityEngine.UI.Image m_cell_25 { get { return cell_25; } }

        [SerializeField] [UGUIElementProperty("cell_26")]
        private UnityEngine.UI.Image cell_26;
        public UnityEngine.UI.Image m_cell_26 { get { return cell_26; } }

        [SerializeField] [UGUIElementProperty("cell_27")]
        private UnityEngine.UI.Image cell_27;
        public UnityEngine.UI.Image m_cell_27 { get { return cell_27; } }

        [SerializeField] [UGUIElementProperty("cell_28")]
        private UnityEngine.UI.Image cell_28;
        public UnityEngine.UI.Image m_cell_28 { get { return cell_28; } }

        [SerializeField] [UGUIElementProperty("cell_29")]
        private UnityEngine.UI.Image cell_29;
        public UnityEngine.UI.Image m_cell_29 { get { return cell_29; } }

        [SerializeField] [UGUIElementProperty("cell_30")]
        private UnityEngine.UI.Image cell_30;
        public UnityEngine.UI.Image m_cell_30 { get { return cell_30; } }

        [SerializeField] [UGUIElementProperty("cell_31")]
        private UnityEngine.UI.Image cell_31;
        public UnityEngine.UI.Image m_cell_31 { get { return cell_31; } }

        [SerializeField] [UGUIElementProperty("cell_32")]
        private UnityEngine.UI.Image cell_32;
        public UnityEngine.UI.Image m_cell_32 { get { return cell_32; } }

        [SerializeField] [UGUIElementProperty("cell_33")]
        private UnityEngine.UI.Image cell_33;
        public UnityEngine.UI.Image m_cell_33 { get { return cell_33; } }

        [SerializeField] [UGUIElementProperty("cell_34")]
        private UnityEngine.UI.Image cell_34;
        public UnityEngine.UI.Image m_cell_34 { get { return cell_34; } }

        [SerializeField] [UGUIElementProperty("cell_35")]
        private UnityEngine.UI.Image cell_35;
        public UnityEngine.UI.Image m_cell_35 { get { return cell_35; } }

        [SerializeField] [UGUIElementProperty("cell_36")]
        private UnityEngine.UI.Image cell_36;
        public UnityEngine.UI.Image m_cell_36 { get { return cell_36; } }

        [SerializeField] [UGUIElementProperty("cell_37")]
        private UnityEngine.UI.Image cell_37;
        public UnityEngine.UI.Image m_cell_37 { get { return cell_37; } }

        [SerializeField] [UGUIElementProperty("cell_38")]
        private UnityEngine.UI.Image cell_38;
        public UnityEngine.UI.Image m_cell_38 { get { return cell_38; } }

        [SerializeField] [UGUIElementProperty("cell_39")]
        private UnityEngine.UI.Image cell_39;
        public UnityEngine.UI.Image m_cell_39 { get { return cell_39; } }

        [SerializeField] [UGUIElementProperty("cell_40")]
        private UnityEngine.UI.Image cell_40;
        public UnityEngine.UI.Image m_cell_40 { get { return cell_40; } }

        [SerializeField] [UGUIElementProperty("cell_41")]
        private UnityEngine.UI.Image cell_41;
        public UnityEngine.UI.Image m_cell_41 { get { return cell_41; } }

        [SerializeField] [UGUIElementProperty("column_button_0")]
        private UnityEngine.UI.Button column_button_0;
        public UnityEngine.UI.Button m_column_button_0 { get { return column_button_0; } }

        [SerializeField] [UGUIElementProperty("column_button_1")]
        private UnityEngine.UI.Button column_button_1;
        public UnityEngine.UI.Button m_column_button_1 { get { return column_button_1; } }

        [SerializeField] [UGUIElementProperty("column_button_2")]
        private UnityEngine.UI.Button column_button_2;
        public UnityEngine.UI.Button m_column_button_2 { get { return column_button_2; } }

        [SerializeField] [UGUIElementProperty("column_button_3")]
        private UnityEngine.UI.Button column_button_3;
        public UnityEngine.UI.Button m_column_button_3 { get { return column_button_3; } }

        [SerializeField] [UGUIElementProperty("column_button_4")]
        private UnityEngine.UI.Button column_button_4;
        public UnityEngine.UI.Button m_column_button_4 { get { return column_button_4; } }

        [SerializeField] [UGUIElementProperty("column_button_5")]
        private UnityEngine.UI.Button column_button_5;
        public UnityEngine.UI.Button m_column_button_5 { get { return column_button_5; } }

        [SerializeField] [UGUIElementProperty("column_button_6")]
        private UnityEngine.UI.Button column_button_6;
        public UnityEngine.UI.Button m_column_button_6 { get { return column_button_6; } }

        [SerializeField] [UGUIElementProperty("result_text")]
        private UnityEngine.UI.Text result_text;
        public UnityEngine.UI.Text m_result_text { get { return result_text; } }

        [SerializeField] [UGUIElementProperty("start_button")]
        private UnityEngine.UI.Button start_button;
        public UnityEngine.UI.Button m_start_button { get { return start_button; } }

        [SerializeField] [UGUIElementProperty("restart_button")]
        private UnityEngine.UI.Button restart_button;
        public UnityEngine.UI.Button m_restart_button { get { return restart_button; } }

        [SerializeField] [UGUIElementProperty("leave_button")]
        private UnityEngine.UI.Button leave_button;
        public UnityEngine.UI.Button m_leave_button { get { return leave_button; } }

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
            title = gameObject.transform.FindChildName("title").GetComponent<UnityEngine.UI.Text>();
            room_state_text = gameObject.transform.FindChildName("room_state_text").GetComponent<UnityEngine.UI.Text>();
            turn_text = gameObject.transform.FindChildName("turn_text").GetComponent<UnityEngine.UI.Text>();
            board_grid = gameObject.transform.FindChildName("board_grid").GetComponent<UnityEngine.UI.Image>();
            cell_0 = gameObject.transform.FindChildName("cell_0").GetComponent<UnityEngine.UI.Image>();
            cell_1 = gameObject.transform.FindChildName("cell_1").GetComponent<UnityEngine.UI.Image>();
            cell_2 = gameObject.transform.FindChildName("cell_2").GetComponent<UnityEngine.UI.Image>();
            cell_3 = gameObject.transform.FindChildName("cell_3").GetComponent<UnityEngine.UI.Image>();
            cell_4 = gameObject.transform.FindChildName("cell_4").GetComponent<UnityEngine.UI.Image>();
            cell_5 = gameObject.transform.FindChildName("cell_5").GetComponent<UnityEngine.UI.Image>();
            cell_6 = gameObject.transform.FindChildName("cell_6").GetComponent<UnityEngine.UI.Image>();
            cell_7 = gameObject.transform.FindChildName("cell_7").GetComponent<UnityEngine.UI.Image>();
            cell_8 = gameObject.transform.FindChildName("cell_8").GetComponent<UnityEngine.UI.Image>();
            cell_9 = gameObject.transform.FindChildName("cell_9").GetComponent<UnityEngine.UI.Image>();
            cell_10 = gameObject.transform.FindChildName("cell_10").GetComponent<UnityEngine.UI.Image>();
            cell_11 = gameObject.transform.FindChildName("cell_11").GetComponent<UnityEngine.UI.Image>();
            cell_12 = gameObject.transform.FindChildName("cell_12").GetComponent<UnityEngine.UI.Image>();
            cell_13 = gameObject.transform.FindChildName("cell_13").GetComponent<UnityEngine.UI.Image>();
            cell_14 = gameObject.transform.FindChildName("cell_14").GetComponent<UnityEngine.UI.Image>();
            cell_15 = gameObject.transform.FindChildName("cell_15").GetComponent<UnityEngine.UI.Image>();
            cell_16 = gameObject.transform.FindChildName("cell_16").GetComponent<UnityEngine.UI.Image>();
            cell_17 = gameObject.transform.FindChildName("cell_17").GetComponent<UnityEngine.UI.Image>();
            cell_18 = gameObject.transform.FindChildName("cell_18").GetComponent<UnityEngine.UI.Image>();
            cell_19 = gameObject.transform.FindChildName("cell_19").GetComponent<UnityEngine.UI.Image>();
            cell_20 = gameObject.transform.FindChildName("cell_20").GetComponent<UnityEngine.UI.Image>();
            cell_21 = gameObject.transform.FindChildName("cell_21").GetComponent<UnityEngine.UI.Image>();
            cell_22 = gameObject.transform.FindChildName("cell_22").GetComponent<UnityEngine.UI.Image>();
            cell_23 = gameObject.transform.FindChildName("cell_23").GetComponent<UnityEngine.UI.Image>();
            cell_24 = gameObject.transform.FindChildName("cell_24").GetComponent<UnityEngine.UI.Image>();
            cell_25 = gameObject.transform.FindChildName("cell_25").GetComponent<UnityEngine.UI.Image>();
            cell_26 = gameObject.transform.FindChildName("cell_26").GetComponent<UnityEngine.UI.Image>();
            cell_27 = gameObject.transform.FindChildName("cell_27").GetComponent<UnityEngine.UI.Image>();
            cell_28 = gameObject.transform.FindChildName("cell_28").GetComponent<UnityEngine.UI.Image>();
            cell_29 = gameObject.transform.FindChildName("cell_29").GetComponent<UnityEngine.UI.Image>();
            cell_30 = gameObject.transform.FindChildName("cell_30").GetComponent<UnityEngine.UI.Image>();
            cell_31 = gameObject.transform.FindChildName("cell_31").GetComponent<UnityEngine.UI.Image>();
            cell_32 = gameObject.transform.FindChildName("cell_32").GetComponent<UnityEngine.UI.Image>();
            cell_33 = gameObject.transform.FindChildName("cell_33").GetComponent<UnityEngine.UI.Image>();
            cell_34 = gameObject.transform.FindChildName("cell_34").GetComponent<UnityEngine.UI.Image>();
            cell_35 = gameObject.transform.FindChildName("cell_35").GetComponent<UnityEngine.UI.Image>();
            cell_36 = gameObject.transform.FindChildName("cell_36").GetComponent<UnityEngine.UI.Image>();
            cell_37 = gameObject.transform.FindChildName("cell_37").GetComponent<UnityEngine.UI.Image>();
            cell_38 = gameObject.transform.FindChildName("cell_38").GetComponent<UnityEngine.UI.Image>();
            cell_39 = gameObject.transform.FindChildName("cell_39").GetComponent<UnityEngine.UI.Image>();
            cell_40 = gameObject.transform.FindChildName("cell_40").GetComponent<UnityEngine.UI.Image>();
            cell_41 = gameObject.transform.FindChildName("cell_41").GetComponent<UnityEngine.UI.Image>();
            column_button_0 = gameObject.transform.FindChildName("column_button_0").GetComponent<UnityEngine.UI.Button>();
            column_button_1 = gameObject.transform.FindChildName("column_button_1").GetComponent<UnityEngine.UI.Button>();
            column_button_2 = gameObject.transform.FindChildName("column_button_2").GetComponent<UnityEngine.UI.Button>();
            column_button_3 = gameObject.transform.FindChildName("column_button_3").GetComponent<UnityEngine.UI.Button>();
            column_button_4 = gameObject.transform.FindChildName("column_button_4").GetComponent<UnityEngine.UI.Button>();
            column_button_5 = gameObject.transform.FindChildName("column_button_5").GetComponent<UnityEngine.UI.Button>();
            column_button_6 = gameObject.transform.FindChildName("column_button_6").GetComponent<UnityEngine.UI.Button>();
            result_text = gameObject.transform.FindChildName("result_text").GetComponent<UnityEngine.UI.Text>();
            start_button = gameObject.transform.FindChildName("start_button").GetComponent<UnityEngine.UI.Button>();
            restart_button = gameObject.transform.FindChildName("restart_button").GetComponent<UnityEngine.UI.Button>();
            leave_button = gameObject.transform.FindChildName("leave_button").GetComponent<UnityEngine.UI.Button>();
            close_button = gameObject.transform.FindChildName("close_button").GetComponent<UnityEngine.UI.Button>();
        }
    }
}
#endif
