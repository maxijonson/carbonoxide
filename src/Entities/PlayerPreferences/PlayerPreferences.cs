using Newtonsoft.Json;

namespace MyCarbonoxidePlugin.Entities;

[JsonObject(MemberSerialization.OptIn)]
public partial class PlayerPreferences
{
    [JsonProperty(PropertyName = "uiScale")]
    public float UiScale { get; set; } = 1.0f;
}
