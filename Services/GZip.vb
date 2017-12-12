Imports System.IO
Imports System.IO.Compression

Public NotInheritable Class GZip
    Public Shared Function Compress(Buffer() As Byte, Optional Level As CompressionLevel = CompressionLevel.Fastest) As Byte()
        Using ms As New MemoryStream
            Using gzs As New GZipStream(ms, Level, True)
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
