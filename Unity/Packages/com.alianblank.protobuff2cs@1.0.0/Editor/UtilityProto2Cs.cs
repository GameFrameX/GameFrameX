using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Proto2CS.Editor
{
    internal static class UtilityProto2Cs
    {
        private class OpcodeInfo
        {
            public string Name;
            public int Opcode;
            public string Description;
            public bool IsReq;
        }

        private const string protoPath = ".";

        private static readonly char[] splitChars = {' ', '\t'};

        private static readonly string[] splitNotesChars = {"//"};
        private static readonly List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();

        public static void Proto2Cs(string inputPath, string outputPath, string namespaceName = "ETHotfix", bool isServer = false)
        {
            // msgOpcode.Clear();
            DirectoryInfo directoryInfo = new DirectoryInfo(inputPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*.proto", SearchOption.AllDirectories);
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Name.Contains("Cmd"))
                {
                    GenerateOpcode(namespaceName, fileInfo.FullName, outputPath, "HotfixOuterOpcode");
                    continue;
                }

                Proto2CS(namespaceName, fileInfo.FullName, outputPath, "HotfixOuterOpcode", isServer);
            }
        }

        private static void GenerateOpcode(string ns, string inputFileName, string outputPath, string outputFileName)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            msgOpcode.Clear();
            var lines = File.ReadAllLines(inputFileName);
            bool isStart = false;
            bool isReq = false;
            foreach (var line in lines)
            {
                if (line.Contains("C2SCmdEnum"))
                {
                    isStart = true;
                    isReq = true;
                    continue;
                }

                if (line.Contains("S2CCmdEnum"))
                {
                    isStart = true;
                    isReq = false;
                    continue;
                }

                if (line.StartsWith("}"))
                {
                    isStart = false;
                    continue;
                }

                if (isStart)
                {
                    if (string.IsNullOrWhiteSpace(line.Trim()))
                    {
                        continue;
                    }

                    var lin = line.Split('=');
                    OpcodeInfo opcodeInfo = new OpcodeInfo();
                    opcodeInfo.Name = lin[0].Trim();
                    opcodeInfo.Opcode = Convert.ToInt32(lin[1].Split(';')[0].Trim());
                    opcodeInfo.IsReq = isReq;
                    var desc = lin[1].Split(new[] {"//"}, StringSplitOptions.None);
                    if (desc.Length > 1)
                    {
                        opcodeInfo.Description = desc[1].Trim();
                    }

                    msgOpcode.Add(opcodeInfo);
                }
            }

            GenReq(outputPath);
            GenResp(outputPath);
        }

        static void GenResp(string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;");
            sb.AppendLine($"using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace ETHotfix");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static partial class HotfixOuterResponseOpcode");
            sb.AppendLine("\t{");
            sb.AppendLine($"		public static SortedDictionary<uint, Type> messageKV = new SortedDictionary<uint, Type>()");
            sb.AppendLine("		{");
            foreach (OpcodeInfo info in msgOpcode)
            {
                if (info.Name.Contains(" "))
                {
                    continue;
                }

                if (info.IsReq)
                {
                    continue;
                }

                sb.AppendLine($"\t\t\t// {info.Description}");
                sb.Append("\t\t\t{");
                sb.Append($"{info.Opcode}, typeof({info.Name.Replace("Cmd_", string.Empty)})");
                sb.Append("},\n");
            }

            sb.AppendLine("\t\t};");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string csPath = Path.Combine(outputPath, "HotfixOuterResponseOpcode.cs");

            File.WriteAllText(csPath, sb.ToString(), Encoding.UTF8);
        }

        static void GenReq(string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using System;");
            sb.AppendLine($"using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace ETHotfix");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic static partial class HotfixOuterRequestOpcode");
            sb.AppendLine("\t{");
            sb.AppendLine($"		public static SortedDictionary<string, ushort> messageKV = new SortedDictionary<string, ushort>()");
            sb.AppendLine("		{");

            foreach (OpcodeInfo info in msgOpcode)
            {
                if (info.Name.Contains(" "))
                {
                    continue;
                }

                if (!info.IsReq)
                {
                    continue;
                }

                sb.AppendLine($"\t\t\t// {info.Description}");
                sb.Append("\t\t\t{");
                sb.Append($"nameof({info.Name.Replace("Cmd_", string.Empty)}), {info.Opcode}");
                sb.Append("},\n");
            }

            sb.AppendLine("		};");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string csPath = Path.Combine(outputPath, "HotfixOuterRequestOpcode.cs");

            File.WriteAllText(csPath, sb.ToString(), Encoding.UTF8);
        }

        public static void Proto2CS(string ns, string protoName, string outputPath, string opcodeClassName, bool isServer = false)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // msgOpcode.Clear();
            string proto = Path.Combine(protoPath, protoName);
            string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(proto) + ".cs");

            string s = File.ReadAllText(proto);

            StringBuilder sb = new StringBuilder();
            StringBuilder sbTemp = new StringBuilder();
            // sb.Append("using ETModel;\n");
            sb.Append("using ProtoBuf;\n");
            sb.Append("using System.Collections.Generic;\n");
            if (isServer)
            {
                sb.AppendLine("using Server.Core.Net.Messages;");
            }
            else
            {
                sb.AppendLine("using Protocol;");
            }

            sb.AppendLine();
            sb.Append($"namespace {ns}\n");
            sb.Append("{\n");

            bool isMsgStart = false;
            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();
                sbTemp.Clear();
                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//ResponseType"))
                {
                    string responseType = line.Split(' ')[1].TrimEnd('\r', '\n');
                    sb.AppendLine($"\t[ResponseType(nameof({responseType}))]");

                    continue;
                }

                if (newline.StartsWith("//"))
                {
                    sb.Append($"\t/// <summary>\n");
                    sb.Append($"\t/// {newline.Replace("//", string.Empty).Replace(" ", string.Empty)}\n");
                    sb.Append($"\t/// </summary>\n");

                    continue;
                }

                if (newline.StartsWith("message"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] {"//"}, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    // msgOpcode.Add(new OpcodeInfo() {Name = msgName, Opcode = ++startOpcode});
                    if (msgOpcode.Exists(m => m.Name.Contains(msgName)))
                    {
                        //sb.Append($"\t[NetMessage({opcodeClassName}.{msgName})]\n");
                    }

                    // if (isServer)
                    {
                        sb.Append($"\t######\n");
                    }

                    sb.Append($"\t[ProtoContract]\n");

                    // if (msgName.StartsWith("Res"))
                    // {
                    //     sb.Append($"\tpublic partial class {msgName} : PacketHandlerBase");
                    // }
                    // else
                    {
                        if (isServer)
                        {
                            sb.Append($"\tpublic partial class {msgName} : Server.Core.Net.Messages.Message");
                        }
                        else
                        {
                            sb.Append($"\tpublic partial class {msgName} : Protocol.Message");
                        }
                    }


                    // if (msgName.StartsWith("Req"))
                    // {
                    //     parentClass = "IRequestMessage";
                    // }
                    // else if (msgName.StartsWith("Res"))
                    // {
                    //     parentClass = "IResponseMessage";
                    // }

                    if (parentClass == "IActorMessage" || parentClass.Contains("IRequestMessage") || parentClass.Contains("IResponseMessage"))
                    {
                        // if (isServer)
                        {
                            var parentCsList = parentClass.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                            if (parentCsList.Length > 1)
                            {
                                sb.Append($", {parentCsList[0]}\n");
                                sb.Replace("######", $"[MessageTypeHandler({parentCsList[1]})]");
                            }
                        }
                    }
                    // else if (parentClass != "")
                    // {
                    //     if (isServer)
                    //     {
                    //         sb.Append($", {parentClass}\n");
                    //     }
                    // }
                    else
                    {
                        sb.Append("\n");
                    }

                    // if (isServer)
                    {
                        sb.Replace("######", string.Empty);
                    }

                    sb.Append("\t{\n");
                    continue;
                }

                if (newline.StartsWith("enum"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] {"//"}, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    sb.Append($"\tpublic enum {msgName}");


                    // if (msgName.StartsWith("Req"))
                    // {
                    //     parentClass = "IRequestMessage";
                    // }
                    // else if (msgName.StartsWith("Res"))
                    // {
                    //     parentClass = "IResponseMessage";
                    // }

                    if (parentClass == "Message" || parentClass == "IActorRequest" || parentClass == "IActorResponse")
                    {
                        sb.Append($": {parentClass}\n");
                    }
                    else if (parentClass != "")
                    {
                        sb.Append($", {parentClass}\n");
                    }
                    else
                    {
                        sb.Append("\n");
                    }

                    sb.Append("\t{\n");
                    continue;
                }

                if (isMsgStart)
                {
                    if (newline == "{")
                    {
                        sb.Append("\t{\n");

                        continue;
                    }

                    if (newline == "}")
                    {
                        isMsgStart = false;
                        sb.Append("\t}\n\n");

                        continue;
                    }

                    if (newline.Trim().StartsWith("//"))
                    {
                        sb.AppendLine(newline);

                        continue;
                    }

                    if (newline.Trim() != "" && newline != "}")
                    {
                        if (newline.StartsWith("repeated"))
                        {
                            Repeated(sb, ns, newline);
                        }
                        else
                        {
                            Members(sb, newline, true);
                        }
                    }
                }
            }

            sb.Append("}\n");
            File.WriteAllText(csPath, sb.ToString(), Encoding.UTF8);
        }


        private static void Repeated(StringBuilder sb, string ns, string newline)
        {
            try
            {
                int index = newline.IndexOf(";", StringComparison.Ordinal);
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                string name = ss[2];
                int n = int.Parse(ss[4]);
                string[] notesList = newline.Split(splitNotesChars, StringSplitOptions.RemoveEmptyEntries);

                sb.Append($"\t\t/// <summary>\n");
                sb.Append($"\t\t/// {(notesList.Length > 1 ? notesList[1] : string.Empty)}\n");
                sb.Append($"\t\t/// </summary>\n");
                sb.Append($"\t\t[ProtoMember({n})]\n");
                sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }

        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";

                    break;
                case "int32":
                    typeCs = "int";

                    break;
                case "bytes":
                    typeCs = "byte[]";

                    break;
                case "uint32":
                    typeCs = "uint";

                    break;
                case "long":
                    typeCs = "long";

                    break;
                case "int64":
                    typeCs = "long";

                    break;
                case "uint64":
                    typeCs = "ulong";

                    break;
                case "uint16":
                    typeCs = "ushort";

                    break;
                case "map<int32,long>":
                    typeCs = "Dictionary<int, long>";

                    break;
                default:
                    typeCs = type;

                    break;
            }

            return typeCs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="newline"></param>
        /// <param name="isRequired"></param>
        private static void Members(StringBuilder sb, string newline, bool isRequired)
        {
            try
            {
                string originNewLine = newline;
                int index = newline.IndexOf(";", StringComparison.Ordinal);
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

                if (ss.Length > 3)
                {
                    string type = ss[0];
                    string name = ss[1];
                    int n = int.Parse(ss[3]);
                    string typeCs = ConvertType(type);
                    string[] notesList = originNewLine.Split(splitNotesChars, StringSplitOptions.RemoveEmptyEntries);

                    sb.Append($"\t\t/// <summary>\n");
                    sb.Append($"\t\t/// {(notesList.Length > 1 ? notesList[1] : string.Empty)}\n");
                    sb.Append($"\t\t/// </summary>\n");
                    sb.Append($"\t\t[ProtoMember({n})]\n");
                    sb.Append($"\t\tpublic {typeCs} {name} {{ get; set; }}\n\n");
                }
                else
                {
                    // enum
                    string name = ss[0];
                    int value = int.Parse(ss[2]);
                    string[] notesList = originNewLine.Split(splitNotesChars, StringSplitOptions.RemoveEmptyEntries);

                    sb.Append($"\t\t/// <summary>\n");
                    sb.Append($"\t\t/// {(notesList.Length > 1 ? notesList[1] : string.Empty)}\n");
                    sb.Append($"\t\t/// </summary>\n");
                    sb.Append($"\t\t{name} = {value}, \n\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
        }
    }
}