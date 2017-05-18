# ArcMap 10.3 Add-In Linking to StraboSpot Database 
This Add-In links the StraboSpot Database with ESRI's ArcMap 

#### Directions for Download
Please note that this Add-In is **NOT** yet packaged for download.

#### Specifications
This Add-In was designed and programmed in the VB.NET language using Microsoft Visual Studio Community 2013 using .NET Framework 3.5 and ArcObjects libraries. Ionic.Zip, Ionic.Zlib (https://dotnetzip.codeplex.com/), and Json.NET (http://www.newtonsoft.com/json) are also utilized. 

**ArcGIS Requirements**
ArcGIS for Desktop version 10.3
The following ArcToolbox extensions must be initalized:
* Conversion Tools
* Data Management Tools

#### Rationale and Structure
The StraboSpot database is a Neo-4j graph database allowing for flexible user input and interacts with the StraboMobile application for phones and tablets for data collection. This Add-In translates the graph structure to a relational structure and provides a user-friendly, quick set-up of the Strabo datasets in ArcMap. 
A StraboSpot Project is loosely equivalent to an ArcGIS File Geodatabase and a StraboSpot Dataset (divided by geometries: point, line, polygon) is equivalent to up to three ArcGIS Feature Classes based on geometry. 

#### Directions for Use
A StraboSpot account (username and password) is required for use of the Strabo online database, and this Add-In. Visit https://strabospot.org for more information about the project and to set up an account.

**Downloading Strabo Datasets in ArcMap**
Strabo projects/datasets are created by inputting data into the StraboMobile app. The projects/datasets must then be synced to the online server (via the app). Once in the online database, the user can download and upload their data using this Add-In. To download one (or many) synced Strabo dataset(s) from a project for use in ArcMap: choose a project-->dataset to download then choose which folder to create the File GDB and store associated files. The Add-In will create a File GDB with the name of the Strabo dataset plus the date (Ex: "*datasetName_mm_dd_yyyy*.gdb"). If the same dataset is downloaded multiple times per day, an extra number will be inserted (Ex: "*datasetName_mm_dd_yyyy*_1.gdb"). 

A separate file folder with the same name as the .gdb file will be created in the same directory the user chooses. This folder will contain any images in the Strabo dataset (as .tiff or .jpeg files as specified by the user) and the .json files needed to make Strabo to ArcGIS conversions (and vice versa if the dataset is uploaded back to Strabo from ArcMap). Thus, it is imperative that the file folder remain on the user's computer and is not moved/renamed if uploading the edited dataset from ArcMap to StraboSpot is desired. 

A single Strabo dataset can contain data of multiple geometries (points, lines and polygons). However, an ArcGIS Feature Class is defined by only one geometry. Therefore, a Strabo dataset will be separated out into up to three different Feature Classes (named by geometry) in the File GDB.

**Uploading from ArcMap to StraboSpot**
In order to use the upload portion of the Add-In, at least one Feature Class must be in the ArcMap Table of Contents (without this, a warning window will open).
There are two types of datasets which can be uploaded to StraboSpot using this Add-In. One is a "native ArcGIS" dataset (i.e. one which has never existed in the Strabo system). Native ArcGIS datasets are uploaded by accessing a shapefile uploading tool on the Strabo website. This can be done manually by the user by visiting: https://strabospot.org/load_shapefile. The advantage the Add-In gives users is it converts a Feature Class to the proper spatial reference for Strabo (GCS WGS 1984) and packages all datasets the user chooses together in a .zip file (this can include datasets from several sources) that is passed to the shapefile upload tool. Some interaction with the shapefile upload tool in the user's default browswer is still required. 

Native Strabo datasets, those downloaded from the server using this Add-In, can also be uploaded back to the server using this Add-In. This allows users to edit field values, add new fields which do not exist in Strabo, or edit geometry (e.g. run Topology) then upload for further Strabo use. The user may upload as many of these datasets as desired, meaning, if a Strabo dataset consisted of points, lines, and polygons (downloaded as separate Feature Classes) the user could upload the entire dataset back in one upload. 
The user has two options for uploading their native Strabo datasets: 
1.) _Overwriting the Dataset in Strabo:_ parses the information in the Feature Class and replaces/adds (does not nullify existing values or delete spots) spot information. This is the *preferred option* for uploading data back to Strabo since it works in a similar manner to the app and creates an old version of the dataset which can be accessed by the user on the Strabo site (Account -> Versioning when logged-in). 
2.) *Create a New Strabo Dataset:* creates a new dataset in the same Strabo project the dataset originated (*a user cannot upload to a different Strabo project using the Add-In-- see Tags discussion below*). All user input data is preserved in this method, but it is not preferred since timestamps (not dates) and spot ID's are updated to the time of upload (this will disconnect any Tags). This method is good for creating a backup copy of data. 

**Tags**
Tags are Strabo's method of assigning conceptual grouping (as opposed to geographic grouping with nests). The following categories are available to users in the app: geologic unit, concept, documentation, other. Once a tag is created in the app, its information can be assigned to many different spots in the user's Strabo project (so datasets can share the same Tags).
A consequence of Tags information being stored at the project level for the Add-In is that if a dataset has spots with Tags, the Tag information will be downloaded as a separate Feature Class in the file geodatabase. Therefore, if a dataset has points, lines, and polygons all with Tag information, six Feature Classes will be created. Tags information inherits geometry by matching SpotIDs. This is especially important since geologic unit information needs to be mapable in ArcMap. 
It is possible for the user to upload changes made to Tags during an ArcMap edit session, but the functionality is limited. Simple text changes (e.g. to a Tag's name or notes) can be made through upload to Strabo (the "*geometry*_Tags" Feature Class must be chosen in the list of datasets for upload). However, if new assignment of Tags is made in ArcMap, those changes will not be reflected upon upload. 

**ArcMap Editing Tips and Tricks**
The following are pointers for how to best edit Strabo ArcMap feature classes after download in order to preserve the option of uploading back to Strabo. 
1.) Keep in mind, that the user must upload datasets to the same Strabo account the dataset was downloaded from. Do not try downloading a dataset to ArcMap in order to copy it to a different account. 
2.) It is important not to change/rename/move the file folder of .json files and images (images can be copied out of that folder if needed) that is created upon download. Important ID information is accessed by the upload code. 
3.) When spots located on image basemaps are downloaded, they inherit the real world coordinates of the next spot up so the information contained within the spot is mapable in ArcMap. Keep in mind that if the geometry is changed for these spots, those changes will not be reflected upon upload in Strabo as Strabo supports pixel coordinates. 
4.) DO NOT CHANGE ID INFORMATION IN THE ATTRIBUTE TABLE. SpotID and FeatID need to remain unchanged. If the user desires to do so, those fields can be locked in ArcMap by following these instructions: open the attribute table -> left click the column to select -> right click and choose "properties" -> in the "field properties" dialog box check "Make field read only" and click "OK." 
5.) Data can only be added to a dataset. Nullified values which were not null upon download will remain not null upon upload. The user is welcome to nullify for the purposes of an ArcMap edited session, but keep in mind that change will not persist in Strabo. 
6.) Strabo geometries once downloaded to ArcMap, often get additional decimal places added (example: -105.30319381599, 38.561504689151 gets changed to   -105.30319381598986, 38.561504689150979) and this change is persisted to the upload. 