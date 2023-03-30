// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2023 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Animancer.Editor.Tools
{
    /// <summary>[Editor-Only] [Pro-Only] A <see cref="SpriteModifierTool"/> for bulk-renaming <see cref="Sprite"/>s.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/tools/rename-sprites">Rename Sprites</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor.Tools/RenameSpritesTool
    /// 
    [Serializable]
    public class RenameSpritesTool : SpriteModifierTool
    {
        /************************************************************************************************************************/

        [NonSerialized] private List<string> _Names;
        [NonSerialized] private bool _NamesAreDirty;
        [NonSerialized] private ReorderableList _SpritesDisplay;
        [NonSerialized] private ReorderableList _NamesDisplay;

        [SerializeField] private string _NewName = "";
        [SerializeField] private int _MinimumDigits;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override int DisplayOrder => 2;

        /// <inheritdoc/>
        public override string Name => "Rename Sprites";

        /// <inheritdoc/>
        public override string HelpURL => Strings.DocsURLs.RenameSprites;

        /// <inheritdoc/>
        public override string Instructions
        {
            get
            {
                if (Sprites.Count == 0)
                    return "Select the Sprites you want to rename.";

                return "Enter the new name(s) you want to give the Sprites then click Apply.";
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnEnable(int index)
        {
            base.OnEnable(index);
            _Names = new List<string>();
            _SpritesDisplay = AnimancerToolsWindow.CreateReorderableObjectList(Sprites, "Sprites");
            _NamesDisplay = AnimancerToolsWindow.CreateReorderableStringList(_Names, "Names");
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void OnSelectionChanged()
        {
            base.OnSelectionChanged();
            _NamesAreDirty = true;
        }

        /************************************************************************************************************************/

        /// <summary>Refreshes the <see cref="_Names"/>.</summary>
        private void UpdateNames()
        {
            if (!_NamesAreDirty)
                return;

            _NamesAreDirty = false;

            var sprites = Sprites;
            AnimancerEditorUtilities.SetCount(_Names, sprites.Count);

            if (string.IsNullOrEmpty(_NewName))
            {
                for (int i = 0; i < sprites.Count; i++)
                    _Names[i] = sprites[i].name;
            }
            else
            {
                var digits = Mathf.FloorToInt(Mathf.Log10(_Names.Count)) + 1;
                if (digits < _MinimumDigits)
                    digits = _MinimumDigits;

                var formatCharacters = new char[digits];
                for (int i = 0; i < digits; i++)
                    formatCharacters[i] = '0';
                var format = new string(formatCharacters);

                for (int i = 0; i < _Names.Count; i++)
                    _Names[i] = _NewName + (i + 1).ToString(format);
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void DoBodyGUI()
        {
            base.DoBodyGUI();

            EditorGUILayout.HelpBox(ReferencesLostMessage, MessageType.Warning);

            AnimancerToolsWindow.BeginChangeCheck();
            var newName = EditorGUILayout.TextField("New Name", _NewName);
            if (AnimancerToolsWindow.EndChangeCheck(ref _NewName, newName))
                _NamesAreDirty = true;

            AnimancerToolsWindow.BeginChangeCheck();
            var digits = EditorGUILayout.IntField("Minimum Digits", _MinimumDigits);
            if (AnimancerToolsWindow.EndChangeCheck(ref _MinimumDigits, Mathf.Max(digits, 1)))
                _NamesAreDirty = true;

            UpdateNames();

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                _SpritesDisplay.DoLayoutList();
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                _NamesDisplay.DoLayoutList();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                GUI.enabled = _NewName.Length > 0;

                if (GUILayout.Button("Clear"))
                {
                    AnimancerGUI.Deselect();
                    AnimancerToolsWindow.RecordUndo();
                    _NewName = "";
                    _NamesAreDirty = true;
                }

                GUI.enabled = _SpritesDisplay.list.Count > 0;

                if (GUILayout.Button("Apply"))
                {
                    AnimancerGUI.Deselect();
                    AskAndApply();
                }
            }
            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        // We could prevent it from causing animations to lose their data by using ISpriteEditorDataProvider
        // instead of TextureImporter, but it's in the 2D Sprite package which Animancer does not otherwise require.

        private const string ReferencesLostMessage =
            "Any references to the renamed Sprites will be lost (including animations that use them)" +
            " but you can use the 'Remap Sprite Animations' tool to reassign them afterwards.";

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override string AreYouSure =>
            "Are you sure you want to rename these Sprites?" +
            "\n\n" + ReferencesLostMessage;

        /************************************************************************************************************************/

        private static Dictionary<Sprite, string> _SpriteToName;

        /// <inheritdoc/>
        protected override void PrepareToApply()
        {
            if (_SpriteToName == null)
                _SpriteToName = new Dictionary<Sprite, string>();
            else
                _SpriteToName.Clear();

            var sprites = Sprites;
            for (int i = 0; i < sprites.Count; i++)
            {
                _SpriteToName.Add(sprites[i], _Names[i]);
            }

            // Renaming selected Sprites will lose the selection without triggering OnSelectionChanged.
            EditorApplication.delayCall += OnSelectionChanged;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void Modify(SpriteDataEditor data, int index, Sprite sprite)
        {
            data.SetName(index, _SpriteToName[sprite]);
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void Modify(TextureImporter importer, List<Sprite> sprites)
        {
            if (sprites.Count == 1 && importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                var sprite = sprites[0];
                var fileName = Path.GetFileNameWithoutExtension(importer.assetPath);
                if (fileName == sprite.name)
                {
                    AssetDatabase.RenameAsset(importer.assetPath, _SpriteToName[sprite]);
                    sprites.Clear();
                }
            }

            base.Modify(importer, sprites);
        }

        /************************************************************************************************************************/
    }
}

#endif

