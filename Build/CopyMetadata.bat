:: 将调试更改过的元数据复制并存档
xcopy 操作与问题指南.url Publish\操作与问题指南.url /e /y
@echo 复制元数据到发布文件夹
xcopy Debug\net6.0-windows10.0.18362.0\Metadata Publish\Metadata /e /y
@echo 复制元数据到存档文件夹
xcopy Debug\net6.0-windows10.0.18362.0\Metadata ..\Metadata /e /y