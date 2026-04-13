namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    private void RegisterCommands()
    {
        foreach (var command in Settings.Command)
        {
            cmd.AddChatCommand(command, this, MyCarbonoxideChatCommand);
        }
    }
}
