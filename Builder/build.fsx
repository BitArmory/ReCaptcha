//#if INTERACTIVE
//open System
//Environment.CurrentDirectory <- workingDir
//#else
//#endif

// include Fake lib

#r "paket:

nuget Fake.Core                    = 5.8.4
nuget Fake.Core.Target             = 5.13.5
nuget Fake.Core.Xml                = 5.13.5
nuget Fake.Runtime                 = 5.13.5
nuget Fake.DotNet.NuGet            = 5.13.5
nuget Fake.DotNet.Cli              = 5.13.5
nuget Fake.DotNet.AssemblyInfoFile = 5.13.5
nuget Fake.DotNet.MSBuild          = 5.13.5
nuget Fake.JavaScript.Npm          = 5.13.5
nuget Fake.IO.FileSystem           = 5.13.5
nuget Fake.IO.Zip                  = 5.13.5
nuget Fake.Tools.Git               = 5.13.5
nuget Fake.DotNet.Testing.xUnit2   = 5.13.5
nuget Fake.BuildServer.AppVeyor    = 5.13.5

nuget SharpCompress = 0.22.0
nuget FSharp.Data = 2.4.6

nuget secure-file                  = 1.0.31

nuget Z.ExtensionMethods.WithTwoNamespace
nuget System.Runtime.Caching //"



#load ".\\.fake\\build.fsx\\intellisense.fsx"

#load @"Utils.fsx"

#if !FAKE
  #r "netstandard"
#endif

open Fake
open Utils
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.DotNet
open Fake.Core
open Fake.IO.Globbing
open Fake.JavaScript
open Utils
open Z.ExtensionMethods

open System.Reflection
open System.Collections.Generic
open SharpCompress
open SharpCompress.Archives
open Newtonsoft.Json

open Fake.BuildServer

BuildServer.install[
   AppVeyor.Installer
]

let workingDir = ChangeWorkingFolder();

Trace.trace (sprintf "WORKING DIR: %s" workingDir)

let ProjectName = "BitArmory.ReCaptcha";
let GitHubUrl = "https://github.com/BitArmory/ReCaptcha"

let Folders = Setup.Folders(workingDir, workingDir)
let Files = Setup.Files(ProjectName, Folders)

let TestProject = TestProject("BitArmory.ReCaptcha.Tests", Folders)

let TestProjects = [ TestProject; ]

let NugetProjects = [
                       NugetProject("BitArmory.ReCaptcha", "BitArmory.ReCaptcha for .NET", Folders)
                    ]

let AllNugetProjects = NugetProjects;

Target.description "MAIN BUILD TASK"
Target.create "dnx" (fun _ ->
    Trace.trace "DNX Build Task"

    let buildConfig (opts : DotNet.BuildOptions) (bc : DotNet.BuildConfiguration) =
        { opts with 
                Configuration = bc}
    
    for np in AllNugetProjects do
      let debug (bo : DotNet.BuildOptions ) = buildConfig bo DotNet.BuildConfiguration.Debug
      let release (bo : DotNet.BuildOptions) = buildConfig bo DotNet.BuildConfiguration.Release

      DotNet.build debug np.Folder
      DotNet.build release np.Folder
      Shell.copyDir np.OutputDirectory (np.Folder @@ "bin") FileFilter.allFiles

    for tp in TestProjects do
      let debug (bo : DotNet.BuildOptions ) = buildConfig bo DotNet.BuildConfiguration.Debug
      let release (bo : DotNet.BuildOptions) = buildConfig bo DotNet.BuildConfiguration.Release
      DotNet.build debug tp.Folder
      DotNet.build release tp.Folder
)

Target.description "NUGET PACKAGE RESTORE TASK"
Target.create "restore" (fun _ -> 
     Trace.trace ".NET Core Restore"
     
     for dp in NugetProjects do
         DotNet.restore id dp.Folder
         
     //and setup test proejcts
     for tp in TestProjects do
         DotNet.restore id tp.Folder
)

open System.Xml

Target.description "NUGET PACKAGE TASK"
Target.create "nuget" (fun _ ->
    Trace.trace "NuGet Task"

    let config (opts: DotNet.PackOptions) =
         {opts with
               Configuration = DotNet.BuildConfiguration.Release
               OutputPath = Some(Folders.Package) }

    for np in AllNugetProjects do
         DotNet.pack config np.Folder
)

Target.description "PROJECT ZIP TASK"
Target.create "zip" (fun _ -> 
    Trace.trace "Zip Task"

    !!(Folders.CompileOutput @@ "**") 
       |> Zip.zip Folders.CompileOutput (Folders.Package @@ sprintf "%s.zip" ProjectName)
)


let MakeAttributes (includeSnk:bool) =
    let attrs = [
                    AssemblyInfo.Description GitHubUrl
                ]
    if includeSnk then
        let pubKey = ReadFileAsHexString Files.SnkFilePublic
        let visibleTo = sprintf "%s, PublicKey=%s" TestProject.Name pubKey
        attrs @ [ AssemblyInfo.InternalsVisibleTo(visibleTo) ]
    else
        attrs @ [ AssemblyInfo.InternalsVisibleTo(TestProject.Name) ]

Target.description "PROJECT BUILDINFO TASK"
Target.create "BuildInfo" (fun _ ->
    
    Trace.trace "Writing Assembly Build Info"

    for p in AllNugetProjects do
       MakeBuildInfo p (fun bip -> 
           { bip with
               ExtraAttrs = MakeAttributes(BuildContext.IsRelease) } )

       Xml.pokeInnerText p.ProjectFile "/Project/PropertyGroup/Version" BuildContext.FullVersion

       let releaseNotes = History.NugetText Files.History GitHubUrl
       Xml.pokeInnerText p.ProjectFile "/Project/PropertyGroup/PackageReleaseNotes" releaseNotes
)

Target.description "PROJECT CLEAN TASK"
Target.create "Clean" (fun _ ->
    
    Shell.cleanDirs [Folders.CompileOutput; Folders.Package;]

    for p in AllNugetProjects do
       Xml.pokeInnerText p.ProjectFile "/Project/PropertyGroup/Version" "0.0.0-localbuild"
       Xml.pokeInnerText p.ProjectFile "/Project/PropertyGroup/PackageReleaseNotes" ""
       Xml.pokeInnerText p.ProjectFile "/Project/PropertyGroup/AssemblyOriginatorKeyFile" ""
       Xml.pokeInnerText p.ProjectFile "/Project/PropertyGroup/SignAssembly" "false"

       MakeBuildInfo p (fun bip ->
            {bip with
               DateTime = System.DateTime.Parse("1/1/2015")
               ExtraAttrs = MakeAttributes(false)
               VersionContext = Some "0.0.0-localbuild"
               })

)


Target.create "ci" (fun _ ->
    Trace.trace "ci Task"
)

Target.description "PROJECT TEST TASK"
Target.create "test" (fun _ ->
   Trace.trace "TEST"
   let config (opts: DotNet.TestOptions) =
      {opts with  
            NoBuild = true }
   DotNet.test config TestProject.Folder
)

Target.description "CI TEST TASK"
Target.create "citest" (fun _ ->
   Trace.trace "CI TEST"
    
   let config (opts: DotNet.TestOptions) =
      {opts with
            NoBuild = true
            Logger = Some "Appveyor"
            TestAdapterPath = Some "." }
   
   DotNet.test config TestProject.Folder
)

Target.description "PROJECT SIGNING KEY SETUP TASK"
Target.create "setup-snk"(fun _ ->
    Trace.trace "Decrypting Strong Name Key (SNK) file."
    let decryptSecret = Environment.environVarOrFail "SNKFILE_SECRET"
    Helpers.decryptFile Files.SnkFile decryptSecret

    for np in AllNugetProjects do
       Xml.pokeInnerText np.ProjectFile "/Project/PropertyGroup/AssemblyOriginatorKeyFile" Files.SnkFile
       Xml.pokeInnerText np.ProjectFile "/Project/PropertyGroup/SignAssembly" "true"
)

open Fake.Core.TargetOperators

// Build order and dependencies are read from left to right. For example,
// Do_This_First ==> Do_This_Second ==> Do_This_Third
//     Clean     ==>     Restore    ==>     Build
//
// REFERENCE:
//  ( ==> ) x y               | Defines a dependency - y is dependent on x
//  ( =?> ) x (y, condition)  | Defines a conditional dependency - y is dependent on x if the condition is true
//  ( <== ) x y
//  ( <=> ) x y               | Defines that x and y are not dependent on each other but y is dependent on all dependencies of x.
//  ( <=? ) y x
//  ( ?=> ) x y               | Defines a soft dependency. x must run before y, if it is present, but y does not require x to be run.

"Clean" ==> "restore" ==> "BuildInfo"

"BuildInfo" =?> ("setup-snk", BuildContext.IsTaggedBuild) ==> "dnx" ==> "zip"

"dnx" ==> "nuget"

"dnx" ==> "test"

"nuget" <=> "zip" ==> "ci"


// start build
Target.runOrDefault "dnx"
