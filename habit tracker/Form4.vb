Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports System.Text.RegularExpressions
Public Class Form4

    Dim dbPath As String = Path.Combine(Application.StartupPath, "sign up.accdb")
    Dim conn As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & dbPath)
    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            conn.Open()
            PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage

            Dim cmd As New OleDbCommand("SELECT * FROM users WHERE name = ?", conn)
            cmd.Parameters.AddWithValue("?", Form1.LoggedInUserName)

            Dim reader As OleDbDataReader = cmd.ExecuteReader()

            If reader.Read() Then

                Label2.Text = "👤 Name: " & reader("name").ToString()
                Label3.Text = "📧 Email: " & reader("email").ToString()
            Else
                Label2.Text = "User not found."
                Label3.Text = ""
            End If

            reader.Close()
        Catch ex As Exception
            MessageBox.Show("Error loading account info: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub B1_Click(sender As Object, e As EventArgs) Handles B1.Click
        Me.Close()
        Form3.Show()
    End Sub

    Private Sub Form4_Load_1()

    End Sub
End Class