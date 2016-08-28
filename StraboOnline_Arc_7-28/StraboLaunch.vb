Public Class StraboLaunch
    Inherits ESRI.ArcGIS.Desktop.AddIns.Button

    Public Sub New()

    End Sub

    Protected Overrides Sub OnClick()
        Dim download As New Download()
        download.ShowDialog()
    End Sub


End Class
