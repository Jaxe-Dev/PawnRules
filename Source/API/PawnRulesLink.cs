using System.Linq;
using System.Reflection;
using PawnRules.Data;
using PawnRules.Interface;
using Verse;

namespace PawnRules.API
{
    /// <summary>
    ///     Provides a link to Pawn Rules and is used to add options to the rules dialog.
    /// </summary>
    public class PawnRulesLink
    {
        internal readonly ModContentPack ModContentPack;

        /// <summary>
        ///     Initializes a link to Pawn Rules. Only one plugin per mod is allowed.
        /// </summary>
        public PawnRulesLink()
        {
            if (Registry.IsActive || !AddonManager.AcceptingAddons) { throw new Mod.Exception("Link must be created before a world is loaded"); }

            var assembly = Assembly.GetCallingAssembly();
            var modContentPack = LoadedModManager.RunningMods.FirstOrDefault(mod => mod.assemblies.loadedAssemblies.Contains(assembly)) ?? throw new Mod.Exception("Assembly not a registered RimWorld mod");
            if (AddonManager.Mods.Contains(modContentPack)) { throw new Mod.Exception("Only one plugin per mod is allowed"); }

            ModContentPack = modContentPack;
            Mod.Log($"Registered with {ModContentPack.Identifier}");
        }

        /// <summary>
        ///     Adds a new Toggle to the Rules dialog that sets a <see cref="bool" />.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<bool> AddToggle(string key, OptionTarget target, string label, string tooltip, bool defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.Checkbox, label, tooltip, defaultValue, allowedInPreset);

        /// <summary>
        ///     Adds a new Entry to the Rules dialog that sets a <see cref="string" /> value.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<string> AddEntry(string key, OptionTarget target, string label, string tooltip, string defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.TextEntry, label, tooltip, defaultValue, allowedInPreset);

        /// <summary>
        ///     Adds a new Entry to the Rules dialog that sets an <see cref="int" /> value.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<int> AddEntry(string key, OptionTarget target, string label, string tooltip, int defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.TextEntry, label, tooltip, defaultValue, allowedInPreset);

        /// <summary>
        ///     Adds a new Entry to the Rules dialog that sets a <see cref="float" /> value.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<float> AddEntry(string key, OptionTarget target, string label, string tooltip, float defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.TextEntry, label, tooltip, defaultValue, allowedInPreset);

        /// <summary>
        ///     Adds a new Button to the Rules dialog that sets a <see cref="string" /> value. Buttons require <see cref="OptionHandle.OnButtonClick" /> to be used to set the value.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<string> AddButton(string key, OptionTarget target, string label, string tooltip, string defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.Button, label, tooltip, defaultValue, allowedInPreset);

        /// <summary>
        ///     Adds a new Button to the Rules dialog that sets a <see cref="bool" /> value. Buttons require <see cref="OptionHandle.OnButtonClick" /> to be used to set the value.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<bool> AddButton(string key, OptionTarget target, string label, string tooltip, bool defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.Button, label, tooltip, defaultValue, allowedInPreset);

        /// <summary>
        ///     Adds a new Button to the Rules dialog that sets an <see cref="int" /> value. Buttons require <see cref="OptionHandle.OnButtonClick" /> to be used to set the value.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<int> AddButton(string key, OptionTarget target, string label, string tooltip, int defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.Button, label, tooltip, defaultValue, allowedInPreset);

        /// <summary>
        ///     Adds a new Button to the Rules dialog that sets a <see cref="float" /> value. Buttons <see cref="OptionHandle.OnButtonClick" /> to be used to set the value.
        /// </summary>
        /// <param name="key">The key used in the save file. Will be automatically prefixed with your Mod Identifier.</param>
        /// <param name="target">The type(s) of pawns this option will apply to.</param>
        /// <param name="label">The label of the widget displayed.</param>
        /// <param name="tooltip">The tooltip displayed for the widget.</param>
        /// <param name="defaultValue">This is the default value if <see paramref="allowedInPreset" /> is false or no default rules exist when a pawn is first given rules.</param>
        /// <param name="allowedInPreset">If set to false it cannot be used in a preset.</param>
        /// <returns>Returns a handle for this option.</returns>
        public OptionHandle<float> AddButton(string key, OptionTarget target, string label, string tooltip, float defaultValue, bool allowedInPreset = true) => AddonOption.Add(this, key, target, OptionWidget.Button, label, tooltip, defaultValue, allowedInPreset);
    }
}
