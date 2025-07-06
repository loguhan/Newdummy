# Terraria 玩家物理系统逻辑

本文件夹包含了从 Terraria 源代码中提取的所有关于玩家碰撞和重力的逻辑。

## 文件结构

### 重力相关
- `GravityFields.cs` - 重力相关字段定义
- `CarpetMovement.cs` - 地毯移动和重力应用
- `WingMovement.cs` - 翅膀飞行和重力控制

### 碰撞检测
- `CollisionImmunity.cs` - 碰撞攻击免疫时间
- `HoneyCollision.cs` - 蜂蜜碰撞处理
- `WaterCollision.cs` - 水中碰撞处理
- `DryCollision.cs` - 干燥地面碰撞处理
- `SlopingCollision.cs` - 斜坡碰撞处理
- `SlopeDownMovement.cs` - 下坡移动处理
- `FloorVisuals.cs` - 地面视觉效果处理
- `NPCCollision.cs` - NPC 碰撞更新
- `BouncingBlocks.cs` - 弹跳方块碰撞处理
- `BordersMovement.cs` - 世界边界碰撞处理

## 行数对应关系

| 方法名 | 原文件行数 | 提取文件 |
|--------|------------|----------|
| 重力字段 | 724-728 | GravityFields.cs |
| CarpetMovement | 13642-13681 | CarpetMovement.cs |
| WingMovement | 13842-13900 | WingMovement.cs |
| GiveImmuneTimeForCollisionAttack | 12803-12816 | CollisionImmunity.cs |
| HoneyCollision | 14806-14818 | HoneyCollision.cs |
| WaterCollision | 14819-14866 | WaterCollision.cs |
| TryFloatingInWater | 14832-14866 | WaterCollision.cs |
| DryCollision | 14867-14943 | DryCollision.cs |
| SlopeDownMovement | 14792-14805 | SlopeDownMovement.cs |
| SlopingCollision | 14944-14967 | SlopingCollision.cs |
| FloorVisuals | 14968-15020 | FloorVisuals.cs |
| ResetFloorFlags | 15021-15027 | FloorVisuals.cs |
| GetFloorTileType | 15028-15042 | FloorVisuals.cs |
| MakeFloorDust | 15043-15167 | FloorVisuals.cs |
| Update_NPCCollision | 19424-19500 | NPCCollision.cs |
| CanParryAgainst | 19501-19506 | NPCCollision.cs |
| TryBouncingBlocks | 22116-22180 | BouncingBlocks.cs |
| BordersMovement | 15168-15195 | BordersMovement.cs |

## 使用方法

这些文件包含了完整的玩家物理系统逻辑，可以用于：
1. 理解 Terraria 的物理系统实现
2. 学习游戏物理编程技巧
3. 作为游戏开发参考
4. 进行代码分析和优化

## 注意事项

- 所有代码都来自 Terraria 1.4.0.5 版本的源代码
- 提取的代码保持了原有的行数对应关系
- 部分方法可能依赖其他类或命名空间，使用时需要相应的依赖 