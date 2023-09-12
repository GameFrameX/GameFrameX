using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Proto2CS.Editor
{
    internal class MessagePackHelper : IProtoGenerateHelper
    {
        public void Run(string inputPath, string outputPath, string namespaceName = "Hotfix", bool isServer = false)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var directoryInfo = new DirectoryInfo(inputPath);
            var fileInfos = directoryInfo.GetFiles("*.proto", SearchOption.AllDirectories);
            foreach (var fileInfo in fileInfos)
            {
                RunGen(namespaceName, fileInfo.FullName, outputPath, isServer);
            }
        }

        private static void RunGen(string ns, string inputPath, string outputPath, bool isServer = false)
        {
            string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(inputPath) + ".cs");

            string s = File.ReadAllText(inputPath);

            StringBuilder sb = new StringBuilder();
            StringBuilder sbTemp = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using MessagePack;");
            sb.AppendLine("using System.Collections.Generic;");
            if (isServer)
            {
                sb.AppendLine("using Server.Core.Net.Messages;");
            }
            else
            {
                sb.AppendLine("using GameFramework.Network;");
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
                    string msgName = newline.Split(Utility.splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }


                    sb.Append($"\t######\n");

                    sb.Append($"\t[MessagePackObject(true)]\n");


                    if (isServer)
                    {
                        sb.Append($"\tpublic partial class {msgName} : Server.Core.Net.Messages.MessageObject");
                    }
                    else
                    {
                        sb.Append($"\tpublic partial class {msgName} : MessageObject");
                    }


                    if (parentClass == "IMessage" || parentClass.Contains("IRequestMessage") || parentClass.Contains("IResponseMessage"))
                    {
                        var parentCsList = parentClass.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (parentCsList.Length > 1)
                        {
                            sb.Append($", {parentCsList[0]}\n");
                            sb.Replace("######", $"[MessageTypeHandler({parentCsList[1]})]");
                        }
                    }
                    else
                    {
                        sb.Append("\n");
                    }

                    sb.Replace("######", string.Empty);


                    sb.Append("\t{\n");
                    continue;
                }

                if (newline.StartsWith("enum"))
                {
                    string parentClass = "";
                    isMsgStart = true;
                    string msgName = newline.Split(Utility.splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    string[] ss = newline.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 2)
                    {
                        parentClass = ss[1].Trim();
                    }

                    sb.Append($"\tpublic enum {msgName}");


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
                string[] ss = newline.Split(Utility.splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = Utility.ConvertType(type);
                string name = ss[2];
                string[] notesList = newline.Split(Utility.splitNotesChars, StringSplitOptions.RemoveEmptyEntries);

                sb.Append($"\t\t/// <summary>\n");
                sb.Append($"\t\t/// {(notesList.Length > 1 ? notesList[1] : string.Empty)}\n");
                sb.Append($"\t\t/// </summary>\n");
                sb.Append($"\t\tpublic List<{type}> {name} = new List<{type}>();\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{newline}\n {e}");
            }
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
                string[] ss = newline.Split(Utility.splitChars, StringSplitOptions.RemoveEmptyEntries);

                if (ss.Length > 3)
                {
                    string type = ss[0];
                    string name = ss[1];
                    string typeCs = Utility.ConvertType(type);
                    string[] notesList = originNewLine.Split(Utility.splitNotesChars, StringSplitOptions.RemoveEmptyEntries);

                    sb.Append($"\t\t/// <summary>\n");
                    sb.Append($"\t\t/// {(notesList.Length > 1 ? notesList[1] : string.Empty)}\n");
                    sb.Append($"\t\t/// </summary>\n");
                    sb.Append($"\t\tpublic {typeCs} {name} {{ get; set; }}\n\n");
                }
                else
                {
                    // enum
                    string name = ss[0];
                    int value = int.Parse(ss[2]);
                    string[] notesList = originNewLine.Split(Utility.splitNotesChars, StringSplitOptions.RemoveEmptyEntries);

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