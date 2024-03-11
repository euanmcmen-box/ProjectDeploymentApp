namespace PDACore;

public class DirectoryConstants
{
    private const string DeploymentProjectsDirectoryName = "DeploymentProjects";

    public static string GetDeploymentDirectoryPath() => $"{Environment.CurrentDirectory}/{DeploymentProjectsDirectoryName}/";
}