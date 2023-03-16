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
    private const string GitRootUrl = "https://github.com/allocine/";

    public bool PreviewPullRequests { get; set; } = true;

    public List<DeploymentApplication> DeploymentApplications { get; } = new();


    private string githubToken = string.Empty;

    public MainWindow()
    {
        InitializeComponent();

        ReadFromConfiguration();

        DeploymentApplications.AddRange(new List<DeploymentApplication>()
        {
            new("Banshee", "gla-Banshee", "dev", "uat", "main"),
            new("Boost", "gla-BoostTicketing", "dev", "uat", "master"),
            new("Cyclops", "gla-Cyclops", "dev", "uat", "main"),
            new("Cypher", "gla-Cypher-API", "dev", "uat", "master"),
            new("Iceman", "gla-Iceman-API", "dev", "uat", "master"),
            new("Legion", "gla-AppDotBoost-AzureFunctions", "dev", "uat", "master"),
            new("Quicksilver", "gla-Quicksilver-API", "dev", "uat", "master")
        });

        CbDeploymentEnvironmentTarget.ItemsSource = Enum.GetValues<DeploymentEnvironmentTarget>();
        CbDeploymentEnvironmentTarget.SelectedIndex = 0;
    }

    private void ReadFromConfiguration()
    {
        TrySetFromEnvironmentVariables();

        if (!string.IsNullOrEmpty(githubToken))
        {
            LblGithubTokenStatus.Content = "Set in Environment Variables";
            return;
        }

        TrySetFromConfig();

        if (!string.IsNullOrEmpty(githubToken))
        {
            LblGithubTokenStatus.Content = "Set in App Config";
            return;
        }

        LblGithubTokenStatus.Content = "NOT SET";

        void TrySetFromEnvironmentVariables()
        {
            var ghTokenEnvironmentVariable = Environment.GetEnvironmentVariable("GH_TOKEN");
            var githubTokenEnvironmentVariable = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

            if (!string.IsNullOrEmpty(ghTokenEnvironmentVariable))
                githubToken = ghTokenEnvironmentVariable;

            if (!string.IsNullOrEmpty(githubTokenEnvironmentVariable))
                githubToken = githubTokenEnvironmentVariable;
        }

        void TrySetFromConfig()
        {
            githubToken = ConfigurationManager.AppSettings["GITHUB_TOKEN"] ?? string.Empty;
        }
    }

    private void BtnCreatePullRequests_OnClick(object sender, RoutedEventArgs e)
    {
        var deploymentLogSb = new StringBuilder();
        var deploymentErrorLogSb = new StringBuilder();

        var selectedDeploymentEnvironmentTarget = (DeploymentEnvironmentTarget)CbDeploymentEnvironmentTarget.SelectedIndex;

        var selectedDeploymentApplications = DeploymentApplications
            .Where(x => x.IsSelected)
            .ToList();

        foreach (var deploymentApplication in selectedDeploymentApplications)
        {
            var (outputLog, outputErrorLog) = SendPullRequestCommand(selectedDeploymentEnvironmentTarget, deploymentApplication);

            if (!string.IsNullOrEmpty(outputLog))
            {
                deploymentLogSb.AppendLine(deploymentApplication.Name);
                deploymentLogSb.AppendLine(outputLog);
            }

            if (!string.IsNullOrEmpty(outputErrorLog))
            {
                deploymentErrorLogSb.AppendLine(deploymentApplication.Name);
                deploymentErrorLogSb.AppendLine(outputErrorLog);
            }
        }

        TbLog.Text = deploymentLogSb.ToString();
        TbError.Text = deploymentErrorLogSb.ToString();
    }

    private (string, string) SendPullRequestCommand(DeploymentEnvironmentTarget target, DeploymentApplication application)
    {
        var previewTextCommandSuffix = PreviewPullRequests ? "--web" : string.Empty;

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

        processStartInfo.EnvironmentVariables.Add("GH_TOKEN", githubToken);

        return processStartInfo;
    }
}