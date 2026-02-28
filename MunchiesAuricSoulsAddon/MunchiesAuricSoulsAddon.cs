using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MunchiesAuricSoulsAddon;

public class MunchiesAuricSoulsAddon : Mod
{
	internal static MunchiesAuricSoulsAddon instance;

	internal Mod MunchiesMod;

	internal Mod CalamityHuntMod;

	internal Mod CalamityMod;

	internal Mod NoxusBossMod;

	internal Mod RagnarokMod;

	internal Mod ThoriumMod;

	internal Mod InfernalEclipseAPI;

	private LocalizedText YharonSoulText;

	private LocalizedText GoozmaSoulText;

	private LocalizedText AvatarSoulText;

	private LocalizedText NamelessSoulText;

	private LocalizedText GoodAppleText;

	private LocalizedText BloodOrangeText;

	private LocalizedText SanguineTangerineText;

	private LocalizedText TaintedCloudberryText;

	private LocalizedText SacredStrawberryText;

	private LocalizedText MiracleFruitText;

	private LocalizedText ElderberryText;

	private LocalizedText DragonfruitText;

	private LocalizedText EnchantedStarfishText;

	private LocalizedText CometShardText;

	private LocalizedText EtherealCoreText;

	private LocalizedText PhantomHeartText;

	private LocalizedText MushroomPlasmaRootText;

	private LocalizedText InfernalBloodText;

	private LocalizedText RedLightningContainerText;

	private LocalizedText ElectrolyteGelPackText;

	private LocalizedText StarlightFuelCellText;

	private LocalizedText EctoheartText;

	private LocalizedText CelestialOnionText;

	private LocalizedText InspirationEssenceText;

	private LocalizedText SingularityCoreText;

	private TypeInfo calamityPlayerType;

	private Type inspirationConsumableBaseType;

	private MethodInfo inspirationConsumeParametersMethod;

	public override void Load()
	{
		instance = this;
		YharonSoulText = ((Mod)this).GetLocalization("Acquisition.YharonSoul", (Func<string>)null);
		GoozmaSoulText = ((Mod)this).GetLocalization("Acquisition.GoozmaSoul", (Func<string>)null);
		AvatarSoulText = ((Mod)this).GetLocalization("Acquisition.AvatarSoul", (Func<string>)null);
		NamelessSoulText = ((Mod)this).GetLocalization("Acquisition.NamelessSoul", (Func<string>)null);
		GoodAppleText = ((Mod)this).GetLocalization("Acquisition.GoodApple", (Func<string>)null);
		BloodOrangeText = ((Mod)this).GetLocalization("Acquisition.BloodOrange", (Func<string>)null);
		SanguineTangerineText = ((Mod)this).GetLocalization("Acquisition.SanguineTangerine", (Func<string>)null);
		TaintedCloudberryText = ((Mod)this).GetLocalization("Acquisition.TaintedCloudberry", (Func<string>)null);
		SacredStrawberryText = ((Mod)this).GetLocalization("Acquisition.SacredStrawberry", (Func<string>)null);
		MiracleFruitText = ((Mod)this).GetLocalization("Acquisition.MiracleFruit", (Func<string>)null);
		ElderberryText = ((Mod)this).GetLocalization("Acquisition.Elderberry", (Func<string>)null);
		DragonfruitText = ((Mod)this).GetLocalization("Acquisition.Dragonfruit", (Func<string>)null);
		EnchantedStarfishText = ((Mod)this).GetLocalization("Acquisition.EnchantedStarfish", (Func<string>)null);
		CometShardText = ((Mod)this).GetLocalization("Acquisition.CometShard", (Func<string>)null);
		EtherealCoreText = ((Mod)this).GetLocalization("Acquisition.EtherealCore", (Func<string>)null);
		PhantomHeartText = ((Mod)this).GetLocalization("Acquisition.PhantomHeart", (Func<string>)null);
		MushroomPlasmaRootText = ((Mod)this).GetLocalization("Acquisition.MushroomPlasmaRoot", (Func<string>)null);
		InfernalBloodText = ((Mod)this).GetLocalization("Acquisition.InfernalBlood", (Func<string>)null);
		RedLightningContainerText = ((Mod)this).GetLocalization("Acquisition.RedLightningContainer", (Func<string>)null);
		ElectrolyteGelPackText = ((Mod)this).GetLocalization("Acquisition.ElectrolyteGelPack", (Func<string>)null);
		StarlightFuelCellText = ((Mod)this).GetLocalization("Acquisition.StarlightFuelCell", (Func<string>)null);
		EctoheartText = ((Mod)this).GetLocalization("Acquisition.Ectoheart", (Func<string>)null);
		CelestialOnionText = ((Mod)this).GetLocalization("Acquisition.CelestialOnion", (Func<string>)null);
		InspirationEssenceText = ((Mod)this).GetLocalization("Acquisition.InspirationEssence", (Func<string>)null);
		SingularityCoreText = ((Mod)this).GetLocalization("Acquisition.SingularityCore", (Func<string>)null);
	}

	public override void PostSetupContent()
	{
		try
		{
			Mod munchiesMod = null;
			if (ModLoader.TryGetMod("Munchies", out munchiesMod))
			{
				MunchiesMod = munchiesMod;
				Mod val = null;
				CalamityHuntMod = (ModLoader.TryGetMod("CalamityHunt", out val) ? val : null);
				Mod val3 = null;
				CalamityMod = (ModLoader.TryGetMod("CalamityMod", out val3) ? val3 : null);
				Mod val2 = null;
				NoxusBossMod = (ModLoader.TryGetMod("NoxusBoss", out val2) ? val2 : null);
				Mod val4 = null;
				RagnarokMod = (ModLoader.TryGetMod("RagnarokMod", out val4) ? val4 : null);
				Mod val6 = null;
				ThoriumMod = (ModLoader.TryGetMod("ThoriumMod", out val6) ? val6 : null);
				Mod val5 = null;
				InfernalEclipseAPI = (ModLoader.TryGetMod("InfernalEclipseAPI", out val5) ? val5 : null);
				AddAuricSouls();
				AddCalamityConsumables();
				AddRagnarokConsumables();
				AddInfernalEclipseConsumables();
			}
			else
			{
				((Mod)this).Logger.Error((object)"Munchies mod not found! This addon requires Munchies to function.");
			}
		}
		catch (Exception ex)
		{
			((Mod)this).Logger.Error((object)("PostSetupContent Error in Munchies Auric Souls Addon: " + ex.Message));
		}
	}

	private void AddAuricSouls()
	{
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		if (CalamityHuntMod != null)
		{
			ModItem modItem = GetModItem(CalamityHuntMod, "YharonSoul");
			ModItem modItem2 = GetModItem(CalamityHuntMod, "GoozmaSoul");
			if (modItem != null)
			{
				CallMunchiesModConsumable(CalamityHuntMod, modItem, () => GetYharonSoulStatus(Main.LocalPlayer), YharonSoulText);
			}
			if (modItem2 != null)
			{
				CallMunchiesModConsumable(CalamityHuntMod, modItem2, () => GetGoozmaSoulStatus(Main.LocalPlayer), GoozmaSoulText);
			}
		}
		if (NoxusBossMod == null)
		{
			return;
		}
		ModItem val = ((CalamityHuntMod != null) ? GetModItem(NoxusBossMod, "AvatarAuricSoul") : null);
		ModItem val2 = ((CalamityHuntMod != null) ? GetModItem(NoxusBossMod, "NamelessAuricSoul") : null);
		ModItem modItem3 = GetModItem(NoxusBossMod, "GoodApple");
		if (val != null)
		{
			CallMunchiesModConsumable(NoxusBossMod, val, () => GetAvatarSoulStatus(Main.LocalPlayer), AvatarSoulText);
		}
		if (val2 != null)
		{
			CallMunchiesModConsumable(NoxusBossMod, val2, () => GetNamelessSoulStatus(Main.LocalPlayer), NamelessSoulText);
		}
		if (modItem3 != null)
		{
			CallMunchiesModMultiConsumable(NoxusBossMod, modItem3, () => GetGoodAppleCurrentCount(Main.LocalPlayer), () => GetGoodAppleMaxCount(), GoodAppleText, Color.Pink);
		}
	}

	private void AddCalamityConsumables()
	{
		if (CalamityMod == null)
		{
			return;
		}
		AddCalamityConsumables_Health();
		AddCalamityConsumables_Mana();
		AddCalamityConsumables_Other();
		AddCalamityConsumables_RageMode();
		AddCalamityConsumables_AdrenalineMode();
	}

	private void AddCalamityConsumables_Health()
	{
		if (CalamityMod == null)
		{
			return;
		}
		ModItem modItem = GetModItem(CalamityMod, "MiracleFruit");
		if (modItem != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem, () => GetCalamityBoolStatus(Main.LocalPlayer, "mFruit"), MiracleFruitText);
		}
		if (CalamityMod.Version < new Version(2, 1))
		{
			ModItem modItem2 = GetModItem(CalamityMod, "BloodOrange");
			ModItem modItem3 = GetModItem(CalamityMod, "Elderberry");
			ModItem modItem4 = GetModItem(CalamityMod, "Dragonfruit");
			if (modItem2 != null)
			{
				CallMunchiesModConsumable(CalamityMod, modItem2, () => GetCalamityBoolStatus(Main.LocalPlayer, "sTangerine"), BloodOrangeText);
			}
			if (modItem3 != null)
			{
				CallMunchiesModConsumable(CalamityMod, modItem3, () => GetCalamityBoolStatus(Main.LocalPlayer, "eBerry"), ElderberryText);
			}
			if (modItem4 != null)
			{
				CallMunchiesModConsumable(CalamityMod, modItem4, () => GetCalamityBoolStatus(Main.LocalPlayer, "dFruit"), DragonfruitText);
			}
			return;
		}
		ModItem modItem5 = GetModItem(CalamityMod, "SanguineTangerine");
		ModItem modItem6 = GetModItem(CalamityMod, "TaintedCloudberry");
		ModItem modItem7 = GetModItem(CalamityMod, "SacredStrawberry");
		if (modItem5 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem5, () => GetCalamityBoolStatus(Main.LocalPlayer, "sTangerine"), SanguineTangerineText);
		}
		if (modItem6 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem6, () => GetCalamityBoolStatus(Main.LocalPlayer, "tCloudberry"), TaintedCloudberryText);
		}
		if (modItem7 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem7, () => GetCalamityBoolStatus(Main.LocalPlayer, "sStrawberry"), SacredStrawberryText);
		}
	}

	private void AddCalamityConsumables_Mana()
	{
		if (CalamityMod == null)
		{
			return;
		}
		ModItem modItem = GetModItem(CalamityMod, "EnchantedStarfish");
		ModItem modItem2 = GetModItem(CalamityMod, "CometShard");
		ModItem modItem3 = GetModItem(CalamityMod, "EtherealCore");
		ModItem modItem4 = GetModItem(CalamityMod, "PhantomHeart");
		if (modItem != null)
		{
			CallMunchiesModMultiConsumable(CalamityMod, modItem, () => Main.LocalPlayer.ConsumedManaCrystals, () => 9, EnchantedStarfishText);
		}
		if (modItem2 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem2, () => GetCalamityBoolStatus(Main.LocalPlayer, "cShard"), CometShardText);
		}
		if (modItem3 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem3, () => GetCalamityBoolStatus(Main.LocalPlayer, "eCore"), EtherealCoreText);
		}
		if (modItem4 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem4, () => GetCalamityBoolStatus(Main.LocalPlayer, "pHeart"), PhantomHeartText);
		}
	}

	private void AddCalamityConsumables_RageMode()
	{
		if (CalamityMod == null)
		{
			return;
		}
		ModItem modItem = GetModItem(CalamityMod, "MushroomPlasmaRoot");
		ModItem modItem2 = GetModItem(CalamityMod, "InfernalBlood");
		ModItem modItem3 = GetModItem(CalamityMod, "RedLightningContainer");
		if (modItem != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem, () => GetCalamityBoolStatus(Main.LocalPlayer, "rageBoostOne"), MushroomPlasmaRootText, Color.Red, "Revengeance", () => GetCalamityBoolStatus(Main.LocalPlayer, "RageEnabled"));
		}
		if (modItem2 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem2, () => GetCalamityBoolStatus(Main.LocalPlayer, "rageBoostTwo"), InfernalBloodText, Color.Red, "Revengeance", () => GetCalamityBoolStatus(Main.LocalPlayer, "RageEnabled"));
		}
		if (modItem3 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem3, () => GetCalamityBoolStatus(Main.LocalPlayer, "rageBoostThree"), RedLightningContainerText, Color.Red, "Revengeance", () => GetCalamityBoolStatus(Main.LocalPlayer, "RageEnabled"));
		}
	}

	private void AddCalamityConsumables_AdrenalineMode()
	{
		if (CalamityMod == null)
		{
			return;
		}
		ModItem modItem = GetModItem(CalamityMod, "ElectrolyteGelPack");
		ModItem modItem2 = GetModItem(CalamityMod, "StarlightFuelCell");
		ModItem modItem3 = GetModItem(CalamityMod, "Ectoheart");
		if (modItem != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem, () => GetCalamityBoolStatus(Main.LocalPlayer, "adrenalineBoostOne"), ElectrolyteGelPackText, Color.Red, "Revengeance", () => GetCalamityBoolStatus(Main.LocalPlayer, "AdrenalineEnabled"));
		}
		if (modItem2 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem2, () => GetCalamityBoolStatus(Main.LocalPlayer, "adrenalineBoostTwo"), StarlightFuelCellText, Color.Red, "Revengeance", () => GetCalamityBoolStatus(Main.LocalPlayer, "AdrenalineEnabled"));
		}
		if (modItem3 != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem3, () => GetCalamityBoolStatus(Main.LocalPlayer, "adrenalineBoostThree"), EctoheartText, Color.Red, "Revengeance", () => GetCalamityBoolStatus(Main.LocalPlayer, "AdrenalineEnabled"));
		}
	}

	private void AddCalamityConsumables_Other()
	{
		if (CalamityMod == null)
		{
			return;
		}
		ModItem modItem = GetModItem(CalamityMod, "CelestialOnion");
		if (modItem != null)
		{
			CallMunchiesModConsumable(CalamityMod, modItem, () => GetCalamityBoolStatus(Main.LocalPlayer, "extraAccessoryML"), CelestialOnionText);
		}
	}

	private void AddRagnarokConsumables()
	{
		if (RagnarokMod == null)
		{
			return;
		}
		ModItem modItem = GetModItem(RagnarokMod, "InspirationEssence");
		if (modItem != null)
		{
			CallMunchiesModMultiConsumable(RagnarokMod, modItem, () => GetInspirationEssenceCurrentCount(Main.LocalPlayer), () => GetInspirationEssenceTotalCount(Main.LocalPlayer), InspirationEssenceText);
		}
	}

	private void AddInfernalEclipseConsumables()
	{
		if (InfernalEclipseAPI == null)
		{
			return;
		}
		ModItem modItem = GetModItem(InfernalEclipseAPI, "SingularityCore");
		if (modItem != null)
		{
			CallMunchiesModConsumable(InfernalEclipseAPI, modItem, () => GetSingularityCoreStatus(Main.LocalPlayer), SingularityCoreText);
		}
	}

	private ModItem GetModItem(Mod mod, string name)
	{
		if (mod == null)
		{
			return null;
		}
		ModItem result = null;
		if (!mod.TryFind<ModItem>(name, out result))
		{
			return null;
		}
		return result;
	}

	private void CallMunchiesModConsumable(Mod sourceMod, ModItem item, Func<bool> hasBeenConsumed, LocalizedText acquisitionText, Color? customColor = null, string difficulty = "classic", Func<bool> availability = null)
	{
		if (MunchiesMod != null && item != null)
		{
			object[] array = new object[11]
			{
				"AddSingleConsumable", sourceMod, "1.4", item, "player", hasBeenConsumed, customColor, difficulty, null, availability,
				acquisitionText
			};
			MunchiesMod.Call(array);
		}
	}

	private void CallMunchiesModMultiConsumable(Mod sourceMod, ModItem item, Func<int> currentCount, Func<int> totalCount, LocalizedText acquisitionText, Color? customColor = null, string difficulty = "classic", Func<bool> availability = null)
	{
		if (MunchiesMod != null && sourceMod != null && item != null)
		{
			object[] array = new object[12]
			{
				"AddMultiUseConsumable", sourceMod, "1.4", item, "player", currentCount, totalCount, customColor, difficulty, null,
				availability, acquisitionText
			};
			MunchiesMod.Call(array);
		}
	}

	private TypeInfo GetCalamityPlayerType()
	{
		if (CalamityMod == null)
		{
			return null;
		}
		if (calamityPlayerType != null)
		{
			return calamityPlayerType;
		}
		calamityPlayerType = CalamityMod.Code.DefinedTypes.FirstOrDefault((TypeInfo t) => t.Name == "CalamityPlayer");
		return calamityPlayerType;
	}

	private object GetCalamityPlayerObject(Player player)
	{
		if (player == null)
		{
			return null;
		}
		try
		{
			TypeInfo calamityPlayerType = GetCalamityPlayerType();
			if (calamityPlayerType == null)
			{
				return null;
			}
			return typeof(Player).GetMethod("GetModPlayer", new Type[0]).MakeGenericMethod(calamityPlayerType.AsType()).Invoke(player, null);
		}
		catch
		{
			return null;
		}
	}

	private bool GetCalamityBoolStatus(Player player, string fieldName)
	{
		object calamityPlayerObject = GetCalamityPlayerObject(player);
		if (calamityPlayerObject == null)
		{
			return false;
		}
		Type type = calamityPlayerObject.GetType();
		FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null && field.FieldType == typeof(bool))
		{
			return (bool)field.GetValue(calamityPlayerObject);
		}
		PropertyInfo property = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null && property.PropertyType == typeof(bool) && property.GetIndexParameters().Length == 0)
		{
			return (bool)property.GetValue(calamityPlayerObject);
		}
		return false;
	}

	private bool GetYharonSoulStatus(Player player)
	{
		if (CalamityHuntMod != null)
		{
			try
			{
				TypeInfo typeInfo = CalamityHuntMod.Code.DefinedTypes.FirstOrDefault((TypeInfo t) => t.Name == "AuricSoulPlayer");
				if (typeInfo != null)
				{
					object obj = typeof(Player).GetMethod("GetModPlayer", new Type[0]).MakeGenericMethod(typeInfo.AsType()).Invoke(player, null);
					FieldInfo field = typeInfo.GetField("yharonSoul");
					if (field != null && obj != null)
					{
						return (bool)field.GetValue(obj);
					}
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private bool GetGoozmaSoulStatus(Player player)
	{
		if (CalamityHuntMod != null)
		{
			try
			{
				TypeInfo typeInfo = CalamityHuntMod.Code.DefinedTypes.FirstOrDefault((TypeInfo t) => t.Name == "AuricSoulPlayer");
				if (typeInfo != null)
				{
					object obj = typeof(Player).GetMethod("GetModPlayer", new Type[0]).MakeGenericMethod(typeInfo.AsType()).Invoke(player, null);
					FieldInfo field = typeInfo.GetField("goozmaSoul");
					if (field != null && obj != null)
					{
						return (bool)field.GetValue(obj);
					}
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private bool GetAvatarSoulStatus(Player player)
	{
		if (NoxusBossMod == null)
		{
			return false;
		}
		return player.GetModPlayer<NoxusAuricSoulPlayer>().hasAvatarSoul;
	}

	private bool GetNamelessSoulStatus(Player player)
	{
		if (NoxusBossMod == null)
		{
			return false;
		}
		return player.GetModPlayer<NoxusAuricSoulPlayer>().hasNamelessSoul;
	}

	private int GetGoodAppleCurrentCount(Player player)
	{
		if (NoxusBossMod == null)
		{
			return 0;
		}
		((Mod)this).Logger.Info((object)"GetGoodAppleCurrentCount called");
		NoxusAuricSoulPlayer modPlayer = player.GetModPlayer<NoxusAuricSoulPlayer>();
		if (modPlayer != null)
		{
			((Mod)this).Logger.Info((object)$"Custom apple count: {modPlayer.goodAppleCount}");
			return modPlayer.goodAppleCount;
		}
		return 0;
	}

	private bool GetGoodAppleStatus(Player player)
	{
		if (NoxusBossMod == null)
		{
			return false;
		}
		return GetGoodAppleCurrentCount(player) > 0;
	}

	private int GetGoodAppleMaxCount()
	{
		return int.MaxValue;
	}

	private int GetInspirationEssenceCurrentCount(Player player)
	{
		GetInspirationConsumeParameters(RagnarokMod, "InspirationEssence", player, out int count, out int _);
		return count;
	}

	private int GetInspirationEssenceTotalCount(Player player)
	{
		GetInspirationConsumeParameters(RagnarokMod, "InspirationEssence", player, out int _, out int total);
		return total;
	}

	private bool GetSingularityCoreStatus(Player player)
	{
		return GetSingleUseConsumableConsumedStatus(InfernalEclipseAPI, "SingularityCore", player);
	}

	private bool GetSingleUseConsumableConsumedStatus(Mod sourceMod, string itemName, Player player)
	{
		if (sourceMod == null || player == null)
		{
			return false;
		}
		ModItem modItem = GetModItem(sourceMod, itemName);
		if (modItem == null)
		{
			return false;
		}
		try
		{
			return !modItem.CanUseItem(player);
		}
		catch
		{
			return false;
		}
	}

	private void GetInspirationConsumeParameters(Mod sourceMod, string itemName, Player player, out int count, out int total)
	{
		count = 0;
		total = 1;
		if (sourceMod == null || player == null)
		{
			return;
		}
		ModItem modItem = GetModItem(sourceMod, itemName);
		if (modItem == null)
		{
			return;
		}
		try
		{
			if (!TryGetInspirationConsumableApi(out Type baseType, out MethodInfo getConsumeParameters))
			{
				return;
			}
			if (!baseType.IsAssignableFrom(modItem.GetType()))
			{
				return;
			}
			object[] parameters = new object[4] { modItem, player, 0, 1 };
			getConsumeParameters.Invoke(null, parameters);
			if (parameters[2] is int num)
			{
				count = num;
			}
			if (parameters[3] is int num2)
			{
				total = num2;
			}
		}
		catch
		{
		}
	}

	private bool TryGetInspirationConsumableApi(out Type baseType, out MethodInfo getConsumeParameters)
	{
		baseType = inspirationConsumableBaseType;
		getConsumeParameters = inspirationConsumeParametersMethod;
		if (baseType != null && getConsumeParameters != null)
		{
			return true;
		}
		if (ThoriumMod == null)
		{
			return false;
		}
		baseType = ThoriumMod.Code.GetType("ThoriumMod.Items.BardItems.InspirationConsumableBase");
		if (baseType == null)
		{
			return false;
		}
		getConsumeParameters = baseType.GetMethod("GetConsumeParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
		if (getConsumeParameters == null)
		{
			return false;
		}
		inspirationConsumableBaseType = baseType;
		inspirationConsumeParametersMethod = getConsumeParameters;
		return true;
	}

	public override void Unload()
	{
		instance = null;
		MunchiesMod = null;
		CalamityHuntMod = null;
		CalamityMod = null;
		NoxusBossMod = null;
		RagnarokMod = null;
		ThoriumMod = null;
		InfernalEclipseAPI = null;
		calamityPlayerType = null;
		inspirationConsumableBaseType = null;
		inspirationConsumeParametersMethod = null;
	}
}
