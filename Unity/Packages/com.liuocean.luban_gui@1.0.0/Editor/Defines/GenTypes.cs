using System;
using Sirenix.OdinInspector;

namespace Luban.Editor
{
    [Flags]
    public enum GenTypes
    {
        None = 0,

        [LabelText("C# 二进制代码")]
        code_cs_bin = 1,

        [LabelText("C# dotnet Json 代码")]
        code_cs_dotnet_json = code_cs_bin << 1,

        [LabelText("C# Unity Json 代码")]
        code_cs_unity_json = code_cs_dotnet_json << 1,

        [LabelText("C# Unity Editor Json 代码")]
        code_cs_unity_editor_json = code_cs_unity_json << 1,

        [LabelText("lua 二进制代码")]
        code_lua_bin = code_cs_unity_editor_json << 1,

        [LabelText("TS 二进制代码")]
        code_typescript_bin = code_lua_bin << 1,

        [LabelText("Protobuf 代码")]
        code_protobuf = code_typescript_bin << 1,

        [LabelText("FlatBuffers 代码")]
        code_flatbuffers = code_protobuf << 1,

        [LabelText("二进制数据")]
        data_bin = code_flatbuffers << 1,

        [LabelText("二进制 Lua 数据")]
        data_lua = data_bin << 1,

        [LabelText("Json 数据")]
        data_json = data_lua << 1,

        [LabelText("Json 2 数据")]
        data_json2 = data_json << 1,

        [LabelText("Json Monolithic 数据")]
        data_json_monolithic = data_json2 << 1,

        [LabelText("Yaml 数据")]
        data_yaml = data_json_monolithic << 1,

        data_resources        = data_yaml             << 1,
        data_template         = data_resources        << 1,
        data_protobuf_bin     = data_template         << 1,
        data_protobuf_json    = data_protobuf_bin     << 1,
        data_flatbuffers_json = data_protobuf_json    << 1,
        convert_json          = data_flatbuffers_json << 1,
        convert_lua           = convert_json          << 1,
        convert_xlsx          = convert_lua           << 1,
        data_bson             = convert_xlsx          << 1
    }
}