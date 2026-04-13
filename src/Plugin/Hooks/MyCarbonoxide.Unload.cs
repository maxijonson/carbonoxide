using MyCarbonoxidePlugin.Migrations;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    private void Unload()
    {
        if (IsDataLoaded)
        {
            // Don't save if data isn't loaded to avoid saving when data is in an inconsistent state due to a loading error
            // SaveData()
        }
        Migration.Unload();
        Instance = null!;
    }
}
