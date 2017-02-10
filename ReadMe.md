# ArcMap Add-In Linking to StraboSpot Database 
This Add-In links the StraboSpot Database with ESRI's ArcMap 

#### Directions for Download
Please note that this Add-In is **NOT** yet packaged for download.

#### Specifications
This Add-In was designed and programmed in VB.NET using Microsoft Visual Studio Community 2013 using .NET Framework 3.5 and ArcObjects libraries. Ionic.Zip, Ionic.Zlib (https://dotnetzip.codeplex.com/), and Json.NET (http://www.newtonsoft.com/json) are also utilized. 

#### Rationale and Structure
The StraboSpot database is Neo-4j graph database allowing for flexible user input and interacts with the StraboMobile application for phones and tablets for data collection. Since ArcGIS uses a relational database structure a conversion of the graph structure to a table format as well as user-friendly, quick set-up of the Strabo datasets in ArcCatalog. 
A StraboSpot Project is equivalent to an ArcGIS File Geodatabase and a StraboSpot Dataset is equivalent to an ArcGIS Feature Class. 

#### Directions for Use
A StraboSpot account (username and password) is required for use of this Add-In. Visit https://strabospot.org for more information about the project and to set up an account.

**Downloading Strabo Datasets in ArcMap**
Strabo datasets are created by inputting data into StraboMobile and then can be synced to the online server. To download one or many Strabo datasets from a project in ArcMap: choose a project-->dataset to download then choose which folder to create the File GDB and store associated files. The download will create a File GDB with the name of the Strabo dataset plus the date (Ex: "*datasetName*_01_01_2017.gdb"). If the dataset is downloaded multiple times per day, an extra number will be inserted (Ex: "*datasetName*_01_01_2017_1.gdb"). 

Another file folder of the same name as the .gdb file will be created as well. This folder houses any images in the Strabo dataset (downloaded as .tiff files) as well as the .json files needed to make Strabo to ESRI conversions (and vice versa if the dataset is uploaded back to Strabo from ArcMap). Therefore, it is imperative that the file folder remain on the user's computer and is not moved if uploading the edited dataset from ArcMap to StraboSpot is desired. 

Keep in mind that a single Strabo dataset can contain data of multiple geometries (points, lines and polygons). However, an ArcGIS Feature Class is defined by only one geometry. Therefore, a Strabo dataset will be separated out into up to three different Feature Classes (named by geometry) into the File GDB.

**Uploading from ArcMap to StraboSpot**
In order to use the upload portion of the Add-In, at least one Feature Class must be in the ArcMap Table of Contents. 
There are two types of datasets which can be uploaded to StraboSpot using this Add-In. One is a "native ArcGIS" dataset (i.e. one which has never existed in the Strabo system). Native ArcGIS datasets are uploaded by accessing a shapefile uploading tool on the Strabo website. This can be done manually by the user by visiting: https://strabospot.org/load_shapefile. The advantage the Add-In gives users is it will convert a Feature Class to the proper spatial reference (GCS WGS 1984) for Strabo and package all datasets the user directs it to together in a .zip file (this can include datasets from several sources) which is then passed to the shapefile upload tool. 

The other type of dataset which can be uploaded using this Add-In is one which originated in the Strabo system and was downloaded for use in ArcMap. This allows users to edit field values, add new fields which do not exist in Strabo, and fix geometry (e.g. run Topology) then upload for use in StraboMobile. The user may upload as many of these datasets as desire, meaning, if a Strabo dataset consisted of points, lines, and polygons (downloaded as separate Feature Classes) the user could upload the entire dataset back in one upload. 

*Editing Tips and Tricks Coming Soon*