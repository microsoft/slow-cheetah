WHERE /q msbuild
IF ERRORLEVEL 1 (
    ECHO Could not find msbuild
    EXIT /B
)
msbuild /t:restore %~dp0\src\SlowCheetah.sln
msbuild /t:restore %~dp0\vs\Microsoft.VisualStudio.SlowCheetah.Full.swixproj
msbuild /t:restore %~dp0\vs\Microsoft.VisualStudio.SlowCheetah.vsmanproj