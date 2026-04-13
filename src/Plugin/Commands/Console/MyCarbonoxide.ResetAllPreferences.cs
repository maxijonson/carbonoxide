using Oxide.Plugins;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    [ConsoleCommand("carbonoxide.reset_all_preferences")]
    private void CCmdResetAllPreferences(ConsoleSystem.Arg arg)
    {
        if (!IsAdmin(arg))
            return;
        Preferences.Players.Clear();
        arg.ReplyWith("All player preferences have been reset.");
    }
}
