Public Class Method
    Inherits Attribute
    Public Property Type As Types
    Public Property Reference As String
    Sub New(type As Types, ref As String)
        Me.Type = type
        Me.Reference = ref
    End Sub
End Class