// include Fake lib
#I @"packages/FAKE/tools/"
#r @"FakeLib.dll"

open System
open Fake
open Fake.AssemblyInfoFile
open Fake.Git
open Fake.AppVeyor

let isAppVeyorBuild = environVar "APPVEYOR" <> null

// Directories
let buildDir  = @"./build/"
let testDir   = @"./test/"
let deployDir = @"./deploy/"
let packageDir = @"./package/"
let sampleDir = @"./sample/"
let packagesDir = @"./packages/"

let projectName = "RailNet.Clients.EcoS"
let projectDescription = "An async-based ECoS model railway client for .NET"
let projectSummary = projectDescription

let releaseNotes = 
    ReadFile "ReleaseNotes.md"
    |> ReleaseNotesHelper.parseReleaseNotes

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir; packageDir]
)

Target "NuGet" (fun _ ->
    RestorePackages()
)

Target "SetVersions" (fun _ ->
    CreateCSharpAssemblyInfo "./src/RailNet.Clients.Ecos/Properties/AssemblyInfo.cs"
        [Attribute.Title "RailNet.Clients.Ecos"
         Attribute.Description projectDescription
         Attribute.Product projectName
         Attribute.Version releaseNotes.AssemblyVersion
         Attribute.Guid "17abf373-a1b5-41d4-8859-89e2b279a0b5"
         Attribute.FileVersion releaseNotes.AssemblyVersion]

    CreateCSharpAssemblyInfo "./src/RailNet.Core/Properties/AssemblyInfo.cs"
        [Attribute.Title "RailNet.Core"
         Attribute.Description projectDescription
         Attribute.Product projectName
         Attribute.Version releaseNotes.AssemblyVersion
         Attribute.FileVersion releaseNotes.AssemblyVersion
         Attribute.Guid "48353882-6320-403e-8c2e-820288731ad0"]
)

Target "CompileLib" (fun _ ->
    !! @"src/RailNet.Clients.Ecos/*.csproj"
      |> MSBuildRelease buildDir "Build"
      |> Log "LibBuild-Output: "
)

Target "CompileTest" (fun _ ->
    !! @"src/Tests/**/*.csproj"
      |> MSBuildRelease testDir "Build"
      |> Log "TestBuild-Output: "
)

Target "CompileSample" (fun _ ->
    !! @"src/Samples/**/*.csproj"
      |> MSBuildRelease sampleDir "Build"
      |> Log "SampleBuild-Output: "
)

Target "NUnitTest" (fun _ ->
    !! (testDir + @"/*Tests.dll")
      |> NUnit (fun p ->
                 {p with
                   DisableShadowCopy = true;
                   OutputFile = testDir + @"TestResults.xml"})
)

//Target "FxCop" (fun _ ->
//    !+ (buildDir + @"/**/*.dll")
//      ++ (buildDir + @"/**/*.exe")
//        |> Scan
//        |> FxCop (fun p ->
//            {p with
//                ReportFileName = testDir + "FXCopResults.xml";
//                ToolPath = fxCopRoot})
//)

Target "CreatePackage" (fun _ ->
    let net45Dir = packageDir @@ "lib/net45/"
    CleanDirs [net45Dir]

    CopyFile net45Dir (buildDir @@ "RailNet.Core.dll")
    CopyFile net45Dir (buildDir @@ "RailNet.Clients.Ecos.dll")
    CopyFile net45Dir (buildDir @@ "RailNet.Core.xml")
    CopyFile net45Dir (buildDir @@ "RailNet.Clients.Ecos.xml")

    CopyFiles packageDir ["README.md"; "ReleaseNotes.md"]

    let ShouldPublish = isAppVeyorBuild && environVar "nugetkey" <> null &&
                        (environVar "APPVEYOR_REPO_BRANCH" = "master" || environVar "APPVEYOR_REPO_BRANCH" = "dev")
    
    if ShouldPublish then printfn "Me should Publish?: %b" ShouldPublish

    let version = if ShouldPublish then 
                    match environVar "APPVEYOR_REPO_BRANCH" with
                    | "master" -> releaseNotes.NugetVersion
                    | "dev" -> releaseNotes.AssemblyVersion + "-alpha" + AppVeyorEnvironment.BuildNumber
                    | _ -> releaseNotes.NugetVersion
                    else
                      releaseNotes.NugetVersion          
      
    NuGet (fun p -> 
        {p with
            Authors = ["Jannis Schaefer"]
            Project = projectName
            Description = projectDescription
            OutputPath = deployDir
            Summary = projectSummary
            WorkingDir = packageDir
            Version = version
            ReleaseNotes = toLines releaseNotes.Notes
            AccessKey = environVarOrDefault "nugetkey" ""

            Dependencies = 
                ["NLog", GetPackageVersion packagesDir "NLog"
                 "Rx-Main", GetPackageVersion packagesDir "Rx-Main"]

            Publish = ShouldPublish }) "RailNet.Clients.Ecos.nuspec"        
)

Target "Zip" (fun _ ->
    !! (buildDir + "/**/*.*")
        -- "*.zip"
        |> Zip buildDir (deployDir + "RailNet." + releaseNotes.AssemblyVersion + ".zip")
)


"Clean"
  ==> "SetVersions"
  ==> "NuGet"
  ==> "CompileLib"
  ==> "CompileTest"  
  //  ==> "FxCop"
  ==> "NUnitTest"
  =?> ("CompileSample", not isLinux) 
  ==> "CreatePackage"
  ==> "Zip"


// start build
RunTargetOrDefault "NUnitTest"
