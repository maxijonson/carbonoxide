using MyCarbonoxidePlugin.Data;
using MyCarbonoxidePlugin.Migrations;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Configuration;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public PlayerPreferencesData Preferences = null!;
    DynamicConfigFile? PreferencesFile;

    public DynamicConfigFile GetPlayerPreferencesDataFile()
    {
        var file = Interface.Oxide.DataFileSystem.GetFile("MyCarbonoxide/player_preferences_data");
        file.Settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        return file;
    }

    public void LoadPlayerPreferencesData()
    {
        PreferencesFile = GetPlayerPreferencesDataFile();
        LoadDataFile(
            ref Preferences,
            PreferencesFile,
            GetDefaultPlayerPreferencesData,
            SavePlayerPreferencesData,
            MigrationFileType.Preferences
        );
    }

    public void SavePlayerPreferencesData()
    {
        PreferencesFile ??= GetPlayerPreferencesDataFile();
        try
        {
            if (Preferences is not null)
                PreferencesFile.WriteObject(Preferences);
        }
        finally
        {
            PreferencesFile.Clear();
        }
    }
}
