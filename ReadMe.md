# ArcMap 10.3 Add-In Linking to StraboSpot Database 
This Add-In links the StraboSpot Database with ESRI's ArcMap 

#### Directions for Installation
1.) Download and save the .esriAddIn file to a convenient location on your computer. 
2.) Double click the file to launch the AddIn Installation Utility window. Click Install. 
3.) Open ArcMap and go to Customize-->Toolbars and check 'Strabo' to add the 'Download' and 'Upload' buttons to the main toolbar.

**Important Note For Use:**
A StraboSpot account (username and password) is required for use of the Strabo online database, and this Add-In. Visit https://strabospot.org or see documentation for more information about the project and to set up an account.

#### Specifications
* Microsoft Visual Studio Community 2013 using .NET Framework 3.5 
* Libraries: ArcObjects, Ionic.Zip, Ionic.Zlib (https://dotnetzip.codeplex.com/), and Json.NET (http://www.newtonsoft.com/json) are also utilized. 
* **ArcGIS Requirements:** ArcGIS for Desktop version 10.3.1
    The following ArcToolbox extensions must be initalized: _Conversion Tools_ and _Data Management Tools_.

#### Download 
Produces the following on the user's computer: 
* ArcGIS File Geodatabase with name "datasetName_mm_dd_yyyy.gdb" containing: feature classes containing Spots separated by geometry (e.g. 'points', 'lines', 'polygons' if all are types in the Strabo dataset), feature classes for any Tags used (also named by geometry- e.g. '_geometry_ _Tags'), and a photo layer if user picks the option to download images in .jpeg format (use by clicking a photo point using the Html Popup tool). 
* A file folder saved to the same location as the file geodatabase (as specified by user) containing: various JSON files needed for download and any images from the dataset if the user opts for downloading as .jpeg or .tiff.

#### Upload
To upload edits made to the dataset in ArcMap back to Strabo the user has two options: 
1.) Overwriting the Dataset: replaces the dataset in Strabo with one also containing the ArcMap edits (does not nullify or delete data). _Preferred Option_- works in the same manner as the StraboSpot app and instigates versioning of the old dataset. 
2.) Create a New Strabo Dataset: populates a new Strabo dataset created in the _same_ Strabo project as the old dataset that also contains the ArcMap edits. The option for deleting any spots not in the ArcMap dataset is given. This method is useful for making backup copies of datasets or producing a dataset of select spots. 
The option to upload datasets not originating in Strabo (Native ArcGIS) is also given the user. If such a dataset is chosen for upload, it will be done through the Shapefile upload tool on strabospot.org. The Add-In produces the Shapefile(s) if in another type, projects them in WGS84 if needed, zips the file(s), and hands the uploading over to strabospot.org in the user's preferred browser. 

**Please see additional documentation for further details and tips for use.**