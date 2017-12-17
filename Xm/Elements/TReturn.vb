Namespace Elements
    <Serializable>
    Public NotInheritable Class TReturn
        Inherits Expression
        Public Property Operand As Expression
        Sub New(Operand As Expression)
            Me.Operand = Operand
        End Sub
        Public Overrides Function ToString() As String
            Return String.Format("Return {0}", Me.Operand)
        End Function
    End Class
End Namespace