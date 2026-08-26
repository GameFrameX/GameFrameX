using System.Text;

namespace GameFrameX.ProtoExport
{
    [Mode(ModeType.CSharp)]
    internal sealed class ProtoBufCSharpHelper : IProtoGenerateHelper
    {
        private string[] _usingStatements = [];
        private bool _shouldGenerateDescription;

        public void Run(MessageInfoList messageInfoList, string outputPath, string namespaceName = "GFXHotfix")
        {
            StringBuilder sb = new StringBuilder();
            sb.AddTemplateHeader();

            if (_usingStatements.Length > 0)
            {
                var normalizedStatements = string.Join(Environment.NewLine,
                    _usingStatements.Select(s => s.Trim().TrimEnd(';') + ";"));
                sb.AppendLine(normalizedStatements);
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");

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

            sb.Append("}");
            sb.AppendLine();
            ExportLogger.WriteLine($"Generate File:{messageInfoList.OutputPath + ".cs"}");
            File.WriteAllText(messageInfoList.OutputPath + ".cs", sb.ToString(), Encoding.UTF8);
        }

        private void AppendEnum(StringBuilder sb, MessageInfo operationCodeInfo)
        {
            sb.AppendLine($"\t/// <summary>");
            sb.AppendLine($"\t/// {operationCodeInfo.Description}");
            sb.AppendLine($"\t/// </summary>");
            if (_shouldGenerateDescription)
            {
                sb.AppendLine($"\t[System.ComponentModel.Description(\"{operationCodeInfo.Description}\")]");
            }
            sb.AppendLine($"\tpublic enum {operationCodeInfo.Name}");
            sb.AppendLine("\t{");
            for (var index = 0; index < operationCodeInfo.Fields.Count; index++)
            {
                var operationField = operationCodeInfo.Fields[index];
                if (string.IsNullOrEmpty(operationField.Type))
                {
                    continue;
                }

                sb.AppendLine($"\t\t/// <summary>");
                sb.AppendLine($"\t\t/// {operationField.Description}");
                sb.AppendLine($"\t\t/// </summary>");
                if (_shouldGenerateDescription)
                {
                    sb.AppendLine($"\t\t[System.ComponentModel.Description(\"{operationField.Description}\")]");
                }
                sb.AppendLine($"\t\t{operationField.Type} = {operationField.Members},");
                if (index < operationCodeInfo.Fields.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("\t}");
            sb.AppendLine();
        }

        private void AppendMessage(StringBuilder sb, MessageInfoList messageInfoList, MessageInfo operationCodeInfo)
        {
            sb.AppendLine($"\t/// <summary>");
            sb.AppendLine($"\t/// {operationCodeInfo.Description}");
            sb.AppendLine($"\t/// </summary>");
            sb.AppendLine($"\t[ProtoContract]");
            if (_shouldGenerateDescription)
            {
                sb.AppendLine($"\t[System.ComponentModel.Description(\"{operationCodeInfo.Description}\")]");
            }
            if (string.IsNullOrEmpty(operationCodeInfo.ParentClass))
            {
                sb.AppendLine($"\tpublic sealed class {operationCodeInfo.Name}");
            }
            else
            {
                sb.AppendLine($"\t[MessageTypeHandler((({messageInfoList.Module}) << 16) + {operationCodeInfo.Opcode})]");
                sb.AppendLine($"\tpublic sealed class {operationCodeInfo.Name} : MessageObject, {operationCodeInfo.ParentClass}");
            }

            sb.AppendLine("\t{");
            for (var index = 0; index < operationCodeInfo.Fields.Count; index++)
            {
                var operationField = operationCodeInfo.Fields[index];
                if (!operationField.IsValid)
                {
                    continue;
                }

                sb.AppendLine($"\t\t/// <summary>");
                sb.AppendLine($"\t\t/// {operationField.Description}");
                sb.AppendLine($"\t\t/// </summary>");
                sb.AppendLine($"\t\t[ProtoMember({operationField.Members})]");
                if (_shouldGenerateDescription)
                {
                    sb.AppendLine($"\t\t[System.ComponentModel.Description(\"{operationField.Description}\")]");
                }
                if (operationField.IsRepeated)
                {
                    var mappedType = TypeMapper.ToCSharp(operationField.Type);
                    sb.AppendLine($"\t\tpublic List<{mappedType}> {operationField.Name} {{ get; set; }} = new List<{mappedType}>();");
                }
                else
                {
                    var mappedType = TypeMapper.ToCSharp(operationField.Type);
                    string defaultValue = string.Empty;
                    if (operationField.IsKv)
                    {
                        defaultValue = $" = new {mappedType}();";
                        sb.AppendLine($"\t\t[ProtoMap(DisableMap = true)]");
                    }

                    sb.AppendLine($"\t\tpublic {mappedType} {operationField.Name} {{ get; set; }}{defaultValue}");
                }

                if (index < operationCodeInfo.Fields.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            if (!string.IsNullOrEmpty(operationCodeInfo.ParentClass))
            {
                sb.AppendLine();
                sb.AppendLine("\t\tpublic override void Clear()");
                sb.AppendLine("\t\t{");
                for (var index = 0; index < operationCodeInfo.Fields.Count; index++)
                {
                    var operationField = operationCodeInfo.Fields[index];
                    if (!operationField.IsValid)
                    {
                        continue;
                    }

                    if (operationField.IsRepeated)
                    {
                        sb.AppendLine($"\t\t\t{operationField.Name}.Clear();");
                    }
                    else
                    {
                        if (operationField.IsKv)
                        {
                            sb.AppendLine($"\t\t\t{operationField.Name}.Clear();");
                        }
                        else
                        {
                            sb.AppendLine($"\t\t\t{operationField.Name} = default;");
                        }
                    }
                }

                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine();
        }

        public void Init(LauncherOptions launcherOptions)
        {
            if (!string.IsNullOrEmpty(launcherOptions.UsingStatements))
            {
                _usingStatements = launcherOptions.UsingStatements.Split('|', StringSplitOptions.RemoveEmptyEntries);
            }

            _shouldGenerateDescription = launcherOptions.IsGenerateDescription;
        }

        public void Post(List<MessageInfoList> operationCodeInfo, LauncherOptions launcherOptions)
        {
        }
    }
}
