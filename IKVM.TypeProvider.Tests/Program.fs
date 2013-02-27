namespace Test

module Tests =

    let[<Literal>] jar = @"D:\Appdev\IKVM.TypeProvider\IKVM.TypeProvider.Tests\SimpleJar\out\artifacts\SimpleJar.jar"
    let[<Literal>] ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM\bin\"
    
    type Jar = IKVM.IKVMProvider<JarFile=jar, IKVMPath=ikvmPath>
    type HW = Jar.hello.HelloWorld

    [<EntryPoint>]
    let main(args) =
        let hw = new HW("IKVM: ")
        printfn "%s" (hw.Say("Hello from java, well .NET"))
        printfn "%s" (hw.Close())
        System.Console.ReadLine() |> ignore
        0
       