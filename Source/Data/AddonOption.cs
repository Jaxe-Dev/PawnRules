using System;
using PawnRules.API;
using PawnRules.Interface;
using Verse;

namespace PawnRules.Data
{
    internal class AddonOption
    {
        public string Key { get; }
        public OptionTarget Target { get; }
        public OptionWidget Widget { get; }
        public string Label { get; set; }
        public string Tooltip { get; set; }
        public Type Type { get; }

        public object DefaultValue { get; }
        public bool AllowedInPreset { get; }

        public OptionHandle Handle { get; private set; }
        public PawnRulesLink Link { get; }
        public float WidgetHeight => (Widget == OptionWidget.Button ? 30f : Text.LineHeight) + 2f;

        private AddonOption(PawnRulesLink link, string key, OptionTarget target, OptionWidget widget, string label, string tooltip, Type type, object defaultValue, bool allowedInPreset)
        {
            Link = link;
            Key = key;
            Target = target;
            Widget = widget;
            Label = label;
            Tooltip = tooltip;
            Type = type;
            DefaultValue = defaultValue;
            AllowedInPreset = allowedInPreset;
        }

        internal static OptionHandle<T> Add<T>(PawnRulesLink link, string key, OptionTarget target, OptionWidget widget, string label, string tooltip, T defaultValue, bool allowedInPreset = true)
        {
            var option = new AddonOption(link, link.ModContentPack.Identifier + "_" + key, target, widget, label, tooltip, typeof(T), defaultValue, allowedInPreset);

            AddonManager.Add(option);

            var handle = new OptionHandle<T>(option);
            option.Handle = handle;

            return handle;
        }
    }
}
