using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace MunchiesAuricSoulsAddon;

public class MunchiesAuricSoulsAddon : Mod
{
    internal static MunchiesAuricSoulsAddon instance;

    // Mod Instances
    internal Mod MunchiesMod, CalamityHuntMod, CalamityMod, NoxusBossMod;
    internal Mod RagnarokMod, ThoriumMod, InfernalEclipseAPI, ContinentOfJourneyMod, HomewardRagnarokMod;

    // Types
    private Type inspirationConsumableBaseType;
    private MethodInfo inspirationConsumeParametersMethod;

    public override void Load()
    {
        instance = this;
    }

    public override void Unload()
    {
        instance = null;
        MunchiesMod = CalamityHuntMod = CalamityMod = NoxusBossMod = null;
        RagnarokMod = ThoriumMod = InfernalEclipseAPI = ContinentOfJourneyMod = HomewardRagnarokMod = null;
        inspirationConsumableBaseType = null;
        inspirationConsumeParametersMethod = null;
    }

    public override void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("Munchies", out MunchiesMod))
        {
            Logger.Error("Munchies mod not found! This addon requires Munchies to function.");
            return;
        }

        // Dynamically locate mods
        ModLoader.TryGetMod("CalamityHunt", out CalamityHuntMod);
        ModLoader.TryGetMod("CalamityMod", out CalamityMod);
        ModLoader.TryGetMod("NoxusBoss", out NoxusBossMod);
        ModLoader.TryGetMod("RagnarokMod", out RagnarokMod);
        ModLoader.TryGetMod("ThoriumMod", out ThoriumMod);
        ModLoader.TryGetMod("InfernalEclipseAPI", out InfernalEclipseAPI);
        ModLoader.TryGetMod("ContinentOfJourney", out ContinentOfJourneyMod);
        ModLoader.TryGetMod("HomewardRagnarok", out HomewardRagnarokMod);

        try
        {
            AddAuricSouls();
            AddCalamityConsumables();
            AddRagnarokConsumables();
            AddInfernalEclipseConsumables();
            AddCoJConsumables();
        }
        catch (Exception ex)
        {
            Logger.Error($"PostSetupContent Error in Munchies Auric Souls Addon: {ex.Message}");
        }
    }

    // Helpers
    private LocalizedText GetLoc(string key) => this.GetLocalization($"Acquisition.{key}", () => key);

    private ModItem GetModItem(Mod mod, string name)
    {
        if (mod != null && mod.TryFind<ModItem>(name, out ModItem result))
            return result;
        return null;
    }

    private void CallMunchiesModConsumable(Mod sourceMod, ModItem item, Func<bool> hasBeenConsumed, LocalizedText acquisitionText, Color? customColor = null, string difficulty = "classic", Func<bool> availability = null, string category = "player")
    {
        if (MunchiesMod != null && item != null)
        {
            MunchiesMod.Call("AddSingleConsumable", sourceMod, "1.4", item, category, hasBeenConsumed, customColor, difficulty, null, availability, acquisitionText);
        }
    }

    private void CallMunchiesModMultiConsumable(Mod sourceMod, ModItem item, Func<int> currentCount, Func<int> totalCount, LocalizedText acquisitionText, Color? customColor = null, string difficulty = "classic", Func<bool> availability = null, string category = "player")
    {
        if (MunchiesMod != null && sourceMod != null && item != null)
        {
            MunchiesMod.Call("AddMultiUseConsumable", sourceMod, "1.4", item, category, currentCount, totalCount, customColor, difficulty, null, availability, acquisitionText);
        }
    }

    private object GetModPlayerObject(Mod mod, Player player, string className)
    {
        if (mod == null || player == null) return null;
        try
        {
            TypeInfo typeInfo = mod.Code.DefinedTypes.FirstOrDefault(t => t.Name == className);
            if (typeInfo == null) return null;
            return typeof(Player).GetMethod("GetModPlayer", Type.EmptyTypes)?.MakeGenericMethod(typeInfo.AsType()).Invoke(player, null);
        }
        catch { return null; }
    }

    private T GetModPlayerFieldOrProperty<T>(Mod mod, Player player, string className, string fieldName)
    {
        object modPlayer = GetModPlayerObject(mod, player, className);
        if (modPlayer == null) return default;

        Type type = modPlayer.GetType();

        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field != null && field.FieldType == typeof(T)) return (T)field.GetValue(modPlayer);

        PropertyInfo prop = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop != null && prop.PropertyType == typeof(T)) return (T)prop.GetValue(modPlayer);

        return default;
    }

    private T GetModConfigValue<T>(Mod mod, string configClassName, string propertyName)
    {
        if (mod == null) return default;
        try
        {
            Type configType = mod.Code.GetType($"{mod.Name}.{configClassName}");
            if (configType == null) return default;

            PropertyInfo instanceProp = configType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            object configInstance = instanceProp?.GetValue(null);

            if (configInstance == null) return default;

            PropertyInfo prop = configType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            return (T)prop.GetValue(configInstance);
        }
        catch { return default; }
    }

    // Hunt and Wrath
    private void AddAuricSouls()
    {
        if (CalamityHuntMod != null)
        {
            AddHuntSoul("YharonSoul", "yharonSoul");
            AddHuntSoul("GoozmaSoul", "goozmaSoul");
            AddHuntSoul("RottenSoul", "olddukeSoul");
            //if (ModLoader.HasMod("CalRemix")) AddHuntSoul("AshenSoul", "pyrogenSoul");
        }

        if (NoxusBossMod != null)
        {
            if (CalamityHuntMod != null)
            {
                AddNoxusSoul("AvatarAuricSoul", "hasAvatarSoul");
                AddNoxusSoul("NamelessAuricSoul", "hasNamelessSoul");
            }

            ModItem apple = GetModItem(NoxusBossMod, "GoodApple");
            if (apple != null)
            {
                CallMunchiesModMultiConsumable(NoxusBossMod, apple,
                    () => GetModPlayerFieldOrProperty<int>(NoxusBossMod, Main.LocalPlayer, "NoxusAuricSoulPlayer", "goodAppleCount"),
                    () => int.MaxValue, GetLoc("GoodApple"), Color.Pink);
            }
        }
    }

    private void AddHuntSoul(string itemName, string fieldName)
    {
        ModItem item = GetModItem(CalamityHuntMod, itemName);
        if (item != null) CallMunchiesModConsumable(CalamityHuntMod, item, () => GetModPlayerFieldOrProperty<bool>(CalamityHuntMod, Main.LocalPlayer, "AuricSoulPlayer", fieldName), GetLoc(itemName));
    }

    private void AddNoxusSoul(string itemName, string fieldName)
    {
        ModItem item = GetModItem(NoxusBossMod, itemName);
        if (item != null) CallMunchiesModConsumable(NoxusBossMod, item, () => GetModPlayerFieldOrProperty<bool>(NoxusBossMod, Main.LocalPlayer, "NoxusAuricSoulPlayer", fieldName), GetLoc(itemName));
    }

    // Calamity
    private void AddCalamityConsumables()
    {
        if (CalamityMod == null) return;

        // Health
        AddCalamityConsumable("MiracleFruit", "mFruit");
        if (CalamityMod.Version < new Version(2, 1))
        {
            AddCalamityConsumable("BloodOrange", "sTangerine"); 
            AddCalamityConsumable("Elderberry", "eBerry");
            AddCalamityConsumable("Dragonfruit", "dFruit");
        }
        else
        {
            AddCalamityConsumable("SanguineTangerine", "sTangerine");
            AddCalamityConsumable("TaintedCloudberry", "tCloudberry");
            AddCalamityConsumable("SacredStrawberry", "sStrawberry");
        }

        // Mana
        ModItem starfish = GetModItem(CalamityMod, "EnchantedStarfish");
        if (starfish != null) CallMunchiesModMultiConsumable(CalamityMod, starfish, () => Main.LocalPlayer.ConsumedManaCrystals, () => 9, GetLoc("EnchantedStarfish"));

        AddCalamityConsumable("CometShard", "cShard");
        AddCalamityConsumable("EtherealCore", "eCore");
        AddCalamityConsumable("PhantomHeart", "pHeart");

        // Rage
        AddCalamityConsumable("MushroomPlasmaRoot", "rageBoostOne", Color.Red, "Revengeance", "RageEnabled");
        AddCalamityConsumable("InfernalBlood", "rageBoostTwo", Color.Red, "Revengeance", "RageEnabled");
        AddCalamityConsumable("RedLightningContainer", "rageBoostThree", Color.Red, "Revengeance", "RageEnabled");

        // Adrenaline
        AddCalamityConsumable("ElectrolyteGelPack", "adrenalineBoostOne", Color.Red, "Revengeance", "AdrenalineEnabled");
        AddCalamityConsumable("StarlightFuelCell", "adrenalineBoostTwo", Color.Red, "Revengeance", "AdrenalineEnabled");
        AddCalamityConsumable("Ectoheart", "adrenalineBoostThree", Color.Red, "Revengeance", "AdrenalineEnabled");

        // Acc Slot
        AddCalamityConsumable("CelestialOnion", "extraAccessoryML");
    }

    private void AddCalamityConsumable(string itemName, string boolField, Color? color = null, string difficulty = "classic", string diffBoolField = null)
    {
        ModItem item = GetModItem(CalamityMod, itemName);
        if (item != null)
        {
            Func<bool> availability = diffBoolField != null ? () => GetModPlayerFieldOrProperty<bool>(CalamityMod, Main.LocalPlayer, "CalamityPlayer", diffBoolField) : null;
            CallMunchiesModConsumable(CalamityMod, item, () => GetModPlayerFieldOrProperty<bool>(CalamityMod, Main.LocalPlayer, "CalamityPlayer", boolField), GetLoc(itemName), color, difficulty, availability);
        }
    }

    // Ragnarok
    private void AddRagnarokConsumables()
    {
        if (RagnarokMod == null) return;
        ModItem essence = GetModItem(RagnarokMod, "InspirationEssence");
        if (essence != null)
        {
            CallMunchiesModMultiConsumable(RagnarokMod, essence,
                () => { GetInspirationConsumeParameters(RagnarokMod, "InspirationEssence", Main.LocalPlayer, out int c, out _); return c; },
                () => { GetInspirationConsumeParameters(RagnarokMod, "InspirationEssence", Main.LocalPlayer, out _, out int t); return t; },
                GetLoc("InspirationEssence"));
        }
    }

    private void GetInspirationConsumeParameters(Mod sourceMod, string itemName, Player player, out int count, out int total)
    {
        count = 0; total = 1;
        if (sourceMod == null || player == null) return;

        ModItem modItem = GetModItem(sourceMod, itemName);
        if (modItem == null) return;

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

        if (ThoriumMod == null) return false;

        baseType = ThoriumMod.Code.GetType("ThoriumMod.Items.BardItems.InspirationConsumableBase");
        if (baseType == null) return false;

        getConsumeParameters = baseType.GetMethod("GetConsumeParameters", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (getConsumeParameters == null) return false;

        inspirationConsumableBaseType = baseType;
        inspirationConsumeParametersMethod = getConsumeParameters;
        return true;
    }

    // IEoR
    private void AddInfernalEclipseConsumables()
    {
        if (InfernalEclipseAPI == null) return;
        ModItem core = GetModItem(InfernalEclipseAPI, "SingularityCore");
        if (core != null)
        {
            CallMunchiesModConsumable(InfernalEclipseAPI, core, () =>
            {
                try { return !core.CanUseItem(Main.LocalPlayer); }
                catch { return false; }
            }, GetLoc("SingularityCore"));
        }
    }

    // Homeward Journey
    private void AddCoJConsumables()
    {
        if (ContinentOfJourneyMod == null) return;

        Color white = Color.White;
        Color coffeeColor = new Color(117, 57, 18);

        // Main Items
        AddCoJItem("SunsHeart", "OtherUpgradesPlayer", "SunsHeart", white);
        AddCoJItem("GreatCrystal", "OtherUpgradesPlayer", "GreatCrystal", white);
        AddCoJItem("AirHandcanon", "OtherUpgradesPlayer", "AirHandcanon", white);
        AddCoJItem("HotCase", "OtherUpgradesPlayer", "HotCase", white);
        AddCoJItem("WhimInABottle", "OtherUpgradesPlayer", "WhimInABottle", white);

        // Switch
        bool skipSwitch = false;
        if (HomewardRagnarokMod != null)
        {
            skipSwitch = GetModConfigValue<bool>(HomewardRagnarokMod, "ServerConfig", "PermanentToAccessories");
        }
        if (!skipSwitch)
        {
            AddCoJItem("TheSwitch", "OtherUpgradesPlayer", "TheSwitch", white);
        }

        // Coffee Items
        AddCoJItem("Americano", "CoffeePlayer", "Americano", coffeeColor);
        AddCoJItem("Latte", "CoffeePlayer", "Latte", coffeeColor);
        AddCoJItem("Mocha", "CoffeePlayer", "Mocha", coffeeColor);
        AddCoJItem("Cappuccino", "CoffeePlayer", "Cappuccino", coffeeColor);
    }

    private void AddCoJItem(string itemName, string playerClass, string propName, Color color)
    {
        ModItem item = GetModItem(ContinentOfJourneyMod, itemName);
        if (item != null)
        {
            CallMunchiesModConsumable(ContinentOfJourneyMod, item,
                () => GetModPlayerFieldOrProperty<int>(ContinentOfJourneyMod, Main.LocalPlayer, playerClass, propName) >= 1,
                GetLoc(itemName), color, "classic", null, "Homeward Journey");
        }
    }

    private bool GetCoJPermanentUpgradeStatus(Player player, int index)
    {
        bool[] arr = GetModPlayerFieldOrProperty<bool[]>(ContinentOfJourneyMod, player, "PermanentUpgradesPlayer", "PermanentUpgradesObtained");
        return arr != null && arr.Length > index && arr[index];
    }
}