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
    ''' <summary>
    ''' Starts server
    ''' </summary>
    Public Sub Start()
        ThreadPool.SetMaxThreads(Me.Workers, Me.IOWorkers)
        Call New Thread(AddressOf Me.Worker) With {.IsBackground = True}.Start()
    End Sub
    ''' <summary>
    ''' Shutsdown server
    ''' </summary>
    Public Sub Shutdown()
        Me.Running = False
    End Sub
    ''' <summary>
    ''' Tries to get the config settings for a requested hostname
    ''' </summary>
    Public Function TryGetConfig(Address As String, ByRef VirtualHost As VirtualHost) As Boolean
        VirtualHost = Me.VirtualHosts.Where(Function(Settings) Settings.Name.Equals(Address.ToLower) Or Address Like Settings.Name).SingleOrDefault
        Return VirtualHost IsNot Nothing
    End Function
    ''' <summary>
    ''' Raises event for messages
    ''' </summary>
    Public Sub Log(Message As String)
        RaiseEvent ServerMessage(Message)
    End Sub
    ''' <summary>
    ''' Raises event if a client throws an exception
    ''' </summary>
    Public Sub ClientExceptionEvent(sender As Object, ex As Exception)
        RaiseEvent ExceptionCaught(sender, ex)
    End Sub
    ''' <summary>
    ''' Plugin Event: Initialize
    ''' </summary>
    Public Sub PluginEventInitialize()
        For Each plugin As IPlugin In Me.Plugins
            plugin.Load(Me)
        Next
    End Sub
    ''' <summary>
    ''' Plugin Event: Before Request
    ''' </summary>
    Public Sub PluginEventRequest(client As Client, ByRef Claimed As Boolean)
        For Each plugin As IPlugin In Me.Plugins
            plugin.ClientRequest(client, Claimed)
        Next
    End Sub
    ''' <summary>
    ''' Plugin Event: Before Send
    ''' </summary>
    Public Sub PluginEventSend(client As Client, ByRef buffer() As Byte, ByRef ContentType As String)
        For Each plugin As IPlugin In Me.Plugins
            plugin.ClientSend(client, buffer, ContentType)
        Next
    End Sub
    ''' <summary>
    ''' Listener main worker method
    ''' </summary>
    Private Sub Worker()
        If (Me.Running) Then
            Me.Running = False
            Me.Handle.WaitOne()
        End If
        Me.Running = True
        Me.Timer = New Stopwatch
        Me.Handle = New ManualResetEvent(False)
        Me.HttpListener = Listener.Create(Me.VirtualHosts)
        Me.HttpListenerSettings()
        Me.HttpListenerPlugins()
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
    ''' <summary>
    ''' Setups the listener settings
    ''' </summary>
    Private Sub HttpListenerSettings()
        With Me.HttpListener.TimeoutManager
            .DrainEntityBody = New TimeSpan(0, 0, Me.TimeOutDrainEntityBody)
            .EntityBody = New TimeSpan(0, 0, Me.TimeOutDrainEntityBody)
            .HeaderWait = New TimeSpan(0, 0, Me.TimeOutHeaderWait)
            .IdleConnection = New TimeSpan(0, 0, Me.TimeOutIdleConnection)
            .RequestQueue = New TimeSpan(0, 0, Me.TimeOutRequestPickup)
            .MinSendBytesPerSecond = Me.MinSendBytesPerSecond
        End With
    End Sub
    ''' <summary>
    ''' Starts the process of an incoming request
    ''' </summary>
    Private Sub ProcessIncomingRequest(Result As IAsyncResult)
        Try
            If (TypeOf Result.AsyncState Is HttpListener) Then
                If (Me.Running) Then
                    If (Not Result.IsCompleted) Then
                        Result.AsyncWaitHandle.WaitOne()
                    End If
                    If (Me.Queue.Count <= Me.MaxWaitQueue) Then
                        Me.Queue.Enqueue(New Client(Me, CType(Result.AsyncState, HttpListener).EndGetContext(Result)))
                    Else
                        Me.Log("[Warning] Request dropped, queue is currently full.")
                        Dim context As HttpListenerContext = CType(Result.AsyncState, HttpListener).EndGetContext(Result)
                        context.Response.StatusCode = HttpStatusCode.ServiceUnavailable
                        context.Response.OutputStream.Close()
                        context.Response.Close()
                    End If
                End If
            Else
                Throw New Exception("unable to handle request")
            End If
        Finally
            If (Me.Running) Then
                Me.HttpListener.BeginGetContext(AddressOf Me.ProcessIncomingRequest, Me.HttpListener)
            End If
        End Try
    End Sub
    ''' <summary>
    ''' Starts client process
    ''' </summary>
    Private Sub BeginClientRequest(sender As Object)
        If (TypeOf sender Is Client) Then
            CType(sender, Client).Process()
        End If
    End Sub
    ''' <summary>
    ''' Blocks current thread while waiting for clients to finish
    ''' </summary>
    Private Sub WaitForClients()
        If (Me.Queue.Any) Then
            WaitHandle.WaitAll(Me.Queue.Select(Function(client) client.Handle).ToArray)
        End If
    End Sub
    ''' <summary>
    ''' Loads plugin assemblies
    ''' </summary>
    Private Sub HttpListenerPlugins()
        For Each plugin As IPlugin In Me.Plugins
            Me.Log(String.Format("+ Plugin '{0}' loaded", plugin.Name))
        Next
    End Sub
End Class
