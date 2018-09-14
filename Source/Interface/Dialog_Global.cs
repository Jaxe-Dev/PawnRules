using PawnRules.Data;
using PawnRules.Patch;
using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal class Dialog_Global : WindowPlus
    {
        private Dialog_Global() : base(Lang.Get("Dialog_Global.Title"), new Vector2(300f, 400f))
        { }

        public static void Open() => Find.WindowStack.Add(new Dialog_Global());

        protected override void DoContent(Rect rect)
        {
            var listing = new Listing_StandardPlus();
            listing.Begin(rect);

            Registry.AllowEmergencyFood = listing.CheckboxLabeled(Lang.Get("Dialog_Global.AllowEmergencyFood"), Registry.AllowEmergencyFood);
            Registry.AllowTrainingFood = listing.CheckboxLabeled(Lang.Get("Dialog_Global.AllowTrainingFood"), Registry.AllowTrainingFood);

            listing.Gap();
            listing.GapLine();
            listing.Gap();

            Registry.ShowFoodPolicy = listing.CheckboxLabeled(GetShowRuleLabel(Lang.Get("RestrictionType.Food")), Registry.ShowFoodPolicy);
            Registry.ShowBondingPolicy = listing.CheckboxLabeled(GetShowRuleLabel(Lang.Get("RestrictionType.Bonding")), Registry.ShowBondingPolicy);
            Registry.ShowAllowCourting = listing.CheckboxLabeled(GetShowRuleLabel(Lang.Get("Rules.AllowCourting")), Registry.ShowAllowCourting);
            Registry.ShowAllowArtisan = listing.CheckboxLabeled(GetShowRuleLabel(Lang.Get("Rules.AllowArtisan")), Registry.ShowAllowArtisan);

            listing.Gap();
            listing.GapLine();
            listing.Gap();

            if (listing.ButtonText(Lang.Get("Dialog_Global.Plans")))
            {
                Close();
                Dialog_Plans.Open();
            }

            listing.End();
        }

        private static string GetShowRuleLabel(string label) => Lang.Get("Dialog_Global.ShowRule", label.Italic());
    }
}
