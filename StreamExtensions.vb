Imports System.IO

Public Module StreamExtensions

    <System.Runtime.CompilerServices.Extension> _
    Public Function ReadAsBytes(input As Stream) As Byte()

        If input.GetType Is GetType(MemoryStream) Then
            Return DirectCast(input, MemoryStream).ToArray
        End If

        Dim buffer As Byte() = New Byte(16 * 1024 - 1) {}
        Using ms As New MemoryStream()
            Dim read As Integer
            While (InlineAssignHelper(read, input.Read(buffer, 0, buffer.Length))) > 0
                ms.Write(buffer, 0, read)
            End While
            Return ms.ToArray()
        End Using
    End Function

    Private Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function


    <System.Runtime.CompilerServices.Extension>
    Public Sub CopyStream(input As Stream, output As Stream)

        If input.GetType.BaseType Is GetType(MemoryStream) Then
            Dim bytes As Byte() = New Byte(input.Length - 1) {}
            input.Read(bytes, 0, CInt(input.Length))
            output.Write(bytes, 0, bytes.Length)
            input.Close()
        Else
            Dim buffer As Byte() = New Byte(32767) {}
            Dim read As Integer
            While (read = input.Read(buffer, 0, buffer.Length)) > 0
                output.Write(buffer, 0, read)
            End While
        End If

    End Sub

End Module
