namespace PDACore;

public static class GitHubCommands
{
    public static string GetCloneRepositoryInstruction(string deploymentApplicationsRootPath, DeploymentApplication application)
    {
        return $@"cd {deploymentApplicationsRootPath} && git clone https://github.com/{application.RepositoryRootName}/{application.RepositoryName}";
    }

    public static string GetRefreshBranchesInstruction(DeploymentContext deploymentContext, DeploymentApplication application)
    {
        var (sourceBranch, targetBranch, _) = GetBranchesForApplication(deploymentContext, application);
        return
            $@"cd {deploymentContext.DeploymentApplicationsRoot}/{application.RepositoryName} && git checkout {sourceBranch} && git pull && git checkout {targetBranch} && git pull";
    }

    public static string GetDeleteMergeBranchIfExistsInstruction(DeploymentContext deploymentContext, DeploymentApplication application)
    {
        var (sourceBranch, targetBranch, _) = GetBranchesForApplication(deploymentContext, application);
        var mergeBranch = GetMergeBranch(targetBranch);
        return
            $@"cd {deploymentContext.DeploymentApplicationsRoot}/{application.RepositoryName} && git checkout {mergeBranch} && git checkout {sourceBranch} && git push origin --delete {mergeBranch} && git branch -d {mergeBranch}";
    }

    public static string GetCreateMergeBranchInstruction(DeploymentContext deploymentContext, DeploymentApplication application)
    {
        var (sourceBranch, targetBranch, _) = GetBranchesForApplication(deploymentContext, application);
        var mergeBranch = GetMergeBranch(targetBranch);
        return
            $@"cd {deploymentContext.DeploymentApplicationsRoot}/{application.RepositoryName} && git checkout {sourceBranch} && git checkout -b {mergeBranch} && git merge {sourceBranch} && git push --set-upstream origin {mergeBranch}";
    }

    public static string GetCreatePullRequestCommandText(DeploymentContext deploymentContext, DeploymentApplication application)
    {
        var (_, targetBranch, title) = GetBranchesForApplication(deploymentContext, application);
        var mergeBranch = GetMergeBranch(targetBranch);
        var url = $@"https://github.com/{application.RepositoryRootName}/{application.RepositoryName}";
        return $"gh pr create --repo \"{url}\" --head \"{mergeBranch}\" --base \"{targetBranch}\" --title \"{title}\" --body \"{title}\" --web";
    }

    private static string GetMergeBranch(string targetBranch) => $"merge-to-{targetBranch}";

    private static (string sourceBranch, string targetBranch, string prTitle) GetBranchesForApplication(DeploymentContext context, DeploymentApplication deploymentApplication)
    {
        return context.DeploymentTarget switch
        {
            DeploymentEnvironmentTarget.Uat => (deploymentApplication.DevBranchName, deploymentApplication.UatBranchName, "deploy to uat"),
            DeploymentEnvironmentTarget.Live => (deploymentApplication.UatBranchName, deploymentApplication.LiveBranchName,"deploy to live"),
            _ => throw new ArgumentOutOfRangeException(nameof(context.DeploymentTarget), context.DeploymentTarget, null)
        };
    } 

}