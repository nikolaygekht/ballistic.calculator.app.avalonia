@echo off
rem Refresh src/raw/*.ds from the compiled assembly. Run after source code changes.
dotnet build project.proj /t:Scan,Raw
