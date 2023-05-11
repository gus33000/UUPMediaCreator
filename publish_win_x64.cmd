@echo off

msbuild /m /t:restore,cli\uupmediaconverter:publish,cli\uupdownload:publish /p:Platform=x64 /p:RuntimeIdentifier=win-x64 /p:PublishDir=%CD%\publish\artifacts\win-x64\CLI /p:PublishSingleFile=true /p:PublishTrimmed=false /p:Configuration=Release UUPMediaCreator.sln