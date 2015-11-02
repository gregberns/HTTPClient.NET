Public Class Parameter

    Private _Name As String
    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
        End Set
    End Property

    Private _Value As String
    Public Property Value() As String
        Get
            Return _Value
        End Get
        Set(ByVal value As String)
            _Value = value
        End Set
    End Property

    Private _Type As ParameterType
    Public Property Type() As ParameterType
        Get
            Return _Type
        End Get
        Set(ByVal value As ParameterType)
            _Type = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Return String.Format("{0}={1}", Name, Value)
    End Function

End Class
