using System;

namespace Global;

public static class Universal
{
    public const string ProgramName = "CanterlotDownloader";
    public static readonly string ProgramFiles = @$"{Environment.ExpandEnvironmentVariables("%ProgramW6432%")}\{ProgramName}";

}