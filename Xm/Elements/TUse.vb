Namespace Elements
    <Serializable>
    Public NotInheritable Class TUse
        Inherits Expression
        Public Property Library As Expression
        Sub New(Library As Expression)
            Me.Library = Library
        End Sub
        Public Overrides Function ToString() As String
            Return String.Format("Use {0}", Me.Library.ToString)
        End Function
    End Class
End Namespace