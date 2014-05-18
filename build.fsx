// include Fake lib
#I @"tools/FAKE/tools/"
#I @"packages/FAKE/tools/"
#r @"FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile

// Directories
let buildDir  = @"./build/"
let testDir   = @"./test/"
let deployDir = @"./deploy/"
let sampleDir = @"./sample/"
let packagesDir = @"./packages/"

// version info
let version = "0.0"  // or retrieve from CI server

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; testDir; deployDir]
)

Target "NuGet" (fun _ ->
    RestorePackages()
)

//Target "SetVersions" (fun _ ->
//    CreateCSharpAssemblyInfo "./src/app/Calculator/Properties/AssemblyInfo.cs"
//        [Attribute.Title "Calculator Command line tool"
//         Attribute.Description "Sample project for FAKE - F# MAKE"
//         Attribute.Guid "A539B42C-CB9F-4a23-8E57-AF4E7CEE5BAA"
//         Attribute.Product "Calculator"
//         Attribute.Version version
//         Attribute.FileVersion version]
//
//    CreateCSharpAssemblyInfo "./src/app/CalculatorLib/Properties/AssemblyInfo.cs"
//        [Attribute.Title "Calculator library"
//         Attribute.Description "Sample project for FAKE - F# MAKE"
//         Attribute.Guid "EE5621DB-B86B-44eb-987F-9C94BCC98441"
//         Attribute.Product "Calculator"
//         Attribute.Version version
//         Attribute.FileVersion version]
//)

Target "CompileLib" (fun _ ->
    !! @"src/RailNet*/*.csproj"
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

Target "Zip" (fun _ ->
    !+ (buildDir + "/**/*.*")
        -- "*.zip"
        |> Scan
        |> Zip buildDir (deployDir + "RailNet." + version + ".zip")
)

// Dependencies
"Clean"
 // ==> "SetVersions"
  ==> "NuGet"
  ==> "CompileLib"
  ==> "CompileTest"
  =?> ("CompileSample", not isLinux) 
//  ==> "FxCop"
  ==> "NUnitTest"
  ==> "Zip"

// start build
RunTargetOrDefault "NUnitTest"
