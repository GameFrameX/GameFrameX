using UnityEditor;

namespace Proto2CS.Editor
{
    internal static class Proto2CSEditor
    {
        [MenuItem("Tools/Proto2CS &E")]
        internal static void AllProto2Cs()
        {
            UtilityProto2Cs.Proto2Cs("./../Protobuf", "./Assets/Hotfix/Network/Message/Proto", "Hotfix.Message.Proto");
            UnityEngine.Debug.Log("导出协议完成");
            AssetDatabase.Refresh();
        }
    }
}