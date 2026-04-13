using System.Linq;
using MyCarbonoxidePlugin.Plugin;

namespace MyCarbonoxidePlugin.Config;

public partial class PluginConfig
{
    public void Validate()
    {
        foreach (var cmd in Command.ToList())
        {
            if (string.IsNullOrWhiteSpace(cmd))
            {
                MyCarbonoxide.Instance.PrintWarning("Empty command found, removing");
                Command.Remove(cmd);
            }
        }
        if (Command is null || Command.Count == 0)
        {
            MyCarbonoxide.Instance.PrintWarning("No commands specified, restoring defaults.");
            Command = new() { "mycarbonoxide" };
        }

        if (Permissions is null)
        {
            MyCarbonoxide.Instance.PrintWarning("Permissions is null, restoring defaults.");
            Permissions = new();
        }
        Permissions.Validate();
    }
}
