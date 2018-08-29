using PawnRules.Data;
using Verse;

namespace PawnRules.SDK
{
    /// <summary>
    ///     The base class of a rules option handle.
    /// </summary>
    public abstract class OptionHandle
    {
        /// <summary>
        ///     Called when the button for this option is clicked. Setting the value must be handled by the delegate. Unused if this option does not implement a button widget.
        /// </summary>
        /// <param name="pawn">The pawn being displayed when the button was clicked.</param>
        /// <returns>Currently unused but as practice return true if the value was changed.</returns>
        public delegate bool OnButtonClickHandler(Pawn pawn);

        /// <summary>
        ///     Called when the button for this option is clicked in the default rules dialog. Setting the value must be handled by the delegate. Unused if this option does not implement a button widget.
        /// </summary>
        /// <param name="target">The target of the default rule.</param>
        /// <returns>Currently unused but as practice return true if the value was changed.</returns>
        public delegate bool OnDefaultButtonClickHandler(OptionTarget target);

        /// <summary>
        ///     Called when the button for this option is clicked. Unused if this option does not implement a button widget.
        /// </summary>
        public OnButtonClickHandler OnButtonClick { get; set; }

        /// <summary>
        ///     Called when the button for this option is clicked in the default rules dialog. Unused if this option does not implement a button widget.
        /// </summary>
        public OnDefaultButtonClickHandler OnDefaultButtonClick { get; set; }
        internal AddonOption Addon { get; }

        internal OptionHandle(AddonOption addon) => Addon = addon;

        /// <summary>
        ///     Used to see if a given pawn has this option.
        /// </summary>
        /// <param name="pawn">The pawn to query.</param>
        /// <returns></returns>
        public bool IsUsedBy(Pawn pawn)
        {
            var rules = Registry.GetRules(pawn);
            return (rules != null) && rules.HasAddon(Addon);
        }

        internal void ChangeValue<T>(Pawn pawn, T newValue)
        {
            var handle = this as OptionHandle<T>;
            handle.SetValue(pawn, handle.OnChangeForPawnForPawn == null ? newValue : handle.OnChangeForPawnForPawn(pawn, handle.GetValue(pawn), newValue));
        }

        internal void ChangePresetValue<T>(Rules rules, T newValue)
        {
            var handle = this as OptionHandle<T>;
            rules.SetAddonValueDirect(handle.Addon, handle.OnChangeForForPreset == null ? newValue : handle.OnChangeForForPreset(rules.Type.AsTarget, rules.GetAddonValue(handle.Addon, (T) handle.Addon.DefaultValue), newValue));
        }

        internal void DoClick(Pawn pawn) => OnButtonClick(pawn);
        internal void DoDefaultClick(OptionTarget target) => OnDefaultButtonClick(target);
    }

    /// <summary>
    ///     Provides a handle for a rules option. This class may not be instantiated manually, create a <see cref="PawnRulesLink" /> to add one.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    public class OptionHandle<T> : OptionHandle
    {
        /// <summary>
        ///     This is called when the value of the option is changing for a pawn. Unused if the option implements a button.
        /// </summary>
        /// <param name="pawn">The pawn whose rule option is being changed.</param>
        /// <param name="oldValue">The original value of the option.</param>
        /// <param name="inputValue">The new value of the option entered in the rules dialog.</param>
        /// <returns>The return value will be the value set for the option.</returns>
        public delegate T OnChangeForPawnHandler(Pawn pawn, T oldValue, T inputValue);

        /// <summary>
        ///     This is called when the value of the option is changing for a preset. Unused if the option implements a button.
        /// </summary>
        /// <param name="target">The target of the preset that is being changed.</param>
        /// <param name="oldValue">The original value of the option.</param>
        /// <param name="inputValue">The new value of the option entered in the rules dialog.</param>
        /// <returns>The return value will be the value set for the option.</returns>
        public delegate T OnChangeForPresetHandler(OptionTarget target, T oldValue, T inputValue);

        /// <summary>
        ///     This is called when the value of the option is changing for a pawn. Unused if option option implements a button.
        /// </summary>
        public OnChangeForPawnHandler OnChangeForPawnForPawn { get; set; }

        /// <summary>
        ///     This is called when the value of the option is changing for a preset type. Unused if the option implements a button.
        /// </summary>
        public OnChangeForPresetHandler OnChangeForForPreset { get; set; }

        /// <summary>
        ///     Gets or sets the displayed label of this option.
        /// </summary>
        public string Label { get => Addon.Label; set => Addon.Label = value; }

        /// <summary>
        ///     Gets or sets the tooltip of this option.
        /// </summary>
        public string Tooltip { get => Addon.Tooltip; set => Addon.Tooltip = value; }

        internal OptionHandle(AddonOption addon) : base(addon)
        { }

        /// <summary>
        ///     Gets the value of this option for the given pawn.
        /// </summary>
        /// <param name="pawn">The pawn to get the value from.</param>
        /// <param name="invalidValue">The value returned if unable to retrieve the option.</param>
        /// <returns>Returns the value if the option is found or <see paramref="invalidValue" /> if not.</returns>
        public T GetValue(Pawn pawn, T invalidValue = default(T))
        {
            var rules = Registry.GetRules(pawn);
            return rules == null ? invalidValue : rules.GetAddonValue(Addon, invalidValue);
        }

        /// <summary>
        ///     Gets the default value of this option for the given target.
        /// </summary>
        /// <param name="target">The default rules target to get the value from.</param>
        /// <param name="invalidValue">The value returned if unable to retrieve the option.</param>
        /// <returns>Returns the value if the option is found or <see paramref="invalidValue" /> if not.</returns>
        public T GetDefaultValue(OptionTarget target, T invalidValue = default(T)) => Registry.GetAddonDefaultValue<T>(target, Addon);

        /// <summary>
        ///     Sets the value of this option for the given pawn.
        /// </summary>
        /// <param name="pawn">The pawn to set the value to.</param>
        /// <param name="value">The new value for the option.</param>
        /// <returns>Returns true if the option was successfully set.</returns>
        public bool SetValue(Pawn pawn, T value)
        {
            var rules = Registry.GetRules(pawn);
            return (rules != null) && rules.SetAddonValueDirect(Addon, value);
        }
    }
}
