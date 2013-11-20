namespace JavaTypeProvider

module PDF =
    let[<Literal>] private jar = @"D:\Appdev\IKVM.TypeProvider\JavaSource\itext\itextpdf-5.4.4.jar"
    let[<Literal>] private ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM\bin\"
    
    type private Jar = Java.JavaProvider<JarFile=jar, IKVMPath=ikvmPath>
    type private iText = Jar.com.itextpdf.text
    type private PDF = Jar.com.itextpdf.text.pdf

    let createPDF (filename:string) (contents:string) = 
        let document = new iText.Document();
        PDF.PdfWriter.getInstance(document, new java.io.FileOutputStream(filename)) |> ignore
        document.``open``()
        document.add(new iText.Paragraph(contents)) |> ignore
        document.close();

module Program = 

    [<EntryPoint>]
    let main argv = 
        let fname = "D:\mypdf.pdf"
        PDF.createPDF fname "Hello, from java"
        printfn "PDF written to %s" fname
        System.Console.ReadLine() |> ignore
        0 // return an integer exit code
