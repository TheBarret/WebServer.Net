Imports System.IO
Imports System.Reflection
Imports System.Text

Namespace Library
    Public Class Stdio
        <Method(Types.Null, "print")>
        Public Shared Sub Print(rt As Runtime, str As Object)
            If (rt.Output IsNot Nothing) Then
                rt.Output.WriteLine(str)
            Else
                Console.WriteLine(str)
            End If
        End Sub
        <Method(Types.Null, "read")>
        Public Shared Function Read(rt As Runtime) As String
            If (rt.Input IsNot Nothing) Then
                Return rt.Input.ReadLine()
            Else
                Return Console.ReadLine
            End If
        End Function
        <Method(Types.Null, "readall")>
        Public Shared Function ReadAll(rt As Runtime) As String
            If (rt.Input IsNot Nothing) Then
                Return rt.Input.ReadToEnd
            Else
                Return Console.ReadLine
            End If
        End Function
    End Class
End Namespace