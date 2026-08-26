#!/usr/bin/env bash
set -euo pipefail
# ponytail: rsync --delete 快照式同步，子仓历史不进主仓；聚合仓内的改动会被下次同步覆盖，贡献走源仓

# pin 行尾语义：repo 字节 = 源仓 blob 字节（防本机 autocrlf=input 与 CI false 行为分歧导致行尾来回翻转）
git config core.autocrlf false

declare -a SYNC=(
  "Unity:GameFrameX/GameFrameX.Unity"
  "Server:GameFrameX/GameFrameX.Server"
  "LayaBox:GameFrameX/GameFrameX.LayaBox"
  "Tools:GameFrameX/GameFrameX.Tools"
  "Config:GameFrameX/GameFrameX.Config"
  "Protobuf:GameFrameX/GameFrameX.Protobuf"
  "FairyGUIProject:GameFrameX/GameFrameX.FairyGUIProject"
)

STAGE=$(mktemp -d)
for entry in "${SYNC[@]}"; do
  dir=${entry%%:*}
  repo=${entry#*:}
  git clone --depth 1 -q "https://github.com/${repo}.git" "$STAGE/$dir"
  # .gitattributes 不进聚合仓：子仓的 LFS/eol 声明会让二进制存成 pointer、行尾被改写，破坏「下载即完整」
  rsync -a --delete --delete-excluded --exclude='.git/' --exclude='.DS_Store' --exclude='.gitattributes' "$STAGE/$dir/" "$dir/"
done
rm -rf "$STAGE"

git add -A

# 体积红线：单文件 >50MB 拒绝继续
BIG=$(git ls-files -z | xargs -0 -I{} du -m "{}" 2>/dev/null | awk '$1 > 50')
if [ -n "$BIG" ]; then
  echo "::error::存在 >50MB 文件，中止（评估 LFS）：
$BIG"
  exit 1
fi

if git diff --cached --quiet; then
  echo "无变化，跳过提交"
else
  git commit -m "sync(bot): aggregate upstream @ $(date +%F)"
fi
