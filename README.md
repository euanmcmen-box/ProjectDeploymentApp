# ProjectDeploymentApp
The "Project Deployment App" creates pull requests to UAT or Live for a selection of applications.

## Installation and Prerequisites
### Installing Github CLI
The application uses the [Github CLI](https://cli.github.com/) to create pull requests.  Github CLI can be installed using Chocolatey, using `choco install gh`

More installation information for Github CLI:

https://github.com/cli/cli#installation

### Creating a Github Personal Access Token

The application uses a Github "Personal Access Token" to create pull requests using the Github CLI on your behalf.  To create a Personal Access Token, go to https://github.com/settings/tokens and generate a classic token with the "repo" scope.

## Configuration
The "ProjectDeploymentApp.dll.config" file is the app.config equivilant file.  

It contains the key "`GITHUB_TOKEN`"  This must be set to the Github Personal Access Token created above.
