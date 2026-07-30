@echo off
setlocal

rem Builds every shipped runtime identifier.
rem
rem   prepare.bat            signs the Windows binaries (the release default)
rem   prepare.bat yes        the same, said explicitly
rem   prepare.bat no         skips signing - for a local or test build with no certificate to hand
rem
rem Only the two Windows archives can be signed at all; there is nothing signtool can do with a Mach-O or
rem an ELF binary, so those four are always passed "no".
rem
rem Stops at the first failure: pack.bat exits non-zero when the publish, the staging, the signing or the
rem zipping fails, and carrying on from there would leave a set of archives where some are current and some
rem are last week's.

set "sign=%~1"
if "%sign%" == "" set "sign=yes"

if not "%sign%" == "yes" if not "%sign%" == "no" (
    echo ERROR: the argument must be yes or no ^(sign the Windows binaries, or do not^). Got "%~1".
    echo        Usage: prepare.bat ^[yes^|no^]   - defaults to yes
    exit /b 1
)

if "%sign%" == "no" (
    echo NOTE: signing is off - the Windows binaries in these archives will be unsigned.
)

rem Clear the previous run's archives, so a RID that fails this time cannot leave last time's zip sitting
rem there looking current. Guarded, or `del` complains on an already-clean tree.
if exist *.zip del /q *.zip

call pack.bat win-x64 %sign%
if errorlevel 1 goto :failed
call pack.bat win-arm64 %sign%
if errorlevel 1 goto :failed
call pack.bat linux-x64 no
if errorlevel 1 goto :failed
call pack.bat linux-arm64 no
if errorlevel 1 goto :failed
call pack.bat osx-x64 no
if errorlevel 1 goto :failed
call pack.bat osx-arm64 no
if errorlevel 1 goto :failed

echo.
if "%sign%" == "no" (
    echo All archives built - Windows binaries UNSIGNED.
) else (
    echo All archives built.
)
exit /b 0

:failed
echo.
echo RELEASE ABORTED - see the error above. The archives are not all current.
exit /b 1
