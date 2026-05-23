using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MunchiesAuricSoulsAddon;

public class MunchiesAuricSoulsAddon : Mod
{
    internal static MunchiesAuricSoulsAddon instance;

    private readonly Dictionary<string, MemberInfo> _reflectionCache = new();
    private readonly Dictionary<string, ModPlayer> _modPlayerTypeCache = new();
    private Type inspirationConsumableBaseType;
    private MethodInfo inspirationConsumeParametersMethod;

    public override void Load()
    {
        instance = this;
    }

    public override void Unload()
    {
        instance = null;
        _reflectionCache.Clear();
        _modPlayerTypeCache.Clear();
        inspirationConsumableBaseType = null;
        inspirationConsumeParametersMethod = null;
    }

    public override void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("Munchies", out Mod munchiesMod))
        {
            Logger.Error("Munchies mod not found! This addon requires Munchies to function.");
            return;
        }

        try
        {
            AddCalamityConsumables(munchiesMod);
            AddAuricSouls(munchiesMod);
            AddInfernalEclipseConsumables(munchiesMod);
            AddSOTSConsumables(munchiesMod);
            AddRagnarokConsumables(munchiesMod);
            AddCoJConsumables(munchiesMod);
            AddFargoConsumables(munchiesMod);
            AddRedemptionConsumables(munchiesMod);
            AddQoTConsumables(munchiesMod);
        }
        catch (Exception ex)
        {
            Logger.Error($"PostSetupContent Error in Munchies Auric Souls Addon: {ex.Message}");
        }
    }

    private void AddAuricSouls(Mod munchies)
    {
        if (ModLoader.TryGetMod("CalamityHunt", out Mod hunt))
        {
            RegisterConsumable(munchies, hunt, "YharonSoul", "AuricSoulPlayer", "yharonSoul");
            RegisterConsumable(munchies, hunt, "GoozmaSoul", "AuricSoulPlayer", "goozmaSoul");
            RegisterConsumable(munchies, hunt, "RottenSoul", "AuricSoulPlayer", "olddukeSoul");

            if (ModLoader.TryGetMod("CalRemix", out Mod remix))
            {
                RegisterConsumable(munchies, hunt, "AshenSoul", "AuricSoulPlayer", "pyrogenSoul", displayMod: remix);
            }
        }

        if (ModLoader.TryGetMod("NoxusBoss", out Mod noxus))
        {
            if (hunt != null)
            {
                RegisterConsumable(munchies, noxus, "AvatarAuricSoul", "NoxusAuricSoulPlayer", "hasAvatarSoul");
                RegisterConsumable(munchies, noxus, "NamelessAuricSoul", "NoxusAuricSoulPlayer", "hasNamelessSoul");
            }

            if (noxus.TryFind("GoodApple", out ModItem apple))
            {
                CallMunchiesMulti(munchies, noxus, apple,
                    () => GetModPlayerValue<int>(noxus, Main.LocalPlayer, "NoxusAuricSoulPlayer", "goodAppleCount"),
                    () => int.MaxValue, GetLoc("GoodApple"), Color.Pink);
            }
        }
    }

    private void AddCalamityConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("CalamityMod", out Mod cal)) return;
        string rev = Language.GetTextValue("Mods.MunchiesAuricSoulsAddon.Difficulty.Calamity");
        // Health
        RegisterConsumable(munchies, cal, "MiracleFruit", "CalamityPlayer", "mFruit");
        RegisterConsumable(munchies, cal, cal.Version < new Version(2, 1) ? "BloodOrange" : "SanguineTangerine", "CalamityPlayer", "sTangerine");
        RegisterConsumable(munchies, cal, cal.Version < new Version(2, 1) ? "Elderberry" : "TaintedCloudberry", "CalamityPlayer", cal.Version < new Version(2, 1) ? "eBerry" : "tCloudberry");
        RegisterConsumable(munchies, cal, cal.Version < new Version(2, 1) ? "Dragonfruit" : "SacredStrawberry", "CalamityPlayer", cal.Version < new Version(2, 1) ? "dFruit" : "sStrawberry");
        // Mana
        if (cal.TryFind("EnchantedStarfish", out ModItem starfish))
        {
            CallMunchiesMulti(munchies, cal, starfish, () => Main.LocalPlayer.ConsumedManaCrystals, () => 9, GetLoc("EnchantedStarfish"));
        }
        RegisterConsumable(munchies, cal, "CometShard", "CalamityPlayer", "cShard");
        RegisterConsumable(munchies, cal, "EtherealCore", "CalamityPlayer", "eCore");
        RegisterConsumable(munchies, cal, "PhantomHeart", "CalamityPlayer", "pHeart");
        // Rage & Adrenaline
        RegisterConsumable(munchies, cal, "MushroomPlasmaRoot", "CalamityPlayer", "rageBoostOne", Color.Red, rev, "RageEnabled");
        RegisterConsumable(munchies, cal, "InfernalBlood", "CalamityPlayer", "rageBoostTwo", Color.Red, rev, "RageEnabled");
        RegisterConsumable(munchies, cal, "RedLightningContainer", "CalamityPlayer", "rageBoostThree", Color.Red, rev, "RageEnabled");
        RegisterConsumable(munchies, cal, "ElectrolyteGelPack", "CalamityPlayer", "adrenalineBoostOne", Color.Red, rev, "AdrenalineEnabled");
        RegisterConsumable(munchies, cal, "StarlightFuelCell", "CalamityPlayer", "adrenalineBoostTwo", Color.Red, rev, "AdrenalineEnabled");
        RegisterConsumable(munchies, cal, "Ectoheart", "CalamityPlayer", "adrenalineBoostThree", Color.Red, rev, "AdrenalineEnabled");
        // Acc Slot
        RegisterConsumable(munchies, cal, "CelestialOnion", "CalamityPlayer", "extraAccessoryML");
    }

    private void AddRagnarokConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("RagnarokMod", out Mod ragnarok)) return;

        if (ragnarok.TryFind("InspirationEssence", out ModItem essence))
        {
            CallMunchiesMulti(munchies, ragnarok, essence,
                () => { GetInspirationConsumeParameters(ragnarok, "InspirationEssence", Main.LocalPlayer, out int c, out _); return c; },
                () => { GetInspirationConsumeParameters(ragnarok, "InspirationEssence", Main.LocalPlayer, out _, out int t); return t; },
                GetLoc("InspirationEssence"));
        }

        if (ragnarok.TryFind("InspirationSingularity", out ModItem singularity))
        {
            CallMunchiesMulti(munchies, ragnarok, singularity,
                () => { GetInspirationConsumeParameters(ragnarok, "InspirationSingularity", Main.LocalPlayer, out int c, out _); return c; },
                () => { GetInspirationConsumeParameters(ragnarok, "InspirationSingularity", Main.LocalPlayer, out _, out int t); return t; },
                GetLoc("InspirationSingularity"));
        }
    }

    private void AddInfernalEclipseConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("InfernalEclipseAPI", out Mod ie)) return;

        if (ie.TryFind("SingularityCore", out ModItem core))
        {
            CallMunchiesSingle(munchies, ie, core, () =>
            {
                try { return !core.CanUseItem(Main.LocalPlayer); }
                catch { return false; }
            }, GetLoc("SingularityCore"));
        }

        if (ie.TryFind("RuinousPlasmaInjection", out ModItem plasma))
        {
            CallMunchiesMulti(munchies, ie, plasma,
                () => GetModPlayerValue<int>(ie, Main.LocalPlayer, "InfernalPlayer", "ruinousPlasmaInjection"),
                () => 5, GetLoc("RuinousPlasmaInjection"));
        }
    }

    private void AddSOTSConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("SOTS", out Mod sots)) return;

        RegisterSOTSMultiUseConsumable(munchies, sots, "VioletStar", "voidStar", 1);
        RegisterSOTSMultiUseConsumable(munchies, sots, "ScarletStar", "voidStar", 1);
        RegisterSOTSMultiUseConsumable(munchies, sots, "VoidenAnkh", "voidAnkh", 5);

        if (sots.TryFind("SoulHeart", out ModItem soulHeart))
        {
            CallMunchiesSingle(munchies, sots, soulHeart,
                () => GetModPlayerValue<int>(sots, Main.LocalPlayer, "VoidPlayer", "voidSoul") >= 1,
                GetLoc("SoulHeart"));
        }
    }

    private void RegisterSOTSMultiUseConsumable(Mod munchies, Mod sots, string itemName, string fieldName, int totalUses)
    {
        if (sots.TryFind(itemName, out ModItem item))
        {
            CallMunchiesMulti(munchies, sots, item,
                () => GetModPlayerValue<int>(sots, Main.LocalPlayer, "VoidPlayer", fieldName),
                () => totalUses, GetLoc(itemName));
        }
    }

    private void AddCoJConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("ContinentOfJourney", out Mod coj)) return;
        // Main
        RegisterConsumable(munchies, coj, "SunsHeart", "OtherUpgradesPlayer", "SunsHeart", Color.White, category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "GreatCrystal", "OtherUpgradesPlayer", "GreatCrystal", Color.White, category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "AirHandcanon", "OtherUpgradesPlayer", "AirHandcanon", Color.White, category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "HotCase", "OtherUpgradesPlayer", "HotCase", Color.White, category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "WhimInABottle", "OtherUpgradesPlayer", "WhimInABottle", Color.White, category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "HeartOfOcean", "OtherUpgradesPlayer", "HeartOfOcean", Color.White, category: "Homeward Journey", isIntCheck: true);
        if (!ModLoader.TryGetMod("HomewardRagnarok", out Mod hr) && GetModConfigValue<bool>(hr, "ServerConfig", "PermanentToAccessories"))
        {
            RegisterConsumable(munchies, coj, "TheSwitch", "OtherUpgradesPlayer", "TheSwitch", Color.White, category: "Homeward Journey", isIntCheck: true);
        }
        // Coffee
        RegisterConsumable(munchies, coj, "Americano", "CoffeePlayer", "Americano", new Color(117, 57, 18), category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "Latte", "CoffeePlayer", "Latte", new Color(117, 57, 18), category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "Mocha", "CoffeePlayer", "Mocha", new Color(117, 57, 18), category: "Homeward Journey", isIntCheck: true);
        RegisterConsumable(munchies, coj, "Cappuccino", "CoffeePlayer", "Cappuccino", new Color(117, 57, 18), category: "Homeward Journey", isIntCheck: true);
    }

    private void AddFargoConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("FargowiltasSouls", out Mod fargo)) return;
        string eternity = Language.GetTextValue("Mods.MunchiesAuricSoulsAddon.Difficulty.Fargo");
        RegisterConsumable(munchies, fargo, "RabiesVaccine", "FargoSoulsPlayer", "RabiesVaccine", new Color(51, 255, 191), eternity, category: "Fargo's Souls Mod");
        RegisterConsumable(munchies, fargo, "DeerSinew", "FargoSoulsPlayer", "DeerSinew", new Color(51, 255, 191), eternity, category: "Fargo's Souls Mod");
        RegisterConsumable(munchies, fargo, "MutantsDiscountCard", "FargoSoulsPlayer", "MutantsDiscountCard", new Color(51, 255, 191), eternity, category: "Fargo's Souls Mod");
        RegisterConsumable(munchies, fargo, "MutantsCreditCard", "FargoSoulsPlayer", "MutantsCreditCard", new Color(51, 255, 191), eternity, category: "Fargo's Souls Mod");
        RegisterConsumable(munchies, fargo, "MutantsPact", "FargoSoulsPlayer", "MutantsPactSlot", new Color(51, 255, 191), eternity, category: "Fargo's Souls Mod", isIntCheck: false);
    }

    private void AddRedemptionConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("Redemption", out Mod redemption)) return;
        RegisterConsumable(munchies, redemption, "GalaxyHeart", "RedePlayer", "galaxyHeart", Color.White, category: "Mod of Redemption", isIntCheck: false);
        RegisterConsumable(munchies, redemption, "MedicKit", "RedePlayer", "medKit", Color.White, category: "Mod of Redemption", isIntCheck: false);
    }

    private void AddQoTConsumables(Mod munchies)
    {
        if (!ModLoader.TryGetMod("ImproveGame", out Mod qot)) return;
        RegisterWorldConsumable(munchies, qot, "ShellShipInBottle_Shimmered", "QuickShimmerSystem", "Unlocked");
        RegisterWorldConsumable(munchies, qot, "WeatherBook", "WeatherController", "Unlocked");
    }

    // Core functions
    private void RegisterConsumable(Mod munchies, Mod targetMod, string itemName, string className, string fieldName, Color? color = null, string difficulty = "classic", string availabilityField = null, string category = "player", bool isIntCheck = false, Mod displayMod = null)
    {
        if (targetMod.TryFind(itemName, out ModItem item))
        {
            Func<bool> consumedCheck = isIntCheck
                ? () => GetModPlayerValue<int>(targetMod, Main.LocalPlayer, className, fieldName) >= 1
                : () => GetModPlayerValue<bool>(targetMod, Main.LocalPlayer, className, fieldName);

            Func<bool> availabilityCheck = null;
            if (!string.IsNullOrEmpty(availabilityField))
            {
                availabilityCheck = () => GetModPlayerValue<bool>(targetMod, Main.LocalPlayer, className, availabilityField);
            }

            Mod finalTabMod = displayMod ?? targetMod;
            CallMunchiesSingle(munchies, finalTabMod, item, consumedCheck, GetLoc(itemName), color, difficulty, availabilityCheck, category);
        }
    }

    private void RegisterWorldConsumable(Mod munchies, Mod targetMod, string itemName, string systemName, string fieldName, Color? color = null, string difficulty = "classic", string availabilityField = null)
    {
        if (targetMod.TryFind(itemName, out ModItem item))
        {
            Func<bool> consumedCheck = () => GetModSystemValue(targetMod, systemName, fieldName);
            Func<bool> availabilityCheck = null;
            if (!string.IsNullOrEmpty(availabilityField))
            {
                availabilityCheck = () => GetModSystemValue(targetMod, systemName, availabilityField);
            }
            CallMunchiesSingle(munchies, targetMod, item, consumedCheck, GetLoc(itemName), color, difficulty, availabilityCheck, "world");
        }
    }

    private bool GetModSystemValue(Mod mod, string systemName, string fieldName)
    {
        try
        {
            if (mod.TryFind(systemName, out ModSystem system))
            {
                Type type = system.GetType();
                FieldInfo field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) return (bool)field.GetValue(null);

                PropertyInfo prop = type.GetProperty(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null) return (bool)prop.GetValue(null);
            }
        }
        catch { }
        return false;
    }

    private T GetModPlayerValue<T>(Mod mod, Player player, string className, string memberName)
    {
        string classKey = $"{mod.Name}.{className}";
        string memberKey = $"{classKey}.{memberName}";

        if (!_modPlayerTypeCache.TryGetValue(classKey, out ModPlayer modPlayerBase))
        {
            if (mod.TryFind(className, out modPlayerBase))
                _modPlayerTypeCache[classKey] = modPlayerBase;
            else
                return default;
        }

        if (!player.TryGetModPlayer(modPlayerBase, out ModPlayer modPlayerInstance))
            return default;

        if (!_reflectionCache.TryGetValue(memberKey, out MemberInfo member))
        {
            Type type = modPlayerInstance.GetType();
            member = (MemberInfo)type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     ?? type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (member != null) _reflectionCache[memberKey] = member;
            else return default;
        }

        return member switch
        {
            FieldInfo f => (T)f.GetValue(modPlayerInstance),
            PropertyInfo p => (T)p.GetValue(modPlayerInstance),
            _ => default
        };
    }

    private void CallMunchiesSingle(Mod munchies, Mod source, ModItem item, Func<bool> consumed, LocalizedText text, Color? color = null, string diff = "classic", Func<bool> avail = null, string category = "player")
    {
        munchies.Call("AddSingleConsumable", source, "1.4", item, category, consumed, color, diff, null, avail, text);
    }

    private void CallMunchiesMulti(Mod munchies, Mod source, ModItem item, Func<int> current, Func<int> total, LocalizedText text, Color? color = null, string diff = "classic", Func<bool> avail = null, string category = "player")
    {
        munchies.Call("AddMultiUseConsumable", source, "1.4", item, category, current, total, color, diff, null, avail, text);
    }

    private LocalizedText GetLoc(string key) => Language.GetOrRegister($"Mods.{Name}.Acquisition.{key}", () => key);

    // Specific Stuff
    private T GetModConfigValue<T>(Mod mod, string configClassName, string propertyName)
    {
        try
        {
            if (mod.TryFind(configClassName, out ModConfig config))
            {
                PropertyInfo prop = config.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                return prop != null ? (T)prop.GetValue(config) : default;
            }
            return default;
        }
        catch { return default; }
    }

    private void GetInspirationConsumeParameters(Mod sourceMod, string itemName, Player player, out int count, out int total)
    {
        count = 0; total = 1;
        if (sourceMod == null || player == null || !sourceMod.TryFind(itemName, out ModItem modItem)) return;

        try
        {
            if (!TryGetInspirationConsumableApi(out Type baseType, out MethodInfo getConsumeParameters) || !baseType.IsAssignableFrom(modItem.GetType())) return;

            object[] parameters = { modItem, player, 0, 1 };
            getConsumeParameters.Invoke(null, parameters);

            if (parameters[2] is int num) count = num;
            if (parameters[3] is int num2) total = num2;
        }
        catch { }
    }

    private bool TryGetInspirationConsumableApi(out Type baseType, out MethodInfo getConsumeParameters)
    {
        baseType = inspirationConsumableBaseType;
        getConsumeParameters = inspirationConsumeParametersMethod;
        if (baseType != null && getConsumeParameters != null) return true;

        if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
        {
            baseType = thorium.Code.GetType("ThoriumMod.Items.BardItems.InspirationConsumableBase");
            if (baseType != null)
            {
                getConsumeParameters = baseType.GetMethod("GetConsumeParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (getConsumeParameters != null)
                {
                    inspirationConsumableBaseType = baseType;
                    inspirationConsumeParametersMethod = getConsumeParameters;
                    return true;
                }
            }
        }
        return false;
    }
}