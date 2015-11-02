Imports System.Text
Imports System.Net
Imports System.IO

Public Class Http
    Implements IHttp


    Private Shared ReadOnly _defaultEncoding As Encoding = Encoding.UTF8

    Function Create() As IHttp
        Return New Http
    End Function

    Protected ReadOnly Property HasParameters() As Boolean
        Get
            Return Parameters.Any()
        End Get
    End Property

    Protected ReadOnly Property HasBody() As Boolean
        Get
            Return Not String.IsNullOrEmpty(RequestBody)
        End Get
    End Property


    Public Property Timeout As Integer Implements IHttp.Timeout

    Public Property ResponseWriter As Action(Of IO.Stream) Implements IHttp.ResponseWriter

    Private _Headers As IList(Of HttpHeader)
    Public Property Headers() As IList(Of HttpHeader) Implements IHttp.Headers
        Get
            Return _Headers
        End Get
        Private Set(ByVal value As IList(Of HttpHeader))
            _Headers = value
        End Set
    End Property

    Private _Parameters As IList(Of HttpParameter)
    Public Property Parameters() As IList(Of HttpParameter) Implements IHttp.Parameters
        Get
            Return _Parameters
        End Get
        Private Set(ByVal value As IList(Of HttpParameter))
            _Parameters = value
        End Set
    End Property

    Public Property RequestBody As String Implements IHttp.RequestBody

    Public Property RequestContentType() As String Implements IHttp.RequestContentType

    Public Property Url As Uri Implements IHttp.Url

    Public Property RequestBodyBytes As Byte() Implements IHttp.RequestBodyBytes

    Public Sub New()
        _Headers = New List(Of HttpHeader)
        _Parameters = New List(Of HttpParameter)

        _restrictedHeaderActions = New Dictionary(Of String, Action(Of HttpWebRequest, String))

        AddSharedHeaderActions()

    End Sub

    Partial Private Sub AddSyncHeaderActions()
    End Sub
    Partial Private Sub AddAsyncHeaderActions()
    End Sub
    Private Sub AddSharedHeaderActions()
        _restrictedHeaderActions.Add("Accept", Function(r, v) InlineAssignHelper(r.Accept, v))
        _restrictedHeaderActions.Add("Content-Type", Function(r, v) InlineAssignHelper(r.ContentType, v))

        '' Set by system 
        '_restrictedHeaderActions.Add("Date", Function(r, v)

        '                                        End Function)
        '' Set by system 
        '_restrictedHeaderActions.Add("Host", Function(r, v)

        '                                        End Function)

        '_restrictedHeaderActions.Add("Range", Function(r, v)
        'AddRange(r, v)
        '                                                End Function)

    End Sub

    Private Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function

    Private _restrictedHeaderActions As IDictionary(Of String, Action(Of HttpWebRequest, String))

    ''' handle restricted headers the .NET way - thanks @dimebrain!
    ''' http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.headers.aspx
    Private Sub AppendHeaders(webRequest As HttpWebRequest)
        For Each header As HttpHeader In Headers
            If _restrictedHeaderActions.ContainsKey(header.Name) Then
                _restrictedHeaderActions(header.Name).Invoke(webRequest, header.Value)
            Else
                webRequest.Headers(header.Name) = header.Value
            End If
        Next
    End Sub

    Private Function EncodeParameters()
        Dim querystring = New StringBuilder

        For Each p As HttpParameter In Parameters
            If querystring.Length > 1 Then
                querystring.Append("&")
            End If
            querystring.AppendFormat("{0}={1}", p.Name.UrlEncode, p.Value.UrlEncode)
        Next

        Return querystring.ToString
    End Function

    Private Sub PreparePostBody(webRequest As HttpWebRequest)
        If HasParameters Then
            '  I dont think I like this...
            webRequest.ContentType = "application/x-www-form-urlencoded"
            RequestBody = EncodeParameters()
        ElseIf HasBody Then
            webRequest.ContentType = RequestContentType
        End If
    End Sub

    Private Shared Sub WriteStringTo(stream As Stream, toWrite As String)
        Dim bytes = _defaultEncoding.GetBytes(toWrite)
        stream.Write(bytes, 0, bytes.Length)
    End Sub

    Private Sub ExtractResponseData(response As HttpResponseObj, webResponse As HttpWebResponse)
        Using webResponse
            response.ContentEncoding = webResponse.ContentEncoding

            response.ContentType = webResponse.ContentType
            response.ContentLength = webResponse.ContentLength
            Dim webResponseStream As Stream = webResponse.GetResponseStream
            ProcessResponseStream(webResponseStream, response)

            response.StatusCode = webResponse.StatusCode
            response.StatusDescription = webResponse.StatusDescription

            response.ResponseStatus = ResponseStatus.Completed

            For Each headerName In webResponse.Headers.AllKeys
                Dim headerValue = webResponse.Headers.Get(headerName)
                response.Headers.Add(New HttpHeader With {.Name = headerName, .Value = headerValue})
            Next

            webResponse.Close()

        End Using
    End Sub

    Private Sub ProcessResponseStream(webResponseStream As Stream, response As HttpResponseObj)
        If ResponseWriter Is Nothing Then
            response.RawBytes = webResponseStream.ReadAsBytes
        Else
            ResponseWriter.Invoke(webResponseStream)
        End If
    End Sub




    Public Function [Get]() As HttpResponseObj Implements IHttp.Get
        Return GetStyleMethodInternal("GET")
    End Function

    Public Function GetAs(httpMethod As String) As HttpResponseObj Implements IHttp.AsGet
        Return GetStyleMethodInternal(httpMethod.ToUpper)
    End Function

    Public Function Post() As HttpResponseObj Implements IHttp.Post
        Return PostPutInternal("POST")
    End Function

    Public Function PostAs(httpMethod As String) As HttpResponseObj Implements IHttp.AsPost
        Return PostPutInternal(httpMethod.ToUpper)
    End Function

    Public Function Put() As HttpResponseObj Implements IHttp.Put
        Return PostPutInternal("PUT")
    End Function

    Public Function Delete() As HttpResponseObj Implements IHttp.Delete
        Return GetStyleMethodInternal("DELETE")
    End Function

    Private Function GetStyleMethodInternal(method As String) As HttpResponseObj
        Dim webRequest As HttpWebRequest = ConfigureWebRequest(method, Url)

        If HasBody And (method = "DELETE") Then
            webRequest.ContentType = RequestContentType
            WriteRequestBody(webRequest)
        End If

        Return GetResponse(webRequest)
    End Function

    Private Function PostPutInternal(method As String) As HttpResponseObj
        Dim webRequest As HttpWebRequest = ConfigureWebRequest(method, Url)

        PreparePostData(webRequest)

        WriteRequestBody(webRequest)
        Return GetResponse(webRequest)
    End Function

    Private Sub ExtractErrorResponse(httpResponse As HttpResponseObj, ex As Exception)
        Dim webException = TryCast(ex, WebException)

        If webException IsNot Nothing AndAlso webException.Status = WebExceptionStatus.Timeout Then
            httpResponse.ResponseStatus = ResponseStatus.TimedOut
            httpResponse.ErrorMessage = ex.Message
            httpResponse.ErrorException = webException
            Return
        End If

        httpResponse.ErrorMessage = ex.Message
        httpResponse.ErrorException = ex
        httpResponse.ResponseStatus = ResponseStatus.[Error]
    End Sub

    Private Function GetResponse(request As HttpWebRequest) As HttpResponseObj
        Dim response = New HttpResponseObj() With {
            .ResponseStatus = ResponseStatus.None
        }

        Try
            Dim webResponse = GetRawResponse(request)
            ExtractResponseData(response, webResponse)
        Catch ex As Exception
            ExtractErrorResponse(response, ex)
        End Try

        Return response
    End Function

    Private Shared Function GetRawResponse(request As HttpWebRequest) As HttpWebResponse
        Try
            Return DirectCast(request.GetResponse(), HttpWebResponse)
        Catch ex As WebException
            ' Check to see if this is an HTTP error or a transport error.
            ' In cases where an HTTP error occurs ( status code >= 400 )
            ' return the underlying HTTP response, otherwise assume a
            ' transport exception (ex: connection timeout) and
            ' rethrow the exception

            If TypeOf ex.Response Is HttpWebResponse Then
                Return TryCast(ex.Response, HttpWebResponse)
            End If
            If ex.Status = WebExceptionStatus.Timeout Then
                request.Abort()
                'request = Nothing
            ElseIf ex.Status = WebExceptionStatus.ConnectFailure Then

            End If
            Throw
        End Try
    End Function

    Private Sub PreparePostData(webRequest As HttpWebRequest)
        'If HasFiles OrElse AlwaysMultipartFormData Then
        '    webRequest.ContentType = GetMultipartFormContentType()
        '    Using requestStream = webRequest.GetRequestStream()
        '        WriteMultipartFormData(requestStream)
        '    End Using
        'End If

        PreparePostBody(webRequest)
    End Sub

    Private Sub WriteRequestBody(webRequest As HttpWebRequest)
        If Not HasBody Then
            Return
        End If

        Dim bytes = If(RequestBodyBytes Is Nothing, _defaultEncoding.GetBytes(RequestBody), RequestBodyBytes)

        webRequest.ContentLength = bytes.Length

        Using requestStream = webRequest.GetRequestStream()
            requestStream.Write(bytes, 0, bytes.Length)
        End Using
    End Sub

    ' TODO: Try to merge the shared parts between ConfigureWebRequest and ConfigureAsyncWebRequest (quite a bit of code
    ' TODO: duplication at the moment).
    Private Function ConfigureWebRequest(method As String, url As Uri) As HttpWebRequest
        Dim wRequest = DirectCast(WebRequest.Create(url), HttpWebRequest)

        AppendHeaders(wRequest)

        wRequest.Method = method

        ' make sure Content-Length header is always sent since default is -1
        wRequest.ContentLength = 0

        wRequest.AutomaticDecompression = DecompressionMethods.Deflate Or DecompressionMethods.GZip Or DecompressionMethods.None

        If Timeout <> 0 Then
            wRequest.Timeout = Timeout
        End If

        Return wRequest
    End Function

End Class
