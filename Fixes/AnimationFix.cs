using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Reflection;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Munchies.UIElements;
using MonoMod.RuntimeDetour;

namespace MunchiesAuricSoulsAddon.Fixes
{
    public class AnimationFix : ModSystem
    {
        private static FieldInfo _textureField;
        private static Hook _drawSelfHook;
        private static Dictionary<int, Item> _dummyItems = new Dictionary<int, Item>();

        private delegate void orig_DrawSelf(CenteredUIImage self, SpriteBatch spriteBatch);

        public override void Load()
        {
            _textureField = typeof(CenteredUIImage).GetField("_texture", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo drawSelfMethod = typeof(CenteredUIImage).GetMethod("DrawSelf", BindingFlags.NonPublic | BindingFlags.Instance);

            if (drawSelfMethod != null)
            {
                _drawSelfHook = new Hook(drawSelfMethod, Hook_DrawSelf);
            }
        }

        public override void Unload()
        {
            _drawSelfHook?.Dispose();
            _drawSelfHook = null;
            _textureField = null;

            _dummyItems?.Clear();
            _dummyItems = null;
        }

        private void Hook_DrawSelf(orig_DrawSelf orig, CenteredUIImage self, SpriteBatch spriteBatch)
        {
            var textureAsset = (Asset<Texture2D>)_textureField?.GetValue(self);

            if (textureAsset == null || !textureAsset.IsLoaded)
            {
                orig(self, spriteBatch);
                return;
            }

            int itemID = -1;
            for (int i = 0; i < TextureAssets.Item.Length; i++)
            {
                if (TextureAssets.Item[i] == textureAsset)
                {
                    itemID = i;
                    break;
                }
            }

            if (itemID <= 0)
            {
                orig(self, spriteBatch);
                return;
            }

            if (!_dummyItems.TryGetValue(itemID, out Item dummyItem))
            {
                dummyItem = new Item();
                dummyItem.SetDefaults(itemID);
                _dummyItems[itemID] = dummyItem;
            }

            Texture2D texture = textureAsset.Value;
            Rectangle sourceRect;

            if (Main.itemAnimations[itemID] != null)
            {
                sourceRect = Main.itemAnimations[itemID].GetFrame(texture);
            }
            else
            {
                sourceRect = texture.Frame(1, 1, 0, 0);
            }

            Vector2 origin = sourceRect.Size() * 0.5f;
            Vector2 position = self.GetDimensions().Center();

            // check for PreDrawInInventory
            bool letDefaultDraw = ItemLoader.PreDrawInInventory(dummyItem, spriteBatch, position, sourceRect, self.Color, Color.White, origin, self.ImageScale);

            if (letDefaultDraw)
            {
                if (Main.itemAnimations[itemID] != null)
                {
                    // animated sheet
                    spriteBatch.Draw(
                        texture: texture,
                        position: position,
                        sourceRectangle: sourceRect,
                        color: self.Color,
                        rotation: self.Rotation,
                        origin: origin,
                        scale: self.ImageScale,
                        effects: SpriteEffects.None,
                        layerDepth: 0f
                    );
                }
                else
                {
                    // static item
                    orig(self, spriteBatch);
                }
            }

            // include PostDrawInInventory
            ItemLoader.PostDrawInInventory(dummyItem, spriteBatch, position, sourceRect, self.Color, Color.White, origin, self.ImageScale);
        }
    }
}