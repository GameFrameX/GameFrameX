// /**
//  *　　　　　　　　┏┓　　　┏┓+ +
//  *　　　　　　　┏┛┻━━━┛┻┓ + +
//  *　　　　　　　┃　　　　　　　┃ 　
//  *　　　　　　　┃　　　━　　　┃ ++ + + +
//  *　　　　　　 ████━████ ┃+
//  *　　　　　　　┃　　　　　　　┃ +
//  *　　　　　　　┃　　　┻　　　┃
//  *　　　　　　　┃　　　　　　　┃ + +
//  *　　　　　　　┗━┓　　　┏━┛
//  *　　　　　　　　　┃　　　┃　　　　　　　　　　　
//  *　　　　　　　　　┃　　　┃ + + + +
//  *　　　　　　　　　┃　　　┃　　　　Code is far away from bug with the animal protecting　　　　　　　
//  *　　　　　　　　　┃　　　┃ + 　　　　神兽保佑,代码无bug　　
//  *　　　　　　　　　┃　　　┃
//  *　　　　　　　　　┃　　　┃　　+　　　　　　　　　
//  *　　　　　　　　　┃　 　　┗━━━┓ + +
//  *　　　　　　　　　┃ 　　　　　　　┣┓
//  *　　　　　　　　　┃ 　　　　　　　┏┛
//  *　　　　　　　　　┗┓┓┏━┳┓┏┛ + + + +
//  *　　　　　　　　　　┃┫┫　┃┫┫
//  *　　　　　　　　　　┗┻┛　┗┻┛+ + + +
//  *
//  *
//  * ━━━━━━感觉萌萌哒━━━━━━
//  * 
//  * 说明：
//  *       用于Android 和IOS 平台的粘贴板的读写访问
//  * 文件名：BlankOperationClipboard.cs
//  * 创建时间：2016年07月14日 
//  * 创建人：Blank Alian
//  */
using UnityEngine;


#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

/// <summary>
/// 粘贴板的读写
/// </summary>
public sealed class BlankOperationClipboard
{

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern string GetClipBoard ();
	
	[DllImport("__Internal")]
	private static extern void SetClipBoard (string text);

#endif


    /// <summary>
    /// 获取粘贴板的值
    /// </summary>
    /// <returns></returns>
    public static string GetValue()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return string.Empty;
#elif UNITY_ANDROID
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.alianhome.operationclipboard.MainActivity"))
        {
            return androidJavaClass.CallStatic<string>("GetClipBoard");
        }
#elif UNITY_IOS
	    return GetClipBoard ();
#endif

    }
    /// <summary>
    /// 设置粘贴板的值
    /// </summary>
    /// <param name="text"></param>
    public static void SetValue(string text)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        TextEditor textEditor = new TextEditor();
        textEditor.text = text;
        textEditor.OnFocus();
        textEditor.Copy();
#elif UNITY_ANDROID
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.alianhome.operationclipboard.MainActivity"))
        {
             androidJavaClass.CallStatic("SetClipBoard", text);
        }
#elif UNITY_IOS
		SetClipBoard (text);
#endif
    }
}
