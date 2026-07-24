@echo off
cd ..
set projectroot=%cd%
dotnet publish -r win-x64 -c Release BallisticCalculator2.sln
dotnet publish -r linux-x64 -c Release BallisticCalculator2.sln

cd Setup
if exist content del .\content\*.* /r /q /s
if exist content rmdir content /q /s
if not exist content mkdir .\content
if not exist content\data mkdir .\content\data

robocopy "%projectroot%\Desktop\ReticleEditor\bin\Release\net8.0\win-x64\publish" "%projectroot%\Setup\content" /S
robocopy "%projectroot%\Desktop\ReticleEditor\bin\Release\net8.0\linux-x64\publish" "%projectroot%\Setup\content" /S
robocopy "%projectroot%\Desktop\BallisticCalculator\bin\Release\net8.0\win-x64\publish" "%projectroot%\Setup\content" /S
robocopy "%projectroot%\Desktop\BallisticCalculator\bin\Release\net8.0\linux-x64\publish" "%projectroot%\Setup\content" /S
robocopy "%projectroot%\data" "%projectroot%\Setup\content\data" /S

cd content
signtool sign /sha1 "%CERTUM_CERTIFICATE_SHA1%" /fd sha256 /tr http://time.certum.pl /td sha256 BallisticCalculator2.exe
signtool sign /sha1 "%CERTUM_CERTIFICATE_SHA1%" /fd sha256 /tr http://time.certum.pl /td sha256 ReticleEditor.exe

cd content
7z a -r BallisticCaculatorPortable.zip *.*
copy BallisticCaculatorPortable.zip ..
del BallisticCaculatorPortable.zip
cd ..
