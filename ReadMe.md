# StraboSpot/ArcMap Add-In
This Add-In provides download/upload capabilities between the StraboSpot Database and ESRI's ArcMap 10.3.1

#### Directions for Installation
1.) Download and save the .esriAddIn file to a convenient location on your computer. 
2.) Double click the file to launch the Add-In Installation Utility window. Click Install. 
3.) Open ArcMap and go to Customize-->Toolbars and check 'StraboSpot' to add the 'Download' and 'Upload' buttons to the main toolbar.

**Important Note For Use:**
A StraboSpot account (username and password) is required for use of the StraboSpot online database, and this Add-In. Visit https://strabospot.org for more information.

#### Specifications
* Microsoft Visual Studio Community 2013 using .NET Framework 3.5 
* Libraries: ArcObjects SDK, Ionic.Zip, Ionic.Zlib (https://dotnetzip.codeplex.com/), and Json.NET (http://www.newtonsoft.com/json). 
* **ArcGIS Requirements:** ArcGIS for Desktop version 10.3.1 with the _Conversion Tools_ and _Data Management Tools_ toolboxes initalized.

#### Download 
Produces the following on the user's computer: 
* ArcGIS File Geodatabase with name "<StraboSpotProjectName_mm_dd_yyyy>.gdb" containing: ArcGIS Feature Datasets for each StraboSpot Dataset in the Project chosen by the user containing feature classes named by geometry (e.g. 'points', 'lines', 'polygons' if all are types in the Strabo dataset) since one StraboSpot Dataset can house multiple geometry types, feature classes for any Tags used (also named by geometry- e.g. '_geometry_ _Tags'), and (at the geodatabase level, named by StraboSpot Dataset) a photo layer if the user chooses to download images in .jpeg format (use with the Html Popup tool). 
* A file folder saved to the same location as the file geodatabase (as specified by user) containing: various JSON files needed for download/upload capabilities and any images downloaded (in separate file folders named by StraboSpot dataset).

#### Upload
The user has three options for upload: 
* **Update Existing StraboSpot Dataset** 
* **Create a New Strabo Dataset**
* **Use Shapefile Uploader**

To upload edits made to Native StraboSpot dataset(s) in ArcMap back to StraboSpot use: 
* **Update Existing StraboSpot Dataset**: (_Preferred Option_) replaces the dataset in Strabo with one also containing the ArcMap edits (does not nullify or delete data).Works in the same manner as the StraboSpot app by instigating versioning of the old dataset.
* **Create a New StraboSpot Dataset**: a new Strabo dataset is created in the same StraboSpot project as the original that also contains the ArcMap edits. The option for deleting any spots not in the ArcMap dataset is given. This method is useful for making backup copies of datasets or producing a dataset of select spots but the new StraboSpot dataset will not contain the original images.

The option to upload datasets not originating in StraboSpot (Native ArcGIS) is also available. There are two options for uploading Native ArcGIS datasets: 
* Transfer the Feature Class to the Feature Dataset corresponding to the downloaded StraboSpot Dataset where the data should be uploaded prior to launching the Add-In then use **Update Existing StraboSpot Dataset**. 
* Use the **Use Shapefile Uploader** option and choose dataset(s) not associated with downloaded StraboSpot data to prompt the Add-In to create a zipped Shapefile package which will be given to StraboSpot via the online Shapefile Uploader tool in the user's preferred browser.
**Please see additional documentation for further details and tips for use.**