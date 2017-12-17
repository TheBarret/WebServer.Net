
Imports System.IO
Imports System.Text
Namespace Library
    Friend Class Output
        Inherits TextWriter
        Sub New(encoding As Encoding)
            Me.m_encoding = encoding
            Me.m_buffer = New StringBuilder
        End Sub
        Public Overrides Sub Write(value As Char)
            Me.m_buffer.Append(value)
        End Sub
        Public Overrides Sub Write(value As String)
            Me.m_buffer.Append(value)
        End Sub
        Private m_buffer As StringBuilder
        Public ReadOnly Property Buffer As StringBuilder
            Get
                Return Me.m_buffer
            End Get
        End Property
        Private m_encoding As Encoding
        Public Overrides ReadOnly Property Encoding As Encoding
            Get
                Return Me.m_encoding
            End Get
        End Property
    End Class
End Namespace