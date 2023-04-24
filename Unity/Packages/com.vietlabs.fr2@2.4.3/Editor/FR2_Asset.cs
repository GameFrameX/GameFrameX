using System.Globalization;
//#define FR2_DEBUG_BRACE_LEVEL
//#define FR2_DEBUG_SYMBOL
//#define FR2_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


#if FR2_ADDRESSABLE
using UnityEditor.AddressableAssets;
using UnityEngine.AddressableAssets;
#endif


using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    public enum FR2_AssetType
    {
        UNKNOWN,
        FOLDER,
        SCRIPT,
        SCENE,
        DLL,
        REFERENCABLE,
        BINARY_ASSET,
        MODEL,
        TERRAIN,
        NON_READABLE
    }

    public enum FR2_AssetState
    {
        NEW,
        CACHE,
        MISSING
    }

    [Serializable]
    public class FR2_Asset
    {
        // ------------------------------ CONSTANTS ---------------------------

        private static readonly HashSet<string> SCRIPT_EXTENSIONS = new HashSet<string>
        {
            ".cs", ".js", ".boo", ".h", ".java", ".cpp", ".m", ".mm"
        };

        private static readonly HashSet<string> REFERENCABLE_EXTENSIONS = new HashSet<string>
        {
            ".anim", ".controller", ".mat", ".unity", ".guiskin", ".prefab",
            ".overridecontroller", ".mask", ".rendertexture", ".cubemap", ".flare",
            ".mat", ".prefab", ".physicsmaterial", ".fontsettings", ".asset", ".prefs", ".spriteatlas"
        };

        private static readonly Dictionary<int, Type> HashClasses = new Dictionary<int, Type>();
        internal static Dictionary<string, GUIContent> cacheImage = new Dictionary<string, GUIContent>();

        private bool _isExcluded;
        private Dictionary<string, HashSet<int>> _UseGUIDs;
        private float excludeTS;

        public static float ignoreTS;


        // ----------------------------- DRAW  ---------------------------------------
        [NonSerialized] private GUIContent fileSizeText;

        // ----------------------------- DRAW  ---------------------------------------

        [SerializeField] public string guid;

        // easy to recalculate: will not cache
        [NonSerialized] private bool m_pathLoaded;
        [NonSerialized] private string m_assetFolder;
        [NonSerialized] private string m_assetName;
        [NonSerialized] private string m_assetPath;
        [NonSerialized] private string m_extension;
        [NonSerialized] private bool m_inEditor;
        [NonSerialized] private bool m_inPlugins;
        [NonSerialized] private bool m_inResources;
        [NonSerialized] private bool m_inStreamingAsset;
        [NonSerialized] private bool m_isAssetFile;

        // Need to read FileInfo: soft-cache (always re-read when needed)
        [SerializeField] public FR2_AssetType type;
        [SerializeField] private string m_fileInfoHash;
        [SerializeField] private string m_assetbundle;
        [SerializeField] private string m_addressable;

        [SerializeField] private string m_atlas;
        [SerializeField] private long m_fileSize;

        [SerializeField] private int m_assetChangeTS;           // Realtime when asset changed (trigger by import asset operation)
        [SerializeField] private int m_fileInfoReadTS;          // Realtime when asset being read

        [SerializeField] private int m_fileWriteTS;     // file's lastModification (file content + meta)
        [SerializeField] private int m_cachefileWriteTS;    // file's lastModification at the time the content being read

        [SerializeField] internal int refreshStamp; // use to check if asset has been deleted (refreshStamp not updated)


        // Do not cache
        [NonSerialized] internal FR2_AssetState state;
        internal Dictionary<string, FR2_Asset> UsedByMap = new Dictionary<string, FR2_Asset>();
        internal HashSet<int> HashUsedByClassesIds = new HashSet<int>();
        [SerializeField] private List<Classes> UseGUIDsList = new List<Classes>();

        public FR2_Asset(string guid)
        {
            this.guid = guid;
            type = FR2_AssetType.UNKNOWN;
        }

        // ----------------------- PATH INFO ------------------------
        public void LoadPathInfo()
        {
            if (m_pathLoaded) return;
            m_pathLoaded = true;

            m_assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

#if FR2_DEBUG
			Debug.LogWarning("Refreshing ... " + loadInfoTS + ":" + AssetDatabase.GUIDToAssetPath(guid));
			if (!m_assetPath.StartsWith("Assets"))
			{
				Debug.Log("LoadAssetInfo: " + m_assetPath);
			}
#endif
            FR2_Unity.SplitPath(m_assetPath, out m_assetName, out m_extension, out m_assetFolder);

            if (m_assetFolder.StartsWith("Assets/"))
            {
                m_assetFolder = m_assetFolder.Substring(7);
            }
            else if (!FR2_Unity.StringStartsWith(m_assetPath, "Packages/", "Project Settings/", "Library/"))
            {
                m_assetFolder = "built-in/";
            }

            m_inEditor = m_assetPath.Contains("/Editor/") || m_assetPath.Contains("/Editor Default Resources/");
            m_inResources = m_assetPath.Contains("/Resources/");
            m_inStreamingAsset = m_assetPath.Contains("/StreamingAssets/");
            m_inPlugins = m_assetPath.Contains("/Plugins/");
            m_isAssetFile = m_assetPath.EndsWith(".asset", StringComparison.Ordinal);
        }

        public string assetName
        {
            get
            {
                LoadPathInfo();
                return m_assetName;
            }
        }

        public string assetPath
        {
            get
            {
                if (!string.IsNullOrEmpty(m_assetPath)) return m_assetPath;
                m_assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(m_assetPath))
                {
                    state = FR2_AssetState.MISSING;
                }
                return m_assetPath;
            }
        }

        public string parentFolderPath { get { LoadPathInfo(); return m_assetFolder; } }
        public string assetFolder { get { LoadPathInfo(); return m_assetFolder; } }
        public string extension { get { LoadPathInfo(); return m_extension; } }

        public bool inEditor { get { LoadPathInfo(); return m_inEditor; } }
        public bool inPlugins { get { LoadPathInfo(); return m_inPlugins; } }
        public bool inResources { get { LoadPathInfo(); return m_inResources; } }
        public bool inStreamingAsset { get { LoadPathInfo(); return m_inStreamingAsset; } }

        // ----------------------- TYPE INFO ------------------------

        internal bool IsFolder { get { return type == FR2_AssetType.FOLDER; } }
        internal bool IsScript { get { return type == FR2_AssetType.SCRIPT; } }
        internal bool IsMissing { get { return state == FR2_AssetState.MISSING; } }
        internal bool IsReferencable
        {
            get
            {
                return type == FR2_AssetType.REFERENCABLE ||
                       type == FR2_AssetType.SCENE;
            }
        }
        internal bool IsBinaryAsset
        {
            get
            {
                return type == FR2_AssetType.BINARY_ASSET ||
                       type == FR2_AssetType.MODEL ||
                       type == FR2_AssetType.TERRAIN;
            }
        }

        // ----------------------- PATH INFO ------------------------
        public bool fileInfoDirty { get { return (type == FR2_AssetType.UNKNOWN) || (m_fileInfoReadTS <= m_assetChangeTS); } }
        public bool fileContentDirty { get { return m_fileWriteTS != m_cachefileWriteTS; } }

        public bool isDirty
        {
            get { return fileInfoDirty || fileContentDirty; }
        }

        bool ExistOnDisk()
        {
            if (IsMissing) return false; // asset not exist - no need to check FileSystem!
            if (type == FR2_AssetType.FOLDER || type == FR2_AssetType.UNKNOWN)
            {
                if (Directory.Exists(m_assetPath))
                {
                    if (type == FR2_AssetType.UNKNOWN) type = FR2_AssetType.FOLDER;
                    return true;
                }

                if (type == FR2_AssetType.FOLDER) return false;
            }

            // must be file here
            if (!File.Exists(m_assetPath)) return false;

            if (type == FR2_AssetType.UNKNOWN) GuessAssetType();
            return true;
        }

        internal void LoadFileInfo()
        {
            if (!fileInfoDirty) return;
            if (string.IsNullOrEmpty(m_assetPath)) LoadPathInfo(); // always reload Path Info

            //Debug.Log("--> Read: " + assetPath + " --> " + m_fileInfoReadTS + "<" + m_assetChangeTS);
            m_fileInfoReadTS = FR2_Unity.Epoch(DateTime.Now);

            if (IsMissing)
            {
                Debug.LogWarning("Should never be here! - missing files can not trigger LoadFileInfo()");
                return;
            }

            if (!ExistOnDisk())
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            if (type == FR2_AssetType.FOLDER) return; // nothing to read

            var assetType = AssetDatabase.GetMainAssetTypeAtPath(m_assetPath);
            if (assetType == typeof(FR2_Cache)) return;

            var info = new FileInfo(m_assetPath);
            m_fileSize = info.Length;
            m_fileInfoHash = info.Length + info.Extension;
            m_addressable = FR2_Unity.GetAddressable(guid);
            //if (!string.IsNullOrEmpty(m_addressable)) Debug.LogWarning(guid + " --> " + m_addressable);
            m_assetbundle = AssetDatabase.GetImplicitAssetBundleName(m_assetPath);

            if (assetType == typeof(Texture2D))
            {
                var importer = AssetImporter.GetAtPath(m_assetPath);
                if (importer is TextureImporter)
                {
                    var tImporter = importer as TextureImporter;
                    if (tImporter.qualifiesForSpritePacking)
                    {
                        m_atlas = tImporter.spritePackingTag;
                    }
                }
            }

            // check if file content changed
            var metaInfo = new FileInfo(m_assetPath + ".meta");
            var assetTime = FR2_Unity.Epoch(info.LastWriteTime);
            var metaTime = FR2_Unity.Epoch(metaInfo.LastWriteTime);

            // update fileChangeTimeStamp
            m_fileWriteTS = Mathf.Max(metaTime, assetTime);
        }

        internal string fileInfoHash { get { LoadFileInfo(); return m_fileInfoHash; } }
        internal long fileSize { get { LoadFileInfo(); return m_fileSize; } }

        public string AtlasName { get { LoadFileInfo(); return m_atlas; } }
        public string AssetBundleName { get { LoadFileInfo(); return m_assetbundle; } }

        public string AddressableName { get { LoadFileInfo(); return m_addressable; } }









        public Dictionary<string, HashSet<int>> UseGUIDs
        {
            get
            {
                if (_UseGUIDs != null)
                {
                    return _UseGUIDs;
                }

                _UseGUIDs = new Dictionary<string, HashSet<int>>(UseGUIDsList.Count);
                for (var i = 0; i < UseGUIDsList.Count; i++)
                {
                    string guid = UseGUIDsList[i].guid;
                    if (_UseGUIDs.ContainsKey(guid))
                    {
                        for (var j = 0; j < UseGUIDsList[i].ids.Count; j++)
                        {
                            int val = UseGUIDsList[i].ids[j];
                            if (_UseGUIDs[guid].Contains(val))
                            {
                                continue;
                            }

                            _UseGUIDs[guid].Add(UseGUIDsList[i].ids[j]);
                        }
                    }
                    else
                    {
                        _UseGUIDs.Add(guid, new HashSet<int>(UseGUIDsList[i].ids));
                    }
                }

                return _UseGUIDs;
            }
        }

        // ------------------------------- GETTERS -----------------------------


        internal bool IsExcluded
        {
            get
            {
                if (excludeTS >= ignoreTS)
                {
                    return _isExcluded;
                }

                excludeTS = ignoreTS;
                _isExcluded = false;

                HashSet<string> h = FR2_Setting.IgnoreAsset;
                foreach (string item in FR2_Setting.IgnoreAsset)
                {
                    if (m_assetPath.StartsWith(item, false, CultureInfo.InvariantCulture))
                    {
                        _isExcluded = true;
                        break;
                    }
                }

                return _isExcluded;
            }
        }

        public void AddUsedBy(string guid, FR2_Asset asset)
        {
            if (UsedByMap.ContainsKey(guid))
            {
                return;
            }

            if (guid == this.guid)
            {
                //Debug.LogWarning("self used");
                return;
            }


            UsedByMap.Add(guid, asset);
            HashSet<int> output;
            if (HashUsedByClassesIds == null)
            {
                HashUsedByClassesIds = new HashSet<int>();
            }

            if (asset.UseGUIDs.TryGetValue(this.guid, out output))
            {
                foreach (int item in output)
                {
                    HashUsedByClassesIds.Add(item);
                }
            }

            // int classId = HashUseByClassesIds    
        }

        public int UsageCount()
        {
            return UsedByMap.Count;
        }

        public override string ToString()
        {
            return string.Format("FR2_Asset[{0}]", m_assetName);
        }

        //--------------------------------- STATIC ----------------------------

        internal static bool IsValidGUID(string guid)
        {
            return AssetDatabase.GUIDToAssetPath(guid) != FR2_Cache.CachePath; // just skip FR2_Cache asset
        }

        internal void MarkAsDirty(bool isMoved = true, bool force = false)
        {
            if (isMoved)
            {
                var newPath = AssetDatabase.GUIDToAssetPath(guid);
                if (newPath != m_assetPath)
                {
                    m_pathLoaded = false;
                    m_assetPath = newPath;
                }
            }

            state = FR2_AssetState.CACHE;
            m_assetChangeTS = FR2_Unity.Epoch(DateTime.Now); // re-read FileInfo
            if (force) m_cachefileWriteTS = 0;
        }

        // --------------------------------- APIs ------------------------------

        internal void GuessAssetType()
        {
            if (SCRIPT_EXTENSIONS.Contains(m_extension))
            {
                type = FR2_AssetType.SCRIPT;
            }
            else if (REFERENCABLE_EXTENSIONS.Contains(m_extension))
            {
                bool isUnity = m_extension == ".unity";
                type = isUnity ? FR2_AssetType.SCENE : FR2_AssetType.REFERENCABLE;

                if (m_extension == ".asset" || isUnity || m_extension == ".spriteatlas")
                {
                    var buffer = new byte[5];
                    FileStream stream = null;

                    try
                    {
                        stream = File.OpenRead(m_assetPath);
                        stream.Read(buffer, 0, 5);
                        stream.Close();
                    }
#if FR2_DEBUG
                    catch (Exception e)
                    {
                        Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + m_assetPath); 
#else
                    catch
                    {
#endif
                        if (stream != null) stream.Close();
                        state = FR2_AssetState.MISSING;
                        return;
                    }
                    finally
                    {
                        if (stream != null) stream.Close();
                    }

                    string str = string.Empty;
                    foreach (byte t in buffer)
                    {
                        str += (char)t;
                    }

                    if (str != "%YAML")
                    {
                        type = FR2_AssetType.BINARY_ASSET;
                    }
                }
            }
            else if (m_extension == ".fbx")
            {
                type = FR2_AssetType.MODEL;
            }
            else if (m_extension == ".dll")
            {
                type = FR2_AssetType.DLL;
            }
            else
            {
                type = FR2_AssetType.NON_READABLE;
            }
        }


        internal void LoadContent()
        {
            if (!fileContentDirty) return;
            m_cachefileWriteTS = m_fileWriteTS;

            if (IsMissing || type == FR2_AssetType.NON_READABLE)
            {
                return;
            }

            if (type == FR2_AssetType.DLL)
            {
#if FR2_DEBUG
            Debug.LogWarning("Parsing DLL not yet supportted ");
#endif
                return;
            }

            if (!ExistOnDisk())
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            ClearUseGUIDs();

            if (IsFolder)
            {
                LoadFolder();
            }
            else if (IsReferencable)
            {
                LoadYAML2();
            }
            else if (IsBinaryAsset)
            {
                LoadBinaryAsset();
            }
        }

        internal void AddUseGUID(string fguid, int fFileId = -1, bool checkExist = true)
        {
            // if (checkExist && UseGUIDs.ContainsKey(fguid)) return;
            if (!IsValidGUID(fguid))
            {
                return;
            }

            if (!UseGUIDs.ContainsKey(fguid))
            {
                UseGUIDsList.Add(new Classes
                {
                    guid = fguid,
                    ids = new List<int>()
                });
                UseGUIDs.Add(fguid, new HashSet<int>());
            }

            if (fFileId != -1)
            {
                if (UseGUIDs[fguid].Contains(fFileId))
                {
                    return;
                }

                UseGUIDs[fguid].Add(fFileId);
                Classes i = UseGUIDsList.FirstOrDefault(x => x.guid == fguid);
                if (i != null)
                {
                    i.ids.Add(fFileId);
                }
            }
        }

        // ----------------------------- STATIC  ---------------------------------------

        internal static int SortByExtension(FR2_Asset a1, FR2_Asset a2)
        {
            if (a1 == null)
            {
                return -1;
            }

            if (a2 == null)
            {
                return 1;
            }

            int result = string.Compare(a1.m_extension, a2.m_extension, StringComparison.Ordinal);
            return result == 0 ? string.Compare(a1.m_assetName, a2.m_assetName, StringComparison.Ordinal) : result;
        }

        internal static List<FR2_Asset> FindUsage(FR2_Asset asset)
        {
            if (asset == null)
            {
                return null;
            }

            List<FR2_Asset> refs = FR2_Cache.Api.FindAssets(asset.UseGUIDs.Keys.ToArray(), true);


            return refs;
        }

        internal static List<FR2_Asset> FindUsedBy(FR2_Asset asset)
        {
            return asset.UsedByMap.Values.ToList();
        }

        internal static List<string> FindUsageGUIDs(FR2_Asset asset, bool includeScriptSymbols)
        {
            var result = new HashSet<string>();
            if (asset == null)
            {
                Debug.LogWarning("Asset invalid : " + asset.m_assetName);
                return result.ToList();
            }

            // for (var i = 0;i < asset.UseGUIDs.Count; i++)
            // {
            // 	result.Add(asset.UseGUIDs[i]);
            // }
            foreach (KeyValuePair<string, HashSet<int>> item in asset.UseGUIDs)
            {
                result.Add(item.Key);
            }

            //if (!includeScriptSymbols) return result.ToList();

            //if (asset.ScriptUsage != null)
            //{
            //	for (var i = 0; i < asset.ScriptUsage.Count; i++)
            //	{
            //    	var symbolList = FR2_Cache.Api.FindAllSymbol(asset.ScriptUsage[i]);
            //    	if (symbolList.Contains(asset)) continue;

            //    	var symbol = symbolList[0];
            //    	if (symbol == null || result.Contains(symbol.guid)) continue;

            //    	result.Add(symbol.guid);
            //	}	
            //}

            return result.ToList();
        }

        internal static List<string> FindUsedByGUIDs(FR2_Asset asset)
        {
            return asset.UsedByMap.Keys.ToList();
        }

        internal float Draw(Rect r,
            bool highlight,
            bool drawPath = true,
            bool showFileSize = true,
            bool showABName = false,
            bool showAtlasName = false,
            bool showUsageIcon = true,
            IWindow window = null
        )
        {
            bool singleLine = r.height <= 18f;
            float rw = r.width;
            bool selected = FR2_Bookmark.Contains(guid);

            r.height = 16f;
            bool hasMouse = Event.current.type == EventType.MouseUp && r.Contains(Event.current.mousePosition);

            if (hasMouse && Event.current.button == 1)
            {
                var menu = new GenericMenu();
                if (m_extension == ".prefab")
                {
                    menu.AddItem(new GUIContent("Edit in Scene"), false, EditPrefab);
                }

                menu.AddItem(new GUIContent("Open"), false, Open);
                menu.AddItem(new GUIContent("Ping"), false, Ping);
                menu.AddItem(new GUIContent(guid), false, CopyGUID);
                //menu.AddItem(new GUIContent("Reload"), false, Reload);

                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Bookmark"), selected, AddToSelection);
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Copy path"), false, CopyAssetPath);
                menu.AddItem(new GUIContent("Copy full path"), false, CopyAssetPathFull);

                //if (IsScript)
                //{
                //    menu.AddSeparator(string.Empty);
                //    AddArray(menu, ScriptSymbols, "+ ", "Definitions", "No Definition", false);

                //    menu.AddSeparator(string.Empty);
                //    AddArray(menu, ScriptUsage, "-> ", "Depends", "No Dependency", true);
                //}

                menu.ShowAsContext();
                Event.current.Use();
            }

            if (IsMissing)
            {
                if (!singleLine)
                {
                    r.y += 16f;
                }

                if (Event.current.type != EventType.Repaint)
                {
                    return 0;
                }

                GUI.Label(r, "(missing) " + guid, EditorStyles.whiteBoldLabel);
                return 0;
            }

            Rect iconRect = GUI2.LeftRect(16f, ref r);
            if (Event.current.type == EventType.Repaint)
            {
                Texture icon = AssetDatabase.GetCachedIcon(m_assetPath);
                if (icon != null)
                {
                    GUI.DrawTexture(iconRect, icon);
                }
            }


            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Rect pingRect = FR2_Setting.PingRow ? new Rect(0, r.y, r.x + r.width, r.height) : iconRect;
                if (pingRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.control || Event.current.command)
                    {
                        if (selected)
                        {
                            RemoveFromSelection();
                        }
                        else
                        {
                            AddToSelection();
                        }

                        if (window != null)
                        {
                            window.Repaint();
                        }
                    }
                    else
                    {
                        Ping();
                    }


                    //Event.current.Use();
                }
            }

            if (Event.current.type != EventType.Repaint)
            {
                return 0;
            }

            if (UsedByMap != null && UsedByMap.Count > 0)
            {
                var str = new GUIContent(UsedByMap.Count.ToString());
                Rect countRect = iconRect;
                countRect.x -= 16f;
                countRect.xMin = -10f;
                GUI.Label(countRect, str, GUI2.miniLabelAlignRight);
            }

            float pathW = drawPath ? EditorStyles.miniLabel.CalcSize(new GUIContent(m_assetFolder)).x : 0;
            float nameW = EditorStyles.boldLabel.CalcSize(new GUIContent(m_assetName)).x;
            Color cc = FR2_Cache.Api.setting.SelectedColor;

            if (singleLine)
            {
                Rect lbRect = GUI2.LeftRect(pathW + nameW, ref r);

                if (selected)
                {
                    Color c1 = GUI.color;
                    GUI.color = cc;
                    GUI.DrawTexture(lbRect, EditorGUIUtility.whiteTexture);
                    GUI.color = c1;
                }

                if (drawPath)
                {
                    Color c2 = GUI.color;
                    GUI.color = new Color(c2.r, c2.g, c2.b, c2.a * 0.5f);
                    GUI.Label(GUI2.LeftRect(pathW, ref lbRect), m_assetFolder, EditorStyles.miniLabel);
                    GUI.color = c2;

                    lbRect.xMin -= 4f;
                    GUI.Label(lbRect, m_assetName, EditorStyles.boldLabel);
                }
                else
                {
                    GUI.Label(lbRect, m_assetName);
                }
            }
            else
            {
                if (drawPath)
                {
                    GUI.Label(new Rect(r.x, r.y + 16f, r.width, r.height), m_assetFolder, EditorStyles.miniLabel);
                }

                Rect lbRect = GUI2.LeftRect(nameW, ref r);
                if (selected)
                {
                    GUI2.Rect(lbRect, cc);
                }

                GUI.Label(lbRect, m_assetName, EditorStyles.boldLabel);
            }

            var rr = GUI2.RightRect(10f, ref r); //margin
            if (highlight)
            {
                rr.xMin += 2f;
                rr.width = 1f;
                GUI2.Rect(rr, GUI2.darkGreen);
            }

            Color c = GUI.color;
            GUI.color = new Color(c.r, c.g, c.b, c.a * 0.5f);

            if (showFileSize)
            {
                Rect fsRect = GUI2.RightRect(40f, ref r); // filesize label

                if (fileSizeText == null)
                {
                    fileSizeText = new GUIContent(FR2_Helper.GetfileSizeString(fileSize));
                }



                GUI.Label(fsRect, fileSizeText, GUI2.miniLabelAlignRight);
            }

            if (!string.IsNullOrEmpty(m_addressable))
            {
                Rect adRect = GUI2.RightRect(100f, ref r);
                GUI.Label(adRect, m_addressable, GUI2.miniLabelAlignRight);
            }



            if (showUsageIcon && HashUsedByClassesIds != null)
            {
                foreach (int item in HashUsedByClassesIds)
                {
                    if (!FR2_Unity.HashClassesNormal.ContainsKey(item))
                    {
                        continue;
                    }

                    string name = FR2_Unity.HashClassesNormal[item];
                    Type t = null;
                    if (!HashClasses.TryGetValue(item, out t))
                    {
                        t = FR2_Unity.GetType(name);
                        HashClasses.Add(item, t);
                    }

                    GUIContent content = null;
                    var isExisted = cacheImage.TryGetValue(name, out content);
                    if (content == null) content = new GUIContent(EditorGUIUtility.ObjectContent(null, t).image, name);

                    if (!isExisted)
                    {
                        cacheImage.Add(name, content);
                    }
                    else
                    {
                        cacheImage[name] = content;
                    }

                    if (content != null)
                    {
                        try
                        {
                            GUI.Label(GUI2.RightRect(15f, ref r), content, GUI2.miniLabelAlignRight);
                        }
#if !FR2_DEBUG
                        catch { }
#else
						catch (Exception e)
						{
							UnityEngine.Debug.LogWarning(e);
						}
#endif
                    }
                }
            }

            if (showAtlasName)
            {
                GUI2.RightRect(10f, ref r); //margin
                Rect abRect = GUI2.RightRect(120f, ref r); // filesize label
                if (!string.IsNullOrEmpty(m_atlas))
                {
                    GUI.Label(abRect, m_atlas, GUI2.miniLabelAlignRight);
                }
            }

            if (showABName)
            {
                GUI2.RightRect(10f, ref r); //margin
                Rect abRect = GUI2.RightRect(100f, ref r); // filesize label
                if (!string.IsNullOrEmpty(m_assetbundle))
                {
                    GUI.Label(abRect, m_assetbundle, GUI2.miniLabelAlignRight);
                }
            }

            if (true)
            {
                GUI2.RightRect(10f, ref r); //margin
                Rect abRect = GUI2.RightRect(100f, ref r); // filesize label
                if (!string.IsNullOrEmpty(m_addressable))
                {
                    GUI.Label(abRect, m_addressable, GUI2.miniLabelAlignRight);
                }
            }

            GUI.color = c;

            if (Event.current.type == EventType.Repaint)
            {
                return rw < pathW + nameW ? 32f : 18f;
            }

            return r.height;
        }



        internal GenericMenu AddArray(GenericMenu menu, List<string> list, string prefix, string title,
            string emptyTitle, bool showAsset, int max = 10)
        {
            //if (list.Count > 0)
            //{
            //    if (list.Count > max)
            //    {
            //        prefix = string.Format("{0} _{1}/", title, list.Count) + prefix;
            //    }

            //    //for (var i = 0; i < list.Count; i++)
            //    //{
            //    //    var def = list[i];
            //    //    var suffix = showAsset ? "/" + FR2_Cache.Api.FindSymbol(def).assetName : string.Empty;
            //    //    menu.AddItem(new GUIContent(prefix + def + suffix), false, () => OpenScript(def));
            //    //}
            //}
            //else
            {
                menu.AddItem(new GUIContent(emptyTitle), true, null);
            }

            return menu;
        }

        internal void CopyGUID()
        {
            EditorGUIUtility.systemCopyBuffer = guid;
            Debug.Log(guid);
        }

        internal void CopyName()
        {
            EditorGUIUtility.systemCopyBuffer = m_assetName;
            Debug.Log(m_assetName);
        }

        internal void CopyAssetPath()
        {
            EditorGUIUtility.systemCopyBuffer = m_assetPath;
            Debug.Log(m_assetPath);
        }

        internal void CopyAssetPathFull()
        {
            string fullName = new FileInfo(m_assetPath).FullName;
            EditorGUIUtility.systemCopyBuffer = fullName;
            Debug.Log(fullName);
        }

        internal void AddToSelection()
        {
            if (!FR2_Bookmark.Contains(guid))
            {
                FR2_Bookmark.Add(guid);
            }

            //var list = Selection.objects.ToList();
            //var obj = FR2_Unity.LoadAssetAtPath<Object>(assetPath);
            //if (!list.Contains(obj))
            //{
            //    list.Add(obj);
            //    Selection.objects = list.ToArray();
            //}
        }

        internal void RemoveFromSelection()
        {
            if (FR2_Bookmark.Contains(guid))
            {
                FR2_Bookmark.Remove(guid);
            }
        }

        internal void Ping()
        {
            EditorGUIUtility.PingObject(
                AssetDatabase.LoadAssetAtPath(m_assetPath, typeof(UnityObject))
            );
        }

        internal void Open()
        {
            AssetDatabase.OpenAsset(
                AssetDatabase.LoadAssetAtPath(m_assetPath, typeof(UnityObject))
            );
        }

        internal void EditPrefab()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath(m_assetPath, typeof(UnityObject));
            UnityObject.Instantiate(prefab);
        }

        //internal void OpenScript(string definition)
        //{
        //    var asset = FR2_Cache.Api.FindSymbol(definition);
        //    if (asset == null) return;

        //    EditorGUIUtility.PingObject(
        //        AssetDatabase.LoadAssetAtPath(asset.assetPath, typeof(Object))
        //        );
        //}

        // ----------------------------- SERIALIZED UTILS ---------------------------------------



        // ----------------------------- LOAD ASSETS ---------------------------------------

        internal void LoadGameObject(GameObject go)
        {
            Component[] compList = go.GetComponentsInChildren<Component>();
            for (var i = 0; i < compList.Length; i++)
            {
                LoadSerialized(compList[i]);
            }
        }

        internal void LoadSerialized(UnityObject target)
        {
            SerializedProperty[] props = FR2_Unity.xGetSerializedProperties(target, true);

            for (var i = 0; i < props.Length; i++)
            {
                if (props[i].propertyType != SerializedPropertyType.ObjectReference)
                {
                    continue;
                }

                UnityObject refObj = props[i].objectReferenceValue;
                if (refObj == null)
                {
                    continue;
                }

                string refGUID = AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(refObj)
                );

                //Debug.Log("Found Reference BinaryAsset <" + assetPath + "> : " + refGUID + ":" + refObj);
                AddUseGUID(refGUID);
            }
        }

        internal void LoadTerrainData(TerrainData terrain)
        {
#if UNITY_2018_3_OR_NEWER
            TerrainLayer[] arr0 = terrain.terrainLayers;
            for (var i = 0; i < arr0.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr0[i]);
                string refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }
#endif


            DetailPrototype[] arr = terrain.detailPrototypes;

            for (var i = 0; i < arr.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr[i].prototypeTexture);
                string refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }

            TreePrototype[] arr2 = terrain.treePrototypes;
            for (var i = 0; i < arr2.Length; i++)
            {
                string aPath = AssetDatabase.GetAssetPath(arr2[i].prefab);
                string refGUID = AssetDatabase.AssetPathToGUID(aPath);
                AddUseGUID(refGUID);
            }

            FR2_Unity.TerrainTextureData[] arr3 = FR2_Unity.GetTerrainTextureDatas(terrain);
            for (var i = 0; i < arr3.Length; i++)
            {
                FR2_Unity.TerrainTextureData texs = arr3[i];
                for (var k = 0; k < texs.textures.Length; k++)
                {
                    Texture2D tex = texs.textures[k];
                    if (tex == null)
                    {
                        continue;
                    }

                    string aPath = AssetDatabase.GetAssetPath(tex);
                    if (string.IsNullOrEmpty(aPath))
                    {
                        continue;
                    }

                    string refGUID = AssetDatabase.AssetPathToGUID(aPath);
                    if (string.IsNullOrEmpty(refGUID))
                    {
                        continue;
                    }

                    AddUseGUID(refGUID);
                }
            }
        }

        private void ClearUseGUIDs()
        {
#if FR2_DEBUG
		    Debug.Log("ClearUseGUIDs: " + assetPath);
#endif

            UseGUIDs.Clear();
            UseGUIDsList.Clear();
        }

        static int binaryLoaded;
        internal void LoadBinaryAsset()
        {
            ClearUseGUIDs();

            UnityObject assetData = AssetDatabase.LoadAssetAtPath(m_assetPath, typeof(UnityObject));
            if (assetData is GameObject)
            {
                type = FR2_AssetType.MODEL;
                LoadGameObject(assetData as GameObject);
            }
            else if (assetData is TerrainData)
            {
                type = FR2_AssetType.TERRAIN;
                LoadTerrainData(assetData as TerrainData);
            }
            else
            {
                LoadSerialized(assetData);
            }

#if FR2_DEBUG
			Debug.Log("LoadBinaryAsset :: " + assetData + ":" + type);
#endif

            assetData = null;

            if (binaryLoaded++ <= 30) return;
            binaryLoaded = 0;
            FR2_Unity.UnloadUnusedAssets();
        }

        internal void LoadYAML()
        {
            if (!File.Exists(m_assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }


            if (m_isAssetFile)
            {
                var s = AssetDatabase.LoadAssetAtPath<FR2_Cache>(m_assetPath);
                if (s != null)
                {
                    return;
                }
            }

            string text = string.Empty;
            try
            {
                text = File.ReadAllText(m_assetPath);
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
                return;
            }

#if FR2_DEBUG
	        Debug.Log("LoadYAML: " + assetPath);
#endif

            //if(assetPath.Contains("Myriad Pro - Bold SDF"))
            //{
            //    Debug.Log("no ne");
            //}
            // PERFORMANCE HOG!
            // var matches = Regex.Matches(text, @"\bguid: [a-f0-9]{32}\b");
            MatchCollection matches = Regex.Matches(text, @".*guid: [a-f0-9]{32}.*\n");

            foreach (Match match in matches)
            {
                Match guidMatch = Regex.Match(match.Value, @"\bguid: [a-f0-9]{32}\b");
                string refGUID = guidMatch.Value.Replace("guid: ", string.Empty);

                Match fileIdMatch = Regex.Match(match.Value, @"\bfileID: ([0-9]*).*");
                int id = -1;
                try
                {
                    id = int.Parse(fileIdMatch.Groups[1].Value) / 100000;
                }
                catch { }

                AddUseGUID(refGUID, id);
            }

            //var idx = text.IndexOf("guid: ");
            //var counter=0;
            //while (idx != -1)
            //{
            //	var guid = text.Substring(idx + 6, 32);
            //	if (UseGUIDs.Contains(guid)) continue;
            //	AddUseGUID(guid);

            //	idx += 39;
            //	if (idx > text.Length-40) break;

            //	//Debug.Log(assetName + ":" +  guid);
            //	idx = text.IndexOf("guid: ", idx + 39);
            //	if (counter++ > 100) break;
            //}

            //if (counter > 100){
            //	Debug.LogWarning("Never finish on " + assetName);
            //}
        }

        internal void LoadYAML2()
        {
            if (!File.Exists(m_assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            if (m_assetPath == "ProjectSettings/EditorBuildSettings.asset")
            {
                var listScenes = EditorBuildSettings.scenes;
                foreach (var scene in listScenes)
                {
                    if (!scene.enabled) continue;
                    var path = scene.path;
                    var guid = AssetDatabase.AssetPathToGUID(path);

                    AddUseGUID(guid, 0);

#if FR2_DEBUG
					Debug.Log("AddScene: " + path);
#endif
                }
            }

            // var text = string.Empty;
            try
            {
                using (var sr = new StreamReader(m_assetPath))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        int index = line.IndexOf("guid: ");
                        if (index < 0)
                        {
                            continue;
                        }

                        string refGUID = line.Substring(index + 6, 32);
                        int indexFileId = line.IndexOf("fileID: ");
                        int fileID = -1;
                        if (indexFileId >= 0)
                        {
                            indexFileId += 8;
                            string fileIDStr =
                                line.Substring(indexFileId, line.IndexOf(',', indexFileId) - indexFileId);
                            try
                            {
                                fileID = int.Parse(fileIDStr) / 100000;
                            }
                            catch { }
                        }

                        AddUseGUID(refGUID, fileID);
                    }
                }

#if FR2_DEBUG
	            if (UseGUIDsList.Count > 0)
	            {
	            	Debug.Log(assetPath + ":" + UseGUIDsList.Count);
	            }
#endif
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("Guess Asset Type error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
            }
        }

        internal void LoadFolder()
        {
            if (!Directory.Exists(m_assetPath))
            {
                state = FR2_AssetState.MISSING;
                return;
            }

            // do not analyse folders outside project
            if (!m_assetPath.StartsWith("Assets/")) return;


            try
            {
                string[] files = Directory.GetFiles(m_assetPath);
                string[] dirs = Directory.GetDirectories(m_assetPath);

                foreach (string f in files)
                {
                    if (f.EndsWith(".meta", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string fguid = AssetDatabase.AssetPathToGUID(f);
                    if (string.IsNullOrEmpty(fguid))
                    {
                        continue;
                    }

                    // AddUseGUID(fguid, true);
                    AddUseGUID(fguid);
                }

                foreach (string d in dirs)
                {
                    string fguid = AssetDatabase.AssetPathToGUID(d);
                    if (string.IsNullOrEmpty(fguid))
                    {
                        continue;
                    }

                    // AddUseGUID(fguid, true);
                    AddUseGUID(fguid);
                }
            }
#if FR2_DEBUG
            catch (Exception e)
            {
                Debug.LogWarning("LoadFolder() error :: " + e + "\n" + assetPath);
#else
            catch
            {
#endif
                state = FR2_AssetState.MISSING;
            }

            //Debug.Log("Load Folder :: " + assetName + ":" + type + ":" + UseGUIDs.Count);
        }



        // ----------------------------- REPLACE GUIDS ---------------------------------------

        internal bool ReplaceReference(string fromGUID, string toGUID, TerrainData terrain = null)
        {
            if (IsMissing)
            {
                return false;
            }

            if (IsReferencable)
            {
                string text = string.Empty;

                if (!File.Exists(m_assetPath))
                {
                    state = FR2_AssetState.MISSING;
                    return false;
                }

                try
                {
                    text = File.ReadAllText(m_assetPath).Replace("\r", "\n");
                    File.WriteAllText(m_assetPath, text.Replace(fromGUID, toGUID));
                    // AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
                    return true;
                }
                catch (Exception e)
                {
                    state = FR2_AssetState.MISSING;
                    //#if FR2_DEBUG
                    Debug.LogWarning("Replace Reference error :: " + e + "\n" + m_assetPath);
                    //#endif
                }

                return false;
            }

            if (type == FR2_AssetType.TERRAIN)
            {
                var fromObj = FR2_Unity.LoadAssetWithGUID<UnityObject>(fromGUID);
                var toObj = FR2_Unity.LoadAssetWithGUID<UnityObject>(toGUID);
                var found = 0;
                // var terrain = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)) as TerrainData;

                if (fromObj is Texture2D)
                {
                    DetailPrototype[] arr = terrain.detailPrototypes;
                    for (var i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].prototypeTexture == (Texture2D)fromObj)
                        {
                            found++;
                            arr[i].prototypeTexture = (Texture2D)toObj;
                        }
                    }

                    terrain.detailPrototypes = arr;
                    FR2_Unity.ReplaceTerrainTextureDatas(terrain, (Texture2D)fromObj, (Texture2D)toObj);
                }

                if (fromObj is GameObject)
                {
                    TreePrototype[] arr2 = terrain.treePrototypes;
                    for (var i = 0; i < arr2.Length; i++)
                    {
                        if (arr2[i].prefab == (GameObject)fromObj)
                        {
                            found++;
                            arr2[i].prefab = (GameObject)toObj;
                        }
                    }

                    terrain.treePrototypes = arr2;
                }

                // EditorUtility.SetDirty(terrain);
                // AssetDatabase.SaveAssets();

                fromObj = null;
                toObj = null;
                terrain = null;
                // FR2_Unity.UnloadUnusedAssets();

                return found > 0;
            }

            Debug.LogWarning("Something wrong, should never be here - Ignored <" + m_assetPath +
                             "> : not a readable type, can not replace ! " + type);
            return false;
        }

        internal bool ReplaceReference(string fromGUID, string toGUID, long toFileId, TerrainData terrain = null)
        {
            // Debug.Log("ReplaceReference: from " + fromGUID + "  to: " + toGUID + "  toFileId: " + toFileId);

            if (IsMissing)
            {
                //				Debug.Log("this asset is missing");
                return false;
            }

            if (IsReferencable)
            {
                if (!File.Exists(m_assetPath))
                {
                    state = FR2_AssetState.MISSING;
                    //					Debug.Log("this asset not exits");
                    return false;
                }

                try
                {
                    var sb = new StringBuilder();
                    string text = File.ReadAllText(assetPath).Replace("\r", "\n");
                    var lines = text.Split('\n');
                    //string result = "";
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        if (line.IndexOf(fromGUID, StringComparison.Ordinal) >= 0)
                        {
                            if (toFileId > 0)
                            {
                                const string FileID = "fileID: ";
                                var index = line.IndexOf(FileID, StringComparison.Ordinal);
                                if (index >= 0)
                                {
                                    var fromFileId = line.Substring(index + FileID.Length, line.IndexOf(',', index) - (index + FileID.Length));
                                    long fileType = 0;
                                    if (!long.TryParse(fromFileId, out fileType))
                                    {
                                        Debug.LogWarning("cannot parse file");
                                        return false;
                                    }
                                    if (fileType.ToString().Substring(0, 3) != toFileId.ToString().Substring(0, 3))
                                    {
                                        //difference file type
                                        Debug.LogWarning("Difference file type");
                                        return false;
                                    }

                                    Debug.Log("ReplaceReference: fromFileId " + fromFileId + "  to File Id " + toFileId);
                                    line = line.Replace(fromFileId, toFileId.ToString());
                                }
                            }
                            line = line.Replace(fromGUID, toGUID);
                        }
                        sb.Append(line);
                        sb.AppendLine();
                        //result += line + "\n";
                    }

                    //File.WriteAllText(assetPath, result.Trim());
                    File.WriteAllText(assetPath, sb.ToString());
                    //AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
                    return true;
                }
                catch (Exception e)
                {
                    state = FR2_AssetState.MISSING;
                    //#if FR2_DEBUG
                    Debug.LogWarning("Replace Reference error :: " + e + "\n" + m_assetPath);
                    //#endif
                }

                return false;
            }

            if (type == FR2_AssetType.TERRAIN)
            {
                var fromObj = FR2_Unity.LoadAssetWithGUID<UnityObject>(fromGUID);
                var toObj = FR2_Unity.LoadAssetWithGUID<UnityObject>(toGUID);
                var found = 0;
                // var terrain = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object)) as TerrainData;

                if (fromObj is Texture2D)
                {
                    DetailPrototype[] arr = terrain.detailPrototypes;
                    for (var i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].prototypeTexture == (Texture2D)fromObj)
                        {
                            found++;
                            arr[i].prototypeTexture = (Texture2D)toObj;
                        }
                    }

                    terrain.detailPrototypes = arr;
                    FR2_Unity.ReplaceTerrainTextureDatas(terrain, (Texture2D)fromObj, (Texture2D)toObj);
                }

                if (fromObj is GameObject)
                {
                    TreePrototype[] arr2 = terrain.treePrototypes;
                    for (var i = 0; i < arr2.Length; i++)
                    {
                        if (arr2[i].prefab == (GameObject)fromObj)
                        {
                            found++;
                            arr2[i].prefab = (GameObject)toObj;
                        }
                    }

                    terrain.treePrototypes = arr2;
                }

                // EditorUtility.SetDirty(terrain);
                // AssetDatabase.SaveAssets();

                // FR2_Unity.UnloadUnusedAssets();

                return found > 0;
            }

            Debug.LogWarning("Something wrong, should never be here - Ignored <" + m_assetPath +
                            "> : not a readable type, can not replace ! " + type);
            return false;
        }

        [Serializable]
        private class Classes
        {
            public string guid;
            public List<int> ids;
        }
    }
}