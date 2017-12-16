Imports System.Net

Namespace Plugins
    Public Interface IPlugin
        Sub Load()
        Sub ClientRequest(Client As Client, ByRef Claimed As Boolean)
        Sub ClientSend(Client As Client, ByRef buffer() As Byte, ByRef ContentType As String)
        ReadOnly Property Name As String
        ReadOnly Property Author As String
    End Interface
End Namespace