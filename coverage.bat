@echo off
rd /s /q coverage 2>nul
md coverage

set configuration=Debug
set opencover="%USERPROFILE%\.nuget\packages\OpenCover\4.7.922\tools\OpenCover.Console.exe"
set reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\4.0.15\tools\net47\ReportGenerator.exe"
set testrunner="%USERPROFILE%\.nuget\packages\xunit.runner.console\2.4.1\tools\net472\xunit.console.x86.exe"
set targets=".\src\Fakes.Tests\bin\%configuration%\net452\TestableFileSystem.Fakes.Tests.dll .\src\Analyzer.Tests\bin\%configuration%\net472\TestableFileSystem.Analyzer.Tests.dll -noshadow"
set filter="+[TestableFileSystem*]*  -[TestableFileSystem.*.Tests*]*  -[TestableFileSystem.Wrappers*]*"
set coveragefile=".\coverage\CodeCoverage.xml"

%opencover% -register:user -target:%testrunner% -targetargs:%targets% -filter:%filter% -hideskipped:All -output:%coveragefile%
%reportgenerator% -targetdir:.\coverage -reports:%coveragefile%
