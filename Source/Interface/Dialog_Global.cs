using PawnRules.Data;
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
            if (listing.ButtonText(Lang.Get("Button.RemoveMod"), Lang.Get("Button.RemoveModDesc"))) { Find.WindowStack.Add(new Dialog_Alert(Lang.Get("Button.RemoveModConfirm"), Dialog_Alert.Buttons.YesNo, Registry.DeactivateMod)); }
            listing.End();
        }
    }
}
