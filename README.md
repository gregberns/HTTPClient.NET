# HTTPClient.NET


Usage

	Dim req As New RestRequest(Method.GET, URIBuilder.CurrentURL)
	
	
	Dim client as New RestClient(baseurl)
	client.Timeout = 10 * 1000
	
	Dim req As New RestRequest(Method.PUT, URIBuilder.CurrentURL)
	req.AddBody("<document><state>Unused</state></document>")
	
	
	Dim resp As IRestResponse = client.Execute(req)

	Response = New HTTPResponse

	Response.FileType = resp.ContentType
	Response.ResponseBody = resp.Content
	Response.ResponseException = resp.ErrorException
	Response.ResponseStatusCode = resp.StatusCode
	Response.ResponseSuccess = resp.ResponseSuccess

	If Response.ResponseSuccess And Response.ResponseStatusCode = 200 Then
		Return True
	Else
		Return False
	End If