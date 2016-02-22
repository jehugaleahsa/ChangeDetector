C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild ../ChangeDetector.sln /p:Configuration=Release
nuget pack ../ChangeDetector/ChangeDetector.csproj -Prop Configuration=Release
nuget push *.nupkg
del *.nupkg