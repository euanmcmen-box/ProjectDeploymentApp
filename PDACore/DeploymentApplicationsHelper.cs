namespace PDACore;

public static class DeploymentApplicationsHelper
{
    public static List<DeploymentApplication> GetDeploymentApplications()
    {
        return new List<DeploymentApplication>()
        {
            new("euanmcmen-box", "HelloPlanet", "HelloPlanet", "develop", "master", ""),
            new("allocine", "Banshee", "gla-Banshee", "dev", "uat", "main"),
            new("allocine", "Boost", "gla-BoostTicketing", "dev", "uat", "master"),
            new("allocine", "Cyclops", "gla-Cyclops", "dev", "uat", "main"),
            new("allocine", "Cypher", "gla-Cypher-API", "dev", "uat", "master"),
            new("allocine", "Iceman", "gla-Iceman-API", "dev", "uat", "master"),
            new("allocine", "Legion", "gla-Legion", "dev", "uat", "master"),
            new("allocine", "Nightcrawler", "gla-Nightcrawler", "dev", "uat", "master"),
            new("allocine", "Quicksilver", "gla-Quicksilver-API", "dev", "uat", "master"),
            new("allocine", "Sage", "gla-Sage", "dev", "uat", "main")
        };
    }
}