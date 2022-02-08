using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Leclair.Stardew.Almanac
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

        /// <summary>Get a translation equivalent to "Almanac".</summary>
        public static string ModName()
        {
            return I18n.GetByKey("mod-name");
        }

        /// <summary>Get a translation equivalent to "The\nFerngil\nFarmer's\nAlmanac".</summary>
        public static string Almanac_Cover()
        {
            return I18n.GetByKey("almanac.cover");
        }

        /// <summary>Get a translation equivalent to "{{season}}, Year {{year}}".</summary>
        /// <param name="season">The value to inject for the <c>{{season}}</c> token.</param>
        /// <param name="year">The value to inject for the <c>{{year}}</c> token.</param>
        public static string Calendar_When(object season, object year)
        {
            return I18n.GetByKey("calendar.when", new { season, year });
        }

        /// <summary>Get a translation equivalent to "M".</summary>
        public static string Calendar_Day_1()
        {
            return I18n.GetByKey("calendar.day.1");
        }

        /// <summary>Get a translation equivalent to "T".</summary>
        public static string Calendar_Day_2()
        {
            return I18n.GetByKey("calendar.day.2");
        }

        /// <summary>Get a translation equivalent to "W".</summary>
        public static string Calendar_Day_3()
        {
            return I18n.GetByKey("calendar.day.3");
        }

        /// <summary>Get a translation equivalent to "Th".</summary>
        public static string Calendar_Day_4()
        {
            return I18n.GetByKey("calendar.day.4");
        }

        /// <summary>Get a translation equivalent to "F".</summary>
        public static string Calendar_Day_5()
        {
            return I18n.GetByKey("calendar.day.5");
        }

        /// <summary>Get a translation equivalent to "Sa".</summary>
        public static string Calendar_Day_6()
        {
            return I18n.GetByKey("calendar.day.6");
        }

        /// <summary>Get a translation equivalent to "Su".</summary>
        public static string Calendar_Day_7()
        {
            return I18n.GetByKey("calendar.day.7");
        }

        /// <summary>Get a translation equivalent to "Planting Dates".</summary>
        public static string Page_Crops()
        {
            return I18n.GetByKey("page.crops");
        }

        /// <summary>Get a translation equivalent to "Last Day For:".</summary>
        public static string Crop_LastDay()
        {
            return I18n.GetByKey("crop.last-day");
        }

        /// <summary>Get a translation equivalent to "Toggle {{mode}}".</summary>
        /// <param name="mode">The value to inject for the <c>{{mode}}</c> token.</param>
        public static string Crop_Toggle(object mode)
        {
            return I18n.GetByKey("crop.toggle", new { mode });
        }

        /// <summary>Get a translation equivalent to "Paddy Bonus".</summary>
        public static string Crop_Paddy()
        {
            return I18n.GetByKey("crop.paddy");
        }

        /// <summary>Get a translation equivalent to "The following growth times and dates assume you have no fertilizer or skill.".</summary>
        public static string Crop_UsingNone()
        {
            return I18n.GetByKey("crop.using-none");
        }

        /// <summary>Get a translation equivalent to "The following growth times and dates assume you are an {{agriculturist}}.".</summary>
        /// <param name="agriculturist">The value to inject for the <c>{{agriculturist}}</c> token.</param>
        public static string Crop_UsingAgri(object agriculturist)
        {
            return I18n.GetByKey("crop.using-agri", new { agriculturist });
        }

        /// <summary>Get a translation equivalent to "The following growth times and dates assume you are using {{fertilizer}}.".</summary>
        /// <param name="fertilizer">The value to inject for the <c>{{fertilizer}}</c> token.</param>
        public static string Crop_UsingSpeed(object fertilizer)
        {
            return I18n.GetByKey("crop.using-speed", new { fertilizer });
        }

        /// <summary>Get a translation equivalent to "The following growth times and dates assume you are an {{agriculturist}} and that you are using {{fertilizer}}.".</summary>
        /// <param name="agriculturist">The value to inject for the <c>{{agriculturist}}</c> token.</param>
        /// <param name="fertilizer">The value to inject for the <c>{{fertilizer}}</c> token.</param>
        public static string Crop_UsingBoth(object agriculturist, object fertilizer)
        {
            return I18n.GetByKey("crop.using-both", new { agriculturist, fertilizer });
        }

        /// <summary>Get a translation equivalent to "They also assume crops that benefit from it are grown near water.".</summary>
        public static string Crop_UsingPaddy()
        {
            return I18n.GetByKey("crop.using-paddy");
        }

        /// <summary>Get a translation equivalent to "Grows in {{count}} days.".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Crop_GrowTime(object count)
        {
            return I18n.GetByKey("crop.grow-time", new { count });
        }

        /// <summary>Get a translation equivalent to "Plant no later than {{date}}.".</summary>
        /// <param name="date">The value to inject for the <c>{{date}}</c> token.</param>
        public static string Crop_LastDate(object date)
        {
            return I18n.GetByKey("crop.last-date", new { date });
        }

        /// <summary>Get a translation equivalent to "Regrows every {{count}} days.".</summary>
        /// <param name="count">The value to inject for the <c>{{count}}</c> token.</param>
        public static string Crop_RegrowTime(object count)
        {
            return I18n.GetByKey("crop.regrow-time", new { count });
        }

        /// <summary>Get a translation equivalent to "Grows faster near water.".</summary>
        public static string Crop_PaddyNote()
        {
            return I18n.GetByKey("crop.paddy-note");
        }

        /// <summary>Get a translation equivalent to "Able to grow to giant size.".</summary>
        public static string Crop_GiantNote()
        {
            return I18n.GetByKey("crop.giant-note");
        }

        /// <summary>Get a translation equivalent to "Grows on a trellis.".</summary>
        public static string Crop_TrellisNote()
        {
            return I18n.GetByKey("crop.trellis-note");
        }

        /// <summary>Get a translation equivalent to "Weather Forecast".</summary>
        public static string Page_Weather()
        {
            return I18n.GetByKey("page.weather");
        }

        /// <summary>Get a translation equivalent to "When:".</summary>
        public static string Festival_When()
        {
            return I18n.GetByKey("festival.when");
        }

        /// <summary>Get a translation equivalent to "{{start}} to {{end}}".</summary>
        /// <param name="start">The value to inject for the <c>{{start}}</c> token.</param>
        /// <param name="end">The value to inject for the <c>{{end}}</c> token.</param>
        public static string Festival_WhenTimes(object start, object end)
        {
            return I18n.GetByKey("festival.when-times", new { start, end });
        }

        /// <summary>Get a translation equivalent to "Where:".</summary>
        public static string Festival_Where()
        {
            return I18n.GetByKey("festival.where");
        }

        /// <summary>Get a translation equivalent to "Date:".</summary>
        public static string Festival_Date()
        {
            return I18n.GetByKey("festival.date");
        }

        /// <summary>Get a translation equivalent to "Sunny".</summary>
        public static string Weather_Sunny()
        {
            return I18n.GetByKey("weather.sunny");
        }

        /// <summary>Get a translation equivalent to "Rain".</summary>
        public static string Weather_Rain()
        {
            return I18n.GetByKey("weather.rain");
        }

        /// <summary>Get a translation equivalent to "Windy".</summary>
        public static string Weather_Debris()
        {
            return I18n.GetByKey("weather.debris");
        }

        /// <summary>Get a translation equivalent to "Thunderstorms".</summary>
        public static string Weather_Lightning()
        {
            return I18n.GetByKey("weather.lightning");
        }

        /// <summary>Get a translation equivalent to "Sunny".</summary>
        public static string Weather_Festival()
        {
            return I18n.GetByKey("weather.festival");
        }

        /// <summary>Get a translation equivalent to "Snow".</summary>
        public static string Weather_Snow()
        {
            return I18n.GetByKey("weather.snow");
        }

        /// <summary>Get a translation equivalent to "Train Schedule".</summary>
        public static string Page_Train()
        {
            return I18n.GetByKey("page.train");
        }

        /// <summary>Get a translation equivalent to "Arrival Times:".</summary>
        public static string Page_TrainArrivals()
        {
            return I18n.GetByKey("page.train-arrivals");
        }

        /// <summary>Get a translation equivalent to "Almanac Always Available".</summary>
        public static string Settings_Available()
        {
            return I18n.GetByKey("settings.available");
        }

        /// <summary>Get a translation equivalent to "When enabled, the Almanac will always be available regardless of whether you've received a copy.".</summary>
        public static string Settings_AvailableDesc()
        {
            return I18n.GetByKey("settings.available-desc");
        }

        /// <summary>Get a translation equivalent to "Use Deterministic Weather".</summary>
        public static string Settings_Weather_Deterministic()
        {
            return I18n.GetByKey("settings.weather.deterministic");
        }

        /// <summary>Get a translation equivalent to "In order for the weather forecast of the Almanac to work, we replace the calculation for tomorrow's weather with one that uses a predictable source of randomness. If this is causing problems, you can disable the behavior here. However, doing so will disable the extended weather forecast.".</summary>
        public static string Settings_Weather_DeterministicDesc()
        {
            return I18n.GetByKey("settings.weather.deterministic-desc");
        }

        /// <summary>Get a translation equivalent to "Controls".</summary>
        public static string Settings_Controls()
        {
            return I18n.GetByKey("settings.controls");
        }

        /// <summary>Get a translation equivalent to "Open Almanac".</summary>
        public static string Settings_Controls_Almanac()
        {
            return I18n.GetByKey("settings.controls.almanac");
        }

        /// <summary>Get a translation equivalent to "Pressing this key will open the Almanac interface.".</summary>
        public static string Settings_Controls_AlmanacDesc()
        {
            return I18n.GetByKey("settings.controls.almanac-desc");
        }

        /// <summary>Get a translation equivalent to "Enable Page".</summary>
        public static string Settings_Enable()
        {
            return I18n.GetByKey("settings.enable");
        }

        /// <summary>Get a translation equivalent to "Whether or not this page should be added to the Almanac.".</summary>
        public static string Settings_EnableDesc()
        {
            return I18n.GetByKey("settings.enable-desc");
        }

        /// <summary>Get a translation equivalent to "Page: Planting Dates".</summary>
        public static string Settings_Crops()
        {
            return I18n.GetByKey("settings.crops");
        }

        /// <summary>Get a translation equivalent to "Crop Previews".</summary>
        public static string Settings_Crops_Preview()
        {
            return I18n.GetByKey("settings.crops.preview");
        }

        /// <summary>Get a translation equivalent to "When Crop Previews are enabled, hovering over a crop in the Planting Dates tab will show you with the calendar what stage the crop will be at on a given day.".</summary>
        public static string Settings_Crops_PreviewDesc()
        {
            return I18n.GetByKey("settings.crops.preview-desc");
        }

        /// <summary>Get a translation equivalent to "Enable Previews".</summary>
        public static string Settings_Crops_Preview_Enable()
        {
            return I18n.GetByKey("settings.crops.preview.enable");
        }

        /// <summary>Get a translation equivalent to "Always Plant on First".</summary>
        public static string Settings_Crops_Preview_Plantonfirst()
        {
            return I18n.GetByKey("settings.crops.preview.plantonfirst");
        }

        /// <summary>Get a translation equivalent to "When enabled, the month overview you see when a crop is selected will always start planting on the first even if the current date is after the first of the month.".</summary>
        public static string Settings_Crops_Preview_PlantonfirstDesc()
        {
            return I18n.GetByKey("settings.crops.preview.plantonfirst-desc");
        }

        /// <summary>Get a translation equivalent to "Use Harvest Sprites".</summary>
        public static string Settings_Crops_Preview_Sprite()
        {
            return I18n.GetByKey("settings.crops.preview.sprite");
        }

        /// <summary>Get a translation equivalent to "When enabled, an image of the harvest product will be used rather than the final growth stage, to make it more easily apparent what days the crop is harvestable.".</summary>
        public static string Settings_Crops_Preview_SpriteDesc()
        {
            return I18n.GetByKey("settings.crops.preview.sprite-desc");
        }

        /// <summary>Get a translation equivalent to "Page: Train Schedule".</summary>
        public static string Settings_Train()
        {
            return I18n.GetByKey("settings.train");
        }

        /// <summary>Get a translation equivalent to "Page: Weather Forecast".</summary>
        public static string Settings_Weather()
        {
            return I18n.GetByKey("settings.weather");
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

