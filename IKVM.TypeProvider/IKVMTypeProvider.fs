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
   let containerType = ProvidedTypeDefinition(thisAssembly, rootNamespace, "IKVMProvider", Some(baseType), IsErased = false)
   
   let walkType (t : Type) =   
       let ty = ProvidedTypeDefinition(t.Name, Some(t), IsErased = false, SuppressRelocation = false)
       
       let getParameters (m : MethodBase) =
           m.GetParameters() 
           |> Seq.map (fun pi -> ProvidedParameter(pi.Name, pi.ParameterType, pi.IsOut, pi.IsOptional))
           |> Seq.toList
   
       let getMembers (mi : MemberInfo) =
           match mi with
           | :? MethodInfo as m ->  
               ProvidedMethod(methodName = m.Name,
                              parameters = getParameters(m), 
                              returnType = m.ReturnType,
                              InvokeCode = (fun args -> Expr.Call(args.Head, m, args.Tail))
                              ) :> MemberInfo
           | :? ConstructorInfo as c ->
               ProvidedConstructor(getParameters c, 
                                   InvokeCode = (fun args -> Expr.NewObject(c, args))
                                   ) :> MemberInfo

       let getMembers (t : Type) = 
           t.GetMembers() |> Seq.map getMembers |> Seq.toList
       
       ty.AddMembersDelayed(fun () -> getMembers t)
       ty.ConvertToGenerated(IO.Path.Combine(dir, t.Name + ".dll"))
       ty
   

   let loader (typeName, jarFile, ikvmPath) =
        let bytes = IKVM.compile ikvmPath dir jarFile
        let t = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseType), IsErased = false, SuppressRelocation = false)
        let jarAssembly = Assembly.Load(bytes)
        t.AddMembersDelayed(fun () -> 
            [
                for t in jarAssembly.GetExportedTypes() do
                    yield walkType t
            ]
        )
        t

   let handler = System.ResolveEventHandler(fun _ args ->
        let asmName = AssemblyName(args.Name)
        // assuming that we reference only dll files
        let expectedName = asmName.Name + ".dll"
        let expectedLocation =
            // we expect to find this assembly near the dll with type provider
            IO.Path.Combine(dir, expectedName)
        if IO.File.Exists expectedLocation then Assembly.LoadFrom expectedLocation else null
        )
   
   do System.AppDomain.CurrentDomain.add_AssemblyResolve handler

   do containerType.DefineStaticParameters(
                         staticParams,
                         (fun typeName [| :? string as jarFile ; :? string as ikvmPath|] ->
                              Helpers.memoize loader (typeName, jarFile, ikvmPath)
                         ))
      this.AddNamespace(rootNamespace, [containerType]) 

   do
        let path = dir + @"\GeneratedTypes.dll"
        containerType.ConvertToGenerated(path)
    
   
    
                   
[<TypeProviderAssembly>] 
do()

