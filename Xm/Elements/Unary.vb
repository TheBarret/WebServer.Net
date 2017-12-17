Namespace Elements
    <Serializable>
    Public NotInheritable Class Unary
        Inherits Expression
        Public Property Operand As Expression
        Public Property Op As Tokens
        Public Property Prefix As Boolean
        Sub New(Operand As Expression, Op As Tokens, Prefix As Boolean)
            Me.Operand = Operand
            Me.Op = Op
            Me.Prefix = Prefix
        End Sub
        Public Overrides Function ToString() As String
            If (Prefix) Then Return String.Format("{0} {1}", Me.Op.FriendlyName, Me.Operand.ToString)
            Return String.Format("{0} {1}", Me.Operand.ToString, Me.Op.FriendlyName)
        End Function
    End Class
End Namespace