using System.Net;
using System.Threading.Tasks;
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
    private const float MOVE_SPEED = 6f; // 提高移动速度
    private const float JUMP_SPEED = -8f;
    
    // 优化重力系统参数
    private const float DEFAULT_GRAVITY = 0.4f;
    private const float MAX_FALL_SPEED = 10f;
    private const float WATER_GRAVITY_MULTIPLIER = 0.5f;
    private const float HONEY_GRAVITY_MULTIPLIER = 0.25f;
    
    private float _currentGravity = DEFAULT_GRAVITY;
    private bool _isInWater = false;
    private bool _isInHoney = false;
    private bool _isFalling = false;
    private int _fallStartY = 0;

    // 添加跳跃相关属性
    private bool _jumpBoost = false;
    private bool _empressBrooch = false;
    private bool _frogLegJumpBoost = false;
    private bool _moonLordLegs = false;
    private bool _wereWolf = false;
    private bool _portableStoolInfo = false;
    private bool _sticky = false;
    private bool _dazed = false;
    private float _jumpSpeedBoost = 0f;
    private int _extraFall = 0;
    private float _jumpHeight = 15f; // 默认跳跃高度
    private float _jumpSpeed = 5.01f; // 默认跳跃速度

    // 添加水平移动系统相关属性
    private float _maxRunSpeed = 12f; // 提高最大移动速度
    private float _accRunSpeed = 12f; // 提高加速度
    private float _runAcceleration = 0.8f; // 提高加速度
    private float _runSlowdown = 0.8f; // 提高减速度
    private bool _onWrongGround = false;
    private bool _chilled = false;
    private int _direction = 1; // 玩家朝向：-1为左，1为右
    private bool _controlLeft = false;
    private bool _controlRight = false;
    private bool _controlJump = false;
    private bool _isOnGround = false;
    private float _groundY = 0f; // 地面高度



    // 添加滑轮系统相关属性
    private bool _pulley = false;
    private byte _pulleyDir = 0;
    private float _stepSpeed = 0f;
    private float _gfxOffY = 0f;
    private bool _isUsingStool = false; // 是否正在使用凳子

    // 基础碰撞状态
    private bool _isOnSlope = false;
    private bool _isOnStairs = false;

    // 基础移动状态
    private bool _isWallSliding = false;
    private int _slideDir = 0;

    public DummyPlayer(SyncPlayer playerInfo, string uuid)
    {
        this.PlayerInfo = playerInfo;
        this.UUID = uuid;
        this.client = new TrClient();
        this.InternalOn();
        
        // 初始化跳跃参数
        UpdateJumpHeight();
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
            const float PLAYER_HEIGHT = 48f;
            const float PLAYER_WIDTH = 16f;
            
            // 计算玩家底部位置
            int centerX = (int)((_position.X + PLAYER_WIDTH / 2f) / 16f);
            int bottomY = (int)((_position.Y + PLAYER_HEIGHT) / 16f);
            
            // 从玩家底部向下搜索地面
            float groundY = float.MaxValue;
            bool foundGround = false;
            
            // 从玩家底部向下搜索，最多搜索100个方块
            for (int y = bottomY; y < Math.Min(bottomY + 100, Main.maxTilesY); y++)
            {
                if (Main.tile[centerX, y]?.active() == true)
                {
                    groundY = y * 16f - PLAYER_HEIGHT;
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
                _groundY = Main.spawnTileY * 16f - PLAYER_HEIGHT;
            }
        }
        catch
        {
            // 如果出错，使用出生点下方
            _groundY = Main.spawnTileY * 16f - 48f;
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
        SetMovementControls(true, false);
    }

    /// <summary>
    /// 开始持续向右移动
    /// </summary>
    public void StartMovingRight()
    {
        SetMovementControls(false, true);
    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public void StopMoving()
    {
        SetMovementControls(false, false);
    }

    /// <summary>
    /// 跳跃
    /// </summary>
    public void Jump()
    {
        if (_isOnGround && !_isJumping)
        {
            // 使用计算后的跳跃速度
            _velocity.Y = -_jumpSpeed;
            _isJumping = true;
            _lastJumpTime = DateTime.Now;
            _controlJump = true;
        }
    }

    /// <summary>
    /// 设置便携式凳子状态
    /// </summary>
    public void SetPortableStool(bool isUsing)
    {
        _isUsingStool = isUsing;
        _portableStoolInfo = isUsing;
    }

    /// <summary>
    /// 获取滑轮状态信息
    /// </summary>
    public string GetPulleyStatus()
    {
        if (!_pulley)
            return "不在滑轮上";
        
        return $"滑轮状态: 方向={_pulleyDir}, 速度={_stepSpeed:F1}, 偏移={_gfxOffY:F1}";
    }

    /// <summary>
    /// 获取便携式凳子状态
    /// </summary>
    public bool IsUsingPortableStool()
    {
        return _isUsingStool;
    }

    /// <summary>
    /// 查找滑轮 - 参考Terraria原版逻辑
    /// </summary>
    private void FindPulley()
    {
        // 如果没有使用便携式凳子且没有左右移动，直接返回
        if (!_portableStoolInfo && !_isMovingLeft && !_isMovingRight)
            return;

        // 计算玩家中心位置
        int tileX = (int)(_position.X + 8f) / 16;
        int tileY = (int)(_position.Y + 24f) / 16;

        // 检查当前位置是否有绳子
        if (Main.tile[tileX, tileY] == null || !Main.tile[tileX, tileY].active() || !Main.tileRope[Main.tile[tileX, tileY].type])
            return;

        float ropeY = _position.Y;

        // 确保相邻方块存在
        if (Main.tile[tileX, tileY - 1] == null)
            Main.tile[tileX, tileY - 1] = new Tile();
        if (Main.tile[tileX, tileY + 1] == null)
            Main.tile[tileX, tileY + 1] = new Tile();

        // 检查上下是否有绳子连接
        bool hasUpperRope = Main.tile[tileX, tileY - 1].active() && Main.tileRope[Main.tile[tileX, tileY - 1].type];
        bool hasLowerRope = Main.tile[tileX, tileY + 1].active() && Main.tileRope[Main.tile[tileX, tileY + 1].type];

        if (!hasUpperRope && !hasLowerRope)
            ropeY = tileY * 16f + 22f;

        // 计算三个可能的X位置（左、中、右）
        float centerX = tileX * 16f + 8f - 16f / 2f;
        int leftX = (int)(centerX - 6f);
        int centerPosX = (int)centerX;
        int rightX = (int)(centerX + 6f);

        // 找到最近的X位置
        int closestPos = 1;
        float minDistance = Math.Abs(_position.X - centerPosX);
        
        if (Math.Abs(_position.X - leftX) < minDistance)
        {
            closestPos = 2;
            minDistance = Math.Abs(_position.X - leftX);
        }
        
        if (Math.Abs(_position.X - rightX) < minDistance)
        {
            closestPos = 3;
        }

        float targetX = centerX;
        
        // 根据最近位置设置目标X和方向
        if (closestPos == 1)
        {
            targetX = centerPosX;
            _pulleyDir = 2;
        }
        else if (closestPos == 2)
        {
            targetX = leftX;
            _pulleyDir = 1;
        }
        else if (closestPos == 3)
        {
            targetX = rightX;
            _pulleyDir = 2;
        }

        // 检查目标位置是否有碰撞
        if (!Collision.SolidCollision(new Microsoft.Xna.Framework.Vector2(targetX, _position.Y), 16, 32))
        {
            // 设置滑轮状态
            _pulley = true;
            _position.X = targetX;
            _gfxOffY = _position.Y - ropeY;
            _stepSpeed = 2.5f;
            _position.Y = ropeY;
            _velocity.X = 0f;
        }
        else
        {
            // 尝试其他位置
            float altX1 = centerPosX;
            _pulleyDir = 2;
            
            if (!Collision.SolidCollision(new Microsoft.Xna.Framework.Vector2(altX1, _position.Y), 16, 32))
            {
                _pulley = true;
                _position.X = altX1;
                _gfxOffY = _position.Y - ropeY;
                _stepSpeed = 2.5f;
                _position.Y = ropeY;
                _velocity.X = 0f;
            }
            else
            {
                float altX2 = rightX;
                _pulleyDir = 2;
                
                if (!Collision.SolidCollision(new Microsoft.Xna.Framework.Vector2(altX2, _position.Y), 16, 32))
                {
                    _pulley = true;
                    _position.X = altX2;
                    _gfxOffY = _position.Y - ropeY;
                    _stepSpeed = 2.5f;
                    _position.Y = ropeY;
                    _velocity.X = 0f;
                }
            }
        }
    }

    /// <summary>
    /// 物理更新 - 处理重力、移动和碰撞
    /// </summary>
    private readonly object _physicsLock = new object();
    private bool _physicsUpdateInProgress = false;

    public void UpdatePhysics()
    {
        // 防止重复执行
        if (_physicsUpdateInProgress)
            return;

        lock (_physicsLock)
        {
            if (_physicsUpdateInProgress)
                return;
            _physicsUpdateInProgress = true;
        }

        try
        {
            // 异步执行物理更新
            Task.Run(() => UpdatePhysicsAsync());
        }
        catch (Exception ex)
        {
            // 记录错误但不中断
            Console.WriteLine($"Physics update error: {ex.Message}");
        }
        finally
        {
            lock (_physicsLock)
            {
                _physicsUpdateInProgress = false;
            }
        }
    }

    private void UpdatePhysicsAsync()
    {
        try
        {
            // 处理跳跃时间
            if (_isJumping && (DateTime.Now - _lastJumpTime).TotalSeconds > JUMP_DURATION)
            {
                _isJumping = false;
                _controlJump = false;
            }
            
            // 如果在地面上，重置跳跃状态
            if (_isOnGround)
            {
                _isJumping = false;
                _controlJump = false;
            }

            // 更新重力参数
            UpdateGravityParameters();

            // 检查滑轮系统
            FindPulley();

            // 如果不在滑轮上，应用正常物理
            if (!_pulley)
            {
                // 应用重力（基于Terraria原版逻辑）
                ApplyGravity();

                // 应用水平移动系统
                UpdateHorizontalMovement();

                // 更新特殊移动
                UpdateSpecialMovement();

                // 更新碰撞检测（包含位置更新）
                UpdateCollision();
                
                // 更新地面状态（在位置更新后进行）
                UpdateGroundState();
            }
            else
            {
                // 滑轮模式下的特殊处理
                if (_isMovingLeft || _isMovingRight)
                {
                    // 在滑轮上移动
                    float moveSpeed = _stepSpeed;
                    if (_isMovingLeft)
                        _position.X -= moveSpeed;
                    else if (_isMovingRight)
                        _position.X += moveSpeed;
                }
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
        catch (Exception ex)
        {
            Console.WriteLine($"Async physics update error: {ex.Message}");
        }
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
        this._timer = new(this.OnFrame, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(30)); // 提高更新频率到30ms
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

    /// <summary>
    /// 更新跳跃高度和速度 - 参考Terraria原版逻辑
    /// </summary>
    private void UpdateJumpHeight()
    {
        // 重置跳跃参数
        _jumpHeight = 15f;
        _jumpSpeed = 5.01f;
        _jumpSpeedBoost = 0f;
        _extraFall = 0;

        // 检查各种跳跃加成
        if (_jumpBoost)
        {
            _jumpHeight = 20f;
            _jumpSpeed = 6.51f;
        }
        
        if (_empressBrooch)
            _jumpSpeedBoost += 2.4f;
            
        if (_frogLegJumpBoost)
        {
            _jumpSpeedBoost += 2.4f;
            _extraFall += 15;
        }
        
        if (_moonLordLegs)
        {
            _jumpSpeedBoost += 1.8f;
            _extraFall += 10;
            _jumpHeight += 1f;
        }
        
        if (_wereWolf)
        {
            _jumpHeight += 2f;
            _jumpSpeed += 0.2f;
        }
        
        if (_portableStoolInfo)
            _jumpHeight += 5f;
            
        _jumpSpeed += _jumpSpeedBoost;
        
        // 粘性状态影响
        if (_sticky)
        {
            _jumpHeight /= 10f;
            _jumpSpeed /= 5f;
        }
        
        // 眩晕状态影响
        if (_dazed)
        {
            _jumpHeight /= 5f;
            _jumpSpeed /= 2f;
        }
    }

    /// <summary>
    /// 水平移动系统 - 基于Terraria原版逻辑
    /// </summary>
    private void UpdateHorizontalMovement()
    {
        // 如果被冰冻，重置加速速度
        if (_chilled)
            _accRunSpeed = _maxRunSpeed;

        // 计算平均速度
        float avgSpeed = (_accRunSpeed + _maxRunSpeed) / 2f;
        float windEffect = 0f;
        bool hasWindEffect = false;

        // 处理向左移动
        if (_controlLeft && _velocity.X > -_maxRunSpeed)
        {
            // 在地面上或飞行时应用加速
            if (_isOnGround || _isJumping)
            {
                if (_velocity.X > _runSlowdown)
                    _velocity.X -= _runSlowdown;
                _velocity.X -= _runAcceleration;
            }

            // 错误地面处理
            if (_onWrongGround)
            {
                if (_velocity.X < -_runSlowdown)
                    _velocity.X += _runSlowdown;
                else
                    _velocity.X = 0f;
            }

            // 更新朝向
            _direction = -1;
        }
        // 处理向右移动
        else if (_controlRight && _velocity.X < _maxRunSpeed)
        {
            // 在地面上或飞行时应用加速
            if (_isOnGround || _isJumping)
            {
                if (_velocity.X < -_runSlowdown)
                    _velocity.X += _runSlowdown;
                _velocity.X += _runAcceleration;
            }

            // 错误地面处理
            if (_onWrongGround)
            {
                if (_velocity.X > _runSlowdown)
                    _velocity.X -= _runSlowdown;
                else
                    _velocity.X = 0f;
            }

            // 更新朝向
            _direction = 1;
        }
        // 没有移动输入时的减速处理
        else
        {
            // 自然减速
            if (_velocity.X > 0)
            {
                _velocity.X -= _runSlowdown;
                if (_velocity.X < 0)
                    _velocity.X = 0;
            }
            else if (_velocity.X < 0)
            {
                _velocity.X += _runSlowdown;
                if (_velocity.X > 0)
                    _velocity.X = 0;
            }
        }

        // 限制最大速度
        if (_velocity.X > _maxRunSpeed)
            _velocity.X = _maxRunSpeed;
        else if (_velocity.X < -_maxRunSpeed)
            _velocity.X = -_maxRunSpeed;
    }

    /// <summary>
    /// 设置移动控制状态
    /// </summary>
    public void SetMovementControls(bool left, bool right)
    {
        _controlLeft = left;
        _controlRight = right;
        
        // 更新移动状态标志
        _isMovingLeft = left;
        _isMovingRight = right;
    }

    /// <summary>
    /// 获取移动状态信息
    /// </summary>
    public string GetMovementStatus()
    {
        string direction = _direction == -1 ? "左" : "右";
        string movement = "";
        
        if (_controlLeft && _controlRight)
            movement = "冲突";
        else if (_controlLeft)
            movement = "向左";
        else if (_controlRight)
            movement = "向右";
        else
            movement = "静止";

        return $"移动: {movement}, 朝向: {direction}, 速度: {_velocity.X:F1}, 最大速度: {_maxRunSpeed:F1}";
    }



    /// <summary>
    /// 更新移动参数 - 基于状态
    /// </summary>
    private void UpdateMovementParameters()
    {
        // 基础移动参数
        _maxRunSpeed = 6f;
        _accRunSpeed = 6f;
        _runAcceleration = 0.4f;
        _runSlowdown = 0.4f;

        // 状态影响
        if (_chilled)
        {
            _maxRunSpeed *= 0.5f;
            _accRunSpeed *= 0.5f;
            _runAcceleration *= 0.5f;
        }
    }

    /// <summary>
    /// 应用重力 - 基于Terraria原版逻辑
    /// </summary>
    private void ApplyGravity()
    {
        // 更新重力参数
        UpdateGravityParameters();
        
        // 基于Terraria原版逻辑应用重力
        // 重力应该始终应用，除非在地面上且没有跳跃
        if (!_isOnGround || _isJumping)
        {
            _velocity.Y += _currentGravity;
        }
        
        // 限制最大下落速度
        if (_velocity.Y > MAX_FALL_SPEED)
        {
            _velocity.Y = MAX_FALL_SPEED;
        }
        
        // 如果在地面上且速度很小，重置速度
        if (_isOnGround && Math.Abs(_velocity.Y) < 0.1f)
        {
            _velocity.Y = 0;
        }
    }

    /// <summary>
    /// 更新重力参数 - 基于Terraria原版逻辑
    /// </summary>
    private void UpdateGravityParameters()
    {
        // 重置重力
        _currentGravity = DEFAULT_GRAVITY;

        // 水中重力影响
        if (_isInWater)
        {
            _currentGravity *= WATER_GRAVITY_MULTIPLIER;
        }

        // 蜂蜜重力影响
        if (_isInHoney)
        {
            _currentGravity *= HONEY_GRAVITY_MULTIPLIER;
        }

        // 冰冻状态影响
        if (_chilled)
        {
            _currentGravity *= 0.5f;
        }
    }

    /// <summary>
    /// 设置环境状态
    /// </summary>
    public void SetEnvironmentState(bool inWater, bool inHoney)
    {
        _isInWater = inWater;
        _isInHoney = inHoney;
        UpdateGravityParameters();
    }

    /// <summary>
    /// 获取重力状态信息
    /// </summary>
    public string GetGravityStatus()
    {
        string environment = "";
        if (_isInWater) environment += "水中 ";
        if (_isInHoney) environment += "蜂蜜中 ";
        if (_chilled) environment += "冰冻 ";
        
        return $"重力: {_currentGravity:F2}, 环境: {environment}, 下落速度: {_velocity.Y:F1}";
    }

    /// <summary>
    /// 优化碰撞检测 - 简化版本
    /// </summary>
    private void UpdateCollision()
    {
        // 检测当前环境
        DetectEnvironment();
        
        // 应用重力影响
        if (_isInWater)
        {
            _velocity.Y *= WATER_GRAVITY_MULTIPLIER;
        }
        else if (_isInHoney)
        {
            _velocity.Y *= HONEY_GRAVITY_MULTIPLIER;
        }
        
        // 墙壁碰撞检测
        CheckWallCollision();
        
        // 基于Terraria原版的干燥地面碰撞处理
        DryCollision();
        
        // 更新位置
        _position += _velocity;
    }

    /// <summary>
    /// 墙壁碰撞检测
    /// </summary>
    private void CheckWallCollision()
    {
        const float PLAYER_HEIGHT = 48f;
        const float PLAYER_WIDTH = 16f;
        
        // 计算玩家碰撞箱的边界
        float playerLeft = _position.X;
        float playerRight = _position.X + PLAYER_WIDTH;
        float playerTop = _position.Y;
        float playerBottom = _position.Y + PLAYER_HEIGHT;
        
        // 计算移动后的位置
        float newLeft = playerLeft + _velocity.X;
        float newRight = playerRight + _velocity.X;
        
        // 检查水平移动是否会导致碰撞
        if (_velocity.X != 0)
        {
            // 检查移动方向上的墙壁
            int checkX = _velocity.X > 0 ? 
                (int)(newRight / 16f) : 
                (int)(newLeft / 16f);
            
            // 检查玩家高度范围内的所有方块
            int startY = (int)(playerTop / 16f);
            int endY = (int)(playerBottom / 16f);
            
            bool hasWallCollision = false;
            
            for (int y = startY; y <= endY; y++)
            {
                if (WorldGen.SolidTile(checkX, y))
                {
                    hasWallCollision = true;
                    break;
                }
            }
            
            if (hasWallCollision)
            {
                // 阻止水平移动
                _velocity.X = 0;
            }
        }
        
        // 检查垂直移动是否会导致碰撞
        if (_velocity.Y != 0)
        {
            // 检查移动方向上的方块
            int checkY = _velocity.Y > 0 ? 
                (int)(playerBottom / 16f) : 
                (int)(playerTop / 16f);
            
            // 检查玩家宽度范围内的所有方块
            int startX = (int)(playerLeft / 16f);
            int endX = (int)(playerRight / 16f);
            
            bool hasVerticalCollision = false;
            
            for (int x = startX; x <= endX; x++)
            {
                if (WorldGen.SolidTile(x, checkY))
                {
                    hasVerticalCollision = true;
                    break;
                }
            }
            
            if (hasVerticalCollision)
            {
                // 阻止垂直移动
                _velocity.Y = 0;
            }
        }
    }

    /// <summary>
    /// 基于Terraria原版的干燥地面碰撞处理
    /// </summary>
    private void DryCollision()
    {
        const float PLAYER_HEIGHT = 48f;
        const float PLAYER_WIDTH = 16f;
        
        // 计算玩家底部位置（基于Terraria原版逻辑）
        int centerX = (int)((_position.X + PLAYER_WIDTH / 2f) / 16f);
        int bottomY = (int)((_position.Y + PLAYER_HEIGHT) / 16f);
        
        // 检查地面方块（检查玩家宽度范围内的所有方块）
        bool hasGround = false;
        float groundY = 0f;
        
        // 检查玩家宽度范围内的所有方块
        for (int x = -1; x <= 1; x++) // 检查中心点及其左右
        {
            int checkX = centerX + x;
            if (WorldGen.SolidTile(checkX, bottomY))
            {
                hasGround = true;
                groundY = bottomY * 16f - PLAYER_HEIGHT;
                break;
            }
        }
        
        if (hasGround)
        {
            // 如果玩家位置低于地面，调整到地面
            if (_position.Y >= groundY)
            {
                _position.Y = groundY;
                _velocity.Y = 0;
                _isOnGround = true;
                _isJumping = false;
            }
        }
        else
        {
            // 尝试自动上坡
            if (TryAutoClimb())
            {
                _isOnGround = true;
                _isJumping = false;
            }
            else
            {
                _isOnGround = false;
            }
        }
    }

    /// <summary>
    /// 尝试自动上坡
    /// </summary>
    private bool TryAutoClimb()
    {
        const float PLAYER_HEIGHT = 48f;
        const float PLAYER_WIDTH = 16f;
        
        // 检查移动方向
        int checkDir = 0;
        if (_controlRight) checkDir = 1;
        else if (_controlLeft) checkDir = -1;
        else return false; // 没有移动，不尝试上坡
        
        // 计算检查位置
        float checkX = _position.X + (checkDir * PLAYER_WIDTH);
        float checkY = _position.Y + PLAYER_HEIGHT;
        
        // 检查前方是否有可上坡的台阶
        for (int step = 1; step <= 2; step++) // 检查1格和2格高度
        {
            float stepHeight = step * 16f;
            float stepCheckY = checkY - stepHeight;
            
            int tileX = (int)(checkX / 16f);
            int tileY = (int)(stepCheckY / 16f);
            
            // 检查台阶是否可通行
            if (IsStepPassable(tileX, tileY, step))
            {
                // 计算新的Y位置
                float newY = stepCheckY - PLAYER_HEIGHT;
                
                // 检查新位置是否有足够的空间
                if (HasEnoughSpace(tileX, newY))
                {
                    // 执行上坡
                    _position.Y = newY;
                    _velocity.Y = 0;
                    _isOnSlope = true; // 标记为在斜坡上
                    return true;
                }
            }
        }
        
        _isOnSlope = false; // 不在斜坡上
        return false;
    }

    /// <summary>
    /// 检查台阶是否可通行
    /// </summary>
    private bool IsStepPassable(int tileX, int tileY, int stepHeight)
    {
        // 检查台阶本身是否有方块
        if (!WorldGen.SolidTile(tileX, tileY))
            return false;
            
        // 检查台阶上方是否有空间（玩家需要站在台阶上）
        if (WorldGen.SolidTile(tileX, tileY - 1))
            return false;
            
        // 检查台阶前方是否有空间（避免卡在墙里）
        int frontTileX = tileX + (_direction > 0 ? 1 : -1);
        if (WorldGen.SolidTile(frontTileX, tileY - 1))
            return false;
            
        // 检查台阶上方2格是否有空间（确保玩家有足够高度）
        if (WorldGen.SolidTile(tileX, tileY - 2))
            return false;
            
        return true;
    }

    /// <summary>
    /// 检查是否有足够的空间
    /// </summary>
    private bool HasEnoughSpace(int tileX, float playerY)
    {
        const float PLAYER_HEIGHT = 48f;
        const float PLAYER_WIDTH = 16f;
        
        // 检查玩家碰撞箱是否有足够的空间
        int startTileY = (int)(playerY / 16f);
        int endTileY = (int)((playerY + PLAYER_HEIGHT) / 16f);
        
        // 检查玩家宽度范围内的所有方块
        for (int x = 0; x < 2; x++) // 玩家宽度为16像素，约等于1格
        {
            int checkTileX = tileX + x;
            
            for (int y = startTileY; y <= endTileY; y++)
            {
                if (WorldGen.SolidTile(checkTileX, y))
                    return false;
            }
        }
        
        // 额外检查：确保玩家不会卡在方块之间
        int playerCenterTileX = (int)((playerY + PLAYER_HEIGHT / 2f) / 16f);
        if (WorldGen.SolidTile(tileX, playerCenterTileX))
            return false;
            
        return true;
    }

    /// <summary>
    /// 检测环境状态
    /// </summary>
    private void DetectEnvironment()
    {
        int tileX = (int)(_position.X / 16f);
        int tileY = (int)(_position.Y / 16f);
        
        // 检测是否在水中
        _isInWater = IsTileWater(tileX, tileY);
        
        // 检测是否在蜂蜜中
        _isInHoney = IsTileHoney(tileX, tileY);
    }

    /// <summary>
    /// 检测是否为水方块
    /// </summary>
    private bool IsTileWater(int x, int y)
    {
        return Main.tile[x, y]?.liquid > 0 && Main.tile[x, y].liquidType() == 0;
    }

    /// <summary>
    /// 检测是否为蜂蜜方块
    /// </summary>
    private bool IsTileHoney(int x, int y)
    {
        return Main.tile[x, y]?.liquid > 0 && Main.tile[x, y].liquidType() == 2;
    }

    /// <summary>
    /// 更新特殊移动 - 基于Terraria原版逻辑
    /// </summary>
    private void UpdateSpecialMovement()
    {
        // 更新墙壁滑行
        UpdateWallSlide();
        
        // 更新下落状态
        UpdateFallState();
    }

    /// <summary>
    /// 墙壁滑行 - 基于Terraria原版WallslideMovement
    /// </summary>
    private void UpdateWallSlide()
    {
        _isWallSliding = false;
        
        if (_slideDir == 0 || _pulley)
            return;
            
        bool canSlide = false;
        float checkX = _position.X;
        
        if (_slideDir == 1)
            checkX += 16f;
            
        float tileX = (checkX + _slideDir) / 16f;
        float tileY = (_position.Y + 48f) / 16f; // 使用正确的玩家高度
        
        // 检测墙壁
        if (WorldGen.SolidTile((int)tileX, (int)tileY) && WorldGen.SolidTile((int)tileX, (int)tileY - 1))
            canSlide = true;
            
        if (!canSlide || (_velocity.Y <= 0 && _velocity.Y >= _currentGravity))
            return;
            
        // 开始滑行
        _isWallSliding = true;
        _fallStartY = (int)(_position.Y / 16f);
        
        if (_controlLeft || _controlRight)
        {
            _velocity.Y = 4f;
        }
        else
        {
            _velocity.Y = 0.5f;
        }
    }



    /// <summary>
    /// 更新下落状态
    /// </summary>
    private void UpdateFallState()
    {
        if (_velocity.Y > 0)
        {
            _isFalling = true;
            if (_fallStartY == 0)
                _fallStartY = (int)(_position.Y / 16f);
        }
        else
        {
            _isFalling = false;
        }
    }





    /// <summary>
    /// 获取特殊移动状态
    /// </summary>
    public string GetSpecialMovementStatus()
    {
        string status = "";
        
        if (_isWallSliding)
            status += "墙壁滑行 ";
        if (_isFalling)
            status += "下落中 ";
        if (_isOnSlope)
            status += "在斜坡上 ";
            
        return $"特殊移动: {status}, 滑行方向: {_slideDir}";
    }

    /// <summary>
    /// 获取地面坐标状态信息
    /// </summary>
    public string GetGroundStatus()
    {
        const float PLAYER_HEIGHT = 48f;
        const float PLAYER_WIDTH = 16f;
        
        // 计算玩家底部位置
        int centerX = (int)((_position.X + PLAYER_WIDTH / 2f) / 16f);
        int bottomY = (int)((_position.Y + PLAYER_HEIGHT) / 16f);
        
        // 检查地面方块
        bool hasGround = false;
        float groundY = 0f;
        
        for (int x = -1; x <= 1; x++)
        {
            int checkX = centerX + x;
            if (WorldGen.SolidTile(checkX, bottomY))
            {
                hasGround = true;
                groundY = bottomY * 16f - PLAYER_HEIGHT;
                break;
            }
        }
        
        return $"位置: ({_position.X:F1}, {_position.Y:F1}), 中心X: {centerX}, 底部Y: {bottomY}, 有地面: {hasGround}, 地面Y: {groundY:F1}, 地面状态: {_isOnGround}, 跳跃状态: {_isJumping}, 速度Y: {_velocity.Y:F2}";
    }

    /// <summary>
    /// 获取上坡状态
    /// </summary>
    public string GetClimbStatus()
    {
        const float PLAYER_HEIGHT = 48f;
        const float PLAYER_WIDTH = 16f;
        
        // 检查当前是否可以上坡
        bool canClimb1 = false;
        bool canClimb2 = false;
        
        if (_controlRight || _controlLeft)
        {
            int checkDir = _controlRight ? 1 : -1;
            float checkX = _position.X + (checkDir * PLAYER_WIDTH);
            float checkY = _position.Y + PLAYER_HEIGHT;
            
            // 检查1格高度
            float step1Y = checkY - 16f;
            int tileX1 = (int)(checkX / 16f);
            int tileY1 = (int)(step1Y / 16f);
            canClimb1 = IsStepPassable(tileX1, tileY1, 1) && HasEnoughSpace(tileX1, step1Y - PLAYER_HEIGHT);
            
            // 检查2格高度
            float step2Y = checkY - 32f;
            int tileX2 = (int)(checkX / 16f);
            int tileY2 = (int)(step2Y / 16f);
            canClimb2 = IsStepPassable(tileX2, tileY2, 2) && HasEnoughSpace(tileX2, step2Y - PLAYER_HEIGHT);
        }
        
        return $"上坡状态: 1格可上坡={canClimb1}, 2格可上坡={canClimb2}, 移动方向={(_controlRight ? "右" : _controlLeft ? "左" : "无")}";
    }

    /// <summary>
    /// 基于Terraria原版的地面状态检测
    /// </summary>
    private void UpdateGroundState()
    {
        const float PLAYER_HEIGHT = 48f;
        const float PLAYER_WIDTH = 16f;
        
        // 基于Terraria原版的FloorVisuals逻辑
        int centerX = (int)((_position.X + PLAYER_WIDTH / 2f) / 16f);
        int bottomY = (int)((_position.Y + PLAYER_HEIGHT) / 16f);
        
        // 检查地面方块（检查玩家宽度范围内的所有方块）
        bool hasGround = false;
        
        for (int x = -1; x <= 1; x++) // 检查中心点及其左右
        {
            int checkX = centerX + x;
            if (WorldGen.SolidTile(checkX, bottomY))
            {
                hasGround = true;
                break;
            }
        }
        
        // 更新地面状态
        if (hasGround)
        {
            _isOnGround = true;
            // 只有在地面上且没有跳跃时才重置跳跃状态
            if (!_isJumping)
            {
                _controlJump = false;
            }
        }
        else
        {
            _isOnGround = false;
        }
        
        // 调试信息
        Console.WriteLine($"地面检测: 位置({_position.X:F1}, {_position.Y:F1}), 中心X={centerX}, 底部Y={bottomY}, 有地面={hasGround}, 地面状态={_isOnGround}, 跳跃状态={_isJumping}, 速度Y={_velocity.Y:F2}");
    }
}

