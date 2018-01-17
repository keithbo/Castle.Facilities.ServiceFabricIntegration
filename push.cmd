@echo off
SET list=Castle.Facilities.ServiceFabricIntegration,Castle.Facilities.ServiceFabricIntegration.Actors
SET list=%list:@=,%
SET Version=%1

FOR %%a IN (%list%) DO (
	nuget push "%%a.%Version%.nupkg" -Source NuGet
)
