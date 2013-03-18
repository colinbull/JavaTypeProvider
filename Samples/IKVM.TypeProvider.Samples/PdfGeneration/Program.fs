// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System

let [<Literal>] jarPath = "itext-5.4.0/itextpdf-5.4.0.jar"
let [<Literal>] ikvmPath = "../../../IKVM/bin/"

type IText = IKVM.IKVMProvider<jarPath, ikvmPath>
type PDF = IText.com.itextpdf.text
type Document = PDF.Document

[<EntryPoint>]
let main argv = 
    let doc =  new Document()
    printfn "Created PDF"
    Console.ReadLine() |> ignore
    0 // return an integer exit code
    