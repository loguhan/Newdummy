// 干燥地面碰撞处理
// 来源：Player.cs 第14867-14943行

public void DryCollision(bool fallThrough, bool ignorePlats)
{
  int Height = !this.onTrack ? this.height : this.height - 10;
  if ((double) this.velocity.Length() > 16.0)
  {
    Vector2 vector2_1 = Collision.TileCollision(this.position, this.velocity, this.width, Height, fallThrough, ignorePlats, (int) this.gravDir);
    float num1 = this.velocity.Length();
    Vector2 vector2_2 = Vector2.Normalize(this.velocity);
    if ((double) vector2_1.Y == 0.0)
      vector2_2.Y = 0.0f;
    Vector2 zero1 = Vector2.Zero;
    bool flag = this.mount.Type == 7 || this.mount.Type == 8 || this.mount.Type == 12 || this.mount.Type == 44 || this.mount.Type == 48 || this.mount.Type == 49;
    Vector2 zero2 = Vector2.Zero;
    while ((double) num1 > 0.0)
    {
      float num2 = num1;
      if ((double) num2 > 16.0)
        num2 = 16f;
      num1 -= num2;
      Vector2 velocity1 = vector2_2 * num2;
      this.velocity = velocity1;
      this.SlopeDownMovement();
      velocity1 = this.velocity;
      if ((double) this.velocity.Y == (double) this.gravity && (!this.mount.Active || !this.mount.Cart && !flag))
        Collision.StepDown(ref this.position, ref velocity1, this.width, this.height, ref this.stepSpeed, ref this.gfxOffY, (int) this.gravDir, this.waterWalk || this.waterWalk2);
      if ((double) this.gravDir == -1.0)
      {
        if ((this.carpetFrame != -1 || (double) this.velocity.Y <= (double) this.gravity) && !this.controlUp)
          Collision.StepUp(ref this.position, ref velocity1, this.width, this.height, ref this.stepSpeed, ref this.gfxOffY, (int) this.gravDir, this.controlUp);
      }
      else if (flag || (this.carpetFrame != -1 || (double) this.velocity.Y >= (double) this.gravity) && !this.controlDown && !this.mount.Cart)
        Collision.StepUp(ref this.position, ref velocity1, this.width, this.height, ref this.stepSpeed, ref this.gfxOffY, (int) this.gravDir, this.controlUp);
      Vector2 Velocity = Collision.TileCollision(this.position, velocity1, this.width, Height, fallThrough, ignorePlats, (int) this.gravDir);
      if (Collision.up && (double) this.gravDir == 1.0)
        this.jump = 0;
      if (this.waterWalk || this.waterWalk2)
      {
        Vector2 velocity2 = this.velocity;
        Velocity = Collision.WaterCollision(this.position, Velocity, this.width, this.height, fallThrough, lavaWalk: this.waterWalk);
        Vector2 velocity3 = this.velocity;
        if (velocity2 != velocity3)
          this.fallStart = (int) ((double) this.position.Y / 16.0);
      }
      this.position = this.position + Velocity;
      bool Falling = false;
      if ((double) Velocity.Y > (double) this.gravity)
        Falling = true;
      if ((double) Velocity.Y < -(double) this.gravity)
        Falling = true;
      this.velocity = Velocity;
      this.UpdateTouchingTiles();
      this.TryBouncingBlocks(Falling);
      this.TryLandingOnDetonator();
      this.SlopingCollision(fallThrough, ignorePlats);
      Collision.StepConveyorBelt((Entity) this, this.gravDir);
      Vector2 velocity4 = this.velocity;
      zero1 += velocity4;
    }
    this.velocity = zero1;
  }
  else
  {
    this.velocity = Collision.TileCollision(this.position, this.velocity, this.width, Height, fallThrough, ignorePlats, (int) this.gravDir);
    if (Collision.up && (double) this.gravDir == 1.0)
      this.jump = 0;
    if (this.waterWalk || this.waterWalk2)
    {
      Vector2 velocity5 = this.velocity;
      this.velocity = Collision.WaterCollision(this.position, this.velocity, this.width, this.height, fallThrough, lavaWalk: this.waterWalk);
      Vector2 velocity6 = this.velocity;
      if (velocity5 != velocity6)
        this.fallStart = (int) ((double) this.position.Y / 16.0);
    }
    this.position = this.position + this.velocity;
  }
} 