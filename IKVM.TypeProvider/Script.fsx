// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "IKVM.fs"
open IKVM

let IKVMPath = @"D:\Appdev\IKVM.TypeProvider\IKVM.TypeProvider\IKVM\bin\"
let outputPath = @"D:\Appdev\"
let JarFile="D:\Appdev\IKVM.TypeProvider\IKVM.TypeProvider\SampleJar\stanford-parser.jar"

IKVM.Provider.getAssembly IKVMPath outputPath JarFile

// Define your library scripting code here

