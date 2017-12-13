Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Collections.Specialized

Public Class Client
    Inherits Dictionary(Of String, String)
    Public Property Parent As Listener
    Public Property Handle As ManualResetEvent
    Public Property Config As VirtualHost
    Public Property Context As HttpListenerContext
    Sub New(Parent As Listener, Context As HttpListenerContext)
        Me.Parent = Parent
        Me.Context = Context
        Me.Handle = New ManualResetEvent(False)
        If (Not Parent.TryGetConfig(Context.Request.Url.Host, Me.Config)) Then
            Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.NotFound)), "text/html", False)
        End If
    End Sub
    Public Sub Process()
        Try
            If (Me.Validated) Then
                Me.Parent.Log(String.Format("[Request] {0} {1} {2} {3}", Me.RemoteEndPoint.ToString, Me.Host, Me.Method, Me.Context.Request.Url.AbsolutePath))
                Me.Response.KeepAlive = Me.Config.KeepAlive
                Me.ValidateRequest(Me.GetLocalPath(Me.Context.Request.Url.AbsolutePath.Replace("/", "\")))
            End If
        Catch ex As Exception
            Me.Parent.ClientExceptionCaught(Me, ex)
            Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.InternalServerError)), "text/html", False)
        Finally
            Me.OutputStream.Close()
            Me.InputStream.Close()
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
            Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.Forbidden)), "text/html", False)
        End If
    End Sub
    Public Sub PrepaireDirectory(dir As String)
        Me.SendRequest(GZip.Compress(Me.GetDirectoryPage(dir)), "text/html", DateTime.Now)
    End Sub
    Public Sub PrepairCustom(Value As Byte(), ContentType As String, SendOk As Boolean)
        Me.SendRequest(GZip.Compress(Value), ContentType, DateTime.Now, SendOk)
    End Sub
    Public Sub PrepairCustom(Value As String, ContentType As String, SendOk As Boolean)
        Me.SendRequest(GZip.Compress(Me.Config.Encoder.GetBytes(Value)), ContentType, DateTime.Now, SendOk)
    End Sub
    Public Function SetStatus(StatusCode As HttpStatusCode) As HttpStatusCode
        Me.Parent.Log(String.Format("[Response] {0}", StatusCode.ToString))
        Me.Response.StatusCode = StatusCode
        Return StatusCode
    End Function
    Private Sub ValidateRequest(AbsolutePath As String)
        If (Me.HasGetData) Then
            If (Not Me.GetVariables(DataType.VARGET, Me)) Then
                Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.MethodNotAllowed)), "text/html", False)
                Return
            End If
        End If
        If (Me.HasPostData) Then
            If (Not Me.GetVariables(DataType.VARPOST, Me)) Then
                Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.MethodNotAllowed)), "text/html", False)
                Return
            End If
        End If
        If (Directory.Exists(AbsolutePath) Or File.Exists(AbsolutePath)) Then
            If (Not Me.IsIllegalPath(AbsolutePath)) Then
                If (File.GetAttributes(AbsolutePath) = FileAttributes.Directory) Then
                    If (Me.HasIndexPage(AbsolutePath)) Then
                        Me.PrepaireFile(AbsolutePath)
                    Else
                        If (Me.Config.AllowDirListing) Then
                            Me.PrepaireDirectory(AbsolutePath)
                        Else
                            Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.Forbidden)), "text/html", False)
                        End If
                    End If
                Else
                    Me.PrepaireFile(AbsolutePath)
                End If
            Else
                Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.Forbidden)), "text/html", False)
            End If
        Else
            Me.PrepairCustom(Me.GetErrorPage(Me.SetStatus(HttpStatusCode.NotFound)), "text/html", False)
        End If
    End Sub
    Public Sub SendRequest(Buffer() As Byte, ContentType As String, LastModified As DateTime, Optional SendOk As Boolean = True)
        If (SendOk) Then Me.SetStatus(HttpStatusCode.OK)
        Me.Response.ContentType = ContentType
        Me.Response.ContentLength64 = Buffer.Length
        For Each entry As KeyValuePair(Of String, String) In Me.Config.Headers
            Me.Response.AddHeader(entry.Key, entry.Value)
        Next
        Me.Response.AddHeader("Content-Encoding", "gzip")
        Me.Response.AddHeader("Keep-Alive", Me.Config.KeepAlive.ToString)
        Me.Response.AddHeader("Accept-Charset", Me.Config.Encoder.BodyName)
        Me.Response.AddHeader("Date", DateTime.Now.ToString("r"))
        Me.Response.AddHeader("Last-Modified", LastModified.ToString("r"))
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
        Return Me.Method.ToLower.Equals("post")
    End Function
    Public Function HasGetData() As Boolean
        Return Me.Method.ToLower.Equals("get") AndAlso Me.Context.Request.QueryString.Count > 0
    End Function
    Public Function GetVariables(Type As DataType, ByRef Collection As Dictionary(Of String, String)) As Boolean
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
    Public ReadOnly Property GetLocalPath(ParamArray Combine() As String) As String
        Get
            Return Path.GetFullPath(String.Format("{0}\{1}", Me.Config.DocumentRoot.FullName, String.Join("\", Combine)))
        End Get
    End Property
    Public ReadOnly Property GetRelativePath(Filename As String) As String
        Get
            Dim path As String = New Uri(Filename).AbsolutePath, absolute As New DirectoryInfo(Me.GetLocalPath)
            Return path.Substring(path.IndexOf(absolute.Name) + absolute.Name.Length)
        End Get
    End Property
    Public ReadOnly Property GetErrorPage(errorcode As HttpStatusCode) As Byte()
        Get
            Return New Generator(Me).ErrorPage(errorcode)
        End Get
    End Property
    Public ReadOnly Property GetDirectoryPage(dir As String) As Byte()
        Get
            Return New Generator(Me).DirectoryPage(dir)
        End Get
    End Property
End Class
