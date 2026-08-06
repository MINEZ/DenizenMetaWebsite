#!/bin/bash
# 拉取更新并在名为 metasite 的 tmux 会话中重启站点。
set -e
cd "$(dirname "$0")"

git pull
git submodule update --init --recursive

# 清理编译中间产物：子模块提交或远程地址变动后，
# 残留的 SourceLink 缓存会导致 MSB3030。
rm -rf obj bin/live_release SharpDenizenTools/SharpDenizenTools/obj

tmux kill-session -t metasite 2>/dev/null || true
tmux new-session -d -s metasite "$(pwd)/start.sh"

echo "已在 tmux 会话 metasite 中启动。"
echo "查看日志：tmux attach -t metasite   （脱离：Ctrl+B 然后 D）"
