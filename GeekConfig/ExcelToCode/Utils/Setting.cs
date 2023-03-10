using ExcelToCode.Excel;
using NLog;
using NLog.Config;
using System.Xml;

namespace ExcelToCode
{
    public static class Setting
    {

        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 普通表缓冲区大小-512KB
        /// </summary>
        public static int NormalBufferSize = 512 * 1024;

        /// <summary>
        /// 语言包缓冲区大小-5M
        /// </summary>
        public static int LanguageBufferSize = 5 * 1024 * 1024;

        /// <summary>
        /// 配置表路径
        /// </summary>
        public static string ConfigPath { private set; get; }


        /// <summary>
        /// 客户端Bin路径
        /// </summary>
        public static string ClientBinPath { private set; get; }

        /// <summary>
        /// 服务器二进制路径
        /// </summary>
        public static string ServerBinPath { private set; get; }

        /// <summary>
        /// 客户端代码路径
        /// </summary>
        public static string ClientCodePath { private set; get; }

        /// <summary>
        /// 服务器代码路径
        /// </summary>
        public static string ServerCodePath { private set; get; }

        public static bool Init()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("Configs/NLog.config");
            LOGGER.Info("Init tool config...");

            if (File.Exists("Configs/config.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("Configs/config.xml");
                XmlElement root = doc.DocumentElement;
                XmlNode listNodes = root.SelectNodes("/config").Item(0);
                foreach (XmlNode node in listNodes)
                {
                    switch (node.Name)
                    {
                        case "normal-buffer-size":
                            NormalBufferSize = int.Parse(node.InnerText);
                            break;
                        case "lan-buffer-size":
                            LanguageBufferSize = int.Parse(node.InnerText);
                            break;
                        case "config-path":
                            ConfigPath = node.InnerText;
                            break;
                        case "server-code-path":
                            ServerCodePath = node.InnerText;
                            break;
                        case "server-bin-path":
                            ServerBinPath = node.InnerText;
                            break;
                        case "client-code-path":
                            ClientCodePath = node.InnerText;
                            break;
                        case "client-bin-path":
                            ClientBinPath = node.InnerText;
                            break;
                    }
                }
                return true;
            }
            else
            {
                LOGGER.Error("服务器配置文件错误或不存在,启动失败!");
                return false;
            }
        }


        public static string GetBinPath(ExportType etype)
        {
            if (etype == ExportType.Client)
            {
                return @ClientBinPath + "/";
            }
            else if (etype == ExportType.Server)
            {
                return @ServerBinPath + "/";
            }
            LOGGER.Error("未知的导出类型{}", etype);
            return "";
        }

        public static string GetCodePath(ExportType etype)
        {
            if (etype == ExportType.Client)
            {
                return @ClientCodePath + "/";
            }
            else if (etype == ExportType.Server)
            {
                return @ServerCodePath + "/";
            }
            LOGGER.Error("未知的导出类型{}", etype);
            return "";
        }

        public static string GetTemplatePath(ExportType etype)
        {
            if (etype == ExportType.Client)
            {
                return @"Configs/Template/Client/";
            }
            else if (etype == ExportType.Server)
            {
                return @"Configs/Template/Server/";
            }
            LOGGER.Error("未知的导出类型{}", etype);
            return "";
        }


    }
}
