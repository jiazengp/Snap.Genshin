# 教程：插件开发

Snap Genshin 的插件系统设计 使得开发者能够开发权限极高的插件  
可以调整 Snap Genshin 的默认行为，修改已经存在的服务与视图  
可以进行任何类型的 服务/视图模型 注册  

开发插件前，你需要 `Clone` 整个 `Snap Genshin` 仓库到本地  
我们推荐你在 `Folk` 后再进行 `Clone`  
`Clone` 完成后，使用 `Visual Studio 2022` 打开 `Snap.Genshin.sln` 文件

## 新建 .NET 6 类库

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
    <Platforms>AnyCPU</Platforms>
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
* 由于 Snap Genshin 使用了 Windows Runtimes API  
插件的目标框架需要基于 `net6.0-windows10.0.18362` 才能使插件正常通过编译

## 添加 `SnapGenshinPlugin`特性

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

## 实现 `IPlugin` 接口

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
        public bool IsEnabled
        {
            get;
            set;
        }
    }
}
```

此时若生成项目，则 Snap Genshin 已经能在插件管理页面中发现新的插件

## 添加导航页面

如果需要添加可导航的新页面则需要准备好一个新的Page  
对应的 xaml 文件中的代码在此省略

``` c#
using System.Windows.Controls;

namespace DGP.Genshin.Sample.Plugin
{
    public partial class SamplePage : Page
    {
        public SamplePage()
        {
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

第三个参数是图标的字符串形式，详见 [segoe-fluent-icons-font](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font)

此时生成程序集可以发现 Snap Genshin 的左侧导航栏已经包含了新的导航页面入口

## 依赖关系注入

由于 Snap Genshin 实现了依赖注入，你也完全可以依赖于这一套系统来注入服务或视图模型

例如：可以在服务类上添加 `[Service]` 特性 在视图模型上添加 `[ViewModel]` 特性  

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
            List<object>? list = new();
            ICollection<FontFamily>? families = Fonts.GetFontFamilies(@"C:\Windows\Fonts\segmdl2.ttf");
            foreach (FontFamily family in families)
            {
                ICollection<Typeface>? typefaces = family.GetTypefaces();
                foreach (Typeface typeface in typefaces)
                {
                    typeface.TryGetGlyphTypeface(out GlyphTypeface glyph);
                    IDictionary<int, ushort> characterMap = glyph.CharacterToGlyphMap;

                    foreach (KeyValuePair<int, ushort> kvp in characterMap)
                    {
                        list.Add(new { Glyph = (char)kvp.Key, Data = kvp.Key });
                    }
                }
            }
            icons = list;
        }
    }
}
```

## 项目示例

关于详细的项目示例，请参考[此处](https://github.com/DGP-Studio/Snap.Genshin/tree/main/Plugins/DGP.Genshin.Sample.Plugin)