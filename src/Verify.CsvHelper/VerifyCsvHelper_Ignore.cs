namespace VerifyTests;

public static partial class VerifyCsvHelper
{
    public static void IgnoreCsvColumns(this VerifySettings settings, params string[] columns) =>
        settings.Context["VerifyCsvHelperIgnoreColumns"] = columns;

    public static SettingsTask IgnoreCsvColumns(this SettingsTask settings, params string[] columns)
    {
        settings.CurrentSettings.IgnoreCsvColumns(columns);
        return settings;
    }

    static string[] GetIgnoreCsvColumns(this IReadOnlyDictionary<string, object> settings)
    {
        if (settings.TryGetValue("VerifyCsvHelperIgnoreColumns", out var value))
        {
            return (string[]) value;
        }

        return [];
    }
}