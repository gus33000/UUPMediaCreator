@echo off

msbuild /m /t:restore,cli\uupmediaconverter:publish,cli\uupdownload:publish,cli\uupmediaconverterdismbroker:publish /p:Platform=x64 /p:RuntimeIdentifier=linux-x64 /p:PublishDir=%CD%\publish\artifacts\linux-x64\CLI /p:PublishSingleFile=true /p:PublishTrimmed=false /p:Configuration=Release UUPMediaCreator.sln