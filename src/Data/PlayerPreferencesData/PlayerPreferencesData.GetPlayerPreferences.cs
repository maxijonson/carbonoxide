using MyCarbonoxidePlugin.Entities;

namespace MyCarbonoxidePlugin.Data;

public partial class PlayerPreferencesData
{
    public PlayerPreferences GetPlayerPreferences(string playerId)
    {
        if (!Players.TryGetValue(playerId, out var prefs))
        {
            prefs = new PlayerPreferences();
            Players[playerId] = prefs;
        }
        return prefs;
    }
}
