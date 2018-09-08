using System;

namespace PawnRules.API
{
    /// <summary>
    ///     Used to set the target type of the pawn that a rule will be applied to. Multiple flags may be set.
    /// </summary>
    [Flags]
    public enum OptionTarget
    {
        /// <summary>
        ///     For all colonists part of the player's faction.
        /// </summary>
        Colonist = 1,
        /// <summary>
        ///     For all animals part of the player's faction.
        /// </summary>
        Animal = 2,
        /// <summary>
        ///     For all guests that are currently staying with the player's faction.
        /// </summary>
        Guest = 4,
        /// <summary>
        ///     For all prisoners that are being held by the player's faction.
        /// </summary>
        Prisoner = 8
    }
}
