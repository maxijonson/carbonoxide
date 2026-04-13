using System.Collections.Generic;
using Oxide.Core.Libraries;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public new Permission permission => base.permission;

    private void RegisterPermissions()
    {
        var permissions = new HashSet<string> { Settings.Permissions.Use, Settings.Permissions.Admin };

        foreach (var perm in permissions)
        {
            if (string.IsNullOrWhiteSpace(perm))
                continue;

            if (!permission.PermissionExists(perm, this))
                permission.RegisterPermission(perm, this);
        }
    }
}
