cd /d ../Tools/ProtoExport\bin\Debug\net8.0
call dotnet ProtoExport.dll --mode cpp --usingStatements "#include <cstdint>|#include <string>|#include <vector>|#include <unordered_map>" --inputPath ./../../../../../Protobuf --outputPath ./../../../../../Unreal/Source/Proto --namespaceName GameFrameX.Proto --isGenerateErrorCode true

pause
