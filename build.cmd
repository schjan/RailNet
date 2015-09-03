@echo off

tools\NuGet.exe install FAKE -OutputDirectory tools -ExcludeVersion
tools\NuGet.exe install NUnit.Runners -OutputDirectory tools -ExcludeVersion
tools\NuGet.exe install GitVersion.CommandLine -OutputDirectory tools -ExcludeVersion
tools\NuGet.exe install FSharp.Data -OutputDirectory tools -ExcludeVersion

tools\FAKE\tools\FAKE.exe build.fsx %*