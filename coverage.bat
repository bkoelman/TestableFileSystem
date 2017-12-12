@echo off
rd /s /q coverage 2>nul
md coverage

set configuration=Debug
set opencover="%USERPROFILE%\.nuget\packages\OpenCover\4.6.519\tools\OpenCover.Console.exe"
set reportgenerator="%USERPROFILE%\.nuget\packages\ReportGenerator\3.1.0\tools\ReportGenerator.exe"
set testrunner="%USERPROFILE%\.nuget\packages\xunit.runner.console\2.3.1\tools\net452\xunit.console.x86.exe"
set targets=".\src\Fakes.Tests\bin\%configuration%\net452\TestableFileSystem.Fakes.Tests.dll .\src\Analyzer.Tests\bin\%configuration%\net452\TestableFileSystem.Analyzer.Tests.dll -noshadow"
set filter="+[TestableFileSystem*]*  -[TestableFileSystem.*.Tests*]*  -[TestableFileSystem.Wrappers*]*"
set coveragefile=".\coverage\CodeCoverage.xml"

%opencover% -register:user -target:%testrunner% -targetargs:%targets% -filter:%filter% -hideskipped:All -output:%coveragefile%
%reportgenerator% -targetdir:.\coverage -reports:%coveragefile%
