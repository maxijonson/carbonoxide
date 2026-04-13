using System;
using System.Linq;
using Oxide.Core.Configuration;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public string GetFilename(DynamicConfigFile dataFile)
    {
        return dataFile.Filename.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
    }

    public string GetBackupFilename(DynamicConfigFile dataFile)
    {
        var filename = GetFilename(dataFile);
        return $"MyCarbonoxide/backups/{filename.Split(".json")[0]}/{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}";
    }
}
