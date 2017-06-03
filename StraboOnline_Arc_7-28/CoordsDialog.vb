Imports System.Windows.Forms
Public Class CoordsDialog
    Private Sub Coordinates_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Coordinates.SelectedIndexChanged
        Coordinates.SelectionMode = SelectionMode.One
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        coordBool = True
        Label2.Text = Coordinates.SelectedItem
        selFID = CType(Coordinates.SelectedItem, Integer)
        MessageBox.Show("You have chosen coordinates from row with FID: " + Coordinates.SelectedItem)
        Me.Close()
    End Sub
End Class