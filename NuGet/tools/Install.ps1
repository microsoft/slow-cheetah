param($rootPath, $toolsPath, $package, $project)

"Installing SlowCheetah to project [{0}]" -f $project.FullName | Write-Host

#Only for debugging
if(!$project){
    $project = (Get-Item C:\Temp\_NET\SlowCheetahMSBuild\SampleProject\SampleProject.csproj)

    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Build")
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Build.Engine")
    [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Build.Framework")
}

# When this package is installed we need to add a property
# to the current project, SlowCheetahTargets, which points to the
# .targets file in the packages folder

function RemoveExistingSlowCheetahPropertyGroups($projectRootElement){
    # if there are any PropertyGroups with a label of "SlowCheetah" they will be removed here
    $pgsToRemove = @()
    foreach($pg in $projectRootElement.PropertyGroups){
        if($pg.Label -and [string]::Compare("SlowCheetah",$pg.Label,$true) -eq 0) {
            # remove this property group
            $pgsToRemove += $pg
        }
    }

    foreach($pg in $pgsToRemove){
        $pg.Parent.RemoveChild($pg)
    }
}

# TODO: Revisit this later, it was causing some exceptions
function CheckoutProjFileIfUnderScc(){
    # http://daltskin.blogspot.com/2012/05/nuget-powershell-and-tfs.html
    $sourceControl = Get-Interface $project.DTE.SourceControl ([EnvDTE80.SourceControl2])
    if($sourceControl.IsItemUnderSCC($project.FullName) -and $sourceControl.IsItemCheckedOut($project.FullName)){
        $sourceControl.CheckOutItem($project.FullName)
    }
}

function EnsureProjectFileIsWriteable(){
    $projItem = Get-ChildItem $project.FullName
    if($projItem.IsReadOnly) {
        "The project file is read-only. Please checkout the project file and re-install this package" | Write-Host -ForegroundColor DarkRed
        throw;
    }
}

function ComputeRelativePathToTargetsFile(){
    $targets = Get-Item ("{0}\tools\SlowCheetah.Transforms.targets" -f $rootPath)
    $projItem = Get-Item $project.FullName

    # we need to compute the relative path to the assembly
    $startLocation = Get-Location

    Set-Location $projItem.Directory | Out-Null
    $relativePath = Resolve-Path -Relative $targets.FullName

    # reset the location
    Set-Location $startLocation | Out-Null

    return $relativePath
}

$projFile = $project.FullName


# Make sure that the project file exists
if(!(Test-Path $projFile)){
    throw ("Project file not found at [{0}]" -f $projFile)
}

# use MSBuild to load the project and add the property

# This is what we want to add to the project
#  <PropertyGroup Label="SlowCheetah">
#      <SlowCheetah_EnableImportFromNuGet Condition=" '$(SC_EnableImportFromNuGet)'=='' ">true</SlowCheetah_EnableImportFromNuGet>
#      <SlowCheetah_NuGetImportPath Condition=" '$(SlowCheetah_NuGetImportPath)'=='' ">$([System.IO.Path]::GetFullPath( $(Filepath) ))</SlowCheetah_NuGetImportPath>
#      <SlowCheetahTargets Condition=" '$(SlowCheetah_EnableImportFromNuGet)'=='true' and Exists('$(SlowCheetah_NuGetImportPath)') ">$(SlowCheetah_NuGetImportPath)</SlowCheetahTargets>
#  </PropertyGroup>


# EnsureProjectFileIsWriteable
# Before modifying the project save everything so that nothing is lost
$DTE.ExecuteCommand("File.SaveAll")
CheckoutProjFileIfUnderScc
EnsureProjectFileIsWriteable

# checkout the project file if it's under source control
# CheckoutProjFileIfUnderScc

$relPathToTargets = ComputeRelativePathToTargetsFile

$projectMSBuild = [Microsoft.Build.Construction.ProjectRootElement]::Open($projFile)

RemoveExistingSlowCheetahPropertyGroups -projectRootElement $projectMSBuild
$propertyGroup = $projectMSBuild.AddPropertyGroup()
$propertyGroup.Label = "SlowCheetah"

$propEnableNuGetImport = $propertyGroup.AddProperty('SlowCheetah_EnableImportFromNuGet', 'true');
$propEnableNuGetImport.Condition = ' ''$(SC_EnableImportFromNuGet)''=='''' ';

$importStmt = ('$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\{0} ))' -f $relPathToTargets)
$propNuGetImportPath = $propertyGroup.AddProperty('SlowCheetah_NuGetImportPath', "$importStmt");
$propNuGetImportPath.Condition = ' ''$(SlowCheetah_NuGetImportPath)''=='''' ';

$propImport = $propertyGroup.AddProperty('SlowCheetahTargets', '$(SlowCheetah_NuGetImportPath)');
$propImport.Condition = ' ''$(SlowCheetah_EnableImportFromNuGet)''==''true'' and Exists(''$(SlowCheetah_NuGetImportPath)'') ';

$projectMSBuild.Save()

"SlowCheetah has been installed into project [{0}]" -f $project.FullName| Write-Host -ForegroundColor DarkGreen
"`nFor more info how to enable SlowCheetah on build servers see http://sedodream.com/2012/12/24/SlowCheetahBuildServerSupportUpdated.aspx" | Write-Host -ForegroundColor DarkGreen