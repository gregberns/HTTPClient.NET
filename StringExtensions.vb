Imports System.Text
Imports System.Web
Imports System.IO

Public Module StringExtensions

    Public Function UrlDecode(input As String) As String
        Return HttpUtility.UrlDecode(input)
    End Function

    ''' <summary>
    ''' Uses Uri.EscapeDataString() based on recommendations on MSDN
    ''' http://blogs.msdn.com/b/yangxind/archive/2006/11/09/don-t-use-net-system-uri-unescapedatastring-in-url-decoding.aspx
    ''' </summary>
    <System.Runtime.CompilerServices.Extension>
    Public Function UrlEncode(input As String) As String
        Const maxLength As Integer = 32766
        If input Is Nothing Then
            Throw New ArgumentNullException("input")
        End If

        If input.Length <= maxLength Then
            Return Uri.EscapeDataString(input)
        End If

        Dim sb As New StringBuilder(input.Length * 2)
        Dim index As Integer = 0
        While index < input.Length
            Dim length As Integer = Math.Min(input.Length - index, maxLength)
            Dim subString As String = input.Substring(index, length)
            sb.Append(Uri.EscapeDataString(subString))
            index += subString.Length
        End While

        Return sb.ToString()
    End Function


    <System.Runtime.CompilerServices.Extension> _
    Public Function HtmlEncode(input As String) As String
        Return System.Web.HttpUtility.HtmlEncode(input)
    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function ToStream(input As String) As Stream

        Return New MemoryStream(Encoding.UTF8.GetBytes(If(input, "")))

    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function ToByteArray(input As String) As Byte()
        Return Encoding.UTF8.GetBytes(If(input, ""))
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function ToBase64(input As String) As String
        Return Convert.ToBase64String(Encoding.Unicode.GetBytes(input))
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function IsNullOrEmpty(input As String) As Boolean
        Return String.IsNullOrEmpty(input)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function IsNotNullOrEmpty(input As String) As Boolean
        Return Not String.IsNullOrEmpty(input)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function Prepend(input As String, value As String) As String
        Return input.Insert(0, value)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function Append(input As String, value As String) As String
        Return input.Insert(input.Length, value)
    End Function

    <System.Runtime.CompilerServices.Extension>
    Public Function Replace(input As String, oldValue As String, newValue As String, comparison As StringComparison) As String
        Dim sb As New StringBuilder()

        Dim previousIndex As Integer = 0
        Dim index As Integer = input.IndexOf(oldValue, comparison)
        While index <> -1
            sb.Append(input.Substring(previousIndex, index - previousIndex))
            sb.Append(newValue)
            index += oldValue.Length

            previousIndex = index
            index = input.IndexOf(oldValue, index, comparison)
        End While
        sb.Append(input.Substring(previousIndex))

        Return sb.ToString()
    End Function

End Module
