using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace ProjectDeploymentApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string GitRootUrl = "https://github.com/allocine/"; //"https://github.com/euanmcmen-box/"; 
    
    public List<DeploymentApplication> DeploymentApplications { get; }

    public MainWindow()
    {
        InitializeComponent();

        DeploymentApplications = new List<DeploymentApplication>()
        {
            //new("Hello Planet", "HelloPlanet", "feature/use-venus", "master", "master", true),
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

    private void BtnCreatePullRequests_OnClick(object sender, RoutedEventArgs e)
    {
        var deploymentLogSb = new StringBuilder();
        var selectedDeploymentEnvironmentTarget = (DeploymentEnvironmentTarget)CbDeploymentEnvironmentTarget.SelectedIndex;

        foreach (var deploymentApplication in DeploymentApplications)
        {
            if (!deploymentApplication.IsSelected) continue;

            var url = SendPullRequestCommand(selectedDeploymentEnvironmentTarget,
                deploymentApplication);

            deploymentLogSb.AppendLine($"{deploymentApplication.Name} - {url}");
        }

        TbLog.Text = deploymentLogSb.ToString();
    }

    private static string SendPullRequestCommand(DeploymentEnvironmentTarget target, DeploymentApplication application)
    {
        var commandText = target switch
        {
            DeploymentEnvironmentTarget.Uat =>
                $"gh pr create --repo \"{GitRootUrl + application.RepositoryName}\" --head \"{application.DevBranchName}\" --base \"{application.UatBranchName}\" --title \"deploy to uat\" --body \"deploy to uat\"",
            DeploymentEnvironmentTarget.Live =>
                $"gh pr create --repo \"{GitRootUrl + application.RepositoryName}\" --head \"{application.UatBranchName}\" --base \"{application.LiveBranchName}\" --title \"deploy to live\" --body \"deploy to live\" --web",
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(target))
        };

        var process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/C {commandText}";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        process.WaitForExit();
        return process.StandardOutput.ReadToEnd();
    }
}