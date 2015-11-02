Imports System.Net

Public Interface IRestResponse

    Property Request As IRestRequest

    Property ContentType As String

    Property ContentLength As Long

    Property ContentEncoding As String

    Property Content As String

    Property StatusCode As HttpStatusCode

    Property StatusDescription As String

    Property RawBytes As Byte()

    Property ResponseUri As Uri

    Property Headers As IList(Of Parameter)

    Property ResponseStatus As ResponseStatus

    Property ErrorMessage As String

    Property ErrorException As Exception

    ReadOnly Property ResponseSuccess As Boolean

End Interface
