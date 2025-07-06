// 下坡移动处理
// 来源：Player.cs 第14792-14805行

public void SlopeDownMovement()
{
  this.sloping = false;
  float y = this.velocity.Y;
  Vector4 vector4 = Collision.WalkDownSlope(this.position, this.velocity, this.width, this.height, this.gravity * this.gravDir);
  this.position.X = vector4.X;
  this.position.Y = vector4.Y;
  this.velocity.X = vector4.Z;
  this.velocity.Y = vector4.W;
  if ((double) this.velocity.Y == (double) y)
    return;
  this.sloping = true;
} 