namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public enum ModFramework
    {
        Oxide,
        Carbon,
    }

#if CARBON
    public ModFramework Framework { get; } = ModFramework.Carbon;
#else
    public ModFramework Framework { get; } = ModFramework.Oxide;
#endif

    public bool IsCarbon => Framework == ModFramework.Carbon;
    public bool IsOxide => Framework == ModFramework.Oxide;
}
