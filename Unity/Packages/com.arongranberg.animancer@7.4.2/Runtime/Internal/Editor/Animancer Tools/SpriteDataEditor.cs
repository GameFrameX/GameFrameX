// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

#if UNITY_2D_SPRITE
using UnityEditor.U2D.Sprites;
#else
#pragma warning disable CS0618 // Type or member is obsolete.
#endif

namespace Animancer.Editor.Tools
{
    /// <summary>A wrapper around the '2D Sprite' package features for editing Sprite data.</summary>
    public class SpriteDataEditor
    {
        /************************************************************************************************************************/
#if UNITY_2D_SPRITE
        /************************************************************************************************************************/

        private static SpriteDataProviderFactories _Factories;

        private static SpriteDataProviderFactories Factories
        {
            get
            {
                if (_Factories == null)
                {
                    _Factories = new SpriteDataProviderFactories();
                    _Factories.Init();
                }

                return _Factories;
            }
        }

        /************************************************************************************************************************/

        private readonly ISpriteEditorDataProvider Provider;
        private SpriteRect[] _SpriteRects;

        /************************************************************************************************************************/

        /// <summary>The number of sprites in the target data.</summary>
        public int SpriteCount
        {
            get => _SpriteRects.Length;
            set => Array.Resize(ref _SpriteRects, value);
        }

        /// <summary>Returns the name of the sprite at the specified `index`.</summary>
        public string GetName(int index) => _SpriteRects[index].name;

        /// <summary>Sets the name of the sprite at the specified `index`.</summary>
        public void SetName(int index, string name) => _SpriteRects[index].name = name;

        /// <summary>Returns the rect of the sprite at the specified `index`.</summary>
        public Rect GetRect(int index) => _SpriteRects[index].rect;

        /// <summary>Sets the rect of the sprite at the specified `index`.</summary>
        public void SetRect(int index, Rect rect) => _SpriteRects[index].rect = rect;

        /// <summary>Returns the pivot of the sprite at the specified `index`.</summary>
        public Vector2 GetPivot(int index) => _SpriteRects[index].pivot;

        /// <summary>Sets the pivot of the sprite at the specified `index`.</summary>
        public void SetPivot(int index, Vector2 pivot) => _SpriteRects[index].pivot = pivot;

        /// <summary>Returns the alignment of the sprite at the specified `index`.</summary>
        public SpriteAlignment GetAlignment(int index) => _SpriteRects[index].alignment;

        /// <summary>Sets the alignment of the sprite at the specified `index`.</summary>
        public void SetAlignment(int index, SpriteAlignment alignment) => _SpriteRects[index].alignment = alignment;

        /// <summary>Returns the border of the sprite at the specified `index`.</summary>
        public Vector4 GetBorder(int index) => _SpriteRects[index].border;

        /// <summary>Sets the border of the sprite at the specified `index`.</summary>
        public void SetBorder(int index, Vector4 border) => _SpriteRects[index].border = border;

        /************************************************************************************************************************/
#else
        /************************************************************************************************************************/

        private SpriteMetaData[] _SpriteSheet;

        /************************************************************************************************************************/

        /// <summary>The number of sprites in the target data.</summary>
        public int SpriteCount
        {
            get => _SpriteSheet.Length;
            set => Array.Resize(ref _SpriteSheet, value);
        }

        /// <summary>Returns the name of the sprite at the specified `index`.</summary>
        public string GetName(int index) => _SpriteSheet[index].name;

        /// <summary>Sets the name of the sprite at the specified `index`.</summary>
        public void SetName(int index, string name) => _SpriteSheet[index].name = name;

        /// <summary>Returns the rect of the sprite at the specified `index`.</summary>
        public Rect GetRect(int index) => _SpriteSheet[index].rect;

        /// <summary>Sets the rect of the sprite at the specified `index`.</summary>
        public void SetRect(int index, Rect rect) => _SpriteSheet[index].rect = rect;

        /// <summary>Returns the pivot of the sprite at the specified `index`.</summary>
        public Vector2 GetPivot(int index) => _SpriteSheet[index].pivot;

        /// <summary>Sets the pivot of the sprite at the specified `index`.</summary>
        public void SetPivot(int index, Vector2 pivot) => _SpriteSheet[index].pivot = pivot;

        /// <summary>Returns the alignment of the sprite at the specified `index`.</summary>
        public SpriteAlignment GetAlignment(int index) => (SpriteAlignment)_SpriteSheet[index].alignment;

        /// <summary>Sets the alignment of the sprite at the specified `index`.</summary>
        public void SetAlignment(int index, SpriteAlignment alignment) => _SpriteSheet[index].alignment = (int)alignment;

        /// <summary>Returns the border of the sprite at the specified `index`.</summary>
        public Vector4 GetBorder(int index) => _SpriteSheet[index].border;

        /// <summary>Sets the border of the sprite at the specified `index`.</summary>
        public void SetBorder(int index, Vector4 border) => _SpriteSheet[index].border = border;

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        private readonly TextureImporter Importer;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SpriteDataEditor"/>.</summary>
        public SpriteDataEditor(TextureImporter importer)
        {
            Importer = importer;

#if UNITY_2D_SPRITE
            Provider = Factories.GetSpriteEditorDataProviderFromObject(importer);
            Provider.InitSpriteEditorDataProvider();

            _SpriteRects = Provider.GetSpriteRects();
#else
            _SpriteSheet = importer.spritesheet;
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Tries to find the index of the data matching the `sprite`.</summary>
        /// <remarks>
        /// Returns -1 if there is no data matching the <see cref="UnityEngine.Object.name"/>.
        /// <para></para>
        /// Returns -2 if there is more than one data matching the <see cref="UnityEngine.Object.name"/> but no
        /// <see cref="Sprite.rect"/> match.
        /// </remarks>
        public int IndexOf(Sprite sprite)
        {
            var nameMatchIndex = -1;

            var count = SpriteCount;
            for (int i = 0; i < count; i++)
            {
                if (GetName(i) == sprite.name)
                {
                    if (GetRect(i) == sprite.rect)
                        return i;

                    if (nameMatchIndex == -1)// First name match.
                        nameMatchIndex = i;
                    else
                        nameMatchIndex = -2;// Already found 2 name matches.
                }
            }

            if (nameMatchIndex == -1)
            {
                Debug.LogError($"No {nameof(SpriteMetaData)} for '{sprite.name}' was found.", sprite);
            }
            else if (nameMatchIndex == -2)
            {
                Debug.LogError($"More than one {nameof(SpriteMetaData)} for '{sprite.name}' was found" +
                    $" but none of them matched the {nameof(Sprite)}.{nameof(Sprite.rect)}." +
                    $" If the texture's Max Size is smaller than its actual size, increase the Max Size before performing this" +
                    $" operation so that the {nameof(Rect)}s can be used to identify the correct data.", sprite);
            }

            return nameMatchIndex;
        }

        /************************************************************************************************************************/

        /// <summary>Logs an error and returns false if the data at the specified `index` is out of the texture bounds.</summary>
        public bool ValidateBounds(int index, Sprite sprite)
        {
            var rect = GetRect(index);
            var widthScale = rect.width / sprite.rect.width;
            var heightScale = rect.height / sprite.rect.height;
            if (rect.xMin < 0 ||
                rect.yMin < 0 ||
                rect.xMax > sprite.texture.width * widthScale ||
                rect.yMax > sprite.texture.height * heightScale)
            {
                var path = AssetDatabase.GetAssetPath(sprite);
                Debug.LogError($"This modification would have put '{sprite.name}' out of bounds" +
                    $" so '{path}' was not modified.", sprite);

                return false;
            }

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Applies any modifications to the target asset.</summary>
        public void Apply()
        {
#if UNITY_2D_SPRITE
            Provider.SetSpriteRects(_SpriteRects);
            Provider.Apply();
#else
            Importer.spritesheet = _SpriteSheet;
            EditorUtility.SetDirty(Importer);
#endif

            Importer.SaveAndReimport();
        }

        /************************************************************************************************************************/
    }
}

#endif

