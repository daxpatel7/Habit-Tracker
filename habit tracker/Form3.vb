Imports System.Data.OleDb
Imports System.IO

Public Class Form3
    Public reminderTimes As New Dictionary(Of String, DateTime?)()
    Public reminderTimer As New Timer With {.Interval = 60000}
    Public userTableName As String
    Dim dbPath As String = Path.Combine(Application.StartupPath, "sign up.accdb")
    Dim conn As New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & dbPath)
    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        FlowLayoutPanel1.Controls.Clear()
        reminderTimes.Clear()


        If String.IsNullOrEmpty(Form1.LoggedInUserName) Then
            Label2.Text = "Welcome!"
        Else
            Label2.Text = "Welcome, " & Form1.LoggedInUserName & "!"
        End If

        Try
            If conn.State = ConnectionState.Closed Then conn.Open()


            userTableName = "Habits_" & Form1.LoggedInUserEmail.Replace("@", "_").Replace(".", "_")


            If Not TableExists(userTableName) Then
                Dim createTableCmd As New OleDbCommand(
                $"CREATE TABLE [{userTableName}] (
                    ID AUTOINCREMENT PRIMARY KEY,
                    HabitName TEXT(255),
                    HabitTime TEXT(50)
                )", conn)
                createTableCmd.ExecuteNonQuery()
            End If


            LoadHabitsFromDatabase()

            AddHandler reminderTimer.Tick, AddressOf CheckReminders
            reminderTimer.Start()
        Catch ex As Exception
            MessageBox.Show("Error loading Form3: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Function TableExists(tableName As String) As Boolean
        Dim schemaTable As DataTable = conn.GetSchema("Tables")
        For Each row As DataRow In schemaTable.Rows
            If row("TABLE_NAME").ToString().Equals(tableName, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub LoadHabitsFromDatabase()
        FlowLayoutPanel1.Controls.Clear()
        Dim query As String = $"SELECT HabitName, HabitTime FROM [{userTableName}]"

        Using cmd As New OleDbCommand(query, conn)
            Using reader As OleDbDataReader = cmd.ExecuteReader()
                While reader.Read()
                    Dim habitName As String = reader("HabitName").ToString()
                    Dim reminderTime As DateTime? = Nothing

                    If Not IsDBNull(reader("HabitTime")) AndAlso Not String.IsNullOrWhiteSpace(reader("HabitTime").ToString()) Then
                        reminderTime = DateTime.Parse(reader("HabitTime").ToString())
                    End If

                    reminderTimes(habitName) = reminderTime
                    AddHabitCard(habitName)
                End While
            End Using
        End Using
    End Sub

    Private Sub SaveHabitToDatabase(habitName As String, reminderTime As DateTime?)
        Dim timeStr As Object = If(reminderTime.HasValue, reminderTime.Value.ToString("HH:mm"), DBNull.Value)
        Dim insertQuery As String = $"INSERT INTO [{userTableName}] (HabitName, HabitTime) VALUES (@name, @time)"

        conn.Open()
        Using cmd As New OleDbCommand(insertQuery, conn)
            cmd.Parameters.AddWithValue("@name", habitName)
            cmd.Parameters.AddWithValue("@time", timeStr)
            cmd.ExecuteNonQuery()
        End Using
        conn.Close()
    End Sub

    Private Sub DeleteHabitFromDatabase(habitName As String)
        Dim deleteQuery As String = $"DELETE FROM [{userTableName}] WHERE HabitName = @name"

        conn.Open()
        Using cmd As New OleDbCommand(deleteQuery, conn)
            cmd.Parameters.AddWithValue("@name", habitName)
            cmd.ExecuteNonQuery()
        End Using
        conn.Close()
    End Sub

    Private Sub UpdateHabitTimeInDatabase(habitName As String, reminderTime As DateTime?)
        Dim timeStr As Object = If(reminderTime.HasValue, reminderTime.Value.ToString("HH:mm"), DBNull.Value)
        Dim updateQuery As String = $"UPDATE [{userTableName}] SET HabitTime = @time WHERE HabitName = @name"

        conn.Open()
        Using cmd As New OleDbCommand(updateQuery, conn)
            cmd.Parameters.AddWithValue("@time", timeStr)
            cmd.Parameters.AddWithValue("@name", habitName)
            cmd.ExecuteNonQuery()
        End Using
        conn.Close()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim popup As New HabitNamePopup()
        If popup.ShowDialog() = DialogResult.OK Then
            Dim habitName As String = popup.TextBox1.Text.Trim()
            If String.IsNullOrWhiteSpace(habitName) Then
                MessageBox.Show("Habit name cannot be empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If
            AddHabitCard(habitName)
            SaveHabitToDatabase(habitName, Nothing)
        End If
    End Sub

    Private Sub AddHabitCard(habitName As String)
        Dim panel As New Panel With {
            .BackColor = Color.White,
            .Size = New Size(FlowLayoutPanel1.Width - 40, 50),
            .BorderStyle = BorderStyle.FixedSingle,
            .Margin = New Padding(10)
        }

        Dim chk As New CheckBox With {.Location = New Point(10, 15), .AutoSize = True}
        panel.Controls.Add(chk)

        Dim lbl As New Label With {.Text = habitName, .Font = New Font("Segoe UI", 10), .AutoSize = True, .Location = New Point(35, 15)}
        panel.Controls.Add(lbl)

        Dim btnReminder As New Button With {.Text = "⏰", .Size = New Size(30, 30), .Location = New Point(panel.Width - 70, 10)}
        AddHandler btnReminder.Click, Sub()
                                          Dim timeForm As New TimePickerPopup()
                                          If timeForm.ShowDialog() = DialogResult.OK Then
                                              Dim selectedTime As DateTime = timeForm.SelectedTime
                                              reminderTimes(lbl.Text) = selectedTime
                                              UpdateHabitTimeInDatabase(lbl.Text, selectedTime)
                                              MessageBox.Show("Reminder set for " & selectedTime.ToShortTimeString(), "Reminder Set")
                                          End If
                                      End Sub
        panel.Controls.Add(btnReminder)

        Dim btnDelete As New Button With {.Text = "🗑️", .Size = New Size(30, 30), .Location = New Point(panel.Width - 35, 10)}
        AddHandler btnDelete.Click, Sub()
                                        If MessageBox.Show("Delete this habit?", "Confirm", MessageBoxButtons.YesNo) = DialogResult.Yes Then
                                            FlowLayoutPanel1.Controls.Remove(panel)
                                            reminderTimes.Remove(lbl.Text)
                                            DeleteHabitFromDatabase(lbl.Text)
                                        End If
                                    End Sub
        panel.Controls.Add(btnDelete)

        FlowLayoutPanel1.Controls.Add(panel)
    End Sub

    Private Sub ShowReminderNotification(habitName As String)
        NotifyIcon1.BalloonTipTitle = "Habit Reminder"
        NotifyIcon1.BalloonTipText = "⏰ Complete your habit: " & habitName
        NotifyIcon1.Visible = True
        NotifyIcon1.ShowBalloonTip(5000)
    End Sub

    Private Sub CheckReminders(sender As Object, e As EventArgs)
        Dim nowTime As String = DateTime.Now.ToString("HH:mm")
        For Each habit In reminderTimes.Keys
            If reminderTimes(habit).HasValue AndAlso reminderTimes(habit).Value.ToString("HH:mm") = nowTime Then
                ShowReminderNotification(habit)
            End If
        Next
    End Sub
    Private Sub Form3_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            Dim procName As String = Process.GetCurrentProcess().ProcessName
            For Each p As Process In Process.GetProcessesByName(procName)
                If p.Id <> Process.GetCurrentProcess().Id Then
                    p.Kill()
                End If
            Next
            Application.Exit()
        Catch ex As Exception
            MessageBox.Show("Error while closing processes: " & ex.Message)
        End Try
    End Sub

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click
        Form4.Show()
        Me.Hide()
    End Sub

    Private Sub PictureBox3_Click(sender As Object, e As EventArgs) Handles PictureBox3.Click
        Form4.Show()
        Me.Hide()
    End Sub

    Private Sub Label4_Click(sender As Object, e As EventArgs) Handles Label4.Click
        Dim popup As New loginconfirm()
        popup.ShowDialog()
        Me.Hide()
    End Sub
    Private Sub PictureBox4_Click(sender As Object, e As EventArgs) Handles PictureBox4.Click
        Dim popup As New loginconfirm()
        popup.ShowDialog()
        Me.Hide()
    End Sub
    Public Sub ResetData()
        FlowLayoutPanel1.Controls.Clear()
        reminderTimes.Clear()
        userTableName = Nothing
    End Sub


End Class
