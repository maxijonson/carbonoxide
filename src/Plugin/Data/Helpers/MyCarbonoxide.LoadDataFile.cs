using System;
using MyCarbonoxidePlugin.Interfaces;
using MyCarbonoxidePlugin.Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core.Configuration;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    [Flags]
    private enum DataActions
    {
        None = 0,
        Save = 1 << 0,
        Backup = 1 << 1,
    }

    public void LoadDataFile<TData>(
        ref TData data,
        DynamicConfigFile dataFile,
        Func<TData> getDefaultData,
        Action saveData,
        MigrationFileType? fileType
    )
        where TData : new()
    {
        try
        {
            var filename = GetFilename(dataFile);

            var actions = DataActions.None;
            data = ReadOrDefault(dataFile, fileType, getDefaultData, ref actions);

            actions |= ValidateLoadPhaseOrReplaceWithDefault(ref data, getDefaultData, filename);
            actions |= HydrateAndPostValidate(ref data, getDefaultData, filename);

            ApplyActions(actions, dataFile, saveData);
            if (dataFile != Config)
            {
                dataFile.Clear();
            }
        }
        catch
        {
            if (dataFile != Config)
            {
                dataFile.Clear();
            }
            throw;
        }
    }

    private TData ReadOrDefault<TData>(
        DynamicConfigFile dataFile,
        MigrationFileType? fileType,
        Func<TData> getDefaultData,
        ref DataActions actions
    )
        where TData : new()
    {
        try
        {
            if (!dataFile.Exists())
            {
                actions |= DataActions.Save;
                return getDefaultData();
            }

            var obj = dataFile.ReadObject<JObject>();
            if (obj is null)
            {
                PrintError(
                    $"Data file '{GetFilename(dataFile)}' is not valid JSON. Creating backup and replacing with default data."
                );
                actions |= DataActions.Backup | DataActions.Save;
                return getDefaultData();
            }

            if (fileType is not null && typeof(IVersionable).IsAssignableFrom(typeof(TData)))
            {
                var currentVersion = Migration.GetVersion(obj);
                var targetVersion = Version;
                if (currentVersion > targetVersion)
                {
                    throw new Exception(
                        $"{fileType} Data file version {currentVersion} is from a newer version of the plugin and cannot be loaded. Please update the plugin to load this data file."
                    );
                }

                if (currentVersion < targetVersion)
                {
                    obj = Migration.RunMigrations(obj, fileType.Value, Version);
                    actions |= DataActions.Save;
                }
            }

            return Deserialize<TData>(obj, dataFile);
        }
        catch (Exception ex)
        {
            PrintError(
                $"Failed to read data file '{GetFilename(dataFile)}'. Creating backup and replacing with default data.\n{ex}"
            );
            actions |= DataActions.Backup | DataActions.Save;
            return getDefaultData();
        }
    }

    private TData Deserialize<TData>(JObject obj, DynamicConfigFile dataFile)
        where TData : new()
    {
        var data =
            obj.ToObject<TData>(JsonSerializer.Create(dataFile.Settings))
            ?? throw new Exception($"Failed to deserialize {typeof(TData).Name} from JSON.");

        return data;
    }

    private DataActions ValidateLoadPhaseOrReplaceWithDefault<TData>(
        ref TData data,
        Func<TData> getDefaultData,
        string filename
    )
        where TData : new()
    {
        if (data is not IValidatable validatable)
        {
            return DataActions.None;
        }

        var result = validatable.Validate(IValidatable.Phase.Load);
        switch (result)
        {
            case IValidatable.Result.Invalid:
                // Loading default data or empty data is more likely to cause cascading issues for other files that may reference this data. It's safer to just crash the plugin at this point.
                throw new Exception(
                    $"Data in '{filename}' is invalid. This likely indicates a bug or is expected if the file was manually edited. Creating backup and replacing with default data."
                );
            case IValidatable.Result.Repaired:
                PrintWarning(
                    $"Data in '{filename}' had issues but was repaired. Creating backup and saving repaired data."
                );
                return DataActions.Backup | DataActions.Save;

            default:
                return DataActions.None;
        }
    }

    private DataActions HydrateAndPostValidate<TData>(ref TData data, Func<TData> getDefaultData, string filename)
        where TData : new()
    {
        var actions = DataActions.None;

        if (data is not IHydratable hydratable)
        {
            return actions;
        }

        hydratable.Hydrate();

        if (data is not IValidatable validatable)
        {
            return actions;
        }

        switch (validatable.Validate(IValidatable.Phase.Hydrate))
        {
            case IValidatable.Result.Invalid:
                // Loading default data or empty data is more likely to cause cascading issues for other files that may reference this data. It's safer to just crash the plugin at this point.
                throw new Exception(
                    $"Hydrated data in '{filename}' is invalid. This likely indicates a bug or is expected if the data was already invalid during load."
                );
            case IValidatable.Result.Repaired:
                PrintWarning(
                    $"Hydrated data in '{filename}' had issues but was repaired. Creating backup and saving repaired data."
                );
                actions |= DataActions.Backup | DataActions.Save;
                break;
        }

        return actions;
    }

    private void ApplyActions(DataActions actions, DynamicConfigFile dataFile, Action saveData)
    {
        if ((actions & DataActions.Backup) != 0)
        {
            CreateBackup(dataFile);
        }

        if ((actions & DataActions.Save) != 0)
        {
            saveData();
        }
    }
}
