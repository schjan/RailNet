#!/bin/bash
if [ ! -f packages/tools/FAKE/tools/Fake.exe ]; then
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FAKE -OutputDirectory packages/tools -ExcludeVersion
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FSharp.Formatting.CommandTool -OutputDirectory packages/tools -ExcludeVersion -Prerelease 
  mono --runtime=v4.0 src/.nuget/NuGet.exe install SourceLink.Fake -OutputDirectory packages/tools -ExcludeVersion 
fi
echo "LS packages"
ls packages
echo "LS packages/tools"
ls packages/tools
mono --runtime=v4.0 .packages/tools/FAKE/tools/FAKE.exe build.fsx $@