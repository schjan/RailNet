#!/bin/bash
if [ ! -f packages/FAKE/tools/Fake.exe ]; then
  mono --runtime=v4.0 tools/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
  mono --runtime=v4.0 tools/NuGet.exe install FSharp.Formatting.CommandTool -OutputDirectory packages -ExcludeVersion -Prerelease 
  mono --runtime=v4.0 tools/NuGet.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion 
  mono --runtime=v4.0 tools/NuGet.exe install NUnit.Runners -OutputDirectory tools -ExcludeVersion 
  mono --runtime=v4.0 tools/NuGet.exe install NUnit -OutputDirectory tools -ExcludeVersion
fi
mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe build.fsx $@
