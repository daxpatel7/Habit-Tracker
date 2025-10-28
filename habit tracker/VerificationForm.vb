Public Class VerificationForm
    Public Property ExpectedCode As String
    Private Sub B1_Click(sender As Object, e As EventArgs) Handles B1.Click
        If T1.Text.Trim() = ExpectedCode Then
            MessageBox.Show("Email verified successfully!", "Verified", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Form1.Hide()
            Form2.Hide()
            Me.Hide()
        Else
            MessageBox.Show("Invalid code. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub


End Class
