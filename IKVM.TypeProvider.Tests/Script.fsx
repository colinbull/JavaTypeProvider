#r @"..\IKVM.TypeProvider\bin\Debug\IKVM.TypeProvider.dll"
#r @"..\IKVM.TypeProvider\IKVM\bin\IKVM.OpenJDK.Core.dll"


let[<Literal>] jar = @"D:\Appdev\IKVM.TypeProvider\SimpleJar\out\artifacts\SimpleJar.jar"
let[<Literal>] ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM.TypeProvider\IKVM\bin\"
let[<Literal>] className = @"hello.HelloWorld"

type SimpleJar = FSharpx.IKVMProvider<JarFile=jar, IKVMPath=ikvmPath>
let F = new SimpleJar.HelloWorld("Colin")

F.Say("Foo");
F.toString()