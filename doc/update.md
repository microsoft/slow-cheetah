# SlowCheetah in VS 2017 

We have made significant changes to the SlowCheetah extension and NuGet package. This is because the old version had the following limitations:
- Installation methods were inconsistent throughout versions, mixing VS extensions with NuGet packages
- Build tools were installed directly to the local app data
- Users' project files were manually edited to include SC files
- Unnecessary files were imported into the project

To fix these issues, the new version includes the following: 
- Support for Visual Studio 2015 and 2017 
- A VS extension that assists with adding and previewing transform 
- A NuGet package that handles all the build and transformation logic 

To use this new version, the older one must be manually removed from your project before installing the new version. From here on, all updates to the extension and NuGet packages will be handled by their respective platform.  

## Instructions 

If SlowCheetah has never been installed on your computer or used in any of your projects, simply install the latest Nuget package [here](https://www.nuget.org/packages/Microsoft.VisualStudio.SlowCheetah/).

Version 3.0.61 and higher of the SlowCheetah Extension should prompt the user to automatically remove any present older installations. If you have issues with this, the following instructions guide you through manually updating your project.

If you have used SlowCheetah before, remove the following lines from your project file. 

First, a PropertyGroup that looks like this:

``` XML
<PropertyGroup Label="SlowCheetah">
  <SlowCheetahToolsPath>$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\..\packages\SlowCheetah.2.5.15\tools\))</SlowCheetahToolsPath>
  <SlowCheetah_EnableImportFromNuGet Condition=" '$(SlowCheetah_EnableImportFromNuGet)'=='' ">true</SlowCheetah_EnableImportFromNuGet>
  <SlowCheetah_NuGetImportPath Condition=" '$(SlowCheetah_NuGetImportPath)'=='' ">$([System.IO.Path]::GetFullPath( $(MSBuildProjectDirectory)\Properties\SlowCheetah\SlowCheetah.Transforms.targets ))</SlowCheetah_NuGetImportPath>
  <SlowCheetahTargets Condition=" '$(SlowCheetah_EnableImportFromNuGet)'=='true' and Exists('$(SlowCheetah_NuGetImportPath)') ">$(SlowCheetah_NuGetImportPath)</SlowCheetahTargets>
</PropertyGroup>
```
Then, remove an import that looks like this:

``` XML
<Import Project="$(SlowCheetahTargets)" Condition="Exists('$(SlowCheetahTargets)')" Label="SlowCheetah" />
```

Lastly, remove the following includes:

``` XML
<None Include="packageRestore.proj" />
<None Include="Properties\SlowCheetah\SlowCheetah.Transforms.targets" />
```

Also, delete the `Properties\SlowCheetah` folder and the `packageRestore.proj` file from your project if they are present. If there are any other lines in the project file or any items in the project tree related to SlowCheetah, those should also be removed. All SlowCheetah related lines that  should be present are the transformation files which are marked with `<IsTransformFile>true</IsTransformFile>` on the project file.

Now, install or update to the latest SlowCheetah package through the NuGet package manager and download the latest extension from the Visual Studio extension gallery.

Optionally, if you no longer plan on using the older version of SlowCheetah on any projects, you may safely delete the `%LocalAppData%\Microsoft\MSBuild\SlowCheetah` folder.
