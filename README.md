# Fplayer - TShock Dummy Player Plugin

一个用于TShock服务器的虚拟玩家插件，可以创建和控制dummy玩家（假玩家）。

## 功能特性

- ✅ 创建和管理dummy玩家
- ✅ 控制dummy玩家移动（左、右、停止）
- ✅ 控制dummy玩家跳跃
- ✅ 控制dummy玩家使用物品
- ✅ 让dummy玩家发送聊天消息
- ✅ 自动位置跟踪和更新
- ✅ 完整的动画支持
- ✅ 配置文件自动备份
- ✅ **持续移动系统** - 支持长时间连续移动
- ✅ **智能物理系统** - 重力、跳跃时间控制
- ✅ **实时物理更新** - 60ms间隔的流畅移动

## 安装

1. 下载最新的 `Fplayer.dll` 文件
2. 将文件放入你的TShock服务器的 `ServerPlugins` 文件夹
3. 重启服务器或使用 `/reload` 命令

## 命令

### `/dummy create [name] [password]`
创建一个新的dummy玩家。

**参数:**
- `name` - dummy玩家的名称
- `password` - 服务器密码（如果服务器有密码保护）

**示例:**
```
/dummy create TestBot mypassword
```

### `/dummy action [name] [action] [direction]`
控制dummy玩家执行动作。

**参数:**
- `name` - dummy玩家的名称
- `action` - 动作类型：`move`、`jump`、`use`、`say`
- `direction` - 移动方向（仅用于move动作）：`left`、`right`、`stop`

**示例:**
```
/dummy action TestBot move left    # TestBot向左移动
/dummy action TestBot move right   # TestBot向右移动
/dummy action TestBot move stop    # TestBot停止移动
/dummy action TestBot jump         # TestBot跳跃
/dummy action TestBot use          # TestBot使用物品
/dummy action TestBot say Hello    # TestBot说"Hello"
```

### `/dummy startmove [name] [left|right]`
开始持续移动。

**参数:**
- `name` - dummy玩家的名称
- `left|right` - 移动方向

**示例:**
```
/dummy startmove TestBot left   # 让TestBot持续向左移动
/dummy startmove TestBot right  # 让TestBot持续向右移动
```

### `/dummy stopmove [name]`
停止持续移动。

**参数:**
- `name` - dummy玩家的名称

**示例:**
```
/dummy stopmove TestBot  # 让TestBot停止移动
```

### `/dummy jump [name]`
让dummy玩家跳跃。

**参数:**
- `name` - dummy玩家的名称

**示例:**
```
/dummy jump TestBot  # 让TestBot跳跃
```

### `/dummy speak [name] [message]`
让dummy玩家发送聊天消息。

**参数:**
- `name` - dummy玩家的名称
- `message` - 要发送的消息

**示例:**
```
/dummy speak TestBot Hello World!
```

### `/dummy remove [name]`
移除指定的dummy玩家。

**参数:**
- `name` - dummy玩家的名称

**示例:**
```
/dummy remove TestBot  # 移除TestBot
```

### `/dummy reconnect [name]`
重新连接指定的dummy玩家。

**参数:**
- `name` - dummy玩家的名称

**示例:**
```
/dummy reconnect TestBot  # 重新连接TestBot
```

## 技术特性

### 持续移动系统
- **持续移动**: 使用 `/dummy startmove` 开始持续移动，直到使用 `/dummy stopmove` 停止
- **实时物理更新**: 60ms间隔的流畅移动和物理计算
- **智能跳跃**: 跳跃时间限制为0.3秒，防止过长跳跃
- **重力系统**: 自动应用重力，让移动更自然

### 位置跟踪系统
- dummy玩家现在维护自己的位置信息
- 移动时会实时更新位置坐标
- 支持像素级精确定位

### 完整的数据包支持
- **PlayerUpdate(13)** - 玩家状态更新，包含位置、速度、标志位
- **PlayerActive(14)** - 玩家活跃状态
- **PlayerAnimation(41)** - 玩家动画效果

### 动画系统
- 行走动画 - 移动时播放
- 跳跃动画 - 跳跃时播放
- 使用物品动画 - 使用物品时播放

## 配置文件

插件会在 `tshock` 文件夹下创建 `fplayer.json` 配置文件。

### 自动备份

使用提供的PowerShell脚本自动备份配置文件：

```powershell
# 创建备份脚本
$backupScript = @"
# 创建备份文件夹
`$backupFolder = [Environment]::GetFolderPath('Desktop') + '\FplayerBackups'
if (!(Test-Path `$backupFolder)) {
    New-Item -ItemType Directory -Path `$backupFolder
}

# 复制配置文件
`$sourceFile = 'tshock\fplayer.json'
`$timestamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
`$backupFile = `$backupFolder + '\fplayer_' + `$timestamp + '.json'

if (Test-Path `$sourceFile) {
    Copy-Item `$sourceFile `$backupFile
    Write-Host "配置文件已备份到: `$backupFile"
} else {
    Write-Host "配置文件不存在: `$sourceFile"
}
"@

$backupScript | Out-File -FilePath "backup_fplayer.ps1" -Encoding UTF8
```

运行备份脚本：
```powershell
.\backup_fplayer.ps1
```

## 常见问题

### Q: dummy玩家创建后没有反应？
A: 确保服务器没有密码保护，或者提供了正确的密码。

### Q: 移动命令没有效果？
A: 新版本已经修复了位置跟踪问题，dummy玩家现在会正确更新位置。

### Q: 如何让dummy玩家持续移动？
A: 使用新的持续移动命令：
- `/dummy startmove [name] left` - 开始持续向左移动
- `/dummy startmove [name] right` - 开始持续向右移动
- `/dummy stopmove [name]` - 停止移动

### Q: 跳跃时间太长怎么办？
A: 新版本已将跳跃时间限制为0.3秒，并且有0.5秒的冷却时间。

### Q: 配置文件在哪里？
A: 配置文件位于 `tshock/fplayer.json`，首次运行插件时会自动创建。

## 开发信息

- **框架**: .NET 6.0
- **TShock版本**: 5.0+
- **Terraria版本**: 1.4.4.9
- **协议**: TrProtocol

## 更新日志

### v2.2.0
- ✅ 统一使用名称（name）管理dummy玩家
- ✅ 改进命令一致性，所有命令都使用名称而不是索引
- ✅ 增强dummy列表显示，包含位置信息
- ✅ 添加FindDummyByName辅助方法
- ✅ 改进错误提示信息

### v2.1.0
- ✅ 添加持续移动系统
- ✅ 实现智能物理系统（重力、跳跃时间控制）
- ✅ 实时物理更新（60ms间隔）
- ✅ 新增命令：`startmove`、`stopmove`、`jump`
- ✅ 改进移动流畅度和自然度

### v2.0.0
- ✅ 修复位置跟踪问题
- ✅ 添加完整的数据包支持
- ✅ 实现动画系统
- ✅ 改进移动、跳跃、使用物品功能
- ✅ 添加位置管理API

### v1.0.0
- ✅ 基础dummy玩家创建
- ✅ 基本移动控制
- ✅ 聊天功能
- ✅ 配置文件系统

## 许可证

本项目采用MIT许可证。