using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace PawnRules.Data
{
    internal static class AddonManager
    {
        private static readonly Regex KeyRegex = new Regex("[a-zA-Z0-9_]+");

        private static readonly Dictionary<string, AddonOption> OptionRegistry = new Dictionary<string, AddonOption>();
        private static readonly List<ModContentPack> ModRegistry = new List<ModContentPack>();

        public static bool AcceptingAddons { get; set; } = true;
        public static bool HasAddons { get; private set; }

        public static IEnumerable<AddonOption> Options => OptionRegistry.Values;
        public static IEnumerable<ModContentPack> Mods => ModRegistry;

        public static void Add(AddonOption addon)
        {
            if (Registry.IsActive || !AcceptingAddons) { throw new Mod.Exception("Options must be added before a world is loaded"); }
            if (string.IsNullOrEmpty(addon.Key)) { throw new Mod.Exception("Key is null or empty"); }
            if (!KeyRegex.IsMatch(addon.Key)) { throw new Mod.Exception("Key must not contain any non-alphanumeric characters except for an underscore"); }
            if (OptionRegistry.ContainsKey(addon.Key)) { throw new Mod.Exception("Key already exists"); }

            if (!ModRegistry.Contains(addon.Link.ModContentPack)) { ModRegistry.Add(addon.Link.ModContentPack); }
            OptionRegistry.Add(addon.Key, addon);

            HasAddons = true;
        }
    }
}
