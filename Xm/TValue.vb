Imports Xm.Elements
Imports Xm.Elements.Types

Public NotInheritable Class TValue
    Public Property Value As Object
    Sub New()
        Me.Value = New Null
    End Sub
    Sub New(Value As Object)
        If (Value Is Nothing) Then
            Me.Value = New Null
        Else
            Me.Value = Value
        End If
    End Sub
    Public Function ScriptType() As Types
        If (TypeOf Me.Value Is String) Then
            Return Types.String
        ElseIf (TypeOf Me.Value Is Integer) Then
            Return Types.Integer
        ElseIf (TypeOf Me.Value Is Single) Then
            Return Types.Float
        ElseIf (TypeOf Me.Value Is Boolean) Then
            Return Types.Boolean
        ElseIf (TypeOf Me.Value Is TFunction) Then
            Return Types.Function
        ElseIf (TypeOf Me.Value Is [Delegate]) Then
            Return Types.Delegate
        ElseIf (TypeOf Me.Value Is Null) Then
            Return Types.Null
        Else
            Return Types.Undefined
        End If
    End Function
    Public Function IsString() As Boolean
        Return Me.ScriptType = Types.String
    End Function
    Public Function IsBoolean() As Boolean
        Return Me.ScriptType = Types.Boolean
    End Function
    Public Function IsInteger() As Boolean
        Return Me.ScriptType = Types.Integer
    End Function
    Public Function IsFloat() As Boolean
        Return Me.ScriptType = Types.Float
    End Function
    Public Function IsFunction() As Boolean
        Return Me.ScriptType = Types.Function
    End Function
    Public Function IsNull() As Boolean
        Return Me.ScriptType = Types.Null
    End Function
    Public Function IsDelegate() As Boolean
        Return Me.ScriptType = Types.Delegate
    End Function
    Public Function IsUndefined() As Boolean
        Return Me.ScriptType = Types.Undefined
    End Function
    Public Function IsPrimitive() As Boolean
        Return Me.ScriptType = Types.String Or Me.ScriptType = Types.Boolean Or
               Me.ScriptType = Types.Integer Or Me.ScriptType = Types.Float
    End Function
    Public Function Cast(Of T)() As T
        If (GetType(T) Is Me.Value.GetType) Then
            Return CType(Me.Value, T)
        End If
        Throw New Exception(String.Format("Cannot cast value '{0}' to '{1}'", Me.Value.GetType.Name, GetType(T).Name))
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("{0} [{1}]", Me.Value, Me.ScriptType)
    End Function
    Public Shared Function Null() As TValue
        Return New TValue
    End Function
End Class
