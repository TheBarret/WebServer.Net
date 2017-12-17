Imports Xm.Elements
Imports Xm.Elements.Types

Namespace AST
    Partial Public Class Parser
        Private Function ParseNull() As Expression
            Try
                Return New TNull
            Finally
                Me.NextToken()
            End Try
        End Function
        Private Function ParseBoolean() As Expression
            Try
                Return New TBoolean(Boolean.Parse(Me.Current.Value))
            Finally
                Me.NextToken()
            End Try
        End Function
        Private Function ParseString() As Expression
            Try
                Return New TString(Me.Current.Value.Substring(1, Me.Current.Value.Length - 2))
            Finally
                Me.NextToken()
            End Try
        End Function
        Private Function ParseIdentifier() As Expression
            Try
                Return New TIdentifier(Me.Current.Value)
            Finally
                Me.NextToken()
            End Try
        End Function
        Private Function ParseSignedParentheses(op As Tokens) As Expression
            Return New Unary(Me.ParseExpression(False), op, True)
        End Function
        Private Function ParseSignedIdentifier(op As Tokens) As Expression
            Try
                Return New Unary(New TIdentifier(Me.Current.Value), op, True)
            Finally
                Me.NextToken()
            End Try
        End Function
        Private Function ParseInteger(signed As Boolean, Optional op As Tokens = Tokens.T_Null) As Expression
            Try
                If (signed And op = Tokens.T_Minus) Then
                    Return New TInteger(Integer.Parse(Me.Current.Value, Env.Integers) * -1)
                End If
                Return New TInteger(Integer.Parse(Me.Current.Value, Env.Integers) * 1)
            Finally
                Me.NextToken()
            End Try
        End Function
        Private Function ParseFloat(signed As Boolean, Optional op As Tokens = Tokens.T_Null) As Expression
            Try
                If (signed And op = Tokens.T_Minus) Then
                    Return New TFloat(Single.Parse(Me.Current.Value.Replace(".", ","), Env.Floats) * -1.0F)
                End If
                Return New TFloat(Single.Parse(Me.Current.Value.Replace(".", ","), Env.Floats) * 1.0F)
            Finally
                Me.NextToken()
            End Try
        End Function
        Private Function ParseReturn() As Expression
            Me.NextToken()
            Return New TReturn(Me.ParseExpression(False))
        End Function
        Private Function ParseParenthesis() As Expression
            Try
                Me.NextToken()
                Return Me.ParseExpression(False)
            Finally
                Me.Match(Tokens.T_ParenthesisClose)
            End Try
        End Function
        Private Function ParseCondition() As Expression
            Me.NextToken()
            Dim e As New TConditional(Me.ParseExpression(False)) With {.True = Me.ParseBraceBlock}
            If (Me.Current.Type = Tokens.T_Else) Then
                Me.NextToken()
                e.False.AddRange(Me.ParseBraceBlock)
            End If
            Return e
        End Function
        Private Function ParseFunction() As Expression
            Me.NextToken()
            If (Me.Current.Type = Tokens.T_Identifier) Then
                Return New TFunction(CType(Me.ParseIdentifier, TIdentifier), Me.ParseParameters, Me.ParseBraceBlock)
            Else
                Return New TFunction(New TIdentifier(Env.RandomName(8, "func_")), Me.ParseParameters, Me.ParseBraceBlock)
            End If
        End Function
        Private Function ParseBraceBlock() As List(Of Expression)
            Dim body As New List(Of Expression)
            Me.Match(Tokens.T_BraceOpen)
            Do While True
                If (Me.Current.Type = Tokens.T_BraceClose) Then
                    Me.NextToken()
                    Exit Do
                ElseIf (Me.Current.Type = Tokens.T_EndOfFile) Then
                    Throw New Exception(String.Format("Unexpected end of file at line {0}", Me.Current.Line))
                Else
                    body.Add(Me.ParseExpression(True))
                End If
            Loop
            Return body
        End Function
        Private Function ParseParameters() As List(Of Expression)
            Dim params As New List(Of Expression)
            Me.Match(Tokens.T_ParenthesisOpen)
            While Me.Current.Type <> Tokens.T_ParenthesisClose
                params.Add(Me.ParseExpression(False))
                If (Me.Current.Type <> Tokens.T_Comma) Then
                    Exit While
                End If
                Me.NextToken()
            End While
            Me.Match(Tokens.T_ParenthesisClose)
            Return params
        End Function
        Private Function ParseImport() As Expression
            Me.NextToken()
            Return New TUse(Me.ParseExpression(True))
        End Function
    End Class
End Namespace