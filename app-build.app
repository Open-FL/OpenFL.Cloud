name: fl-cloud-server
branch: Release
project-name: OpenFL.Cloud.Console
flags: NO_INFO_TO_ZIP;NO_STRUCTURE

#Build Info
solution: .\src\OpenFL.Cloud.sln
buildout: .\src\%project-name%\bin\%branch%\netcoreapp2.1\publish
buildcmd: dotnet publish {0} -c Release
include: %buildout%\*
target: %buildout%\%project-name%.dll
output: .\docs\latest\%name%.zip