Imports Xm.Elements
Imports Xm.Elements.Types

Public NotInheritable Class Serializer
    Public Const Shift As Byte = &H0
    Public Const Offset As Byte = &H5
    Public Shared Function Create(Context As String) As Byte()
        Return New AST.Parser().Tokenize(New Lexer.Parser().Analyze(Context), Serializer.Offset, Serializer.Shift)
    End Function
    Public Shared Function Create(Context As String, offset As Byte, shift As Byte) As Byte()
        Return New AST.Parser().Tokenize(New Lexer.Parser().Analyze(Context), offset, shift)
    End Function
    Public Shared Function Create(stream As List(Of Token), offset As Byte, shift As Byte) As Byte()
        Dim buffer As New List(Of Byte) From {&H58, &H4D, &H54, offset, shift}
        For Each current As Token In stream
            buffer.Add(current.Type)
            Select Case current.Type
                Case Tokens.T_Identifier
                    Services.WriteString(buffer, current.Value, offset, shift)
                Case Tokens.T_String
                    Services.WriteString(buffer, current.Value, offset, shift)
                Case Tokens.T_Integer
                    buffer.AddRange(BitConverter.GetBytes(Integer.Parse(current.Value, Env.Integers)))
                Case Tokens.T_Float
                    buffer.AddRange(BitConverter.GetBytes(Single.Parse(current.Value.Replace(".", ","), Env.Floats)))
                Case Tokens.T_Bool
                    buffer.AddRange(BitConverter.GetBytes(Boolean.Parse(current.Value)))
            End Select
        Next
        Return buffer.ToArray
    End Function
    Public Shared Function Deserialize(buffer() As Byte) As List(Of Token)
        If (buffer IsNot Nothing AndAlso buffer.Length >= 5) Then
            If (Services.HasHeader(buffer)) Then
                Dim offset As Byte = buffer(3), shift As Byte = buffer(4)
                Dim stream As New List(Of Token), current As Token, i As Integer = 5
                Do
                    current = New Token(String.Empty, CType(buffer(i), Tokens))
                    Select Case current.Type
                        Case Tokens.T_Identifier
                            For x As Integer = i + 1 To buffer.Length - 1 Step 2
                                If (buffer(x) = 0) Then Exit For
                                current.Value &= Strings.ChrW(BitConverter.ToUInt16({buffer(x), buffer(x + 1)}, 0) - offset)
                                i += 2
                            Next
                            current.Value = Services.Unmask(current.Value, shift)
                            stream.Add(current)
                            i += 2
                            Continue Do
                        Case Tokens.T_String
                            For x As Integer = i + 1 To buffer.Length - 1 Step 2
                                If (buffer(x) = 0) Then Exit For
                                current.Value &= Strings.ChrW(BitConverter.ToUInt16({buffer(x), buffer(x + 1)}, 0) - offset)
                                i += 2
                            Next
                            current.Value = Services.Unmask(current.Value, shift)
                            stream.Add(current)
                            i += 2
                            Continue Do
                        Case Tokens.T_Integer
                            current.Value = BitConverter.ToInt32({buffer(i + 1), buffer(i + 2), buffer(i + 3), buffer(i + 4)}, 0).ToString
                            stream.Add(current)
                            i += 5
                            Continue Do
                        Case Tokens.T_Float
                            current.Value = BitConverter.ToSingle({buffer(i + 1), buffer(i + 2), buffer(i + 3), buffer(i + 4)}, 0).ToString
                            current.Value.Replace(",", ".") '//Adjust decimal notation
                            stream.Add(current)
                            i += 5
                            Continue Do
                        Case Tokens.T_Bool
                            current.Value = BitConverter.ToBoolean(buffer, i + 1).ToString
                            stream.Add(current)
                            i += 2
                            Continue Do
                    End Select
                    i += 1
                    stream.Add(current)
                Loop Until current.Type = Tokens.T_EndOfFile
                Return stream
            End If
            Throw New Exception("header mismatch")
        End If
        Throw New Exception("invalid buffer")
    End Function
End Class
