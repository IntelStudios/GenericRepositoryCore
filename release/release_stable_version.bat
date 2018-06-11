echo off
setlocal EnableDelayedExpansion
setlocal

rem Detecting GIT branch
FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse --abbrev-ref HEAD`) DO (
	SET GIT_BRANCH=%%F
)
set GIT_LOCAL_BRANCH=%GIT_BRANCH:origin/=%
echo GIT_BRANCH			%GIT_BRANCH%
echo GIT_LOCAL_BRANCH	%GIT_LOCAL_BRANCH%

rem Change directory to a location of this .bat file
pushd  %~dp0

rem Updating library version
powershell -File .\Release-Stable-Version.ps1 -branch %GIT_LOCAL_BRANCH% || goto :error

rem building package failed
IF %ERRORLEVEL% NEQ 0 (
	goto :error
)

rem Moving back to executing directory
popd

endlocal
exit /b 0

:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%