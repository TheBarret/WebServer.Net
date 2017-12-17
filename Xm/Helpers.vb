Imports System.Reflection
Imports Exp = System.Linq.Expressions.Expression
Public NotInheritable Class Helpers
    Public Shared Function Addition(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() + b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() + b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() + b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() + b.Cast(Of Single)())
        Else
            Return New TValue(a.Value.ToString & b.Value.ToString)
        End If
    End Function
    Public Shared Function Subtraction(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() - b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() - b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() - b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() - b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function Multiplication(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() * b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() * b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() * b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() * b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function Division(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() \ b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() / b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() / b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() / b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function Modulo(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() Mod b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() Mod b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() Mod b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() Mod b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function SignedPositive(a As TValue) As TValue
        If (a.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() * 1)
        ElseIf (a.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() * 1.0R)
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function SignedNegative(a As TValue) As TValue
        If (a.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() * -1)
        ElseIf (a.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() * -1.0R)
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function LogicalAnd(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Boolean And b.ScriptType = Types.Boolean) Then
            Return New TValue(a.Cast(Of Boolean)() And b.Cast(Of Boolean)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function LogicalOr(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Boolean And b.ScriptType = Types.Boolean) Then
            Return New TValue(a.Cast(Of Boolean)() Or b.Cast(Of Boolean)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function LogicalXor(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Boolean And b.ScriptType = Types.Boolean) Then
            Return New TValue(a.Cast(Of Boolean)() Xor b.Cast(Of Boolean)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function Negate(a As TValue) As TValue
        If (a.ScriptType = Types.Integer) Then
            Return New TValue(Not a.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float) Then
            Return New TValue(Not CLng(a.Cast(Of Single)()))
        ElseIf (a.ScriptType = Types.Boolean) Then
            Return New TValue(Not a.Cast(Of Boolean)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function IsEqual(a As TValue, b As TValue) As TValue
        Return New TValue(a.Value.Equals(b.Value))
    End Function
    Public Shared Function IsNotEqual(a As TValue, b As TValue) As TValue
        Return New TValue(Not a.Value.Equals(b.Value))
    End Function
    Public Shared Function IsGreater(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() > b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() > b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() > b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() > b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function IsLesser(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() < b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() < b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() < b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() < b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function IsEqualOrGreater(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() >= b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() >= b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() >= b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() >= b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function IsEqualOrLesser(a As TValue, b As TValue) As TValue
        If (a.ScriptType = Types.Integer And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Integer)() <= b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Integer) Then
            Return New TValue(a.Cast(Of Single)() <= b.Cast(Of Integer)())
        ElseIf (a.ScriptType = Types.Integer And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Integer)() <= b.Cast(Of Single)())
        ElseIf (a.ScriptType = Types.Float And b.ScriptType = Types.Float) Then
            Return New TValue(a.Cast(Of Single)() <= b.Cast(Of Single)())
        Else
            Return TValue.Null
        End If
    End Function
    Public Shared Function CreateDelegate(Type As Type, Name As String) As [Delegate]
        Dim method As MethodInfo = Type.GetMethod(Name, BindingFlags.Public Or BindingFlags.Static Or BindingFlags.IgnoreCase)
        If (method IsNot Nothing AndAlso method.IsStatic And method.IsPublic) Then
            Return method.CreateDelegate(Exp.GetDelegateType((From p In method.GetParameters Select p.ParameterType).Concat(New Type() {method.ReturnType}).ToArray))
        End If
        Throw New Exception(String.Format("Method '{0}.{1}' not found with these parameters", Type.Name, Name))
    End Function
    Public Shared Function Unwrap(value As Object) As Object
        Do While TypeOf value Is TValue Or TypeOf value Is Null
            If (TypeOf value Is TValue) Then
                value = CType(value, TValue).Value
            ElseIf (TypeOf value Is Null) Then
                value = Nothing
            End If
        Loop
        Return value
    End Function
End Class
