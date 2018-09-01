using PawnRules.Data;
using PawnRules.Patch;
using UnityEngine;
using Verse;

namespace PawnRules.Interface
{
    internal class Dialog_Global : WindowPlus
    {
        public Dialog_Global() : base(Lang.Get("Dialog_Global.Title"), new Vector2(300f, 300))
        { }

        public override void DoContent(Rect rect)
        {
            var listing = new Listing_Standard();
            listing.Begin(rect);

            Registry.ShowFoodPolicy = ShowRuleListing(listing, Lang.Get("RestrictionType.Food"), Registry.ShowFoodPolicy);
            Registry.ShowBondingPolicy = ShowRuleListing(listing, Lang.Get("RestrictionType.Bonding"), Registry.ShowBondingPolicy);
            Registry.ShowAllowCourting = ShowRuleListing(listing, Lang.Get("Rules.AllowCourting"), Registry.ShowAllowCourting);
            Registry.ShowAllowArtisan = ShowRuleListing(listing, Lang.Get("Rules.AllowArtisan"), Registry.ShowAllowArtisan);

            listing.Gap();
            listing.GapLine();
            listing.Gap();

            if (listing.ButtonText(Lang.Get("Button.RemoveMod"), Lang.Get("Button.RemoveModDesc"))) { Find.WindowStack.Add(new Dialog_Alert(Lang.Get("Button.RemoveModConfirm"), Dialog_Alert.Buttons.YesNo, Registry.DeactivateMod)); }
            listing.End();
        }

        private static bool ShowRuleListing(Listing_Standard listing, string label, bool value)
        {
            listing.CheckboxLabeled(Lang.Get("Dialog_Global.ShowRule", label.Italic()), ref value);
            return value;
        }
    }
}
