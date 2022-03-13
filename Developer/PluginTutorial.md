# 教程：插件开发

## 开始之前

我们假定你已经

* 掌握了基本的WPF开发知识与技巧
* 对 依赖注入/控制反转 有一定的了解

## 开发适用于 Snap Genshin 的插件程序集
Snap Genshin 的插件系统设计 使得开发者能够开发权限较高的插件  
可以调整 Snap Genshin 的默认行为，修改已经存在的服务与视图  
可以进行任何类型的 服务/工厂/视图模型/视图 注册  

开发插件前，你需要 `Clone` 整个 `Snap Genshin` 仓库到本地  
完整克隆的方法请参阅 [开发人员文档](DeveloperGuide)  
`Clone` 完成后，使用 `Visual Studio 2022` 打开 `Snap.Genshin.sln` 文件

## 修改主项目的生成行为

* 由于主项目使用了自动递增版本号行为  
可能会导致插件程序集引用了相比 Snap Genshin 发行版更高的版本  
进而使插件程序集无法被加载到 Snap Genshin 中
* 需要将 `DGP.Genshin` 项目的`预生成事件`清空
* 找到 `DGP.Genshin\Properties\AssemblyInfo.cs` 文件  
将 `[assembly: AssemblyVersion("*.*.*.*")]`  
修改为 `[assembly: AssemblyVersion("0.0.0.0")]`
* 也可以修改为指定的发行版本号以屏蔽较低版本的 Snap Genshin 加载你的插件

## 新建 `.NET 6` 类库

我们推荐你在 `Plugins` 文件夹下新建项目，这样可以与我们的教程高度匹配  
否则，可能需要按要求修改一些相对路径
新建项目完成后，修改项目的项目文件 `*.csproj`  
下面给出示例xml

``` xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!--需要基于 `net6.0-windows10.0.18362` 才能使插件正常通过编译-->
    <TargetFramework>net6.0-windows10.0.18362</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <!--必须启用动态加载-->
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <!--必须指定生成目标为x64-->
    <PlatformTarget>x64</PlatformTarget>
    <!--将PDB嵌入到生成的程序集内-->
    <DebugType>embedded</DebugType>
    <UseWPF>true</UseWPF>
    <!--不能生成为引用程序集-->
    <ProduceReferenceAssembly>False</ProduceReferenceAssembly>
    <Platforms>AnyCPU;x64</Platforms>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove="bin\**" />
    <EmbeddedResource Remove="bin\**" />
    <None Remove="bin\**" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\DGP.Genshin\DGP.Genshin.csproj">
      <Private>false</Private>
      <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
  </ItemGroup>

  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <!--将生成的主程序集复制到Plugins文件夹内-->
    <Exec Command="xcopy &quot;$(TargetPath)&quot; &quot;$(SolutionDir)Build\Debug\net6.0-windows10.0.18362.0\Plugins&quot; /y" />
  </Target>

</Project>
```

## 添加 `SnapGenshinPlugin` 特性

Snap Genshin 凭借 
``` c#
DGP.Genshin.Core.Plugins.SnapGenshinPluginAttribute
```

进行插件程序集与普通程序集的区分  
只有包含了该特性的程序集才会被Snap Genshin 认为是插件而加载

``` c#
[assembly:SnapGenshinPlugin]
```
可以将该行代码放在项目的任何 `C#` 文件中，注意：assembly 特性不能放置在 名称空间中  
较为合理的位置是开发者熟悉的 `AssemblyInfo.cs` 文件

## 实现 `IPlugin` 主接口

在上述工作完成后，你需要主类以实现 
``` c#
DGP.Genshin.Core.Plugins.IPlugin
```
接口  

``` c#
using DGP.Genshin.Core.Plugins;
using System;

namespace DGP.Genshin.Sample.Plugin
{
    /// <summary>
    /// 插件实例实现
    /// </summary>
    public class SamplePlugin : IPlugin
    {
        public string Name => "插件名称";
        public string Description => "插件描述";
        public string Author => "DGP Studio";
        public Version Version => new("0.0.0.1");
        [Obsolete] public bool IsEnabled { get; set; }
    }
}
```

此时若生成项目，则 Snap Genshin 已经能在插件管理页面中发现新的插件

## `ImportPage` 添加导航页面

> 此操作是可选的

如果需要添加可导航的新页面则需要准备好一个新的Page  
对应的 xaml 文件中的代码在此省略

``` c#
using System.Windows.Controls;

namespace DGP.Genshin.Sample.Plugin
{
    [View]
    public partial class SamplePage : Page
    {
        public SamplePage(SmapleViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
```

并在实现了插件的主类上标注对应的 `[ImportPage]` 特性

``` c#
using DGP.Genshin.Core.Plugins;
using System;

namespace DGP.Genshin.Sample.Plugin
{
    /// <summary>
    /// 插件实例实现
    /// </summary>
    [ImportPage(typeof(SamplePage), "插件页面名称", "\uE734")]
    public class SamplePlugin : IPlugin
    {
        ···
    }
}
```

Snap Genshin 基于 
``` c#
DGP.Genshin.Core.Plugins.ImportPageAttribute
```
特性发现插件注册的导航页面
``` c#
[ImportPage(typeof(SamplePage), "插件页面名称", "\uE734")]
```
一个插件可以通过此方法注册多个导航页面


第三个参数是图标的字符串形式，详见 [segoe-fluent-icons-font](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font)  
也可以使用另一个 `ImportPage` 的构造函数，采用了 `IconFactory` 类作为第三个参数

此时生成程序集可以发现 Snap Genshin 的左侧导航栏已经包含了新的导航页面入口

## 依赖关系注入

由于 Snap Genshin 实现了依赖注入，你也完全可以依赖于这一套系统来注入服务或视图模型  
例如：
* 可以在服务类上添加 `[Service]` 特性 
* 在视图模型上添加 `[ViewModel]` 特性  
* 在页面上添加 `[View]` 特性
* 在工厂类上添加 `[Factory]`特性

``` c#
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Snap.Core.DependencyInjection;
using System.Collections.Generic;
using System.Windows.Media;

namespace DGP.Genshin.Sample.Plugin
{
    [ViewModel(InjectAs.Transient)]
    internal class SampleViewModel : ObservableObject
    {
        private IEnumerable<object> icons;

        public IEnumerable<object> Icons { get => icons; set => SetProperty(ref icons, value); }

        public SampleViewModel()
        {
            icons = new();
        }
    }
}
```

# 进阶

## Snap Genshin 生命周期感知

### `IAppStartUp` 应用程序启动事件感知

在你的插件的主类上实现
``` c#
DGP.Genshin.Core.LifeCycle.IAppStartUp
```
接口，该接口提供了 `Happen(IContainer)` 方法  
以便在程序启动时对你的插件注入的类进行操作

`IContainer` 提供 `Find()` 方法 以便插件发现注入的类

> 该容器是已经定型的容器，仅能从中发现你的插件注入的服务

### AppExitingMessage 应用程序退出事件感知

在任何你注入的类中 实现 

``` c#
Microsoft.Toolkit.Mvvm.Messaging.IRecipient<DGP.Genshin.Message.AppExitingMessage>
```
接口，并在构造器中注入 
```
Microsoft.Toolkit.Mvvm.Messaging.IMessaenger
```
类型

在构造器中 调用 IMessenger 的相关注册消息方法  
并且不要忘记在析构器中 取消注册

在 `IRecipient<AppExitingMessage>` 的接口方法中就可以处理应用程序退出时的逻辑了

## 保存设置

### 与应用程序设置储存到一起

在你访问设置的类中实例化一个
```
DGP.Genshin.Service.Abstraction.Setting.SettingDefinition<T>
```
的静态只读变量  
该类提供了方便的方法供你储存与读取设置  
设置项会在程序启动时读取完成，会在程序退出的最后保存

> 在注册新的设置项前需要前往  
> `DGP.Genshin.Service.Abstraction.Setting.Setting2`  
> 类中查看已有的设置项，避免与已有的注册项冲突  


## 项目示例

关于详细的项目示例，请参考[此处](https://github.com/DGP-Studio/Snap.Genshin/tree/main/Plugins)