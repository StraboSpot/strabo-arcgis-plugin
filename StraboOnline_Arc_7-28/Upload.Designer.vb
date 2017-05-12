<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Upload
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.RadioButton1 = New System.Windows.Forms.RadioButton()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.PasswordBox = New System.Windows.Forms.TextBox()
        Me.Username = New System.Windows.Forms.TextBox()
        Me.RadioButton3 = New System.Windows.Forms.RadioButton()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.LogIn = New System.Windows.Forms.Button()
        Me.back = New System.Windows.Forms.Button()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Coordinates = New System.Windows.Forms.ListBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(99, 121)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(388, 26)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Upload ArcGIS Geodatabase to Strabo"
        '
        'RadioButton1
        '
        Me.RadioButton1.AutoSize = True
        Me.RadioButton1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RadioButton1.Location = New System.Drawing.Point(136, 158)
        Me.RadioButton1.Name = "RadioButton1"
        Me.RadioButton1.Size = New System.Drawing.Size(281, 54)
        Me.RadioButton1.TabIndex = 2
        Me.RadioButton1.TabStop = True
        Me.RadioButton1.Text = "Update Existing Dataset" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(Overwrites with Versioning)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10)
        Me.RadioButton1.UseVisualStyleBackColor = True
        Me.RadioButton1.Visible = False
        '
        'Button1
        '
        Me.Button1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.Location = New System.Drawing.Point(251, 355)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 37)
        Me.Button1.TabIndex = 4
        Me.Button1.Text = "OK"
        Me.Button1.UseVisualStyleBackColor = True
        Me.Button1.Visible = False
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.Location = New System.Drawing.Point(157, 121)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(289, 26)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Create a New Strabo Project"
        Me.Label2.Visible = False
        '
        'Button2
        '
        Me.Button2.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button2.Location = New System.Drawing.Point(479, 405)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(101, 42)
        Me.Button2.TabIndex = 8
        Me.Button2.Text = "Upload"
        Me.Button2.UseVisualStyleBackColor = True
        Me.Button2.Visible = False
        '
        'ListBox1
        '
        Me.ListBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 25
        Me.ListBox1.Location = New System.Drawing.Point(172, 217)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(239, 129)
        Me.ListBox1.TabIndex = 27
        Me.ListBox1.Visible = False
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(167, 164)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(233, 50)
        Me.Label5.TabIndex = 28
        Me.Label5.Text = "        Choose Dataset(s)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "    Click For Multiple Items"
        Me.Label5.Visible = False
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(59, 264)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(98, 25)
        Me.Label8.TabIndex = 34
        Me.Label8.Text = "Password"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label9.Location = New System.Drawing.Point(59, 189)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(102, 25)
        Me.Label9.TabIndex = 33
        Me.Label9.Text = "Username"
        '
        'PasswordBox
        '
        Me.PasswordBox.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.PasswordBox.Location = New System.Drawing.Point(235, 259)
        Me.PasswordBox.Name = "PasswordBox"
        Me.PasswordBox.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.PasswordBox.Size = New System.Drawing.Size(296, 35)
        Me.PasswordBox.TabIndex = 32
        '
        'Username
        '
        Me.Username.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Username.Location = New System.Drawing.Point(235, 184)
        Me.Username.Name = "Username"
        Me.Username.Size = New System.Drawing.Size(296, 35)
        Me.Username.TabIndex = 31
        '
        'RadioButton3
        '
        Me.RadioButton3.AutoSize = True
        Me.RadioButton3.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.RadioButton3.Location = New System.Drawing.Point(136, 225)
        Me.RadioButton3.Name = "RadioButton3"
        Me.RadioButton3.Size = New System.Drawing.Size(282, 54)
        Me.RadioButton3.TabIndex = 37
        Me.RadioButton3.TabStop = True
        Me.RadioButton3.Text = "Create New Strabo Dataset" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(use existing Strabo Project)"
        Me.RadioButton3.UseVisualStyleBackColor = True
        Me.RadioButton3.Visible = False
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label6.Location = New System.Drawing.Point(150, 121)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(296, 26)
        Me.Label6.TabIndex = 38
        Me.Label6.Text = "Create a New Strabo Dataset"
        Me.Label6.Visible = False
        '
        'LogIn
        '
        Me.LogIn.AutoSize = True
        Me.LogIn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.LogIn.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LogIn.Location = New System.Drawing.Point(251, 314)
        Me.LogIn.Name = "LogIn"
        Me.LogIn.Size = New System.Drawing.Size(76, 35)
        Me.LogIn.TabIndex = 41
        Me.LogIn.Text = "Log In"
        Me.LogIn.UseVisualStyleBackColor = True
        '
        'back
        '
        Me.back.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.back.Location = New System.Drawing.Point(12, 405)
        Me.back.Name = "back"
        Me.back.Size = New System.Drawing.Size(89, 42)
        Me.back.TabIndex = 42
        Me.back.Text = "Back"
        Me.back.UseVisualStyleBackColor = True
        Me.back.Visible = False
        '
        'LinkLabel1
        '
        Me.LinkLabel1.AutoSize = True
        Me.LinkLabel1.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.LinkLabel1.Location = New System.Drawing.Point(433, 7)
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.Size = New System.Drawing.Size(153, 25)
        Me.LinkLabel1.TabIndex = 43
        Me.LinkLabel1.TabStop = True
        Me.LinkLabel1.Text = "Visit StraboSpot"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(69, 66)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(305, 25)
        Me.Label3.TabIndex = 44
        Me.Label3.Text = "Choose Coordinates For Spot,"
        Me.Label3.Visible = False
        '
        'Coordinates
        '
        Me.Coordinates.FormattingEnabled = True
        Me.Coordinates.ItemHeight = 20
        Me.Coordinates.Location = New System.Drawing.Point(189, 105)
        Me.Coordinates.Name = "Coordinates"
        Me.Coordinates.Size = New System.Drawing.Size(222, 244)
        Me.Coordinates.TabIndex = 45
        Me.Coordinates.Visible = False
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.Location = New System.Drawing.Point(380, 66)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(71, 25)
        Me.Label4.TabIndex = 46
        Me.Label4.Text = "Label4"
        Me.Label4.Visible = False
        '
        'Upload
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(144.0!, 144.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.ClientSize = New System.Drawing.Size(592, 459)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Coordinates)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.ListBox1)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.LinkLabel1)
        Me.Controls.Add(Me.RadioButton1)
        Me.Controls.Add(Me.RadioButton3)
        Me.Controls.Add(Me.back)
        Me.Controls.Add(Me.LogIn)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.PasswordBox)
        Me.Controls.Add(Me.Username)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Upload"
        Me.Text = "Upload"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents RadioButton1 As System.Windows.Forms.RadioButton
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents ListBox1 As System.Windows.Forms.ListBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents PasswordBox As System.Windows.Forms.TextBox
    Friend WithEvents Username As System.Windows.Forms.TextBox
    Friend WithEvents RadioButton3 As System.Windows.Forms.RadioButton
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents LogIn As System.Windows.Forms.Button
    Friend WithEvents back As System.Windows.Forms.Button
    Friend WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Coordinates As System.Windows.Forms.ListBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
End Class
