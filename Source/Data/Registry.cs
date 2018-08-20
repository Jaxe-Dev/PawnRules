using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PawnRules.Interface;
using PawnRules.Patch;
using PawnRules.SDK;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnRules.Data
{
    internal class Registry : WorldObject
    {
        private const string WorldObjectDefName = "PawnRules_Registry";
        private const string CurrentVersion = "v" + Mod.Version;

        public static Registry Instance { get; private set; }
        public static bool IsActive => !_isDeactivating && (Instance != null) && (Current.Game?.World != null);

        private static bool _isDeactivating;

        private readonly Dictionary<Type, Dictionary<IPresetableType, Presetable>> _voidPresets = new Dictionary<Type, Dictionary<IPresetableType, Presetable>>();
        private readonly Dictionary<Type, Dictionary<IPresetableType, Dictionary<string, Presetable>>> _presets = new Dictionary<Type, Dictionary<IPresetableType, Dictionary<string, Presetable>>>();
        private readonly Dictionary<PawnType, Rules> _defaults = new Dictionary<PawnType, Rules>();
        private readonly Dictionary<Pawn, Rules> _rules = new Dictionary<Pawn, Rules>();

        private List<Presetable> _savedPresets = new List<Presetable>();
        private List<Binding> _savedBindings = new List<Binding>();
        private List<Binding> _savedDefaults = new List<Binding>();

        public static void Initialize()
        {
            var worldObjects = Current.Game.World.worldObjects;
            var instance = (Registry) worldObjects.ObjectsAt(0).FirstOrDefault(worldObject => worldObject is Registry);

            if (instance != null) { return; }

            var def = DefDatabase<WorldObjectDef>.GetNamed(WorldObjectDefName);
            def.worldObjectClass = typeof(Registry);
            instance = (Registry) WorldObjectMaker.MakeWorldObject(def);
            def.worldObjectClass = typeof(WorldObject);
            instance.Tile = 0;
            worldObjects.Add(instance);
        }

        public static T GetVoidPreset<T>(IPresetableType type) where T : Presetable => (T) Instance._voidPresets[typeof(T)][type];

        public static T GetPreset<T>(IPresetableType type, string name) where T : Presetable
        {
            if (!Instance._presets.ContainsKey(typeof(T)) || !Instance._presets[typeof(T)].ContainsKey(type) || !Instance._presets[typeof(T)][type].ContainsKey(name)) { return null; }

            return (T) Instance._presets[typeof(T)][type][name];
        }

        public static IEnumerable<T> GetPresets<T>(IPresetableType type) where T : Presetable
        {
            if (!Instance._presets.ContainsKey(typeof(T)) || !Instance._presets[typeof(T)].ContainsKey(type)) { return new T[] { }; }

            return Instance._presets[typeof(T)][type].Values.Cast<T>().ToArray();
        }

        private static void AddPreset(Presetable preset)
        {
            if (!Instance._presets.ContainsKey(preset.GetType())) { Instance._presets[preset.GetType()] = new Dictionary<IPresetableType, Dictionary<string, Presetable>>(); }
            if (!Instance._presets[preset.GetType()].ContainsKey(preset.Type)) { Instance._presets[preset.GetType()][preset.Type] = new Dictionary<string, Presetable>(); }

            Instance._presets[preset.GetType()][preset.Type][preset.Name] = preset;
        }

        public static T CreatePreset<T>(IPresetableType type, string name) where T : Presetable
        {
            var preset = (T) Activator.CreateInstance(typeof(T), type, name);
            AddPreset(preset);
            return preset;
        }

        public static T RenamePreset<T>(T old, string name) where T : Presetable
        {
            var preset = Instance._presets[old.GetType()][old.Type][old.Name];

            Instance._presets[preset.GetType()][preset.Type].Remove(preset.Name);
            preset.Name = name;
            Instance._presets[preset.GetType()][preset.Type].Add(preset.Name, preset);

            return (T) preset;
        }

        public static void DeletePreset<T>(T preset) where T : Presetable
        {
            if ((preset == null) || preset.IsVoid) { throw new Mod.Exception("Tried to delete void preset"); }

            Instance._presets[preset.GetType()][preset.Type].Remove(preset.Name);

            if (typeof(T) == typeof(Rules))
            {
                foreach (var rule in Instance._rules.Where(rule => rule.Value == preset).ToArray()) { Instance._rules[rule.Key] = GetVoidPreset<Rules>(rule.Value.Type).CloneRulesFor(rule.Key); }
                foreach (var rule in Instance._defaults.Where(rule => rule.Value == preset).ToArray()) { Instance._defaults[rule.Key] = GetVoidPreset<Rules>(rule.Value.Type); }
            }
            else if (typeof(T) == typeof(Restriction))
            {
                foreach (var rulesType in Instance._presets[typeof(Rules)].Values.ToArray())
                {
                    foreach (var presetable in rulesType.Values.ToArray())
                    {
                        var rule = (Rules) presetable;
                        var type = (RestrictionType) preset.Type;
                        if(rule.GetRestriction(type) == preset) { rule.SetRestriction(type, GetVoidPreset<Restriction>(type));}
                    }
                }
            }
        }

        public static bool PresetNameExists<T>(IPresetableType type, string name) => Instance._presets.ContainsKey(typeof(T)) && Instance._presets[typeof(T)].ContainsKey(type) && Instance._presets[typeof(T)][type].ContainsKey(name);
        public static Rules GetDefaultRules(PawnType type) => Instance._defaults[type];
        public static void SetDefaultRules(Rules rules) => Instance._defaults[rules.Type] = rules;

        public static T GetAddonDefaultValue<T>(OptionTarget target, AddonOption addon, T invalidValue = default(T))
        {
            var rules = GetDefaultRules(PawnType.FromTarget(target));
            if ((rules == null) || !addon.AllowedInPreset) { return invalidValue; }
            return rules.GetAddonValue(addon, invalidValue);
        }

        public static Rules GetRules(Pawn pawn)
        {
            if (!pawn.CanHaveRules()) { return null; }
            return Instance._rules.ContainsKey(pawn) ? Instance._rules[pawn] : null;
        }

        public static Rules GetOrCreateRules(Pawn pawn)
        {
            if (!pawn.CanHaveRules()) { return null; }
            if (Instance._rules.ContainsKey(pawn)) { return Instance._rules[pawn]; }

            var defaultRules = GetDefaultRules(pawn.GetTargetType());
            var rules = defaultRules.IsVoid ? defaultRules.CloneRulesFor(pawn) : defaultRules;
            Instance._rules.Add(pawn, rules);

            return rules;
        }

        public static void ReplaceRules(Pawn pawn, Rules rules) => Instance._rules[pawn] = rules;
        public static void ReplaceDefaultRules(PawnType type, Rules rules) => Instance._defaults[type] = rules;

        private static Rules ChangeTypeOrCreateRules(Pawn pawn, PawnType type)
        {
            Instance._rules[pawn] = new Rules(pawn, type ?? pawn.GetTargetType());
            return Instance._rules[pawn];
        }

        public static Rules CloneRules(Pawn original, Pawn cloner)
        {
            if (!original.CanHaveRules()) { return null; }
            if (!Instance._rules.ContainsKey(original)) { return GetOrCreateRules(cloner); }
            if (Instance._rules.ContainsKey(cloner)) { DeleteRules(cloner); }

            var cloned = Instance._rules[original].CloneRulesFor(cloner);
            Instance._rules.Add(cloner, cloned);

            return cloned;
        }

        public static void DeleteRules(Pawn pawn)
        {
            if ((pawn == null) || !Instance._rules.ContainsKey(pawn)) { return; }
            Instance._rules.Remove(pawn);
        }

        public static void FactionUpdate(Thing thing, Faction newFaction, bool? guest = null)
        {
            if (!(thing is Pawn pawn) || !pawn.Spawned || !pawn.Dead) { return; }

            PawnType type;

            if (newFaction == Faction.OfPlayer)
            {
                if ((guest == null) || pawn.IsColonistPlayerControlled) { type = pawn.RaceProps.Animal ? PawnType.Animal : PawnType.Colonist; }
                else { type = guest.Value ? PawnType.Guest : PawnType.Prisoner; }
            }
            else if ((guest == null ? pawn.Faction : pawn.HostFaction) == Faction.OfPlayer)
            {
                DeleteRules(pawn);
                return;
            }
            else { return; }

            if (GetDefaultRules(type).IsIgnored()) { return; }

            ChangeTypeOrCreateRules(pawn, type);
        }

        public static void DeactivateMod()
        {
            _isDeactivating = true;

            ModsConfig.SetActive(Mod.ContentPack.Identifier, false);

            var runningMods = PrivateAccess.Verse_LoadedModManager_RunningMods();
            runningMods.Remove(Mod.ContentPack);

            var addonMods = new StringBuilder();
            foreach (var mod in AddonManager.Mods)
            {
                addonMods.AppendLine(mod.Name);
                ModsConfig.SetActive(mod.Identifier, false);
                runningMods.Remove(mod);
            }

            ModsConfig.Save();

            if (Find.WorldObjects.Contains(Instance)) { Find.WorldObjects.Remove(Instance); }

            const string saveName = "PawnRules_Removed";

            GameDataSaveLoader.SaveGame(saveName);

            var message = addonMods.Length > 0 ? Lang.Get("Button.RemoveModAndAddonsComplete", saveName.Bold(), addonMods.ToString()) : Lang.Get("Button.RemoveModComplete", saveName.Bold());
            Find.WindowStack.Add(new Dialog_Alert(message, Dialog_Alert.Buttons.Ok, GenCommandLine.Restart));
        }

        public override void PostAdd()
        {
            base.PostAdd();
            Instance = this;
            InitVoids();
            InitDefaults();
        }

        private void InitVoids()
        {
            _voidPresets[typeof(Rules)] = new Dictionary<IPresetableType, Presetable>();
            foreach (var type in PawnType.List) { _voidPresets[typeof(Rules)][type] = Presetable.CreateVoidPreset<Rules>(type); }

            _voidPresets[typeof(Restriction)] = new Dictionary<IPresetableType, Presetable>();
            foreach (var type in RestrictionType.List) { _voidPresets[typeof(Restriction)][type] = Presetable.CreateVoidPreset<Restriction>(type); }
        }

        private void InitDefaults()
        {
            foreach (var type in PawnType.List.Where(type => !_defaults.ContainsKey(type))) { _defaults.Add(type, (Rules) _voidPresets[typeof(Rules)][type]); }
        }

        public override void ExposeData()
        {
            if (_isDeactivating) { return; }
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Instance = this;
                InitVoids();
            }

            var version = CurrentVersion;
            Scribe_Values.Look(ref version, "version");

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _savedPresets.Clear();
                _savedDefaults.Clear();
                _savedBindings.Clear();

                foreach (var classType in _presets.Values)
                {
                    foreach (var type in classType.Values) { _savedPresets.AddRange(type.Values.ToArray()); }
                }

                _savedDefaults.AddRange(_defaults.Values.Where(preset => !preset.IsVoid).Select(preset => new Binding(preset.Type, preset)).ToArray());
                _savedBindings.AddRange(_rules.Where(rules => rules.Key.CanHaveRules()).Select(rules => new Binding(rules.Key, rules.Value.IsIgnored() ? null : rules.Value)).ToArray());
            }

            Scribe_Collections.Look(ref _savedPresets, "presets", LookMode.Deep);
            Scribe_Collections.Look(ref _savedBindings, "bindings", LookMode.Deep);
            Scribe_Collections.Look(ref _savedDefaults, "defaults", LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.PostLoadInit) { return; }

            _presets.Clear();
            _defaults.Clear();
            _rules.Clear();

            foreach (var preset in _savedPresets) { AddPreset(preset); }

            foreach (var preset in _savedDefaults) { _defaults.Add(preset.Target, preset.Rules); }
            InitDefaults();

            foreach (var binding in _savedBindings.Where(binding => binding.Pawn.CanHaveRules())) { _rules.Add(binding.Pawn, binding.Rules); }

            _savedPresets.Clear();
            _savedDefaults.Clear();
            _savedBindings.Clear();
            Presetable.ResetIds();
        }
    }
}
