namespace FSharpx

open System
open System.Reflection
open System.Reflection.Emit
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
   
   let walkType (t : Type) =   
       let ty = ProvidedTypeDefinition(t.Name, Some(baseType))
       
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
       ty

   let loader (typeName, jarFile ,classNames) =
        let rtAss = Assembly.LoadFrom(config.RuntimeAssembly)
        ikvm.runtime.Startup.addBootClassPathAssemby(rtAss)
        let urls = [|new java.net.URL("file:" + jarFile)|]
        let loader = new java.net.URLClassLoader(urls);
        let cl = Class.forName(classNames, true, loader)
        let instanceType = ikvm.runtime.Util.getInstanceTypeFromClass(cl);
        let t = ProvidedTypeDefinition(thisAssembly, rootNamespace, typeName, Some(baseType))
        let toNest = walkType instanceType
        t.AddMemberDelayed(fun _ -> toNest)
        t

   do containerType.DefineStaticParameters(
                         staticParams,
                         (fun typeName [| :? string as jarFile ; :? string as classNames|] ->
                              Helpers.memoize loader (typeName, jarFile, classNames)
                         ))

   do System.AppDomain.CurrentDomain.add_AssemblyResolve(fun _ args ->
        let name = System.Reflection.AssemblyName(args.Name)
        let existingAssembly = 
            System.AppDomain.CurrentDomain.GetAssemblies()
            |> Seq.tryFind(fun a -> System.Reflection.AssemblyName.ReferenceMatchesDefinition(name, a.GetName()))
        match existingAssembly with
        | Some a -> a
        | None -> null
        )


   do this.AddNamespace(rootNamespace, [containerType]) 
    
                   
[<TypeProviderAssembly>] 
do()

