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
    Dim projectFile As String
    Dim projectJson As Object
    Dim spotGeometry As String

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
    'NEW Compare function
    Function CompareESRItoOrig(ByVal esriID, ByVal spotAttr, ByVal spotGeometry, ByVal geoType, ByVal wholeJson) As Object
        Dim spot As Object
        Dim thisSpot As Object
        Dim origSpotGeo As Object
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
        Dim otherFeat As Object
        Dim chunkNum As Integer
        Dim rand As Random = New Random
        Dim randDig As String = rand.Next(1, 10)
        Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
        Dim unixTime As Int64
        Dim prjid As String = unixTime.ToString + randDig
        Dim today As String = Date.Today
        Dim newValue As JToken
        Dim innerID As String = ""
        Dim aoID As Object
        Dim blockNum As Integer
        Dim keyExists As Boolean
        Dim value As String = ""
        Dim selfValue As String = ""
        Dim featIDVal As String = ""
        Dim esriGeo(2) As String
        Dim geoPairs As Integer = 0
        Dim attribute As Object
        Dim origGeom As String = ""
        Dim origGeom2 As String = ""
        Dim strLine As String
        Dim parts As String()
        Dim spotToAdd As JToken
        Dim origGeoType As Object
        Dim newSpot As StringBuilder
        Dim splitGeo As String()
        Dim splitXY As String()
        Dim origSpotGeo2 As Object

        'If esriID is null ("") then do not loop through the whole Json features
        'Loop through spotAttr and spotGeo and add to end of wholeJson object (when new ESRI rows are made, they are at the bottom of the table)
        If esriID.Equals("") Then
            'Need to add in if the esriID is never found in the originalJson, it is a new spot and needs to be added to wholeJson
            'Based on the assumption the user would not try to fill out the SpotID or FeatID when adding a new attribute-- add explicit documentation
            spotToAdd = New JObject()
            newSpot = New StringBuilder()
            'Add original geometry part 
            newSpot.Append("{" + """original_geometry"":{")
            newSpot.Append("""type"": " + """" + geoType + """" + ",")
            newSpot.Append("""coordinates"":[")
            If geoType.Equals("Point") Then
                splitGeo = spotGeometry.Split(New Char() {" "}, 2)
                newSpot.Append(splitGeo(0) + "," + splitGeo(1).Trim + "]")
            Else    'Coordinates are lines or polygons
                splitGeo = spotGeometry.Split(New Char() {" "})
                For Each xy In splitGeo
                    newSpot.Append("[" + xy + "],")
                Next
                newSpot.Remove(newSpot.Length - 1, 1)
            End If
            newSpot.Append("},")
            'Add geometry part
            newSpot.Append("""geometry"":{")
            newSpot.Append("""type"": " + """" + geoType + """" + ",")
            newSpot.Append("""coordinates"":[")
            If geoType.Equals("Point") Then
                splitGeo = spotGeometry.Split(New Char() {" "}, 2)
                newSpot.Append(splitGeo(0) + "," + splitGeo(1).Trim + "]")
            Else    'Coordinates are lines or polygons
                splitGeo = spotGeometry.Split(New Char() {" "})
                For Each xy In splitGeo
                    newSpot.Append("[" + xy + "],")
                Next
                newSpot.Remove(newSpot.Length - 1, 1)
            End If
            newSpot.Append("},")
            'Add attribute/properties part 
            newSpot.Append("""properties"": {")
            'Append everthing in the row sent to the function 
            For Each at In spotAttr 'There should only be one 
                Debug.Print(at.ToString)
                strLine = at.ToString.Trim("[", "]").Trim
                parts = strLine.Split(New Char() {","}, 2)
                If String.IsNullOrEmpty(parts(1)) Then
                    Debug.Print("The ESRI value returned is null...")
                    Continue For    'Only add to the spot the info the user has put in this attribute 
                End If
                newSpot.Append("""" + parts(0).Trim + """" + ":" + """" + parts(1).Trim + """" + ",")
            Next
            newSpot.Remove(newSpot.Length - 1, 1)   'Get rid of last comma
            newSpot.Append("}," + """type"": ""Feature""}")
            Debug.Print("New spot Json: " + newSpot.ToString())
            Dim featJson As Object = wholeJson
            Dim feat As String = New JavaScriptSerializer().Serialize(featJson)
            feat = feat.Remove(feat.Length - 3, 3)
            Debug.Print(feat)
            feat += "},"
            feat += newSpot.ToString()
            feat += "]"
            feat += "}"
            Debug.Print(feat)
            wholeJson = New JavaScriptSerializer().DeserializeObject(feat)
            'Add it to wholeJson
        Else  'Begin to loop through the wholeJSON file to find the corresponding spot if esriID equals something
            For Each spot In wholeJson("features")
                origGeom = String.Empty
                origGeom2 = String.Empty
                thisSpot = spot("properties")
                origGeoType = spot("geometry")("type")
                If origGeoType.ToString.Equals("Point") Then
                    origSpotGeo = spot("geometry")("coordinates")
                    origSpotGeo2 = spot("original_geometry")("coordinates")
                    For Each part In origSpotGeo
                        origGeom += part.ToString + " "
                    Next
                    For Each part In origSpotGeo2
                        origGeom2 += part.ToString + " "
                    Next
                Else
                    origSpotGeo = spot("geometry")("coordinates")
                    origSpotGeo2 = spot("original_geometry")("coordinates")
                    For Each part In origSpotGeo
                        origGeom += part(0).ToString + "," + part(1).ToString + " "
                    Next
                    For Each part In origSpotGeo2
                        origGeom2 += part(0).ToString + "," + part(1).ToString + " "
                    Next
                End If
                If Not (origGeoType.ToString.Equals(geoType)) Then  'If there's no chance of an ID match, then make sure geometry = original_geometry
                    If Not origGeom.Equals(origGeom2) Then
                        spot("geometry")("coordinates") = spot("original_geometry")("coordinates")
                    End If
                End If
                'Find the spot which matches with the ESRIblock 
                If (thisSpot("id")).ToString.Equals(esriID.ToString) Then
                    'First compare the geometry data
                    If origGeom.Equals(origGeom2) Then  'If the original geometry=geometry then both are real world coords-- change to ArcMap if needed
                        If Not (spotGeometry.Equals(origGeom)) Then   'Replace geometry with the user chosen one from ArcMap
                            spotGeometry = spotGeometry.TrimEnd
                            If origGeoType.ToString.Equals("Point") And geoType.Equals("Point") Then
                                splitXY = spotGeometry.Split(New Char() {" "}, 2)
                                spot("geometry")("coordinates")(0) = CType(splitXY(0), Decimal)
                                spot("geometry")("coordinates")(1) = CType(splitXY(1), Decimal)
                            Else    'Works for lines or polygons
                                splitGeo = spotGeometry.Split(New Char() {" "})
                                Dim coordinates As JArray = New JArray()
                                Dim geoArr As JArray = New JArray()
                                For Each xy In splitGeo
                                    Debug.Print(xy)
                                    splitXY = xy.Split(New Char() {","}, 2)
                                    geoArr = New JArray()
                                    geoArr.Add(CType(splitXY(0), Decimal))
                                    geoArr.Add(CType(splitXY(1), Decimal))
                                    coordinates.Add(geoArr)
                                Next
                                spot("geometry")("coordinates") = coordinates
                            End If
                            Debug.Print("Changed " + origGeom + " to " + spotGeometry)
                        End If
                    Else 'The original geometry=/geometry, drop geometry and then rename the original geometry to geometry 
                        spot("geometry")("coordinates") = spot("original_geometry")("coordinates")
                    End If
                    Debug.Print("Original spot geometry: " + origGeom)
                    Debug.Print("User given spot geometry: " + spotGeometry)
                    'Then compare the attribute data 
                    For Each at As KeyValuePair(Of Integer, Object) In spotAttr
                        attribute = at.Value
                        'For Each ln In attribute
                        '    Debug.Print("******" + ln.ToString)
                        'Next
                        attribute.TryGetValue("self", selfValue)
                        If (Not String.IsNullOrEmpty(selfValue)) Then   'Skip any images ESRI spots
                            Debug.Print("Image Attribute")
                            Continue For
                        End If
                        'If true, this is part of the main spot array or under "other_features"
                        attribute.TryGetValue("FeatID", featIDVal)
                        If (String.IsNullOrEmpty(featIDVal)) OrElse (attribute("SpotID").ToString.Equals(attribute("FeatID").ToString)) Then
                            Debug.Print("Large Array Data: ")
                            If Not (String.IsNullOrEmpty(featIDVal)) Then
                                Debug.Print("SpotID " + attribute("SpotID").ToString + " FeatID " + attribute("FeatID").ToString)
                            Else
                                Debug.Print("SpotID " + attribute("SpotID").ToString)
                            End If
                            For Each ln In attribute
                                strLine = ln.ToString.Trim("[", "]").Trim
                                parts = strLine.Split(New Char() {","}, 2)
                                If String.IsNullOrEmpty(parts(1)) Then      'If the ESRI value is null, skip the line
                                    'Debug.Print("The ESRI value returned is null...")
                                    parts(1) = ""
                                    Continue For
                                End If
                                parts(0) = parts(0).Trim
                                parts(1) = parts(1).Trim
                                If parts(0).Equals("modified_timestamp") Or parts(0).Equals("date") Or parts(0).Equals("SpotID") Or parts(0).Equals("FeatID") _
                                    Or parts(0).Equals("time") Or parts(0).Equals("self") Or parts(0).Equals("type") Or parts(0).Equals("FID") Then Continue For
                                Try
                                    'Debug.Print(parts(0) + parts(1))
                                    keyExists = thisSpot.ContainsKey(parts(0))
                                    If keyExists.Equals(True) Then   'If the key exists then the key was in the original large array-- check to see if it has been changed
                                        thisSpot.TryGetValue(parts(0), value)
                                        If String.IsNullOrEmpty(value) Then
                                            value = ""
                                        End If
                                        'Debug.Print(value)
                                        If value.Equals(parts(1)) Then
                                            'Debug.Print("KVP exists")
                                            Exit Try
                                        Else    'If the key exists in original spot but is not the same value, update
                                            thisSpot(parts(0)) = parts(1)
                                            Debug.Print("Changed: " + parts(0) + parts(1))
                                            changesCount += 1
                                        End If
                                    ElseIf thisSpot.ContainsKey("rock_unit") Then
                                        rockUnit = thisSpot("rock_unit")
                                        keyExists = rockUnit.ContainsKey(parts(0))
                                        If parts(0).Equals("rock_unit_notes") Then
                                            parts(0) = "notes"
                                        End If
                                        If parts(0).Equals("rock_unit_description") Then
                                            parts(0) = "description"
                                        End If
                                        If keyExists.Equals(True) Then
                                            rockUnit.TryGetValue(parts(0), value)
                                            If String.IsNullOrEmpty(value) Then
                                                value = ""
                                            End If
                                            If value.Equals(parts(1)) Then
                                                'Debug.Print("KVP exists")
                                                Exit Try
                                            Else
                                                rockUnit(parts(0)) = parts(1)
                                                Debug.Print("Changed: " + parts(0) + parts(1))
                                                changesCount += 1
                                            End If
                                        End If
                                    ElseIf thisSpot.ContainsKey("trace") Then
                                        traceData = thisSpot("trace")
                                        keyExists = traceData.ContainsKey(parts(0))
                                        If keyExists.Equals(True) Then
                                            traceData.TryGetValue(parts(0), value)
                                            If String.IsNullOrEmpty(value) Then
                                                value = ""
                                            End If
                                            If value.Equals(parts(1)) Then
                                                'Debug.Print("KVP exists")
                                                Exit Try
                                            Else
                                                traceData(parts(0)) = parts(1)
                                                Debug.Print("Changed: " + parts(0) + parts(1))
                                                changesCount += 1
                                            End If
                                        End If
                                    ElseIf thisSpot.ContainsKey("other_features") Then
                                        otherFeat = thisSpot("other_features")
                                        keyExists = otherFeat.ContainsKey(parts(0))
                                        If parts(0).Equals("other_type") Then
                                            parts(0) = "type"
                                        End If
                                        If parts(0).Equals("other_description") Then
                                            parts(0) = "description"
                                        End If
                                        If parts(0).Equals("other_name") Then
                                            parts(0) = "name"
                                        End If
                                        If keyExists.Equals(True) Then
                                            otherFeat.TryGetValue(parts(0), value)
                                            If String.IsNullOrEmpty(value) Then
                                                value = ""
                                            End If
                                            If value.Equals(parts(1)) Then
                                                'Debug.Print("KVP exists")
                                                Exit Try
                                            Else
                                                otherFeat(parts(0)) = parts(1)
                                                Debug.Print("Changed: " + parts(0) + parts(1))
                                                changesCount += 1
                                            End If
                                        End If
                                    ElseIf keyExists.Equals(False) And parts(1).Equals("") Then 'If the key wasn't in the original large array and the value is null, don't add it
                                        Continue For
                                    Else    'If the key wasn't in the original large array, but there is an ESRI value, add both key and value 
                                        Debug.Print("KVP does not exist")
                                        newValue = parts(1)
                                        thisSpot.Add(parts(0), newValue)
                                        changesCount += 1
                                        Debug.Print(parts(0) + parts(1))
                                    End If
                                Catch ex As Exception
                                    Debug.Print("Main array exception: " + ex.Message.ToString)
                                End Try
                            Next
                        ElseIf (Not attribute("SpotID").ToString.Equals(attribute("FeatID").ToString)) And (Not String.IsNullOrEmpty(featIDVal)) Then 'Both FeatID and SpotID are filled but not equal- inner array
                            Debug.Print("Detailed Array Data: ")
                            innerID = attribute("FeatID").ToString
                            Debug.Print("SpotID " + attribute("SpotID").ToString + " FeatID " + innerID)
                            For Each ln In attribute
                                strLine = ln.ToString.Trim("[", "]").Trim
                                parts = strLine.Split(New Char() {","}, 2)
                                If String.IsNullOrEmpty(parts(1)) Then
                                    'Debug.Print("The ESRI value returned is null...")
                                    parts(1) = ""
                                    Continue For
                                End If
                                parts(0) = parts(0).Trim
                                parts(1) = parts(1).Trim
                                If parts(0).Equals("modified_timestamp") Or parts(0).Equals("date") Or parts(0).Equals("SpotID") Or parts(0).Equals("FeatID") _
                                    Or parts(0).Equals("time") Or parts(0).Equals("self") Or parts(0).Equals("type") Or parts(0).Equals("FID") Then Continue For
                                Try
                                    If thisSpot.ContainsKey("samples") Then
                                        chunkNum = 0
                                        sampleData = thisSpot("samples")
                                        For Each chunk In sampleData
                                            sampleID = (chunk)("id")
                                            If sampleID.ToString.Contains(innerID) Then
                                                keyExists = (sampleData(chunkNum)).ContainsKey(parts(0))
                                                If keyExists.Equals(True) Then
                                                    sampleData(chunkNum).TryGetValue(parts(0), value)
                                                    If String.IsNullOrEmpty(value) Then
                                                        value = ""
                                                    End If
                                                    If value.Equals(parts(1)) Then
                                                        'Debug.Print("KVP exists")
                                                        Exit Try
                                                    Else
                                                        sampleData(chunkNum)(parts(0)) = parts(1)
                                                        Debug.Print("Changed: " + parts(0) + parts(1))
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
                                                        keyExists = (aoData(blockNum)).ContainsKey(parts(0))
                                                        If keyExists.Equals(True) Then
                                                            aoData(blockNum).TryGetValue(parts(0), value)
                                                            If String.IsNullOrEmpty(value) Then
                                                                value = ""
                                                            End If
                                                            If value.Equals(parts(1)) Then
                                                                'Debug.Print("KVP exists")
                                                                Exit Try
                                                            Else
                                                                aoData(blockNum)(parts(0)) = parts(1)
                                                                Debug.Print("Changed: " + parts(0) + parts(1))
                                                                changesCount += 1
                                                            End If
                                                        End If
                                                    End If
                                                    blockNum += 1
                                                Next
                                            End If
                                            If oriID.ToString.Contains(innerID) Then
                                                keyExists = (oriData(chunkNum)).ContainsKey(parts(0))
                                                If keyExists.Equals(True) Then
                                                    oriData(chunkNum).TryGetValue(parts(0), value)
                                                    If String.IsNullOrEmpty(value) Then
                                                        value = ""
                                                    End If
                                                    If value.Equals(parts(1)) Then
                                                        'Debug.Print("KVP exists")
                                                        Exit Try
                                                    Else
                                                        oriData(chunkNum)(parts(0)) = parts(1)
                                                        Debug.Print("Changed: " + parts(0) + parts(1))
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
                                        If parts(0).Equals("_3d_structures_type") Then
                                            parts(0) = "type"
                                        End If
                                        For Each chunk In _3dData
                                            _3dID = chunk("id")
                                            If _3dID.ToString.Contains(innerID) Then
                                                keyExists = (_3dData(chunkNum)).ContainsKey(parts(0))
                                                If keyExists.Equals(True) Then
                                                    _3dData(chunkNum).TryGetValue(parts(0), value)
                                                    If String.IsNullOrEmpty(value) Then
                                                        value = ""
                                                    End If
                                                    If value.Equals(parts(1)) Then
                                                        'Debug.Print("KVP exists")
                                                        Exit Try
                                                    Else
                                                        _3dData(chunkNum)(parts(0)) = parts(1)
                                                        Debug.Print("Changed: " + parts(0) + parts(1))
                                                        changesCount += 1
                                                    End If
                                                End If
                                            End If
                                            chunkNum += 1
                                        Next
                                    End If
                                    If keyExists.Equals(False) Then
                                        Debug.Print("KVP does not exist")
                                        newValue = parts(1)
                                        thisSpot.Add(parts(0), newValue)
                                        changesCount += 1
                                        Debug.Print(parts(0) + parts(1))
                                    End If
                                Catch ex As Exception
                                    Debug.Print("Inner exception: " + ex.Message.ToString)
                                End Try
                            Next
                        End If
                        If changesCount > 0 Then
                            Debug.Print("Changes made")
                            unixTime = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                            thisSpot("modified_timestamp") = unixTime
                        End If
                    Next
                End If
            Next
        End If
        Return wholeJson
    End Function

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs)
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
        Dim fileName As String = ""
        Dim jsonPath As String = ""
        Dim typeResponse As Boolean
        Dim shpDatasetName As String = ""
        Dim shpDatasets As ArrayList = New ArrayList()
        Dim shpFile As String = "C:\temp\StraboShps"
        Dim featToShp As ESRI.ArcGIS.ConversionTools.FeatureClassToShapefile = New ESRI.ArcGIS.ConversionTools.FeatureClassToShapefile()
        Dim reProject As ESRI.ArcGIS.DataManagementTools.CopyFeatures = New ESRI.ArcGIS.DataManagementTools.CopyFeatures()
        gp.OverwriteOutput = True
        gp.AddOutputsToMap = False
        Dim splitFile() As String
        Dim endFile As String = ""
        Dim chosenDatasets As New StringBuilder()
        Dim chosenIndList As New StringBuilder()
        Dim fileLocation As DirectoryInfo
        Dim straboDatasetName As String = ""
        Dim sev As Object
        Dim wholeFile As String
        Dim fcJson As Object
        Dim esriRows As Object
        Dim row As Object
        Dim parts As String()

        If System.IO.Directory.Exists(shpFile) Then
            For Each file As String In
            System.IO.Directory.GetFiles(shpFile)
                System.IO.File.Delete(file)
            Next
        End If

        If RadioButton1.Checked Then        'Initiates Versioning of the Strabo Dataset (Updates Project, Dataset, Dataset Spots)- Overwriting Option
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
                jsonPath = fileName + "\" + featLayer.Name + "toJson.json"
                Try
                    If System.IO.Directory.Exists(jsonPath) Then
                        System.IO.Directory.Delete(jsonPath)
                        Debug.Print("Error: Json File already exists.")
                    End If
                Catch ex As Exception
                    Debug.Print("Exception down near checking for ESRI Json file already in existence: " + ex.ToString)
                End Try

                If typeResponse.Equals(True) Then   'Native Strabo Dataset
                    Dim datasetName As String = dataset.AliasName
                    Dim datasetData As New StringBuilder()
                    Dim datasetFileName As String = ""
                    Dim datasetSplit As String()
                    Dim wholeJson As Object
                    Dim isCreated As String
                    Dim authorization As String
                    Dim binaryauthorization As Byte()
                    fileLocation = New DirectoryInfo(fileName)
                    datasetName = dataset.AliasName
                    If Not (datasetName.Contains("_Tags")) Then  'If the Feature Class is not full of Tags then GET dataset
                        'Gather Dataset Data
                        For Each File In fileLocation.GetFiles()
                            If File IsNot Nothing Then
                                If File.ToString.ToLower.Contains("dataset") Then
                                    datasetFileName = File.FullName
                                End If
                            End If
                        Next
                        datasetFileName = datasetFileName.Remove(datasetFileName.Length - 5, 5)
                        datasetSplit = datasetFileName.Split(New Char() {"-"}, 2)
                        selDatasetNum = datasetSplit(1)
                        datasetSplit = (New DirectoryInfo(datasetFileName).Parent.Name).Split(New Char() {"_"})
                        straboDatasetName = datasetSplit(0)

                        'GET Dataset GeoJSON
                        s = HttpWebRequest.Create("https://strabospot.org/db/datasetspotsarc/" + selDatasetNum)
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
                            Debug.Print("Dataset wholeJson: " + responseFromServer)
                            wholeJson = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)

                        Catch WebException As Exception
                            MessageBox.Show(WebException.Message)
                        End Try
                    End If

                    '*****************UPDATE STRABO PROJECT (POST)************** 
                    Dim rand As Random = New Random
                    Dim randDig As String = rand.Next(1, 10)
                    Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
                    Dim modTimeStamp As Int64 = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                    Dim today As String = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    Dim prjData As String
                    Dim projectFileName As String = ""
                    Dim prjName As String()
                    Dim uri As String
                    For Each File In fileLocation.GetFiles()
                        If File IsNot Nothing Then
                            If File.ToString.ToLower.Contains("project") Then
                                projectFileName = File.FullName
                            End If
                        End If
                    Next
                    projectFileName = projectFileName.Remove(projectFileName.Length - 5, 5)
                    prjName = projectFileName.Split(New Char() {"-"}, 2)
                    selprojectNum = prjName(1)
                    Debug.Print("Project Number " + selprojectNum)

                    'Get Project Info from Strabo
                    s = HttpWebRequest.Create("https://strabospot.org/db/project/" + selprojectNum)
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
                        projectJson = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try
                    projectJson("modified_timestamp") = modTimeStamp    'Signal updating project to the database

                    'Run Tags(Features) to Json for comparison with projectJson  
                    If (datasetName.Contains("_Tags")) Then
                        Debug.Print("Converting Tags Feature Class")
                        'Execute Features to Json on the Tags Feature Class
                        featToJson.in_features = ws + "\" + dataset.AliasName
                        featToJson.out_json_file = jsonPath
                        featToJson.format_json = "FORMATTED"
                        sev = Nothing
                        Try
                            gp.Execute(featToJson, Nothing)
                            Console.WriteLine(gp.GetMessages(sev))

                        Catch ex As Exception
                            Console.WriteLine(gp.GetMessages(sev))
                        End Try
                        'Compare with the original project JSON 
                        sr = New StreamReader(jsonPath)
                        wholeFile = File.ReadAllText(jsonPath)
                        fcJson = New JavaScriptSerializer().Deserialize(Of Object)(wholeFile)
                        esriRows = fcJson("features")
                        Dim tagkvp As String = ""
                        Dim val As String = ""
                        Dim modifiedTags As String = ""
                        Dim esriTagID As String = ""
                        Dim origTag As Object
                        Dim tagNum As Integer = 0
                        For Each tg In esriRows
                            row = tg("attributes")
                            esriTagID = CType(row("Tags_tagID"), String)
                            origTag = projectJson("tags")
                            For Each straboTag In origTag
                                If (esriTagID.Equals(straboTag("id").ToString)) And (Not modifiedTags.Contains(esriTagID)) Then
                                    For Each kvp In row
                                        tagkvp = kvp.ToString().Trim("[", "]").Trim
                                        parts = tagkvp.Split(New Char() {","}, 2)
                                        If String.IsNullOrEmpty(parts(1)) Then
                                            Continue For
                                        End If
                                        If parts(0).Equals("OBJECTID") Or parts(0).Contains("_SpotID") Or parts(0).Contains("_tagID") Then Continue For
                                        If origTag.TryGetValue(parts(0), val) Then
                                            If String.IsNullOrEmpty(val) Then
                                                val = ""
                                            End If
                                            If parts(1).Equals(val) Then
                                                Continue For
                                            Else
                                                origTag(tagNum)(parts(0)) = parts(1)
                                                modifiedTags += esriTagID   'Prevents the edits from getting re-edited later if a tag has multiple spots
                                            End If
                                        End If
                                    Next
                                End If
                                tagNum += 1 'Go to the next Tag in the Tags Array in the Project Json
                            Next
                        Next
                        'Upload the edited project Json to Strabo
                        prjData = New JavaScriptSerializer().Serialize(projectJson)
                        uri = "https://strabospot.org/db/project/" + selprojectNum
                        s = HttpWebRequest.Create(uri)
                        enc = New System.Text.UTF8Encoding()

                        Debug.Print(prjData)    'Check to see if the modified timestamp updated
                        postdatabytes = enc.GetBytes(prjData)
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
                            If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                                MessageBox.Show("Strabo Project " + selprojectNum + " successfully updated!")
                            Else
                                MessageBox.Show("Error updating Strabo Project. Try your request again.")
                            End If

                        Catch WebException As Exception
                            MessageBox.Show(WebException.Message)
                        End Try
                        Continue For    'Go to next Feature Dataset the user has chosen 
                    End If

                    'Upload edited Json to Strabo to begin signaling versioning
                    prjData = New JavaScriptSerializer().Serialize(projectJson)
                    uri = "https://strabospot.org/db/project/" + selprojectNum
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()

                    Debug.Print(prjData)    'Check to see if the modified timestamp updated
                    postdatabytes = enc.GetBytes(prjData)
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
                        If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                            MessageBox.Show("Strabo Project " + selprojectNum + " successfully updated!")
                        Else
                            MessageBox.Show("Error updating Strabo Project. Try your request again.")
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Reproject the dataset if not in WGS_84 using the CopyFeatures tool 
                    If Not (datasetSpatRef.Equals("GCS_WGS_1984")) Then
                        reProject.in_features = ws + "\" + dataset.AliasName
                        reProject.out_feature_class = ws + "\" + dataset.AliasName + "_Projected"
                        gp.AddOutputsToMap = False
                        gp.SetEnvironmentValue("outputCoordinateSystem", "GEOGCS['GCS_WGS_1984',DATUM['D_WGS_1984',SPHEROID['WGS_1984',6378137.0,298.257223563]],PRIMEM['Greenwich',0.0],UNIT['Degree',0.0174532925199433],AUTHORITY['EPSG',4326]]")
                        sev = Nothing
                        Try
                            gp.Execute(reProject, Nothing)
                            Console.WriteLine(gp.GetMessages(sev))

                        Catch ex As Exception
                            Console.WriteLine(gp.GetMessages(sev))
                        End Try
                    End If
                    '***********************UPDATE DATASET POST******************
                    'Update the dataset in Strabo- change modified timestamp to reflect the change for versioning purposes 
                    modTimeStamp = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                    today = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    Debug.Print("Dataset Name: " + straboDatasetName)
                    Debug.Print("Dataset Number: " + selDatasetNum)
                    Debug.Print("Dataset File Name: " + datasetFileName)
                    uri = "https://strabospot.org/db/dataset/" + selDatasetNum
                    Debug.Print("Dataset URI")
                    Debug.Print(uri)
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    datasetData.Append("{" + """id"" : ")
                    datasetData.Append(CType(selDatasetNum, Int64))
                    datasetData.Append("," + """name"" : """ + "" + straboDatasetName + """,")
                    datasetData.Append("""modified_timestamp"" : ")
                    datasetData.Append(modTimeStamp)
                    datasetData.Append("," + """date"" : """ + today.ToString + """" + "}")
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
                        datastream = result.GetResponseStream()
                        reader = New StreamReader(datastream)
                        responseFromServer = reader.ReadToEnd()
                        Dim p As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
                        isCreated = p.ToString
                        If statusCode.Equals("Created") Or statusCode.Equals("OK") Then
                            MessageBox.Show("Strabo Dataset info updated")
                        Else
                            MessageBox.Show("Error updating Strabo Dataset. Try your request again.")
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
                    featToJson.out_json_file = jsonPath
                    featToJson.format_json = "FORMATTED"
                    sev = Nothing
                    Try
                        gp.Execute(featToJson, Nothing)
                        Console.WriteLine(gp.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(gp.GetMessages(sev))
                    End Try
                    'If the Json File exists this feature has been uploaded to Strabo
                    'PARSE THE NEWLY CREATED ESRIJSON FILE THEN EDIT THE ORIGINAL 
                    sr = New StreamReader(jsonPath)
                    wholeFile = File.ReadAllText(jsonPath)
                    fcJson = New JavaScriptSerializer().Deserialize(Of Object)(wholeFile)
                    esriRows = fcJson("features")
                    Dim coord As Object
                    Dim esriID As String = String.Empty
                    Dim spotAttr As New Dictionary(Of Integer, Object)
                    Dim spotGeo As New Dictionary(Of Integer, Object)
                    Dim compareID As Object
                    Dim coords As String
                    Dim newFields As New Dictionary(Of Object, Object)
                    Dim spotID As String = String.Empty
                    Dim featID As String = String.Empty
                    Dim datasetID As String
                    Dim id As Int64
                    Dim name As String = ""
                    Dim spotName As String = ""
                    Dim geoType As String = ""
                    For Each i In esriRows
                        Debug.Print("New Row")
                        row = i("attributes")
                        'For Each ln In row
                        '    Debug.Print("*" + ln.ToString)
                        'Next
                        coord = i("geometry")
                        coords = String.Empty
                        If coord.ContainsKey("paths") Then
                            coord = coord("paths")(0)
                            For Each arr In coord
                                Debug.Print(arr(0).ToString)
                                Debug.Print(arr(1).ToString)
                                coords += arr(0).ToString + "," + arr(1).ToString + " "
                            Next
                            geoType = "LineString"
                        ElseIf coord.ContainsKey("rings") Then
                            coord = coord("rings")(0)
                            For Each arr In coord
                                Debug.Print(arr(0).ToString)
                                Debug.Print(arr(1).ToString)
                                coords += arr(0).ToString + "," + arr(1).ToString + " "
                            Next
                            geoType = "Polygon"
                        Else
                            Debug.Print("Coordinate set: ")
                            For Each c In coord
                                Debug.Print(c.ToString)
                                c = c.ToString().Trim("[", "]").Trim
                                parts = c.Split(New Char() {","}, 2)
                                coords += parts(1).Trim + " "
                            Next
                            geoType = "Point"
                        End If
                        row.TryGetValue("SpotID", spotID)
                        row.TryGetValue("FeatID", featID)
                        If String.IsNullOrEmpty(spotID) And String.IsNullOrEmpty(featID) Then
                            'This row was added in the ArcMap session
                            'Add the row info along with a SpotID, date, time, modified timestamp to the dictionary
                            rand = New Random
                            randDig = rand.Next(1, 10)
                            datasetID = ((DateTime.UtcNow - startEpoch).TotalMilliseconds).ToString + randDig
                            id = CType(datasetID, Int64)
                            row("id") = id
                            row("date") = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                            row("time") = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                            row("modified_timestamp") = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                            'Send this info to be added as a new spot to wholeJson in the function 
                            wholeJson = CompareESRItoOrig("", row, coords, geoType, wholeJson)
                        ElseIf row.ContainsKey("SpotID") And esriID.Equals(String.Empty) Then   'At the beginning 
                            '(SpotGeo and SpotAttr should both be nothing or have count = 0)
                            Debug.Print("Only works at the beginning")
                            esriID = row("SpotID").ToString
                            spotGeo.Add(1, coords)
                            Coordinates.Items.Add(coords)
                            spotAttr.Add(1, row)
                        ElseIf row.ContainsKey("SpotID") And (Not esriID.Equals(String.Empty)) Then 'If there is already a value in esriID from last row, check to see if the new row is a continuation of the same spot
                            compareID = row("SpotID").ToString
                            If compareID.Equals(esriID) Then    'If the new row is part of the same spot then add to spotAttr and spotGeo
                                'ATTRIBUTES
                                If spotAttr.Count = 0 Then
                                    Debug.Print("Brand New Spot Attributes")
                                    spotAttr.Add(1, row)
                                Else
                                    Debug.Print("Adding another attribute of the same spot")
                                    spotAttr.Add(spotAttr.Count + 1, row)
                                End If
                                'GEOMETRIES
                                Debug.Print("New Row, Same Spot " + compareID + " " + esriID)
                                If spotGeo.Count = 0 Then
                                    Debug.Print("Brand New Spot Geometries")
                                    spotGeo.Add(1, coords)
                                    Coordinates.Items.Add(coords)
                                ElseIf spotGeo.ContainsValue(coords) Then
                                    Continue For
                                    Debug.Print("Same geometry as last")
                                Else
                                    Debug.Print("Different geometries for the same spot's data")
                                    spotGeo.Add(spotGeo.Count + 1, coords)
                                    Coordinates.Items.Add(coords)
                                End If
                            Else
                                'If the row will be treated as a new spot then send old spot info to compare function 
                                'After that, add current row info to the cleared out dictionaries and reassign the esriID
                                If Coordinates.Items.Count > 1 Then
                                    Coordinates.Visible = True
                                Else    'Would this ever be because there are zero items/geometries?
                                    spotGeometry = spotGeo(1).ToString
                                End If
                                Debug.Print("The geometry passed to the compare function: " + spotGeometry)
                                Debug.Print("******************SEND TO COMPARE FUNCTION******************")
                                wholeJson = CompareESRItoOrig(esriID, spotAttr, spotGeometry, geoType, wholeJson)
                                Coordinates.Items.Clear()
                                spotGeo.Clear()
                                spotAttr.Clear()
                                spotAttr.Add(1, row)
                                spotGeo.Add(1, coords)
                                esriID = row("SpotID").ToString
                                Continue For
                            End If
                        End If
                    Next
                    'Catch the very last attribute in the ESRIJson 
                    If Coordinates.Items.Count > 1 Then
                        Coordinates.Visible = True
                    Else    'Would this ever be because there are zero items/geometries?
                        spotGeometry = spotGeo(1).ToString
                    End If
                    Debug.Print("sending last esri spot")
                    wholeJson = CompareESRItoOrig(esriID, spotAttr, spotGeometry, geoType, wholeJson)

                    'IDs remain the same value since the dataset is being overwritten
                    'Modified timestamps get changed within the Compare Function if appropriate 
                    'Need to remove the "self" and "original_geometry" objects from the JSON before uploading to Strabo
                    Dim editedFile As String = JsonConvert.SerializeObject(wholeJson)
                    Debug.Print("Through the Compare Function: " + editedFile)
                    Dim jWholeJson As JObject = New JObject()
                    jWholeJson = JObject.Parse(editedFile)
                    Dim jProperties As JObject = New JObject()
                    Dim jImages As JArray = New JArray()
                    Dim img As JToken
                    Dim self As JToken
                    For Each spot As JObject In jWholeJson("features")
                        spot.Property("original_geometry").Remove()
                        jProperties = spot("properties")
                        If jProperties.TryGetValue("self", self) Then
                            jProperties.Property("self").Remove()
                        End If
                        If jProperties.TryGetValue("images", img) Then
                            jImages = jProperties("images")
                            For Each i As JObject In jImages
                                i.Property("self").Remove()
                            Next
                        End If
                    Next
                    Dim editedJson As String = JsonConvert.SerializeObject(jWholeJson)
                    Debug.Print(editedJson)
                    'Delete and Resave the edited Dataset Json 
                    If System.IO.File.Exists(fileName + "\dataset-" + selDatasetNum + ".json") Then
                        System.IO.File.Delete(fileName + "\dataset-" + selDatasetNum + ".json")
                        Debug.Print("File Deleted")
                    End If
                    System.IO.File.WriteAllText(fileName + "\dataset-" + selDatasetNum + ".json", editedJson)
                    If System.IO.File.Exists(fileName + "\dataset-" + selDatasetNum + ".json") Then
                        Debug.Print("File Resaved")
                    End If
                    'Use the edited wholeJson to populate the original dataset using Strabo API: Upload Features
                    uri = "https://www.strabospot.org/db/datasetspots/" + selDatasetNum
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
                        sev = Nothing
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

                Me.Close()  'Close the Upload Dialog Box
            End If

        ElseIf RadioButton3.Checked Then        'Add a new dataset to the original project 

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
                    sev = Nothing
                    Try
                        gp.Execute(reProject, Nothing)
                        Console.WriteLine(gp.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(gp.GetMessages(sev))
                    End Try
                End If
                jsonPath = fileName + "\" + featLayer.Name + "toJson.json"
                Try
                    If System.IO.Directory.Exists(jsonPath) Then
                        System.IO.Directory.Delete(jsonPath)
                        Debug.Print("Error: Json File already exists.")
                    End If
                Catch ex As Exception
                    Debug.Print("Exception down near checking for ESRI Json file already in existence: " + ex.ToString)
                End Try

                If typeResponse.Equals(True) Then
                    'Update project, Create New Dataset, Add Dataset to the Original Project, Update Dataset
                    '************************GET Dataset GeoJSON****************************
                    Dim datasetName As String = dataset.AliasName
                    Dim datasetData As New StringBuilder()
                    Dim datasetFileName As String = ""
                    Dim datasetSplit As String()
                    fileLocation = New DirectoryInfo(fileName)
                    For Each File In fileLocation.GetFiles()
                        If File IsNot Nothing Then
                            If File.ToString.ToLower.Contains("dataset") Then
                                datasetFileName = File.FullName
                            End If
                        End If
                    Next
                    datasetFileName = datasetFileName.Remove(datasetFileName.Length - 5, 5)
                    datasetSplit = datasetFileName.Split(New Char() {"-"}, 2)
                    selDatasetNum = datasetSplit(1)
                    datasetSplit = (New DirectoryInfo(datasetFileName).Parent.Name).Split(New Char() {"_"})
                    straboDatasetName = datasetSplit(0)
                    Dim wholeJson As Object
                    Dim isCreated As String
                    Dim authorization As String
                    Dim binaryauthorization As Byte()
                    s = HttpWebRequest.Create("https://strabospot.org/db/datasetspotsarc/" + selDatasetNum)
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
                        wholeJson = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    Dim rand As Random = New Random
                    Dim randDig As String = rand.Next(1, 10)
                    Dim startEpoch As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
                    Dim modTimeStamp As Int64 = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                    Dim today As String = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    Dim prjData As String = ""
                    fileLocation = New DirectoryInfo(fileName)
                    Dim projectFileName As String = ""
                    Dim prjName As String()
                    For Each File In fileLocation.GetFiles()
                        If File IsNot Nothing Then
                            If File.ToString.ToLower.Contains("project") Then
                                projectFileName = File.FullName
                            End If
                        End If
                    Next
                    projectFileName = projectFileName.Remove(projectFileName.Length - 5, 5)
                    prjName = projectFileName.Split(New Char() {"-"}, 2)
                    selprojectNum = prjName(1)
                    Debug.Print(selprojectNum)
                    sr = New StreamReader(fileName + "\project-" + selprojectNum + ".json")
                    projectFile = File.ReadAllText(fileName + "\project-" + selprojectNum + ".json")
                    projectJson = New JavaScriptSerializer().Deserialize(Of Object)(projectFile)
                    projectJson("modified_timestamp") = modTimeStamp    'Signal updating project to the database
                    sr.Close()
                    '********************LATER ADD IN PARSER FOR CHANGES IN THE TAGS INFO*****************************************
                    'Delete and Resave the edited Project Json 
                    If System.IO.File.Exists(fileName + "\project-" + selprojectNum + ".json") Then
                        System.IO.File.Delete(fileName + "\project-" + selprojectNum + ".json")
                        Debug.Print("File Deleted")
                    End If
                    System.IO.File.WriteAllText(fileName + "\project-" + selprojectNum + ".json", prjData)
                    If System.IO.File.Exists(fileName + "\project-" + selprojectNum + ".json") Then
                        Debug.Print("File Resaved")
                    End If
                    prjData = New JavaScriptSerializer().Serialize(projectJson)
                    Dim uri As String = "https://strabospot.org/db/project/" + selprojectNum
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()

                    Debug.Print(prjData)    'Check to see if the modified timestamp updated
                    postdatabytes = enc.GetBytes(prjData)
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
                            MessageBox.Show("Strabo Project " + selprojectNum + " successfully updated!")
                        Else
                            MessageBox.Show("Error updating Strabo Project. Try your request again.")
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Create New Dataset
                    rand = New Random
                    randDig = rand.Next(1, 10)
                    uri = "https://strabospot.org/db/dataset"
                    startEpoch = New DateTime(1970, 1, 1, 0, 0, 0, 0)
                    modTimeStamp = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                    Dim seldatasetID As String = modTimeStamp.ToString + randDig
                    datasetName = dataset.AliasName
                    Dim self As String = uri + "/" + seldatasetID
                    datasetData = New StringBuilder()

                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    datasetData.Append("{" + Environment.NewLine + """id"" : ")
                    datasetData.Append(CType(seldatasetID, Int64))
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
                        End If

                    Catch WebException As Exception
                        MessageBox.Show(WebException.Message)
                    End Try

                    'Then Add the New Dataset to the Project 
                    Dim addDataset As String
                    uri = "https://www.strabospot.org/db/projectDatasets/" + selprojectNum
                    Debug.Print(uri)
                    s = HttpWebRequest.Create(uri)
                    enc = New System.Text.UTF8Encoding()
                    addDataset = "{" + Environment.NewLine + """id"" : """ + seldatasetID + """" + Environment.NewLine + "}"
                    postdatabytes = enc.GetBytes(addDataset)
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
                            MessageBox.Show("Strabo dataset " + datasetName + " Successfully Added to Project# " + selprojectNum)
                        Else
                            MessageBox.Show("""Error"": ""Dataset """ + datasetName + """ not found.""")
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
                    featToJson.out_json_file = jsonPath
                    featToJson.format_json = "FORMATTED"
                    sev = Nothing
                    Try
                        gp.Execute(featToJson, Nothing)
                        Console.WriteLine(gp.GetMessages(sev))

                    Catch ex As Exception
                        Console.WriteLine(gp.GetMessages(sev))
                    End Try

                    'PARSE THE NEWLY CREATED ESRIJSON FILE THEN FIND AND EDIT THE ORIGINAL JSON FILE
                    'Set up GeoJson StringBuilder 
                    sr = New StreamReader(jsonPath)
                    wholeFile = File.ReadAllText(jsonPath)
                    fcJson = New JavaScriptSerializer().Deserialize(Of Object)(wholeFile)
                    esriRows = fcJson("features")
                    Dim coord As Object
                    Dim esriID As String = String.Empty
                    Dim spotAttr As New Dictionary(Of Integer, Object)
                    Dim spotGeo As New Dictionary(Of Integer, Object)
                    Dim compareID As Object
                    Dim coords As String
                    Dim newFields As New Dictionary(Of Object, Object)
                    Dim spotID As String = String.Empty
                    Dim featID As String = String.Empty
                    Dim datasetID As String
                    Dim id As Int64
                    Dim name As String = ""
                    Dim spotName As String = ""
                    Dim geoType As String = ""
                    For Each i In esriRows
                        Debug.Print("New Row")
                        row = i("attributes")
                        'For Each ln In row
                        '    Debug.Print("*" + ln.ToString)
                        'Next
                        coord = i("geometry")
                        coords = String.Empty
                        If coord.ContainsKey("paths") Then
                            coord = coord("paths")(0)
                            For Each arr In coord
                                Debug.Print(arr(0).ToString)
                                Debug.Print(arr(1).ToString)
                                coords += arr(0).ToString + "," + arr(1).ToString + " "
                            Next
                            geoType = "LineString"
                        ElseIf coord.ContainsKey("rings") Then
                            coord = coord("rings")(0)
                            For Each arr In coord
                                Debug.Print(arr(0).ToString)
                                Debug.Print(arr(1).ToString)
                                coords += arr(0).ToString + "," + arr(1).ToString + " "
                            Next
                            geoType = "Polygon"
                        Else
                            Debug.Print("Coordinate set: ")
                            For Each c In coord
                                Debug.Print(c.ToString)
                                c = c.ToString().Trim("[", "]").Trim
                                parts = c.Split(New Char() {","}, 2)
                                coords += parts(1).Trim + " "
                            Next
                            geoType = "Point"
                        End If

                        row.TryGetValue("SpotID", spotID)
                        row.TryGetValue("FeatID", featID)
                        If String.IsNullOrEmpty(spotID) And String.IsNullOrEmpty(featID) Then
                            'This row was added in the ArcMap session
                            'Add the row info along with a SpotID, date, time, modified timestamp to the dictionary
                            rand = New Random
                            randDig = rand.Next(1, 10)
                            datasetID = ((DateTime.UtcNow - startEpoch).TotalMilliseconds).ToString + randDig
                            id = CType(datasetID, Int64)
                            row("id") = id
                            row("date") = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                            row("time") = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                            row("modified_timestamp") = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                            'Send this info to be added as a new spot to wholeJson in the function 
                            wholeJson = CompareESRItoOrig("", row, coords, geoType, wholeJson)
                        ElseIf row.ContainsKey("SpotID") And esriID.Equals(String.Empty) Then   'At the beginning 
                            '(SpotGeo and SpotAttr should both be nothing or have count = 0)
                            Debug.Print("Only works at the beginning")
                            esriID = row("SpotID").ToString
                            spotGeo.Add(1, coords)
                            Coordinates.Items.Add(coords)
                            spotAttr.Add(1, row)
                        ElseIf row.ContainsKey("SpotID") And (Not esriID.Equals(String.Empty)) Then 'If there is already a value in esriID from last row, check to see if the new row is a continuation of the same spot
                            compareID = row("SpotID").ToString
                            If compareID.Equals(esriID) Then    'If the new row is part of the same spot then add to spotAttr and spotGeo
                                'ATTRIBUTES
                                If spotAttr.Count = 0 Then
                                    Debug.Print("Brand New Spot Attributes")
                                    spotAttr.Add(1, row)
                                Else
                                    Debug.Print("Adding another attribute of the same spot")
                                    spotAttr.Add(spotAttr.Count + 1, row)
                                End If
                                'GEOMETRIES
                                Debug.Print("New Row, Same Spot " + compareID + " " + esriID)
                                If spotGeo.Count = 0 Then
                                    Debug.Print("Brand New Spot Geometries")
                                    spotGeo.Add(1, coords)
                                    Coordinates.Items.Add(coords)
                                ElseIf spotGeo.ContainsValue(coords) Then
                                    Continue For
                                    Debug.Print("Same geometry as last")
                                Else
                                    Debug.Print("Different geometries for the same spot's data")
                                    spotGeo.Add(spotGeo.Count + 1, coords)
                                    Coordinates.Items.Add(coords)
                                End If
                            Else
                                'If the row will be treated as a new spot then send old spot info to compare function 
                                'After that, add current row info to the cleared out dictionaries and reassign the esriID
                                If Coordinates.Items.Count > 1 Then
                                    Coordinates.Visible = True
                                Else    'Would this ever be because there are zero items/geometries?
                                    spotGeometry = spotGeo(1).ToString
                                End If
                                Debug.Print("******************SEND TO COMPARE FUNCTION******************")
                                wholeJson = CompareESRItoOrig(esriID, spotAttr, spotGeometry, geoType, wholeJson)
                                Coordinates.Items.Clear()
                                spotGeo.Clear()
                                spotAttr.Clear()
                                spotAttr.Add(1, row)
                                spotGeo.Add(1, coords)
                                esriID = row("SpotID").ToString
                                Continue For
                            End If
                        End If
                    Next
                    'Catch the very last attribute in the ESRIJson 
                    If Coordinates.Items.Count > 1 Then
                        Coordinates.Visible = True
                    Else    'Would this ever be because there are zero items/geometries?
                        spotGeometry = spotGeo(1).ToString
                    End If
                    Debug.Print("sending last esri spot")
                    wholeJson = CompareESRItoOrig(esriID, spotAttr, spotGeometry, geoType, wholeJson)

                    'Always change the spot's ID number and all associated IDs (so it doesn't become linked to the original spot in the database)
                    'This changes ALL IDs within the original Json------This is so the NEW Strabo dataset does not become linked  
                    'with the original dataset in the database
                    'Will need to be left off for the Overwriting Dataset Option (IDs will not change in that case, only modified timestamp where applicable)
                    Dim thisSpot As Object
                    Dim unixTime As Int64
                    Dim chunkNum As Integer
                    Dim _3dData As Object
                    Dim sampleData As Object
                    Dim oriData As Object
                    Dim aoData As Object
                    Dim blockNum As Integer
                    For Each spot In wholeJson("features")
                        thisSpot = spot("properties")
                        randDig = rand.Next(1, 10)
                        unixTime = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                        Dim newID As String = unixTime + randDig
                        thisSpot("id") = CType(newID, Int64)
                        thisSpot("self") = "https://strabospot.org/db/feature/" + newID
                        If thisSpot.ContainsKey("3d_structures") Then
                            chunkNum = 0
                            _3dData = thisSpot("_3d_structures")
                            For Each chunk In _3dData
                                unixTime = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                                randDig = rand.Next(1, 10)
                                newID = unixTime + randDig
                                _3dData(chunkNum)("id") = CType(newID, Int64)
                                chunkNum += 1
                            Next
                        End If
                        If thisSpot.ContainsKey("orientation_data") Then
                            chunkNum = 0
                            oriData = thisSpot("orientation_data")
                            For Each chunk In oriData
                                unixTime = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                                randDig = rand.Next(1, 10)
                                newID = unixTime + randDig
                                oriData(chunkNum)("id") = CType(newID, Int64)
                                If oriData(chunkNum).ContainsKey("associated_orientation") Then
                                    aoData = oriData(chunkNum)("associated_orientation")
                                    blockNum = 0
                                    For Each block In aoData
                                        unixTime = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                                        randDig = rand.Next(1, 10)
                                        newID = unixTime + randDig
                                        aoData(blockNum)("id") = CType(newID, Int64)
                                        blockNum += 1
                                    Next
                                End If
                                chunkNum += 1
                            Next
                        End If
                        If thisSpot.ContainsKey("samples") Then
                            chunkNum = 0
                            sampleData = thisSpot("samples")
                            For Each chunk In sampleData
                                unixTime = (DateTime.UtcNow - startEpoch).TotalMilliseconds
                                randDig = rand.Next(1, 10)
                                newID = unixTime + randDig
                                sampleData(chunkNum)("id") = CType(newID, Int64)
                                chunkNum += 1
                            Next
                        End If
                    Next
                    'Need to remove the "self" and "original_geometry" objects from the JSON before uploading to Strabo
                    Dim editedFile As String = JsonConvert.SerializeObject(wholeJson)
                    Dim jWholeJson As JObject = New JObject()
                    jWholeJson = JObject.Parse(editedFile)
                    Dim jProperties As JObject = New JObject()
                    Dim jImages As JArray = New JArray()
                    Dim img As JToken
                    For Each spot As JObject In jWholeJson("features")
                        spot.Property("original_geometry").Remove()
                        jProperties = spot("properties")
                        jProperties.Property("self").Remove()
                        If jProperties.TryGetValue("images", img) Then
                            jImages = jProperties("images")
                            For Each i As JObject In jImages
                                i.Property("self").Remove()
                            Next
                        End If
                    Next
                    Dim editedJson As String = JsonConvert.SerializeObject(jWholeJson)
                    Debug.Print("Edited Json: " + editedJson)
                    'Delete and Resave the edited Dataset Json 
                    If System.IO.File.Exists(fileName + "\dataset-" + selprojectNum + ".json") Then
                        System.IO.File.Delete(fileName + "\dataset-" + selprojectNum + ".json")
                        Debug.Print("File Deleted")
                    End If
                    System.IO.File.WriteAllText(fileName + "\dataset-" + selprojectNum + ".json", editedJson)
                    If System.IO.File.Exists(fileName + "\dataset-" + selprojectNum + ".json") Then
                        Debug.Print("File Resaved")
                    End If

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

                        sev = Nothing
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

                Me.Close() 'Close the Upload Dialog Box in ArcMap
            End If
        End If
        'Return user to the choices for uploading files
        Button2.Visible = False
        Label2.Visible = False
        Label5.Visible = False
        Label6.Visible = False
        ListBox1.Visible = False

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
            Label5.Visible = True
            ListBox1.Visible = True

        ElseIf RadioButton3.Checked Then
            Label6.Visible = True
            Label5.Visible = True
            ListBox1.Visible = True
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

    'Private Sub getProjects_Click(sender As Object, e As EventArgs)
    '    If Not Projects.Items.Equals(Nothing) Then
    '        Projects.Items.Clear()
    '        projectlist = String.Empty
    '        projectIDs = String.Empty
    '    End If

    '    'Get Project list first- "Get My Projects" from the Strabo API
    '    s = HttpWebRequest.Create("https://www.strabospot.org/db/myProjects")
    '    enc = New System.Text.UTF8Encoding()
    '    s.Method = "GET"
    '    s.ContentType = "application/json"

    '    authorization = emailaddress + ":" + password
    '    binaryauthorization = System.Text.Encoding.UTF8.GetBytes(authorization)
    '    authorization = Convert.ToBase64String(binaryauthorization)
    '    authorization = "Basic " + authorization
    '    s.Headers.Add("Authorization", authorization)

    '    Try
    '        Dim result = s.GetResponse()
    '        datastream = result.GetResponseStream()
    '        reader = New StreamReader(datastream)
    '        responseFromServer = reader.ReadToEnd()

    '        Dim j As Object = New JavaScriptSerializer().Deserialize(Of Object)(responseFromServer)
    '        j = j("projects")


    '        For Each i In j
    '            projectlist = projectlist + i("name") + "," + Environment.NewLine
    '            projectIDs = projectIDs + i("id").ToString() + "," + Environment.NewLine

    '        Next
    '        Projects.Visible = True

    '    Catch WebException As Exception
    '        MessageBox.Show(WebException.Message)
    '    End Try

    '    'Convert the String list into an Array and add to the List Box
    '    Dim projectlistArray As System.Array
    '    projectlistArray = projectlist.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)
    '    Projects.Items.AddRange(projectlistArray)
    'End Sub

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
            Label6.Visible = False
            Label5.Visible = False
            Button2.Visible = False
            Label2.Visible = False
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

    Private Sub Label3_Click(sender As Object, e As EventArgs) Handles Label3.Click

    End Sub

    Private Sub Coordinates_SelectedIndexChanged(sender As Object, e As EventArgs) Handles Coordinates.SelectedIndexChanged
        Coordinates.SelectionMode = SelectionMode.One
    End Sub
End Class