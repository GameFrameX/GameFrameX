/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

#if ENABLE_UI_FAIRYGUI
using FairyGUI;
using FairyGUI.Utils;
using GameFrameX.UI.Runtime;
using GameFrameX.UI.FairyGUI.Runtime;
using GameFrameX.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
    [OptionUIConfig(UIPackageName)]
    public sealed partial class UIConnectFourBattlePanel : FUI
    {
        public const string UIPackageName = "UIConnectFour";
        public const string UIResName = "UIConnectFourBattlePanel";
        public const string URL = "ui://cfour001battl1";

        /// <summary>
        /// {uiResName}的组件类型(GComponent、GButton、GProcessBar等)，它们都是GObject的子类。
        /// </summary>
        public GComponent self { get; private set; }

		public GGraph m_mask { get; private set; }
		public GGraph m_window_bg { get; private set; }
		public GGraph m_top_bg { get; private set; }
		public GTextField m_room_title_text { get; private set; }
		public GTextField m_room_state_text { get; private set; }
		public GTextField m_turn_text { get; private set; }
		public UIConnectFourButton m_leave_button { get; private set; }
		public UIConnectFourButton m_close_button { get; private set; }
		public GTextField m_result_text { get; private set; }
		public GGraph m_board_bg { get; private set; }
		public GGraph m_cell_0 { get; private set; }
		public GGraph m_cell_1 { get; private set; }
		public GGraph m_cell_2 { get; private set; }
		public GGraph m_cell_3 { get; private set; }
		public GGraph m_cell_4 { get; private set; }
		public GGraph m_cell_5 { get; private set; }
		public GGraph m_cell_6 { get; private set; }
		public GGraph m_cell_7 { get; private set; }
		public GGraph m_cell_8 { get; private set; }
		public GGraph m_cell_9 { get; private set; }
		public GGraph m_cell_10 { get; private set; }
		public GGraph m_cell_11 { get; private set; }
		public GGraph m_cell_12 { get; private set; }
		public GGraph m_cell_13 { get; private set; }
		public GGraph m_cell_14 { get; private set; }
		public GGraph m_cell_15 { get; private set; }
		public GGraph m_cell_16 { get; private set; }
		public GGraph m_cell_17 { get; private set; }
		public GGraph m_cell_18 { get; private set; }
		public GGraph m_cell_19 { get; private set; }
		public GGraph m_cell_20 { get; private set; }
		public GGraph m_cell_21 { get; private set; }
		public GGraph m_cell_22 { get; private set; }
		public GGraph m_cell_23 { get; private set; }
		public GGraph m_cell_24 { get; private set; }
		public GGraph m_cell_25 { get; private set; }
		public GGraph m_cell_26 { get; private set; }
		public GGraph m_cell_27 { get; private set; }
		public GGraph m_cell_28 { get; private set; }
		public GGraph m_cell_29 { get; private set; }
		public GGraph m_cell_30 { get; private set; }
		public GGraph m_cell_31 { get; private set; }
		public GGraph m_cell_32 { get; private set; }
		public GGraph m_cell_33 { get; private set; }
		public GGraph m_cell_34 { get; private set; }
		public GGraph m_cell_35 { get; private set; }
		public GGraph m_cell_36 { get; private set; }
		public GGraph m_cell_37 { get; private set; }
		public GGraph m_cell_38 { get; private set; }
		public GGraph m_cell_39 { get; private set; }
		public GGraph m_cell_40 { get; private set; }
		public GGraph m_cell_41 { get; private set; }
		public UIConnectFourButton m_column_button_0 { get; private set; }
		public UIConnectFourButton m_column_button_1 { get; private set; }
		public UIConnectFourButton m_column_button_2 { get; private set; }
		public UIConnectFourButton m_column_button_3 { get; private set; }
		public UIConnectFourButton m_column_button_4 { get; private set; }
		public UIConnectFourButton m_column_button_5 { get; private set; }
		public UIConnectFourButton m_column_button_6 { get; private set; }
		public UIConnectFourButton m_start_button { get; private set; }
		public UIConnectFourButton m_restart_button { get; private set; }
		public GTextField m_tips_text { get; private set; }

        private static GObject CreateGObject()
        {
            return UIPackage.CreateObject(UIPackageName, UIResName);
        }

        public static UIConnectFourBattlePanel CreateInstance()
        {
            return Create(CreateGObject());
        }

        public static UIConnectFourBattlePanel Create(GObject go)
        {
            var fui = go.displayObject.gameObject.GetOrAddComponent<UIConnectFourBattlePanel>();
            fui?.SetGObject(go);
            fui?.InitView();
            return fui;
        }

        /// <summary>
        /// 通过此方法获取的FUI，在Dispose时不会释放GObject，需要自行管理（一般在配合FGUI的Pool机制时使用）。
        /// </summary>
        public static UIConnectFourBattlePanel GetFormPool(GObject go)
        {
            var fui = go.Get<UIConnectFourBattlePanel>();
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
				m_turn_text = (GTextField)com.GetChild("turn_text");
				m_leave_button = UIConnectFourButton.Create(com.GetChild("leave_button"));
				m_close_button = UIConnectFourButton.Create(com.GetChild("close_button"));
				m_result_text = (GTextField)com.GetChild("result_text");
				m_board_bg = (GGraph)com.GetChild("board_bg");
				m_cell_0 = (GGraph)com.GetChild("cell_0");
				m_cell_1 = (GGraph)com.GetChild("cell_1");
				m_cell_2 = (GGraph)com.GetChild("cell_2");
				m_cell_3 = (GGraph)com.GetChild("cell_3");
				m_cell_4 = (GGraph)com.GetChild("cell_4");
				m_cell_5 = (GGraph)com.GetChild("cell_5");
				m_cell_6 = (GGraph)com.GetChild("cell_6");
				m_cell_7 = (GGraph)com.GetChild("cell_7");
				m_cell_8 = (GGraph)com.GetChild("cell_8");
				m_cell_9 = (GGraph)com.GetChild("cell_9");
				m_cell_10 = (GGraph)com.GetChild("cell_10");
				m_cell_11 = (GGraph)com.GetChild("cell_11");
				m_cell_12 = (GGraph)com.GetChild("cell_12");
				m_cell_13 = (GGraph)com.GetChild("cell_13");
				m_cell_14 = (GGraph)com.GetChild("cell_14");
				m_cell_15 = (GGraph)com.GetChild("cell_15");
				m_cell_16 = (GGraph)com.GetChild("cell_16");
				m_cell_17 = (GGraph)com.GetChild("cell_17");
				m_cell_18 = (GGraph)com.GetChild("cell_18");
				m_cell_19 = (GGraph)com.GetChild("cell_19");
				m_cell_20 = (GGraph)com.GetChild("cell_20");
				m_cell_21 = (GGraph)com.GetChild("cell_21");
				m_cell_22 = (GGraph)com.GetChild("cell_22");
				m_cell_23 = (GGraph)com.GetChild("cell_23");
				m_cell_24 = (GGraph)com.GetChild("cell_24");
				m_cell_25 = (GGraph)com.GetChild("cell_25");
				m_cell_26 = (GGraph)com.GetChild("cell_26");
				m_cell_27 = (GGraph)com.GetChild("cell_27");
				m_cell_28 = (GGraph)com.GetChild("cell_28");
				m_cell_29 = (GGraph)com.GetChild("cell_29");
				m_cell_30 = (GGraph)com.GetChild("cell_30");
				m_cell_31 = (GGraph)com.GetChild("cell_31");
				m_cell_32 = (GGraph)com.GetChild("cell_32");
				m_cell_33 = (GGraph)com.GetChild("cell_33");
				m_cell_34 = (GGraph)com.GetChild("cell_34");
				m_cell_35 = (GGraph)com.GetChild("cell_35");
				m_cell_36 = (GGraph)com.GetChild("cell_36");
				m_cell_37 = (GGraph)com.GetChild("cell_37");
				m_cell_38 = (GGraph)com.GetChild("cell_38");
				m_cell_39 = (GGraph)com.GetChild("cell_39");
				m_cell_40 = (GGraph)com.GetChild("cell_40");
				m_cell_41 = (GGraph)com.GetChild("cell_41");
				m_column_button_0 = UIConnectFourButton.Create(com.GetChild("column_button_0"));
				m_column_button_1 = UIConnectFourButton.Create(com.GetChild("column_button_1"));
				m_column_button_2 = UIConnectFourButton.Create(com.GetChild("column_button_2"));
				m_column_button_3 = UIConnectFourButton.Create(com.GetChild("column_button_3"));
				m_column_button_4 = UIConnectFourButton.Create(com.GetChild("column_button_4"));
				m_column_button_5 = UIConnectFourButton.Create(com.GetChild("column_button_5"));
				m_column_button_6 = UIConnectFourButton.Create(com.GetChild("column_button_6"));
				m_start_button = UIConnectFourButton.Create(com.GetChild("start_button"));
				m_restart_button = UIConnectFourButton.Create(com.GetChild("restart_button"));
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
			m_turn_text = null;
			m_leave_button = null;
			m_close_button = null;
			m_result_text = null;
			m_board_bg = null;
			m_cell_0 = null;
			m_cell_1 = null;
			m_cell_2 = null;
			m_cell_3 = null;
			m_cell_4 = null;
			m_cell_5 = null;
			m_cell_6 = null;
			m_cell_7 = null;
			m_cell_8 = null;
			m_cell_9 = null;
			m_cell_10 = null;
			m_cell_11 = null;
			m_cell_12 = null;
			m_cell_13 = null;
			m_cell_14 = null;
			m_cell_15 = null;
			m_cell_16 = null;
			m_cell_17 = null;
			m_cell_18 = null;
			m_cell_19 = null;
			m_cell_20 = null;
			m_cell_21 = null;
			m_cell_22 = null;
			m_cell_23 = null;
			m_cell_24 = null;
			m_cell_25 = null;
			m_cell_26 = null;
			m_cell_27 = null;
			m_cell_28 = null;
			m_cell_29 = null;
			m_cell_30 = null;
			m_cell_31 = null;
			m_cell_32 = null;
			m_cell_33 = null;
			m_cell_34 = null;
			m_cell_35 = null;
			m_cell_36 = null;
			m_cell_37 = null;
			m_cell_38 = null;
			m_cell_39 = null;
			m_cell_40 = null;
			m_cell_41 = null;
			m_column_button_0 = null;
			m_column_button_1 = null;
			m_column_button_2 = null;
			m_column_button_3 = null;
			m_column_button_4 = null;
			m_column_button_5 = null;
			m_column_button_6 = null;
			m_start_button = null;
			m_restart_button = null;
			m_tips_text = null;
            self = null;
        }

        private UIConnectFourBattlePanel(GObject gObject) : base(gObject)
        {
        }
    }
}
#endif