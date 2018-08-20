# Pawn Rules
#### Version 1.0
Built for **RimWorld 1.0.x / 0.19.x**\
Supports **ModSync RW**

[Link to Ludeon Forum Post](https://ludeon.com/forums/index.php?topic=43086.0)

------------

Pawn Rules is a mod that allows custom rules to be assigned individually to your colonists, animals, guests and prisoners.

Currently the following rules can be applied:
- Disallow certain foods
  - *Ignored if binging on food*
- Disallow bonding with certain animals
  - *This has no effect on existing bonds*
- Disallow new romances
  - *This has no effect on existing relations and engaged couples can still get married*
- Disallow constructing items that have a quality level
  - *Can still haul materials to blueprints*

------------

Supports addons created by other modders by allowing easy creation of new rule options while handling the GUI and world storage saving. For more information check out the [wiki page on Addons](https://github.com/Jaxe-Dev/PawnRules/wiki/Addons).

------------

##### INSTALLATION
- **[Download the latest release](https://github.com/Jaxe-Dev/PawnRules/releases/latest) and unzip it into your *RimWorld/Mods* folder.**

------------

###### TECHINICAL DETAILS
>This mod can be safely removed from a save without breaking the game. To do so go to *Options* from the main rules window for a character and select *Remove Mod*.
>
> A save of your world will be made with no traces of this mod and the game will restart. Skipping the *Remove Mod* step will result in errors being displayed the first time a save is loaded although no further problems should occur.

------------

The following original methods are patched using Harmony:
```C#
RimWorld.FoodUtility.BestFoodSourceOnMap : Prefix
RimWorld.FoodUtility.BestFoodInInventory : Prefix
RimWorld.GenConstruct.CanConstruct : Postfix
RimWorld.InteractionWorker_RomanceAttempt.SuccessChance : Prefix
RimWorld.JobGiver_PackFood.IsGoodPackableFoodFor : Postfix
RimWorld.JoyGiver_Ingest.CanIngestForJoy : Prefix
RimWorld.PawnUtility.TrySpawnHatchedOrBornPawn : Postfix
RimWorld.RelationsUtility.TryDevelopBondRelation : Prefix
Verse.Game.FinalizeInit : Postfix
Verse.Pawn.GetGizmos : Postfix
Verse.Pawn.Kill : Postfix
Verse.Pawn.SetFaction : Prefix
Verse.Pawn.SetFactionDirect : Prefix
Verse.Pawn_GuestTracker.SetGuestStatus : Prefix
```
