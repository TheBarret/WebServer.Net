Namespace Library
    Public Class Float
        <Method(Types.Float, "abs")>
        Public Shared Function Abs(value As Single) As Single
            Return Math.Abs(value)
        End Function
    End Class
End Namespace