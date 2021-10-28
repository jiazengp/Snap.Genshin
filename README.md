<div align="center"> 

[![Snap.Genshin](https://socialify.git.ci/DGP-Studio/Snap.Genshin/image?description=1&font=Inter&forks=1&logo=https%3A%2F%2Fgithub.com%2FDGP-Studio%2FSnap.Genshin%2Fblob%2Fmain%2FDesign%2FSGLogo.png%3Fraw%3Dtrue&pattern=Signal&stargazers=1&theme=Dark)](https://github.com/DGP-Studio/Snap.Genshin/stargazers)

### 状态
![lines](https://img.shields.io/tokei/lines/github/DGP-Studio/Snap.Genshin?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/DGP-Studio/Snap.Genshin?style=flat-square)
![GitHub issues](https://img.shields.io/github/issues/DGP-Studio/Snap.Genshin?style=flat-square)
![GitHub closed issues](https://img.shields.io/github/issues-closed/DGP-Studio/Snap.Genshin?style=flat-square)

![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/DGP-Studio/Snap.Genshin/latest?style=flat-square)
### 前往下载

[![GitHub downloads](https://img.shields.io/github/downloads/DGP-Studio/Snap.Genshin/total?style=flat-square)](https://github.com/DGP-Studio/Snap.Genshin/releases)
[![GitHub release (latest by date)](https://img.shields.io/github/downloads/DGP-studio/Snap.Genshin/latest/total?style=flat-square)](https://github.com/DGP-Studio/Snap.Genshin/releases/latest)

### 友情链接

[![KeqingNiuza](https://img.shields.io/badge/Scighost-KeqingNiuza-red/total?style=flat-square)](https://github.com/Scighost/KeqingNiuza) [![Genshin Gacha Export](https://img.shields.io/badge/sunfkny-genshin_gacha_export-red/total?style=flat-square)](https://github.com/sunfkny/genshin-gacha-export)

### 说明

图片取自程序的开发版本，可能会与发行版有所出入

### 最低系统要求

> **最低：Windows 10 1903 - 10.0.18362.x**  
> 推荐：Windows 11 21H1 - 10.0.22000.x  
> 运行时：.NET 5  
> 运行时内存需求：300MB  
> 储存空间需求：300MB

# Snap Genshin
> 现代化响应式UI设计，杜绝无响应  
> 全自动更新流程，省心一键升级

# 标题栏

> 显示当前用户相关信息，支持快速多账号切换  
> 简化米游社每日签到流程，一键签到领取奖励  
> 显示米游社实时便笺，树脂余量、探索派遣，了如指掌

# 首页

> 显示最新官方公告  
> 官服，B服无缝切换，一键启动，支持窗口化全屏

![首页](https://i.loli.net/2021/10/10/5pQdSKxrEDAzg7t.png)

# 祈愿记录

> 支持多UID切换  
> 从游戏日志文件中获取Url/手动输入Url 双模式  
> 展示角色/武器/常驻祈愿的统计信息  
> 展示累计获得的角色与武器  
> 分祈愿展示累积获得的角色与武器  

> 支持导出到 Excel  
> 祈愿记录导出的 Excel 可以导入到 [voderl/genshin-gacha-analyzer](https://github.com/voderl/genshin-gacha-analyzer)进行分析

> 支持从 [sunfkny/genshin-gacha-export](https://github.com/sunfkny/genshin-gacha-export) 导入

![祈愿记录](https://i.loli.net/2021/10/10/Y2JUDdTpSGlhOme.png)

# 玩家信息

> 按UID查询玩家信息  
> 展示全角色等级武器圣遗物命座信息  
> 展示玩家基础信息、世界探索、尘歌壶、深境螺旋、活动信息  

![record.png](https://i.loli.net/2021/10/10/L7naeUitXCp3Ggf.png)

# 天空岛数据库

> 从 [天空岛数据库](https://youngmoe.com/) 获取数据，重新呈现  
> 优化的队伍推荐功能  
> 直接访问玩家角色信息，提供最佳推荐方案

![database1.png](https://i.loli.net/2021/10/10/j48U3Av2n9ftOQD.png)

</div>

# 反馈须知

如果你在低于我们要求的最低Windows版本上运行程序并出现了问题，我们不会修复  
程序所在目录下的 **crash.log** 与 **latest.log** 文件包含了崩溃信息,请一并提交


如果你需要反馈的信息包含大量的图片  
或任何不便于在issue中提供的信息  
可以加入我们的QQ群
> 我们的反馈QQ群：**910780153**


# 开发人员

如果你是一位正在编写原神相关程序的开发者  
我们欢迎你查看、使用、或借鉴我们在下方提供的方案

* [DGP.Genshin.Common](https://github.com/DGP-Studio/DGP.Genshin.Common)  
公共API支持库，包含了与其他API交互需要的公共组件

* [DGP.Genshin.MiHoYoAPI](https://github.com/DGP-Studio/DGP.Genshin.MiHoYoAPI)  
米哈游原神API交互

* [DGP.Genshin.YoungMoeAPI](https://github.com/DGP-Studio/DGP.Genshin.YoungMoeAPI)  
天空岛数据库API交互

发起 pull request 以向我们提交代码  
本项目及所有子项目使用 c# .NET 5 编写  
主程序使用了 WPF 作为基础UI框架

## 高级：如何手动生成并调试本项目

要求：VS2019 + 工作负荷：.NET 桌面开发（未来需要：使用C++的桌面开发，通用Windows平台开发）
1. 生成 DGP.Snap.AutoVersion 项目
1. 生成 DGP.Genshin 项目
1. 将 根目录的 Metadata 文件夹复制到 Build\Debug\net5.0-windows10.0.18362.0
1. 现在就可以正常调试主程序了

## 高级：如何进入开发者模式

开发者模式相对于用户模式增加了一些导航入口点，其中包含了正在制作尚未完成的内容
1. 在程序关闭的情况下打开生成文件根目录下的settings.json
1. 找到 `"IsDevMode": false` 片段
1. 修改为 `"IsDevMode": true`
1. 再次启动程序后即时生效

## 高级：提交代码规范

1. 如果修改了子库的代码，一定要确保先发起子库的PR，否则你的提交极有可能是不完整的
1. 对主库的修改应当仅包含界面与界面的业务逻辑
1. 对子库的修改应当仅包含相关API的交互代码
1. 对 DGP.Genshin.Common 的要求相对较宽，可以包含任意代码