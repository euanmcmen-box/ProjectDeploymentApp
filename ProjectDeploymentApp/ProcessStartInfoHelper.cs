using System.Diagnostics;

namespace ProjectDeploymentApp;

public static class ProcessStartInfoHelper
{
    public static ProcessStartInfo GetForegroundCommandProcessStartInfo(string githubToken, string commandText)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {commandText}",
            WindowStyle = ProcessWindowStyle.Normal,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
            UseShellExecute = false,
        };

        processStartInfo.EnvironmentVariables.Add("GH_TOKEN", githubToken);

        return processStartInfo;
    }

    public static ProcessStartInfo GetBackgroundCommandProcessStartInfo(string githubToken, string commandText)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/C {commandText}",
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        processStartInfo.EnvironmentVariables.Add("GH_TOKEN", githubToken);

        return processStartInfo;
    }
}