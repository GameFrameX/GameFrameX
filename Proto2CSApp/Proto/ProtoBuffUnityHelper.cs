using System.Text;

namespace Proto2CS.Editor
{
    /// <summary>
    /// 生成ProtoBuf 协议文件
    /// </summary>
    [Mode(ModeType.Unity)]
    internal class ProtoBuffUnityHelper : IProtoGenerateHelper
    {
        public void Run(OperationCodeInfoList operationCodeInfoList, string outputPath, string namespaceName = "Hotfix")
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using ProtoBuf;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using GameFrameX.Network.Runtime;");
            sb.AppendLine();
            sb.Append($"namespace {namespaceName}\n");
            sb.Append("{\n");

            foreach (var operationCodeInfo in operationCodeInfoList.OperationCodeInfos)
            {
                if (operationCodeInfo.IsEnum)
                {
                    sb.Append($"\t/// <summary>\n");
                    sb.Append($"\t/// {operationCodeInfo.Description}\n");
                    sb.Append($"\t/// </summary>\n");
                    sb.AppendLine($"\tpublic enum {operationCodeInfo.Name}");
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
                    sb.Append($"\t[ProtoContract]\n");
                    if (string.IsNullOrEmpty(operationCodeInfo.ParentClass))
                    {
                        sb.AppendLine($"\tpublic partial class {operationCodeInfo.Name}");
                    }
                    else
                    {
                        sb.AppendLine($"\t[MessageTypeHandler({operationCodeInfo.Opcode})]");
                        sb.AppendLine($"\tpublic partial class {operationCodeInfo.Name} : MessageObject, {operationCodeInfo.ParentClass}");
                    }

                    sb.AppendLine("\t{");
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
                            sb.Append($"\t\t[ProtoMember({operationField.Members})]\n");
                            sb.Append($"\t\tpublic List<{operationField.Type}> {operationField.Name} = new List<{operationField.Type}>();\n\n");
                        }
                        else
                        {
                            sb.Append($"\t\t/// <summary>\n");
                            sb.Append($"\t\t/// {operationField.Description}\n");
                            sb.Append($"\t\t/// </summary>\n");
                            sb.Append($"\t\t[ProtoMember({operationField.Members})]\n");
                            sb.Append($"\t\tpublic {operationField.Type} {operationField.Name} {{ get; set; }}\n\n");
                        }
                    }

                    sb.AppendLine("\t}\n");
                }
            }

            sb.Append("}\n");

            File.WriteAllText(operationCodeInfoList.OutputPath + ".cs", sb.ToString(), Encoding.UTF8);
        }
    }
}