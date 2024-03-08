namespace ProjectDeploymentApp;

public static class GitHubCommands
{
    public static string GetCloneRepositoryInstruction(DeploymentApplication application)
    {
        return $@"cd {DirectoryConstants.GetDeploymentDirectoryPath()} && git clone https://github.com/{application.RepositoryRootName}/{application.RepositoryName}";
    }

    public static string GetRefreshBranchesInstruction(DeploymentApplication application, string sourceBranch,
        string targetBranch)
    {
        return
            $@"cd {DirectoryConstants.GetDeploymentDirectoryPath()}/{application.RepositoryName} && git checkout {sourceBranch} && git pull && git checkout {targetBranch} && git pull";
    }

    public static string GetDeleteMergeBranchIfExistsInstruction(DeploymentApplication application,
        string sourceBranch, string mergeBranch)
    {
        return
            $@"cd {DirectoryConstants.GetDeploymentDirectoryPath()}/{application.RepositoryName} && git checkout {mergeBranch} && git checkout {sourceBranch} && git branch -d {mergeBranch}";
    }

    public static string GetCreateMergeBranchInstruction(DeploymentApplication application, string sourceBranch,
        string mergeBranch)
    {
        return
            $@"cd {DirectoryConstants.GetDeploymentDirectoryPath()}/{application.RepositoryName} && git checkout {sourceBranch} && git checkout -b {mergeBranch} && git merge {sourceBranch} && git push --set-upstream origin {mergeBranch}";
    }

    public static string GetCreatePullRequestCommandText(DeploymentApplication application, string sourceBranch,
        string targetBranch, string title)
    {
        var url = $@"https://github.com/{application.RepositoryRootName}/{application.RepositoryName}";
        return $"gh pr create --repo \"{url}\" --head \"{sourceBranch}\" --base \"{targetBranch}\" --title \"{title}\" --body \"{title}\" --web";
    }
}