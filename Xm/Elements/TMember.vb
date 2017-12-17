Namespace Elements
    <Serializable>
    Public NotInheritable Class TMember
        Inherits Expression
        Public Property Target As Expression
        Public Property Member As Expression
        Public Property Parameters As List(Of Expression)
        Sub New(Target As Expression, Member As Expression)
            Me.Target = Target
            Me.Member = Member
            Me.Parameters = New List(Of Expression)
        End Sub
        Sub New(Target As Expression, Member As Expression, Parameters As List(Of Expression))
            Me.Target = Target
            Me.Member = Member
            Me.Parameters = Parameters
        End Sub
        Public Overrides Function ToString() As String
            Return String.Format("{0}.{1}({2})", Me.Target, Me.Member, String.Join(",", Me.Parameters.Select(Function(x) x)))
        End Function
    End Class
End Namespace