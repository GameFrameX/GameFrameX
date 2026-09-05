using System.IO;
using GameFrameX.Foundation.Options;
using GameFrameX.ProtoExport.Persistence;

namespace GameFrameX.ProtoExport
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var launcherOptions = new OptionsBuilder<LauncherOptions>(args).Build();
                if (launcherOptions == null)
                {
                    Console.WriteLine(Loc.Log_ArgsParseError);
                    throw new Exception(Loc.Log_ArgsParseError);
                }

                // --print-lock：只读 lock 文件并打印，不触发任何导出。便于 review。
                if (args.Any(a => a == "--print-lock"))
                {
                    var path = ResolveLockPath(launcherOptions);
                    var data = File.Exists(path)
                        ? MessageIdLockStore.Load(path)
                        : MessageIdLock.CreateEmpty();
                    Console.WriteLine(Persistence.LockSeedGenerator.FormatLockForDisplay(data));
                    return 0;
                }

                // --regenerate-lock：用当前 proto 解析出的 Opcode 序列化为 lock 种子（一次性迁移）。
                if (args.Any(a => a == "--regenerate-lock"))
                {
                    var path = ResolveLockPath(launcherOptions);
                    SeedLockFromCurrent(launcherOptions);
                    Console.WriteLine(string.Format(Loc.Log_LockSeedGenerated, path));
                    return 0;
                }

                if (!Enum.TryParse<ModeType>(launcherOptions.Mode, true, out var modeType))
                {
                    Console.WriteLine(Loc.Log_UnsupportedMode);
                    throw new Exception(Loc.Log_UnsupportedMode);
                }

                ProtoBufMessageHandler.Start(launcherOptions, modeType);
                Console.WriteLine(Loc.Log_ExportSuccess);
            }
            catch (Exception e)
            {
                Console.WriteLine(Loc.Log_ExportFailed);
                Console.WriteLine(e);
                throw;
            }

            return 0;
        }

        private static string ResolveLockPath(LauncherOptions launcherOptions)
        {
            if (!string.IsNullOrWhiteSpace(launcherOptions.MessageIdLockPath))
            {
                return launcherOptions.MessageIdLockPath;
            }

            throw new InvalidOperationException(
                "使用 --print-lock / --regenerate-lock 时必须同时指定 --messageIdLockPath");
        }

        private static void SeedLockFromCurrent(LauncherOptions launcherOptions)
        {
            if (string.IsNullOrWhiteSpace(launcherOptions.InputPath) || !Directory.Exists(launcherOptions.InputPath))
            {
                throw new DirectoryNotFoundException(string.Format(Loc.Err_InputPathNotExist, launcherOptions.InputPath));
            }

            var files = Directory.GetFiles(launcherOptions.InputPath, "*.proto", SearchOption.AllDirectories);
            var lists = new List<MessageInfoList>(files.Length);

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var isServerOnly = fileName.EndsWith("-s") || fileName.EndsWith("_s");
                if (!launcherOptions.IsServer && isServerOnly)
                {
                    continue;
                }

                var info = MessageHelper.Parse(File.ReadAllText(file), fileName, launcherOptions.OutputPath, launcherOptions.IsGenerateErrorCode);
                if (!launcherOptions.IsServer && info.Module < 0)
                {
                    continue;
                }

                lists.Add(info);
            }

            Persistence.LockSeedGenerator.SeedFromCurrentOpcodes(launcherOptions.MessageIdLockPath, lists);
        }
    }
}
