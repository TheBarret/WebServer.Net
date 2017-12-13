Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.IO
Imports System.Text
Imports System.Collections.Specialized

Module Extensions
    <Extension>
    Public Function HumanReadable(value As Long) As String
        Static sizes() As String = {"B", "KB", "MB", "GB", "TB"}
        Dim len As Long = value, position As Integer = 0
        While len >= 1024 And position < sizes.Length - 1
            position += 1
            len = len \ 1024
        End While
        Return String.Format("{0:0.##} {1}", len, sizes(position))
    End Function
    <Extension>
    Public Function ToDictionary(src As NameValueCollection, Request As HttpListenerRequest) As Dictionary(Of String, String)
        Dim collection As New Dictionary(Of String, String)
        For Each name As String In src.AllKeys
            collection.Add(name, Request.QueryString.Item(name))
        Next
        Return collection
    End Function
    <Extension>
    Public Function ToDictionary(src As Stream) As Dictionary(Of String, String)
        Dim collection As New Dictionary(Of String, String)
        Using sr As New StreamReader(src)
            For Each entry As String In sr.ReadToEnd.Split("&"c)
                Dim values() As String = Strings.Split(entry, "=")
                If (values.Any(Function(x) x.Length > 0)) Then
                    collection.Add(WebUtility.UrlDecode(values.First), WebUtility.UrlDecode(values.Last))
                End If
            Next
        End Using
        Return collection
    End Function
End Module
