@echo off

msbuild /m /t:restore,cli\uupmediaconverter:publish,cli\uupdownload:publish /p:Platform=arm64 /p:RuntimeIdentifier=linux-arm64 /p:PublishDir=%CD%\publish\artifacts\linux-arm64\CLI /p:PublishSingleFile=true /p:PublishTrimmed=false /p:Configuration=Release UUPMediaCreator.sln