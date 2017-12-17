Namespace Elements.Types
    <Serializable>
    Public NotInheritable Class TNull
        Inherits Expression
        Public Overrides Function ToString() As String
            Return "Null"
        End Function
    End Class
End Namespace