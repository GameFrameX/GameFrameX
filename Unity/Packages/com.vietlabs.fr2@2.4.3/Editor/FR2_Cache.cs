//#define FR2_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    [InitializeOnLoad]
    public class FR2_CacheHelper : AssetPostprocessor
    {
        private static HashSet<string> scenes;
        private static HashSet<string> guidsIgnore;

        static FR2_CacheHelper()
        {
            EditorApplication.update -= InitHelper;
            EditorApplication.update += InitHelper;
        }

        private static void InitHelper()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            // if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!FR2_Cache.isReady)
            {
                return;
            }

            if (!FR2_Cache.Api.disabled)
            {
                InitListScene();
                InitIgnore();

#if UNITY_2018_1_OR_NEWER
                EditorBuildSettings.sceneListChanged -= InitListScene;
                EditorBuildSettings.sceneListChanged += InitListScene;
#endif

                EditorApplication.projectWindowItemOnGUI -= OnGUIProjectItem;
                EditorApplication.projectWindowItemOnGUI += OnGUIProjectItem;

                FR2_Cache.onReady -= OnCacheReady;
                FR2_Cache.onReady += OnCacheReady;
            }

            EditorApplication.update -= InitHelper;
        }

        // private class AssetModificationHelper: UnityEditor.AssetModificationProcessor
        // {
        //     static void OnWillCreateAsset(string assetName)
        //     {
        //         FR2_Cache.Api.makeDirty();
        //     }
        //     static AssetDeleteResult OnWillDeleteAsset(string name,RemoveAssetOptions options)
        //     {
        //         FR2_Cache.Api.makeDirty();
        //         return AssetDeleteResult.DidDelete;
        //     }
        //     private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        //     {
        //         FR2_Cache.Api.makeDirty();
        //         AssetMoveResult assetMoveResult = AssetMoveResult.DidMove;

        //         // Perform operations on the asset and set the value of 'assetMoveResult' accordingly.

        //         return assetMoveResult;
        //     }
        //     static string[] OnWillSaveAssets(string[] paths)
        //     {
        //         FR2_Cache.Api.makeDirty();
        //         return paths;
        //     }
        // }

        private static void OnCacheReady()
        {
            InitIgnore();
            // force repaint all project panels
            EditorApplication.RepaintProjectWindow();
        }

        public static void InitIgnore()
        {
            guidsIgnore = new HashSet<string>();
            foreach (string item in FR2_Setting.IgnoreAsset)
            {
                string guid = AssetDatabase.AssetPathToGUID(item);
                if (guidsIgnore.Contains(guid))
                {
                    continue;
                }

                guidsIgnore.Add(guid);
            }
        }

        private static void InitListScene()
        {
            scenes = new HashSet<string>();
            // string[] scenes = new string[sceneCount];
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                string sce = AssetDatabase.AssetPathToGUID(scene.path);
                // Debug.Log(scene.path + " " + sce);
                if (scenes.Contains(sce))
                {
                    continue;
                }

                scenes.Add(sce);
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {

            FR2_Cache.DelayCheck4Changes();
            //Debug.Log("OnPostProcessAllAssets : " + ":" + importedAssets.Length + ":" + deletedAssets.Length + ":" + movedAssets.Length + ":" + movedFromAssetPaths.Length);

            if (!FR2_Cache.isReady)
            {
#if FR2_DEBUG
			Debug.Log("Not ready, will refresh anyway !");
#endif
                return;
            }

            // FR2 not yet ready
            if (FR2_Cache.Api.AssetMap == null) return;

            for (var i = 0; i < importedAssets.Length; i++)
            {
                if (importedAssets[i] == FR2_Cache.CachePath)
                {
                    continue;
                }

                string guid = AssetDatabase.AssetPathToGUID(importedAssets[i]);
                if (!FR2_Asset.IsValidGUID(guid))
                {
                    continue;
                }

                if (FR2_Cache.Api.AssetMap.ContainsKey(guid))
                {
                    FR2_Cache.Api.RefreshAsset(guid, true);

#if FR2_DEBUG
				Debug.Log("Changed : " + importedAssets[i]);
#endif

                    continue;
                }

                FR2_Cache.Api.AddAsset(guid);
#if FR2_DEBUG
			Debug.Log("New : " + importedAssets[i]);
#endif
            }

            for (var i = 0; i < deletedAssets.Length; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(deletedAssets[i]);
                FR2_Cache.Api.RemoveAsset(guid);

#if FR2_DEBUG
			Debug.Log("Deleted : " + deletedAssets[i]);
#endif
            }

            for (var i = 0; i < movedAssets.Length; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(movedAssets[i]);
                FR2_Asset asset = FR2_Cache.Api.Get(guid);
                if (asset != null)
                {
                    asset.MarkAsDirty(true, false);
                }
            }

#if FR2_DEBUG
		Debug.Log("Changes :: " + importedAssets.Length + ":" + FR2_Cache.Api.workCount);
#endif

            FR2_Cache.Api.Check4Work();
        }

        private static void OnGUIProjectItem(string guid, Rect rect)
        {
            var r = new Rect(rect.x, rect.y, 1f, 16f);
            if (scenes.Contains(guid))
            {
                EditorGUI.DrawRect(r, GUI2.Theme(new Color32(72, 150, 191, 255), Color.blue));
            }
            else if (guidsIgnore.Contains(guid))
            {
                var ignoreRect = new Rect(rect.x + 3f, rect.y + 6f, 2f, 2f);
                EditorGUI.DrawRect(ignoreRect, GUI2.darkRed);
            }

            if (!FR2_Cache.isReady)
            {
                return; // not ready
            }

            if (!FR2_Setting.ShowReferenceCount)
            {
                return;
            }

            FR2_Cache api = FR2_Cache.Api;
            if (FR2_Cache.Api.AssetMap == null)
            {
                FR2_Cache.Api.Check4Changes(false);
            }

            FR2_Asset item;

            if (!api.AssetMap.TryGetValue(guid, out item))
            {
                return;
            }

            if (item == null || item.UsedByMap == null)
            {
                return;
            }

            if (item.UsedByMap.Count > 0)
            {
                var content = new GUIContent(item.UsedByMap.Count.ToString());
                r.width = 0f;
                r.xMin -= 100f;
                GUI.Label(r, content, GUI2.miniLabelAlignRight);
            }
        }
    }

    [Serializable]
    public class FR2_Setting
    {
        private static FR2_Setting d;

        public bool alternateColor = true;
        public int excludeTypes; //32-bit type Mask
        public FR2_RefDrawer.Mode groupMode;
        public List<string> listIgnore = new List<string>();
        public bool pingRow = true;
        public bool referenceCount = true;

        public bool showFileSize;
        public bool displayFileSize = true;
        public bool displayAtlasName = false;
        public bool displayAssetBundleName = false;

        public bool showUsedByClassed = true;
        public FR2_RefDrawer.Sort sortMode;

        public int treeIndent = 10;


        public Color32 rowColor = new Color32(0, 0, 0, 12);
        public Color32 ScanColor = new Color32(0, 204, 102, 255);
        public Color SelectedColor = new Color(0, 0f, 1f, 0.25f);

        [NonSerialized] private static HashSet<string> _hashIgnore;
        //		private static Dictionary<string, List<string>> _IgnoreFiltered;
        public static Action OnIgnoreChange;


        //public bool scanScripts		= false;



        /*
		Doesn't have a settings option - I will include one in next update
		
		2. Hide the reference number - Should be in the setting above so will be coming next
		3. Cache file path should be configurable - coming next in the setting
		4. Disable / Selectable color in alternative rows - coming next in the setting panel
		5. Applied filters aren't saved - Should be fixed in next update too
		6. Hide Selection part - should be com as an option so you can quickly toggle it on or off
		7. Click whole line to ping - coming next by default and can adjustable in the setting panel
		
		*/

        internal static FR2_Setting s
        {
            get { return FR2_Cache.Api ? FR2_Cache.Api.setting : d ?? (d = new FR2_Setting()); }
        }

        public static bool ShowUsedByClassed
        {
            get { return s.showUsedByClassed; }
        }

        public static bool ShowFileSize
        {
            get { return s.showFileSize; }
        }

        public static int TreeIndent
        {
            get { return s.treeIndent; }
            set
            {
                if (s.treeIndent == value)
                {
                    return;
                }

                s.treeIndent = value;
                setDirty();
            }
        }

        public static bool ShowReferenceCount
        {
            get { return s.referenceCount; }
            set
            {
                if (s.referenceCount == value)
                {
                    return;
                }

                s.referenceCount = value;
                setDirty();
            }
        }
        public static bool AlternateRowColor
        {
            get { return s.alternateColor; }
            set
            {
                if (s.alternateColor == value)
                {
                    return;
                }

                s.alternateColor = value;
                setDirty();
            }
        }

        public static Color32 RowColor
        {
            get { return s.rowColor; }
            set
            {
                if (s.rowColor.Equals(value))
                {
                    return;
                }

                s.rowColor = value;
                setDirty();
            }
        }

        public static bool PingRow
        {
            get { return s.pingRow; }
            set
            {
                if (s.pingRow == value)
                {
                    return;
                }

                s.pingRow = value;
                setDirty();
            }
        }

        public static HashSet<string> IgnoreAsset
        {
            get
            {
                if (_hashIgnore == null)
                {
                    _hashIgnore = new HashSet<string>();
                    if (s == null || s.listIgnore == null)
                    {
                        return _hashIgnore;
                    }

                    for (var i = 0; i < s.listIgnore.Count; i++)
                    {
                        if (_hashIgnore.Contains(s.listIgnore[i]))
                        {
                            continue;
                        }

                        _hashIgnore.Add(s.listIgnore[i]);
                    }
                }

                return _hashIgnore;
            }
        }

        //		public static Dictionary<string, List<string>> IgnoreFiltered
        //		{
        //			get
        //			{
        //				if (_IgnoreFiltered == null)
        //				{
        //					initIgnoreFiltered();
        //				}
        //
        //				return _IgnoreFiltered;
        //			}
        //		}

        //static public bool ScanScripts
        //{
        //	get  { return s.scanScripts; }
        //	set  {
        //		if (s.scanScripts == value) return;
        //		s.scanScripts = value; setDirty();
        //	}
        //}

        public static FR2_RefDrawer.Mode GroupMode
        {
            get { return s.groupMode; }
            set
            {
                if (s.groupMode.Equals(value))
                {
                    return;
                }

                s.groupMode = value;
                setDirty();
            }
        }

        public static FR2_RefDrawer.Sort SortMode
        {
            get { return s.sortMode; }
            set
            {
                if (s.sortMode.Equals(value))
                {
                    return;
                }

                s.sortMode = value;
                setDirty();
            }
        }

        public static bool HasTypeExcluded
        {
            get { return s.excludeTypes != 0; }
        }

        private static void setDirty()
        {
            if (FR2_Cache.Api != null)
            {
                EditorUtility.SetDirty(FR2_Cache.Api);
            }
        }

        //		private static void initIgnoreFiltered()
        //		{
        //			FR2_Asset.ignoreTS = Time.realtimeSinceStartup;
        //
        //			_IgnoreFiltered = new Dictionary<string, List<string>>();
        //			var lst = new List<string>(s.listIgnore);
        //			lst = lst.OrderBy(x => x.Length).ToList();
        //			int count = lst.Count;
        //			for (var i = 0; i < count; i++)
        //			{
        //				string str = lst[i];
        //				_IgnoreFiltered.Add(str, new List<string> {str});
        //				for (int j = count - 1; j > i; j--)
        //				{
        //					if (lst[j].StartsWith(str))
        //					{
        //						_IgnoreFiltered[str].Add(lst[j]);
        //						lst.RemoveAt(j);
        //						count--;
        //					}
        //				}
        //			}
        //		}

        public static void AddIgnore(string path)
        {
            if (string.IsNullOrEmpty(path) || IgnoreAsset.Contains(path) || path == "Assets")
            {
                return;
            }

            s.listIgnore.Add(path);
            _hashIgnore.Add(path);
            AssetType.SetDirtyIgnore();
            FR2_CacheHelper.InitIgnore();
            //initIgnoreFiltered();

            FR2_Asset.ignoreTS = Time.realtimeSinceStartup;
            if (OnIgnoreChange != null)
            {
                OnIgnoreChange();
            }
        }


        public static void RemoveIgnore(string path)
        {
            if (!IgnoreAsset.Contains(path))
            {
                return;
            }

            _hashIgnore.Remove(path);
            s.listIgnore.Remove(path);
            AssetType.SetDirtyIgnore();
            FR2_CacheHelper.InitIgnore();
            //initIgnoreFiltered();

            FR2_Asset.ignoreTS = Time.realtimeSinceStartup;
            if (OnIgnoreChange != null)
            {
                OnIgnoreChange();
            }
        }

        public static bool IsTypeExcluded(int type)
        {
            return (s.excludeTypes >> type & 1) != 0;
        }

        public static void ToggleTypeExclude(int type)
        {
            bool v = (s.excludeTypes >> type & 1) != 0;
            if (v)
            {
                s.excludeTypes &= ~(1 << type);
            }
            else
            {
                s.excludeTypes |= 1 << type;
            }

            setDirty();
        }

        public static int GetExcludeType()
        {
            return s.excludeTypes;
        }

        public static bool IsIncludeAllType()
        {
            // Debug.Log ((AssetType.FILTERS.Length & s.excludeTypes) + "  " + Mathf.Pow(2, AssetType.FILTERS.Length) ); 
            return s.excludeTypes == 0 || Mathf.Abs(s.excludeTypes) == Mathf.Pow(2, AssetType.FILTERS.Length);
        }

        public static void ExcludeAllType()
        {
            s.excludeTypes = -1;
        }

        public static void IncludeAllType()
        {
            s.excludeTypes = 0;
        }

        public void DrawSettings()
        {
            if (FR2_Unity.DrawToggle(ref pingRow, "Full Row click to Ping"))
            {
                setDirty();
            }

            GUILayout.BeginHorizontal();
            {
                if (FR2_Unity.DrawToggle(ref alternateColor, "Alternate Odd & Even Row Color"))
                {
                    setDirty();
                    FR2_Unity.RepaintFR2Windows();
                }

                EditorGUI.BeginDisabledGroup(!alternateColor);
                {
                    Color c = EditorGUILayout.ColorField(rowColor);
                    if (!c.Equals(rowColor))
                    {
                        rowColor = c;
                        setDirty();
                        FR2_Unity.RepaintFR2Windows();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            if (FR2_Unity.DrawToggle(ref referenceCount, "Show Usage Count in Project panel"))
            {
                setDirty();
                FR2_Unity.RepaintProjectWindows();
            }

            if (FR2_Unity.DrawToggle(ref showUsedByClassed, "Show Asset Type in use"))
            {
                setDirty();
                FR2_Unity.RepaintFR2Windows();
            }

            GUILayout.BeginHorizontal();
            {
                Color c = EditorGUILayout.ColorField("Duplicate Scan Color", ScanColor);
                if (!c.Equals(ScanColor))
                {
                    ScanColor = c;
                    setDirty();
                    FR2_Unity.RepaintFR2Windows();
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    public class FR2_Cache : ScriptableObject
    {
        internal const string CACHE_VERSION = "2.1";
        internal const string DEFAULT_CACHE_PATH = "Assets/FR2_Cache.asset";

        internal static int cacheStamp;
        internal static Action onReady;

        internal static bool _triedToLoadCache;
        internal static FR2_Cache _cache;

        internal static string _cacheGUID;
        internal static string _cachePath;
        public static int priority = 10;

        [SerializeField] private bool _autoRefresh;
        [SerializeField] private string _curCacheVersion;

        [SerializeField] private bool _disabled;
        [SerializeField] public List<FR2_Asset> AssetList;


        private int frameSkipped;
        [NonSerialized] internal Dictionary<string, FR2_Asset> AssetMap;
        [NonSerialized] internal List<FR2_Asset> queueLoadContent;


        internal bool ready;
        [SerializeField] internal FR2_Setting setting = new FR2_Setting();

        // ----------------------------------- INSTANCE -------------------------------------

        [SerializeField] public int timeStamp;
        [NonSerialized] internal int workCount;


        public static void DrawPriorityGUI()
        {
            float w = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 120f;
            FR2_Cache.priority = EditorGUILayout.IntSlider("  Scan Priority", FR2_Cache.priority, 0, 50);
            EditorGUIUtility.labelWidth = w;
        }

        internal static string CacheGUID
        {
            get
            {
                if (!string.IsNullOrEmpty(_cacheGUID))
                {
                    return _cacheGUID;
                }

                if (_cache != null)
                {
                    _cachePath = AssetDatabase.GetAssetPath(_cache);
                    _cacheGUID = AssetDatabase.AssetPathToGUID(_cachePath);
                    return _cacheGUID;
                }

                return null;
            }
        }

        internal static string CachePath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cachePath))
                {
                    return _cachePath;
                }

                if (_cache != null)
                {
                    _cachePath = AssetDatabase.GetAssetPath(_cache);
                    return _cachePath;
                }

                return null;
            }
        }

        public bool Dirty { get; private set; }

        internal static FR2_Cache Api
        {
            get
            {
                if (_cache != null)
                {
                    return _cache;
                }

                if (!_triedToLoadCache)
                {
                    TryLoadCache();
                }

                return _cache;
            }
        }

        internal bool disabled
        {
            get { return _disabled; }
            set
            {
                if (_disabled == value)
                {
                    return;
                }

                _disabled = value;

                if (_disabled)
                {
                    //Debug.LogWarning("FR2 is disabled - Stopping all works !");	
                    ready = false;
                    EditorApplication.update -= AsyncProcess;
                }
                else
                {
                    FR2_Cache.Api.Check4Changes(false);
                }
            }
        }

        internal static bool isReady
        {
            get
            {
                if (!_triedToLoadCache)
                {
                    TryLoadCache();
                }

                return _cache != null && _cache.ready;
            }
        }

        internal static bool hasCache
        {
            get
            {
                if (!_triedToLoadCache)
                {
                    TryLoadCache();
                }

                return _cache != null;
            }
        }

        internal float progress
        {
            get
            {
                int n = workCount - queueLoadContent.Count;
                return workCount == 0 ? 1 : n / (float)workCount;
            }
        }

        public static bool CheckSameVersion()
        {
            // Debug.Log((_cache == null) + " " + _cache._curCacheVersion );
            if (_cache == null)
            {
                return false;
            }

            return _cache._curCacheVersion == CACHE_VERSION;
        }

        public void makeDirty()
        {
            Dirty = true;
        }

        private static void FoundCache(bool savePrefs, bool writeFile)
        {
            //Debug.LogWarning("Found Cache!");

            _cachePath = AssetDatabase.GetAssetPath(_cache);
            _cache.ReadFromCache();
            _cache.Check4Changes(false);
            _cacheGUID = AssetDatabase.AssetPathToGUID(_cachePath);

            if (savePrefs)
            {
                EditorPrefs.SetString("fr2_cache.guid", _cacheGUID);
            }

            if (writeFile)
            {
                File.WriteAllText("Library/fr2_cache.guid", _cacheGUID);
            }
        }

        private static bool RestoreCacheFromGUID(string guid, bool savePrefs, bool writeFile)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return RestoreCacheFromPath(path, savePrefs, writeFile);
        }

        private static bool RestoreCacheFromPath(string path, bool savePrefs, bool writeFile)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            _cache = FR2_Unity.LoadAssetAtPath<FR2_Cache>(path);
            if (_cache != null)
            {
                FoundCache(savePrefs, writeFile);
            }

            return _cache != null;
        }

        private static void TryLoadCache()
        {
            _triedToLoadCache = true;

            if (RestoreCacheFromPath(DEFAULT_CACHE_PATH, false, false))
            {
                return;
            }

            // Check EditorPrefs
            string pref = EditorPrefs.GetString("fr2_cache.guid", string.Empty);
            if (RestoreCacheFromGUID(pref, false, false))
            {
                return;
            }

            // Read GUID from File
            if (File.Exists("Library/fr2_cache.guid"))
            {
                if (RestoreCacheFromGUID(File.ReadAllText("Library/fr2_cache.guid"), true, false))
                {
                    return;
                }
            }

            // Search whole project
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            for (var i = 0; i < allAssets.Length; i++)
            {
                if (allAssets[i].EndsWith("/FR2_Cache.asset", StringComparison.Ordinal))
                {
                    RestoreCacheFromPath(allAssets[i], true, true);
                    break;
                }
            }
        }

        internal static void DeleteCache()
        {
            if (_cache == null)
            {
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_cachePath))
                {
                    AssetDatabase.DeleteAsset(_cachePath);
                }
            }
            catch { }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        internal static void CreateCache()
        {
            _cache = CreateInstance<FR2_Cache>();
            _cache._curCacheVersion = CACHE_VERSION;
            string path = Application.dataPath + DEFAULT_CACHE_PATH
                              .Substring(0, DEFAULT_CACHE_PATH.LastIndexOf('/') + 1).Replace("Assets", string.Empty);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            AssetDatabase.CreateAsset(_cache, DEFAULT_CACHE_PATH);
            EditorUtility.SetDirty(_cache);

            FoundCache(true, true);
            DelayCheck4Changes();
        }

        internal static List<string> FindUsage(string[] listGUIDs)
        {
            if (!isReady)
            {
                return null;
            }

            List<FR2_Asset> refs = Api.FindAssets(listGUIDs, true);

            for (var i = 0; i < refs.Count; i++)
            {
                List<FR2_Asset> tmp = FR2_Asset.FindUsage(refs[i]);

                for (var j = 0; j < tmp.Count; j++)
                {
                    FR2_Asset itm = tmp[j];
                    if (refs.Contains(itm))
                    {
                        continue;
                    }

                    refs.Add(itm);
                }
            }

            return refs.Select(item => item.guid).ToList();
        }

        private void OnEnable()
        {
#if FR2_DEBUG
		Debug.Log("OnEnabled : " + _cache);
#endif
            if (_cache == null)
            {
                _cache = this;
            }

            Check4Changes(false);
        }

        internal void ReadFromCache()
        {
            if (AssetList == null) AssetList = new List<FR2_Asset>();

            FR2_Unity.Clear(ref queueLoadContent);
            FR2_Unity.Clear(ref AssetMap);

            for (var i = 0; i < AssetList.Count; i++)
            {
                var item = AssetList[i];
                item.state = FR2_AssetState.CACHE;

                var path = AssetDatabase.GUIDToAssetPath(item.guid);
                if (string.IsNullOrEmpty(path))
                {
                    item.type = FR2_AssetType.UNKNOWN; // to make sure if GUIDs being reused for a different kind of asset
                    item.state = FR2_AssetState.MISSING;
                    AssetMap.Add(item.guid, item);
                    continue;
                }

                if (AssetMap.ContainsKey(item.guid))
                {
#if FR2_DEBUG
					Debug.LogWarning("Something wrong, cache found twice <" + item.guid + ">");
#endif
                    continue;
                }

                AssetMap.Add(item.guid, item);
            }
        }

        internal void ReadFromProject(bool force)
        {
            if (AssetMap == null || AssetMap.Count == 0) ReadFromCache();

            var paths = AssetDatabase.GetAllAssetPaths();
            cacheStamp++;
            workCount = 0;
            if (queueLoadContent != null) queueLoadContent.Clear();

            // Check for new assets
            foreach (var p in paths)
            {
                var isValid = FR2_Unity.StringStartsWith(p, "Assets/", "Packages/", "Library/", "ProjectSettings/");

                if (!isValid)
                {
#if FR2_DEBUG
					Debug.LogWarning("Ignore asset: " + p);
#endif
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(p);
                if (!FR2_Asset.IsValidGUID(guid))
                {
                    continue;
                }

                FR2_Asset asset;
                if (!AssetMap.TryGetValue(guid, out asset))
                {
                    AddAsset(guid);
                }
                else
                {
                    asset.refreshStamp = cacheStamp; // mark this asset so it won't be deleted
                    if (!asset.isDirty && !force) continue;

                    if (force) asset.MarkAsDirty(true, true);

                    workCount++;
                    queueLoadContent.Add(asset);
                }
            }

            // Check for deleted assets
            for (var i = AssetList.Count - 1; i >= 0; i--)
            {
                if (AssetList[i].refreshStamp != cacheStamp)
                {
                    RemoveAsset(AssetList[i]);
                }
            }
        }

        internal static void DelayCheck4Changes()
        {
            EditorApplication.update -= Check;
            EditorApplication.update += Check;
        }

        static void Check()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            if (Api == null) return;

            EditorApplication.update -= Check;
            Api.Check4Changes(false);
        }

        internal void Check4Changes(bool force)
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                DelayCheck4Changes();
                return;
            }

            ready = false;
            ReadFromProject(force);

#if FR2_DEBUG
		Debug.Log("After checking :: WorkCount :: " + workCount + ":" + AssetMap.Count + ":" + AssetList.Count);
#endif
            Check4Work();
        }

        internal void RefreshAsset(string guid, bool force)
        {
            FR2_Asset asset;

            if (!AssetMap.TryGetValue(guid, out asset))
            {
                return;
            }

            RefreshAsset(asset, force);
        }

        internal void RefreshSelection()
        {
            string[] list = FR2_Unity.Selection_AssetGUIDs;
            for (var i = 0; i < list.Length; i++)
            {
                RefreshAsset(list[i], true);
            }

            Check4Work();
        }

        internal void RefreshAsset(FR2_Asset asset, bool force)
        {
            asset.MarkAsDirty(true, force);
            DelayCheck4Changes();

            //#if FR2_DEBUG
            //		    Debug.Log("RefreshAsset: " + asset.guid + ":" + workCount);
            //#endif
            //			
            //			workCount++;
            //
            //			if (force)
            //			{
            //				asset.MarkAsDirty(true, true);
            //				
            //				if (asset.type == FR2_AssetType.FOLDER && !asset.IsMissing)
            //				{
            //					string[] dirs = Directory.GetDirectories(asset.assetPath, "*", SearchOption.AllDirectories);
            //					//refresh children directories as well
            //
            //					for (var i = 0; i < dirs.Length; i++)
            //					{
            //						string guid = AssetDatabase.AssetPathToGUID(dirs[i]);
            //						FR2_Asset child = Api.Get(guid);
            //						if (child == null)
            //						{
            //							continue;
            //						}
            //
            //						workCount++;
            //						child.MarkAsDirty();
            //						queueLoadContent.Add(child);
            //					}
            //				}
            //			}
            //			
            //			queueLoadContent.Add(asset);
        }

        internal void AddAsset(string guid)
        {
            if (AssetMap.ContainsKey(guid))
            {
                Debug.LogWarning("guid already exist <" + guid + ">");
                return;
            }

            var asset = new FR2_Asset(guid);
            asset.LoadPathInfo();
            asset.refreshStamp = cacheStamp;

            AssetList.Add(asset);
            AssetMap.Add(guid, asset);
            //Debug.LogWarning("Add - AssetList: " + AssetList.Count);

            // Do not load content for FR2_Cache asset
            if (guid == CacheGUID)
            {
                return;
            }

            workCount++;
            queueLoadContent.Add(asset);
        }

        internal void RemoveAsset(string guid)
        {
            if (!AssetMap.ContainsKey(guid))
            {
                return;
            }

            RemoveAsset(AssetMap[guid]);
        }

        internal void RemoveAsset(FR2_Asset asset)
        {
            AssetList.Remove(asset);

            // Deleted Asset : still in the map but not in the AssetList
            asset.state = FR2_AssetState.MISSING;
        }

        internal void Check4Usage()
        {
#if FR2_DEBUG
			Debug.Log("Check 4 Usage");
#endif

            foreach (var item in AssetList)
            {
                if (item.IsMissing) continue;
                FR2_Unity.Clear(ref item.UsedByMap);
            }

            foreach (var item in AssetList)
            {
                if (item.IsMissing) continue;
                AsyncUsedBy(item);
            }

            workCount = 0;
            ready = true;
        }

        internal void Check4Work()
        {
            if (disabled) return;

            if (workCount == 0)
            {
                Check4Usage();
                return;
            }

            ready = false;
            EditorApplication.update -= AsyncProcess;
            EditorApplication.update += AsyncProcess;
        }

        internal void AsyncProcess()
        {
            if (this == null)
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                return;
            }

            if (frameSkipped++ < 10 - 2 * priority)
            {
                return;
            }

            frameSkipped = 0;
            float t = Time.realtimeSinceStartup;

#if FR2_DEBUG
			Debug.Log(Mathf.Round(t) + " : " + progress*workCount + "/" + workCount + ":" + isReady + " ::: " + queueLoadContent.Count);
#endif

            if (!AsyncWork(queueLoadContent, AsyncLoadContent, t))
            {
                return;
            }

            EditorApplication.update -= AsyncProcess;
            EditorUtility.SetDirty(this);

            Check4Usage();
        }

        internal bool AsyncWork<T>(List<T> arr, Action<int, T> action, float t)
        {
            float FRAME_DURATION = 1 / 1000f * (priority * 5 + 1); //prevent zero

            int c = arr.Count;
            var counter = 0;

            while (c-- > 0)
            {
                T last = arr[c];
                arr.RemoveAt(c);
                action(c, last);
                //workCount--;

                float dt = Time.realtimeSinceStartup - t - FRAME_DURATION;
                if (dt >= 0)
                {
                    return false;
                }

                counter++;
            }

            return true;
        }

        internal void AsyncLoadContent(int idx, FR2_Asset asset)
        {
            //Debug.Log("Async: " + idx);
            if (asset.fileInfoDirty) asset.LoadFileInfo();
            if (asset.fileContentDirty) asset.LoadContent();
        }

        internal void AsyncUsedBy(FR2_Asset asset)
        {
            if (AssetMap == null)
            {
                Check4Changes(false);
            }

            if (asset.IsFolder)
            {
                return;
            }

#if FR2_DEBUG
			Debug.Log("Async UsedBy: " + asset.assetPath);
#endif

            foreach (KeyValuePair<string, HashSet<int>> item in asset.UseGUIDs)
            {
                FR2_Asset tAsset;
                if (AssetMap.TryGetValue(item.Key, out tAsset))
                {
                    if (tAsset == null || tAsset.UsedByMap == null)
                    {
                        continue;
                    }

                    if (!tAsset.UsedByMap.ContainsKey(asset.guid))
                    {
                        tAsset.AddUsedBy(asset.guid, asset);
                    }
                }
            }
        }


        //---------------------------- Dependencies -----------------------------

        internal FR2_Asset Get(string guid, bool isForce = false)
        {
            return AssetMap.ContainsKey(guid) ? AssetMap[guid] : null;
        }

        internal List<FR2_Asset> FindAssetsOfType(FR2_AssetType type)
        {
            var result = new List<FR2_Asset>();
            foreach (KeyValuePair<string, FR2_Asset> item in AssetMap)
            {
                if (item.Value.type != type)
                {
                    continue;
                }

                result.Add(item.Value);
            }

            return result;
        }
        internal FR2_Asset FindAsset(string guid, string fileId)
        {
            if (AssetMap == null)
            {
                Check4Changes(false);
            }
            if (!isReady)
            {
#if FR2_DEBUG
			Debug.LogWarning("Cache not ready !");
#endif
                return null;
            }

            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            //for (var i = 0; i < guids.Length; i++)
            {
                //string guid = guids[i];
                FR2_Asset asset;
                if (!AssetMap.TryGetValue(guid, out asset))
                {
                    return null;
                }

                if (asset.IsMissing)
                {
                    return null;
                }

                if (asset.IsFolder)
                {
                    return null;
                }
                else
                {
                    return asset;
                }
            }
        }
        internal List<FR2_Asset> FindAssets(string[] guids, bool scanFolder)
        {
            if (AssetMap == null)
            {
                Check4Changes(false);
            }

            var result = new List<FR2_Asset>();

            if (!isReady)
            {
#if FR2_DEBUG
			Debug.LogWarning("Cache not ready !");
#endif
                return result;
            }

            var folderList = new List<FR2_Asset>();

            if (guids.Length == 0)
            {
                return result;
            }

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                FR2_Asset asset;
                if (!AssetMap.TryGetValue(guid, out asset))
                {
                    continue;
                }

                if (asset.IsMissing)
                {
                    continue;
                }

                if (asset.IsFolder)
                {
                    if (!folderList.Contains(asset))
                    {
                        folderList.Add(asset);
                    }
                }
                else
                {
                    result.Add(asset);
                }
            }

            if (!scanFolder || folderList.Count == 0)
            {
                return result;
            }

            int count = folderList.Count;
            for (var i = 0; i < count; i++)
            {
                FR2_Asset item = folderList[i];

                // for (var j = 0; j < item.UseGUIDs.Count; j++)
                // {
                //     FR2_Asset a;
                //     if (!AssetMap.TryGetValue(item.UseGUIDs[j], out a)) continue;
                foreach (KeyValuePair<string, HashSet<int>> useM in item.UseGUIDs)
                {
                    FR2_Asset a;
                    if (!AssetMap.TryGetValue(useM.Key, out a))
                    {
                        continue;
                    }

                    if (a.IsMissing)
                    {
                        continue;
                    }

                    if (a.IsFolder)
                    {
                        if (!folderList.Contains(a))
                        {
                            folderList.Add(a);
                            count++;
                        }
                    }
                    else
                    {
                        result.Add(a);
                    }
                }
            }

            return result;
        }

        //---------------------------- Dependencies -----------------------------

        internal List<List<string>> ScanSimilar(Action IgnoreWhenScan, Action IgnoreFolderWhenScan)
        {
            if (AssetMap == null)
            {
                Check4Changes(true);
            }

            var dict = new Dictionary<string, List<FR2_Asset>>();
            foreach (KeyValuePair<string, FR2_Asset> item in AssetMap)
            {
                if (item.Value == null)
                {
                    continue;
                }

                if (item.Value.IsMissing || item.Value.IsFolder)
                {
                    continue;
                }

                if (item.Value.inPlugins)
                {
                    continue;
                }

                if (item.Value.inEditor)
                {
                    continue;
                }

                if (!item.Value.assetPath.StartsWith("Assets/"))
                {
                    continue;
                }

                // if (item.Value.extension != ".png" && item.Value.extension != ".jpg") continue; 
                if (FR2_Setting.IsTypeExcluded(AssetType.GetIndex(item.Value.extension)))
                {
                    // Debug.LogWarning("ignore: " +item.Value.assetPath);
                    if (IgnoreWhenScan != null)
                    {
                        IgnoreWhenScan();
                    }

                    continue;
                }

                var isBreak = false;
                foreach (string ignore in FR2_Setting.s.listIgnore)
                {
                    if (item.Value.assetPath.StartsWith(ignore))
                    {
                        isBreak = true;
                        if (IgnoreFolderWhenScan != null)
                        {
                            IgnoreFolderWhenScan();
                        }

                        // Debug.Log("ignore " + item.Value.assetPath + " path ignore " + ignore);
                        break;
                    }
                }

                if (isBreak)
                {
                    continue;
                }


                string hash = item.Value.fileInfoHash;
                if (string.IsNullOrEmpty(hash))
                {
#if FR2_DEBUG
                    Debug.LogWarning("Hash can not be null! ");
#endif
                    continue;
                }

                List<FR2_Asset> list;
                if (!dict.TryGetValue(hash, out list))
                {
                    list = new List<FR2_Asset>();
                    dict.Add(hash, list);
                }

                list.Add(item.Value);
            }

            List<List<FR2_Asset>> result = dict.Values.Where(item => item.Count > 1).ToList();

            result.Sort((item1, item2) => { return item2[0].fileSize.CompareTo(item1[0].fileSize); });

            return result.Select(l => l.Select(i => i.assetPath).ToList()).ToList();
        }


        //internal List<FR2_DuplicateInfo> ScanDuplication(){
        //	if (AssetMap == null) Check4Changes(false);

        //	var dict = new Dictionary<string, FR2_DuplicateInfo>();
        //	foreach (var item in AssetMap){
        //		if (item.Value.IsMissing || item.Value.IsFolder) continue;
        //		var hash = item.Value.GetFileInfoHash();
        //		FR2_DuplicateInfo info;

        //		if (!dict.TryGetValue(hash, out info)){
        //			info = new FR2_DuplicateInfo(hash, item.Value.fileSize);
        //			dict.Add(hash, info);
        //		}

        //		info.assets.Add(item.Value);
        //	}

        //	var result = new List<FR2_DuplicateInfo>();
        //	foreach (var item in dict){
        //		if (item.Value.assets.Count > 1){
        //			result.Add(item.Value);
        //		}
        //	}

        //	result.Sort((item1, item2)=>{
        //		return item2.fileSize.CompareTo(item1.fileSize);
        //	});

        //	return result;
        //}

        private static HashSet<string> SPECIAL_USE_ASSETS = new HashSet<string>()
        {
            "Assets/link.xml", // this file used to control build/link process do not remove
			"Assets/csc.rsp",
            "Assets/mcs.rsp",
            "Assets/GoogleService-Info.plist",
            "Assets/google-services.json",
        };

        private static HashSet<string> SPECIAL_EXTENSIONS = new HashSet<string>()
        {
            ".asmdef",
            ".cginc",
            ".cs",
            ".dll",
        };


        internal List<FR2_Asset> ScanUnused()
        {
            if (AssetMap == null)
            {
                Check4Changes(false);
            }

            var result = new List<FR2_Asset>();
            foreach (KeyValuePair<string, FR2_Asset> item in AssetMap)
            {
                FR2_Asset v = item.Value;
                if (v.IsMissing || v.inEditor || v.IsScript || v.inResources || v.inPlugins || v.inStreamingAsset ||
                v.IsFolder)
                {
                    continue;
                }

                if (!v.assetPath.StartsWith("Assets/")) continue; // ignore built-in / packages assets
                if (SPECIAL_USE_ASSETS.Contains(v.assetPath)) continue; // ignore assets with special use (can not remove)
                if (SPECIAL_EXTENSIONS.Contains(v.extension)) continue;

                if (v.type == FR2_AssetType.DLL) continue;
                if (v.type == FR2_AssetType.SCRIPT) continue;
                if (v.type == FR2_AssetType.UNKNOWN) continue;

                if (v.IsExcluded) continue;
                if (!string.IsNullOrEmpty(v.AtlasName)) continue;
                if (!string.IsNullOrEmpty(v.AssetBundleName)) continue;
                if (!string.IsNullOrEmpty(v.AddressableName)) continue;

                if (v.UsedByMap.Count == 0) //&& !FR2_Asset.IGNORE_UNUSED_GUIDS.Contains(v.guid)
                {
                    result.Add(v);
                }
            }

            result.Sort((item1, item2) =>
            {
                if (item1.extension == item2.extension)
                {
                    return item1.assetPath.CompareTo(item2.assetPath);
                }

                return item1.extension.CompareTo(item2.extension);
            });
            return result;
        }
    }
}