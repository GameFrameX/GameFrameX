using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Newtonsoft.Json;


public class Setting
{

    public string BaseMessageName { set; get; }

    /// <summary>
    /// Proto 工程路径
    /// </summary>
    public string ServerProtoPath { set; get; }

    /// <summary>
    /// 服务器代码导出路径
    /// </summary>
    public string ServerOutPath { set; get; }
    public string ClientFormatNameSpace { get; set; }

    public string ServerNameSpace { get; set; }
    
    public string ClientProtoPath { set; get; }
    /// <summary>
    /// 客户端代码导出路径
    /// </summary>
    public string ClientOutPath { set; get; }

    /// <summary>
    /// 服务器是否使用代码生成的Resovler
    /// </summary>
    public bool ServerIsGenerated { set; get; }

    public List<string> NoExportList { set; get; } = new List<string>();

    public static Setting Ins { get; private set; }

    public static bool Init()
    {
        var cfgPath = "Configs/config.json";
        if (File.Exists(cfgPath))
        {
            Ins = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(cfgPath));
            return true;
        }
        else
        {
            Console.WriteLine("服务器配置文件错误或不存在,启动失败!");
            return false;
        }
    }
}