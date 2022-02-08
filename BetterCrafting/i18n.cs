



using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Leclair.Stardew.BetterCrafting
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "General Options".</summary>
        public static string Setting_General()
        {
            return I18n.GetByKey("setting.general");
        }

        /// <summary>Get a translation equivalent to "Replace Crafting Menu".</summary>
        public static string Setting_ReplaceCrafting()
        {
            return I18n.GetByKey("setting.replace-crafting");
        }

        /// <summary>Get a translation equivalent to "When enabled, the crafting menu is replaced with a reimplementation.".</summary>
        public static string Setting_ReplaceCrafting_Tip()
        {
            return I18n.GetByKey("setting.replace-crafting.tip");
        }

        /// <summary>Get a translation equivalent to "Replace Cooking Menu".</summary>
        public static string Setting_ReplaceCooking()
        {
            return I18n.GetByKey("setting.replace-cooking");
        }

        /// <summary>Get a translation equivalent to "When enabled, the cooking menu is replaced with a reimplementation.".</summary>
        public static string Setting_ReplaceCooking_Tip()
        {
            return I18n.GetByKey("setting.replace-cooking.tip");
        }

        /// <summary>Get a translation equivalent to "Enable Categories".</summary>
        public static string Setting_EnableCategories()
        {
            return I18n.GetByKey("setting.enable-categories");
        }

        /// <summary>Get a translation equivalent to "Display category tabs in crafting menus, rather than one big list of everything.".</summary>
        public static string Setting_EnableCategories_Tip()
        {
            return I18n.GetByKey("setting.enable-categories.tip");
        }

        /// <summary>Get a translation equivalent to "Crafting".</summary>
        public static string Setting_Crafting()
        {
            return I18n.GetByKey("setting.crafting");
        }

        /// <summary>Get a translation equivalent to "These settings apply specifically to the reimplemented crafting menu.".</summary>
        public static string Setting_Crafting_Tip()
        {
            return I18n.GetByKey("setting.crafting.tip");
        }

        /// <summary>Get a translation equivalent to "Force Uniform Sizes".</summary>
        public static string Setting_UniformGrid()
        {
            return I18n.GetByKey("setting.uniform-grid");
        }

        /// <summary>Get a translation equivalent to "When enabled, big craftables will be squished down to fit in a single row.".</summary>
        public static string Setting_UniformGrid_Tip()
        {
            return I18n.GetByKey("setting.uniform-grid.tip");
        }

        /// <summary>Get a translation equivalent to "Sort Big Craftables Last".</summary>
        public static string Setting_BigCraftablesLast()
        {
            return I18n.GetByKey("setting.big-craftables-last");
        }

        /// <summary>Get a translation equivalent to "When enabled, big craftables will all be put at the end of the list.".</summary>
        public static string Setting_BigCraftablesLast_Tip()
        {
            return I18n.GetByKey("setting.big-craftables-last.tip");
        }

        /// <summary>Get a translation equivalent to "Cooking".</summary>
        public static string Setting_Cooking()
        {
            return I18n.GetByKey("setting.cooking");
        }

        /// <summary>Get a translation equivalent to "These settings apply specifically to the reimplemented cooking menu.".</summary>
        public static string Setting_Cooking_Tip()
        {
            return I18n.GetByKey("setting.cooking.tip");
        }

        /// <summary>Get a translation equivalent to "Use Seasoning".</summary>
        public static string Setting_Seasoning()
        {
            return I18n.GetByKey("setting.seasoning");
        }

        /// <summary>Get a translation equivalent to "Whether or not seasoning should be used to enhance cooked items. When set to "Inventory Only", seasoning will only be used from your inventory and not from the fridge or any other source.".</summary>
        public static string Setting_Seasoning_Tip()
        {
            return I18n.GetByKey("setting.seasoning.tip");
        }

        /// <summary>Get a translation equivalent to "Never".</summary>
        public static string Seasoning_Disabled()
        {
            return I18n.GetByKey("seasoning.disabled");
        }

        /// <summary>Get a translation equivalent to "Always".</summary>
        public static string Seasoning_Enabled()
        {
            return I18n.GetByKey("seasoning.enabled");
        }

        /// <summary>Get a translation equivalent to "Inventory Only".</summary>
        public static string Seasoning_Inventory()
        {
            return I18n.GetByKey("seasoning.inventory");
        }

        /// <summary>Get a translation equivalent to "Hide Unknown Recipes".</summary>
        public static string Setting_HideUnknown()
        {
            return I18n.GetByKey("setting.hide-unknown");
        }

        /// <summary>Get a translation equivalent to "When enabled, recipes you haven't learned yet won't be displayed in the menu.".</summary>
        public static string Setting_HideUnknown_Tip()
        {
            return I18n.GetByKey("setting.hide-unknown.tip");
        }

        /// <summary>Get a translation equivalent to "Edit Categories".</summary>
        public static string Tooltip_EditMode()
        {
            return I18n.GetByKey("tooltip.edit-mode");
        }

        /// <summary>Get a translation equivalent to "Only Show Favorites".</summary>
        public static string Tooltip_Favorites()
        {
            return I18n.GetByKey("tooltip.favorites");
        }

        /// <summary>Get a translation equivalent to "Use Seasoning".</summary>
        public static string Tooltip_Seasoning()
        {
            return I18n.GetByKey("tooltip.seasoning");
        }

        /// <summary>Get a translation equivalent to "Use Uniform Grid".</summary>
        public static string Tooltip_Uniform()
        {
            return I18n.GetByKey("tooltip.uniform");
        }

        /// <summary>Get a translation equivalent to "New Category".</summary>
        public static string Category_New()
        {
            return I18n.GetByKey("category.new");
        }

        /// <summary>Get a translation equivalent to "Favorites".</summary>
        public static string Category_Favorites()
        {
            return I18n.GetByKey("category.favorites");
        }

        /// <summary>Get a translation equivalent to "Combat and Rings".</summary>
        public static string Category_CombatRings()
        {
            return I18n.GetByKey("category.combat_rings");
        }

        /// <summary>Get a translation equivalent to "Consumables".</summary>
        public static string Category_Consumables()
        {
            return I18n.GetByKey("category.consumables");
        }

        /// <summary>Get a translation equivalent to "Decoration".</summary>
        public static string Category_Decoration()
        {
            return I18n.GetByKey("category.decoration");
        }

        /// <summary>Get a translation equivalent to "Fertilizer and Seeds".</summary>
        public static string Category_FertilizerSeeds()
        {
            return I18n.GetByKey("category.fertilizer_seeds");
        }

        /// <summary>Get a translation equivalent to "Fishing".</summary>
        public static string Category_Fishing()
        {
            return I18n.GetByKey("category.fishing");
        }

        /// <summary>Get a translation equivalent to "Machinery".</summary>
        public static string Category_Machinery()
        {
            return I18n.GetByKey("category.machinery");
        }

        /// <summary>Get a translation equivalent to "Miscellaneous".</summary>
        public static string Category_Misc()
        {
            return I18n.GetByKey("category.misc");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

