namespace ProjectDeploymentApp
{
    public class DeploymentApplication
    {
        public string Name { get; }

        public bool IsSelected { get; set; }

        public string RepositoryName { get; }

        public string DevBranchName { get; }

        public string UatBranchName { get; }

        public string LiveBranchName { get; }

        public DeploymentApplication(string name, string repositoryName, string devBranchName, string uatBranchName, string liveBranchName, bool isSelected = false)
        {
            Name = name;
            RepositoryName = repositoryName;
            DevBranchName = devBranchName;
            UatBranchName = uatBranchName;
            LiveBranchName = liveBranchName;
            IsSelected = isSelected;
        }
    }
}
