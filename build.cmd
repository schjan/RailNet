@echo off
if not exist packages\FAKE\tools\Fake.exe ( 
  src\.nuget\NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
)
if not exist packages\SourceLink.Fake\tools\SourceLink.fsx ( 
  src\.nuget\NuGet.exe install SourceLink.Fake -OutputDirectory packages -ExcludeVersion
)
if not exist packages\NUnit.Runners\tools\SourceLink.fsx ( 
  src\.nuget\NuGet.exe install NUnit.Runners -OutputDirectory packages -ExcludeVersion
)
packages\FAKE\tools\FAKE.exe build.fsx %*