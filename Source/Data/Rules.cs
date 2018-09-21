using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using PawnRules.Patch;
using Verse;

namespace PawnRules.Data
{
    internal class Rules : Presetable
    {
        public Pawn Pawn { get; private set; }
        public new PawnType Type { get => (PawnType) base.Type; private set => base.Type = value; }

        private readonly Dictionary<RestrictionType, Restriction> _restrictions = new Dictionary<RestrictionType, Restriction>();
        private readonly Dictionary<AddonOption, object> _addonValues = new Dictionary<AddonOption, object>();

        public bool AllowCourting = true;
        public bool AllowArtisan = true;

        public bool HasAddons { get; private set; }
        public float AddonsRectHeight { get; private set; }

        public Rules()
        { }

        public Rules(Pawn pawn, PawnType type = null)
        {
            Pawn = pawn;
            Type = type ?? pawn.GetTargetType();
        }

        public Rules(PawnType type, string name) : base(name) => Type = type;

        public Rules(XElement xml)
        {
            Name = xml.Element("Name")?.Value;
            if (Name.NullOrEmpty())
            {
                Mod.Warning("Skipping unnamed rules preset");
                return;
            }

            Type = PawnType.FromId(xml.Attribute("Type")?.Value);
            if (Type == null)
            {
                Mod.Warning("Skipping invalid rules preset type");
                return;
            }

            var restrictions = xml.Element("Restrictions")?.Elements();
            if (restrictions != null)
            {
                foreach (var restriction in restrictions)
                {
                    var type = RestrictionType.FromId(restriction.Attribute("Type")?.Value);
                    if (type == null)
                    {
                        Mod.Warning("Skipping invalid restriction type in rules preset");
                        continue;
                    }

                    _restrictions.Add(type, Registry.GetPreset<Restriction>(type, restriction.Value));
                }
            }

            InitAddons();

            var addons = xml.Element("Addons")?.Elements();
            if (addons != null)
            {
                foreach (var addon in addons)
                {
                    var key = addon.Attribute("Key")?.Value;
                    if (key == null)
                    {
                        Mod.Warning("Skipping invalid addon value key in rules preset");
                        continue;
                    }

                    _addonValues.Add(AddonManager.GetAddon(key), addon.Value);
                }
            }

            var allowCourting = xml.Element("AllowCourting")?.Value;
            if (allowCourting != null) { AllowCourting = XmlConvert.ToBoolean(allowCourting); }

            var allowArtisan = xml.Element("AllowCourting")?.Value;
            if (allowArtisan != null) { AllowCourting = XmlConvert.ToBoolean(allowArtisan); }
        }

        public void CopyRules(Rules rules)
        {
            Type = rules.Type;

            CopyRestrictions(rules._restrictions);

            AllowCourting = rules.AllowCourting;
            AllowArtisan = rules.AllowArtisan;

            if (!rules.HasAddons) { return; }
            _addonValues.Clear();
            InitAddons();

            foreach (var addon in rules._addonValues.Keys.ToArray()) { _addonValues[addon] = rules._addonValues[addon]; }
        }

        private void CopyRestrictions(IDictionary<RestrictionType, Restriction> from)
        {
            foreach (var type in from.Keys.ToArray()) { _restrictions[type] = from[type]; }
        }

        public Rules CloneRulesFor(Pawn pawn)
        {
            var rules = new Rules(pawn);
            rules.CopyRules(this);
            return rules;
        }

        public Rules ClonePreset(string name = null)
        {
            var rules = new Rules(Type, name ?? Name);
            rules.CopyRules(this);
            return rules;
        }

        private void InitAddons()
        {
            if (!AddonManager.HasAddons || HasAddons) { return; }

            foreach (var option in AddonManager.Options)
            {
                if (!Type.IsTargetted(option.Target)) { continue; }
                _addonValues.Add(option, option.DefaultValue);
                AddonsRectHeight += option.WidgetHeight;
            }

            HasAddons = _addonValues.Count > 0;
            if (HasAddons) { AddonsRectHeight += 12f; }
        }

        public IEnumerable<AddonOption> GetAddons() => _addonValues.Keys.ToArray();

        public bool HasAddon(AddonOption addon) => HasAddons && _addonValues.ContainsKey(addon);

        public T GetAddonValue<T>(AddonOption addon, T invalidValue) => _addonValues.ContainsKey(addon) ? (T) _addonValues[addon] : invalidValue;

        public bool SetAddonValueDirect(AddonOption addon, object value)
        {
            if (!_addonValues.ContainsKey(addon)) { return false; }

            _addonValues[addon] = value;
            return true;
        }

        public void SetToVanilla()
        {
            foreach (var type in _restrictions.Keys.ToArray()) { _restrictions[type] = Registry.GetVoidPreset<Restriction>(type); }

            AllowCourting = true;
            AllowArtisan = true;

            foreach (var addon in _addonValues.Keys.ToArray()) { _addonValues[addon] = addon.DefaultValue; }
        }

        internal override bool IsIgnored() => _restrictions.Values.All(restrictions => restrictions.IsVoid) && AllowCourting && AllowArtisan && (!AddonManager.HasAddons || _addonValues.All(addon => addon.Value == addon.Key.DefaultValue));

        public Restriction GetRestriction(RestrictionType type) => _restrictions.ContainsKey(type) ? _restrictions[type] : Registry.GetVoidPreset<Restriction>(type);

        public void SetRestriction(RestrictionType type, Restriction restriction) => _restrictions[type] = restriction;

        public void SetPawn(Pawn pawn)
        {
            Pawn = pawn;
            Type = pawn.GetTargetType();
        }

        private void ExposeAddon<T>(AddonOption addon)
        {
            var oldValue = (T) _addonValues[addon];
            var newValue = ScribePlus.LookValue(oldValue, addon.Key, (T) addon.DefaultValue);

            if ((Scribe.mode == LoadSaveMode.Saving) || Equals(newValue, oldValue)) { return; }

            SetAddonValueDirect(addon, newValue);
        }

        protected override void ExposePresetData()
        {
            if ((Scribe.mode != LoadSaveMode.Saving) || IsPreset) { Type = PawnType.FromId(ScribePlus.LookValue(Type?.Id, "type")); }

            if (Scribe.mode == LoadSaveMode.LoadingVars) { InitAddons(); }

            foreach (var type in RestrictionType.List)
            {
                var restriction = GetRestriction(type);
                if ((Scribe.mode != LoadSaveMode.Saving) || !restriction.IsVoid) { SetRestriction(type, ScribePlus.LookReference(restriction, type.Id.ToLower(), Registry.GetVoidPreset<Restriction>(type))); }
            }

            AllowCourting = ScribePlus.LookValue(AllowCourting, "courting", true);
            AllowArtisan = ScribePlus.LookValue(AllowArtisan, "artisan", true);

            if (!HasAddons) { return; }

            foreach (var addon in _addonValues.Keys.ToArray())
            {
                if (addon.Type == typeof(string)) { ExposeAddon<string>(addon); }
                else if (addon.Type == typeof(bool)) { ExposeAddon<bool>(addon); }
                else if (addon.Type == typeof(int)) { ExposeAddon<int>(addon); }
                else if (addon.Type == typeof(float)) { ExposeAddon<float>(addon); }
            }
        }

        protected override string GetPresetId() => $"Rules_{Type?.Id ?? "Binding"}_{Pawn?.GetUniqueLoadID() ?? Name ?? Id.ToString()}";

        public override XElement ToXml()
        {
            var xml = new XElement("Rules", new XAttribute("Type", Type.Id));

            xml.Add(new XElement("Name", Name));
            xml.Add(new XElement("Restrictions", from restriction in _restrictions where !restriction.Value.IsVoid select new XElement("Restriction", new XAttribute("Type", restriction.Key.Id), restriction.Value.Name)));
            xml.Add(new XElement("AllowCourting", AllowCourting));
            xml.Add(new XElement("AllowArtisan", AllowArtisan));

            if (HasAddons) { xml.Add(new XElement("AddonValues", from addon in _addonValues select new XElement("AddonValue", new XAttribute("Key", addon.Key.Key), addon.Value))); }

            return xml;
        }
    }
}
