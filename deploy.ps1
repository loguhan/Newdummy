# Fplayer 部署脚本 v2.4.0

Write-Host "开始部署 Fplayer v2.4.0..." -ForegroundColor Green

# 添加所有更改
git add .

# 提交更改
git commit -m "v2.4.0: 坐标系统全面修正

- 修正了假人创建时的坐标计算，现在假人会正确站在地面上
- 重新计算了所有物理系统的坐标，包括地面检测、碰撞检测、滑轮系统等
- 添加了 /dummy ground [name] 命令来查看详细的坐标信息
- 优化了地面检测逻辑，使用玩家中心X坐标和底部Y坐标
- 修正了滑轮系统和墙壁滑行的坐标计算
- 统一了所有坐标系统，确保物理行为的一致性"

# 创建标签
git tag -a v2.4.0 -m "Fplayer v2.4.0 - 坐标系统全面修正"

# 推送到远程仓库
git push origin main
git push origin v2.4.0

Write-Host "部署完成！" -ForegroundColor Green
Write-Host "版本: v2.4.0" -ForegroundColor Yellow
Write-Host "主要更新: 坐标系统全面修正" -ForegroundColor Yellow 