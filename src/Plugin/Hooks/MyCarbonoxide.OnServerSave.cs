namespace MyCarbonoxidePlugin.Plugin;

public partial class MyCarbonoxide
{
    private void OnServerSave()
    {
        if (IsDataLoaded)
        {
            // Don't save if data isn't loaded to avoid saving when data is in an inconsistent state due to a loading error
            // SaveData();
        }
    }
}
