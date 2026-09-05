using System.Text.RegularExpressions;

namespace GameFrameX.ProtoExport;

public static partial class MessageHelper
{
    // 正则表达式匹配enums
    private const string EnumPattern = @"enum\s+(\w+)\s*\{\s*([^}]*)\s*\}";

    // 正则表达式匹配messages
    private const string MessagePattern = @"message\s+(\w+)\s*\{\s*([^}]+)\s*\}";
    private const string CommentPattern = @"//([^\n]*)\n\s*(enum|message)\s+(\w+)\s*{";
    private const string StartPattern = @"option start = (\d+);";
    private const string ModulePattern = @"option module = (-?\d+);";
    private const string PackagePattern = @"package (\w+);";

    // 文件名前缀格式：_<模块ID>_<名称>（如 _0010_Basic、_-0120_Inner_Social），模块 ID 取自前缀；分隔符兼容 - （如 _0010-Basic）
    private const string FileNameModulePattern = @"^_(-?\d+)[_-]";

    // 疑似前缀：以 _+数字 开头但缺第二个下划线（如 _0010Basic），视为命名格式错误而非无前缀
    private const string FileNameModuleLikePattern = @"^_-?\d";


    public static MessageInfoList Parse(string proto, string fileName, string filePath, bool isGenerateErrorCode)
    {
        var packageMatch = Regex.Match(proto, PackagePattern, RegexOptions.Singleline);

        if (!packageMatch.Success)
        {
            ExportLogger.WriteLine(Loc.Log_PackageNotFound);
            throw new Exception(string.Format(Loc.Err_PackageNotFound, fileName));
        }

        var messageInfo = new MessageInfoList
        {
            OutputPath = Path.Combine(filePath, fileName),
            ModuleName = packageMatch.Groups[1].Value,
            FileName = fileName,
        };

        // 模块 ID 解析：文件名前缀优先，回退到 option module 声明
        messageInfo.Module = ResolveModule(proto, fileName, out var moduleSource);
        messageInfo.ModuleSource = moduleSource;

        var packageName = packageMatch.Groups[1].Value;
        ExportLogger.WriteLine(string.Format(Loc.Log_PackageModuleLine, packageName, messageInfo.Module,
            moduleSource == MessageInfoList.ModuleSourceKind.FileName ? "fileName" : "option"));
        // 使用正则表达式提取枚举类型
        ParseEnum(proto, packageName, messageInfo.Infos);

        // 使用正则表达式提取消息类型
        ParseMessage(proto, packageName, messageInfo.Infos, isGenerateErrorCode);

        ParseComment(proto, packageName, messageInfo.Infos);

        // 消息码排序配对
        MessageIdHandler(messageInfo.Infos, 10);
        return messageInfo;
    }

    /// <summary>
    /// 解析模块 ID：文件名前缀 <c>_&lt;模块ID&gt;_</c> 优先，缺失时回退到 proto 内容的 <c>option module</c> 声明。
    /// <para>
    /// 决策表：两者都有必须一致（不一致抛异常）；仅文件名有 → 用文件名；仅 option 有 → 用 option；都没有 → 抛异常。
    /// 文件名以 _+数字 开头但缺第二个下划线（如 _0010Basic）视为命名格式错误，抛异常。
    /// </para>
    /// </summary>
    private static short ResolveModule(string proto, string fileName, out MessageInfoList.ModuleSourceKind source)
    {
        // 归一为纯文件名（兼容调用方传入带路径或带扩展名的情形），^_ 前缀匹配才可靠
        var fileNameOnly = Path.GetFileNameWithoutExtension(fileName);

        short? fromFileName = null;
        var fileNameMatch = Regex.Match(fileNameOnly, FileNameModulePattern);
        if (fileNameMatch.Success)
        {
            if (short.TryParse(fileNameMatch.Groups[1].Value, out var parsed))
            {
                fromFileName = parsed;
            }
            else
            {
                ExportLogger.WriteLine(Loc.Log_ModuleRangeError);
                throw new FormatException(string.Format(Loc.Err_ModuleRangeFileName, short.MinValue, short.MaxValue));
            }
        }
        else if (Regex.IsMatch(fileNameOnly, FileNameModuleLikePattern))
        {
            ExportLogger.WriteLine(Loc.Log_ModuleFileNameFormatError);
            throw new FormatException(string.Format(Loc.Err_ModuleFileNameFormat, fileNameOnly));
        }

        short? fromOption = null;
        var moduleMatch = Regex.Match(proto, ModulePattern, RegexOptions.Singleline);
        if (moduleMatch.Success)
        {
            if (short.TryParse(moduleMatch.Groups[1].Value, out var value))
            {
                fromOption = value;
            }
            else
            {
                ExportLogger.WriteLine(Loc.Log_ModuleRangeError);
                throw new FormatException(string.Format(Loc.Err_ModuleRangeOption, short.MinValue, short.MaxValue));
            }
        }

        if (fromFileName.HasValue)
        {
            if (fromOption.HasValue && fromOption.Value != fromFileName.Value)
            {
                ExportLogger.WriteLine(Loc.Log_ModuleMismatch);
                throw new FormatException(string.Format(Loc.Err_ModuleMismatch, fileNameOnly, fromFileName.Value, fromOption.Value));
            }

            source = MessageInfoList.ModuleSourceKind.FileName;
            return fromFileName.Value;
        }

        if (fromOption.HasValue)
        {
            source = MessageInfoList.ModuleSourceKind.Option;
            return fromOption.Value;
        }

        ExportLogger.WriteLine(Loc.Log_ModuleNotFound);
        throw new Exception(Loc.Err_ModuleNotFound);
    }

    /// <summary>
    /// 当 <c>true</c> 时，<see cref="Parse"/> 内部不再自动给 <c>Opcode</c> 赋值；
    /// 调用方需自行通过 <see cref="Persistence.MessageIdAllocator"/> 等机制分配 SubId。
    /// <para>
    /// 进程级静态标志位：导出器进程模型为单次命令行（CLI）/ 单实例 GUI，不会并发触发两轮解析。
    /// 若未来并发场景出现，需改为参数注入（<see cref="Parse"/> 接收 skipAutoAssign 形参）。
    /// </para>
    /// </summary>
    public static bool SkipAutoAssignOpcode { get; set; }

    private static void MessageIdHandler(List<MessageInfo> operationCodeInfos, int start)
    {
        if (SkipAutoAssignOpcode)
        {
            // 关闭旧的自增分配。Opcode==0 的消息将由外部 MessageIdAllocator 处理。
            return;
        }

        foreach (var operationCodeInfo in operationCodeInfos)
        {
            if (operationCodeInfo.IsMessage)
            {
                if (operationCodeInfo.Opcode > 0)
                {
                    continue;
                }

                operationCodeInfo.Opcode = start;
                // if (operationCodeInfo.IsRequest)
                // {
                //     operationCodeInfo.ResponseMessage = FindResponse(operationCodeInfos, operationCodeInfo.MessageName);
                //     if (operationCodeInfo.ResponseMessage != null)
                //     {
                //         operationCodeInfo.ResponseMessage.Opcode = operationCodeInfo.Opcode;
                //     }
                // }

                start++;
            }
        }
    }

    private static void ParseComment(string proto, string packageName, List<MessageInfo> operationCodeInfos)
    {
        MatchCollection enumMatches = Regex.Matches(proto, CommentPattern, RegexOptions.Singleline);
        foreach (Match match in enumMatches)
        {
            if (match.Groups.Count > 3)
            {
                var comment = match.Groups[1].Value;
                var type = match.Groups[3].Value;
                foreach (var operationCodeInfo in operationCodeInfos)
                {
                    if (operationCodeInfo.Name == type)
                    {
                        operationCodeInfo.Description = comment.Trim();
                        break;
                    }
                }
            }
        }
    }

    private static void ParseEnum(string proto, string packageName, List<MessageInfo> codes)
    {
        MatchCollection enumMatches = Regex.Matches(proto, EnumPattern, RegexOptions.Singleline);
        foreach (Match match in enumMatches)
        {
            MessageInfo info = new MessageInfo(true);
            codes.Add(info);
            string blockName = match.Groups[1].Value;
            if (!Utility.IsCamelCase(blockName))
            {
                throw new Exception(string.Format(Loc.Err_EnumNameNotCamelCase, packageName, blockName) + "\n");
            }

            info.Name = blockName;
            // Console.WriteLine("Enum Name: " + match.Groups[1].Value);
            // Console.WriteLine("Contents: " + match.Groups[2].Value);
            var blockContent = match.Groups[2].Value.Trim();
            foreach (var line in blockContent.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.TrimStart().StartsWith("//"))
                {
                    // 这个字段被注释
                    continue;
                }

                MessageMember field = new MessageMember(true);
                info.Fields.Add(field);
                // 解析注释
                var lineSplit = line.Split("//", StringSplitOptions.RemoveEmptyEntries);
                if (lineSplit.Length > 1)
                {
                    // 有注释
                    field.Description = lineSplit[1].Trim();
                }

                if (lineSplit.Length > 0)
                {
                    var fieldType = lineSplit[0].Trim().Trim(';');
                    var fieldSplit = fieldType.Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (fieldSplit.Length > 1)
                    {
                        var name = fieldSplit[0].Trim();
                        if (!Utility.IsCamelCase(name))
                        {
                            throw new Exception(string.Format(Loc.Err_EnumFieldNotCamelCase, packageName, name) + "\n");
                        }

                        field.Type = name;
                        int member = int.Parse(fieldSplit[1].Replace(";", "").Trim());
                        if (!CheckVerifyMember(info.Fields, member) && member != 0)
                        {
                            throw new Exception(string.Format(Loc.Err_EnumMemberDuplicated, packageName, name, member));
                        }

                        field.Members = member;
                    }
                }
            }
        }
    }

    private static void ParseMessage(string proto, string packageName, List<MessageInfo> codes, bool isGenerateErrorCode = false)
    {
        MatchCollection messageMatches = Regex.Matches(proto, MessagePattern, RegexOptions.Singleline);
        foreach (Match match in messageMatches)
        {
            string messageName = match.Groups[1].Value;
            var blockContent = match.Groups[2].Value.Trim();
            MessageInfo info = new MessageInfo();
            codes.Add(info);
            if (!Utility.IsCamelCase(messageName))
            {
                throw new Exception(string.Format(Loc.Err_MessageNameNotCamelCase, packageName, messageName) + "\n");
            }

            info.Name = messageName;
            foreach (var line in blockContent.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.TrimStart().StartsWith("//"))
                {
                    // 这个字段被注释
                    continue;
                }

                MessageMember field = new MessageMember();
                info.Fields.Add(field);
                // 解析注释
                var lineSplit = line.Split("//", StringSplitOptions.RemoveEmptyEntries);
                if (lineSplit.Length > 1)
                {
                    // 有注释
                    field.Description = lineSplit[1].Trim();
                }

                // 字段
                if (lineSplit.Length > 0)
                {
                    var fieldSplit = lineSplit[0].Split('=', StringSplitOptions.RemoveEmptyEntries);
                    if (fieldSplit.Length > 1)
                    {
                        var members = int.Parse(fieldSplit[1].Replace(";", "").Trim());
                        if (!CheckVerifyMember(info.Fields, members))
                        {
                            throw new Exception(string.Format(Loc.Err_MessageMemberDuplicated, packageName, messageName));
                        }

                        field.Members = members;
                    }

                    if (fieldSplit.Length > 0)
                    {
                        var fieldSplitStrings = fieldSplit[0].Split(Utility.splitChars, StringSplitOptions.RemoveEmptyEntries);
                        var key = fieldSplitStrings[0].Trim();
                        if (key.Trim().StartsWith("map") && fieldSplitStrings.Length < 3)
                        {
                            throw new Exception(string.Format(Loc.Err_MapCommaMissing, packageName, messageName, key) + "\n");
                        }

                        if (fieldSplitStrings.Length > 2)
                        {
                            if (key.Trim().StartsWith("repeated"))
                            {
                                field.IsRepeated = true;
                                field.Type = fieldSplitStrings[1].Trim();
                            }
                            else
                            {
                                field.Type = key + fieldSplitStrings[1].Trim();
                                if (key.Trim().StartsWith("map"))
                                {
                                    field.IsKv = true;
                                }
                            }

                            var name = fieldSplitStrings[2].Trim();

                            if (!Utility.IsCamelCase(name))
                            {
                                throw new Exception(string.Format(Loc.Err_FieldNotCamelCase, packageName, messageName, name) + "\n");
                            }

                            field.Name = name;
                        }
                        else if (fieldSplitStrings.Length > 1)
                        {
                            field.Type = fieldSplitStrings[0].Trim();
                            var name = fieldSplitStrings[1].Trim();
                            if (!Utility.IsCamelCase(name))
                            {
                                throw new Exception(string.Format(Loc.Err_FieldNotCamelCase, packageName, messageName, name) + "\n");
                            }

                            field.Name = name;
                        }
                    }
                }
            }

            if (isGenerateErrorCode && info.IsResponse && !info.IsNotify)
            {
                MessageMember field = new MessageMember();
                field.Description = "返回的错误码";
                field.Name = "ErrorCode";
                field.Type = "int32";
                field.Members = 2047;
                info.Fields.Add(field);
            }
        }
    }

    /// <summary>
    /// 检查tag 是否验证通过
    /// </summary>
    /// <param name="info"></param>
    /// <param name="members"></param>
    /// <returns></returns>
    static bool CheckVerifyMember(List<MessageMember> info, int members)
    {
        foreach (var messageMember in info)
        {
            if (messageMember.Members == members)
            {
                return false;
            }
        }

        return true;
    }
}