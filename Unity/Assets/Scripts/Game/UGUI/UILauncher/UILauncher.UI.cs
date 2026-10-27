/** This is an automatically generated class by UGUI. Please do not modify it. **/

#if ENABLE_UI_UGUI
using Cysharp.Threading.Tasks;
using GameFrameX.Entity.Runtime;
using GameFrameX.UI.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.UGUI.Runtime;
using UnityEngine;

namespace Unity.Startup
{
	/// <summary>
	/// 代码生成的UI代码UILauncher
	/// </summary>
	[DisallowMultipleComponent]
	public sealed partial class UILauncher : UGUI
	{
		public GameObject self { get; private set; }

		#region Properties

		[SerializeField] [UGUIElementProperty("BgImage")]
		private UnityEngine.UI.Image BgImage;

		public UnityEngine.UI.Image m_BgImage
		{
			get { return BgImage;}
		}

		[SerializeField] [UGUIElementProperty("tip_text")]
		private UnityEngine.UI.Text tip_text;

		public UnityEngine.UI.Text m_tip_text
		{
			get { return tip_text;}
		}

		[SerializeField] [UGUIElementProperty("progress_bar/Background")]
		private UnityEngine.UI.Image progress_bar_Background;

		public UnityEngine.UI.Image m_progress_bar_Background
		{
			get { return progress_bar_Background;}
		}

		[SerializeField] [UGUIElementProperty("progress_bar/Fill Area/Fill")]
		private UnityEngine.UI.Image progress_bar_FillArea_Fill;

		public UnityEngine.UI.Image m_progress_bar_FillArea_Fill
		{
			get { return progress_bar_FillArea_Fill;}
		}

		[SerializeField] [UGUIElementProperty("progress_bar/Fill Area")]
		private UnityEngine.Transform progress_bar_FillArea;

		public UnityEngine.Transform m_progress_bar_FillArea
		{
			get { return progress_bar_FillArea;}
		}

		[SerializeField] [UGUIElementProperty("progress_bar")]
		private UnityEngine.UI.Slider progress_bar;

		public UnityEngine.UI.Slider m_progress_bar
		{
			get { return progress_bar;}
		}

		[SerializeField] [UGUIElementProperty("upgrade/Image")]
		private UnityEngine.UI.Image upgrade_Image;

		public UnityEngine.UI.Image m_upgrade_Image
		{
			get { return upgrade_Image;}
		}

		[SerializeField] [UGUIElementProperty("upgrade/TextContent")]
		private UnityEngine.UI.Text upgrade_TextContent;

		public UnityEngine.UI.Text m_upgrade_TextContent
		{
			get { return upgrade_TextContent;}
		}

		[SerializeField] [UGUIElementProperty("upgrade/EnterButton/Text")]
		private UnityEngine.UI.Text upgrade_EnterButton_Text;

		public UnityEngine.UI.Text m_upgrade_EnterButton_Text
		{
			get { return upgrade_EnterButton_Text;}
		}

		[SerializeField] [UGUIElementProperty("upgrade/EnterButton")]
		private UnityEngine.UI.Button upgrade_EnterButton;

		public UnityEngine.UI.Button m_upgrade_EnterButton
		{
			get { return upgrade_EnterButton;}
		}

		[SerializeField] [UGUIElementProperty("upgrade")]
		private UnityEngine.UI.Image upgrade;

		public UnityEngine.UI.Image m_upgrade
		{
			get { return upgrade;}
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
			tip_text = gameObject.transform.FindChildName("tip_text").GetComponent<UnityEngine.UI.Text>();
			progress_bar_Background = gameObject.transform.FindChildName("progress_bar/Background").GetComponent<UnityEngine.UI.Image>();
			progress_bar_FillArea_Fill = gameObject.transform.FindChildName("progress_bar/Fill Area/Fill").GetComponent<UnityEngine.UI.Image>();
			progress_bar_FillArea = gameObject.transform.FindChildName("progress_bar/Fill Area").GetComponent<UnityEngine.Transform>();
			progress_bar = gameObject.transform.FindChildName("progress_bar").GetComponent<UnityEngine.UI.Slider>();
			upgrade_Image = gameObject.transform.FindChildName("upgrade/Image").GetComponent<UnityEngine.UI.Image>();
			upgrade_TextContent = gameObject.transform.FindChildName("upgrade/TextContent").GetComponent<UnityEngine.UI.Text>();
			upgrade_EnterButton_Text = gameObject.transform.FindChildName("upgrade/EnterButton/Text").GetComponent<UnityEngine.UI.Text>();
			upgrade_EnterButton = gameObject.transform.FindChildName("upgrade/EnterButton").GetComponent<UnityEngine.UI.Button>();
			upgrade = gameObject.transform.FindChildName("upgrade").GetComponent<UnityEngine.UI.Image>();
		}
	}
}
#endif
