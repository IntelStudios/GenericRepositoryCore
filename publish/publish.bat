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

del /QS %WORKSPACE%\Build 

set msbuild_location="C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsMSBuildCmd.bat"
set mstest_location="C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe"
set GITDIR=%WORKSPACE%\git

IF NOT EXIST %msbuild_location% (
  echo %msbuild_location% does not exist. Please install Visual Studio
  exit /b 1  
)

IF NOT EXIST %mstest_location% (
  echo %mstest_location% does not exist. Please install Visual Studio
  exit /b 1  
)

echo **** Update Library Version ***
cd %WORKSPACE%/publish/
powershell -File %WORKSPACE%/publish/Update-Version.ps1 -branch %GIT_LOCAL_BRANCH% 

rem package already exists
IF %ERRORLEVEL% == -1 (
	echo exit /b 0
)

rem building package failed
IF %ERRORLEVEL% NEQ 0 (
	goto :error
)

echo **** Cleanup previous builds ****
rmdir "%WORKSPACE%\build\*.*" /q /s
mkdir %WORKSPACE%\build
call %msbuild_location%
cd %WORKSPACE%

echo ********* BUILD Library to folder **********
MSbuild "%WORKSPACE%\GenericRepository.sln" /p:outdir="%WORKSPACE%\build\GenericRepository" /p:Configuration="Release" /p:Platform="Any CPU" /v:minimal /clp:Summary  || goto :error

echo ********* EXECUTE GenericRepository TESTS *************
mkdir %WORKSPACE%\build\TestsResults

del "%WORKSPACE%\build\TestsResults\GenericRepository.trx"
%mstest_location% /testcontainer:"%WORKSPACE%\build\GenericRepository\GenericRepository.Test.dll" /resultsfile:"%WORKSPACE%\build\TestsResults\GenericRepository.trx"

echo ********* BUILD GenericRepository NUGET PACKAGE *************
echo Mounting NuGet repository %NUGET_STORAGE% as drive M:
%SystemRoot%\System32\net.exe use M: %NUGET_STORAGE% /user:%NUGET_USERNAME% %NUGET_PASSWORD% /persistent:no

IF "%GIT_LOCAL_BRANCH%"=="master" (
  ECHO Deleting old master packages
  del M:\*GenericRepository*master*
)

M:\nuget.exe pack %WORKSPACE%\publish\Package.nuspec -OutputDirectory M:\ || goto :error
echo Unmounting drive M:
%SystemRoot%\System32\net.exe use M: /delete /yes

endlocal
exit /b 0

:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%