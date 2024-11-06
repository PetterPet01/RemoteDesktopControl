Imports System.Net
Imports System.IO
Imports System.Net.Sockets
Imports System.ComponentModel
Imports System.Runtime.InteropServices


Public Class Form2
    Dim Listning As TcpListener
    Dim Allclient As TcpClient
    Dim clientList As New List(Of ClassforClient)
    Dim pReader As StreamReader
    Dim pClient As ClassforClient

    Dim OCPic As New List(Of PictureBox)
    Dim OnlineList As New List(Of String)

    Dim LastLineStr As String
    Public PhaseCheck As Integer
    Dim IDJoined As String
    Dim IsReceiving As Boolean
    Dim ClientExitedCheck As Boolean
    Dim ClientExitID As String
    Dim ClientExitIDIndex As Integer
    Public Refreshing As Boolean
    Dim MeClosed As Boolean
    Dim ResPercentValue As New List(Of Integer)
    Public ChangingResolution As Boolean
    Public ClientIndexForChangingRes As Integer
    Public ResolutionValue As Integer = 50
    Dim CurrentPicSendingID As String

    Dim wRatio As Decimal
    Dim hRatio As Decimal
    Public ClientMouseX As Integer
    Public ClientMouseY As Integer
    Dim ClientScreenResolutionW As New List(Of Integer)
    Dim ClientScreenResolutionH As New List(Of Integer)

    Public SendMouseLocation As Boolean
    Public TellUDPClientToExit As Boolean
    Dim MouseClickState As Integer
    Dim MouseDownTick As Integer
    Dim isMouseHolding As Boolean
    Dim ScrollTimes As Integer
    Dim MouseSendable As Boolean
    <DllImport("user32.dll", EntryPoint:="GetAsyncKeyState")> Public Shared Function GetAsyncKeyState(ByVal vKey As Integer) As Short
    End Function

    Private Function ConvertGAKSToString() As String
        Dim kc As New KeysConverter
        Dim str As String
        For i = 0 To 255
            If (GetAsyncKeyState(i) <> 0) Then
                str = kc.ConvertToString(i)
            End If
        Next
        Return str
    End Function
    Private Function ConvertGAKSToInteger() As Integer
        Dim int As Integer
        For i = 0 To 255
            If (GetAsyncKeyState(i) <> 0) Then
                int = i
            End If
        Next
        Return int
    End Function
    Private Sub CreateOCPic(w As Integer, h As Integer, lOffset As Integer, tOffset As Integer)
        Dim OcPicc As New PictureBox
        With OcPicc
            .Width = w
            .Height = h
            .BackColor = Color.Red
            .Left = 0 + lOffset
            .Top = ListBox1.Top + (tOffset * OCPic.Count)
            .Show()
            .BringToFront()
        End With
        Me.SplitContainer4.Panel2.Controls.Add(OcPicc)
        OCPic.Add(OcPicc)
        OCPic(OCPic.Count - 1).BringToFront()
    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Listning = New TcpListener(IPAddress.Any, 4396)
        Listning.Start()
        UpdateList("Server Starting", False)
        Listning.BeginAcceptTcpClient(New AsyncCallback(AddressOf AcceptClient), Listning)
        Form3.Show()
        Form3.Hide()
    End Sub
    ' create a delegate
    Delegate Sub _cUpdate(ByVal str As String, ByVal relay As Boolean)
    Sub UpdateList(ByVal str As String, ByVal relay As Boolean)
        On Error Resume Next
        If InvokeRequired Then
            Invoke(New _cUpdate(AddressOf UpdateList), str, relay)
        Else
            TextBox1.AppendText(str & vbNewLine)
            LastLineStr = str
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
    Sub AcceptClient(ByVal ar As IAsyncResult)
        If MeClosed = False Then
            pClient = New ClassforClient(Listning.EndAcceptTcpClient(ar))
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
    Sub ClientExited(ByVal client As ClassforClient)
        clientList.Remove(client)
        UpdateList("Client Exited!", True)

        ClientExitID = client.IDName
        Dim OnlineListCEIDIndex As Integer = OnlineList.IndexOf(ClientExitID)
        'If OnlineList.Count > 0 And OnlineListCEIDIndex + 1 <= OnlineList.Count Then
        '    OnlineList(OnlineListCEIDIndex) = ""
        'End If
        Try
            If OnlineList.Count > 0 Then
                OnlineList(OnlineListCEIDIndex) = ""
            End If
            ClientScreenResolutionW.Remove(ClientExitID)
        Catch ex As Exception
        End Try


        'timerCheckClientAlive = New Timer

        'AddHandler timerCheckClientAlive.Tick, AddressOf SubTimerCheckClientAlive
        'timerCheckClientAlive.Interval = 500
        'timerCheckClientAlive.Start()

        ClientExitedCheck = True
        'Form3.NextClientAuthenIndex -= 1
        ''If Refreshing = False Then
        ''End If
        PhaseCheck = 1
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
        If LastLineStr <> "" Then
            If PhaseCheck = 1 Then
                If LastLineStr.Contains("Joined ID") Then
                    Dim IDStart As Integer = LastLineStr.IndexOf(":") + 2
                    IDJoined = LastLineStr.Substring(IDStart, LastLineStr.Length - IDStart)
                    'Me.Text = IDJoined
                    PhaseCheck = 2
                    UpdateList("Server says | Check ID: " & IDJoined, True)
                End If

            End If
            If PhaseCheck = 2 Then
                If LastLineStr.Contains("Check OK") Then
                    If Not ComboBox1.Items.Contains(IDJoined) Then
                        ComboBox1.Items.Add(IDJoined)
                    End If

                    If ListBox1.SelectedIndex < 0 Then
                        If Refreshing Then
                            'ComboBox1.Text = ComboBox1.Items(ClientExitIDIndex).ToString
                            ListBox1.SetSelected(ClientExitIDIndex, True)
                            ClientExitIDIndex = 0
                            Refreshing = False
                            TellUDPClientToExit = False
                        Else
                            'ComboBox1.Text = ComboBox1.Items(0).ToString
                            ListBox1.SetSelected(0, True)
                        End If
                    End If
                    clientList(clientList.Count - 1).IDName = IDJoined
                    If IsReceiving = False Then
                        UpdateList("Server says: Send Screen | " & IDJoined, True)
                        Form1.Show()
                        Form1.Hide()
                        For index = 1 To 10
                            'LastLineStr = ""
                            PhaseCheck = 0
                            IDJoined = ""
                            IsReceiving = True
                        Next
                        If Form3.NextClientAuthenIndex <= Form3.clientListCount - 1 Then
                            Form3.NextClientAuthenIndex += 1
                        End If
                        'ElseIf IDJoined = ComboBox1.Text And IsReceiving = True Then
                    ElseIf IDJoined = ListBox1.SelectedItem.ToString And IsReceiving = True Then
                        UpdateList("Server says: Send Screen | " & IDJoined, True)
                        Form1.Show()
                        Form1.Hide()
                        For index = 1 To 10
                            'LastLineStr = ""
                            PhaseCheck = 0
                            IDJoined = ""
                            IsReceiving = True
                        Next
                        If Form3.NextClientAuthenIndex <= Form3.clientListCount - 1 Then
                            Form3.NextClientAuthenIndex += 1
                        End If
                        'ElseIf IDJoined <> ComboBox1.Text And IsReceiving = True Then
                    ElseIf IDJoined <> ListBox1.SelectedItem.ToString And IsReceiving = True Then
                        For index = 1 To 10
                            'LastLineStr = ""
                            PhaseCheck = 0
                            IDJoined = ""
                        Next
                        If Form3.NextClientAuthenIndex <= Form3.clientListCount - 1 Then
                            Form3.NextClientAuthenIndex += 1
                        End If
                    End If
                ElseIf LastLineStr.Contains("Check Not OK") Then
                    PhaseCheck = 1
                End If
                If LastLineStr.Contains("Check ID") Then
                    UpdateList("Server says | Check ID: " & IDJoined, True)
                End If
            End If
            'If LastLineStr.Contains("Check OK") And IDJoined = ComboBox1.Text And IsReceiving = True Then
            If ListBox1.SelectedIndex >= 0 Then
                If LastLineStr.Contains("Check OK") And IDJoined = ListBox1.SelectedItem.ToString And IsReceiving = True Then
                    UpdateList("Server says: Send Screen | " & IDJoined, True)
                    If Form3.NextClientAuthenIndex <= Form3.clientListCount - 1 Then
                        Form3.NextClientAuthenIndex += 1
                    End If
                    PhaseCheck = 0
                End If
            End If
        End If
    End Sub

    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        If ClientExitedCheck Then
            'ClientExitIDIndex = ComboBox1.Items.IndexOf(ClientExitID)
            ClientExitIDIndex = ListBox1.Items.IndexOf(ClientExitID)
            'If ComboBox1.Text = ClientExitID Then
            Try
                If ListBox1.SelectedItem.ToString = ClientExitID Then
                    Form1.Close()
                    ListBox1.SetSelected(ListBox1.SelectedIndex, False)
                End If
            Catch ex As Exception
            End Try
            'If ComboBox1.Items.Contains(ClientExitID) Then
            'If ListBox1.Items.Contains(ClientExitID) Then
            '    'ComboBox1.Items.Remove(ClientExitID)
            '    ListBox1.Items.Remove(ClientExitID)
            '    If ComboBox1.Text = ClientExitID Then
            '        ComboBox1.Text = ""
            '    End If
            '    ListBox1.SelectedIndex = 0
            'End If
            'ClientExitID = ""
            ClientExitedCheck = False
        End If
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        'UpdateList("Server says: Send Screen | " & ComboBox1.SelectedItem.ToString, True)
        'Form1.Close()
        'Form1.Show()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'If ComboBox1.SelectedItem <> Nothing Then
        '    UpdateList("Server says: Refresh | " & ComboBox1.SelectedItem.ToString, True)
        'End If
        'If ListBox1.SelectedItem <> CurrentPicSendingID And OnlineList(ListBox1.Items.IndexOf(CurrentPicSendingID)) <> "" Then
        '    For index = 0 To clientList.Count - 1
        '        If clientList(index).IDName = CurrentPicSendingID Then
        '            ClientIndexForChangingRes = index
        '        End If
        '    Next
        '    Form3.TellUDPClientToExit()
        '    Form3.TellUDPClientNotToExit()
        'End If
        'Form1.PictureBox1.Image = Nothing
        MouseSendable = False
        Form1.Close()
        Form1.Show()
        Form1.Hide()

        For index = 0 To clientList.Count - 1
            If clientList(index).IDName = CurrentPicSendingID Then
                ClientIndexForChangingRes = index
            End If
        Next
        If ListBox1.SelectedItem <> CurrentPicSendingID And OnlineList(ListBox1.Items.IndexOf(CurrentPicSendingID)) <> "" Then
            Form3.TellUDPClientNotToExit()
            Form3.TellUDPClientToExit4()
        End If

        'Form3.ResetUDPServer()
        If ListBox1.Items.Count > 0 And ListBox1.SelectedItem.ToString <> "" Then
            If ListBox1.SelectedItem <> CurrentPicSendingID Then
                UpdateList("Server says: Refresh Exit UDP | " & ListBox1.SelectedItem.ToString, True)
            Else
                UpdateList("Server says: Refresh Don't UDP | " & ListBox1.SelectedItem.ToString, True)
            End If
            Form1.Close()
            LastLineStr = ""
            PhaseCheck = 0
            IDJoined = ""
            IsReceiving = False
            ClientExitedCheck = False
            ClientExitID = ""
            Refreshing = True
            End If
            Timer8.Start()


        'If ListBox1.SelectedIndex >= 0 Then
        '    Form3.SetFalse()
        '    TellUDPClientToExit = True
        '    Form3.Timer3.Start()
        'End If

    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged

    End Sub

    Private Sub Timer3_Tick(sender As Object, e As EventArgs) Handles Timer3.Tick
        If IDJoined <> "" Then
            If Not ListBox1.Items.Contains(IDJoined) Then
                ListBox1.Items.Add(IDJoined)
                OnlineList.Add(IDJoined)
                CreateOCPic(8, 8, 5, 13)
                ResPercentValue.Add(50)
            End If
            Dim IDJoinedIndex As Integer = ListBox1.Items.IndexOf(IDJoined)
            If OnlineList(IDJoinedIndex) = "" Then
                OnlineList(IDJoinedIndex) = IDJoined
            End If
        End If
        If ListBox1.Items.Count > 0 Then
            For index = 0 To ListBox1.Items.Count - 1
                If ListBox1.Items(index).ToString = OnlineList(index) Then
                    OCPic(index).BackColor = Color.Green
                Else
                    OCPic(index).BackColor = Color.Red
                End If
            Next
        End If
        If LastLineStr <> "" Then
            If LastLineStr.Contains("Screen Res") Then
                If Refreshing = False Then
                    If ClientScreenResolutionW.Count < clientList.Count Then
                        Dim ScreenWidthStart As Integer = LastLineStr.IndexOf("[") + 2
                        Dim ScreenWidthEnd As Integer = LastLineStr.IndexOf("]")
                        Dim ScreenWidth As String = LastLineStr.Substring(ScreenWidthStart, ScreenWidthEnd - ScreenWidthStart)
                        ClientScreenResolutionW.Add(ScreenWidth)
                    End If
                    If ClientScreenResolutionH.Count < clientList.Count Then
                        Dim ScreenHeightStart As Integer = LastLineStr.IndexOf("{") + 2
                        Dim ScreenHeightEnd As Integer = LastLineStr.IndexOf("}")
                        Dim ScreenHeight As String = LastLineStr.Substring(ScreenHeightStart, ScreenHeightEnd - ScreenHeightStart)
                        ClientScreenResolutionH.Add(ScreenHeight)
                    End If
                End If
            End If
        End If
        Me.Text = Form3.NextClientAuthenIndex
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Form3.ResetUDPServer()
        If ListBox1.SelectedItem.ToString <> "" Then
            UpdateList("Just Nothing", True)
            UpdateList("Server says: Send Screen | " & ListBox1.SelectedItem.ToString, True)
            Form1.Close()
            Form1.Show()
            Form1.Hide()
            TrackBar1.Value = ResPercentValue(ListBox1.Items.IndexOf(ListBox1.SelectedItem.ToString))
            'If ListBox1.SelectedItem <> CurrentPicSendingID Then
            '    Form3.SetFalse()
            '    TellUDPClientToExit = True
            '    Form3.Timer3.Start()
            'End If
            MouseSendable = False
            If ListBox1.SelectedItem <> CurrentPicSendingID And OnlineList(ListBox1.Items.IndexOf(CurrentPicSendingID)) <> "" Then
                For index = 0 To clientList.Count - 1
                    If clientList(index).IDName = CurrentPicSendingID Then
                        ClientIndexForChangingRes = index
                    End If
                Next
                Form3.TellUDPClientToExit()
                Form3.TellUDPClientNotToExit()
            End If
        End If
    End Sub

    Private Sub Form2_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        MeClosed = True
        Listning.Stop()

        End
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs)
        Form1.Show()
    End Sub

    Private Sub Timer4_Tick(sender As Object, e As EventArgs) Handles Timer4.Tick
        MainScreen.Image = Form1.PictureBox1.Image
        Dim IDStart As Integer

        Dim IDIndex As Integer
        If LastLineStr <> "" Then
            If LastLineStr.Contains("Send Screen") Then
                IDStart = LastLineStr.IndexOf("|") + 2
                CurrentPicSendingID = LastLineStr.Substring(IDStart, LastLineStr.Length - IDStart)
                IDIndex = ListBox1.Items.IndexOf(CurrentPicSendingID)
            End If
        End If
        If CurrentPicSendingID <> "" Then
            If CurrentPicSendingID = OnlineList(IDIndex) Then
                TrackBar1.Enabled = True
            Else
                TrackBar1.Enabled = False
            End If
        End If
        If ListBox1.SelectedItem <> "" Then
            If ListBox1.SelectedItem = OnlineList(ListBox1.SelectedIndex) Then
                Button3.Enabled = True
            Else
                Button3.Enabled = False
            End If
        End If
        If MainScreen.Image Is Nothing And clientList.Count = 0 Then
            TrackBar1.Enabled = False
            Button3.Enabled = False
        End If
        L_CurrentPicSendingID.Text = "Watching ID: " & CurrentPicSendingID
        Resolution.Text = "Resolution: " & TrackBar1.Value & "%"
    End Sub

    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        For index = 0 To clientList.Count - 1
            If clientList(index).IDName = CurrentPicSendingID Then
                ClientIndexForChangingRes = index
                ResolutionValue = TrackBar1.Value
                ResPercentValue(index) = TrackBar1.Value
                ChangingResolution = True
                Me.Text = ClientIndexForChangingRes
            End If
        Next
    End Sub

    Private Sub TrackBar1_MouseUp(sender As Object, e As MouseEventArgs) Handles TrackBar1.MouseUp
        ChangingResolution = False
    End Sub

    Private Sub Timer5_Tick(sender As Object, e As EventArgs) Handles Timer5.Tick
        If MainScreen.Image Is Nothing Then
        Else
            Me.Text = MainScreen.Image.Width & "     " & MainScreen.Image.Height
            Dim imageSize As Size = MainScreen.Image.Size
            Dim ratio As Decimal = Math.Min(SC1Panel2Size.Width / imageSize.Width, SC1Panel2Size.Height / imageSize.Height)
            Panel1.Width = CInt(imageSize.Width * ratio)
            Panel1.Height = CInt(imageSize.Height * ratio)
            Panel1.Left = (SC1Panel2Size.Width - Panel1.Width) / 2
            Panel1.Top = (SC1Panel2Size.Height - Panel1.Height) / 2
            Me.Text = ClientMouseX & "     " & ClientMouseY
        End If
        'If Form3.TurnOffTUDPCTE Then
        '    TellUDPClientToExit = False
        'End If
        Button1.Text = MouseDownTick
        If Form1.PictureBox1.Image IsNot Nothing Then
            MouseSendable = True
        Else
            MouseSendable = False
        End If
    End Sub

    Private Sub PictureBox2_MouseMove(sender As Object, e As MouseEventArgs) Handles MainScreen.MouseMove
        MousePointer.Location = New Point(e.Location)
        If MainScreen.Image Is Nothing Then
        Else
            If ResolutionValue <> 0 And ClientScreenResolutionW.Count > 0 And ClientScreenResolutionH.Count > 0 Then
                'wRatio = MainScreen.Image.Width / MainScreen.Width
                'hRatio = MainScreen.Image.Height / MainScreen.Height              
                wRatio = ClientScreenResolutionW(ClientIndexForChangingRes) / MainScreen.Width
                hRatio = ClientScreenResolutionH(ClientIndexForChangingRes) / MainScreen.Height
                ClientMouseX = Math.Floor(MousePointer.Left * wRatio)
                ClientMouseY = Math.Floor(MousePointer.Top * hRatio)
            End If
            If clientList.Count > 0 Then
                If MouseSendable Then
                    SendMouseLocation = True
                End If
            End If
                Timer6.Start()
        End If
    End Sub

    Private Sub MainScreen_MouseEnter(sender As Object, e As EventArgs) Handles MainScreen.MouseEnter
        If MainScreen.Image Is Nothing Then
        Else
            If clientList.Count > 0 Then
                If MouseSendable Then
                    SendMouseLocation = True
                End If
            End If
        End If
        For index = 0 To clientList.Count - 1
            If clientList(index).IDName = CurrentPicSendingID Then
                ClientIndexForChangingRes = index
                'Me.Text = ClientIndexForChangingRes
            End If
        Next
        MainScreen.Select()
    End Sub

    Private Sub MainScreen_MouseLeave(sender As Object, e As EventArgs) Handles MainScreen.MouseLeave
        SendMouseLocation = False
        SC1Panel2Size.Select()
    End Sub

    Private Sub MainScreen_MouseWheel(sender As Object, e As MouseEventArgs) Handles MainScreen.MouseWheel
        Timer6.Start()
        If e.Delta > 0 Then
            ScrollTimes += 1
            TextBox2.Text = ScrollTimes
        ElseIf e.Delta < 0 Then
            ScrollTimes -= 1
            TextBox2.Text = ScrollTimes
        End If
    End Sub

    Private Sub Timer6_Tick(sender As Object, e As EventArgs) Handles Timer6.Tick
        Form3.SendScroll(ScrollTimes)
        ScrollTimes = 0
        TextBox2.Text = ScrollTimes
        Timer6.Stop()
    End Sub

    Private Sub MainScreen_MouseDown(sender As Object, e As MouseEventArgs) Handles MainScreen.MouseDown
        If e.Button = MouseButtons.Left Then
            MouseClickState = 1
            Timer7.Start()
        End If
        If e.Button = MouseButtons.Right Then
            MouseClickState = 2
        End If
        If e.Button = MouseButtons.Middle Then
            MouseClickState = 3
        End If
    End Sub

    Private Sub MainScreen_MouseUp(sender As Object, e As MouseEventArgs) Handles MainScreen.MouseUp
        Select Case MouseClickState
            Case 1
                MouseClickState = 4
            Case 2
                MouseClickState = 5
            Case 3
                MouseClickState = 6
        End Select
        Form3.SendClick(MouseClickState, False, isMouseHolding)
        Timer7.Stop()
        MouseClickState = 0
        MouseDownTick = 0
        isMouseHolding = False
    End Sub

    Private Sub Timer7_Tick(sender As Object, e As EventArgs) Handles Timer7.Tick
        MouseDownTick += 1
        If MouseDownTick = 10 Then
            isMouseHolding = True
            Timer7.Stop()
            Form3.SendClick(MouseClickState + 3, True, False)
        End If
    End Sub

    Private Sub Timer8_Tick(sender As Object, e As EventArgs) Handles Timer8.Tick
        'If ConvertGAKSToString() <> "" Then
        '    Button2.Text = ConvertGAKSToString()
        'End If
    End Sub
End Class