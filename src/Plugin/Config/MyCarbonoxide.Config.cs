using MyCarbonoxidePlugin.Config;
using MyCarbonoxidePlugin.Migrations;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public PluginConfig Settings = new();

    private PluginConfig GetDefaultConfig()
    {
        Puts("Creating default config...");
        return new PluginConfig { Version = Version };
    }

    protected override void LoadConfig()
    {
        base.LoadConfig();

        Config.Settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        LoadDataFile(ref Settings, Config, GetDefaultConfig, SaveConfig, MigrationFileType.Config);
        SaveConfig();
    }

    protected override void LoadDefaultConfig()
    {
        Settings = GetDefaultConfig();
    }

    protected override void SaveConfig()
    {
        Config.WriteObject(Settings);
    }
}
