using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MouthKing.Core;
public class ConfigurationManager
{
    private const string CONFIG_FILE = "appsettings.json";
    private static AppConfig _config;

    public static AppConfig Config => _config ??= LoadConfig();

    public static AppConfig LoadConfig()
    {
        try
        {
            if (File.Exists(CONFIG_FILE))
            {
                var json = File.ReadAllText(CONFIG_FILE);
                return JsonSerializer.Deserialize(json, AotJsonSerializerContext.Default.AppConfig) ?? new AppConfig();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"加载配置文件失败: {ex.Message}");
        }

        return new AppConfig();
    }

    public static void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, AotJsonSerializerContext.Default.AppConfig);
            File.WriteAllText(CONFIG_FILE, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"保存配置文件失败: {ex.Message}");
        }
    }
}