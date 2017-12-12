Module Program

    Sub Main()
        Console.Title = "Webserver"
        Console.WindowWidth = 60
        Console.WindowHeight = 20
        Console.WriteLine("Loading server...")
        Dim listener As Listener = listener.Load(".\config.xml")
        AddHandler listener.StatusChange, AddressOf ListenerStatus
        AddHandler listener.ServerHeartBeat, AddressOf ListenerHeartBeat
        AddHandler listener.ExceptionCaught, AddressOf ListenerException
        AddHandler listener.ServerMessage, AddressOf ListenerMessage
        Try
            For Each vhost In listener.VirtualHosts
                Console.WriteLine("-> Found: {0}", vhost.Prefix)
            Next
            Do
                listener.Start()
            Loop Until Console.ReadKey.Key = ConsoleKey.Q
        Catch ex As Exception
            ListenerException(Nothing, ex)
        Finally
            listener.Shutdown()
            RemoveHandler listener.StatusChange, AddressOf ListenerStatus
            RemoveHandler listener.ServerHeartBeat, AddressOf ListenerHeartBeat
            RemoveHandler listener.ExceptionCaught, AddressOf ListenerException
            RemoveHandler listener.ServerMessage, AddressOf ListenerMessage
        End Try
    End Sub
    Private Sub ListenerStatus(Running As Boolean)
        Console.WriteLine("Server ready and listening")
    End Sub
    Private Sub ListenerHeartBeat(time As TimeSpan)
        Console.Title = String.Format("Webserver - {0}", time.ToString)
    End Sub
    Private Sub ListenerMessage(Message As String)
        Console.WriteLine(Message)
    End Sub
    Private Sub ListenerException(sender As Object, ex As Exception)
        If (sender Is Nothing) Then
            Console.WriteLine(String.Format("Exception caught: {0}", ex.Message))
        Else
            Console.WriteLine(String.Format("Exception caught: {0} {1}", sender.GetType.Name, ex.Message))
        End If
    End Sub
End Module
