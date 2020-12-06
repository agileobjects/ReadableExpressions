@echo off
dotnet restore
msbuild ReadableExpressions /t:Pack /p:PackageOutputPath=../NuGet /p:Configuration=Release /v:m