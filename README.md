# Carbonoxide

A template for building Rust game plugins for both Oxide and Carbon frameworks with multi-file structure support so you don't have to write it all in one large file. 

Carbonoxide was used to build [Contracts](https://www.rustcontracts.com/): a time-rotating quest system.

## Features

- **📂 Multi-file structure:** Thanks to [MJSU's Plugin.Merge](https://github.com/dassjosh/Plugin.Merge) tool, you can write your code in multiple files and have them automatically merged into a single plugin file.
- **💰 Lite edition (optional):** Wrap paid-only code in `#if PREMIUM` and every build also gives you a free Lite version of your plugin from the same code.
- **🔄 Dual framework support:** Easily switch between Oxide and Carbon without modifying your project configuration.
- **🚀 Production and staging support:** Build for production and staging at the same time with local test servers
- **📡 Game servers included:** One-click scripts to update and run game servers for all frameworks and branches (production and staging included). They are setup so that they can run all at the same time!
- **⚡ Ready for Dev:** Get straight to coding without worrying about things like assembly references and local servers. Just run a few one-click scripts and you're good to go!

## Prerequisites

- [dotnet](https://dotnet.microsoft.com/en-us/download/dotnet) SDK - for building the plugin and running the build scripts
- Windows - for running the Rust server and scripts
- (Optional) NodeJS - for [merging partial classes](#optional-merging-partial-classes) outputed by Plugin.Merge. Required for the [Lite edition](#optional-lite-edition).

## Getting Started

> Note: The following instructions will get you setup for all environments, but you can choose to omit some environments you're not planning to build for (e.g: don't run any of the staging scripts if you don't plan on building for staging). The template is designed to only build for the available environments!

1. Clone the repository and navigate to the project directory. You can also use Github's template feature to create your own repository based on this template.
2. Find and Replace the following in the codebase:
   1. `MyCarbonoxide` (CASE SENSITIVE!) - Replace with the the name of your plugin, no spaces (e.g., `GatherManager`). Check for files that have `MyCarbonoxide` in their name as well!
   2. `mycarbonoxide` (CASE SENSITIVE!) - Replace with the lowercase name of your plugin, no spaces (e.g., `gathermanager`). This is used for things like config file names and permission strings.
3. Run all the `update_*.bat` scripts (not `_update*.bat` files!) to create/update the local game servers. They will be created in the `servers` folder.
4. Run all the `run_*.bat` scripts at least once to generate the framework folders and initialize their worlds. On Oxide servers, this also writes the publicized game assemblies to `servers/oxide-*/publicized`, which the build needs. (do this after every update, so the build gets the latest developer assemblies for Carbon and publicized assemblies for Oxide)
5. In Carbon servers `config.json` (`servers/carbon-*/carbon/config.json`), set `DeveloperMode` to `true` so that developer assemblies will be generated on the first run.
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
dotnet msbuild plugin.csproj -restore -t:ValidateAll
```

This runs the normal build first, then compiles `src` against each environment you have a server for, and compiles the merged `MyCarbonoxide.cs` the same way a server would. With the [Lite edition](#optional-lite-edition) on, it also compiles `src` without `PREMIUM` and compiles `lite/MyCarbonoxide.cs`.

## VS Code Setup

- Use `Tasks: Run Build Task` command to view all the build tasks available, instead of typing out the commands listed above.
- To switch between Oxide and Carbon project contexts in VS Code (e.g., to see Carbon-specific conditional compilation symbols), use the `CSharp: Change the active document's project context` command while inside a `.cs` file and select the desired environment project.

## (Optional) Merging partial classes

Plugin.Merge merges all partial classes into one file.At the time of writing, it just appends all partial classes one after another, as they are discovered. This leaves your final plugin with partial classes sprinkled around your final plugin file, which can look a bit messy (even if you don't work on the final file directly, curators still need to read through it!).

I created a [`post-merge.ts`](scripts/post-merge.ts) script to automatically merge the partial classes into a single class declaration. It is written in TypeScript, because I'm much more experienced with it than C# and it was originally written for one of my paid plugins (Contracts), which has 10k+ lines of code and 50+ partial classes declarations.

By default, the build will skip merging partial classes, but if you want to enable it, just follow these steps:

1. Ensure you have NodeJS installed.
   > I personally use [Volta](https://volta.sh/), because it automatically manages NodeJS versions for all my other TypeScript repos, but you can also install it [globally](https://nodejs.org/) or use something like [nvm](https://github.com/nvm-sh/nvm)
2. Install the dependencies by running `npm install` in the project directory.
3. The build process will automatically start using the `post-merge.ts` script in the build process when it detects NodeJS and the presence of the `node_modules` folder.

## (Optional) Lite edition

> This feature is almost exactly how I ship Contracts' Premium and Lite editions!

If you sell your plugin, you might want to put a free Lite version out there too, with only some of the features. Instead of keeping two copies of your code, you wrap the paid-only parts in `#if PREMIUM` and every build makes both files from the same `src`:

- `MyCarbonoxide.cs`: the full plugin.
- `lite/MyCarbonoxide.cs`: the Lite plugin, with everything inside `#if PREMIUM` removed.

Both files have the same class name, `[Info]`, config and data paths. Server owners can upgrade by swapping the file and their config and data keep working.

To turn it on:

1. Set `LiteEdition` to `true` in `Directory.Build.props`.
2. Set up NodeJS like in [merging partial classes](#optional-merging-partial-classes). Here it's not optional. The server never defines `PREMIUM`, so without the post-merge script your `#if PREMIUM` code would be missing from the full plugin too. The build stops if it can't find NodeJS.
3. Wrap your paid-only code:

```cs
public partial class MyCarbonoxide
{
#if PREMIUM
    private void OpenLeaderboard(BasePlayer player)
    {
        // ...
    }
#endif

    private int GetMaxHomes(BasePlayer player)
    {
#if PREMIUM
        return Settings.MaxHomes;
#else
        return 1;
#endif
    }
}
```

`#if !PREMIUM` works too, for code that should only be in the Lite version.

A few rules:

- Only `#if PREMIUM`, `#if !PREMIUM`, `#else` and `#endif` are supported. No `#elif`, and no combining like `#if PREMIUM && CARBON` (put one `#if` inside the other instead). The build stops with an error if it finds one.
- The `#if` has to be inside a class body. It doesn't work outside of it, because Plugin.Merge resolves `#if` blocks around a whole file, a whole class or `using` lines on its own, before the Lite step runs. `PREMIUM` isn't defined at that point, so that code gets dropped from both files. To make a whole file premium, wrap what's inside the class instead:

```cs

// ❌ BAD

#if PREMIUM
public partial class MyCarbonoxide
{
    // the whole file goes here
}
#endif

// ✅ GOOD

public partial class MyCarbonoxide
{
#if PREMIUM
    // the whole file goes here
#endif
}
```

- Wrapping a method call doesn't wrap the method itself. Lite still compiles, but the method's code ships in the Lite file. Before a release, search `lite/MyCarbonoxide.cs` for your paid features to make sure they're gone.
  - In Contracts, I usually keep the last shipped version of Lite. Before I upload a new Lite version, I compare it with the current Lite build to ensure that no paid features have accidentally been included. (using VS Code's file comparison feature)
- **This is more of a strong recommendation but not mandatory**: Leave full config and data classes in both versions, so a full config still loads on Lite. Only wrap the code that does things/logic: hooks, commands, UI.
  - Here's how I reason about it when developing Contracts: only wrap your "secret sauce", the algorithms and logic by premium directives (anything that makes your paid features come alive). Don't wrap stuff that just "defines" a premium feature: Data structures, config, and other non-logic code should remain outside of `#if PREMIUM` blocks. This makes your code much easier to maintain without accidentally breaking the Lite version. It also makes a much nicer upgrade experience for server owners: all they need to do is swap the plugin file. No new data files or config options suddenly appear.

Each build copies the full plugin to your local servers. Run `copy_lite.bat` to test the Lite one instead (the next build puts the full one back). [`ValidateAll`](#validating-all-environments) also checks that the Lite version compiles.

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
