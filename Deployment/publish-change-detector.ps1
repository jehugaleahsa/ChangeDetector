&dotnet pack "..\ChangeDetector\ChangeDetector.csproj" --configuration Release --output $PWD

.\NuGet.exe push ChangeDetector.*.nupkg -Source https://www.nuget.org/api/v2/package

Remove-Item ChangeDetector.*.nupkg