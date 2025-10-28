Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Net.Mail
Imports System.Text.RegularExpressions

Public Class Form1
    Dim dbPath As String = Path.Combine(Application.StartupPath, "sign up.accdb")
    Dim conn As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & dbPath)

    Public Shared LoggedInUserName As String
    Public Shared LoggedInUserEmail As String
    Public Shared CurrentHabits As New Dictionary(Of String, String)()

    Public Function SendVerificationCode(toEmail As String) As String
        Dim code As String = New Random().Next(100000, 999999).ToString()

        Try
            Dim smtp As New SmtpClient("smtp.gmail.com", 587)
            smtp.EnableSsl = True
            smtp.Credentials = New NetworkCredential("yourhabittrackerapp@gmail.com", "xkro junh buxj vaxh ")

            Dim mail As New MailMessage()
            mail.From = New MailAddress("yourhabittrackerapp@gmail.com")
            mail.To.Add(toEmail)
            mail.Subject = "Your Verification Code"
            mail.IsBodyHtml = True
            mail.Body = "
           <html>
            <body style='font-family: Arial; padding: 30px; background: linear-gradient(to bottom right, #d4f5e9, #a3e4d7, #58d68d); color: #1b4f39;'>
                <div style='background-color: white; padding: 20px; border-radius: 12px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);'>
                <h2 style='color: #196f3d; text-align: center;'>🌿 Welcome to Habit Tracker!</h2>
                <p>Thanks for signing up to <strong>Habit Tracker</strong>. Here’s your verification code:</p>
                <div style='font-size: 36px; font-weight: bold; color: #27ae60; text-align: center; margin: 25px 0;'>
                " & code & "
                </div>
                <p>This code is valid for 5 minutes. Please do not share it with anyone.</p>
                <p style='font-size: 12px; color: gray;'>If you didn’t request this code, you can safely ignore this message.</p>
                </div>
            </body>
           </html>"

            smtp.Send(mail)
        Catch ex As Exception
            MessageBox.Show("Failed to send email: " & ex.Message)
            Return Nothing
        End Try

        Return code
    End Function

    Private Sub B1_Click(sender As Object, e As EventArgs) Handles B1.Click
        Dim name As String = T1.Text.Trim()
        Dim email As String = T2.Text.Trim()
        Dim password As String = T3.Text.Trim()

        If name = "" OrElse email = "" OrElse password = "" Then
            MessageBox.Show("Please fill all fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If Not Regex.IsMatch(email, "^[^@\s]+@[^@\s]+\.[^@\s]+$") Then
            MessageBox.Show("Invalid email format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        Try
            conn.Open()
            ' Check if email already exists
            Dim checkCmd As New OleDbCommand("SELECT COUNT(*) FROM [users] WHERE [email] = ?", conn)
            checkCmd.Parameters.AddWithValue("?", email)
            Dim count As Integer = CInt(checkCmd.ExecuteScalar())

            If count > 0 Then
                MessageBox.Show("This email is already registered. Please log in instead.", "Account Exists", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
        Catch ex As Exception
            MessageBox.Show("Database error during validation: " & ex.Message)
            Return
        Finally
            conn.Close()
        End Try

        Dim code As String = SendVerificationCode(email)
        If code Is Nothing Then Return

        Dim verifyForm As New VerificationForm()
        verifyForm.ExpectedCode = code

        If verifyForm.ShowDialog() = DialogResult.OK Then
            Try
                conn.Open()
                ' Insert new user into users table
                Dim cmd As New OleDbCommand("INSERT INTO [users] ([name], [email], [password]) VALUES (?, ?, ?)", conn)
                cmd.Parameters.AddWithValue("?", name)
                cmd.Parameters.AddWithValue("?", email)
                cmd.Parameters.AddWithValue("?", password)
                cmd.ExecuteNonQuery()

                ' Create a habits table for this user
                LoggedInUserName = name
                LoggedInUserEmail = email

                ' Create a habits table for this user (use email for uniqueness)
                Dim safeEmail As String = email.Replace("@", "_").Replace(".", "_")
                Dim tableName As String = "Habits_" & safeEmail
                Dim createTableCmd As New OleDbCommand(
                      "CREATE TABLE [" & tableName & "] (
                        HabitID AUTOINCREMENT PRIMARY KEY,
                        HabitName TEXT(255),
                        HabitTime TEXT(10),
                        CreatedDate DATETIME
                        )", conn)
                createTableCmd.ExecuteNonQuery()

                ' Store username for later use
                LoggedInUserName = name

                MessageBox.Show("Account created successfully!", "Success")

                ' Go to main form
                Dim f3 As New Form3()
                f3.Show()
                Me.Hide()

                ' Clear input fields
                T1.Clear()
                T2.Clear()
                T3.Clear()

            Catch ex As Exception
                MessageBox.Show("Database error: " & ex.Message)
            Finally
                conn.Close()
            End Try
        Else
            MessageBox.Show("Email verification failed.", "Failed")
        End If
    End Sub

    Private Sub Label5_Click(sender As Object, e As EventArgs) Handles Label5.Click
        Form2.Show()
        Me.Hide()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        T3.PasswordChar = "●"c
    End Sub

    Private Sub T2_TextChanged(sender As Object, e As EventArgs) Handles T2.TextChanged
        For Each c As Char In T2.Text
            If Char.IsUpper(c) Then
                MessageBox.Show("Email must be in Lowercase Letter.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                T2.Text = T2.Text.Replace(c, "")
                T2.SelectionStart = T2.Text.Length
                Exit For
            End If
        Next
    End Sub
End Class
