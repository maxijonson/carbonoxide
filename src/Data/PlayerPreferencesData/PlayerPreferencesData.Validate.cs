using System;
using System.Linq;
using MyCarbonoxidePlugin.Interfaces;
using MyCarbonoxidePlugin.Plugin;

namespace MyCarbonoxidePlugin.Data;

public partial class PlayerPreferencesData : IValidatable
{
    public IValidatable.Result Validate(IValidatable.Phase phase)
    {
        var result = IValidatable.Result.Valid;

        foreach (var (playerId, prefs) in Players.ToList())
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                MyCarbonoxide.Instance.PrintError(
                    "Found a PlayerPreferences entry with an empty or whitespace player ID. This entry will be removed."
                );
                Players.Remove(playerId);
                result = IValidatable.Combine(result, IValidatable.Result.Repaired);
                continue;
            }

            if (prefs.UiScale < 0.5f || prefs.UiScale > 1.0f)
            {
                MyCarbonoxide.Instance.PrintWarning(
                    $"Player {playerId} has an invalid UiScale of {prefs.UiScale}. Clamping to valid range."
                );
                prefs.UiScale = Math.Clamp(prefs.UiScale, 0.5f, 1.0f);
                result = IValidatable.Combine(result, IValidatable.Result.Repaired);
            }
        }

        return result;
    }
}
