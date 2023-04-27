using UnityEditor;

namespace ExcelExporter.Editor
{
    internal static class ExcelExporterEditor
    {
        [MenuItem("Tools/ExcelExporter &E")]
        internal static void AllProto2Cs()
        {
            ExcelExporter.Export();
            UnityEngine.Debug.Log("导出配置完成");
            AssetDatabase.Refresh();
        }
    }
}