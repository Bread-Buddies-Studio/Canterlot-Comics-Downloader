global using static Global.Universal;

namespace Global;

public static class Universal
{
    // Constants //
    public const string baseURL = "https://www.canterlotcomics.com"; // Canterlot Comics Website //
    public const string ProgramName = "CanterlotDownloader";
    public static readonly string ProgramFiles = @$"{Environment.ExpandEnvironmentVariables("%ProgramW6432%")}\{ProgramName}";

    public static readonly string PanelsTemp = $@"{ProgramFiles}\PanelsTemp";
    public static readonly string UpscaledTemp = $@"{ProgramFiles}\UpscaledTemp";
    public static readonly string UpscalerDirectory = $@"{ProgramFiles}\Upscaler";
    public static readonly string UpscalerEXE = $@"{UpscalerDirectory}\Upscaler.exe";
}