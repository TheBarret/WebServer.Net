Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Threading

Public Class Client
    Public Property Parent As Listener
    Public Property Handle As ManualResetEvent
    Public Property Environment As VirtualHost
    Public Property Context As HttpListenerContext
    Sub New(Parent As Listener, Context As HttpListenerContext)
        Me.Parent = Parent
        Me.Context = Context
        Me.Handle = New ManualResetEvent(False)
        If (Not Parent.TryGetConfig(Context.Request.Url.Host, Me.Environment)) Then
            Me.SetStatus(HttpStatusCode.NotFound)
        End If
    End Sub
    Public Sub Process()
        Try
            If (Me.Validated) Then
                Me.Parent.Log(String.Format("[Request] {0} {1} {2} {3}", Me.RemoteEndPoint.ToString, Me.Host, Me.Method, Me.Context.Request.Url.AbsolutePath))
                Me.Response.KeepAlive = Me.Environment.KeepAlive
                Me.ValidateRequest(Me.GetLocalPath(Me.Context.Request.Url.AbsolutePath.Replace("/", "\")))
            End If
        Catch ex As Exception
            Me.Parent.ClientExceptionCaught(Me, ex)
            Me.SetStatus(HttpStatusCode.InternalServerError)
        Finally
            Me.Stream.Close()
            Me.Handle.Set()
            Me.Handle.Close()
        End Try
    End Sub
    Public Sub PrepaireFile(Filename As String)
        If (Not Me.IsHiddenFileType(Filename)) Then
            Using fs As New FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.None)
                Dim buffer() As Byte = New Byte(CInt(fs.Length - 1)) {}
                fs.Read(buffer, 0, buffer.Length)
                Me.SendRequest(GZip.Compress(buffer), Me.Parent.GetContentType(Path.GetExtension(Filename)), File.GetLastWriteTime(Filename))
            End Using
        Else
            Me.SetStatus(HttpStatusCode.Forbidden)
        End If
    End Sub
    Public Sub PrepaireDirectory(Dir As String)
        Me.SendRequest(GZip.Compress(New Generator(Me).DirectoryList(Dir)), "text/html", DateTime.Now)
    End Sub
    Public Sub PrepairCustom(Value As String, ContentType As String)
        Dim buffer() As Byte = Me.Environment.Encoder.GetBytes(Value)
        Me.SendRequest(buffer, ContentType, DateTime.Now)
    End Sub
    Public Sub SetStatus(StatusCode As HttpStatusCode)
        Me.Parent.Log(String.Format("[Response] {0}", StatusCode.ToString))
        Me.Response.StatusCode = StatusCode
    End Sub
    Private Sub ValidateRequest(AbsolutePath As String)
        If (Me.HasQueryVariables AndAlso (Not Me.IsValidQueryLength Or Not Me.IsValidQuerySizes)) Then
            Me.SetStatus(HttpStatusCode.MethodNotAllowed)
        Else
            If (Directory.Exists(AbsolutePath) Or File.Exists(AbsolutePath)) Then
                If (Not Me.IsIllegalPath(AbsolutePath)) Then
                    If (File.GetAttributes(AbsolutePath) = FileAttributes.Directory) Then
                        If (Me.HasIndexPage(AbsolutePath)) Then
                            Me.PrepaireFile(AbsolutePath)
                        Else
                            If (Me.Environment.AllowDirListing) Then
                                Me.PrepaireDirectory(AbsolutePath)
                            Else
                                Me.SetStatus(HttpStatusCode.Forbidden)
                            End If
                        End If
                    Else
                        Me.PrepaireFile(AbsolutePath)
                    End If
                Else
                    Me.SetStatus(HttpStatusCode.Forbidden)
                End If
            Else
                Me.SetStatus(HttpStatusCode.NotFound)
            End If
        End If
    End Sub
    Public Sub SendRequest(Buffer() As Byte, ContentType As String, LastModified As DateTime)
        Me.SetStatus(HttpStatusCode.OK)
        With Me.Context
            .Response.ContentType = ContentType
            .Response.ContentLength64 = Buffer.Length
            For Each entry As KeyValuePair(Of String, String) In Me.Environment.Headers
                .Response.AddHeader(entry.Key, entry.Value)
            Next
            .Response.AddHeader("Content-Encoding", "gzip")
            .Response.AddHeader("Keep-Alive", Me.Environment.KeepAlive.ToString)
            .Response.AddHeader("Accept-Charset", Me.Environment.Encoder.BodyName)
            .Response.AddHeader("Date", DateTime.Now.ToString("r"))
            .Response.AddHeader("Last-Modified", LastModified.ToString("r"))
            .Response.OutputStream.Write(Buffer, 0, Buffer.Length)
            .Response.OutputStream.Flush()
        End With
    End Sub
    Public Function HasIndexPage(ByRef AbsolutePath As String) As Boolean
        For Each entry As String In Directory.GetFiles(AbsolutePath)
            If (Me.Environment.DefaultIndexPages.Contains(Path.GetFileName(entry).ToLower)) Then
                AbsolutePath = entry
                Return True
            End If
        Next
        Return False
    End Function
    Public Function IsHiddenFileType(AbsolutePath As String) As Boolean
        For Each entry As String In Me.Environment.HiddenFileTypes
            If (Path.GetFileName(AbsolutePath) Like entry) Then
                Return True
            End If
        Next
        Return False
    End Function
    Public Function IsIllegalPath(AbsolutePath As String) As Boolean
        For Each entry As String In Me.Environment.IllegalPathChars
            If (AbsolutePath Like entry) Then
                Return True
            End If
        Next
        Return False
    End Function
    Public Function HasQueryVariables() As Boolean
        Return Me.Context.Request.QueryString.Count > 0
    End Function
    Public Function IsValidQueryLength() As Boolean
        Return Me.Context.Request.QueryString.Count < Me.Environment.MaxQueryLength
    End Function
    Public Function IsValidQuerySizes() As Boolean
        For Each var As String In Me.Context.Request.QueryString
            If (Me.Context.Request.QueryString.Item(var).Length > Me.Environment.MaxQuerySize) Then
                Return False
            End If
        Next
        Return True
    End Function
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
    Public ReadOnly Property Stream As Stream
        Get
            Return Me.Context.Response.OutputStream
        End Get
    End Property
    Public ReadOnly Property Validated As Boolean
        Get
            Return Me.Environment IsNot Nothing
        End Get
    End Property
    Public ReadOnly Property GetLocalPath(ParamArray Combine() As String) As String
        Get
            Return Path.GetFullPath(String.Format("{0}\{1}", Me.Environment.DocumentRoot.FullName, String.Join("\", Combine)))
        End Get
    End Property
    Public ReadOnly Property GetRelativePath(Filename As String) As String
        Get
            Dim path As String = New Uri(Filename).AbsolutePath, absolute As New DirectoryInfo(Me.GetLocalPath)
            Return path.Substring(path.IndexOf(absolute.Name) + absolute.Name.Length)
        End Get
    End Property
End Class
