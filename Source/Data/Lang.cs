using Verse;

namespace PawnRules.Data
{
    internal static class Lang
    {
        public static string Get(string key, params object[] args) => (Mod.Id + "." + key).Translate(args);
    }
}
