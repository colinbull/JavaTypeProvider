namespace Test

module Tests =
    open FSharpx

    let[<Literal>] jar = @"D:\Appdev\IKVM.TypeProvider\SimpleJar\out\artifacts\SimpleJar.jar"
    let[<Literal>] ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM.TypeProvider\IKVM\bin\"
    let[<Literal>] className = @"hello.HelloWorld"
    
    type Jar = FSharpx.IKVMProvider<JarFile=jar, IKVMPath=ikvmPath>
    type HW = Jar.hello.HelloWorld

    [<EntryPoint>]
    let main(args) =
        let hw = new HW("Colin")
        printf "%s" (hw.Say("Foo"));
        System.Console.ReadLine()
        0
       