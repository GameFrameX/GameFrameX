using System;
using GameFrameX.Runtime;
using Hotfix.Manager;
using Hotfix.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.HotFix.Editor
{
    /// <summary>
    /// 四子棋客户端资源构建脚本。
    /// 用代码生成 UIConnectFourBattlePanel.prefab，并在 UIRoomListPanel.prefab 上克隆出创建四子棋房间按钮。
    /// 仅供批处理调用：Unity -batchmode -quit -executeMethod Unity.HotFix.Editor.ConnectFourPrefabBuilder.BuildAll
    /// </summary>
    public static class ConnectFourPrefabBuilder
    {
        private const string BattlePrefabDirectory = "Assets/Bundles/UI/UIConnectFour";
        private const string BattlePrefabPath = BattlePrefabDirectory + "/UIConnectFourBattlePanel.prefab";
        private const string RoomListPrefabPath = "Assets/Bundles/UI/UIRoom/UIRoomListPanel.prefab";

        private const int Columns = ConnectFourDataManager.ColumnCount;
        private const int Rows = ConnectFourDataManager.RowCount;

        // 与 UIConnectFourBattlePanel.Binder.UGUI 中的取色保持一致。
        private static readonly Color EmptyCellColor = new Color32(46, 56, 92, 255);

        private static Font _font;
        private static Sprite _uiSprite;
        private static Sprite _backgroundSprite;
        private static Sprite _knobSprite;

        public static void BuildAll()
        {
            try
            {
                BuildBattlePrefab();
                PatchRoomListPrefab();
                AssetDatabase.SaveAssets();
                Debug.Log("[ConnectFourPrefabBuilder] 四子棋 UI 资源构建完成");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[ConnectFourPrefabBuilder] 四子棋 UI 资源构建失败: " + e);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// 构建 UIConnectFourBattlePanel.prefab：遮罩、标题区、7×6 棋盘、7 个落子按钮与底部操作区。
        /// </summary>
        private static void BuildBattlePrefab()
        {
            LoadBuiltinResources();

            if (!AssetDatabase.IsValidFolder(BattlePrefabDirectory))
            {
                AssetDatabase.CreateFolder("Assets/Bundles/UI", "UIConnectFour");
            }

            var root = new GameObject("UIConnectFourBattlePanel");
            var rootRect = root.AddComponent<RectTransform>();
            StretchToParent(rootRect);

            var mask = CreateImage("mask", root.transform, _uiSprite, new Color(0f, 0f, 0f, 0.392f));
            StretchToParent(GetRect(mask));
            AddButton(mask, null, 0);

            var windowBg = CreateImage("window_bg", root.transform, _uiSprite, new Color(0.22f, 0.22f, 0.22f, 1f));
            SetCentered(GetRect(windowBg), Vector2.zero, new Vector2(720f, 1000f));

            CreateText("title", root.transform, "四子棋对战", 32, new Vector2(0f, 462f), new Vector2(520f, 44f), Color.white);
            CreateText("room_state_text", root.transform, "房间状态", 20, new Vector2(0f, 424f), new Vector2(660f, 30f), new Color(0.85f, 0.88f, 0.95f, 1f));
            CreateText("turn_text", root.transform, "等待开局", 26, new Vector2(0f, 386f), new Vector2(660f, 36f), new Color(0.98f, 0.85f, 0.45f, 1f));

            // 顶部 7 个落子按钮，与棋盘 7 列对齐。
            for (var column = 0; column < Columns; column++)
            {
                var x = -228f + column * 72f;
                var buttonGo = CreateImage("column_button_" + column, root.transform, _uiSprite, new Color(0.33f, 0.62f, 0.35f, 1f));
                SetCentered(GetRect(buttonGo), new Vector2(x, 340f), new Vector2(64f, 44f));
                AddButton(buttonGo, (column + 1).ToString(), 24);
            }

            // 7×6 棋盘容器。显示行第 0 行为顶行。
            var board = CreateImage("board_grid", root.transform, _backgroundSprite, new Color(0.09f, 0.11f, 0.2f, 1f));
            SetCentered(GetRect(board), new Vector2(0f, -14f), new Vector2(544f, 472f));

            for (var displayRow = 0; displayRow < Rows; displayRow++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    var displayIndex = displayRow * Columns + column;
                    var x = -228f + column * 72f;
                    var y = 192f - displayRow * 72f;
                    var cell = CreateImage("cell_" + displayIndex, board.transform, _knobSprite, EmptyCellColor);
                    SetCentered(GetRect(cell), new Vector2(x, y), new Vector2(64f, 64f));
                }
            }

            CreateText("result_text", root.transform, "等待玩家加入", 26, new Vector2(0f, -308f), new Vector2(660f, 36f), Color.white);

            CreateButton("leave_button", root.transform, "退出房间", 22, new Vector2(-170f, -380f), new Vector2(150f, 52f), new Color(0.62f, 0.28f, 0.24f, 1f));
            CreateButton("start_button", root.transform, "开始游戏", 22, new Vector2(0f, -380f), new Vector2(150f, 52f), new Color(0.33f, 0.62f, 0.35f, 1f));
            CreateButton("restart_button", root.transform, "再来一局", 22, new Vector2(170f, -380f), new Vector2(150f, 52f), new Color(0.26f, 0.49f, 0.76f, 1f));
            CreateButton("close_button", root.transform, "×", 30, new Vector2(320f, 462f), new Vector2(48f, 48f), new Color(0.62f, 0.28f, 0.24f, 1f));

            SetLayerRecursive(root.transform, LayerMask.NameToLayer("UI"));

            // 挂载界面组件并回填 SerializeField 引用。
            var panel = root.AddComponent<UIConnectFourBattlePanel>();
            BindSerializedFields(panel, root.transform);

            PrefabUtility.SaveAsPrefabAsset(root, BattlePrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            Debug.Log("[ConnectFourPrefabBuilder] 已生成 " + BattlePrefabPath);
        }

        /// <summary>
        /// 在 UIRoomListPanel.prefab 上克隆 create_button，生成创建四子棋房间入口。
        /// </summary>
        private static void PatchRoomListPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(RoomListPrefabPath);
            try
            {
                var existing = root.transform.Find("create_connectfour_button");
                if (existing != null)
                {
                    UnityEngine.Object.DestroyImmediate(existing.gameObject);
                }

                var createButton = root.transform.Find("create_button");
                if (createButton == null)
                {
                    throw new Exception("UIRoomListPanel.prefab 中未找到 create_button");
                }

                var clone = UnityEngine.Object.Instantiate(createButton.gameObject);
                clone.name = "create_connectfour_button";
                var cloneRect = clone.GetComponent<RectTransform>();
                cloneRect.SetParent(root.transform, false);
                cloneRect.SetSiblingIndex(createButton.GetSiblingIndex() + 1);
                cloneRect.anchoredPosition = new Vector2(274f, 296f);
                cloneRect.sizeDelta = new Vector2(190f, 52f);

                var image = clone.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.26f, 0.49f, 0.76f, 1f);
                }

                var label = clone.transform.Find("title_text");
                if (label != null)
                {
                    var labelText = label.GetComponent<Text>();
                    if (labelText != null)
                    {
                        labelText.text = "创建四子棋房间";
                        labelText.fontSize = 22;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, RoomListPrefabPath);
                Debug.Log("[ConnectFourPrefabBuilder] 已为 UIRoomListPanel.prefab 增加 create_connectfour_button");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BindSerializedFields(UIConnectFourBattlePanel panel, Transform root)
        {
            var so = new SerializedObject(panel);
            so.FindProperty("mask").objectReferenceValue = FindComponent<Image>(root, "mask");
            so.FindProperty("window_bg").objectReferenceValue = FindComponent<Image>(root, "window_bg");
            so.FindProperty("title").objectReferenceValue = FindComponent<Text>(root, "title");
            so.FindProperty("room_state_text").objectReferenceValue = FindComponent<Text>(root, "room_state_text");
            so.FindProperty("turn_text").objectReferenceValue = FindComponent<Text>(root, "turn_text");
            so.FindProperty("board_grid").objectReferenceValue = FindComponent<Image>(root, "board_grid");
            for (var i = 0; i < Columns * Rows; i++)
            {
                so.FindProperty("cell_" + i).objectReferenceValue = FindComponent<Image>(root, "cell_" + i);
            }

            for (var i = 0; i < Columns; i++)
            {
                so.FindProperty("column_button_" + i).objectReferenceValue = FindComponent<Button>(root, "column_button_" + i);
            }

            so.FindProperty("result_text").objectReferenceValue = FindComponent<Text>(root, "result_text");
            so.FindProperty("start_button").objectReferenceValue = FindComponent<Button>(root, "start_button");
            so.FindProperty("restart_button").objectReferenceValue = FindComponent<Button>(root, "restart_button");
            so.FindProperty("leave_button").objectReferenceValue = FindComponent<Button>(root, "leave_button");
            so.FindProperty("close_button").objectReferenceValue = FindComponent<Button>(root, "close_button");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T FindComponent<T>(Transform root, string name) where T : Component
        {
            var target = root.FindChildName(name);
            return target == null ? null : target.GetComponent<T>();
        }

        private static void LoadBuiltinResources()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            _backgroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            _knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        private static RectTransform GetRect(GameObject go)
        {
            return go.GetComponent<RectTransform>();
        }

        private static GameObject CreateNode(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = CreateNode(name, parent);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = Image.Type.Sliced;
            return go;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, Vector2 position, Vector2 size, Color color)
        {
            var go = CreateNode(name, parent);
            SetCentered(GetRect(go), position, size);
            var text = go.AddComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, int labelFontSize, Vector2 position, Vector2 size, Color color)
        {
            var go = CreateImage(name, parent, _uiSprite, color);
            SetCentered(GetRect(go), position, size);
            AddButton(go, label, labelFontSize);
            return go;
        }

        private static Button AddButton(GameObject go, string label, int labelFontSize)
        {
            var button = go.AddComponent<Button>();
            var image = go.GetComponent<Image>();
            if (image != null)
            {
                button.targetGraphic = image;
            }

            if (label != null)
            {
                CreateText("title_text", go.transform, label, labelFontSize, Vector2.zero, Vector2.zero, Color.white);
                var labelRect = GetRect(go.transform.Find("title_text").gameObject);
                StretchToParent(labelRect);
            }

            return button;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetCentered(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetLayerRecursive(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            for (var i = 0; i < transform.childCount; i++)
            {
                SetLayerRecursive(transform.GetChild(i), layer);
            }
        }
    }
}
