// 斜坡碰撞处理
// 来源：Player.cs 第14944-14967行

public void SlopingCollision(bool fallThrough, bool ignorePlats)
{
  if (ignorePlats || this.controlDown || this.grappling[0] >= 0 || (double) this.gravDir == -1.0)
    this.stairFall = true;
  Vector4 vector4 = Collision.SlopeCollision(this.position, this.velocity, this.width, this.height, this.gravity, this.stairFall);
  if (Collision.stairFall)
    this.stairFall = true;
  else if (!fallThrough)
    this.stairFall = false;
  if (Collision.stair && (double) Math.Abs(vector4.Y - this.position.Y) > 8.0 + (double) Math.Abs(this.velocity.X))
  {
    this.gfxOffY -= vector4.Y - this.position.Y;
    this.stepSpeed = 4f;
  }
  Vector2 velocity = this.velocity;
  this.position.X = vector4.X;
  this.position.Y = vector4.Y;
  this.velocity.X = vector4.Z;
  this.velocity.Y = vector4.W;
  if ((double) this.gravDir != -1.0 || (double) this.velocity.Y != 0.0100999996066093)
    return;
  this.velocity.Y = 0.0f;
} 