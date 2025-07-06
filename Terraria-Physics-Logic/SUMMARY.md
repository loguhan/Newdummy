# Terraria 物理系统提取总结

## 提取完成！

已成功从 `Player.cs` 中提取了所有关于碰撞和重力的逻辑到 `Terraria-Physics-Logic` 文件夹。

## 提取的文件列表

### 重力相关 (3个文件)
1. **GravityFields.cs** (8行) - 重力相关字段定义
2. **CarpetMovement.cs** (40行) - 地毯移动和重力应用
3. **WingMovement.cs** (161行) - 翅膀飞行和重力控制

### 碰撞检测 (10个文件)
4. **CollisionImmunity.cs** (16行) - 碰撞攻击免疫时间
5. **HoneyCollision.cs** (15行) - 蜂蜜碰撞处理
6. **WaterCollision.cs** (50行) - 水中碰撞处理 (包含 TryFloatingInWater 嵌套方法)
7. **SlopeDownMovement.cs** (16行) - 下坡移动处理
8. **DryCollision.cs** (79行) - 干燥地面碰撞处理
9. **SlopingCollision.cs** (26行) - 斜坡碰撞处理
10. **FloorVisuals.cs** (168行) - 地面视觉效果处理 (包含3个嵌套方法)
11. **BordersMovement.cs** (34行) - 世界边界碰撞处理
12. **NPCCollision.cs** (94行) - NPC 碰撞更新 (包含 CanParryAgainst 嵌套方法)
13. **BouncingBlocks.cs** (55行) - 弹跳方块碰撞处理

## 总计
- **文件数量**: 13个
- **总行数**: 约 762行
- **包含嵌套方法**: 5个嵌套方法

## 原文件行数对应关系

| 文件名 | 原文件行数 | 提取行数 | 描述 |
|--------|------------|----------|------|
| GravityFields.cs | 724-728 | 8 | 重力字段定义 |
| CarpetMovement.cs | 13642-13681 | 40 | 地毯移动重力应用 |
| WingMovement.cs | 13842-13900 | 161 | 翅膀飞行控制 |
| CollisionImmunity.cs | 12803-12816 | 16 | 碰撞免疫时间 |
| HoneyCollision.cs | 14806-14818 | 15 | 蜂蜜碰撞 |
| WaterCollision.cs | 14819-14866 | 50 | 水中碰撞+浮力 |
| SlopeDownMovement.cs | 14792-14805 | 16 | 下坡移动 |
| DryCollision.cs | 14867-14943 | 79 | 干燥地面碰撞 |
| SlopingCollision.cs | 14944-14967 | 26 | 斜坡碰撞 |
| FloorVisuals.cs | 14968-15167 | 168 | 地面效果+3个嵌套方法 |
| BordersMovement.cs | 15168-15195 | 34 | 世界边界 |
| NPCCollision.cs | 19424-19506 | 94 | NPC碰撞+格挡 |
| BouncingBlocks.cs | 22116-22180 | 55 | 弹跳方块 |

## 使用方法

1. 所有文件都包含了完整的代码逻辑
2. 每个文件都有详细的注释说明来源行数
3. 嵌套方法已包含在相应的文件中
4. 可以直接用于学习和参考

## 注意事项

- 代码保持了原有的逻辑结构
- 所有方法都标注了原始行数来源
- 部分方法可能依赖其他类或命名空间
- 建议结合 README.md 了解整体结构 