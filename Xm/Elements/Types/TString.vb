Imports Xm.Library

Namespace Elements.Types
    <Serializable>
    Public NotInheritable Class TString
        Inherits Expression
        Public Property Value As String
        Sub New(Value As String)
            Me.Value = Value
        End Sub
        Public Function Escaped() As String
            Return StringParser.Parse(Me.Value)
        End Function
        Public Overrides Function ToString() As String
            Return String.Format("{0}", Me.Value)
        End Function
    End Class
End Namespace