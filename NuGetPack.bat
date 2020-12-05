@echo off
msbuild ReadableExpressions /t:Pack /p:PackageOutputPath=../NuGet /p:Configuration=Release /v:m