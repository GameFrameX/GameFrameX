//#define FR2_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CBParams = System.Collections.Generic.List<System.Collections.Generic.List<string>>;
using Object = UnityEngine.Object;

namespace vietlabs.fr2
{
    internal class FR2_DuplicateTree2 : IRefDraw
    {
        private const float TimeDelayDelete = .5f;

        private static readonly FR2_FileCompare fc = new FR2_FileCompare();
        private readonly FR2_TreeUI2.GroupDrawer groupDrawer;
        private CBParams cacheAssetList;
        public bool caseSensitive = false;
        private Dictionary<string, List<FR2_Ref>> dicIndex; //index, list

        private bool dirty;
        private int excludeCount;
        private string guidPressDelete;
        internal List<FR2_Ref> list;
        internal Dictionary<string, FR2_Ref> refs;
        public int scanExcludeByIgnoreCount;
        public int scanExcludeByTypeCount;
        private string searchTerm = "";
        private float TimePressDelete;

        public FR2_DuplicateTree2(IWindow window)
        {
            this.window = window;
            groupDrawer = new FR2_TreeUI2.GroupDrawer(DrawGroup, DrawAsset);
        }

        public IWindow window { get; set; }

        public bool Draw(Rect rect)
        {

            return false;
        }

        public bool DrawLayout()
        {
            if (dirty)
            {
                RefreshView(cacheAssetList);
            }

            if (fc.nChunks2 > 0 && fc.nScaned < fc.nChunks2)
            {
                Rect rect = GUILayoutUtility.GetRect(1, Screen.width, 18f, 18f);
                float p = fc.nScaned / (float)fc.nChunks2;

                EditorGUI.ProgressBar(rect, p, string.Format("Scanning {0} / {1}", fc.nScaned, fc.nChunks2));
                GUILayout.FlexibleSpace();
                return true;
            }

            if (groupDrawer.hasValidTree)
            {
                groupDrawer.tree.itemPaddingRight = 60f;
            }
            groupDrawer.DrawLayout();
            DrawHeader();
            return false;
        }

        public int ElementCount()
        {
            return list == null ? 0 : list.Count;
        }

        private void DrawAsset(Rect r, string guid)
        {
            FR2_Ref rf;
            if (!refs.TryGetValue(guid, out rf))
            {
                return;
            }

            rf.asset.Draw(r, false,
                FR2_Setting.GroupMode != FR2_RefDrawer.Mode.Folder,
                FR2_Setting.ShowFileSize,
                FR2_Setting.s.displayAssetBundleName,
                FR2_Setting.s.displayAtlasName,
                FR2_Setting.s.showUsedByClassed,
                window);

            Texture tex = AssetDatabase.GetCachedIcon(rf.asset.assetPath);
            if (tex == null)
            {
                return;
            }

            Rect drawR = r;
            drawR.x = drawR.x + drawR.width; // (groupDrawer.TreeNoScroll() ? 60f : 70f) ;
            drawR.width = 40f;
            drawR.y += 1;
            drawR.height -= 2;

            if (GUI.Button(drawR, "Use", EditorStyles.miniButton))
            {
                if (FR2_Export.IsMergeProcessing)
                {
                    Debug.LogWarning("Previous merge is processing");
                }
                else
                {
                    //AssetDatabase.SaveAssets();
                    //EditorGUIUtility.systemCopyBuffer = rf.asset.guid;
                    //EditorGUIUtility.systemCopyBuffer = rf.asset.guid;
                    // Debug.Log("guid: " + rf.asset.guid + "  systemCopyBuffer " + EditorGUIUtility.systemCopyBuffer);
                    int index = rf.index;
                    Selection.objects = list.Where(x => x.index == index)
                        .Select(x => FR2_Unity.LoadAssetAtPath<Object>(x.asset.assetPath)).ToArray();
                    FR2_Export.MergeDuplicate(rf.asset.guid);
                }
            }

            if (rf.asset.UsageCount() > 0)
            {
                return;
            }

            drawR.x -= 25;
            drawR.width = 20;
            if (wasPreDelete(guid))
            {
                Color col = GUI.color;
                GUI.color = Color.red;
                if (GUI.Button(drawR, "X", EditorStyles.miniButton))
                {
                    guidPressDelete = null;
                    AssetDatabase.DeleteAsset(rf.asset.assetPath);
                }

                GUI.color = col;
                window.WillRepaint = true;
            }
            else
            {
                if (GUI.Button(drawR, "X", EditorStyles.miniButton))
                {
                    guidPressDelete = guid;
                    TimePressDelete = Time.realtimeSinceStartup;
                    window.WillRepaint = true;
                }
            }
        }

        private bool wasPreDelete(string guid)
        {
            if (guidPressDelete == null || guid != guidPressDelete)
            {
                return false;
            }

            if (Time.realtimeSinceStartup - TimePressDelete < TimeDelayDelete)
            {
                return true;
            }

            guidPressDelete = null;
            return false;
        }

        private void DrawGroup(Rect r, string label, int childCount)
        {
            // GUI.Label(r, label + " (" + childCount + ")", EditorStyles.boldLabel);
            FR2_Asset asset = dicIndex[label][0].asset;

            Texture tex = AssetDatabase.GetCachedIcon(asset.assetPath);
            Rect rect = r;

            if (tex != null)
            {
                rect.width = 16f;
                GUI.DrawTexture(rect, tex);
            }

            rect = r;
            rect.xMin += 16f;
            GUI.Label(rect, asset.assetName, EditorStyles.boldLabel);

            rect = r;
            rect.xMin += rect.width - 50f;
            GUI.Label(rect, FR2_Helper.GetfileSizeString(asset.fileSize), EditorStyles.miniLabel);

            rect = r;
            rect.xMin += rect.width - 70f;
            GUI.Label(rect, childCount.ToString(), EditorStyles.miniLabel);

            rect = r;
            rect.xMin += rect.width - 70f;
        }


        // private List<FR2_DuplicateFolder> duplicated;

        public void Reset(CBParams assetList)
        {
            fc.Reset(assetList, OnUpdateView, RefreshView);
        }

        private void OnUpdateView(CBParams assetList) { }

        public bool isExclueAnyItem()
        {
            return excludeCount > 0 || scanExcludeByTypeCount > 0;
        }

        public bool isExclueAnyItemByIgnoreFolder()
        {
            return scanExcludeByIgnoreCount > 0;
        }

        // void OnActive
        private void RefreshView(CBParams assetList)
        {
            cacheAssetList = assetList;
            dirty = false;
            list = new List<FR2_Ref>();
            refs = new Dictionary<string, FR2_Ref>();
            dicIndex = new Dictionary<string, List<FR2_Ref>>();
            if (assetList == null)
            {
                return;
            }

            int minScore = searchTerm.Length;
            string term1 = searchTerm;
            if (!caseSensitive)
            {
                term1 = term1.ToLower();
            }

            string term2 = term1.Replace(" ", string.Empty);
            excludeCount = 0;

            for (var i = 0; i < assetList.Count; i++)
            {
                var lst = new List<FR2_Ref>();
                for (var j = 0; j < assetList[i].Count; j++)
                {
                    var path = assetList[i][j];
                    if (!path.StartsWith("Assets/"))
                    {
                        Debug.LogWarning("Ignore asset: " + path);
                        continue;
                    }

                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (string.IsNullOrEmpty(guid))
                    {
                        continue;
                    }

                    if (refs.ContainsKey(guid))
                    {
                        continue;
                    }

                    FR2_Asset asset = FR2_Cache.Api.Get(guid);
                    if (asset == null) continue;
                    if (!asset.assetPath.StartsWith("Assets/")) continue; // ignore builtin, packages, ...

                    var fr2 = new FR2_Ref(i, 0, asset, null);

                    if (FR2_Setting.IsTypeExcluded(fr2.type))
                    {
                        excludeCount++;
                        continue; //skip this one
                    }

                    if (string.IsNullOrEmpty(searchTerm))
                    {
                        fr2.matchingScore = 0;
                        list.Add(fr2);
                        lst.Add(fr2);
                        refs.Add(guid, fr2);
                        continue;
                    }

                    //calculate matching score
                    string name1 = fr2.asset.assetName;
                    if (!caseSensitive)
                    {
                        name1 = name1.ToLower();
                    }

                    string name2 = name1.Replace(" ", string.Empty);

                    int score1 = FR2_Unity.StringMatch(term1, name1);
                    int score2 = FR2_Unity.StringMatch(term2, name2);

                    fr2.matchingScore = Mathf.Max(score1, score2);
                    if (fr2.matchingScore > minScore)
                    {
                        list.Add(fr2);
                        lst.Add(fr2);
                        refs.Add(guid, fr2);
                    }
                }

                dicIndex.Add(i.ToString(), lst);
            }

            ResetGroup();
        }

        private void ResetGroup()
        {
            groupDrawer.Reset(list,
                rf => rf.asset.guid
                , GetGroup, SortGroup);
            if (window != null)
            {
                window.Repaint();
            }
        }

        private string GetGroup(FR2_Ref rf)
        {
            return rf.index.ToString();
        }

        private void SortGroup(List<string> groups)
        {
            // groups.Sort( (item1, item2) =>
            // {
            // 	if (item1 == "Others" || item2 == "Selection") return 1;
            // 	if (item2 == "Others" || item1 == "Selection") return -1;
            // 	return item1.CompareTo(item2);
            // });
        }

        public void SetDirty()
        {
            dirty = true;
        }

        public void RefreshSort() { }

        private void DrawHeader()
        {
            var text = groupDrawer.hasValidTree ? "Rescan" : "Scan";

            if (GUILayout.Button(text))
            {
                // if (FR2_Cache)
                {
                    OnCacheReady();
                    return;
                }

                // FR2_Cache.onReady -= OnCacheReady;
                // FR2_Cache.onReady += OnCacheReady;
                // FR2_Cache.Api.Check4Changes(false);
            }
        }

        private void OnCacheReady()
        {
            scanExcludeByTypeCount = 0;
            Reset(FR2_Cache.Api.ScanSimilar(IgnoreTypeWhenScan, IgnoreFolderWhenScan));
            FR2_Cache.onReady -= OnCacheReady;
        }

        private void IgnoreTypeWhenScan()
        {
            scanExcludeByTypeCount++;
        }

        private void IgnoreFolderWhenScan()
        {
            scanExcludeByIgnoreCount++;
        }
    }

    internal class FR2_FileCompare
    {
        public static HashSet<FR2_Chunk> HashChunksNotComplete;
        internal static int streamClosedCount;
        private CBParams cacheList;
        public List<FR2_Head> deads = new List<FR2_Head>();
        public List<FR2_Head> heads = new List<FR2_Head>();

        public int nChunks;
        public int nChunks2;
        public int nScaned;
        public Action<CBParams> OnCompareComplete;
        public Action<CBParams> OnCompareUpdate;
        // private int streamCount;

        public void Reset(CBParams list, Action<CBParams> onUpdate, Action<CBParams> onComplete)
        {
            nChunks = 0;
            nScaned = 0;
            nChunks2 = 0;
            // streamCount = streamClosedCount = 0;
            HashChunksNotComplete = new HashSet<FR2_Chunk>();

            if (heads.Count > 0)
            {
                for (var i = 0; i < heads.Count; i++)
                {
                    heads[i].CloseChunk();
                }
            }

            deads.Clear();
            heads.Clear();

            OnCompareUpdate = onUpdate;
            OnCompareComplete = onComplete;
            if (list.Count <= 0)
            {
                OnCompareComplete(new CBParams());
                return;
            }

            cacheList = list;
            for (var i = 0; i < list.Count; i++)
            {
                var file = new FileInfo(list[i][0]);
                int nChunk = Mathf.CeilToInt(file.Length / (float)FR2_Head.chunkSize);
                nChunks2 += nChunk;
            }

            // for(int i =0;i< list.Count;i++)
            // {
            //     AddHead(list[i]);
            // }
            AddHead(cacheList[cacheList.Count - 1]);
            cacheList.RemoveAt(cacheList.Count - 1);

            EditorApplication.update -= ReadChunkAsync;
            EditorApplication.update += ReadChunkAsync;
        }

        public FR2_FileCompare AddHead(List<string> files)
        {
            if (files.Count < 2)
            {
                Debug.LogWarning("Something wrong ! head should not contains < 2 elements");
            }

            var chunkList = new List<FR2_Chunk>();
            for (var i = 0; i < files.Count; i++)
            {
                // streamCount++;

                // try 
                // {
                // 	Debug.Log("new stream ");
                // 	stream = new FileStream(files[i], FileMode.Open, FileAccess.Read);
                // }
                // catch (Exception e)
                // {
                // 	Debug.LogWarning(e + "\nCan not open file: " + files[i]);
                // 	if (stream != null) stream.Close();
                // 	continue;
                // }

                chunkList.Add(new FR2_Chunk
                {
                    file = files[i],
                    buffer = new byte[FR2_Head.chunkSize]
                });
            }

            var file = new FileInfo(files[0]);
            int nChunk = Mathf.CeilToInt(file.Length / (float)FR2_Head.chunkSize);

            heads.Add(new FR2_Head
            {
                fileSize = file.Length,
                currentChunk = 0,
                nChunk = nChunk,
                chunkList = chunkList
            });

            nChunks += nChunk;

            return this;
        }

        // private bool checkCompleteAllCurFile()
        // {
        // 	return streamClosedCount + HashChunksNotComplete.Count >= streamCount; //-1 for safe
        // }

        private void ReadChunkAsync()
        {
            bool alive = ReadChunk();
			if (alive)
			{
				return;
			}

            if (cacheList.Count > 0)
            {
                AddHead(cacheList[cacheList.Count - 1]);
                cacheList.RemoveAt(cacheList.Count - 1);
            }

            var update = false;
            for (int i = heads.Count - 1; i >= 0; i--)
            {
                FR2_Head h = heads[i];
                if (!h.isDead) continue;

                h.CloseChunk();
                heads.RemoveAt(i);
                if (h.chunkList.Count > 1)
                {
                    update = true;
                    deads.Add(h);
                }
            }

            if (update)
            {
                Trigger(OnCompareUpdate);
            }

            if (!alive && cacheList.Count <= 0) //&& cacheList.Count <= 0 complete all chunk and cache list empty
            {
                foreach (FR2_Chunk item in HashChunksNotComplete)
                {
                    if (item.stream != null && item.stream.CanRead)
                    {
                        Debug.Log("Close Stream!");

                        item.stream.Close();
                        item.stream = null;
                    }
                }

                HashChunksNotComplete.Clear();
                // Debug.Log("complete ");
                nScaned = nChunks;
                EditorApplication.update -= ReadChunkAsync;
                Trigger(OnCompareComplete);
            }
        }

        private void Trigger(Action<CBParams> cb)
        {
            if (cb == null)
            {
                return;
            }

            CBParams list = deads.Select(item => item.GetFiles()).ToList();

            //#if FR2_DEBUG
            //        Debug.Log("Callback ! " + deads.Count + ":" + heads.Count);
            //#endif
            cb(list);
        }

        private bool ReadChunk()
        {
            var alive = false;

            for (var i = 0; i < heads.Count; i++)
            {
                FR2_Head h = heads[i];
                if (h.isDead)
                {
					//Debug.LogWarning("Should never be here : " + h.chunkList[0].file);
                    continue;
                }

                nScaned++;
                alive = true;
                h.ReadChunk();
                h.CompareChunk(heads);
                break;
            }

            //if (!alive) return false;

            //alive = false;
            //for (var i = 0; i < heads.Count; i++)
            //{
            //    var h = heads[i];
            //    if (h.isDead) continue;

            //    h.CompareChunk(heads);
            //    alive |= !h.isDead;
            //}

            return alive;
        }
    }

    internal class FR2_Head
    {
        public const int chunkSize = 10240;

        public List<FR2_Chunk> chunkList;
        public int currentChunk;

        public long fileSize;

        public int nChunk;
        public int size; //last stream read size

        public bool isDead
        {
            get { return currentChunk == nChunk || chunkList.Count == 1; }
        }

        public List<string> GetFiles()
        {
            return chunkList.Select(item => item.file).ToList();
        }

        public void AddToDict(byte b, FR2_Chunk chunk, Dictionary<byte, List<FR2_Chunk>> dict)
        {
            List<FR2_Chunk> list;
            if (!dict.TryGetValue(b, out list))
            {
                list = new List<FR2_Chunk>();
                dict.Add(b, list);
            }

            list.Add(chunk);
        }

        public void CloseChunk()
        {
            for (var i = 0; i < chunkList.Count; i++)
            {
                FR2_FileCompare.streamClosedCount++;

                if (chunkList[i].stream != null)
                {
					#if FR2_DEBUG
					Debug.Log("stream close: " + chunkList[i].file);
					#endif

                    chunkList[i].stream.Close();
                    chunkList[i].stream = null;
                }
            }
        }

        public void ReadChunk()
        {
#if FR2_DEBUG
        if (currentChunk == 0) Debug.LogWarning("Read <" + chunkList[0].file + "> " + currentChunk + ":" + nChunk);
#endif
            if (currentChunk == nChunk)
            {
                Debug.LogWarning("Something wrong, should dead <" + isDead + ">");
                return;
            }

            int from = currentChunk * chunkSize;
            size = (int)Mathf.Min(fileSize - from, chunkSize);

			for (var i = 0; i < chunkList.Count; i++)
			{
				FR2_Chunk chunk = chunkList[i];
				if (chunk.streamError) continue;
                chunk.size = size;

                if (chunk.streamInited == false)
                {
                    chunk.streamInited = true;

                    try
                    {
						#if FR2_DEBUG
						Debug.Log("New chunk: " + chunk.file);
						#endif
                        chunk.stream = new FileStream(chunk.file, FileMode.Open, FileAccess.Read);
                    }
					#if FR2_DEBUG
                    catch (Exception e)
                    {
						
						Debug.LogWarning("Exception: " + e + "\n" + chunk.file + "\n" + chunk.stream);
					#else
					catch {
					#endif

						chunk.streamError = true;
                        if (chunk.stream != null) // just to make sure we close the stream
                        {
                            chunk.stream.Close();
                            chunk.stream = null;
                        }
                    }

                    if (chunk.stream == null)
                    {
						chunk.streamError = true;
                        continue;
                    }
                }

                try
                {
                    chunk.stream.Read(chunk.buffer, 0, size);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e + "\n" + chunk.file);

					chunk.streamError = true;
                    chunk.stream.Close();
				}
			}

			// clean up dead chunks
			for (var i = chunkList.Count-1; i>=0; i--)
			{
				if (chunkList[i].streamError) chunkList.RemoveAt(i);
			}

			if (chunkList.Count == 1)
			{
				Debug.LogWarning("No more chunk in list");
			}

            currentChunk++;
        }

        public void CompareChunk(List<FR2_Head> heads)
        {
            int idx = chunkList.Count;
            byte[] buffer = chunkList[idx - 1].buffer;

            while (--idx >= 0)
            {
                FR2_Chunk chunk = chunkList[idx];
                int diff = FirstDifferentIndex(buffer, chunk.buffer, size);
                if (diff == -1)
                {
                    continue;
                }
#if FR2_DEBUG
            Debug.Log(string.Format(
                " --> Different found at : idx={0} diff={1} size={2} chunk={3}",
            idx, diff, size, currentChunk));
#endif

                byte v = buffer[diff];
                var d = new Dictionary<byte, List<FR2_Chunk>>(); //new heads
                chunkList.RemoveAt(idx);
                FR2_FileCompare.HashChunksNotComplete.Add(chunk);

                AddToDict(chunk.buffer[diff], chunk, d);

                for (int j = idx - 1; j >= 0; j--)
                {
                    FR2_Chunk tChunk = chunkList[j];
                    byte tValue = tChunk.buffer[diff];
                    if (tValue == v)
                    {
                        continue;
                    }

                    idx--;
                    FR2_FileCompare.HashChunksNotComplete.Add(tChunk);
                    chunkList.RemoveAt(j);
                    AddToDict(tChunk.buffer[diff], tChunk, d);
                }

                foreach (KeyValuePair<byte, List<FR2_Chunk>> item in d)
                {
                    List<FR2_Chunk> list = item.Value;
                    if (list.Count == 1)
                    {
#if FR2_DEBUG
                    Debug.Log(" --> Dead head found for : " + list[0].file);
#endif
						if (list[0].stream != null) list[0].stream.Close();
                    }
                    else if (list.Count > 1) // 1 : dead head
                    {
#if FR2_DEBUG
                    Debug.Log(" --> NEW HEAD : " + list[0].file);
#endif
                        heads.Add(new FR2_Head
                        {
                            nChunk = nChunk,
                            fileSize = fileSize,
                            currentChunk = currentChunk - 1,
                            chunkList = list
                        });
                    }
                }
            }
        }

        internal static int FirstDifferentIndex(byte[] arr1, byte[] arr2, int maxIndex)
        {
            for (var i = 0; i < maxIndex; i++)
            {
                if (arr1[i] != arr2[i])
                {
                    return i;
                }
            }

            return -1;
        }
    }

    internal class FR2_Chunk
    {
        public byte[] buffer;
        public string file;
        public long size;

        public bool streamInited = false;
		public bool streamError = false;
        public FileStream stream;
    }
}