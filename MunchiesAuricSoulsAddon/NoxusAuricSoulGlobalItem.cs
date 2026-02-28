using Terraria;
using Terraria.ModLoader;

namespace MunchiesAuricSoulsAddon;

public class NoxusAuricSoulGlobalItem : GlobalItem
{
	public override bool OnPickup(Item item, Player player)
	{
		Mod val = null;
		if (!ModLoader.TryGetMod("NoxusBoss", out val))
		{
			return base.OnPickup(item, player);
		}
		NoxusAuricSoulPlayer modPlayer = player.GetModPlayer<NoxusAuricSoulPlayer>();
		ModItem val2 = null;
		if (val.TryFind<ModItem>("AvatarAuricSoul", out val2) && item.type == val2.Type)
		{
			modPlayer.hasAvatarSoul = true;
		}
		ModItem val3 = null;
		if (val.TryFind<ModItem>("NamelessAuricSoul", out val3) && item.type == val3.Type)
		{
			modPlayer.hasNamelessSoul = true;
		}
		return base.OnPickup(item, player);
	}

	public override bool? UseItem(Item item, Player player)
	{
		NoxusAuricSoulPlayer modPlayer = player.GetModPlayer<NoxusAuricSoulPlayer>();
		Mod val = null;
		if (ModLoader.TryGetMod("NoxusBoss", out val))
		{
			ModItem val2 = null;
			if (val.TryFind<ModItem>("GoodApple", out val2) && item.type == val2.Type)
			{
				modPlayer.goodAppleCount++;
				MunchiesAuricSoulsAddon instance = MunchiesAuricSoulsAddon.instance;
				if (instance != null)
				{
					((Mod)instance).Logger.Info((object)$"Good Apple consumed! New count: {modPlayer.goodAppleCount}");
				}
			}
			ModItem val3 = null;
			if (val.TryFind<ModItem>("GoodAppleResetter", out val3) && item.type == val3.Type)
			{
				modPlayer.goodAppleCount = 0;
				MunchiesAuricSoulsAddon instance2 = MunchiesAuricSoulsAddon.instance;
				if (instance2 != null)
				{
					((Mod)instance2).Logger.Info((object)"Good Apple count reset to 0 by GoodAppleResetter");
				}
			}
		}
		return null;
	}
}
