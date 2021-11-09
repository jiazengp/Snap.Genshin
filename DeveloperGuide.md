# 开发人员文档

我们正在采用下述的准则  
[框架设计准则[en-us]](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) [框架设计准则[zh-cn]](https://docs.microsoft.com/zh-cn/dotnet/standard/design-guidelines/)  
尽管部分模块尚未遵循，但我们力求新的提交遵循上述准则
 
本项目及所有子项目已经迁移至使用 .NET 6 × C# 10 + VS2022 编写环境下  
主程序使用了 WPF 作为基础 UI 框架  
*由于 Win UI 3 目前阶段存在大量  BUG  故暂时不予使用*

## DGP.Genshin

UI层 基本架构如下
![架构图](https://i.loli.net/2021/10/30/tDQqhNPIOae1Ui6.png)

## [DGP.Genshin.Common](https://github.com/DGP-Studio/DGP.Genshin.Common)
公共组件支持库

* 包含**请求与响应模式**的基本类型
* 包含大量的扩展方法
* 包含了核心日志功能
* 包含了简单的下载器实现

## [DGP.Genshin.DataViewer](https://github.com/DGP-Studio/DGP.Genshin.DataViewer)
解包数据可视化呈现

* 不再积极维护

## DGP.Genshin.Mate
树脂便捷显示

* 独立程序，点击系统托盘图标显示实时树脂


## [DGP.Genshin.MiHoYoAPI](https://github.com/DGP-Studio/DGP.Genshin.MiHoYoAPI)

米哈游原神API交互

* 祈愿记录（完全封装）
* 玩家信息查询相关
* 实时便笺
* 签到与补签
* 用户信息与角色信息
* 米游社帖子
* 旅行日志信息
* 玩家信息对外展示开关
* 米游社每日任务

## DGP.Genshin.Updater

更新器

* 支持自动与手动安装

## [DGP.Genshin.YoungMoeAPI](https://github.com/DGP-Studio/DGP.Genshin.YoungMoeAPI)

天空岛数据库API交互

* 11层角色使用信息
* 12层角色武器圣遗物信息
* 数据库记录信息
* 队伍上场次数信息（用于推荐配队）

## DGP.Snap.AutoVersion

用于生成主项目时自动更改版本号

## 生成与调试

要求：`VS2022` ，工作负荷：`.NET 桌面开发（未来需要：使用C++的桌面开发，通用Windows平台开发）`
1. 生成 `DGP.Snap.AutoVersion` 项目
1. 生成 `DGP.Genshin` 项目
1. 将 根目录的 `Metadata` 文件夹复制到 `Build\Debug\net6.0-windows10.0.18362.0`
1. *注：`MetaData` 文件夹有时不会即时随仓库更新，可以从最新的发行版中提取*
1. 现在就可以正常测试程序了

如无必要，请勿随意更改`生成事件`与`生成后事件`

## 开发者模式

开发者模式相对于用户模式增加了一些导航入口点，其中包含了正在制作尚未完成的内容
1. 在程序关闭的情况下打开生成文件根目录下的settings.json
1. 找到 `"IsDevMode": false` 片段，修改为 `"IsDevMode": true`
1. 再次启动程序后即时生效

## 提交代码

* 如果修改了子库的代码，一定要确保先发起子库的PR，否则你的提交极有可能是不完整的
* 对主库的修改应当仅包含界面与界面的业务逻辑
* 对子库的修改应当仅包含相关API的交互代码
* 对 `DGP.Genshin.Common` 的要求相对较宽，可以包含任意代码