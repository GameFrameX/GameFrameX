dotnet ./Tools/Luban.dll --target client --dataTarget bin --codeTarget cs-bin --xargs outputDataDir=../Unity/Assets//Bundles/Config  --xargs outputCodeDir=../Unity/Assets//Hotfix/Config/Generate  -x l10n.provider=default -x l10n.textFile.keyFieldName=key  -x l10n.textFile.path=./Excels/Localization.xlsx --conf ./Luban.conf

pause