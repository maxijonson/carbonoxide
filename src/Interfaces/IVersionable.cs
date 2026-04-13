using Oxide.Core;

namespace MyCarbonoxidePlugin.Interfaces;

public interface IVersionable
{
    VersionNumber Version { get; set; }
}
