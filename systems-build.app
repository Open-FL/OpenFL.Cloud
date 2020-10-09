name: fl-cloud-systems
branch: Release
project-name: OpenFL.Cloud
flags: NO_INFO_TO_ZIP;NO_STRUCTURE

#Build Info
solution: .\src\OpenFL.Cloud.sln
buildout: .\src\%project-name%\bin\%branch%\netstandard2.0\publish
buildcmd: dotnet publish {0} -c Release
include: %buildout%\Open*.dll;%buildout%\Plugin*.dll;%buildout%\System*.dll;%buildout%\Utility.dll;%buildout%\Newtonsoft.Json.dll
target: %buildout%\%project-name%.dll
output: .\docs\latest\%name%.zip