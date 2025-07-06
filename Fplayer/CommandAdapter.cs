using TShockAPI;
using Microsoft.Xna.Framework;
using TrProtocol.Packets;
using Terraria;
namespace Fplayer;

/// <summary>
/// 命令执行结果
/// </summary>
public class CommandResult
{
    public bool Success { get; }
    public string Message { get; }

    public CommandResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}

internal class CommandAdapter
{
    private static readonly Dictionary<string, CommandDelegate> _actions = new()
    {
        { "remove", RemoveDummy },
        { "list", DummyList },
        { "reconnect", ReConnect },
        { "create", CreateDummyWrapper },
        { "action", DummyAction },
        { "startmove", StartMove },
        { "stopmove", StopMove },
        { "jump", Jump },
        { "tp", TeleportDummyToPlayer },
        { "stool", TogglePortableStool },
        { "pulley", CheckPulleyStatus },
        { "movement", CheckMovementStatus },
        { "gravity", CheckGravityStatus },
        { "collision", CheckCollisionStatus },
        { "special", CheckSpecialMovementStatus },
        { "climb", CheckClimbStatus },
        { "ground", CheckGroundStatus }
    };

    public static void Adapter(CommandArgs args)
    {
        if (args.Parameters.Count >= 1)
        {
            var subcmd = args.Parameters[0].ToLower();
            if (_actions.TryGetValue(subcmd, out var action))
            {
                action(args);
                return;
            }
        }
        args.Player.SendInfoMessage("dummy remove [name] 移除目标假人");
        args.Player.SendInfoMessage("dummy list 假人列表");
        args.Player.SendInfoMessage("dummy reconnect [name] 重新连接");
        args.Player.SendInfoMessage("dummy create [name] 创建假人");
        args.Player.SendInfoMessage("dummy startmove [name] [left|right] 开始持续移动");
        args.Player.SendInfoMessage("dummy stopmove [name] 停止移动");
        args.Player.SendInfoMessage("dummy jump [name] 跳跃");
        args.Player.SendInfoMessage("dummy tp [name] [player] 传送假人到玩家位置");
        args.Player.SendInfoMessage("dummy stool [name] [on|off] 设置便携式凳子状态");
        args.Player.SendInfoMessage("dummy pulley [name] 查看滑轮状态");
        args.Player.SendInfoMessage("dummy movement [name] 查看移动状态");
        args.Player.SendInfoMessage("dummy gravity [name] 查看重力状态");
        args.Player.SendInfoMessage("dummy collision [name] 查看碰撞状态");
        args.Player.SendInfoMessage("dummy special [name] 查看特殊移动状态");
        args.Player.SendInfoMessage("dummy climb [name] 查看上坡状态");
        args.Player.SendInfoMessage("dummy ground [name] 查看地面坐标状态");
    }

    private static void ReConnect(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy reconnect <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy != null)
        {
            if (!dummy.Active)
            {
                dummy.GameLoop("127.0.0.1", Plugin.Port, TShock.Config.Settings.ServerPassword);
                args.Player.SendSuccessMessage($"假人 '{dummyName}' 重新连接中");
            }
            else
            {
                args.Player.SendInfoMessage($"假人 '{dummyName}' 已经连接");
            }
        }
        else
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
        }
    }

    private static void DummyList(CommandArgs args)
    {
        if (Plugin.DummyPlayers.Count == 0)
        {
            args.Player.SendInfoMessage("当前没有活跃的假人");
            return;
        }

        args.Player.SendInfoMessage($"共有 {Plugin.DummyPlayers.Count} 个假人:");
        foreach (var dummy in Plugin.DummyPlayers)
        {
            var status = dummy.Active ? "活跃" : "离线";
            args.Player.SendInfoMessage($"- {dummy.Name}: {status}, 位置: ({dummy.Position.X:F1}, {dummy.Position.Y:F1})");
        }
    }

    private static void RemoveDummy(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy remove <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy != null)
        {
            Plugin.DummyPlayers.Remove(dummy);
            dummy.Close();
            args.Player.SendSuccessMessage($"假人 '{dummyName}' 已移除");
        }
        else
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
        }
    }

    /// <summary>
    /// 创建假人
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <returns>命令结果</returns>
    public static CommandResult CreateDummy(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            return new CommandResult(false, "用法: /dummy create <假人名称>");
        }

        string dummyName = args.Parameters[1];
        
        // 检查假人名称是否已存在
        if (Plugin.DummyPlayers.Any(d => d.Name == dummyName))
        {
            return new CommandResult(false, $"假人 '{dummyName}' 已存在");
        }

        // 确定初始位置 - 始终使用世界出生点
        Vector2 initialPosition;
        try
        {
            // 使用世界出生点 - 确保坐标正确
            int spawnX = Main.spawnTileX;
            int spawnY = Main.spawnTileY;
            
            // 转换为像素坐标，考虑玩家高度（48像素）
            // 让假人站在地面上，而不是悬浮在空中
            float playerHeight = 48f;
            initialPosition = new Vector2(spawnX * 16f, spawnY * 16f - playerHeight);
            
        }
        catch
        {
            // 如果获取出生点失败，使用默认位置
            initialPosition = new Vector2(100 * 16f, 100 * 16f - 48f);
        }

        // 创建假人
        var dummy = new DummyPlayer(new SyncPlayer
        {
            Name = dummyName,
            SkinVariant = 0,
            Hair = 0,
            HairDye = 0,
            HideMisc = 0,
            HairColor = new TrProtocol.Models.Color(255, 255, 255),
            SkinColor = new TrProtocol.Models.Color(255, 255, 255),
            EyeColor = new TrProtocol.Models.Color(255, 255, 255),
            ShirtColor = new TrProtocol.Models.Color(255, 255, 255),
            UnderShirtColor = new TrProtocol.Models.Color(255, 255, 255),
            PantsColor = new TrProtocol.Models.Color(255, 255, 255),
            ShoeColor = new TrProtocol.Models.Color(255, 255, 255)
        }, Guid.NewGuid().ToString());

        // 设置初始位置
        dummy.SetPosition(initialPosition.X, initialPosition.Y);

        Plugin.DummyPlayers.Add(dummy);
        
        // 启动假人连接
        try
        {
            dummy.GameLoop("127.0.0.1", Plugin.Port, TShock.Config.Settings.ServerPassword);
        }
        catch (Exception ex)
        {
            // 如果连接失败，从列表中移除
            Plugin.DummyPlayers.Remove(dummy);
            return new CommandResult(false, $"假人 '{dummyName}' 连接失败: {ex.Message}");
        }
        
        string creator = args.Player?.Name ?? "服务端";
        return new CommandResult(true, $"假人 '{dummyName}' 已由 {creator} 在世界出生点 ({initialPosition.X:F1}, {initialPosition.Y:F1}) 创建并连接");
    }

    private static void DummyAction(CommandArgs args)
    {
        if (args.Parameters.Count < 3)
        {
            args.Player.SendErrorMessage("用法: /dummy action [name] [move|jump|use|say] [参数]");
            return;
        }
        if (!string.IsNullOrEmpty(args.Parameters[1]) && !string.IsNullOrEmpty(args.Parameters[2]))
        {
            var ply = FindDummyByName(args.Parameters[1]);
            if (ply == null)
            {
                args.Player.SendErrorMessage("目标假人不存在!");
                return;
            }
            var action = args.Parameters[2].ToLower();
            switch (action)
            {
                case "move":
                    if (args.Parameters.Count < 4)
                    {
                        args.Player.SendErrorMessage("用法: /dummy action [name] move [left|right|stop]");
                        return;
                    }
                    ply.Move(args.Parameters[3]);
                    args.Player.SendSuccessMessage($"假人 '{args.Parameters[1]}' 执行移动动作: {args.Parameters[3]}");
                    break;
                case "jump":
                    ply.Jump();
                    args.Player.SendSuccessMessage($"假人 '{args.Parameters[1]}' 执行跳跃动作");
                    break;
                case "use":
                    ply.Use();
                    args.Player.SendSuccessMessage($"假人 '{args.Parameters[1]}' 执行使用物品动作");
                    break;
                case "say":
                    if (args.Parameters.Count < 4)
                    {
                        args.Player.SendErrorMessage("用法: /dummy action [name] say [消息]");
                        return;
                    }
                    ply.ChatText(args.Parameters[3]);
                    args.Player.SendSuccessMessage($"假人 '{args.Parameters[1]}' 发送消息: {args.Parameters[3]}");
                    break;
                default:
                    args.Player.SendErrorMessage("未知动作类型，支持: move, jump, use, say");
                    break;
            }
        }
    }

    private static void StartMove(CommandArgs args)
    {
        if (args.Parameters.Count < 3)
        {
            args.Player.SendErrorMessage("用法: /dummy startmove [name] [left|right]");
            return;
        }

        string dummyName = args.Parameters[1];
        string direction = args.Parameters[2].ToLower();
        var dummy = FindDummyByName(dummyName);

        if (dummy == null)
            {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
                return;
            }

            switch (direction)
            {
                case "left":
                dummy.StartMovingLeft();
                args.Player.SendSuccessMessage($"假人 '{dummyName}' 开始持续向左移动");
                    break;
                case "right":
                dummy.StartMovingRight();
                args.Player.SendSuccessMessage($"假人 '{dummyName}' 开始持续向右移动");
                    break;
                default:
                args.Player.SendErrorMessage("方向参数错误，请使用 left 或 right");
                    break;
        }
    }

    private static void StopMove(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy stopmove [name]");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);

        if (dummy == null)
            {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
                return;
            }

        dummy.StopMoving();
        args.Player.SendSuccessMessage($"假人 '{dummyName}' 已停止移动");
    }

    private static void Jump(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy jump <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

            dummy.Jump();
        args.Player.SendSuccessMessage($"假人 '{dummyName}' 执行跳跃");
        }

    /// <summary>
    /// 将假人传送到指定玩家身边
    /// 用法: /dummy tp [假人名] [玩家名]
    /// </summary>
    private static void TeleportDummyToPlayer(CommandArgs args)
    {
        if (args.Parameters.Count < 3)
        {
            args.Player.SendErrorMessage("用法: /dummy tp [假人名] [玩家名]");
            return;
        }
        string dummyName = args.Parameters[1];
        string playerName = args.Parameters[2];
        var dummy = FindDummyByName(dummyName);
        var target = TShockAPI.TSPlayer.FindByNameOrID(playerName).FirstOrDefault();
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }
        if (target == null || !target.Active)
        {
            args.Player.SendErrorMessage($"玩家 '{playerName}' 不在线");
            return;
        }
        // 取玩家服务器坐标（像素）
        float x = target.X;
        float y = target.Y;
        dummy.TeleportTo(x, y);
        args.Player.SendSuccessMessage($"假人 '{dummyName}' 已传送到玩家 '{target.Name}' 的位置 ({x:F1}, {y:F1})");
    }

    /// <summary>
    /// 根据名称查找假人
    /// </summary>
    /// <param name="name">假人名称</param>
    /// <returns>找到的假人，如果不存在则返回null</returns>
    private static DummyPlayer? FindDummyByName(string name)
    {
        return Plugin.DummyPlayers.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // 包装CreateDummy方法以适配CommandDelegate签名
    private static void CreateDummyWrapper(CommandArgs args)
    {
        var result = CreateDummy(args);
        if (result.Success)
        {
            args.Player.SendSuccessMessage(result.Message);
        }
        else
        {
            args.Player.SendErrorMessage(result.Message);
        }
    }

    /// <summary>
    /// 切换便携式凳子状态
    /// </summary>
    private static void TogglePortableStool(CommandArgs args)
    {
        if (args.Parameters.Count < 3)
        {
            args.Player.SendErrorMessage("用法: /dummy stool <假人名称> <on|off>");
            return;
        }

        string dummyName = args.Parameters[1];
        string stoolState = args.Parameters[2].ToLower();
        
        var dummy = FindDummyByName(dummyName);
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        bool isUsingStool = stoolState == "on";
        dummy.SetPortableStool(isUsingStool);
        
        string status = isUsingStool ? "启用" : "禁用";
        args.Player.SendSuccessMessage($"假人 '{dummyName}' 便携式凳子已{status}");
    }

    /// <summary>
    /// 检查滑轮状态
    /// </summary>
    private static void CheckPulleyStatus(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy pulley <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        string pulleyStatus = dummy.GetPulleyStatus();
        bool stoolStatus = dummy.IsUsingPortableStool();
        
        args.Player.SendInfoMessage($"假人 '{dummyName}' 状态:");
        args.Player.SendInfoMessage($"- 滑轮: {pulleyStatus}");
        args.Player.SendInfoMessage($"- 便携式凳子: {(stoolStatus ? "启用" : "禁用")}");
        args.Player.SendInfoMessage($"- 位置: ({dummy.Position.X:F1}, {dummy.Position.Y:F1})");
    }

    /// <summary>
    /// 检查移动状态
    /// </summary>
    private static void CheckMovementStatus(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy movement <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        string movementStatus = dummy.GetMovementStatus();
        
        args.Player.SendInfoMessage($"假人 '{dummyName}' 移动状态:");
        args.Player.SendInfoMessage($"- {movementStatus}");
        args.Player.SendInfoMessage($"- 位置: ({dummy.Position.X:F1}, {dummy.Position.Y:F1})");
    }



    /// <summary>
    /// 检查重力状态
    /// </summary>
    private static void CheckGravityStatus(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy gravity <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        string gravityStatus = dummy.GetGravityStatus();
        
        args.Player.SendInfoMessage($"假人 '{dummyName}' 重力状态:");
        args.Player.SendInfoMessage($"- {gravityStatus}");
    }

    /// <summary>
    /// 检查碰撞状态
    /// </summary>
    private static void CheckCollisionStatus(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy collision <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        args.Player.SendInfoMessage($"假人 '{dummyName}' 碰撞状态:");
        args.Player.SendInfoMessage($"- 位置: ({dummy.Position.X:F1}, {dummy.Position.Y:F1})");
        args.Player.SendInfoMessage($"- 速度: ({dummy.Velocity.X:F1}, {dummy.Velocity.Y:F1})");
    }

    /// <summary>
    /// 检查特殊移动状态
    /// </summary>
    private static void CheckSpecialMovementStatus(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy special <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        string specialStatus = dummy.GetSpecialMovementStatus();
        
        args.Player.SendInfoMessage($"假人 '{dummyName}' 特殊移动状态:");
        args.Player.SendInfoMessage($"- {specialStatus}");
    }

    /// <summary>
    /// 检查上坡状态
    /// </summary>
    private static void CheckClimbStatus(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy climb <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        string climbStatus = dummy.GetClimbStatus();
        
        args.Player.SendInfoMessage($"假人 '{dummyName}' 上坡状态:");
        args.Player.SendInfoMessage($"- {climbStatus}");
    }

    /// <summary>
    /// 检查地面坐标状态
    /// </summary>
    private static void CheckGroundStatus(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy ground <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy == null)
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
            return;
        }

        string groundStatus = dummy.GetGroundStatus();
        
        args.Player.SendInfoMessage($"假人 '{dummyName}' 地面状态:");
        args.Player.SendInfoMessage($"- {groundStatus}");
    }
}
