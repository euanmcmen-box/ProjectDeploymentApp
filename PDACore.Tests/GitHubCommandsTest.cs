namespace PDACore.Tests;

[TestClass]
public class GitHubCommandsTest
{
    private DeploymentApplication cypherDeploymentApplication =
        new("allocine", "Cypher", "gla-Cypher-API", "dev", "uat", "master");

    private DeploymentContext liveDeploymentContext = new DeploymentContext()
    {
        DeploymentApplicationsRoot = @"C:\pda\apps",
        DeploymentTarget = DeploymentEnvironmentTarget.Live
    };

    [TestMethod]
    public void GetDeleteMergeBranchIfExistsInstruction_ShouldBeExpected()
    {
        // Arrange
        const string expected = $@"cd C:\pda\apps/gla-Cypher-API && git checkout merge-to-master && git checkout uat && git push origin --delete merge-to-master && git branch -d merge-to-master";

        // Act
        var actual =
            GitHubCommands.GetDeleteMergeBranchIfExistsInstruction(liveDeploymentContext,
                cypherDeploymentApplication);

        // Assert
        Assert.AreEqual(actual, expected);
    }
}