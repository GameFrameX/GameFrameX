namespace GameFrameX.ProtoExport;

public static class ProtoBufMessageHandler
{
    public static void Start(LauncherOptions launcherOptions, ModeType modeType)
    {
        // 先验证输入参数，再删除输出目录
        if (string.IsNullOrWhiteSpace(launcherOptions.InputPath) || !Directory.Exists(launcherOptions.InputPath))
        {
            throw new DirectoryNotFoundException(string.Format(Loc.Err_InputPathNotExist, launcherOptions.InputPath));
        }

        IProtoGenerateHelper protoGenerateHelper = null;
        var types = typeof(IProtoGenerateHelper).Assembly.GetTypes();
        foreach (var type in types)
        {
            var attrs = type.GetCustomAttributes(typeof(ModeAttribute), true);
            if (attrs?.Length > 0 && (attrs[0] is ModeAttribute modeAttribute) && modeAttribute.Mode == modeType)
            {
                protoGenerateHelper = (IProtoGenerateHelper)Activator.CreateInstance(type);
                break;
            }
        }

        if (protoGenerateHelper == null)
        {
            throw new NotSupportedException(string.Format(Loc.Err_UnsupportedModeType, modeType, string.Join(", ", Enum.GetNames<ModeType>())));
        }

        protoGenerateHelper.Init(launcherOptions);

        // 参数验证通过后再清理并创建输出目录
        var outputDirectoryInfo = new DirectoryInfo(launcherOptions.OutputPath);
        if (outputDirectoryInfo.Exists)
        {
            outputDirectoryInfo.Delete(true);
        }

        outputDirectoryInfo.Create();

        launcherOptions.OutputPath = outputDirectoryInfo.FullName;

        var files = Directory.GetFiles(launcherOptions.InputPath, "*.proto", SearchOption.AllDirectories);

        // 若指定了 lock 文件路径，关闭 Parse 内的自增分配 —— Opcode 留 0，
        // 后续由 MessageIdCoordinator 统一按 lock 分配，再让 helper 消费。
        var useLock = !string.IsNullOrWhiteSpace(launcherOptions.MessageIdLockPath);
        if (useLock)
        {
            MessageHelper.SkipAutoAssignOpcode = true;
        }

        try
        {
            var messageInfoLists = new List<MessageInfoList>(files.Length);
            var skippedCount = 0;

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                // 客户端构建跳过服务器内部协议文件（_s/-s 后缀），仅服务器导出
                var isServerOnly = fileName.EndsWith("-s") || fileName.EndsWith("_s");
                if (!launcherOptions.IsServer && isServerOnly)
                {
                    ExportLogger.WriteLine(string.Format(Loc.Log_SkipServerOnlyFile, fileName));
                    skippedCount++;
                    continue;
                }

                var operationCodeInfo = MessageHelper.Parse(File.ReadAllText(file), fileName, launcherOptions.OutputPath, launcherOptions.IsGenerateErrorCode);

                // 客户端构建跳过模块 id 小于 0 的内部协议（如 Inner*），仅服务器导出
                if (!launcherOptions.IsServer && operationCodeInfo.Module < 0)
                {
                    ExportLogger.WriteLine(string.Format(Loc.Log_SkipInternalModule, operationCodeInfo.Module, fileName));
                    skippedCount++;
                    continue;
                }

                if (launcherOptions.CommentValidation != CommentValidationLevel.None)
                {
                    CommentValidator.Validate(operationCodeInfo, launcherOptions.CommentValidation);
                }

                messageInfoLists.Add(operationCodeInfo);
            }

            // 分配 SubId（lock 模式）或跳过（自增模式保持旧行为）
            if (useLock)
            {
                var result = Persistence.MessageIdCoordinator.AssignAndPersist(launcherOptions.MessageIdLockPath, messageInfoLists);
                ExportLogger.WriteLine(string.Format(Loc.Log_LockSummary, result.ModuleCount, result.NewlyAssignedCount, string.Join(", ", result.NewlyAssigned)));
            }

            ExportLogger.WriteLine(string.Format(Loc.Log_ScanCompleted, files.Length, messageInfoLists.Count, skippedCount,
                launcherOptions.IsServer ? Loc.Term_ServerMode : Loc.Term_ClientMode));

            // Opcode 已确定，调用各 helper 生成代码
            foreach (var list in messageInfoLists)
            {
                protoGenerateHelper.Run(list, launcherOptions.OutputPath, launcherOptions.NamespaceName);
            }

            protoGenerateHelper.Post(messageInfoLists, launcherOptions);
        }
        finally
        {
            if (useLock)
            {
                MessageHelper.SkipAutoAssignOpcode = false;
            }
        }
    }
}
