Imports System.Net

Public Class HttpResponseObj
    Implements IHttpResponse

    Sub New()
        Headers = New List(Of HttpHeader)
    End Sub

    Property ContentType As String Implements IHttpResponse.ContentType
    Property ContentLength As Long Implements IHttpResponse.ContentLength
    Property ContentEncoding As String Implements IHttpResponse.ContentEncoding

    Private _Content As String
    Public ReadOnly Property Content() As String Implements IHttpResponse.Content
        Get
            If _Content Is Nothing Then
                _Content = RawBytes.AsString()
            End If
            Return _Content
        End Get
    End Property

    Property StatusCode As HttpStatusCode Implements IHttpResponse.StatusCode

    Property StatusDescription As String Implements IHttpResponse.StatusDescription

    Property RawBytes As Byte() Implements IHttpResponse.RawBytes

    Private _Headers As IList(Of HttpHeader)
    Public Property Headers() As IList(Of HttpHeader)
        Get
            Return _Headers
        End Get
        Private Set(ByVal value As IList(Of HttpHeader))
            _Headers = value
        End Set
    End Property


    Private _ResponseStatus As ResponseStatus
    Public Property ResponseStatus() As ResponseStatus Implements IHttpResponse.ResponseStatus
        Get
            Return _ResponseStatus
        End Get
        Set(ByVal value As ResponseStatus)
            _ResponseStatus = value
        End Set
    End Property

    Public Property ErrorMessage As String Implements IHttpResponse.ErrorMessage

    Public Property ErrorException As Exception Implements IHttpResponse.ErrorException



End Class
