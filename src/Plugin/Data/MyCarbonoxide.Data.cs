using MyCarbonoxidePlugin.Migrations;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public bool IsDataLoaded = false;

    private void LoadData()
    {
        LoadPlayerPreferencesData();
        IsDataLoaded = true;
        Migration.Unload();
    }
}
