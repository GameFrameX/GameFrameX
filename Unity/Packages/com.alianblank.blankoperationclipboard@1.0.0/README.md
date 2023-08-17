# BlankOperationClipboard
    Unity 读写Android 和 iOS 的粘贴板插件

# Docs

> [文档说明在这里](https://blog.alianhome.com/BlankOperationClipboard)

# DEMO

```csharp

using UnityEngine;

public class BlankOperationClipboardDemo : MonoBehaviour
{
    private string text = "demoText";
    private string result = "";
    void OnGUI()
    {
        text = GUILayout.TextField(text, GUILayout.Width(500), GUILayout.Height(100));
        if (GUILayout.Button("SetValue", GUILayout.Width(500), GUILayout.Height(100)))
        {
            BlankOperationClipboard.SetValue(text);
        }
        if (GUILayout.Button("GetValue", GUILayout.Width(500), GUILayout.Height(100)))
        {
            result = BlankOperationClipboard.GetValue();
        }
        GUILayout.Label(result);
    }
}

```