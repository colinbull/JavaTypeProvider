namespace Test

module Tests =
    open FSharpx

    [<Literal>]let jar = @"D:\Appdev\IKVM.TypeProvider\SimpleJar\out\artifacts\SimpleJar.jar"
    [<Literal>]let className = @"hello.HelloWorld"

    type SimpleJar = FSharpx.IKVM<JarFile=jar, ClassNames=className>
    
    let F = SimpleJar.HelloWorld()
    
    
    //SimpleJar.org.Echo()
    //type Echoer = SimpleJar.
       