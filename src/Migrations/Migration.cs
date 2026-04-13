using System;
using System.Collections.Generic;
using System.Linq;
using MyCarbonoxidePlugin.Plugin;
using Newtonsoft.Json.Linq;
using Oxide.Core;

namespace MyCarbonoxidePlugin.Migrations;

public enum MigrationFileType
{
    Config,
    Preferences,
}

public abstract class Migration
{
    private static List<Migration>? _migrations;
    private static List<Migration> Migrations
    {
        get
        {
            if (_migrations is not null)
                return _migrations;

            _migrations = new List<Migration> { new Migration_1_0_0() }
                .OrderBy(m => m.Version)
                .ToList();
            return _migrations;
        }
    }

    public abstract VersionNumber Version { get; }

    public abstract void Migrate(JObject obj, MigrationFileType type);

    public static void Unload()
    {
        _migrations?.Clear();
        _migrations = null;
    }

    public static JObject RunMigrations(JObject obj, MigrationFileType type, VersionNumber toVersion)
    {
        var currentVersion = GetVersion(obj);

        var minSupportedVersion = new VersionNumber(0, 0, 0);
        if (currentVersion < minSupportedVersion)
            throw new Exception(
                $"{type} Data file version {currentVersion} is too old to be migrated. Minimum supported version is {minSupportedVersion}. Download an older version of the plugin that supports migrating from version {currentVersion} or manually update the data file version to {minSupportedVersion} to enable migration (not recommended)."
            );

        foreach (var migration in Migrations)
        {
            if (migration.Version <= currentVersion || migration.Version > toVersion)
                continue;

            migration.Migrate(obj, type);
            SetVersion(obj, migration.Version);
            MyCarbonoxide.Instance.Puts($"{type} Migration Applied: {currentVersion} -> {migration.Version}");
            currentVersion = migration.Version;
        }
        SetVersion(obj, toVersion);
        MyCarbonoxide.Instance.Puts($"{type} Data file successfully migrated to version {toVersion}.");

        return obj;
    }

    public static VersionNumber GetVersion(JObject obj)
    {
        var versionProp = obj.Property("version") ?? obj.Property("Version");
        if (versionProp is null || versionProp.Value is not JObject v)
            throw new Exception("Version property is missing or invalid in data file.");

        return new VersionNumber(v.Value<int>("Major"), v.Value<int>("Minor"), v.Value<int>("Patch"));
    }

    public static void SetVersion(JObject obj, VersionNumber version)
    {
        var versionProp = obj.Property("version") ?? obj.Property("Version");
        if (versionProp is null)
        {
            versionProp = new JProperty("version", null);
            obj.Add(versionProp);
        }

        versionProp.Value = JObject.FromObject(
            new
            {
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Patch,
            }
        );
    }
}
