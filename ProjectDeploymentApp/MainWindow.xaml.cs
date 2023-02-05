using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
// ReSharper disable InconsistentNaming

namespace ProjectDeploymentApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string GitRootUrl = "https://github.com/euanmcmen-box/"; // "https://github.com/allocine/";
    
    public List<DeploymentApplication> DeploymentApplications { get; }

    private bool previewPullRequests;

    private string githubToken = default!;

    public MainWindow()
    {
        InitializeComponent();

        ReadFromConfiguration();

        DeploymentApplications = new List<DeploymentApplication>()
        {
            new("Hello Planet", "HelloPlanet", "feature/use-venus", "master", "master", true),
            new("Banshee", "gla-Banshee", "dev", "uat", "main"),
            new("Boost", "gla-BoostTicketing", "dev", "uat", "master", false),
            new("Cyclops", "gla-Cyclops", string.Empty, "uat", "main"),
            new("Cypher", "gla-Cypher-API", "dev", "uat", "master", false),
            new("Iceman", "gla-Iceman", "dev", "uat", "master"),
            new("Quicksilver", "gla-Quicksilver-API", "dev", "uat", "master")
        };

        CbDeploymentEnvironmentTarget.ItemsSource = Enum.GetValues<DeploymentEnvironmentTarget>();
        CbDeploymentEnvironmentTarget.SelectedIndex = 0;

        LbApplications.ItemsSource = DeploymentApplications;
    }

    private void ReadFromConfiguration()
    {
        githubToken = ConfigurationManager.AppSettings["GITHUB_SSH_KEY"] ??
                      throw new InvalidOperationException("GITHUB_SSH_KEY is not set in the application config.");

        previewPullRequests = bool.TryParse(ConfigurationManager.AppSettings["PREVIEW_PULL_REQUESTS"], out var settingValue) && settingValue;
    }

    private void BtnCreatePullRequests_OnClick(object sender, RoutedEventArgs e)
    {
        var deploymentLogSb = new StringBuilder();
        var selectedDeploymentEnvironmentTarget = (DeploymentEnvironmentTarget)CbDeploymentEnvironmentTarget.SelectedIndex;

        var selectedDeploymentApplications = DeploymentApplications
            .Where(x => x.IsSelected)
            .ToList();

        foreach (var deploymentApplication in selectedDeploymentApplications)
        {
            var (outputLog, outputErrorLog) = SendPullRequestCommand(selectedDeploymentEnvironmentTarget, deploymentApplication);

            deploymentLogSb.AppendLine(deploymentApplication.Name);
            deploymentLogSb.AppendLine($"PR URL? - {outputLog}");
            deploymentLogSb.AppendLine($"Errors? - {outputErrorLog}");
        }

        TbLog.Text = deploymentLogSb.ToString();
    }

    private (string, string) SendPullRequestCommand(DeploymentEnvironmentTarget target, DeploymentApplication application)
    {
        var previewTextCommandSuffix = previewPullRequests ? "--web" : string.Empty;

        var commandText = target switch
        {
            DeploymentEnvironmentTarget.Uat =>
                $"gh pr create --repo \"{GitRootUrl + application.RepositoryName}\" --head \"{application.DevBranchName}\" --base \"{application.UatBranchName}\" --title \"deploy to uat\" --body \"deploy to uat\" {previewTextCommandSuffix}",
            DeploymentEnvironmentTarget.Live =>
                $"gh pr create --repo \"{GitRootUrl + application.RepositoryName}\" --head \"{application.UatBranchName}\" --base \"{application.LiveBranchName}\" --title \"deploy to live\" --body \"deploy to live\" {previewTextCommandSuffix}",
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(target))
        };

        var process = new Process();
        process.StartInfo = GetStartInfoForCommand(commandText);
        process.Start();
        process.WaitForExit();

        var outputLog = process.StandardOutput.ReadToEnd();
        var outputErrorLog = process.StandardError.ReadToEnd();

        return (outputLog, outputErrorLog);
    }

    private ProcessStartInfo GetStartInfoForCommand(string commandText)
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

        if (!EnvironmentVariablesContainsGithubAuthToken(processStartInfo))
        {
            processStartInfo.EnvironmentVariables.Add("GH_TOKEN", githubToken);
        }

        return processStartInfo;

        static bool EnvironmentVariablesContainsGithubAuthToken(ProcessStartInfo startInfo) =>
            startInfo.EnvironmentVariables.ContainsKey("GH_TOKEN") ||
            startInfo.EnvironmentVariables.ContainsKey("GITHUB_TOKEN");
    }
}