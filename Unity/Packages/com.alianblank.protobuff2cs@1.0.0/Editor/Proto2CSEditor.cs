using UnityEditor;

namespace Proto2CS.Editor
{
    internal static class Proto2CSEditor
    {
        [MenuItem("Tools/Proto2CS &C")]
        internal static void AllProto2Cs()
        {
            UtilityProto2MessagePackCs.Proto2Cs("./../Protobuf", "./Assets/Hotfix.Proto/Proto", "Hotfix.Proto.Proto");
            UtilityProto2MessagePackCs.Proto2Cs("./../Protobuf", "./../Server/Server.Proto/Proto", "Server.Proto.Proto", true);
            UnityEngine.Debug.Log("导出协议完成");
            AssetDatabase.Refresh();
        }
    }
}