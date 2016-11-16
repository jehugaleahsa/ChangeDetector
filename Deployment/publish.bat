"C:\Program Files (x86)\MSBuild\14.0\Bin" ../ChangeDetector.sln /p:Configuration=Release
nuget pack ../ChangeDetector/ChangeDetector.csproj -Prop Configuration=Release
nuget push *.nupkg
del *.nupkg