Namespace Library
    Public Class Any
        <Method(Types.Any, "type")>
        Public Shared Function Type(value As Object) As String
            Dim tval As New TValue(value)
            Return tval.ScriptType.ToString
        End Function
        <Method(Types.Any, "isnull")>
        Public Shared Function IsNull(value As Object) As Boolean
            Dim tval As New TValue(value)
            Return tval.IsNull
        End Function
        <Method(Types.Any, "isnumber")>
        Public Shared Function IsNumber(value As Object) As Boolean
            Dim tval As New TValue(value)
            Return tval.IsInteger Or tval.IsFloat
        End Function
        <Method(Types.Any, "isstring")>
        Public Shared Function IsString(value As Object) As Boolean
            Dim tval As New TValue(value)
            Return tval.IsString
        End Function
        <Method(Types.Any, "isbool")>
        Public Shared Function IsBoolean(value As Object) As Boolean
            Dim tval As New TValue(value)
            Return tval.IsBoolean
        End Function
        <Method(Types.Any, "isfunction")>
        Public Shared Function IsFunction(value As Object) As Boolean
            Dim tval As New TValue(value)
            Return tval.IsFunction
        End Function
        <Method(Types.Any, "tostring")>
        Public Shared Function ConvertToString(value As Object) As String
            Return value.ToString
        End Function
        <Method(Types.Any, "tobool")>
        Public Shared Function ConvertToBoolean(value As Object) As Boolean
            Dim result As Boolean
            If (Boolean.TryParse(value.ToString, result)) Then
                Return result
            End If
            Return Nothing
        End Function
        <Method(Types.Any, "tointeger")>
        Public Shared Function ConvertToInteger(value As Object) As Integer
            If (Char.IsNumber(value.ToString, 0)) Then
                Dim result As Integer
                If (Integer.TryParse(value.ToString, result)) Then
                    Return result
                Else
                    Return CInt(value.ToString)
                End If
            End If
            Return Nothing
        End Function
        <Method(Types.Any, "tofloat")>
        Public Shared Function ConvertToFloat(value As Object) As Single
            If (Char.IsNumber(value.ToString, 0)) Then
                Dim result As Single
                If (Single.TryParse(value.ToString, result)) Then
                    Return result
                Else
                    Return CSng(value.ToString)
                End If
            End If
            Return Nothing
        End Function
    End Class
End Namespace