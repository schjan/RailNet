#!/bin/bash
if [ ! -f packages/FAKE/tools/Fake.exe ]; then
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FSharp.Formatting.CommandTool -OutputDirectory packages -ExcludeVersion -Prerelease 
  mono --runtime=v4.0 src/.nuget/NuGet.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion 
fi
echo "LS packages"
ls packages
echo "LS packages/tools"
ls packages/tools
mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe build.fsx $@