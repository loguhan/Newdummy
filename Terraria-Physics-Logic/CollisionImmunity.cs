// 碰撞攻击免疫时间处理
// 来源：Player.cs 第12803-12816行

public void GiveImmuneTimeForCollisionAttack(int time)
{
  if (this._timeSinceLastImmuneGet <= 20)
    ++this._immuneStrikes;
  else
    this._immuneStrikes = 1;
  this._timeSinceLastImmuneGet = 0;
  if (this._immuneStrikes >= 3 || this.immune && this.immuneTime > time)
    return;
  this.immune = true;
  this.immuneNoBlink = true;
  this.immuneTime = time;
} 