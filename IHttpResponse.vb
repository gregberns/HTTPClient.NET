Imports System.Net

Public Interface IHttpResponse

    Property ContentType As String
    Property ContentLength As Long
    Property ContentEncoding As String

    ReadOnly Property Content() As String


    Property StatusCode As HttpStatusCode

    Property StatusDescription As String

    Property RawBytes As Byte()

    Property ResponseStatus() As ResponseStatus
   

    Property ErrorMessage As String

    Property ErrorException As Exception


End Interface
