namespace PDACore.Tests;

[TestClass]
public class GitHubCommandsTest
{
    private DeploymentApplication cypherDeploymentApplication =
        new("allocine", "Cypher", "gla-Cypher-API", "dev", "uat", "master");

    private DeploymentContext liveDeploymentContext = new DeploymentContext
    {
        DeploymentApplicationsRoot = @"C:\pda\apps",
        DeploymentTarget = DeploymentEnvironmentTarget.Live
    };

    [TestMethod]
    public void GetDeleteMergeBranchIfExistsInstruction_ShouldBeExpected()
    {
        // Arrange
        const string expected = @"cd C:\pda\apps/gla-Cypher-API && git checkout merge-to-master && git checkout uat && git push origin --delete merge-to-master && git branch -d merge-to-master";

        // Act
        var actual =
            GitHubCommands.GetDeleteMergeBranchIfExistsInstruction(liveDeploymentContext,
                cypherDeploymentApplication);

        // Assert
        Assert.AreEqual(actual, expected);
    }

    [TestMethod]
    public void GetCreateMergeBranchInstruction_ShouldBeExpected()
    {
        // Arrange
        const string expected = @"cd C:\pda\apps/gla-Cypher-API && git checkout uat && git checkout -b merge-to-master && git merge uat && git push --set-upstream origin merge-to-master";

        // Act
        var actual =
            GitHubCommands.GetCreateMergeBranchInstruction(liveDeploymentContext,
                cypherDeploymentApplication);

        // Assert
        Assert.AreEqual(actual, expected);
    }

    [TestMethod]
    public void GetCreatePullRequestCommandText_ShouldBeExpected()
    {
        // Arrange
        const string expected = @"gh pr create --repo ""https://github.com/allocine/gla-Cypher-API"" --head ""merge-to-master"" --base ""master"" --title ""deploy to live"" --body ""deploy to live"" --web";

        // Act
        var actual =
            GitHubCommands.GetCreatePullRequestCommandText(liveDeploymentContext,
                cypherDeploymentApplication);

        // Assert
        Assert.AreEqual(actual, expected);
    }

    [TestMethod]
    public void GetRefreshBranchesInstruction_ShouldBeExpected()
    {
        // Arrange
        const string expected = @"cd C:\pda\apps/gla-Cypher-API && git checkout uat && git pull && git checkout master && git pull";

        // Act
        var actual =
            GitHubCommands.GetRefreshBranchesInstruction(liveDeploymentContext,
                cypherDeploymentApplication);

        // Assert
        Assert.AreEqual(actual, expected);
    }

    [TestMethod]
    public void GetCloneRepositoryInstruction_ShouldBeExpected()
    {
        // Arrange
        const string expected = @"cd C:\pda\apps && git clone https://github.com/allocine/gla-Cypher-API";

        // Act
        var actual =
            GitHubCommands.GetCloneRepositoryInstruction(liveDeploymentContext.DeploymentApplicationsRoot,
                cypherDeploymentApplication);

        // Assert
        Assert.AreEqual(actual, expected);
    }



}