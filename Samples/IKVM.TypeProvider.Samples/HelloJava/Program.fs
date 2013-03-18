open System

let [<Literal>] jarPath = @"Calculator/out/artifacts/Calculator/Calculator.jar"
let [<Literal>] ikvmPath = "../../../IKVM/bin/"

type Calc = IKVM.IKVMProvider<jarPath, ikvmPath>
type Calculator = Calc.com.example.Calculator

[<EntryPoint>]
let main argv = 
    let calculator = new Calculator()
    printfn "Result: %A" (calculator.compute("5 + 5"))
    Console.ReadLine() |> ignore
    0 // return an integer exit code
