# cat-netcore
cat netcore客户端

去除心跳功能。（net core基本库不支持）。
部分功能在netcore下的实现修改。
配置文件读取更改。
示例配置文件为：

appsettings.json

{
  "ZhaoGangMonitor": {
    "Severs": "192.168.1.251,192.168.1.252"
  },
  "FrameworkSetting": {
    "SystemAlias": "zhaogang.netcore.cat.sdk"
  }
}

可根据需要修改。


