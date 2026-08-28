#!/usr/bin/env bash
# 每日采集 8 仓 GitHub 流量快照，追加 JSONL（按月分文件）。
# 用法：GH_TOKEN=<token> bash scripts/metrics-collect.sh [输出目录]
# Traffic API 只保留 14 天，本脚本必须每日跑一次。
set -euo pipefail

OUT_DIR="${1:-metrics}"
REPOS=(
  GameFrameX/GameFrameX
  GameFrameX/GameFrameX.Server
  GameFrameX/GameFrameX.Unity
  GameFrameX/GameFrameX.Foundation
  GameFrameX/GameFrameX.Config
  GameFrameX/GameFrameX.Protobuf
  GameFrameX/GameFrameX.Tools
  GameFrameX/GameFrameX.FairyGUIProject
)

mkdir -p "$OUT_DIR"
OUT_FILE="$OUT_DIR/metrics-$(date +%Y-%m).jsonl"
DATE=$(date +%Y-%m-%d)

for repo in "${REPOS[@]}"; do
  meta=$(gh api "repos/$repo" --jq '{stars: .stargazers_count, forks: .forks_count, open_issues: .open_issues_count}')
  views=$(gh api "repos/$repo/traffic/views" --jq '{count: .count, uniques: .uniques}')
  clones=$(gh api "repos/$repo/traffic/clones" --jq '{count: .count, uniques: .uniques}')
  referrers=$(gh api "repos/$repo/traffic/popular/referrers" --jq '[.[] | {referrer: .referrer, count: .count, uniques: .uniques}]')
  echo "{\"date\":\"$DATE\",\"repo\":\"$repo\",\"stats\":$meta,\"views_14d\":$views,\"clones_14d\":$clones,\"top_referrers\":$referrers}" >> "$OUT_FILE"
done

echo "collected ${#REPOS[@]} repos -> $OUT_FILE"
