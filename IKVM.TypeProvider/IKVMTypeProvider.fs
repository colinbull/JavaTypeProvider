namespace FSharpx

open System
open System.Reflection
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Core.CompilerServices
open Samples.FSharp.ProvidedTypes
open IKVM
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
   let baseType = typeof<obj>
   let outputPath = config.ResolutionFolder + @"\bin\Debug"
   let staticParams = 
        [
            ProvidedStaticParameter("JarFile", typeof<string>)
            ProvidedStaticParameter("IKVMPath", typeof<string>,  System.IO.Path.Combine(config.ResolutionFolder,@"IKVM\bin"))
        ]
   let containerType = ProvidedTypeDefinition(thisAssembly, rootNamespace, "IKVM", Some(baseType))

   let assemblyResolver ikvmPath = ResolveEventHandler(fun _ args ->
        let asmName = AssemblyName(args.Name)
        let expectedName = asmName.Name + ".dll"
        let paths = 
            [
                IO.Path.Combine(outputPath, expectedName)
                IO.Path.Combine(ikvmPath, expectedName)
            ]
        let assembly =
            List.tryPick (fun path ->
                if IO.File.Exists path 
                then 
                    Debug.WriteLine(sprintf "Resolved: %s" path)
                    Some(Assembly.LoadFrom path)
                else None
            ) paths
        match assembly with
        | Some(a) -> a
        | None -> null)
    
   let addToNamespace (assembly : Assembly) = 
       assembly.GetExportedTypes()
       |> Seq.groupBy (fun t -> t.Namespace)
       |> Seq.iter (fun (ns, ts) -> 
                          let t = ts |> Seq.map (fun t -> ProvidedTypeDefinition(assembly, ns, t.Name, Some(t))) |> Seq.toList
                          this.AddNamespace(ns, t)
                       )

   let getTypes (typ : ProvidedTypeDefinition, ikvmPath,jarFile) = 
       let outDll = IKVM.compile ikvmPath outputPath jarFile
       let ass = ProvidedAssembly.RegisterGenerated(outDll)
       typ.AddAssemblyTypesAsNestedTypesDelayed(fun _ -> ass)

   do containerType.DefineStaticParameters(
                                      staticParams,
                                      (fun typeName [| :? string as jarFile; :? string as ikvmPath|] ->
                                          Helpers.memoize (fun ps ->
                                                    let ikvm, jar = ps
                                                    System.AppDomain.CurrentDomain.add_AssemblyResolve (assemblyResolver ikvm)
                                                    let  t= ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseType))
                                                    (getTypes(t, ikvm,jar))
                                                    t
                                          ) (ikvmPath, jarFile)
                                          
                                    ))

   do this.AddNamespace(rootNamespace, [containerType]) 
    
                   
[<TypeProviderAssembly>] 
do()

