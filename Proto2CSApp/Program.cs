using Proto2CS.Editor;
using System;
using System.Threading;

namespace GameFrameX
{
    internal static class Program
    {
        private static IProtoGenerateHelper ProtoGenerateHelper;
        static string protoPath = "";
        static string filePath = "";
        static string namespaceStr = "";

        private static int Main(string[] args)
        {
            // 检查是否传递了足够的参数
            if (args.Length < 4)
            {
                Console.WriteLine("参数不足。正确的格式为：Program.exe [protoPath] [server/client] [filePath] [namespace]");
                return 0;
            }

            // 解析参数
            protoPath = args[0];
            string mode = args[1].ToLower(); // 将模式参数转为小写，以便比较
            filePath = args[2];
            namespaceStr = args[3];

            // 验证模式参数是否正确
            if (!Enum.TryParse<ModeType>(mode, true, out var modeType))
            {
                Console.WriteLine("错误：第二个参数必须是 'server' 或 'client'");
                return 0;
            }

            // 验证文件路径是否提供
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("错误：文件路径不能为空");
                return 0;
            }

            // 显示接收到的参数信息
            Console.WriteLine($"协议文件路径: {protoPath}");
            Console.WriteLine($"运行模式: {mode}");
            Console.WriteLine($"文件路径: {filePath}");

            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var types = typeof(IProtoGenerateHelper).Assembly.GetTypes();
            foreach (var type in types)
            {
                var attrs = type.GetCustomAttributes(typeof(ModeAttribute), true);
                if (attrs?.Length > 0 && (attrs[0] is ModeAttribute modeAttribute) && modeAttribute.Mode == modeType)
                {
                    ProtoGenerateHelper = (IProtoGenerateHelper)Activator.CreateInstance(type);
                    break;
                }
            }

            var files = Directory.GetFiles(protoPath, "*.proto", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var operationCodeInfo = MessageHelper.Parse(File.ReadAllText(file), Path.GetFileNameWithoutExtension(file), filePath);
                ProtoGenerateHelper?.Run(operationCodeInfo, filePath, Path.GetFileNameWithoutExtension(file));
            }

            return 0;
        }
    }
}