using MyCarbonoxidePlugin.Lang;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    private void MyCarbonoxideChatCommand(BasePlayer player, string command, string[] args)
    {
        if (!CanUseMyCarbonoxide(player.UserIDString))
        {
            SendReply(player, m(LangKeys.Commands.Unauthorized, player.UserIDString));
            return;
        }
    }
}
