Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.IO
Imports System.Text

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
End Module
