// NPC 碰撞更新
// 来源：Player.cs 第19424-19506行

public void Update_NPCCollision()
{
  if (this.creativeGodMode)
    return;
  Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int) this.position.X, (int) this.position.Y, this.width, this.height);
  for (int index = 0; index < 200; ++index)
  {
    if (Main.npc[index].active && !Main.npc[index].friendly && Main.npc[index].damage > 0)
    {
      int specialHitSetter = -1;
      switch (Main.npc[index].type)
      {
        case 396:
        case 397:
        case 398:
        case 400:
        case 401:
          specialHitSetter = 1;
          break;
        case 636:
          specialHitSetter = 1;
          break;
      }
      if ((specialHitSetter != -1 || !this.immune) && (this.dash != 2 || index != this.eocHit || this.eocDash <= 0) && !this.npcTypeNoAggro[Main.npc[index].type])
      {
        float damageMultiplier = 1f;
        NPC npc1 = Main.npc[index];
        npc1.position = npc1.position + Main.npc[index].netOffset;
        Microsoft.Xna.Framework.Rectangle npcRect = new Microsoft.Xna.Framework.Rectangle((int) Main.npc[index].position.X, (int) Main.npc[index].position.Y, Main.npc[index].width, Main.npc[index].height);
        NPC.GetMeleeCollisionData(rectangle, index, ref specialHitSetter, ref damageMultiplier, ref npcRect);
        if (rectangle.Intersects(npcRect))
        {
          if (!this.npcTypeNoAggro[Main.npc[index].type])
          {
            bool flag1 = true;
            bool flag2 = false;
            int num1 = this.CanParryAgainst(rectangle, npcRect, Main.npc[index].velocity) ? 1 : 0;
            float num2 = this.thorns;
            float knockback = 10f;
            if (this.turtleThorns)
              num2 = 2f;
            if (num1 != 0)
            {
              num2 = 2f;
              knockback = 5f;
              flag1 = false;
              flag2 = true;
            }
            int hitDirection = -1;
            if ((double) Main.npc[index].position.X + (double) (Main.npc[index].width / 2) < (double) this.position.X + (double) (this.width / 2))
              hitDirection = 1;
            int Damage = Main.DamageVar((float) Main.npc[index].damage * damageMultiplier, -this.luck);
            int num3 = Item.NPCtoBanner(Main.npc[index].BannerID());
            if (num3 > 0 && this.HasNPCBannerBuff(num3))
              Damage = !Main.expertMode ? (int) ((double) Damage * (double) ItemID.Sets.BannerStrength[Item.BannerToItem(num3)].NormalDamageReceived) : (int) ((double) Damage * (double) ItemID.Sets.BannerStrength[Item.BannerToItem(num3)].ExpertDamageReceived);
            if (this.whoAmI == Main.myPlayer && (double) num2 > 0.0 && !this.immune && !Main.npc[index].dontTakeDamage)
            {
              int damage = (int) ((double) Damage * (double) num2);
              if (damage > 1000)
                damage = 1000;
              this.ApplyDamageToNPC(Main.npc[index], damage, knockback, -hitDirection, false);
            }
            if (this.resistCold && Main.npc[index].coldDamage)
              Damage = (int) ((double) Damage * 0.699999988079071);
            if (!this.immune && !flag2)
              this.StatusFromNPC(Main.npc[index]);
            if (flag1)
              this.Hurt(PlayerDeathReason.ByNPC(index), Damage, hitDirection, cooldownCounter: specialHitSetter);
            if (num1 != 0)
            {
              this.GiveImmuneTimeForCollisionAttack(this.longInvince ? 60 : 30);
              this.AddBuff(198, 300, false);
            }
          }
          else
            continue;
        }
        NPC npc2 = Main.npc[index];
        npc2.position = npc2.position - Main.npc[index].netOffset;
      }
    }
  }
}

public bool CanParryAgainst(
  Microsoft.Xna.Framework.Rectangle blockingPlayerRect,
  Microsoft.Xna.Framework.Rectangle enemyRect,
  Vector2 enemyVelocity)
{
  return this.shieldParryTimeLeft > 0 && Math.Sign(enemyRect.Center.X - blockingPlayerRect.Center.X) == this.direction && enemyVelocity != Vector2.Zero && !this.immune;
} 