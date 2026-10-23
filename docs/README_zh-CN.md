<div align="center">

# MINEZ DenizenMetaWebsite

面向 MINEZ 服务器维护的 [DenizenMetaWebsite](https://github.com/DenizenScript/DenizenMetaWebsite) Fork，部署于 [denizen-meta.minez.cc](https://denizen-meta.minez.cc/)。

<a href="../README.md">English</a> ｜
<a href="README_zh-CN.md">简体中文</a>

<br>

<div>
<a href="https://github.com/DenizenScript/DenizenMetaWebsite"><img src="badges/upstream.svg" alt="upstream"></a>
<a href="../LICENSE.txt"><img src="https://img.shields.io/badge/license-MIT-9e9e9e" alt="license"></a>
</div>

<br>

<a href="https://denizen-meta.minez.cc/">访问站点</a> ｜
<a href="https://github.com/MINEZ/DenizenMetaWebsite/issues">问题反馈</a> ｜
<a href="#相对上游的改动">改动一览</a>

</div>

> [!IMPORTANT]
> **本项目是非官方 Fork，与 DenizenScript 团队没有关系，既未经其认可，也不由其提供支持。**
>
> 与本 Fork 有关的一切，都不要带到上游仓库、官方 Discord 或论坛。站点本身的问题请提交至[本仓库的 issue](https://github.com/MINEZ/DenizenMetaWebsite/issues)；条目内容的问题请提交至 meta 的出处，即 [MINEZ/Denizen 的 issue](https://github.com/MINEZ/Denizen/issues)。

## 关于

本仓库跟随上游 `master` 分支。所部署的站点为 [MINEZ/Denizen](https://github.com/MINEZ/Denizen) 而非官方项目的命令、标签、事件与对象类型提供文档，因此部分条目与 [meta.denizenscript.com](https://meta.denizenscript.com/) 不同，有些甚至在那里并不存在。除下文列出的内容外，其余部分与上游完全一致。

我们的提交以变基方式跟随上游，历史保持线性：

```bash
git fetch upstream
git rebase upstream/master
```

变基会重写提交哈希，因此同步后推送需要 `git push origin master --force-with-lease`。

## 相对上游的改动

| 名称 | 类型 | 改动 |
| --- | --- | --- |
| meta 源 | 行为 | 变更 |
| 源归档缓存 | 行为 | 新增 |
| 失败的重新载入 | 行为 | 修复 |
| 首页、页脚与页面标题 | 内容 | 变更 |
| 部署脚本 | 部署 | 变更 |

**meta 源。** 站点基于 [MINEZ/SharpDenizenTools](https://github.com/MINEZ/SharpDenizenTools) 构建，载入的是 MINEZ/Denizen、MINEZ/DenizenCore 与 MINEZ/Depenizen 的 meta，外加 [CraftEngine](https://github.com/Xiao-MoMi/craft-engine) 自带的 Denizen 集成。仅存在于本 Fork 或 CraftEngine 中的条目因此都有文档，也不会有条目重复列出。

**源归档缓存。** 此前每次启动与每次经 Webhook 重新载入，都会把各个源的归档完整地重新下载一遍；网络较慢时，一次超时就足以让整个源被跳过。现将归档缓存于 `cache/sources/`，12 小时内直接复用。下载失败时改用过期的缓存，只有既下载不到、缓存中也没有时，才判定该源失败。经 Webhook 触发的重新载入会绕过缓存，以保证推送后取到的是最新内容。

**失败的重新载入。** 重新载入此前会无条件地用新结果替换已载入的文档，一次下载失败便可能毁掉原本完好的数据，而站点照常响应，外部无从察觉。现在，若有源下载失败且此前已有可用的文档，便放弃本次替换，保留原有文档。

**首页、页脚与页面标题。** 这些内容原样承袭自上游，称本站为 Denizen 的文档，并邀请访客加入官方 Discord，很容易被误认为官方站点，也容易让人把本 Fork 的问题带往官方渠道。现在首页第一屏就说明本站文档化的是哪个 Fork，并以警示框指出这不是官方文档，给出官方站点与本组织 issue 的地址；页脚亦作同样声明。上游的开发者署名与版权声明保留在页脚中，并注明所依据的许可证与本站点的部署来源。页面标题加上了“MINEZ”前缀。

**部署脚本。** `start.sh` 原先监听 `http://*:8098`，使端口直接暴露于公网；本站点由反向代理对外提供服务，故改为仅监听回环地址。`update.sh` 原先会在 `screen` 中启动站点，现在只负责拉取与编译，并会先清理残留的编译产物——子模块的提交或远程地址变动后，残留的 SourceLink 缓存会导致 `MSB3030`。两个脚本都会在出错时立即停止，并可从任意目录调用。

## 部署

为 Debian Linux 设计并在其上测试。站点应运行在 Apache2、nginx 等反向代理之后。

1. 确保已安装 `git` 与 `dotnet-8-sdk`。
2. 为该服务添加一个用户（通常经 `adduser`，然后 `su` 到该用户）。
3. 连同子模块克隆本仓库，然后进入该目录：

   ```bash
   git clone --recursive https://github.com/MINEZ/DenizenMetaWebsite
   ```

4. 新建名为 `config` 的目录，在其中新建名为 `config.fds` 的文本文件，写入下方示例中的任意配置项。所有配置项均可省略，空文件即为默认配置。
5. 运行 `./update.sh` 编译，再运行 `./start.sh` 在前台启动站点。如需常驻，可自行选用 `tmux`、systemd 服务等方式。
6. 将反向代理指向 `http://127.0.0.1:8098`。如需更换端口，请修改 `start.sh`。

更新时，先停掉站点，运行 `./update.sh`，再以 `./start.sh` 重新启动。

### 在 Windows 上测试

- 安装 [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)。
- 按上文新建 `config/config.fds`。
- 在 VS Code 中打开本目录，在其终端中运行 `.\start.ps1`。
- 在浏览器中打开 `localhost:8098`。

### 配置

```yml
# 设为一个 Webhook 令牌后，可让站点从源重新载入 meta 文档。
# 用法是向 "<站点地址>/Webhook/Reload?token=some_token_here" 发送 POST 请求。
# 省略此项即不启用重新载入。
reload-webhook-token: some_token_here
# 设为一组替代的 meta 文档源地址。省略此项即使用默认的源列表。
alt-sources:
- https://example.com
# 设为导航栏与首页显示的站点名称，默认为“Denizen Meta Documentation”。
header-line: Denizen Meta Documentation
```

经 Webhook 重新载入时，总会绕过上文所述的缓存，重新下载全部源。

## 相关仓库

| 仓库 | 用途 |
| --- | --- |
| [MINEZ/Denizen](https://github.com/MINEZ/Denizen) | 脚本引擎，即本站点所文档化的对象 |
| [MINEZ/SharpDenizenTools](https://github.com/MINEZ/SharpDenizenTools) | 站点所基于的 meta 解析与脚本检查库 |
| [MINEZ/DenizenVSCode](https://github.com/MINEZ/DenizenVSCode) | VS Code 扩展，其悬停链接指向本站点 |

## 上游

DenizenMetaWebsite 由 Alex "mcmonkey" Goodwin 代表 [DenizenScript 团队](https://denizenscript.com/)开发，官方部署位于 [meta.denizenscript.com](https://meta.denizenscript.com/)。此处的改动为单一服务器的需求而作，独立维护。请勿将相关讨论带入上游的各个渠道。

## 许可证

DenizenMetaWebsite 以 MIT 许可证开源，版权归 The Denizen Script Team 所有。本 Fork 的修改同样适用该许可证。完整条款见 [LICENSE.txt](../LICENSE.txt)。

简而言之：你几乎可以做任何事，但不得就你使用本软件所造成的后果追究任何开发者的责任。
