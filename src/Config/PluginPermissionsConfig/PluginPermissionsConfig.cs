using Newtonsoft.Json;

namespace MyCarbonoxidePlugin.Config;

[JsonObject(MemberSerialization.OptIn)]
public partial class PluginPermissionsConfig
{
    [JsonProperty(PropertyName = "Use MyCarbonoxide")]
    public string Use { get; set; } = "mycarbonoxide.use";

    [JsonProperty(PropertyName = "Admin")]
    public string Admin { get; set; } = "mycarbonoxide.admin";
}
