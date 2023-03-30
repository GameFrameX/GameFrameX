// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor.Tools
{
    /// <summary>[Editor-Only] [Pro-Only]
    /// A base <see cref="AnimancerToolsWindow.Tool"/> for modifying <see cref="Sprite"/>s.
    /// </summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools">Animancer Tools</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/SpriteModifierTool
    /// 
    [Serializable]
    public abstract class SpriteModifierTool : AnimancerToolsWindow.Tool
    {
        /************************************************************************************************************************/

        private static readonly List<Sprite> SelectedSprites = new List<Sprite>();
        private static bool _HasGatheredSprites;

        /// <summary>The currently selected <see cref="Sprite"/>s.</summary>
        public static List<Sprite> Sprites
        {
            get
            {
                if (!_HasGatheredSprites)
                {
                    _HasGatheredSprites = true;
                    GatherSelectedSprites(SelectedSprites);
                }

                return SelectedSprites;
            }
        }

        /// <inheritdoc/>
        public override void OnSelectionChanged()
        {
            _HasGatheredSprites = false;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoBodyGUI()
        {
#if !UNITY_2D_SPRITE
            EditorGUILayout.HelpBox(
                "This tool works best with Unity's '2D Sprite' package." +
                " You should import it via the Package Manager before using this tool.",
                MessageType.Warning);

            if (AnimancerGUI.TryUseClickEventInLastRect())
                EditorApplication.ExecuteMenuItem("Window/Package Manager");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds all <see cref="Sprite"/>s in the <see cref="Selection.objects"/> or their sub-assets to the
        /// list of `sprites`.
        /// </summary>
        public static void GatherSelectedSprites(List<Sprite> sprites)
        {
            sprites.Clear();

            var selection = Selection.objects;
            for (int i = 0; i < selection.Length; i++)
            {
                var selected = selection[i];
                if (selected is Sprite sprite)
                {
                    sprites.Add(sprite);
                }
                else if (selected is Texture2D texture)
                {
                    sprites.AddRange(LoadAllSpritesInTexture(texture));
                }
            }

            sprites.Sort(NaturalCompare);
        }

        /************************************************************************************************************************/

        /// <summary>Returns all the <see cref="Sprite"/> sub-assets of the `texture`.</summary>
        public static Sprite[] LoadAllSpritesInTexture(Texture2D texture)
            => LoadAllSpritesAtPath(AssetDatabase.GetAssetPath(texture));

        /// <summary>Returns all the <see cref="Sprite"/> assets at the `path`.</summary>
        public static Sprite[] LoadAllSpritesAtPath(string path)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            var sprites = new List<Sprite>();
            for (int j = 0; j < assets.Length; j++)
            {
                if (assets[j] is Sprite sprite)
                    sprites.Add(sprite);
            }
            return sprites.ToArray();
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="EditorUtility.NaturalCompare"/> on the <see cref="Object.name"/>s.</summary>
        public static int NaturalCompare(Object a, Object b) => EditorUtility.NaturalCompare(a.name, b.name);

        /************************************************************************************************************************/

        /// <summary>The message to confirm that the user is certain they want to apply the changes.</summary>
        protected virtual string AreYouSure => "Are you sure you want to modify these Sprites?";

        /// <summary>Called immediately after the user confirms they want to apply changes.</summary>
        protected virtual void PrepareToApply() { }

        /// <summary>Applies the desired modifications to the `data` before it is saved.</summary>
        protected virtual void Modify(SpriteDataEditor data, int index, Sprite sprite) { }

        /// <summary>Applies the desired modifications to the `data` before it is saved.</summary>
        protected virtual void Modify(TextureImporter importer, List<Sprite> sprites)
        {
            var dataEditor = new SpriteDataEditor(importer);

            var hasError = false;

            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];
                var index = dataEditor.IndexOf(sprite);
                if (index < 0)
                    continue;

                Modify(dataEditor, index, sprite);
                sprites.RemoveAt(i--);

                if (!dataEditor.ValidateBounds(index, sprite))
                    hasError = true;
            }

            if (!hasError)
                dataEditor.Apply();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Asks the user if they want to modify the target <see cref="Sprite"/>s and calls <see cref="Modify"/>
        /// on each of them before saving any changes.
        /// </summary>
        protected void AskAndApply()
        {
            if (!EditorUtility.DisplayDialog("Are You Sure?",
                AreYouSure + "\n\nThis operation cannot be undone.",
                "Modify", "Cancel"))
                return;

            PrepareToApply();

            var pathToSprites = new Dictionary<string, List<Sprite>>();
            var sprites = Sprites;
            for (int i = 0; i < sprites.Count; i++)
            {
                var sprite = sprites[i];

                var path = AssetDatabase.GetAssetPath(sprite);

                if (!pathToSprites.TryGetValue(path, out var spritesAtPath))
                    pathToSprites.Add(path, spritesAtPath = new List<Sprite>());

                spritesAtPath.Add(sprite);
            }

            foreach (var asset in pathToSprites)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(asset.Key);

                Modify(importer, asset.Value);

                if (asset.Value.Count > 0)
                {
                    var message = ObjectPool.AcquireStringBuilder()
                        .Append("Modification failed: unable to find data in '")
                        .Append(asset.Key)
                        .Append("' for ")
                        .Append(asset.Value.Count)
                        .Append(" Sprites:");

                    for (int i = 0; i < sprites.Count; i++)
                    {
                        message.AppendLine()
                            .Append(" - ")
                            .Append(sprites[i].name);
                    }

                    Debug.LogError(message.ReleaseToString(), AssetDatabase.LoadAssetAtPath<Object>(asset.Key));
                }
            }
        }

        /************************************************************************************************************************/
    }
}

#endif

