Imports System.Data.OleDb
Imports System.IO
Imports System.Text.RegularExpressions

Public Class Form2
    Dim dbPath As String = Path.Combine(Application.StartupPath, "sign up.accdb")
    Dim conn As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & dbPath)

    Private Sub B1_Click(sender As Object, e As EventArgs) Handles B1.Click
        Dim email As String = T1.Text.Trim()
        Dim password As String = T2.Text.Trim()

        If email = "" OrElse password = "" Then
            MessageBox.Show("Please enter both email and password.", "Missing Info")
            Return
        End If

        If Not Regex.IsMatch(email, "^[^@\s]+@[^@\s]+\.[^@\s]+$") Then
            MessageBox.Show("Invalid email format.", "Error")
            Return
        End If

        Try
            conn.Open()
            Dim cmd As New OleDbCommand("SELECT * FROM users WHERE email = ? AND password = ?", conn)
            cmd.Parameters.AddWithValue("?", email)
            cmd.Parameters.AddWithValue("?", password)

            Dim reader As OleDbDataReader = cmd.ExecuteReader()

            If reader.Read() Then

                Dim code As String = Form1.SendVerificationCode(email)
                If code Is Nothing Then
                    conn.Close()
                    Return
                End If

                Dim verifyForm As New VerificationForm()
                verifyForm.ExpectedCode = code

                If verifyForm.ShowDialog() = DialogResult.OK Then

                    Dim userName As String = reader("name").ToString()
                    Form1.LoggedInUserName = userName
                    Form1.LoggedInUserEmail = email


                    Dim safeEmail As String = email.Replace("@", "_").Replace(".", "_")
                    Dim tableName As String = "Habits_" & safeEmail


                    Dim createTableCmd As New OleDbCommand(
                        $"CREATE TABLE [{tableName}] (
                            ID AUTOINCREMENT PRIMARY KEY,
                            HabitName TEXT(255),
                            HabitTime TEXT(50)
                        )", conn)

                    Try
                        createTableCmd.ExecuteNonQuery()
                    Catch ex As Exception

                    End Try

                    MessageBox.Show("Login successful!", "Welcome")
                    Dim u As String = reader("name").ToString()
                    Form1.LoggedInUserName = u
                    Form1.CurrentHabits.Clear()
                    Dim f3 As New Form3()
                    f3.Show()
                    Me.Hide()
                    T1.Clear()
                    T2.Clear()
                Else
                    MessageBox.Show("Email verification failed.", "Failed")
                End If
            Else

                MessageBox.Show("Invalid email or password.", "Login Failed")
            End If

            reader.Close()
        Catch ex As Exception
            MessageBox.Show("Database error: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        T2.PasswordChar = "●"c
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click
        Form1.Show()
        Me.Close()
    End Sub

    Private Sub T1_TextChanged(sender As Object, e As EventArgs) Handles T1.TextChanged
        For Each c As Char In T1.Text
            If Char.IsUpper(c) Then
                MessageBox.Show("Email must be in Lowercase Letter.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                T1.Text = T1.Text.Replace(c, "")
                T1.SelectionStart = T1.Text.Length
                Exit For
            End If
        Next
    End Sub
End Class
