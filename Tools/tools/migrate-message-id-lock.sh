#!/usr/bin/env bash
# 一次性迁移脚本：把当前 proto 文件解析出的 (Name → Opcode) 序列化为 proto-message-ids.lock.json，
# 作为后续稳定分配的种子。仅在迁移时刻调用一次。
#
# 重要语义：
# - 旧版本（无 lock）Opcode 来自「行序自增」—— 历史上任何一次插入/重排都已造成协议漂移，
#   本脚本不会回滚那些漂移，只是「冻结当前这一刻」让后续稳定。
# - 调用前必须先以旧方式正常导出过一次，确保所有 proto 已被解析、Opcode 已写入。
# - 调用后请将 proto-message-ids.lock.json 提交进 git；CI/本地导出都应传 --messageIdLockPath。

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
PROTO_EXPORT_DIR="$ROOT_DIR/ProtoExport"
LOCK_FILE="$ROOT_DIR/proto-message-ids.lock.json"

# 默认扫描 TestProtos/ 目录；调用方可通过环境变量覆盖
INPUT_PATH="${INPUT_PATH:-$ROOT_DIR/TestProtos}"

if [[ ! -d "$INPUT_PATH" ]]; then
  echo "[ERR] 协议目录不存在：$INPUT_PATH" >&2
  exit 1
fi

if [[ ! -d "$PROTO_EXPORT_DIR" ]]; then
  echo "[ERR] ProtoExport 目录不存在：$PROTO_EXPORT_DIR" >&2
  exit 1
fi

echo "[Migrate] 协议根目录：$INPUT_PATH"
echo "[Migrate] lock 输出：$LOCK_FILE"

# 优先用现成的二进制（避免每次都 dotnet build）
DLL_PATH="$ROOT_DIR/../../Protobuf/Tools/ProtoExport.dll"
if [[ ! -f "$DLL_PATH" ]]; then
  echo "[Migrate] 未找到已编译产物，回退 dotnet run"
  (cd "$PROTO_EXPORT_DIR" && dotnet run --no-build --project "$PROTO_EXPORT_DIR/ProtoExport.csproj" -- \
    --inputPath "$INPUT_PATH" \
    --mode CSharp \
    --outputPath "$ROOT_DIR/obj/migrate" \
    --isServer true \
    --messageIdLockPath "$LOCK_FILE" \
    --regenerate-lock)
else
  dotnet "$DLL_PATH" \
    --inputPath "$INPUT_PATH" \
    --mode CSharp \
    --outputPath "$ROOT_DIR/obj/migrate" \
    --isServer true \
    --messageIdLockPath "$LOCK_FILE" \
    --regenerate-lock
fi

echo "[Migrate] 完成。请检查 $LOCK_FILE 后提交进 git。"