using System.Collections.Generic;
using MyCarbonoxidePlugin.Lang;

namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    public string m(string key, string? userId, params object[] args)
    {
        // Puts($"m({key}, {userId}{(args.Length > 0 ? ", " : "")}{string.Join(", ", args)}) ({args.Length} args)");
        try
        {
            return string.Format(lang.GetMessage(key, this, userId), args);
        }
        catch
        {
            try
            {
                return lang.GetMessage(key, this, null);
            }
            catch
            {
                return key;
            }
        }
    }

    private new void LoadDefaultMessages()
    {
        lang.RegisterMessages(
            new Dictionary<string, string>
            {
                // Commands
                [LangKeys.Commands.Unauthorized] = "You do not have permission to use this command.",
            },
            this,
            "en"
        );
    }
}
