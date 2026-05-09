using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MonoMod.RuntimeDetour;

namespace MunchiesAuricSoulsAddon.Fixes
{
    public class ItemTooltipFix : ModSystem
    {
        private Hook reportListItemCtorHook;

        private delegate void orig_ReportListItem_ctor(object self, object consumable);
        private delegate void hook_ReportListItem_ctor(orig_ReportListItem_ctor orig, object self, object consumable);

        public override void Load()
        {
            if (!ModLoader.TryGetMod("Munchies", out Mod munchies)) return;

            try
            {
                Type reportListItemType = munchies.Code.GetType("Munchies.UIElements.ReportListItem");
                Type consumableType = munchies.Code.GetType("Munchies.Models.Consumable");

                if (reportListItemType == null || consumableType == null) return;

                ConstructorInfo ctor = reportListItemType.GetConstructor(new Type[] { consumableType });
                if (ctor == null) return;

                reportListItemCtorHook = new Hook(
                    ctor,
                    new hook_ReportListItem_ctor(ReportListItem_Ctor_Detour)
                );
                reportListItemCtorHook.Apply();
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"Failed to apply Munchies UI Hook: {ex.Message}");
            }
        }

        public override void Unload()
        {
            reportListItemCtorHook?.Undo();
            reportListItemCtorHook?.Dispose();
            reportListItemCtorHook = null;
        }

        private void ReportListItem_Ctor_Detour(orig_ReportListItem_ctor orig, object self, object consumable)
        {
            orig(self, consumable);

            if (consumable == null) return;

            try
            {
                bool isVanilla = false;
                PropertyInfo vanillaProp = consumable.GetType().GetProperty("Vanilla", BindingFlags.Public | BindingFlags.Instance);
                if (vanillaProp != null) isVanilla = (bool)vanillaProp.GetValue(consumable);
                else
                {
                    FieldInfo vanillaField = consumable.GetType().GetField("Vanilla", BindingFlags.Public | BindingFlags.Instance);
                    if (vanillaField != null) isVanilla = (bool)vanillaField.GetValue(consumable);
                }

                if (!isVanilla)
                {
                    Item originalItem = null;
                    PropertyInfo itemProp = consumable.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
                    if (itemProp != null) originalItem = (Item)itemProp.GetValue(consumable);
                    else
                    {
                        FieldInfo itemField = consumable.GetType().GetField("Item", BindingFlags.Public | BindingFlags.Instance);
                        if (itemField != null) originalItem = (Item)itemField.GetValue(consumable);
                    }

                    if (originalItem != null && originalItem.type > ItemID.None)
                    {
                        if (ContentSamples.ItemsByType.TryGetValue(originalItem.type, out Item sampleItem))
                        {
                            Item fixedItem = sampleItem.Clone();
                            fixedItem.stack = 1; 

                            FieldInfo targetItemField = self.GetType().GetField("Item", BindingFlags.NonPublic | BindingFlags.Instance);
                            targetItemField?.SetValue(self, fixedItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"Munchies Hook execution failed: {ex.Message}");
            }
        }
    }
}