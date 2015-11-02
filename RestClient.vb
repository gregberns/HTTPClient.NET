Public Class RestClient
    Implements IRestClient

    Public HttpFactory As IHttpFactory = New SimpleFactory(Of Http)

    Public Sub New()

        AcceptTypes = New List(Of String)
        DefaultParameters = New List(Of Parameter)

        'Add Deserializer Handlers
        [AddHandler]("application/xml")

    End Sub

    Public Sub New(baseUrl As String)
        Me.New()
        Me.BaseUrl = baseUrl
    End Sub

    Private AcceptTypes As IList(Of String)

    Private _DefaultParameters As IList(Of Parameter)
    Public Property DefaultParameters() As IList(Of Parameter) Implements IRestClient.DefaultParameters
        Get
            Return _DefaultParameters
        End Get
        Private Set(ByVal value As IList(Of Parameter))
            _DefaultParameters = value
        End Set
    End Property

    Public Sub [AddHandler](contentType As String)

        If contentType <> "*" Then
            AcceptTypes.Add(contentType)
            Dim accepts = String.Join(", ", AcceptTypes.ToArray())
            Me.RemoveDefaultParameter("Accept")
            Me.AddDefaultParameter("Accept", accepts, ParameterType.HttpHeader)
        End If

    End Sub

    Public Sub RemoveDefaultParameter(name As String)
        Dim parameter = Me.DefaultParameters.SingleOrDefault(Function(p) p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        If parameter IsNot Nothing Then
            Me.DefaultParameters.Remove(parameter)
        End If
    End Sub

    Public Sub AddDefaultParameter(name As String, value As String, type As ParameterType)
        Me.AddDefaultParameter(New Parameter With {.Name = name, .Value = value, .Type = ParameterType.HttpHeader})
    End Sub

    Public Sub AddDefaultParameter(param As Parameter)
        If param.Type = ParameterType.RequestBody Then
            Throw New NotSupportedException("Cannot set request body from default headers. UseRequest.AddBody()")
        End If
        Me.DefaultParameters.Add(param)
    End Sub

    Private _Timeout As Integer
    Public Property Timeout() As Integer Implements IRestClient.Timeout
        Get
            Return _Timeout
        End Get
        Set(ByVal value As Integer)
            _Timeout = value
        End Set
    End Property


    Private _BaseUrl As String
    Public Property BaseUrl() As String
        Get
            Return _BaseUrl
        End Get
        Set(ByVal value As String)
            _BaseUrl = value
            If _BaseUrl IsNot Nothing And _BaseUrl.EndsWith("/") Then
                _BaseUrl = _BaseUrl.Substring(0, _BaseUrl.Length - 1)
            End If
        End Set
    End Property

    Public Function BuildUri(request As IRestRequest)

        Dim assembled As String = request.Resource
        Dim urlParams = request.Parameters.Where(Function(p) p.Type = ParameterType.UrlSegment)
        For Each p As Parameter In urlParams
            If p.Value = Nothing Then
                Throw New ArgumentException(String.Format("Cannot build uri when url segment parameter '{0}' value is null", p.Name), "request")
            End If
            assembled = assembled.Replace("{" + p.Name + "{", p.Value.ToString().UrlEncode())
        Next

        If String.IsNullOrEmpty(assembled) = False And assembled.StartsWith("/") Then
            assembled = assembled.Substring(1)
        End If

        If String.IsNullOrEmpty(BaseUrl) = False Then
            assembled = If(String.IsNullOrEmpty(assembled), Me.BaseUrl, String.Format("{0}/{1}", Me.BaseUrl, assembled))
        End If

        Dim parameters As IEnumerable(Of Parameter)

        If request.Method <> Method.POST AndAlso request.Method <> Method.PUT Then
            ' build and attach querystring if this is a get-style request
            parameters = request.Parameters.Where(Function(p) p.Type = ParameterType.GetOrPost OrElse p.Type = ParameterType.QueryString).ToList()
        Else
            parameters = request.Parameters.Where(Function(p) p.Type = ParameterType.QueryString).ToList()
        End If

        If parameters.Any() = False Then
            Return New Uri(assembled)
        End If

        Dim data = EncodeParameters(parameters)
        Dim separator = If(assembled.Contains("?"), "&", "?")
        assembled = String.Concat(assembled, separator, data)

        Return New Uri(assembled)

    End Function

    Private Function EncodeParameters(parameters As IEnumerable(Of Parameter)) As String
        Dim param = parameters.Select(Function(p) EncodeParameter(p)).ToArray()
        Return String.Join("&", param)
    End Function

    Private Function EncodeParameter(p As Parameter) As String

        If p.Value = Nothing Then
            Return String.Concat(p.Name.UrlEncode(), "=")
        End If

        Return String.Concat(p.Name.UrlEncode(), "=", p.Value.ToString.UrlEncode)

    End Function

    Private Sub ConfigureHTTP(request As IRestRequest, http As IHttp)

        http.ResponseWriter = request.ResponseWriter

        For Each p As Parameter In DefaultParameters
            If request.Parameters.Any(Function(p2) p2.Name = p.Name AndAlso p2.Type = p.Type) Then
                Continue For
            End If

            request.AddParameter(p)
        Next

        If request.Parameters.All(Function(p2) p2.Name.ToLower <> "accept") Then
            Dim accepts = String.Join(", ", AcceptTypes.ToArray())
            request.AddParameter("Accept", accepts, ParameterType.HttpHeader)
        End If

        http.Url = BuildUri(request)

        Dim timeout = If(request.Timeout > 0, request.Timeout, Me.Timeout)
        If timeout > 0 Then
            http.Timeout = timeout
        End If

        Dim headers = From p In request.Parameters
                      Where p.Type = ParameterType.HttpHeader
                      Select New HttpHeader With {.Name = p.Name, .Value = p.Value}

        For Each header As HttpHeader In headers
            http.Headers.Add(header)
        Next

        Dim params = From p In request.Parameters
                     Where p.Type = ParameterType.GetOrPost And p.Value <> Nothing
                     Select New HttpParameter With {.Name = p.Name, .Value = p.Value.ToString}

        For Each p As HttpParameter In params
            http.Parameters.Add(p)
        Next

        Dim body = (From p In request.Parameters
                    Where p.Type = ParameterType.RequestBody
                    Select p).FirstOrDefault

        If body IsNot Nothing Then
            Dim val As Object = body.Value
            If TypeOf val Is Byte() Then
                http.RequestBodyBytes = DirectCast(val, Byte())
            Else
                http.RequestBody = body.Value.ToString
            End If
            http.RequestContentType = body.Name
        End If

    End Sub

    Public Function DownloadData(request As IRestRequest) As Byte() Implements IRestClient.DownloadData
        Dim response = Execute(request)
        Return response.RawBytes
    End Function

    Public Function Execute(request As IRestRequest) As IRestResponse Implements IRestClient.Execute
        Dim m = [Enum].GetName(GetType(Method), request.Method)

        Select Case request.Method
            Case Method.POST, Method.PUT
                Return Execute(request, m, AddressOf DoExecuteAsPost)
            Case Else
                Return Execute(request, m, AddressOf DoExecuteAsGet)
        End Select

    End Function

    Private Function DoExecuteAsGet(http As IHttp, method As String)
        Return http.AsGet(method)
    End Function

    Private Function DoExecuteAsPost(http As IHttp, method As String)
        Return http.AsPost(method)
    End Function

    Private Function Execute(request As IRestRequest, httpMethod As String, getResponse As Func(Of IHttp, String, HttpResponseObj)) As IRestResponse

        Dim response As IRestResponse = New RestResponse

        Try
            Dim http = HttpFactory.Create()

            ConfigureHTTP(request, http)

            response = ConvertToRestResponse(request, getResponse(http, httpMethod))
            response.Request = request

        Catch ex As Exception
            response.ResponseStatus = ResponseStatus.Error
            response.ErrorMessage = ex.Message
            response.ErrorException = ex
        End Try

        Return response
    End Function


    Private Function ConvertToRestResponse(request As IRestRequest, httpResponse As HttpResponseObj) As RestResponse
        Dim restResponse = New RestResponse()
        restResponse.Content = httpResponse.Content
        restResponse.ContentEncoding = httpResponse.ContentEncoding
        restResponse.ContentLength = httpResponse.ContentLength
        restResponse.ContentType = httpResponse.ContentType
        restResponse.ErrorException = httpResponse.ErrorException
        restResponse.ErrorMessage = httpResponse.ErrorMessage
        restResponse.RawBytes = httpResponse.RawBytes
        restResponse.ResponseStatus = httpResponse.ResponseStatus
        'restResponse.ResponseUri = httpResponse.ResponseUri
        'restResponse.Server = httpResponse.Server
        restResponse.StatusCode = httpResponse.StatusCode
        restResponse.StatusDescription = httpResponse.StatusDescription
        restResponse.Request = request

        For Each header As HttpHeader In httpResponse.Headers
            restResponse.Headers.Add(New Parameter() With { _
                .Name = header.Name, _
                .Value = header.Value, _
                .Type = ParameterType.HttpHeader _
            })
        Next

        Return restResponse
    End Function


End Class

Public Interface IHttpFactory
    Function Create() As IHttp
End Interface

Public Class SimpleFactory(Of T As {IHttp, New})
    Implements IHttpFactory
    Public Function Create() As IHttp Implements IHttpFactory.Create
        Return New T()
    End Function
End Class