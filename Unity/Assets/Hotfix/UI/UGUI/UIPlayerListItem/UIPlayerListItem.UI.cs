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
	/// 代码生成的UI代码UIPlayerListItem
	/// </summary>
	[DisallowMultipleComponent]
	[OptionUIConfig(null, "Assets/Bundles/UI/UILogin")]
	public sealed partial class UIPlayerListItem : UGUI
	{
		public GameObject self { get; private set; }

		#region Properties

		[UGUIElementProperty("click_Button")]
		private UnityEngine.UI.Button click_Button;

		public UnityEngine.UI.Button m_click_Button
		{
			get { return click_Button;}
		}

		[UGUIElementProperty("icon")]
		private GameFrameX.UI.UGUI.Runtime.UIImage icon;

		public GameFrameX.UI.UGUI.Runtime.UIImage m_icon
		{
			get { return icon;}
		}

		[UGUIElementProperty("level_text")]
		private UnityEngine.UI.Text level_text;

		public UnityEngine.UI.Text m_level_text
		{
			get { return level_text;}
		}

		[UGUIElementProperty("name_text")]
		private UnityEngine.UI.Text name_text;

		public UnityEngine.UI.Text m_name_text
		{
			get { return name_text;}
		}

		#endregion

		public static UIPlayerListItem Create(GameObject go)
		{
			var ui = go.GetOrAddComponent<UIPlayerListItem>();
			ui?.InitView();
			return ui;
		}

		/// <summary>
		/// 通过此方法获取的UGUI，在Dispose时不会释放GameObject，需要自行管理（一般在配合对象池机制时使用）。
		/// </summary>
		public static UIPlayerListItem GetFromPool(GameObject go)
		{
			var ui = go.GetComponent<UIPlayerListItem>();
			if (ui == null)
			{
				ui = Create(go);
			}

			ui.IsFromPool = true;
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
			click_Button = gameObject.transform.FindChildName("click_Button").GetComponent<UnityEngine.UI.Button>();
			icon = gameObject.transform.FindChildName("icon").GetComponent<GameFrameX.UI.UGUI.Runtime.UIImage>();
			level_text = gameObject.transform.FindChildName("level_text").GetComponent<UnityEngine.UI.Text>();
			name_text = gameObject.transform.FindChildName("name_text").GetComponent<UnityEngine.UI.Text>();
		}
	}
}
#endif
