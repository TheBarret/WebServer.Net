Imports System.IO
Imports Webserver
Imports Webserver.Plugins
Imports System.Text

Public Class Plugin
    Inherits TextWriter
    Implements IPlugin
    Public Property Output As StringBuilder
    Public Sub Load(Listener As Listener) Implements IPlugin.Load
        Me.Output = New StringBuilder
        Listener.ContentTypes.Add(".xm", "text/html")
    End Sub
    Public Sub ClientRequest(Client As Webserver.Client, ByRef Claimed As Boolean) Implements IPlugin.ClientRequest
        Dim requested As String = Client.LocalPath(Client.Request.Url.AbsolutePath)
        If (Not File.GetAttributes(requested) = FileAttributes.Directory AndAlso File.Exists(requested)) Then
            If (Path.GetExtension(requested).ToLower = ".xm") Then
                Try
                    '// Set request claimed
                    Claimed = True
                    '// Prepair script environment and evaluate it
                    Using sr As New StreamReader(New FileStream(requested, FileMode.Open, FileAccess.Read))
                        Using Runtime As New Runtime(sr.ReadToEnd) With {.Output = Me, .Input = New NullReader}
                            Runtime.AddGlobal("host", Client.Host)
                            Runtime.AddGlobal("method", Client.Method)
                            Runtime.AddGlobal("userip", Client.RemoteAddress)
                            Runtime.AddGlobal("useragent", Client.UserAgent)
                            Runtime.AddGlobal("directory", Client.LocalPath)
                            Runtime.AddGlobal("protocol", Client.ProtocolVersion.ToString)
                            Runtime.AddGlobal(Of String, String)(Client.Data)
                            Runtime.Evaluate()
                        End Using
                    End Using
                Finally
                    '// Send output buffer
                    Client.SendRequest(Client.Encoding.GetBytes(Me.Output.ToString), "text/html", File.GetLastWriteTime(requested))
                    Me.Output.Clear()
                End Try
            End If
        End If
    End Sub
    Public Sub ClientSend(Client As Webserver.Client, ByRef buffer() As Byte, ByRef ContentType As String) Implements IPlugin.ClientSend
        '// Nothing doing in here
    End Sub
#Region "Properties"
    Public ReadOnly Property Name As String Implements IPlugin.Name
        Get
            Return "XM Script Handler"
        End Get
    End Property
    Public ReadOnly Property Author As String Implements IPlugin.Author
        Get
            Return "Barret"
        End Get
    End Property
#End Region
#Region "TextWriter"
    Public Overrides Sub Write(value As Object)
        Me.Output.Append(value)
    End Sub
    Public Overrides Sub Write(value As Char)
        Me.Output.Append(value)
    End Sub
    Public Overrides Sub Write(value As String)
        Me.Output.Append(value)
    End Sub
    Public Overrides ReadOnly Property Encoding As Encoding
        Get
            Return Text.Encoding.UTF8
        End Get
    End Property
#End Region
End Class
