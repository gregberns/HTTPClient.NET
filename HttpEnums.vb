Public Enum Method
    [GET]
    POST
    PUT
    DELETE
End Enum

Public Enum ParameterType
    Cookie
    GetOrPost
    UrlSegment
    HttpHeader
    RequestBody
    QueryString
End Enum

Public Enum DataFormat
    Xml
    Json
    Multipart
End Enum

Public Enum ResponseStatus
    None
    Completed
    [Error]
    TimedOut
    Aborted
End Enum