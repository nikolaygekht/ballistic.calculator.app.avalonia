@echo off
rem Update PackageReference versions within their declared ranges (respects upper bounds).
rem   UpdateDeps.bat              -> dry-run report
rem   UpdateDeps.bat --apply      -> write the updates back to the .csproj files
dotnet run --project Tools\DependencyUpdater -c Release -- %*
