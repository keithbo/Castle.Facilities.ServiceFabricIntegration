@echo off

msbuild Castle.Facilities.ServiceFabricIntegration.sln /p:Configuration=Debug
msbuild Castle.Facilities.ServiceFabricIntegration.sln /p:Configuration=Debug /p:Platform=x64
msbuild Castle.Facilities.ServiceFabricIntegration.sln /p:Configuration=Release
msbuild Castle.Facilities.ServiceFabricIntegration.sln /p:Configuration=Release /p:Platform=x64