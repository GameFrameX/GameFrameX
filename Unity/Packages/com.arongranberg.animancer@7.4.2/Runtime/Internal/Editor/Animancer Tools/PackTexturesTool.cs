// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Tools
{
    /// <summary>[Editor-Only] [Pro-Only] 
    /// A <see cref="AnimancerToolsWindow.Tool"/> for packing multiple <see cref="Texture2D"/>s into a single image.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools/pack-textures">Pack Textures</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/PackTexturesTool
    /// 
    [Serializable]
    public class PackTexturesTool : AnimancerToolsWindow.Tool
    {
        /************************************************************************************************************************/

        [SerializeField] private List<Object> _AssetsToPack;
        [SerializeField] private int _Padding;
        [SerializeField] private int _MaximumSize = 8192;

        [NonSerialized] private ReorderableList _TexturesDisplay;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int DisplayOrder => 0;

        /// <inheritdoc/>
        public override string Name => "Pack Textures";

        /// <inheritdoc/>
        public override string HelpURL => Strings.DocsURLs.PackTextures;

        /// <inheritdoc/>
        public override string Instructions
        {
            get
            {
                if (_AssetsToPack.Count == 0)
                    return "Add the texture, sprites, and folders you want to pack to the list.";

                return "Set the other details then click Pack and it will ask where you want to save the combined texture.";
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnEnable(int index)
        {
            base.OnEnable(index);
            if (_AssetsToPack == null)
                _AssetsToPack = new List<Object>();
            _TexturesDisplay = AnimancerToolsWindow.CreateReorderableObjectList(_AssetsToPack, "Textures", true);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoBodyGUI()
        {
            GUILayout.BeginVertical();
            _TexturesDisplay.DoLayoutList();
            GUILayout.EndVertical();
            HandleDragAndDropIntoList(GUILayoutUtility.GetLastRect(), _AssetsToPack, overwrite: false);
            RemoveDuplicates(_AssetsToPack);

            AnimancerToolsWindow.BeginChangeCheck();
            var padding = EditorGUILayout.IntField("Padding", _Padding);
            AnimancerToolsWindow.EndChangeCheck(ref _Padding, padding);

            AnimancerToolsWindow.BeginChangeCheck();
            var maximumSize = EditorGUILayout.IntField("Maximum Size", _MaximumSize);
            maximumSize = Math.Max(maximumSize, 16);
            AnimancerToolsWindow.EndChangeCheck(ref _MaximumSize, maximumSize);

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                GUI.enabled = _AssetsToPack.Count > 0;

                if (GUILayout.Button("Clear"))
                {
                    AnimancerGUI.Deselect();
                    AnimancerToolsWindow.RecordUndo();
                    _AssetsToPack.Clear();
                }

                if (GUILayout.Button("Pack"))
                {
                    AnimancerGUI.Deselect();
                    Pack();
                }
            }
            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        /// <summary>Removes any items from the `list` that are the same as earlier items.</summary>
        private static void RemoveDuplicates<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var item = list[i];
                if (item == null)
                    continue;

                for (int j = 0; j < i; j++)
                {
                    if (item.Equals(list[j]))
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Combines the <see cref="_AssetsToPack"/> into a new texture and saves it.</summary>
        private void Pack()
        {
            var textures = GatherTextures();
            if (textures.Count == 0 ||
                !MakeTexturesReadable(textures))
                return;

            var path = GetCommonDirectory(_AssetsToPack);

            path = EditorUtility.SaveFilePanelInProject("Save Packed Texture", "PackedTexture", "png",
                "Where would you like to save the packed texture?", path);

            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                const string ProgressTitle = "Packing Textures";
                EditorUtility.DisplayProgressBar(ProgressTitle, "Gathering", 0);

                var tightSprites = GatherTightSprites();

                EditorUtility.DisplayProgressBar(ProgressTitle, "Packing", 0.1f);

                var packedTexture = new Texture2D(0, 0, TextureFormat.ARGB32, false);

                var tightTextures = new Texture2D[tightSprites.Count];
                var index = 0;
                foreach (var sprite in tightSprites)
                    tightTextures[index++] = sprite.texture;

                var packedUVs = packedTexture.PackTextures(tightTextures, _Padding, _MaximumSize);

                EditorUtility.DisplayProgressBar(ProgressTitle, "Encoding", 0.4f);
                var bytes = packedTexture.EncodeToPNG();
                if (bytes == null)
                    return;

                EditorUtility.DisplayProgressBar(ProgressTitle, "Writing", 0.5f);
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();

                var importer = GetTextureImporter(path);
                importer.maxTextureSize = Math.Max(packedTexture.width, packedTexture.height);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;

                var data = new SpriteDataEditor(importer)
                {
                    SpriteCount = 0
                };

                CopyCompressionSettings(importer, textures);
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                // Use the UV coordinates to set up sprites for the new texture.
                EditorUtility.DisplayProgressBar(ProgressTitle, "Generating Sprites", 0.7f);

                data.SpriteCount = tightSprites.Count;
                index = 0;
                foreach (var sprite in tightSprites)
                {
                    var rect = packedUVs[index];
                    rect.x *= packedTexture.width;
                    rect.y *= packedTexture.height;
                    rect.width *= packedTexture.width;
                    rect.height *= packedTexture.height;

                    var spriteRect = rect;
                    spriteRect.x += spriteRect.width * sprite.rect.x / sprite.texture.width;
                    spriteRect.y += spriteRect.height * sprite.rect.y / sprite.texture.height;
                    spriteRect.width *= sprite.rect.width / sprite.texture.width;
                    spriteRect.height *= sprite.rect.height / sprite.texture.height;

                    var pivot = sprite.pivot;
                    pivot.x /= rect.width;
                    pivot.y /= rect.height;

                    data.SetName(index, sprite.name);
                    data.SetRect(index, spriteRect);
                    data.SetAlignment(index, GetAlignment(sprite.pivot));
                    data.SetPivot(index, pivot);
                    data.SetBorder(index, sprite.border);

                    index++;
                }

                EditorUtility.DisplayProgressBar(ProgressTitle, "Saving", 0.9f);

                data.Apply();

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();

                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /************************************************************************************************************************/

        private HashSet<Texture2D> GatherTextures()
        {
            var textures = new HashSet<Texture2D>();

            for (int i = 0; i < _AssetsToPack.Count; i++)
            {
                var obj = _AssetsToPack[i];
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (obj is Texture2D texture)
                    textures.Add(texture);
                else if (obj is Sprite sprite)
                    textures.Add(sprite.texture);
                else if (obj is DefaultAsset)
                    ForEachTextureInFolder(path, tex => textures.Add(tex));
            }

            return textures;
        }

        /************************************************************************************************************************/

        private HashSet<Sprite> GatherTightSprites()
        {
            var sprites = new HashSet<Sprite>();

            for (int i = 0; i < _AssetsToPack.Count; i++)
            {
                var obj = _AssetsToPack[i];
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (obj is Texture2D texture)
                    GatherTightSprites(sprites, texture);
                else if (obj is Sprite sprite)
                    sprites.Add(CreateTightSprite(sprite));
                else if (obj is DefaultAsset)
                    ForEachTextureInFolder(path, tex => GatherTightSprites(sprites, tex));
            }

            return sprites;
        }

        /************************************************************************************************************************/

        private static void GatherTightSprites(ICollection<Sprite> sprites, Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var foundSprite = false;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                {
                    sprite = CreateTightSprite(sprite);
                    sprites.Add(sprite);
                    foundSprite = true;
                }
            }

            if (!foundSprite)
            {
                var sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                sprite.name = texture.name;
                sprites.Add(sprite);
            }
        }

        /************************************************************************************************************************/

        private static Sprite CreateTightSprite(Sprite sprite)
        {
            var rect = sprite.rect;
            var width = Mathf.CeilToInt(rect.width);
            var height = Mathf.CeilToInt(rect.height);
            if (width == sprite.texture.width &&
                height == sprite.texture.height)
                return sprite;

            var pixels = sprite.texture.GetPixels(
                Mathf.FloorToInt(rect.x),
                Mathf.FloorToInt(rect.y),
                width,
                height);

            var texture = new Texture2D(width, height, sprite.texture.format, false, true);
            texture.SetPixels(pixels);
            texture.Apply();

            rect.x = 0;
            rect.y = 0;

            var pivot = sprite.pivot;
            pivot.x /= rect.width;
            pivot.y /= rect.height;

            var newSprite = Sprite.Create(texture, rect, pivot, sprite.pixelsPerUnit);
            newSprite.name = sprite.name;
            return newSprite;
        }

        /************************************************************************************************************************/

        private static bool MakeTexturesReadable(HashSet<Texture2D> textures)
        {
            var hasAsked = false;

            foreach (var texture in textures)
            {
                var importer = GetTextureImporter(texture);
                if (importer == null)
                    continue;

                if (importer.isReadable &&
                    importer.textureCompression == TextureImporterCompression.Uncompressed)
                    continue;

                if (!hasAsked)
                {
                    if (!EditorUtility.DisplayDialog("Make Textures Readable and Uncompressed?",
                        "This tool requires the source textures to be marked as readable and uncompressed in their import settings.",
                        "Make Textures Readable and Uncompressed", "Cancel"))
                        return false;
                    hasAsked = true;
                }

                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return true;
        }

        /************************************************************************************************************************/

        private static void ForEachTextureInFolder(string path, Action<Texture2D> action)
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(Texture2D)}", new string[] { path });
            for (int i = 0; i < guids.Length; i++)
            {
                path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                    action(texture);
            }
        }

        /************************************************************************************************************************/

        private static void CopyCompressionSettings(TextureImporter copyTo, IEnumerable<Texture2D> copyFrom)
        {
            var first = true;
            foreach (var texture in copyFrom)
            {
                var copyFromImporter = GetTextureImporter(texture);
                if (copyFromImporter == null)
                    continue;

                if (first)
                {
                    first = false;

                    copyTo.textureCompression = copyFromImporter.textureCompression;
                    copyTo.crunchedCompression = copyFromImporter.crunchedCompression;
                    copyTo.compressionQuality = copyFromImporter.compressionQuality;
                    copyTo.filterMode = copyFromImporter.filterMode;
                }
                else
                {
                    if (IsHigherQuality(copyFromImporter.textureCompression, copyTo.textureCompression))
                        copyTo.textureCompression = copyFromImporter.textureCompression;

                    if (copyFromImporter.crunchedCompression)
                        copyTo.crunchedCompression = true;

                    if (copyTo.compressionQuality < copyFromImporter.compressionQuality)
                        copyTo.compressionQuality = copyFromImporter.compressionQuality;

                    if (copyTo.filterMode > copyFromImporter.filterMode)
                        copyTo.filterMode = copyFromImporter.filterMode;
                }
            }
        }

        /************************************************************************************************************************/

        private static bool IsHigherQuality(TextureImporterCompression higher, TextureImporterCompression lower)
        {
            switch (higher)
            {
                case TextureImporterCompression.Uncompressed:
                    return lower != TextureImporterCompression.Uncompressed;

                case TextureImporterCompression.Compressed:
                    return lower == TextureImporterCompression.CompressedLQ;

                case TextureImporterCompression.CompressedHQ:
                    return
                        lower == TextureImporterCompression.Compressed ||
                        lower == TextureImporterCompression.CompressedLQ;

                case TextureImporterCompression.CompressedLQ:
                    return false;

                default:
                    throw AnimancerUtilities.CreateUnsupportedArgumentException(higher);
            }
        }

        /************************************************************************************************************************/

        private static string GetCommonDirectory<T>(IList<T> objects) where T : Object
        {
            if (objects == null)
                return null;

            var count = objects.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                if (objects[i] == null)
                {
                    objects.RemoveAt(i);
                    count--;
                }
            }

            if (count == 0)
                return null;

            var path = AssetDatabase.GetAssetPath(objects[0]);
            path = Path.GetDirectoryName(path);

            for (int i = 1; i < count; i++)
            {
                var otherPath = AssetDatabase.GetAssetPath(objects[i]);
                otherPath = Path.GetDirectoryName(otherPath);

                while (string.Compare(path, 0, otherPath, 0, path.Length) != 0)
                {
                    path = Path.GetDirectoryName(path);
                }
            }

            return path;
        }

        /************************************************************************************************************************/

        private static SpriteAlignment GetAlignment(Vector2 pivot)
        {
            switch (pivot.x)
            {
                case 0:
                    switch (pivot.y)
                    {
                        case 0: return SpriteAlignment.BottomLeft;
                        case 0.5f: return SpriteAlignment.BottomCenter;
                        case 1: return SpriteAlignment.BottomRight;
                    }
                    break;
                case 0.5f:
                    switch (pivot.y)
                    {
                        case 0: return SpriteAlignment.LeftCenter;
                        case 0.5f: return SpriteAlignment.Center;
                        case 1: return SpriteAlignment.RightCenter;
                    }
                    break;
                case 1:
                    switch (pivot.y)
                    {
                        case 0: return SpriteAlignment.TopLeft;
                        case 0.5f: return SpriteAlignment.TopCenter;
                        case 1: return SpriteAlignment.TopRight;
                    }
                    break;
            }

            return SpriteAlignment.Custom;
        }

        /************************************************************************************************************************/

        private static TextureImporter GetTextureImporter(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return null;

            return GetTextureImporter(path);
        }

        private static TextureImporter GetTextureImporter(string path)
            => AssetImporter.GetAtPath(path) as TextureImporter;

        /************************************************************************************************************************/
    }
}

#endif

