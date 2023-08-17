using UnityEditor;

public class InspectorLockShortcut : EditorWindow
{
    [MenuItem("Window/Toggle Inspector Lock %l")]
    private static void ToggleInspectorLock()
    {
        // 获取Inspector窗口
        var inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        var inspectorWindow = EditorWindow.GetWindow(inspectorType);
        // 使用反射调用isLocked属性
        var isLockedProperty = inspectorType.GetProperty("isLocked");
        var isLocked = (bool) isLockedProperty.GetValue(inspectorWindow);
        isLockedProperty.SetValue(inspectorWindow, !isLocked);
        // PrefabUtility.CreatePrefab( "Assets/Editor/ToggleInspectorLock.prefab", inspectorWindow);
    }
}