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
	/// 代码生成的UI代码UIMain
	/// </summary>
	[DisallowMultipleComponent]
	[OptionUIConfig(null, "Assets/Bundles/UI/UIMain")]
	public sealed partial class UIMain : UGUI
	{
		public GameObject self { get; private set; }

		#region Properties

		[SerializeField] [UGUIElementProperty("BgImage")]
		private UnityEngine.UI.Image BgImage;

		public UnityEngine.UI.Image m_BgImage
		{
			get { return BgImage;}
		}

		[SerializeField] [UGUIElementProperty("bag_button/Text")]
		private UnityEngine.UI.Text bag_button_Text;

		public UnityEngine.UI.Text m_bag_button_Text
		{
			get { return bag_button_Text;}
		}

		[SerializeField] [UGUIElementProperty("bag_button")]
		private UnityEngine.UI.Button bag_button;

		public UnityEngine.UI.Button m_bag_button
		{
			get { return bag_button;}
		}

		[SerializeField] [UGUIElementProperty("room_button/Text")]
		private UnityEngine.UI.Text room_button_Text;

		public UnityEngine.UI.Text m_room_button_Text
		{
			get { return room_button_Text;}
		}

		[SerializeField] [UGUIElementProperty("room_button")]
		private UnityEngine.UI.Button room_button;

		public UnityEngine.UI.Button m_room_button
		{
			get { return room_button;}
		}

		[SerializeField] [UGUIElementProperty("player_icon")]
		private GameFrameX.UI.UGUI.Runtime.UIImage player_icon;

		public GameFrameX.UI.UGUI.Runtime.UIImage m_player_icon
		{
			get { return player_icon;}
		}

		[SerializeField] [UGUIElementProperty("player_name")]
		private UnityEngine.UI.Text player_name;

		public UnityEngine.UI.Text m_player_name
		{
			get { return player_name;}
		}

		[SerializeField] [UGUIElementProperty("player_level")]
		private UnityEngine.UI.Text player_level;

		public UnityEngine.UI.Text m_player_level
		{
			get { return player_level;}
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
			BgImage = gameObject.transform.FindChildName("BgImage").GetComponent<UnityEngine.UI.Image>();
			bag_button_Text = gameObject.transform.FindChildName("bag_button/Text").GetComponent<UnityEngine.UI.Text>();
			bag_button = gameObject.transform.FindChildName("bag_button").GetComponent<UnityEngine.UI.Button>();
			room_button_Text = gameObject.transform.FindChildName("room_button/Text").GetComponent<UnityEngine.UI.Text>();
			room_button = gameObject.transform.FindChildName("room_button").GetComponent<UnityEngine.UI.Button>();
			player_icon = gameObject.transform.FindChildName("player_icon").GetComponent<GameFrameX.UI.UGUI.Runtime.UIImage>();
			player_name = gameObject.transform.FindChildName("player_name").GetComponent<UnityEngine.UI.Text>();
			player_level = gameObject.transform.FindChildName("player_level").GetComponent<UnityEngine.UI.Text>();
		}
	}
}
#endif
