namespace FSharpx

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

[<TypeProvider>]
type public IKVMTypeProvider(config: TypeProviderConfig) as this = 
   inherit TypeProviderForNamespaces()
   
   let debug msg =
       System.Diagnostics.Debug.WriteLine(msg);

   
   let thisAssembly = Assembly.GetExecutingAssembly()
   let rootNamespace = "FSharpx"
   let dir = IO.Path.GetDirectoryName(config.RuntimeAssembly)
   let baseType = typeof<obj>
   let staticParams = 
        [
            ProvidedStaticParameter("JarFile", typeof<string>)
            ProvidedStaticParameter("IKVMPath", typeof<string>, Path.Combine(config.ResolutionFolder, "IKVM"))
        ]
   let containerType = ProvidedTypeDefinition(thisAssembly, rootNamespace, "IKVMProvider", Some(baseType))
   
   let loader (typeName, jarFile, ikvmPath) =
        this.RegisterProbingFolder(ikvmPath)
        let assemblyPath = IKVM.compile ikvmPath dir jarFile
        let t = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseType))
        t.AddAssemblyTypesAsNestedTypesDelayed(fun () -> Assembly.LoadFrom(assemblyPath))
        t

   do containerType.DefineStaticParameters(
                         staticParams,
                         (fun typeName [| :? string as jarFile ; :? string as ikvmPath|] ->
                              Helpers.memoize loader (typeName, jarFile, ikvmPath)
                         ))
   do this.AddNamespace(rootNamespace, [containerType]) 
                
[<TypeProviderAssembly>] 
do()

