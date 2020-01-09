# archiver-tool


![](https://github.com/asizikov/archiver-tool/workflows/build-application/badge.svg)

## How to build

make sure you have `.NET Core 3` SDK installed.

navigate to the solution folder and execute
```
dotnet build --configuration Release
dotnet test
```

## How to run

```
dotnet run GZipTest compress "/path/to/source.file" "/path/to/compressed/file"
```