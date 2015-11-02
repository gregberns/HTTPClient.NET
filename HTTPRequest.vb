Imports System.Net
Imports System.IO

Public Class HTTPRequest

    Private resp As HTTPResponse
    Private _Timeout As Integer = 60 'Seconds

    Public Enum AcceptTypes
        none
        xml
        json
        html
        mhtml
        textxml
        textplain
        native
        pdf
        xls
        xlsx
        csv
        png

    End Enum

    Sub New()
        _DefaultContentType = ""
    End Sub

    Sub New(DefaultAcceptHeader As AcceptTypes, Timeout As Integer)
        _DefaultAcceptHeader = DefaultAcceptHeader
        _DefaultContentType = DefaultAcceptHeader
        _Timeout = Timeout
    End Sub

#Region "Headers"

    Public AcceptHeader As AcceptTypes
    Public AuthHeader As String
    Public ContentTypeHeader As AcceptTypes
    Public OtherHeaders As New List(Of String)
    Public OutputFileName As String

    Private _BodyContent As String
    Public ReadOnly Property BodyContent As String
        Get
            Return _BodyContent
        End Get
    End Property

    Private _DefaultAcceptHeader As AcceptTypes
    Public ReadOnly Property DefaultAcceptHeader As AcceptTypes
        Get
            Return _DefaultAcceptHeader
        End Get
    End Property

    Private _DefaultContentType As String
    Public ReadOnly Property DefaultContentType As AcceptTypes
        Get
            Return _DefaultContentType
        End Get
    End Property

    'GET, POST, PUT, DELETE
    Private _MethodType As String
    Public ReadOnly Property MethodType As String
        Get
            Return _MethodType
        End Get
    End Property

    Private _URL As String
    Public ReadOnly Property URL As String
        Get
            Return _URL
        End Get
    End Property

#End Region

    Private Function GetAcceptType(ByVal accept As AcceptTypes) As String
        Select Case accept
            Case AcceptTypes.none
                Return Nothing
            Case AcceptTypes.xml
                Return "application/xml"
            Case AcceptTypes.json
                Return "application/json"
            Case AcceptTypes.html
                Return "text/html"
            Case AcceptTypes.mhtml
                Return "multi-part/related"
            Case AcceptTypes.textxml
                Return "text/xml"
            Case AcceptTypes.textplain
                Return "text/plain"
            Case AcceptTypes.native
                Return "xop+xml"
            Case AcceptTypes.pdf
                Return "application/pdf"
            Case AcceptTypes.xls
                Return "application/vnd.ms-excel"
            Case AcceptTypes.xlsx
                Return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            Case AcceptTypes.png
                Return "image/png"
            Case Else
                Throw New Exception("AcceptType doesn't exist")
        End Select
    End Function

    Public Function MIMETypeToString(ByVal accept As AcceptTypes)
        Return GetAcceptType(accept)
    End Function

    Public Function GETReq(ByVal URL As String)
        _MethodType = "GET"
        _URL = URL
        Return Send()
    End Function

    Public Function POST(ByVal URL As String, Body As String)
        _MethodType = "POST"
        _URL = URL
        _BodyContent = Body
        Return Send()
    End Function

    Public Function PUT(ByVal URL As String, Body As String)
        _MethodType = "PUT"
        _URL = URL
        _BodyContent = Body
        Return Send()
    End Function

    Public Function DELETE(ByVal URL As String)
        _MethodType = "DELETE"
        _URL = URL
        Return Send()
    End Function

    Private Function Send() As HTTPResponse
        resp = New HTTPResponse
        'Set Defaults
        Prepare()

        Request()

        'Clean up
        Close()

        Return resp
    End Function

    Private Sub Prepare()

        Select Case _MethodType
            Case "GET"
                If AcceptHeader = Nothing Then
                    AcceptHeader = DefaultAcceptHeader
                End If
            Case "POST"
                If ContentTypeHeader = AcceptTypes.none Then
                    ContentTypeHeader = DefaultContentType
                End If
            Case "PUT"
                If ContentTypeHeader = AcceptTypes.none Then
                    ContentTypeHeader = DefaultContentType
                End If
            Case "DELETE"

        End Select

    End Sub

    Private Sub Close()

        ContentTypeHeader = Nothing
        AcceptHeader = Nothing

    End Sub

    Private Function Request() As Boolean
        Dim responseData As String = ""
        Dim response As HttpWebResponse = Nothing
        Dim hwrequest As HttpWebRequest = Nothing
        Try
            hwrequest = WebRequest.Create(_URL)

            hwrequest.AllowAutoRedirect = True
            hwrequest.UserAgent = "http_requester/0.1"
            hwrequest.Timeout = _Timeout * 1000 'In milliseconds
            hwrequest.Method = _MethodType

            For Each h In OtherHeaders
                hwrequest.Headers.Add(h)
            Next

            Select Case _MethodType
                Case "GET"
                    hwrequest.Accept = GetAcceptType(AcceptHeader)
                Case "POST"
                    hwrequest.ContentType = GetAcceptType(ContentTypeHeader)
                Case "PUT"
                    hwrequest.ContentType = GetAcceptType(ContentTypeHeader)
            End Select

            Select Case _MethodType
                Case "POST"
                    Dim encoding As New Text.ASCIIEncoding() 'Use UTF8Encoding for XML requests
                    Dim postByteArray() As Byte = encoding.GetBytes(_BodyContent)
                    hwrequest.ContentLength = postByteArray.Length
                    Dim postStream As IO.Stream = hwrequest.GetRequestStream()
                    postStream.Write(postByteArray, 0, postByteArray.Length)
                    postStream.Close()
                Case "PUT"
                    Dim encoding As New Text.ASCIIEncoding() 'Use UTF8Encoding for XML requests
                    Dim postByteArray() As Byte = encoding.GetBytes(BodyContent)
                    hwrequest.ContentLength = postByteArray.Length
                    Dim postStream As IO.Stream = hwrequest.GetRequestStream()
                    postStream.Write(postByteArray, 0, postByteArray.Length)
                    postStream.Close()
            End Select

            Try
                response = hwrequest.GetResponse()
                resp.ResponseSuccess = True
            Catch ex As WebException
                resp.ResponseSuccess = False

                If ex.Response Is Nothing Then
                    resp.ResponseException = ex
                Else
                    response = ex.Response
                End If
            End Try

            'If there was no Response 
            If resp.ResponseException IsNot Nothing Then
                Dim ex As WebException = resp.ResponseException
                resp.ResponseBody = ex.Message
                If ex.Status = WebExceptionStatus.Timeout Then
                    resp.ResponseStatusCode = HttpStatusCode.RequestTimeout
                End If
                Return False
            End If

            resp.ResponseStatusCode = response.StatusCode
            resp.FileType = response.ContentType

            'Non-OK message returned
            If resp.ResponseStatusCode <> HttpStatusCode.OK Then
                resp.ResponseBody = SysFxns.ReadStream(response.GetResponseStream())
                If resp.ResponseBody = "" Then
                    resp.ResponseBody = String.Format("HTTP Error: {0}({1})", response.StatusDescription, CInt(response.StatusCode))
                    resp.ResponseStatusCode = CInt(response.StatusCode)
                End If
                Return False
            End If

            Select Case response.ContentType
                Case "application/xml",
                    "application/atom+xml;type=entry",
                    "text/xml",
                    "text/html",
                    "text/plain"
                    resp.ResponseBody = SysFxns.ReadStream(response.GetResponseStream())
                Case "application/json"
                    resp.ResponseBody = SysFxns.ReadStream(response.GetResponseStream())
                Case "application/pdf",
                    "application/vnd.ms-excel",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    Try
                        SysFxns.SaveFile(OutputFileName, response.GetResponseStream())
                    Catch ex As Exception
                        resp.ResponseBody = "Internal IB Error: Could not save File:: " + ex.Message
                        Return False
                    End Try
                Case "image/png"
                    SysFxns.SaveFile(OutputFileName + ".png", response.GetResponseStream())
                    resp.FileType = "image/png"
                Case Else
                    Throw New Exception("ContentType not defined: " + response.ContentType)
            End Select

            Return True

        Catch ex As WebException
            If ex.Response Is Nothing Then
                resp.ResponseException = ex
                Return False
            End If
            Dim rr As HttpWebResponse = ex.Response
            resp.ResponseStatusCode = rr.StatusCode

            'Response code here on open document can be 404, doesn't exist

            Try
                Using stream = ex.Response.GetResponseStream()
                    Using reader = New StreamReader(stream)
                        resp.ResponseBody = reader.ReadToEnd()
                    End Using
                End Using
            Catch e As Exception

                Return False
            End Try

            Return False
        Catch e As Exception
            'Return responseData = "An error occurred: " & e.Message
            resp.ResponseBody = e.Message
            resp.ResponseException = e
            If response IsNot Nothing Then resp.ResponseStatusCode = response.StatusCode
            Return False
        Finally
            Try
                hwrequest.Abort()
                hwrequest = Nothing
                response.Close()
            Catch ex As Exception
            End Try
        End Try
    End Function


End Class
