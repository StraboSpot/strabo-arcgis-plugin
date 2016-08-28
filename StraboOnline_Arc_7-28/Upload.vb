Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Reflection
Imports System.Web.Script.Serialization
Imports System.Object
Imports System.Windows.Forms
Imports System.IO.Packaging
Imports Microsoft.Win32
Imports Ionic.Zip
'Import necessary extensions for FileGDB-- Remember to add as refs too!
Imports ESRI.ArcGIS.DataSourcesGDB
Imports ESRI.ArcGIS.esriSystem
Imports ESRI.ArcGIS.Geodatabase
Imports ESRI.ArcGIS.Geometry
Imports System.Globalization
Imports ESRI.ArcGIS.Geoprocessing
Imports ESRI.ArcGIS.ConversionTools
Imports ESRI.ArcGIS.DataManagementTools
Imports ESRI.ArcGIS.Geoprocessor
Imports ESRI.ArcGIS.Carto

Public Class Upload

    Dim gp As ESRI.ArcGIS.Geoprocessor.Geoprocessor = New ESRI.ArcGIS.Geoprocessor.Geoprocessor()
    Dim pMxDoc As ESRI.ArcGIS.ArcMapUI.IMxDocument
    Dim pMap As ESRI.ArcGIS.Carto.IMap
    Dim featLayer As IFeatureLayer
    Dim pLayerCount As Integer
    Dim ws As String
    Dim indexList As String

    Dim s As HttpWebRequest
    Dim enc As UTF8Encoding
    Dim postdata As String
    Dim postdatabytes As Byte()
    Dim responseFromServer As String
    Dim reader As StreamReader
    Dim datastream As Stream
    Dim isvalid As String
    Dim authorization As String
    Dim binaryauthorization As Byte()

    Dim projectlist As String
    Dim datasetlist As String
    Dim stringSeparators() As String = {","}
    Dim selDatasetNum As String
    Dim selprojectNum As String
    Dim projectIDs As String
    Dim datasetIDs As String
    Dim mapIndices As String
    Dim mapIndicesList As System.Array
    Dim selIndex As Integer

    Public Shared Function checkDatasetType(ByVal fC As IFeatureClass) As Boolean
        If fC.Fields.FindField("SpotID") >= 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function getDefaultBrowser() As String
        'This function simply looks into the registry and returns the default browser
        Dim browser As String = String.Empty
        Dim key As RegistryKey = Nothing
        Try
            key = Registry.ClassesRoot.OpenSubKey("HTTP\shell\open\command", False)

            'trim off quotes
            browser = key.GetValue(Nothing).ToString().ToLower().Replace("""", "")
            If Not browser.EndsWith("exe") Then
                'get rid of everything after the ".exe"
                browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4)
            End If
        Finally
            If key IsNot Nothing Then
                key.Close()
            End If
        End Try
        Return browser
    End Function

    Private Sub linklabel1_Linkclicked(ByVal sender As Object, ByVal e As Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked

        Me.LinkLabel1.LinkVisited = True
        System.Diagnostics.Process.Start("http://www.strabospot.org")

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        'Stores the project name given to Strabo 
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        pMxDoc = My.ArcMap.Application.Document
        pMap = pMxDoc.FocusMap
        Dim pGeoDataset As IGeoDataset
        Dim type As String
        Dim spatRef As ISpatialReference
        Dim mapIndex As Integer
        Dim datasetSpatRef As String
        Dim dataset As ESRI.ArcGIS.Geodatabase.IFeatureClass
        Dim featToJson As ESRI.ArcGIS.ConversionTools.FeaturesToJSON = New ESRI.ArcGIS.ConversionTools.FeaturesToJSON()
        Dim project As ESRI.ArcGIS.DataManagementTools.Project = New ESRI.ArcGIS.DataManagementTools.Project()
        Dim spatRefFactory As ISpatialReferenceFactory3 = New SpatialReferenceEnvironmentClass()
        Dim wgs84iSR As ISpatialReference = spatRefFactory.CreateSpatialReference(4326)
        Dim dt As Object = ""
        Dim fileName As String
        Dim jsonPath As String
        Dim typeResponse As Boolean
        Dim shpDatasetName As String
        Dim shpDatasets As ArrayList = New ArrayList()
        Dim shpFile As String = "C:\temp\StraboShps"
        Dim featToShp As ESRI.ArcGIS.ConversionTools.FeatureClassToShapefile = New ESRI.ArcGIS.ConversionTools.FeatureClassToShapefile()
        gp.OverwriteOutput = True
        gp.AddOutputsToMap = False
        Dim splitFile() As String
        Dim endFile As String
        Dim chosenDatasets As New StringBuilder()
        Dim chosenIndList As New StringBuilder()

        If System.IO.Directory.Exists(shpFile) Then
            For Each file As String In
            System.IO.Directory.GetFiles(shpFile)
                System.IO.File.Delete(file)
            Next
        End If

        If RadioButton1.Checked Then        'Creating a New Strabo Project and Accompanying Datasets
            'Create a new Strabo Project 
            Dim rand As Random = New Random
            Dim randDig As String = rand.Next(1, 10)
            Dim uri As String = "http://strabospot.org/db/project"
            Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
            Dim modTimeStamp As Int64 = (DateTime.Now - startEpoch).TotalMilliseconds
            Dim prjid As String = modTimeStamp.ToString + randDig
            Dim today As String = Date.Today
            Dim prjName As String = TextBox1.Text
            Dim self As String = uri + "/" + prjid
            Dim prjData As New StringBuilder()
            Dim isCreated As String
            Dim authorization As String
            Dim binaryauthorization As Byte()

            s = HttpWebRequest.Create(uri)
            enc = New System.Text.UTF8Encoding()
            prjData.Append("{" + Environment.NewLine + """self"" : """ + self + """," + Environment.NewLine + """id"" : """ + prjid + """,")
            prjData.Append(Environment.NewLine + """date"" : """ + today.ToString + """," + Environment.NewLine + """modified_timestamp"" : """ + modTimeStamp.ToString)
            prjData.Append("""," + Environment.NewLine + """description"" : {" + Environment.NewLine + """project_name"" : """ + "" + prjName + """")
            prjData.Append(Environment.NewLine + "}" + Environment.NewLine + "}")
            Dim strPrjData As String = prjData.ToString()
            Debug.Print(strPrjData)
            postdatabytes = enc.GetBytes(strPrjData)
            s.Method = "POST"
            s.ContentType = "application/json"
            s.ContentLength = postdatabytes.Length

            authorization = emailaddress + ":" + password
            binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
            authorization = Convert.ToBase64String(binaryauthorization)
            authorization = "Basic " + authorization
            s.Headers.Add("Authorization", authorization)

            Using stream = s.GetRequestStream()
                stream.Write(postdatabytes, 0, postdatabytes.Length)
            End Using

            Try
                Dim result = s.GetResponse()
                datastream = result.GetResponseStream()
                reader = New StreamReader(datastream)
                responseFromServer = reader.ReadToEnd()

                Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                isCreated = p.ToString

                If isCreated.Equals("""Error"": ""Invalid body JSON sent.""") Then
                    MessageBox.Show("Error creating Strabo Project. Try your request again.")
                    TextBox1.Clear()

                Else
                    MessageBox.Show("Strabo Project " + TextBox1.Text + " Successfully Created!")
                End If

            Catch WebException As Exception
                MessageBox.Show(WebException.Message)
            End Try

            'This For Loop Creates a New Strabo dataset per selected dataset, adds to existing project, 
            'then Runs the Features to Json ArcToolbox tool, parses that file, and edits the original GeoJson response
            'then updates the already created Strabo dataset with the ArcMap edited dataset
            For Each selDataset In ListBox1.SelectedIndices
                'Find the workspace (geodatabase) of the Selected Dataset
                mapIndex = mapIndicesList(selDataset)
                Debug.Print(mapIndex)
                featLayer = pMap.Layer(mapIndex)
                pGeoDataset = pMap.Layer(mapIndex)
                spatRef = pGeoDataset.SpatialReference
                dataset = featLayer.FeatureClass
                ws = (CType(dataset, IDataset)).Workspace.PathName.ToString
                type = featLayer.DataSourceType
                Debug.Print(type)
                datasetSpatRef = spatRef.Name
                'Call function to check whether the dataset is Native Arc vs. Strabo 
                typeResponse = checkDatasetType(dataset)
                Debug.Print(typeResponse)
                'Assign the workspace (minus .gdb extension), check folder existence/create if not in directory  
                If ws.EndsWith(".gdb") Or ws.EndsWith(".mdb") Then
                    fileName = ws.Remove(ws.Length - 4)
                Else
                    fileName = ws
                End If
                If Not System.IO.Directory.Exists(fileName) Then
                    System.IO.Directory.CreateDirectory(fileName)
                End If
                gp.SetEnvironmentValue("workspace", ws)

                If Not (datasetSpatRef.Equals("GCS_WGS_1984")) Then
                    project.out_coor_system = wgs84iSR
                    project.in_dataset = ws + "\" + dataset.AliasName
                    project.out_dataset = ws + "\" + dataset.AliasName + "_Projected"
                    Dim sev As Object = Nothing
                    Try
                        gp.Execute(project, Nothing)
                        Console.WriteLine(gp.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(gp.GetMessages(sev))
                    End Try
                End If

                If typeResponse.Equals(True) Then

                    'Create a new dataset in an existing project:
                    'Create new Dataset, Get List of Projects, Add Dataset to Project 
                    rand = New Random
                    randDig = rand.Next(1, 10)
                    uri = "http://strabospot.org/db/dataset"
                    modTimeStamp = (DateTime.Now - startEpoch).TotalMilliseconds
                    Dim datasetid As String = modTimeStamp.ToString + randDig
                    today = Date.Today
                    Dim datasetName As String = dataset.AliasName
                    self = uri + "/" + datasetid
                    Dim datasetData As New StringBuilder()

                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    datasetData.Append("{" + Environment.NewLine + """id"" : """ + datasetid + """,")
                    datasetData.Append(Environment.NewLine + """date"" : """ + today.ToString + """,")
                    datasetData.Append(Environment.NewLine + """modified_timestamp"" : """ + modTimeStamp.ToString)
                    datasetData.Append("""," + Environment.NewLine + """description"" : {")
                    datasetData.Append(Environment.NewLine + """name"" : """ + "" + datasetName + """")
                    datasetData.Append(Environment.NewLine + "}" + Environment.NewLine + "}")
                    Dim strDatasetData As String = datasetData.ToString()
                    Debug.Print(strDatasetData)
                    postdatabytes = enc.GetBytes(strDatasetData)
                    s.Method = "POST"
                    s.ContentType = "application/json"
                    s.ContentLength = postdatabytes.Length

                    authorization = emailaddress + ":" + password
                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                    authorization = Convert.ToBase64String(binaryauthorization)
                    authorization = "Basic " + authorization
                    s.Headers.Add("Authorization", authorization)

                    Using stream = s.GetRequestStream()
                        stream.Write(postdatabytes, 0, postdatabytes.Length)
                    End Using

                    Try
                        Dim result = s.GetResponse()
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()

                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString

                        If isCreated.Equals("""Error"": ""Invalid body JSON sent.""") Then
                            MessageBox.Show("Error creating Strabo Project. Try your request again.")
                            TextBox1.Clear()

                        Else
                            MessageBox.Show("Strabo dataset " + datasetName + " Successfully Created!")
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Get the selected Project's ID number
                    Dim selIndex As Integer
                    Dim projectIDsList As System.Array
                    projectIDsList = projectIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
                    selIndex = Projects.SelectedIndex
                    selprojectNum = projectIDsList(selIndex)
                    selprojectNum = selprojectNum.Trim

                    'Then Add the New Dataset to the Project 
                    Dim addDataset As New StringBuilder()
                    uri = "http://www.strabospot.org/db/projectDatasets/" + selprojectNum
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    addDataset.Append("{" + Environment.NewLine + """id"" : """ + datasetid + """" + Environment.NewLine + "}")
                    Dim strAddDataset As String = addDataset.ToString()
                    postdatabytes = enc.GetBytes(strAddDataset)
                    s.Method = "POST"
                    s.ContentType = "application/json"
                    s.ContentLength = postdatabytes.Length

                    authorization = emailaddress + ":" + password
                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                    authorization = Convert.ToBase64String(binaryauthorization)
                    authorization = "Basic " + authorization
                    s.Headers.Add("Authorization", authorization)

                    Using stream = s.GetRequestStream()
                        stream.Write(postdatabytes, 0, postdatabytes.Length)
                    End Using

                    Try
                        Dim result = s.GetResponse()
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()

                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString

                        If isCreated.Equals("""Error"": ""Dataset """ + datasetid + """ not found.""") Then
                            MessageBox.Show("Error creating Strabo Project.")
                            TextBox1.Clear()

                        Else
                            MessageBox.Show("Strabo dataset " + datasetName + " Successfully Added to " + Projects.SelectedItem)
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Execute Features To Json from Conversion Tools Toolbox 
                    If Not datasetSpatRef.Equals("GCS_WGS_1984") Then
                        featToJson.in_features = ws + "\" + dataset.AliasName + "_Projected"
                    Else
                        featToJson.in_features = ws + "\" + dataset.AliasName
                    End If
                    jsonPath = fileName + "\" + featLayer.Name + "toJson.json"
                    If Not System.IO.Directory.Exists(jsonPath) Then
                        featToJson.out_json_file = jsonPath
                        featToJson.format_json = "FORMATTED"

                        Dim sev As Object = Nothing
                        Try

                            gp.Execute(featToJson, Nothing)
                            Console.WriteLine(gp.GetMessages(sev))

                        Catch ex As Exception
                            Console.WriteLine(gp.GetMessages(sev))
                        End Try
                        'If the Json File exists this feature has been uploaded to Strabo
                    Else
                        MessageBox.Show("Error: Json File already exists.")
                        Continue For
                    End If

                    ''Read the Json file to String
                    'sr = New StreamReader(jsonPath)
                    'Dim attributes As String
                    'Dim jsonSB As New StringBuilder()
                    'Dim geoType As String
                    ''/////////////////////////////////////////////NEEDS WORK!////////////////////////////////////////////
                    ''///////////////////////////////////////////////////////////////////////////////////////////////////
                    'If (sr.ReadLine()).Equals("""attributes"" : {") Then
                    '    attributes = sr.ReadToEnd()
                    '    Debug.Print(attributes)
                    'End If

                    'If the dataset is Native Arc add to list of shapefiles
                ElseIf (typeResponse.Equals(False)) Then
                    If Not System.IO.Directory.Exists(shpFile) Then
                        System.IO.Directory.CreateDirectory(shpFile)
                    End If
                    If (type.Contains("Shapefile")) Then
                        shpDatasetName = ws + "\" + dataset.AliasName
                        If (System.IO.File.Exists(shpDatasetName + ".shp")) Then
                            For Each file As String In
                                System.IO.Directory.GetFiles(ws)
                                If file.Contains(dataset.AliasName + ".") Then
                                    splitFile = file.Split("\")
                                    endFile = splitFile(splitFile.Length - 1)
                                    If (Not endFile.Contains(".lock")) Then
                                        System.IO.File.Copy(file, shpFile + "\" + endFile, True)
                                    End If
                                End If
                            Next
                            shpDatasets.Add(shpDatasetName)
                        End If
                    Else
                        If datasetSpatRef.Equals("GCS_WGS_1984") Then
                            shpDatasetName = ws + "\" + dataset.AliasName
                        ElseIf (Not (datasetSpatRef.Equals("GCS_WGS_1984"))) Then
                            shpDatasetName = ws + "\" + dataset.AliasName + "_Projected"
                        End If
                        featToShp.Input_Features = shpDatasetName
                        'Store in a separate folder
                        featToShp.Output_Folder = shpFile

                        Dim sev As Object = Nothing
                        Try
                            gp.Execute(featToShp, Nothing)
                            Console.WriteLine(gp.GetMessages(sev))

                        Catch ex As Exception
                            Console.WriteLine(gp.GetMessages(sev))
                        End Try
                        shpDatasets.Add(shpDatasetName)
                    End If
                End If
            Next

            'Once all Native Arc datasets are put in the Temp folder, zip it and give to StraboSpot
            If Not shpDatasets.Count.Equals(0) Then
                Dim zipShp As String = shpFile + ".zip"
                If System.IO.Directory.Exists(zipShp) Then
                    Directory.Delete(zipShp)
                End If
                'Use DotNetZip library to zip the Shapefile  
                Try
                    Using zip As ZipFile = New ZipFile
                        zip.AddDirectory(shpFile, "")
                        zip.Save(zipShp)
                    End Using
                Catch ex1 As Exception
                    Console.Error.WriteLine("exception: {0}", ex1.ToString)
                End Try

                'Use Jason's code to upload the zipped shapefile of several feature classes to StraboSpot
                Dim response As Byte()
                Dim arcid As String

                Using wc As New System.Net.WebClient()
                    'UPLOAD the file to strabospot. ***NEED ZIPSHP TO POINT TO THE CORRECT ZIPPED FILE BEFORE TRYING****
                    response = wc.UploadFile("http://www.strabospot.org/arcupload.php", zipShp)
                    'the response from the server is a token for finishing the upload
                    arcid = wc.Encoding.GetString(response)
                    'Start the default browser to finish the shapefile upload
                    Process.Start(getDefaultBrowser, "http://www.strabospot.org/loadarcshapefile?arcid=" + arcid)
                End Using
            End If

        ElseIf RadioButton2.Checked Then        'Overwriting or Delete/Replace Dataset in existing StraboProject
            'Get the user chosen project to Overwrite 
            Dim selIndex As Integer
            Dim projectIDsList As System.Array
            projectIDsList = projectIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
            selIndex = Projects.SelectedIndex
            selprojectNum = projectIDsList(selIndex)
            selprojectNum = selprojectNum.Trim

        ElseIf RadioButton3.Checked Then        'Use an existing Strabo Project to add new datasets 
            'This For Loop Creates a New Strabo dataset per selected dataset, adds to existing project, 
            'then Runs the Features to Json ArcToolbox tool, parses that file, and edits the original GeoJson response
            'then updates the already created Strabo dataset with the ArcMap edited dataset

            For Each selDataset In ListBox1.SelectedIndices
                'Find the workspace (geodatabase) of the Selected Dataset
                mapIndex = mapIndicesList(selDataset)
                Debug.Print(mapIndex)
                featLayer = pMap.Layer(mapIndex)
                pGeoDataset = pMap.Layer(mapIndex)
                spatRef = pGeoDataset.SpatialReference
                dataset = featLayer.FeatureClass
                ws = (CType(dataset, IDataset)).Workspace.PathName.ToString
                type = featLayer.DataSourceType
                gp.SetEnvironmentValue("workspace", ws)
                datasetSpatRef = spatRef.Name
                'Call function to check whether the dataset is Native Arc vs. Strabo 
                typeResponse = checkDatasetType(dataset)
                'Check to see if the Folder with same workspace exists, if not, create it. 
                If ws.EndsWith(".gdb") Or ws.EndsWith(".mdb") Then
                    fileName = ws.Remove(ws.Length - 4)
                Else
                    fileName = ws
                End If
                If Not System.IO.Directory.Exists(fileName) Then
                    System.IO.Directory.CreateDirectory(fileName)
                End If
                If Not (datasetSpatRef.Equals("GCS_WGS_1984")) Then
                    If type.Contains("Shapefile") Then
                        project.out_coor_system = wgs84iSR
                        project.in_dataset = ws + "\" + dataset.AliasName + ".shp"
                        project.out_dataset = ws + "\" + dataset.AliasName + "_Projected.shp"
                    Else
                        project.out_coor_system = wgs84iSR
                        project.in_dataset = ws + "\" + dataset.AliasName
                        project.out_dataset = ws + "\" + dataset.AliasName + "_Projected"
                    End If

                    Dim sev As Object = Nothing
                    Try

                        gp.Execute(project, Nothing)
                        Console.WriteLine(gp.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(gp.GetMessages(sev))
                    End Try
                End If

                If typeResponse.Equals(True) Then

                    'Create a new dataset in an existing project:
                    'Create new Dataset, Get List of Projects, Add Dataset to Project 
                    Dim rand As Random = New Random
                    Dim randDig As String = rand.Next(1, 10)
                    Dim uri As String = "http://strabospot.org/db/dataset"
                    Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
                    Dim modTimeStamp As Int64 = (DateTime.Now - startEpoch).TotalMilliseconds
                    Dim id As String = modTimeStamp.ToString + randDig
                    Dim today As String = Date.Today
                    Dim datasetName As String = dataset.AliasName
                    Dim self As String = uri + "/" + id
                    Dim datasetData As New StringBuilder()
                    Dim isCreated As String
                    Dim authorization As String
                    Dim binaryauthorization As Byte()

                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    datasetData.Append("{" + Environment.NewLine + """id"" : """ + id + """,")
                    datasetData.Append(Environment.NewLine + """date"" : """ + today.ToString + """,")
                    datasetData.Append(Environment.NewLine + """modified_timestamp"" : """ + modTimeStamp.ToString)
                    datasetData.Append("""," + Environment.NewLine + """description"" : {")
                    datasetData.Append(Environment.NewLine + """name"" : """ + "" + datasetName + """")
                    datasetData.Append(Environment.NewLine + "}" + Environment.NewLine + "}")
                    Dim strDatasetData As String = datasetData.ToString()
                    Debug.Print(strDatasetData)
                    postdatabytes = enc.GetBytes(strDatasetData)
                    s.Method = "POST"
                    s.ContentType = "application/json"
                    s.ContentLength = postdatabytes.Length

                    authorization = emailaddress + ":" + password
                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                    authorization = Convert.ToBase64String(binaryauthorization)
                    authorization = "Basic " + authorization
                    s.Headers.Add("Authorization", authorization)

                    Using stream = s.GetRequestStream()
                        stream.Write(postdatabytes, 0, postdatabytes.Length)
                    End Using

                    Try
                        Dim result = s.GetResponse()
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()

                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString

                        If isCreated.Equals("""Error"": ""Invalid body JSON sent.""") Then
                            MessageBox.Show("Error creating Strabo Project. Try your request again.")
                            TextBox1.Clear()

                        Else
                            MessageBox.Show("Strabo dataset " + datasetName + " Successfully Created!")
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Get the selected Project's ID number
                    Dim selIndex As Integer
                    Dim projectIDsList As System.Array
                    projectIDsList = projectIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
                    selIndex = Projects.SelectedIndex
                    selprojectNum = projectIDsList(selIndex)
                    selprojectNum = selprojectNum.Trim

                    'Then Add the New Dataset to the Project 
                    Dim addDataset As New StringBuilder()
                    uri = "http://www.strabospot.org/db/projectDatasets/" + selprojectNum
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    addDataset.Append("{" + Environment.NewLine + """id"" : """ + id + """" + Environment.NewLine + "}")
                    Dim strAddDataset As String = addDataset.ToString()
                    postdatabytes = enc.GetBytes(strAddDataset)
                    s.Method = "POST"
                    s.ContentType = "application/json"
                    s.ContentLength = postdatabytes.Length

                    authorization = emailaddress + ":" + password
                    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
                    authorization = Convert.ToBase64String(binaryauthorization)
                    authorization = "Basic " + authorization
                    s.Headers.Add("Authorization", authorization)

                    Using stream = s.GetRequestStream()
                        stream.Write(postdatabytes, 0, postdatabytes.Length)
                    End Using

                    Try
                        Dim result = s.GetResponse()
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()

                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString

                        If isCreated.Equals("""Error"": ""Dataset """ + id + """ not found.""") Then
                            MessageBox.Show("Error creating Strabo Project.")
                            TextBox1.Clear()

                        Else
                            MessageBox.Show("Strabo dataset " + datasetName + " Successfully Added to " + Projects.SelectedItem)
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Execute Features To Json from Conversion Tools Toolbox 
                    If Not datasetSpatRef.Equals("GCS_WGS_1984") Then
                        featToJson.in_features = ws + "\" + dataset.AliasName + "_Projected"
                    Else
                        featToJson.in_features = ws + "\" + dataset.AliasName
                    End If
                    jsonPath = fileName + "\" + featLayer.Name + "toJson.json"
                    If Not System.IO.Directory.Exists(jsonPath) Then
                        featToJson.out_json_file = jsonPath
                        featToJson.format_json = "FORMATTED"

                        Dim sev As Object = Nothing
                        Try

                            gp.Execute(featToJson, Nothing)
                            Console.WriteLine(gp.GetMessages(sev))

                        Catch ex As Exception
                            Console.WriteLine(gp.GetMessages(sev))
                        End Try
                        'If the Json File exists this feature has been uploaded to Strabo
                    Else
                        MessageBox.Show("Error: Json File already exists.")
                        Continue For
                    End If

                    'Read the Json file to String
                    Dim attributes As String()
                    Dim jsonSB As New StringBuilder()
                    Dim geoType As String
                    Dim orgResponse As String()

                    ''Get the geometryType of the feature class to help locate the original Json response on 
                    'Using strm As StreamReader = New StreamReader(jsonPath)
                    '    If (strm.ReadLine()).Contains("geometryType") Then
                    '        If (strm.ReadLine()).Contains("Point") Then
                    '            geoType = "Pts"
                    '        ElseIf (strm.ReadLine()).Contains("Polyline") Then
                    '            geoType = "Lines"
                    '        ElseIf (strm.ReadLine()).Contains("Polygon") Then
                    '            geoType = "Polygons"
                    '        End If
                    '        'Find the original server response from Strabo (file created upon download) 
                    '        For Each f As String In
                    '            System.IO.Directory.GetFiles(fileName)
                    '            If f.Contains("orig" + geoType) Then
                    '                Using reader As StreamReader = New StreamReader(fileName + "\orig" + geoType)
                    '                    orgResponse = (reader.ReadToEnd()).Split("properties")
                    '                End Using
                    '            End If
                    '        Next
                    '    End If
                    'End Using

                    'Using sr As StreamReader = New StreamReader(jsonPath)
                    '    If (sr.ReadLine()).Equals("""attributes"" : {") Then
                    '        attributes = (sr.ReadToEnd()).Split("""attributes"" : {")
                    '        For Each i In attributes
                    '            Debug.Print(i)
                    '            i.Split(Environment.NewLine)
                    '            For Each line In i
                    '                Debug.Print(line)
                    '                line.Split(New Char() {":"}, 2)

                    '            Next
                    '        Next
                    '    End If
                    'End Using
                    '//////////////////////////////////////NEEDS WORK!!!!/////////////////////////////////////////////
                    '////////////////////////////////////////////////////////////////////////////////////////////////

                    'If the dataset is Native Arc add to list of shapefiles
                ElseIf (typeResponse.Equals(False)) Then
                    If Not System.IO.Directory.Exists(shpFile) Then
                        System.IO.Directory.CreateDirectory(shpFile)
                    End If
                    If (type.Contains("Shapefile")) Then
                        shpDatasetName = ws + "\" + dataset.AliasName
                        If (System.IO.File.Exists(shpDatasetName + ".shp")) Then
                            For Each file As String In
                                System.IO.Directory.GetFiles(ws)
                                If file.Contains(dataset.AliasName + ".") Then
                                    splitFile = file.Split("\")
                                    endFile = splitFile(splitFile.Length - 1)
                                    If (Not endFile.Contains(".lock")) Then
                                        System.IO.File.Copy(file, shpFile + "\" + endFile, True)
                                    End If
                                End If
                            Next
                        End If
                    Else
                        If datasetSpatRef.Equals("GCS_WGS_1984") Then
                            shpDatasetName = ws + "\" + dataset.AliasName
                        ElseIf (Not (datasetSpatRef.Equals("GCS_WGS_1984"))) Then
                            shpDatasetName = ws + "\" + dataset.AliasName + "_Projected"
                        End If
                        featToShp.Input_Features = shpDatasetName
                        'Store in a separate folder
                        featToShp.Output_Folder = shpFile

                        Dim sev As Object = Nothing
                        Try
                            gp.Execute(featToShp, Nothing)
                            Console.WriteLine(gp.GetMessages(sev))

                        Catch ex As Exception
                            Console.WriteLine(gp.GetMessages(sev))
                        End Try
                    End If
                    shpDatasets.Add(shpDatasetName)
                End If
            Next

            'Execute Feature Class to Shapefile script in Conversion Toolbox 
            If Not shpDatasets.Count.Equals(0) Then
                Dim zipShp As String = shpFile + ".zip"
                Debug.Print(zipShp)
                If System.IO.File.Exists(zipShp) Then
                    System.IO.File.Delete(zipShp)
                End If
                'Use DotNetZip library to zip the Shapefile  
                Try
                    Using zip As ZipFile = New ZipFile
                        zip.AddDirectory(shpFile, "")
                        zip.Save(zipShp)
                    End Using
                Catch ex1 As Exception
                    Console.Error.WriteLine("exception: {0}", ex1.ToString)
                End Try

                'Use Jason's code to upload the zipped shapefile of several feature classes to StraboSpot
                Dim response As Byte()
                Dim arcid As String

                Using wc As New System.Net.WebClient()
                    'UPLOAD the file to strabospot. ***NEED ZIPSHP TO POINT TO THE CORRECT ZIPPED FILE BEFORE TRYING****
                    response = wc.UploadFile("http://www.strabospot.org/arcupload.php", zipShp)
                    'the response from the server is a token for finishing the upload
                    arcid = wc.Encoding.GetString(response)
                    'Start the default browser to finish the shapefile upload
                    Process.Start(getDefaultBrowser, "http://www.strabospot.org/loadarcshapefile?arcid=" + arcid)
                End Using
            End If
        End If
        'Return user to the choices for uploading files
        Button2.Visible = False
        Label2.Visible = False
        Label3.Visible = False
        Label5.Visible = False
        Label6.Visible = False
        TextBox1.Visible = False
        ListBox1.Visible = False
        getProjects.Visible = False
        Projects.Visible = False

        RadioButton1.Visible = True
        RadioButton2.Visible = True
        RadioButton3.Visible = True
        Label1.Visible = True
        Button1.Visible = True
        back.Visible = True

        ListBox1.Items.Clear()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        RadioButton1.Visible = False
        RadioButton2.Visible = False
        RadioButton3.Visible = False
        Button1.Visible = False
        Button2.Visible = True
        back.Visible = True
        Label1.Visible = False

        If RadioButton1.Checked Then
            Label2.Visible = True
            Label3.Visible = True
            TextBox1.Visible = True
            Label5.Visible = True
            ListBox1.Visible = True

        ElseIf RadioButton2.Checked Then
            'Tools for Overwriting a Strabo Project are displayed
            getProjects.Visible = True
            ListBox1.Visible = True
            Projects.Visible = True
            Label5.Visible = True
            'Something to ask whether to Delete/Replace whole dataset versus Find and Replace by Feature

        ElseIf RadioButton3.Checked Then
            Label6.Visible = True
            Label5.Visible = True
            'getProjects.Visible = True
            ListBox1.Visible = True
            'Projects.Visible = True
        End If

        'Add the ArcMap Session Layers to the ListBox
        pMxDoc = My.ArcMap.Application.Document
        pMap = pMxDoc.FocusMap
        pLayerCount = pMap.LayerCount
        Debug.Print(pLayerCount.ToString)
        'If no layers in Table of Contents direct user to add some
        If pLayerCount.Equals(0) Then
            MessageBox.Show("Error: Add Layers to Table of Contents")

            'Add each layer to the ListBox 
            '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        Else
            Dim index As Integer = 0
            While index < pLayerCount
                Dim lyr As ILayer
                lyr = pMap.Layer(index)
                If TypeOf lyr Is ESRI.ArcGIS.Geodatabase.FeatureClass Then
                    featLayer = pMap.Layer(index)
                    ListBox1.Items.Add(featLayer.Name)
                    mapIndices = mapIndices + index.ToString() + ","
                End If
                index += 1
            End While
        End If
        mapIndicesList = mapIndices.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListBox1.SelectedIndexChanged
        ListBox1.SelectionMode = SelectionMode.MultiSimple
    End Sub

    Private Sub getProjects_Click(sender As Object, e As EventArgs) Handles getProjects.Click
        If Not Projects.Items.Equals(Nothing) Then
            Projects.Items.Clear()
            projectlist = String.Empty
            projectIDs = String.Empty
        End If

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
            Projects.Visible = True

        Catch WebException As Exception
            MessageBox.Show(WebException.Message)
        End Try

        'Convert the String list into an Array and add to the List Box
        Dim projectlistArray As System.Array
        projectlistArray = projectlist.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
        Projects.Items.AddRange(projectlistArray)
    End Sub

    Private Sub LogIn_Click(sender As Object, e As EventArgs) Handles LogIn.Click
        'Send username and password to authenticate 
        emailaddress = Username.Text
        password = PasswordBox.Text

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
                'Hide Log in tools 
                Label8.Visible = False
                Label9.Visible = False
                Username.Visible = False
                PasswordBox.Visible = False
                LogIn.Visible = False

                'Make tools for choosing upload method visible 
                RadioButton1.Visible = True
                RadioButton2.Visible = True
                RadioButton3.Visible = True
                Label1.Visible = True
                Button1.Visible = True
                back.Visible = True

            Else
                MessageBox.Show("Incorrect Username and Password; try again")
            End If

        Catch WebException As Exception
            MessageBox.Show(WebException.Message)
        End Try

    End Sub

    Private Sub back_Click(sender As Object, e As EventArgs) Handles back.Click
        If RadioButton1.Visible = True Then
            'Go back to the Log-In screen 
            RadioButton1.Visible = False
            RadioButton2.Visible = False
            RadioButton3.Visible = False
            Label1.Visible = False
            Button1.Visible = False
            back.Visible = False

            Label1.Visible = True
            Label8.Visible = True
            Label9.Visible = True
            Username.Visible = True
            PasswordBox.Visible = True
            LogIn.Visible = True

        ElseIf ListBox1.Visible = True Then
            'Go back to the options screen 
            Projects.Visible = False
            getProjects.Visible = False
            Label6.Visible = False
            Label5.Visible = False
            Button2.Visible = False
            Label2.Visible = False
            Label3.Visible = False
            TextBox1.Visible = False
            ListBox1.Visible = False
            If Not ListBox1.Items.Equals(Nothing) Then
                ListBox1.Items.Clear()
                indexList = String.Empty
            End If

            RadioButton1.Visible = True
            RadioButton2.Visible = True
            RadioButton3.Visible = True
            Label1.Visible = True
            Button1.Visible = True
        End If
    End Sub

    'Enter button functionality for LogIn button, second page (radio button selections), and Upload button
    Private Sub PasswordBox_KeyDown(sender As Object, e As KeyEventArgs) Handles PasswordBox.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            LogIn_Click(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub RadioButton1_KeyDown(sender As Object, e As KeyEventArgs) Handles RadioButton1.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            Button1_Click(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub RadioButton2_KeyDown(sender As Object, e As KeyEventArgs) Handles RadioButton2.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            Button1_Click(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub RadioButton3_KeyDown(sender As Object, e As KeyEventArgs) Handles RadioButton3.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            Button1_Click(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub ListBox1_KeyDown(sender As Object, e As KeyEventArgs) Handles ListBox1.KeyDown
        If e.KeyCode.Equals(Keys.Enter) Then
            Button2_Click(Me, EventArgs.Empty)
        End If
    End Sub
End Class