# slow-cheetah
[![NuGet package](https://img.shields.io/nuget/v/SlowCheetah.svg)](https://nuget.org/packages/SlowCheetah)
[![Build status](https://ci.appveyor.com/api/projects/status/qqvu367widkayo05/branch/master?svg=true)](https://ci.appveyor.com/project/sayedihashimi/slow-cheetah/branch/master)

XML Transforms for app.config and other XML files.

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
* Visual Studio 2012-2017

## Supported File Types

SlowCheetah supports transformations for XML files, specified by [XDT](https://msdn.microsoft.com/en-us/library/dd465326(v=vs.110).aspx). Transform file created by the extension follow this format.

## Features

Perform transformations of XML files on build per configuration and publish profiles.

Quickly add and preview transaformations to a file in the project. 

## [How to Perform Transformations](doc/transforming_files.md)
