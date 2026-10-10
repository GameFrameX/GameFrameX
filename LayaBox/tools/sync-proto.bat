@echo off
rem 全量同步 ../Protobuf 业务协议到 assets/resources/protobuf(protobufjs 运行时加载)。
rem 规则:仅同步 _<module>_<Name>.proto 业务协议;Inner 内部协议(_- 前缀)不下发客户端。
rem 目标目录先清空旧 .proto,保证与 Proto2TsExport_LayaBox 生成物命名一致。

setlocal enabledelayedexpansion
set "scriptDir=%~dp0"
set "sourceDir=%scriptDir%..\..\Protobuf"
set "destDir=%scriptDir%..\assets\resources\protobuf"

if not exist "%destDir%" mkdir "%destDir%"

del /q "%destDir%\*.proto" >nul 2>&1

set "count=0"
for %%f in ("%sourceDir%\_*.proto") do (
    set "name=%%~nxf"
    if not "!name:~1,1!"=="-" (
        copy /Y "%%~ff" "%destDir%\!name!" >nul
        set /a count+=1
        echo sync: !name!
    )
)

if !count! LSS 9 (
    echo [WARN] expect at least 9 proto files, got !count!. check %sourceDir%
    exit /b 1
)

echo done: !count! proto files -^> %destDir%
pause
