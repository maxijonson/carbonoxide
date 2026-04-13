using MyCarbonoxidePlugin.Plugin;

namespace MyCarbonoxidePlugin.Config;

public partial class PluginPermissionsConfig
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Use))
        {
            MyCarbonoxide.Instance.PrintWarning("Use permission is null or whitespace, restoring defaults.");
            Use = "mycarbonoxide.use";
        }

        if (string.IsNullOrWhiteSpace(Admin))
        {
            MyCarbonoxide.Instance.PrintWarning("Admin permission is null or whitespace, restoring defaults.");
            Admin = "mycarbonoxide.admin";
        }
    }
}
