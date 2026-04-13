using System.Collections.Generic;
using MyCarbonoxidePlugin.Entities;
using MyCarbonoxidePlugin.Interfaces;
using Newtonsoft.Json;
using Oxide.Core;

namespace MyCarbonoxidePlugin.Data;

[JsonObject(MemberSerialization.OptIn)]
public partial class PlayerPreferencesData : IVersionable
{
    [JsonProperty(PropertyName = "players")]
    public Dictionary<string, PlayerPreferences> Players { get; set; } = new();

    [JsonProperty(PropertyName = "version")]
    public VersionNumber Version { get; set; }
}
