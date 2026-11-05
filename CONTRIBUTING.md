# Microsoft.VisualStudio.SlowCheetah

This project welcomes contributions and suggestions. Most contributions require you to
agree to a Contributor License Agreement (CLA) declaring that you have the right to,
and actually do, grant us the rights to use your contribution. For details, visit
https://cla.microsoft.com.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/)
or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Pull requests

When you submit a pull request, a CLA-bot will automatically determine whether you need
to provide a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the
instructions provided by the bot. You will only need to do this once across all repositories using our CLA.
## Prerequisites

All dependencies can be installed by running the `init.ps1` script at the root of the repository
using Windows PowerShell or [PowerShell Core][pwsh] (on any OS).
Some dependencies installed by `init.ps1` may only be discoverable from the same command line environment the init script was run from due to environment variables, so be sure to launch Visual Studio or build the repo from that same environment.
Alternatively, run `init.ps1 -InstallLocality Machine` (which may require elevation) in order to install dependencies at machine-wide locations so Visual Studio and builds work everywhere.

The only prerequisite for building, testing, and deploying from this repository
is the [.NET SDK](https://get.dot.net/).
You should install the version specified in `global.json` or a later version within
the same major.minor.Bxx "hundreds" band.
For example if 2.2.300 is specified, you may install 2.2.300, 2.2.301, or 2.2.310
while the 2.2.400 version would not be considered compatible by .NET SDK.
See [.NET Core Versioning](https://learn.microsoft.com/dotnet/core/versions/) for more information.

## Package restore

The easiest way to restore packages may be to run `init.ps1` which automatically authenticates
to the feeds that packages for this repo come from, if any.
`dotnet restore` or `nuget restore` also work but may require extra steps to authenticate to any applicable feeds.

## Building

The Visual Studio extension is built on Windows with the Visual Studio development tools installed.

Building and packing the Visual Studio extension require full Visual Studio MSBuild.
From a Visual Studio Developer PowerShell, run `msbuild src\SlowCheetah.slnx -restore -p:Configuration=Release`.
Managed tests use xUnit v3 on Microsoft.Testing.Platform and can be run with `dotnet test --solution src\SlowCheetah.slnx --no-build -c Release`.

## Testing

You can use `dotnet test` to build and/or test the repo.

There may be tests that are known to be unstable or have special requirements. These can be avoided by running tests using the [dotnet-test-cloud.ps1](tools/dotnet-test-cloud.ps1) script *after* running `dotnet build`.

The test projects target .NET Framework 4.7.2 and run as managed tests, not NativeAOT executables.
Run `.\tools\dotnet-test-cloud.ps1 -Configuration Release` after building to collect coverage and exclude tests with the `FailsInCloudTest=true` trait.
Use xUnit's `--filter-method`, `--filter-class`, or `--filter-trait` options after `--` when filtering individual `dotnet test` runs; TUnit's `--treenode-filter` does not apply.

## Releases

Use `nbgv tag` to create a tag for a particular commit that you mean to release.
[Learn more about `nbgv` and its `tag` and `prepare-release` commands](https://dotnet.github.io/Nerdbank.GitVersioning/docs/nbgv-cli.html).

Push the tag.

### Azure Pipelines

When your repo builds with Azure Pipelines, use the `azure-pipelines/release.yml` pipeline.
Trigger the pipeline by adding the `auto-release` tag on a run of your main `azure-pipelines.yml` pipeline.

## Tutorial and API documentation

Markup links are checked in CI with [mlc](https://github.com/becheran/mlc).
Run `mlc` from the repository root to use the same settings, configured in `.mlc.toml`.

API and hand-written docs are found under the `docfx/` directory and are built by [docfx](https://dotnet.github.io/docfx/).

You can make changes and host the site locally to preview them by switching to that directory and running the `dotnet docfx --serve` command.
After making a change, you can rebuild the docs site while the localhost server is running by running `dotnet docfx` again from a separate terminal.

DocFX metadata extraction suppresses `NU1901` through `NU1904` in `docfx/docfx.json`.
DocFX otherwise turns MSBuild workspace warnings into fatal documentation warnings under `--warningsAsErrors`.
Normal builds still report these audit warnings, and other documentation warnings remain fatal in CI.

The `.github/workflows/docs.yml` GitHub Actions workflow publishes the content of these docs to github.io if the workflow itself and [GitHub Pages is enabled for your repository](https://docs.github.com/en/pages/quickstart).

### Documentation validation and Copilot feed authentication

The `.github/workflows/docs_validate.yml` and `.github/workflows/copilot-setup-steps.yml`
workflows use GitHub OIDC to authenticate
as the **azure-public/vside package pull** Entra application before `init.ps1` restores packages.
It requests an Azure DevOps access token and supplies it through
`NuGetPackageSourceCredentials_<source name>` for each Azure Artifacts source in
`nuget.config`, including repositories that use a different name such as
`msft_consumption_public`.
No client secret or PAT is required, and no credentials are written to `nuget.config`.

Run `tools/Configure-GitHubOidc.ps1` from the repository to configure GitHub's immutable
OIDC subject format and create matching Entra federated credentials.
Sign in with `gh auth login` and
`az login --tenant 72f988bf-86f1-41af-91ab-2d7cd011db47 --allow-no-subscriptions` first.
The signed-in identities need repository administration and permission to manage the app's
federated credentials. Use `-WhatIf` to preview without making changes.
The script discovers the calling repository, trusts pull requests, its default branch,
and the `copilot` environment used by Copilot code review and cloud agent setup.
It can be rerun without duplicating credentials.
Use `-Branches microbuild,release` to add other trusted branches; the default branch
and the PR and Copilot environment contexts are always included.
The script reports each created or reused credential.
Credential names identify the owner, repository, and PR, environment, or branch context, for example
`github-AArnott-Library.Template-pull-request`,
`github-AArnott-Library.Template-environment-copilot`, and
`github-AArnott-Library.Template-branch-main`. Names requiring punctuation replacement
or truncation include a short hash suffix to preserve uniqueness within Entra's name limits.

This changes OIDC subjects for **all workflows** in the repository. Update any other cloud
trust policies (including environment subjects) before running it; existing Entra credentials
are preserved. The script does not configure Azure DevOps feed permissions.

For repositories configured before Copilot environment trust was included, rerun the script
with any additional trusted branches retained, then re-request the Copilot review.
Copilot setup runs use an OIDC subject ending in `:environment:copilot`, rather than the
PR or branch subject, so those existing credentials alone do not authenticate Copilot.
Without the environment credential, Azure login fails with `AADSTS700213` before package restore.

Authentication is enabled only for repositories owned by the `microsoft` organization,
because this Entra tenant requires enterprise-issued GitHub assertions.
Same-repository dependency update PRs, including Renovate and Dependabot, authenticate
using the job's explicit `id-token: write` permission.
Repositories owned by non-microsoft accounts and fork PRs
skip authentication and retain anonymous restore behavior;
new upstream dependencies may still need to be ingested by a trusted run first.
Do not switch this workflow to `pull_request_target` to give untrusted PR code credentials.
Repositories based on this template must configure their own trusted subjects and, if necessary,
update the application and tenant IDs in the workflow.

## Updating dependencies

This repo uses Renovate to keep dependencies current.
Configuration is in the `.github/renovate.json` file.
[Learn more about configuring Renovate](https://docs.renovatebot.com/configuration-options/).

When changing the renovate.json file, follow [these validation steps](https://docs.renovatebot.com/config-validation/).

If Renovate is not creating pull requests when you expect it to, check that the [Renovate GitHub App](https://github.com/apps/renovate) is configured for your account or repo.

## Merging latest from Library.Template

### Maintaining your repo based on this template

The best way to keep your repo in sync with Library.Template's evolving features and best practices is to periodically merge the template into your repo:

```ps1
git fetch
git checkout origin/main
./tools/MergeFrom-Template.ps1
# resolve any conflicts, then commit the merge commit.
git push origin -u HEAD
```

[pwsh]: https://learn.microsoft.com/powershell/scripting/install/installing-powershell
