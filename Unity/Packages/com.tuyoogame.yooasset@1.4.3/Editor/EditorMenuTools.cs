using UnityEditor;

namespace YooAsset.Editor
{
    static class EditorMenuTools
    {
        [MenuItem("YooAsset/Clear SandBox", false, 401)]
        static void DeleteSandBoxPath()
        {
            EditorTools.ClearFolder(PathHelper.GetPersistentRootPath());
            YooLogger.Log("Clear SandBox Over");
        }
    }
}