using System.Text.Json;
using TrProtocol.Models;
using TShockAPI;
namespace Fplayer;

public class Config
{
    public static Config Instance { get; private set; } = new();
    
    public DummyInfo[] Dummys { get; set; } = Array.Empty<DummyInfo>();

    public Config()
    {
        SetDefault();
    }

    private void SetDefault()
    {
        this.Dummys = new DummyInfo[1];
        this.Dummys[0] = new DummyInfo() { Name = "熙恩" };
    }

    public static void Read()
    {
        var configPath = Path.Combine(TShock.SavePath, "fplayer.json");
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                Instance = JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            catch (Exception ex)
            {
                TShock.Log.Error($"读取配置文件失败: {ex.Message}");
                Instance = new Config();
            }
        }
        else
        {
            Instance = new Config();
            Save();
        }
    }

    public static void Save()
    {
        try
        {
            var configPath = Path.Combine(TShock.SavePath, "fplayer.json");
            var json = JsonSerializer.Serialize(Instance, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }
        catch (Exception ex)
        {
            TShock.Log.Error($"保存配置文件失败: {ex.Message}");
        }
    }
}

public class DummyInfo
{
    public string Password { get; set; } = string.Empty;
    public string UUID { get; set; } = Guid.NewGuid().ToString();
    public byte SkinVariant { get; set; }
    public byte Hair { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte HairDye { get; set; }
    public byte HideMisc { get; set; }
    public Color HairColor { get; set; }
    public Color SkinColor { get; set; }
    public Color EyeColor { get; set; }
    public Color ShirtColor { get; set; }
    public Color UnderShirtColor { get; set; }
    public Color PantsColor { get; set; }
    public Color ShoeColor { get; set; }
}