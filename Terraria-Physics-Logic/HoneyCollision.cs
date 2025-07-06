// 蜂蜜碰撞处理
// 来源：Player.cs 第14806-14818行

public void HoneyCollision(bool fallThrough, bool ignorePlats)
{
  int Height = !this.onTrack ? this.height : this.height - 20;
  Vector2 velocity = this.velocity;
  this.velocity = Collision.TileCollision(this.position, this.velocity, this.width, Height, fallThrough, ignorePlats, (int) this.gravDir);
  Vector2 vector2 = this.velocity * 0.25f;
  if ((double) this.velocity.X != (double) velocity.X)
    vector2.X = this.velocity.X;
  if ((double) this.velocity.Y != (double) velocity.Y)
    vector2.Y = this.velocity.Y;
  this.position = this.position + vector2;
} 