using System.Net;
using Microsoft.Xna.Framework;
using TrProtocol;
using TrProtocol.Models;
using TrProtocol.Packets;
using TrProtocol.Packets.Modules;
using TShockAPI;
using Terraria;

namespace Fplayer;
internal class DummyPlayer
{
    public byte PlayerSlot { get; private set; }
    public string CurRelease = "Terraria279";

    private readonly string UUID;

    private readonly SyncPlayer PlayerInfo;
    public bool IsPlaying { get; private set; }
    
    // 添加Name属性
    public string Name => PlayerInfo.Name;

    public event Action<DummyPlayer, NetworkText, TrProtocol.Models.Color>? OnChat;
    public event Action<DummyPlayer, string>? OnMessage;
    public Func<bool> shouldExit = () => false;

    private readonly Dictionary<Type, Action<Packet>> handlers = new Dictionary<Type, Action<Packet>>();

    private readonly TrClient client;

    public TSPlayer TSPlayer => TShock.Players[this.PlayerSlot];

    public bool Active { get; private set; }

    private Timer _timer = null!;

    // 添加位置跟踪
    private Microsoft.Xna.Framework.Vector2 _position;
    private Microsoft.Xna.Framework.Vector2 _velocity;

    public Microsoft.Xna.Framework.Vector2 Position => _position;
    public Microsoft.Xna.Framework.Vector2 Velocity => _velocity;

    // 添加移动控制
    private bool _isMovingLeft = false;
    private bool _isMovingRight = false;
    private bool _isJumping = false;
    private DateTime _lastJumpTime = DateTime.MinValue;
    private const float JUMP_DURATION = 0.3f; // 跳跃持续时间0.3秒
    private const float MOVE_SPEED = 3f;
    private const float JUMP_SPEED = -8f;
    private const float GRAVITY = 0.5f; // 重力
    private bool _isOnGround = false; // 是否在地面上
    private float _groundY = 0f; // 动态地面高度

    public DummyPlayer(SyncPlayer playerInfo, string uuid)
    {
        this.PlayerInfo = playerInfo;
        this.UUID = uuid;
        this.client = new TrClient();
        this.InternalOn();
    }

    public void KillServer()
    {
        this.client.KillServer();
    }

    public void Close()
    {
        this.IsPlaying = false;
        this.Active = false;
        this.client.Close();
        this._timer?.Dispose();
    }

    public void SendPacket(Packet packet)
    {
        if (packet is IPlayerSlot ips)
        {
            ips.PlayerSlot = this.PlayerSlot;
        }
        this.client.Send(packet);
    }
    public void Hello(string message)
    {
        this.SendPacket(new ClientHello { Version = message });
    }

    public void TileGetSection(int x, int y)
    {
        this.SendPacket(new RequestTileData { Position = new Position { X = x, Y = y } });
    }

    public void Spawn(short x, short y)
    {
        // 设置初始位置 - 使用世界出生点
        var spawnX = Main.spawnTileX * 16f;
        var spawnY = Main.spawnTileY * 16f;
        
        // 如果指定了坐标，使用指定坐标，否则使用出生点
        if (x != 0 || y != 0)
        {
            _position = new Microsoft.Xna.Framework.Vector2(x * 16f, y * 16f);
        }
        else
        {
            _position = new Microsoft.Xna.Framework.Vector2(spawnX, spawnY);
        }
        
        _velocity = Microsoft.Xna.Framework.Vector2.Zero;
        
        // 计算地面高度
        CalculateGroundY();
        
        this.SendPacket(new SpawnPlayer
        {
            Position = new ShortPosition { X = (short)(_position.X / 16f), Y = (short)(_position.Y / 16f) },
            Context = TrProtocol.Models.PlayerSpawnContext.SpawningIntoWorld
        });
    }

    /// <summary>
    /// 计算地面高度 - 从世界数据获取真实地面
    /// </summary>
    private void CalculateGroundY()
    {
        try
        {
            int tileX = (int)(_position.X / 16f);
            int tileY = (int)(_position.Y / 16f);
            
            // 从当前位置向下搜索地面，确保不会陷入土里
            float groundY = float.MaxValue;
            bool foundGround = false;
            
            // 从当前位置向下搜索，最多搜索100个方块
            for (int y = tileY; y < Math.Min(tileY + 100, Main.maxTilesY); y++)
            {
                if (Main.tile[tileX, y]?.active() == true)
                {
                    groundY = y * 16f;
                    foundGround = true;
                    break;
                }
            }
            
            if (foundGround)
            {
                _groundY = groundY;
            }
            else
            {
                // 如果没找到，使用出生点下方
                _groundY = Main.spawnTileY * 16f + 32f;
            }
        }
        catch
        {
            // 如果出错，使用出生点下方
            _groundY = Main.spawnTileY * 16f + 32f;
        }
    }

    /// <summary>
    /// 设置dummy玩家的位置
    /// </summary>
    public void SetPosition(float x, float y)
    {
        _position = new Microsoft.Xna.Framework.Vector2(x, y);
        // 重新计算地面高度
        CalculateGroundY();
    }

    /// <summary>
    /// 获取dummy玩家的位置
    /// </summary>
    public Microsoft.Xna.Framework.Vector2 GetPosition()
    {
        return _position;
    }

    /// <summary>
    /// 开始持续向左移动
    /// </summary>
    public void StartMovingLeft()
    {
        _isMovingLeft = true;
        _isMovingRight = false;
    }

    /// <summary>
    /// 开始持续向右移动
    /// </summary>
    public void StartMovingRight()
    {
        _isMovingRight = true;
        _isMovingLeft = false;
    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public void StopMoving()
    {
        _isMovingLeft = false;
        _isMovingRight = false;
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    public void Jump()
    {
        if (_isOnGround && !_isJumping)
        {
            _velocity.Y = JUMP_SPEED;
            _isJumping = true;
            _lastJumpTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 物理更新 - 处理重力、移动和碰撞
    /// </summary>
    public void UpdatePhysics()
    {
        // 处理跳跃时间
        if (_isJumping && (DateTime.Now - _lastJumpTime).TotalSeconds > JUMP_DURATION)
        {
            _isJumping = false;
        }

        // 应用重力
        if (!_isJumping)
        {
            _velocity.Y += GRAVITY;
        }

        // 处理水平移动
        if (_isMovingLeft)
        {
            _velocity.X = -MOVE_SPEED;
        }
        else if (_isMovingRight)
        {
            _velocity.X = MOVE_SPEED;
        }
        else
        {
            _velocity.X = 0;
        }

        // 更新位置
        _position += _velocity;

        // 重新计算地面高度（移动时动态更新）
        CalculateGroundY();

        // 改进的地面检测和碰撞处理
        if (_position.Y >= _groundY)
        {
            _position.Y = _groundY;
            _velocity.Y = 0;
            _isOnGround = true;
            _isJumping = false; // 着地时停止跳跃状态
        }
        else
        {
            _isOnGround = false;
        }

        // 限制最大下落速度
        if (_velocity.Y > 10f)
        {
            _velocity.Y = 10f;
        }

        // 边界检查 - 防止掉出世界
        if (_position.X < 0)
        {
            _position.X = 0;
            _velocity.X = 0;
        }
        else if (_position.X > Main.maxTilesX * 16f)
        {
            _position.X = Main.maxTilesX * 16f;
            _velocity.X = 0;
        }

        // 发送更新数据包
        SendPlayerUpdate();
    }

    /// <summary>
    /// 发送玩家更新数据包（简化版本）
    /// </summary>
    private void SendPlayerUpdate()
    {
        if (!this.Active || this.client == null) return;

        byte flag1 = 0;
        if (_isMovingLeft || _isMovingRight) flag1 |= 0b00000001; // 移动标志
        if (_isJumping) flag1 |= 0b00000010; // 跳跃标志

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        
        ushort length = 38 + 3;
        bw.Write(length);
        bw.Write((byte)13);
        
        bw.Write((byte)this.PlayerSlot);
        bw.Write(flag1);
        bw.Write((byte)0); // Flag2
        bw.Write((byte)0); // Flag3
        bw.Write((byte)0); // Flag4
        bw.Write((byte)0); // SelectedItem
        bw.Write(_position.X);
        bw.Write(_position.Y);
        bw.Write(_velocity.X);
        bw.Write(_velocity.Y);
        bw.Write(0f); // PotionOfReturnOriginalUsePosition.X
        bw.Write(0f); // PotionOfReturnOriginalUsePosition.Y
        bw.Write(0f); // PotionOfReturnHomePosition.X
        bw.Write(0f); // PotionOfReturnHomePosition.Y
        
        this.client.SendRaw(ms.ToArray());
    }

    public void SendPlayer(string uuid)
    {
        this.SendPacket(new ClientUUID() { UUID = uuid });
        this.SendPacket(this.PlayerInfo);
        this.SendPacket(new PlayerHealth { StatLifeMax = 100, StatLife = 100 });
        for (byte i = 0; i < 73; ++i)
        {
            this.SendPacket(new SyncEquipment { ItemSlot = i });
        }
    }

    public void ChatText(string message)
    {
        this.SendPacket(new NetTextModuleC2S
        {
            Command = "Say",
            Text = message
        });
    }

    /// <summary>
    /// 发送PlayerUpdate(13号)数据包
    /// 根据Part6.5.1文档，PlayerUpdate包含以下字段：
    /// - PlayerID (1字节)
    /// - Flag1-Flag4 (4个字节，位标志)
    /// - SelectedItem (1字节)
    /// - Position (Vector2, 8字节)
    /// - Velocity (Vector2, 8字节)
    /// - PotionOfReturnOriginalUsePosition (Vector2, 8字节)
    /// - PotionOfReturnHomePosition (Vector2, 8字节)
    /// </summary>
    private void SendPlayerUpdate(byte flag1, byte flag2, byte flag3, byte flag4, byte selectedItem, Microsoft.Xna.Framework.Vector2 velocity)
    {
        if (!this.Active || this.client == null) return;
        
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        
        // 包体长度 = 1(PlayerID)+4(Flags)+1(SelectedItem)+8*4(Vector2) = 1+4+1+32=38
        // 包头: 长度(ushort,小端) + 类型(byte)
        ushort length = 38 + 3; // 包体+包头(2+1)
        bw.Write(length);
        bw.Write((byte)13); // 13号包
        
        // 包体
        bw.Write((byte)this.PlayerSlot); // PlayerID
        bw.Write(flag1); // Flag1 - 移动、跳跃、使用物品等标志
        bw.Write(flag2); // Flag2 - 其他状态标志
        bw.Write(flag3); // Flag3 - 其他状态标志
        bw.Write(flag4); // Flag4 - 其他状态标志
        bw.Write(selectedItem); // SelectedItem - 当前选中的物品
        bw.Write(_position.X); // Position.X - 使用自己的位置
        bw.Write(_position.Y); // Position.Y - 使用自己的位置
        bw.Write(velocity.X); // Velocity.X
        bw.Write(velocity.Y); // Velocity.Y
        bw.Write(0f); // PotionOfReturnOriginalUsePosition.X
        bw.Write(0f); // PotionOfReturnOriginalUsePosition.Y
        bw.Write(0f); // PotionOfReturnHomePosition.X
        bw.Write(0f); // PotionOfReturnHomePosition.Y
        
        this.client.SendRaw(ms.ToArray());
    }

    /// <summary>
    /// 发送PlayerActive(14号)数据包
    /// 确保玩家处于活跃状态
    /// </summary>
    private void SendPlayerActive(bool active)
    {
        if (!this.Active || this.client == null) return;
        
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        
        // 包体长度 = 1(PlayerID)+1(Active) = 2
        // 包头: 长度(ushort,小端) + 类型(byte)
        ushort length = 2 + 3; // 包体+包头(2+1)
        bw.Write(length);
        bw.Write((byte)14); // 14号包
        
        // 包体
        bw.Write((byte)this.PlayerSlot); // PlayerID
        bw.Write(active); // Active状态
        
        this.client.SendRaw(ms.ToArray());
    }

    /// <summary>
    /// 发送PlayerAnimation(41号)数据包
    /// 用于播放玩家动画
    /// </summary>
    private void SendPlayerAnimation(byte animationType)
    {
        if (!this.Active || this.client == null) return;
        
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        
        // 包体长度 = 1(PlayerID)+1(AnimationType) = 2
        // 包头: 长度(ushort,小端) + 类型(byte)
        ushort length = 2 + 3; // 包体+包头(2+1)
        bw.Write(length);
        bw.Write((byte)41); // 41号包
        
        // 包体
        bw.Write((byte)this.PlayerSlot); // PlayerID
        bw.Write(animationType); // 动画类型
        
        this.client.SendRaw(ms.ToArray());
    }

    public void Move(string direction)
    {
        switch (direction.ToLower())
        {
            case "left":
                StartMovingLeft();
                break;
            case "right":
                StartMovingRight();
                break;
            case "stop":
                StopMoving();
                break;
            default:
                return;
        }
        // 发送活跃状态和动画（可选）
        SendPlayerActive(true);
        // 不再直接加速度或位置，移动由UpdatePhysics处理
    }

    public void Use()
    {
        // 使用物品标志: Flag1第2位为true
        // 确保玩家处于活跃状态
        SendPlayerActive(true);
        // 发送使用物品动画
        SendPlayerAnimation(3); // 使用物品动画
        SendPlayerUpdate(0b00000100, 0, 0, 0, 0, Microsoft.Xna.Framework.Vector2.Zero);
    }

    public void On<T>(Action<T> handler) where T : Packet
    {
        void Handler(Packet p) => handler((T) p);

        if (this.handlers.TryGetValue(typeof(T), out var val))
        {
            this.handlers[typeof(T)] = val + Handler;
        }
        else
        {
            this.handlers.Add(typeof(T), Handler);
        }
    }

    private void InternalOn()
    {
        this.On<StatusText>(status => OnChat?.Invoke(this, status.Text, new TrProtocol.Models.Color(255, 255, 255)));
        this.On<NetTextModuleS2C>(text => OnChat?.Invoke(this, text.Text, text.Color));
        this.On<SmartTextMessage>(text => OnChat?.Invoke(this, text.Text, text.Color));
        this.On<Kick>(kick =>
        {
            OnMessage?.Invoke(this, "Kicked : " + kick.Reason);
            this.Close();
        });
        this.On<LoadPlayer>(player =>
        {
            this.PlayerSlot = player.PlayerSlot;
            this.SendPlayer(this.UUID);
            this.SendPacket(new RequestWorldInfo());
        });
        this.On<WorldData>(i =>
        {
            if (!this.IsPlaying)
            {
                this.TileGetSection(i.SpawnX, i.SpawnY);
                this.Spawn(i.SpawnX, i.SpawnY);
                this.IsPlaying = true;
            }
        });
        this.On<StartPlaying>(_ => this.SendPacket(new RequestWorldInfo()));
    }

    public void GameLoop(string host, int port, string password)
    {
        this.client.Connect(host, port);
        this.GameLoopInternal(password);
    }
    public void GameLoop(IPEndPoint endPoint, string password, IPEndPoint? proxy = null)
    {
        this.client.Connect(endPoint, proxy);
        this.GameLoopInternal(password);
    }

    private void GameLoopInternal(string password)
    {
        this.Hello(this.CurRelease);
        this.On<RequestPassword>(_ => this.SendPacket(new SendPassword { Password = password }));
        this.Active = true;
        // 修改定时器间隔为60ms，用于物理更新
        this._timer = new(this.OnFrame, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(60));
        Task.Run(() =>
        {
            while (this.Active && !this.shouldExit())
            {
                var packet = this.client.Receive();
                try
                {
                    if (this.handlers.TryGetValue(packet.GetType(), out var act))
                    {
                        act(packet);
                    }
                }
                catch (Exception e)
                {
                    TShock.Log.ConsoleError($"{this.PlayerInfo.Name} Exception caught when trying to parse packet {packet.Type}\n{e}");
                }
            }
            this.Close();
        });
    }

    private void OnFrame(object? state)
    {
        if (this.Active)
        {
            // 发送活跃状态
            this.SendPacket(new PlayerActive() { PlayerSlot = this.PlayerSlot, Active = true });
            
            // 更新物理（移动、重力等）
            this.UpdatePhysics();
        }
    }

    /// <summary>
    /// 传送假人到指定服务器坐标（像素）
    /// </summary>
    public void TeleportTo(float x, float y)
    {
        _position = new Microsoft.Xna.Framework.Vector2(x, y);
        CalculateGroundY();
        _velocity = Microsoft.Xna.Framework.Vector2.Zero;
        // 立即同步一次位置
        SendPlayerUpdate();
    }
}

