#!/bin/bash
if [ ! -f tools/FAKE/tools/Fake.exe ]; then
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FAKE -OutputDirectory packages/tools -ExcludeVersion
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FSharp.Formatting.CommandTool -OutputDirectory packages/tools -ExcludeVersion -Prerelease 
  mono --runtime=v4.0 src/.nuget/NuGet.exe install SourceLink.Fake -OutputDirectory packages/tools -ExcludeVersion 
fi
echo "LS"
ls
echo "LS TOOLS"
ls tools
echo "LS ./TOOLS"
ls ./tools
echo "LS SRC"
ls src
echo "LS SRC/TOOLS"
ls src/tools
echo "LS packages"
ls packages
echo "LS packages/FAKE"
ls packages/FAKE
echo "LS packages/FAKE/tools"
ls packages/FAKE/tools
mono --runtime=v4.0 ./tools/FAKE/tools/FAKE.exe build.fsx $@