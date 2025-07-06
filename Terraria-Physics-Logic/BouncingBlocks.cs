// 弹跳方块碰撞处理
// 来源：Player.cs 第22116-22180行

private void TryBouncingBlocks(bool Falling)
{
  int num1 = (double) this.velocity.Y >= 5.0 || (double) this.velocity.Y <= -5.0 ? (!this.wet ? 1 : 0) : 0;
  bool flag1 = false;
  bool flag2 = false;
  float num2 = 1f;
  if (num1 == 0)
    return;
  bool flag3 = false;
  int num3 = 0;
  foreach (Point touchedTile in this.TouchedTiles)
  {
    Tile tile = Main.tile[touchedTile.X, touchedTile.Y];
    if (tile != null && tile.active() && tile.nactive() && (flag1 || Main.tileBouncy[(int) tile.type]))
    {
      flag3 = true;
      num3 = touchedTile.Y;
      break;
    }
  }
  if (!flag3)
    return;
  this.velocity.Y *= -0.8f;
  if (this.controlJump)
    this.velocity.Y = MathHelper.Clamp(this.velocity.Y, -13f, 13f);
  this.position.Y = (float) (num3 * 16 - ((double) this.velocity.Y < 0.0 ? this.height : -16));
  this.FloorVisuals(Falling);
  if (flag2)
  {
    Vector2 rotationVector2 = (this.fullRotation - 1.570796f).ToRotationVector2();
    if ((double) rotationVector2.Y > 0.0)
      rotationVector2.Y *= -1f;
    rotationVector2.Y = (float) ((double) rotationVector2.Y * 0.5 - 0.5);
    float num4 = -rotationVector2.Y;
    if ((double) num4 < 0.0)
      num4 = 0.0f;
    float num5 = (float) ((double) num4 * 1.5 + 1.0);
    float num6 = MathHelper.Clamp(Math.Abs(this.velocity.Y) * num5 * num2, 2f, 16f);
    this.velocity = rotationVector2 * num6;
    float num7 = 20f;
    Vector2 vector2 = this.Center + (this.fullRotation + 1.570796f).ToRotationVector2() * num7;
    Vector2 bottom = this.Bottom;
    ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.Keybrand, new ParticleOrchestraSettings()
    {
      PositionInWorld = bottom
    }, new int?(this.whoAmI));
  }
  this.velocity.Y = MathHelper.Clamp(this.velocity.Y, -20f, 20f);
  if ((double) this.velocity.Y * (double) this.gravDir >= 0.0)
    return;
  this.fallStart = (int) this.position.Y / 16;
} 