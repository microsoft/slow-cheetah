# SlowCheetah Build Targets

### Summary
SlowCheetah alters the build process for projects to ensure that the transformations are executed in the correct order and that the transformed files are used properly. To allow users to extend the behavior for their own build process, this document outlines all the native targets for SlowCheetah, including properties and items that can be modified to alter existing target behavior.

## Main

### Targets

**ScCollectTransformFiles**: Collects all the files with *TransformOnBuild* true and adds them to *ScFilesToTransform*, with the necessary metadata for transformation. Runs before *ScApplyTransforms*.

**ScApplyTransforms**: First creates the destination directory for all the transformations that will occur. Then, applies the transform task to each item of *ScFilesToTransform* per their metadata. Runs after *CopyFilesToOutputDirectory*.

### Properties

**ScAllowReferencedConfig**: Whether config files from referenced projects are allowed when resolving references. Defaults to true.

**BuildDependsOn**: List of targets that should be run as part of the build process. *ScApplyTransforms* get appended to this.

### ItemGroups

**ScFilesToTransform**: Populated in ScCollectTransformationFiles. Contains all the data for files that should be transformed. Metadata:

- *SourceFile*: Transformation source.

- *TransformFile*: Transformation input.

- *DestinationFile*: Output file of transformation. The directory for this file is created in case it does not previously exist.

## App

### Targets

**ScCollectAppFiles**: Removes the app.config file from *ScFilesToTransform* and adds it to ScAppConfigToTransform for separate transformation. Copies the original app.config to *ScIntermediateAppConfig*. Depends on *ScCollectTransformFiles*. Runs before *ScApplyTransforms*.

**ScTransformAppConfig**: Performs transformations on *ScIntermediateAppConfig*. Replaces the app config location before it is copied to the output. Depends on *ScCollectAppFiles*. Runs before *_CopyAppConfig*.

### Properties

**ScIsApp**: Whether the project is an application. Used in *SetupProject* and *ClickOnce* targets. If *ScIsWap* is true, sets to false. Defaults to true.

**ScAppConfigName**: Name of the application configuration file that should be especially transformed. Defaults to “app.config”.

### Item Groups

**ScAppConfigToTransform**: Populated in *ScCollectAppFiles*. Contains data for application configuration files that should be transformed. For metadata, see *ScFilesToTransform*.

## Web

### Targets

**ScCollectWebFiles**: Gets the publish configuration and adds publish transformation data to ScFilesToTransform. Depends on *ScCollectTransformFiles*. Runs before *ScApplyWebTransforms*.

**ScApplyWebTransforms**: Inserted into the deploy pipeline. Depends on *ScApplyTransforms*.

### Properties

**ScIsWap**: Whether the project is a web application. Verifies the project GUIDs. Defaults to false.

**ScAppConfigName**: Changed to “web.config”.

### ItemGroups

**ScFilesToTransform**: Adds publish transform metadata:
- *PublishDestinationFile*: Location of the temporary directory used to publish from
- *PublishTransformFile*: Location of the publish profile transform file.

## Additional Targets

### SetupProject

Ensures a Setup Project uses the correct location for copying output files. Enabled if *ScEnableSetupProjects* is true (default is true).

### ClickOnce

Ensures that the published files are extracted from the correct location for Click Once publishing.