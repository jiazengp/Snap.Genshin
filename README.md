<div align="center"> 

[![Snap.Genshin](https://socialify.git.ci/DGP-Studio/Snap.Genshin/image?description=1&font=Inter&forks=1&logo=https%3A%2F%2Fgithub.com%2FDGP-Studio%2FSnap.Genshin%2Fblob%2Fmain%2FDesign%2FSGLogo.png%3Fraw%3Dtrue&pattern=Signal&stargazers=1&theme=Dark)](https://github.com/DGP-Studio/Snap.Genshin/stargazers)

![lines](https://img.shields.io/tokei/lines/github/DGP-Studio/Snap.Genshin?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/DGP-Studio/Snap.Genshin?style=flat-square)
![GitHub issues](https://img.shields.io/github/issues/DGP-Studio/Snap.Genshin?style=flat-square)
![GitHub closed issues](https://img.shields.io/github/issues-closed/DGP-Studio/Snap.Genshin?style=flat-square)

[![qq](https://img.shields.io/badge/chat-on_qq-green?style=flat-square)](https://jq.qq.com/?_wv=1027&k=tdKQiXyl)

### 前往下载

[![GitHub downloads](https://img.shields.io/github/downloads/DGP-Studio/Snap.Genshin/total?style=flat-square)](https://github.com/DGP-Studio/Snap.Genshin/releases)
[![GitHub release (latest by date)](https://img.shields.io/github/downloads/DGP-studio/Snap.Genshin/latest/total?style=flat-square)](https://github.com/DGP-Studio/Snap.Genshin/releases/latest)

### 友情链接

[![KeqingNiuza](https://img.shields.io/badge/Scighost-KeqingNiuza-red/total?style=flat-square)](https://github.com/Scighost/KeqingNiuza)

### 最低系统要求

> **最低：Windows 10 1903 - 10.0.18362.x**  
> 推荐：Windows 11 21H1 - 10.0.22000.x  
> 运行时：.NET 6 [下载 .NET 6 运行时](https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-6.0.0-windows-x64-installer)  
> 运行时内存需求：150MB  
> 储存空间需求：500MB

# DGP.Genshin.Mate

> 你最好的树脂伴侣，无需复杂设置，即点即用，方便快捷  
> 独立程序，点击系统托盘图标显示实时树脂  
> 暂时不随主程序发布，需要额外 **进群 [![qq](https://img.shields.io/badge/chat-on_qq-green?style=flat-square)](https://jq.qq.com/?_wv=1027&k=tdKQiXyl)** 领取，或自行 Build

![mate.png](/Design/ImgAssets/mate.png)

# 标题栏

> 显示当前用户相关信息，支持快速多账号切换  
> 简化米游社每日签到流程，一键签到领取奖励  
> 显示当日当月摩拉原石收入情况   
> 显示米游社实时便笺，树脂余量、探索派遣，了如指掌

![标题栏按钮.png](/Design/ImgAssets/titlebarbuttons.png)

# 祈愿记录

> 支持多UID切换  
> 从游戏日志文件中获取Url/手动输入Url 双模式  
> 展示角色/武器/常驻祈愿的统计信息  
> 展示累计获得的角色与武器  
> 分祈愿展示累积获得的角色与武器  

> 支持导出到 Excel  
> 祈愿记录导出的 Excel 可以导入到 [voderl/genshin-gacha-analyzer](https://github.com/voderl/genshin-gacha-analyzer)进行分析

> 支持从 [sunfkny/genshin-gacha-export](https://github.com/sunfkny/genshin-gacha-export) 导入

![祈愿记录](/Design/ImgAssets/gacha.png)

# 玩家信息

> 按UID查询玩家信息  
> 展示全角色等级武器圣遗物命座信息  
> 展示玩家基础信息、世界探索、尘歌壶、深境螺旋、活动信息  

![record.png](/Design/ImgAssets/record.png)

# [![logo](https://youngmoe.com/static/img/png/logo.c2ceb873.png)](https://youngmoe.com/)

> 从 [天空岛数据库](https://youngmoe.com/) 获取数据，重新呈现  
> 优化的队伍推荐功能  
> 直接访问玩家角色信息，提供最佳推荐方案

![天空岛.png](/Design/ImgAssets/celestia.png)

</div>

# 反馈须知

**如果你在低于我们要求的最低Windows版本上运行程序并出现了问题，我们不会修复**  
程序所在目录下的 **crash.log** 与 **latest.log** 文件包含了崩溃信息,请一并提交

如果你需要反馈的信息包含大量的图片  
或任何不便于在issue中提供的信息  
可以加入我们的QQ群

> 我们的QQ群：**910780153**


# 开发人员

如果你是一位正在编写原神相关程序的开发者  
可以加入上方的QQ群，~~群内有很多拥有相关开发经验的大佬，可以一起探讨学习~~  

## 我们的方案

* [DGP.Genshin.Common](https://github.com/DGP-Studio/DGP.Genshin.Common)  
公共组件支持库

* [DGP.Genshin.DataViewer](https://github.com/DGP-Studio/DGP.Genshin.DataViewer)  
解包数据可视化

* [DGP.Genshin.MiHoYoAPI](https://github.com/DGP-Studio/DGP.Genshin.MiHoYoAPI)  
米哈游原神API交互

* [DGP.Genshin.YoungMoeAPI](https://github.com/DGP-Studio/DGP.Genshin.YoungMoeAPI)  
天空岛数据库API交互

## 参阅

* [Snap Genshin 开发人员指南](DeveloperGuide.md)