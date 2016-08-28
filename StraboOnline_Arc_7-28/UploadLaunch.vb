Public Class UploadLaunch

    Inherits ESRI.ArcGIS.Desktop.AddIns.Button

    Public Sub New()

    End Sub

    Protected Overrides Sub OnClick()
        Dim upload As New Upload()
        upload.ShowDialog()
    End Sub


End Class
