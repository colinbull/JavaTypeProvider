namespace JavaTypeProvider

open System
open System.IO

module Neo4j =

    let[<Literal>] private jar = @"D:\Appdev\IKVM.TypeProvider\JavaSource\neo4j-desktop-2.0.0-RC1.jar"
    let[<Literal>] private ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM\bin\"

    type Jar = Java.JavaProvider<JarFile=jar, IKVMPath=ikvmPath>
    type Neo4J = Jar.org.neo4j.graphdb
    type Neo4JFactory = Jar.org.neo4j.graphdb.factory

    let ensureDirectory path = 
        if Directory.Exists(path)
        then path
        else Directory.CreateDirectory(path).FullName

    let createDb path = 
        (new Neo4JFactory.GraphDatabaseFactory()).newEmbeddedDatabase(ensureDirectory path)

    let transact f (graphdb:Neo4J.GraphDatabaseService) = 
        let tx = graphdb.beginTx()
        try
            f graphdb
            tx.success()
        with e -> 
            tx.failure()

module Program = 
   
    let[<Literal>] private DB_PATH = @"example_db"

    type Relations =
         | KNOWS
         with
            interface Neo4j.Neo4J.RelationshipType with
                member x.name() = 
                    match x with
                    | KNOWS -> "KNOWS"

    let graphDB = Neo4j.createDb DB_PATH

    [<EntryPoint>]
    let main argv = 
        
        Neo4j.transact (fun gdb -> 
            let fn = gdb.createNode()
            fn.setProperty("message", "Hello, ")
            let sn = gdb.createNode()
            sn.setProperty("message", "World!")

            let rel = fn.createRelationshipTo(sn, Relations.KNOWS)
            rel.setProperty("message", "breave Neo4j ")
            
            printfn "%A" (fn.getProperty("message"))
            printfn "%A" (rel.getProperty("message"))
            printfn "%A" (sn.getProperty("message"))
        ) graphDB
        
        printfn "Done!!!!"
        System.Console.ReadLine() |> ignore
        0 // return an integer exit code
