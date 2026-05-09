using System;
using System.Reflection;
using System.Collections;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;

namespace MunchiesAuricSoulsAddon.Fixes
{
    public class MunchiesUIFix : ModSystem
    {
        private FieldInfo _instanceField;
        private PropertyInfo _instanceProp;
        private FieldInfo _reportUIField;
        private PropertyInfo _reportUIProp;
        private FieldInfo _mainPanelField;
        private FieldInfo _reportPanelField;
        private FieldInfo _tabsField;

        private int _lastTabCount = -1;
        private int _initialTabCount = -1; 

        public override void PostSetupContent()
        {
            if (!ModLoader.TryGetMod("Munchies", out Mod munchies)) return;

            Type reportUISystemType = munchies.Code.GetType("Munchies.UIElements.ReportUISystem");
            Type reportUIType = munchies.Code.GetType("Munchies.UIElements.ReportUI");

            if (reportUISystemType != null && reportUIType != null)
            {
                var flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                _instanceField = reportUISystemType.GetField("Instance", flags);
                _instanceProp = reportUISystemType.GetProperty("Instance", flags);

                _reportUIField = reportUISystemType.GetField("ReportUI", flags);
                _reportUIProp = reportUISystemType.GetProperty("ReportUI", flags);

                _mainPanelField = reportUIType.GetField("mainPanel", flags);
                _reportPanelField = reportUIType.GetField("reportPanel", flags);
                _tabsField = reportUIType.GetField("tabs", flags);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            object sysInstance = _instanceField?.GetValue(null) ?? _instanceProp?.GetValue(null);
            if (sysInstance == null) return;

            object reportUI = _reportUIField?.GetValue(sysInstance) ?? _reportUIProp?.GetValue(sysInstance);
            if (reportUI == null) return;

            var tabs = _tabsField?.GetValue(reportUI) as IList;
            if (tabs == null || tabs.Count <= 1) return;

            var mainPanel = _mainPanelField?.GetValue(reportUI) as UIElement;
            var reportPanel = _reportPanelField?.GetValue(reportUI) as UIElement;

            if (mainPanel == null || reportPanel == null) return;

            if (_initialTabCount == -1)
            {
                _initialTabCount = tabs.Count;
            }

            bool needsFixing = false;

            if (_initialTabCount > 0 && tabs.Count > _initialTabCount)
            {
                while (tabs.Count > _initialTabCount)
                {
                    if (tabs[0] is UIElement oldTab)
                    {
                        oldTab.Remove(); 
                    }
                    tabs.RemoveAt(0);
                }
                needsFixing = true;
            }

            float panelWidth = 300f;
            float panelHeight = 500f;
            float tabSize = 36f;
            float spacing = 10f;
            int tabsPerRow = (int)((panelWidth - spacing) / tabSize);

            if (tabs.Count != _lastTabCount)
            {
                needsFixing = true;
            }

            if (tabs.Count > tabsPerRow && tabs[tabsPerRow] is UIElement overflowTab)
            {
                if (overflowTab.Top.Pixels == 0f)
                {
                    needsFixing = true;
                }
            }

            if (!needsFixing) return;

            int rowCount = ((tabs.Count - 1) / tabsPerRow) + 1;
            float topOffset = (rowCount * tabSize) - (tabSize * 0.25f);

            mainPanel.Height.Set(panelHeight + topOffset, 0f);
            reportPanel.Top.Set(topOffset, 0f);

            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i] is UIElement tab)
                {
                    int row = i / tabsPerRow;
                    int col = i % tabsPerRow;

                    tab.Top.Set(row * tabSize, 0f);
                    tab.Left.Set(spacing + (col * tabSize), 0f);
                }
            }

            mainPanel.Recalculate();
            _lastTabCount = tabs.Count;
        }
    }
}