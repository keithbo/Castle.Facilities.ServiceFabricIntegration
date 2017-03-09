@echo off
SET list=Castle.Facilities.ServiceFabricIntegration,Castle.Facilities.ServiceFabricIntegration.Actors
SET list=%list:@=,%
SET Configuration=%1

FOR %%a IN (%list%) DO (
	msbuild "%%a\%%a.csproj" /p:Configuration="%Configuration%"
	msbuild "%%a\%%a.csproj" /p:Platform="x64" /p:Configuration="%Configuration%"
	nuget pack "%%a\%%a.csproj" -Symbols
)

