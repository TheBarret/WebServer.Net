Namespace Services
    Public NotInheritable Class CRC32
        Private Property Buffer As Byte()
        Private Property Seed As UInt32
        Private Property Table As UInt32()
        Private Property Polynomial As UInt32
        Sub New(buffer() As Byte)
            Me.Buffer = buffer
            Me.Seed = &HFFFFFFFFUI
            Me.Polynomial = &HEDB88320UI
            Me.Initialize()
        End Sub
        Sub New(buffer() As Byte, seed As UInt32)
            Me.Buffer = buffer
            Me.Seed = seed
            Me.Polynomial = &HEDB88320UI
            Me.Initialize()
        End Sub
        Sub New(buffer() As Byte, seed As UInt32, polynomial As UInt32)
            Me.Buffer = buffer
            Me.Seed = seed
            Me.Polynomial = polynomial
            Me.Initialize()
        End Sub
        Public Function ComputeHashToString() As String
            Dim hash As UInt32 = Me.Seed
            For i As Integer = 0 To Buffer.Length - 1
                hash = (hash >> 8) Xor Me.Table(CInt(Buffer(i) Xor hash And &HFF))
            Next
            Return String.Concat(BitConverter.GetBytes(hash).Select(Function(x) x.ToString("X")))
        End Function
        Public Function ComputeHashToBytes() As Byte()
            Dim hash As UInt32 = Me.Seed
            For i As Integer = 0 To Buffer.Length - 1
                hash = (hash >> 8) Xor Me.Table(CInt(Buffer(i) Xor hash And &HFF))
            Next
            Return BitConverter.GetBytes(hash)
        End Function
        Private Sub Initialize()
            Dim buffer() As UInt32 = New UInt32(255) {}
            For i As Integer = 0 To 255
                Dim entry As UInteger = CUInt(i)
                For j As Integer = 0 To 7
                    If (entry And 1) = 1 Then
                        entry = (entry >> 1) Xor Me.Polynomial
                    Else
                        entry = entry >> 1
                    End If
                Next
                buffer(i) = entry
            Next
            Me.Table = buffer
        End Sub
    End Class
End Namespace