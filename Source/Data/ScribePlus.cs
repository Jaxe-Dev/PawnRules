using System.Collections.Generic;
using Verse;

namespace PawnRules.Data
{
    internal static class ScribePlus
    {
        public static List<T> LookList<T>(List<T> list, string key, bool ignoreIfEmpty = true)
        {
            if ((Scribe.mode == LoadSaveMode.Saving) && !ignoreIfEmpty && ((list == null) || (list.Count == 0))) { return list; }
            Scribe_Collections.Look(ref list, key, LookMode.Deep);

            return list ?? new List<T>();
        }

        public static T LookReference<T>(T reference, string key, T defaultValue = null) where T : class, ILoadReferenceable
        {
            if ((Scribe.mode == LoadSaveMode.Saving) && Equals(reference, defaultValue)) { return reference; }
            Scribe_References.Look(ref reference, key);

            return reference ?? defaultValue;
        }

        public static T LookValue<T>(T value, string key, T defaultValue = default(T))
        {
            Scribe_Values.Look(ref value, key, defaultValue);
            return value;
        }
    }
}
