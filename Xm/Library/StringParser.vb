Imports System.Text

Namespace Library
    Public Class StringParser
        Public Shared Function Parse(str As String) As String
            Dim buffer As New StringBuilder, state As Boolean = False
            For Each ch As Char In str.ToCharArray
                Select Case ch
                    Case "\"c
                        If (state) Then
                            buffer.Append("\")
                            Continue For
                        Else
                            state = True
                            Continue For
                        End If
                    Case "r"c
                        If (state) Then
                            buffer.Append(ControlChars.Cr)
                            state = False
                            Continue For
                        End If
                    Case "n"c
                        If (state) Then
                            buffer.Append(ControlChars.Lf)
                            state = False
                            Continue For
                        End If
                    Case "t"c
                        If (state) Then
                            buffer.Append(ControlChars.Tab)
                            state = False
                            Continue For
                        End If
                End Select
                buffer.Append(ch)
            Next
            Return buffer.ToString
        End Function
    End Class
End Namespace