Imports System.IO

Namespace Library
    Public Class IO
        <Method(Types.Null, "fread")>
        Public Shared Function FileRead(filename As String) As String
            If (File.Exists(filename)) Then
                Return File.ReadAllText(filename)
            End If
            Return String.Empty
        End Function
        <Method(Types.Null, "exists")>
        Public Shared Function FileOrDirectoryExists(filename As String) As Boolean
            Return File.Exists(filename) Or Directory.Exists(filename)
        End Function
        <Method(Types.Null, "fwrite")>
        Public Shared Sub FileWrite(filename As String, value As String, append As Boolean)
            Using sw As New StreamWriter(filename, append)
                sw.Write(value)
                sw.Flush()
            End Using
        End Sub
        <Method(Types.Null, "isfile")>
        Public Shared Function IsFile(filename As String) As Boolean
            Return File.Exists(filename)
        End Function
        <Method(Types.Null, "isdirectory")>
        Public Shared Function IsDirectory(filename As String) As Boolean
            Return Directory.Exists(filename)
        End Function
    End Class
End Namespace