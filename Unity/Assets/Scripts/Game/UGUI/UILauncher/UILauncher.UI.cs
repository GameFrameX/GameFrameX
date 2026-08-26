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

		[SerializeField] [UGUIElementProperty("TipText")]
		private UnityEngine.UI.Text TipText;

		public UnityEngine.UI.Text m_TipText
		{
			get { return TipText;}
		}

		[SerializeField] [UGUIElementProperty("ProgressBar/Background")]
		private UnityEngine.UI.Image ProgressBar_Background;

		public UnityEngine.UI.Image m_ProgressBar_Background
		{
			get { return ProgressBar_Background;}
		}

		[SerializeField] [UGUIElementProperty("ProgressBar/Fill Area/Fill")]
		private UnityEngine.UI.Image ProgressBar_FillArea_Fill;

		public UnityEngine.UI.Image m_ProgressBar_FillArea_Fill
		{
			get { return ProgressBar_FillArea_Fill;}
		}

		[SerializeField] [UGUIElementProperty("ProgressBar/Fill Area")]
		private UnityEngine.Transform ProgressBar_FillArea;

		public UnityEngine.Transform m_ProgressBar_FillArea
		{
			get { return ProgressBar_FillArea;}
		}

		[SerializeField] [UGUIElementProperty("ProgressBar")]
		private UnityEngine.UI.Slider ProgressBar;

		public UnityEngine.UI.Slider m_ProgressBar
		{
			get { return ProgressBar;}
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
			TipText = gameObject.transform.FindChildName("TipText").GetComponent<UnityEngine.UI.Text>();
			ProgressBar_Background = gameObject.transform.FindChildName("ProgressBar/Background").GetComponent<UnityEngine.UI.Image>();
			ProgressBar_FillArea_Fill = gameObject.transform.FindChildName("ProgressBar/Fill Area/Fill").GetComponent<UnityEngine.UI.Image>();
			ProgressBar_FillArea = gameObject.transform.FindChildName("ProgressBar/Fill Area").GetComponent<UnityEngine.Transform>();
			ProgressBar = gameObject.transform.FindChildName("ProgressBar").GetComponent<UnityEngine.UI.Slider>();
			upgrade_Image = gameObject.transform.FindChildName("upgrade/Image").GetComponent<UnityEngine.UI.Image>();
			upgrade_TextContent = gameObject.transform.FindChildName("upgrade/TextContent").GetComponent<UnityEngine.UI.Text>();
			upgrade_EnterButton_Text = gameObject.transform.FindChildName("upgrade/EnterButton/Text").GetComponent<UnityEngine.UI.Text>();
			upgrade_EnterButton = gameObject.transform.FindChildName("upgrade/EnterButton").GetComponent<UnityEngine.UI.Button>();
			upgrade = gameObject.transform.FindChildName("upgrade").GetComponent<UnityEngine.UI.Image>();
		}
	}
}
#endif
