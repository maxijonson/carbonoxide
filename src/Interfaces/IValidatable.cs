namespace MyCarbonoxidePlugin.Interfaces;

public interface IValidatable
{
    public enum Result
    {
        Valid = 0, // Data is valid
        Repaired = 1, // Data had issues but was salvageable and should be backed up and saved in its repaired state.
        Invalid = 2, // Data is invalid and should be rejected (backed up and replaced with defaults)
    }

    public enum Phase
    {
        Load,
        Hydrate,
    }

    Result Validate(Phase phase);

    public static Result Combine(Result a, Result b)
    {
        return a >= b ? a : b;
    }
}
