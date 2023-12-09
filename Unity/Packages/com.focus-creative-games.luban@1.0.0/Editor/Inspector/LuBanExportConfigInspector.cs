using UnityEditor;
using UnityEngine;

namespace Luban.Editor
{
    [CustomEditor(typeof(LubanExportConfig))]
    public class LuBanExportConfigInspector : UnityEditor.Editor
    {
        private SerializedProperty m_input_data_dir;
        private SerializedProperty m_output_data_dir;
        private SerializedProperty m_tpl_path;
        private SerializedProperty m_which_dll;
        private SerializedProperty m_output_code_dir;
        private SerializedProperty m_output_data_resources_list_file;
        private SerializedProperty m_output_exclude_tags;
        private SerializedProperty m_output_data_file_extension;
        private SerializedProperty m_output_data_compact_json;
        private SerializedProperty m_output_data_json_monolithic_file;
        private SerializedProperty m_preview_command;
        private SerializedProperty m_before_gen;
        private SerializedProperty m_after_gen;
        private SerializedProperty m_job;
        private SerializedProperty m_define_xml;
        private SerializedProperty m_gen_types;
        private SerializedProperty m_service;
        private SerializedProperty m_i10n_timezone;
        private SerializedProperty m_i10n_input_text_files;
        private SerializedProperty m_i10n_text_field_name;
        private SerializedProperty m_i10n_output_not_translated_text_file;
        private SerializedProperty m_i10n_path;
        private SerializedProperty m_i10n_patch_input_data_dir;
        private SerializedProperty m_naming_convertion_module;
        private SerializedProperty m_naming_convertion_bean_member;
        private SerializedProperty m_naming_convertion_enum_member;
        private SerializedProperty m_external_selectors;

        private void OnEnable()
        {
            m_before_gen = serializedObject.FindProperty("before_gen");
            m_after_gen = serializedObject.FindProperty("after_gen");
            m_which_dll = serializedObject.FindProperty("which_dll");
            m_tpl_path = serializedObject.FindProperty("tpl_path");
            m_job = serializedObject.FindProperty("job");
            m_define_xml = serializedObject.FindProperty("define_xml");
            m_gen_types = serializedObject.FindProperty("gen_types");
            m_service = serializedObject.FindProperty("service");

            m_input_data_dir = serializedObject.FindProperty("input_data_dir");

            m_output_data_dir = serializedObject.FindProperty("output_data_dir");
            m_output_code_dir = serializedObject.FindProperty("output_code_dir");
            m_output_data_resources_list_file = serializedObject.FindProperty("output_data_resources_list_file");
            m_output_exclude_tags = serializedObject.FindProperty("output_exclude_tags");
            m_output_data_file_extension = serializedObject.FindProperty("output_data_file_extension");
            m_output_data_compact_json = serializedObject.FindProperty("output_data_compact_json");
            m_output_data_json_monolithic_file = serializedObject.FindProperty("output_data_json_monolithic_file");

            m_i10n_timezone = serializedObject.FindProperty("i10n_timezone");
            m_i10n_input_text_files = serializedObject.FindProperty("i10n_input_text_files");
            m_i10n_text_field_name = serializedObject.FindProperty("i10n_text_field_name");
            m_i10n_output_not_translated_text_file = serializedObject.FindProperty("i10n_output_not_translated_text_file");
            m_i10n_path = serializedObject.FindProperty("i10n_path");
            m_i10n_patch_input_data_dir = serializedObject.FindProperty("i10n_patch_input_data_dir");

            m_naming_convertion_module = serializedObject.FindProperty("naming_convertion_module");
            m_naming_convertion_bean_member = serializedObject.FindProperty("naming_convertion_bean_member");
            m_naming_convertion_enum_member = serializedObject.FindProperty("naming_convertion_enum_member");

            m_external_selectors = serializedObject.FindProperty("external_selectors");


            m_preview_command = serializedObject.FindProperty("preview_command");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var config = (LubanExportConfig)target;

            EditorGUILayout.PropertyField(m_before_gen, new GUIContent("前置命令"));
            EditorGUILayout.PropertyField(m_after_gen, new GUIContent("后置命令"));

            EditorGUILayout.PropertyField(m_which_dll, new GUIContent("Client&Server Dll"));
            EditorGUILayout.PropertyField(m_tpl_path, new GUIContent("模板路径"));
            EditorGUILayout.PropertyField(m_job, new GUIContent("任务"));
            EditorGUILayout.PropertyField(m_define_xml, new GUIContent("定义XML"));
            EditorGUILayout.PropertyField(m_gen_types, new GUIContent("生成类型"));
            EditorGUILayout.PropertyField(m_service, new GUIContent("服务类型"));

            EditorGUILayout.PropertyField(m_input_data_dir, new GUIContent("输入数据文件夹"));

            EditorGUILayout.PropertyField(m_output_data_dir, new GUIContent("输出数据文件夹"));
            EditorGUILayout.PropertyField(m_output_code_dir, new GUIContent("输出代码文件夹"));
            EditorGUILayout.PropertyField(m_output_data_resources_list_file, new GUIContent("输出数据资源列表文件"));
            EditorGUILayout.PropertyField(m_output_exclude_tags, new GUIContent("输出排除标签"));
            EditorGUILayout.PropertyField(m_output_data_file_extension, new GUIContent("输出数据文件扩展名"));
            EditorGUILayout.PropertyField(m_output_data_compact_json, new GUIContent("输出数据压缩JSON"));
            EditorGUILayout.PropertyField(m_output_data_json_monolithic_file, new GUIContent("输出数据JSON独立文件"));


            EditorGUILayout.PropertyField(m_i10n_timezone, new GUIContent("多语言时区"));
            EditorGUILayout.PropertyField(m_i10n_input_text_files, new GUIContent("多语言文本文件"));
            EditorGUILayout.PropertyField(m_i10n_text_field_name, new GUIContent("多语言文本字段名"));
            EditorGUILayout.PropertyField(m_i10n_output_not_translated_text_file, new GUIContent("未翻译文本存放位置"));
            EditorGUILayout.PropertyField(m_i10n_path, new GUIContent("多语言文件夹"));
            EditorGUILayout.PropertyField(m_i10n_patch_input_data_dir, new GUIContent("多语言补丁文件夹"));

            EditorGUILayout.PropertyField(m_naming_convertion_module, new GUIContent("Bean 命名风格"));
            EditorGUILayout.PropertyField(m_naming_convertion_bean_member, new GUIContent("Bean 命名风格"));
            EditorGUILayout.PropertyField(m_naming_convertion_enum_member, new GUIContent("Enum 命名风格"));

            EditorGUILayout.PropertyField(m_external_selectors, new GUIContent("外部选择器"));

            EditorGUILayout.PropertyField(m_preview_command, new GUIContent("预览命令"));

            if (GUILayout.Button("生成"))
            {
                config.Gen();
            }

            if (GUILayout.Button("预览命令"))
            {
                config.Preview();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}