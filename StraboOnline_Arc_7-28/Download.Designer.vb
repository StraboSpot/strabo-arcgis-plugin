<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Download
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Download))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Username = New System.Windows.Forms.TextBox()
        Me.PasswordBox = New System.Windows.Forms.TextBox()
        Me.LogIn = New System.Windows.Forms.Button()
        Me.Sel = New System.Windows.Forms.Label()
        Me.getDatasets = New System.Windows.Forms.Button()
        Me.Datasets = New System.Windows.Forms.ListBox()
        Me.Projects = New System.Windows.Forms.ListBox()
        Me.backForm2 = New System.Windows.Forms.Button()
        Me.straboToGIS = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.Browse = New System.Windows.Forms.Button()
        Me.PathName = New System.Windows.Forms.TextBox()
        Me.BackDatasets = New System.Windows.Forms.Button()
        Me.choose = New System.Windows.Forms.Button()
        Me.browseDir = New System.Windows.Forms.Label()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.RadioButton2 = New System.Windows.Forms.RadioButton()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.progBar = New System.Windows.Forms.ProgressBar()
        Me.progLabel = New System.Windows.Forms.Label()
        Me.filesSaved = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(229, 90)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(182, 29)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Log In to Strabo"
        '
        'Username
        '
        Me.Username.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Username.Location = New System.Drawing.Point(260, 182)
        Me.Username.Name = "Username"
        Me.Username.Size = New System.Drawing.Size(296, 35)
        Me.Username.TabIndex = 4
        '
        'PasswordBox
        '
        Me.PasswordBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.PasswordBox.Location = New System.Drawing.Point(260, 257)
        Me.PasswordBox.Name = "PasswordBox"
        Me.PasswordBox.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.PasswordBox.Size = New System.Drawing.Size(296, 35)
        Me.PasswordBox.TabIndex = 5
        '
        'LogIn
        '
        Me.LogIn.AutoSize = True
        Me.LogIn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.LogIn.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LogIn.Location = New System.Drawing.Point(291, 359)
        Me.LogIn.Name = "LogIn"
        Me.LogIn.Size = New System.Drawing.Size(76, 35)
        Me.LogIn.TabIndex = 6
        Me.LogIn.Text = "Log In"
        Me.LogIn.UseVisualStyleBackColor = True
        '
        'Sel
        '
        Me.Sel.AutoSize = True
        Me.Sel.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Sel.Location = New System.Drawing.Point(134, 119)
        Me.Sel.Name = "Sel"
        Me.Sel.Size = New System.Drawing.Size(373, 29)
        Me.Sel.TabIndex = 7
        Me.Sel.Text = "Select Strabo Project and Dataset"
        Me.Sel.Visible = False
        '
        'getDatasets
        '
        Me.getDatasets.AutoSize = True
        Me.getDatasets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.getDatasets.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.getDatasets.Location = New System.Drawing.Point(239, 161)
        Me.getDatasets.Name = "getDatasets"
        Me.getDatasets.Size = New System.Drawing.Size(128, 35)
        Me.getDatasets.TabIndex = 9
        Me.getDatasets.Text = "Get Projects"
        Me.getDatasets.UseVisualStyleBackColor = True
        Me.getDatasets.Visible = False
        '
        'Datasets
        '
        Me.Datasets.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Datasets.FormattingEnabled = True
        Me.Datasets.ItemHeight = 29
        Me.Datasets.Location = New System.Drawing.Point(184, 203)
        Me.Datasets.Name = "Datasets"
        Me.Datasets.Size = New System.Drawing.Size(272, 149)
        Me.Datasets.TabIndex = 15
        Me.Datasets.Visible = False
        '
        'Projects
        '
        Me.Projects.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Projects.FormattingEnabled = True
        Me.Projects.ItemHeight = 29
        Me.Projects.Location = New System.Drawing.Point(184, 203)
        Me.Projects.Name = "Projects"
        Me.Projects.Size = New System.Drawing.Size(272, 149)
        Me.Projects.TabIndex = 16
        Me.Projects.Visible = False
        '
        'backForm2
        '
        Me.backForm2.AutoSize = True
        Me.backForm2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.backForm2.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.backForm2.Location = New System.Drawing.Point(12, 515)
        Me.backForm2.Name = "backForm2"
        Me.backForm2.Size = New System.Drawing.Size(148, 35)
        Me.backForm2.TabIndex = 17
        Me.backForm2.Text = "Back to Log-In"
        Me.backForm2.UseVisualStyleBackColor = True
        Me.backForm2.Visible = False
        '
        'straboToGIS
        '
        Me.straboToGIS.AutoSize = True
        Me.straboToGIS.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.straboToGIS.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.straboToGIS.Location = New System.Drawing.Point(254, 381)
        Me.straboToGIS.Name = "straboToGIS"
        Me.straboToGIS.Size = New System.Drawing.Size(132, 35)
        Me.straboToGIS.TabIndex = 20
        Me.straboToGIS.Text = "Import Spots"
        Me.straboToGIS.UseVisualStyleBackColor = True
        Me.straboToGIS.Visible = False
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(84, 187)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(102, 25)
        Me.Label4.TabIndex = 21
        Me.Label4.Text = "Username"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(84, 262)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(98, 25)
        Me.Label3.TabIndex = 22
        Me.Label3.Text = "Password"
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LinkLabel1.Location = New System.Drawing.Point(488, 0)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(153, 25)
        Me.LinkLabel1.TabIndex = 24
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "Visit StraboSpot"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(162, 105)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(317, 29)
        Me.Label2.TabIndex = 25
        Me.Label2.Text = "Create ArcGIS Geodatabase"
        Me.Label2.Visible = False
        '
        'Browse
        '
        Me.Browse.AutoSize = True
        Me.Browse.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Browse.Location = New System.Drawing.Point(84, 218)
        Me.Browse.Name = "Browse"
        Me.Browse.Size = New System.Drawing.Size(133, 45)
        Me.Browse.TabIndex = 26
        Me.Browse.Text = "Browse Files"
        Me.Browse.UseVisualStyleBackColor = True
        Me.Browse.Visible = False
        '
        'PathName
        '
        Me.PathName.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.PathName.Location = New System.Drawing.Point(320, 222)
        Me.PathName.Name = "PathName"
        Me.PathName.Size = New System.Drawing.Size(267, 35)
        Me.PathName.TabIndex = 27
        Me.ToolTip1.SetToolTip(Me.PathName, "A new folder will be created in this directory and all " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "images and JSON files wi" & _
        "ll be downloaded there. A .gdb" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "will also be created (of the same name) in this " & _
        "location. ")
        Me.PathName.Visible = False
        '
        'BackDatasets
        '
        Me.BackDatasets.AutoSize = True
        Me.BackDatasets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.BackDatasets.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BackDatasets.Location = New System.Drawing.Point(459, 515)
        Me.BackDatasets.Name = "BackDatasets"
        Me.BackDatasets.Size = New System.Drawing.Size(169, 35)
        Me.BackDatasets.TabIndex = 31
        Me.BackDatasets.Text = "Back to Datasets"
        Me.BackDatasets.UseVisualStyleBackColor = True
        Me.BackDatasets.Visible = False
        '
        'choose
        '
        Me.choose.AutoSize = True
        Me.choose.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.choose.Location = New System.Drawing.Point(217, 363)
        Me.choose.Name = "choose"
        Me.choose.Size = New System.Drawing.Size(194, 39)
        Me.choose.TabIndex = 33
        Me.choose.Text = "Choose Dataset(s)"
        Me.choose.UseVisualStyleBackColor = True
        Me.choose.Visible = False
        '
        'browseDir
        '
        Me.browseDir.AutoSize = True
        Me.browseDir.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.browseDir.Location = New System.Drawing.Point(84, 154)
        Me.browseDir.Name = "browseDir"
        Me.browseDir.Size = New System.Drawing.Size(472, 25)
        Me.browseDir.TabIndex = 34
        Me.browseDir.Text = "Browse to the desired file location of the geodatabase"
        Me.browseDir.Visible = False
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RadioButton1.Location = New System.Drawing.Point(150, 288)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(316, 29)
        Me.RadioButton1.TabIndex = 38
        Me.RadioButton1.Text = "Download Images in .Tiff Format"
        Me.ToolTip1.SetToolTip(Me.RadioButton1, "Downloads TIFF images to the chosen Working Directory. " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "TIFF downloads are usual" & _
        "ly faster, but do not geotag " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "the images. ")
        Me.RadioButton1.UseVisualStyleBackColor = True
        Me.RadioButton1.Visible = False
        '
        'RadioButton2
        '
        Me.RadioButton2.AutoSize = True
        Me.RadioButton2.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RadioButton2.Location = New System.Drawing.Point(150, 322)
        Me.RadioButton2.Name = "RadioButton2"
        Me.RadioButton2.Size = New System.Drawing.Size(341, 29)
        Me.RadioButton2.TabIndex = 39
        Me.RadioButton2.Text = "Download Images in .JPEG Format"
        Me.ToolTip1.SetToolTip(Me.RadioButton2, "Downloads images as JPEG to the working directory." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "This option goes slower, but " & _
        "does geotag the " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "images for later use. ")
        Me.RadioButton2.UseVisualStyleBackColor = True
        Me.RadioButton2.Visible = False
        '
        'ToolTip1
        '
        Me.ToolTip1.AutoPopDelay = 7000
        Me.ToolTip1.InitialDelay = 50
        Me.ToolTip1.IsBalloon = True
        Me.ToolTip1.ReshowDelay = 100
        Me.ToolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info
        Me.ToolTip1.ToolTipTitle = "Tip:"
        '
        'progBar
        '
        Me.progBar.Location = New System.Drawing.Point(84, 322)
        Me.progBar.Name = "progBar"
        Me.progBar.Size = New System.Drawing.Size(503, 29)
        Me.progBar.TabIndex = 40
        Me.progBar.Visible = False
        '
        'progLabel
        '
        Me.progLabel.AutoSize = True
        Me.progLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.progLabel.Location = New System.Drawing.Point(85, 294)
        Me.progLabel.Name = "progLabel"
        Me.progLabel.Size = New System.Drawing.Size(57, 20)
        Me.progLabel.TabIndex = 41
        Me.progLabel.Text = "Label5"
        Me.progLabel.Visible = False
        '
        'filesSaved
        '
        Me.filesSaved.AutoSize = True
        Me.filesSaved.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.filesSaved.Location = New System.Drawing.Point(163, 218)
        Me.filesSaved.Name = "filesSaved"
        Me.filesSaved.Size = New System.Drawing.Size(123, 40)
        Me.filesSaved.TabIndex = 42
        Me.filesSaved.Text = "Files/Images" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "will be saved at: "
        Me.filesSaved.Visible = False
        '
        'Download
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(144.0!, 144.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSize = True
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(640, 562)
        Me.Controls.Add(Me.filesSaved)
        Me.Controls.Add(Me.choose)
        Me.Controls.Add(Me.progLabel)
        Me.Controls.Add(Me.progBar)
        Me.Controls.Add(Me.getDatasets)
        Me.Controls.Add(Me.RadioButton2)
        Me.Controls.Add(Me.RadioButton1)
        Me.Controls.Add(Me.browseDir)
        Me.Controls.Add(Me.BackDatasets)
        Me.Controls.Add(Me.PathName)
        Me.Controls.Add(Me.Browse)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.straboToGIS)
        Me.Controls.Add(Me.backForm2)
        Me.Controls.Add(Me.Sel)
        Me.Controls.Add(Me.LogIn)
        Me.Controls.Add(Me.PasswordBox)
        Me.Controls.Add(Me.Username)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Datasets)
        Me.Controls.Add(Me.Projects)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "Download"
        Me.ShowIcon = False
        Me.Text = "Download"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Username As System.Windows.Forms.TextBox
    Friend WithEvents PasswordBox As System.Windows.Forms.TextBox
    Friend WithEvents LogIn As System.Windows.Forms.Button
    Friend WithEvents Sel As System.Windows.Forms.Label
    Friend WithEvents getDatasets As System.Windows.Forms.Button
    Friend WithEvents Datasets As System.Windows.Forms.ListBox
    Friend WithEvents Projects As System.Windows.Forms.ListBox
    Friend WithEvents backForm2 As System.Windows.Forms.Button
    Friend WithEvents straboToGIS As System.Windows.Forms.Button
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents Browse As System.Windows.Forms.Button
    Friend WithEvents PathName As System.Windows.Forms.TextBox
    Friend WithEvents BackDatasets As System.Windows.Forms.Button
    Friend WithEvents choose As System.Windows.Forms.Button
    Friend WithEvents browseDir As System.Windows.Forms.Label
    Friend WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Friend WithEvents RadioButton1 As System.Windows.Forms.RadioButton
    Friend WithEvents RadioButton2 As System.Windows.Forms.RadioButton
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents progBar As System.Windows.Forms.ProgressBar
    Friend WithEvents progLabel As System.Windows.Forms.Label
    Friend WithEvents filesSaved As System.Windows.Forms.Label
End Class
