/** This is an automatically generated class by UGUI. Please do not modify it. **/

#if ENABLE_UI_UGUI
using Cysharp.Threading.Tasks;
using GameFrameX.Entity.Runtime;
using GameFrameX.UI.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.UGUI.Runtime;
using UnityEngine;

namespace Hotfix.UI
{
	/// <summary>
	/// 代码生成的UI代码UIPlayerList
	/// </summary>
	[DisallowMultipleComponent]
	[OptionUIConfig(null, "Assets/Bundles/UI/UILogin")]
	public sealed partial class UIPlayerList : UGUI
	{
		public GameObject self { get; private set; }

		#region Properties

		[SerializeField] [UGUIElementProperty("left_Panel/Scroll View/Viewport/Content")]
		private UnityEngine.Transform left_Panel_ScrollView_Viewport_Content;

		public UnityEngine.Transform m_left_Panel_ScrollView_Viewport_Content
		{
			get { return left_Panel_ScrollView_Viewport_Content;}
		}

		[SerializeField] [UGUIElementProperty("left_Panel/Scroll View/Viewport")]
		private UnityEngine.UI.Image left_Panel_ScrollView_Viewport;

		public UnityEngine.UI.Image m_left_Panel_ScrollView_Viewport
		{
			get { return left_Panel_ScrollView_Viewport;}
		}

		[SerializeField] [UGUIElementProperty("left_Panel/Scroll View/Scrollbar Vertical/Sliding Area/Handle")]
		private UnityEngine.UI.Image left_Panel_ScrollView_ScrollbarVertical_SlidingArea_Handle;

		public UnityEngine.UI.Image m_left_Panel_ScrollView_ScrollbarVertical_SlidingArea_Handle
		{
			get { return left_Panel_ScrollView_ScrollbarVertical_SlidingArea_Handle;}
		}

		[SerializeField] [UGUIElementProperty("left_Panel/Scroll View/Scrollbar Vertical/Sliding Area")]
		private UnityEngine.Transform left_Panel_ScrollView_ScrollbarVertical_SlidingArea;

		public UnityEngine.Transform m_left_Panel_ScrollView_ScrollbarVertical_SlidingArea
		{
			get { return left_Panel_ScrollView_ScrollbarVertical_SlidingArea;}
		}

		[SerializeField] [UGUIElementProperty("left_Panel/Scroll View/Scrollbar Vertical")]
		private UnityEngine.UI.Scrollbar left_Panel_ScrollView_ScrollbarVertical;

		public UnityEngine.UI.Scrollbar m_left_Panel_ScrollView_ScrollbarVertical
		{
			get { return left_Panel_ScrollView_ScrollbarVertical;}
		}

		[SerializeField] [UGUIElementProperty("left_Panel/Scroll View")]
		private UnityEngine.UI.ScrollRect left_Panel_ScrollView;

		public UnityEngine.UI.ScrollRect m_left_Panel_ScrollView
		{
			get { return left_Panel_ScrollView;}
		}

		[SerializeField] [UGUIElementProperty("left_Panel")]
		private UnityEngine.UI.Image left_Panel;

		public UnityEngine.UI.Image m_left_Panel
		{
			get { return left_Panel;}
		}

		[SerializeField] [UGUIElementProperty("right_Panel/selected_name")]
		private UnityEngine.UI.Text right_Panel_selected_name;

		public UnityEngine.UI.Text m_right_Panel_selected_name
		{
			get { return right_Panel_selected_name;}
		}

		[SerializeField] [UGUIElementProperty("right_Panel/selected_level")]
		private UnityEngine.UI.Text right_Panel_selected_level;

		public UnityEngine.UI.Text m_right_Panel_selected_level
		{
			get { return right_Panel_selected_level;}
		}

		[SerializeField] [UGUIElementProperty("right_Panel/selected_icon")]
		private UnityEngine.UI.Image right_Panel_selected_icon;

		public UnityEngine.UI.Image m_right_Panel_selected_icon
		{
			get { return right_Panel_selected_icon;}
		}

		[SerializeField] [UGUIElementProperty("right_Panel/login_button/Text")]
		private UnityEngine.UI.Text right_Panel_login_button_Text;

		public UnityEngine.UI.Text m_right_Panel_login_button_Text
		{
			get { return right_Panel_login_button_Text;}
		}

		[SerializeField] [UGUIElementProperty("right_Panel/login_button")]
		private UnityEngine.UI.Button right_Panel_login_button;

		public UnityEngine.UI.Button m_right_Panel_login_button
		{
			get { return right_Panel_login_button;}
		}

		[SerializeField] [UGUIElementProperty("right_Panel")]
		private UnityEngine.UI.Image right_Panel;

		public UnityEngine.UI.Image m_right_Panel
		{
			get { return right_Panel;}
		}

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
			left_Panel_ScrollView_Viewport_Content = gameObject.transform.FindChildName("left_Panel/Scroll View/Viewport/Content").GetComponent<UnityEngine.Transform>();
			left_Panel_ScrollView_Viewport = gameObject.transform.FindChildName("left_Panel/Scroll View/Viewport").GetComponent<UnityEngine.UI.Image>();
			left_Panel_ScrollView_ScrollbarVertical_SlidingArea_Handle = gameObject.transform.FindChildName("left_Panel/Scroll View/Scrollbar Vertical/Sliding Area/Handle").GetComponent<UnityEngine.UI.Image>();
			left_Panel_ScrollView_ScrollbarVertical_SlidingArea = gameObject.transform.FindChildName("left_Panel/Scroll View/Scrollbar Vertical/Sliding Area").GetComponent<UnityEngine.Transform>();
			left_Panel_ScrollView_ScrollbarVertical = gameObject.transform.FindChildName("left_Panel/Scroll View/Scrollbar Vertical").GetComponent<UnityEngine.UI.Scrollbar>();
			left_Panel_ScrollView = gameObject.transform.FindChildName("left_Panel/Scroll View").GetComponent<UnityEngine.UI.ScrollRect>();
			left_Panel = gameObject.transform.FindChildName("left_Panel").GetComponent<UnityEngine.UI.Image>();
			right_Panel_selected_name = gameObject.transform.FindChildName("right_Panel/selected_name").GetComponent<UnityEngine.UI.Text>();
			right_Panel_selected_level = gameObject.transform.FindChildName("right_Panel/selected_level").GetComponent<UnityEngine.UI.Text>();
			right_Panel_selected_icon = gameObject.transform.FindChildName("right_Panel/selected_icon").GetComponent<UnityEngine.UI.Image>();
			right_Panel_login_button_Text = gameObject.transform.FindChildName("right_Panel/login_button/Text").GetComponent<UnityEngine.UI.Text>();
			right_Panel_login_button = gameObject.transform.FindChildName("right_Panel/login_button").GetComponent<UnityEngine.UI.Button>();
			right_Panel = gameObject.transform.FindChildName("right_Panel").GetComponent<UnityEngine.UI.Image>();
		}
	}
}
#endif
