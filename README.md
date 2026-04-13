# Carbonoxide

A template for building Rust game plugins for both Oxide and Carbon frameworks.

## Features

- **📂 Multi-file structure:** Thanks to [MJSU's Plugin.Merge](https://github.com/dassjosh/Plugin.Merge) tool, you can write your code in multiple files and have them automatically merged into a single plugin file.
- **🔄 Dual framework support:** Easily switch between Oxide and Carbon without modifying your project configuration.
- **🚀 Production and staging support:** Build for production and staging at the same time with local test servers
- **📡 Game servers included:** One-click scripts to update and run game servers for all frameworks and branches (production and staging included). They are setup so that they can run all at the same time!
- **⚡ Ready for Dev:** Get straight to coding without worrying about things like assembly references and local servers. Just run a few one-click scripts and you're good to go!

## Prerequisites

- [dotnet](https://dotnet.microsoft.com/en-us/download/dotnet) SDK - for building the plugin and running the build scripts
- Windows - for running the Rust server and scripts
- NodeJS - (Optional) for merging partial classes outputed by Plugin.Merge (disabled by default, see `Directory.Build.targets` to enable)

## Getting Started

1. Clone the repository and navigate to the project directory. You can also use Github's template feature to create your own repository based on this template.
2. Find and Replace the following in the codebase:
   1. `MyCarbonoxide` (CASE SENSITIVE!) - Replace with the the name of your plugin, no spaces (e.g., `GatherManager`). Check for files that have `MyCarbonoxide` in their name as well!
   2. `mycarbonoxide` (CASE SENSITIVE!) - Replace with the lowercase name of your plugin, no spaces (e.g., `gathermanager`). This is used for things like config file names and permission strings.
3. Run all the `update_*.bat` scripts (not `_update*.bat` files!) to create/update the local game servers. They will be created in the `servers` folder.
4. In Carbon servers `config.json` (`servers/carbon-*/carbon/config.json`), set `DeveloperMode` to `true` so that developer assemblies will be generated on the first run.
5. Run all the `run_*.bat` scripts at least once to generate the framework folders and initialize their worlds. (do this every update for Carbon servers, so you get the latest developer assemblies)
6. If you want to start fresh without the opinionated structure I've included, you can delete all the included files in the `src` folder except for `MyCarbonoxide.cs`. The included files are just a suggestion to demonstrate the multi-file structure and how to use partial classes.

## Building the Plugin

> Before building, make sure to run the update and run scripts at least once to intialize the servers and the framework assemblies. For Carbon, make sure to edit the `config.json` files to set `DeveloperMode` to `true` so that the developer assemblies will be generated.

Even though the final plugin file is a single file that should run on all environments, you can still build for each environment separately.

1. (first time only) `dotnet tool restore` — Install the required .NET tools (Plugin.Merge and CSharpier).
2. Build:

```bash
# Build against oxide-production (default), merge, format, and copy to all servers
# Ideal for quick iteration during development, but keep in mind that this is based on oxide-production only
dotnet build plugin.csproj

# Watch mode (same caveats as above, but rebuilds on file changes)
dotnet watch build --project plugin.csproj

# Build for a specific environment (merge, format, and copy to that server only)
dotnet build oxide-production.csproj
dotnet build oxide-staging.csproj
dotnet build carbon-production.csproj
dotnet build carbon-staging.csproj
```

## Validating All Environments

To verify the plugin compiles against all 4 environments at once (compile-only, no merge/format/copy):

```bash
dotnet msbuild plugin.csproj -t:ValidateAll
```

This runs the normal build first, then compiles against each environment's assemblies.

## VS Code Setup

- Use `Tasks: Run Build Task` command to view all the build tasks available, instead of typing out the commands listed above.
- To switch between Oxide and Carbon project contexts in VS Code (e.g., to see Carbon-specific conditional compilation symbols), use the `CSharp: Change the active document's project context` command while inside a `.cs` file and select the desired environment project.

## (Optional) post-merge script

Plugin.Merge merges all partial classes into one file, but, at the time of writing, it just appends all partial classes one after another, as they are discovered. This leaves your final plugin with partial classes sprinkled around your final plugin file, which can look a bit messy (even if you don't work on the final file directly, curators still need to read through it!).

I created a [`post-merge.ts`](scripts/post-merge.ts) script to manually merge the partial classes into a single class declaration. It is written in TypeScript, because I'm much more experienced with it than C# and it was originally written for one of my paid plugins (Contracts), which has 10k+ lines of code and 50+ partial classes declarations.

By default, this script is not used, since I'm not assuming you have NodeJS installed. If you do, you can:

1. Install the dependencies by running `npm install` in the project directory.
2. Enable the post-merge script in `Directory.Build.targets` by uncommenting the relevant lines.
3. Run the build scripts as usual, and the final plugin file will have all partial classes merged into a single class declaration, making it cleaner and easier to read for curators and other developers.

## Plugin Dependencies

If you have other plugins that you want your local servers to use as dependencies, you can add them to the `dependencies` folder and run the `copy_dependencies.bat` script to copy them to the local servers' plugin folders. This is a convenient script you can use instead of manually copying the plugin files to each server.

## Included Codebase

The code included in `src` is a massively opinionated structure that I personally work with. You'll most likely end up trashing the entire codebase to fit your own style. I included it to give you an idea of how you can structure your plugin's files and because I use this template for my own plugins.

If you're interested in keeping some parts of it, here's a brief overview of notable included features

### Migration System

Allows you to modify a JObject before it is deserialized into config/data classes. This is useful for things like renaming fields, changing data structures, and other breaking changes. There's an example migration included that demonstrates how to use the system to rename a field in the config. (`Migration_1_0_0.cs`)

### Validation & Hydration

Data entities that implement the `IValidatable` will have their `Validate` method called at load time and after hydration (if they implement `IHydratable`). Validation can be used to also repair corrupted data gracefully instead of outright rejecting the entire file.

Data entities that implement `IHydratable` will have their `Hydrate` method called after deserialization, allowing you to perform any additional initialization or transformation of the data.

### Converters

#### KeyedDictionaryConverter

The template comes with a `IKeyed` interface that can be used to define entities that will be keyed in a dictionary inside data classes. Instead of having to repeat the dictionary key in the entity itself (risking typos and inconsistencies), `IKeyed` entities will have the dictionary key assigned to their `Key` property automatically set when used with the `KeyedDictionaryConverter`.

**BEFORE**

```cs
[JsonObject(MemberSerialization.OptIn)]
public class MyData
{
   [JsonProperty(PropertyName = "entities")]
   public Dictionary<string, MyEntity> Entities { get; set; } = new();
}

[JsonObject(MemberSerialization.OptIn)]
public class MyEntity
{
   [JsonProperty(PropertyName = "id")]
   public string Id { get; set; }

   [JsonProperty(PropertyName = "name")]
   public string Name { get; set; }
}
```

```json
{
  "entities": {
    "entity1": {
      "id": "entity1",
      "name": "Entity 1"
    },
    "entity2": {
      "id": "entity2",
      "name": "Entity 2"
    }
  }
}
```

**AFTER**

```cs
[JsonObject(MemberSerialization.OptIn)]
public class MyData
{
   [JsonProperty(PropertyName = "entities")]
   [JsonConverter(typeof(KeyedDictionaryConverter<MyEntity>))]
   public Dictionary<string, MyEntity> Entities { get; set; } = new();
}

[JsonObject(MemberSerialization.OptIn)]
public class MyEntity : IKeyed
{
   string IKeyed.Key
   {
       get => Id;
       set => Id = value;
   }

   public string Id { get; set; }

   [JsonProperty(PropertyName = "name")]
   public string Name { get; set; }
}
```

```json
{
  "entities": {
    "entity1": {
      "name": "Entity 1"
    },
    "entity2": {
      "name": "Entity 2"
    }
  }
}
```
