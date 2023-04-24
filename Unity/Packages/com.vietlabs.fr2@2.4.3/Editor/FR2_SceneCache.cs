#if UNITY_2018_3_OR_NEWER
#define SUPPORT_NESTED_PREFAB
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_2017_1_OR_NEWER
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif
#if SUPPORT_NESTED_PREFAB
using UnityEditor.Experimental.SceneManagement;

#endif


namespace vietlabs.fr2
{
    public class FR2_SceneCache
    {
        private static FR2_SceneCache _api;
        public static Action onReady;
        public static bool ready = true;
        private Dictionary<Component, HashSet<HashValue>> _cache = new Dictionary<Component, HashSet<HashValue>>();
        public int current;
        public Dictionary<string, HashSet<Component>> folderCache = new Dictionary<string, HashSet<Component>>();

        private List<GameObject> listGO;

        //public HashSet<string> prefabDependencies = new HashSet<string>();
        public Dictionary<GameObject, HashSet<string>> prefabDependencies =
            new Dictionary<GameObject, HashSet<string>>();

        public int total;
        private IWindow window;

        public FR2_SceneCache()
        {
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged -= OnSceneChanged;
            EditorApplication.hierarchyChanged += OnSceneChanged;
#else
			EditorApplication.hierarchyWindowChanged -= OnSceneChanged;
			EditorApplication.hierarchyWindowChanged += OnSceneChanged;
#endif

#if UNITY_2018_2_OR_NEWER
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
#endif

#if UNITY_2017_1_OR_NEWER
            SceneManager.activeSceneChanged -= OnSceneChanged;
            SceneManager.activeSceneChanged += OnSceneChanged;

            SceneManager.sceneLoaded -= OnSceneChanged;
            SceneManager.sceneLoaded += OnSceneChanged;

            Undo.postprocessModifications -= OnModify;
            Undo.postprocessModifications += OnModify;
#endif

#if SUPPORT_NESTED_PREFAB
            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += prefabClose;
            PrefabStage.prefabStageOpened -= prefabOnpen;
            PrefabStage.prefabStageClosing += prefabClose;


#endif
        }

        public static FR2_SceneCache Api
        {
            get
            {
                if (_api == null)
                {
                    _api = new FR2_SceneCache();
                }

                return _api;
            }
        }

        private bool _dirty = true;
        public bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        public Dictionary<Component, HashSet<HashValue>> cache
        {
            get
            {
                if (_cache == null)
                {
                    refreshCache(window);
                }

                return _cache;
            }
        }

        public void refreshCache(IWindow window)
        {
            if (window == null)
            {
                return;
            }

            // if(!ready) return;
            this.window = window;

            _cache = new Dictionary<Component, HashSet<HashValue>>();
            folderCache = new Dictionary<string, HashSet<Component>>();
            prefabDependencies = new Dictionary<GameObject, HashSet<string>>();

            ready = false;

            List<GameObject> listRootGO = null;

#if SUPPORT_NESTED_PREFAB

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                GameObject rootPrefab = PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot;
                if (rootPrefab != null)
                {
                    listRootGO = new List<GameObject> { rootPrefab };
                }
            }

#else
#endif
            if (listRootGO == null)
            {
                listGO = FR2_Unity.getAllObjsInCurScene().ToList();
            }
            else
            {
                listGO = new List<GameObject>();
                foreach (GameObject item in listRootGO)
                {
                    listGO.AddRange(FR2_Unity.getAllChild(item, true));
                }
            }

            total = listGO.Count;
            current = 0;
            // Debug.Log("refresh cache total " + total);
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;

            // foreach (var item in FR2_Helper.getAllObjsInCurScene())
            // {
            //     // Debug.Log("object in scene: " + item.name);
            //     Component[] components = item.GetComponents<Component>();
            //     foreach (var com in components)
            //     {
            //         if(com == null) continue;
            //         SerializedObject serialized = new SerializedObject(com);
            //         SerializedProperty it = serialized.GetIterator().Copy();
            //         while (it.NextVisible(true))
            //         {

            //             if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
            //             if (it.objectReferenceValue == null) continue;

            // 			if(!_cache.ContainsKey(com)) _cache.Add(com, new HashSet<SerializedProperty>());
            // 			if(!_cache[com].Contains(it))
            // 				_cache[com].Add(it.Copy());
            //         }
            //     }
            // }
            Dirty = false;
        }

        private void OnUpdate()
        {
            for (var i = 0; i < 5 * FR2_Cache.priority; i++)
            {
                if (listGO == null || listGO.Count <= 0)
                {
                    //done
                    // Debug.Log("done");
                    EditorApplication.update -= OnUpdate;
                    ready = true;
                    Dirty = false;
                    listGO = null;
                    if (onReady != null)
                    {
                        onReady();
                    }

                    if (window != null)
                    {
                        window.OnSelectionChange();
                    }

                    return;
                }

                int index = listGO.Count - 1;

                GameObject go = listGO[index];
                if (go == null)
                {
                    continue;
                }

                string prefabGUID = FR2_Unity.GetPrefabParent(go);
                if (!string.IsNullOrEmpty(prefabGUID))
                {
                    Transform parent = go.transform.parent;
                    while (parent != null)
                    {
                        GameObject g = parent.gameObject;
                        if (!prefabDependencies.ContainsKey(g))
                        {
                            prefabDependencies.Add(g, new HashSet<string>());
                        }

                        prefabDependencies[g].Add(prefabGUID);
                        parent = parent.parent;
                    }
                }

                Component[] components = go.GetComponents<Component>();

                foreach (Component com in components)
                {
                    if (com == null)
                    {
                        continue;
                    }

                    var serialized = new SerializedObject(com);
                    SerializedProperty it = serialized.GetIterator().Copy();
                    while (it.NextVisible(true))
                    {
                        if (it.propertyType != SerializedPropertyType.ObjectReference)
                        {
                            continue;
                        }

                        if (it.objectReferenceValue == null)
                        {
                            continue;
                        }

                        var isSceneObject = true;
                        string path = AssetDatabase.GetAssetPath(it.objectReferenceValue);
                        if (!string.IsNullOrEmpty(path))
                        {
                            string dir = Path.GetDirectoryName(path);
                            if (!string.IsNullOrEmpty(dir))
                            {
                                isSceneObject = false;
                                if (!folderCache.ContainsKey(dir))
                                {
                                    folderCache.Add(dir, new HashSet<Component>());
                                }

                                if (!folderCache[dir].Contains(com))
                                {
                                    folderCache[dir].Add(com);
                                }
                            }
                        }

                        if (!_cache.ContainsKey(com))
                        {
                            _cache.Add(com, new HashSet<HashValue>());
                        }

                        _cache[com].Add(new HashValue
                        { target = it.objectReferenceValue, isSceneObject = isSceneObject });

                        // if (!_cache.ContainsKey(com)) _cache.Add(com, new HashSet<SerializedProperty>());
                        // if (!_cache[com].Contains(it))
                        //     _cache[com].Add(it.Copy());
                        // string path = AssetDatabase.GetAssetPath(it.objectReferenceValue);

                        // if (string.IsNullOrEmpty(path)) continue;
                        // string dir = System.IO.Path.GetDirectoryName(path);
                        // if (string.IsNullOrEmpty(dir)) continue;
                        // if (!folderCache.ContainsKey(dir)) folderCache.Add(dir, new HashSet<Component>());
                        // if (!folderCache[dir].Contains(com))
                        //     folderCache[dir].Add(com);
                    }
                }

                listGO.RemoveAt(index);
                current++;
            }
        }

        private void OnSceneChanged()
        {
            if (!Application.isPlaying)
            {
                Api.refreshCache(window);
                return;
            }

            SetDirty();
        }

#if UNITY_2017_1_OR_NEWER
        private UndoPropertyModification[] OnModify(UndoPropertyModification[] modifications)
        {
            for (var i = 0; i < modifications.Length; i++)
            {
                if (modifications[i].currentValue.objectReference != null)
                {
                    SetDirty();
                    break;
                }
            }

            return modifications;
        }
#endif


        public void SetDirty()
        {
            Dirty = true;
        }


        public class HashValue
        {
            public bool isSceneObject;

            public Object target;
            //public SerializedProperty pro;

            //			public HashValue(SerializedProperty pro, bool isSceneObject)
            //			{
            //				//this.pro = pro;
            //				this.isSceneObject = isSceneObject;
            //			}
        }
#if SUPPORT_NESTED_PREFAB

        private void prefabClose(PrefabStage obj)
        {
            if (!Application.isPlaying)
            {
                Api.refreshCache(window);
                return;
            }

            SetDirty();
        }

        private void prefabOnpen(PrefabStage obj)
        {
            if (!Application.isPlaying)
            {
                Api.refreshCache(window);
                return;
            }

            SetDirty();
        }
#endif

#if UNITY_2017_1_OR_NEWER
        private void OnSceneChanged(Scene scene, LoadSceneMode mode)
        {
            OnSceneChanged();
        }

        private void OnSceneChanged(Scene arg0, Scene arg1)
        {
            OnSceneChanged();
        }
#endif
    }
}