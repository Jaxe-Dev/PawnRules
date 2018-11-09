# Pawn Rules
![Mod Version](https://img.shields.io/badge/Mod_Version-1.2.2-blue.svg)
![RimWorld Version](https://img.shields.io/badge/Built_for_RimWorld-1.0-blue.svg)
![Harmony Version](https://img.shields.io/badge/Powered_by_Harmony-1.2.0.1-blue.svg)\
![Steam Subscribers](https://img.shields.io/badge/dynamic/xml.svg?label=Steam+Subscribers&query=//table/tr[2]/td[1]&colorB=blue&url=https://steamcommunity.com/sharedfiles/filedetails/%3Fid=1499843448&suffix=+total)
![GitHub Downloads](https://img.shields.io/github/downloads/Jaxe-Dev/PawnRules/total.svg?colorB=blue&label=GitHub+Downloads)


[Link to Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1499843448)\
[Link to Ludeon Forum thread](https://ludeon.com/forums/index.php?topic=43086.0)

---

Pawn Rules is a mod that allows custom rules to be assigned individually to your colonists, animals, guests and prisoners.

Currently the following rules can be applied:
- Disallow certain foods
  - *Ignored if binging on food or optionally if malnourished or training an animal*
- Disallow bonding with certain animals
  - *This has no effect on existing bonds*
- Disallow new romances
  - *This has no effect on existing relations and engaged couples can still get married*
- Disallow constructing items that have a quality level
  - *Can still haul materials to blueprints*

Any of these rules can be disabled and hidden from the rules window. Rules presets and defaults can be imported and exported between games.

---

Supports addons created by other modders by allowing easy creation of new rule options while handling the GUI and world storage saving. For more information check out the [wiki page on Addons](https://github.com/Jaxe-Dev/PawnRules/wiki/Addons).

---

##### STEAM INSTALLATION
- **[Go to the Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=1499843448) and subscribe to the mod.**

---

##### NON-STEAM INSTALLATION
- **[Download the latest release](https://github.com/Jaxe-Dev/PawnRules/releases/latest) and unzip it into your *RimWorld/Mods* folder.**

---

###### TECHINICAL DETAILS
>This mod can be safely removed from a save without breaking the game. To do so go to *Show settings* from the *Mod Settings* menu and select *Remove Mod*.
>
> A save of your world will be made with no traces of this mod and the game will restart. Skipping the *Remove Mod* step will result in errors being displayed the first time a save is loaded although no further problems should occur.

---

The following base methods are patched with Harmony:
```
Postfix : RimWorld.FoodRestriction.Allows
Postfix : RimWorld.FoodUtility.WillEat
Postfix : RimWorld.GenConstruct.CanConstruct
Prefix* : RimWorld.InteractionWorker_RomanceAttempt.RandomSelectionWeight
Prefix* : RimWorld.InteractionWorker_RomanceAttempt.SuccessChance
Prefix* : RimWorld.Pawn_FoodRestrictionTracker.Configurable
Prefix* : RimWorld.Pawn_FoodRestrictionTracker.CurrentFoodRestriction
Prefix  : RimWorld.Pawn_GuestTracker.SetGuestStatus
Postfix : RimWorld.PawnUtility.TrySpawnHatchedOrBornPawn
Prefix* : RimWorld.RelationsUtility.TryDevelopBondRelation
Prefix  : RimWorld.WorkGiver_InteractAnimal.HasFoodToInteractAnimal
Postfix : RimWorld.WorkGiver_InteractAnimal.HasFoodToInteractAnimal
Prefix  : RimWorld.WorkGiver_InteractAnimal.TakeFoodForAnimalInteractJob
Postfix : RimWorld.WorkGiver_InteractAnimal.TakeFoodForAnimalInteractJob
Postfix : Verse.Game.FinalizeInit
Postfix : Verse.Pawn.GetGizmos
Postfix : Verse.Pawn.Kill
Prefix  : Verse.Pawn.SetFaction
Postfix : Verse.PawnGenerator.GeneratePawn
Prefix  : Verse.Profile.MemoryUtility.ClearAllMapsAndWorld

A prefix marked by a * denotes that in some circumstances the original method will be bypassed
```
