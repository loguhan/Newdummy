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
        { "jump", Jump }
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
    }

    private static void ReConnect(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /fdummy reconnect <假人名称>");
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
            args.Player.SendErrorMessage("用法: /fdummy remove <假人名称>");
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
        if (args.Parameters.Count < 1)
        {
            return new CommandResult(false, "用法: /fdummy create <假人名称>");
        }

        string dummyName = args.Parameters[0];
        
        // 检查假人名称是否已存在
        if (Plugin.DummyPlayers.Any(d => d.Name == dummyName))
        {
            return new CommandResult(false, $"假人 '{dummyName}' 已存在");
        }

        // 确定初始位置
        Vector2 initialPosition;
        if (args.Player != null)
        {
            // 如果是玩家创建的，在玩家位置创建
            initialPosition = new Vector2(args.Player.X, args.Player.Y);
        }
        else
        {
            // 如果是服务端创建的，在出生点创建
            initialPosition = new Vector2(Main.spawnTileX * 16f, Main.spawnTileY * 16f);
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
        
        string creator = args.Player?.Name ?? "服务端";
        return new CommandResult(true, $"假人 '{dummyName}' 已由 {creator} 在位置 ({initialPosition.X:F1}, {initialPosition.Y:F1}) 创建");
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
                case "say":
                    if (args.Parameters.Count < 4)
                    {
                        args.Player.SendErrorMessage("用法: /dummy action [name] say [内容]");
                        return;
                    }
                    var msg = string.Join(' ', args.Parameters.Skip(3));
                    ply.ChatText(msg);
                    args.Player.SendSuccessMessage($"假人[{args.Parameters[1]}] 说: {msg}");
                    break;
                case "move":
                    if (args.Parameters.Count < 4)
                    {
                        args.Player.SendErrorMessage("用法: /dummy action [name] move [left|right|stop]");
                        return;
                    }
                    var dir = args.Parameters[3].ToLower();
                    ply.Move(dir);
                    args.Player.SendSuccessMessage($"假人[{args.Parameters[1]}] move: {dir}");
                    break;
                case "jump":
                    ply.Jump();
                    args.Player.SendSuccessMessage($"假人[{args.Parameters[1]}] 跳跃");
                    break;
                case "use":
                    ply.Use();
                    args.Player.SendSuccessMessage($"假人[{args.Parameters[1]}] 使用物品");
                    break;
                default:
                    args.Player.SendErrorMessage("未知action类型: " + action);
                    break;
            }
        }
        else
        {
            args.Player.SendErrorMessage("请输入正确的名称和action类型!");
        }
    }

    private static void StartMove(CommandArgs args)
    {
        if (args.Parameters.Count < 3)
        {
            args.Player.SendErrorMessage("用法: /dummy startmove [name] [left|right]");
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
            var direction = args.Parameters[2].ToLower();
            switch (direction)
            {
                case "left":
                    ply.StartMovingLeft();
                    args.Player.SendSuccessMessage($"假人[{args.Parameters[1]}] 开始持续向左移动");
                    break;
                case "right":
                    ply.StartMovingRight();
                    args.Player.SendSuccessMessage($"假人[{args.Parameters[1]}] 开始持续向右移动");
                    break;
                default:
                    args.Player.SendErrorMessage("方向必须是 left 或 right");
                    break;
            }
        }
        else
        {
            args.Player.SendErrorMessage("请输入正确的名称和方向!");
        }
    }

    private static void StopMove(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /dummy stopmove [name]");
            return;
        }
        if (!string.IsNullOrEmpty(args.Parameters[1]))
        {
            var ply = FindDummyByName(args.Parameters[1]);
            if (ply == null)
            {
                args.Player.SendErrorMessage("目标假人不存在!");
                return;
            }
            ply.StopMoving();
            args.Player.SendSuccessMessage($"假人[{args.Parameters[1]}] 停止移动");
        }
        else
        {
            args.Player.SendErrorMessage("请输入正确的名称!");
        }
    }

    private static void Jump(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("用法: /fdummy jump <假人名称>");
            return;
        }

        string dummyName = args.Parameters[1];
        var dummy = FindDummyByName(dummyName);
        
        if (dummy != null)
        {
            dummy.Jump();
            args.Player.SendSuccessMessage($"假人 '{dummyName}' 跳跃");
        }
        else
        {
            args.Player.SendErrorMessage($"假人 '{dummyName}' 不存在");
        }
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
}
