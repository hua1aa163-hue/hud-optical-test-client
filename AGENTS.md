# Codex 协作说明

## 项目入口

- 解决方案：`HudOpticalTestClient.sln`
- 主程序：`HudOpticalTestClient/HudOpticalTestClient.csproj`
- 冒烟检查：`HudOpticalTestClient.SmokeTests/HudOpticalTestClient.SmokeTests.csproj`
- 构建：`dotnet build HudOpticalTestClient.sln -c Release`

## 修改约定

- `main` 保持可构建；日常修改默认使用 `codex/<任务名>` 分支。
- 提交前至少运行一次 Release 构建；协议改动还要运行 SmokeTests。
- 不提交 `bin`、`obj`、`.vs`、用户配置、密钥、日志或发布产物。
- TCP、投影屏幕和真实 HUD 联调不能由离线构建替代，交付时写明实际验证范围。
- 协议有变化时，同步更新 `协议实现说明.md` 与协议实现说明。

