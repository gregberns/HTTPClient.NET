Imports System.Net

Public Class RestResponse
    Implements IRestResponse
    Private _content As String

    ''' <summary>
    ''' Default constructor
    ''' </summary>
    Public Sub New()
        Headers = New List(Of Parameter)()
        'Cookies = New List(Of RestResponseCookie)()
    End Sub
    ''' <summary>
    ''' The RestRequest that was made to get this RestResponse
    ''' </summary>
    ''' <remarks>
    ''' Mainly for debugging if ResponseStatus is not OK
    ''' </remarks> 
    Public Property Request() As IRestRequest Implements IRestResponse.Request
        Get
            Return m_Request
        End Get
        Set(value As IRestRequest)
            m_Request = value
        End Set
    End Property
    Private m_Request As IRestRequest
    ''' <summary>
    ''' MIME content type of response
    ''' </summary>
    Public Property ContentType() As String Implements IRestResponse.ContentType
        Get
            Return m_ContentType
        End Get
        Set(value As String)
            m_ContentType = value
        End Set
    End Property
    Private m_ContentType As String
    ''' <summary>
    ''' Length in bytes of the response content
    ''' </summary>
    Public Property ContentLength() As Long Implements IRestResponse.ContentLength
        Get
            Return m_ContentLength
        End Get
        Set(value As Long)
            m_ContentLength = value
        End Set
    End Property
    Private m_ContentLength As Long
    ''' <summary>
    ''' Encoding of the response content
    ''' </summary>
    Public Property ContentEncoding() As String Implements IRestResponse.ContentEncoding
        Get
            Return m_ContentEncoding
        End Get
        Set(value As String)
            m_ContentEncoding = value
        End Set
    End Property
    Private m_ContentEncoding As String
    ''' <summary>
    ''' String representation of response content
    ''' </summary>
    Public Property Content() As String Implements IRestResponse.Content
        Get
            If _content Is Nothing Then
                _content = RawBytes.AsString()
            End If
            Return _content
        End Get
        Set(value As String)
            _content = value
        End Set
    End Property
    ''' <summary>
    ''' HTTP response status code
    ''' </summary>
    Public Property StatusCode() As HttpStatusCode Implements IRestResponse.StatusCode
        Get
            Return m_StatusCode
        End Get
        Set(value As HttpStatusCode)
            m_StatusCode = value
        End Set
    End Property
    Private m_StatusCode As HttpStatusCode
    ''' <summary>
    ''' Description of HTTP status returned
    ''' </summary>
    Public Property StatusDescription() As String Implements IRestResponse.StatusDescription
        Get
            Return m_StatusDescription
        End Get
        Set(value As String)
            m_StatusDescription = value
        End Set
    End Property
    Private m_StatusDescription As String
    ''' <summary>
    ''' Response content
    ''' </summary>
    Public Property RawBytes() As Byte() Implements IRestResponse.RawBytes
        Get
            Return m_RawBytes
        End Get
        Set(value As Byte())
            m_RawBytes = value
        End Set
    End Property
    Private m_RawBytes As Byte()
    ''' <summary>
    ''' The URL that actually responded to the content (different from request if redirected)
    ''' </summary>
    Public Property ResponseUri() As Uri Implements IRestResponse.ResponseUri
        Get
            Return m_ResponseUri
        End Get
        Set(value As Uri)
            m_ResponseUri = value
        End Set
    End Property
    Private m_ResponseUri As Uri
    ''' <summary>
    ''' HttpWebResponse.Server
    ''' </summary>
    'Public Property Server() As String Implements IRestResponse.s
    '    Get
    '        Return m_Server
    '    End Get
    '    Set(value As String)
    '        m_Server = value
    '    End Set
    'End Property
    'Private m_Server As String
    ''' <summary>
    ''' Cookies returned by server with the response
    ''' </summary>
    'Public Property Cookies() As IList(Of RestResponseCookie)
    '    Get
    '        Return m_Cookies
    '    End Get
    '    Protected Friend Set(value As IList(Of RestResponseCookie))
    '        m_Cookies = Value
    '    End Set
    'End Property
    'Private m_Cookies As IList(Of RestResponseCookie)
    ''' <summary>
    ''' Headers returned by server with the response
    ''' </summary>
    Public Property Headers() As IList(Of Parameter) Implements IRestResponse.Headers
        Get
            Return m_Headers
        End Get
        Protected Friend Set(value As IList(Of Parameter))
            m_Headers = value
        End Set
    End Property
    Private m_Headers As IList(Of Parameter)

    Private _responseStatus As ResponseStatus = ResponseStatus.None
    ''' <summary>
    ''' Status of the request. Will return Error for transport errors.
    ''' HTTP errors will still return ResponseStatus.Completed, check StatusCode instead
    ''' </summary>
    Public Property ResponseStatus() As ResponseStatus Implements IRestResponse.ResponseStatus
        Get
            Return _responseStatus
        End Get
        Set(value As ResponseStatus)
            _responseStatus = value
        End Set
    End Property

    ''' <summary>
    ''' Transport or other non-HTTP error generated while attempting request
    ''' </summary>
    Public Property ErrorMessage() As String Implements IRestResponse.ErrorMessage
        Get
            Return m_ErrorMessage
        End Get
        Set(value As String)
            m_ErrorMessage = value
        End Set
    End Property
    Private m_ErrorMessage As String

    ''' <summary>
    ''' The exception thrown during the request, if any
    ''' </summary>
    Public Property ErrorException() As Exception Implements IRestResponse.ErrorException
        Get
            Return m_ErrorException
        End Get
        Set(value As Exception)
            m_ErrorException = value
        End Set
    End Property
    Private m_ErrorException As Exception

    Public ReadOnly Property ResponseSuccess() As Boolean Implements IRestResponse.ResponseSuccess
        Get
            If ResponseStatus <> ResponseStatus.Completed Then
                Return False
            End If

            If ErrorMessage <> "" Or ErrorMessage IsNot Nothing Then
                Return False
            End If

            Return True
        End Get
    End Property

End Class

'Public Class RestResponse(Of T)
'    Inherits RestResponseBase
'    Implements IRestResponse(Of T)

'    ''' <summary>
'    ''' Deserialized entity data
'    ''' </summary>
'    Public Property Data() As T Implements IRestResponse(Of T).Data
'        Get
'            Return m_Data
'        End Get
'        Set(value As T)
'            m_Data = value
'        End Set
'    End Property
'    Private m_Data As T

'    Public Shared Narrowing Operator CType(response As RestResponse) As RestResponse(Of T)
'        Return New RestResponse(Of T)() With { _
'             .ContentEncoding = response.ContentEncoding, _
'             .ContentLength = response.ContentLength, _
'             .ContentType = response.ContentType, _
'             .ErrorMessage = response.ErrorMessage, _
'             .ErrorException = response.ErrorException, _
'             .Headers = response.Headers, _
'             .RawBytes = response.RawBytes, _
'             .ResponseStatus = response.ResponseStatus, _
'             .ResponseUri = response.ResponseUri, _
'             .Server = response.Server, _
'             .StatusCode = response.StatusCode, _
'             .StatusDescription = response.StatusDescription, _
'             .Request = response.Request _
'        }
'    End Operator

'    Public Overloads Property Content As String Implements IRestResponse.Content

'    Public Overloads Property ContentEncoding As String Implements IRestResponse.ContentEncoding

'    Public Overloads Property ContentLength As Long Implements IRestResponse.ContentLength

'    Public Overloads Property ContentType As String Implements IRestResponse.ContentType

'    Public Overloads Property ErrorException As Exception Implements IRestResponse.ErrorException

'    Public Overloads Property ErrorMessage As String Implements IRestResponse.ErrorMessage

'    Public Overloads Property Headers As IList(Of Parameter) Implements IRestResponse.Headers

'    Public Overloads Property RawBytes As Byte() Implements IRestResponse.RawBytes

'    Public Overloads Property Request As IRestRequest Implements IRestResponse.Request

'    Public Overloads Property ResponseStatus As ResponseStatus Implements IRestResponse.ResponseStatus

'    Public Overloads Property ResponseUri As Uri Implements IRestResponse.ResponseUri

'    Public Overloads Property StatusCode As HttpStatusCode Implements IRestResponse.StatusCode

'    Public Overloads Property StatusDescription As String Implements IRestResponse.StatusDescription


'End Class

'Public Class RestResponseOther
'    Inherits RestResponseBase
'    Implements IRestResponse

'    Sub New()
'        'MyBase.New()

'    End Sub

'    Public Overloads Property Content As String Implements IRestResponse.Content

'    Public Overloads Property ContentEncoding As String Implements IRestResponse.ContentEncoding

'    Public Overloads Property ContentLength As Long Implements IRestResponse.ContentLength

'    Public Overloads Property ContentType As String Implements IRestResponse.ContentType

'    Public Overloads Property ErrorException As Exception Implements IRestResponse.ErrorException

'    Public Overloads Property ErrorMessage As String Implements IRestResponse.ErrorMessage

'    Public Overloads Property Headers As IList(Of Parameter) Implements IRestResponse.Headers

'    Public Overloads Property RawBytes As Byte() Implements IRestResponse.RawBytes

'    Public Overloads Property Request As IRestRequest Implements IRestResponse.Request

'    Public Overloads Property ResponseStatus As ResponseStatus Implements IRestResponse.ResponseStatus

'    Public Overloads Property ResponseUri As Uri Implements IRestResponse.ResponseUri

'    Public Overloads Property StatusCode As HttpStatusCode Implements IRestResponse.StatusCode

'    Public Overloads Property StatusDescription As String Implements IRestResponse.StatusDescription

'End Class
