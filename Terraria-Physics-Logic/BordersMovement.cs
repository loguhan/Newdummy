// 世界边界碰撞处理
// 来源：Player.cs 第15168-15195行

public void BordersMovement()
{
  if ((double) this.position.X < (double) Main.leftWorld + 640.0 + 16.0)
  {
    Main.cameraX = 0.0f;
    this.position.X = (float) ((double) Main.leftWorld + 640.0 + 16.0);
    this.velocity.X = 0.0f;
  }
  if ((double) this.position.X + (double) this.width > (double) Main.rightWorld - 640.0 - 32.0)
  {
    Main.cameraX = 0.0f;
    this.position.X = (float) ((double) Main.rightWorld - 640.0 - 32.0) - (float) this.width;
    this.velocity.X = 0.0f;
  }
  if ((double) this.position.Y < (double) Main.topWorld + 640.0 + 16.0)
  {
    this.position.Y = (float) ((double) Main.topWorld + 640.0 + 16.0);
    if ((double) this.velocity.Y < 0.11)
      this.velocity.Y = 0.11f;
    this.gravDir = 1f;
    AchievementsHelper.HandleSpecialEvent(this, 11);
  }
  if ((double) this.position.Y > (double) Main.bottomWorld - 640.0 - 32.0 - (double) this.height)
  {
    this.position.Y = (float) ((double) Main.bottomWorld - 640.0 - 32.0) - (float) this.height;
    this.velocity.Y = 0.0f;
  }
  if ((double) this.position.Y <= (double) Main.bottomWorld - 640.0 - 150.0 - (double) this.height)
    return;
  AchievementsHelper.HandleSpecialEvent(this, 10);
} 