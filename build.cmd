@echo off
if not exist tools\FAKE\tools\Fake.exe ( 
  tools\NuGet.exe install FAKE -OutputDirectory tools -ExcludeVersion
)
if not exist tools\SourceLink.Fake\tools\SourceLink.fsx ( 
  tools\NuGet.exe install SourceLink.Fake -OutputDirectory tools -ExcludeVersion
)
if not exist tools\NUnit.Runners\tools\SourceLink.fsx ( 
  tools\NuGet.exe install NUnit.Runners -OutputDirectory tools -ExcludeVersion
)
tools\FAKE\tools\FAKE.exe build.fsx %*