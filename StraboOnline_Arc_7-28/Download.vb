Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Reflection
Imports System.Web.Script.Serialization
Imports System.Object
Imports System.Windows.Forms
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
        System.Diagnostics.Process.Start("http://www.strabospot.org")

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

            's = HttpWebRequest.Create("192.168.0.5")
            s = HttpWebRequest.Create("http://strabospot.org/userAuthenticate")
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
        s = HttpWebRequest.Create("http://www.strabospot.org/db/myProjects")
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

        Dim uri As String = "http://www.strabospot.org/db/projectDatasets/" + selprojectNum
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
        Dim thisVal As String
        Dim geoproc As ESRI.ArcGIS.Geoprocessor.Geoprocessor = New ESRI.ArcGIS.Geoprocessor.Geoprocessor()
        Dim featToJson As ESRI.ArcGIS.ConversionTools.JSONToFeatures = New ESRI.ArcGIS.ConversionTools.JSONToFeatures()
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

        'Set the arcpy workspace environment- important because features will need to be saved here
        Dim envPath As String
        envPath = PathName.Text + "\" + pair.Value + ".gdb"
        geoproc.SetEnvironmentValue("workspace", envPath)
        'Debug.Print(envPath)

        'Here, the code will launch a For Each statement to create three separate ESRI JSON 
        'formatted files- Point, Line, and Polygon

        fieldsURL = "http://www.strabospot.org/db/datasetFields/" + selDatasetNum
        Debug.Print(fieldsURL)

        Dim geometries As ArrayList = New ArrayList()
        geometries.Add("point")
        geometries.Add("line")
        geometries.Add("polygon")

        Dim fileName As String = PathName.Text + "\" + pair.Value
        If (Not System.IO.Directory.Exists(fileName)) Then
            System.IO.Directory.CreateDirectory(fileName)
        End If

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
                    esriJson.Append("""SpotID"" : ""SpotID""," + Environment.NewLine)

                    For Each i In f
                        'Add each field to the Field Aliases array 
                        If i.Equals("trace") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("""_3d_structures_type"" : ""_3d_structures_type""," + Environment.NewLine)
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("""other_type"" : ""other_type""," + Environment.NewLine)
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("""rock_unit_notes"" : ""rock_unit_notes""," + Environment.NewLine)
                        Else
                            esriJson.Append("""" + i + """ : """ + i + """," + Environment.NewLine)
                        End If
                        'Debug.Print(i)
                    Next

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
                    esriJson.Append("}," + Environment.NewLine + "{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""SpotID""" + Environment.NewLine)
                    esriJson.Append("}")

                    'Add the fields to the array 
                    For Each i In f
                        If i.Equals("trace") Then
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
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_notes""," + Environment.NewLine)
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
                    'Write all the Spots of type point to Features array
                    esriJson.Append(Environment.NewLine + "]," + Environment.NewLine)
                    esriJson.Append("""features"" : [" + Environment.NewLine)

                    s = HttpWebRequest.Create("http://strabospot.org/db/datasetspotsarc/" + selDatasetNum + "/point")
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
                        origJsonPath = fileName + "\origPts.json"
                        System.IO.File.WriteAllText(origJsonPath, responseFromServer)

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
                            esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")

                            'Get basic values 
                            For Each line In thisSpot
                                If Not line.ToString.Contains("System.Object") Then
                                    'Debug.Print(line.ToString)
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    If Not parts(0).Equals("self") Then
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                    Debug.Print(strLine)
                                    parts = strLine.Split(New Char() {","}, 2)
                                    Debug.Print(parts(0), parts(1))
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    If parts(0).Equals("notes") Then
                                        parts(0) = "rock_unit_notes"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
                                Next
                            End If

                            If thisSpot.ContainsKey("trace") Then
                                traceData = thisSpot("trace")
                                Dim line As Object
                                For Each line In traceData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    Debug.Print(strLine)
                                    parts = strLine.Split(New Char() {","}, 2)
                                    Debug.Print(parts(0), parts(1))
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                                    parts(1) = Replace(parts(1), vbLf, " ")
                                                    parts(1) = Replace(parts(1), """", "'")
                                                    aoData.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
                                                Next
                                                aoData.Append(Environment.NewLine + "}," + Environment.NewLine + """geometry"": {" + Environment.NewLine)
                                                aoData.Append("""x"" : " + coord(0).ToString + "," + Environment.NewLine)
                                                aoData.Append("""y"" : " + coord(1).ToString + Environment.NewLine + "}")
                                                aoData.Append(Environment.NewLine + "}," + Environment.NewLine)
                                            Next
                                        ElseIf line.ToString.Contains("System.Object") Then
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            parts(1) = Replace(parts(1), vbLf, " ")
                                            parts(1) = Replace(parts(1), """", "'")
                                            Dim elementList As String
                                            For Each i In orData(chunkNum)(parts(0))
                                                elementList = elementList + i + ", "
                                            Next
                                            elementList.TrimEnd(", ")
                                            ' Debug.Print(elementList)
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + elementList + """")
                                        Else
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            parts(1) = Replace(parts(1), vbLf, " ")
                                            parts(1) = Replace(parts(1), """", "'")
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("type") Then
                                            parts(0) = "_3d_structures_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("self") Then
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("type") Then
                                            parts(0) = "other_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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

                    'Save the ESRI Formatted Json in the same file as the Original GeoJson   
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

                    Dim sev As Object = Nothing

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
                    esriJson.Append("""SpotID"" : ""SpotID""," + Environment.NewLine)
                    For Each i In f
                        'Add each field to the Field Aliases array 
                        If i.Equals("trace") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("""_3d_structures_type"" : ""_3d_structures_type""," + Environment.NewLine)
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("""other_type"" : ""other_type""," + Environment.NewLine)
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("""rock_unit_notes"" : ""rock_unit_notes""," + Environment.NewLine)
                        Else
                            esriJson.Append("""" + i + """ : """ + i + """," + Environment.NewLine)
                        End If
                        'Debug.Print(i)
                    Next

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
                    esriJson.Append("}," + Environment.NewLine + "{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""SpotID""" + Environment.NewLine)
                    esriJson.Append("}")

                    'Add the fields to the array 
                    For Each i In f
                        If i.Equals("trace") Then
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
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_notes""," + Environment.NewLine)
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
                    'Write all the Spots of type point to Features array
                    esriJson.Append(Environment.NewLine + "]," + Environment.NewLine)
                    esriJson.Append("""features"" : [" + Environment.NewLine)

                    s = HttpWebRequest.Create("http://strabospot.org/db/datasetspotsarc/" + selDatasetNum + "/line")
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

                        'Save the original GeoJson response from the server   
                        origJsonPath = fileName + "\origLines.json"
                        System.IO.File.WriteAllText(origJsonPath, responseFromServer)

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
                            esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                            'Check for any root values (single line- not nested)
                            'Get basic values 
                            For Each line In thisSpot
                                If Not line.ToString.Contains("System.Object") Then
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    If Not parts(0).Equals("self") Then
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    If parts(0).Equals("notes") Then
                                        parts(0) = "rock_unit_notes"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
                                Next
                            End If

                            If thisSpot.ContainsKey("trace") Then
                                traceData = thisSpot("trace")
                                Dim line As Object
                                For Each line In traceData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    Debug.Print(strLine)
                                    parts = strLine.Split(New Char() {","}, 2)
                                    Debug.Print(parts(0), parts(1))
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                                    parts(1) = Replace(parts(1), vbLf, " ")
                                                    parts(1) = Replace(parts(1), """", "'")
                                                    aoData.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                                Debug.Print(aoData.ToString)
                                            Next
                                        ElseIf line.ToString.Contains("System.Object") Then
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            parts(1) = Replace(parts(1), vbLf, " ")
                                            parts(1) = Replace(parts(1), """", "'")
                                            Dim elementList As String
                                            For Each i In orData(chunkNum)(parts(0))
                                                elementList = elementList + i + ", "
                                            Next
                                            elementList.TrimEnd(", ")
                                            'Debug.Print(elementList)
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + elementList + """")
                                        Else
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            parts(1) = Replace(parts(1), vbLf, " ")
                                            parts(1) = Replace(parts(1), """", "'")
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("type") Then
                                            parts(0) = "_3d_structures_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("self") Then
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("type") Then
                                            parts(0) = "other_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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

                    Dim sev As Object = Nothing

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
                    esriJson.Append("""SpotID"" : ""SpotID""," + Environment.NewLine)
                    For Each i In f
                        'Add each field to the Field Aliases array 
                        If i.Equals("trace") Then
                            Continue For
                        ElseIf i.Equals("_3d_structures") Then
                            esriJson.Append("""_3d_structures_type"" : ""_3d_structures_type""," + Environment.NewLine)
                        ElseIf i.Equals("other_features") Then
                            esriJson.Append("""other_type"" : ""other_type""," + Environment.NewLine)
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("""rock_unit_notes"" : ""rock_unit_notes""," + Environment.NewLine)
                        Else
                            esriJson.Append("""" + i + """ : """ + i + """," + Environment.NewLine)
                        End If
                'Debug.Print(i)
                    Next

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
                    esriJson.Append("}," + Environment.NewLine + "{" + Environment.NewLine)
                    esriJson.Append("""name"" : ""SpotID""," + Environment.NewLine)
                    esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                    esriJson.Append("""alias"" : ""SpotID""" + Environment.NewLine)
                    esriJson.Append("}")

                    'Add the fields to the array 
                    For Each i In f
                        If i.Equals("trace") Then
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
                        ElseIf i.Equals("rock_unit") Then
                            esriJson.Append("," + Environment.NewLine + "{" + Environment.NewLine)
                            esriJson.Append("""name"" : ""rock_unit_notes""," + Environment.NewLine)
                            esriJson.Append("""type"" : ""esriFieldTypeString""," + Environment.NewLine)
                            esriJson.Append("""alias"" : ""rock_unit_notes""," + Environment.NewLine)
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
                    'Write all the Spots of type point to Features array
                    esriJson.Append(Environment.NewLine + "]," + Environment.NewLine)
                    esriJson.Append("""features"" : [" + Environment.NewLine)

                    s = HttpWebRequest.Create("http://strabospot.org/db/datasetspotsarc/" + selDatasetNum + "/polygon")
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

                        'Save the original GeoJson response from the server   
                        origJsonPath = fileName + "\origPolygons.json"
                        System.IO.File.WriteAllText(origJsonPath, responseFromServer)

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
                            esriJson.Append("," + Environment.NewLine + """SpotID"" : " + """" + spotID.ToString + """")
                            'Check for any root values (single line- not nested)
                            'Get basic values 
                            For Each line In thisSpot
                                If Not line.ToString.Contains("System.Object") Then
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    parts = strLine.Split(New Char() {","}, 2)
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    If Not parts(0).Equals("self") Then
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    If parts(0).Equals("notes") Then
                                        parts(0) = "rock_unit_notes"
                                    End If
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
                                Next
                            End If

                            If thisSpot.ContainsKey("trace") Then
                                traceData = thisSpot("trace")
                                Dim line As Object
                                For Each line In traceData
                                    strLine = line.ToString().Trim("[", "]").Trim
                                    Debug.Print(strLine)
                                    parts = strLine.Split(New Char() {","}, 2)
                                    Debug.Print(parts(0), parts(1))
                                    parts(1) = Replace(parts(1), vbLf, " ")
                                    parts(1) = Replace(parts(1), """", "'")
                                    esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                                    parts(1) = Replace(parts(1), vbLf, " ")
                                                    parts(1) = Replace(parts(1), """", "'")
                                                    aoData.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                            parts(1) = Replace(parts(1), vbLf, " ")
                                            parts(1) = Replace(parts(1), """", "'")
                                            Dim elementList As String
                                            For Each i In orData(chunkNum)(parts(0))
                                                elementList = elementList + i + ", "
                                            Next
                                            elementList.TrimEnd(", ")
                                            'Debug.Print(elementList)
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + elementList + """")
                                        Else
                                            strLine = line.ToString().Trim("[", "]").Trim
                                            parts = strLine.Split(New Char() {","}, 2)
                                            parts(1) = Replace(parts(1), vbLf, " ")
                                            parts(1) = Replace(parts(1), """", "'")
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("type") Then
                                            parts(0) = "_3d_structures_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("self") Then
                                            esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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
                                        parts(1) = Replace(parts(1), vbLf, " ")
                                        parts(1) = Replace(parts(1), """", "'")
                                        If parts(0).Equals("type") Then
                                            parts(0) = "other_type"
                                        End If
                                        esriJson.Append("," + Environment.NewLine + """" + parts(0) + """: """ + parts(1) + """")
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

                    Dim sev As Object = Nothing

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

        'Activate any existing hyperlinks for each layer with "self" field
        dt = "self"
        If (geoproc.Exists(envPath + "\points", dt)) Or (geoproc.Exists(envPath + "\lines", dt)) Or (geoproc.Exists(envPath + "\polygons", dt)) Then
            'Based on Amirian text pg. 322 and code for current ArcMap session from Kristen Jordan
            Dim hotlinkField As String = "self"
            Dim pMxDoc As ESRI.ArcGIS.ArcMapUI.IMxDocument
            Dim pMap As ESRI.ArcGIS.Carto.IMap
            pMxDoc = My.ArcMap.Application.Document
            pMap = pMxDoc.FocusMap
            Dim featLayer As IFeatureLayer2
            Dim pLayerCount As Integer = pMap.LayerCount
            'Debug.Print(pLayerCount)
            Dim index As Integer = 0
            While index < pLayerCount
                featLayer = pMap.Layer(index)
                Dim hLContainer As IHotlinkContainer = featLayer
                hLContainer.HotlinkField = hotlinkField
                hLContainer.HotlinkType = esriHyperlinkType.esriHyperlinkTypeURL
                index += 1
            End While
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