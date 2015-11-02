Imports System.IO

Public Class SysFxns

    Public Shared Sub SaveFile(Path As String, str As Stream)
        Dim ba(1024) As Byte
        Dim fs As New FileStream(Path, FileMode.Create)
        Dim br As Integer
        Do
            br = str.Read(ba, 0, ba.Length)
            If br > 0 Then
                fs.Write(ba, 0, br)
            Else
                Exit Do
            End If
        Loop
        fs.Close()
    End Sub

    Public Shared Sub SaveFile(Path As String, buff As String)
        Dim fs As FileStream
        Dim sw As StreamWriter
        fs = New FileStream(Path, FileMode.Create)
        sw = New StreamWriter(fs, System.Text.Encoding.UTF8)
        sw.Write(buff)
        sw.Close()
        fs.Close()
    End Sub

    Public Shared Function ReadStream(_Stream As Stream) As String
        Dim buff As String = ""
        Dim responseStream As IO.StreamReader = New IO.StreamReader(_Stream)
        buff = responseStream.ReadToEnd()
        responseStream.Close()
        Return buff
    End Function

    Public Shared Function StringListToFolderPath(folders As List(Of String)) As String
        Dim path As String = ""

        If folders.Count = 1 Then
            Return folders(0)
        End If

        path = folders(0)

        For i = 1 To folders.Count - 1
            path = String.Format("{0}{1}{2}", folders(i), "/", path)
        Next

        Return path
    End Function


End Class
