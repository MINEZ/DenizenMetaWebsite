<div align="center">

# MINEZ DenizenMetaWebsite

A fork of [DenizenMetaWebsite](https://github.com/DenizenScript/DenizenMetaWebsite) maintained for the MINEZ server, deployed at [denizen-meta.minez.cc](https://denizen-meta.minez.cc/).

<a href="README.md">English</a> ｜
<a href="docs/README_zh-CN.md">简体中文</a>

<br>

<div>
<a href="https://github.com/DenizenScript/DenizenMetaWebsite"><img src="docs/badges/upstream.svg" alt="upstream"></a>
<a href="LICENSE.txt"><img src="https://img.shields.io/badge/license-MIT-9e9e9e" alt="license"></a>
</div>

<br>

<a href="https://denizen-meta.minez.cc/">Visit the Site</a> ｜
<a href="https://github.com/MINEZ/DenizenMetaWebsite/issues">Issue Tracker</a> ｜
<a href="#changes-from-upstream">What's Changed</a>

</div>

> [!IMPORTANT]
> **This is an unofficial fork and is not affiliated with, endorsed by, or supported by the DenizenScript team.**
>
> Never take anything about this fork to the upstream repository, the official Discord, or the forums. Problems with the site itself belong in [this repository's issue tracker](https://github.com/MINEZ/DenizenMetaWebsite/issues); problems with what an entry says belong in [MINEZ/Denizen's](https://github.com/MINEZ/Denizen/issues), where the meta comes from.

## About

This repository tracks the upstream `master` branch. The site it serves documents the commands, tags, events and object types of [MINEZ/Denizen](https://github.com/MINEZ/Denizen) rather than the official project, so some entries differ from [meta.denizenscript.com](https://meta.denizenscript.com/) and some do not exist there at all. Everything not listed below is identical to upstream.

Our commits follow upstream by rebase, so the history stays linear:

```bash
git fetch upstream
git rebase upstream/master
```

Because rebasing rewrites commit hashes, pushing after a sync requires `git push origin master --force-with-lease`.

## Changes from Upstream

| Name | Type | Change |
| --- | --- | --- |
| Meta sources | Behaviour | Changed |
| Source archive cache | Behaviour | Added |
| Failed reloads | Behaviour | Fixed |
| Homepage, footer and page titles | Content | Changed |
| Deployment scripts | Deployment | Changed |

**Meta sources.** The site is built on [MINEZ/SharpDenizenTools](https://github.com/MINEZ/SharpDenizenTools), which loads the meta of MINEZ/Denizen, MINEZ/DenizenCore and MINEZ/Depenizen, plus the Denizen integration that [CraftEngine](https://github.com/Xiao-MoMi/craft-engine) ships with. Entries that exist only in this fork or in CraftEngine are therefore documented, and nothing is listed twice.

**Source archive cache.** Every start and every webhook reload used to download each source archive again in full. On a slow connection a single timeout was enough for a whole source to be skipped. Archives are now cached in `cache/sources/` and reused for 12 hours. When a download fails, an expired copy is used instead, and a source only counts as failed when it can neither be downloaded nor found in the cache. Reloads triggered by the webhook bypass the cache, so a push is always picked up.

**Failed reloads.** A reload used to replace the loaded docs with whatever it got, so one failed download could wipe out a perfectly good set of docs while the site carried on serving as if nothing had happened. When a source fails and docs are already loaded, the reload is now abandoned and the previous docs are kept.

**Homepage, footer and page titles.** As inherited from upstream, the site described itself as the Denizen documentation and invited visitors to the official Discord, which made it easy to take for the official site and to carry problems specific to this fork into official channels. The homepage now says at first glance which fork it documents and, in a warning box, that it is not the official documentation, pointing to the official site and to our issue tracker. The footer says the same. Upstream's credits and copyright notice are kept in the footer, along with the license they are used under and where this deployment comes from. Page titles carry a "MINEZ" prefix.

**Deployment scripts.** `start.sh` used to listen on `http://*:8098`, exposing the port to the internet; the site sits behind a reverse proxy, so it now listens on the loopback address only. `update.sh` used to start the site inside `screen`; it now only pulls and builds, and clears stale build output first, since leftover SourceLink caches cause `MSB3030` after a submodule's commit or remote changes. Both scripts stop on the first error and can be run from any directory.

## Deployment

Designed for and tested on Debian Linux. The site is meant to run behind a reverse proxy such as Apache2 or nginx.

1. Make sure `git` and `dotnet-8-sdk` are available.
2. Add a user for the service (via `adduser` generally, then `su` to that user).
3. Clone the repository with its submodule, then enter the folder:

   ```bash
   git clone --recursive https://github.com/MINEZ/DenizenMetaWebsite
   ```

4. Make a folder named `config`, and inside it a text file named `config.fds` holding any of the keys from the sample below. All keys are optional; an empty file gives the default configuration.
5. Run `./update.sh` to build, then `./start.sh` to run the site in the foreground. Keep it running with whatever you prefer, such as `tmux` or a systemd service.
6. Point the reverse proxy at `http://127.0.0.1:8098`. To use another port, edit `start.sh`.

To update, stop the site, run `./update.sh`, then start it again with `./start.sh`.

### Testing on Windows

- Install [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0).
- Create `config/config.fds` as above.
- Open this folder in VS Code, and run `.\start.ps1` in its terminal.
- Open `localhost:8098` in your browser.

### Configuration

```yml
# Set to a webhook token that will cause the website to reload meta documentation from source.
# This can be used by sending a POST request to "<base url>/Webhook/Reload?token=some_token_here"
# Exclude this config key to not have any reloading.
reload-webhook-token: some_token_here
# Set to a list of alternate meta doc source URLs. Exclude this key to use the default source list.
alt-sources:
- https://example.com
# Set to the site name shown in the navigation bar and on the homepage. Defaults to "Denizen Meta Documentation".
header-line: Denizen Meta Documentation
```

A webhook reload always downloads every source again, bypassing the cache described above.

## Related Repositories

| Repository | Purpose |
| --- | --- |
| [MINEZ/Denizen](https://github.com/MINEZ/Denizen) | Scripting engine documented by this site |
| [MINEZ/SharpDenizenTools](https://github.com/MINEZ/SharpDenizenTools) | Meta and script-checking library the site is built on |
| [MINEZ/DenizenVSCode](https://github.com/MINEZ/DenizenVSCode) | VS Code extension, whose hover links point at this site |

## Upstream

DenizenMetaWebsite is developed by Alex "mcmonkey" Goodwin on behalf of [the DenizenScript team](https://denizenscript.com/), and the official deployment lives at [meta.denizenscript.com](https://meta.denizenscript.com/). The changes here are built for one server's needs and are maintained separately. Please keep discussion of them out of upstream's channels.

## License

DenizenMetaWebsite is open source under the MIT License, Copyright (c) The Denizen Script Team. Modifications in this fork are provided under the same terms. See [LICENSE.txt](LICENSE.txt) for the full text.

The short of it: you can do basically whatever you want, except hold any developer liable for what you do with the software.
