global using static Core.Services.GlobalService;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services;

public static class GlobalService
{
    // Constants //
    public const string baseURL = "https://www.canterlotcomics.com"; // Canterlot Comics Website //
    public const string ProgramName = "CanterlotDownloader";
    public static readonly string ProgramFiles = @$"{Environment.ExpandEnvironmentVariables("%ProgramW6432%")}\{ProgramName}";
    public static readonly string LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public static readonly string AppData = $@"{LocalAppData}\{ProgramName}";

    public static readonly string PanelsTemp = $@"{AppData}\PanelsTemp";
    public static readonly string UpscaledTemp = $@"{AppData}\UpscaledTemp";
    public static readonly string UpscalerDirectory = $@"{ProgramFiles}\Upscaler";
    public static readonly string UpscalerEXE = $@"{UpscalerDirectory}\Upscaler.exe";
}