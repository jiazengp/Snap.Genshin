# 开发人员文档

我们正在采用下述的准则  
[框架设计准则[en-us]](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) [框架设计准则[zh-cn]](https://docs.microsoft.com/zh-cn/dotnet/standard/design-guidelines/)  
尽管部分模块尚未遵循，但我们力求新的提交遵循上述准则
 
本项目及所有子项目已经迁移至使用 .NET 6 × C# 10 + VS2022 编写环境下  
主程序使用了 WPF 作为基础 UI 框架  
*由于 Win UI 3 目前阶段存在大量  BUG  故暂时不使用*

## Snap Genshin Foundation

<details>
<summary>DGP.Genshin</summary>

主程序

</details>

<details>
<summary>DGP.Genshin.Common</summary>

公共组件支持库

* 包含**请求与响应模式**的基本类型
* 包含大量的扩展方法
* 包含了核心日志功能
* 包含了简单的下载器实现

</details>

<details>
<summary>DGP.Genshin.DataViewer</summary>

解包数据可视化呈现

* 不再积极维护

</details>
<details>
<summary>DGP.Genshin.Mate</summary>

树脂便捷显示

* 独立程序，点击系统托盘图标显示实时树脂

</details>

<details>
<summary>DGP.Genshin.MiHoYoAPI</summary>

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

</details>

<details>
<summary>DGP.Genshin.Updater</summary>

更新器

* 支持自动与手动安装
</details>

<details>
<summary>DGP.Snap.AutoVersion</summary>

用于生成主项目时自动更改版本号

</details>

## Snap Genshin 类型设计规范

### 名称规范

* 初始化视图模型 √ OpenUI × Initialize  
* 更新视图模型属性 √ Update × Refresh

### ViewModel 类型规范

``` c#
[ViewModel(ViewModelType.Transient)]
public class MyViewModel ： ObservableObject, IRecipient<T>, IOtherInterface
{
    //private consts

    //private readonly services

    //private providers

    //observables
    //private observables called methods

    //public constructors

    //private command called methods

    //IRecipient<T>.Receive methods
}
```
### 插件开发

Snap Genshin 的插件系统设计使得开发者的权限非常之高  
可以进行任何类型的 服务/视图模型 注册  
开发插件前，你需要 clone 整个 Snap Genshin 仓库到本地  
我们推荐你 folk一个分支 后再进行 clone

#### 关键信息
首先，从新建一个 .NET 6 类库开始

* 由于 Snap Genshin 使用了 Windows Runtimes API  
插件的目标框架需要基于 `net6.0-windows10.0.18362` 才能使插件正常通过编译

下面列出了项目的项目文件内的部分xml

``` xml
<PropertyGroup>
    <TargetFramework>net6.0-windows10.0.18362</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <PlatformTarget>x64</PlatformTarget>
    <DebugType>embedded</DebugType>
    <UseWPF>true</UseWPF>
    <ProduceReferenceAssembly>False</ProduceReferenceAssembly>
    <BaseOutputPath>..\..\Build\Debug\net6.0-windows10.0.18362.0\Plugins</BaseOutputPath>
</PropertyGroup>
```

其中，需要特别注意下面几条
``` xml
<TargetFramework>net6.0-windows10.0.18362</TargetFramework>
```

``` xml
<EnableDynamicLoading>true</EnableDynamicLoading>
```

```xml
<BaseOutputPath>..\..\Build\Debug\net6.0-windows10.0.18362.0\Plugins</BaseOutputPath>
```

`<BaseOutputPath>`决定了输出的路径，修改到此处使其能在生成后直接被 Snap Genshin 主程序发现

需要将 `DGP.Genshin` 等项目 添加到项目引用

完成后 项目文件也应做出相应修改
```xml
  <ItemGroup>
    <ProjectReference Include="..\..\DGP.Genshin.Common\DGP.Genshin.Common.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
    <ProjectReference Include="..\..\DGP.Genshin.MiHoYoAPI\DGP.Genshin.MiHoYoAPI.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
    <ProjectReference Include="..\..\DGP.Genshin\DGP.Genshin.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
    <ProjectReference Include="..\..\Modified\appcenter-sdk-dotnet\SDK\AppCenterAnalytics\Microsoft.AppCenter.Analytics.WindowsDesktop\Microsoft.AppCenter.Analytics.WindowsDesktop.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
    <ProjectReference Include="..\..\Modified\appcenter-sdk-dotnet\SDK\AppCenterCrashes\Microsoft.AppCenter.Crashes.WindowsDesktop\Microsoft.AppCenter.Crashes.WindowsDesktop.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
    <ProjectReference Include="..\..\Modified\appcenter-sdk-dotnet\SDK\AppCenter\Microsoft.AppCenter.WindowsDesktop\Microsoft.AppCenter.WindowsDesktop.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
  </ItemGroup>
```

下面两个节点

```xml
<Private>false</Private>
<ExcludeAssets>runtime</ExcludeAssets>
```
是非常重要的，写明这两个节点可以使编译器在生成时跳过那些程序集  
有关这两个节点的详细信息，请参阅 [微软官方文档](https://docs.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support#simple-plugin-with-no-dependencies)

在上述工作完成后，你需要实现 `DGP.Genshin.Core.Plugins.IPlugin` 接口  
此时编译程序集已经能在DGP.Genshin.exe 中发现插件

如果需要添加可导航的新页面则需要准备好一个新的Page类型  
并在实现了插件的类上标注 `[ImportPage]` 特性

由于 Snap Genshin 实现了依赖注入，你也完全可以依赖于这一套系统来注入服务

可以 在服务类上添加 `[Service]` 特性 在视图模型上添加 `[ViewModel]` 特性  
理论上，如果你想，可以注入任何类型，但是我们只推荐你注入上述的两种类型

在完成上述工作后你的插件就具有页面，并能在 Snap Genshin 左侧的导航栏中呈现了
我们很快会推出服务替换功能，届时，可以实现 Snap Genshin 内部的抽象接口并修改 Snap Genshin 的默认行为。

关于目前详细的项目示例，请参考[此处](https://github.com/DGP-Studio/Snap.Genshin/tree/main/Plugins/DGP.Genshin.Sample.Plugin)

## 生成与调试

要求：`VS2022` ，工作负荷：`.NET 桌面开发（未来需要：使用C++的桌面开发，通用Windows平台开发）`
如果未在下方说明，正常的项目均可按常见的调试方法调试

<details>
<summary>调试 DGP.Genshin</summary>

1. 生成 `DGP.Snap.AutoVersion` 项目
1. 生成 `DGP.Genshin` 项目
1. 将 根目录的 `Metadata` 文件夹复制到 `Build\Debug\net6.0-windows10.0.18362.0`
1. *注：`MetaData` 文件夹有时不会即时随仓库更新，可以从最新的发行版中提取*
1. 现在就可以正常调试程序了

如无必要，请勿随意更改`生成事件`与`生成后事件`
</details>

<details>
<summary>调试 DGP.Genshin.Mate</summary>

1. 生成 `DGP.Genshin.Mate` 项目
1. 将 `Build\Debug\net6.0-windows10.0.18362.0` 文件夹下的 `cookielist.dat` 复制到 `Build\Debug-Mate\net6.0-windows10.0.18362.0` 文件夹下
1. 我们假定你在做上一步前已经完成了一次主程序的调试并输入了有效的cookie并正常退出程序
1. 现在就可以正常调试程序了

</details>

## 开发者模式

开发者模式相对于用户模式增加了一些导航入口点，其中包含了正在制作尚未完成的内容
1. 在程序关闭的情况下打开生成文件根目录下的 `settings.json`
1. 找到 `"IsDevMode": false` 片段，修改为 `"IsDevMode": true`
1. 再次启动程序后即时生效

## 提交代码

* 如果修改了子库的代码，一定要确保先发起子库的PR，否则你的提交极有可能是不完整的
* 对主库的修改应当仅包含界面与界面的业务逻辑
* 对子库的修改应当仅包含相关API的交互代码
* 对 `DGP.Genshin.Common` 的要求相对较宽，可以包含任意代码


# 进阶

## 米游社Http抓包

自行搜索 
* Fiddler
* 移动设备Filddler代理

## 逆向米游社APK

自行搜索
* apktool (https://bitbucket.org/iBotPeaches/apktool/downloads/)
* dex2jar (https://sourceforge.net/projects/dex2jar/files/)
* jd-gui
* IDA Pro

## 米游社接口动态密钥

米哈游的部分接口调用需要提交形如 `DS:a,b,c` Header 的动态密钥  
所有的动态密钥算法势必有 **匹配的** `x-rpc-app_version` Header  
下方将详细说明动态密钥的生成方法（下文中的 `{}` 符号仅用于包裹参数，不应实际出现在你构造的字符串中）

### Salt

[查看详情](https://gist.github.com/Lightczx/373c5940b36e24b25362728b52dec4fd)

### 一代动态密钥

首先需要通过逆向安卓APK来获取米游社App对应版本的 `Salt`  
在获取了Salt后便可以开始计算动态密钥  
一代动态密钥形如 `{t},{r},{check}`  
* `t` 是发送请求时的unix时间戳（秒）  
* `r` 是一串六位的随机字符串，包含大小写字符与阿拉伯数字  
* `check` 是根据上述条件计算得出的：  
    * 首先构造以下字符串 `salt={Salt}&t={t}&r={r}` 
    * 再将此字符串进行 `MD5` 运算,运算结果即为`check`

最终,实际计算完成的 `{t},{r},{check}` 即为 `DS:` 后面的内容

### 二代动态密钥

自2.11.1版本开始，部分米游社内的接口（玩家信息相关）便需要附加二代动态密钥了  
首先仍需要通过逆向安卓APK来获取米游社App对应版本的 `Salt`  
在获取了Salt后便可以开始计算动态密钥  
二代动态密钥形如 `{t},{r},{check}`  
* `t` 是发送请求时的unix时间戳（秒）  
* `r` 是一串六位的随机字符串，包含阿拉伯数字（实际上可以包含大小写字符，但是米游社本身未使用，入乡随俗）  
该值通常在 `[100000,200000)` 区间内
* `check` 是根据上述条件计算得出的：  

    * 首先构造以下字符串 `salt={Salt}&t={t}&r={r}&b={b}&q={q}`
        * `b` 是请求的 Body（Json格式)
            * 如果发送的是 `GET` 请求  
            `b` 应该为空字符串：即 `...&b=&q...` , 
            * 如果发送的是 `POST` 请求  
            `b` 需要包含Json字符串应仅表示一个对象，即最外层仅有一对`{}`或`[]` ）
        * `q` 是请求的 url Query 参数，注意需要对参数进行字典排序（如 `&a=...&b=...`）
    * 再将此字符串进行 `MD5` 运算,运算结果即为`check`

最终,实际计算完成的 `{t},{r},{check}` 即为 `DS:` 后面的内容