# ProjectDeploymentApp
The "Project Deployment App" creates pull requests to UAT or Live for a selection of applications.

It uses the `gh pr create` command from https://cli.github.com/.

## Configuration
The "ProjectDeploymentApp.dll.config" file is the app.config equivilant file.  It contains the key "`GITHUB_TOKEN`"  This must be set to a Github Personal Access Token.

This token is used by the application to create pull requests for applications on your behalf.

To create a Personal Access Token, go to https://github.com/settings/tokens and generate a classic token with the "repo" scope.
