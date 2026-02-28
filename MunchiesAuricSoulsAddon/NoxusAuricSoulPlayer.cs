using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MunchiesAuricSoulsAddon;

public class NoxusAuricSoulPlayer : ModPlayer
{
	public bool hasAvatarSoul;

	public bool hasNamelessSoul;

	public int goodAppleCount;

	public override void SaveData(TagCompound tag)
	{
		tag["hasAvatarSoul"] = hasAvatarSoul;
		tag["hasNamelessSoul"] = hasNamelessSoul;
		tag["goodAppleCount"] = goodAppleCount;
	}

	public override void LoadData(TagCompound tag)
	{
		hasAvatarSoul = tag.GetBool("hasAvatarSoul");
		hasNamelessSoul = tag.GetBool("hasNamelessSoul");
		goodAppleCount = tag.GetInt("goodAppleCount");
	}
}
