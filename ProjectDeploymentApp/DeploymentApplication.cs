namespace ProjectDeploymentApp
{
    public class DeploymentApplication
    {
        public string RootUrl { get; set; }
        public string Name { get; }

        public bool IsSelected { get; set; }

        public string RepositoryName { get; }

        public string DevBranchName { get; }

        public string UatBranchName { get; }

        public string LiveBranchName { get; }

        public DeploymentApplication(string rootUrl, string name, string repositoryName, string devBranchName, string uatBranchName, string liveBranchName, bool isSelected = false)
        {
            RootUrl = rootUrl;
            Name = name;
            RepositoryName = repositoryName;
            DevBranchName = devBranchName;
            UatBranchName = uatBranchName;
            LiveBranchName = liveBranchName;
            IsSelected = isSelected;
        }
    }
}
