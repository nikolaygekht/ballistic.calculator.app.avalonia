@echo off
setlocal

rem Builds, stages, signs and zips one runtime identifier.
rem
rem Every fallible step is checked and the script exits non-zero, because a half-built release is worse
rem than no release: a publish that silently failed leaves the previous build's files staged, and a
rem signtool that silently failed ships unsigned binaries under a signed-looking name. `setlocal` also
rem restores the working directory on the way out, so an early exit cannot leave the caller in Setup\content.

if "%~1" == "" (
    echo ERROR: no runtime identifier given. Usage: pack.bat ^<rid^> ^<yes^|no^>
    exit /b 1
)
if not "%~2" == "yes" if not "%~2" == "no" (
    echo ERROR: the second argument must be yes or no ^(sign, or do not sign^). Got "%~2".
    exit /b 1
)

cd ..
set projectroot=%cd%

echo building %~1...
dotnet publish -r %~1 -c Release BallisticCalculator2.sln
if errorlevel 1 (
    echo ERROR: dotnet publish failed for %~1.
    exit /b 1
)

cd Setup
if exist content rmdir content /q /s
if exist content (
    echo ERROR: could not clear Setup\content - a file in it is probably open.
    exit /b 1
)
mkdir .\content
mkdir .\content\data

rem robocopy is the exception to `if errorlevel 1`: it reports 0-7 on success (1 means "files were
rem copied"), and only 8 and above are real failures.
robocopy "%projectroot%\Desktop\ReticleEditor\bin\Release\net8.0\%~1\publish" "%projectroot%\Setup\content" /S /NFL /NDL
if errorlevel 8 goto :copyfailed
robocopy "%projectroot%\Desktop\BallisticCalculator\bin\Release\net8.0\%~1\publish" "%projectroot%\Setup\content" /S /NFL /NDL
if errorlevel 8 goto :copyfailed
robocopy "%projectroot%\data" "%projectroot%\Setup\content\data" /S /NFL /NDL
if errorlevel 8 goto :copyfailed

rem A publish can succeed and still stage nothing shippable if one of the paths above is wrong, so check
rem that what we are about to sign and zip is actually here.
if not exist "content\BallisticCalculator2.dll" (
    echo ERROR: content\BallisticCalculator2.dll is missing - check the publish paths for %~1.
    exit /b 1
)

if "%~2" == "no" goto :skipsign
echo signing...
if "%CERTUM_CERTIFICATE_SHA1%" == "" (
    echo ERROR: CERTUM_CERTIFICATE_SHA1 is not set, so nothing can be signed.
    exit /b 1
)
cd content
signtool sign /sha1 "%CERTUM_CERTIFICATE_SHA1%" /fd sha256 /tr http://time.certum.pl /td sha256 BallisticCalculator2.exe
if errorlevel 1 goto :signfailed
signtool sign /sha1 "%CERTUM_CERTIFICATE_SHA1%" /fd sha256 /tr http://time.certum.pl /td sha256 ReticleEditor.exe
if errorlevel 1 goto :signfailed
cd ..
:skipsign

echo packing...
cd content
rem The mask is `*`, not `*.*`. 7-Zip's `*.*` matches only names that contain a dot, so it silently
rem dropped the extension-less launchers -- BallisticCalculator2 and ReticleEditor -- from the Linux and
rem macOS archives, which then held managed assemblies and native libraries but nothing to start.
rem
rem Written straight to the parent, so the archive is never a candidate for adding to itself. `7z a`
rem ADDS to an existing archive, so delete any previous one first: otherwise a re-run keeps entries that
rem are no longer shipped (a renamed data folder, for instance).
if exist "..\BallisticCaculatorPortable-%~1.zip" del "..\BallisticCaculatorPortable-%~1.zip"
7z a -r "..\BallisticCaculatorPortable-%~1.zip" *
rem 7-Zip: 0 = ok, 1 = warning (something was skipped), 2 and above = fatal. A skipped file is exactly
rem the defect that shipped archives with no launcher in them, so anything but 0 stops here.
if errorlevel 1 goto :packfailed
cd ..

if not exist "BallisticCaculatorPortable-%~1.zip" (
    echo ERROR: the archive for %~1 was not created.
    exit /b 1
)

echo done: BallisticCaculatorPortable-%~1.zip
exit /b 0

:copyfailed
echo ERROR: robocopy failed while staging %~1 ^(exit code %errorlevel%^).
exit /b 1

:signfailed
echo ERROR: signtool failed for %~1 - the binaries are NOT signed.
exit /b 1

:packfailed
echo ERROR: 7z failed for %~1 ^(exit code %errorlevel%^).
exit /b 1
