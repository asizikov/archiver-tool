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

on Unix/MacOS:

```
dotnet GZipTest.dll [compress|decompress] /path/to/source.file /path/to/processed/file
```

on Windows:

```
GZipTest.exe [compress|decompress] "C:\path\to the source.file" "C:\path\to processed.bin"
```

