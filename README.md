# GEDs

Web application to generate GEDs personal files: http://sage-geds.tpsgc-pwgsc.gc.ca/ 

#### Requirements

> .NET 4.0

Data source is implemented in ActiveDirectoryConnector to change it to a different source type just implement your data reader against Plugin.IDataReader and set the connector string through the website.

All settings are set through the website first. To preset values for settings fill in defaults in  Data\Migrations\Configuration.cs
