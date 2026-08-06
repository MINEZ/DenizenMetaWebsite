#!/bin/bash
# 拉取更新并编译。启动请另行运行 ./start.sh。
set -e
cd "$(dirname "$0")"

git pull
git submodule update --init --recursive

# 清理编译中间产物：子模块提交或远程地址变动后，
# 残留的 SourceLink 缓存会导致 MSB3030。
rm -rf obj bin/live_release SharpDenizenTools/SharpDenizenTools/obj

dotnet build --configuration Release -o ./bin/live_release

echo
echo "编译完成。停掉正在运行的站点后，运行 ./start.sh 启动。"
