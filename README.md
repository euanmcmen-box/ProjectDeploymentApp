# ProjectDeploymentApp
The "Project Deployment App" creates pull requests to UAT or Live for a selection of applications.

> [!TIP]
> Run the application as close to the root drive as possible.  For example, I run the application from the following root path: "C:\pda\ProjectDeploymentApp.exe"
>This avoids any issues with long file names.

## Pre-requisite Installation

### Installing Github CLI

The application uses the [Github CLI](https://cli.github.com/) to create pull requests.  Github CLI can be installed using Chocolatey, using `choco install gh`

More installation information for Github CLI:

https://github.com/cli/cli#installation

## Configuration

### Setting the Github Personal Access Token in the application config

The application uses a Github "Personal Access Token" to create pull requests using the Github CLI on your behalf.  To create a Personal Access Token, go to https://github.com/settings/tokens and generate a classic token with the "repo" scope.

The "ProjectDeploymentApp.dll.config" file is the app.config equivilant file.  It contains the key "`GITHUB_TOKEN`"  This must be set to the Github Personal Access Token created above.

### Adding and Removing Applications from the Applications.json file

The application references Github repositories specified in the Applications.json file.  New solutions created can be added here, and old solutions can be removed.

## First Time Run

### Initialisating the Repositories

The application will check for the presense of repositories it expects to work with.  If it cannot find the applications or the "apps" directory, the `Initialse Repos` button will be enabled.

This will run a series of commands and clone all repos.

Cloning all repositories isn't required on subsequent runs -- the repositories are persisted in the application's working directory at the relative path "/apps".

You can delete the "/apps" folder and re-clone all repositories if you want to.

## Going Forward

This section assumes the application is running normally and all repositories are cloned.

### Selecting the Deployment Target and Projects to Deploy

The dropdown contains two values: UAT and Live.  These define whether the deployment should target the UAT environment or the Live environment respectively.

Select projects using the checkboxes.

### Creating PRs

Press "Create PRs" to run commands and create pull requests in the browser for each selected project.  The pull requests are opened in the browser as previews and can be created or created as draft using the Github controls.

Sometimes a selected project won't have any changes and no PR can be created.

### Cleaning up Unmerged or Unchanged Projects

In the event that a selected project has no changes and therefore no PR, a trailing "merge-to-" branch will remain on the `origin` until deleted.

Press this button to delete all "merge-to" branches for all selected projects.

> [!WARNING]
> This will delete the merge branches for unmerged PRs for any selected projects as well.  Ensure **all** PRs created in the previous step are merged; or deselect all projects and select the projects to clean up the "merge-to-" branches for.

## Screenshots

### First Launch of the Application

![First Launch 1](Images/Project%20Deployment%20App%20-%20First%20Run%20-%201.png)

### First Launch after running Repository Initialisation

![First Launch 2](Images/Project%20Deployment%20App%20-%20First%20Run%20-%202.png)

### Subsequent Launches and Normal Operation

![Normal Operation](Images/Project%20Deployment%20App%20Ready.png)