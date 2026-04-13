using Oxide.Plugins;

namespace MyCarbonoxidePlugin.Plugin;

[Info("MyCarbonoxide", "<Author>", "1.0.0")]
[Description("Description")]
public partial class MyCarbonoxide : RustPlugin
{
    public static MyCarbonoxide Instance { get; private set; } = null!;

    public MyCarbonoxide()
    {
        Instance = this;
    }
}
