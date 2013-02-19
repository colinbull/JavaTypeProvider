namespace Test

module Tests =
    open FSharpx

    let[<Literal>] jar = @"D:\Appdev\IKVM.TypeProvider\SimpleJar\out\artifacts\SimpleJar.jar"
    let[<Literal>] ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM.TypeProvider\IKVM\bin\"
    let[<Literal>] className = @"hello.HelloWorld"
    
    type SimpleJar = FSharpx.IKVMProvider<JarFile=jar, IKVMPath=ikvmPath>
    

    [<EntryPoint>]
    let main(args) =
        let hw = new SimpleJar.HelloWorld("Colin")
        hw.Say("Foo");
        hw.toString()
        System.Console.ReadLine()
        0
       