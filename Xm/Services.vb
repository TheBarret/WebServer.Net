Imports System.IO
Imports System.IO.Compression

Public NotInheritable Class Services
    Public Shared Function HasHeader(buffer() As Byte) As Boolean
        Return buffer IsNot Nothing AndAlso buffer.Length >= 3 AndAlso {buffer(0), buffer(1), buffer(2)}.SequenceEqual({&H58, &H4D, &H54})
    End Function
    Public Shared Function Mask(value As String, shift As Byte) As String
        If (shift > 8 Or shift < 0) Then shift = 5
        Return value.Select(Function(ch) Strings.AscW(ch) << shift).Aggregate(String.Empty, Function(x, y) String.Format("{0}{1}", x, Strings.ChrW(y * 2)))
    End Function
    Public Shared Function Unmask(value As String, shift As Byte) As String
        If (shift > 8 Or shift < 0) Then shift = 5
        Return value.Select(Function(ch) Strings.AscW(ch) >> shift).Aggregate(String.Empty, Function(x, y) String.Format("{0}{1}", x, Strings.ChrW(y \ 2)))
    End Function
    Public Shared Sub WriteString(buffer As List(Of Byte), value As String, offset As Byte, shift As Byte)
        For Each ch As Char In Services.Mask(value, shift)
            buffer.AddRange(BitConverter.GetBytes(CUShort(Strings.AscW(ch) + offset)))
        Next
        buffer.Add(0)
    End Sub
    Public Shared Function Compress(Buffer() As Byte) As Byte()
        Using ms As New MemoryStream
            Using gzs As New GZipStream(ms, CompressionLevel.Optimal, True)
                gzs.Write(Buffer, 0, Buffer.Length)
            End Using
            Return ms.ToArray
        End Using
    End Function
    Public Shared Function Decompress(Buffer() As Byte) As Byte()
        Using input As New MemoryStream(Buffer)
            Using gzs As New GZipStream(input, CompressionMode.Decompress)
                Using ms As New MemoryStream
                    gzs.CopyTo(ms)
                    Return ms.ToArray
                End Using
            End Using
        End Using
    End Function
End Class
