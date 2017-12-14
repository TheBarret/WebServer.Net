Imports System.Net
Imports System.Threading
Imports Webserver.Plugins

Public Class Listener
    Inherits Config
    Public Property Running As Boolean
    Public Property Handle As ManualResetEvent
    Public Property Timer As Stopwatch
    Public Property Queue As Queue(Of Client)
    Public Property HttpListener As HttpListener
    Public Event ServerHeartBeat(latency As TimeSpan)
    Public Event ServerMessage(Message As String)
    Public Event StatusChange(Running As Boolean)
    Public Event ExceptionCaught(sender As Object, ex As Exception)
    Sub New()
        Me.Queue = New Queue(Of Client)
    End Sub
    Public Sub Start()
        ThreadPool.SetMaxThreads(Me.Workers, Me.IOWorkers)
        Call New Thread(AddressOf Me.Worker) With {.IsBackground = True}.Start()
    End Sub
    Public Sub Shutdown()
        Me.Running = False
    End Sub
    Public Function TryGetConfig(Address As String, ByRef VirtualHost As VirtualHost) As Boolean
        VirtualHost = Me.VirtualHosts.Where(Function(vhost) vhost.Name.Equals(Address.ToLower) Or Address Like vhost.Name).SingleOrDefault
        Return VirtualHost IsNot Nothing
    End Function
    Public Sub Log(Message As String)
        RaiseEvent ServerMessage(Message)
    End Sub
    Public Sub ClientExceptionCaught(sender As Object, ex As Exception)
        RaiseEvent ExceptionCaught(sender, ex)
    End Sub
    Public Sub PluginEventInitialize()
        For Each plugin As IPlugin In Me.Plugins
            plugin.Load()
        Next
    End Sub
    Public Sub PluginEventRequest(client As Client, ByRef Claimed As Boolean)
        For Each plugin As IPlugin In Me.Plugins
            plugin.ClientRequest(client, Claimed)
        Next
    End Sub
    Public Sub PluginEventSend(client As Client, ByRef buffer() As Byte, ByRef ContentType As String)
        For Each plugin As IPlugin In Me.Plugins
            plugin.ClientSend(client, buffer, ContentType)
        Next
    End Sub
    Private Sub Worker()
        If (Me.Running) Then
            Me.Running = False
            Me.Handle.WaitOne()
        End If
        Me.Running = True
        Me.Timer = New Stopwatch
        Me.Handle = New ManualResetEvent(False)
        Me.HttpListener = Listener.Create(Me.VirtualHosts)
        Me.HttpListener.Start()
        Me.HttpListener.BeginGetContext(AddressOf Me.ProcessIncomingRequest, Me.HttpListener)
        Me.PluginEventInitialize()
        RaiseEvent StatusChange(True)
        Try
            Do
                Me.Timer.Start()
                If (Me.Queue.Any) Then
                    ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf Me.BeginClientRequest), Me.Queue.Dequeue)
                End If
                Thread.Sleep(Me.Delay)
                Me.Timer.Reset()
                RaiseEvent ServerHeartBeat(Me.Timer.Elapsed)
            Loop While Me.Running
            Me.Timer.Stop()
        Catch ex As Exception
            Me.Running = False
            RaiseEvent ExceptionCaught(Me, ex)
        Finally
            Me.WaitForClients()
            Me.HttpListener.Close()
            Me.Handle.Set()
            Me.Handle.Close()
            RaiseEvent StatusChange(False)
        End Try
    End Sub
    Private Sub ProcessIncomingRequest(Result As IAsyncResult)
        Try
            If (TypeOf Result.AsyncState Is HttpListener) Then
                If (Me.Running) Then
                    If (Not Result.IsCompleted) Then
                        Result.AsyncWaitHandle.WaitOne()
                    End If
                    Me.Queue.Enqueue(New Client(Me, CType(Result.AsyncState, HttpListener).EndGetContext(Result)))
                End If
            Else
                Throw New Exception("unable to handle request")
            End If
        Finally
            Me.HttpListener.BeginGetContext(AddressOf Me.ProcessIncomingRequest, Me.HttpListener)
        End Try
    End Sub
    Private Sub BeginClientRequest(sender As Object)
        Try
            If (TypeOf sender Is Client) Then
                CType(sender, Client).Process()
            End If
        Catch ex As Exception
            RaiseEvent ExceptionCaught(sender, ex)
        End Try
    End Sub
    Private Sub WaitForClients()
        If (Me.Queue.Any) Then
            WaitHandle.WaitAll(Me.Queue.Select(Function(client) client.Handle).ToArray)
        End If
    End Sub
End Class
