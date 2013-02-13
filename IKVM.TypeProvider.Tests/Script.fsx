#r @"..\IKVM.TypeProvider\bin\Debug\IKVM.TypeProvider.dll"
//#r @"..\IKVM.TypeProvider\IKVM\bin\IKVM.OpenJDK.Core.dll"


[<Literal>]let jar = @"D:\Appdev\IKVM.TypeProvider\SimpleJar\out\artifacts\SimpleJar.jar"
[<Literal>]let className = @"hello.HelloWorld"

type SimpleJar = FSharpx.IKVM<JarFile=jar, ClassNames=className>
type HelloWorld = SimpleJar.HelloWorld
let F = new HelloWorld("Foo")

F.Say("Foo");
F.toString()