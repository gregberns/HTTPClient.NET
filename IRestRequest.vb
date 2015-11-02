Imports System.IO

Public Interface IRestRequest


    Property ResponseWriter As Action(Of Stream)

    Property Parameters As List(Of Parameter)

    Property Method As Method

    Property Resource As String

    Property RequestFormat As DataFormat

    Property Timeout As Integer

    Function AddBody(body As Object, Optional contentType As String = "") As IRestRequest

    Function AddParameter(name As String, value As String, type As ParameterType) As IRestRequest

    Function AddParameter(parameter As Parameter) As IRestRequest

    Function AddHeader(name As String, value As String) As IRestRequest

End Interface
