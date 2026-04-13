namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public new void Puts(string format, params object[] args)
    {
        base.Puts(string.Format(format, args));
    }

    public new void PrintWarning(string format, params object[] args)
    {
        base.PrintWarning(string.Format(format, args));
    }

#pragma warning disable CS0109 // Carbon warning
    public new void PrintError(string format, params object[] args)
#pragma warning restore CS0109
    {
        base.PrintError(string.Format(format, args));
    }
}
