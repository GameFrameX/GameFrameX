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

		[SerializeField] [UGUIElementProperty("user_name/Placeholder")]
		private UnityEngine.UI.Text user_name_Placeholder;

		public UnityEngine.UI.Text m_user_name_Placeholder
		{
			get { return user_name_Placeholder;}
		}

		[SerializeField] [UGUIElementProperty("user_name/Text")]
		private UnityEngine.UI.Text user_name_Text;

		public UnityEngine.UI.Text m_user_name_Text
		{
			get { return user_name_Text;}
		}

		[SerializeField] [UGUIElementProperty("user_name")]
		private UnityEngine.UI.InputField user_name;

		public UnityEngine.UI.InputField m_user_name
		{
			get { return user_name;}
		}

		[SerializeField] [UGUIElementProperty("password/Placeholder")]
		private UnityEngine.UI.Text password_Placeholder;

		public UnityEngine.UI.Text m_password_Placeholder
		{
			get { return password_Placeholder;}
		}

		[SerializeField] [UGUIElementProperty("password/Text")]
		private UnityEngine.UI.Text password_Text;

		public UnityEngine.UI.Text m_password_Text
		{
			get { return password_Text;}
		}

		[SerializeField] [UGUIElementProperty("password")]
		private UnityEngine.UI.InputField password;

		public UnityEngine.UI.InputField m_password
		{
			get { return password;}
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

		[SerializeField] [UGUIElementProperty("error_text")]
		private UnityEngine.UI.Text error_text;

		public UnityEngine.UI.Text m_error_text
		{
			get { return error_text;}
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
			user_name_Placeholder = gameObject.transform.FindChildName("user_name/Placeholder").GetComponent<UnityEngine.UI.Text>();
			user_name_Text = gameObject.transform.FindChildName("user_name/Text").GetComponent<UnityEngine.UI.Text>();
			user_name = gameObject.transform.FindChildName("user_name").GetComponent<UnityEngine.UI.InputField>();
			password_Placeholder = gameObject.transform.FindChildName("password/Placeholder").GetComponent<UnityEngine.UI.Text>();
			password_Text = gameObject.transform.FindChildName("password/Text").GetComponent<UnityEngine.UI.Text>();
			password = gameObject.transform.FindChildName("password").GetComponent<UnityEngine.UI.InputField>();
			enter_Text = gameObject.transform.FindChildName("enter/Text").GetComponent<UnityEngine.UI.Text>();
			enter = gameObject.transform.FindChildName("enter").GetComponent<UnityEngine.UI.Button>();
			error_text = gameObject.transform.FindChildName("error_text").GetComponent<UnityEngine.UI.Text>();
			TitleText = gameObject.transform.FindChildName("TitleText").GetComponent<UnityEngine.UI.Text>();
		}
	}
}
#endif
