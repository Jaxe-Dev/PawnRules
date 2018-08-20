using System.Linq;

namespace PawnRules.Data
{
    internal class RestrictionType : IPresetableType
    {
        public static readonly RestrictionType Food = new RestrictionType("Food", Lang.Get("RestrictionType.Food"), Lang.Get("RestrictionType.FoodPlural"), Lang.Get("RestrictionType.FoodCategorization"));
        public static readonly RestrictionType Bonding = new RestrictionType("Bonding", Lang.Get("RestrictionType.Bonding"), Lang.Get("RestrictionType.BondingPlural"), Lang.Get("RestrictionType.BondingCategorization"));

        public static readonly RestrictionType[] List =
        {
                    Food,
                    Bonding
        };

        public string Id { get; }
        public string Label { get; }
        public string LabelPlural { get; }
        public string Categorization { get; }

        private RestrictionType(string id, string label, string labelPlural, string categorization)
        {
            Id = id;
            Label = label;
            LabelPlural = labelPlural;
            Categorization = categorization;
        }

        public static RestrictionType FromId(string id) => List.FirstOrDefault(type => type.Id == id);
    }
}
