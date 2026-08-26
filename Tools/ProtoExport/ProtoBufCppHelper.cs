using System.Text;

namespace GameFrameX.ProtoExport;

[Mode(ModeType.Cpp)]
internal sealed class ProtoBufCppHelper : IProtoGenerateHelper
{
    private string[] _includeStatements = [];
    private bool _shouldGenerateDescription;

    public void Init(LauncherOptions launcherOptions)
    {
        if (!string.IsNullOrEmpty(launcherOptions.UsingStatements))
        {
            _includeStatements = launcherOptions.UsingStatements.Split('|', StringSplitOptions.RemoveEmptyEntries);
        }

        _shouldGenerateDescription = launcherOptions.IsGenerateDescription;
    }

    public void Run(MessageInfoList messageInfoList, string outputPath, string namespaceName = "GFXHotfix")
    {
        StringBuilder sb = new StringBuilder();
        sb.AddTemplateHeader();
        sb.AppendLine("#pragma once");
        sb.AppendLine();

        if (_includeStatements.Length > 0)
        {
            var normalizedStatements = string.Join(Environment.NewLine,
                _includeStatements.Select(s => s.Trim()));
            sb.AppendLine(normalizedStatements);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(namespaceName))
        {
            var namespaces = namespaceName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            foreach (var ns in namespaces)
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
            }
        }

        foreach (var operationCodeInfo in messageInfoList.Infos)
        {
            if (operationCodeInfo.IsEnum)
            {
                AppendEnum(sb, operationCodeInfo);
            }
            else
            {
                AppendMessage(sb, messageInfoList, operationCodeInfo);
            }
        }

        if (!string.IsNullOrEmpty(namespaceName))
        {
            var namespaces = namespaceName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            for (int i = namespaces.Length - 1; i >= 0; i--)
            {
                sb.Append("} // namespace ").AppendLine(namespaces[i]);
            }
        }

        sb.AppendLine();
        ExportLogger.WriteLine($"Generate File:{messageInfoList.OutputPath + ".h"}");
        File.WriteAllText(messageInfoList.OutputPath + ".h", sb.ToString(), Encoding.UTF8);
    }

    private void AppendEnum(StringBuilder sb, MessageInfo operationCodeInfo)
    {
        sb.AppendLine($"/// \\brief {operationCodeInfo.Description}");
        sb.AppendLine($"enum class {operationCodeInfo.Name}");
        sb.AppendLine("{");
        for (var index = 0; index < operationCodeInfo.Fields.Count; index++)
        {
            var operationField = operationCodeInfo.Fields[index];
            if (string.IsNullOrEmpty(operationField.Type))
            {
                continue;
            }

            sb.Append($"\t{operationField.Type} = {operationField.Members},");
            if (!string.IsNullOrEmpty(operationField.Description))
            {
                sb.Append($" // {operationField.Description}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("};");
        sb.AppendLine();
    }

    private void AppendMessage(StringBuilder sb, MessageInfoList messageInfoList, MessageInfo operationCodeInfo)
    {
        sb.AppendLine($"/// \\brief {operationCodeInfo.Description}");

        if (string.IsNullOrEmpty(operationCodeInfo.ParentClass))
        {
            sb.AppendLine($"class {operationCodeInfo.Name}");
        }
        else
        {
            var parentClass = operationCodeInfo.ParentClass
                .Replace("IRequestMessage", "public IRequestMessage")
                .Replace("INotifyMessage", "public INotifyMessage")
                .Replace("IResponseMessage", "public IResponseMessage")
                .Replace("IHeartBeatMessage", "public IHeartBeatMessage");
            sb.AppendLine($"class {operationCodeInfo.Name} : public MessageObject, {parentClass}");
        }

        sb.AppendLine("{");
        sb.AppendLine("public:");

        if (!string.IsNullOrEmpty(operationCodeInfo.ParentClass))
        {
            sb.AppendLine($"\tstatic constexpr uint32_t MESSAGE_ID = (({messageInfoList.Module}) << 16) + {operationCodeInfo.Opcode};");
            sb.AppendLine();
        }

        for (var index = 0; index < operationCodeInfo.Fields.Count; index++)
        {
            var operationField = operationCodeInfo.Fields[index];
            if (!operationField.IsValid)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(operationField.Description))
            {
                sb.AppendLine($"\t/// \\brief {operationField.Description}");
            }

            if (operationField.IsRepeated)
            {
                var mappedType = TypeMapper.ToCpp(operationField.Type);
                sb.AppendLine($"\tstd::vector<{mappedType}> {operationField.Name}{{}};");
            }
            else if (operationField.IsKv)
            {
                var mappedType = TypeMapper.ToCpp(operationField.Type);
                sb.AppendLine($"\t{mappedType} {operationField.Name}{{}};");
            }
            else
            {
                var mappedType = TypeMapper.ToCpp(operationField.Type);
                var defaultValue = GetCppDefaultValue(operationField.Type);
                sb.AppendLine($"\t{mappedType} {operationField.Name}{{{defaultValue}}};");
            }

            if (index < operationCodeInfo.Fields.Count - 1)
            {
                sb.AppendLine();
            }
        }

        if (!string.IsNullOrEmpty(operationCodeInfo.ParentClass))
        {
            sb.AppendLine();
            sb.AppendLine("\tvoid Clear() override");
            sb.AppendLine("\t{");
            for (var index = 0; index < operationCodeInfo.Fields.Count; index++)
            {
                var operationField = operationCodeInfo.Fields[index];
                if (!operationField.IsValid)
                {
                    continue;
                }

                if (operationField.IsRepeated)
                {
                    sb.AppendLine($"\t\t{operationField.Name}.clear();");
                }
                else if (operationField.IsKv)
                {
                    sb.AppendLine($"\t\t{operationField.Name}.clear();");
                }
                else
                {
                    var defaultValue = GetCppDefaultValue(operationField.Type);
                    sb.AppendLine($"\t\t{operationField.Name} = {defaultValue};");
                }
            }

            sb.AppendLine("\t}");
        }

        sb.AppendLine("};");
        sb.AppendLine();
    }

    private static string GetCppDefaultValue(string protoType)
    {
        switch (protoType)
        {
            case "int32":
            case "sint32":
            case "sfixed32":
            case "uint32":
            case "fixed32":
            case "int64":
            case "sint64":
            case "sfixed64":
            case "uint64":
            case "fixed64":
            case "float":
            case "double":
                return "0";
            case "bool":
                return "false";
            case "string":
                return "";
            case "bytes":
                return "";
            default:
                return "";
        }
    }

    public void Post(List<MessageInfoList> operationCodeInfo, LauncherOptions launcherOptions)
    {
    }
}
