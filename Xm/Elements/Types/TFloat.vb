Namespace Elements.Types
    <Serializable>
    Public NotInheritable Class TFloat
        Inherits Expression
        Public Property Value As Single
        Sub New(Value As Single)
            Me.Value = Value
        End Sub
        Public Overrides Function ToString() As String
            Return String.Format("{0}", Me.Value)
        End Function
    End Class
End Namespace