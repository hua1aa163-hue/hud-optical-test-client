# HUD 光学测试客户端

本项目参考 `SimpleProtocolServer` 重构，是一个面向 HUD 光学软件的 Windows TCP 客户端。

主要变化：

- 原投影/图片测试子界面成为唯一主工作台，保留目录扫描、图片预览、投放、定时投图和批量联测。
- TCP 角色由监听服务器改为主动连接光学软件服务器，默认地址按协议截图设置为 `127.0.0.1:5556`。
- 根据《HUD通信协议 V3.1》内置 `t1`～`t21` 和 5 条辅助指令；每条发送内容均可直接编辑。
- 测试项的用途、动态参数、返回字段顺序和含义在界面中并排显示，便于对照。
- 批量联测启动时冻结当前发送内容，避免运行中编辑导致测试批次前后不一致。

构建与验证：

```powershell
dotnet build .\HudOpticalTestClient.sln -c Release -p:UseSharedCompilation=false
dotnet run --project .\HudOpticalTestClient.SmokeTests\HudOpticalTestClient.SmokeTests.csproj -c Release --no-build
```
