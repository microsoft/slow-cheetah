param($rootPath, $toolsPath, $package, $project)

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
        # "pg label: [{0}]" -f $pg.Label | Write-Host -ForegroundColor Green
        if($pg.Label -and [string]::Compare("SlowCheetah",$pg.Label,$true) -eq 0) {
            # remove this property group
            $pgsToRemove += $pg
        }
    }

    foreach($pg in $pgsToRemove){
        # "Removing SlowCheetah Property Group" | Write-Host -ForegroundColor Red
        $pg.Parent.RemoveChild($pg)
    }
}

"SLOWCHEETAH INSTALLING" | Write-Host
$projFile = $project.FullName


# Make sure that the project file exists
if(!(Test-Path $projFile)){
    throw ("Project file not found at [{0}]" -f $projFile)
}

# use MSBuild to load the project and add the property

# This is what we want to add to the project
#  <PropertyGroup Label="SlowCheetah">
#      <SlowCheetah_EnableImportFromNuGet Condition=" '$(SC_EnableImportFromNuGet)'=='' ">true</SlowCheetah_EnableImportFromNuGet>
#      <SlowCheetah_NuGetImportPath Condition=" '$(SlowCheetah_NuGetImportPath)'=='' ">insert-full-path</SlowCheetah_NuGetImportPath>
#      <SlowCheetahTargets Condition=" '$(SlowCheetah_EnableImportFromNuGet)'=='true' and Exists('$(SlowCheetah_NuGetImportPath)') ">$(SlowCheetah_NuGetImportPath)</SlowCheetahTargets>
#  </PropertyGroup>

"Updating project [{0}] to import SlowCheetah .targets file" | Write-Host

$projectMSBuild = [Microsoft.Build.Construction.ProjectRootElement]::Open($projFile)

RemoveExistingSlowCheetahPropertyGroups -projectRootElement $projectMSBuild
$propertyGroup = $projectMSBuild.AddPropertyGroup()
$propertyGroup.Label = "SlowCheetah"

$propEnableNuGetImport = $propertyGroup.AddProperty('SlowCheetah_EnableImportFromNuGet', 'true');
$propEnableNuGetImport.Condition = ' ''$(SC_EnableImportFromNuGet)''=='''' ';

$propNuGetImportPath = $propertyGroup.AddProperty('SlowCheetah_NuGetImportPath', "toolsPath: $toolsPath");
$propNuGetImportPath.Condition = ' ''$(SlowCheetah_NuGetImportPath)''=='''' ';

$propImport = $propertyGroup.AddProperty('SlowCheetahTargets', '$(SlowCheetah_NuGetImportPath)');
$propImport.Condition = ' ''$(SlowCheetah_EnableImportFromNuGet)''==''true'' and Exists(''$(SlowCheetah_NuGetImportPath)'') ';

$projectMSBuild.Save()
