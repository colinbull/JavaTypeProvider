#r "tools/Fake/tools/FakeLib.dll"

open Fake
open System.IO

let nugetPath = Path.Combine(__SOURCE_DIRECTORY__,@"tools\NuGet\NuGet.exe")


let projectName, version = "JavaTypeProvider",  if isLocalBuild then ReadFileAsString "local_build_number.txt" else tcBuildNumber

let buildDir, nugetDir, deployDir = @"build\artifacts", @"build\nuget", @"build\deploy"
let nugetDocsDir = nugetDir @@ "docs"
let ikvmDir = ".\IKVM"
let nugetKey = if System.IO.File.Exists "./nuget-key.txt" then ReadFileAsString "./nuget-key.txt" else ""

let appReferences = !! @".\**\JavaTypeProvider.fsproj"

Target "RestorePackages" RestorePackages

Target "Clean" (fun _ -> 
    CleanDirs [buildDir]
)

Target "AssemblyInfo" (fun _ -> 

        AssemblyInfo (fun p -> 
            { p with 
                CodeLanguage = FSharp
                AssemblyVersion = version
                AssemblyTitle = projectName
                Guid = "7F7075C7-D6D3-491D-8B31-15254D77F871"
                OutputFileName = "./JavaTypeProvider/AssemblyInfo.fs"                
            })

)

Target "BuildApp" (fun _ ->
    MSBuild buildDir "Build" ["Configuration","Release"; "Platform", "anycpu"] appReferences |> Log "BuildApp: "


)

Target "BuildNuGet" (fun _ ->
    CleanDirs [nugetDir]
    XCopy ikvmDir (nugetDir @@ "IKVM")
    [
        "lib", buildDir + "\JavaTypeProvider.dll"
    ] |> Seq.iter (fun (folder, path) -> 
                    let dir = nugetDir @@ folder @@ "net40"
                    CreateDir dir
                    CopyFile dir path)
    NuGet (fun p ->
        {p with             
            Authors = ["Colin Bull"]
            Project = projectName
            Description = "Java Type Provider"
            Version = version
            OutputPath = nugetDir
            WorkingDir = nugetDir
            AccessKey = nugetKey
           // ToolPath = "tools\Nuget\Nuget.exe"
            Publish = nugetKey <> ""})
        ("./JavaTypeProvider.nuspec")
    [
       (nugetDir) + sprintf "\JavaTypeProvider.%s.nupkg" version
    ] |> CopyTo deployDir
)


Target "Default" DoNothing

"RestorePackages"
    ==> "Clean"
    ==> "AssemblyInfo"
    ==> "BuildApp"
    ==> "BuildNuGet"
    ==> "Default"
  
if not isLocalBuild then
    "Clean" ==> "SetAssemblyInfo" ==> "BuildApp" |> ignore

// start build
RunParameterTargetOrDefault "target" "Default"
