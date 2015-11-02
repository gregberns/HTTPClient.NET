Imports System.IO

Public Interface IHttp

    Property ResponseWriter As Action(Of Stream)

    Property Timeout As Integer

    Property Headers As IList(Of HttpHeader)
    Property Parameters As IList(Of HttpParameter)

    Property RequestBody As String
    Property RequestContentType As String
    Property RequestBodyBytes As Byte()

    Property Url As Uri

    Function [Get]() As HttpResponseObj
    Function Post() As HttpResponseObj
    Function Put() As HttpResponseObj
    Function Delete() As HttpResponseObj
    Function AsGet(httpMethod As String) As HttpResponseObj
    Function AsPost(httpMethod As String) As HttpResponseObj


End Interface
