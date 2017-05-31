@echo off
WHERE /q msbuild
IF ERRORLEVEL 1 (
    ECHO Error: Could not find msbuild. Make sure msbuild is in the PATH and try again.
    EXIT /B %ERRORLEVEL%
)
msbuild /t:restore %~dp0\src\SlowCheetah.sln
msbuild /t:restore %~dp0\vs\Microsoft.VisualStudio.SlowCheetah.Full.swixproj
msbuild /t:restore %~dp0\vs\Microsoft.VisualStudio.SlowCheetah.vsmanproj