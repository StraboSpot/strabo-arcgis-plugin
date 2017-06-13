Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Reflection
Imports System.Web.Script.Serialization
Imports System.Object
Imports System.Windows.Forms
Imports System.Drawing

'Import necessary extensions for FileGDB-- Remember to add as refs too!
Imports ESRI.ArcGIS.DataSourcesGDB
Imports ESRI.ArcGIS.esriSystem
Imports ESRI.ArcGIS.Geodatabase
Imports ESRI.ArcGIS.Geometry
Imports System.Globalization
Imports ESRI.ArcGIS.Geoprocessing
Imports ESRI.ArcGIS.ConversionTools
Imports ESRI.ArcGIS.Geoprocessor
Imports ESRI.ArcGIS.Carto
Imports ESRI.ArcGIS.ArcMapUI
Imports ESRI.ArcGIS.ADF
Imports ESRI.ArcGIS.ADF.Connection.Local
Public Class Download

    'Declare variables shared by the subs of the Class 
    Dim projectlist As String
    Dim datasetlist As String
    Dim stringSeparators() As String = {","}
    Dim selDataset As String
    Dim selDatasetNum As String
    Dim selprojectNum As String
    Dim projectIDs As String
    Dim datasetIDs As String

    Private Sub linklabel1_Linkclicked(ByVal sender As Object, ByVal e As Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked

        Me.LinkLabel1.LinkVisited = True
        'System.Diagnostics.Process.Start("http://192.168.0.5")
        System.Diagnostics.Process.Start("https://strabospot.org")

    End Sub

    'First REST command: authorize user 
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles LogIn.Click
        'Save the user input from text boxes into the email address and password global vars.
        emailaddress = Username.Text
        password = PasswordBox.Text

        If emailaddress = "" Or password = "" Then
            Dim myMQ2 As New System.Messaging.MessageQueue(".\MyNewQueue")
            Dim message As String = "You must enter a valid username and password."
            Dim Result As DialogResult
            Result = MessageBox.Show(message)
        Else

            Dim s As HttpWebRequest
            Dim enc As UTF8Encoding
            Dim postdata As String
            Dim postdatabytes As Byte()
            Dim responseFromServer As String
            Dim reader As StreamReader
            Dim datastream As Stream
            Dim isvalid As String

            's = HttpWebRequest.Create("http://192.168.0.5/userAuthenticate")
            s = HttpWebRequest.Create("https://strabospot.org/userAuthenticate")
            enc = New System.Text.UTF8Encoding()
            postdata = "{""email"" : """ + emailaddress + """,""password"" : """ + password + """}"
            postdatabytes = enc.GetBytes(postdata)
            s.Method = "POST"
            s.ContentType = "application/json"
            s.ContentLength = postdatabytes.Length

            Using stream = s.GetRequestStream()
                stream.Write(postdatabytes, 0, postdatabytes.Length)
            End Using

            Try
                Dim result = s.GetResponse()
                datastream = result.GetResponseStream()
                reader = New StreamReader(datastream)
                responseFromServer = reader.ReadToEnd()

                Dim j As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                isvalid = j("valid")

                If isvalid = True Then
                    'Hide Log In phase elements
                    Label1.Visible = False
                    Label4.Visible = False
                    Label3.Visible = False
                    LogIn.Visible = False
                    Username.Visible = False
                    PasswordBox.Visible = False

                    'Turn on Strabo Choose Phase elements
                    Sel.Visible = True
                    getDatasets.Visible = True
                    Projects.Visible = True
                    backForm2.Visible = True
                    To_GDBpg.Visible = True
                    choose.Visible = True
                    sel_Dataset.Visible = True

                Else
                    MessageBox.Show("Incorrect Username and Password; try again")
                End If

            Catch WebException As Exception
                MessageBox.Show(WebException.Message)
            End Try

        End If

    End Sub

    'The getDatasets button actually requests the projects of the user from StraboSpot
    Private Sub getDatasets_Click(sender As Object, e As EventArgs) Handles getDatasets.Click
        If Not Projects.Items.Equals(Nothing) Then
            Projects.Items.Clear()
            projectlist = String.Empty
            projectIDs = String.Empty
        End If
        Dim s As HttpWebRequest
        Dim enc As UTF8Encoding
        Dim responseFromServer As String
        Dim reader As StreamReader
        Dim datastream As Stream
        Dim authorization As String
        Dim binaryauthorization As Byte()

        'Get Project list first- "Get My Projects" from the Strabo API

        s = HttpWebRequest.Create("https://strabospot.org/db/myProjects")
        's = HttpWebRequest.Create("http://192.168.0.5/db/myProjects")
        enc = New System.Text.UTF8Encoding()
        s.Method = "GET"
        s.ContentType = "application/json"

        authorization = emailaddress + ":" + password
        binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
        authorization = Convert.ToBase64String(binaryauthorization)
        authorization = "Basic " + authorization
        s.Headers.Add("Authorization", authorization)

        Try
            Dim result = s.GetResponse()
            datastream = result.GetResponseStream()
            reader = New StreamReader(datastream)
            responseFromServer = reader.ReadToEnd()

            Dim j As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
            j = j("projects")


            For Each i In j
                projectlist = projectlist + i("name") + "," + Environment.NewLine
                projectIDs = projectIDs + i("id").ToString() + "," + Environment.NewLine

            Next
            Datasets.Visible = False
            Projects.Visible = True

        Catch WebException As Exception
            MessageBox.Show(WebException.Message)
        End Try

        'Convert the String list into an Array and add to the List Box
        Dim projectlistArray As System.Array
        projectlistArray = projectlist.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
        Projects.Items.AddRange(projectlistArray)

    End Sub


    Private Sub backForm2_Click(sender As Object, e As EventArgs) Handles backForm2.Click
        'Back to Log In Phase elements 
        Label1.Visible = True
        Label4.Visible = True
        Label3.Visible = True
        LogIn.Visible = True
        Username.Visible = True
        PasswordBox.Visible = True
        'Hide Strabo Choose Phase elements 
        Sel.Visible = False
        getDatasets.Visible = False
        Projects.Visible = False
        Datasets.Visible = False
        sel_Dataset.Visible = False
        choose.Visible = False
        To_GDBpg.Visible = False
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Projects.SelectedIndexChanged
        If Not Datasets.Items.Equals(Nothing) Then
            Datasets.Items.Clear()
            datasetlist = String.Empty
            datasetIDs = String.Empty
        End If
        Dim selIndex As Integer
        Dim projectIDsList As System.Array
        projectIDsList = projectIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
        selIndex = Projects.SelectedIndex
        selprojectNum = projectIDsList(selIndex)
        selprojectNum = selprojectNum.Trim

        Dim s As HttpWebRequest
        Dim enc As UTF8Encoding
        Dim responseFromServer As String
        Dim reader As StreamReader
        Dim datastream As Stream
        Dim authorization As String
        Dim binaryauthorization As Byte()

        'Dim uri As String = "http://192.168.0.5/db/projectDatasets/" + selprojectNum
        Dim uri As String = "https://strabospot.org/db/projectDatasets/" + selprojectNum
        s = HttpWebRequest.Create(uri)
        enc = New System.Text.UTF8Encoding()
        s.Method = "GET"
        s.ContentType = "application/json"

        authorization = emailaddress + ":" + password
        binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
        authorization = Convert.ToBase64String(binaryauthorization)
        authorization = "Basic " + authorization
        s.Headers.Add("Authorization", authorization)

        Try
            Dim result = s.GetResponse()
            datastream = result.GetResponseStream()
            reader = New StreamReader(datastream)
            responseFromServer = reader.ReadToEnd()

            Dim d As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
            d = d("datasets")

            For Each i In d

                datasetlist = datasetlist + i("name") + "," + Environment.NewLine
                datasetIDs = datasetIDs + i("id").ToString() + "," + Environment.NewLine

            Next
            Projects.Visible = False
            Datasets.Visible = True

        Catch WebException As Exception
            MessageBox.Show(WebException.Message)
        End Try

        'Convert the dataset list to an array and add to the List Box 
        Dim datasetlistArray As System.Array
        datasetlistArray = datasetlist.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
        Datasets.Items.AddRange(datasetlistArray)
    End Sub

    Private Sub Browse_Click(sender As Object, e As EventArgs) Handles Browse.Click
        If (FolderBrowserDialog1.ShowDialog() = Windows.Forms.DialogResult.OK) Then
            PathName.Text = FolderBrowserDialog1.SelectedPath

        End If
    End Sub

    Private Sub To_GDBpg_Click(sender As Object, e As EventArgs) Handles To_GDBpg.Click
        'Turn the GDB Phase elements to visible
        PathName.Visible = True
        Browse.Visible = True
        Label2.Visible = True
        browseDir.Visible = True
        straboToGIS.Visible = True
        BackDatasets.Visible = True
        RadioButton1.Visible = True
        RadioButton2.Visible = True

        'Hide the Strabo Choose Phase elements 
        Sel.Visible = False
        getDatasets.Visible = False
        Projects.Visible = False
        Datasets.Visible = False
        backForm2.Visible = False
        To_GDBpg.Visible = False
        sel_Dataset.Visible = False
        choose.Visible = False

    End Sub

    Private Sub BackDatasets_Click(sender As Object, e As EventArgs) Handles BackDatasets.Click
        'Turn the Strabo Choose phase elements back on 
        Sel.Visible = True
        getDatasets.Visible = True
        Projects.Visible = True
        backForm2.Visible = True
        To_GDBpg.Visible = True
        sel_Dataset.Visible = True
        choose.Visible = True

        'Hide the GDB Phase elements 
        PathName.Visible = False
        Browse.Visible = False
        Label2.Visible = False
        browseDir.Visible = False
        straboToGIS.Visible = False
        BackDatasets.Visible = False

    End Sub

    Private Sub choose_Click(sender As Object, e As EventArgs) Handles choose.Click

        'Save the name of the user selected dataset 
        selDataset = Datasets.SelectedItem.ToString()

        'Display the selected dataset within the textbox
        sel_Dataset.Text = selDataset
        selDataset = selDataset.Trim()
        'Save the index of the selected dataset in order to get the dataset ID
        Dim selIndex As Integer
        Dim datasetIDsList As System.Array
        datasetIDsList = datasetIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
        selIndex = Datasets.SelectedIndex
        selDatasetNum = datasetIDsList(selIndex)
        selDatasetNum = selDatasetNum.Trim

    End Sub

    'Function patterned off of Amirian Text 
    'and http://help.arcgis.com/en/sdk/10.0/arcobjects_net/conceptualhelp/index.html#/Creating_geodatabases/0001000004t8000000/

    Public Shared Function CreateFileGDBWorkspace(ByVal path As String, ByVal dataset As String) As KeyValuePair(Of IWorkspace, String)
        'First create the file geodatabase 

        'Set name of fileGDB (name of the dataset plus a modifier)
        Dim fileGDBName As String
        dataset = dataset.Replace(" ", String.Empty)
        fileGDBName = dataset + "_" + DateTime.Today.ToString("MM_dd_yyyy", CultureInfo.InvariantCulture)
        Dim fullGDBPath As String = path + "\" + fileGDBName + ".gdb"
        If System.IO.Directory.Exists(fullGDBPath) And Not System.IO.Directory.Exists(path + "\" + fileGDBName + "_1.gdb") Then
            'For some reason (??) Arc will add the _1 automatically
            fileGDBName = fileGDBName + "_1"
        ElseIf System.IO.Directory.Exists(path + "\" + fileGDBName + "_1.gdb") And Not System.IO.Directory.Exists(path + "\" + fileGDBName + "_2.gdb") Then
            fileGDBName = fileGDBName + "_2"
        ElseIf (System.IO.Directory.Exists(path + "\" + fileGDBName + "_2.gdb")) Then   'If there is a _3 and above
            'Replace (overwrite) or keep the file and rename it (on the user to put in the appropriate string)
            'Share the pathname and database name with the RenameFile form through SharedVars class
            Dim SaveFileDialog1 As SaveFileDialog
            SaveFileDialog1 = New SaveFileDialog
            SaveFileDialog1.CreatePrompt = True
            SaveFileDialog1.OverwritePrompt = True
            SaveFileDialog1.FileName = fileGDBName + "_2"
            SaveFileDialog1.DefaultExt = "gdb"
            SaveFileDialog1.Filter = "Geodatabase Files | *.gdb"
            SaveFileDialog1.InitialDirectory = path
            SaveFileDialog1.ShowDialog()
            If (SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK) Then
                Dim userInput As String = SaveFileDialog1.FileName.ToString
                Dim fullpath() As String = userInput.Split("\")
                Dim chunk As Integer = fullpath.Length
                fileGDBName = fullpath(chunk - 1)
                fileGDBName = fileGDBName.Remove(fileGDBName.Length - 4)
                'Debug.Print(fileGDBName)
            End If

        Else
            fileGDBName = fileGDBName
        End If
        'Create the file geodatabase workspace factory 
        Dim factoryType As Type = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory")
        Dim wsFactory As IWorkspaceFactory = CType(Activator.CreateInstance(factoryType), IWorkspaceFactory)

        'Create the file geodatabase
        Dim wsName As IWorkspaceName = wsFactory.Create(path, fileGDBName, Nothing, 0) ''My.ArcMap.Application.hWnd

        'Open the geodatabase 
        Dim Name As IName = CType(wsName, IName)
        Dim ws As IWorkspace = CType(Name.Open(), IWorkspace)
        Return New KeyValuePair(Of IWorkspace, String)(ws, fileGDBName)
    End Function

    'Functions used for encoding image coordinate data
    'From https://stackoverflow.com/questions/29761026/how-do-i-convert-longitude-and-latitude-to-gps-exif-byte-array?rq=1
    'And https://stackoverflow.com/questions/11569704/converting-lat-long-gps-coordinates-to-an-exif-rational-byte-array?noredirect=1&lq=1
    Public Shared Function intToBytes(ByVal int As Int32) As Byte()
        Return BitConverter.GetBytes(int)
    End Function
    Public Shared Function dblCoordToByteArray(ByVal coordinate As Double) As Byte()
        Dim temp As Double
        Dim degrees As Int32
        Dim minutes As Int32
        Dim secondsNom As Int32
        Dim secondsDen As Int32
        Dim result(24) As Byte

        temp = Math.Abs(coordinate)
        degrees = Math.Truncate(temp)

        temp = (temp - degrees) * 60
        minutes = Math.Truncate(temp)

        temp = (temp - minutes) * 60
        secondsNom = Math.Truncate(1000 * temp)
        secondsDen = 1000

        'Get the Bytes for Degrees/1
        System.Array.Copy(intToBytes(degrees), 0, result, 0, 4)
        System.Array.Copy(intToBytes(1), 0, result, 4, 4)
        'Get the Bytes for Minutes/1
        System.Array.Copy(intToBytes(minutes), 0, result, 8, 4)
        System.Array.Copy(intToBytes(1), 0, result, 12, 4)
        'Get the Bytes for Seconds/1000
        System.Array.Copy(intToBytes(secondsNom), 0, result, 16, 4)
        System.Array.Copy(intToBytes(secondsDen), 0, result, 20, 4)

        Return result
    End Function
    'Geotagging images idea inspired by this blog post (but code sample not helpful...): https://weblogs.asp.net/zroiy/embedding-gps-coordinates-and-other-info-in-jpeg-images-with-c
    'Function inspired by this blog post: http://addgpsjpg.blogspot.com/2010/05/imports-system_1257.html#comment-form
    'Needed this: https://bytes.com/topic/visual-basic-net/answers/379499-null-terminated-strings
    'How to access different Image Property Items: https://msdn.microsoft.com/en-us/library/ms534416(v=vs.85).aspx
    'Integers used for the PropertyItem.Type Value https://msdn.microsoft.com/en-us/library/system.drawing.imaging.propertyitem.type(v=vs.110).aspx
    'Exif Tags reference: http://www.exiv2.org/tags.html
    Public Shared Function geotagPhotos(ByVal coordinates As Object, ByVal imageName As String, ByVal image As Image, ByVal imgFormat As Imaging.ImageFormat, ByVal imgDate As DateTime) As Boolean
        Dim propItem As Imaging.PropertyItem = Nothing
        Dim propItems As Imaging.PropertyItem()
        Dim imageSaved As Boolean
        'This image has no gps data in the properties
        propItems = image.PropertyItems
        Dim latExists As Boolean = False
        Dim longExists As Boolean = False
        Dim dateTimeExists As Boolean = False
        Try
            'Add in the DateTime taken- this is derrived from the ID of the image
            Dim dateTimeBytes(20) As Byte
            Dim index As Integer = 0
            For Each ch In imgDate.ToString("yyyy-MM-dd HH:mm:ss")
                dateTimeBytes(index) = Asc(ch)
                index += 1
            Next
            dateTimeBytes(index) = Asc(ControlChars.NullChar)
            For Each propItem In propItems
                If propItem.Id = 36867 Then
                    dateTimeExists = True
                    Debug.Print("the date id exists in the property items of this image")
                End If
            Next
            If dateTimeExists = False Then
                propItem.Id = 36867
                image.SetPropertyItem(propItem)
            End If
            propItem = image.GetPropertyItem(36867)
            propItem.Id = 36867
            propItem.Type = 2
            propItem.Len = dateTimeBytes.Length
            propItem.Value = dateTimeBytes
            image.SetPropertyItem(propItem)

            'Set Longitude value
            Dim longBytes As Byte() = dblCoordToByteArray(CType(coordinates(0).ToString, Double))
            For Each propItem In propItems
                If propItem.Id = 4 Then
                    longExists = True
                    Debug.Print("the long id exists in the property items of this image")
                End If
            Next
            If longExists = False Then
                propItem.Id = 4
                image.SetPropertyItem(propItem)
            End If
            propItem = image.GetPropertyItem(4)
            propItem.Id = 4
            propItem.Type = 5 'Means an array of fractions 
            propItem.Len = longBytes.Length
            propItem.Value = longBytes
            image.SetPropertyItem(propItem)

            'Set the Latitude Value 
            Dim latBytes As Byte() = dblCoordToByteArray(CType(coordinates(1).ToString, Double))
            For Each propItem In propItems
                If propItem.Id = 2 Then
                    latExists = True
                    Debug.Print("the lat id exists in the property items of this image")
                End If
            Next
            If latExists = False Then
                propItem.Id = 2
                image.SetPropertyItem(propItem)
            End If
            propItem.Id = 2
            propItem.Type = 5
            propItem.Len = latBytes.Length
            propItem.Value = latBytes
            image.SetPropertyItem(propItem)

            'Set the Long/Lat Refs
            longExists = False
            latExists = False
            Dim cardDir(2) As Byte
            'Longitude
            For Each propItem In propItems
                If propItem.Id = 3 Then
                    longExists = True
                    Debug.Print("the long id ref exists in the property items of this image")
                End If
            Next
            If longExists = False Then
                propItem.Id = 3
                image.SetPropertyItem(propItem)
            End If
            If CType(coordinates(0).ToString, Integer) < 0 Then
                Debug.Print("West longitude")
                cardDir(0) = Asc("W")
                cardDir(1) = 0
                propItem.Id = 3
                propItem.Type = 2
                propItem.Len = cardDir.Length
                propItem.Value = cardDir
                image.SetPropertyItem(propItem)
            ElseIf CType(coordinates(0).ToString, Integer) > 0 Then
                Debug.Print("East longitude")
                cardDir(0) = Asc("E")
                cardDir(1) = 0
                propItem.Id = 3
                propItem.Type = 2
                propItem.Len = cardDir.Length
                propItem.Value = cardDir
                image.SetPropertyItem(propItem)
            End If
            'Latitude
            For Each propItem In propItems
                If propItem.Id = 1 Then
                    latExists = True
                    Debug.Print("the lat id ref exists in the property items of this image")
                End If
            Next
            If CType(coordinates(1).ToString, Integer) < 0 Then
                Debug.Print("South latitude")
                cardDir(0) = Asc("S")
                cardDir(1) = 0
                propItem.Id = 1
                propItem.Type = 2
                propItem.Len = cardDir.Length
                propItem.Value = cardDir
                image.SetPropertyItem(propItem)
                image.SetPropertyItem(propItem)
            ElseIf CType(coordinates(1).ToString, Integer) > 0 Then
                Debug.Print("North latitude")
                cardDir(0) = Asc("N")
                cardDir(1) = 0
                propItem.Id = 1
                propItem.Type = 2
                propItem.Len = cardDir.Length
                propItem.Value = cardDir
                image.SetPropertyItem(propItem)
            End If
            image.Save(imageName, imgFormat)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
        If System.IO.File.Exists(imageName) Then
            imageSaved = True
        Else
            imageSaved = False
        End If
        Return imageSaved

    End Function


    Private Sub straboToGIS_Click(sender As Object, e As EventArgs) Handles straboToGIS.Click
        'First create the File Geodatabase Workspace 
        Dim pair As KeyValuePair(Of IWorkspace, String) = CreateFileGDBWorkspace(PathName.Text, selDataset)
        'Set up the ESRI JSON Files 
        Dim s As HttpWebRequest
        Dim enc As UTF8Encoding
        Dim responseFromServer As String
        Dim reader As StreamReader
        Dim datastream As Stream
        Dim authorization As String
        Dim binaryauthorization As Byte()
        Dim f As Object
        Dim fieldsURL As String
        Dim esriJson As New StringBuilder()
        Dim coord As Object
        Dim thisSpot As Object
        Dim geoproc As ESRI.ArcGIS.Geoprocessor.Geoprocessor = New ESRI.ArcGIS.Geoprocessor.Geoprocessor()
        Dim featToJson As ESRI.ArcGIS.ConversionTools.JSONToFeatures = New ESRI.ArcGIS.ConversionTools.JSONToFeatures()
        Dim makeTable As ESRI.ArcGIS.DataManagementTools.CreateTable = New ESRI.ArcGIS.DataManagementTools.CreateTable()
        Dim addFields As ESRI.ArcGIS.DataManagementTools.AddField = New ESRI.ArcGIS.DataManagementTools.AddField()
        geoproc.OverwriteOutput = True
        Dim JSONPath As String
        Dim origJsonPath As String
        Dim rockData As Object
        Dim orData As Object
        Dim _3dData As Object
        Dim sampleData As Object
        Dim imgData As Object
        Dim traceData As Object
        Dim strLine As String
        Dim parts As String()
        Dim spotID As Long
        Dim chunkNum As Integer
        Dim dt As Object = ""
        Dim assocOri As Object
        Dim aoData As New StringBuilder()
        Dim otherFeat As Object
        Dim Client As New WebClient
        Dim imgFile As String
        Dim imgID As String
        Dim spotIDs As String
        Dim sev As Object = Nothing
        Client.Credentials = New NetworkCredential(emailaddress, password)
        selprojectNum = selprojectNum.Trim


        Dim fileName As String = PathName.Text + "\" + pair.Value
        If (Not System.IO.Directory.Exists(fileName)) Then
            System.IO.Directory.CreateDirectory(fileName)
        End If

        'Set the arcpy workspace environment- important because features will need to be saved here
        Dim envPath As String
        envPath = PathName.Text + "\" + pair.Value + ".gdb"
        geoproc.SetEnvironmentValue("workspace", envPath)
        'Debug.Print(envPath)

        'Here, the code will launch a For Each statement to create three separate ESRI JSON 
        'formatted files- Point, Line, and Polygon
        'fieldsURL = "http://192.168.0.5/db/datasetFields/" + selDatasetNum
        fieldsURL = "https://strabospot.org/db/datasetFields/" + selDatasetNum
        Debug.Print(fieldsURL)

        'Save the original response with all geometry info to file 
        s = HttpWebRequest.Create("https://strabospot.org/db/datasetspotsarc/" + selDatasetNum)
        's = HttpWebRequest.Create("http://192.168.0.5/db/datasetspotsarc/" + selDatasetNum)
        enc = New System.Text.UTF8Encoding()
        s.Method = "GET"
        s.ContentType = "application/json"

        authorization = emailaddress + ":" + password
        binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
        authorization = Convert.ToBase64String(binaryauthorization)
        authorization = "Basic " + authorization
        s.Headers.Add("Authorization", authorization)

        Try
            Dim result = s.GetResponse()
            datastream = result.GetResponseStream()
            reader = New StreamReader(datastream)
            responseFromServer = reader.ReadToEnd()

            'Save the original GeoJSON response from the server  
            origJsonPath = fileName + "\dataset-" + selDatasetNum + ".json"
            System.IO.File.WriteAllText(origJsonPath, responseFromServer)

        Catch WebException As Exception
            MessageBox.Show(WebException.Message)
        End Try

        Dim geometries As ArrayList = New ArrayList()
        geometries.Add("point")
        geometries.Add("line")
        geometries.Add("polygon")

        For Each geometry In geometries
            '//////////////////////////////////////////////////////POINTS BEGIN/////////////////////////////////////////
            'Write the rest of the Point File 
            If geometry.Equals("point") Then

                'Make the request for Fields
                s = HttpWebRequest.Create(fieldsURL + "/point")
                enc = New System.Text.UTF8Encoding()
                s.Method = "GET"
                s.ContentType = "application/json"

                authorization = emailaddress + ":" + password
                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                authorization = Convert.ToBase64String(binaryauthorization)
                authorization = "Basic " + authorization
                s.Headers.Add("Authorization", authorization)

                Try
                    Dim result = s.GetResponse()
                    datastream = result.GetResponseStream()
                    reader = New StreamReader(datastream)
                    responseFromServer = reader.ReadToEnd()

                    Debug.Print(responseFromServer)

                    f = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)

                Catch WebException As Exception
                    MessageBox.Show(WebException.Message)
                End Try

                If f IsNot Nothing Then 'Not Null 

                    'Start the ESRI JSON formatting 
                    esriJson.Append("{" + Environment.NewLine + """displayFieldName"" : " + """" + selDataset + """" + "," + Environment.NewLine)
                    esriJson.Append("""fieldAliases"" : {" + Environment.NewLine)
                    esriJson.Append("""FID"" : ""FID""," + Environment.NewLine)

                    For Each i In f
                        'Add each field to the Field Aliases array 
                        If i.Equals("trace") Or i.Equals("id") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("""_3d_structures_type"" : ""_3d_structures_type""," + Environment.NewLine)
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("""other_type"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""other_description"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""other_name"" : ""other_name""," + Environment.NewLine)
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("""rock_unit_notes"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""rock_unit_description"" : ""rock_unit_description""," + Environment.NewLine)
                        Else
                            esriJson.Append("""" + i + """ : """ + i + """," + Environment.NewLine)
                        End If
                        'Debug.Print(i)
                    Next
                    esriJson.Append("""SpotID"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""FeatID"" : ""FeatID""," + Environment.NewLine)
                    'Complete the rest of the Points File 
                    esriJson.Remove(esriJson.Length - 3, 3)
                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                    esriJson.Append("""geometryType"" : ""esriGeometryPoint""," + Environment.NewLine)
                    esriJson.Append("""spatialReference"" : {" + Environment.NewLine)
                    esriJson.Append("""wkid"" : 4326," + Environment.NewLine)
                    esriJson.Append("""latestWkid"" : 4326" + Environment.NewLine)
                    esriJson.Append("}," + Environment.NewLine)
                    'Set up the Fields Array
                    esriJson.Append("""fields"" : [" + Environment.NewLine)
                    esriJson.Append("{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""FID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeOID""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""FID""" + Environment.NewLine)
                    esriJson.Append("}")

                    'Add the fields to the array 
                    For Each i In f
                        If i.Equals("trace") Or i.Equals("id") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""_3d_structures_type""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""_3d_structures_type""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_name""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_name""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")

                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_description""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_description""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.ToString.Equals("description") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.ToString.Contains("notes") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")
                        Else
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        End If
                    Next
                    esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine + """name"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""SpotID""" + Environment.NewLine)
                    esriJson.Append("}," + Environment.NewLine + "{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""FeatID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""FeatID""" + Environment.NewLine)
                    esriJson.Append("}")

                    'Write all the Spots of type point to Features array
                    esriJson.Append(Environment.NewLine + "]," + Environment.NewLine)
                    esriJson.Append("""features"" : [" + Environment.NewLine)
                    s = HttpWebRequest.Create("https://strabospot.org/db/datasetspotsarc/" + selDatasetNum + "/point")
                    's = HttpWebRequest.Create("http://192.168.0.5/db/datasetspotsarc/" + selDatasetNum + "/point")
                    enc = New System.Text.UTF8Encoding()
                    s.Method = "GET"
                    s.ContentType = "application/json"

                    authorization = emailaddress + ":" + password
                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                    authorization = Convert.ToBase64String(binaryauthorization)
                    authorization = "Basic " + authorization
                    s.Headers.Add("Authorization", authorization)

                    Try
                        Dim result = s.GetResponse()
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()

                        'Debug.Print(responseFromServer)

                        Dim sp As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        sp = sp("features")

                        Dim FIDNum As Integer = 1

                        For Each spot In sp
                            esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                            thisSpot = spot("properties")
                            esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                            FIDNum += 1

                            coord = spot("geometry")("coordinates")
                            spotID = thisSpot("id")
                            spotIDs += spotID.ToString + ","
                            esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")

                            'Get basic values 
                            For Each line In thisSpot
                                If Not line.ToString.Contains("System.Object") Then
                                    'Debug.Print(line.ToString)
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    If Not parts(0).Equals("self") Then
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    End If
                                Else
                                    Continue For
                                End If
                            Next

                            'For Each i In rootDataList

                            '    If thisSpot.ContainsKey(i) Then
                            '        thisVal = thisSpot(i).ToString
                            '    Else
                            '        Continue For
                            '    End If
                            '    esriJson.Append("," + Environment.NewLine + """" + i + """: """ + thisVal + """")

                            'Next

                            'Check for values from other keys (subcategories)
                            If thisSpot.ContainsKey("rock_unit") Then
                                rockData = thisSpot("rock_unit")
                                Dim line As Object
                                For Each line In rockData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    If parts(0).Equals("notes") Then
                                        parts(0) = "rock_unit_notes"
                                    End If
                                    If parts(0).Equals("description") Then
                                        parts(0) = "rock_unit_description"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                Next
                            End If

                            If thisSpot.ContainsKey("trace") Then
                                traceData = thisSpot("trace")
                                Dim line As Object
                                For Each line In traceData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                Next
                            End If

                            esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                            esriJson.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                            esriJson.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                            esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)

                            If thisSpot.containskey("orientation_data") Then
                                chunkNum = 0
                                orData = thisSpot("orientation_data")
                                For Each chunk In orData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        If line.ToString.Contains("associated_orientation") Then
                                            assocOri = orData(chunkNum)("associated_orientation")
                                            For Each block In assocOri
                                                FIDNum += 1
                                                'Start a new attribute for the associated orientation
                                                aoData.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                                aoData.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                                aoData.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                                For Each l In block
                                                    strLine = l.ToString().Trim("[", "]").Trim
                                                    parts = strLine.Split(New Char() {","}, 2)
                                                    If String.IsNullOrEmpty(parts(1)) Then
                                                        parts(1) = ""
                                                    Else
                                                        parts(1) = Replace(parts(1), vbLf, "")
                                                        parts(1) = Replace(parts(1), """", "'")
                                                    End If
                                                    If parts(0).Equals("id") Then
                                                        parts(0) = "FeatID"
                                                    End If
                                                    aoData.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                                Next
                                                aoData.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                                aoData.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                                                aoData.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                                                aoData.Append(Environment.NewLine + "}," + Environment.NewLine)
                                            Next
                                        ElseIf line.ToString.Contains("System.Object") Then
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            If String.IsNullOrEmpty(parts(1)) Then
                                                parts(1) = ""
                                            Else
                                                parts(1) = Replace(parts(1), vbLf, "")
                                                parts(1) = Replace(parts(1), """", "'")
                                            End If
                                            Dim elementList As String
                                            For Each i In orData(chunkNum)(parts(0))
                                                elementList = elementList + i + ", "
                                            Next
                                            elementList.TrimEnd(", ")
                                            If parts(0).Equals("id") Then
                                                parts(0) = "FeatID"
                                            End If
                                            ' Debug.Print(elementList)
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + elementList + """")
                                        Else
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            If String.IsNullOrEmpty(parts(1)) Then
                                                parts(1) = ""
                                            Else
                                                parts(1) = Replace(parts(1), vbLf, "")
                                                parts(1) = Replace(parts(1), """", "'")
                                            End If
                                            If parts(0).Equals("id") Then
                                                parts(0) = "FeatID"
                                            End If
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                        End If
                                    Next
                                    If esriJson.ToString.EndsWith(Environment.NewLine) Then
                                        Continue For
                                    Else
                                        esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                        esriJson.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                                        esriJson.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                                        esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                                    End If
                                    chunkNum += 1
                                    FIDNum += 1
                                    'Add in associated orientation(s) before moving to the next orientation
                                    esriJson.Append(aoData.ToString)
                                    aoData.Length = 0
                                Next
                            End If

                            If thisSpot.containskey("samples") Then
                                chunkNum = 0
                                sampleData = thisSpot("samples")
                                For Each chunk In sampleData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                                    esriJson.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If

                            If thisSpot.containskey("_3d_structures") Then
                                chunkNum = 0
                                _3dData = thisSpot("_3d_structures")
                                For Each chunk In _3dData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("type") Then
                                            parts(0) = "_3d_structures_type"
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                                    esriJson.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If

                            If thisSpot.ContainsKey("images") Then
                                chunkNum = 0
                                imgData = thisSpot("images")
                                For Each chunk In imgData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            imgID = parts(1).Trim
                                            'parts(0) = "FeatID"
                                            'Debug.Print(imgID)
                                        ElseIf parts(0).Equals("self") Then
                                            'Change to https for older datasets (before server switch)
                                            'parts(1) = parts(1).Trim()
                                            'Dim urlSplit As String() = parts(1).Split(New Char() {":"})
                                            'If urlSplit(0).Equals("http") Then
                                            '    parts(1) = Replace(parts(1), "http", "https")
                                            'End If
                                            'Debug.Print("Image URL: " + parts(1))
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                            Dim statusCode As String = ""
                                            Dim image As Image
                                            Dim imageResult As HttpWebResponse
                                            Dim imgSavedResult As Boolean
                                            Dim timeStamp As Int64 = CType(imgID.Remove(imgID.Length - 1, 1), Int64)    'Get rid of the last digit 
                                            Dim imgDateTime As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(timeStamp)
                                            'Download the image to same file Json files are saved as a .Tiff
                                            If RadioButton1.Checked Then
                                                imgFile = fileName + "\" + imgID + ".tiff"
                                                Debug.Print(imgFile)
                                                s = HttpWebRequest.Create("https://strabospot.org/db/image/" + imgID)
                                                's = HttpWebRequest.Create("http://192.168.0.5/db/image/" + imgID)
                                                's = HttpWebRequest.Create(parts(1))
                                                enc = New System.Text.UTF8Encoding()
                                                s.Method = "GET"
                                                s.ContentType = "application/json"
                                                authorization = emailaddress + ":" + password
                                                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                                                authorization = Convert.ToBase64String(binaryauthorization)
                                                authorization = "Basic " + authorization
                                                s.Headers.Add("Authorization", authorization)
                                                Try
                                                    imageResult = CType(s.GetResponse(), HttpWebResponse)
                                                    statusCode = imageResult.StatusCode.ToString
                                                    If statusCode.Equals("OK") Then
                                                        image = System.Drawing.Image.FromStream(imageResult.GetResponseStream)
                                                        'imgSavedResult = geotagPhotos(coord, imgFile, image, Imaging.ImageFormat.Tiff, imgDateTime)
                                                        image.Save(imgFile, Imaging.ImageFormat.Tiff)
                                                        'Debug.Print("Image Saved? " + imgSavedResult.ToString)
                                                    End If
                                                Catch WebException As Exception
                                                    MessageBox.Show(WebException.Message)
                                                End Try
                                                    'Debug.Print(imgFile)
                                                    'Client.DownloadFile(parts(1), imgFile)
                                                ElseIf RadioButton2.Checked Then    'Save to the same file as the Json files as a .JPEG
                                                    imgFile = fileName + "\" + imgID + ".jpeg"
                                                    Debug.Print(imgFile)
                                                    's = HttpWebRequest.Create(parts(1))
                                                    s = HttpWebRequest.Create("https://strabospot.org/db/image/" + imgID)
                                                    's = HttpWebRequest.Create("http://192.168.0.5/db/image/" + imgID)
                                                    enc = New System.Text.UTF8Encoding()
                                                    s.Method = "GET"
                                                    s.ContentType = "application/json"
                                                    authorization = emailaddress + ":" + password
                                                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                                                    authorization = Convert.ToBase64String(binaryauthorization)
                                                    authorization = "Basic " + authorization
                                                    s.Headers.Add("Authorization", authorization)
                                                    Try
                                                        imageResult = CType(s.GetResponse(), HttpWebResponse)
                                                        statusCode = imageResult.StatusCode.ToString
                                                        If statusCode.Equals("OK") Then
                                                        image = System.Drawing.Image.FromStream(imageResult.GetResponseStream)
                                                        imgSavedResult = geotagPhotos(coord, imgFile, image, Imaging.ImageFormat.Jpeg, imgDateTime)
                                                        Debug.Print("Image Saved? " + imgSavedResult.ToString)
                                                        End If
                                                    Catch WebException As Exception
                                                        MessageBox.Show(WebException.Message)
                                                    End Try
                                                End If
                                        Else
                                            Continue For
                                        End If
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                                    esriJson.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If

                            If thisSpot.ContainsKey("other_features") Then
                                chunkNum = 0
                                otherFeat = thisSpot("other_features")
                                For Each chunk In otherFeat
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        If parts(0).Equals("type") Then
                                            parts(0) = "other_type"
                                        End If
                                        If parts(0).Equals("description") Then
                                            parts(0) = "other_description"
                                        End If
                                        If parts(0).Equals("name") Then
                                            parts(0) = "other_name"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                                    esriJson.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If
                        Next

                        'Needs to be reevaluated due to inner 'relationships' array... Does it really belong in Arc??
                        '    If thisSpot.ContainsKey("inferences") Then
                        '        infData = thisSpot("inference")
                        '        For Each line In infData
                        '            strLine = line.ToString().Trim("[", "]")
                        '            parts = strLine.Split(New Char() {","})
                        '            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
                        '        Next
                        '    End If
                        'Next

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    esriJson.Remove(esriJson.Length - 3, 3)

                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "}")

                    'Save the ESRI Formatted Json in the same folder as the Original GeoJson   
                    If (System.IO.Directory.Exists(fileName)) Then
                        JSONPath = fileName + "\arcJSONpts.json"
                        'Debug.Print(JSONPath)
                        System.IO.File.WriteAllText(JSONPath, esriJson.ToString())
                    End If

                    esriJson.Length = 0

                    'call the jsontofeatures_conversion tool in order to populate the file gdb- which was set as the workspace
                    'based on the instuctions given at: http://resources.arcgis.com/en/help/arcobjects-net/conceptualhelp/index.html#//0001000003rr000000
                    featToJson.in_json_file = JSONPath
                    featToJson.out_features = System.IO.Path.Combine(envPath, "points")

                    Try
                        geoproc.Execute(featToJson, Nothing)
                        Console.WriteLine(geoproc.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(geoproc.GetMessages(sev))
                    End Try

                    If (geoproc.Exists(envPath + "\points", dt)) Then
                        MessageBox.Show("Points Feature Class Successfully Created!")
                    Else
                        MessageBox.Show("Error loading Points Feature Class...")
                    End If

                End If
                '///////////////////////////////////////////////END POINTS//////////////////////////////////////////////
                '//////////////////////////////////////////////////////////////////////////////////////////////////////

                '/////////////////////////////////////////////BEGIN LINES//////////////////////////////////////////////
                'Create a Line JSON file 
            ElseIf geometry.Equals("line") Then

                'Make the request for Fields
                s = HttpWebRequest.Create(fieldsURL + "/line")
                enc = New System.Text.UTF8Encoding()
                s.Method = "GET"
                s.ContentType = "application/json"

                authorization = emailaddress + ":" + password
                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                authorization = Convert.ToBase64String(binaryauthorization)
                authorization = "Basic " + authorization
                s.Headers.Add("Authorization", authorization)

                Try
                    Dim result = s.GetResponse()
                    datastream = result.GetResponseStream()
                    reader = New StreamReader(datastream)
                    responseFromServer = reader.ReadToEnd()

                    'Debug.Print(responseFromServer)

                    f = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)

                Catch WebException As Exception
                    MessageBox.Show(WebException.Message)
                End Try

                If f IsNot Nothing Then 'Not Null 

                    'Start the ESRI JSON formatting 
                    esriJson.Append("{" + Environment.NewLine + """displayFieldName"" : " + """" + selDataset + """" + "," + Environment.NewLine)
                    esriJson.Append("""fieldAliases"" : {" + Environment.NewLine)
                    esriJson.Append("""FID"" : ""FID""," + Environment.NewLine)

                    For Each i In f
                        'Add each field to the Field Aliases array 
                        If i.Equals("trace") Or i.Equals("id") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("""_3d_structures_type"" : ""_3d_structures_type""," + Environment.NewLine)
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("""other_type"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""other_description"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""other_name"" : ""other_name""," + Environment.NewLine)
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("""rock_unit_notes"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""rock_unit_description"" : ""rock_unit_description""," + Environment.NewLine)
                        Else
                            esriJson.Append("""" + i + """ : """ + i + """," + Environment.NewLine)
                        End If
                        'Debug.Print(i)
                    Next
                    esriJson.Append("""SpotID"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""FeatID"" : ""FeatID""," + Environment.NewLine)
                    'Complete the rest of the Line File 
                    esriJson.Remove(esriJson.Length - 3, 3)
                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                    esriJson.Append("""geometryType"" : ""esriGeometryPolyline""," + Environment.NewLine)
                    esriJson.Append("""spatialReference"" : {" + Environment.NewLine)
                    esriJson.Append("""wkid"" : 4326," + Environment.NewLine)
                    esriJson.Append("""latestWkid"" : 4326" + Environment.NewLine)
                    esriJson.Append("}," + Environment.NewLine)
                    'Set up the Fields Array
                    esriJson.Append("""fields"" : [" + Environment.NewLine)
                    esriJson.Append("{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""FID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeOID""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""FID""" + Environment.NewLine)
                    esriJson.Append("}")

                    'Add the fields to the array 
                    For Each i In f
                        If i.Equals("trace") Or i.Equals("id") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""_3d_structures_type""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""_3d_structures_type""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_name""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_name""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_description""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_description""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.ToString.Contains("notes") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")
                        Else
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        End If
                    Next
                    esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine + """name"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""SpotID""" + Environment.NewLine)
                    esriJson.Append("}," + Environment.NewLine + "{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""FeatID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""FeatID""" + Environment.NewLine)
                    esriJson.Append("}")
                    'Write all the Spots of type point to Features array
                    esriJson.Append(Environment.NewLine + "]," + Environment.NewLine)
                    esriJson.Append("""features"" : [" + Environment.NewLine)

                    s = HttpWebRequest.Create("https://strabospot.org/db/datasetspotsarc/" + selDatasetNum + "/line")
                    's = HttpWebRequest.Create("http://192.168.0.5/db/datasetspotsarc/" + selDatasetNum + "/line")
                    enc = New System.Text.UTF8Encoding()
                    s.Method = "GET"
                    s.ContentType = "application/json"

                    authorization = emailaddress + ":" + password
                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                    authorization = Convert.ToBase64String(binaryauthorization)
                    authorization = "Basic " + authorization
                    s.Headers.Add("Authorization", authorization)

                    Try
                        Dim result = s.GetResponse()
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()

                        'Debug.Print(responseFromServer)

                        Dim sp As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        sp = sp("features")

                        Dim FIDNum As Integer = 1

                        For Each spot In sp
                            esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                            thisSpot = spot("properties")
                            esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                            FIDNum += 1

                            coord = spot("geometry")("coordinates")
                            spotID = thisSpot("id")
                            spotIDs += spotID.ToString + ","
                            esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                            'Check for any root values (single line- not nested)
                            'Get basic values 
                            For Each line In thisSpot
                                If Not line.ToString.Contains("System.Object") Then
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    If Not parts(0).Equals("self") Then
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    End If
                                Else
                                    Continue For
                                End If
                            Next

                            'For Each i In rootDataList

                            '    If thisSpot.ContainsKey(i) Then

                            '        thisVal = thisSpot(i).ToString
                            '    Else
                            '        Continue For
                            '    End If
                            '    esriJson.Append("," + Environment.NewLine + """" + i + """: """ + thisVal + """")
                            'Next

                            If thisSpot.ContainsKey("rock_unit") Then
                                rockData = thisSpot("rock_unit")
                                Dim line As Object
                                For Each line In rockData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    If parts(0).Equals("notes") Then
                                        parts(0) = "rock_unit_notes"
                                    End If
                                    If parts(0).Equals("description") Then
                                        parts(0) = "rock_unit_description"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                Next
                            End If

                            If thisSpot.ContainsKey("trace") Then
                                traceData = thisSpot("trace")
                                Dim line As Object
                                For Each line In traceData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                Next
                            End If

                            esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                            esriJson.Append("""paths"": [" + Environment.NewLine + "[" + Environment.NewLine)
                            For Each line In coord
                                esriJson.Append("[" + line(0).ToString + "," + Environment.NewLine)
                                esriJson.Append(line(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                            Next
                            esriJson.Remove(esriJson.Length - 3, 3)
                            esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                        + "}" + Environment.NewLine + "}," + Environment.NewLine)

                            If thisSpot.containskey("orientation_data") Then
                                chunkNum = 0
                                orData = thisSpot("orientation_data")
                                For Each chunk In orData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        If line.ToString.Contains("associated_orientation") Then
                                            assocOri = orData(chunkNum)("associated_orientation")
                                            For Each block In assocOri
                                                FIDNum += 1
                                                'Start a new attribute for the associated orientation data
                                                aoData.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                                aoData.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                                aoData.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                                For Each l In block
                                                    strLine = l.ToString().Trim("[", "]").Trim
                                                    parts = strLine.Split(New Char() {","}, 2)
                                                    If String.IsNullOrEmpty(parts(1)) Then
                                                        parts(1) = ""
                                                    Else
                                                        parts(1) = Replace(parts(1), vbLf, "")
                                                        parts(1) = Replace(parts(1), """", "'")
                                                    End If
                                                    If parts(0).Equals("id") Then
                                                        parts(0) = "FeatID"
                                                    End If
                                                    aoData.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                                Next
                                                aoData.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                                aoData.Append("""paths"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                                For Each c In coord
                                                    aoData.Append("[" + c(0).ToString + "," + Environment.NewLine)
                                                    aoData.Append(c(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                                Next
                                                aoData.Remove(aoData.Length - 3, 3)
                                                aoData.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                            + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                                'Debug.Print(aoData.ToString)
                                            Next
                                        ElseIf line.ToString.Contains("System.Object") Then
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            If String.IsNullOrEmpty(parts(1)) Then
                                                parts(1) = ""
                                            Else
                                                parts(1) = Replace(parts(1), vbLf, "")
                                                parts(1) = Replace(parts(1), """", "'")
                                            End If
                                            Dim elementList As String
                                            For Each i In orData(chunkNum)(parts(0))
                                                elementList = elementList + i + ", "
                                            Next
                                            elementList.TrimEnd(", ")
                                            If parts(0).Equals("id") Then
                                                parts(0) = "FeatID"
                                            End If
                                            'Debug.Print(elementList)
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + elementList + """")
                                        Else
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            If String.IsNullOrEmpty(parts(1)) Then
                                                parts(1) = ""
                                            Else
                                                parts(1) = Replace(parts(1), vbLf, "")
                                                parts(1) = Replace(parts(1), """", "'")
                                            End If
                                            If parts(0).Equals("id") Then
                                                parts(0) = "FeatID"
                                            End If
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                        End If
                                    Next
                                    If esriJson.ToString.EndsWith(Environment.NewLine) Then
                                        Continue For
                                    Else
                                        esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                        esriJson.Append("""paths"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                        For Each line In coord
                                            esriJson.Append("[" + line(0).ToString + "," + Environment.NewLine)
                                            esriJson.Append(line(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                        Next
                                        esriJson.Remove(esriJson.Length - 3, 3)
                                        esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                    + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    End If
                                    chunkNum += 1
                                    FIDNum += 1
                                    'Add in the associated orientation(s) before moving to the next orientation 
                                    esriJson.Append(aoData.ToString)
                                    aoData.Length = 0
                                Next
                            End If
                            If thisSpot.containskey("samples") Then
                                chunkNum = 0
                                sampleData = thisSpot("samples")
                                For Each chunk In sampleData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""paths"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                    For Each line In coord
                                        esriJson.Append("[" + line(0).ToString + "," + Environment.NewLine)
                                        esriJson.Append(line(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If
                            If thisSpot.containskey("_3d_structures") Then
                                chunkNum = 0
                                _3dData = thisSpot("_3d_structures")
                                For Each chunk In _3dData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        If parts(0).Equals("type") Then
                                            parts(0) = "_3d_structures_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""paths"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                    For Each line In coord
                                        esriJson.Append("[" + line(0).ToString + "," + Environment.NewLine)
                                        esriJson.Append(line(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If
                            If thisSpot.ContainsKey("images") Then
                                chunkNum = 0
                                imgData = thisSpot("images")
                                For Each chunk In imgData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            imgID = parts(1).Trim
                                            'Debug.Print(imgID)
                                        ElseIf parts(0).Equals("self") Then
                                            'Change to https for older datasets (before server switch)
                                            'parts(1) = parts(1).Trim()
                                            'Dim urlSplit As String() = parts(1).Split(New Char() {":"})
                                            'If urlSplit(0).Equals("http") Then
                                            '    parts(1) = Replace(parts(1), "http", "https")
                                            'End If
                                            'Debug.Print("Image URL: " + parts(1))
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                            Dim statusCode As String = ""
                                            Dim image As Image
                                            Dim imageResult As HttpWebResponse
                                            'Download the image to same file Json files are saved as a .Tiff
                                            If RadioButton1.Checked Then
                                                imgFile = fileName + "\" + imgID + ".tiff"
                                                Debug.Print(imgFile)
                                                's = HttpWebRequest.Create(parts(1))
                                                s = HttpWebRequest.Create("https://strabospot.org/db/image/" + imgID)
                                                's = HttpWebRequest.Create("http://192.168.0.5/db/image/" + imgID)
                                                enc = New System.Text.UTF8Encoding()
                                                s.Method = "GET"
                                                s.ContentType = "application/json"
                                                authorization = emailaddress + ":" + password
                                                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                                                authorization = Convert.ToBase64String(binaryauthorization)
                                                authorization = "Basic " + authorization
                                                s.Headers.Add("Authorization", authorization)
                                                Try
                                                    imageResult = CType(s.GetResponse(), HttpWebResponse)
                                                    statusCode = imageResult.StatusCode.ToString
                                                    If statusCode.Equals("OK") Then
                                                        image = System.Drawing.Image.FromStream(imageResult.GetResponseStream)
                                                        image.Save(imgFile, System.Drawing.Imaging.ImageFormat.Tiff)
                                                    End If
                                                Catch WebException As Exception
                                                    MessageBox.Show(WebException.Message)
                                                End Try
                                                'Debug.Print(imgFile)
                                                'Client.DownloadFile(parts(1), imgFile)
                                            ElseIf RadioButton2.Checked Then    'Save to the same file as the Json files as a .JPEG
                                                imgFile = fileName + "\" + imgID + ".jpeg"
                                                Debug.Print(imgFile)
                                                's = HttpWebRequest.Create(parts(1))
                                                s = HttpWebRequest.Create("https://strabospot.org/db/image/" + imgID)
                                                's = HttpWebRequest.Create("http://192.168.0.5/db/image/" + imgID)
                                                enc = New System.Text.UTF8Encoding()
                                                s.Method = "GET"
                                                s.ContentType = "application/json"
                                                authorization = emailaddress + ":" + password
                                                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                                                authorization = Convert.ToBase64String(binaryauthorization)
                                                authorization = "Basic " + authorization
                                                s.Headers.Add("Authorization", authorization)
                                                Try
                                                    imageResult = CType(s.GetResponse(), HttpWebResponse)
                                                    statusCode = imageResult.StatusCode.ToString
                                                    If statusCode.Equals("OK") Then
                                                        image = System.Drawing.Image.FromStream(imageResult.GetResponseStream)
                                                        image.Save(imgFile, System.Drawing.Imaging.ImageFormat.Jpeg)
                                                    End If
                                                Catch WebException As Exception
                                                    MessageBox.Show(WebException.Message)
                                                End Try
                                            End If
                                        Else
                                            Continue For
                                        End If
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""paths"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                    For Each line In coord
                                        esriJson.Append("[" + line(0).ToString + "," + Environment.NewLine)
                                        esriJson.Append(line(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If

                            If thisSpot.ContainsKey("other_features") Then
                                chunkNum = 0
                                otherFeat = thisSpot("other_features")
                                For Each chunk In otherFeat
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        If parts(0).Equals("type") Then
                                            parts(0) = "other_type"
                                        End If
                                        If parts(0).Equals("description") Then
                                            parts(0) = "other_description"
                                        End If
                                        If parts(0).Equals("name") Then
                                            parts(0) = "other_name"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""paths"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                    For Each line In coord
                                        esriJson.Append("[" + line(0).ToString + "," + Environment.NewLine)
                                        esriJson.Append(line(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If
                        Next

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    esriJson.Remove(esriJson.Length - 3, 3)

                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "}")

                    'Create a JSON File on the user's computer 
                    If (System.IO.Directory.Exists(fileName)) Then
                        JSONPath = fileName + "\arcJSONlines.json"
                        'Debug.Print(JSONPath)
                        System.IO.File.WriteAllText(JSONPath, esriJson.ToString())
                    End If

                    esriJson.Length = 0

                    featToJson.in_json_file = JSONPath
                    featToJson.out_features = System.IO.Path.Combine(envPath, "lines")

                    Try
                        geoproc.Execute(featToJson, Nothing)
                        Console.WriteLine(geoproc.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(geoproc.GetMessages(sev))
                    End Try


                    If (geoproc.Exists(envPath + "\lines", dt)) Then
                        MessageBox.Show("Lines Feature Class Successfully Created!")
                    Else
                        MessageBox.Show("Error loading Lines Feature Class...")
                    End If

                End If
                '//////////////////////////////////////////END LINES/////////////////////////////////////////////
                '////////////////////////////////////////////////////////////////////////////////////////////////

                '//////////////////////////////////////BEGIN POLYGONS///////////////////////////////////////////
                '///////////////////////////////////////////////////////////////////////////////////////////////
                'Create a Polygon JSON File 
            ElseIf geometry.Equals("polygon") Then

                'Make the request for Fields
                s = HttpWebRequest.Create(fieldsURL + "/polygon")
                enc = New System.Text.UTF8Encoding()
                s.Method = "GET"
                s.ContentType = "application/json"

                authorization = emailaddress + ":" + password
                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                authorization = Convert.ToBase64String(binaryauthorization)
                authorization = "Basic " + authorization
                s.Headers.Add("Authorization", authorization)

                Try
                    Dim result = s.GetResponse()
                    datastream = result.GetResponseStream()
                    reader = New StreamReader(datastream)
                    responseFromServer = reader.ReadToEnd()

                    'Debug.Print(responseFromServer)

                    f = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)


                Catch WebException As Exception
                    MessageBox.Show(WebException.Message)
                End Try

                If f IsNot Nothing Then 'Not Null 

                    'Start the ESRI JSON formatting 
                    esriJson.Append("{" + Environment.NewLine + """displayFieldName"" : " + """" + selDataset + """" + "," + Environment.NewLine)
                    esriJson.Append("""fieldAliases"" : {" + Environment.NewLine)
                    esriJson.Append("""FID"" : ""FID""," + Environment.NewLine)

                    For Each i In f
                        'Add each field to the Field Aliases array 
                        If i.Equals("trace") Or i.Equals("id") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("""_3d_structures_type"" : ""_3d_structures_type""," + Environment.NewLine)
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("""other_type"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""other_description"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""other_name"" : ""other_name""," + Environment.NewLine)
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("""rock_unit_notes"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""rock_unit_description"" : ""rock_unit_description""," + Environment.NewLine)
                        Else
                            esriJson.Append("""" + i + """ : """ + i + """," + Environment.NewLine)
                        End If
                        'Debug.Print(i)
                    Next
                    esriJson.Append("""SpotID"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""FeatID"" : ""FeatID""," + Environment.NewLine)
                    'Complete the rest of the Points File 
                    esriJson.Remove(esriJson.Length - 3, 3)
                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine)
                    esriJson.Append("""geometryType"" : ""esriGeometryPolygon""," + Environment.NewLine)
                    esriJson.Append("""spatialReference"" : {" + Environment.NewLine)
                    esriJson.Append("""wkid"" : 4326," + Environment.NewLine)
                    esriJson.Append("""latestWkid"" : 4326" + Environment.NewLine)
                    esriJson.Append("}," + Environment.NewLine)
                    'Set up the Fields Array
                    esriJson.Append("""fields"" : [" + Environment.NewLine)
                    esriJson.Append("{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""FID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeOID""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""FID""" + Environment.NewLine)
                    esriJson.Append("}")

                    'Add the fields to the array 
                    For Each i In f
                        If i.Equals("trace") Or i.Equals("id") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""_3d_structures_type""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""_3d_structures_type""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_type""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_description""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""other_name""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""other_name""," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")

                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_description""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_description""," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")
                        ElseIf i.ToString.Contains("notes") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""length"" : 1024" + Environment.NewLine)
                            esriJson.Append("}")
                        Else
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : """ + i + """," + Environment.NewLine)
                            esriJson.Append("""length"" : 160" + Environment.NewLine)
                            esriJson.Append("}")
                        End If
                    Next
                    esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine + """name"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""SpotID""" + Environment.NewLine)
                    esriJson.Append("}," + Environment.NewLine + "{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""FeatID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""FeatID""" + Environment.NewLine)
                    esriJson.Append("}")
                    'Write all the Spots of type point to Features array
                    esriJson.Append(Environment.NewLine + "]," + Environment.NewLine)
                    esriJson.Append("""features"" : [" + Environment.NewLine)
                    s = HttpWebRequest.Create("https://strabospot.org/db/datasetspotsarc/" + selDatasetNum + "/polygon")
                    's = HttpWebRequest.Create("http://192.168.0.5/db/datasetspotsarc/" + selDatasetNum + "/polygon")
                    enc = New System.Text.UTF8Encoding()
                    s.Method = "GET"
                    s.ContentType = "application/json"

                    authorization = emailaddress + ":" + password
                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                    authorization = Convert.ToBase64String(binaryauthorization)
                    authorization = "Basic " + authorization
                    s.Headers.Add("Authorization", authorization)

                    Try
                        Dim result = s.GetResponse()
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()

                        'Debug.Print(responseFromServer)

                        Dim sp As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        sp = sp("features")

                        Dim FIDNum As Integer = 1

                        For Each spot In sp
                            esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                            thisSpot = spot("properties")
                            esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                            FIDNum += 1

                            coord = spot("geometry")("coordinates")
                            spotID = thisSpot("id")
                            spotIDs += spotID.ToString + ","
                            esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                            'Check for any root values (single line- not nested)
                            'Get basic values 
                            For Each line In thisSpot
                                If Not line.ToString.Contains("System.Object") Then
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    If Not parts(0).Equals("self") Then
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    End If
                                Else
                                    Continue For
                                End If
                            Next

                            'For Each i In rootDataList

                            '    If thisSpot.ContainsKey(i) Then

                            '        thisVal = thisSpot(i).ToString
                            '    Else
                            '        Continue For
                            '    End If
                            '    esriJson.Append("," + Environment.NewLine + """" + i + """: """ + thisVal + """")
                            'Next

                            If thisSpot.ContainsKey("rock_unit") Then
                                rockData = thisSpot("rock_unit")
                                Dim line As Object
                                For Each line In rockData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    If parts(0).Equals("notes") Then
                                        parts(0) = "rock_unit_notes"
                                    End If
                                    If parts(0).Equals("description") Then
                                        parts(0) = "rock_unit_description"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                Next
                            End If

                            If thisSpot.ContainsKey("trace") Then
                                traceData = thisSpot("trace")
                                Dim line As Object
                                For Each line In traceData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    'Debug.Print(strLine)
                                    parts = strLine.Split(New Char() {","}, 2)
                                    'Debug.Print(parts(0), parts(1))
                                    If String.IsNullOrEmpty(parts(1)) Then
                                        parts(1) = ""
                                    Else
                                        parts(1) = Replace(parts(1), vbLf, "")
                                        parts(1) = Replace(parts(1), """", "'")
                                    End If
                                    If parts(0).Equals("id") Then
                                        parts(0) = "FeatID"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                Next
                            End If

                            esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                            esriJson.Append("""rings"": [" + Environment.NewLine + "[" + Environment.NewLine)

                            'Append coordinates to the file 
                            For Each chunk In coord
                                For Each i In chunk
                                    esriJson.Append("[" + Environment.NewLine + i(0).ToString + "," + Environment.NewLine)
                                    esriJson.Append(i(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                Next
                            Next
                            esriJson.Remove(esriJson.Length - 3, 3)
                            esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                        + "}" + Environment.NewLine + "}," + Environment.NewLine)

                            If thisSpot.containskey("orientation_data") Then
                                chunkNum = 0
                                orData = thisSpot("orientation_data")
                                For Each chunk In orData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        If line.ToString.Contains("associated_orientation") Then
                                            assocOri = orData(chunkNum)("associated_orientation")
                                            For Each block In assocOri
                                                'Finish off the previous orientation data attribute with geometry 
                                                FIDNum += 1
                                                'Start a new attribute for the associated orientation data
                                                aoData.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                                aoData.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                                aoData.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                                For Each l In block
                                                    strLine = l.ToString().Trim("[", "]").Trim
                                                    parts = strLine.Split(New Char() {","}, 2)
                                                    If String.IsNullOrEmpty(parts(1)) Then
                                                        parts(1) = ""
                                                    Else
                                                        parts(1) = Replace(parts(1), vbLf, "")
                                                        parts(1) = Replace(parts(1), """", "'")
                                                    End If
                                                    If parts(0).Equals("id") Then
                                                        parts(0) = "FeatID"
                                                    End If
                                                    aoData.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                                Next
                                                aoData.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                                aoData.Append("""rings"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                                'Append coordinates to the file 
                                                For Each c In coord
                                                    For Each i In c
                                                        aoData.Append("[" + Environment.NewLine + i(0).ToString + "," + Environment.NewLine)
                                                        aoData.Append(i(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                                    Next
                                                Next
                                                aoData.Remove(aoData.Length - 3, 3)
                                                aoData.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                            + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                            Next
                                        ElseIf line.ToString.Contains("System.Object") Then
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            If String.IsNullOrEmpty(parts(1)) Then
                                                parts(1) = ""
                                            Else
                                                parts(1) = Replace(parts(1), vbLf, "")
                                                parts(1) = Replace(parts(1), """", "'")
                                            End If
                                            Dim elementList As String
                                            For Each i In orData(chunkNum)(parts(0))
                                                elementList = elementList + i + ", "
                                            Next
                                            elementList.TrimEnd(", ")
                                            If parts(0).Equals("id") Then
                                                parts(0) = "FeatID"
                                            End If
                                            'Debug.Print(elementList)
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + elementList + """")
                                        Else
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            If String.IsNullOrEmpty(parts(1)) Then
                                                parts(1) = ""
                                            Else
                                                parts(1) = Replace(parts(1), vbLf, "")
                                                parts(1) = Replace(parts(1), """", "'")
                                            End If
                                            If parts(0).Equals("id") Then
                                                parts(0) = "FeatID"
                                            End If
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                        End If
                                    Next
                                    If esriJson.ToString.EndsWith(Environment.NewLine) Then
                                        Continue For
                                    Else
                                        esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                        esriJson.Append("""rings"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                        'Append coordinates to the file 
                                        For Each c In coord
                                            For Each i In c
                                                esriJson.Append("[" + Environment.NewLine + i(0).ToString + "," + Environment.NewLine)
                                                esriJson.Append(i(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                            Next
                                        Next
                                        esriJson.Remove(esriJson.Length - 3, 3)
                                        esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                    + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    End If
                                    FIDNum += 1
                                    chunkNum += 1
                                    esriJson.Append(aoData.ToString)
                                    aoData.Length = 0
                                Next
                            End If

                            If thisSpot.containskey("samples") Then
                                chunkNum = 0
                                sampleData = thisSpot("samples")
                                For Each chunk In sampleData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""rings"": [" + Environment.NewLine + "[" + Environment.NewLine)

                                    'Append coordinates to the file 
                                    For Each c In coord
                                        For Each i In c
                                            esriJson.Append("[" + Environment.NewLine + i(0).ToString + "," + Environment.NewLine)
                                            esriJson.Append(i(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                        Next
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If

                            If thisSpot.containskey("_3d_structures") Then
                                chunkNum = 0
                                _3dData = thisSpot("_3d_structures")
                                For Each chunk In _3dData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            parts(0) = "FeatID"
                                        End If
                                        If parts(0).Equals("type") Then
                                            parts(0) = "_3d_structures_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""rings"": [" + Environment.NewLine + "[" + Environment.NewLine)

                                    'Append coordinates to the file 
                                    For Each c In coord
                                        For Each i In c
                                            esriJson.Append("[" + Environment.NewLine + i(0).ToString + "," + Environment.NewLine)
                                            esriJson.Append(i(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                        Next
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If
                            If thisSpot.ContainsKey("images") Then
                                chunkNum = 0
                                imgData = thisSpot("images")
                                For Each chunk In imgData
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("id") Then
                                            imgID = parts(1).Trim
                                            'Debug.Print(imgID)
                                        ElseIf parts(0).Equals("self") Then
                                            'parts(1) = parts(1).Trim
                                            ''Change to https for older datasets (before server switch)
                                            'Dim urlSplit As String() = parts(1).Split(New Char() {":"})
                                            'If urlSplit(0).Equals("http") Then
                                            '    parts(1) = Replace(parts(1), "http", "https")
                                            'End If
                                            'Debug.Print("Image URL: " + parts(1))
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                            Dim statusCode As String = ""
                                            Dim image As Image
                                            Dim imageResult As HttpWebResponse
                                            'Download the image to same file Json files are saved as a .Tiff
                                            If RadioButton1.Checked Then
                                                imgFile = fileName + "\" + imgID + ".tiff"
                                                Debug.Print(imgFile)
                                                's = HttpWebRequest.Create(parts(1))
                                                s = HttpWebRequest.Create("https://strabospot.org/db/image/" + imgID)
                                                's = HttpWebRequest.Create("http://192.168.0.5/db/image/" + imgID)
                                                enc = New System.Text.UTF8Encoding()
                                                s.Method = "GET"
                                                s.ContentType = "application/json"
                                                authorization = emailaddress + ":" + password
                                                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                                                authorization = Convert.ToBase64String(binaryauthorization)
                                                authorization = "Basic " + authorization
                                                s.Headers.Add("Authorization", authorization)
                                                Try
                                                    imageResult = CType(s.GetResponse(), HttpWebResponse)
                                                    statusCode = imageResult.StatusCode.ToString
                                                    If statusCode.Equals("OK") Then
                                                        image = System.Drawing.Image.FromStream(imageResult.GetResponseStream)
                                                        image.Save(imgFile, System.Drawing.Imaging.ImageFormat.Tiff)
                                                    End If
                                                Catch WebException As Exception
                                                    MessageBox.Show(WebException.Message)
                                                End Try


                                                'Debug.Print(imgFile)
                                                'Client.DownloadFile(parts(1), imgFile)
                                            ElseIf RadioButton2.Checked Then    'Save to the same file as the Json files as a .JPEG
                                                imgFile = fileName + "\" + imgID + ".jpeg"
                                                Debug.Print(imgFile)
                                                's = HttpWebRequest.Create(parts(1))
                                                s = HttpWebRequest.Create("https://strabospot.org/db/image/" + imgID)
                                                's = HttpWebRequest.Create("http://192.168.0.5/db/image/" + imgID)
                                                enc = New System.Text.UTF8Encoding()
                                                s.Method = "GET"
                                                s.ContentType = "application/json"
                                                authorization = emailaddress + ":" + password
                                                binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                                                authorization = Convert.ToBase64String(binaryauthorization)
                                                authorization = "Basic " + authorization
                                                s.Headers.Add("Authorization", authorization)
                                                Try
                                                    imageResult = CType(s.GetResponse(), HttpWebResponse)
                                                    statusCode = imageResult.StatusCode.ToString
                                                    If statusCode.Equals("OK") Then
                                                        image = System.Drawing.Image.FromStream(imageResult.GetResponseStream)
                                                        image.Save(imgFile, System.Drawing.Imaging.ImageFormat.Jpeg)
                                                    End If
                                                Catch WebException As Exception
                                                    MessageBox.Show(WebException.Message)
                                                End Try
                                            End If
                                        Else
                                            Continue For
                                        End If
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""rings"": [" + Environment.NewLine + "[" + Environment.NewLine)

                                    'Append coordinates to the file 
                                    For Each c In coord
                                        For Each i In c
                                            esriJson.Append("[" + Environment.NewLine + i(0).ToString + "," + Environment.NewLine)
                                            esriJson.Append(i(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                        Next
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If

                            If thisSpot.ContainsKey("other_features") Then
                                chunkNum = 0
                                otherFeat = thisSpot("other_features")
                                For Each chunk In otherFeat
                                    esriJson.Append("{" + Environment.NewLine + """attributes"": {" + Environment.NewLine)
                                    esriJson.Append("""FID"" : " + """" + FIDNum.ToString + """")
                                    esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                                    For Each line In chunk
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            parts(1) = ""
                                        Else
                                            parts(1) = Replace(parts(1), vbLf, "")
                                            parts(1) = Replace(parts(1), """", "'")
                                        End If
                                        If parts(0).Equals("type") Then
                                            parts(0) = "other_type"
                                        End If
                                        If parts(0).Equals("description") Then
                                            parts(0) = "other_description"
                                        End If
                                        If parts(0).Equals("name") Then
                                            parts(0) = "other_name"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1).TrimStart + """")
                                    Next
                                    esriJson.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                    esriJson.Append("""rings"": [" + Environment.NewLine + "[" + Environment.NewLine)
                                    'Append coordinates to the file 
                                    For Each c In coord
                                        For Each i In c
                                            esriJson.Append("[" + Environment.NewLine + i(0).ToString + "," + Environment.NewLine)
                                            esriJson.Append(i(1).ToString + Environment.NewLine + "]," + Environment.NewLine)
                                        Next
                                    Next
                                    esriJson.Remove(esriJson.Length - 3, 3)
                                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "]" + Environment.NewLine _
                                                + "}" + Environment.NewLine + "}," + Environment.NewLine)
                                    chunkNum += 1
                                    FIDNum += 1
                                Next
                            End If
                        Next

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    esriJson.Remove(esriJson.Length - 3, 3)

                    esriJson.Append(Environment.NewLine + "]" + Environment.NewLine + "}")

                    'Create a JSON File on the user's computer 
                    If (System.IO.Directory.Exists(fileName)) Then
                        JSONPath = fileName + "\arcJSONpolys.json"
                        'Debug.Print(JSONPath)
                        System.IO.File.WriteAllText(JSONPath, esriJson.ToString())
                    End If

                    featToJson.in_json_file = JSONPath
                    featToJson.out_features = System.IO.Path.Combine(envPath, "polygons")

                    Try
                        geoproc.Execute(featToJson, Nothing)
                        Console.WriteLine(geoproc.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(geoproc.GetMessages(sev))
                    End Try

                    If (geoproc.Exists(envPath + "\polygons", dt)) Then
                        MessageBox.Show("Polygons Feature Class Successfully Created!")
                    Else
                        MessageBox.Show("Error loading Polygons Feature Class...")
                    End If

                End If
            End If
            '////////////////////////////////////////END POLYGONS////////////////////////////////////////////////////
            '////////////////////////////////////////////////////////////////////////////////////////////////////////
        Next

        '///////////////////////////////////////////IMPORT TAGS FROM PROJECT JSON/////////////////////////////////////

        'Need to get info from the Strabo Project Json which will be put into a separate table in the FGDB
        Dim prj As Object
        s = HttpWebRequest.Create("https://strabospot.org/db/project/" + selprojectNum)
        's = HttpWebRequest.Create("http://192.168.0.5/db/project/" + selprojectNum)
        enc = New System.Text.UTF8Encoding()
        s.Method = "GET"
        s.ContentType = "application/json"

        authorization = emailaddress + ":" + password
        binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
        authorization = Convert.ToBase64String(binaryauthorization)
        authorization = "Basic " + authorization
        s.Headers.Add("Authorization", authorization)

        Dim tagFields As String = "SpotID,"
        Dim tagSpotIDs As String = ""
        Dim numTags As Integer = 0
        Try
            Dim result = s.GetResponse()
            datastream = result.GetResponseStream()
            reader = New StreamReader(datastream)
            responseFromServer = reader.ReadToEnd()

            Debug.Print(responseFromServer)

            JSONPath = fileName + "\project-" + selprojectNum + ".json"
            System.IO.File.WriteAllText(JSONPath, responseFromServer)

            prj = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)

        Catch WebException As Exception
            MessageBox.Show(WebException.Message)
        End Try

        If prj IsNot Nothing Then
            'Iterate the tags json to check for the tags that have SpotIDs that were found in the dataset
            'This is because tags can be used in multiple datasets in a Strabo project, but we want just those data
            'from the Strabo dataset, so the Tags table can then be joined to the points, lines, and polygons feature classes
            If prj.ContainsKey("tags") Then
                For Each tg In prj("tags")
                    If tg.ContainsKey("spots") Then
                        For Each spot In tg("spots")
                            If spotIDs.Contains(spot.ToString) Then 'It belongs with the dataset(s)
                                For Each line In tg
                                    If Not line.ToString.Contains("System.Object") Then
                                        strLine = line.ToString().Trim("[", "]").Trim
                                        parts = strLine.Split(New Char() {","}, 2)
                                        If parts(0).Equals("id") Then
                                            parts(0) = "tagID"
                                        End If
                                        If Not tagFields.Contains(parts(0).ToString) Then
                                            tagFields += parts(0) + ","
                                        End If
                                    End If
                                Next
                                numTags += 1
                            End If
                        Next
                    End If
                Next
                'If there are tags associated with this dataset (checked with Spot IDs) then create all the Tags Feature Classes
                If numTags > 0 Then
                    Debug.Print(tagFields)
                    'Create the Tags Table in ArcMap 
                    makeTable.out_path = envPath
                    makeTable.out_name = "Tags"
                    Try
                        geoproc.Execute(makeTable, Nothing)
                        Console.WriteLine(geoproc.GetMessages(sev))
                    Catch ex As Exception
                        Console.WriteLine(geoproc.GetMessages(sev))
                    End Try

                    'Add the fields to the table
                    Dim tagFieldsSplit As String() = tagFields.Split(New Char() {","}, StringSplitOptions.RemoveEmptyEntries)
                    If geoproc.Exists(envPath + "\Tags", dt) Then
                        addFields.in_table = "Tags"
                        For Each field In tagFieldsSplit
                            'Debug.Print(field)
                            addFields.field_name = field
                            addFields.field_type = "TEXT"
                            If field.Equals("description") Or field.Equals("notes") Then
                                addFields.field_length = 1024
                            Else
                                addFields.field_length = 160
                            End If
                            Try
                                geoproc.Execute(addFields, Nothing)
                                Console.WriteLine(geoproc.GetMessages(sev))
                            Catch ex As Exception
                                Console.WriteLine(geoproc.GetMessages(sev))
                            End Try
                        Next
                    End If
                    'Add tag data to the TagsTable
                    Dim workspaceFactory As IWorkspaceFactory = New ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactory
                    Dim featWorkspace As IFeatureWorkspace = workspaceFactory.OpenFromFile(envPath, 0)
                    Dim tagTable As ITable = featWorkspace.OpenTable("Tags")
                    'Dim rowSubTypes As IRowSubtypes
                    Dim fieldIndex As Integer
                    'Dim row As IRow
                    Dim iCur As ICursor = tagTable.Insert(True)
                    Dim rowBuf As IRowBuffer
                    For Each tg In prj("tags")
                        If tg.ContainsKey("spots") Then
                            For Each spot In tg("spots")
                                If spotIDs.Contains(spot.ToString) Then 'Insert a new row for each SpotID
                                    'row = tagTable.CreateRow()
                                    'rowSubTypes = CType(row, IRowSubtypes)
                                    'rowSubTypes.InitDefaultValues()
                                    rowBuf = tagTable.CreateRowBuffer()
                                    fieldIndex = tagTable.FindField("SpotID")
                                    rowBuf.Value(fieldIndex) = spot.ToString
                                    For Each line In tg
                                        If Not line.ToString.Contains("System.Object") Then
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            If parts(0).Equals("id") Then
                                                parts(0) = "tagID"
                                            End If
                                            fieldIndex = tagTable.FindField(parts(0))
                                            rowBuf.Value(fieldIndex) = parts(1).TrimStart
                                            Debug.Print(parts(0) + " " + parts(1))
                                        End If
                                    Next
                                    iCur.InsertRow(rowBuf)
                                End If
                            Next
                        End If
                    Next
                    Try
                        iCur.Flush()
                    Catch ex As Exception
                        Console.WriteLine(ex.Message)
                    Finally
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(iCur)
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(rowBuf)
                    End Try

                    'Link Tag Table with geometry from each feature class
                    dt = "SpotID"
                    Dim makeQTable As ESRI.ArcGIS.DataManagementTools.MakeQueryTable = New ESRI.ArcGIS.DataManagementTools.MakeQueryTable()
                    Dim makeTableView As ESRI.ArcGIS.DataManagementTools.MakeTableView = New ESRI.ArcGIS.DataManagementTools.MakeTableView()
                    Dim copyFeat As ESRI.ArcGIS.DataManagementTools.CopyFeatures = New ESRI.ArcGIS.DataManagementTools.CopyFeatures()
                    Dim delIdent As ESRI.ArcGIS.DataManagementTools.DeleteIdentical = New ESRI.ArcGIS.DataManagementTools.DeleteIdentical()
                    Dim queryFields As String = ""
                    For Each field In tagFieldsSplit
                        queryFields += "Tags." + field + ";"
                    Next
                    queryFields = queryFields.Remove(queryFields.Length - 1)
                    Dim cur As ICursor
                    Dim queryDef As IQueryDef = featWorkspace.CreateQueryDef()
                    If (geoproc.Exists(envPath + "\points", dt)) Then
                        'Make Table View
                        makeTableView.in_table = envPath + "\points"
                        makeTableView.out_view = envPath + "\pointsVIEW"
                        geoproc.AddOutputsToMap = False
                        Try
                            geoproc.Execute(makeTableView, Nothing)
                            Console.WriteLine(geoproc.GetMessages(sev))
                        Catch ex As Exception
                            Console.WriteLine(ex)
                        End Try
                        'Check if the Query will result in any records returned 
                        Dim ptsRow As IRow = Nothing
                        Try
                            queryDef.Tables = "Tags, points"
                            queryDef.SubFields = "points.Shape,points.SpotID,Tags.SpotID,Tags.type"
                            queryDef.WhereClause = """Tags"".""SpotID"" = ""points"".""SpotID"""
                            cur = queryDef.Evaluate()
                            ptsRow = cur.NextRow()
                        Catch ex As Exception
                            Debug.Print(ex.ToString)
                            Debug.Print("Points query definition exception")
                        End Try
                        If ptsRow IsNot Nothing Then
                            'If the query returns a result Make Query Table 
                            makeQTable.in_table = "Tags;" + envPath + "\pointsVIEW"
                            makeQTable.out_table = envPath + "\Tags_Points"
                            makeQTable.in_key_field_option = "NO_KEY_FIELD"
                            geoproc.AddOutputsToMap = False
                            makeQTable.in_field = "points.Shape;points.SpotID;" + queryFields
                            makeQTable.where_clause = """Tags"".""SpotID"" = ""points"".""SpotID"""
                            Try
                                geoproc.Execute(makeQTable, Nothing)
                                Console.WriteLine(geoproc.GetMessages(sev))
                            Catch ex As Exception
                                Debug.Print("MakeQueryTable Exception Caught")
                                Console.WriteLine(ex.ToString)
                            End Try
                            'Copy Features to Save to Database
                            geoproc.AddOutputsToMap = True
                            copyFeat.in_features = envPath + "\Tags_Points"
                            copyFeat.out_feature_class = envPath + "\Pts_Tags"
                            Try
                                geoproc.Execute(copyFeat, Nothing)
                                Console.WriteLine(geoproc.GetMessages(sev))
                            Catch ex As Exception
                                Debug.Print("Copy Features Exception Caught")
                                Console.WriteLine(ex.ToString)
                            End Try
                        End If
                    End If
                    If geoproc.Exists(envPath + "\lines", dt) Then
                        'Make Table View 
                        makeTableView.in_table = envPath + "\lines"
                        makeTableView.out_view = envPath + "\linesVIEW"
                        geoproc.AddOutputsToMap = False
                        Try
                            geoproc.Execute(makeTableView, Nothing)
                            Console.WriteLine(geoproc.GetMessages(sev))
                        Catch ex As Exception
                            Console.WriteLine(ex)
                        End Try
                        'Check if the Query will result in any records returned 
                        Dim linesRow As IRow = Nothing
                        Try
                            queryDef.Tables = "Tags, lines"
                            queryDef.SubFields = "lines.Shape,lines.SpotID,Tags.SpotID,Tags.type"
                            queryDef.WhereClause = """Tags"".""SpotID"" = ""lines"".""SpotID"""
                            cur = queryDef.Evaluate()
                            linesRow = cur.NextRow()
                        Catch ex As Exception
                            Debug.Print(ex.ToString)
                            Debug.Print("Lines query defintion exception")
                        End Try
                        If linesRow IsNot Nothing Then
                            'If the query returns a result Make Query Table
                            makeQTable.in_table = "Tags;" + envPath + "\linesVIEW"
                            makeQTable.out_table = envPath + "\Tags_Lines"
                            makeQTable.in_key_field_option = "NO_KEY_FIELD"
                            geoproc.AddOutputsToMap = False
                            makeQTable.in_field = "lines.Shape;lines.SpotID;" + queryFields
                            makeQTable.where_clause = """Tags"".""SpotID"" = ""lines"".""SpotID"""
                            Try
                                geoproc.Execute(makeQTable, Nothing)
                                Console.WriteLine(geoproc.GetMessages(sev))
                            Catch ex As Exception
                                Debug.Print("MakeQueryTable Exception Caught")
                                Console.WriteLine(ex.ToString)
                            End Try
                            'Copy Features to Database
                            geoproc.AddOutputsToMap = True
                            copyFeat.in_features = envPath + "\Tags_Lines"
                            copyFeat.out_feature_class = envPath + "\Lines_Tags"
                            Try
                                geoproc.Execute(copyFeat, Nothing)
                                Console.WriteLine(geoproc.GetMessages(sev))
                            Catch ex As Exception
                                Debug.Print("Copy Features Exception Caught")
                                Console.WriteLine(ex.ToString)
                            End Try
                        End If
                    End If
                    If geoproc.Exists(envPath + "\polygons", dt) Then
                        'Make Table View
                        makeTableView.in_table = envPath + "\polygons"
                        makeTableView.out_view = envPath + "\polygonsVIEW"
                        geoproc.AddOutputsToMap = False
                        Try
                            geoproc.Execute(makeTableView, Nothing)
                            Console.WriteLine(geoproc.GetMessages(sev))
                        Catch ex As Exception
                            Console.WriteLine(ex.ToString)
                        End Try
                        'Check if the Query will result in any records returned 
                        Dim polyRow As IRow = Nothing
                        Try
                            queryDef.Tables = "Tags, polygons"
                            queryDef.SubFields = "polygons.Shape,polygons.SpotID,Tags.SpotID,Tags.type"
                            queryDef.WhereClause = """Tags"".""SpotID"" = ""polygons"".""SpotID"""
                            cur = queryDef.Evaluate()
                            polyRow = cur.NextRow()
                        Catch ex As Exception
                            Debug.Print(ex.ToString)
                            Debug.Print("Polygon query definition exception")
                        End Try
                        If polyRow IsNot Nothing Then
                            'If the query returns a result Make Query Table  
                            makeQTable.in_table = "Tags;" + envPath + "\polygonsVIEW"
                            makeQTable.out_table = envPath + "\Tags_Polygons"
                            makeQTable.in_key_field_option = "NO_KEY_FIELD"
                            geoproc.AddOutputsToMap = False
                            makeQTable.in_field = "polygons.Shape;polygons.SpotID;" + queryFields
                            makeQTable.where_clause = """Tags"".""SpotID"" = ""polygons"".""SpotID"""
                            Try
                                geoproc.Execute(makeQTable, Nothing)
                                Console.WriteLine(geoproc.GetMessages(sev))
                            Catch ex As Exception
                                Console.WriteLine(ex.ToString)
                            End Try
                            'Save Features to Database
                            geoproc.AddOutputsToMap = True
                            copyFeat.in_features = envPath + "\Tags_Polygons"
                            copyFeat.out_feature_class = envPath + "\Polygons_Tags"
                            Try
                                geoproc.Execute(copyFeat, Nothing)
                                Console.WriteLine(geoproc.GetMessages(sev))
                            Catch ex As Exception
                                Debug.Print("Copy Features Exception Caught")
                                Console.WriteLine(ex.ToString)
                            End Try
                        End If
                    End If
                    dt = Nothing
                    'Try running Delete Identical Rows Tool on any Tags Feature Classes created
                    If geoproc.Exists(envPath + "\Lines_Tags", dt) Then
                        'Run the Delete Identical Rows tool 
                        delIdent.in_dataset = envPath + "\Lines_Tags"
                        delIdent.fields = "Tag_name;Tags_SpotID"
                        Try
                            geoproc.Execute(delIdent, Nothing)
                            Console.WriteLine(geoproc.GetMessages(sev))
                        Catch ex As Exception
                            Debug.Print("Delete Identical Lines Exception Caught")
                            Console.WriteLine(ex.ToString)
                        End Try
                    End If
                    If geoproc.Exists(envPath + "\Pts_Tags", dt) Then
                        'Run the Delete Identical Rows tool 
                        delIdent.in_dataset = envPath + "\Pts_Tags"
                        delIdent.fields = "Tags_name;Tags_SpotID"
                        Try
                            geoproc.Execute(delIdent, Nothing)
                            Console.WriteLine(geoproc.GetMessages(sev))
                        Catch ex As Exception
                            Debug.Print("Delete Identical Points Exception Caught")
                            Console.WriteLine(ex.ToString)
                        End Try
                    End If
                    If geoproc.Exists(envPath + "\Polygons_Tags", dt) Then
                        'Run the Delete Identical Rows tool 
                        delIdent.in_dataset = envPath + "\Polygons_Tags"
                        delIdent.fields = "Tag_name;Tags_SpotID"
                        Try
                            geoproc.Execute(delIdent, Nothing)
                            Console.WriteLine(geoproc.GetMessages(sev))
                        Catch ex As Exception
                            Debug.Print("Delete Identical Polygons Exception Caught")
                            Console.WriteLine(ex.ToString)
                        End Try
                    End If
                End If
            End If
        End If
        'Activate any existing hyperlinks for each layer with "self" field and change required fields
        dt = "self"
        If (geoproc.Exists(envPath + "\points", dt)) Or (geoproc.Exists(envPath + "\lines", dt)) Or (geoproc.Exists(envPath + "\polygons", dt)) Then
            'Based on Amirian text pg. 322 and code for current ArcMap session from Kristen Jordan
            Dim hotlinkField As String = "self"
            Dim pMxDoc As ESRI.ArcGIS.ArcMapUI.IMxDocument
            Dim pMap As ESRI.ArcGIS.Carto.IMap
            pMxDoc = My.ArcMap.Application.Document
            pMap = pMxDoc.FocusMap
            Dim featLayer As IFeatureLayer
            Dim pLayerCount As Integer = pMap.LayerCount
            Dim featClass As IFeatureClass
            'Debug.Print(pLayerCount)
            Dim index As Integer = 0
            While index < pLayerCount
                'Hyperlink settings
                featLayer = pMap.Layer(index)
                featClass = featLayer.FeatureClass
                Dim hLContainer As IHotlinkContainer = featLayer
                hLContainer.HotlinkField = hotlinkField
                hLContainer.HotlinkType = esriHyperlinkType.esriHyperlinkTypeURL
                index += 1
            End While
        End If

        'Run the GeoTagged Photos to Points tool- check to see if there are any photos saved in the file folder
        Dim fileLocation As DirectoryInfo = New DirectoryInfo(fileName)
        Debug.Print("Checking file Name " + fileName + " for photos....")
        Dim containsPhotos As Boolean = False
        For Each File In fileLocation.GetFiles()
            If File.Extension.ToString.Equals(".jpeg") Or File.Extension.ToString.Equals(".tiff") Then
                containsPhotos = True
                Exit For
            End If
        Next
        selDataset = selDataset.Replace(" ", String.Empty)  'Take out any spaces in the dataset's name
        Debug.Print(envPath + "\" + selDataset + "_Images")
        If containsPhotos = True Then
            Dim photosToPoints As ESRI.ArcGIS.DataManagementTools.GeoTaggedPhotosToPoints = New ESRI.ArcGIS.DataManagementTools.GeoTaggedPhotosToPoints()
            photosToPoints.Input_Folder = fileName
            photosToPoints.Output_Feature_Class = envPath + "\" + selDataset + "_Images"
            photosToPoints.Add_Photos_As_Attachments = True
            photosToPoints.Include_Non_GeoTagged_Photos = True
            geoproc.AddOutputsToMap = True
            Try
                geoproc.Execute(photosToPoints, Nothing)
                Console.WriteLine(geoproc.GetMessages(sev))
            Catch ex As Exception
                Debug.Print("GeoTagged Photos To Points Exception Caught")
                Debug.Print(ex.ToString)
            End Try
        End If
        MessageBox.Show("All Feature Classes Loaded.")
    End Sub

    'Enter Button functionality 
    Private Sub PasswordBox_KeyDown(sender As Object, e As KeyEventArgs) Handles PasswordBox.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            Button1_Click(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub Datasets_KeyDown(sender As Object, e As KeyEventArgs) Handles Datasets.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            choose_Click(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub PathName_KeyDown(sender As Object, e As KeyEventArgs) Handles PathName.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            straboToGIS_Click(Me, EventArgs.Empty)
        End If
    End Sub

End Class