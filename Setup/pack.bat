@echo off
setlocal

rem Builds, stages, signs and archives one runtime identifier.
rem
rem   pack.bat <rid> <yes|no> [zip|targz]      the format defaults to zip
rem
rem zip is right for Windows and wrong for Unix: the format carries no permission bits, so the
rem extension-less launchers arrive without the execute bit and need a chmod before they will run.
rem 7-Zip's tar writer stamps mode 0777 on every entry, so a tar.gz extracts ready to run — at the
rem cost of marking data files executable too, which is untidy and harmless. 7-Zip cannot do better;
rem it has no idea which of these files are programs.
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

set "format=%~3"
if "%format%" == "" set "format=zip"
if not "%format%" == "zip" if not "%format%" == "targz" (
    echo ERROR: the third argument must be zip or targz. Got "%~3".
    echo        Usage: pack.bat ^<rid^> ^<yes^|no^> ^[zip^|targz^]
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

echo packing %~1 as %format%...

rem Two notes that apply to both formats. The mask is `*`, not `*.*`: 7-Zip's `*.*` matches only names
rem containing a dot, so it silently dropped the extension-less launchers -- BallisticCalculator2 and
rem ReticleEditor -- from the Linux and macOS archives, which then held managed assemblies and native
rem libraries but nothing to start. And `7z a` ADDS to an existing archive, so any previous one is
rem deleted first: otherwise a re-run keeps entries that are no longer shipped (a renamed data folder,
rem for instance). Archives are written to the parent, so one is never a candidate for adding to itself.
rem
rem 7-Zip exit codes: 0 = ok, 1 = warning (something was skipped), 2 and above = fatal. A skipped file is
rem exactly the defect that shipped launcher-less archives, so anything but 0 stops here.

if "%format%" == "targz" goto :packtargz

set "archive=BallisticCalculatorPortable-%~1.zip"
if exist "%archive%" del "%archive%"
cd content
7z a -r "..\%archive%" *
if errorlevel 1 goto :packfailed
cd ..
goto :packed

:packtargz
rem Two steps rather than one pipe, deliberately: in `A | B` cmd reports only B's exit code, so a
rem failing tar step would go unnoticed -- the whole point of this script is to fail loudly. The
rem intermediate .tar lives in the parent so it cannot end up inside itself, and is deleted after.
set "archive=BallisticCalculatorPortable-%~1.tar.gz"
set "tarfile=BallisticCalculatorPortable-%~1.tar"
if exist "%archive%" del "%archive%"
if exist "%tarfile%" del "%tarfile%"
cd content
7z a -ttar "..\%tarfile%" *
if errorlevel 1 goto :packfailed
cd ..
7z a -tgzip "%archive%" "%tarfile%"
if errorlevel 1 goto :packfailed
del "%tarfile%"

:packed
if not exist "%archive%" (
    echo ERROR: the archive for %~1 was not created.
    exit /b 1
)

echo done: %archive%
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
