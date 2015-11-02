Public Interface IRestClient

    Property DefaultParameters As IList(Of Parameter)
    Property Timeout As Integer

    Function DownloadData(request As IRestRequest) As Byte()

    Function Execute(request As IRestRequest) As IRestResponse

End Interface
