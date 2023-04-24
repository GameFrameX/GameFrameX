using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    public class FR2_UsedInBuild : IRefDraw
    {
        private readonly FR2_TreeUI2.GroupDrawer groupDrawer;

        private bool dirty;
        private readonly FR2_RefDrawer drawer;
        internal Dictionary<string, FR2_Ref> refs;

        public FR2_UsedInBuild(IWindow window)
        {
            this.window = window;
            drawer = new FR2_RefDrawer(window);
            dirty = true;
            drawer.SetDirty();
        }

        public IWindow window { get; set; }


        public int ElementCount()
        {
            return refs == null ? 0 : refs.Count;
        }

        public bool Draw(Rect rect)
        {
            if (dirty)
            {
                RefreshView();
            }

            return drawer.Draw(rect);
        }

        public bool DrawLayout()
        {
            //Debug.Log("draw");
            if (dirty)
            {
                RefreshView();
            }

            return drawer.DrawLayout();
        }

        public void SetDirty()
        {
            dirty = true;
            drawer.SetDirty();
        }

        public void RefreshView()
        {
            var scenes = new HashSet<string>();
            // string[] scenes = new string[sceneCount];
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene == null)
                {
                    continue;
                }

                if (scene.enabled == false)
                {
                    continue;
                }

                string sce = AssetDatabase.AssetPathToGUID(scene.path);

                if (scenes.Contains(sce))
                {
                    continue;
                }

                scenes.Add(sce);
            }

            refs = FR2_Ref.FindUsage(scenes.ToArray());

            foreach (string VARIABLE in scenes)
            {
                FR2_Ref asset = null;
                if (!refs.TryGetValue(VARIABLE, out asset))
                {
                    continue;
                }


                asset.depth = 1;
            }

            List<FR2_Asset> list = FR2_Cache.Api.AssetList;
            int count = list.Count;

            for (var i = 0; i < count; i++)
            {
                FR2_Asset item = list[i];
                if (item.inEditor) continue;
                if (item.inPlugins)
                {
                    if (item.type == FR2_AssetType.SCENE) continue;
                }

                if (item.inResources || item.inStreamingAsset || item.inPlugins)
                {
                    if (refs.ContainsKey(item.guid))
                    {
                        continue;
                    }

                    refs.Add(item.guid, new FR2_Ref(0, 1, item, null));
                }
            }

            // remove ignored items
            var vals = refs.Values.ToArray();
            foreach (var item in vals)
            {
                foreach (var ig in FR2_Setting.s.listIgnore)
                {
                    if (!item.asset.assetPath.StartsWith(ig)) continue;
                    refs.Remove(item.asset.guid);
                    //Debug.Log("Remove: " + item.asset.assetPath + "\n" + ig);
                    break;
                }
            }

            drawer.SetRefs(refs);
            dirty = false;
        }

        internal void RefreshSort()
        {
            drawer.RefreshSort();
        }
    }
}