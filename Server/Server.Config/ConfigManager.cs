using System.Text.Json;
using cfg;

namespace Server.Config;

public class ConfigManager
{
    public static ConfigManager Instance { get; } = new ConfigManager();
    public Tables Tables { get; private set; }

    public void LoadConfig()
    {
        Tables = new cfg.Tables(Loader);
        Console.WriteLine("== load success ==");
    }

    private static JsonElement Loader(string file)
    {
        var configJson = File.ReadAllBytes($"json/{file}.json");
        JsonElement jsonElement = JsonDocument.Parse(configJson).RootElement;
        return jsonElement;
    }
}