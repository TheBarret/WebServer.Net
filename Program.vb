Module Program

    Sub Main()
        Console.Title = "Webserver"
        Console.WindowWidth = 80
        Console.WindowHeight = 20
        ListenerMessage("Loading server...")
        Dim listener As Listener = Nothing
        Try
            listener = listener.Load(".\config.xml")
            AddHandler listener.StatusChange, AddressOf ListenerStatus
            AddHandler listener.ServerHeartBeat, AddressOf ListenerHeartBeat
            AddHandler listener.ExceptionCaught, AddressOf ListenerException
            AddHandler listener.ServerMessage, AddressOf ListenerMessage
            Do
                listener.Start()
            Loop Until Console.ReadKey.Key = ConsoleKey.Q
            'Catch ex As Exception
            'ListenerException(Nothing, ex)
            'Console.Read()
        Finally
            If (listener IsNot Nothing AndAlso listener.Running) Then
                listener.Shutdown()
                RemoveHandler listener.StatusChange, AddressOf ListenerStatus
                RemoveHandler listener.ServerHeartBeat, AddressOf ListenerHeartBeat
                RemoveHandler listener.ExceptionCaught, AddressOf ListenerException
                RemoveHandler listener.ServerMessage, AddressOf ListenerMessage
            End If
        End Try
    End Sub
    Private Sub ListenerStatus(Running As Boolean)
        ListenerMessage("Server ready and listening")
    End Sub
    Private Sub ListenerHeartBeat(latency As TimeSpan)
        Console.Title = String.Format("Webserver - {0}", latency.ToString)
    End Sub
    Private Sub ListenerMessage(Message As String)
        Console.WriteLine("[{0}] {1}", DateTime.Now.ToShortTimeString, Message)
    End Sub
    Private Sub ListenerException(sender As Object, ex As Exception)
        If (sender Is Nothing) Then
            ListenerMessage(String.Format("[Exception] {0}", ex.Message))
        Else
            ListenerMessage(String.Format("[Exception][{0}] {1}", sender.GetType.Name, ex.Message))
        End If
    End Sub
End Module
