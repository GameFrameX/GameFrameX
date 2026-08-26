using System.Text;

namespace GameFrameX.ProtoExport
{
    [Mode(ModeType.TypeScript)]
    internal class ProtoBufTypeScriptHelper : IProtoGenerateHelper
    {
        private string _importPath = "../network/";
        private bool _shouldGenerateDescription;

        public void Init(LauncherOptions launcherOptions)
        {
            if (!string.IsNullOrEmpty(launcherOptions.ImportPath))
            {
                _importPath = launcherOptions.ImportPath;
            }

            _shouldGenerateDescription = launcherOptions.IsGenerateDescription;
        }

        public void Run(MessageInfoList messageInfoList, string outputPath, string namespaceName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"import IRequestMessage from \"{_importPath}IRequestMessage\";\n");
            sb.Append($"import IResponseMessage from \"{_importPath}IResponseMessage\";\n");
            sb.Append($"import INotifyMessage from \"{_importPath}INotifyMessage\";\n");
            sb.Append($"import IHeartBeatMessage from \"{_importPath}IHeartBeatMessage\";\n");
            sb.Append($"import MessageObject from \"{_importPath}MessageObject\";\n");
            sb.Append($"import ProtoMessageHelper from \"{_importPath}ProtoMessageHelper\";\n");
            sb.Append("\n");
            sb.Append($"export namespace {messageInfoList.ModuleName} {'{'}\n");

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

            sb.Append("}\n");
            ExportLogger.WriteLine($"Generate File:{messageInfoList.OutputPath + ".ts"}");
            File.WriteAllText(messageInfoList.OutputPath + ".ts", sb.ToString(), Encoding.UTF8);
        }

        private void AppendDescription(StringBuilder sb, string description)
        {
            if (!_shouldGenerateDescription)
            {
                return;
            }

            sb.Append($"\t/// <summary>\n");
            sb.Append($"\t/// {description}\n");
            sb.Append($"\t/// </summary>\n");
        }

        private void AppendEnum(StringBuilder sb, MessageInfo operationCodeInfo)
        {
            AppendDescription(sb, operationCodeInfo.Description);
            sb.Append($"\texport enum {operationCodeInfo.Name}\n");
            sb.Append("\t{\n");
            foreach (var operationField in operationCodeInfo.Fields)
            {
                AppendDescription(sb, $"\t{operationField.Description}");
                sb.Append($"\t\t{operationField.Type} = {operationField.Members}, \n");
            }

            sb.Append("\t}\n\n");
        }

        private void AppendMessage(StringBuilder sb, MessageInfoList messageInfoList, MessageInfo operationCodeInfo)
        {
            AppendDescription(sb, operationCodeInfo.Description);

            if (string.IsNullOrEmpty(operationCodeInfo.ParentClass))
            {
                sb.Append($"\texport class {operationCodeInfo.Name} {'{'}\n");
            }
            else
            {
                sb.Append($"\texport class {operationCodeInfo.Name} extends MessageObject implements {operationCodeInfo.ParentClass} {'{'}\n");
                sb.Append("\n");

                sb.Append($"\t\tpublic static register(): void{'{'}\n");
                if (operationCodeInfo.IsRequest)
                {
                    sb.Append($"\t\t\tProtoMessageHelper.registerReqMessage('{messageInfoList.ModuleName}.{operationCodeInfo.Name}', {(messageInfoList.Module << 16) + operationCodeInfo.Opcode});\n");
                }
                else
                {
                    sb.Append($"\t\t\tProtoMessageHelper.registerRespMessage('{messageInfoList.ModuleName}.{operationCodeInfo.Name}', {(messageInfoList.Module << 16) + operationCodeInfo.Opcode});\n");
                }

                sb.Append($"\t\t{'}'}\n\n");

                sb.Append($"\t\tpublic readonly PackageName: string = '{messageInfoList.ModuleName}.{operationCodeInfo.Name}';\n");

                sb.Append("\n");
            }

            foreach (var operationField in operationCodeInfo.Fields)
            {
                if (!operationField.IsValid)
                {
                    continue;
                }

                AppendDescription(sb, $"\t{operationField.Description}");
                sb.Append($"\t\t{operationField.Name}:{TypeMapper.ToTypeScriptFromProto(operationField.Type)};\n\n");
            }

            sb.Append("\t}\n\n");
        }

        public void Post(List<MessageInfoList> operationCodeInfo, LauncherOptions launcherOptions)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AddTemplateHeader();

            foreach (var messageInfoList in operationCodeInfo)
            {
                stringBuilder.Append($"import {{ {messageInfoList.ModuleName} }} from \"./{messageInfoList.FileName}\";\n");
            }

            stringBuilder.Append("\n");
            stringBuilder.Append("export default class ProtoMessageRegister {\n");
            stringBuilder.Append("\tpublic static getProtoBuffList(): string[] {\n");

            stringBuilder.Append("\t\treturn [\n");

            foreach (var messageInfoList in operationCodeInfo)
            {
                stringBuilder.Append($"\t\t\t\"resources/protobuf/{messageInfoList.FileName}.proto\",\n");
            }

            stringBuilder.Append("\t\t];\n");
            stringBuilder.Append("\t}\n");
            stringBuilder.Append("\n");
            stringBuilder.Append("\tpublic static register(): void {\n");
            foreach (var messageInfoList in operationCodeInfo)
            {
                foreach (var messageInfo in messageInfoList.Infos)
                {
                    if (!string.IsNullOrEmpty(messageInfo.ParentClass))
                    {
                        stringBuilder.Append($"\t\t{messageInfoList.ModuleName}.{messageInfo.Name}.register();\n");
                    }
                }
            }

            stringBuilder.Append("    }\n}\n");
            File.WriteAllText(launcherOptions.OutputPath + "/ProtoMessageRegister.ts", stringBuilder.ToString(), Encoding.UTF8);
        }
    }
}
