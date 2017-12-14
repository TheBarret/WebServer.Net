Imports Webserver
Imports Webserver.Plugins
Imports System.Text.RegularExpressions

Public Class Smileys
    Implements IPlugin
    Public Sub Load() Implements Plugins.IPlugin.Load
        '// Initialize code here...
    End Sub
    Public Sub ClientRequest(Client As Client, ByRef Claimed As Boolean) Implements Plugins.IPlugin.ClientRequest
        'If (Not Claimed) Then
        '   ...do something with the request...
        '   Claimed = True
        'End If
    End Sub
    Public Sub ClientSend(Client As Client, ByRef buffer() As Byte, ByRef ContentType As String) Implements Plugins.IPlugin.ClientSend
        If (ContentType Like "text/*") Then
            buffer = Client.Encoding.GetBytes(Regex.Replace(Client.Encoding.GetString(buffer), "server", " :-) ", RegexOptions.IgnoreCase))
        End If
    End Sub
End Class
