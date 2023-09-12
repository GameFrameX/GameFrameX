using UnityEditor;

namespace Proto2CS.Editor
{
    internal static class Proto2CSEditor
    {
        private static readonly IProtoGenerateHelper ProtoGenerateHelper;

        static Proto2CSEditor()
        {
            ProtoGenerateHelper = new MessagePackHelper();
        }

        [MenuItem("Tools/Proto2CS &C")]
        internal static void AllProto2Cs()
        {
            ProtoGenerateHelper.Run("./../Protobuf", "./Assets/Hotfix.Proto/Proto", "Hotfix.Proto.Proto");
            ProtoGenerateHelper.Run("./../Protobuf", "./../Server/Server.Proto/Proto", "Server.Proto.Proto", true);
            UnityEngine.Debug.Log("导出协议完成");
            AssetDatabase.Refresh();
        }
    }
}