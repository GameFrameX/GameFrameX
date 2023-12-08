using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
// using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

// using FilePathAttribute = Sirenix.OdinInspector.FilePathAttribute;

namespace Luban.Editor
{
    [CreateAssetMenu(fileName = "Luban", menuName = "Luban/ExportConfig")]
    public class LubanExportConfig : ScriptableObject
    {
        #region 生命周期

        // [LabelText("生成前")] [BeforeGenSelector] [BoxGroup("生命周期")]
        [HideInInspector]
        public string before_gen;

        // [LabelText("生成后")] [AfterGenSelector] [BoxGroup("生命周期")]
        [HideInInspector]
        public string after_gen;

        #endregion

        #region 必要参数

        // [Required] [LabelText("Client&Server Dll")] [FilePath(Extensions = "dll", RequireExistingPath = true)] [BoxGroup("必要参数")]
        [HideInInspector]
        public string which_dll = "../Tools/Luban.ClientServer/Luban.ClientServer.dll";

        // [Command("-t", false)] [FolderPath(RequireExistingPath = true)] [LabelText("模板文件夹")] [BoxGroup("必要参数")]
        [HideInInspector]
        public string tpl_path;

        [Command("-j")]
        // [BoxGroup("必要参数")]
        [HideInInspector]
        public string job = "cfg --";

        // [Required]
        [Command("-d")]
        // [FilePath(Extensions = "xml")] [OnValueChanged(nameof(_OnDefineChange))] [LabelText("Root.xml")] [BoxGroup("必要参数")]
        [HideInInspector]
        public string define_xml;

        // [Required] 
        [Command("--input_data_dir")]
        // [LabelText("数据文件夹")] [FolderPath] [BoxGroup("必要参数")]
        [HideInInspector]
        public string input_data_dir;

        // [Required] 
        [Command("--gen_types")]
        // [LabelText("生成类型")] [BoxGroup("必要参数")]
        [HideInInspector]
        public GenTypes gen_types;

        // [Required] 
        // [LabelText("生成类型")]
        [Command("-s")] [Tooltip("一般为 server, client 等")]
        // [BoxGroup("必要参数")] [ShowIf("@System.IO.File.Exists(define_xml)")] [ValueDropdown(nameof(service_dropdown))]
        [HideInInspector]
        public string service;

        /*private ValueDropdownList<string> service_dropdown
        {
            get
            {
                if (_service_dropdown != null)
                {
                    return _service_dropdown;
                }

                _service_dropdown = new ValueDropdownList<string>();

                if (!File.Exists(define_xml))
                {
                    return _service_dropdown;
                }

                var doc = new XmlDocument();
                doc.Load(define_xml);

                XmlNodeList nodes = doc.DocumentElement.SelectNodes("service");

                if (nodes is null || nodes.Count <= 0)
                {
                    return _service_dropdown;
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    string name = nodes[i].Attributes["name"].Value.Trim();

                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    _service_dropdown.Add(name);
                }

                return _service_dropdown;
            }
        }

        private ValueDropdownList<string> _service_dropdown;

        private void _OnDefineChange()
        {
            if (!File.Exists(define_xml))
            {
                return;
            }

            _service_dropdown = null;
        }*/

        #endregion

        #region 输出配置

        [Command("--output_data_dir")]
        // [LabelText("配置文件夹")] [FolderPath] [FoldoutGroup("输出配置")]
        [HideInInspector]
        public string output_data_dir;

        [Command("--output_code_dir")]
        // [LabelText("代码文件夹")] [FolderPath] [FoldoutGroup("输出配置")]
        [HideInInspector]
        public string output_code_dir;

        [Command("--output:data:resource_list_file")]
        // [LabelText("资源引用存放文件")] [FilePath] [FoldoutGroup("输出配置")]
        [HideInInspector]
        public string output_data_resources_list_file;

        [Command("--output:exclude_tags")]
        // [LabelText("排除哪些 Tag")] [FoldoutGroup("输出配置")]
        [HideInInspector]
        public string output_exclude_tags;

        [Command("--output:data:file_extension")]
        // [LabelText("自定义配置后缀")] [FoldoutGroup("输出配置")]
        [HideInInspector]
        public string output_data_file_extension;

        [Command("--output:data:compact_json")]
        // [LabelText("紧凑 Json")] [ShowIf("@_CheckGenType(\"json\")")] [FoldoutGroup("输出配置")]
        [HideInInspector]
        public bool output_data_compact_json;

        [Command("--output:data:json_monolithic_file")]
        // [LabelText("Monolithic Json")] [ShowIf("@_CheckGenType(\"monolithic\")")] [FoldoutGroup("输出配置")]
        [HideInInspector]
        public string output_data_json_monolithic_file;

        private bool _CheckGenType(string which_key_word)
        {
            foreach (GenTypes type in Enum.GetValues(typeof(GenTypes)))
            {
                if (!type.ToString().Contains(which_key_word))
                {
                    continue;
                }

                if (!gen_types.HasFlag(type))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        #endregion

        #region I10N

        [Command("--l10n:timezone")]
        // [LabelText("多语言时区")] [FoldoutGroup("I10N")]
        [HideInInspector]
        public string i10n_timezone;

        [Command("--l10n:input_text_files")]
        // [LabelText("多语言文件(','分割)")] [FoldoutGroup("I10N")]
        [HideInInspector]
        public string i10n_input_text_files;

        [Command("--l10n:text_field_name")]
        // [LabelText("默认 Text 属性名")] [FoldoutGroup("I10N")]
        [HideInInspector]
        public string i10n_text_field_name;

        [Command("--l10n:output_not_translated_text_file")]
        // [LabelText("未翻译存放位置")] [FilePath] [FoldoutGroup("I10N")]
        [HideInInspector]
        public string i10n_output_not_translated_text_file;

        [Command("--l10n:patch")]
        // [LabelText("多语言补丁")] [FoldoutGroup("I10N")]
        [HideInInspector]
        public string i10n_path;

        [Command("--l10n:patch_input_data_dir")]
        // [LabelText("多语言补丁文件夹")] [FolderPath] [FoldoutGroup("I10N")] [ShowIf("@!string.IsNullOrEmpty(i10n_path)")]
        [HideInInspector]
        public string i10n_patch_input_data_dir;

        #endregion

        #region 其他

        [Command("--naming_convention:module")]
        // [LabelText("Module 命名风格")] [FoldoutGroup("其他配置")]
        [HideInInspector]
        public NamingConvertion naming_convertion_module = NamingConvertion.PascalCase;

        [Command("--naming_convention:bean_member")]
        // [LabelText("Bean 命名风格")] [FoldoutGroup("其他配置")]
        [HideInInspector]
        public NamingConvertion naming_convertion_bean_member = NamingConvertion.PascalCase;

        [Command("--naming_convention:enum_member")]
        // [LabelText("Enum 命名风格")] [FoldoutGroup("其他配置")]
        [HideInInspector]
        public NamingConvertion naming_convertion_enum_member = NamingConvertion.PascalCase;

        [Command("--cs:use_unity_vector")]
        // [LabelText("使用 Unity Vector")] [FoldoutGroup("其他配置")]
        [HideInInspector]
        public bool cs_use_unity_vestor;

        [Command("--external:selectors")]
        // [LabelText("外部选择器")] [FoldoutGroup("其他配置")]
        [HideInInspector]
        public string external_selectors;


        [TextArea(5, 15)]
        // [LabelText("预览命令")] [ShowInInspector] [NonSerialized]
        [HideInInspector]
        public string preview_command;

        #endregion

        // [Button("生成")]
        public void Gen()
        {
            Preview();
            GenUtils.Gen(_GetCommand(), before_gen, after_gen);
        }

        private void Generator(bool isServer)
        {
            if (isServer)
            {
                service = "server";
                gen_types = GenTypes.code_cs_dotnet_json | GenTypes.data_json;
                output_code_dir = "../Server/Server.Config/Config";
                output_data_dir = "../Server/Server.Config/Json";
            }
            else
            {
                service = "client";
                gen_types = GenTypes.code_cs_unity_json | GenTypes.data_json;
                output_code_dir = "Assets/Hotfix/Config/Generate";
                output_data_dir = "Assets/Bundles/Config";
            }

            define_xml = "../Config/Defines/__root__.xml";
            input_data_dir = "../Config/Excels";
            Preview();
            GenUtils.Gen(_GetCommand(), before_gen, after_gen);
        }

        /// <summary>
        /// 生成客户端
        /// </summary>
        [MenuItem("Tools/LuBan Config/Generator Client")]
        public static void AutoGenClient()
        {
            // var fromScriptableObject = MonoScript.FromScriptableObject(CreateInstance<LubanExportConfig>());
            // var assetPath = AssetDatabase.GetAssetPath(fromScriptableObject);
            //
            // Debug.Log(assetPath);

            CreateInstance<LubanExportConfig>()?.Generator(false);
        }

        /// <summary>
        /// 生成服务器
        /// </summary>
        [MenuItem("Tools/LuBan Config/Generator Server")]
        public static void AutoGenServer()
        {
            CreateInstance<LubanExportConfig>()?.Generator(true);
        }

        // [Button("删除")]
        public void Delete()
        {
            bool isExists = Directory.Exists(output_code_dir);
            if (isExists)
            {
                Directory.Delete(output_code_dir, true);
            }

            AssetDatabase.Refresh();
        }

        // [Button("预览")]
        public void Preview()
        {
            preview_command = $"{GenUtils._DOTNET} {_GetCommand()}";
        }

        private string _GetCommand()
        {
            var fromScriptableObject = MonoScript.FromScriptableObject(this);
            var assetPath = AssetDatabase.GetAssetPath(fromScriptableObject);
            DirectoryInfo directoryInfo = new DirectoryInfo(assetPath);
            tpl_path = directoryInfo.Parent.Parent + "/Templates";

            string lineEnd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? " ^" : " \\";

            StringBuilder sb = new StringBuilder();

            var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            sb.Append(which_dll);

            foreach (var field_info in fields)
            {
                var command = field_info.GetCustomAttribute<CommandAttribute>();

                if (command is null)
                {
                    continue;
                }

                var value = field_info.GetValue(this)?.ToString();

                // 当前值为空 或者 False, 或者 None(Enum 默认值)
                // 则继续循环
                if (string.IsNullOrEmpty(value) || string.Equals(value, "False") || string.Equals(value, "None"))
                {
                    continue;
                }


                if (string.Equals(value, "True"))
                {
                    value = string.Empty;
                }

                value = value.Replace(", ", ",");

                sb.Append($" {command.option} {value} ");

                if (command.new_line)
                {
                    sb.Append($"{lineEnd} \n");
                }
            }

            return sb.ToString();
        }
    }
}