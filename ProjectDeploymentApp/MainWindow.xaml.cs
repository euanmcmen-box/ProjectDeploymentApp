﻿using System;
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
    public bool PreviewPullRequests { get; set; } = true;

    public bool UseBranchingStrategy { get; set; } = false;

    public List<DeploymentApplication> DeploymentApplications { get; } = new();

    private string githubToken = string.Empty;

    private const string projectDeploymentRootUrl = @"C:/Users/euan.mcmenemin/source/project-deployment-root";

    public MainWindow()
    {
        InitializeComponent();

        ReadFromConfiguration();

        DeploymentApplications.AddRange(new List<DeploymentApplication>()
        {
            new("euanmcmen-box", "HelloPlanet", "HelloPlanet", "develop", "master", ""),
            new("allocine", "Banshee", "gla-Banshee", "dev", "uat", "main"),
            new("allocine", "Boost", "gla-BoostTicketing", "dev", "uat", "master"),
            new("allocine", "Cyclops", "gla-Cyclops", "dev", "uat", "main"),
            new("allocine", "Cypher", "gla-Cypher-API", "dev", "uat", "master"),
            new("allocine", "Iceman", "gla-Iceman-API", "dev", "uat", "master"),
            new("allocine", "Legion", "gla-Legion", "dev", "uat", "master"),
            new("allocine", "Quicksilver", "gla-Quicksilver-API", "dev", "uat", "master")
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

        var selectedDeploymentEnvironmentTarget =
            (DeploymentEnvironmentTarget)CbDeploymentEnvironmentTarget.SelectedIndex;

        var selectedDeploymentApplications = DeploymentApplications
            .Where(x => x.IsSelected)
            .ToList();

        foreach (var deploymentApplication in selectedDeploymentApplications)
        {
            var (outputLog, outputErrorLog) =
                SendPullRequestCommand(selectedDeploymentEnvironmentTarget, deploymentApplication);

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

    private (string, string) SendPullRequestCommand(DeploymentEnvironmentTarget target,
        DeploymentApplication application)
    {
        string sourceBranch, targetBranch, title;

        switch (target)
        {
            case DeploymentEnvironmentTarget.Uat:
            {
                title = "deploy to uat";
                sourceBranch = application.DevBranchName;
                targetBranch = application.UatBranchName;
                break;
            }
            case DeploymentEnvironmentTarget.Live:
            {
                title = "deploy to live";
                sourceBranch = application.UatBranchName;
                targetBranch = application.LiveBranchName;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }

        var commands = new List<string>();

        if (UseBranchingStrategy)
        {
            commands.AddRange(GetBranchingStrategyCommands(application, sourceBranch, targetBranch, title));
        }
        else
        { 
            commands.Add(GetCreatePullRequestCommandText(application, sourceBranch, targetBranch, title));
        }

        var outputLogSb = new StringBuilder();
        var outputErrorLogSb = new StringBuilder();

        foreach (var command in commands)
        {
            var (outputLog, outputErrorLog) = SendCommand(command);
            outputLogSb.AppendLine(outputLog);
            outputErrorLogSb.AppendLine(outputErrorLog);
        }

        return (outputLogSb.ToString(), outputErrorLogSb.ToString());
    }

    private List<string> GetBranchingStrategyCommands(DeploymentApplication application, string sourceBranch,
        string targetBranch, string title)
    {
        var mergeBranch = $"merge-to-{targetBranch}";

        var result = new List<string>()
        {
            $@"cd {projectDeploymentRootUrl}/{application.RepositoryName} && " +
            $"git stash && " +
            $"git checkout {sourceBranch} && " +
            $"git pull && " +
            $"git checkout {targetBranch} && " +
            $"git pull && " +
            $"git checkout -b {mergeBranch} && " +
            $"git merge {sourceBranch} && " +
            $"git push --set-upstream origin {mergeBranch}",
            GetCreatePullRequestCommandText(application, mergeBranch, targetBranch, title)
        };

        return result;
    }

    private string GetCreatePullRequestCommandText(DeploymentApplication application, string sourceBranch,
        string targetBranch, string title)
    {
        var previewTextCommandSuffix = PreviewPullRequests ? "--web" : string.Empty;
        var url = $@"https://github.com/{application.RepositoryRootName}/{application.RepositoryName}";
        return
            $"gh pr create --repo \"{url}\" --head \"{sourceBranch}\" --base \"{targetBranch}\" --title \"{title}\" --body \"{title}\" {previewTextCommandSuffix}";
    }

    private (string, string) SendCommand(string commandText)
    {
        var commandProcess = new Process();
        commandProcess.StartInfo = GetStartInfoForCommand(commandText);
        commandProcess.Start();
        commandProcess.WaitForExit();

        var output = commandProcess.StandardOutput.ReadToEnd();

        var errorOutput = commandProcess.StandardError.ReadToEnd();

        return (output, errorOutput);
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