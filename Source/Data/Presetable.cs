using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PawnRules.Interface;
using Verse;

namespace PawnRules.Data
{
    internal abstract class Presetable : IExposable, ILoadReferenceable
    {
        private const int MaxIdLength = 20;

        public static readonly string VoidName = Lang.Get("Preset.None");

        private static readonly Regex ValidNameRegex = new Regex("^(?:[a-zA-Z0-9]|[a-zA-Z0-9]+[a-zA-Z0-9 ]*[a-zA-Z0-9]+)$");

        private static int _count;
        protected readonly int Id;

        public IPresetableType Type { get; protected set; }
        public string Name { get; set; }
        public bool IsVoid { get; protected set; }

        public bool IsPreset => !Name.NullOrEmpty();

        protected Presetable()
        {
            _count++;
            Id = _count;
        }

        protected Presetable(string name) : this() => Name = name;

        protected abstract string GetPresetId();

        protected abstract void ExposePresetData();
        internal abstract bool IsIgnored();
        public abstract XElement ToXml();

        public static void SetName<T>(IPresetableType type, Action<T> onCreate) where T : Presetable
        {
            var localType = type;
            void OnCommit(string name)
            {
                var preset = Registry.CreatePreset<T>(localType, name);
                onCreate(preset);
            }

            Dialog_SetName.Open(Lang.Get("Dialog_SetName.PresetTitleNew"), Lang.Get("Dialog_SetName.PresetLabel"), OnCommit, name => NameIsValid<T>(type, name));
        }

        public static void SetName<T>(T preset, Action<T> onRename) where T : Presetable
        {
            var localPreset = preset;
            void OnCommit(string name) => onRename(Registry.RenamePreset(localPreset, name));

            Dialog_SetName.Open(Lang.Get("Dialog_SetName.PresetTitle", preset.Name), Lang.Get("Dialog_SetName.PresetLabel"), OnCommit, name => NameIsValid<T>(preset.Type, name), preset.Name);
        }

        public static bool NameIsValid<T>(IPresetableType type, string name) => (name.Length <= MaxIdLength) && !string.Equals(name, Lang.Get("Preset.None"), StringComparison.OrdinalIgnoreCase) && !string.Equals(name, Lang.Get("Preset.Personalized"), StringComparison.OrdinalIgnoreCase) && ValidNameRegex.IsMatch(name) && !Registry.PresetNameExists<T>(type, name);

        public static T CreateVoidPreset<T>(IPresetableType type) where T : Presetable
        {
            var preset = (T) Activator.CreateInstance(typeof(T), type, VoidName);
            preset.IsVoid = true;
            return preset;
        }

        public static void ResetIds() => _count = 0;

        public void ExposeData()
        {
            if ((Scribe.mode != LoadSaveMode.Saving) || IsPreset) { Name = ScribePlus.LookValue(Name, "name"); }
            ExposePresetData();
        }

        public string GetUniqueLoadID() => $"{Mod.Id}_{GetPresetId()}";
    }
}
