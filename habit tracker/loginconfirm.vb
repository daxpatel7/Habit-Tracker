Public Class loginconfirm
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Form1.LoggedInUserName = Nothing
        Form1.LoggedInUserEmail = Nothing
        Form1.CurrentHabits.Clear()

        If Form3 IsNot Nothing Then
            Form3.FlowLayoutPanel1.Controls.Clear()
            Form3.reminderTimes.Clear()
            Form3.userTableName = Nothing
            Form3.Close()
        End If


        If Form2 Is Nothing OrElse Form2.IsDisposed Then
            Form2 = New Form2()
        End If
        Form2.Show()
        Me.Close()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Close()
    End Sub
End Class
