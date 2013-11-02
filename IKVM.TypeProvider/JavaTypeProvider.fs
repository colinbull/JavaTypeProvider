namespace Java

open System
open System.IO
open System.Reflection
open System.Reflection.Emit
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Core.CompilerServices
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Quotations

type Cache private () =
   static let mutable instance = Dictionary<_, _>()
   static member Instance = instance

module Helpers = 

    let memoize f =
        fun n ->
            match Cache.Instance.TryGetValue(n) with
            | (true, v) -> v
            | _ ->
                let temp = f(n)
                Cache.Instance.Add(n, temp)
                temp

    let fullPath (config :TypeProviderConfig) path = 
        if Path.IsPathRooted(path)
        then path
        else Path.Combine(config.ResolutionFolder, path)

    let watchForChanges invalidate (fileName:string) = 
      let w = new FileSystemWatcher(Filter = Path.GetFileName(fileName), Path = Path.GetDirectoryName(fileName))
      w.Changed.Add(fun _ -> invalidate())
      w.EnableRaisingEvents <- true

[<TypeProvider>]
type public JavaTypeProvider(config: TypeProviderConfig) as this = 
   inherit TypeProviderForNamespaces()

   let thisAssembly = Assembly.GetExecutingAssembly()
   let rootNamespace = "Java"
   let dir = IO.Path.GetDirectoryName(config.RuntimeAssembly)
   let baseType = typeof<obj>
   let staticParams = 
        [
            ProvidedStaticParameter("JarFile", typeof<string>)
            ProvidedStaticParameter("IKVMPath", typeof<string>, Path.Combine(config.ResolutionFolder, "IKVM"))
        ]
   let containerType = ProvidedTypeDefinition(thisAssembly, rootNamespace, "JavaProvider", Some(baseType))
   
   let invalidate key = (fun () ->
        if Cache.Instance.Remove(key) 
        then 
            GlobalProvidedAssemblyElementsTable.theTable.Clear()
            this.Invalidate()
       )

   let loader (typeName, jarFile, ikvmPath) =
        this.RegisterProbingFolder(ikvmPath)
        this.RegisterProbingFolder(Path.GetDirectoryName(jarFile))
        let assemblyBytes = IKVMCompiler.compile ikvmPath config.TemporaryFolder jarFile
        let assembly = Assembly.Load(assemblyBytes)
        GlobalProvidedAssemblyElementsTable.theTable.[assembly] <- assemblyBytes

        let t = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseType))
        t.AddAssemblyTypesAsNestedTypesDelayed(fun _ -> assembly)
        t

   do containerType.DefineStaticParameters(
                         staticParams,
                         (fun typeName [| :? string as jarFile ; :? string as ikvmPath|] ->
                              let jar, ikvm = Helpers.fullPath config jarFile, Helpers.fullPath config ikvmPath
                              Helpers.watchForChanges (invalidate (typeName, jar, ikvm)) jar
                              Helpers.memoize loader (typeName, jar, ikvm)
                         ))
   do 
      this.AddNamespace(rootNamespace, [containerType]) 
                
[<TypeProviderAssembly>] 
do()

