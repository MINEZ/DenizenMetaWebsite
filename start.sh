#!/bin/bash
# 编译并在当前控制台前台运行站点。仅监听回环地址，外部访问经由反向代理。
set -e
cd "$(dirname "$0")"
dotnet build --configuration Release -o ./bin/live_release
ASPNETCORE_ENVIRONMENT=Production ASPNETCORE_URLS=http://127.0.0.1:8098 dotnet ./bin/live_release/DenizenMetaWebsite.dll
