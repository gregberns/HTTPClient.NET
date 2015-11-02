Public Class HttpHeader

    Public Property Name As String

    Public Property Value As String

    Public Overrides Function ToString() As String
        Return String.Format("{0}: {1}", Name, Value)
    End Function

End Class
