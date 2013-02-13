namespace FSharpx

open System
open System.Reflection
open System.Collections.Generic
open System.Diagnostics
open Microsoft.FSharp.Core.CompilerServices
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Quotations
open java.net
open java.lang

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
   let staticParams = 
        [
            ProvidedStaticParameter("JarFile", typeof<string>)
            ProvidedStaticParameter("ClassNames", typeof<string>)
        ]
   let containerType = ProvidedTypeDefinition(thisAssembly, rootNamespace, "IKVM", Some(baseType))
   
   let walkType (targetType : ProvidedTypeDefinition) (t : Type) =      
       let getParameters (m : MethodBase) =
           m.GetParameters() 
           |> Seq.map (fun pi -> ProvidedParameter(pi.Name, pi.ParameterType, pi.IsOut, pi.IsOptional))
           |> Seq.toList
   
       let getMembers (mi : MemberInfo) =
           match mi.MemberType with
           | MemberTypes.Method -> 
               let m = mi :?> MethodInfo 
               ProvidedMethod(methodName = m.Name,
                              parameters = getParameters(m), 
                              returnType = m.ReturnType,
                              InvokeCode = (fun args -> Expr.Call(args.Head, m, args.Tail))
                              ) :> MemberInfo
           | MemberTypes.Constructor ->
               let c = mi :?> ConstructorInfo
               ProvidedConstructor(getParameters c, 
                                   InvokeCode = (fun args -> Expr.NewObject(c, args))
                                   ) :> MemberInfo
           
   
       let getMembers (t : Type) = 
           t.GetMembers() |> Seq.map getMembers |> Seq.toList

       targetType.AddMembersDelayed(fun () -> getMembers t)
       targetType

   do containerType.DefineStaticParameters(
                         staticParams,
                         (fun typeName [| :? string as jarFile ; :? string as classNames|] ->
                              let urls = [|new java.net.URL("file:" + jarFile)|]
                              let loader = new java.net.URLClassLoader(urls);
                              let cl = Class.forName(classNames, true, loader)
                              let instanceType = ikvm.runtime.Util.getInstanceTypeFromClass(cl);
                              let  t= ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(instanceType))
                              walkType t instanceType
                              
                          ))

   let handler = System.ResolveEventHandler(fun _ args ->
            let asmName = AssemblyName(args.Name)
            // assuming that we reference only dll files
            let expectedName = asmName.Name + ".dll"
            let expectedLocation =
                // we expect to find this assembly near the dll with type provider
                let d = IO.Path.GetDirectoryName(config.RuntimeAssembly)
                IO.Path.Combine(d, expectedName)
            if IO.File.Exists expectedLocation then Assembly.LoadFrom expectedLocation else null
            )

   do System.AppDomain.CurrentDomain.add_AssemblyResolve handler

   do this.AddNamespace(rootNamespace, [containerType]) 
    
                   
[<TypeProviderAssembly>] 
do()

