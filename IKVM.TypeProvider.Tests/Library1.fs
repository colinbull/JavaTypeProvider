namespace Test

module Tests =
    open FSharpx

    [<Literal>]let jar = @"D:\Appdev\SimpleJar\out\production\SimpleJar\org\Echo.class"
    [<Literal>]let ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM.TypeProvider\IKVM\bin\"

    type SimpleJar = FSharpx.IKVM<JarFile=jar, IKVMPath=ikvmPath>
    
    let F = SimpleJar.org.Echo()
    //SimpleJar.org.Echo()
    //type Echoer = SimpleJar.
       