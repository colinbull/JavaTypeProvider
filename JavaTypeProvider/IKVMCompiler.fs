module IKVMCompiler
    
    open System
    open System.IO
    open System.Diagnostics
    open System.Reflection

    let private runProcess infoAction (timeOut:TimeSpan) errorF messageF =
        use p = new Process()
        p.StartInfo.UseShellExecute <- false
        infoAction p.StartInfo
        p.StartInfo.RedirectStandardOutput <- true
        p.StartInfo.RedirectStandardError <- true
    
        p.ErrorDataReceived.Add (fun d -> if d.Data <> null then errorF d.Data)
        p.OutputDataReceived.Add (fun d -> if d.Data <> null then messageF d.Data)
        try
            p.Start() |> ignore
        with
        | exn -> failwithf "Start of process %s failed. %s" p.StartInfo.FileName exn.Message
    
        p.BeginErrorReadLine()
        p.BeginOutputReadLine()     
      
        if timeOut = TimeSpan.MaxValue then
            p.WaitForExit()
        else
            if not <| p.WaitForExit(int timeOut.TotalMilliseconds) then
                try
                    p.Kill()
                with exn ->
                    failwithf "Process %s %s timed out." p.StartInfo.FileName p.StartInfo.Arguments
        
        p.ExitCode  

    let getAssemblyName jarFile =
        let name = Path.GetFileNameWithoutExtension(jarFile)
        name + ".dll"

    let private getIKVMArgs outputPath (jarFile : string) = 
        let outDll = Path.Combine(outputPath, getAssemblyName jarFile)
        outDll, sprintf "-target:library -out:%s %s" outDll jarFile
    
    let private logError errs msg =
        errs := msg :: (!errs)
        System.Diagnostics.Debug.WriteLine("IKVMTypeProvider Error: " + msg) 

    let private logMsg msg =
        System.Diagnostics.Debug.WriteLine("IKVMTypeProvider: " + msg)

    let private IKVM ikvmPath args errorF msgF =
         runProcess (fun si -> 
                       si.WorkingDirectory <- ikvmPath
                       si.FileName <- ikvmPath + "ikvmc.exe"
                       si.Arguments <- args
                       si.CreateNoWindow <- true
                    ) TimeSpan.MaxValue errorF msgF

    let private runIKVM ikvmPath outputPath jarFile =
        let outDll, args = getIKVMArgs outputPath jarFile
        let errors = ref []
        let exitCode = IKVM ikvmPath args (logError errors) logMsg
        if exitCode = 0 
        then 
            let bytes = File.ReadAllBytes(outDll)
            File.Delete(outDll)
            bytes
        else failwithf "IVKMC ended with non-zero exitcode (Code: %d)\r\n%s" exitCode (String.Join("\r\n", !errors |> List.rev))
    
    let compile ikvmPath outputPath jarFile =
        if not <| String.IsNullOrEmpty(jarFile)
        then  runIKVM ikvmPath outputPath jarFile
        else failwith "A jar/class file path must be given (wildcards accepted)"
        