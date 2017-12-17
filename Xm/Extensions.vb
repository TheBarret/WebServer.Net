Imports Xm.Elements
Imports Xm.Elements.Types
Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices

Public Module Extensions
    <Extension>
    Public Sub SaveAs(stream As Byte(), filename As String)
        Using fs As New FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)
            fs.Write(stream, 0, stream.Length)
        End Using
    End Sub
    <Extension>
    Public Function Compress(stream As Byte()) As Byte()
        Using ms As New MemoryStream
            Using gzs As New GZipStream(ms, CompressionLevel.Optimal, True)
                gzs.Write(stream, 0, stream.Length)
            End Using
            Return ms.ToArray
        End Using
    End Function
    <Extension>
    Public Function Decompress(stream As Byte()) As Byte()
        Using gzs As New GZipStream(New MemoryStream(stream), CompressionMode.Decompress)
            Using ms As New MemoryStream
                gzs.CopyTo(ms)
                Return ms.ToArray
            End Using
        End Using
    End Function
    <Extension>
    Public Sub ForEach(dict As Dictionary(Of String, Object), action As Action(Of String, Object))
        For Each pair As KeyValuePair(Of String, Object) In dict
            action.Invoke(pair.Key, pair.Value)
        Next
    End Sub
    <Extension>
    Public Function ConvertToList(Of T)(value As T) As List(Of T)
        Return New List(Of T) From {value}
    End Function
    <Extension>
    Public Function AsList(Of T)(value As T, collection As List(Of T)) As List(Of T)
        Dim result As New List(Of T) From {value}
        result.AddRange(collection)
        Return result
    End Function
    <Extension>
    Public Function RequiresRuntime(d As [Delegate]) As Boolean
        Return d.Method.GetParameters.Any AndAlso d.Method.GetParameters.First.ParameterType Is GetType(Runtime)
    End Function
    <Extension>
    Public Function IsPrimitive(value As Expression) As Boolean
        Select Case value.GetType
            Case GetType(TString)
                Return True
            Case GetType(TBoolean)
                Return True
            Case GetType(TInteger)
                Return True
            Case GetType(TFloat)
                Return True
            Case Else
                Return False
        End Select
    End Function
    <Extension>
    Public Function GetExpressionType(value As Expression) As Types
        Select Case value.GetType
            Case GetType(TString)
                Return Types.String
            Case GetType(TBoolean)
                Return Types.Boolean
            Case GetType(TInteger)
                Return Types.Integer
            Case GetType(TFloat)
                Return Types.Float
            Case GetType(TFunction)
                Return Types.Function
            Case GetType(TNull)
                Return Types.Null
            Case Else
                Return Types.Undefined
        End Select
    End Function
    <Extension>
    Public Function FriendlyName(type As Tokens) As String
        Select Case type
            Case Tokens.T_Plus : Return "+"
            Case Tokens.T_Minus : Return "-"
            Case Tokens.T_Mult : Return "*"
            Case Tokens.T_Div : Return "/"
            Case Tokens.T_Mod : Return "%"
            Case Tokens.T_Assign : Return "="
            Case Tokens.T_Or : Return "|"
            Case Tokens.T_And : Return "&"
            Case Tokens.T_Xor : Return "^"
            Case Tokens.T_Comma : Return ","
            Case Tokens.T_Dot : Return "."
            Case Tokens.T_Negate : Return "!"
            Case Tokens.T_Equal : Return "=="
            Case Tokens.T_NotEqual : Return "!="
            Case Tokens.T_Greater : Return ">"
            Case Tokens.T_Lesser : Return "<"
            Case Tokens.T_EqualOrGreater : Return ">="
            Case Tokens.T_EqualOrLesser : Return "<="
            Case Tokens.T_ParenthesisOpen : Return "("
            Case Tokens.T_ParenthesisClose : Return ")"
            Case Tokens.T_BraceOpen : Return "{"
            Case Tokens.T_BraceClose : Return "}"
            Case Tokens.T_Null : Return "Null"
            Case Tokens.T_EndStatement : Return ";"
            Case Tokens.T_EndOfFile : Return "EOF"
            Case Else : Return type.ToString
        End Select
    End Function
End Module
