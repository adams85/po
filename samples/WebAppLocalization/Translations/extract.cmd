@ECHO OFF

SETLOCAL EnableDelayedExpansion

SET Languages=en-US,de-DE

FOR %%L IN (%Languages%) DO (
  SET LanguageSwitches=!LanguageSwitches! -l %%L
)

echo Extracting translations...

cd "%~dp0\.."
dotnet po scan -p WebAppLocalization.csproj | dotnet po extract -o "%~dp0\WebApp.pot" -m %LanguageSwitches% --no-backup
