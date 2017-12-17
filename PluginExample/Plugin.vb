Imports Webserver
Imports Webserver.Plugins
Imports System.Text.RegularExpressions

Public Class Plugin
    Implements IPlugin
    Public Sub Load(Listener As Listener) Implements IPlugin.Load
        ' Initialize code here...
    End Sub
    Public Sub ClientRequest(Client As Client, ByRef Claimed As Boolean) Implements IPlugin.ClientRequest
        ' We can claim a request before the server handles it, this would be usefull if we implement a custom script handler
        ' When claimed is set to true, the server will not process the request and close it when the plugin routine is finished.

        ' !! Remark !! 
        ' Plugins made by other developers can ignore the 'Claimed' status

        'If (Not Claimed AndAlso Client.Request.Url.AbsolutePath.Equals("/alias")) Then
        '   ...do something with the request...
        '   Client.SendRequest(buffer,ContentType,Date,False)
        '   Claimed = True
        'End If
    End Sub
    Public Sub ClientSend(Client As Client, ByRef buffer() As Byte, ByRef ContentType As String) Implements IPlugin.ClientSend
        ' We can modify the buffer before it is send, for instance we can 
        ' replace a text phrase with something else as shown here below

        'If (ContentType Like "text/*") Then
        '   buffer = Client.Encoding.GetBytes(Regex.Replace(Client.Encoding.GetString(buffer), "phrase", "replacement", RegexOptions.IgnoreCase))
        'End If
    End Sub
    Public ReadOnly Property Name As String Implements IPlugin.Name
        Get
            Return "Example"
        End Get
    End Property
    Public ReadOnly Property Author As String Implements IPlugin.Author
        Get
            Return "Barret"
        End Get
    End Property
End Class
