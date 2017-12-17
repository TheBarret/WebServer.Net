Imports System.Text
Imports System.Security.Cryptography

Namespace Library
    Public Class [String]
        <Method(Types.String, "length")>
        Public Shared Function Length(str As String) As Integer
            Return str.Length
        End Function
        <Method(Types.String, "ucase")>
        Public Shared Function UCase(str As String) As String
            Return Strings.UCase(str)
        End Function
        <Method(Types.String, "lcase")>
        Public Shared Function LCase(str As String) As String
            Return Strings.LCase(str)
        End Function
        <Method(Types.String, "reverse")>
        Public Shared Function Reverse(str As String) As String
            Return Strings.StrReverse(str)
        End Function
        <Method(Types.String, "left")>
        Public Shared Function LeftOfString(str As String, len As Integer) As String
            Return Strings.Left(str, len)
        End Function
        <Method(Types.String, "right")>
        Public Shared Function RightOfString(str As String, len As Integer) As String
            Return Strings.Right(str, len)
        End Function
        <Method(Types.String, "substr")>
        Public Shared Function SubStr(str As String, index As Integer, len As Integer) As String
            Return str.Substring(index, len)
        End Function
        <Method(Types.String, "hex")>
        Public Shared Function Hexadecimal(str As String) As String
            Return String.Concat(Encoding.UTF8.GetBytes(str).Select(Function(v) v.ToString("X")))
        End Function
        <Method(Types.String, "md5")>
        Public Shared Function MD5Hash(str As String) As String
            Return String.Concat(MD5.Create.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(Function(v) v.ToString("X")))
        End Function
        <Method(Types.String, "sha1")>
        Public Shared Function SHA1Hash(str As String) As String
            Return String.Concat(SHA1.Create.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(Function(v) v.ToString("X")))
        End Function
        <Method(Types.String, "sha256")>
        Public Shared Function SHA256Hash(str As String) As String
            Return String.Concat(SHA256.Create.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(Function(v) v.ToString("X")))
        End Function
        <Method(Types.String, "sha384")>
        Public Shared Function SHA384Hash(str As String) As String
            Return String.Concat(SHA384.Create.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(Function(v) v.ToString("X")))
        End Function
        <Method(Types.String, "sha512")>
        Public Shared Function SHA512Hash(str As String) As String
            Return String.Concat(SHA512.Create.ComputeHash(Encoding.UTF8.GetBytes(str)).Select(Function(v) v.ToString("X")))
        End Function
    End Class
End Namespace