using System.Text;

namespace Proto2CS.Editor
{
    /// <summary>
    /// 生成ProtoBuf 协议文件
    /// </summary>
    [Mode(ModeType.TypeScript)]
    internal class ProtoBuffTypeScriptHelper : IProtoGenerateHelper
    {
        public void Run(OperationCodeInfoList operationCodeInfoList, string outputPath, string namespaceName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("import IRequestMessage from \"../network/IRequestMessage\";\n");
            sb.Append("import IResponseMessage from \"../network/IResponseMessage\";\n");
            sb.Append("import MessageObject from \"../network/MessageObject\";\n");
            sb.Append("import RegisterRequestMessageClass from \"../network/RegisterRequestMessageClass\";\n");
            sb.Append("import RegisterResponseMessageClass from \"../network/RegisterResponseMessageClass\";\n");
            // sb.Append($"namespace {namespaceName} {'{'}\n");

            foreach (var operationCodeInfo in operationCodeInfoList.OperationCodeInfos)
            {
                if (operationCodeInfo.IsEnum)
                {
                    sb.Append($"\t/// <summary>\n");
                    sb.Append($"\t/// {operationCodeInfo.Description}\n");
                    sb.Append($"\t/// </summary>\n");
                    sb.AppendLine($"\texport enum {operationCodeInfo.Name}");
                    sb.AppendLine("\t{");
                    foreach (var operationField in operationCodeInfo.Fields)
                    {
                        sb.Append($"\t\t/// <summary>\n");
                        sb.Append($"\t\t/// {operationField.Description}\n");
                        sb.Append($"\t\t/// </summary>\n");
                        sb.Append($"\t\t{operationField.Type} = {operationField.Members}, \n");
                    }

                    sb.AppendLine("\t}\n");
                }
                else
                {
                    sb.Append($"\t/// <summary>\n");
                    sb.Append($"\t/// {operationCodeInfo.Description}\n");
                    sb.Append($"\t/// </summary>\n");
                    if (operationCodeInfo.IsRequest)
                    {
                        sb.Append($"\t@RegisterRequestMessageClass({operationCodeInfo.Opcode})\n");
                    }
                    else
                    {
                        sb.Append($"\t@RegisterResponseMessageClass({operationCodeInfo.Opcode})\n");
                    }

                    if (string.IsNullOrEmpty(operationCodeInfo.ParentClass))
                    {
                        sb.AppendLine($"\texport class {operationCodeInfo.Name} {'{'}");
                    }
                    else
                    {
                        // sb.AppendLine($"\t[MessageTypeHandler({operationCodeInfo.Opcode})]");
                        sb.AppendLine($"\texport class {operationCodeInfo.Name} extends MessageObject implements {operationCodeInfo.ParentClass} {'{'}");
                    }

                    foreach (var operationField in operationCodeInfo.Fields)
                    {
                        if (!operationField.IsValid)
                        {
                            continue;
                        }

                        if (operationField.IsRepeated)
                        {
                            sb.Append($"\t\t/// <summary>\n");
                            sb.Append($"\t\t/// {operationField.Description}\n");
                            sb.Append($"\t\t/// </summary>\n");
                            sb.Append($"\t\t{operationField.Name}:{ConvertType(operationField.Type)};\n\n");
                        }
                        else
                        {
                            sb.Append($"\t\t/// <summary>\n");
                            sb.Append($"\t\t/// {operationField.Description}\n");
                            sb.Append($"\t\t/// </summary>\n");
                            sb.Append($"\t\t{operationField.Name}:{ConvertType(operationField.Type)};\n\n");
                        }
                    }

                    sb.Append($"\t\tpublic static get PackageName(): string{'{'}\n");
                    sb.Append($"\t\t\treturn '{namespaceName}.{operationCodeInfo.Name}';\n");
                    sb.Append($"\t\t{'}'}\n");

                    sb.AppendLine("\t}\n");
                }
            }

            // sb.Append("}\n");

            File.WriteAllText(operationCodeInfoList.OutputPath + ".ts", sb.ToString(), Encoding.UTF8);
        }

        static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "double":
                case "float":
                    typeCs = "number";
                    break;
                case "uint32":
                case "fixed32":
                    typeCs = "uint";
                    break;
                case "string":
                    typeCs = "string";
                    break;
                case "bool":
                    typeCs = "boolean";
                    break;
                default:
                    if (type.StartsWith("Dictionary<"))
                    {
                        var typeMap = type.Replace("Dictionary", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty).Split(',');
                        if (typeMap.Length == 2)
                        {
                            typeCs = $"Map<{ConvertType(typeMap[0].Trim())}, {ConvertType(typeMap[1].Trim())}>";
                            break;
                        }
                    }

                    typeCs = type;
                    break;
            }

            return typeCs;
        }
    }
}