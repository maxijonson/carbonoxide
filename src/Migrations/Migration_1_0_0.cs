using Newtonsoft.Json.Linq;
using Oxide.Core;

namespace MyCarbonoxidePlugin.Migrations;

public class Migration_1_0_0 : Migration
{
    // Migration summary:
    // This is a stub migration example that demonstrates how to use the migration system to edit JObjects during a migraiton.

    public override VersionNumber Version => new(1, 0, 0);

    public override void Migrate(JObject obj, MigrationFileType type)
    {
        if (type == MigrationFileType.Preferences)
        {
            MigratePreferences(obj);
        }
    }

    private void MigratePreferences(JObject obj)
    {
        // Rename "PlayerPreferences" to "Players"
        if (obj.TryGetValue("PlayerPreferences", out var playerPrefsToken))
        {
            obj["Players"] = playerPrefsToken;
            obj.Remove("PlayerPreferences");
        }
    }
}
