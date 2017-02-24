# ArcMap Add-In Linking to StraboSpot Database 
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
The StraboSpot database is a Neo-4j graph database allowing for flexible user input and interacts with the StraboMobile application for phones and tablets for data collection. This Add-In translates the graph structure to a table format (since ArcGIS uses a relational database structure) and provides a user-friendly, quick set-up of the Strabo datasets in ArcGIS. 
A StraboSpot Project is equivalent to an ArcGIS File Geodatabase and a StraboSpot Dataset (divided by geometries) is equivalent to an ArcGIS Feature Class. 

#### Directions for Use
A StraboSpot account (username and password) is required for use of this Add-In. Visit https://strabospot.org for more information about the project and to set up an account.

**Downloading Strabo Datasets in ArcMap**
Strabo projects/datasets are created by inputting data into the StraboMobile app. The projects/datasets must then be synced to the online server (via the app) to use this Add-In. To download one (or many) synced Strabo dataset(s) from a project for use in ArcMap: choose a project-->dataset to download then choose which folder to create the File GDB and store associated files. The Add-In will create a File GDB with the name of the Strabo dataset plus the date (Ex: "*datasetName_mm_dd_yyyy*.gdb"). If the same dataset is downloaded multiple times per day, an extra number will be inserted (Ex: "*datasetName_mm_dd_yyyy*_1.gdb"). 

A separate file folder with the same name as the .gdb file will be created. This folder will contain any images in the Strabo dataset (as .tiff files) and the .json files needed to make Strabo to ArcGIS conversions (and vice versa if the dataset is uploaded back to Strabo from ArcMap). Therefore, it is imperative that the file folder remain on the user's computer and is not moved if uploading the edited dataset from ArcMap to StraboSpot is desired. 

A single Strabo dataset can contain data of multiple geometries (points, lines and polygons). However, an ArcGIS Feature Class is defined by only one geometry. Therefore, a Strabo dataset will be separated out into up to three different Feature Classes (named by geometry) in the File GDB.

**Uploading from ArcMap to StraboSpot**
In order to use the upload portion of the Add-In, at least one Feature Class must be in the ArcMap Table of Contents. 
There are two types of datasets which can be uploaded to StraboSpot using this Add-In. One is a "native ArcGIS" dataset (i.e. one which has never existed in the Strabo system). Native ArcGIS datasets are uploaded by accessing a shapefile uploading tool on the Strabo website. This can be done manually by the user by visiting: https://strabospot.org/load_shapefile. The advantage the Add-In gives users is it converts a Feature Class to the proper spatial reference for Strabo (GCS WGS 1984) and packages all datasets the user chooses together in a .zip file (this can include datasets from several sources) that is passed to the shapefile upload tool. 

Native Strabo datasets, those downloaded from the server using this Add-In, can also be uploaded back to the server using this Add-In. This allows users to edit field values, add new fields which do not exist in Strabo, or edit geometry (e.g. run Topology) then upload for further Strabo use. The user may upload as many of these datasets as desired, meaning, if a Strabo dataset consisted of points, lines, and polygons (downloaded as separate Feature Classes) the user could upload the entire dataset back in one upload. 

*Editing Tips and Tricks Coming Soon*