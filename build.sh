#!/bin/bash
if [ ! -f tools/FAKE/tools/Fake.exe ]; then
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FAKE -OutputDirectory tools -ExcludeVersion
  mono --runtime=v4.0 src/.nuget/NuGet.exe install FSharp.Formatting.CommandTool -OutputDirectory tools -ExcludeVersion -Prerelease 
  mono --runtime=v4.0 src/.nuget/NuGet.exe install SourceLink.Fake -OutputDirectory tools -ExcludeVersion 
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
mono --runtime=v4.0 ./tools/FAKE/tools/FAKE.exe build.fsx $@