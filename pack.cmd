@echo=off
SET list=Castle.Facilities.ServiceFabricIntegration,Castle.Facilities.ServiceFabricIntegration.Actors
SET list=%list:@=,%
SET DPROPS=Configuration=Debug
SET RPROPS=Configuration=Release

FOR %%a IN (%list%) DO (
	nuget pack "%%a\%%a.csproj" -Build -Prop %DPROPS%
	nuget pack "%%a\%%a.csproj" -Build -Prop %RPROPS%
)

