using UnityEditor;
using UnityEngine;
using vietlabs.fr2;

[CustomEditor(typeof(FR2_Cache))]
internal class FR2_CacheEditor : Editor
{
    private static string inspectGUID;
    private static int index;

    public override void OnInspectorGUI()
    {
        var c = (FR2_Cache)target;

        GUILayout.Label("Total : " + c.AssetList.Count);
        FR2_Cache.DrawPriorityGUI();

        Object s = Selection.activeObject;
        if (s == null)
        {
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(s));

        if (inspectGUID != guid)
        {
            inspectGUID = guid;
            index = c.AssetList.FindIndex(item => item.guid == guid);
        }

        if (index != -1)
        {
            if (index >= c.AssetList.Count)
            {
                index = 0;
            }

            serializedObject.Update();
            SerializedProperty prop = serializedObject.FindProperty("AssetList").GetArrayElementAtIndex(index);
            EditorGUILayout.PropertyField(prop, true);
        }
    }
}