Namespace Library
    Public Class [Integer]
        <Method(Types.Integer, "abs")>
        Public Shared Function Abs(value As Integer) As Integer
            Return Math.Abs(value)
        End Function
        <Method(Types.Integer, "iseven")>
        Public Shared Function IsEven(value As Integer) As Boolean
            Return value Mod 2 = 0
        End Function
        <Method(Types.Integer, "isodd")>
        Public Shared Function IsOdd(value As Integer) As Boolean
            Return value Mod 2 <> 0
        End Function
        <Method(Types.Integer, "hex")>
        Public Shared Function Hexadecimal(value As Integer) As String
            Return value.ToString("X")
        End Function
    End Class
End Namespace