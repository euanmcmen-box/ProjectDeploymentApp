using PDACore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

// ReSharper disable InconsistentNaming

namespace ProjectDeploymentApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public List<DeploymentApplication> DeploymentApplications { get; } = new();

    public bool DirectoryStateValid { get; set; }

    public StringBuilder AppLog { get; }

    public StringBuilder ProcessLog { get; }

    private string githubToken = string.Empty;

    private readonly string deploymentProjectsRootPath;

    public MainWindow()
    {
        InitializeComponent();

        deploymentProjectsRootPath = $"{Environment.CurrentDirectory}/apps/";

        CbDeploymentEnvironmentTarget.ItemsSource = Enum.GetValues<DeploymentEnvironmentTarget>();
        CbDeploymentEnvironmentTarget.SelectedIndex = 0;

        ProcessLog = new StringBuilder();
        AppLog = new StringBuilder();

        ReadDeploymentApplications();

        ReadGithubTokenFromConfiguration();

        CheckRepositories();

        ConfigureButtons();
    }

    private void ReadDeploymentApplications()
    {
        var applications = JsonSerializer.Deserialize<List<DeploymentApplication>>( File.ReadAllText("Applications.json"));

        if (applications == null)
        {
            LblApplicationStatus.Content = "ERROR";
        }
        else
        {
            DeploymentApplications.AddRange(applications);
            LblApplicationStatus.Content = "OK";
        }
    }

    private void ReadGithubTokenFromConfiguration()
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

        LblGithubTokenStatus.Content = "ERROR";

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

    private void CheckRepositories()
    {
        DirectoryStateValid = true;

        foreach (var deploymentApplication in DeploymentApplications)
        {
            if (!Directory.Exists($"{deploymentProjectsRootPath}/{deploymentApplication.RepositoryName}"))
            {
                WriteToApplicationLog(deploymentApplication, "Repository missing.  Run repository initialization.");
                DirectoryStateValid = false;
                continue;
            }

            WriteToApplicationLog(deploymentApplication, "OK");
        }

        LblRepositoryStatus.Content = DirectoryStateValid ? "OK" : "ERROR";
    }

    private void ConfigureButtons()
    {
        BtnInitialiseRepos.IsEnabled = !string.IsNullOrEmpty(githubToken) && !DirectoryStateValid;
        BtnCreatePullRequests.IsEnabled = !string.IsNullOrEmpty(githubToken) && DirectoryStateValid;
        BtnCleanupBranches.IsEnabled = !string.IsNullOrEmpty(githubToken) && DirectoryStateValid;
    }

    private async void BtnCreatePullRequests_OnClick(object sender, RoutedEventArgs e)
    {
        var (deploymentContext, selectedDeploymentApplications) = GetDeploymentParameters();

        foreach (var deploymentApplication in selectedDeploymentApplications)
        {
            WriteToApplicationLog(deploymentApplication, "Starting...");

            var commandsList = new List<string>
            {
                GitHubCommands.GetRefreshBranchesInstruction(deploymentContext, deploymentApplication),
                GitHubCommands.GetDeleteMergeBranchIfExistsInstruction(deploymentContext, deploymentApplication),
                GitHubCommands.GetCreateMergeBranchInstruction(deploymentContext, deploymentApplication),
                GitHubCommands.GetCreatePullRequestCommandText(deploymentContext, deploymentApplication)
            };

            await RunCommandsAsync(deploymentApplication, commandsList);
        }
    }

    private async void  BtnCleanupBranches_OnClick(object sender, RoutedEventArgs e)
    {
        var (deploymentContext, selectedDeploymentApplications) = GetDeploymentParameters();

        foreach (var deploymentApplication in selectedDeploymentApplications)
        {
            WriteToApplicationLog(deploymentApplication, "Cleaning up...");

            var commandsList = new List<string>
            {
                GitHubCommands.GetRefreshBranchesInstruction(deploymentContext, deploymentApplication),
                GitHubCommands.GetDeleteMergeBranchIfExistsInstruction(deploymentContext, deploymentApplication),
            };    

            await RunCommandsAsync(deploymentApplication, commandsList);
        }
    }

    private async void BtnInitialiseRepos_OnClick(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists($"{deploymentProjectsRootPath}"))
        {
            Directory.CreateDirectory(deploymentProjectsRootPath);
        }

        foreach (var deploymentApplication in DeploymentApplications)
        {
            WriteToApplicationLog(deploymentApplication, "Cloning...");

            if (Directory.Exists($"{deploymentProjectsRootPath}/{deploymentApplication.RepositoryName}"))
            {
                WriteToApplicationLog(deploymentApplication, "Directory exists.  Skipping repo initialization.");
                continue;
            }

            await SendForegroundCommandAsync(GitHubCommands.GetCloneRepositoryInstruction(deploymentProjectsRootPath, deploymentApplication));

            WriteToApplicationLog(deploymentApplication, "Complete");
        }

        CheckRepositories();

        ConfigureButtons();
    }

    private (DeploymentContext, List<DeploymentApplication>) GetDeploymentParameters()
    {
        var deploymentContext = new DeploymentContext()
        {
            DeploymentApplicationsRoot = deploymentProjectsRootPath,
            DeploymentTarget = (DeploymentEnvironmentTarget)CbDeploymentEnvironmentTarget.SelectedIndex
        };

        var deploymentApplications = DeploymentApplications.Where(x => x.IsSelected).ToList();

        return (deploymentContext, deploymentApplications);
    }

    private void WriteToApplicationLog(DeploymentApplication application, string message)
    {
        AppLog.AppendLine($"{application.Name} - {message}");
        TbApplicationLog.Text = AppLog.ToString();
    }

    private async Task RunCommandsAsync(DeploymentApplication deploymentApplication, List<string> commandsList)
    {
        WriteToApplicationLog(deploymentApplication, "Commands created.  Executing...");

        foreach (var command in commandsList)
        {
            WriteToApplicationLog(deploymentApplication, $"Executing command: <<< {command} >>>");
            await SendForegroundCommandAsync(command);
        }

        WriteToApplicationLog(deploymentApplication, "Complete");
    }

    //private async Task SendBackgroundCommandAsync(string commandText)
    //{
    //    var commandProcess = new Process();
    //    commandProcess.StartInfo = ProcessStartInfoHelper.GetBackgroundCommandProcessStartInfo(githubToken, commandText);
    //    commandProcess.Start();

    //    await commandProcess.WaitForExitAsync();

    //    var output = await commandProcess.StandardOutput.ReadToEndAsync();
    //    var errorOutput = await commandProcess.StandardError.ReadToEndAsync();

    //    ProcessLog.AppendLine(output);
    //    ProcessLog.AppendLine(errorOutput);
    //    TbProcessOutput.Text = ProcessLog.ToString();
    //}

    private async Task SendForegroundCommandAsync(string commandText)
    {
        var commandProcess = new Process();
        commandProcess.StartInfo = ProcessStartInfoHelper.GetForegroundCommandProcessStartInfo(githubToken, commandText);;
        commandProcess.Start();
        await commandProcess.WaitForExitAsync();
    }
}