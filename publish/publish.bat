echo off
setlocal EnableDelayedExpansion

IF "%GIT_BRANCH%"=="" (
  ECHO GIT_BRANCH variable is not set, detecting current branch
  FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse --abbrev-ref HEAD`) DO (
    SET GIT_BRANCH=%%F
  )
)

setlocal
IF "%WORKSPACE%"=="" (
  ECHO WORKSPACE variable is not set
  rem strip release/ from current path
  set WORKSPACE=!CD:~0,-8!
)

set GIT_LOCAL_BRANCH=%GIT_BRANCH:origin/=%

echo GIT_BRANCH			%GIT_BRANCH%
echo WORKSPACE			%WORKSPACE%
echo GIT_LOCAL_BRANCH	%GIT_LOCAL_BRANCH%

del /q /s %WORKSPACE%\Build 

echo **** Cleanup previous builds from %WORKSPACE%\build\****
rmdir "%WORKSPACE%\build\*.*" /q /s
mkdir %WORKSPACE%\build
cd %WORKSPACE%

echo ********* BUILD Library to folder into %WORKSPACE%\build\GenericRepository **********
dotnet build "%WORKSPACE%\GenericRepositoryCore.sln" -o "%WORKSPACE%\build\GenericRepository" -c Release  || goto :error

echo ********* EXECUTE GenericRepository TESTS *************
mkdir %WORKSPACE%\build\TestsResults
dotnet test -r c:\tmp\test --logger "trx;LogFileName=%WORKSPACE%\build\TestsResults\TestResults.trx"

del "%WORKSPACE%\build\TestsResults\GenericRepository.trx"
dotnet test -r "%WORKSPACE%\build\TestsResults"

echo ********* Publishing NuGet package from %WORKSPACE%\build\GenericRepository\*.nupkg **********
dotnet nuget push %WORKSPACE%\build\GenericRepository\*.nupkg -s "GenericRepositoryFeed" --api-key 34v4urq5hmpyljr5mbpo25cexmx7cbrljzjd3ujhlvwcbzwrt6gq

endlocal
exit /b 0

:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%