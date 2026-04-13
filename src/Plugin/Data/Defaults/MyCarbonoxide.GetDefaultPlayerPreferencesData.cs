using MyCarbonoxidePlugin.Data;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public PlayerPreferencesData GetDefaultPlayerPreferencesData()
    {
        Puts("Creating default player preferences data...");
        return new PlayerPreferencesData() { Version = Version };
    }
}
