using UnityEngine;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public bool CanUseMyCarbonoxidePermission(string playerId, string perm, string? defaultPerm)
    {
        if (string.IsNullOrWhiteSpace(perm))
        {
            if (string.IsNullOrWhiteSpace(defaultPerm))
                return true;
            if (!permission.PermissionExists(defaultPerm))
                permission.RegisterPermission(defaultPerm, this);
            return permission.UserHasPermission(playerId, defaultPerm);
        }
        return permission.UserHasPermission(playerId, perm);
    }

    public bool CanUseMyCarbonoxide(string playerId)
    {
        return CanUseMyCarbonoxidePermission(playerId, Settings.Permissions.Use, "mycarbonoxide.use");
    }

    public bool IsMyCarbonoxideAdmin(string playerId)
    {
        return CanUseMyCarbonoxidePermission(playerId, Settings.Permissions.Admin, "mycarbonoxide.admin");
    }

    public bool IsAdmin(BasePlayer player)
    {
        if (player is null)
            return false;
        return player.IsAdmin || IsMyCarbonoxideAdmin(player.UserIDString);
    }

    public bool IsAdmin(ConsoleSystem.Arg arg)
    {
        if (arg is null)
            return false;
        if (arg.IsAdmin)
            return true;

        var player = arg.Player();
        if (player is null)
            return false;
        return IsAdmin(player);
    }
}
