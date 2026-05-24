using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace MunchiesAuricSoulsAddon
{
    // Maybe someday I will do it as API with mod calls and etc
    public class DrawAPI : ModSystem
    {
        public static Dictionary<string, Asset<Texture2D>> CustomImages = new();
        public static Dictionary<string, bool> ImageToggledStates = new();
        public static Dictionary<string, float> AnimationStartTimes = new();
        public static Dictionary<string, LocalizedText> CategoriesToInject = new();

        private static string ItemToDrawImageFor = null;
        private static string CurrentlyHoveredToggledItem = null;

        private Hook drawSelfHook;
        private Hook rightClickHook;
        private Hook tooltipHook;
        private Hook redrawHook;

        private static FieldInfo itemField;

        public static Asset<Texture2D> PanelBg;
        public static Asset<Texture2D> PanelBorder;

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server) return;

            PanelBg = Main.Assets.Request<Texture2D>("Images/UI/PanelBackground");
            PanelBorder = Main.Assets.Request<Texture2D>("Images/UI/PanelBorder");

            if (ModLoader.TryGetMod("Munchies", out Mod munchies))
            {
                Type reportItemType = munchies.Code.GetType("Munchies.UIElements.ReportListItem");
                Type reportUIType = munchies.Code.GetType("Munchies.UIElements.ReportUI");

                if (reportItemType != null)
                {
                    MethodInfo drawSelfMethod = reportItemType.GetMethod("DrawSelf", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    MethodInfo rightClickMethod = reportItemType.GetMethod("RightClick", BindingFlags.Instance | BindingFlags.Public);

                    if (drawSelfMethod != null) drawSelfHook = new Hook(drawSelfMethod, DrawSelf_Detour);
                    if (rightClickMethod != null) rightClickHook = new Hook(rightClickMethod, RightClick_Detour);
                }

                if (reportUIType != null)
                {
                    MethodInfo redrawMethod = reportUIType.GetMethod("RedrawConsumablesList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (redrawMethod != null) redrawHook = new Hook(redrawMethod, Redraw_Detour);
                }

                MethodInfo tooltipMethod = typeof(UICommon).GetMethod("TooltipMouseText", BindingFlags.Static | BindingFlags.Public);
                if (tooltipMethod != null) tooltipHook = new Hook(tooltipMethod, TooltipMouseText_Detour);
            }
        }

        public override void Unload()
        {
            drawSelfHook?.Dispose();
            rightClickHook?.Dispose();
            tooltipHook?.Dispose();
            redrawHook?.Dispose();
            CustomImages.Clear();
            ImageToggledStates.Clear();
            AnimationStartTimes.Clear();
            CategoriesToInject.Clear();
            PanelBg = null;
            PanelBorder = null;
        }

        public static void RegisterImage(string itemName, string imagePath)
        {
            if (!CustomImages.ContainsKey(itemName))
                CustomImages[itemName] = ModContent.Request<Texture2D>(imagePath);
        }

        public static void RegisterCategory(string targetItemName, LocalizedText categoryName)
        {
            CategoriesToInject[targetItemName] = categoryName;
        }

        private delegate void RedrawDelegate(object self);
        private void Redraw_Detour(RedrawDelegate orig, object self)
        {
            orig(self);
            try
            {
                FieldInfo listField = self.GetType().GetField("reportList", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (listField == null) return;

                UIList reportList = (UIList)listField.GetValue(self);
                if (reportList == null || reportList.Count == 0) return;
                List<UIElement> oldItems = new List<UIElement>();
                foreach (UIElement item in reportList)
                {
                    oldItems.Add(item);
                }

                reportList.Clear();

                foreach (UIElement item in oldItems)
                {
                    if (item.GetType().Name == "ReportListItem")
                    {
                        string itemName = GetItemNameFromReportItem(item);
                        if (itemName != null && CategoriesToInject.ContainsKey(itemName))
                        {
                            CategoryHeaderElement header = new CategoryHeaderElement(CategoriesToInject[itemName]);
                            reportList.Add(header);
                        }
                    }
                    reportList.Add(item);
                }
                reportList.Recalculate();
            }
            catch { }
        }

        private delegate void RightClickDelegate(UIElement self, UIMouseEvent evt);
        private void RightClick_Detour(RightClickDelegate orig, UIElement self, UIMouseEvent evt)
        {
            orig(self, evt);
            if (self.GetType().Name != "ReportListItem") return;

            string itemName = GetItemNameFromReportItem(self);
            if (itemName != null && CustomImages.ContainsKey(itemName))
            {
                bool currentState = ImageToggledStates.GetValueOrDefault(itemName, false);
                ImageToggledStates[itemName] = !currentState;

                if (!currentState) AnimationStartTimes[itemName] = Main.GlobalTimeWrappedHourly;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        private delegate void DrawSelfDelegate(UIElement self, SpriteBatch spriteBatch);
        private void DrawSelf_Detour(DrawSelfDelegate orig, UIElement self, SpriteBatch spriteBatch)
        {
            if (self.IsMouseHovering && self.GetType().Name == "ReportListItem")
            {
                string itemName = GetItemNameFromReportItem(self);
                if (itemName != null && CustomImages.ContainsKey(itemName) && ImageToggledStates.GetValueOrDefault(itemName, false))
                {
                    CurrentlyHoveredToggledItem = itemName;
                    ItemToDrawImageFor = itemName;
                }
            }
            orig(self, spriteBatch);
            CurrentlyHoveredToggledItem = null;
        }

        private delegate void TooltipMouseTextDelegate(string text);
        private void TooltipMouseText_Detour(TooltipMouseTextDelegate orig, string text)
        {
            if (CurrentlyHoveredToggledItem != null) return;
            orig(text);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("MunchiesAuricSoulsAddon: Image Overlay", delegate {
                    if (ItemToDrawImageFor != null)
                    {
                        DrawHoverImage(Main.spriteBatch);
                        ItemToDrawImageFor = null;
                    }
                    return true;
                }, InterfaceScaleType.UI));
            }
        }

        private static void DrawHoverImage(SpriteBatch spriteBatch)
        {
            if (CustomImages.TryGetValue(ItemToDrawImageFor, out Asset<Texture2D> texAsset) && texAsset.IsLoaded)
            {
                Texture2D tex = texAsset.Value;
                Vector2 drawPos = Main.MouseScreen + new Vector2(20, 20);
                int padding = 10;
                int drawWidth = tex.Width;
                int drawHeight = tex.Height;
                Rectangle sourceRect = new Rectangle(0, 0, drawWidth, drawHeight);

                if (tex.Width == 1500 && tex.Height == 8648)
                {
                    drawWidth = 250;
                    drawHeight = 184;
                    int columns = 6;
                    int totalFrames = 279;
                    int fps = 15;
                    float startTime = AnimationStartTimes.GetValueOrDefault(ItemToDrawImageFor, Main.GlobalTimeWrappedHourly);
                    float elapsedTime = Main.GlobalTimeWrappedHourly - startTime;
                    int currentFrame = (int)(elapsedTime * fps) % totalFrames;
                    int col = currentFrame % columns;
                    int row = currentFrame / columns;
                    sourceRect = new Rectangle(col * drawWidth, row * drawHeight, drawWidth, drawHeight);
                }

                Rectangle panelRect = new Rectangle((int)drawPos.X, (int)drawPos.Y, drawWidth + padding * 2, drawHeight + padding * 2);
                Color bgColor = new Color(63, 82, 151) * 0.9f;
                Utils.DrawSplicedPanel(spriteBatch, PanelBg.Value, panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height, 10, 10, 10, 10, bgColor);
                Utils.DrawSplicedPanel(spriteBatch, PanelBorder.Value, panelRect.X, panelRect.Y, panelRect.Width, panelRect.Height, 10, 10, 10, 10, Color.Black);
                spriteBatch.Draw(tex, drawPos + new Vector2(padding, padding), sourceRect, Color.White);
            }
        }

        private static string GetItemNameFromReportItem(UIElement reportListItem)
        {
            if (reportListItem == null || reportListItem.GetType().Name != "ReportListItem") return null;
            try
            {
                FieldInfo consumableField = reportListItem.GetType().GetField("Consumable", BindingFlags.Public | BindingFlags.Instance);
                if (consumableField != null)
                {
                    object consumable = consumableField.GetValue(reportListItem);
                    if (consumable != null)
                    {
                        PropertyInfo itemProp = consumable.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance);
                        FieldInfo itemField2 = consumable.GetType().GetField("Item", BindingFlags.Public | BindingFlags.Instance);

                        Item terrariaItem = null;
                        if (itemProp != null) terrariaItem = (Item)itemProp.GetValue(consumable);
                        else if (itemField2 != null) terrariaItem = (Item)itemField2.GetValue(consumable);

                        if (terrariaItem != null && terrariaItem.ModItem != null)
                        {
                            return terrariaItem.ModItem.Name;
                        }
                    }
                }
                itemField ??= reportListItem.GetType().GetField("Item", BindingFlags.NonPublic | BindingFlags.Instance);
                if (itemField != null)
                {
                    Item item = (Item)itemField.GetValue(reportListItem);
                    if (item != null && item.ModItem != null) return item.ModItem.Name;
                }
            }
            catch { }
            return null;
        }
    }

    public class CategoryHeaderElement : UIElement
    {
        public LocalizedText CategoryName;

        public CategoryHeaderElement(LocalizedText name)
        {
            CategoryName = name;

            Height.Set(28f, 0f);
            Width.Set(-10f, 1f);
            Left.Set(10f, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dims = GetDimensions();
            Color bgColor = new Color(35, 45, 95) * 0.95f;

            Utils.DrawSplicedPanel(spriteBatch, DrawAPI.PanelBg.Value, (int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height, 10, 10, 10, 10, bgColor);
            Utils.DrawSplicedPanel(spriteBatch, DrawAPI.PanelBorder.Value, (int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height, 10, 10, 10, 10, Color.Black);

            string text = CategoryName.Value;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
            Vector2 textPos = new Vector2(dims.X + dims.Width / 2 - textSize.X / 2, dims.Y + dims.Height / 2 - textSize.Y / 2);

            Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, text, textPos.X, textPos.Y, new Color(127, 148, 181), Color.Black, Vector2.Zero);
        }
    }
}