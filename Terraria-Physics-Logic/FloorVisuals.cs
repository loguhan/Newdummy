// 地面视觉效果处理
// 来源：Player.cs 第14968-15167行

public void FloorVisuals(bool Falling)
{
  int x = (int) (((double) this.position.X + (double) (this.width / 2)) / 16.0);
  int y = (int) (((double) this.position.Y + (double) this.height) / 16.0);
  if ((double) this.gravDir == -1.0)
    y = (int) ((double) this.position.Y - 0.100000001490116) / 16;
  int type = Player.GetFloorTileType(x, y);
  if (type <= -1)
  {
    this.ResetFloorFlags();
  }
  else
  {
    this.sticky = type == 229;
    this.slippy = type == 161 || type == 162 || type == 163 || type == 164 || type == 200 || type == (int) sbyte.MaxValue;
    this.slippy2 = type == 197;
    this.powerrun = type == 198;
    this.runningOnSand = TileID.Sets.Conversion.Sand[type] || TileID.Sets.Conversion.Sandstone[type] || TileID.Sets.Conversion.HardenedSand[type];
    if (Main.tile[x - 1, y].slope() != (byte) 0 || Main.tile[x, y].slope() != (byte) 0 || Main.tile[x + 1, y].slope() != (byte) 0)
      type = -1;
    if ((this.wet ? 0 : (!this.mount.Cart ? 1 : 0)) == 0)
      return;
    this.MakeFloorDust(Falling, type);
  }
}

private void ResetFloorFlags()
{
  this.slippy = false;
  this.slippy2 = false;
  this.sticky = false;
  this.powerrun = false;
  this.runningOnSand = false;
}

private static int GetFloorTileType(int x, int y)
{
  int num = -1;
  if (Main.tile[x - 1, y] == null)
    Main.tile[x - 1, y] = new Tile();
  if (Main.tile[x + 1, y] == null)
    Main.tile[x + 1, y] = new Tile();
  if (Main.tile[x, y] == null)
    Main.tile[x, y] = new Tile();
  if (Main.tile[x, y].nactive() && Main.tileSolid[(int) Main.tile[x, y].type])
    num = (int) Main.tile[x, y].type;
  else if (Main.tile[x - 1, y].nactive() && Main.tileSolid[(int) Main.tile[x - 1, y].type])
    num = (int) Main.tile[x - 1, y].type;
  else if (Main.tile[x + 1, y].nactive() && Main.tileSolid[(int) Main.tile[x + 1, y].type])
    num = (int) Main.tile[x + 1, y].type;
  return num;
}

private void MakeFloorDust(bool Falling, int type)
{
  if (type != 147 && type != 25 && type != 53 && type != 189 && type != 0 && type != 123 && type != 57 && type != 112 && type != 116 && type != 196 && type != 193 && type != 195 && type != 197 && type != 199 && type != 229 && type != 371 && type != 460)
    return;
  int num1 = 1;
  if (Falling)
    num1 = 20;
  for (int index1 = 0; index1 < num1; ++index1)
  {
    bool flag = true;
    int Type = 76;
    if (type == 53)
      Type = 32;
    if (type == 189)
      Type = 16;
    if (type == 0)
      Type = 0;
    if (type == 123)
      Type = 53;
    if (type == 57)
      Type = 36;
    if (type == 112)
      Type = 14;
    if (type == 116)
      Type = 51;
    if (type == 196)
      Type = 108;
    if (type == 193)
      Type = 4;
    if (type == 195 || type == 199)
      Type = 5;
    if (type == 197)
      Type = 4;
    if (type == 229)
      Type = 153;
    if (type == 371)
      Type = 243;
    if (type == 460)
      Type = 108;
    if (type == 25)
      Type = 37;
    if (Type == 32 && Main.rand.Next(2) == 0)
      flag = false;
    if (Type == 14 && Main.rand.Next(2) == 0)
      flag = false;
    if (Type == 51 && Main.rand.Next(2) == 0)
      flag = false;
    if (Type == 36 && Main.rand.Next(2) == 0)
      flag = false;
    if (Type == 0 && Main.rand.Next(3) != 0)
      flag = false;
    if (Type == 53 && Main.rand.Next(3) != 0)
      flag = false;
    Color newColor = new Color();
    if (type == 193)
      newColor = new Color(30, 100, (int) byte.MaxValue, 100);
    if (type == 197)
      newColor = new Color(97, 200, (int) byte.MaxValue, 100);
    if (!Falling)
    {
      float num2 = Math.Abs(this.velocity.X) / 3f;
      if ((double) Main.rand.Next(100) > (double) num2 * 100.0)
        flag = false;
    }
    if (flag)
    {
      float num3 = this.velocity.X;
      if ((double) num3 > 6.0)
        num3 = 6f;
      if ((double) num3 < -6.0)
        num3 = -6f;
      if ((double) this.velocity.X != 0.0 | Falling)
      {
        int index2 = Dust.NewDust(new Vector2(this.position.X, (float) ((double) this.position.Y + (double) this.height - 2.0)), this.width, 6, Type, Alpha: 50, newColor: newColor);
        if ((double) this.gravDir == -1.0)
          Main.dust[index2].position.Y -= (float) (this.height + 4);
        if (Type == 76)
        {
          Main.dust[index2].scale += (float) Main.rand.Next(3) * 0.1f;
          Main.dust[index2].noLight = true;
        }
        if (Type == 16 || Type == 108 || Type == 153)
          Main.dust[index2].scale += (float) Main.rand.Next(6) * 0.1f;
        if (Type == 37)
        {
          Main.dust[index2].scale += 0.25f;
          Main.dust[index2].alpha = 50;
        }
        if (Type == 5)
          Main.dust[index2].scale += (float) Main.rand.Next(2, 8) * 0.1f;
        Main.dust[index2].noGravity = true;
        if (num1 > 1)
        {
          Main.dust[index2].velocity.X *= 1.2f;
          Main.dust[index2].velocity.Y *= 0.8f;
          --Main.dust[index2].velocity.Y;
          Main.dust[index2].velocity *= 0.8f;
          Main.dust[index2].scale += (float) Main.rand.Next(3) * 0.1f;
          Main.dust[index2].velocity.X = (float) (((double) Main.dust[index2].position.X - ((double) this.position.X + (double) (this.width / 2))) * 0.200000002980232);
          if ((double) Main.dust[index2].velocity.Y > 0.0)
            Main.dust[index2].velocity.Y *= -1f;
          Main.dust[index2].velocity.X += num3 * 0.3f;
        }
        else
          Main.dust[index2].velocity *= 0.2f;
        Main.dust[index2].position.X -= num3 * 1f;
        if ((double) this.gravDir == -1.0)
          Main.dust[index2].velocity.Y *= -1f;
      }
    }
  }
} 