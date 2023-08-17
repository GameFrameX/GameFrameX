# BlankGetChannel

> Unity 分发ios 和android 渠道号获取

# Docs

> [文档说明在这里](https://blog.alianhome.com/BlankGetChannel)

# Example

```csharp

    using UnityEngine;

    public class BlankGetChannelExample : MonoBehaviour
    {
        private string value;
        void OnGUI()
        {
            GUILayout.Label(value);
            if (GUILayout.Button("GET", GUILayout.Width(200), GUILayout.Height(100)))
            {
                value = BlankGetChannel.GetChannelName("channel");
            }
        }
    }
```
