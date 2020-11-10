using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PawnRules.API;
using PawnRules.Interface;
using PawnRules.Patch;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PawnRules.Data
{
    internal class Registry : WorldObject
    {
        private const string WorldObjectDefName = "PawnRules_Registry";
        private const string CurrentVersion = "v" + Mod.Version;

        public static bool IsActive => !_isDeactivating && (_instance != null) && (Current.ProgramState == ProgramState.Playing);

        private static Registry _instance;

        private static bool _isDeactivating;

        public static bool ShowFoodPolicy { get => _instance._showFoodPolicy; set => _instance._showFoodPolicy = value; }
        public static bool ShowBondingPolicy { get => _instance._showBondingPolicy; set => _instance._showBondingPolicy = value; }
        public static bool ShowAllowCourting { get => _instance._showAllowCourting; set => _instance._showAllowCourting = value; }
        public static bool ShowAllowArtisan { get => _instance._showAllowArtisan; set => _instance._showAllowArtisan = value; }

        public static bool AllowDrugsRestriction { get => _instance._allowDrugsRestriction; set => _instance._allowDrugsRestriction = value; }
        public static bool AllowEmergencyFood { get => _instance._allowEmergencyFood; set => _instance._allowEmergencyFood = value; }
        public static bool AllowTrainingFood { get => _instance._allowTrainingFood; set => _instance._allowTrainingFood = value; }
        public static bool AllowRestingFood { get => _instance._allowRestingFood; set => _instance._allowRestingFood = value; }
        public static Pawn ExemptedTrainer { get => _instance._exemptedTrainer; set => _instance._exemptedTrainer = value; }

        private string _loadedVersion;

        private readonly Dictionary<Type, Dictionary<IPresetableType, Presetable>> _voidPresets = new Dictionary<Type, Dictionary<IPresetableType, Presetable>>();
        private readonly Dictionary<Type, Dictionary<IPresetableType, Dictionary<string, Presetable>>> _presets = new Dictionary<Type, Dictionary<IPresetableType, Dictionary<string, Presetable>>>();
        private readonly Dictionary<PawnType, Rules> _defaults = new Dictionary<PawnType, Rules>();
        private readonly Dictionary<Pawn, Rules> _rules = new Dictionary<Pawn, Rules>();

        private List<Presetable> _savedPresets = new List<Presetable>();
        private List<Binding> _savedBindings = new List<Binding>();
        private List<Binding> _savedDefaults = new List<Binding>();

        private bool _showFoodPolicy = true;
        private bool _showBondingPolicy = true;
        private bool _showAllowCourting = true;
        private bool _showAllowArtisan = true;

        private bool _allowDrugsRestriction;
        private bool _allowEmergencyFood;
        private bool _allowTrainingFood;
        private bool _allowRestingFood;

        private Pawn _exemptedTrainer;

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

        public static void Clear() => _instance = null;

        public static T GetVoidPreset<T>(IPresetableType type) where T : Presetable => (T) _instance._voidPresets[typeof(T)][type];

        public static T GetPreset<T>(IPresetableType type, string name) where T : Presetable
        {
            if (!_instance._presets.ContainsKey(typeof(T)) || !_instance._presets[typeof(T)].ContainsKey(type) || !_instance._presets[typeof(T)][type].ContainsKey(name)) { return null; }

            return (T) _instance._presets[typeof(T)][type][name];
        }

        public static IEnumerable<T> GetPresets<T>(IPresetableType type) where T : Presetable
        {
            if (!_instance._presets.ContainsKey(typeof(T)) || !_instance._presets[typeof(T)].ContainsKey(type)) { return new T[] { }; }

            return _instance._presets[typeof(T)][type].Values.Cast<T>().OrderBy(preset => preset.Name).ToArray();
        }

        private static void AddPreset(Presetable preset)
        {
            if (!_instance._presets.ContainsKey(preset.GetType())) { _instance._presets[preset.GetType()] = new Dictionary<IPresetableType, Dictionary<string, Presetable>>(); }
            if (!_instance._presets[preset.GetType()].ContainsKey(preset.Type)) { _instance._presets[preset.GetType()][preset.Type] = new Dictionary<string, Presetable>(); }

            _instance._presets[preset.GetType()][preset.Type][preset.Name] = preset;
        }

        public static T CreatePreset<T>(IPresetableType type, string name) where T : Presetable
        {
            var preset = (T) Activator.CreateInstance(typeof(T), type, name);
            AddPreset(preset);
            return preset;
        }

        public static T RenamePreset<T>(T old, string name) where T : Presetable
        {
            var preset = _instance._presets[old.GetType()][old.Type][old.Name];

            _instance._presets[preset.GetType()][preset.Type].Remove(preset.Name);
            preset.Name = name;
            _instance._presets[preset.GetType()][preset.Type].Add(preset.Name, preset);

            return (T) preset;
        }

        public static void DeletePreset<T>(T preset) where T : Presetable
        {
            if ((preset == null) || preset.IsVoid) { throw new Mod.Exception("Tried to delete void preset"); }

            if (!_instance._presets.ContainsKey(preset.GetType()) || !_instance._presets[preset.GetType()].ContainsKey(preset.Type)) { return; }

            _instance._presets[preset.GetType()][preset.Type].Remove(preset.Name);

            if (typeof(T) == typeof(Rules))
            {
                foreach (var rule in _instance._rules.Where(rule => rule.Value == preset).ToArray()) { _instance._rules[rule.Key] = GetVoidPreset<Rules>(rule.Value.Type).CloneRulesFor(rule.Key); }
                foreach (var rule in _instance._defaults.Where(rule => rule.Value == preset).ToArray()) { _instance._defaults[rule.Key] = GetVoidPreset<Rules>(rule.Value.Type); }
            }
            else if ((typeof(T) == typeof(Restriction)) && _instance._presets.ContainsKey(typeof(Rules)))
            {
                foreach (var rulesType in _instance._presets[typeof(Rules)].Values.ToArray())
                {
                    foreach (var presetable in rulesType.Values.ToArray())
                    {
                        var rule = (Rules) presetable;
                        var type = (RestrictionType) preset.Type;
                        if (rule.GetRestriction(type) == preset) { rule.SetRestriction(type, GetVoidPreset<Restriction>(type)); }
                    }
                }
            }
        }

        public static bool PresetNameExists<T>(IPresetableType type, string name) => _instance._presets.ContainsKey(typeof(T)) && _instance._presets[typeof(T)].ContainsKey(type) && _instance._presets[typeof(T)][type].ContainsKey(name);
        public static Rules GetDefaultRules(PawnType type) => _instance._defaults[type];
        public static void SetDefaultRules(Rules rules) => _instance._defaults[rules.Type] = rules;

        public static T GetAddonDefaultValue<T>(OptionTarget target, AddonOption addon, T invalidValue = default)
        {
            var rules = GetDefaultRules(PawnType.FromTarget(target));
            if ((rules == null) || !addon.AllowedInPreset) { return invalidValue; }
            return rules.GetAddonValue(addon, invalidValue);
        }

        public static Rules GetRules(Pawn pawn)
        {
            if (!pawn.CanHaveRules()) { return null; }
            return _instance._rules.ContainsKey(pawn) ? _instance._rules[pawn] : null;
        }

        public static Rules GetOrNewRules(Pawn pawn)
        {
            if (!pawn.CanHaveRules()) { return null; }
            if (_instance._rules.ContainsKey(pawn)) { return _instance._rules[pawn]; }

            var rules = GetVoidPreset<Rules>(pawn.GetTargetType()).CloneRulesFor(pawn);
            _instance._rules.Add(pawn, rules);

            return rules;
        }

        public static Rules GetOrDefaultRules(Pawn pawn)
        {
            if (!pawn.CanHaveRules()) { return null; }
            if (_instance._rules.ContainsKey(pawn)) { return _instance._rules[pawn]; }

            var defaultRules = GetDefaultRules(pawn.GetTargetType());
            var rules = defaultRules.IsVoid ? defaultRules.CloneRulesFor(pawn) : defaultRules;
            _instance._rules.Add(pawn, rules);

            return rules;
        }

        public static void ReplaceRules(Pawn pawn, Rules rules) => _instance._rules[pawn] = rules;
        public static void ReplaceDefaultRules(PawnType type, Rules rules) => _instance._defaults[type] = rules;

        private static void ChangeTypeOrCreateRules(Pawn pawn, PawnType type)
        {
            if (type == pawn.GetTargetType()) { return; }
            var defaultRules = GetDefaultRules(type);
            _instance._rules[pawn] = defaultRules.IsVoid ? defaultRules.CloneRulesFor(pawn) : defaultRules;
        }

        public static Rules CloneRules(Pawn original, Pawn cloner)
        {
            if (!original.CanHaveRules()) { return null; }
            if (!_instance._rules.ContainsKey(original)) { return GetOrDefaultRules(cloner); }
            if (_instance._rules.ContainsKey(cloner)) { DeleteRules(cloner); }

            var cloned = _instance._rules[original].CloneRulesFor(cloner);
            _instance._rules.Add(cloner, cloned);

            return cloned;
        }

        public static void DeleteRules(Pawn pawn)
        {
            if ((pawn == null) || !_instance._rules.ContainsKey(pawn)) { return; }
            _instance._rules.Remove(pawn);
        }

        public static void FactionUpdate(Thing thing, Faction newFaction, bool? guest = null)
        {
            if (!IsActive || !(thing is Pawn pawn) || pawn.Dead) { return; }

            var oldFaction = guest == null ? pawn.Faction : pawn.HostFaction;
            PawnType type;

            if (newFaction?.IsPlayer ?? false)
            {
                if ((guest == null) || (pawn.Faction?.IsPlayer ?? false)) { type = pawn.RaceProps.Animal ? PawnType.Animal : PawnType.Colonist; }
                else { type = guest.Value ? PawnType.Guest : PawnType.Prisoner; }
            }
            else if (oldFaction?.IsPlayer ?? false)
            {
                DeleteRules(pawn);
                return;
            }
            else { return; }

            ChangeTypeOrCreateRules(pawn, type);
        }

        public static void DeactivateMod()
        {
            _isDeactivating = true;

            ModsConfig.SetActive(Mod.Instance.Content.PackageId, false);

            var runningMods = Access.Field_Verse_LoadedModManager_RunningMods_Get();
            runningMods.Remove(Mod.Instance.Content);

            var addonMods = new StringBuilder();
            foreach (var mod in AddonManager.Mods)
            {
                addonMods.AppendLine(mod.Name);
                ModsConfig.SetActive(mod.PackageId, false);
                runningMods.Remove(mod);
            }

            ModsConfig.Save();

            if (Find.WorldObjects.Contains(_instance)) { Find.WorldObjects.Remove(_instance); }

            const string saveName = "PawnRules_Removed";

            GameDataSaveLoader.SaveGame(saveName);

            var message = addonMods.Length > 0 ? Lang.Get("Button.RemoveModAndAddonsComplete", saveName.Bold(), addonMods.ToString()) : Lang.Get("Button.RemoveModComplete", saveName.Bold());
            Dialog_Alert.Open(message, Dialog_Alert.Buttons.Ok, GenCommandLine.Restart);
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

        public override void PostAdd()
        {
            base.PostAdd();
            _instance = this;
            InitVoids();
            InitDefaults();
        }

        public override void SpawnSetup() { }

        public override void PostRemove() { }

        public override void Print(LayerSubMesh subMesh) { }

        public override void Draw() { }

        public override void ExposeData()
        {
            if (_isDeactivating) { return; }
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                _instance = this;
                InitVoids();
            }

            if (Scribe.mode == LoadSaveMode.Saving) { _loadedVersion = CurrentVersion; }
            Scribe_Values.Look(ref _loadedVersion, "version");
            if ((Scribe.mode == LoadSaveMode.PostLoadInit) && (_loadedVersion != CurrentVersion)) { Mod.Warning($"Registry loaded was saved by a different mod version ({_loadedVersion ?? "vNULL"} loaded, current is {CurrentVersion})"); }

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _savedPresets.Clear();
                _savedDefaults.Clear();
                _savedBindings.Clear();

                foreach (var type in _presets.Values.SelectMany(classType => classType.Values)) { _savedPresets.AddRange(type.Values.ToArray()); }

                _savedDefaults.AddRange(_defaults.Values.Where(preset => !preset.IsVoid).Select(preset => new Binding(preset.Type, preset)).ToArray());
                _savedBindings.AddRange(_rules.Where(rules => rules.Key.CanHaveRules()).Select(rules => new Binding(rules.Key, rules.Value.IsIgnored() ? null : rules.Value)).ToArray());
            }

            Scribe_Values.Look(ref _showFoodPolicy, "showFoodPolicy", true);
            Scribe_Values.Look(ref _showBondingPolicy, "showBondingPolicy", true);
            Scribe_Values.Look(ref _showAllowCourting, "showAllowCourting", true);
            Scribe_Values.Look(ref _showAllowArtisan, "showAllowArtisan", true);

            Scribe_Values.Look(ref _allowDrugsRestriction, "allowDrugsRestriction");
            Scribe_Values.Look(ref _allowEmergencyFood, "allowEmergencyFood");
            Scribe_Values.Look(ref _allowTrainingFood, "allowTrainingFood");
            Scribe_Values.Look(ref _allowRestingFood, "allowRestingFood");

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

        public static XElement ToXml()
        {
            var xml = new XElement("PawnRulesPlan", new XAttribute("Version", CurrentVersion));

            var presets = new XElement("Presets");

            foreach (var type in _instance._presets.Values)
            {
                foreach (var presetType in type.Values) { presets.Add(from preset in presetType where !preset.Value.IsVoid select preset.Value.ToXml()); }
            }

            xml.Add(presets);

            xml.Add(new XElement("Defaults", from rule in _instance._defaults.Values where !rule.IsVoid select new XElement("Preset", new XAttribute("Type", rule.Type.Id), rule.Name)));

            return xml;
        }

        public static void FromXml(XElement xml)
        {
            var version = xml.Attribute("Version")?.Value;
            if (version != CurrentVersion) { Mod.Warning($"Loaded xml from a different mod version ({version ?? "vNULL"} loaded, current is {CurrentVersion})"); }

            var presets = xml.Element("Presets")?.Elements().ToArray();
            if (presets == null) { return; }

            foreach (var restriction in presets.Where(preset => preset.Name == "Restriction").Select(preset => new Restriction(preset))) { AddPreset(restriction); }
            foreach (var rules in presets.Where(preset => preset.Name == "Rules").Select(preset => new Rules(preset))) { AddPreset(rules); }

            var defaults = xml.Element("Defaults")?.Elements();
            if (defaults == null) { return; }
            foreach (var preset in defaults)
            {
                var type = PawnType.FromId(preset.Attribute("Type")?.Value);

                var name = preset.Value;
                if ((type == null) || name.NullOrEmpty())
                {
                    Mod.Warning("Skipping invalid default preset");
                    continue;
                }

                _instance._defaults[type] = GetPreset<Rules>(type, name);
            }
        }
    }
}
