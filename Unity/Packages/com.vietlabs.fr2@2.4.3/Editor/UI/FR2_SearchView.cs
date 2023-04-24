using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    public class FR2_SearchView
    {
        private bool caseSensitive;
        private string searchTerm = string.Empty;

        public static GUIStyle toolbarSearchField;
        public static GUIStyle toolbarSearchFieldCancelButton;
        public static GUIStyle toolbarSearchFieldCancelButtonEmpty;

        public static void InitSearchStyle()
        {
            toolbarSearchField = "ToolbarSeachTextFieldPopup";
            toolbarSearchFieldCancelButton = "ToolbarSeachCancelButton";
            toolbarSearchFieldCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
        }

        public bool DrawLayout()
        {
            bool dirty = false;

            if (toolbarSearchField == null)
            {
                InitSearchStyle();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                bool v = GUILayout.Toggle(caseSensitive, "Aa", EditorStyles.toolbarButton, GUILayout.Width(24f));
                if (v != caseSensitive)
                {
                    caseSensitive = v;
                    dirty = true;
                }

                GUILayout.Space(2f);
                string value = GUILayout.TextField(searchTerm, toolbarSearchField, GUILayout.Width(140f));
                if (searchTerm != value)
                {
                    searchTerm = value;
                    dirty = true;
                }

                GUIStyle style = string.IsNullOrEmpty(searchTerm)
                    ? toolbarSearchFieldCancelButtonEmpty
                    : toolbarSearchFieldCancelButton;
                if (GUILayout.Button("Cancel", style))
                {
                    searchTerm = string.Empty;
                    dirty = true;
                }
            }
            GUILayout.EndHorizontal();

            return dirty;
        }
    }
}