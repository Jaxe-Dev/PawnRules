using System.Linq;
using PawnRules.API;

namespace PawnRules.Data
{
    internal class PawnType : IPresetableType
    {
        public static readonly PawnType Colonist = new PawnType("Colonist", Lang.Get("PawnType.Colonist"), Lang.Get("PawnType.ColonistPlural"), OptionTarget.Colonist);
        public static readonly PawnType Animal = new PawnType("Animal", Lang.Get("PawnType.Animal"), Lang.Get("PawnType.AnimalPlural"), OptionTarget.Animal);
        public static readonly PawnType Guest = new PawnType("Guest", Lang.Get("PawnType.Guest"), Lang.Get("PawnType.GuestPlural"), OptionTarget.Guest);
        public static readonly PawnType Prisoner = new PawnType("Prisoner", Lang.Get("PawnType.Prisoner"), Lang.Get("PawnType.PrisonerPlural"), OptionTarget.Prisoner);

        public static readonly PawnType[] List =
        {
                    Colonist,
                    Animal,
                    Guest,
                    Prisoner
        };

        public string Id { get; }
        public string Label { get; }
        public string LabelPlural { get; }

        public OptionTarget AsTarget { get; }

        private PawnType(string id, string label, string labelPlural, OptionTarget target = default(OptionTarget))
        {
            Id = id;
            Label = label;
            LabelPlural = labelPlural;
            AsTarget = target;
        }

        public bool IsTargetted(OptionTarget target) => (AsTarget != default(OptionTarget)) && ((AsTarget & target) == AsTarget);

        public static PawnType FromTarget(OptionTarget target) => List.FirstOrDefault(type => type.AsTarget == target);
        public static PawnType FromId(string id) => List.FirstOrDefault(type => type.Id == id);
    }
}
