
// include Fake lib
#I @"../packages/build/FAKE/tools"
#I @"../packages/build/DotNetZip/lib/net20"
#r @"FakeLib.dll"
#r @"DotNetZip.dll"

#load @"Utils.fsx"

open Fake
open Utils
open System.Reflection
open Helpers
open Fake.Testing.NUnit3
open Fake

let workingDir = ChangeWorkingFolder();

trace (sprintf "WORKING DIR: %s" workingDir)

let ProjectName = "BitArmory.ReCaptcha";
let GitHubUrl = "https://github.com/BitArmory/ReCaptcha"

let Folders = Setup.Folders(workingDir)
let Files = Setup.Files(Folders)

let Projects = Setup.Projects(ProjectName, Folders)

let TestProject = TestProject("BitArmory.ReCaptcha.Tests", Folders)

let SignAssembly = false //BuildContext.IsTaggedBuild


let NugetProjects = [
                       NugetProject("BitArmory.ReCaptcha", "BitArmory.ReCaptcha for .NET", Folders)
                    ]


let AllNugetProjects = NugetProjects;


Target "msb" (fun _ ->
   
    let tag = "msb_build";

    let buildProps = [ 
                        "AssemblyOriginatorKeyFile", Projects.SnkFile
                        "SignAssembly", SignAssembly.ToString()
                     ]
   
    // USE SOLUTION FILE WITH MSBUILD, ESPEICALLY WITH MULTI_TARGET BUILDS THAT HAVE:
    // <TargetFrameworks>net40;netstandard1.3;netstandard2.0</TargetFrameworks>
    !! Projects.SolutionFile
    |> MSBuildReleaseExt null buildProps "Build"
    |> Log "AppBuild-Output: "

    //copy outputs.
    traceFAKE "Copying MS Build outputs..."

    for dp in NugetProjects do
        CopyDir (dp.OutputDirectory @@ tag) dp.MsBuildFolderRelease allFiles
    
)



Target "dnx" (fun _ ->
    trace "DNX Build Task"

    let tag = "dnx_build"
    
    for np in AllNugetProjects do
      DotnetBuild np tag
)

Target "restore" (fun _ -> 
     trace "MS NuGet Project Restore"

     let lookIn = Folders.Lib @@ "build"
     let toolPath = findToolInSubPath "NuGet.exe" lookIn

     tracefn "NuGet Tool Path: %s" toolPath

     Projects.SolutionFile
     |> RestoreMSSolutionPackages (fun p ->
            { 
              p with 
                OutputPath = (Folders.Source @@ "packages" )
                ToolPath = toolPath
            }
        )

     trace ".NET Core Restore"

     for dp in NugetProjects do
         DotnetRestore dp
 )


Target "info" (fun _ ->

   trace "info"

   //let nupkgs = !!(Folders.Package @@"*.nupkg")
   //             --("**" @@ "**symbols**")
   
   let files = !!(Folders.Package @@ "*")

   for x in files do
      printfn "%s" x

 )

open Ionic.Zip
open System.Xml

Target "nuget" (fun _ ->
    trace "NuGet Task"
    

    for np in NugetProjects do
         DotnetPack np Folders.Package
   
    //Copy
    findAndCopy "*.nupkg" Folders.CompileOutput Folders.Package
)

Target "push" (fun _ ->
    trace "NuGet Push Task"
    
    failwith "Only CI server should publish on NuGet"
)



Target "zip" (fun _ -> 
    trace "Zip Task"

    !!(Folders.CompileOutput @@ "**") 
       -- (Folders.CompileOutput @@ "**" @@ "*.deps.json")
       -- (Folders.CompileOutput @@ "**" @@ "*.vsixmanifest")
       |> Zip Folders.CompileOutput (Folders.Package @@ sprintf "%s.zip" ProjectName)
)

open AssemblyInfoFile

let MakeAttributes (includeSnk:bool) =
    let attrs = [
                    Attribute.Description GitHubUrl
                ]
    if includeSnk then
        let pubKey = ReadFileAsHexString Projects.SnkFilePublic
        let visibleTo = sprintf "%s, PublicKey=%s" TestProject.Name pubKey
        attrs @ [ Attribute.InternalsVisibleTo(visibleTo) ]
    else
        attrs @ [ Attribute.InternalsVisibleTo(TestProject.Name) ]


Target "BuildInfo" (fun _ ->
    
    trace "Writing Assembly Build Info"

    for p in AllNugetProjects do
       MakeBuildInfo p p.Folder (fun bip -> 
           { bip with
               ExtraAttrs = MakeAttributes(SignAssembly) })

       XmlPokeInnerText p.ProjectFile "/Project/PropertyGroup/Version" BuildContext.FullVersion

       let releaseNotes = History.NugetText Files.History GitHubUrl
       XmlPokeInnerText p.ProjectFile "/Project/PropertyGroup/PackageReleaseNotes" releaseNotes
)


Target "Clean" (fun _ ->
    DeleteFile Files.TestResultFile
    CleanDirs [Folders.CompileOutput; Folders.Package]

    for p in AllNugetProjects do
       XmlPokeInnerText p.ProjectFile "/Project/PropertyGroup/Version" "0.0.0-localbuild"
       XmlPokeInnerText p.ProjectFile "/Project/PropertyGroup/PackageReleaseNotes" ""
       XmlPokeInnerText p.ProjectFile "/Project/PropertyGroup/AssemblyOriginatorKeyFile" ""
       XmlPokeInnerText p.ProjectFile "/Project/PropertyGroup/SignAssembly" "false"

       MakeBuildInfo p p.Folder (fun bip ->
            {bip with
               DateTime = System.DateTime.Parse("1/1/2015")
               ExtraAttrs = MakeAttributes(false) } )

)

open Fake.Testing

let RunTests() =
    CreateDir Folders.Test

    let tool = findToolInSubPath "nunit3-console.exe" Folders.Lib
    
    //Test Data Sets
    !! TestProject.TestAssembly
    |> NUnit3 (fun p -> { p with 
                            ProcessModel = NUnit3ProcessModel.SingleProcessModel
                            ToolPath = tool
                            ShadowCopy = false
                            ResultSpecs = [Files.TestResultFile]
                            ErrorLevel = TestRunnerErrorLevel.Error }) 

open Fake.AppVeyor

Target "ci" (fun _ ->
    
    trace "ci Task"
)

Target "test" (fun _ ->
    trace "TEST"
    RunTests()
)

Target "citest" (fun _ ->
    trace "CI TEST"
    RunTests()
    AppVeyor.UploadTestResultsXml TestResultsType.NUnit3 Folders.Test
)


Target "setup-snk"(fun _ ->
    trace "Decrypting Strong Name Key (SNK) file."
    let decryptSecret = environVarOrFail "SNKFILE_SECRET"
    decryptFile Projects.SnkFile decryptSecret

    for np in AllNugetProjects do
       XmlPokeInnerText np.ProjectFile "/Project/PropertyGroup/AssemblyOriginatorKeyFile" Projects.SnkFile
       XmlPokeInnerText np.ProjectFile "/Project/PropertyGroup/SignAssembly" "true"
)


"Clean"
    ==> "restore"
    ==> "BuildInfo"

//build systems, order matters
"BuildInfo"
    =?> ("setup-snk", SignAssembly)
    ==> "msb"
    ==> "zip"

"BuildInfo"
    =?> ("setup-snk", SignAssembly)
    ==> "dnx"
    ==> "zip"

"BuildInfo"
    =?> ("setup-snk", SignAssembly)
    ==> "zip"

"dnx"
    ==> "nuget"


"nuget"
    ==> "ci"

"nuget"
    ==> "push"

"zip"
    ==> "ci"

"citest"
    ==> "ci"

//test task depends on msbuild
"msb"
    ==> "test"



// start build
RunTargetOrDefault "msb"
