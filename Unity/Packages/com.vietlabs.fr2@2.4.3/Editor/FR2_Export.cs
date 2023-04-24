//#define REPLACE_SAME_TYPE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{
    public class FR2_Export
    {
        // private static int processIndex;
        private const int maxThread = 5;

        //[MenuItem("Assets/FR2/Tools/Merge Duplicates")]

        private static Dictionary<string, ProcessReplaceData> listReplace;
        private static HashSet<string> cacheSelection;


        private static List<Thread> lstThreads;
        public static bool IsMergeProcessing { get; private set; }


        public static void ExportCSV(FR2_Ref[] csvSource)
        {
            var result = FR2_CSV.GetCSVRows(csvSource);
            if (result.Length > 0)
            {
                EditorGUIUtility.systemCopyBuffer = result;
                Debug.Log("[FR2] CSV file content (" + csvSource.Length + " assets) copied to clipboard!");
            }
            else
            {
                Debug.LogWarning("[FR2] Nothing to export!");
            }
        }

        [MenuItem("Assets/FR2/Toggle Ignore", false, 19)]
        private static void Ignore()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            Object[] actives = Selection.objects;
            for (var i = 0; i < actives.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(actives[i]);
                if (path.Equals(FR2_Cache.DEFAULT_CACHE_PATH))
                {
                    continue;
                }

                if (FR2_Setting.IgnoreAsset.Contains(path))
                {
                    FR2_Setting.RemoveIgnore(path);
                }
                else
                {
                    FR2_Setting.AddIgnore(path);
                }
            }
        }

        [MenuItem("Assets/FR2/Copy GUID", false, 20)]
        private static void CopyGUID()
        {
            EditorGUIUtility.systemCopyBuffer = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(Selection.activeObject)
            );
        }

        [MenuItem("Assets/FR2/Export Selection", false, 21)]
        private static void ExportSelection()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            FR2_Unity.ExportSelection();
        }

        [MenuItem("Assets/FR2/Select Dependencies (assets I use)", false, 22)]
        private static void SelectDependencies_wtme()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            SelectDependencies(false);
        }

        [MenuItem("Assets/FR2/Refresh")]
        public static void ForceRefreshSelection()
        {
            var guids = Selection.assetGUIDs;
            if (!FR2_Cache.isReady) return; // cache not ready!

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                if (guid == FR2_Cache.CachePath)
                {
                    continue;
                }

                if (!FR2_Asset.IsValidGUID(guid))
                {
                    continue;
                }

                if (FR2_Cache.Api.AssetMap.ContainsKey(guid))
                {
                    FR2_Cache.Api.RefreshAsset(guid, true);
#if FR2_DEBUG
				UnityEngine.Debug.Log("Changed : " + guids[i]);
#endif

                    continue;
                }

                FR2_Cache.Api.AddAsset(guid);
            }

            FR2_Cache.Api.Check4Work();
        }

        [MenuItem("Assets/FR2/Select Dependencies included me", false, 23)]
        private static void SelectDependencies_wme()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            SelectDependencies(true);
        }

        //[MenuItem("Assets/FR2/Select")] 
        [MenuItem("Assets/FR2/Select Used (assets used me)", false, 24)]
        private static void SelectUsed_wtme()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            SelectUsed(false);
        }

        [MenuItem("Assets/FR2/Select Used included me", false, 25)]
        private static void SelectUsed_wme()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            SelectUsed(true);
        }

        [MenuItem("Assets/FR2/Export Dependencies", false, 40)]
        private static void ExportDependencies()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            var deps = GetSelectionDependencies();
            if (deps == null) return;

            Selection.objects = deps.ToArray();
            FR2_Unity.ExportSelection();
        }

        [MenuItem("Assets/FR2/Export Assets (no scripts)", false, 41)]
        private static void ExportAsset()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            List<Object> list = GetSelectionDependencies();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] is MonoScript)
                {
                    list.RemoveAt(i);
                }
            }

            //Debug.Log(i + ":" + list[i] + ":" + list[i].GetType());
            Selection.objects = list.ToArray();
            FR2_Unity.ExportSelection();
        }

        [MenuItem("Assets/FR2/Delete Unused Assets With Selections", false, 42)]
        private static void DeleteUnusedAssetsWithSelections()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return;
            }

            FR2_Cache api = FR2_Cache.Api;

            string[] guids = Selection.assetGUIDs;
            for (int i = 0; i < guids.Length; i++)
            {
                FR2_Asset item;
                bool isMatch = api.AssetMap.TryGetValue(guids[i], out item);
                if (isMatch)
                {
                    if (item.UsedByMap.Count <= 0)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                        Debug.Log("删除 ::: " + assetPath);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }
        }

        public static void MergeDuplicate(string guid_file)
        {
            // for (int i = 0; i < Selection.objects.Length; i++)
            // {
            //     Object item = Selection.objects[i];
            //     Debug.Log(item.name);
            // }
            //string guid_file = EditorGUIUtility.systemCopyBuffer;
            long toFileId = 0;
            var string_arr = guid_file.Split('/');
            if (string_arr.Length > 1)
            {
                toFileId = long.Parse(string_arr[1]);
            }
            string guid = string_arr[0];
            // var wat = new System.Diagnostics.Stopwatch();
            // wat.Start();
            //validate clipboard guid

            string gPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(gPath) || !gPath.StartsWith("Assets/"))
            {
                Debug.LogWarning("Invalid guid <" + guid + "> in clipboard, can not replace !");
                return;
            }

            var temp = FR2_Unity.Selection_AssetGUIDs;//cheat refresh selection, DO NOT delete
            HashSet<string> guids_files = FR2_Unity._Selection_AssetGUIDs;

            var realKey = "";
            foreach (var item in guids_files)
            {
                if (item.StartsWith(guid_file, System.StringComparison.Ordinal))
                {
                    realKey = item;
                }
            }
            if (string.IsNullOrEmpty(realKey))
            {
                Debug.LogWarning("Clipboard guid <" + guid +
                    "> not found in Selection, you may not intentionally replace selection assets by clipboard guid");
                //				foreach (var item in guids_files) {
                //					Debug.Log ("item: " + item);
                //				}
                return;
            }
            guids_files.Remove(realKey);
            cacheSelection = new HashSet<string>();
            foreach (var item in cacheSelection)
            {
                cacheSelection.Add(item);
            }
            if (guids_files.Count == 0)
            {
                Debug.LogWarning("No new asset selected to replace, must select all duplications to replace");
                return;
            }


            //check asset type, only replace same type
#if REPLACE_SAME_TYPE
            var type1 = AssetDatabase.GetMainAssetTypeAtPath(gPath);
            var importType1 = AssetImporter.GetAtPath(gPath);
#endif


            List<FR2_Asset> assetList = new List<FR2_Asset>();
            var lstFind = new List<string>();
            foreach (var item in guids_files)
            {
                var arr = item.Split('/');

                string g = arr[0];

#if REPLACE_SAME_TYPE
                var p2 = AssetDatabase.GUIDToAssetPath(g);
                var type2 = AssetDatabase.GetMainAssetTypeAtPath(p2);

                if(type1 != type2)
                {
                    Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                    continue;
                }
                if(type1 == typeof(UnityEngine.Texture2D))
                {
                    var importType2 = AssetImporter.GetAtPath(p2) as TextureImporter;
                    var textureImportType1 = importType1 as TextureImporter;
                    if (importType2 == null || textureImportType1 == null)
                    {
                        Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                        continue;
                    }
                    if(textureImportType1.textureType != importType2.textureType)
                    {
                        Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                        continue;
                    }
                    if (textureImportType1.textureType == TextureImporterType.Sprite)
                    {
                        if (textureImportType1.spriteImportMode != importType2.spriteImportMode)
                        {
                            Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                            continue;
                        }
                    }
                    //Debug.Log("import type " + mainImportType);
                }
                //Debug.Log("type: " + mainType);
#endif
                lstFind.Add(g);

                //                if (arr.Length > 1)
                //                {
                //                    long file = long.Parse(arr[1]);
                //                    
                //                }
            }
            if (lstFind.Count == 0)
            {
                Debug.LogWarning("No new asset selected to replace, must select all duplications to replace");
                return;
            }

            assetList = FR2_Cache.Api.FindAssets(lstFind.ToArray(), false);

            //replace one by one
            listReplace = new Dictionary<string, ProcessReplaceData>();
            lstThreads = new List<Thread>();
            for (int i = assetList.Count - 1; i >= 0; i--)
            {
                //Debug.Log("FR2 Replace GUID : " + assetList[i].guid + " ---> " + guid + " : " + assetList[i].UsedByMap.Count + " assets updated");
                string fromId = assetList[i].guid;

                List<FR2_Asset> arr = assetList[i].UsedByMap.Values.ToList();
                for (var j = 0; j < arr.Count; j++)
                {
                    FR2_Asset a = arr[j];
                    if (!listReplace.ContainsKey(a.assetPath))
                    {
                        listReplace.Add(a.assetPath, new ProcessReplaceData());
                    }

                    listReplace[a.assetPath].datas.Add(new ReplaceData
                    {
                        from = fromId,
                        to = guid,
                        asset = a,

                        toFileId = toFileId
                    });
                }
            }

            foreach (KeyValuePair<string, ProcessReplaceData> item in listReplace)
            {
                item.Value.processIndex = item.Value.datas.Count - 1;
            }

            IsMergeProcessing = true;
            EditorApplication.update -= ApplicationUpdate;
            EditorApplication.update += ApplicationUpdate;

            // for (var i = assetList.Count - 1; i >= 0; i--)
            // {
            //     // Debug.Log("FR2 Replace GUID : " + assetList[i].guid + " ---> " + guid + " : " + assetList[i].UsedByMap.Count + " assets updated");
            //     var from = assetList[i].guid;

            //     var arr = assetList[i].UsedByMap.Values.ToList();
            //     for (var j = 0; j < arr.Count; j ++)
            //     {
            //         var a = arr[j];
            //         var result = a.ReplaceReference(from, guid);

            //         if (result && !dictAsset.ContainsKey(a.guid))
            //         {
            //             dictAsset.Add(a.guid, 1);
            //         }
            //     }
            // }
            // Debug.Log("Time replace guid " + wat.ElapsedMilliseconds);
            // wat = new System.Diagnostics.Stopwatch();
            // wat.Start();
            // var listRefresh = dictAsset.Keys.ToList();
            // for (var i = 0; i < listRefresh.Count; i++)
            // {
            //     FR2_Cache.Api.RefreshAsset(listRefresh[i], true);
            // }

            // FR2_Cache.Api.RefreshSelection();
            // FR2_Cache.Api.Check4Usage();
            // AssetDatabase.Refresh();
            // Debug.Log("Time replace guid " + wat.ElapsedMilliseconds);
        }

        private static void ApplicationUpdate()
        {
            bool notComplete = listReplace.Where(x => x.Value.processIndex >= 0).Count() > 0;
            if (lstThreads.Count <= 0 && notComplete)
            {
                foreach (KeyValuePair<string, ProcessReplaceData> item in listReplace)
                {
                    if (item.Value.processIndex >= 0)
                    {
                        ReplaceData a = item.Value.datas[item.Value.processIndex--];
                        a.isTerrian = a.asset.type == FR2_AssetType.TERRAIN;
                        if (a.isTerrian)
                        {
                            a.terrainData =
                                AssetDatabase.LoadAssetAtPath(a.asset.assetPath, typeof(Object)) as TerrainData;
                        }

                        a.isSucess = a.asset.ReplaceReference(a.from, a.to, a.toFileId, a.terrainData);
                    }
                }
            }

            for (int i = lstThreads.Count - 1; i >= 0; i--)
            {
                if (!lstThreads[i].IsAlive)
                {
                    lstThreads.RemoveAt(i);
                }
            }

            //if (lstThreads.Count <= 0 && !notComplete) //complete 
            {

                foreach (KeyValuePair<string, ProcessReplaceData> item in listReplace)
                {
                    List<ReplaceData> lst = item.Value.datas;
                    for (var i = 0; i < lst.Count; i++)
                    {
                        ReplaceData data = lst[i];
                        if (!data.isUpdated && data.isSucess)
                        {
                            data.isUpdated = true;
                            if (data.isTerrian)
                            {
                                EditorUtility.SetDirty(data.terrainData);
                                AssetDatabase.SaveAssets();
                                data.terrainData = null;
                                FR2_Unity.UnloadUnusedAssets();
                            }
                            else
                            {
								try 
								{
									AssetDatabase.ImportAsset(data.asset.assetPath, ImportAssetOptions.Default);
								}
								catch (Exception e)
								{
									Debug.LogWarning(data.asset.assetPath + "\n" + e);
								}
								
							}
						}
					}
				}
                var guidsRefreshed = new HashSet<string>();
                EditorApplication.update -= ApplicationUpdate;
                foreach (KeyValuePair<string, ProcessReplaceData> item in listReplace)
                {
                    List<ReplaceData> lst = item.Value.datas;
                    for (var i = 0; i < lst.Count; i++)
                    {
                        ReplaceData data = lst[i];
                        if (data.isSucess && !guidsRefreshed.Contains(data.asset.guid))
                        {
                            guidsRefreshed.Add(data.asset.guid);
                            FR2_Cache.Api.RefreshAsset(data.asset.guid, true);
                        }
                    }
                }

                lstThreads = null;
                listReplace = null;
                FR2_Cache.Api.RefreshSelection();
                FR2_Cache.Api.Check4Work();

                AssetDatabase.Refresh();
                IsMergeProcessing = false;
            }
        }


        //[MenuItem("Assets/FR2/Tools/Fix Model Import Material")]
        //public static void FixModelImportMaterial(){
        //	if (Selection.activeObject == null) return;
        //	CreatePrefabReplaceModel((GameObject)Selection.activeObject);
        //}

        //[MenuItem("GameObject/FR2/Paste Materials", false, 10)]
        //public static void PasteMaterials(){
        //	if (Selection.activeObject == null) return;

        //	var r = Selection.activeGameObject.GetComponent<Renderer>();
        //	Undo.RecordObject(r, "Replace Materials");
        //	r.materials = model_materials;
        //	EditorUtility.SetDirty(r);
        //}

        //[MenuItem("GameObject/FR2/Copy Materials", false, 10)]
        //public static void CopyMaterials(){
        //	if (Selection.activeObject == null) return;
        //	var r = Selection.activeGameObject.GetComponent<Renderer>();
        //	if (r == null) return;
        //	model_materials = r.sharedMaterials;
        //}

        //-------------------------- APIs ----------------------

        private static void SelectDependencies(bool includeMe)
        {
            List<FR2_Asset> list = FR2_Cache.Api.FindAssets(FR2_Unity.Selection_AssetGUIDs, false);
            var dict = new Dictionary<string, Object>();

            if (includeMe)
            {
                AddToDict(dict, list.ToArray());
            }

            for (var i = 0; i < list.Count; i++)
            {
                AddToDict(dict, FR2_Asset.FindUsage(list[i]).ToArray());
            }

            Selection.objects = dict.Values.ToArray();
        }

        private static void SelectUsed(bool includeMe)
        {
            List<FR2_Asset> list = FR2_Cache.Api.FindAssets(FR2_Unity.Selection_AssetGUIDs, false);
            var dict = new Dictionary<string, Object>();

            if (includeMe)
            {
                AddToDict(dict, list.ToArray());
            }

            for (var i = 0; i < list.Count; i++)
            {
                AddToDict(dict, list[i].UsedByMap.Values.ToArray());
            }

            Selection.objects = dict.Values.ToArray();
        }


        //-------------------------- UTILS ---------------------

        internal static void AddToDict(Dictionary<string, Object> dict, params FR2_Asset[] list)
        {
            for (var j = 0; j < list.Length; j++)
            {
                string guid = list[j].guid;
                if (!dict.ContainsKey(guid))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    dict.Add(guid, FR2_Unity.LoadAssetAtPath<Object>(assetPath));
                }
            }
        }

        private static List<Object> GetSelectionDependencies()
        {
            if (!FR2_Cache.isReady)
            {
                Debug.LogWarning("FR2 cache not yet ready, please open Window > FR2_Window and hit scan project!");
                return null;
            }

            return FR2_Cache.FindUsage(FR2_Unity.Selection_AssetGUIDs).Select(
                guid =>
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    return FR2_Unity.LoadAssetAtPath<Object>(assetPath);
                }
            ).ToList();
        }

        private class ProcessReplaceData
        {
            public readonly List<ReplaceData> datas = new List<ReplaceData>();
            public int processIndex;
        }

        private class ReplaceData
        {
            public FR2_Asset asset;
            public string from;
            public bool isSucess;
            public bool isTerrian;
            public bool isUpdated;
            public TerrainData terrainData;
            public string to;

            public long toFileId;
        }

        //	AssetDatabase.ImportAsset(oAssetPath, ImportAssetOptions.Default);
        //	importer.importMaterials = false;
        //	var importer = AssetImporter.GetAtPath(oAssetPath) as ModelImporter;
        //	var nModel = AssetDatabase.LoadAssetAtPath<GameObject>(oAssetPath);

        //	// Reimport model with importMaterial = false
        //	var extension = Path.GetExtension(oAssetPath);

        //	model_materials = model.GetComponent<Renderer>().sharedMaterials;
        //	var oGUID = AssetDatabase.AssetPathToGUID(oAssetPath);

        //	var oAssetPath = AssetDatabase.GetAssetPath(model);
        //	if (model == null) return;
        //{
        //static void CreatePrefabReplaceModel(GameObject model)

        //static Material[] model_materials;

        //	//create prefab from new model
        //	var prefabPath = oAssetPath.Replace(extension, ".prefab");
        //	var clone = (GameObject)Object.Instantiate(nModel);
        //	clone.GetComponent<Renderer>().sharedMaterials = model_materials;
        //	PrefabUtility.CreatePrefab(prefabPath, clone, ReplacePrefabOptions.ReplaceNameBased);
        //	AssetDatabase.SaveAssets();
        //	GameObject.DestroyImmediate(clone);
        //}
    }
}