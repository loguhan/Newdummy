// 地毯移动和重力应用
// 来源：Player.cs 第13642-13681行

public void CarpetMovement()
{
  bool flag = false;
  if (this.grappling[0] == -1 && this.carpet && !this.canJumpAgain_Cloud && !this.canJumpAgain_Sandstorm && !this.canJumpAgain_Blizzard && !this.canJumpAgain_Fart && !this.canJumpAgain_Sail && !this.canJumpAgain_Unicorn && !this.canJumpAgain_Santank && !this.canJumpAgain_WallOfFleshGoat && !this.canJumpAgain_Basilisk && this.jump == 0 && (double) this.velocity.Y != 0.0 && this.rocketTime == 0 && (double) this.wingTime == 0.0 && !this.mount.Active)
  {
    if (this.controlJump && this.canCarpet)
    {
      this.canCarpet = false;
      this.carpetTime = 300;
    }
    if (this.carpetTime > 0 && this.controlJump)
    {
      this.fallStart = (int) ((double) this.position.Y / 16.0);
      flag = true;
      --this.carpetTime;
      float gravity = this.gravity;
      if ((double) this.gravDir == 1.0 && (double) this.velocity.Y > -(double) gravity)
        this.velocity.Y = (float) -((double) gravity + 9.99999997475243E-07);
      else if ((double) this.gravDir == -1.0 && (double) this.velocity.Y < (double) gravity)
        this.velocity.Y = gravity + 1E-06f;
      this.carpetFrameCounter += 1f + Math.Abs(this.velocity.X * 0.5f);
      if ((double) this.carpetFrameCounter > 8.0)
      {
        this.carpetFrameCounter = 0.0f;
        ++this.carpetFrame;
      }
      if (this.carpetFrame < 0)
        this.carpetFrame = 0;
      if (this.carpetFrame > 5)
        this.carpetFrame = 0;
    }
  }
  if (!flag)
    this.carpetFrame = -1;
  else
    this.slowFall = false;
} 