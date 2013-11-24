namespace JavaTypeProvider

module Tests =
    let[<Literal>] jar = @"D:\Appdev\IKVM.TypeProvider\JavaSource\netty-4.0.12.Final\jar\all-in-one\netty-all-4.0.12.Final.jar"
    let[<Literal>] ikvmPath = @"D:\Appdev\IKVM.TypeProvider\IKVM\bin\"
    
    type Jar = Java.JavaProvider<JarFile=jar, IKVMPath=ikvmPath>
    type Netty = Jar.io.netty
    type NettyCh = Jar.io.netty.channel

    type DiscardServerHandler() = 
        inherit NettyCh.ChannelInboundHandlerAdapter()

        override x.channelRead(ctx, msg:obj) =
            ctx.writeAndFlush(msg)|> ignore
            
        override x.exceptionCaught(ctx, exn) =
            printfn "Error: %s" exn.StackTrace
            ctx.close() |> ignore

    type EchoServer(port:int) = 
        member x.Run() = 
            let bossGroup = new NettyCh.nio.NioEventLoopGroup()
            let workerGroup = new NettyCh.nio.NioEventLoopGroup()
            try
               let b = Netty.bootstrap.ServerBootstrap()
               let future = 
                  b.group(bossGroup, workerGroup)
                   .childHandler(new DiscardServerHandler())
                   //.childOption(NettyCh.ChannelOption.SO_KEEPALIVE, true)
                   .channel(typeof<NettyCh.socket.nio.NioServerSocketChannel> |> java.lang.Class.op_Implicit)
                 //  .option(NettyCh.ChannelOption.SO_BACKLOG, 128)
                   .bind(port)
                   .sync()

               printfn "Server listening"
               System.Console.ReadLine() |> ignore

               future.channel().closeFuture().sync() |> ignore

            with e -> 
                printfn "Server errored %A" e
                System.Console.ReadLine() |> ignore
                bossGroup.shutdownGracefully() |> ignore
                workerGroup.shutdownGracefully() |> ignore

    [<EntryPoint>]
    let main(args) =
        printfn "Starting up netty DISCARD server"

        let server = new EchoServer(8080)
        server.Run()

        0
       