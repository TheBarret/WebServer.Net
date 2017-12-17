Imports System.Text.RegularExpressions
Imports System.Text

Namespace Lexer
    Public NotInheritable Class Parser
        Inherits List(Of Token)
        Public Function Analyze(context As String) As List(Of Token)
            If (Not String.IsNullOrEmpty(context)) Then
                Dim position As Integer = 0, line As Integer = 1, index As Integer = 0, flag As Boolean, len As Integer = context.Length
                Using Definitions As New Definitions
                    Do
                        flag = False
                        For Each Rule As KeyValuePair(Of Tokens, String) In Definitions
                            Dim match As Match = Definitions.Match(Rule.Key, context)
                            If (match.Success) Then
                                flag = True
                                context = context.Remove(match.Index, match.Length)
                                If (Rule.Key = Tokens.T_Newline) Then
                                    line += 1
                                ElseIf (Rule.Key = Tokens.T_LineComment) Then
                                    line += Regex.Matches(match.Value, "\r\n").Count
                                ElseIf (Rule.Key = Tokens.T_BlockComment) Then
                                    line += Regex.Matches(match.Value, "\r\n").Count
                                End If
                                Me.Add(New Token(match.Value, Rule.Key, line, index, match.Length))
                                index += match.Length
                            End If
                            If (flag) Then Exit For
                        Next
                        If (Not flag) Then
                            Throw New Exception(String.Format("Undefined character '{0}' at index {1} line {2}", context(0), index, line))
                        End If
                    Loop Until index = len
                End Using
                Me.Add(Token.Create(Tokens.T_EndOfFile, String.Empty, line, index))
                Return Me.Where(Function(token) token.Type <> Tokens.T_Space AndAlso token.Type <> Tokens.T_Newline AndAlso
                                            token.Type <> Tokens.T_BlockComment AndAlso token.Type <> Tokens.T_LineComment).ToList
            End If
            Throw New Exception("context is empty")
        End Function
    End Class
End Namespace