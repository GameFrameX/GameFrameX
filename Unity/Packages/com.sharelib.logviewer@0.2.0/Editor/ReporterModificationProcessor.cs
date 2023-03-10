using System.IO;
using UnityEditor;
using UnityEngine;

public class ReporterModificationProcessor : UnityEditor.AssetModificationProcessor
{
    [InitializeOnLoad]
    public class BuildInfo
    {
        static BuildInfo()
        {
            EditorApplication.update += Update;
        }

        static bool _isCompiling = true;

        static void Update()
        {
            if (!EditorApplication.isCompiling && _isCompiling)
            {
                //Debug.Log("Finish Compile");
                if (!Directory.Exists(Application.dataPath + "/StreamingAssets"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");
                }

                string infoPath = Application.dataPath + "/StreamingAssets/build_info";
                string buildInfo = $"Build from {SystemInfo.deviceName} at {System.DateTime.Now}";
                File.WriteAllText(infoPath, buildInfo);
            }

            _isCompiling = EditorApplication.isCompiling;
        }
    }
}