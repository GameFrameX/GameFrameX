dotnet ./Tools/ProtoExport.dll  --mode cpp --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" --inputPath ./ --outputPath ./Unreal/Source/Proto --namespaceName GameFrameX.Proto --isGenerateErrorCode true

pause
