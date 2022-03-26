# 开发人员文档

我们正在采用下述的准则  
[框架设计准则[en-us]](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)   [框架设计准则[zh-cn]](https://docs.microsoft.com/zh-cn/dotnet/standard/design-guidelines/)  
尽管部分模块尚未遵循，但我们力求新的提交遵循上述准则

本项目及所有子项目已经迁移至使用 .NET 6 × C# 10 + VS2022 编写环境下  
主程序使用了 WPF 作为基础 UI 框架  
*由于 Win UI 3 目前阶段存在大量  BUG  故暂时不使用*

## 实现文档

[Snap Genshin 接口与实现](ApiDoc)

## 克隆Snap Genshin 主项目

```
git clone --recurse-submodules --depth 1 https://github.com/DGP-Studio/Snap.Genshin.git
```

## 生成与调试

要求：`VS2022` ，工作负荷：`.NET 桌面开发（未来可能需要：使用C++的桌面开发，通用Windows平台开发）`
如果未在下方说明，正常的项目均可按常见的调试方法调试

### 调试 DGP.Genshin

1. 删除 `DGP.Genshin` 对 `DGP.Genshin.Secret` 的共享项目引用（如果有）
1. 生成 `DGP.Snap.AutoVersion` 项目
1. 生成 `DGP.Genshin` 项目
1. 将 根目录的 `Metadata` 文件夹复制到 `Build\Debug\net6.0-windows10.0.18362.0`
1. 现在就可以正常调试程序而不报错了

## 提交代码

* 如果修改了子库的代码，一定要确保发起子库的PR，否则你的提交是不完整的
* 对主库的修改应当仅包含界面与界面的业务逻辑
* 对子库的修改应当仅包含相关API的交互代码
* 对 `Common` 的要求相对较宽，可以包含任意代码


# 米游社接口

## 米游社Http抓包

自行搜索 
* Fiddler
* 移动设备Filddler代理

## 逆向米游社APK

* apktool (https://bitbucket.org/iBotPeaches/apktool/downloads/)
* dex2jar (https://sourceforge.net/projects/dex2jar/files/)
* jd-gui
* IDA Pro

## 米游社接口动态密钥

米哈游的部分接口调用需要提交形如 `DS:a,b,c` Normal Header 的动态密钥  
所有的动态密钥算法势必有 **匹配的** `x-rpc-app_version` Normal Header  
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
该值通常在 `[100000,200000)` 内
* `check` 是根据上述条件计算得出的：  

    * 首先构造以下字符串 `salt={Salt}&t={t}&r={r}&b={b}&q={q}`
        * `b` 是请求的 Body（Json格式)
            * 如果发送的是 `GET` 请求  
            `b` 应该为空字符串：即 `...&b=&q...` , 
            * 如果发送的是 `POST` 请求  
            `b` 需要包含Json字符串应仅表示一个Json对象，即最外层仅有一对`{}`或`[]` ）
        * `q` 是请求的 url Query 参数，注意需要对参数进行字典排序（如 `&a=...&b=...`）
    * 再将此字符串进行 `MD5` 运算,运算结果即为`check`

最终,实际计算完成的 `{t},{r},{check}` 即为 `DS:` 后面的内容