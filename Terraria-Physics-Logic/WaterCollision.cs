// 水中碰撞处理
// 来源：Player.cs 第14819-14866行

public void WaterCollision(bool fallThrough, bool ignorePlats)
{
  int Height = !this.onTrack ? this.height : this.height - 20;
  Vector2 velocity = this.velocity;
  this.velocity = Collision.TileCollision(this.position, this.velocity, this.width, Height, fallThrough, ignorePlats, (int) this.gravDir);
  Vector2 vector2 = this.velocity * 0.5f;
  if ((double) this.velocity.X != (double) velocity.X)
    vector2.X = this.velocity.X;
  if ((double) this.velocity.Y != (double) velocity.Y)
    vector2.Y = this.velocity.Y;
  this.position = this.position + vector2;
  this.TryFloatingInWater();
}

private void TryFloatingInWater()
{
  if (!this.ShouldFloatInWater)
    return;
  float waterLineHeight;
  if (Collision.GetWaterLine(this.Center.ToTileCoordinates(), out waterLineHeight))
  {
    float y = this.Center.Y;
    if (this.mount.Active && this.mount.Type == 37)
      y -= 6f;
    float num = y + 8f;
    if ((double) num + (double) this.velocity.Y < (double) waterLineHeight)
      return;
    if ((double) y > (double) waterLineHeight)
    {
      this.velocity.Y -= 0.4f;
      if ((double) this.velocity.Y >= -6.0)
        return;
      this.velocity.Y = -6f;
    }
    else
    {
      this.velocity.Y = waterLineHeight - num;
      if ((double) this.velocity.Y < -3.0)
        this.velocity.Y = -3f;
      if ((double) this.velocity.Y != 0.0)
        return;
      this.velocity.Y = float.Epsilon;
    }
  }
  else
    this.velocity.Y -= 0.4f;
} 