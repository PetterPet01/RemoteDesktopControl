Imports System.Net.Sockets
Imports System.Threading
Imports System.Drawing
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.InteropServices
Imports System.Net
Imports System.Net.NetworkInformation
Imports System.ComponentModel

Public Class Form1
    Dim client As New TcpClient
    Dim port As Integer
    Dim server As TcpListener
    Dim ns As NetworkStream
    Dim listening As New Thread(AddressOf Listen)
    Dim GetImage As New Thread(AddressOf ReceiveImage)

    Dim wRatio As Integer
    Dim hRatio As Integer

    Private Sub ReceiveImage()
        Dim bf As New BinaryFormatter
        While client.Connected = True
            Try
                ns = client.GetStream
                PictureBox1.Image = bf.Deserialize(ns)
            Catch ex As Exception
            End Try
        End While
    End Sub

    Private Sub Listen()
        Try
            While client.Connected = False
                server.Start()
                client = server.AcceptTcpClient
            End While
            GetImage.Start()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        port = 4395
        server = New TcpListener(IPAddress.Any, port)
        listening.Start()
        wRatio = Math.Floor(Me.Width * (Me.Width / PictureBox1.Width))
        hRatio = Math.Floor(Me.Height * (Me.Height / PictureBox1.Height))
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        'port = 4395
        'server = New TcpListener(IPAddress.Any, port)
        'listening.Start()
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        PictureBox1.Width = Me.Width - PictureBox1.Left * 2
        PictureBox1.Height = Me.Height - PictureBox1.Top - 20

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        server.Stop()
        client.Close()
        client.Dispose()
    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        If NumericUpDown1.Value < 10 Then
            NumericUpDown1.Value = 10
        ElseIf NumericUpDown1.Value > 100 Then
            NumericUpDown1.Value = 100
        Else
        End If
    End Sub
End Class
