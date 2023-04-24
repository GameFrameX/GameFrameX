using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    public class FR2_Selection : IRefDraw
    {
        internal HashSet<string> guidSet = new HashSet<string>();
        internal HashSet<string> instSet = new HashSet<string>(); // Do not reference directly to SceneObject (which might be destroyed anytime)

        public int Count
        {
            get { return guidSet.Count + instSet.Count; }
        }

        public bool Contains(string guidOrInstID)
        {
            return guidSet.Contains(guidOrInstID) || instSet.Contains(guidOrInstID);
        }

        public bool Contains(UnityObject sceneObject)
        {
            var id = sceneObject.GetInstanceID().ToString();
            return instSet.Contains(id);
        }

        public void Add(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            instSet.Add(id); // hashset does not need to check exist before add
            dirty = true;
        }

        public void AddRange(params UnityObject[] sceneObjects)
        {
            foreach (var go in sceneObjects)
            {
                var id = go.GetInstanceID().ToString();
                instSet.Add(id); // hashset does not need to check exist before add	
            }

            dirty = true;
        }

        public void Add(string guid)
        {
            if (guidSet.Contains(guid)) return;
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogWarning("Invalid GUID: " + guid);
                return;
            }

            guidSet.Add(guid);
            dirty = true;
        }

        public void AddRange(params string[] guids)
        {
            foreach (var id in guids)
            {
                Add(id);
            }
            dirty = true;
        }

        public void Remove(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            instSet.Remove(id);
            dirty = true;
        }

        public void Remove(string guidOrInstID)
        {
            guidSet.Remove(guidOrInstID);
            instSet.Remove(guidOrInstID);

            dirty = true;
        }

        public void Clear()
        {
            guidSet.Clear();
            instSet.Clear();
            dirty = true;
        }

        public bool isSelectingAsset
        {
            get { return instSet.Count == 0; }
        }

        public void Add(FR2_Ref rf)
        {
            if (rf.isSceneRef)
            {
                Add(rf.component);
            }
            else
            {
                Add(rf.asset.guid);
            }
        }

        public void Remove(FR2_Ref rf)
        {
            if (rf.isSceneRef)
            {
                Remove(rf.component);
            }
            else
            {
                Remove(rf.asset.guid);
            }
        }

        // ------------ instance

        private bool dirty;
        private readonly FR2_RefDrawer drawer;
        internal Dictionary<string, FR2_Ref> refs;
        internal bool isLock;

        public FR2_Selection(IWindow window)
        {
            this.window = window;
            drawer = new FR2_RefDrawer(window);
            drawer.groupDrawer.hideGroupIfPossible = true;
            drawer.forceHideDetails = true;
            drawer.level0Group = string.Empty;

            dirty = true;
            drawer.SetDirty();
        }

        public IWindow window { get; set; }

        public int ElementCount()
        {
            return refs == null ? 0 : refs.Count;
        }

        public bool DrawLayout()
        {
            if (dirty) RefreshView();
            return drawer.DrawLayout();
        }

        public bool Draw(Rect rect)
        {
            if (dirty) RefreshView();
            if (refs == null) return false;

            DrawLock(new Rect(rect.xMax - 12f, rect.yMin - 12f, 16f, 16f));

            return drawer.Draw(rect);
        }

        public void SetDirty()
        {
            drawer.SetDirty();
        }

        private static readonly Color PRO = new Color(0.8f, 0.8f, 0.8f, 1f);
        private static readonly Color INDIE = new Color(0.1f, 0.1f, 0.1f, 1f);

        public void DrawLock(Rect rect)
        {
            GUI2.ContentColor(() =>
            {
                var icon = isLock ? FR2_Icon.Lock : FR2_Icon.Unlock;
                if (GUI2.Toggle(rect, ref isLock, icon))
                {
                    window.WillRepaint = true;
                    window.OnSelectionChange();
                }
            }, GUI2.Theme(PRO, INDIE));
        }

        public void RefreshView()
        {
            if (refs == null) refs = new Dictionary<string, FR2_Ref>();
            refs.Clear();

            if (instSet.Count > 0)
            {
                foreach (var instId in instSet)
                {
                    refs.Add(instId, new FR2_SceneRef(0, EditorUtility.InstanceIDToObject(int.Parse(instId))));
                }
            }
            else
            {
                foreach (var guid in guidSet)
                {
                    var asset = FR2_Cache.Api.Get(guid, false);
                    refs.Add(guid, new FR2_Ref(0, 0, asset, null)
                    {
                        isSceneRef = false
                    });
                }
            }

            drawer.SetRefs(refs);
            dirty = false;
        }
    }
}