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
	/// 代码生成的UI代码UILogin
	/// </summary>
	[DisallowMultipleComponent]
	[OptionUIConfig(null, "Assets/Bundles/UI/UILogin")]
	public sealed partial class UILogin : UGUI
	{
		public GameObject self { get; private set; }

		#region Properties

		[SerializeField] [UGUIElementProperty("UserName/Placeholder")]
		private UnityEngine.UI.Text UserName_Placeholder;

		public UnityEngine.UI.Text m_UserName_Placeholder
		{
			get { return UserName_Placeholder;}
		}

		[SerializeField] [UGUIElementProperty("UserName/Text")]
		private UnityEngine.UI.Text UserName_Text;

		public UnityEngine.UI.Text m_UserName_Text
		{
			get { return UserName_Text;}
		}

		[SerializeField] [UGUIElementProperty("UserName")]
		private UnityEngine.UI.InputField UserName;

		public UnityEngine.UI.InputField m_UserName
		{
			get { return UserName;}
		}

		[SerializeField] [UGUIElementProperty("Password/Placeholder")]
		private UnityEngine.UI.Text Password_Placeholder;

		public UnityEngine.UI.Text m_Password_Placeholder
		{
			get { return Password_Placeholder;}
		}

		[SerializeField] [UGUIElementProperty("Password/Text")]
		private UnityEngine.UI.Text Password_Text;

		public UnityEngine.UI.Text m_Password_Text
		{
			get { return Password_Text;}
		}

		[SerializeField] [UGUIElementProperty("Password")]
		private UnityEngine.UI.InputField Password;

		public UnityEngine.UI.InputField m_Password
		{
			get { return Password;}
		}

		[SerializeField] [UGUIElementProperty("enter/Text")]
		private UnityEngine.UI.Text enter_Text;

		public UnityEngine.UI.Text m_enter_Text
		{
			get { return enter_Text;}
		}

		[SerializeField] [UGUIElementProperty("enter")]
		private UnityEngine.UI.Button enter;

		public UnityEngine.UI.Button m_enter
		{
			get { return enter;}
		}

		[SerializeField] [UGUIElementProperty("ErrorText")]
		private UnityEngine.UI.Text ErrorText;

		public UnityEngine.UI.Text m_ErrorText
		{
			get { return ErrorText;}
		}

		[SerializeField] [UGUIElementProperty("TitleText")]
		private UnityEngine.UI.Text TitleText;

		public UnityEngine.UI.Text m_TitleText
		{
			get { return TitleText;}
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
			UserName_Placeholder = gameObject.transform.FindChildName("UserName/Placeholder").GetComponent<UnityEngine.UI.Text>();
			UserName_Text = gameObject.transform.FindChildName("UserName/Text").GetComponent<UnityEngine.UI.Text>();
			UserName = gameObject.transform.FindChildName("UserName").GetComponent<UnityEngine.UI.InputField>();
			Password_Placeholder = gameObject.transform.FindChildName("Password/Placeholder").GetComponent<UnityEngine.UI.Text>();
			Password_Text = gameObject.transform.FindChildName("Password/Text").GetComponent<UnityEngine.UI.Text>();
			Password = gameObject.transform.FindChildName("Password").GetComponent<UnityEngine.UI.InputField>();
			enter_Text = gameObject.transform.FindChildName("enter/Text").GetComponent<UnityEngine.UI.Text>();
			enter = gameObject.transform.FindChildName("enter").GetComponent<UnityEngine.UI.Button>();
			ErrorText = gameObject.transform.FindChildName("ErrorText").GetComponent<UnityEngine.UI.Text>();
			TitleText = gameObject.transform.FindChildName("TitleText").GetComponent<UnityEngine.UI.Text>();
		}
	}
}
#endif
