Public Class TimePickerPopup
    Public Property SelectedTime As DateTime
    Private Sub TimePickerPopup_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        DateTimePicker1.Format = DateTimePickerFormat.Custom
        DateTimePicker1.CustomFormat = "hh:mm tt"
        DateTimePicker1.ShowUpDown = True
    End Sub
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        SelectedTime = DateTimePicker1.Value
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub
End Class
