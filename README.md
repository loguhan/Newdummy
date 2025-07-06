# Fplayer - TShock Dummy Player Plugin

一个用于TShock服务器的虚拟玩家插件，可以创建和控制dummy玩家（假玩家）。

---

## 功能特性

- ✅ 创建和管理dummy玩家
- ✅ 控制dummy玩家移动（左、右、停止）
- ✅ 控制dummy玩家跳跃
- ✅ 控制dummy玩家使用物品
- ✅ 让dummy玩家发送聊天消息
- ✅ 自动位置跟踪和更新
- ✅ **持续移动系统** - 支持长时间连续移动
- ✅ **智能物理系统** - 重力、跳跃时间控制
- ✅ **实时物理更新** - 60ms间隔的流畅移动
- ✅ **传送功能** - 将假人传送到指定玩家身边

---

## 命令说明

### `/dummy create [name]`
创建一个新的dummy玩家。

- `name` - dummy玩家的名称

**示例：**
```
/dummy create MyDummy
```

### `/dummy tp [name] [player]`
将假人传送到指定玩家身边。

- `name` - dummy玩家的名称
- `player` - 目标玩家名称

**示例：**
```
/dummy tp MyDummy PlayerName
```

### `/dummy action [name] [action] [direction]`
控制dummy玩家执行动作。

- `name` - dummy玩家的名称
- `action` - 动作类型：`move`、`jump`、`use`、`say`
- `direction` - 移动方向（仅用于move动作）：`left`、`right`、`stop`

**示例：**
```
/dummy action MyDummy move left    # 向左移动
/dummy action MyDummy move right   # 向右移动
/dummy action MyDummy move stop    # 停止移动
/dummy action MyDummy jump         # 跳跃
/dummy action MyDummy use          # 使用物品
/dummy action MyDummy say Hello    # 说"Hello"
```

### `/dummy startmove [name] [left|right]`
开始持续移动。

- `name` - dummy玩家的名称
- `left|right` - 移动方向

**示例：**
```
/dummy startmove MyDummy left   # 持续向左移动
/dummy startmove MyDummy right  # 持续向右移动
```

### `/dummy stopmove [name]`
停止持续移动。

- `name` - dummy玩家的名称

**示例：**
```
/dummy stopmove MyDummy  # 停止移动
```

### `/dummy jump [name]`
让dummy玩家跳跃。

- `name` - dummy玩家的名称

**示例：**
```
/dummy jump MyDummy  # 跳跃
```

### `/dummy speak [name] [message]`
让dummy玩家发送聊天消息。

- `name` - dummy玩家的名称
- `message` - 要发送的消息

**示例：**
```
/dummy speak MyDummy Hello World!
```

### `/dummy remove [name]`
移除指定的dummy玩家。

- `name` - dummy玩家的名称

**示例：**
```
/dummy remove MyDummy  # 移除MyDummy
```

### `/dummy reconnect [name]`
重新连接指定的dummy玩家。

- `name` - dummy玩家的名称

**示例：**
```
/dummy reconnect MyDummy  # 重新连接MyDummy
```

### `/dummy list`
显示所有假人列表。

**示例：**
```
/dummy list
```

---


### 数据包支持
- **PlayerUpdate(13)** - 玩家状态更新（位置、速度、标志位）
- **PlayerActive(14)** - 玩家活跃状态
- **PlayerAnimation(41)** - 动画效果

### 动画系统
- 行走、跳跃、使用物品均有动画

---

## 配置文件与备份

插件会在 `tshock` 文件夹下创建 `fplayer.json` 配置文件。

### 自动备份脚本

可用PowerShell脚本自动备份配置文件：

## 常见问题

- **Q: dummy玩家创建后没有反应？**
  - A: 确保服务器没有密码保护，或提供了正确的密码。
- **Q: 如何让dummy玩家持续移动？**
  - A: 使用 `/dummy startmove [name] left|right` 开始，`/dummy stopmove [name]` 停止。
- **Q: 配置文件在哪里？**
  - A: `tshock/fplayer.json`，首次运行插件时自动创建。

---

## 开发信息

- **框架**: .NET 6.0
- **TShock版本**: 5.0+
- **Terraria版本**: 1.4.4.9
- **协议**: TrProtocol

---

## 许可证

本项目采用MIT许可证。

---

> 项目地址：https://github.com/loguhan/Newdummy
