@echo off

SETLOCAL

SET CLONE_TEMP_PATH=%TEMP%\AspNetSkeleton

REM https://stackoverflow.com/questions/28889166/do-batch-files-support-exit-traps#answer-28890881

if "%~1" equ ":main" (
  shift /1
  goto main
)

cmd /d /c "%~f0" :main %*

ECHO.
ECHO Cleaning up...

rmdir /s /q %CLONE_TEMP_PATH%

exit /b

:main

rmdir /s /q %CLONE_TEMP_PATH% 2>nul

git clone https://github.com/adams85/aspnetskeleton2.git %CLONE_TEMP_PATH%

call %CLONE_TEMP_PATH%\restore-tools.cmd

exit /b