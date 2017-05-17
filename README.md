# slow-cheetah
[![NuGet package](https://img.shields.io/nuget/v/Microsoft.VisualStudio.SlowCheetah.svg)](https://nuget.org/packages/Microsoft.VisualStudio.SlowCheetah)
[![Build status](https://ci.appveyor.com/api/projects/status/qqvu367widkayo05/branch/master?svg=true)](https://ci.appveyor.com/project/jviau/slow-cheetah/branch/master)

Transformations for XML files (such as app.config) and JSON files.

Includes two primary components:
1. NuGet package that adds an msbuild task to perform tansforms on build.
2. Visual Studio extension for generating and previewing transforms.

This project has adopted the [Microsoft Open Source Code of
Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct
FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com)
with any additional questions or comments.

## Supported Platforms
* Visual Studio 2015-2017

## Supported File Types

SlowCheetah supports transformations for XML files, specified by [XDT](https://msdn.microsoft.com/en-us/library/dd465326(v=vs.110).aspx) and for JSON files, specified by [JDT](https://github.com/Microsoft/json-document-transforms). Transform files created by the extension follow these formats.

## Features

Perform transformations of XML and JSON files on build per configuration and publish profiles.

Quickly add and preview transformations to a file in the project. 

## [How to Perform Transformations](doc/transforming_files.md)
