using SMLHelper.V2.Json;
using SMLHelper.V2.Json.Converters;
using SMLHelper.V2.Options.Attributes;
using SMLHelper.V2.Options;
#if SUBNAUTICA
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
#elif BELOWZERO
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#endif
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    [Menu("SnapBuilder", LoadOn = MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened)]
    internal class Config : ConfigFile
    {
        [JsonIgnore]
        private Toggle snapping;

        [JsonIgnore]
        public Toggle Snapping => snapping ??= new Toggle(ToggleSnappingKey, ToggleSnappingMode, EnabledByDefault);

        [JsonIgnore]
        private Toggle fineSnapping;

        [JsonIgnore]
        public Toggle FineSnapping => fineSnapping ??= new Toggle(FineSnappingKey, FineSnappingMode, false);

        [JsonIgnore]
        private Toggle fineRotation;
        [JsonIgnore]
        public Toggle FineRotation => fineRotation ??= new Toggle(FineRotationKey, FineRotationMode, false);

        [Toggle(LabelLanguageId = "Options.SnappingEnabledByDefault"), OnChange(nameof(EnabledByDefaultChanged))]
        public bool EnabledByDefault { get; set; } = true;
        private void EnabledByDefaultChanged(ToggleChangedEventArgs e)
            => Snapping.EnabledByDefault = e.Value;

        [Keybind(LabelLanguageId = "Options.ToggleSnappingKey"), OnChange(nameof(ToggleSnappingKeyChanged))]
        public KeyCode ToggleSnappingKey { get; set; } = KeyCode.Mouse2;
        private void ToggleSnappingKeyChanged(KeybindChangedEventArgs e)
            => Snapping.KeyCode = e.Key;

        [JsonConverter(typeof(StringEnumConverter))]
        [Choice(LabelLanguageId = "Options.ToggleSnappingMode"), OnChange(nameof(ToggleSnappingModeChanged))]
        public Toggle.Mode ToggleSnappingMode { get; set; } = Toggle.Mode.Press;
        private void ToggleSnappingModeChanged(ChoiceChangedEventArgs e)
            => Snapping.KeyMode = (Toggle.Mode)e.Index;

        [Keybind(LabelLanguageId = "Options.FineSnappingKey"), OnChange(nameof(FineSnappingKeyChanged))]
        public KeyCode FineSnappingKey { get; set; } = KeyCode.LeftControl;
        private void FineSnappingKeyChanged(KeybindChangedEventArgs e)
            => FineSnapping.KeyCode = e.Key;

        [JsonConverter(typeof(StringEnumConverter))]
        [Choice(LabelLanguageId = "Options.FineSnappingMode"), OnChange(nameof(FineSnappingModeChanged))]
        public Toggle.Mode FineSnappingMode { get; set; } = Toggle.Mode.Hold;
        private void FineSnappingModeChanged(ChoiceChangedEventArgs e)
            => FineSnapping.KeyMode = (Toggle.Mode)e.Index;

        [Keybind(LabelLanguageId = "Options.FineRotationKey"), OnChange(nameof(FineRotationKeyChanged))]
        public KeyCode FineRotationKey { get; set; } = KeyCode.LeftAlt;
        private void FineRotationKeyChanged(KeybindChangedEventArgs e)
            => FineRotation.KeyCode = e.Key;

        [JsonConverter(typeof(StringEnumConverter))]
        [Choice(LabelLanguageId = "Options.FineRotationMode"), OnChange(nameof(FineRotationModeChanged))]
        public Toggle.Mode FineRotationMode { get; set; } = Toggle.Mode.Hold;
        private void FineRotationModeChanged(ChoiceChangedEventArgs e)
            => FineRotation.KeyMode = (Toggle.Mode)e.Index;

        [JsonConverter(typeof(FloatConverter), 2)]
        [Slider(0.01f, 1, LabelLanguageId = "Options.SnapRounding", Step = 0.01f, Format = "{0:##0%}", DefaultValue = 0.5f)]
        public float SnapRounding { get; set; } = 0.5f;

        [JsonConverter(typeof(FloatConverter), 2)]
        [Slider(0.01f, 1, LabelLanguageId = "Options.FineSnapRounding", Step = 0.01f, Format = "{0:##0%}", DefaultValue = 0.2f)]
        public float FineSnapRounding { get; set; } = 0.2f;

        [Slider(0, 90, LabelLanguageId = "Options.RotationRounding", DefaultValue = 45)]
        public int RotationRounding { get; set; } = 45;

        [Slider(0, 45, LabelLanguageId = "Options.FineRotationRounding", DefaultValue = 5)]
        public int FineRotationRounding { get; set; } = 5;

        public bool HasUpgraded = false;

        public void Initialise()
        {
            Upgrade();
        }

        public void ResetToggles()
        {
            Snapping.Reset();
            FineSnapping.Reset();
            FineRotation.Reset();
        }

        private void Upgrade()
        {
            if (HasUpgraded)
                return;

            HasUpgraded = true;
            Save();

            if (!Main.PreviousConfigFileExists)
                return;

            FineSnapRounding *= 2;
            Save();
        }
    }
}
