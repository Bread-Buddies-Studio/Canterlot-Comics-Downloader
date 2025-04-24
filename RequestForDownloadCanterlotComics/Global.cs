global using static Global.Universal;

namespace Global;

public static class Universal
{
    // Constants //
    public const string baseURL = "https://www.canterlotcomics.com"; // Canterlot Comics Website //
    public const string ProgramName = "CanterlotDownloader";
    public static readonly string ProgramFiles = @$"{Environment.ExpandEnvironmentVariables("%ProgramW6432%")}\{ProgramName}";

}