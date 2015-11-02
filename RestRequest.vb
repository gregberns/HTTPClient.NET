Imports System.IO

Public Class RestRequest
    Implements IRestRequest


    Public Property ResponseWriter As Action(Of Stream) Implements IRestRequest.ResponseWriter

    Public Sub New()
        Parameters = New List(Of Parameter)
    End Sub

    Public Sub New(HttpMethod As Method)
        Me.New()
        Method = HttpMethod
    End Sub

    Public Sub New(HttpMethod As Method, resource As String)
        Me.New()
        Method = HttpMethod
        Me.Resource = resource
    End Sub

    Public Function AddBody(body As Object, Optional contentType As String = "") As IRestRequest Implements IRestRequest.AddBody

        Dim _contentType As String
        Dim serialized As String

        '' Need to Serialize the object to XML??
        If contentType = "" Then
            Select Case RequestFormat
                Case DataFormat.Xml
                    serialized = body
                    _contentType = "application/xml"
                Case DataFormat.Json
                    serialized = body
                    _contentType = "application/json"
                Case DataFormat.Multipart
                    serialized = body
                    _contentType = " multipart/form-data"
                Case Else
                    serialized = body
                    _contentType = ""
            End Select
        Else
            _contentType = contentType
            serialized = body
        End If

        Return AddParameter(_contentType, serialized, ParameterType.RequestBody)
    End Function

    Public Function AddHeader(name As String, value As String) As IRestRequest Implements IRestRequest.AddHeader
        Return AddParameter(name, value, ParameterType.HttpHeader)
    End Function

    Public Function AddParameter(name As String, value As String, type As ParameterType) As IRestRequest Implements IRestRequest.AddParameter
        Return AddParameter(New Parameter With {.Name = name, .Value = value, .Type = type})
    End Function

    Public Function AddParameter(p As Parameter) As IRestRequest Implements IRestRequest.AddParameter
        Parameters.Add(p)
        Return Me
    End Function


    ''' <summary>
    ''' The Resource URL to make the request against.
    ''' Tokens are substituted with UrlSegment parameters and match by name.
    ''' Should not include the scheme or domain. Do not include leading slash.
    ''' Combined with RestClient.BaseUrl to assemble final URL:
    ''' {BaseUrl}/{Resource} (BaseUrl is scheme + domain, e.g. http://example.com)
    ''' </summary>
    ''' <example>
    ''' // example for url token replacement
    ''' request.Resource = "Products/{ProductId}";
    '''	request.AddParameter("ProductId", 123, ParameterType.UrlSegment);
    ''' </example>
    Private _Resource As String = ""
    Public Property Resource() As String Implements IRestRequest.Resource
        Get
            Return _Resource
        End Get
        Set(ByVal value As String)
            _Resource = value
        End Set
    End Property


    Private _RequestFormat As DataFormat = DataFormat.Xml
    Public Property RequestFormat() As DataFormat Implements IRestRequest.RequestFormat
        Get
            Return _RequestFormat
        End Get
        Set(ByVal value As DataFormat)
            _RequestFormat = value
        End Set
    End Property

    Private _Parameters As List(Of Parameter)
    Public Property Parameters() As List(Of Parameter) Implements IRestRequest.Parameters
        Get
            Return _Parameters
        End Get
        Private Set(ByVal value As List(Of Parameter))
            _Parameters = value
        End Set
    End Property

    Private _method As Method
    Public Property Method() As Method Implements IRestRequest.Method
        Get
            Return _method
        End Get
        Set(ByVal value As Method)
            _method = value
        End Set
    End Property

    Private _Timeout As Integer = 0
    Public Property Timeout() As Integer Implements IRestRequest.Timeout
        Get
            Return _Timeout
        End Get
        Set(ByVal value As Integer)
            _Timeout = value
        End Set
    End Property

End Class
