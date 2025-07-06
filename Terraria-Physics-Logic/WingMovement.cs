// 翅膀飞行和重力控制
// 来源：Player.cs 第13842-13900行

public void WingMovement()
{
  if (this.wingsLogic == 4 && this.TryingToHoverUp)
  {
    this.velocity.Y -= 0.2f * this.gravDir;
    if ((double) this.gravDir == 1.0)
    {
      if ((double) this.velocity.Y > 0.0)
        --this.velocity.Y;
      else if ((double) this.velocity.Y > -(double) Player.jumpSpeed)
        this.velocity.Y -= 0.2f;
      if ((double) this.velocity.Y < -(double) Player.jumpSpeed * 3.0)
        this.velocity.Y = (float) (-(double) Player.jumpSpeed * 3.0);
    }
    else
    {
      if ((double) this.velocity.Y < 0.0)
        ++this.velocity.Y;
      else if ((double) this.velocity.Y < (double) Player.jumpSpeed)
        this.velocity.Y += 0.2f;
      if ((double) this.velocity.Y > (double) Player.jumpSpeed * 3.0)
        this.velocity.Y = Player.jumpSpeed * 3f;
    }
    this.wingTime -= 2f;
  }
  else
  {
    float num1 = 0.1f;
    float num2 = 0.5f;
    float num3 = 1.5f;
    float num4 = 0.5f;
    float num5 = 0.1f;
    if (this.wingsLogic == 26)
    {
      num2 = 0.75f;
      num5 = 0.15f;
      num4 = 1f;
      num3 = 2.5f;
      num1 = 0.125f;
    }
    if (this.wingsLogic == 8 || this.wingsLogic == 11 || this.wingsLogic == 24 || this.wingsLogic == 27 || this.wingsLogic == 22)
      num3 = 1.66f;
    if (this.wingsLogic == 21 || this.wingsLogic == 12 || this.wingsLogic == 20 || this.wingsLogic == 23)
      num3 = 1.805f;
    if (this.wingsLogic == 37)
    {
      num2 = 0.75f;
      num5 = 0.15f;
      num4 = 1f;
      num3 = 2.5f;
      num1 = 0.125f;
    }
    if (this.wingsLogic == 44)
    {
      num2 = 0.85f;
      num5 = 0.15f;
      num4 = 1f;
      num3 = 2.75f;
      num1 = 0.125f;
      if (this.TryingToHoverUp)
      {
        this.velocity.Y -= 0.4f * this.gravDir;
        if ((double) this.gravDir == 1.0)
        {
          if ((double) this.velocity.Y > 0.0)
            --this.velocity.Y;
          else if ((double) this.velocity.Y > -(double) Player.jumpSpeed)
            this.velocity.Y -= 0.2f;
          if ((double) this.velocity.Y < -(double) Player.jumpSpeed * 3.0)
            this.velocity.Y = (float) (-(double) Player.jumpSpeed * 3.0);
        }
        else
        {
          if ((double) this.velocity.Y < 0.0)
            ++this.velocity.Y;
          else if ((double) this.velocity.Y < (double) Player.jumpSpeed)
            this.velocity.Y += 0.2f;
          if ((double) this.velocity.Y > (double) Player.jumpSpeed * 3.0)
            this.velocity.Y = Player.jumpSpeed * 3f;
        }
      }
      if (this.TryingToHoverDown && !this.controlJump && (double) this.velocity.Y != 0.0)
        this.velocity.Y += 0.4f;
    }
    if (this.wingsLogic == 45)
    {
      num2 = 0.95f;
      num5 = 0.15f;
      num4 = 1f;
      num3 = 4.5f;
      if (this.TryingToHoverUp)
      {
        this.velocity.Y -= 0.4f * this.gravDir;
        if ((double) this.gravDir == 1.0)
        {
          if ((double) this.velocity.Y > 0.0)
            --this.velocity.Y;
          else if ((double) this.velocity.Y > -(double) Player.jumpSpeed)
            this.velocity.Y -= 0.2f;
          if ((double) this.velocity.Y < -(double) Player.jumpSpeed * 3.0)
            this.velocity.Y = (float) (-(double) Player.jumpSpeed * 3.0);
        }
        else
        {
          if ((double) this.velocity.Y < 0.0)
            ++this.velocity.Y;
          else if ((double) this.velocity.Y < (double) Player.jumpSpeed)
            this.velocity.Y += 0.2f;
          if ((double) this.velocity.Y > (double) Player.jumpSpeed * 3.0)
            this.velocity.Y = Player.jumpSpeed * 3f;
        }
      }
      if (this.TryingToHoverDown && !this.controlJump && (double) this.velocity.Y != 0.0)
        this.velocity.Y += 0.4f;
    }
    if (this.wingsLogic == 29 || this.wingsLogic == 32)
    {
      num2 = 0.85f;
      num5 = 0.15f;
      num4 = 1f;
      num3 = 3f;
      num1 = 0.135f;
    }
    if (this.wingsLogic == 30 || this.wingsLogic == 31)
    {
      num4 = 1f;
      num3 = 2.45f;
      if (!this.TryingToHoverDown)
        num1 = 0.15f;
    }
    this.velocity.Y -= num1 * this.gravDir;
    if ((double) this.gravDir == 1.0)
    {
      if ((double) this.velocity.Y > 0.0)
        this.velocity.Y -= num2;
      else if ((double) this.velocity.Y > -(double) Player.jumpSpeed * (double) num4)
        this.velocity.Y -= num5;
      if ((double) this.velocity.Y < -(double) Player.jumpSpeed * (double) num3)
        this.velocity.Y = -Player.jumpSpeed * num3;
    }
    else
    {
      if ((double) this.velocity.Y < 0.0)
        this.velocity.Y += num2;
      else if ((double) this.velocity.Y < (double) Player.jumpSpeed * (double) num4)
        this.velocity.Y += num5;
      if ((double) this.velocity.Y > (double) Player.jumpSpeed * (double) num3)
        this.velocity.Y = Player.jumpSpeed * num3;
    }
    if ((this.wingsLogic == 22 || this.wingsLogic == 28 || this.wingsLogic == 30 || this.wingsLogic == 31 || this.wingsLogic == 37 || this.wingsLogic == 45) && this.TryingToHoverDown && !this.controlLeft && !this.controlRight)
      this.wingTime -= 0.5f;
    else
      --this.wingTime;
  }
  if (!this.empressBrooch || (double) this.wingTime == 0.0)
    return;
  this.wingTime = (float) this.wingTimeMax;
} 