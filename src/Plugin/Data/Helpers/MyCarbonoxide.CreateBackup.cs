using System;
using Oxide.Core;
using Oxide.Core.Configuration;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public string? CreateBackup(DynamicConfigFile dataFile)
    {
        var backupFilename = GetBackupFilename(dataFile);
        var backupFile = Interface.Oxide.DataFileSystem.GetFile(backupFilename);
        var filename = GetFilename(dataFile);
        try
        {
            backupFile.WriteObject(dataFile.ReadObject<object>());
            PrintWarning(
                $"Created backup of 'oxide/data/MyCarbonoxide/{filename}' at 'oxide/data/{backupFilename}.json' ."
            );
            return backupFilename;
        }
        catch (Exception backupEx)
        {
            PrintError($"Failed to create backup of 'oxide/data/MyCarbonoxide/{filename}'.");
            PrintError(backupEx.ToString());
        }
        finally
        {
            backupFile.Clear();
        }
        return null;
    }
}
