Imports System.IO
Imports System.Reflection
Imports System.Text

Namespace Library
    Public Class Common
        <Method(Types.Null, "isset")>
        Public Shared Function IsSet(rt As Runtime, ref As String) As Boolean
            Return rt.Scope.Variable(ref)
        End Function
        <Method(Types.Null, "base")>
        Public Shared Function BasePath() As String
            Return String.Format("{0}\", Path.GetDirectoryName(Assembly.GetExecutingAssembly.Location)).Replace("\\", "\")
        End Function
        <Method(Types.Null, "eval")>
        Public Shared Sub Evaluate(rt As Runtime, expr As String)
            Using runtime As New Runtime(expr)
                runtime.SetInput(rt.Input)
                runtime.SetOutput(rt.Output)
                runtime.Evaluate()
            End Using
        End Sub
    End Class
End Namespace