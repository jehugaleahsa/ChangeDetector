msbuild ../ChangeDetector.sln /p:Configuration=Release
nuget pack ../ChangeDetector/ChangeDetector.csproj -Prop Configuration=Release
nuget push *.nupkg
del *.nupkg