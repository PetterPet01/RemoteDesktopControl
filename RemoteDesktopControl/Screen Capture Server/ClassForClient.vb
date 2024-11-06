Imports System.Net.Sockets
Imports System.IO
Public Class ClassforClient
    Public Event getMessage(ByVal str As String)
    Public Event clientLogout(ByVal client As ClassforClient)
    Private sendMessage As StreamWriter
    Private listClient As TcpClient
    Public IDName As Integer
    Sub New(ByVal forClient As TcpClient)
        listClient = forClient
        listClient.GetStream.BeginRead(New Byte() {0}, 0, 0, AddressOf ReadAllClient, Nothing)
    End Sub
    Private Sub ReadAllClient()
        Try
            RaiseEvent getMessage(New StreamReader(listClient.GetStream).ReadLine)
            listClient.GetStream.BeginRead(New Byte() {0}, 0, 0, New AsyncCallback(AddressOf ReadAllClient), Nothing)
        Catch ex As Exception
            RaiseEvent clientLogout(Me)
        End Try
    End Sub
    Public Sub Send(ByVal Messsage As String)
        sendMessage = New StreamWriter(listClient.GetStream)
        sendMessage.WriteLine(Messsage)
        sendMessage.Flush()
    End Sub
End Class