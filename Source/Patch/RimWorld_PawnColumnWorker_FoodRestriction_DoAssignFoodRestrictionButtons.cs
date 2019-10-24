using System.Collections.Generic;
using System.Linq;
using Harmony;
using PawnRules.Data;
using PawnRules.Interface;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnRules.Patch
{
    [HarmonyPatch(typeof(PawnColumnWorker_FoodRestriction), "DoAssignFoodRestrictionButtons")]
    internal static class RimWorld_PawnColumnWorker_FoodRestriction_DoAssignFoodRestrictionButtons
    {
        private static bool Prefix(Rect rect, Pawn pawn)
        {
            var num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
            var num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
            var num3 = rect.x;

            var rect2 = new Rect(num3, rect.y + 2f, num, rect.height - 4f);
            var rect3 = rect2;

            var rules = Registry.GetOrNewRules(pawn);
            var label = rules.GetDisplayName();
            var buttonLabel = label.Truncate(rect2.width);

            Rules GetPayload(Pawn p) => rules;
            Widgets.Dropdown(rect3, pawn, GetPayload, GenerateMenu, buttonLabel, null, label, null, null, true);

            num3 += num;
            num3 += 4f;

            var rect4 = new Rect(num3, rect.y + 2f, num2, rect.height - 4f);

            if (Widgets.ButtonText(rect4, "AssignTabEdit".Translate())) { Dialog_Rules.Open(pawn); }

            return false;
        }

        private static IEnumerable<Widgets.DropdownMenuElement<Rules>> GenerateMenu(Pawn pawn)
        {
            var menu = new List<Widgets.DropdownMenuElement<Rules>>
            {
                new Widgets.DropdownMenuElement<Rules>
                {
                    option = new FloatMenuOption(Lang.Get("Preset.Personalized").Italic(), () =>
                                                                                           {
                                                                                               Registry.ReplaceRules(pawn, pawn.GetRules().CloneRulesFor(pawn));
                                                                                               Dialog_Rules.Open(pawn);
                                                                                           })
                }
            };

            menu.AddRange(Registry.GetPresets<Rules>(pawn.GetTargetType()).Select(rule => new Widgets.DropdownMenuElement<Rules> { option = new FloatMenuOption(rule.GetDisplayName(), () => Registry.ReplaceRules(pawn, rule)), payload = rule }));

            return menu;
        }
    }
}
