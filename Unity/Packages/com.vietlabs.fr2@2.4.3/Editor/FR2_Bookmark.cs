using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    public class FR2_Bookmark : IRefDraw
    {
        internal static HashSet<string> guidSet = new HashSet<string>();
        internal static HashSet<string> instSet = new HashSet<string>(); // Do not reference directly to SceneObject (which might be destroyed anytime)

        public static int Count
        {
            get { return guidSet.Count + instSet.Count; }
        }

        public static bool Contains(string guidOrInstID)
        {
            return guidSet.Contains(guidOrInstID) || instSet.Contains(guidOrInstID);
        }

        public static bool Contains(UnityObject sceneObject)
        {
            var id = sceneObject.GetInstanceID().ToString();
            return instSet.Contains(id);
        }
        public static bool Contains(FR2_Ref rf)
        {
            if (rf.isSceneRef)
            {
                if (instSet == null) return false;
                return instSet.Contains(rf.component.GetInstanceID().ToString());
            }
            else
            {
                if (guidSet == null) return false;
                return guidSet.Contains(rf.asset.guid);
            }
        }
        public static void Add(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            instSet.Add(id); // hashset does not need to check exist before add
            dirty = true;
        }

        public static void Add(string guid)
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
            //Debug.Log(instSet.Count + " : " + guidSet.Count);
        }

        public static void Remove(UnityObject sceneObject)
        {
            if (sceneObject == null) return;
            var id = sceneObject.GetInstanceID().ToString();
            instSet.Remove(id);
            dirty = true;
        }

        public static void Remove(string guidOrInstID)
        {
            guidSet.Remove(guidOrInstID);
            instSet.Remove(guidOrInstID);
            dirty = true;
        }

        public static void Clear()
        {
            guidSet.Clear();
            instSet.Clear();
            dirty = true;
        }

        public static void Add(FR2_Ref rf)
        {

            if (rf.isSceneRef)
            {
                //Debug.Log("add " + rf.component);
                Add(rf.component);
            }
            else
            {
                Add(rf.asset.guid);
            }
        }

        public static void Remove(FR2_Ref rf)
        {

            if (rf.isSceneRef)
            {
                //Debug.Log("remove: " + rf.component);
                Remove(rf.component);
            }
            else
            {
                Remove(rf.asset.guid);
            }
        }

        public static void Commit()
        {
            var list = new List<Object>();

            foreach (string guid in guidSet)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                if (obj != null) list.Add(obj);
            }

            foreach (string instID in instSet)
            {
                var id = int.Parse(instID);
                var obj = EditorUtility.InstanceIDToObject(id);
                if (obj != null) list.Add(obj);
            }

            Selection.objects = list.ToArray();
        }

        // ------------ instance

        //private readonly FR2_TreeUI2.GroupDrawer groupDrawer;
        private static bool dirty;
        private readonly FR2_RefDrawer drawer;
        internal Dictionary<string, FR2_Ref> refs = new Dictionary<string, FR2_Ref>();

        public FR2_Bookmark(IWindow window)
        {
            this.window = window;
            drawer = new FR2_RefDrawer(window);
            drawer.messageNoRefs = "Do bookmark something!";

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
            if (refs == null)
            {
                Debug.Log("Refs is null!");
                return false;
            }

            var bottomRect = new Rect(rect.x + 1f, rect.yMax - 16f, rect.width - 2f, 16f);
            DrawButtons(bottomRect);

            rect.yMax -= 16f;
            return drawer.Draw(rect);
        }

        public void SetDirty()
        {
            drawer.SetDirty();
        }

        void DrawButtons(Rect rect)
        {
            if (Count == 0) return;

            GUILayout.BeginArea(rect);
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Select"))
                    {
                        Commit();
                        window.WillRepaint = true;
                    }

                    if (GUILayout.Button("Clear"))
                    {
                        Clear();
                        window.WillRepaint = true;
                    }

                    if (GUILayout.Button("CSV"))
                    {
                        FR2_Export.ExportCSV(refs.Values.ToArray());
                    }

                    if (GUILayout.Button("Delete"))
                    {
                        FR2_Unity.BackupAndDeleteAssets(refs.Values.ToArray());
                        Clear();
                        GUIUtility.ExitGUI();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        public void RefreshView()
        {
            if (refs == null) refs = new Dictionary<string, FR2_Ref>();
            refs.Clear();

            foreach (var guid in guidSet)
            {
                var asset = FR2_Cache.Api.Get(guid, false);
                refs.Add(guid, new FR2_Ref(0, 0, asset, null));
            }

            foreach (var instId in instSet)
            {
                refs.Add(instId, new FR2_SceneRef(0, EditorUtility.InstanceIDToObject(int.Parse(instId))));
            }


            drawer.SetRefs(refs);

            //Debug.Log("RefreshView: " + refs.Count);
            dirty = false;
        }

        internal void RefreshSort()
        {
            drawer.RefreshSort();
        }
    }
}