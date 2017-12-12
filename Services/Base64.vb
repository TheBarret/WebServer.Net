Imports System.Text

Public Class Base64
    Public Shared Function Decode(Value As String, Encoding As Encoding) As Byte()
        If (Value.Contains(" ")) Then
            Value = Value.Replace(" ", String.Empty)
        End If
        If (Value.Contains(ControlChars.Tab)) Then
            Value = Value.Replace(ControlChars.Tab, String.Empty)
        End If
        If (Value.Contains(ControlChars.Cr)) Then
            Value = Value.Replace(ControlChars.Cr, String.Empty)
        End If
        If (Value.Contains(ControlChars.Lf)) Then
            Value = Value.Replace(ControlChars.Lf, String.Empty)
        End If
        Return Convert.FromBase64String(Value.Trim)
    End Function
End Class
