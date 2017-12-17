Namespace Elements.Types
    <Serializable>
    Public NotInheritable Class TIdentifier
        Inherits Expression
        Public Property Value As String
        Sub New(Value As String)
            Me.Value = Value
        End Sub
        Public Overrides Function ToString() As String
            Return String.Format("{0}", Me.Value)
        End Function
    End Class
End Namespace