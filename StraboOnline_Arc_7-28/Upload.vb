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
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

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

    Dim sr As StreamReader
    Dim wholeFile As String()
    Dim sb As New StringBuilder()
    Dim esriFile As String
    Dim phrase As String() = {"""attributes"""}
    Dim attributes As List(Of String)
    Dim numAttributes As Integer
    Dim numLines As Integer = 0
    Dim parts As String()
    Dim delChars As Char() = {"{", "}", "[", "]"}
    Dim attr As String()
    Dim spots As Integer = 0

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

    Public Shared Function CompareSpotIDs(ByVal origID As String, ByVal wholeESRI As String) As String
        Dim strLine() As String
        Dim block As String
        Dim sndSplit() As String
        Dim delim As String() = New String() {"""SpotID""" + "  " + """" + origID + """" + ","}
        Dim delim2 As String() = New String() {"""SpotID"""}
        If wholeESRI.Contains(origID) Then
            strLine = wholeESRI.Split(delim, StringSplitOptions.None)
            sndSplit = strLine(1).Split(delim2, StringSplitOptions.None)
            block = sndSplit(0).Trim
        End If
        'Debug.Print(block)
        Return block
    End Function

    Public Shared Function CompareESRItoOrig(ByVal spotID As String, ByVal wholeJSON As Object, ByVal ESRIblock As String) As Object
        Dim spot As Object
        Dim thisSpot As Object
        Dim splitESRI As String() = ESRIblock.Split(Environment.NewLine)
        Dim currentLine As String()
        Dim spotGeo As Object
        Dim changesCount As Integer = 0
        Dim traceData As Object
        Dim oriData As Object
        Dim oriID As Object
        Dim _3dData As Object
        Dim _3dID As Object
        Dim sampleData As Object
        Dim sampleID As Object
        Dim aoData As Object
        Dim rockUnit As Object
        Dim chunkNum As Integer
        Dim rand As Random = New Random
        Dim randDig As String = rand.Next(1, 10)
        Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
        Dim unixTime As Int64
        Dim newValue As JToken
        Dim innerID As String = ""
        Dim aoID As Object
        Dim lineExists As Boolean
        Dim blockNum As Integer
        Dim keyExists As Boolean
        Dim value As String = ""
        Dim esriGeo(2) As String
        Dim geoPairs As Integer = 0
        'Begin to loop through the wholeJSON file to find the corresponding spot
        For Each spot In wholeJSON("features")
            thisSpot = spot("properties")
            spotGeo = spot("geometry")("coordinates")
            'Find the spot which matches with the ESRIblock 
            If (thisSpot("id")).ToString.Equals(spotID) Then
                'Loop through ESRIblock for current spot info
                For Each ln In splitESRI
                    If ln.Contains("""geometry"": ") Then Continue For
                    'If the geometry is not a point (i.e. line or polygon) the value won't be split-- gather each pair of coords and compare to appropriate original coord
                    If Not ln.Contains(" ") Then
                        If ln.EndsWith(",") Then
                            esriGeo(0) = ln.TrimEnd(",").Trim
                        Else
                            esriGeo(1) = ln.Trim
                            Debug.Print(esriGeo(0) + esriGeo(1))
                            Debug.Print(spotGeo(geoPairs)(0).ToString)
                            If (esriGeo(0).Equals((spotGeo(geoPairs)(0)).ToString)) And (esriGeo(1).Equals(spotGeo(geoPairs)(1).ToString)) Then
                                Debug.Print("X and Y coordinates match")
                                geoPairs += 1
                                Continue For
                            ElseIf (Not esriGeo(0).Equals((spotGeo(geoPairs)(0)).ToString)) Then
                                spotGeo(geoPairs)(0) = esriGeo(0)
                                Debug.Print("Changed X " + spotGeo(geoPairs)(0) + "to " + esriGeo(0))
                                If (Not esriGeo(1).Equals(spotGeo(geoPairs)(1).ToString)) Then
                                    spotGeo(geoPairs)(1) = esriGeo(1)
                                    Debug.Print("ALSO Changed Y " + spotGeo(geoPairs)(1) + "to " + esriGeo(1))
                                End If
                                changesCount += 1
                            ElseIf (Not esriGeo(1).Equals(spotGeo(geoPairs)(1).ToString)) Then
                                spotGeo(geoPairs)(1) = esriGeo(1)
                                Debug.Print("Changed Y " + spotGeo(geoPairs)(1) + "to " + esriGeo(1))
                                changesCount += 1
                            End If
                            geoPairs += 1
                        End If
                        Continue For
                    End If
                    currentLine = ln.Split(New Char() {" "}, 2)
                    currentLine(0) = currentLine(0).Replace(vbLf, "")
                    currentLine(1) = currentLine(1).Replace(""" ", """")
                    currentLine(1) = currentLine(1).Replace(""",", """")
                    currentLine(1) = currentLine(1).TrimEnd(",").Trim
                    currentLine(0) = currentLine(0).TrimStart("""").TrimEnd("""")
                    currentLine(1) = currentLine(1).TrimStart("""").TrimEnd("""")
                    If currentLine(0).Equals("modified_timestamp") Or currentLine(0).Equals("date") _
                        Or currentLine(0).Equals("time") Or currentLine(0).Equals("self") Or currentLine(0).Equals("type") Then Continue For
                    Debug.Print("ESRI Data: " + currentLine(0) + " " + currentLine(1))
                    If String.IsNullOrEmpty(currentLine(1)) Then
                        'Debug.Print("The ESRI value returned is null...")
                        currentLine(1) = ""
                    End If
                    ''Save additional IDs within the spot array for use in complex arrays
                    If currentLine(0).Equals("id") Then
                        If currentLine(1).Contains(spotID) Then
                            Continue For
                        Else
                            innerID = currentLine(1)
                            'Debug.Print("Inner ID: " + innerID)
                            Continue For
                        End If
                    End If
                    If currentLine(0).Equals("x") Or currentLine(0).Equals("y") Then
                        If spotGeo(1).ToString.Equals(currentLine(1)) Or spotGeo(0).ToString.Equals(currentLine(1)) Then
                            'Debug.Print("Same geometry coordinates...")
                        Else
                            'Debug.Print("Different coordinates...")
                            If currentLine(0).Equals("x") Then
                                spotGeo(0) = currentLine(1)
                                changesCount += 1
                            ElseIf currentLine(1).Equals("y") Then
                                spotGeo(1) = currentLine(1)
                                changesCount += 1
                            End If
                        End If
                        Continue For
                    End If
                    Try
                        'Check if the value is in the outermost array of the spot's info
                        keyExists = thisSpot.ContainsKey(currentLine(0))
                        If keyExists.Equals(True) And innerID.Equals("") Then   'If the key exists and there is no innerID yet, must belong to outermost array of info
                            lineExists = thisSpot.TryGetValue(currentLine(0), value)
                            If String.IsNullOrEmpty(value) Then
                                value = ""
                            End If
                            If value.Equals(currentLine(1)) Then
                                'Debug.Print("KVP exists")
                                Exit Try
                            ElseIf lineExists.Equals(True) Then
                                thisSpot(currentLine(0)) = currentLine(1)
                                Debug.Print("Changed: " + currentLine(0) + currentLine(1))
                                changesCount += 1
                            End If

                        ElseIf keyExists.Equals(False) Or (keyExists.Equals(True) And innerID <> ("")) Then
                            'If the key value is in the outermost array and belongs there (i.e. has not advanced to other Key arrays) then replace with CL value
                            Try
                                If thisSpot.ContainsKey("rock_unit") Then
                                    rockUnit = thisSpot("rock_unit")
                                    keyExists = rockUnit.ContainsKey(currentLine(0))
                                    If keyExists.Equals(True) Then
                                        lineExists = rockUnit.TryGetValue(currentLine(0), value)
                                        If String.IsNullOrEmpty(value) Then
                                            value = ""
                                        End If
                                        If value.Equals(currentLine(1)) Then
                                            'Debug.Print("KVP exists")
                                            Exit Try
                                        ElseIf lineExists.Equals(True) Then
                                            rockUnit(currentLine(0)) = currentLine(1)
                                            'Debug.Print("Changed: " + currentLine(0) + currentLine(1))
                                            changesCount += 1
                                        End If
                                    End If
                                End If
                                If thisSpot.ContainsKey("trace") Then
                                    traceData = thisSpot("trace")
                                    keyExists = traceData.ContainsKey(currentLine(0))
                                    If keyExists.Equals(True) Then
                                        lineExists = traceData.TryGetValue(currentLine(0), value)
                                        If String.IsNullOrEmpty(value) Then
                                            value = ""
                                        End If
                                        If value.Equals(currentLine(1)) Then
                                            'Debug.Print("KVP exists")
                                            Exit Try
                                        ElseIf lineExists.Equals(True) Then
                                            traceData(currentLine(0)) = currentLine(1)
                                            'Debug.Print("Changed: " + currentLine(0) + currentLine(1))
                                            changesCount += 1
                                        End If
                                    End If
                                End If
                                If thisSpot.ContainsKey("samples") Then
                                    chunkNum = 0
                                    sampleData = thisSpot("samples")
                                    For Each chunk In sampleData
                                        sampleID = (chunk)("id")
                                        If sampleID.ToString.Contains(innerID) Then
                                            keyExists = (sampleData(chunkNum)).ContainsKey(currentLine(0))
                                            If keyExists.Equals(True) Then
                                                lineExists = (sampleData(chunkNum)).TryGetValue(currentLine(0), value)
                                                If String.IsNullOrEmpty(value) Then
                                                    value = ""
                                                End If
                                                If value.Equals(currentLine(1)) Then
                                                    'Debug.Print("KVP exists")
                                                    Exit Try
                                                ElseIf lineExists.Equals(True) Then
                                                    sampleData(currentLine(0)) = currentLine(1)
                                                    'Debug.Print("Changed: " + currentLine(0) + currentLine(1))
                                                    changesCount += 1
                                                End If
                                            End If
                                        End If
                                        chunkNum += 1
                                    Next
                                End If
                                If thisSpot.ContainsKey("orientation_data") Then
                                    chunkNum = 0
                                    oriData = thisSpot("orientation_data")
                                    For Each chunk In oriData
                                        oriID = (chunk)("id")
                                        If oriData(chunkNum).ContainsKey("associated_orientation") Then
                                            aoData = oriData(chunkNum)("associated_orientation")
                                            blockNum = 0
                                            For Each block In aoData
                                                aoID = block("id")
                                                If aoID.ToString.Contains(innerID) Then
                                                    keyExists = (aoData(blockNum)).ContainsKey(currentLine(0))
                                                    If keyExists.Equals(True) Then
                                                        lineExists = (aoData(blockNum)).TryGetValue(currentLine(0), value)
                                                        If String.IsNullOrEmpty(value) Then
                                                            value = ""
                                                        End If
                                                        If value.Equals(currentLine(1)) Then
                                                            'Debug.Print("KVP exists")
                                                            Exit Try
                                                        ElseIf lineExists.Equals(True) Then
                                                            aoData(blockNum)(currentLine(0)) = currentLine(1)
                                                            'Debug.Print("Changed: " + currentLine(0) + currentLine(1))
                                                            changesCount += 1
                                                        End If
                                                    End If
                                                End If
                                                blockNum += 1
                                            Next
                                        End If
                                        If oriID.ToString.Contains(innerID) Then
                                            keyExists = (oriData(chunkNum)).ContainsKey(currentLine(0))
                                            If keyExists.Equals(True) Then
                                                lineExists = (oriData(chunkNum)).TryGetValue(currentLine(0), value)
                                                If String.IsNullOrEmpty(value) Then
                                                    value = ""
                                                End If
                                                If value.Equals(currentLine(1)) Then
                                                    'Debug.Print("KVP exists")
                                                    Exit Try
                                                ElseIf lineExists.Equals(True) Then
                                                    oriData(chunkNum)(currentLine(0)) = currentLine(1)
                                                    'Debug.Print("Changed: " + currentLine(0) + currentLine(1))
                                                    changesCount += 1
                                                End If
                                            End If
                                        End If
                                        chunkNum += 1
                                    Next
                                End If
                                If thisSpot.ContainsKey("_3d_structures") Then
                                    chunkNum = 0
                                    _3dData = thisSpot("_3d_structures")
                                    For Each chunk In _3dData
                                        _3dID = chunk("id")
                                        If _3dID.ToString.Contains(innerID) Then
                                            keyExists = (_3dData(chunkNum)).ContainsKey(currentLine(0))
                                            If keyExists.Equals(True) Then
                                                lineExists = (_3dData(chunkNum)).TryGetValue(currentLine(0), value)
                                                If String.IsNullOrEmpty(value) Then
                                                    value = ""
                                                End If
                                                If value.Equals(currentLine(1)) Then
                                                    'Debug.Print("KVP exists")
                                                    Exit Try
                                                ElseIf lineExists.Equals(True) And innerID.Equals("") Then
                                                    _3dData(chunkNum)(currentLine(0)) = currentLine(1)
                                                    'Debug.Print("Changed: " + currentLine(0) + currentLine(1))
                                                    changesCount += 1
                                                End If
                                            End If
                                        End If
                                        chunkNum += 1
                                    Next
                                End If
                                'If the key was never found (it was a new key:value made in ArcMap) then add it to the large array
                                If keyExists.Equals(False) Then
                                    'Debug.Print("KVP does not exist")
                                    newValue = currentLine(1)
                                    thisSpot.Add(currentLine(0), newValue)
                                    changesCount += 1
                                End If
                                If changesCount > 0 Then
                                    Debug.Print("Changes made")
                                    unixTime = (DateTime.Now - startEpoch).TotalMilliseconds
                                    thisSpot("modified_timestamp") = unixTime
                                End If
                            Catch ex As Exception
                                Debug.Print("Inner exception: " + ex.Message.ToString)
                            End Try
                        End If
                    Catch ex As Exception
                        Debug.Print(ex.Message.ToString)
                    End Try
                Next
            Else
                Continue For
            End If
            Return wholeJSON
        Next
    End Function

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
        Dim spatRefFactory As ISpatialReferenceFactory3 = New SpatialReferenceEnvironmentClass()
        Dim wgs84iSR As ISpatialReference3 = spatRefFactory.CreateSpatialReference(4326)
        Dim dt As Object = ""
        Dim fileName As String
        Dim jsonPath As String
        Dim typeResponse As Boolean
        Dim shpDatasetName As String
        Dim shpDatasets As ArrayList = New ArrayList()
        Dim shpFile As String = "C:\temp\StraboShps"
        Dim featToShp As ESRI.ArcGIS.ConversionTools.FeatureClassToShapefile = New ESRI.ArcGIS.ConversionTools.FeatureClassToShapefile()
        Dim reProject As ESRI.ArcGIS.DataManagementTools.CopyFeatures = New ESRI.ArcGIS.DataManagementTools.CopyFeatures()
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
            Dim uri As String = "https://strabospot.org/db/project"
            Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
            Dim modTimeStamp As Int64 = (DateTime.UtcNow - startEpoch).TotalMilliseconds
            Dim prjid As String = modTimeStamp.ToString + randDig
            Dim today As String = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            Dim prjName As String = TextBox1.Text
            Dim self As String = uri + "/" + prjid
            Dim prjData As New StringBuilder()
            Dim isCreated As String
            Dim authorization As String
            Dim binaryauthorization As Byte()

            s = HttpWebRequest.Create(uri)
            enc = New System.Text.UTF8Encoding()
            prjData.Append("{" + Environment.NewLine + """self"" : """ + self + """," + Environment.NewLine + """id"" : ")
            prjData.Append(CType(prjid, Int64))
            prjData.Append("," + Environment.NewLine + """date"" : """ + today.ToString + """," + Environment.NewLine + """modified_timestamp"" : ")
            prjData.Append(modTimeStamp)
            prjData.Append("," + Environment.NewLine + """description"" : {" + Environment.NewLine + """project_name"" : """ + "" + prjName + """")
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
                Dim result As HttpWebResponse = CType(s.GetResponse(), HttpWebResponse)
                Dim statusCode As String = result.StatusCode.ToString
                datastream = result.GetResponseStream()
                reader = New StreamReader(datastream)
                responseFromServer = reader.ReadToEnd()
                Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                isCreated = p.ToString
                If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                    MessageBox.Show("Strabo Project " + TextBox1.Text + " Successfully Created!")
                Else
                    MessageBox.Show("Error creating Strabo Project. Try your request again.")
                    TextBox1.Clear()
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
                Debug.Print(fileName)
                If Not System.IO.Directory.Exists(fileName) Then
                    System.IO.Directory.CreateDirectory(fileName)
                End If
                gp.SetEnvironmentValue("workspace", ws)

                If Not (datasetSpatRef.Equals("GCS_WGS_1984")) Then
                    reProject.in_features = ws + "\" + dataset.AliasName
                    reProject.out_feature_class = ws + "\" + dataset.AliasName + "_Projected"
                    gp.AddOutputsToMap = False
                    gp.SetEnvironmentValue("outputCoordinateSystem", "GEOGCS['GCS_WGS_1984',DATUM['D_WGS_1984',SPHEROID['WGS_1984',6378137.0,298.257223563]],PRIMEM['Greenwich',0.0],UNIT['Degree',0.0174532925199433],AUTHORITY['EPSG',4326]]")
                    Dim sev As Object = Nothing
                    Try
                        gp.Execute(reProject, Nothing)
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
                    uri = "https://strabospot.org/db/dataset"
                    modTimeStamp = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                    Dim datasetid As String = modTimeStamp.ToString + randDig
                    today = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    Dim datasetName As String = dataset.AliasName
                    self = uri + "/" + datasetid
                    Dim datasetData As New StringBuilder()

                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    datasetData.Append("{" + Environment.NewLine + """id"" : ")
                    datasetData.Append(CType(datasetid, Int64))
                    datasetData.Append("," + Environment.NewLine + """name"" : """ + "" + datasetName + """,")
                    datasetData.Append(Environment.NewLine + """modified_timestamp"" : ")
                    datasetData.Append(modTimeStamp)
                    datasetData.Append("," + Environment.NewLine + """date"" : """ + today.ToString + """")
                    datasetData.Append(Environment.NewLine + "}")
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
                        Dim result As HttpWebResponse = CType(s.GetResponse(), HttpWebResponse)
                        Dim statusCode As String = result.StatusCode.ToString
                        Debug.Print(statusCode)
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()
                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString
                        If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                            MessageBox.Show("Strabo Dataset " + TextBox1.Text + " Successfully Created!")
                        Else
                            MessageBox.Show("Error creating Strabo Dataset. Try your request again.")
                            TextBox1.Clear()
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Then Add the New Dataset to the Project 
                    Dim addDataset As New StringBuilder()
                    uri = "https://strabospot.org/db/projectDatasets/" + prjid
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    addDataset.Append("{" + Environment.NewLine + """id"" : ")
                    addDataset.Append(datasetid)
                    addDataset.Append(Environment.NewLine + "}")
                    Debug.Print(addDataset.ToString())
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
                        Dim result As HttpWebResponse = CType(s.GetResponse(), HttpWebResponse)
                        Dim statusCode As String = result.StatusCode.ToString
                        Debug.Print(statusCode)
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()
                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString
                        If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                            MessageBox.Show("Strabo dataset " + datasetName + " Successfully Added to " + Projects.SelectedItem)
                        Else
                            MessageBox.Show("""Error"": ""Dataset """ + datasetid + """ not found.""")
                            TextBox1.Clear()
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
                    'PARSE THE NEWLY CREATED ESRIJSON FILE THEN EDIT THE ORIGINAL 
                    'Set up GeoJson StringBuilder 
                    sr = New StreamReader(jsonPath)
                    wholeFile = File.ReadAllLines(jsonPath)
                    Debug.Print(jsonPath)
                    For Each i In wholeFile
                        If i.ToString.Contains("null") Or i.ToString.Contains("FID") Then
                            Continue For
                        Else
                            esriFile = esriFile + i.ToString + Environment.NewLine
                            numLines += 1
                        End If
                    Next
                    wholeFile = esriFile.Split(phrase, StringSplitOptions.None)
                    numAttributes = wholeFile.Length - 1
                    attributes = New List(Of String)(wholeFile)
                    attributes.RemoveAt(0)
                    attr = attributes.ToArray()
                    Dim geoCoords As String = String.Empty
                    For Each a In attr
                        a = a.Remove(0, 2)
                        a = a.Replace("},", "")
                        For Each c In delChars
                            a = a.Replace(c, "")
                        Next
                        Dim splitLines As String() = a.Split(New Char() {Environment.NewLine}, numLines, StringSplitOptions.RemoveEmptyEntries)
                        For Each value In splitLines
                            value = value.Trim
                            If value.Equals(String.Empty) Or value.Equals(",") Or value.Equals("""paths""" + " :") Or value.Equals("""rings""" + " :") Then
                                Continue For
                                'If the value doesn't contain a colon it is likely a geometry coordinate from a line or polygon
                            ElseIf Not value.Contains(":") Then
                                geoCoords += value + Environment.NewLine
                            Else
                                parts = value.Split(New Char() {":"}, 2)
                                'If the specific SpotID is already in the string builder then it is a continuation of the same spot. This information isn't needed. 
                                '----But... What if the coordinates changed between features but share the same spot? Need to add in a comparison or confirmation 
                                'for the user to either pick the geometry they want for a spot or compare early to the origJson and take the different one----
                                If parts(0).Contains("SpotID") Then
                                    If sb.ToString.Contains(parts(1)) Then
                                        sb.Append("")
                                        geoCoords = String.Empty
                                    ElseIf s.Equals(attr(0).ToString) Then    'Check if this is the first spot in the sequence-- if so keep adding 
                                        sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                    Else    'If the specific SpotID is not already in the string builder, this means it has moved to a different spot's info. Add a break and continue. 
                                        If sb.ToString.Contains(geoCoords) Then
                                            sb.Append("----------------------------------")
                                            sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                            spots += 1
                                        Else
                                            sb.Append("""geometry"": " + Environment.NewLine)
                                            sb.Append(geoCoords)
                                            geoCoords = String.Empty
                                            sb.Append("----------------------------------")
                                            sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                            spots += 1
                                        End If
                                    End If
                                ElseIf parts(0).Contains("x") Then
                                    If sb.ToString.Contains(parts(1)) Then
                                        sb.Append("")
                                    Else
                                        sb.Append("""geometry"": " + Environment.NewLine)
                                        sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                    End If
                                ElseIf parts(0).Contains("y") Then
                                    If sb.ToString.Contains(parts(1)) Then
                                        sb.Append("")
                                    Else
                                        sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                    End If
                                Else
                                    sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                End If
                            End If
                        Next
                    Next
                    Dim strSeparator() As String = {"----------------------------------"}
                    attr = sb.ToString.Split(strSeparator, StringSplitOptions.None)
                    Dim origJsonFile As String = ""

                    If dataset.ShapeType.ToString.Equals("esriGeometryPoint") Then
                        origJsonFile = fileName + "\origPts.json"
                    ElseIf dataset.ShapeType.ToString.Equals("esriGeometryPolyline") Then
                        origJsonFile = fileName + "\origLines.json"
                    ElseIf dataset.ShapeType.ToString.Equals("esriGeometryPolygon") Then
                        origJsonFile = fileName + "\origPolygons.json"
                    End If
                    Dim sr2 = New StreamReader(origJsonFile)
                    Dim file2 As String
                    file2 = File.ReadAllText(origJsonFile)
                    Dim wholeJson As Object = New JavaScriptSerializer().Deserialize(Of Object)(file2)
                    Dim strLine As String
                    Dim spotList As Object
                    Dim spotID As Long
                    Dim esriJson As String
                    For Each spot In attr
                        esriJson += spot
                        Debug.Print(spot)
                    Next
                    For Each item In wholeJson("features")
                        spotList = item("properties")
                        For Each line In spotList
                            strLine = line.ToString().Trim("[", "]").Trim
                            parts = strLine.Split(New Char() {","}, 2)
                            If parts(0).Equals("id") Then
                                spotID = (parts(1))
                            End If
                        Next
                        strLine = CompareSpotIDs(spotID, esriJson)
                        If Not strLine.Equals(Nothing) Then
                            wholeJson = CompareESRItoOrig(spotID, wholeJson, strLine)
                        End If
                    Next
                    Dim editedJson As String = JsonConvert.SerializeObject(wholeJson)
                    Debug.Print(editedJson)
                    'Use the edited wholeJson to populate the new dataset using Strabo API: Upload Features
                    uri = "https://www.strabospot.org/db/datasetspots/" + datasetid
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    postdatabytes = enc.GetBytes(editedJson)
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
                        Dim result As HttpWebResponse = CType(s.GetResponse(), HttpWebResponse)
                        Dim statusCode As String = result.StatusCode.ToString
                        Debug.Print(statusCode)
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()
                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString
                        If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                            MessageBox.Show(datasetName + " added to Database")
                        Else
                            MessageBox.Show("""Error"": " + datasetName + " not added")
                            TextBox1.Clear()
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

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
                        Debug.Print(shpDatasetName)
                        Debug.Print(shpFile)
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
            Dim zipShp As String
            If Not shpDatasets.Count.Equals(0) Then
                zipShp = shpFile + ".zip"
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
                Debug.Print(zipShp)
                'Use Jason's code to upload the zipped shapefile of several feature classes to StraboSpot
                Dim response As Byte()
                Dim arcid As String
                Using wc As New System.Net.WebClient()
                    'UPLOAD the file to strabospot. ***NEED ZIPSHP TO POINT TO THE CORRECT ZIPPED FILE BEFORE TRYING****
                    response = wc.UploadFile("https://strabospot.org/arcupload.php", zipShp)
                    'the response from the server is a token for finishing the upload
                    arcid = wc.Encoding.GetString(response)
                    'Start the default browser to finish the shapefile upload
                    Process.Start(getDefaultBrowser, "https://strabospot.org/loadarcshapefile?arcid=" + arcid)
                End Using
            End If

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
                    Debug.Print(type)
                    'Use Copy Features Tool to change the projection of the dataset (should work for any dataset type)
                    reProject.in_features = ws + "\" + dataset.AliasName
                    reProject.out_feature_class = ws + "\" + dataset.AliasName + "_Projected"
                    gp.AddOutputsToMap = False
                    gp.SetEnvironmentValue("outputCoordinateSystem", "GEOGCS['GCS_WGS_1984',DATUM['D_WGS_1984',SPHEROID['WGS_1984',6378137.0,298.257223563]],PRIMEM['Greenwich',0.0],UNIT['Degree',0.0174532925199433],AUTHORITY['EPSG',4326]]")
                    Dim sev As Object = Nothing
                    Try
                        gp.Execute(reProject, Nothing)
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
            Dim uri As String = "https://strabospot.org/db/dataset"
            Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
            Dim modTimeStamp As Int64 = (DateTime.UtcNow - startEpoch).TotalMilliseconds
            Dim id As String = modTimeStamp.ToString + randDig
            Dim today As String = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            Dim datasetName As String = dataset.AliasName
            Dim self As String = uri + "/" + id
            Dim datasetData As New StringBuilder()
            Dim isCreated As String
            Dim authorization As String
            Dim binaryauthorization As Byte()

            s = HttpWebRequest.Create(uri)
            enc = New System.Text.UTF8Encoding()
            datasetData.Append("{" + Environment.NewLine + """id"" : ")
            datasetData.Append(CType(id, Int64))
            datasetData.Append("," + Environment.NewLine + """name"" : """ + "" + datasetName + """,")
            datasetData.Append(Environment.NewLine + """modified_timestamp"" : ")
            datasetData.Append(modTimeStamp)
            datasetData.Append("," + Environment.NewLine + """date"" : """ + today.ToString + """")
            datasetData.Append(Environment.NewLine + "}")
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
                Dim result As HttpWebResponse = CType(s.GetResponse(), HttpWebResponse)
                Dim statusCode As String = result.StatusCode.ToString
                Debug.Print(statusCode)
                datastream = result.GetResponseStream()
                reader = New StreamReader(datastream)
                responseFromServer = reader.ReadToEnd()
                Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                isCreated = p.ToString
                If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                    MessageBox.Show("Strabo Dataset " + datasetName + " Successfully Created!")
                Else
                    MessageBox.Show("Error creating Strabo Dataset. Try your request again.")
                    TextBox1.Clear()
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
            uri = "https://www.strabospot.org/db/projectDatasets/" + selprojectNum
            Debug.Print(uri)
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
                Dim result As HttpWebResponse = CType(s.GetResponse(), HttpWebResponse)
                Dim statusCode As String = result.StatusCode.ToString
                Debug.Print(statusCode)
                datastream = result.GetResponseStream()
                reader = New StreamReader(datastream)
                responseFromServer = reader.ReadToEnd()
                Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                isCreated = p.ToString
                If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                    MessageBox.Show("Strabo dataset " + datasetName + " Successfully Added to " + Projects.SelectedItem)
                Else
                    MessageBox.Show("""Error"": ""Dataset """ + datasetName + """ not found.""")
                    TextBox1.Clear()
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

            'PARSE THE NEWLY CREATED ESRIJSON FILE THEN FIND AND EDIT THE ORIGINAL JSON FILE
            'Set up GeoJson StringBuilder 
            sr = New StreamReader(jsonPath)
            wholeFile = File.ReadAllLines(jsonPath)
            For Each i In wholeFile
                If i.ToString.Contains("null") Or i.ToString.Contains("FID") Then
                    Continue For
                Else
                    esriFile = esriFile + i.ToString + Environment.NewLine
                    numLines += 1
                End If
            Next
            wholeFile = esriFile.Split(phrase, StringSplitOptions.None)
            numAttributes = wholeFile.Length - 1
            'Debug.Print(numAttributes.ToString)
            attributes = New List(Of String)(wholeFile)
            attributes.RemoveAt(0)
            attr = attributes.ToArray()
            Dim geoCoords As String = String.Empty
            For Each a In attr
                a = a.Remove(0, 2)
                a = a.Replace("},", "")
                For Each c In delChars
                    a = a.Replace(c, "")
                Next
                Dim splitLines As String() = a.Split(New Char() {Environment.NewLine}, numLines, StringSplitOptions.RemoveEmptyEntries)
                For Each value In splitLines
                    value = value.Trim
                    If value.Equals(String.Empty) Or value.Equals(",") Or value.Equals("""paths""" + " :") Or value.Equals("""rings""" + " :") Then
                        Continue For
                        'If the value doesn't contain a colon it is likely a geometry coordinate from a line or polygon
                    ElseIf Not value.Contains(":") Then
                        geoCoords += value + Environment.NewLine
                    Else
                        parts = value.Split(New Char() {":"}, 2)
                        'If the specific SpotID is already in the string builder then it is a continuation of the same spot. This information isn't needed. 
                        If parts(0).Contains("SpotID") Then
                            If sb.ToString.Contains(parts(1)) Then
                                sb.Append("")
                                geoCoords = String.Empty
                            ElseIf s.Equals(attr(0).ToString) Then    'Check if this is the first spot in the sequence-- if so keep adding 
                                sb.Append(parts(0) + parts(1) + Environment.NewLine)
                            Else    'If the specific SpotID is not already in the string builder, this means it has moved to a different spot's info. Add a break and continue. 
                                If sb.ToString.Contains(geoCoords) Then
                                    sb.Append("----------------------------------")
                                    sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                    spots += 1
                                Else
                                    sb.Append("""geometry"": " + Environment.NewLine)
                                    sb.Append(geoCoords)
                                    geoCoords = String.Empty
                                    sb.Append("----------------------------------")
                                    sb.Append(parts(0) + parts(1) + Environment.NewLine)
                                    spots += 1
                                End If
                            End If
                        ElseIf parts(0).Contains("x") Then
                            If sb.ToString.Contains(parts(1)) Then
                                sb.Append("")
                            Else
                                sb.Append("""geometry"": " + Environment.NewLine)
                                sb.Append(parts(0) + parts(1) + Environment.NewLine)
                            End If
                        ElseIf parts(0).Contains("y") Then
                            If sb.ToString.Contains(parts(1)) Then
                                sb.Append("")
                            Else
                                sb.Append(parts(0) + parts(1) + Environment.NewLine)
                            End If
                        Else
                            sb.Append(parts(0) + parts(1) + Environment.NewLine)
                        End If
                    End If
                Next
            Next
            Dim strSeparator() As String = {"----------------------------------"}
            attr = sb.ToString.Split(strSeparator, StringSplitOptions.None)
            'Get the original Json file to edit----------------------------------------------Think about if this is not there, getting the it in a stream from Strabo
            Dim origJsonFile As String = ""
            If dataset.ShapeType.ToString.Equals("esriGeometryPoint") Then
                origJsonFile = fileName + "\origPts.json"
            ElseIf dataset.ShapeType.ToString.Equals("esriGeometryPolyline") Then
                origJsonFile = fileName + "\origLines.json"
            ElseIf dataset.ShapeType.ToString.Equals("esriGeometryPolygon") Then
                origJsonFile = fileName + "\origPolygons.json"
            End If
            Dim sr2 = New StreamReader(origJsonFile)
            Dim file2 As String
            file2 = File.ReadAllText(origJsonFile)
            Dim wholeJson As Object = New JavaScriptSerializer().Deserialize(Of Object)(file2)
            Dim strLine As String
            Dim spotList As Object
            Dim spotID As Long
            Dim esriJson As String
            For Each spot In attr
                esriJson += spot
            Next
            For Each item In wholeJson("features")
                spotList = item("properties")
                For Each line In spotList
                    strLine = line.ToString().Trim("[", "]").Trim
                    parts = strLine.Split(New Char() {","}, 2)
                    If parts(0).Equals("id") Then
                        spotID = (parts(1))
                    End If
                Next
                strLine = CompareSpotIDs(spotID, esriJson)
                If Not strLine.Equals(Nothing) Then
                    wholeJson = CompareESRItoOrig(spotID, wholeJson, strLine)
                End If
            Next
            Dim editedJson As String = JsonConvert.SerializeObject(wholeJson)
            Debug.Print("Edited Json: " + editedJson)

            'Use the edited wholeJson to populate the new dataset using Strabo API: Upload Features
            uri = "https://www.strabospot.org/db/datasetspots/" + id
            Debug.Print(uri)
            s = HttpWebRequest.Create(uri)
            enc = New System.Text.UTF8Encoding()
            postdatabytes = enc.GetBytes(editedJson)
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
                Dim result As HttpWebResponse = CType(s.GetResponse(), HttpWebResponse)
                Dim statusCode As String = result.StatusCode.ToString
                Debug.Print(statusCode)
                datastream = result.GetResponseStream()
                reader = New StreamReader(datastream)
                responseFromServer = reader.ReadToEnd()
                Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                isCreated = p.ToString
                If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                    MessageBox.Show(datasetName + "added to Database")
                Else
                    MessageBox.Show("""Error"": " + datasetName + "not added")
                    TextBox1.Clear()
                End If

            Catch WebException As Exception
                MessageBox.Show(WebException.Message)
            End Try

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
                    Else 'Execute Feature Class to Shapefile script in Conversion Toolbox
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
                    response = wc.UploadFile("https://www.strabospot.org/arcupload.php", zipShp)
                    'the response from the server is a token for finishing the upload
                    arcid = wc.Encoding.GetString(response)
                    'Start the default browser to finish the shapefile upload
                    Process.Start(getDefaultBrowser, "https://www.strabospot.org/loadarcshapefile?arcid=" + arcid)
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
        RadioButton3.Visible = True
        Label1.Visible = True
        Button1.Visible = True
        back.Visible = True

        ListBox1.Items.Clear()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        RadioButton1.Visible = False
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

        ElseIf RadioButton3.Checked Then
            Label6.Visible = True
            Label5.Visible = True
            getProjects.Visible = True
            ListBox1.Visible = True
            Projects.Visible = True
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
        s = HttpWebRequest.Create("https://www.strabospot.org/db/myProjects")
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
                'Hide Log in tools 
                Label8.Visible = False
                Label9.Visible = False
                Username.Visible = False
                PasswordBox.Visible = False
                LogIn.Visible = False

                'Make tools for choosing upload method visible 
                RadioButton1.Visible = True
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

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Me.LinkLabel1.LinkVisited = True
        System.Diagnostics.Process.Start("https://www.strabospot.org")
    End Sub
End Class