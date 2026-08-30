dotnet.exe ./Tools/Luban.dll --target client --dataTarget json --codeTarget cs-simple-json --xargs outputDataDir=../Godot/Assets/Bundles/Config  --xargs outputCodeDir=../Godot/Assets/Hotfix/Config/Generate --xargs tableImporter.name=gameframex -x l10n.provider=gameframex -x l10n.textFile.keyFieldName=key -x l10n.textFile.path=./Excels/Local/ --conf ./Luban.conf

pause
