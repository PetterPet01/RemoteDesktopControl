Imports System.Net
Imports System.IO
Imports System.Net.Sockets
Imports System.Text.Encoding
Public Class Form3
    Dim Listning As TcpListener
    Dim Allclient As TcpClient
    Dim clientList As New List(Of AuthenticationCFC)
    Public clientListCount As Integer
    Dim pReader As StreamReader
    Dim pClient As AuthenticationCFC

    Public NextClientAuthenIndex As Integer
    Dim LastLineStr As String
    Dim MeClosed As Boolean
    Dim CurrentPicSendingClientIndex As Integer

    'UDP Part
    Dim UDPServer As UdpClient = New UdpClient(4398)
    Dim clientEP As IPEndPoint
    Public TurnOffTUDPCTE As Boolean
    Dim onceCheck As Boolean
    Dim hmm As Boolean
    Dim indexToSend As Integer
    Public Sub TellUDPClientToExit()
        For index = 0 To clientList.Count - 1
            If index <> Form2.ListBox1.SelectedIndex Then
                indexToSend = index
                UpdateListIndiv3("Exit Server UDP", True)
            End If
        Next
        Timer2.Stop()
        UDPServer.Close()
        UDPServer = Nothing
        UDPServer = New UdpClient(4398)
        Timer2.Start()
    End Sub
    Public Sub TellUDPClientToExit2()
        indexToSend = Form2.ListBox1.SelectedIndex
        UpdateListIndiv3("Exit Server UDP", True)
        UDPServer.Close()
        UDPServer = New UdpClient(4398)
    End Sub
    Public Sub TellUDPClientToExit3()
        For index = 0 To clientList.Count - 1
            indexToSend = index
            UpdateListIndiv3("Exit Server UDP", True)
        Next
        UDPServer.Close()
        UDPServer = New UdpClient(4398)
    End Sub
    Public Sub TellUDPClientToExit4()
        For index = 0 To clientList.Count - 1
            If index <> Form2.ListBox1.SelectedIndex Then
                indexToSend = index
                UpdateListIndiv3("Exit Server UDP", True)
            End If
        Next
        Timer2.Stop()
        UDPServer.Close()
        UDPServer = Nothing
        UDPServer = New UdpClient(4398)
        Timer5.Start()
    End Sub
    Public Sub ResetUDPServer()
        UDPServer.Close()
        UDPServer = New UdpClient(4398)
    End Sub
    Public Sub TellUDPClientNotToExit()
        indexToSend = Form2.ListBox1.SelectedIndex
        UpdateListIndiv3("Stop Exiting Server", True)
    End Sub
    Public Sub SendClick(MCS As Integer, Holding As Boolean, UnHolded As Boolean)
        CurrentPicSendingClientIndex = Form2.ClientIndexForChangingRes
        Select Case MCS
            Case 4
                If Holding Then
                    UpdateListIndiv2("Left Hold", True)
                Else
                    If UnHolded Then
                        UpdateListIndiv2("Left Unhold", True)
                    Else
                        UpdateListIndiv2("Left Click", True)
                    End If
                End If
            Case 5
                UpdateListIndiv2("Right Click", True)
            Case 6
                UpdateListIndiv2("Middle Click", True)
        End Select
    End Sub

    Public Sub SendScroll(ScrollTimes As Integer)
        CurrentPicSendingClientIndex = Form2.ClientIndexForChangingRes
        If ScrollTimes > 0 Then
            UpdateListIndiv2("Scroll Up: " & ScrollTimes, True)
        ElseIf ScrollTimes < 0 Then
            UpdateListIndiv2("Scroll Down: " & Math.Abs(ScrollTimes), True)
        End If
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Listning = New TcpListener(IPAddress.Any, 4397)
        Listning.Start()
        UpdateList("Server Starting", False)
        Listning.BeginAcceptTcpClient(New AsyncCallback(AddressOf AcceptClient), Listning)
        UDPServer.Client.ReceiveTimeout = 100
        UDPServer.Client.Blocking = False
    End Sub
    ' create a delegate
    Delegate Sub _cUpdate(ByVal str As String, ByVal relay As Boolean)
    Sub UpdateList(ByVal str As String, ByVal relay As Boolean)
        On Error Resume Next
        If InvokeRequired Then
            Invoke(New _cUpdate(AddressOf UpdateList), str, relay)
        Else
            LastLineStr = str
            TextBox1.AppendText(str & vbNewLine)
            ' if relay we will send a string
            If relay Then send(str)
        End If
    End Sub
    Sub send(ByVal str As String)
        For x As Integer = 0 To clientList.Count - 1
            Try
                clientList(x).Send(str)
            Catch ex As Exception
                clientList.RemoveAt(x)
            End Try
        Next
    End Sub

    Sub UpdateListIndiv(ByVal str As String, ByVal relay As Boolean)
        On Error Resume Next
        If InvokeRequired Then
            Invoke(New _cUpdate(AddressOf UpdateListIndiv), str, relay)
        Else
            LastLineStr = str
            TextBox1.AppendText(str & vbNewLine)
            ' if relay we will send a string
            If relay Then sendIndiv(str)
        End If
    End Sub
    Sub sendIndiv(ByVal str As String)
        'Try
        clientList(NextClientAuthenIndex).Send(str)
        'Catch ex As Exception
        '    clientList.RemoveAt(NextClientAuthenIndex)
        'End Try
    End Sub
    Sub UpdateListIndiv2(ByVal str As String, ByVal relay As Boolean)
        On Error Resume Next
        If InvokeRequired Then
            Invoke(New _cUpdate(AddressOf UpdateListIndiv2), str, relay)
        Else
            ' if relay we will send a string
            If relay Then sendIndiv2(str)
        End If
    End Sub

    Sub sendIndiv2(ByVal str As String)
        'Try
        clientList(CurrentPicSendingClientIndex).Send(str)
        'Catch ex As Exception
        '    clientList.RemoveAt(NextClientAuthenIndex)
        'End Try
    End Sub
    Sub UpdateListIndiv3(ByVal str As String, ByVal relay As Boolean)
        On Error Resume Next
        If InvokeRequired Then
            Invoke(New _cUpdate(AddressOf UpdateListIndiv3), str, relay)
        Else
            ' if relay we will send a string
            If relay Then sendIndiv3(str)
        End If
    End Sub
    Sub sendIndiv3(ByVal str As String)
        'Try
        clientList(indexToSend).Send(str)
        'Catch ex As Exception
        '    clientList.RemoveAt(NextClientAuthenIndex)
        'End Try
    End Sub
    Sub AcceptClient(ByVal ar As IAsyncResult)
        If MeClosed = False Then
            pClient = New AuthenticationCFC(Listning.EndAcceptTcpClient(ar))
            AddHandler(pClient.getMessage), AddressOf MessageReceived
            AddHandler(pClient.clientLogout), AddressOf ClientExited
            clientList.Add(pClient)
            UpdateList("New Client Joined!", True)
            Listning.BeginAcceptTcpClient(New AsyncCallback(AddressOf AcceptClient), Listning)
        End If
    End Sub
    Sub MessageReceived(ByVal str As String)
        UpdateList(str, True)
    End Sub
    Sub ClientExited(ByVal client As AuthenticationCFC)
        clientList.Remove(client)
        UpdateList("Client Exited!", True)
        If Form2.Refreshing = False And NextClientAuthenIndex <> Form2.ListBox1.Items.Count Then
            NextClientAuthenIndex -= 1
        End If
    End Sub
    Private Sub TextBox2_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBox2.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            UpdateList("Server says : " & TextBox2.Text, True)
            TextBox2.Clear()
        End If
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        UpdateList("Server says : " & TextBox2.Text, True)
        TextBox2.Clear()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If clientList.Count > 0 Then
            If clientList.Count > NextClientAuthenIndex Then
                UpdateListIndiv("Please Authenticate Client " & NextClientAuthenIndex + 1, True)
                UpdateListIndiv("Please Authenticate Client " & NextClientAuthenIndex + 1, True)
                UpdateListIndiv("Please Authenticate Client " & NextClientAuthenIndex + 1, True)
                If Form2.PhaseCheck = 0 Then Form2.PhaseCheck = 1
            Else
            End If
        End If
        clientListCount = clientList.Count
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        UpdateListIndiv("HI", True)
    End Sub
    Private Sub Form3_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        MeClosed = True
        Listning.Stop()
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If clientEP Is Nothing Then
        Else
            Me.Text = clientEP.ToString
        End If
        Try
            Dim ep As IPEndPoint = New IPEndPoint(IPAddress.Any, 5823)
            Dim rcvbytes() As Byte = UDPServer.Receive(ep)
            Dim RcvedMsg As String = ASCII.GetString(rcvbytes)
            clientEP = ep
            Me.Text = ep.ToString
        Catch ex As Exception
        End Try
        If Form2.ChangingResolution Then
            CurrentPicSendingClientIndex = Form2.ClientIndexForChangingRes
            UpdateListIndiv2("Change Resolution: " & Form2.ResolutionValue, True)

        End If
        If Form2.SendMouseLocation Then
            'UpdateListIndiv2("MP: [ " & Form2.ClientMouseX & "] { " & Form2.ClientMouseY & "}", True)
            'MsgBox("Yey")
            Try
                Dim sendbytes() As Byte = ASCII.GetBytes("MP: [ " & Form2.ClientMouseX & "] { " & Form2.ClientMouseY & "}")
                UDPServer.Send(sendbytes, sendbytes.Length, clientEP)
            Catch ex As Exception
                'Me.Text = ex.Message
            End Try
        End If
    End Sub

    Private Sub Timer5_Tick(sender As Object, e As EventArgs) Handles Timer5.Tick
        Timer2.Start()
        Timer5.Stop()
    End Sub

    'Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
    '    If Form2.TellUDPClientToExit And TurnOffTUDPCTE = False Then
    '        If onceCheck = False Then
    '            Timer4.Start()
    '            onceCheck = True
    '        End If
    '        If Timer4.Enabled Then
    '            If hmm = False Then
    '                If clientEP IsNot Nothing And clientEP.Address.ToString <> "0.0.0.0" Then
    '                    Dim sendbytes() As Byte = ASCII.GetBytes("Exit Server")
    '                    UDPServer.Send(sendbytes, sendbytes.Length, clientEP)
    '                End If
    '            End If
    '        End If
    '    End If
    'End Sub

    'Private Sub Timer4_Tick(sender As Object, e As EventArgs) Handles Timer4.Tick
    '    hmm = True
    '    If TurnOffTUDPCTE = False Then
    '        UDPServer.Close()
    '        UDPServer = New UdpClient(4398)
    '    Else
    '        Timer4.Stop()
    '        TurnOffTUDPCTE = False
    '        Timer3.Stop()
    '        onceCheck = False
    '        hmm = False
    '    End If
    '    'Form2.TellUDPClientToExit = False
    '    If Form2.TellUDPClientToExit And onceCheck = True Then
    '        TurnOffTUDPCTE = True
    '    End If
    'End Sub
End Class