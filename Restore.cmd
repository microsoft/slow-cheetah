@echo off
WHERE /q msbuild
IF ERRORLEVEL 1 (
    ECHO Error: Could not find msbuild. Make sure msbuild is in the PATH and try again.
    EXIT /B %ERRORLEVEL%
)
msbuild /t:restore %~dp0\src\SlowCheetah.sln