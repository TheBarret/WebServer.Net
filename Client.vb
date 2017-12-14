Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Globalization
Imports System.Collections.Specialized
Imports Webserver.Services
Imports Webserver.Plugins

Public Class Client
    Public Property Listener As Listener
    Public Property Handle As ManualResetEvent
    Public Property Config As VirtualHost
    Public Property Context As HttpListenerContext
    Public Property Data As Dictionary(Of String, String)
    Sub New(Listener As Listener, Context As HttpListenerContext)
        Me.Listener = Listener
        Me.Context = Context
        Me.Handle = New ManualResetEvent(False)
        Me.Data = New Dictionary(Of String, String)
        If (Not Listener.TryGetConfig(Context.Request.Url.Host, Me.Config)) Then
            Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.NotFound)), "text/html", False)
        End If
    End Sub
    Public Sub Process()
        Try
            If (Me.Validated) Then
                Dim Claimed As Boolean = False
                Me.Listener.Log(String.Format("[Request] {0} {1} {2} {3}", Me.RemoteEndPoint.ToString, Me.Host, Me.Method, Me.Context.Request.Url.AbsolutePath))
                Me.Response.KeepAlive = Me.Config.KeepAlive
                Me.StoreUserData()
                Me.Listener.PluginEventRequest(Me, Claimed)
                If (Not Claimed) Then
                    Me.ValidateRequest(Me.LocalPath(Me.Context.Request.Url.AbsolutePath.Replace("/", "\")))
                End If
            End If
        Catch ex As Exception
            Me.Listener.ClientExceptionCaught(Me, ex)
            Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.InternalServerError)), "text/html", False)
        Finally
            Me.OutputStream.Close()
            Me.InputStream.Close()
            Me.Handle.Set()
            Me.Handle.Close()
        End Try
    End Sub
    Public Sub PrepaireFile(Filename As String)
        If (Not Me.IsHiddenFileType(Filename)) Then
            Dim buffer() As Byte = Nothing
            Using fs As New FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.None)
                buffer = New Byte(CInt(fs.Length - 1)) {}
                fs.Read(buffer, 0, buffer.Length)
            End Using
            Me.SendRequest(buffer, Me.Listener.GetContentType(Path.GetExtension(Filename)), File.GetLastWriteTime(Filename))
        Else
            Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.Forbidden)), "text/html", False)
        End If
    End Sub
    Public Sub PrepaireDirectory(dir As String)
        Me.SendRequest(Me.DirectoryPage(dir), "text/html", DateTime.Now)
    End Sub
    Public Sub PrepairCustom(Value As Byte(), ContentType As String, SendOk As Boolean)
        Me.SendRequest(Value, ContentType, DateTime.Now, SendOk)
    End Sub
    Public Sub PrepairCustom(Value As String, ContentType As String, SendOk As Boolean)
        Me.SendRequest(Me.Config.Encoder.GetBytes(Value), ContentType, DateTime.Now, SendOk)
    End Sub
    Public Function SetStatus(StatusCode As HttpStatusCode) As HttpStatusCode
        Me.Listener.Log(String.Format("[Response] {0}", StatusCode.ToString))
        Me.Response.StatusCode = StatusCode
        Return StatusCode
    End Function
    Private Sub StoreUserData()
        If (Me.HasGetData) Then
            If (Not Me.GetData(DataType.VARGET, Me.Data)) Then
                Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.MethodNotAllowed)), "text/html", False)
                Return
            End If
        End If
        If (Me.HasPostData) Then
            If (Not Me.GetData(DataType.VARPOST, Me.Data)) Then
                Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.MethodNotAllowed)), "text/html", False)
                Return
            End If
        End If
    End Sub
    Private Sub ValidateRequest(AbsolutePath As String)
        If (Directory.Exists(AbsolutePath) Or File.Exists(AbsolutePath)) Then
            If (Not Me.IsIllegalPath(AbsolutePath)) Then
                If (File.GetAttributes(AbsolutePath) = FileAttributes.Directory) Then
                    If (Me.HasIndexPage(AbsolutePath)) Then
                        Me.PrepaireFile(AbsolutePath)
                    Else
                        If (Me.Config.AllowDirListing) Then
                            Me.PrepaireDirectory(AbsolutePath)
                        Else
                            Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.Forbidden)), "text/html", False)
                        End If
                    End If
                Else
                    Me.PrepaireFile(AbsolutePath)
                End If
            Else
                Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.Forbidden)), "text/html", False)
            End If
        Else
            Me.PrepairCustom(Me.ErrorPage(Me.SetStatus(HttpStatusCode.NotFound)), "text/html", False)
        End If
    End Sub
    Public Sub SendRequest(Buffer() As Byte, ContentType As String, LastModified As DateTime, Optional HttpStatusCodeOk As Boolean = True)
        '// Set responds if true
        If (HttpStatusCodeOk) Then
            Me.SetStatus(HttpStatusCode.OK)
        End If
        '// Run plugins events
        Me.Listener.PluginEventSend(Me, Buffer, ContentType)
        '// Compress buffer
        Buffer = GZip.Compress(Buffer)
        '// Content Negotiation
        Me.Response.AddHeader("Accept", "*/*")
        Me.Response.AddHeader("Accept-Charset", Me.Encoding.BodyName)
        Me.Response.AddHeader("Accept-Language", Me.Culture.Name)
        '// Message Body Information
        Me.Response.ContentType = ContentType
        Me.Response.ContentLength64 = Buffer.Length
        Me.Response.ContentEncoding = Me.Encoding
        Me.Response.AddHeader("Content-Encoding", "gzip")
        '// Caching
        Me.Response.AddHeader("Pragma", "no-cache")
        Me.Response.AddHeader("Cache-Control", "no-cache")
        '// Connection Management
        Me.Response.AddHeader("connection", Me.Connection)
        '// Conditionals
        Me.Response.AddHeader("Last-Modified", LastModified.ToString("r"))
        '// Others
        Me.Response.AddHeader("Date", DateTime.Now.ToString("r"))
        Me.Response.AddHeader("CRC32", New CRC32(Buffer).ComputeHashToString)
        '// User Defined
        For Each entry As KeyValuePair(Of String, String) In Me.Config.Headers
            Me.Response.AddHeader(entry.Key, entry.Value)
        Next
        '// Send response
        Me.Response.SendChunked = False
        Me.Response.OutputStream.Write(Buffer, 0, Buffer.Length)
        Me.Response.OutputStream.Flush()
    End Sub
    Public Function HasIndexPage(ByRef AbsolutePath As String) As Boolean
        For Each entry As String In Directory.GetFiles(AbsolutePath)
            If (Me.Config.DefaultIndexPages.Contains(Path.GetFileName(entry).ToLower)) Then
                AbsolutePath = entry
                Return True
            End If
        Next
        Return False
    End Function
    Public Function IsHiddenFileType(AbsolutePath As String) As Boolean
        For Each entry As String In Me.Config.HiddenFileTypes
            If (Path.GetFileName(AbsolutePath) Like entry) Then
                Return True
            End If
        Next
        Return False
    End Function
    Public Function IsIllegalPath(AbsolutePath As String) As Boolean
        For Each entry As String In Me.Config.IllegalPathChars
            If (AbsolutePath Like entry) Then
                Return True
            End If
        Next
        Return False
    End Function
    Public Function HasPostData() As Boolean
        Return Me.Method.ToLower.Equals("post") AndAlso Me.InputStream.Length > 0
    End Function
    Public Function HasGetData() As Boolean
        Return Me.Method.ToLower.Equals("get") AndAlso Me.Context.Request.QueryString.Count > 0
    End Function
    Public Function GetData(Type As DataType, Collection As Dictionary(Of String, String)) As Boolean
        If (Type = DataType.VARGET) Then
            Dim count As Integer = 0
            If (Me.Context.Request.QueryString.Count > 0) Then
                For Each pair As KeyValuePair(Of String, String) In Me.Query.ToDictionary(Me.Request)
                    If (count > Me.Config.MaxQueryLength) Then
                        Return False
                    ElseIf (pair.Value.Length > Me.Config.MaxQuerySize) Then
                        Return False
                    End If
                    Collection.Add(pair.Key, pair.Value)
                    count += 1
                Next
            End If
        ElseIf (Type = DataType.VARPOST) Then
            Dim count As Integer = 0
            For Each pair As KeyValuePair(Of String, String) In Me.InputStream.ToDictionary
                If (count > Me.Config.MaxQueryLength) Then
                    Return False
                ElseIf (pair.Value.Length > Me.Config.MaxQuerySize) Then
                    Return False
                End If
                Collection.Add(pair.Key, pair.Value)
                count += 1
            Next
        End If
        Return True
    End Function
    Public ReadOnly Property Encoding As Encoding
        Get
            Return Me.Config.Encoder
        End Get
    End Property
    Public ReadOnly Property Culture As CultureInfo
        Get
            Return Me.Config.Culture
        End Get
    End Property
    Public ReadOnly Property UserAgent As String
        Get
            Return Me.Context.Request.UserAgent
        End Get
    End Property
    Public ReadOnly Property Host As String
        Get
            Return Me.Context.Request.Url.Host
        End Get
    End Property
    Public ReadOnly Property RemoteEndPoint As IPEndPoint
        Get
            Return Me.Context.Request.RemoteEndPoint
        End Get
    End Property
    Public ReadOnly Property RemoteAddress As String
        Get
            Return Me.RemoteEndPoint.Address.ToString
        End Get
    End Property
    Public ReadOnly Property Method As String
        Get
            Return Me.Context.Request.HttpMethod
        End Get
    End Property
    Public ReadOnly Property ProtocolVersion As String
        Get
            Return Me.Context.Request.ProtocolVersion.ToString
        End Get
    End Property
    Public ReadOnly Property Response As HttpListenerResponse
        Get
            Return Me.Context.Response
        End Get
    End Property
    Public ReadOnly Property Request As HttpListenerRequest
        Get
            Return Me.Context.Request
        End Get
    End Property
    Public ReadOnly Property Query As NameValueCollection
        Get
            Return Me.Context.Request.QueryString
        End Get
    End Property
    Public ReadOnly Property OutputStream As Stream
        Get
            Return Me.Context.Response.OutputStream
        End Get
    End Property
    Public ReadOnly Property InputStream As Stream
        Get
            Return Me.Context.Request.InputStream
        End Get
    End Property
    Public ReadOnly Property Validated As Boolean
        Get
            Return Me.Config IsNot Nothing
        End Get
    End Property
    Public ReadOnly Property Connection As String
        Get
            Return If(Me.Config.KeepAlive, "keep-alive", "close").ToString
        End Get
    End Property
    Public ReadOnly Property ErrorPage(StatusCode As HttpStatusCode) As Byte()
        Get
            Return New Generator(Me).ErrorPage(StatusCode)
        End Get
    End Property
    Public ReadOnly Property DirectoryPage(dir As String) As Byte()
        Get
            Return New Generator(Me).DirectoryPage(dir)
        End Get
    End Property
    Public ReadOnly Property LocalPath(ParamArray Combine() As String) As String
        Get
            Return Path.GetFullPath(String.Format("{0}\{1}", Me.Config.DocumentRoot.FullName, String.Join("\", Combine)))
        End Get
    End Property
    Public ReadOnly Property RelativePath(Filename As String) As String
        Get
            Dim path As String = New Uri(Filename).AbsolutePath, absolute As New DirectoryInfo(Me.LocalPath)
            Return path.Substring(path.IndexOf(absolute.Name) + absolute.Name.Length)
        End Get
    End Property
End Class
