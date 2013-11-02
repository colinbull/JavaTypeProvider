namespace TestComposite

//Here I get the types. But you will see when you use the intellisense completion.

module Tests =
    let[<Literal>] jar = @"D:\Appdev\IKVM.TypeProvider\JavaSource\Calculator\out\artifacts\Calculator\Calculator.jar"
    let[<Literal>] ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM\bin\"
//    
//    type Jar = Java.JavaProvider<JarFile=jar, IKVMPath=ikvmPath>
//    type Calaculator = Jar.com.example.Calculator
//
//    [<EntryPoint>]
//    let main(args) =
//        let calc = new Calaculator()
//        printfn "%s" (calc.compute())
//        System.Console.ReadLine() |> ignore
//        0

