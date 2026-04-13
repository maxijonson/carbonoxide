using System.Collections.Generic;
using MyCarbonoxidePlugin.Interfaces;
using Newtonsoft.Json;
using Oxide.Core;

namespace MyCarbonoxidePlugin.Config;

[JsonObject(MemberSerialization.OptIn)]
public partial class PluginConfig : IVersionable
{
    [JsonProperty(PropertyName = "Command")]
    public HashSet<string> Command { get; set; } = new() { "mycarbonoxide" };

    [JsonProperty(PropertyName = "Permissions")]
    public PluginPermissionsConfig Permissions { get; set; } = new();

    [JsonProperty(PropertyName = "Version")]
    public VersionNumber Version { get; set; }
}
