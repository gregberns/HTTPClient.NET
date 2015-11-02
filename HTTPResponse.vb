Imports System.Net

Public Class HTTPResponse

    Public Property ResponseException As Exception
    Public Property ResponseSuccess As Boolean = False
    Public Property ResponseStatusCode As HttpStatusCode
    Public Property ResponseBody As String = ""
    Public Property FileType As String = ""
    'Public Property ObjectType As String = ""

End Class
