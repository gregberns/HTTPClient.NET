Imports System.Text
Imports System.IO

Public Module MiscExtensions
    
    <System.Runtime.CompilerServices.Extension>
    Public Function AsString(buffer As Byte()) As String
        If buffer Is Nothing Then Return ""

        Dim encoding As Encoding = encoding.UTF8

        Return encoding.GetString(buffer, 0, buffer.Length)

    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Iterator Function DistinctBy(Of TSource, TKey)(source As IEnumerable(Of TSource), keySelector As Func(Of TSource, TKey)) As IEnumerable(Of TSource)
        Dim seenKeys As New HashSet(Of TKey)()
        For Each element As TSource In source
            If seenKeys.Add(keySelector(element)) Then
                Yield element
            End If
        Next
    End Function

End Module
