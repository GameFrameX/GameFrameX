using System;
// using Sirenix.OdinInspector;

namespace Luban.Editor
{
    [Flags]
    public enum GenTypes
    {
        None = 0,

        /// <summary>
        /// C# 二进制代码
        /// </summary>
        // [LabelText("C# 二进制代码")]
        code_cs_bin = 1,

        /// <summary>
        /// C# dotnet Json 代码
        /// </summary>
        // [LabelText("C# dotnet Json 代码")]
        code_cs_dotnet_json = code_cs_bin << 1,

        /// <summary>
        /// C# Unity Json 代码
        /// </summary>
        // [LabelText("C# Unity Json 代码")]
        code_cs_unity_json = code_cs_dotnet_json << 1,

        /// <summary>
        /// C# Unity Editor Json 代码
        /// </summary>
        // [LabelText("C# Unity Editor Json 代码")]
        code_cs_unity_editor_json = code_cs_unity_json << 1,

        /// <summary>
        /// lua 二进制代码
        /// </summary>
        // [LabelText("lua 二进制代码")]
        code_lua_bin = code_cs_unity_editor_json << 1,

        /// <summary>
        /// TS 二进制代码
        /// </summary>
        // [LabelText("TS 二进制代码")]
        code_typescript_bin = code_lua_bin << 1,

        /// <summary>
        /// Protobuf 代码
        /// </summary>
        // [LabelText("Protobuf 代码")]
        code_protobuf = code_typescript_bin << 1,

        /// <summary>
        /// FlatBuffers 代码
        /// </summary>
        // [LabelText("FlatBuffers 代码")]
        code_flatbuffers = code_protobuf << 1,

        /// <summary>
        /// 二进制数据
        /// </summary>
        // [LabelText("二进制数据")]
        data_bin = code_flatbuffers << 1,

        /// <summary>
        /// 二进制 Lua 数据
        /// </summary>
        // [LabelText("二进制 Lua 数据")]
        data_lua = data_bin << 1,

        /// <summary>
        /// Json 数据
        /// </summary>
        // [LabelText("Json 数据")]
        data_json = data_lua << 1,

        /// <summary>
        /// Json 2 数据
        /// </summary>
        // [LabelText("Json 2 数据")]
        data_json2 = data_json << 1,

        /// <summary>
        /// Json Monolithic 数据
        /// </summary>
        // [LabelText("Json Monolithic 数据")]
        data_json_monolithic = data_json2 << 1,

        /// <summary>
        /// Yaml 数据
        /// </summary>
        // [LabelText("Yaml 数据")]
        data_yaml = data_json_monolithic << 1,

        data_resources = data_yaml << 1,
        data_template = data_resources << 1,
        data_protobuf_bin = data_template << 1,
        data_protobuf_json = data_protobuf_bin << 1,
        data_flatbuffers_json = data_protobuf_json << 1,
        convert_json = data_flatbuffers_json << 1,
        convert_lua = convert_json << 1,
        convert_xlsx = convert_lua << 1,
        data_bson = convert_xlsx << 1
    }
}