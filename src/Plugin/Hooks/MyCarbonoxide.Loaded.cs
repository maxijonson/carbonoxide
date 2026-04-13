using System;
using Oxide.Core;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    private void Loaded()
    {
        try
        {
            LoadData();
        }
        catch (Exception ex)
        {
            PrintError(
                $"Error loading data and could not recover safely. Plugin will be unloaded. Please check your data files and fix any errors."
            );
            PrintError(ex.ToString());
            NextTick(() => Interface.Oxide.UnloadPlugin(Name));
        }
    }
}
